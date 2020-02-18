using System;
using System.Collections.Generic;
using Framework.Logging;
using Framework.Util;
using Framework.Context;

namespace Framework.Model
{
    internal abstract class ModelElementImplementation
    {
        // Configuration properties for Model Element Implementation...
        protected const string _DocgenSourcePrefix  = "DocgenSourcePrefix";     // Prefix for creation of 'source' URN.
        protected const string _DocgenTagList       = "DocgenTagList";          // Comma separated list of tagged values to be retrieved.

        /// <summary>
        /// These are identifying properties of the implementation object and are managed by the tool-specific implementation
        /// classes...
        /// </summary>
        protected string _name;         // The name of the artifact;
        protected string _aliasName;    // Optional alias name of the artifact (empty string if not used).
        protected int _elementID;       // Unique ID of the artifact in the repository;
        protected string _globalID;     // Globally unique ID of element (across repositories).
        protected bool _isValid;        // Set to false in case the associated repository component is known to be invalid/deleted.
        protected ModelElementType _type;       // Identifies the implementation type.
        protected ModelImplementation _model;   // Reference to the associated model implementation.

        private int _referenceCount;        // Keeps track of the number of interfaces using this particular implementation.
        private long _lastUpdate;           // Keeps track of the elapsed time since the elements has been loaded from the repository.
        
        /// <summary>
        /// Returns the local repository ID of the element (non-portable across repositories)
        /// </summary>
        internal int ElementID                            {get {return this._elementID; }}

        /// <summary>
        /// Returns the globally-unique ID of the element (portable across repositories)
        /// </summary>
        internal string GlobalID                          {get {return this._globalID; }}

        /// <summary>
        /// Returns 'true' when the element has valid state.
        /// </summary>
        internal bool Valid                               {get {return this._isValid; } }

        /// <summary>
        /// Returns the type of the model element.
        /// </summary>
        internal ModelElementType Type                    {get {return this._type; }}

        /// <summary>
        /// Returns the model implementation associated with this model element.
        /// </summary>
        internal ModelImplementation ModelImplementation  {get {return this._model; }}

        /// <summary>
        /// Get- or set the element name.
        /// </summary>
        internal string Name
        {
            get { return this._name; }
            set { this.SetName(value); }
        }

        /// <summary>
        /// Get- or set an alternative name for the element
        /// </summary>
        internal string AliasName
        {
            get { return this._aliasName; }
            set { this.SetAliasName(value); }
        }

        /// <summary>
        /// These methods must be implemented by specialized implementations...
        /// </summary>
        internal abstract void AddStereotype(string stereotype);
        internal abstract string GetAnnotation();
        internal abstract void SetAnnotation(string text);
        internal abstract string GetTag(string tagName);
        internal abstract void SetTag(string tagName, string tagValue, bool createIfNotExist = false);
        internal abstract bool HasStereotype(List<string> stereotypes);
        internal abstract bool HasStereotype(string stereotype);
        internal abstract void SetName(string newName);
        internal abstract void SetAliasName(string newAliasName);

        /// <summary>
        /// This function is activated whenever the repository detects a change to the associated model item. 
        /// The refresh operation should update internal structures to assure they are consistent with the state of the repository!
        /// </summary>
        internal void RefreshObject()
        {
            Logger.WriteInfo("Framework.Model.ModelElementImplementation >> Element '" + this._type.ToString() + "->" + 
                             this._name + "' just gotten a 'refresh object' request, currently ignored!");
        }

        /// <summary>
        /// Used to keep track of interfaces that use this implementation. When we detect the first interface that is actually
        /// using this implementation, we register the implementation object in the dictionary so future interfaces can find it.
        /// This is the ONLY place where we add the registration!
        /// </summary>
        /// <returns>Reference to ACTUALLY registered implementation, which COULD be different from this instance!</returns>
        internal ModelElementImplementation AddReference()
        {
            if (this._referenceCount++ == 0)
            {
                ModelElementImplementation registeredImp = this._model.RegisterElementImp(this);

                // We could get a different object in return when we attempt double registration...
                if (!ReferenceEquals(this, registeredImp))
                {
                    Logger.WriteInfo("Framework.Model.ModelElementImplementation.addReference >> Attempt to register same implementation twice, recovering...");
                    this._referenceCount--;                 // Incremented wrongly, interface MUST switch to actually registered instance!
                    registeredImp._referenceCount++;        // Tell this instance that it has a new interface.
                    return registeredImp;
                }
            }
            return this;
        }

        /// <summary>
        /// This method searches the model element for any tags (from the configured list of relevant tags) that have contents. 
        /// The method returns a list of MEDocumentation elements for each non-empty tag found, including the standard annotation of 
        /// the element.
        /// The method is defined virtual so that its behavior can be changed by derived implementations.
        /// </summary>
        /// <returns>List of documentation nodes or empty list on error or if no comments available.</returns>
        internal virtual List<MEDocumentation> GetDocumentation()
        {
            try
            {
                var docList = new List<MEDocumentation>();
                ContextSlt context = ContextSlt.GetContextSlt();
                string sourcePrefix = context.GetConfigProperty(_DocgenSourcePrefix);
                string[] tagList = context.GetConfigProperty(_DocgenTagList).Split(',');

                // First of all, harvest annotation from any of the configured tags (if present) and add to the annotation list...
                // We ONLY copy them if the configuration key allows us to do this...
                // Entries in the tag list must be formatted as 'key:tag'.
                foreach (string tag in tagList)
                {
                    string configKey = tag.Substring(0, tag.IndexOf(":"));
                    string tagName = tag.Substring(configKey.Length + 1);
                    if (context.GetBoolSetting(configKey))
                    {
                        string tagVal = this.GetTag(tagName);
                        if (tagVal != string.Empty) docList.Add(new MEDocumentation(sourcePrefix + tagName, tagVal));
                    }
                }

                // Finally, appends generic notes to the list (if present and allowed to store)....
                if (context.GetBoolSetting(FrameworkSettings._DENotes))
                {
                    string notes = GetAnnotation();
                    if (notes != string.Empty) AddNotesToDoclist(ref docList, notes, "en", sourcePrefix + "annotation");
                }
                return docList;
            }
            catch (Exception exc)
            {
                Logger.WriteError("SparxEA.Model.ModelElementImplementation.getDocumentation >> Caught an exception: " + exc.ToString());
            }
            return null;
        }

        /// <summary>
        /// Set the 'isValid' indicator to 'false' in order to mark the implementation as invalid. Please note that this has no further side
        /// effects on the class and the use of the indicator is context specific. Currently, an implementation can be made invalid, but
        /// this can never be turned around (in other words: it will remain invalid for ever)!
        /// </summary>
        internal void InValidate()
        {
            this._isValid = false;
        }

        /// <summary>
        /// The reference count is used to keep track of interfaces that use this implementation. When the count reaches zero,
        /// this implies that there are no more interested interfaces and we de-register the object to avoid polluting the
        /// dictionary with unused copies of implementation objects.
        /// This is the ONLY place where we remove the registration!
        /// </summary>
        internal void RemoveReference()
        {
            // Might fail when we try to remove references during shut-down where the model might have gone already!
            try
            {
                if (--this._referenceCount == 0) this._model.UnregisterElementImp(this);
                if (this._referenceCount < 0) this._referenceCount = 0; // Reference count is never allowed to be smaller then 0!
            }
            catch (Exception exc)
            {
                Logger.WriteError("Framework.Model.ModelElementImplementation.removeReference >> Caught exception: " + exc.ToString());
            }
        }

        /// <summary>
        /// Default constructor is used to guarantee that the reference count is initialized on creation. Reference counts must be
        /// managed explicitly from the interfaces.
        /// </summary>
        protected ModelElementImplementation(ModelImplementation model)
        {
            this._referenceCount = 0;
            this._model = model;
            this._type = ModelElementType.Unknown;
            this._name = string.Empty;
            this._aliasName = string.Empty;
            this._elementID = -1;
            this._globalID = string.Empty;
            this._lastUpdate = DateTime.Now.Ticks;
            this._isValid = true;
        }

        /// <summary>
        /// Helper method for derived implementation classes that aid in the inserting of generic annotation in a DocList. 
        /// The method checks whether the annotation field starts with a language code (which must be specified as "[code]") and if found, 
        /// inserts this as proper languange code in the DocList. 
        /// Only 2-character language codes are accepted!
        /// </summary>
        /// <param name="docList">Reference to doc list to be updated.</param>
        /// <param name="notes">The notes field to include</param>
        /// <param name="languageCode">Default language code to use</param>
        /// <param name="sourceID">Source identifier (tag name)</param>
        protected void AddNotesToDoclist(ref List<MEDocumentation> docList, string notes, string languageCode, string sourceID)
        {
            if (notes != string.Empty)
            {
                if (notes[0] == '[')
                {
                    // We have a language tag-code, use the contents as language settings...
                    int endBracket = notes.IndexOf(']');
                    string actualNotes = notes.Substring(endBracket + 1);
                    string newLanguageCode = notes.Substring(1, endBracket - 1);
                    // Only accept 2-character language codes, anything else is considered 'no language code'!
                    if (newLanguageCode.Length != 2)
                    {
                        actualNotes = notes;
                        newLanguageCode = languageCode;
                    }
                    docList.Add(new MEDocumentation(sourceID, actualNotes, newLanguageCode));
                }
                else docList.Add(new MEDocumentation(sourceID, notes, languageCode));
            }
        }
    }
}
