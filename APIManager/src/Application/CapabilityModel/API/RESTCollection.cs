using System;
using Framework.Model;
using Framework.Logging;
using Framework.Context;
using Framework.Util;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// Helper class that acts as the (abstract) base class for collections. Collections can be maintained at Operation-, API- or Global
    /// scope and are used to group specific sets of codes/parameters that we want to apply to API operations.
    /// </summary>
    internal abstract class RESTCollection
    {
        /// <summary>
        /// Collections can have any of the following scopes:
        /// Global = ECDM-wide template that can be used for any API. These are stored as part of the Framework.
        /// API = API-specific template that can be used for any operation within the associated API. These are stored as part of the ServiceModel.
        /// Operation = Operation-specific collections.
        /// If no scope is defined, the default is always 'API'. The scope is defined during construction and can not be changed afterwards!
        /// </summary>
        internal enum CollectionScope { Unknown, Global, API, Operation }

        // Configuration properties used by this module:
        private const string _RESTCollectionIDTag                   = "RESTCollectionIDTag";
        private const string _RESTCollectionScopeTag                = "RESTCollectionScopeTag";

        private MEPackage _owningPackage;                           // The package in which the collection lives.
        private RESTResourceCapability _parentResource;             // For operation-scoped collections, this is the declaration of the parent resource that contains the operation.
        private CollectionScope _scope;                             // Scope of this collection.
        private string _collectionID;                               // Collection identifier.
        private bool _isLocked;                                     // Set to 'true' when global scope and locked.

        // Since context management of the collection is implementation dependent, we assign a number of properties as protected properties so
        // we can read/write these at parent level, yet manage them by specialized classes...
        protected string _name;                                     // Collection name, identical to collectionClass name (when one is present).
        protected MEClass _collectionClass;                         // UML representation of the collection.

        abstract protected void SetName(string name);               // Must be implemented by specialized classes to assign a (new) name to the collection and, when appropriate, create the class.

        /// <summary>
        /// Returns the UML class used to represent the collection within the model.
        /// </summary>
        internal MEClass CollectionClass { get { return this._collectionClass; } }

        /// <summary>
        /// Returns the unique collection identifier.
        /// </summary>
        internal string CollectionID { get { return this._collectionID; } }

        /// <summary>
        /// Get or Set the name of the collection. Note that, if we assign a name to an object that is not yet associated with
        /// an MEClass, we create the class and assign any attributes that are already in the collection.
        /// And if we assign a new name to a collection and a class already exists with that name, an exception will be thrown.
        /// In case we don't have a valid context (no class and package), the name is stored locally only.
        /// </summary>
        /// <exception cref="ArgumentException">Will be thrown when a class already exists with the given name.</exception>
        internal string Name
        {
            get { return this._collectionClass != null? this._collectionClass.Name: string.Empty; }
            set { SetName(value); }
        }

        /// <summary>
        /// Returns the resource that 'owns' the operation that in turn owns the collection. Valid ONLY for 'Operation' scoped collections.
        /// Returns NULL if no operation is (yet) defined.
        /// </summary>
        internal RESTResourceCapability ParentResource { get { return this._parentResource; } }

        /// <summary>
        /// Returns the package in which the collection lives.
        /// </summary>
        internal MEPackage OwningPackage { get { return this._owningPackage; } }

        /// <summary>
        /// Returns the collection scope.
        /// </summary>
        internal CollectionScope Scope {  get { return this._scope; } }

        /// <summary>
        /// Basic constructor, which assigns a number of generic properties and leaves the remainder of the construction to the specialized class.
        /// </summary>
        /// <param name="parent">For operation-scoped collections, this is the resource owning the operation. Must be NULL for template collections.</param>
        /// <param name="collectionName">Name to be assigned to the collection.</param>
        /// <param name="package">Package that must contain the collection. The location of the package determines the scope of the
        /// collection: if this is a ServiceModel package, the scope is 'API'. If the package is an Operation-type, the scope is 'Operation'
        /// and all others are considered 'Global'.</param>
        /// <exception cref="InvalidOperationException">Is thrown when we can't find the attribute classifier.</exception>
        internal RESTCollection(RESTResourceCapability parent, string collectionName, MEPackage package)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTCollection >> Creating instance '" +
                             collectionName + "' in Package '" + package.Parent.Name + "/" + package.Name + "'...");
            this._owningPackage = package;
            this._name = collectionName;
            this._isLocked = false;
            this._parentResource = parent;
            this._collectionID = string.Empty;
            this._scope = CollectionScope.Unknown;
        }

        /// <summary>
        /// This constructor is called with an existing Collection class and initialises the collection from that class.
        /// </summary>
        /// <param name="parent">For operation-scoped collections, this is the resource owning the operation. Must be NULL for template collections.</param>
        /// <param name="collectionClass">Collection class.</param>
        /// <exception cref="InvalidOperationException">Thrown when a collection class is passed that is not of the correct stereotype or 
        /// when we can't find the correct attribute classifier.</exception>
        internal RESTCollection(RESTResourceCapability parent, MEClass collectionClass)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTCollection >> Creating instance from class '" +
                              collectionClass.Name + "'...");
            ContextSlt context = ContextSlt.GetContextSlt();
            this._isLocked = false;
            this._collectionClass = collectionClass;
            this._owningPackage = collectionClass.OwningPackage;
            this._name = collectionClass.Name;
            this._parentResource = parent;
        }

        /// <summary>
        /// The default constructor is used only when we want to create new (template) collections. In this case, we want to assign owning
        /// package, name and contents iteratively.
        /// </summary>
        /// <param name="parent">For non-template collections, this is the parent resource owning the collection.</param>
        /// <param name="initialScope">Contains the initial scope for the collection.</param>
        internal RESTCollection(RESTResourceCapability parent, CollectionScope initialScope = CollectionScope.Unknown)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTCollection >> Default constructor.");
            this._isLocked = false;
            this._collectionClass = null;
            this._owningPackage = null;
            this._name = string.Empty;
            this._parentResource = parent;
            this._collectionID = string.Empty;
            this._scope = initialScope;
        }

        /// <summary>
        /// This function is invoked when the entire collection has to be destroyed.
        /// Any exceptions are ignored and on return, the collection class and -name are cleared. Other parameters remain intact,
        /// allowing the class to be re-initialized when needed.
        /// The function is declared virtual to allow specialized classes to implement alternative logic.
        /// </summary>
        internal virtual void DeleteCollection()
        {
            try
            {
                if (this._collectionClass != null)
                {
                    LockCollection();
                    OwningPackage.DeleteClass(this._collectionClass);
                    this._collectionClass = null;
                }
                this._name = string.Empty;
            }
            catch (Exception exc)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTCollection.DeleteCollection >> Caught an exception: " +
                                 Environment.NewLine + exc.ToString());
            }
            finally { UnlockCollection(); }
        }

        /// <summary>
        /// Should be used to finalize creation of a new collection and write contents to the model. T
        /// </summary>
        /// <param name="name">The name to be assigned to the collection.</param>
        /// <param name="package">The package that must hold the collection.</param>
        /// <param name="scope">The new collection scope.</param>
        /// <exception cref="InvalidOperationException">Is thrown when the collection has a scope other then 'Unknown'.</exception>
        internal void Serialize(string name, MEPackage package, CollectionScope scope)
        {
            this._owningPackage = package;
            this._scope = scope;
            SetName(name);
        }

        /// <summary>
        /// Helper function that creates a new collection class within the owning package and subsequently initializes the ID and scope.
        /// Precondition is that the 'name' property has a valid value.
        /// </summary>
        /// <param name="stereotype">Specialized-class dependent stereotype to be assigned to the new class.</param>
        /// <exception cref="InvalidOperationException">Thrown when an attempt is made to create a class without a valid name.</exception>
        protected void CreateCollectionClass(string stereotype)
        {
            if (!string.IsNullOrEmpty(this._name))
            {
                this._collectionClass = this._owningPackage.CreateClass(this._name, stereotype);
                this._collectionID = this._collectionClass.ElementID.ToString();
                this._collectionClass.SetTag(ContextSlt.GetContextSlt().GetConfigProperty(_RESTCollectionIDTag), this._collectionID);
                this._collectionClass.SetTag(ContextSlt.GetContextSlt().GetConfigProperty(_RESTCollectionScopeTag), EnumConversions<CollectionScope>.EnumToString(this._scope));
            }
            else
            {
                string message = "Attempt to create a new collection class without valid name!";
                Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTCollection.CreateCollectionClass >> " + message);
                throw new InvalidOperationException(message);
            }
        }

        /// <summary>
        /// Helper function that initializes collection scope- and ID from the collection class.
        /// Precondition is that collectionClass is present!
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the collection class has not yet been loaded.</exception>
        protected void InitCollectionClass()
        {
            if (this._collectionClass != null)
            {
                ContextSlt context = ContextSlt.GetContextSlt();
                this._collectionID = this._collectionClass.GetTag(context.GetConfigProperty(_RESTCollectionIDTag));
                this._scope = EnumConversions<CollectionScope>.StringToEnum(this._collectionClass.GetTag(context.GetConfigProperty(_RESTCollectionScopeTag)));
            }
            else
            {
                string message = "Attempt to initialize an non-existent class!";
                Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTCollection.InitCollectionClass >> " + message);
                throw new InvalidOperationException(message);
            }
        }

        /// <summary>
        /// If we have a global collection, the function locks the collection package for changes.
        /// </summary>
        protected void LockCollection()
        {
            if (this._owningPackage != null && this._scope == CollectionScope.Global && !this._isLocked)
            {
                this._owningPackage.Lock();
                this._isLocked = true;
            }
        }

        /// <summary>
        /// Helper function that loads a (new) value in the 'scope' property. If we have a collection class, the scope value is written
        /// to this class as well.
        /// </summary>
        /// <param name="newScope">New value for the scope property.</param>
        protected void SetScope(RESTCollection.CollectionScope newScope)
        {
            this._scope = newScope;
            if (this._collectionClass != null)
            {
                this._collectionClass.SetTag(ContextSlt.GetContextSlt().GetConfigProperty(_RESTCollectionScopeTag), 
                                             EnumConversions<CollectionScope>.EnumToString(this._scope));
            }
        }

        /// <summary>
        /// If we have a global collection, the function unlocks the collection package (we don't check whether the package is indeed locked).
        /// </summary>
        protected void UnlockCollection()
        {
            if (this._owningPackage != null && this._scope == CollectionScope.Global && this._isLocked)
            {
                this._owningPackage.Unlock();
                this._isLocked = false;
            }
        }
    }
}
