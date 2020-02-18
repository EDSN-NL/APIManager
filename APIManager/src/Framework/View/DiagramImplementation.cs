using System.Collections.Generic;
using Framework.Model;

namespace Framework.View
{
    internal abstract class DiagramImplementation
    {
        protected string _name;         // The name of the diagram;
        protected int _diagramID;       // Unique ID of the diagram within the repository;
        protected string _globalID;     // Globally unique ID of the diagram (across repositories).
        protected MEPackage _owningPackage;     // The package in which the diagram lives.
        protected ModelImplementation _model;   // Reference to the associated model implementation.
        protected bool _isValid;        // Set to false in case the associated diagram is known to be invalid/deleted.

        private int _referenceCount;    // Keeps track of the number of interfaces using this particular implementation.

        internal string Name              {get { return this._name; }}
        internal int DiagramID            {get { return this._diagramID; }}
        internal string GlobalID          {get { return this._globalID; }}
        internal MEPackage OwningPackage  {get { return new MEPackage(this._owningPackage); }}
        internal bool Valid               { get { return this._isValid; } }

        /// <summary>
        /// Add a list of associations to the diagram. Note that the diagram is NOT refreshed, facilitating a number of updates to 
        /// be performed in sequence.
        /// </summary>
        /// <param name="assocList">Associations to add.</param>
        internal abstract void AddAssociationList(List<MEAssociation> assocList);

        /// <summary>
        /// Add a list of classes to the diagram. Note that the diagram is NOT refreshed, facilitating a number of updates to 
        /// be performed in sequence.
        /// </summary>
        /// <param name="classList">Classes to add.</param>
        internal abstract void AddClassList(List<MEClass> classList);

        /// <summary>
        /// Create the diagram properties note element for this diagram and add it to the left-top corner. Note that the diagram is
        /// NOT refreshed, facilitating a number of updates to be performed in sequence. Invoke the 'show', 'redraw' or 'refresh'
        /// operations to update the diagram 'on screen'.
        /// </summary>
        internal abstract void AddDiagramProperties();

        /// <summary>
        /// Used to keep track of interfaces that use this implementation. When we detect the first interface that is actually
        /// using this implementation, we register the implementation object in the dictionary so future interfaces can find it.
        /// This is the ONLY place where we add the registration!
        /// </summary>
        internal void AddReference()
        {
            if (this._referenceCount++ == 0) this._model.RegisterDiagramImp(this);
        }

        /// <summary>
        /// Redraw the diagram, required after one or more 'add' operations to actually show the added elements on the diagram.
        /// </summary>
        internal abstract void Redraw();

        /// <summary>
        /// Refresh the contents of all elements that are shown on the diagram without actually redrawing the entire diagram.
        /// </summary>
        internal abstract void Refresh();

        /// <summary>
        /// This function is called when the repository has detected a change on the underlying repository object, which might require 
        /// refresh of internal state.
        /// </summary>
        internal abstract void RefreshObject();

        /// <summary>
        /// Saves the diagram to the specified file, using specified path. Path must NOT end with a separator
        /// and the file name must NOT have an extension! The extension is defined by the specified image type.
        /// The type of diagram to be created depends on current configuration settings.
        /// </summary>
        /// <param name="pathName">Absolute path to use, must NOT end with a separator!</param>
        /// <param name="baseFileName">Optional filename, without extension, when omitted, the diagram name is used instead.</param>
        internal abstract void Save(string pathName, string baseFileName);

        /// <summary>
        /// Copies the document to the Windows Clipboard as a device independent bitmap.
        /// </summary>
        internal abstract void SaveToClipboard();

        /// <summary>
        /// Changes the color of the specified class on the diagram to the specified color.
        /// </summary>
        /// <param name="thisClass">Class to be changed.</param>
        /// <param name="color">Color to assign to the class.</param>
        internal abstract void SetClassColor(MEClass thisClass, Diagram.ClassColor color);

        /// <summary>
        /// Layout and show a (new) diagram. Must be called after creation (and optionally adding some elements) in order to
        /// actually show the diagram to the user.
        /// </summary>
        internal abstract void Show();

        /// <summary>
        /// Updates the 'show connector stereotypes' property of the current diagram.
        /// </summary>
        /// <param name="mustShow">Set to 'true' to show connector stereotypes, 'false' otherwise.</param>
        internal abstract void ShowConnectorStereotypes(bool mustShow);

        /// <summary>
        /// The reference count is used to keep track of interfaces that use this implementation. When the count reaches zero,
        /// this implies that there are no more interested interfaces and we de-register the object to avoid polluting the
        /// dictionary with unused copies of implementation objects.
        /// This is the ONLY place where we remove the registration!
        /// </summary>
        internal void RemoveReference()
        {
            if (--this._referenceCount == 0)
            {
                this._model.UnregisterDiagramImp(this);
                if (this._owningPackage != null) this._owningPackage.Dispose();
                this._owningPackage = null;
            }
            if (this._referenceCount < 0) this._referenceCount = 0; // Reference count is never allowed to be smaller then 0!
        }

        /// <summary>
        /// Default constructor is used to guarantee that the reference count is initialized on creation. It must be
        /// managed explicitly from the interfaces.
        /// </summary>
        protected DiagramImplementation(ModelImplementation model)
        {
            this._referenceCount = 0;
            this._model = model;
            this._owningPackage = null;
            this._isValid = true;
        }

        /// <summary>
        /// Set the 'isValid' indicator to 'false' in order to mark the implementation as invalid. Please note that this has no further side
        /// effects on the class and the use of the indicator is context specific. Currently, an implementation can be made invalid, but
        /// this can never be turned around (in other words: it will remain invalid for ever)!
        /// </summary>
        protected void InValidate()
        {
            this._isValid = false;
        }
    }
}
