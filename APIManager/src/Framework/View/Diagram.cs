using System;
using System.Collections.Generic;
using Framework.Logging;
using Framework.Model;
using Framework.Exceptions;

namespace Framework.View
{
    internal sealed class Diagram: IDisposable
    {
        // This enumeration can be used to specify a color for a class on the diagram...
        internal enum ClassColor { Default, Black, White, Red, Orange, Yellow, Green, Blue, Purple }

        private DiagramImplementation _imp = null;  // The associated implementation object; does all the 'real' work.
        private bool _disposed;                     // Mark myself as invalid after call to dispose!

        // Retrieve some diagram properties. Please note that the 'package' property returns a COPY of the diagram package
        // and the caller is responsible for proper disposal of this copy.
        internal string Name              {get { return (this._imp != null) ? this._imp.Name : string.Empty; }}
        internal int DiagramID            {get { return (this._imp != null) ? this._imp.DiagramID : -1; }}
        internal string GlobalID          {get { return (this._imp != null) ? this._imp.GlobalID : string.Empty; }}
        internal MEPackage OwningPackage  {get { return (this._imp != null) ? this._imp.OwningPackage : null; } }

        /// <summary>
        /// The constructor receives a, platform-dependent, implementation object and creates a new Diagram object that
        /// is associated with the provided implementation object.
        /// </summary>
        /// <param name="imp">Diagram implementation object.</param>
        internal Diagram (int diagramID)
        {
            this._imp = ModelSlt.GetModelSlt().GetDiagramImplementation(diagramID);
            this._disposed = false;
            if (this._imp != null)
            {
                this._imp.AddReference();
            }
            else
            {
                Logger.WriteError("Framework.View.Diagram >> Could not obtain implementation object for diagram with ID: '" + diagramID + "'!");
            }
        }

        /// <summary>
        /// Create a new Diagram based on a provided implementation object.
        /// </summary>
        /// <param name="imp">The implementation to be used.</param>
        internal Diagram(DiagramImplementation imp)
        {
            this._disposed = false;
            this._imp = imp;
            if (this._imp != null)
            {
                this._imp.AddReference();
            }
            else
            {
                Logger.WriteError("Framework.Model.ModelElement >> Empty implementation passed!");
            }
        }

        /// <summary>
        /// The copy constructor creates a new Diagram instance from an original copy and assures that the reference count is updated accordingly.
        /// </summary>
        /// <param name="copy">Diagram to use as basis for copy.</param>
        internal Diagram (Diagram copy)
        {
            this._imp = copy._imp;
            this._disposed = false;
            if (this._imp != null)
            {
                this._imp.AddReference();
            }
            else
            {
                Logger.WriteError("Framework.View.Diagram >> Empty implementation passed to copy constructor!");
            }
        }

        /// <summary>
        /// Add a list of associations to the diagram. Note that the diagram is NOT refreshed, facilitating a number of updates to 
        /// be performed in sequence.
        /// </summary>
        /// <param name="assocList">Associations to add.</param>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal void AddAssociationList(List<MEAssociation> assocList)
        {
            if (this._imp != null) this._imp.AddAssociationList(assocList);
            else throw new MissingImplementationException("DiagramImplementation");
        }

        /// <summary>
        /// Add a list of classes to the diagram. Note that the diagram is NOT refreshed, facilitating a number of updates to 
        /// be performed in sequence.
        /// </summary>
        /// <param name="classList">Classes to add.</param>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal void AddClassList(List<MEClass> classList)
        {
            if (this._imp != null) this._imp.AddClassList(classList);
            else throw new MissingImplementationException("DiagramImplementation");
        }

        /// <summary>
        /// Create the diagram properties note element for this diagram and add it to the left-top corner. Note that the diagram is
        /// NOT refreshed, facilitating a number of updates to be performed in sequence. Invoke the 'show', 'redraw' or 'refresh'
        /// operations to update the diagram 'on screen'.
        /// </summary>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal void AddDiagramProperties()
        {
            if (this._imp != null) this._imp.AddDiagramProperties();
            else throw new MissingImplementationException("DiagramImplementation");
        }

        /// <summary>
        /// This is the normal entry for all users of the object that want to indicate that the interface is not required anymore.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Redraw the diagram, required after one or more 'add' operations to actually show the added elements on the diagram.
        /// </summary>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal void Redraw()
        {
            if (this._imp != null) this._imp.Redraw();
            else throw new MissingImplementationException("DiagramImplementation");
        }

        /// <summary>
        /// Refresh the contents of all elements that are shown on the diagram without actually redrawing the entire diagram.
        /// </summary>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal void Refresh()
        {
            if (this._imp != null) this._imp.Refresh();
            else throw new MissingImplementationException("DiagramImplementation");
        }

        /// <summary>
        /// Saves the diagram to the specified file, using specified path. Path must NOT end with a separator
        /// and the file name must NOT have an extension! 
        /// The type of diagram to be created depends on current configuration settings.
        /// </summary>
        /// <param name="pathName">Absolute path to use, must NOT end with a separator!</param>
        /// <param name="baseFileName">Optional filename, without extension, when omitted, the diagram name is used instead.</param>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal void Save(string pathName, string baseFileName = null)
        {
            if (this._imp != null) this._imp.Save(pathName, baseFileName);
            else throw new MissingImplementationException("DiagramImplementation");
        }

        /// <summary>
        /// Copies the document to the Windows Clipboard as a device independent bitmap.
        /// </summary>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal void SaveToClipboard()
        {
            if (this._imp != null) this._imp.SaveToClipboard();
            else throw new MissingImplementationException("DiagramImplementation");
        }

        /// <summary>
        /// Changes the color of the specified class on the diagram to the specified color.
        /// </summary>
        /// <param name="thisClass">Class to be changed.</param>
        /// <param name="color">Color to assign to the class.</param>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal void SetClassColor (MEClass thisClass, ClassColor color)
        {
            if (this._imp != null) this._imp.SetClassColor(thisClass, color);
            else throw new MissingImplementationException("DiagramImplementation");
        }

        /// <summary>
        /// Layout and show a (new) diagram. Must be called after creation (and optionally adding some elements) in order to
        /// actually show the diagram to the user.
        /// </summary>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal void Show()
        {
            if (this._imp != null) this._imp.Show();
            else throw new MissingImplementationException("DiagramImplementation");
        }

        /// <summary>
        /// Updates the 'show connector stereotypes' property of the current diagram.
        /// </summary>
        /// <param name="mustShow">Set to 'true' to show connector stereotypes, 'false' otherwise.</param>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal void ShowConnectorStereotypes(bool mustShow)
        {
            if (this._imp != null) this._imp.ShowConnectorStereotypes(mustShow);
            else throw new MissingImplementationException("DiagramImplementation");
        }

        /// <summary>
        /// We need a destructor to assure that, whenever the interface object goes out of scope, the reference count
        /// to the implementation object is decreased. This facilitates proper management of implementation objects (multiple
        /// interfaces might reference the same implementation).
        /// </summary>
        ~Diagram()
        {
            Dispose(false);
        }

        /// <summary>
        /// This is the actual disposing interface, which takes case of structural removal of the implementation type when no longer
        /// needed.
        /// </summary>
        /// <param name="disposing">Set to 'true' when called directly. Set to 'false' when called from the finalizer.</param>
        private void Dispose(bool disposing)
        {
            //We attempt to decrease the reference count in all cases, independent of origin of call. 
            //But we only send data to the logger when called from 'normal' code...
            if (!this._disposed)
            {
                try
                {
                    if (this._imp != null)
                    {
                        this._imp.RemoveReference();
                        this._imp = null;
                    }
                    this._disposed = true;
                }
                catch { };   // Ignore any exceptions, no use in processing them here.
            }
        }
    }
}
