using System.Collections.Generic;
using Framework.Model;
using Framework.Exceptions;
using Framework.Util;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// Represents a Resource in a REST service. Resources can exist on root- or intermediate levels in a resource model
    /// and can contain operations as well as Path Expressions as children (depending on their archetype).
    /// A Resource can have an empty name, which means that it does not appear as part of the URL. However, in the UML model,
    /// it must ALWAYS exist as a model element! Empty resources are identified by a reserved name "[Empty]". The use of angle brackets 
    /// facilitates introduction of multiple reserved names in the future.
    /// Example: "https://api.enexis.nl/customers/v1/{CustomerID}.
    /// Separator characters ('/') must NEVER be specified in the names of resources, these will be added automatically when needed.
    /// Depending on the archetype of the resource, some restrictions apply:
    /// 1) Collection and Store resources MUST own at least ONE PathExpression object and SHALL NOT be used at the end of an URL.
    /// 2) Controller resources MUST ONLY exist at the end of an URL and SHALL NOT contain PathExpressions.
    /// 3) Document resources MUST ONLY exist at the end of a REST Flow, are NOT displayed as part of the URL and thus CAN NOT have any children.
    /// 4) Identifier resources are not so much actual resources, but represent a resource instance (primary key).
    /// </summary>
    internal class RESTResourceCapability: Capability, IRESTResourceContainer
    {
        // This is used to specify the generic resource type that is represented by this resource within a given REST flow...
        internal enum ResourceArchetype { Collection, Controller, Document, Identifier, Store, Unknown }

        /// <summary>
        /// Add one or more child resources to the current resource. We do not check whether the names are indeed unique, 
        /// this should have been done during creation (e.g. in user interface).
        /// We don't attempt to filter on resource types since we might have valid reason to add each type of resource
        /// to the parent (not just collections).
        /// It is the responsibility of the resource to register with the parent capability tree.
        /// </summary>
        /// <param name="resourceNames">List of resources that must be added.</param>
        /// <param name="newMinorVersion">True when a new minor version must be created.</param>
        /// <returns>True if operation completed successfully, false on errors.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        public bool AddResources(List<RESTResourceDeclaration> resources, bool newMinorVersion)
        {
            if (this._imp != null) return ((RESTResourceCapabilityImp)this._imp).AddResources(resources, newMinorVersion);
            else throw new MissingImplementationException("RESTResourceCapabilityImp");
        }

        /// <summary>
        /// Deletes the resource identified by the specified resource-class object. This well delete the entire resource hierarchy.
        /// </summary>
        /// <param name="resourceClass">Identifies the resource to be deleted.</param>
        /// <param name="newMinorVersion">Set to 'true' when minor version must be updated, 'false' to keep existing version.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        public void DeleteResource(MEClass resourceClass, bool newMinorVersion)
        {
            if (this._imp != null) ((RESTResourceCapabilityImp)this._imp).DeleteResource(resourceClass, newMinorVersion);
            else throw new MissingImplementationException("RESTResourceCapabilityImp");
        }

        /// <summary>
        /// Renames the resource identified by the specified resource-class object.
        /// </summary>
        /// <param name="resourceClass">Collection to be renamed.</param>
        /// <param name="oldName">Original name of the collection.</param>
        /// <param name="newName">New name for the collection, in PascalCase.</param>
        /// <param name="newMinorVersion">Set to 'true' when minor version must be updated, 'false' to keep existing version.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        public void RenameResource(MEClass resourceClass, string oldName, string newName, bool newMinorVersion)
        {
            if (this._imp != null) ((RESTResourceCapabilityImp)this._imp).RenameResource(resourceClass, oldName, newName, newMinorVersion);
            else throw new MissingImplementationException("RESTResourceCapabilityImp");
        }

        /// <summary>
        /// Facilitates iteration over the set of child resources associated with this resource.
        /// </summary>
        /// <returns>Resource Capability enumerator.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        public IEnumerable<RESTResourceCapability> ResourceList()
        {
            if (this._imp != null) return ((RESTResourceCapabilityImp)this._imp).ResourceList();
            else throw new MissingImplementationException("RESTResourceCapabilityImp");
        }

        /// <summary>
        /// Returns the list of all child resource capabilities associated with this resource.
        /// </summary>
        /// <returns>List of REST-Resource capabilities.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        public List<RESTResourceCapability> GetResources()
        {
            if (this._imp != null) return ((RESTResourceCapabilityImp)this._imp).GetResources();
            else throw new MissingImplementationException("RESTResourceCapabilityImp");
        }

        /// <summary>
        /// Returns true if the resource represents resource collection (container of other resources).
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal bool IsCollection
        {
            get
            {
                if (this._imp != null) return ((RESTResourceCapabilityImp)this._imp).IsCollection;
                else throw new MissingImplementationException("RESTResourceCapabilityImp");
            }
        }

        /// <summary>
        /// Returns true if the resource represents a sub-API (resource directly at top-level of the API).
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal bool IsRootLevel
        {
            get
            {
                if (this._imp != null) return ((RESTResourceCapabilityImp)this._imp).IsRootLevel;
                else throw new MissingImplementationException("RESTResourceCapabilityImp");
            }
        }

        /// <summary>
        /// Returns the archetype of this resource.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal ResourceArchetype Archetype
        {
            get
            {
                if (this._imp != null) return ((RESTResourceCapabilityImp)this._imp).Archetype;
                else throw new MissingImplementationException("RESTResourceCapabilityImp");
            }
        }

        /// <summary>
        /// Returns the package that 'owns' the information model for this resource if it has archetype Collection or Store. 
        /// Each collection-type resource has its own package, which is a child of the Service Model. All collection names MUST be 
        /// unique across an entire REST API!
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal MEPackage ResourceCollectionPackage
        {
            get
            {
                if (this._imp != null) return ((RESTResourceCapabilityImp)this._imp).ResourcePackage;
                else throw new MissingImplementationException("RESTResourceCapabilityImp");
            }
        }

        /// <summary>
        /// Generic constructor to be used for existing class models. The constructor initialises a root resource, that is, a resource that
        /// is directly linked to an Interface.
        /// </summary>
        /// <param name="myInterface">The interface for which we create the resource.</param>
        /// <param name="hierarchy">Class hierarchy consisting of Resource- and associated Path- and Operation objects.</param>
        internal RESTResourceCapability(RESTInterfaceCapability myInterface, TreeNode<MEClass> hierarchy): base(hierarchy.Data.ElementID)
        {
            if (!Valid) RegisterCapabilityImp(new RESTResourceCapabilityImp(myInterface, hierarchy));
        }

        /// <summary>
        /// Generic constructor to be used for existing class models. The constructor initialises an intermediate resource, that is, a resource that
        /// is a child of another resource.
        /// </summary>
        /// <param name="parentResource">The resource that acts as a parent for this resource.</param>
        /// <param name="hierarchy">Class hierarchy consisting of Resource- and associated child Resources and Operation objects.</param>
        internal RESTResourceCapability(RESTResourceCapability parentResource, TreeNode<MEClass> hierarchy) : base(hierarchy.Data.ElementID)
        {
            if (!Valid) RegisterCapabilityImp(new RESTResourceCapabilityImp(parentResource, hierarchy));
        }

        /// <summary>
        /// Creates a new resource based on a resource declaration object. This object contains all the information necessary to create 
        /// the associated model elements and attributes. If the resource declaration contains an existing class, we ignore all other
        /// information and simply register a new interface for that class ("shared resource").
        /// </summary>
        /// <param name="myInterface">Interface capability that acts as parent for the resource.</param>
        /// <param name="operation">Resource declaration object, created by user and containing all necessary information.</param>
        internal RESTResourceCapability(RESTInterfaceCapability myInterface, RESTResourceDeclaration resource) : base(resource.ResourceClass)
        {
            if (resource.ResourceClass != null)
            {
                // When a valid class is present, the base constructor has already registered the associated implementation and the
                // only thing for us to do is to assign an (additional) parent class...
                ((RESTResourceCapabilityImp)this._imp).AssignParent(myInterface);
            }
            else RegisterCapabilityImp(new RESTResourceCapabilityImp(myInterface, resource));
        }

        /// <summary>
        /// Creates a new resource based on a resource declaration object. This object contains all the information necessary to create 
        /// the associated model elements and attributes. If the resource declaration contains an existing class, we ignore all other
        /// information and simply register a new interface for that class ("shared resource").
        /// </summary>
        /// <param name="parentResource">Resource that acts as parent for the resource.</param>
        /// <param name="resource">Resource declaration object, created by user and containing all necessary information.</param>
        internal RESTResourceCapability(RESTResourceCapability parentResource, RESTResourceDeclaration resource): base(resource.ResourceClass)
        {
            if (resource.ResourceClass != null)
            {
                // When a valid class is present, the base constructor has already registered the associated implementation and the
                // only thing for us to do is to assign an additional parent class...
                ((RESTResourceCapabilityImp)this._imp).AssignParent(parentResource);
            }
            else RegisterCapabilityImp(new RESTResourceCapabilityImp(parentResource, resource));
        }

        /// <summary>
        /// Creates an Resource Capability object based on its MEClass object. This object MUST have been constructed earlier 
        /// (e.g. on instantiating a Service hierarchy). If the implementation could not be found, a MissingImplementationException is thrown.
        /// </summary>
        /// <param name="capabilityClass">Capability class for the Resource Capability.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal RESTResourceCapability(MEClass capabilityClass) : base(capabilityClass) { }

        /// <summary>
        /// Create new Resource Capability object based on existing implementation...
        /// </summary>
        /// <param name="thisImp">Implementation to use.</param>
        internal RESTResourceCapability(RESTResourceCapabilityImp thisImp) : base(thisImp) { }

        /// <summary>
        /// Copy Constructor, using other resource object as basis.
        /// </summary>
        /// <param name="thisCap">resource to use as basis.</param>
        internal RESTResourceCapability(RESTResourceCapability thisCap) : base(thisCap) { }

        /// <summary>
        /// Create- and add a new operation to the current resource. The operation is specified using its Operation Declaration object 
        /// and we do not check whether the operation is indeed unique, this should have been done during creation (e.g. in user interface).
        /// It is the responsibility of the operation to register with the parent capability.
        /// </summary>
        /// <param name="operation">New operation that must be added.</param>
        /// <param name="newMinorVersion">True when a new minor version must be created.</param>
        /// <returns>True if operation completed successfully, false on errors.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        public bool AddOperation(RESTOperationDeclaration operation, bool newMinorVersion)
        {
            if (this._imp != null) return ((RESTResourceCapabilityImp)this._imp).AddOperation(operation, newMinorVersion);
            else throw new MissingImplementationException("RESTResourceCapabilityImp");
        }

        /// <summary>
        /// This function checks whether this resource has at least one associated operation.
        /// </summary>
        /// <returns>True if the resource contains at least one operation.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal bool HasOperations()
        {
            if (this._imp != null) return ((RESTResourceCapabilityImp)this._imp).HasOperations();
            else throw new MissingImplementationException("RESTResourceCapabilityImp");
        }
    }
}
