using Framework.Model;
using Framework.Exceptions;
using Framework.Util;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// Represents an Operation within a service. Operations are associated with one or more Interface Capabilities (parents) and each Operation
    /// might have a Request- and Response Message Capability.
    /// </summary>
    internal class OperationCapability: Capability
    {
        /// <summary>
        /// Each Operation of a Service has a unique Operation identifier, which is a zero-based sequence number assigned during creation.
        /// </summary>
        internal int OperationID
        {
            get
            {
                if (this._imp != null) return ((OperationCapabilityImp)this._imp).OperationID;
                else throw new MissingImplementationException("InterfaceCapabilityImp");
            }
        }

        /// <summary>
        /// Returns the package that 'owns' the information model for this operation.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal MEPackage OperationPackage
        {
            get
            {
                if (this._imp != null) return ((OperationCapabilityImp)this._imp).OperationPackage;
                else throw new MissingImplementationException("InterfaceCapabilityImp");
            }
        }

        /// <summary>
        /// Returns the namespace token to be used for Operation-dependent schemas.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal string NSToken
        {
            get
            {
                if (this._imp != null) return ((OperationCapabilityImp)this._imp).NSToken;
                else throw new MissingImplementationException("InterfaceCapabilityImp");
            }
        }

        /// <summary>
        /// Returns the fully qualified namespace URI for the Operation.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal string FQName
        {
            get
            {
                if (this._imp != null) return ((OperationCapabilityImp)this._imp).FQName;
                else throw new MissingImplementationException("InterfaceCapabilityImp");
            }
        }

        /// <summary>
        /// Creates a new instance of an operation. The constructor creates the operation package and request- and response messages underneath this
        /// package. Corresponding classes are created in the Service Model.
        /// </summary>
        /// <param name="myInterface">The interface for which we're creating the operation.</param>
        /// <param name="operationName">The name of the operation.</param>
        internal OperationCapability(InterfaceCapability myInterface, string operationName): base()
        {
            RegisterCapabilityImp(new OperationCapabilityImp(myInterface, operationName));
        }

        /// <summary>
        /// Generic constructor to be used for existing class models. Th constructor initialises local context and creates the subordinate messages.
        /// </summary>
        /// <param name="myInterface">The interface for which we create the operation.</param>
        /// <param name="operationCapability">The associated operation class.</param>
        internal OperationCapability(InterfaceCapability myInterface, TreeNode<MEClass> hierarchy): base(hierarchy.Data.ElementID)
        {
            if (!Valid) RegisterCapabilityImp(new OperationCapabilityImp(myInterface, hierarchy));
        }

        /// <summary>
        /// Creates an Operation Capability interface object based on its MEClass object. This object MUST have been constructed earlier 
        /// (e.g. on instantiating a Service hierarchy). If the implementation could not be found, a MissingImplementationException is thrown.
        /// </summary>
        /// <param name="capabilityClass">Capability class for the Operation Capability.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal OperationCapability(MEClass capabilityClass) : base(capabilityClass) { }

        /// <summary>
        /// Create new Operation Capability Interface based on existing implementation...
        /// </summary>
        /// <param name="thisImp">Implementation to use.</param>
        internal OperationCapability(OperationCapabilityImp thisImp) : base(thisImp) { }

        /// <summary>
        /// Copy Constructor, using other interface as basis.
        /// </summary>
        /// <param name="thisCap">Interface to use as basis.</param>
        internal OperationCapability(OperationCapability thisCap) : base(thisCap) { }

        /// <summary>
        /// Associate the operation with the given Interface. This implies registering as a child of the interface and creating
        /// an association between the two classes. If 'newMinorVersion' is set to 'true', the minor version of the operation
        /// is incremented (we do NOT touch the versions of our parent interface and/or service since we do not know for sure
        /// what the scope of the call is (multiple operations can be involved).
        /// Finally, a log message is created for the operation.
        /// </summary>
        /// <param name="thisInterface">Interface with which we want to be associated.</param>
        /// <param name="newMinorVersion">Set to 'true' if the minor version must be incremented.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal void AssociateInterface(InterfaceCapability thisInterface, bool newMinorVersion)
        {
            if (this._imp != null) ((OperationCapabilityImp)this._imp).AssociateInterface(thisInterface, newMinorVersion);
            else throw new MissingImplementationException("OperationCapabilityImp");
        }

        /// <summary>
        /// Dissociate the Operation from the specified Interface. We do NOT check whether the Interface is indeed associated (thus, an exception will be
        /// thrown when the specified Interface is not in the list of associated Interfaces). If this was the ONLY interface associated with the Operation,
        /// the method deletes the Operation and all associated resources. Otherwise, if Interfaces remain associated, we create a log message for the 
        /// Operation. Since the operation invokes Interface.deleteChild, the association between Interface and Operation is deleted as well.
        /// </summary>
        /// <param name="thisInterface">Interface to dissociate.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal void DissociateInterface(InterfaceCapability thisInterface)
        {
            if (this._imp != null) ((OperationCapabilityImp)this._imp).DissociateInterface(thisInterface);
            else throw new MissingImplementationException("OperationCapabilityImp");
        }

        /// <summary>
        /// Protected constructor that is used by derived interface specializations that have an implementation object to register.
        /// </summary>
        /// <param name="elementID">Class ID of the derived interface MEClass object.</param>
        protected OperationCapability(int elementID) : base(elementID) { }

        /// <summary>
        /// Protected default constructor that is used by derived interface specializations that have to create an implemenetation.
        /// </summary>
        protected OperationCapability() : base() { }
    }
}
