using System.Collections.Generic;
using Framework.Model;
using Framework.Exceptions;
using Framework.Util;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// Implements a REST Interface Capability, e.g. Service containing REST Resources containing HTTP Operations. REST Interface
    /// is based on the generic (SOAP) Interface, alas without the implementation of Operations, since these are at a different level
    /// in case of REST.
    /// Since the REST Interface is capable of storing Resource capabilities, it also implements the IRESTResourceContainer interface.
    /// </summary>
    internal class RESTInterfaceCapability : InterfaceCapability, IRESTResourceContainer
    {
        /// <summary>
        /// Simple data structure used to pass meta data to the interface creation.
        /// </summary>
        internal struct MetaData
        {
            internal string qualifiedName;      // Qualified API Name, must include major version (e.g. "APIName_V2").
            internal string description;        // API Description.
            internal string termsOfService;     // API Terms of service description.
            internal string licenseName;        // API License description.
            internal string licenseURL;         // URL that refers to license details.
            internal string contactName;        // Name of API contact person.
            internal string contactEMail;       // E-Mail of API contact person.
            internal string contactURL;         // URL of API contact person.
        }

        /// <summary>
        /// Add one or more resources to the current interface. We do not check whether the names are indeed unique, 
        /// this should have been done during creation (e.g. in user interface).
        /// We don't attempt to filter on resource types since we might have valid reason to add each type of resource
        /// to the Interface (not just collections).
        /// It is the responsibility of the resource to register with the parent capability tree.
        /// </summary>
        /// <param name="resourceNames">List of resources that must be added.</param>
        /// <param name="newMinorVersion">True when a new minor version must be created.</param>
        /// <returns>True if operation completed successfully, false on errors.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        public bool AddResources(List<RESTResourceDeclaration> resources, bool newMinorVersion)
        {
            if (this._imp != null) return ((RESTInterfaceCapabilityImp)this._imp).AddResources(resources, newMinorVersion);
            else throw new MissingImplementationException("RESTInterfaceCapabilityImp");
        }

        /// <summary>
        /// Deletes the resource identified by the specified resource-class object. This well delete the entire resource hierarchy.
        /// </summary>
        /// <param name="resourceClass">Identifies the resource to be deleted.</param>
        /// <param name="newMinorVersion">Set to 'true' when minor version must be updated, 'false' to keep existing version.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        public void DeleteResource(MEClass resourceClass, bool newMinorVersion)
        {
            if (this._imp != null) ((RESTInterfaceCapabilityImp)this._imp).DeleteResource(resourceClass, newMinorVersion);
            else throw new MissingImplementationException("RESTInterfaceCapabilityImp");
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
            if (this._imp != null) ((RESTInterfaceCapabilityImp)this._imp).RenameResource(resourceClass, oldName, newName, newMinorVersion);
            else throw new MissingImplementationException("RESTInterfaceCapabilityImp");
        }

        /// <summary>
        /// Facilitates iteration over the set of resources associated with this interface.
        /// </summary>
        /// <returns>Resource Capability enumerator.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        public IEnumerable<RESTResourceCapability> ResourceList()
        {
            if (this._imp != null) return ((RESTInterfaceCapabilityImp)this._imp).ResourceList();
            else throw new MissingImplementationException("RESTInterfaceCapabilityImp");
        }

        /// <summary>
        /// Create constructor, used to create a new instance of an Interface. The constructor assumes that the package structure
        /// exists and that there exists a service to which we can connect the new capability. The constructor creates the
        /// appropriate model elements in the correct packages and links stuff together. If no collectionNames are specified, the
        /// constructor only creates the Interface, Common Schema and association with the service. Collections can be added seperately.
        /// It does NOT register itself with the Service, since this constructor is only used to create the appropriate model elements
        /// and does not support actual schema generation.
        /// <param name="myService">All capabilities are, directly or indirectly, always associated with a single Service.</param>
        /// <param name="metaData">Set of metadata properties for the new interface.</param>
        /// <param name="resources">Optional: the resource collections that this initial version must support.</param>
        /// </summary>
        internal RESTInterfaceCapability(Service myService, MetaData metaData, List<RESTResourceDeclaration> resources): base()
        {
            RegisterCapabilityImp(new RESTInterfaceCapabilityImp(myService, metaData, resources));
        }

        /// <summary>
        /// Generic constructor to be used for existing class models. The constructor creates all of the subordinate objects (common schema and 
        /// all collections) and builds the complete object hierarchy, including registration with the parent Service.
        /// Please note that it is the responsibility of the CHILD to register with the PARENT and not the other way around. In other words: the
        /// collection capabilities will add themselves to the capability tree.
        /// The constructor receives a 'ready-built' hierarchy of all collections and operations through the 'hierarchy' property. The
        /// root-node represents the Interface.
        /// </summary>
        /// <param name="myService">Associated service instance.</param>
        /// <param name="hierarchy">Capability hierarchy for this Interface.</param>
        internal RESTInterfaceCapability(Service myService, TreeNode<MEClass> hierarchy): base(hierarchy.Data.ElementID)
        {
            if (!Valid) RegisterCapabilityImp(new RESTInterfaceCapabilityImp(myService, hierarchy));
        }

        /// <summary>
        /// Creates an Interface Capability interface object based on its MEClass object. This object MUST have been constructed earlier 
        /// (e.g. on instantiating a Service hierarchy). If the implementation could not be found, a MissingImplementationException is thrown.
        /// </summary>
        /// <param name="capabilityClass">Capability class for the Interface Capability.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal RESTInterfaceCapability(MEClass capabilityClass) : base(capabilityClass) { }

        /// <summary>
        /// Create new Interface Capability Interface based on existing implementation...
        /// </summary>
        /// <param name="thisImp">Implementation to use.</param>
        internal RESTInterfaceCapability(RESTInterfaceCapabilityImp thisImp) : base(thisImp) { }

        /// <summary>
        /// Copy Constructor, using other interface as basis.
        /// </summary>
        /// <param name="thisCap">Interface to use as basis.</param>
        internal RESTInterfaceCapability(RESTInterfaceCapability thisCap) : base(thisCap) { }
    }
}
