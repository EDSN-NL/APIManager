using System;
using Framework.Model;
using Framework.Exceptions;
using Framework.Util;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// Represents a REST Operation, which must be one of the supported HTTP operation types. Each REST Operation is associated with either
    /// a Resource Collection or a Path Expression and each must have a name that is unique across the entire API definition. So, we can have
    /// multiple 'Get', 'Post', etc. command types but all have unique names. This implies that the command structure can follow the same
    /// rules as laid down for SOAP operations with some minor differences:
    /// REST Commands 
    /// </summary>
    internal class RESTOperationResultCapability: Capability
    {
        // Identifies the various HTTP Operation Result categories (Unknown is added only to specify an 'unknown' category and must never
        // be used in actual responses)...
        // We explicitly assign numeric values to the enumeration so that they match the HTTP prefix codes.
        // 1 = Request received, continuing process.
        // 2 = The action was successfully received, understood, and accepted.
        // 3 = Further action must be taken in order to complete the request.
        // 4 = The request contains bad syntax or cannot be fulfilled.
        // 5 = The server failed to fulfill an apparently valid request.
        // Default = Will cover all not-explicitly-defined result categories/codes.
        internal enum ResponseCategory { Unknown = 0, Informational = 1, Success = 2, Redirection = 3, ClientError = 4, ServerError = 5, Default }

        /// <summary>
        /// Constructor to be used for existing REST Operation Results associated with an Operation. The constructor receives
        /// the existing operation result class and builds an in-memory representation for further processing.
        /// </summary>
        /// <param name="parent">The operation for which we create the operation result.</param>
        /// <param name="operation">The associated operation result class.</param>
        internal RESTOperationResultCapability(RESTOperationCapability parent, MEClass result) : base(result.ElementID)
        {
            if (!Valid) RegisterCapabilityImp(new RESTOperationResultCapabilityImp(parent, result));
        }

        /// <summary>
        /// Creates a new operation result based on an operation result declaration object. This object contains all the information necessary 
        /// to create the associated model elements and attributes.
        /// </summary>
        /// <param name="parent">Resource that acts as parent for the operation.</param>
        /// <param name="operation">Operation declaration object, created by user and containing all necessary information.</param>
        internal RESTOperationResultCapability(RESTOperationCapability parent, RESTOperationResultDeclaration result): base()
        {
            RegisterCapabilityImp(new RESTOperationResultCapabilityImp(parent, result));
        }

        /// <summary>
        /// Creates a REST Operation Result Capability interface object based on its MEClass object. This object MUST have been constructed earlier 
        /// (e.g. on instantiating a Service hierarchy). If the implementation could not be found, a MissingImplementationException is thrown.
        /// </summary>
        /// <param name="capabilityClass">Capability class for the Operation Result Capability.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal RESTOperationResultCapability(MEClass capabilityClass) : base(capabilityClass) { }

        /// <summary>
        /// Create new Operation Result Capability Interface based on existing implementation...
        /// </summary>
        /// <param name="thisImp">Implementation to use.</param>
        internal RESTOperationResultCapability(RESTOperationResultCapabilityImp thisImp) : base(thisImp) { }

        /// <summary>
        /// Copy Constructor, using other interface as basis.
        /// </summary>
        /// <param name="thisCap">Interface to use as basis.</param>
        internal RESTOperationResultCapability(RESTOperationResultCapability thisCap) : base(thisCap) { }

        /// <summary>
        /// This method is invoked when the user has made one or more changes to an Operation Result Capability. The method receives an
        /// Operation Result Declaration object that contains the (updated) information for the Operation Result.
        /// </summary>
        /// <param name="result">Updated Operation Result properties.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal void EditOperation(RESTOperationResultDeclaration result)
        {
            if (this._imp != null) ((RESTOperationResultCapabilityImp)this._imp).EditOperation(result);
            else throw new MissingImplementationException("RESTOperationResultCapabilityImp");
        }
    }
}
