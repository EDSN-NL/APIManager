using System;
using System.Collections.Generic;
using Framework.Logging;
using Framework.View;
using Framework.Context;

namespace Framework.Model
{
    internal abstract class ModelImplementation
    {
        /// <summary>
        /// The dictionary is used to keep track of all created Model Element implementation objects. 
        /// It is sorted by type first and instance ID's second.
        /// The diagram list is used to keep track of diagram implementations.
        /// </summary>
        private SortedList<ModelElementType, SortedList<string, ModelElementImplementation>> _dictionary;
        private SortedList<int, DiagramImplementation> _diagramList;

        /// <summary>
        /// Create a new association instance between source and target classes. This is actually a wrapper for the
        /// class-based createAssociation, which facilitates a more 'logical' way of creation associations: instead
        /// of specifying 'half an endpoint' and calling createAssociation on the source class, we can now create
        /// two 'complete' endpoints and instruct the model to create the association. Both methods have the same
        /// effect though.
        /// </summary>
        /// <param name="source">Owner-side of the association (start).</param>
        /// <param name="target">Destination of the association (end).</param>
        /// <param name="type">The type of association (aggregation, specialization, composition, etc.).</param>
        /// <param name="name">Name of the association, could be an empty string.</param>
        /// <returns>Newly created association or NULL in case of errors.</returns>
        internal virtual MEAssociation CreateAssociation(EndpointDescriptor source, EndpointDescriptor destination, MEAssociation.AssociationType type, string name)
        {
            return source.EndPoint.CreateAssociation(source, destination, type, name);
        }

        /// <summary>
        /// Finds a class based on its name and a repository path.
        /// </summary>
        /// <param name="path">Path name, elements separated by ':' and max. 6 levels of depth.</param>
        /// <param name="className">Name of the class to find.</param>
        /// <returns>Retrieved class or NULL on errors / nothing found.</returns>
        internal abstract MEClass FindClass(string path, string className);

        /// <summary>
        /// Retrieve a data type by name and path from the repository. The path parameter specifies the 
        /// package in which we have to locate the type. This also defines whether we return a BDT, CDT or 
        /// PRIM data type. The meta-type is defined by the name (name should be unique within the package).
        /// </summary>
        /// <param name="path">Full path towards the package in which we have to search for the data type.
        /// Path elements must be separated by ':' characters.</param>
        /// <param name="typeName">The name of the type to select.</param>
        /// <returns>Retrieve data type or NULL on errors / nothing found.</returns>
        internal abstract MEDataType FindDataType(string path, string typeName);

        /// <summary>
        /// Finds an object by its name and a repository path.
        /// </summary>
        /// <param name="path">Path name, elements separated by ':' and max. 6 levels of depth.</param>
        /// <param name="objectName">Name of the object to find.</param>
        /// <returns>Retrieved object or NULL on errors / nothing found.</returns>
        internal abstract MEObject FindObject(string path, string className);

        /// <summary>
        /// Finds a package by its name and a repository path.
        /// </summary>
        /// <param name="path">Path name, elements separated by ':' and max. 6 levels of depth.</param>
        /// <param name="packageName">Name of the package to find.</param>
        /// <returns>Retrieved package or NULL on errors / nothing found.</returns>
        internal abstract MEPackage FindPackage(string path, string packageName);

        /// <summary>
        /// This function returns a list of all Classes that contain a tag with provided name and a tag value matching the provided value.
        /// </summary>
        /// <param name="tagName">Tag that must be present in the Class.</param>
        /// <param name="tagValue">Matching value string (query performs a 'like' on this value).</param>
        /// <returns>List of Classes that contain the specified tag and value.</returns>
        internal abstract List<MEClass> FindTaggedValue(string tagName, string tagValue);

        /// <summary>
        /// This method removes the contents of the dictionary and diagram list. Use with caution since calling the method will break
        /// all interface objects that are still around!
        /// </summary>
        internal virtual void Flush()
        {
            try
            {
                this._dictionary.Clear();
                this._diagramList.Clear();
            }
            catch (Exception exc)
            {
                Logger.WriteError("Framework.Model.ModelImplementation.flush >> Flushing failed because: " + exc.ToString());
            }
        }

        /// <summary>
        /// This function provides an efficient mechanism to obtain classes that are associated with the provided class. The function returns a list
        /// of all MEClass objects that are referenced from the provided source class, i.e. are at the 'receiving end' of an association.
        /// </summary>
        /// <param name="source">Class for which we want to obtain associated classes.</param>
        /// <param name="stereotype">Stereotype of the association.</param>
        /// <returns>List of associated classes (could be empty if none found).</returns>
        internal abstract List<MEClass> GetAssociatedClasses(MEClass source, string stereotype);

        /// <summary>
        /// Converts the given type identifier to the proper Data Type object. Based on the meta-type of the retrieved object,
        /// the returned type is constructed as either an MEDataType, MEEnumeratedType or an MEUnion.
        /// </summary>
        /// <param name="typeID">Repository object identifier, must be of a data type!</param>
        /// <returns>Appropriate data type object.</returns>
        internal abstract MEDataType GetDataType(int typeID);

        /// <summary>
        /// Converts the given type identifier to the proper Data Type object. Based on the meta-type of the retrieved object,
        /// the returned type is constructed as either an MEDataType, MEEnumeratedType or an MEUnion.
        /// </summary>
        /// <param name="typeGUID">Globally unique object identifier, must be of a data type!</param>
        /// <returns>Appropriate data type object.</returns>
        internal abstract MEDataType GetDataType(string typeGUID);

        /// <summary>
        /// Factory method must be implemented by derived tool-specific implementation objects and is responsible for the construction
        /// of new Diagram implementation objects.
        /// </summary>
        /// <param name="diagramID">Tool-specific unique instance ID of the diagram.</param>
        /// <returns>Diagram implementation or NULL in case of errors.</returns>
        internal abstract DiagramImplementation GetDiagramImplementation(int diagramID);

        /// <summary>
        /// Factory method must be implemented by derived tool-specific implementation objects and is responsible for the
        /// construction of proper Model Element implementation objects.
        /// </summary>
        /// <param name="type">Type of object to created.</param>
        /// <param name="elementID">Tool-specific unique instance ID.</param>
        /// <returns>Model Element implementation or NULL in case of errors.</returns>
        internal abstract ModelElementImplementation GetModelElementImplementation(ModelElementType type, int elementID);

        /// <summary>
        /// Factory method must be implemented by derived tool-specific implementation objects and is responsible for the
        /// construction of proper Model Element implementation objects, using a globally unique ID (GUID) as key.
        /// </summary>
        /// <param name="type">Type of object to created.</param>
        /// <param name="elementGUID">Tool-specific globally unique element ID (GUID).</param>
        /// <returns>Model Element implementation or NULL in case of errors.</returns>
        internal abstract ModelElementImplementation GetModelElementImplementation(ModelElementType type, string elementGUID);

        /// <summary>
        /// Returns the name of te currently opened model (EA project name).
        /// </summary>
        /// <returns>Name of currently opened project or empty string if unknown.</returns>
        internal abstract string GetModelName();

        /// <summary>
        /// Retrieves the model repository type. Typically, this requires a tool-specific implementation.
        /// </summary>
        /// <returns>Repository type enumeration.</returns>
        internal abstract ModelSlt.RepositoryType GetRepositoryType();

        /// <summary>
        /// Many model elements are specializations of another element type. In order to reduce the number of levels in the dictionary and
        /// to avoid redundant entries, we maintain the dictionary on 'root-types' only. This function receives a ModelElementType and
        /// returns the root type of that type in order to arrive at the correct dictionary entry.
        /// The method is implemented in this repository-specific module since we also have to assure that the root types exist in different
        /// tables within the repository in order to avoid ID overlap!
        /// </summary>
        /// <param name="type">Specialized element type.</param>
        /// <returns>Associated root element type or Unknown type if not found.</returns>
        internal abstract ModelElementType GetRootType(ModelElementType type);

        /// <summary>
        /// This method is called during startup of the plugin and must initialize all model-specific stuff.
        /// The method is called from the ModelSlt during 'bind'.
        /// </summary>
        internal virtual void Initialize()
        {
            if (this._dictionary == null)
            {
                this._dictionary = new SortedList<ModelElementType, SortedList<string, ModelElementImplementation>>();
                this._diagramList = new SortedList<int, DiagramImplementation>();
            }
        }

        /// <summary>
        /// Attempts to lock the model associated with the specified root package. The function checks the current locking status. If already locked
        /// by another user, an error is displayed. Otherwise, if locking failed (for whatever reason), an error is displayed.
        /// If the package is already locked by the current user, no action is performed.
        /// Note that we only check the locking status of the model root. This implies that things might go wrong in case lower-level items are 
        /// locked by another user!
        /// </summary>
        /// <returns>True if locked successfully.</returns>
        internal abstract bool LockModel(MEPackage modelRoot);

        /// <summary>
        /// Attempts to lock the specified package. The function checks the current locking status. If already locked
        /// by another user, an error is displayed. Otherwise, if locking failed (for whatever reason), an error is displayed.
        /// If the package is already locked by the current user, no action is performed.
        /// If security is not enabled on the repository, the function always returns 'true' but does not perform any actual operation!
        /// Depending on parameter 'recursiveLock', the function only locks the current package (recursiveLock is false), or the entire
        /// package structure (recursiveLock is true).
        /// This function does NOT use the 'AutomaticLocking' and 'PersistentModelLocks' configuration options.
        /// </summary>
        /// <param name="packagePath">Absolute path from the repository root to the package that must be locked (repository root NOT included).</param>
        /// <param name="recursiveLock">When set to 'true', the function will recursively lock all packages below the specified package.</param>
        /// <returns>True if locked successfully.</returns>
        internal abstract bool LockPackage(string packagePath, bool recursiveLock);

        /// <summary>
        /// Forces the repository implementation to refresh the entire model tree. This can be
        /// called after a number of model changes to assure that the model view is consistent with these changes.
        /// </summary>
        internal abstract void Refresh();

        /// <summary>
        /// This method is called during plugin shutdown and must release resources...
        /// The method is called from the ModelSlt during its shut-down cycle. It might be overriden by specializations.
        /// </summary>
        internal virtual void ShutDown ()
        {
            this._dictionary.Clear();
            this._diagramList.Clear();
            this._dictionary = null;
            this._diagramList = null;
        }

        /// <summary>
        /// Each DiagramImplementation object that is created by the framework must register itself with the model.
        /// Diagrams are registered by their repository ID.
        /// By registering the implementation object, it is possible to create arbitrary numbers of Diagram objects
        /// that all share the same implementation. Since the Diagram itself should not contain any statefull information,
        /// this is not an issue but can be advantageous since it simplifies object management for the interface objects.
        /// </summary>
        /// <param name="thisDiagram">The diagram implementation object that must be registered.</param>
        /// <returns>Reference to the actual registered diagram (could be different from 'thisDiagram' if it's a duplicate).</returns>
        internal DiagramImplementation RegisterDiagramImp(DiagramImplementation thisDiagram)
        {
            if (this._diagramList == null) return null;

            DiagramImplementation returnInstance = thisDiagram;
            if (this._diagramList.ContainsKey(thisDiagram.DiagramID))
            {
                Logger.WriteInfo("Framework.Model.ModelImplementation.registerDiagramImp >> Diagram with name '" + thisDiagram.Name +
                                    "' and key '" + thisDiagram.DiagramID + "' already registered!");
                returnInstance = this._diagramList[thisDiagram.DiagramID];
            }
            else
            {
                //Type has not been registered yet. Create an entry for it...
                this._diagramList.Add(thisDiagram.DiagramID, thisDiagram);
            }
            return returnInstance;
        }

        /// <summary>
        /// Each ModelElementImplementation object that is created by the framework must register itself with the model.
        /// Elements are registered by type (actually, by root-type) and by UniqueID.
        /// Since we now collect all objects of the same root-type in the same registry, we must add the actual base-type to
        /// the object key since sometimes objects are registered multiple times, by base- as well as derived types and it is not
        /// always possible to have a unified case. We thus create an object key consisting of the actual base-type of the class 
        /// in combination with the Object ID.
        /// By registering the implementation object, it is possible to create arbitrary numbers of ModelElement objects
        /// that all share the same implementation. Since the ModelElement itself should not contain any statefull information,
        /// this is not an issue but can be advantageous since it simplifies object management for the interface objects.
        /// </summary>
        /// <param name="thisElement">The model element implementation object that must be registered.</param>
        /// <returns>Reference to the actual registered element (could be different from 'thisElement' if it's a duplicate).</returns>
        internal ModelElementImplementation RegisterElementImp(ModelElementImplementation thisElement)
        {
            if (this._dictionary == null) return null;

            ModelElementImplementation returnInstance = thisElement;
            ModelElementType keyType = GetRootType(thisElement.Type);
            string objectKey = thisElement.Type.ToString() + "." + thisElement.ElementID.ToString();

            if (this._dictionary.ContainsKey(keyType))
            {
                if (!this._dictionary[keyType].ContainsKey(objectKey))
                {
                    this._dictionary[keyType].Add(objectKey, thisElement);
                }
                else
                {
                    Logger.WriteInfo("Framework.Model.ModelImplementation.registerElementImp >> ModelElement with name '" + thisElement.Name +
                                        "' and key '" + thisElement.ElementID + "' already registered!");
                    returnInstance = this._dictionary[keyType][objectKey];
                }
            }
            else
            {
                //Type has not been registered yet. Create an entry for it...
                var typeEntry = new SortedList<string, ModelElementImplementation>
                {
                    { objectKey, thisElement }
                };
                this._dictionary.Add(keyType, typeEntry);
            }
            return returnInstance;
        }

        /// <summary>
        /// Attempts to unlock the model defined by the specified root package. We only perform any actions in case Automatic Locking is enabled and
        /// we have not specified persistent locks.
        /// The function fails silently on errors.
        /// </summary>
        internal abstract void UnlockModel(MEPackage modelRoot);

        /// <summary>
        /// Attempts to unlock the specified package. 
        /// If security is not enabled on the repository, the function does not perform any actual operation!
        /// Depending on parameter 'recursiveUnLock', the function only unlocks the current package (recursiveUnLock is false), or the entire
        /// package structure (recursiveUnLock is true). If the package could not be found, we generate an error message.
        /// This function does NOT use the 'AutomaticLocking' and 'PersistentModelLocks' configuration options.
        /// </summary>
        /// <param name="packagePath">Absolute path from the repository root to the package that must be locked (repository root NOT included).</param>
        /// <param name="recursiveUnLock">When set to 'true', the function will recursively unlock all packages below the specified package.</param>
        internal abstract void UnlockPackage(string packagePath, bool recursiveUnLock);

        /// <summary>
        /// Removes a DiagramImplementation that has been registered earlier. Fails silently if the diagram is not registered.
        /// This method must be used with caution since it might corrupt the associations between interface Diagrams and the
        /// associated implementation objects! Register/Deregister is typically managed by the implementation objects themselves.
        /// </summary>
        /// <param name="thisDiagram">The DiagramImplementation to be un-registered.</param>
        internal void UnregisterDiagramImp(DiagramImplementation thisDiagram)
        {
            if (this._diagramList != null)
            {
                if (this._diagramList.ContainsKey(thisDiagram.DiagramID))
                {
                    this._diagramList.Remove(thisDiagram.DiagramID);
                }
                else
                {
                    //Type has not been registered yet. Can't remove anything...
                    Logger.WriteInfo("Framework.Model.ModelImplementation.unregisterDiagramImp >> DiagramImplementation [" + thisDiagram.DiagramID +"]  with name '" + thisDiagram.Name +
                                     "' can not be removed because no entry exists for the object");
                }
            }
        }

        /// <summary>
        /// Loads the model repository type. Typically, this requires a tool-specific implementation.
        /// </summary>
        /// <returns>Repository type enumeration.</returns>
        internal abstract  void SetRepositoryType(ModelSlt.RepositoryType type);

        /// <summary>
        /// Removes a ModelElementImplementation that has been registered earlier. Fails silently if the element is not registered.
        /// This method must be used with caution since it might corrupt the associations between interface ModelElements and the
        /// associated implementation objects! Register/Deregister is typically managed by the implementation objects themselves.
        /// </summary>
        /// <param name="thisElement">The ModelElementImplementation to be un-registered.</param>
        internal void UnregisterElementImp(ModelElementImplementation thisElement)
        {
            if (this._dictionary != null)
            {
                ModelElementType keyType = GetRootType(thisElement.Type);
                string objectKey = thisElement.Type.ToString() + "." + thisElement.ElementID.ToString();
                if (this._dictionary.ContainsKey(keyType))
                {
                    // Attempts to remove entry, fails silently if non existing.
                    this._dictionary[keyType].Remove(objectKey);
                }
                else
                {
                    //Type has not been registered yet. Can't remove anything...
                    Logger.WriteInfo("Framework.Model.ModelImplementation.unregisterElementImp >> ModelElementImplementation [" + thisElement.ElementID + "] with name '" + thisElement.Name +
                                     "' and type '" + thisElement.Type + "' can not be removed because no entry exists for the object");
                }
            }
        }

        /// <summary>
        /// Searches the collection of registered diagram implementations for the occurance of the 
        /// specified instance. If found, the instance is returned and if not found, the method returns NULL;
        /// </summary>
        /// <param name="instanceID">Unique instance key of diagram implementation to be retrieved.</param>
        /// <returns>Diagram object or NULL if nothing found.</returns>
        internal DiagramImplementation FindRegisteredDiagramImp(int instanceID)
        {
            return (this._diagramList.ContainsKey(instanceID)) ? this._diagramList[instanceID] : null;
        }

        /// <summary>
        /// Searches the collection of registered element implementations of the given type for the occurance of the 
        /// specified instance. If found, the instance is returned and if not found, the method returns NULL;
        /// </summary>
        /// <param name="type">Type of the element to be located.</param>
        /// <param name="instanceID">Unique instance key of model element implementation to be retrieved.</param>
        /// <returns>ModelElement object or NULL if nothing found.</returns>
        internal ModelElementImplementation FindRegisteredElementImp(ModelElementType type, int instanceID)
        {
            ModelElementType keyType = GetRootType(type);
            string objectKey = type.ToString() + "." + instanceID.ToString();
            return (this._dictionary.ContainsKey(keyType) && this._dictionary[keyType].ContainsKey(objectKey)) ? this._dictionary[keyType][objectKey] : null;
        }

        /// <summary>
        /// Default constructor makes sure that the Model Element Implementation dictionary and diagram lists are created.
        /// </summary>
        protected ModelImplementation()
        {
            this._dictionary = new SortedList<ModelElementType, SortedList<string, ModelElementImplementation>>();
            this._diagramList = new SortedList<int, DiagramImplementation>();
        }
    }
}
