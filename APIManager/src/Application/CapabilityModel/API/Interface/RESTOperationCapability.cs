﻿using System;
using System.Collections.Generic;
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
    internal class RESTOperationCapability: OperationCapability
    {
        /// <summary>
        /// Returns the list of operation-specific MIME types produced by this operation. This will be an empty list if the 
        /// operation only produces the standard MIME types.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal List<string> ConsumedMIMEList
        {
            get
            {
                if (this._imp != null) return ((RESTOperationCapabilityImp)this._imp).ConsumedMIMEList;
                else throw new MissingImplementationException("RESTOperationCapabilityImp");
            }
        }

        /// <summary>
        /// Returns Request Headers defined for this operation (if any).
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal List<RESTHeaderParameterDescriptor> RequestHeaders
        {
            get
            {
                if (this._imp != null) return ((RESTOperationCapabilityImp)this._imp).RequestHeaders;
                else throw new MissingImplementationException("RESTOperationCapabilityImp");
            }
        }

        /// <summary>
        /// Returns cardinality of request body document (only valid in case such a body has been defined).
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal Cardinality RequestCardinality
        {
            get
            {
                if (this._imp != null) return ((RESTOperationCapabilityImp)this._imp).RequestCardinality;
                else throw new MissingImplementationException("RESTOperationCapabilityImp");
            }
        }

        /// <summary>
        /// Returns true when the operation has pagination support enabled.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal bool UsePagination
        {
            get
            {
                if (this._imp != null) return ((RESTOperationCapabilityImp)this._imp).UsePagination;
                else throw new MissingImplementationException("RESTOperationCapabilityImp");
            }
        }

        /// <summary>
        /// Returns true when request header parameters are defined for this operation.
        /// </summary>
        internal bool UseRequestHeaders
        {
            get
            {
                if (this._imp != null) return ((RESTOperationCapabilityImp)this._imp).UseRequestHeaders;
                else throw new MissingImplementationException("RESTOperationCapabilityImp");
            }
        }

        /// <summary>
        /// Returns the HTTP operation type that is associated with this REST operation (as an enumeration).
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal HTTPOperation HTTPOperationType
        {
            get
            {
                if (this._imp != null) return ((RESTOperationCapabilityImp)this._imp).HTTPOperationType;
                else throw new MissingImplementationException("RESTOperationCapabilityImp");
            }
        }

        /// <summary>
        /// Returns the collection of Operation Result Descriptors for this Operation.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal RESTResponseCodeCollection ResponseCodeCollection
        {
            get
            {
                if (this._imp != null) return ((RESTOperationCapabilityImp)this._imp).ResponseCodeCollection;
                else throw new MissingImplementationException("RESTOperationCapabilityImp");
            }
        }

        /// <summary>
        /// Returns the resource that 'owns' this operation.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal RESTResourceCapability ParentResource
        {
            get
            {
                if (this._imp != null) return ((RESTOperationCapabilityImp)this._imp).ParentResource;
                else throw new MissingImplementationException("RESTOperationCapabilityImp");
            }
        }

        /// <summary>
        /// Returns the list of operation-specific MIME types produced by this operation. This will be an empty list if the 
        /// operation only produces the standard MIME types.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal List<string> ProducedMIMEList
        {
            get
            {
                if (this._imp != null) return ((RESTOperationCapabilityImp)this._imp).ProducedMIMEList;
                else throw new MissingImplementationException("RESTOperationCapabilityImp");
            }
        }

        /// <summary>
        /// Returns the Document resource that is used as request body (if assigned). The property is NULL when no request body is defined.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal RESTResourceCapability RequestBodyDocument
        {
            get
            {
                if (this._imp != null) return ((RESTOperationCapabilityImp)this._imp).RequestBodyDocument;
                else throw new MissingImplementationException("RESTOperationCapabilityImp");
            }
        }

        /// <summary>
        /// Returns true when the operation must define Link Headers in the Ok response.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal bool UseLinkHeaders
        {
            get
            {
                if (this._imp != null) return ((RESTOperationCapabilityImp)this._imp).UseLinkHeaders;
                else throw new MissingImplementationException("RESTOperationCapabilityImp");
            }
        }

        /// <summary>
        /// Constructor to be used for existing REST Operation associated with a resource collection. The constructor receives
        /// the existing operation class and builds an in-memory representation for further processing.
        /// </summary>
        /// <param name="parent">The resource for which we create the operation.</param>
        /// <param name="operation">The associated operation class.</param>
        internal RESTOperationCapability(RESTResourceCapability parent, TreeNode<MEClass> hierarchy) : base(hierarchy.Data.ElementID)
        {
            if (!Valid) RegisterCapabilityImp(new RESTOperationCapabilityImp(parent, hierarchy));
        }

        /// <summary>
        /// Creates a new resource based on a resource declaration object. This object contains all the information necessary to create 
        /// the associated model elements and attributes.
        /// </summary>
        /// <param name="parent">Resource that acts as parent for the operation.</param>
        /// <param name="operation">Operation declaration object, created by user and containing all necessary information.</param>
        internal RESTOperationCapability(RESTResourceCapability parent, RESTOperationDeclaration operation): base()
        {
            RegisterCapabilityImp(new RESTOperationCapabilityImp(parent, operation));
        }

        /// <summary>
        /// Creates a REST Operation Capability interface object based on its MEClass object. This object MUST have been constructed earlier 
        /// (e.g. on instantiating a Service hierarchy). If the implementation could not be found, a MissingImplementationException is thrown.
        /// </summary>
        /// <param name="capabilityClass">Capability class for the Operation Capability.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal RESTOperationCapability(MEClass capabilityClass) : base(capabilityClass) { }

        /// <summary>
        /// Create new Operation Capability Interface based on existing implementation...
        /// </summary>
        /// <param name="thisImp">Implementation to use.</param>
        internal RESTOperationCapability(RESTOperationCapabilityImp thisImp) : base(thisImp) { }

        /// <summary>
        /// Copy Constructor, using other interface as basis.
        /// </summary>
        /// <param name="thisCap">Interface to use as basis.</param>
        internal RESTOperationCapability(RESTOperationCapability thisCap) : base(thisCap) { }

        /// <summary>
        /// This method is invoked when the user has made one or more changes to an Operation Capability. The method receives an
        /// Operation Declaration object that contains the (updated) information for the Operation.
        /// </summary>
        /// <param name="operation">Updated Operation properties.</param>
        /// <param name="minorVersionUpdate">Set to 'true' to force update of the minor version of the API.</param>
        /// <returns>True on successfull completion, false on errors.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal bool Edit(RESTOperationDeclaration operation, bool minorVersionUpdate)
        {
            if (this._imp != null) return ((RESTOperationCapabilityImp)this._imp).Edit(operation, minorVersionUpdate);
            else throw new MissingImplementationException("RESTOperationCapabilityImp");
        }
    }
}
