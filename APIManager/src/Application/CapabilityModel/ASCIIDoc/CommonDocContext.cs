using System;
using System.IO;
using System.Collections.Generic;
using Framework.Context;
using Framework.Logging;

namespace Plugin.Application.CapabilityModel.ASCIIDoc
{
    /// <summary>
    /// A documentation context can be considered a 'chapter' in a document. Each context contains class- as well as classifier documentation.
    /// the Common documentation context consists of a list of common class documentation, followed by a list of classifier documentation.
    /// </summary>
    internal sealed class CommonDocContext: DocContext
    {
        private SortedList<string, ClassDocNode> _commonClasses;

        /// <summary>
        /// Creates a new context with specified ID, name and header text.
        /// </summary>
        /// <param name="contextID">Unique ID of this context. By definition, this must be the namespace token of the associated namespace.</param>
        /// <param name="contextName">The name of the context.</param>
        /// <param name="headerText">Header text for the context.</param>
        /// <param name="chapterNumber">Chapter number in a sequence of Doc Nodes.</param>
        internal CommonDocContext(string contextID, string contextName, string headerText, int level): base(contextID, contextName, headerText, level)
        {
            this._commonClasses = new SortedList<string, ClassDocNode>();
        }

        /// <summary>
        /// Implementation of abstract 'Save' method, which writes the ASCIIDoc-formatted contents of the Common documentation context to the
        /// specified file stream. The stream is otherwise not modified, which facilitates multiple save operations to the same stream to work
        /// Ok.
        /// </summary>
        /// <param name="stream">Stream that is going to receive contents.</param>
        internal override void Save(StreamWriter stream)
        {
            const string _ClassDocNode      = "@CLASSDOCNODE@";
            const string _ClassifierDocNode = "@CLASSIFIERDOCNODE@";

            Logger.WriteInfo("Plugin.Application.CapabilityModel.ASCIIDoc.CommonDocContext.Save >> Writing contents...");
            ContextSlt context = ContextSlt.GetContextSlt();
            string contents = context.GetResourceString(FrameworkSettings._ASCIIDocCommonTemplate);
            string indent = string.Empty;
            for (int i = 0; i < this._level; indent += "=", i++) ;

            contents = contents.Replace("@LVL@", indent);
            contents = contents.Replace("@COMMONANCHOR@", this._name.ToLower());
            contents = contents.Replace("@COMMONNAME@", this._name);
            contents = contents.Replace("@COMMONNOTES@", this._headerText);

            // Build documentation for common classes...
            string cmnClassContents = string.Empty;
            if (this._commonClasses.Count > 0)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.ASCIIDoc.CommonDocContext.GetFormattedContents >> We have common classes...");
                cmnClassContents = context.GetResourceString(FrameworkSettings._ASCIIDocCommonClasses);
                cmnClassContents = cmnClassContents.Replace("@LVL@", indent);
                cmnClassContents = cmnClassContents.Replace("@TITLE@", "Common Classes");
                bool firstOne = true;
                foreach (ClassDocNode node in this._commonClasses.Values)
                {
                    if (this._xrefList.ContainsKey(node.Name)) node.AddXREF(this._xrefList[node.Name]);
                    cmnClassContents = cmnClassContents.Replace(_ClassDocNode, firstOne? node.ASCIIDoc + _ClassDocNode : 
                                                                                         Environment.NewLine + node.ASCIIDoc + _ClassDocNode);
                    firstOne = false;
                }
                cmnClassContents = cmnClassContents.Replace(_ClassDocNode, string.Empty);
            }
            contents = contents.Replace("@ASCIIDocCommonClasses@", cmnClassContents);

            // Build documentation for common classifiers...
            string cmnClassifiers = string.Empty;
            if (this._classifiers.Count > 0)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.ASCIIDoc.CommonDocContext.GetFormattedContents >> We have common classifiers...");
                cmnClassifiers = context.GetResourceString(FrameworkSettings._ASCIIDocClassifiers);
                cmnClassifiers = cmnClassifiers.Replace("@LVL@", indent);
                cmnClassifiers = cmnClassifiers.Replace("@TITLE@", "Common Data Types");
                bool firstOne = true;
                foreach (ClassifierDocNode node in this._classifiers.Values)
                {
                    if (this._xrefList.ContainsKey(node.Name)) node.AddXREF(this._xrefList[node.Name]);
                    cmnClassifiers = cmnClassifiers.Replace(_ClassifierDocNode, firstOne ? node.ASCIIDoc + _ClassifierDocNode :
                                                                                           Environment.NewLine + node.ASCIIDoc + _ClassifierDocNode);
                    firstOne = false;
                }
                cmnClassifiers = cmnClassifiers.Replace(_ClassifierDocNode, string.Empty);
            }
            contents = contents.Replace("@ASCIIDocClassifiers@", cmnClassifiers);
            Logger.WriteInfo("Plugin.Application.CapabilityModel.ASCIIDoc.CommonDocContext.GetFormattedContents >> Saving...");

            // Finally, write the formatted contents to the specified stream...
            stream.Write(contents);
        }

        /// <summary>
        /// This method serves two purposes: First, it is used to initialize a new documentation node for a class. Second, it can select
        /// a class that has been initialized earlier. In case of the first, the 'notes' parameter is mandatory. In case of the second,
        /// the 'notes' parameter is ignored by the method.
        /// </summary>
        /// <param name="className">The name of the class to switch-to (or create).</param>
        /// <param name="notes">In case of first call, this contains class body text.</param>
        /// <param name="isEmpty">When TRUE, this is a placeholder (class without attributes).</param>
        internal void SwitchClass(string className, string notes, bool isEmpty)
        {
            if (!this._commonClasses.ContainsKey(className))
            {
                this._currentNode = new ClassDocNode(this._contextID, className, notes, this._level + 1, isEmpty);
                this._commonClasses.Add(className, this._currentNode);
            }
            else this._currentNode = this._commonClasses[className];
        }

        /// <summary>
        /// For the Common context, classifiers are defined at level 1.
        /// </summary>
        /// <returns>Current level.</returns>
        protected override int GetClassifierLevel()
        {
            return this._level + 1;
        }
    }
}
