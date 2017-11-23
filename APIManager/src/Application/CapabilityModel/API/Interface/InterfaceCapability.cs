using System.Collections.Generic;
using Framework.Model;
using Framework.Logging;
using Framework.Exceptions;
using Framework.Util;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// Implements an Interface Capability, e.g. Service containing operations containing request- and response messages.
    /// </summary>
    internal class InterfaceCapability: Capability
    {
        /// <summary>
        /// Returns the Common Schema that is associated with this Interface Capability.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal CommonSchemaCapability CommonSchema
        {
            get
            {
                if (this._imp != null) return ((InterfaceCapabilityImp)this._imp).CommonSchema;
                else throw new MissingImplementationException("InterfaceCapabilityImp");
            }
        }

        /// <summary>
        /// Create constructor, used to create a new instance of an Interface. The constructor assumes that the package structure
        /// exists and that there exists a service to which we can connect the new capability. The constructor creates the
        /// appropriate model elements in the correct packages and links stuff together. If no operationNames are specified, the
        /// constructor only creates the Interface, Common Schema and association with the service. Operations can be added seperately.
        /// It does NOT register itself with the Service, since this constructor is only used to create the appropriate model elements
        /// and does not support actual schema generation.
        /// <param name="myService">All capabilities are, directly or indirectly, always associated with a single Service.</param>
        /// <param name="capabilityName">Name of the interface to be created.</param>
        /// <param name="operationNames">Optional: the operations that this initial version must support.</param>
        /// </summary>
        internal InterfaceCapability(Service myService, string capabilityName, List<string> operationNames): base()
        {
            RegisterCapabilityImp(new InterfaceCapabilityImp(myService, capabilityName, operationNames));
        }

        /// <summary>
        /// Generic constructor to be used for existing class models. The constructor creates all of the subordinate objects (common schema and 
        /// all operations) and builds the complete object hierarchy, including registration with the parent Service.
        /// Please note that it is the responsibility of the CHILD to register with the PARENT and not the other way around. In other words: the
        /// operation capabilities will add themselves to the capability tree.
        /// The constructor receives a 'ready-built' hierarchy of all operations and messages through the 'hierarchy' property. The
        /// root-node represents the Interface.
        /// </summary>
        /// <param name="myService">Associated service instance.</param>
        /// <param name="hierarchy">Capability hierarchy for this Interface.</param>
        internal InterfaceCapability(Service myService, TreeNode<MEClass> hierarchy): base(hierarchy.Data.ElementID)
        {
            if (!Valid) RegisterCapabilityImp(new InterfaceCapabilityImp(myService, hierarchy));
        }

        /// <summary>
        /// Creates an Interface Capability interface object based on its MEClass object. This object MUST have been constructed earlier 
        /// (e.g. on instantiating a Service hierarchy). If the implementation could not be found, a MissingImplementationException is thrown.
        /// </summary>
        /// <param name="capabilityClass">Capability class for the Interface Capability.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal InterfaceCapability(MEClass capabilityClass) : base(capabilityClass) { }

        /// <summary>
        /// Create new Interface Capability Interface based on existing implementation...
        /// </summary>
        /// <param name="thisImp">Implementation to use.</param>
        internal InterfaceCapability(InterfaceCapabilityImp thisImp) : base(thisImp) { }

        /// <summary>
        /// Copy Constructor, using other interface as basis.
        /// </summary>
        /// <param name="thisCap">Interface to use as basis.</param>
        internal InterfaceCapability(InterfaceCapability thisCap) : base(thisCap) { }

        /// <summary>
        /// Add one or more operations to the current interface. We do not check whether the operation names are indeed unique, this should have
        /// been done before! 
        /// It is the responsibility of the operation to register with the parent capability tree.
        /// </summary>
        /// <param name="operationNames">List of operations that must be added.</param>
        /// <param name="newMinorVersion">True when a new minor version must be created.</param>
        /// <returns>True if operation completed successfully, false on errors.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal bool AddOperations (List<string> operationNames, bool newMinorVersion)
        {
            if (this._imp != null) return ((InterfaceCapabilityImp)this._imp).AddOperations(operationNames, newMinorVersion);
            else throw new MissingImplementationException("InterfaceCapabilityImp");
        }

        /// <summary>
        /// This method is called in order to link (associate) one or more Operation capabilities with the current Interface.
        /// Association implies registering the Operation as a child and creating an association between the Interface- and
        /// Operation classes. When 'newMinorVersion' is set to 'true', the version of the Interface, associated Common Schema 
        /// and the Service are all incremented.
        /// Finally, an appropriate log message is generated for the Interface, associated Common Schema and Service.
        /// </summary>
        /// <param name="operationList">One or more operations to associate with this Interface.</param>
        /// <param name="newMinorVersion">Set to 'true' if the minor version of the Interface must be bumped.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal void AssociateOperations (List<OperationCapability> operationList, bool newMinorVersion)
        {
            if (this._imp != null) ((InterfaceCapabilityImp)this._imp).AssociateOperations(operationList, newMinorVersion);
            else throw new MissingImplementationException("InterfaceCapabilityImp");
        }

        /// <summary>
        /// This method is called in order to link (associate) one or more Operation capability classes with the current interface.
        /// Association implies registering the Operation as a child and creating an association between the Interface- and
        /// Operation classes. When 'newMinorVersion' is set to 'true', the version of the Interface, associated Common Schema 
        /// and the Service are all incremented.
        /// Finally, an appropriate log message is generated for the Interface, associated Common Schema and Service.
        /// </summary>
        /// <param name="operationList">One or more operations to associate with this Interface.</param>
        /// <param name="newMinorVersion">Set to 'true' if the minor version of the Interface must be bumped.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal void AssociateOperations(List<MEClass> operationList, bool newMinorVersion)
        {
            if (this._imp != null) ((InterfaceCapabilityImp)this._imp).AssociateOperations(operationList, newMinorVersion);
            else throw new MissingImplementationException("InterfaceCapabilityImp");
        }

        /// <summary>
        /// Deletes the operation identified by the specified operation-class object. When the deleteResources indicator is set to 'true', the
        /// method physically deletes all resources (elements, packages, associations, etc.) that are used by the operation. When the indicator
        /// is set to 'false' (the default), the operation is just unlinked from the interface, but no resources are deleted.
        /// We do NOT update our service annotation and/or version info in this method since we do not know whether the delete has been executed
        /// one one or many interfaces. This could result in confusing log messages and/or incorrect versions so we leave the service update
        /// to the event code, which knows the context.
        /// </summary>
        /// <param name="operationClass">Identifies the operation to be deleted.</param>
        /// <param name="newMinorVersion">Set to 'true' when operation minor version must be updated, 'false' to keep existing version.</param>
        /// <param name="deleteResources">When set to 'true', the associated operation class with all related packages, elements, etc. are deleted 
        /// from the repository. When set to 'false' (default), the operation association is removed but all resources remain intact.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal void DeleteOperation (MEClass operationClass, bool newMinorVersion, bool deleteResources = false)
        {
            if (this._imp != null) ((InterfaceCapabilityImp)this._imp).DeleteOperation(operationClass, newMinorVersion, deleteResources);
            else throw new MissingImplementationException("InterfaceCapabilityImp");
        }

        /// <summary>
        /// Returns the list of all Operation capabilities associated to this Interface.
        /// </summary>
        /// <returns>List of Operation capabilities.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal List<OperationCapability> GetOperations()
        {
            if (this._imp != null) return ((InterfaceCapabilityImp)this._imp).GetOperations();
            else throw new MissingImplementationException("InterfaceCapabilityImp");
        }

        /// <summary>
        /// Renames the operation identified by the specified operation-class object.
        /// We do NOT update our service annotation and/or version info in this method since we do not know whether the rename has been executed
        /// one one or many interfaces. This could result in confusing log messages and/or incorrect versions so we leave the service update
        /// to the event code, which knows the context.
        /// </summary>
        /// <param name="operationClass">Operation to be renamed.</param>
        /// <param name="oldName">Original name of the operation.</param>
        /// <param name="newName">New name for the operation, in PascalCase.</param>
        /// <param name="newMinorVersion">Set to 'true' when operation minor version must be updated, 'false' to keep existing version.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal void RenameOperation(MEClass operationClass, string oldName, string newName, bool newMinorVersion)
        {
            if (this._imp != null) ((InterfaceCapabilityImp)this._imp).RenameOperation(operationClass, oldName, newName, newMinorVersion);
            else throw new MissingImplementationException("InterfaceCapabilityImp");
        }

        /// <summary>
        /// Protected constructor that is used by derived interface specializations that have an implementation object to register.
        /// </summary>
        /// <param name="elementID">Class ID of the derived interface MEClass object.</param>
        protected InterfaceCapability(int elementID) : base(elementID) { }

        /// <summary>
        /// Protected default constructor that is used by derived interface specializations that have to create an implemenetation.
        /// </summary>
        protected InterfaceCapability() : base() { }
    }
}
