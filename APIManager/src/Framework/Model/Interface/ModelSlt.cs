using System.Collections.Generic;
using Framework.Logging;
using Framework.View;
using Framework.Exceptions;

namespace Framework.Model
{
    /// <summary>
    /// This implements the Model Singleton interface class. Each model must have a, platform-dependent, implementation object
    /// that will perform the actual work.
    /// The model maintains a list of all ModelElements that have been created so-far. This list is sorted by ModelElement type
    /// and uniqueID of each element.
    /// </summary>
    internal sealed class ModelSlt
    {
        // This enumeration defines the type of repository that we're using. 'Local' implies local, tool-specific, storage and
        // the other definitions match respective SQL databases...
        internal enum RepositoryType { Local, MySQL, SQLServer, Oracle, PostgreSQL, Unknown }

        // This is the actual Model singleton. It is created automatically on first load.
        private static readonly ModelSlt _modelSlt = new ModelSlt();
        private ModelImplementation _modelImp;

        /// <summary>
        /// Getter and setter for the type of repository (typically, the type is determined outside model scope, e.g. when a
        /// model file is opened. This is implementation dependent. In case of Sparx EA, the type is determined by the controller.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal RepositoryType ModelRepositoryType
        {
            get
            {
                if (this._modelImp != null) return this._modelImp.GetRepositoryType();
                else throw new MissingImplementationException("ModelImplementation");
            }
            set
            {
                if (this._modelImp != null) this._modelImp.SetRepositoryType(value);
                else throw new MissingImplementationException("ModelImplementation");
            }
        }


        /// <summary>
        /// Create a new association instance between source and target.
        /// </summary>
        /// <param name="source">Owner-side of the association (start).</param>
        /// <param name="target">Destination of the association (end).</param>
        /// <param name="type">Type of association.</param>
        /// <param name="name">Optional name of the association, could be omitted.</param>
        /// <returns>Newly created association or NULL in case of errors.</returns>
        internal MEAssociation CreateAssociation(EndpointDescriptor source, EndpointDescriptor target, MEAssociation.AssociationType type, string name = null)
        {
            if (this._modelImp != null) return this._modelImp.CreateAssociation(source, target, type, name);
            else throw new MissingImplementationException("ModelImplementation");
        }

        /// <summary>
        /// Finds a class by its name and a repository path.
        /// </summary>
        /// <param name="path">Path name, elements separated by ':' and max. 6 levels of depth.</param>
        /// <param name="className">Name of the class to find.</param>
        /// <returns>Retrieved class or NULL on errors / nothing found.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal MEClass FindClass(string path, string className)
        {
            if (this._modelImp != null) return this._modelImp.FindClass(path, className);
            else throw new MissingImplementationException("ModelImplementation");
        }

        /// <summary>
        /// Retrieve a data type by name and path from the repository. The path parameter specifies the 
        /// package in which we have to locate the type. This also defines whether we return a BDT, CDT or 
        /// PRIM data type. The meta-type is defined by the name (name should be unique within the package).
        /// </summary>
        /// <param name="path">Full path towards the package in which we have to search for the data type.
        /// Path elements must be separated by ':' characters.</param>
        /// <param name="typeName">The name of the type to select.</param>
        /// <returns>Retrieve data type or NULL on errors / nothing found.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal MEDataType FindDataType(string path, string typeName)
        {
            if (this._modelImp != null) return this._modelImp.FindDataType(path, typeName);
            else throw new MissingImplementationException("ModelImplementation");
        }

        /// <summary>
        /// Finds an object by its name and a repository path.
        /// </summary>
        /// <param name="path">Path name, elements separated by ':' and max. 6 levels of depth.</param>
        /// <param name="objectName">Name of the object to find.</param>
        /// <returns>Retrieved object or NULL on errors / nothing found.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal MEObject FindObject(string path, string className)
        {
            if (this._modelImp != null) return this._modelImp.FindObject(path, className);
            else throw new MissingImplementationException("ModelImplementation");
        }

        /// <summary>
        /// Finds a package by its name and a repository path.
        /// </summary>
        /// <param name="path">Path name, elements separated by ':' and max. 6 levels of depth.</param>
        /// <param name="packageName">Name of the package to find.</param>
        /// <returns>Retrieved package or NULL on errors / nothing found.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal MEPackage FindPackage(string path, string packageName)
        {
            if (this._modelImp != null) return this._modelImp.FindPackage(path, packageName);
            else throw new MissingImplementationException("ModelImplementation");
        }

        /// <summary>
        /// Can be used to remove all context from the model. Since this involves cleaning the implementation caches, make sure
        /// that no interface objects are still around since these might break after the flush!
        /// </summary>
        internal void Flush()
        {
            if (this._modelImp != null) this._modelImp.Flush();
        }

        /// <summary>
        /// This function provides an efficient mechanism to obtain classes that are associated with the provided class. The function returns a list
        /// of all MEClass objects that are referenced from the provided source class, i.e. are at the 'receiving end' of an association.
        /// </summary>
        /// <param name="source">Class for which we want to obtain associated classes.</param>
        /// <param name="stereotype">The stereotype of the association.</param>
        /// <returns>List of associated classes (could be empty if none found).</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal List<MEClass> GetAssociatedClasses(MEClass source, string stereotype)
        {
            if (this._modelImp != null) return this._modelImp.GetAssociatedClasses(source, stereotype);
            else throw new MissingImplementationException("ModelImplementation");
        }

        /// <summary>
        /// Converts the given type identifier to the proper Data Type object. Based on the meta-type of the retrieved object,
        /// the returned type is constructed as either an MEDataType, MEEnumeratedType or an MEUnion.
        /// </summary>
        /// <param name="typeID">Repository object identifier, must be of a data type!</param>
        /// <returns>Appropriate data type object.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal MEDataType GetDataType(int typeID)
        {
            if (this._modelImp != null) return this._modelImp.GetDataType(typeID);
            else throw new MissingImplementationException("ModelImplementation");
        }

        /// <summary>
        /// Converts the given type identifier to the proper Data Type object. Based on the meta-type of the retrieved object,
        /// the returned type is constructed as either an MEDataType, MEEnumeratedType or an MEUnion.
        /// </summary>
        /// <param name="typeGUID">Globally unique object identifier, must be of a data type!</param>
        /// <returns>Appropriate data type object.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal MEDataType GetDataType(string typeGUID)
        {
            if (this._modelImp != null) return this._modelImp.GetDataType(typeGUID);
            else throw new MissingImplementationException("ModelImplementation");
        }

        /// <summary>
        /// Factory method for the construction of DiagramImlementation objects according to Diagram ID.
        /// </summary>
        /// <param name="diagramID">The unique tool-specific diagram identifier.</param>
        /// <returns>Diagram implementation object or NULL in case of errors.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal DiagramImplementation GetDiagramImplementation(int diagramID)
        {
            if (this._modelImp != null) return this._modelImp.GetDiagramImplementation(diagramID);
            else throw new MissingImplementationException("ModelImplementation");
        }

        /// <summary>
        /// Public Model "factory" method. Simply returns the static instance.
        /// </summary>
        /// <returns>Context singleton object</returns>
        internal static ModelSlt GetModelSlt() { return _modelSlt; }

        /// <summary>
        /// This is a factory method that constructs the proper ModelElementImplementation object according to the provided type and
        /// object ID.
        /// </summary>
        /// <param name="type">The requested model element type.</param>
        /// <param name="elementID">The unique and tool-specific element identifier.</param>
        /// <returns>Proper implementation object or NULL in case of errors.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal ModelElementImplementation GetModelElementImplementation(ModelElementType type, int elementID)
        {
            if (this._modelImp != null) return this._modelImp.GetModelElementImplementation(type, elementID);
            else throw new MissingImplementationException("ModelImplementation");
        }

        /// <summary>
        /// This is a factory method that constructs the proper ModelElementImplementation object according to the provided type and
        /// globally unique object ID (GUID).
        /// </summary>
        /// <param name="type">The requested model element type.</param>
        /// <param name="elementGUID">The globally unique element identifier.</param>
        /// <returns>Proper implementation object or NULL in case of errors.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal ModelElementImplementation GetModelElementImplementation(ModelElementType type, string elementGUID)
        {
            if (this._modelImp != null) return this._modelImp.GetModelElementImplementation(type, elementGUID);
            else throw new MissingImplementationException("ModelImplementation");
        }

        /// <summary>
        /// This method is called during startup of the plugin and must initialize all model-specific stuff.
        /// The method is called from the ControllerSlt during its initialization cycle.
        /// <param name="imp">Model implementation to be used.</param>
        /// </summary>
        internal void Initialize(ModelImplementation imp)
        {
            Logger.WriteInfo("Framework.Model.ModelSlt.initialize >> Initializing...");
            this._modelImp = imp;
            if (this._modelImp != null) this._modelImp.Initialize();
        }

        /// <summary>
        /// Forces the repository implementation to refresh the entire model tree. 
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal void Refresh()
        {
            if (this._modelImp != null) this._modelImp.Refresh();
            else throw new MissingImplementationException("ModelImplementation");
        }

        /// <summary>
        /// This method is called during plugin shutdown and must release resources...
        /// The method is called from the ControllerSlt during its shut-down cycle.
        /// </summary>
        internal void ShutDown()
        {
            Logger.WriteInfo("Framework.Model.ModelSlt.shutDown >> Shutting down...");
            if (this._modelImp != null) this._modelImp.ShutDown();
            this._modelImp = null;
        }

        /// <summary>
        /// The private constructor is called once on initial load and assures that exactly one valid object is present at all times.
        /// </summary>
        private ModelSlt()
        {
            this._modelImp = null;     // No implementation for now.
        }
    }
}
