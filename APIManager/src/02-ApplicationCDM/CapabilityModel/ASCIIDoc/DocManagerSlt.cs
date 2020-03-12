using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Framework.Logging;
using Framework.Context;

namespace Plugin.Application.CapabilityModel.ASCIIDoc
{
    /// <summary>
    /// The documentation manager singleton manages the lists of documentation nodes for the capability model. Documentation manager has one
    /// single 'Common' documentation context as well as a list of 'Operation' documentation contexts. Typically, Documentation manager is used
    /// to create the documentation for a single Interface capability.
    /// </summary>
    internal sealed class DocManagerSlt
    {
        // Configuration properties used by this module...
        private const string _ASCIIDocFileSuffix    = "ASCIIDocFileSuffix";
        private const string _ASCIIDocRootLevel     = "ASCIIDocRootLevel";
        private const string _ASCIIDocRootNumber    = "ASCIIDocRootNumber";

        // This is the actual Context singleton. It is created automatically on first load.
        private static readonly DocManagerSlt _docManagerSlt = new DocManagerSlt();

        private CommonDocContext _commonContext;                        // We have exactly one (optional) common context.
        private SortedList<string, OperationDocContext> _operations;    // ..and a list of operations.
        private int _level;                                             // Defines the starting level for the documentation structure.
        
        /// <summary>
        /// Getter for the common documentation context (must have been created earlier by invoking 'InitializeCommonDocContext', otherwise
        /// contents are not guaranteed to be valid!
        /// </summary>
        internal CommonDocContext CommonDocContext { get { return this._commonContext; } }

        /// <summary>
        /// Public Model "factory" method. Simply returns the static instance.
        /// </summary>
        /// <returns>DocManager singleton object</returns>
        internal static DocManagerSlt GetDocManagerSlt() { return _docManagerSlt; }

        /// <summary>
        /// This method releases all current resources and must be called before building a new documentation context.
        /// </summary>
        internal void Flush()
        {
            this._operations = new SortedList<string, OperationDocContext>();
            this._commonContext = null;
        }

        /// <summary>
        /// This function creates a new OperationDocContext, stores it in the list of operations and returns the created instance for further
        /// processing.
        /// </summary>
        /// <param name="contextID">Unique context identifier. By definition, this is the namespace token of the associated operation.</param>
        /// <param name="contextName">Operation name.</param>
        /// <param name="headerText">Operation body text (documentation).</param>
        /// <returns>Newly created operation documentation context.</returns>
        /// <exception cref="ArgumentException">Thrown when existing contextID is passed as unique key.</exception>
        internal OperationDocContext GetNewOperationDocContext(string contextID, string contextName, string headerText)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.ASCIIDoc.DocManagerSlt.GetNewOperationDocContext >> Requesting new operation context '" + 
                             contextID + ":" + contextName + "'...");
            if (!this._operations.ContainsKey(contextID))
            {
                var newDocContext = new OperationDocContext(contextID, contextName, headerText, this._level + 1);
                if (this._operations == null) this._operations = new SortedList<string, OperationDocContext>();
                this._operations.Add(contextID, newDocContext);
                return newDocContext;
            }
            else
            {
                string message = "New operation documentation context '" + contextName + "' requested with duplicate ID: '" + contextID + "'!";
                Logger.WriteError("Plugin.Application.CapabilityModel.ASCIIDoc.DocManagerSlt.GetNewOperationDocContext >> " + message);
                throw new ArgumentException(message);
            }
        }

        /// <summary>
        /// Function returns reference to an existing Operation documentation context. Returns NULL if not found.
        /// </summary>
        /// <param name="contextID">Unique context identifier. By definition, this is the namespace token of the associated operation.</param>
        /// <returns>Operation documentation context or NULL if not found.</returns>
        internal OperationDocContext GetOperationDocContext(string contextID)
        {
            return (this._operations != null && this._operations.ContainsKey(contextID)) ? this._operations[contextID] : null;
        }

        /// <summary>
        /// Must be invoked in order to establish a valid documentation context for common definitions. Since Common definitions are written
        /// to a new chapter, the level is identical to the default level of the Documentation Manager.
        /// </summary>
        /// <param name="contextID">Unique context identifier. By definition, this is the namespace token of the common capability.</param>
        /// <param name="contextName">Name of the common documentation context.</param>
        /// <param name="headerText">Common documentation context body text.</param>
        /// <returns></returns>
        internal CommonDocContext InitializeCommonDocContext(string contextID, string contextName, string headerText)
        {
            this._commonContext = new CommonDocContext(contextID, contextName, headerText, this._level);
            return this._commonContext;
        }

        /// <summary>
        /// Can be called to save the entire documentation context to a specified file. For the file name, only the base-name must be 
        /// specified (no extension). The extension is added by retrieving the appropriate configuration item.
        /// Title and Notes must be passed to assign a title to the overall document, together with associated body text (notes).
        /// </summary>
        /// <param name="absolutePath">Location for the file, without trailing separator.</param>
        /// <param name="baseFileName">Filename without extension.</param>
        /// <param name="title">Title for the document.</param>
        /// <param name="notes">Header notes (below Title).</param>
        internal void Save(string absolutePath, string baseFileName, string title, string notes)
        {
            StreamWriter writer = null;
            ContextSlt context = ContextSlt.GetContextSlt();
            string extension = context.GetConfigProperty(_ASCIIDocFileSuffix);
            string chapterNr = context.GetConfigProperty(_ASCIIDocRootNumber);
            Logger.WriteInfo("Plugin.Application.CapabilityModel.ASCIIDoc.DocManagerSlt.Save >> Writing contents to '" + 
                             absolutePath + "\\" + baseFileName + extension + "'...");
            try
            {
                using (writer = new StreamWriter(absolutePath + "\\" + baseFileName + extension, false, Encoding.UTF8))
                {
                    string contents = context.GetResourceString(FrameworkSettings._ASCIIDocOperationListTemplate);
                    string indent = string.Empty;
                    for (int i = 0; i < this._level; indent += "=", i++) ;
                    contents = contents.Replace("@LVL@", indent);
                    contents = contents.Replace("@CHAPTNR@", chapterNr);
                    contents = contents.Replace("@TITLE@", title);
                    contents = contents.Replace("@NOTES@", notes);
                    writer.Write(contents);

                    // Get our list of operation sorted by operation name...
                    var sortedOperations = new List<OperationDocContext>(this._operations.Values);
                    sortedOperations.Sort();
                    foreach (OperationDocContext operation in sortedOperations) operation.Save(writer);
                    //writer.Close();
                }

                if (this._commonContext != null)
                {
                    // For the Common definitions, we create a separate file, since it will be a separate chapter.
                    using (writer = new StreamWriter(absolutePath + "\\" + "Common_" + baseFileName + extension, false, Encoding.UTF8))
                    {
                        this._commonContext.Save(writer);
                        //writer.Close();
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.ASCIIDoc.DocManagerSlt.Save >> Error writing to '" +
                                  absolutePath + "\\" + baseFileName + extension + "' because:\n" + exc.ToString());
            }
        }

        /// <summary>
        /// The private constructor is called once on initial load and assures that exactly one valid object is present at all times.
        /// The constructor will also initialise the root-level from configuration.
        /// </summary>
        private DocManagerSlt()
        {
            this._commonContext = null;
            this._operations = null;
            if (!int.TryParse(ContextSlt.GetContextSlt().GetConfigProperty(_ASCIIDocRootLevel), out this._level)) this._level = 1;
        }
    }
}
