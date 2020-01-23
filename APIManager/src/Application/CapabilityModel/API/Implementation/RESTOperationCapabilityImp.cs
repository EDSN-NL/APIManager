using System;
using System.Collections.Generic;
using Framework.Model;
using Framework.Logging;
using Framework.Context;
using Framework.Util;
using Framework.View;

namespace Plugin.Application.CapabilityModel.API
{
    internal class RESTOperationCapabilityImp: OperationCapabilityImp, IDisposable
    {
        // Configuration properties used by this module:
        private const string _RESTOperationClassStereotype      = "RESTOperationClassStereotype";
        private const string _RESTOperationPkgStereotype        = "RESTOperationPkgStereotype";
        private const string _ResourceClassStereotype           = "ResourceClassStereotype";
        private const string _RequestPkgName                    = "RequestPkgName";
        private const string _ResponsePkgName                   = "ResponsePkgName";
        private const string _RequestPkgStereotype              = "RequestPkgStereotype";
        private const string _ResponsePkgStereotype             = "ResponsePkgStereotype";
        private const string _ArchetypeTag                      = "ArchetypeTag";
        private const string _RequestPaginationClassName        = "RequestPaginationClassName";
        private const string _ResponsePaginationClassName       = "ResponsePaginationClassName";
        private const string _OperationResultClassName          = "OperationResultClassName";
        private const string _PaginationRoleName                = "PaginationRoleName";
        private const string _APISupportModelPathName           = "APISupportModelPathName";
        private const string _RequestHdrParamClassName          = "RequestHdrParamClassName";
        private const string _ResponseHdrParamClassName         = "ResponseHdrParamClassName";
        private const string _ConsumesMIMEListTag               = "ConsumesMIMEListTag";
        private const string _ProducesMIMEListTag               = "ProducesMIMEListTag";
        private const string _RESTUseHeaderParametersTag        = "RESTUseHeaderParametersTag";
        private const string _RESTUseLinkHeaderTag              = "RESTUseLinkHeaderTag";
        private const string _RCCStereotype                     = "RCCStereotype";
        private const string _HPCStereotype                     = "HPCStereotype";

        // This is NOT a configuration item since the use of the old Operation Result Capabilities has been deprecated!
        private const string _DEPRECATEDOperationResultStereotype = "RESTOperationResult";

        private RESTResourceCapability _parent;                 // Parent resource capability that owns this operation.
        private HTTPOperation _operationType;                   // The HTTP operation type associated with the operation.
        private List<string> _producedMIMETypes;                // List of non-standard MIME types produced by the operation.
        private List<string> _consumedMIMETypes;                // List of non-standard MIME types consumed by the operation.
        private RESTResourceCapability _requestBodyDocument;    // If the operation has a request body, this is the associated Document Resource.
        private Cardinality _requestCardinality;                // Cardinality of request body document. Only valid if requestBodyDocument is specified.
        private RESTResponseCodeCollection _responseCollection; // Contains the list of response codes (plus associated metadata).
        private RESTHeaderParameterCollection _requestHeaderCollection;     // Contains the set of request header parameters.
        private bool _useLinkHeaders;                           // Set to 'true' when the response must contain a definition for Link Headers.
        private bool _usePagination;                            // Set to 'true' when the operation uses pagination.
        private bool _disposed;                                 // Mark myself as invalid after call to dispose!

        private const string _CollectionNamePostfix             = "Responses";  // Will be added to the operation name to create a response code collection name.

        // Request Header collections all have the same name. This is no issue since request headers all live in a separate (operation) package...
        private const string _HdrCollectionName = "RequestHeaders";

        /// <summary>
        /// This is the normal entry for all users of the object that want to indicate that the resource declaration is not required anymore.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Getters for class properties:
        /// HTTPOperationType = Returns the HTTP operation type that is associated with this REST operation (as an enumeration).
        /// UseLinkHeaders = Returns true if the operation Ok response must contain a definition for Link Headers.
        /// ConsumedMIMEList = Returns list of non-standard MIME types consumed by the operation.
        /// ProducedMIMEList = Returns list of non-standard MIME types produced by the operation.
        /// ParentResource = The resource that 'owns' this operation.
        /// RequestBodyDocument = If the operation has a request body, this returns the associated Document Resource.
        /// RequestCardinality = Cardinality of request document (valid only if RequestBodyDocument is not NULL).
        /// UsePagination = True if the operation has pagination support.
        /// </summary>
        internal HTTPOperation HTTPOperationType                { get { return this._operationType; } }
        internal bool UseLinkHeaders                            { get { return this._useLinkHeaders; } }
        internal List<string> ConsumedMIMEList                  { get { return this._consumedMIMETypes; } }
        internal List<string> ProducedMIMEList                  { get { return this._producedMIMETypes; } }
        internal RESTResourceCapability ParentResource          { get { return this._parent; } }
        internal RESTResourceCapability RequestBodyDocument     { get { return this._requestBodyDocument; } }
        internal Cardinality RequestCardinality                 { get { return this._requestCardinality; } }
        internal bool UsePagination                             { get { return this._usePagination; } }


        /// <summary>
        /// Returns the list of request header parameters for this operation (can be empty if no headers are defined).
        /// </summary>
        internal RESTHeaderParameterCollection RequestHeaders
        {
            get { return this._requestHeaderCollection; }
        }

        /// <summary>
        /// Returns the list of operation response codes with their metadata...
        /// </summary>
        internal RESTResponseCodeCollection ResponseCodeCollection
        {
            get { return this._responseCollection; }
        }

        /// <summary>
        /// Creates a new operation based on an operation declaration object. This object contains all the information necessary to create 
        /// the associated model elements and attributes.
        /// We create all Operation classes in the associated Operation package.
        /// </summary>
        /// <param name="myCollection">Resource collection that acts as parent for the operation.</param>
        /// <param name="operation">Operation declaration object, created by user and containing all necessary information.</param>
        internal RESTOperationCapabilityImp(RESTResourceCapability parentResource, RESTOperationDeclaration operation) :
                        base(parentResource, operation.Name, ContextSlt.GetContextSlt().GetConfigProperty(_RESTOperationPkgStereotype))
        {
            try
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp (declaration) >> Creating operation '" +
                                 parentResource.Name + "." + operation.Name + "'...");
                ContextSlt context = ContextSlt.GetContextSlt();
                ModelSlt model = ModelSlt.GetModelSlt();
                this._parent = parentResource;
                this._operationType = operation.OperationType;
                var myInterface = new RESTOperationCapability(this);
                this._consumedMIMETypes = operation.ConsumedMIMETypes;
                this._producedMIMETypes = operation.ProducedMIMETypes;
                this._requestBodyDocument = operation.RequestDocument;
                this._usePagination = operation.PaginationIndicator;

                this._capabilityClass = OperationPackage.CreateClass(operation.Name, context.GetConfigProperty(_RESTOperationClassStereotype));
                if (this._capabilityClass != null)
                {
                    this._capabilityClass.SetTag(context.GetConfigProperty(_RESTUseHeaderParametersTag), string.Empty);     // This one is DEPRECATED, force to empty string!
                    this._capabilityClass.SetTag(context.GetConfigProperty(_ArchetypeTag), operation.OperationType.TypeName, true);
                    this._capabilityClass.SetTag(context.GetConfigProperty(_RESTUseLinkHeaderTag), operation.UseLinkHeaderIndicator.ToString(), true);
                    this._capabilityClass.Version = parentResource.RootService.Version;
                    this._assignedRole = operation.Name;

                    // Load MIME Types...
                    string MIMETypes = string.Empty;
                    if (this._producedMIMETypes != null && this._producedMIMETypes.Count > 0)
                    {
                        bool firstOne = true;
                        foreach(string MIMEType in this._producedMIMETypes)
                        {
                            MIMETypes += firstOne ? MIMEType : "," + MIMEType;
                            firstOne = false;
                        }
                    }
                    this._capabilityClass.SetTag(context.GetConfigProperty(_ProducesMIMEListTag), MIMETypes, true);
                    MIMETypes = string.Empty;
                    if (this._consumedMIMETypes != null && this._consumedMIMETypes.Count > 0)
                    {
                        bool firstOne = true;
                        foreach (string MIMEType in this._consumedMIMETypes)
                        {
                            MIMETypes += firstOne ? MIMEType : "," + MIMEType;
                            firstOne = false;
                        }
                    }
                    this._capabilityClass.SetTag(context.GetConfigProperty(_ConsumesMIMEListTag), MIMETypes, true);

                    // Load the documentation...
                    List<string> documentation = new List<string>();
                    if (!string.IsNullOrEmpty(operation.Summary)) documentation.Add("Summary: " + operation.Summary);
                    if (!string.IsNullOrEmpty(operation.Description)) documentation.Add(operation.Description);
                    if (documentation.Count > 0) MEChangeLog.SetRTFDocumentation(this._capabilityClass, documentation);

                    // Define all query parameter attributes...
                    foreach (RESTParameterDeclaration param in operation.Parameters)
                    {
                        RESTParameterDeclaration.ConvertToAttribute(this._capabilityClass, param);
                    }

                    // Explicitly request a new Operation ID for this class (could not do this in the parent constructor since the capabilityClass
                    // object has not been initialized yet at that point).
                    AssignNewOperationID();

                    // Establish link with our Parent...
                    var parentEndpoint = new EndpointDescriptor(parentResource.CapabilityClass, "1", parentResource.AssignedRole, null, false);
                    var operationEndpoint = new EndpointDescriptor(this._capabilityClass, "1", this._assignedRole, null, true);
                    model.CreateAssociation(parentEndpoint, operationEndpoint, MEAssociation.AssociationType.MessageAssociation);
                    InitialiseParent(parentResource);

                    // Each operation will receive by default a private package for operation specific request- and response data models.
                    // We leave them empty but users can use these packages to create operation specific models and create Document Resources
                    // that are associated with these models.
                    // Note that we don't create an operation-specific 'Common' package, since this would be identical to the top-level
                    // Data Model package (all operations share the same namespace)! 
                    string pkgName = context.GetConfigProperty(_ResponsePkgName);
                    string pkgStereotype = context.GetConfigProperty(_ResponsePkgStereotype);
                    MEPackage msgPackage = OperationPackage.FindPackage(pkgName, pkgStereotype);
                    if (msgPackage == null) msgPackage = OperationPackage.CreatePackage(pkgName, pkgStereotype, 30);
                    pkgName = context.GetConfigProperty(_RequestPkgName);
                    pkgStereotype = context.GetConfigProperty(_RequestPkgStereotype);
                    msgPackage = OperationPackage.FindPackage(pkgName, pkgStereotype);
                    if (msgPackage == null) msgPackage = OperationPackage.CreatePackage(pkgName, pkgStereotype, 20);

                    // Check whether we must create an association with a request body...
                    if (this._requestBodyDocument != null)
                    {
                        string roleName = RESTUtil.GetAssignedRoleName(this._requestBodyDocument.CapabilityClass.Name);
                        if (roleName.EndsWith("Type")) roleName = roleName.Substring(0, roleName.IndexOf("Type"));
                        var componentEndpoint = new EndpointDescriptor(this._requestBodyDocument.CapabilityClass, operation.RequestCardinality.ToString(), roleName, null, true);
                        model.CreateAssociation(operationEndpoint, componentEndpoint, MEAssociation.AssociationType.MessageAssociation);
                    }

                    // Load Response Code collection and create an association with that collection...
                    this._responseCollection = operation.ResponseCollection;
                    string collectionName = operation.ResponseCollection.Name;
                    this._responseCollection.Serialize(operation.Name + _CollectionNamePostfix, OperationPackage, RESTResponseCodeCollection.CollectionScope.Operation);
                    var collectionEndpoint = new EndpointDescriptor(this._responseCollection.CollectionClass, new Cardinality(Cardinality._Mandatory).ToString(), this._responseCollection.Name, null, true);
                    model.CreateAssociation(operationEndpoint, collectionEndpoint, MEAssociation.AssociationType.MessageAssociation);

                    // Leoad Request Header collection and create an association with that collection...
                    this._requestHeaderCollection = operation.RequestHeaders;
                    this._requestHeaderCollection.Serialize(_HdrCollectionName, OperationPackage, RESTCollection.CollectionScope.Operation);
                    collectionEndpoint = new EndpointDescriptor(this._requestHeaderCollection.CollectionClass, new Cardinality(Cardinality._Mandatory).ToString(), this._requestHeaderCollection.Name, null, true);
                    CapabilityClass.CreateAssociation(operationEndpoint, collectionEndpoint, MEAssociation.AssociationType.MessageAssociation);

                    // Check whether we have to use Pagination. If so, we first attempt to create an association with the Request Pagination parameters,
                    // followed by the Response Pagination parameters...
                    string supportLocation = context.GetConfigProperty(_APISupportModelPathName);
                    if (UsePagination)
                    {
                        MEClass paginationClass = model.FindClass(supportLocation, context.GetConfigProperty(_RequestPaginationClassName));
                        if (paginationClass != null)
                        {
                            var paginationEndpoint = new EndpointDescriptor(paginationClass, "1", context.GetConfigProperty(_PaginationRoleName), null, true);
                            model.CreateAssociation(operationEndpoint, paginationEndpoint, MEAssociation.AssociationType.MessageAssociation);
                            paginationClass = model.FindClass(supportLocation, context.GetConfigProperty(_ResponsePaginationClassName));
                            if (paginationClass != null)
                            {
                                paginationEndpoint = new EndpointDescriptor(paginationClass, "1", context.GetConfigProperty(_PaginationRoleName), null, true);
                                model.CreateAssociation(operationEndpoint, paginationEndpoint, MEAssociation.AssociationType.MessageAssociation);
                            }
                            else Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp (declaration) >> Unable to retrieve Response Pagination class '" +
                                                   supportLocation + "/" + context.GetConfigProperty(_ResponsePaginationClassName) + "'!");

                        }
                        else Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp (declaration) >> Unable to retrieve Request Pagination class '" +
                                               supportLocation + "/" + context.GetConfigProperty(_RequestPaginationClassName) + "'!");
                    }
                    CreateLogEntry("Initial release.");
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp (declaration) >> Error creating operation: " + exc.ToString());
                this._capabilityClass = null;   // Assures that instance is declared invalid.
            }
        }

        /// <summary>
        /// Generic constructor to be used for existing class models. The constructor initialises local context.
        /// </summary>
        /// <param name="parentResource">The resource collection for which we create the operation.</param>
        /// <param name="hierarchy">Starting from our Capability, the hierarchy contains associated Operation Results.</param>
        internal RESTOperationCapabilityImp(RESTResourceCapability parentResource, TreeNode<MEClass> hierarchy) : 
            base(parentResource, hierarchy.Data, 
                 ContextSlt.GetContextSlt().GetConfigProperty(_RESTOperationPkgStereotype),
                 ContextSlt.GetContextSlt().GetConfigProperty(_RESTOperationClassStereotype))
        {
            try
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp (existing) >> Creating new instance '" +
                                 parentResource.Name + "." + hierarchy.Data.Name + "'...");
                ContextSlt context = ContextSlt.GetContextSlt();
                this._parent = parentResource;
                this._capabilityClass = hierarchy.Data;
                this._requestCardinality = new Cardinality();
                this._responseCollection = null;
                this._assignedRole = parentResource.FindChildClassRole(this._capabilityClass.Name, context.GetConfigProperty(_RESTOperationClassStereotype));
                string operationArchetype = this._capabilityClass.GetTag(context.GetConfigProperty(_ArchetypeTag));
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp (existing) >> Operation is of archetype: '" + operationArchetype + "'...");
                this._operationType = new HTTPOperation(EnumConversions<HTTPOperation.Type>.StringToEnum(operationArchetype));
                this._useLinkHeaders = string.Compare(this._capabilityClass.GetTag(context.GetConfigProperty(_RESTUseLinkHeaderTag)), "true", true) == 0;
                this._requestHeaderCollection = new RESTHeaderParameterCollection(this.ParentResource, RESTCollection.CollectionScope.Operation);

                // We might encounter 'old' API's that still use the deprecated structure. For now, assume the best...
                List<MEClass> deprecatedResponseClasses = null;

                // Retrieve the MIME types...
                this._consumedMIMETypes = new List<string>();
                this._producedMIMETypes = new List<string>();
                string consumedTag = this._capabilityClass.GetTag(context.GetConfigProperty(_ConsumesMIMEListTag));
                string producedTag = this._capabilityClass.GetTag(context.GetConfigProperty(_ProducesMIMEListTag));
                if (!string.IsNullOrEmpty(consumedTag))
                {
                    string[] MIMEList = consumedTag.Split(',');
                    foreach (string MIMEEntry in MIMEList) this._consumedMIMETypes.Add(MIMEEntry.Trim());
                }
                if (!string.IsNullOrEmpty(producedTag))
                {
                    string[] MIMEList = producedTag.Split(',');
                    foreach (string MIMEEntry in MIMEList) this._producedMIMETypes.Add(MIMEEntry.Trim());
                }

                // Try to locate associated document resources...
                string resourceStereotype = context.GetConfigProperty(_ResourceClassStereotype);
                var myInterface = new RESTOperationCapability(this);
                foreach (TreeNode<MEClass> node in hierarchy.Children)
                {
                    if (node.Data.HasStereotype(resourceStereotype))
                    {
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp >> Found request body '" +
                                         node.Data.Name + "'...");
                        // Here we can initialize by MEClass only since the capability must have been created earlier (in my parent Resource).
                        this._requestBodyDocument = new RESTResourceCapability(node.Data);
                    }
                    else if (node.Data.HasStereotype(_DEPRECATEDOperationResultStereotype))
                    {
                        if (deprecatedResponseClasses == null) deprecatedResponseClasses = new List<MEClass>();
                        deprecatedResponseClasses.Add(node.Data);
                    }
                    else
                    {
                        Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp >> Unknown child type '" +
                                          node.GetType() + "' with name '" + node.Data.Name + "'!");
                        this._capabilityClass = null;
                        return;
                    }
                }

                // Check whether we're using Pagination, attempt to locate response- and header collections and document cardinality ...
                string paginationClassName = context.GetConfigProperty(_RequestPaginationClassName);
                string collectionStereotype = context.GetConfigProperty(_RCCStereotype);
                string headerCollectionStereotype = context.GetConfigProperty(_HPCStereotype);
                foreach (MEAssociation association in this._capabilityClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation))
                {
                    if (association.Destination.EndPoint.HasStereotype(resourceStereotype))
                    {
                        if (this._requestBodyDocument != null && association.Destination.EndPoint.Name == this._requestBodyDocument.Name)
                        {
                            this._requestCardinality = association.GetCardinality(MEAssociation.AssociationEnd.Destination);
                        }
                    }
                    else if (association.Destination.EndPoint.Name == paginationClassName)
                    {
                        // With regard to pagination, we only look for the request class (it should have both a request- and a response)...
                        this._usePagination = true;
                    }
                    else if (association.Destination.EndPoint.HasStereotype(collectionStereotype))
                    {
                        this._responseCollection = new RESTResponseCodeCollection(this._parent, association.Destination.EndPoint);
                    }
                    else if (association.Destination.EndPoint.HasStereotype(headerCollectionStereotype))
                    {
                        this._requestHeaderCollection = new RESTHeaderParameterCollection(this._parent, association.Destination.EndPoint);
                    }
                }
                if (this._responseCollection == null && deprecatedResponseClasses == null)
                {
                    Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp >> Unable to find my Response Code Collection!");
                    this._capabilityClass = null;
                    return;
                }

                // We might encounter API's that still use the deprecated 'UseHeaderParameters' tag. When found, we convert this to the new header structure.
                string useHeaderParametersTag = this._capabilityClass.GetTag(context.GetConfigProperty(_RESTUseHeaderParametersTag));
                bool transformHeaders = string.IsNullOrEmpty(useHeaderParametersTag) ? false : (string.Compare(useHeaderParametersTag, "true", true) == 0);

                // When we discovered a list of deprecated response capabilities, we will transform these into the new structure.
                // The transformation immediately creates the correct header structures, so we don't have to perform a separate
                // header transformation in this case.
                if (deprecatedResponseClasses != null) TransformResponseStructure(deprecatedResponseClasses, transformHeaders);
                else if (transformHeaders)             TransformHeaderStructure();

                // Finally, make sure that the old 'use headers' tag is removed in case it had 'a' value...
                if (!string.IsNullOrEmpty(useHeaderParametersTag)) this._capabilityClass.SetTag(context.GetConfigProperty(_RESTUseHeaderParametersTag), string.Empty);
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp (existing) >> Error creating capability because: " + exc.Message);
                this._capabilityClass = null;   // Assures that instance is declared invalid.
            }
        }

        /// <summary>
        /// Overrides the default Capability.delete in order to assure that the entire Operation structure is deleted properly.
        /// If the Operation has a request and/or response package that contains classes, the method issues a warning to the user since there
        /// might be Document Resources that are supposed to have an association with this data.
        /// On return, all operation resources, including the package tree, are deleted and the Capability is INVALID.
        /// </summary>
        internal override void Delete()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp.Delete >> Deleting the operation and all associated resources...");
            ContextSlt context = ContextSlt.GetContextSlt();
            string requestPkgName = context.GetConfigProperty(_RequestPkgName);
            string responsePkgName = context.GetConfigProperty(_ResponsePkgName);
            string requestPkgStereotype = context.GetConfigProperty(_RequestPkgStereotype);
            string responsePkgStereotype = context.GetConfigProperty(_ResponsePkgStereotype);

            if (this.OperationPackage != null)
            {
                // Check if we have request- or response data, we issue a warning if so.
                bool hasData = false;
                MEPackage package = this.OperationPackage.FindPackage(requestPkgName, requestPkgStereotype);
                hasData = package != null && package.HasContents();
                if (!hasData)
                {
                    package = this.OperationPackage.FindPackage(responsePkgName, responsePkgStereotype);
                    hasData = package != null && package.HasContents();
                }
                if (hasData) Logger.WriteWarning("Deleted Operation '" + this.Name + "' contains message data, please verify integrity!");
            }
            this._parent.RemoveChild(new RESTOperationCapability(this));            // Detaches the operation from the parent.
            base.Delete();                                                          // Deletes the class structure and package.
        }

        /// <summary>
        /// This method is invoked when the user has made one or more changes to an Operation Capability. The method receives an
        /// Operation Declaration object that contains the (updated) information for the Operation. The method updates metadata and
        /// associations where appropriate.
        /// </summary>
        /// <param name="operation">Updated Operation properties.</param>
        /// <param name="newMinorVersion">Set to true to force update of API minor version. Parameter is ignored when CM is active!</param>
        /// <returns>True on successfull completion, false on errors.</returns>
        internal bool Edit(RESTOperationDeclaration operation, bool newMinorVersion)
        {
            if (operation.Status == RESTOperationDeclaration.DeclarationStatus.Edited)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp.Edit >> Editing '" + operation.Name + "'...");
                ContextSlt context = ContextSlt.GetContextSlt();
                ModelSlt model = ModelSlt.GetModelSlt();

                // Check whether our type has changed...
                if (this._operationType != operation.OperationType)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp.Edit >> Changed archetype from '" + 
                                     this._operationType + "' to '" + operation.OperationType + "'!");
                    this._operationType = operation.OperationType;
                    this._capabilityClass.SetTag(context.GetConfigProperty(_ArchetypeTag), operation.OperationType.TypeName);
                }
                
                // Check whether our name has changed...
                if (this.Name != operation.Name)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp.Edit >> Changed name from '" +
                                     this.Name + "' to '" + operation.Name + "'!");
                    // Operations have their own package, which now requires a name update as well...
                    if (this._capabilityClass.OwningPackage.Parent.FindPackage(operation.Name, context.GetConfigProperty(_RESTOperationPkgStereotype)) == null)
                    {
                        this._capabilityClass.OwningPackage.Name = operation.Name;
                    }
                    else
                    {
                        Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp.Edit >> Operation rename from '" +
                                           this.Name + "' to '" + operation.Name + "' failed: name already in use!");
                        return false;
                    }
                    // Update class name and role name (which must match the class name)...
                    this._capabilityClass.Name = operation.Name;
                    this._assignedRole = operation.Name;
                    foreach (MEAssociation assoc in Parent.CapabilityClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation))
                    {
                        if (assoc.Destination.EndPoint == this.CapabilityClass)
                        {
                            assoc.SetName(operation.Name, MEAssociation.AssociationEnd.Destination);
                            break;
                        }
                    }
                }

                // Check changes to 'use link header'...
                if (operation.UseLinkHeaderIndicator != this._useLinkHeaders)
                {
                    this._useLinkHeaders = operation.UseLinkHeaderIndicator;
                    this._capabilityClass.SetTag(context.GetConfigProperty(_RESTUseLinkHeaderTag), operation.UseLinkHeaderIndicator.ToString());
                }

                // Make sure to update pagination: add if required and not there yet or remove if not required anymore...
                UpdatePagination(operation.PaginationIndicator);

                // (Re-)Load MIME Types...
                if (this._consumedMIMETypes != operation.ConsumedMIMETypes || this._producedMIMETypes != operation.ProducedMIMETypes)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp.Edit >> MIME Types have changed!");
                    string MIMETypes = string.Empty;
                    this._consumedMIMETypes = operation.ConsumedMIMETypes;
                    this._producedMIMETypes = operation.ProducedMIMETypes;
                    if (this._producedMIMETypes != null && this._producedMIMETypes.Count > 0)
                    {
                        bool firstOne = true;
                        foreach (string MIMEType in this._producedMIMETypes)
                        {
                            MIMETypes += firstOne ? MIMEType : "," + MIMEType;
                            firstOne = false;
                        }
                    }
                    this._capabilityClass.SetTag(context.GetConfigProperty(_ProducesMIMEListTag), MIMETypes);
                    MIMETypes = string.Empty;
                    if (this._consumedMIMETypes != null && this._consumedMIMETypes.Count > 0)
                    {
                        bool firstOne = true;
                        foreach (string MIMEType in this._consumedMIMETypes)
                        {
                            MIMETypes += firstOne ? MIMEType : "," + MIMEType;
                            firstOne = false;
                        }
                    }
                    this._capabilityClass.SetTag(context.GetConfigProperty(_ConsumesMIMEListTag), MIMETypes);
                }

                // Replace the request body element...
                // Check whether we must replace an existing association with a Document Resource...
                var operationEndpoint = new EndpointDescriptor(this._capabilityClass, "1", this._assignedRole, null, true);
                if (this._requestBodyDocument != operation.RequestDocument)
                {
                    if (this._requestBodyDocument != null)
                    {
                        foreach (MEAssociation assoc in this._capabilityClass.AssociationList)
                        {
                            if (assoc.Destination.EndPoint == this._requestBodyDocument.CapabilityClass)
                            {
                                this._capabilityClass.DeleteAssociation(assoc);
                                this._requestBodyDocument = null;
                                break;
                            }
                        }
                    }
                    if (operation.RequestDocument != null)
                    {
                        this._requestBodyDocument = operation.RequestDocument;
                        this._requestCardinality = operation.RequestCardinality;
                        string roleName = RESTUtil.GetAssignedRoleName(this._requestBodyDocument.CapabilityClass.Name);
                        if (roleName.EndsWith("Type")) roleName = roleName.Substring(0, roleName.IndexOf("Type"));
                        string cardinality = operation.RequestCardinality.ToString();
                        var componentEndpoint = new EndpointDescriptor(this._requestBodyDocument.CapabilityClass, cardinality, roleName, null, true);
                        model.CreateAssociation(operationEndpoint, componentEndpoint, MEAssociation.AssociationType.MessageAssociation);
                    }
                }
                else if (this._requestCardinality != operation.RequestCardinality && this._requestBodyDocument != null)
                {
                    // Request cardinality has changed, update existing association...
                    foreach (MEAssociation assoc in this._capabilityClass.AssociationList)
                    {
                        if (assoc.Destination.EndPoint == this._requestBodyDocument.CapabilityClass)
                        {
                            this._requestCardinality = operation.RequestCardinality;
                            assoc.SetCardinality(this._requestCardinality, MEAssociation.AssociationEnd.Destination);
                            break;
                        }
                    }
                }
                
                // (Re-)Load documentation...
                List<string> documentation = new List<string>();
                if (!string.IsNullOrEmpty(operation.Summary)) documentation.Add("Summary: " + operation.Summary);
                if (!string.IsNullOrEmpty(operation.Description)) documentation.Add(operation.Description);
                if (documentation.Count > 0) MEChangeLog.SetRTFDocumentation(this._capabilityClass, documentation);

                // (re-)Define all query parameter attributes (ConvertToAttribute properly handles existing attributes)...
                foreach (RESTParameterDeclaration param in operation.Parameters) RESTParameterDeclaration.ConvertToAttribute(this._capabilityClass, param);

                // Replace the response collection by a (possibly new) version...
                this._responseCollection = operation.ResponseCollection;

                // Check what happened to our header parameters: if all have been deleted, we might have to delete the collection class.
                // On the other hand, there might be a new collection (where there was none), in which case we have to serialize- and create
                // the collection!
                // Finally, it could be that the collection itself has been changed.
                if (this._requestHeaderCollection.Collection.Count > 0 && operation.RequestHeaders.Collection.Count == 0)
                {
                    this._requestHeaderCollection.DeleteCollection();   // Clears all header parameters and removes the associated UML class.
                }
                else if (this._requestHeaderCollection.Collection.Count == 0 && operation.RequestHeaders.Collection.Count > 0)
                {
                    this._requestHeaderCollection.DeleteCollection();           // Assures that 'old' collection is cleaned up.
                    this._requestHeaderCollection = operation.RequestHeaders;   // Replace old by new.
                    this._requestHeaderCollection.Serialize(_HdrCollectionName, OwningPackage, RESTCollection.CollectionScope.Operation);

                    // To be sure, we check whether we have an association with this header class (should not be the case)...
                    bool foundClass = false;
                    foreach (MEAssociation assoc in CapabilityClass.AssociationList)
                    {
                        if (assoc.Destination.EndPoint.Name == this._requestHeaderCollection.Name)
                        {
                            foundClass = true;
                            break;
                        }
                    }
                    if (!foundClass)
                    {
                        if (this._requestHeaderCollection.CollectionClass != null)
                        {
                            var sourceEndpoint = new EndpointDescriptor(CapabilityClass, "1", this._assignedRole, null, false);
                            var headersEndpoint = new EndpointDescriptor(this._requestHeaderCollection.CollectionClass, "1", this._requestHeaderCollection.Name, null, true);
                            CapabilityClass.CreateAssociation(sourceEndpoint, headersEndpoint, MEAssociation.AssociationType.MessageAssociation);
                        }
                        else
                        {
                            Logger.WriteWarning("Unable to create request header parameter collection '" +
                                                this._requestHeaderCollection.Name + "' in package '" + OwningPackage.Name + "'!");
                        }
                    }
                }
                else this._requestHeaderCollection.UpdateCollection(operation.RequestHeaders);

                // This will update the service version, followed by all child capabilities!
                // But the operation is executed ONLY when configuration management is disabled (with CM enabled, versions are
                // managed differently).
                if (!this._rootService.UseConfigurationMgmt && newMinorVersion) RootService.IncrementVersion();

                CreateLogEntry("Changes made to Operation.");
            }
            return true;
        }

        /// <summary>
        /// Returns a short textual identification of the capability type.
        /// </summary>
        /// <returns>Capability type name.</returns>
        internal override string GetCapabilityType()
        {
            return "REST Operation";
        }

        /// <summary>
        /// Creates an Interface object that matches the current Implementation.
        /// </summary>
        /// <returns>Interface object.</returns>
        internal override Capability GetInterface() { return new RESTOperationCapability(this); }

        /// <summary>
        /// Process the capability (i.e. generate output according to provided processor.).
        /// </summary>
        /// <param name="stage">The processing stage we're currently in, passed verbatim to processor.</param>
        /// <returns>True when processing can commence, false on errors.</returns>
        internal override bool HandleCapability(CapabilityProcessor processor, ProcessingStage stage)
        {
            // Since all the actual work is being performed by the processor, simply pass information onwards...
            return processor.ProcessCapability(new RESTOperationCapability(this), stage);
        }

        /// <summary>
        /// This method is called whenever a (new) parent of the Capability has taken ownership of the Capability. 
        /// If this parent is a Resource Collection or a Path Expression, we have to register the current instance with that Interface.
        /// </summary>
        /// <param name="parent">The parent Capability that has taken ownership of this Capability.</param>
        internal override void InitialiseParent(Capability parent)
        {
            if (parent is RESTResourceCapability)
            {
                this._parent = parent as RESTResourceCapability;
                parent.AddChild(new RESTOperationCapability(this));
            }
        }

        /// <summary>
        /// This is the actual disposing interface, which takes case of structural removal of the implementation type when no longer
        /// needed.
        /// </summary>
        /// <param name="disposing">Set to 'true' when called directly. Set to 'false' when called from the finalizer.</param>
        private void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                try
                {
                    this._parent = null;
                    this._producedMIMETypes = null;
                    this._consumedMIMETypes = null;
                    this._responseCollection = null;
                    this._disposed = true;
                }
                catch { };   // Ignore any exceptions, no use in processing them here.
            }
        }

        /// <summary>
        /// Helper method that takes the old header parameter classes and creates request- and response headers according to the new structure. 
        /// The method assumes that at least the response code structure adheres to current standards.
        /// When we can't find the 'old' header template classes, the method performs no actions!
        /// </summary>
        private void TransformHeaderStructure()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            MEClass requestParamClass = model.FindClass(context.GetConfigProperty(_APISupportModelPathName),
                                                        context.GetConfigProperty(_RequestHdrParamClassName));
            MEClass responseParamClass = model.FindClass(context.GetConfigProperty(_APISupportModelPathName),
                                                         context.GetConfigProperty(_ResponseHdrParamClassName));

            Logger.WriteWarning("Detected a deprecated header structure for operation '" + Name + "'; updating model structure in progress!");

            // First, we create the request headers using a fully qualified collection (that can be serialized immediately)...
            if (requestParamClass != null)
            {
                this._requestHeaderCollection = new RESTHeaderParameterCollection(this.ParentResource, _HdrCollectionName, this.OperationPackage);
                foreach (MEAttribute attrib in requestParamClass.Attributes)
                {
                    this._requestHeaderCollection.AddHeaderParameter(new RESTHeaderParameterDescriptor(attrib.Name, attrib.Classifier, attrib.Annotation));
                }
            }
            else Logger.WriteWarning("Unable to find old request header-parameter template '" + 
                                     context.GetConfigProperty(_APISupportModelPathName) + ":" + 
                                     context.GetConfigProperty(_RequestHdrParamClassName) + "', no request headers have been transformed!");

            // Next, we create a response header collection for each response code...
            if (responseParamClass != null)
            {
                foreach (MEAttribute attrib in responseParamClass.Attributes)
                {
                    foreach (RESTOperationResultDescriptor resultDesc in this._responseCollection.Collection)
                    {
                        resultDesc.AddHeaderParameter(new RESTHeaderParameterDescriptor(attrib.Name, attrib.Classifier, attrib.Annotation));
                    }
                }
            }
            else Logger.WriteWarning("Unable to find old response header-parameter template '" +
                         context.GetConfigProperty(_APISupportModelPathName) + ":" +
                         context.GetConfigProperty(_ResponseHdrParamClassName) + "', no response headers have been transformed!");
        }

        /// <summary>
        /// A helper function that receives a list of 'old' Response Capability classes from this operation and transforms this into
        /// our new Response Collection structure.
        /// </summary>
        /// <param name="deprecatedResponseClasses">List of all Operation Result Capability classes to be transformed.</param>
        /// <param name="transformHeaders">When 'true', we ALSO have to transform the old header structure (based on a single flag)
        /// to the new request- and response header structure.</param>
        private void TransformResponseStructure(List<MEClass> deprecatedResponseClasses, bool transformHeaders)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            Logger.WriteWarning("Detected one or more deprecated Operation Result Capabilities for operation '" + Name + 
                                "'; updating model structure in progress!");

            // First of all, create a new Response Code Collection and create an association with it... 
            if (this._responseCollection == null)
            {
                this._responseCollection = new RESTResponseCodeCollection(this._parent, Name + _CollectionNamePostfix, this.OperationPackage);
                var operationEndpoint = new EndpointDescriptor(this._capabilityClass, "1", this._assignedRole, null, true);
                var collectionEndpoint = new EndpointDescriptor(this._responseCollection.CollectionClass, new Cardinality(Cardinality._Mandatory).ToString(), this._responseCollection.Name, null, true);
                var newAssoc = this._capabilityClass.CreateAssociation(operationEndpoint, collectionEndpoint, MEAssociation.AssociationType.MessageAssociation);

                // Assures that the collection is shown on the diagram with the proper association type...
                Diagram resourceDiagram = this._parent.OwningPackage.FindDiagram(this._parent.Name);
                if (resourceDiagram != null)
                {
                    resourceDiagram.AddClassList(new List<MEClass>() { this._responseCollection.CollectionClass });
                    resourceDiagram.AddAssociationList(new List<MEAssociation>() { newAssoc });
                    resourceDiagram.Redraw();
                }
            }

            // Next, check whether we have Header Parameters for this operation and if so, create the appropriate request- and response collections...
            // For the request headers, we create a qualified collection, containing name and owning package. This assures the collection is serialized immediately.
            // For the response headers, we create an 'abstract' collection that is serialized only after adding it to the response code collection.
            var rspHeaderCollection = new RESTHeaderParameterCollection(this.ParentResource, RESTCollection.CollectionScope.Operation);
            if (transformHeaders)
            {
                ModelSlt model = ModelSlt.GetModelSlt();
                MEClass requestParamClass = model.FindClass(context.GetConfigProperty(_APISupportModelPathName),
                                                            context.GetConfigProperty(_RequestHdrParamClassName));
                MEClass responseParamClass = model.FindClass(context.GetConfigProperty(_APISupportModelPathName),
                                                             context.GetConfigProperty(_ResponseHdrParamClassName));
                if (requestParamClass != null)
                {
                    this._requestHeaderCollection = new RESTHeaderParameterCollection(this.ParentResource, _HdrCollectionName, this.OperationPackage);
                    foreach (MEAttribute attrib in requestParamClass.Attributes)
                    {
                        this._requestHeaderCollection.AddHeaderParameter(new RESTHeaderParameterDescriptor(attrib.Name, attrib.Classifier, attrib.Annotation));
                    }
                }
                else Logger.WriteWarning("Unable to find old request header-parameter template '" +
                                         context.GetConfigProperty(_APISupportModelPathName) + ":" +
                                         context.GetConfigProperty(_RequestHdrParamClassName) + "', no request headers have been transformed!");

                if (responseParamClass != null)
                {
                    foreach (MEAttribute attrib in responseParamClass.Attributes)
                    {
                        rspHeaderCollection.AddHeaderParameter(new RESTHeaderParameterDescriptor(attrib.Name, attrib.Classifier, attrib.Annotation));
                    }
                }
                else Logger.WriteWarning("Unable to find old response header-parameter template '" +
                                         context.GetConfigProperty(_APISupportModelPathName) + ":" +
                                         context.GetConfigProperty(_ResponseHdrParamClassName) + "', no response headers have been transformed!");
            }

            // Next, iterate through all of the old Capability Classes and collect response metadata for a new Response Descriptor...
            foreach (MEClass oldResponse in deprecatedResponseClasses)
            {
                string resultCode = oldResponse.FindAttribute("Code").FixedValue;
                string description = MEChangeLog.GetDocumentationAsText(oldResponse);
                var category = (resultCode == RESTOperationResultDescriptor._DefaultCode) ? RESTOperationResultDescriptor.ResponseCategory.Default :
                                                                                           (RESTOperationResultDescriptor.ResponseCategory)int.Parse(resultCode[0].ToString());
                var payloadType = RESTOperationResultDescriptor.ResultPayloadType.None;
                MEClass responsePayload = null;
                RESTResourceCapability document = null;
                Cardinality cardinality = null;

                // Check associations in order to detect payload...
                string defaultResponseClassName = context.GetConfigProperty(_OperationResultClassName);
                string resourceClassStereotype = context.GetConfigProperty(_ResourceClassStereotype);
                foreach (MEAssociation association in oldResponse.TypedAssociations(MEAssociation.AssociationType.MessageAssociation))
                {
                    if (association.Destination.EndPoint.Name == defaultResponseClassName)
                    {
                        payloadType = RESTOperationResultDescriptor.ResultPayloadType.DefaultResponse;
                        responsePayload = association.Destination.EndPoint;
                        cardinality = new Cardinality(association.Destination.Cardinality);
                        break;
                    }
                    else if (association.Destination.EndPoint.HasStereotype(resourceClassStereotype))
                    {
                        payloadType = RESTOperationResultDescriptor.ResultPayloadType.Document;
                        document = new RESTResourceCapability(association.Destination.EndPoint);
                        cardinality = new Cardinality(association.Destination.Cardinality);
                        break;
                    }
                }

                // Now we have collected enough information to create a new response descriptor and remove the old stuff...
                this._responseCollection.AddOperationResult(new RESTOperationResultDescriptor(this._responseCollection, resultCode,
                                                                                              category, payloadType, description, rspHeaderCollection,
                                                                                              null, responsePayload, document, cardinality));
                oldResponse.OwningPackage.DeleteClass(oldResponse);
            }
        }

        /// <summary>
        /// Helper method that updates the current pagination settings: add them if required, remove if not required anymore.
        /// </summary>
        /// <param name="mustHavePagination">Post-condition: True if the capability must have pagination.</param>
        private void UpdatePagination(bool mustHavePagination)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string requestPaginationClassName = context.GetConfigProperty(_RequestPaginationClassName);
            string responsePaginationClassName = context.GetConfigProperty(_ResponsePaginationClassName);
            bool hasPagination = false;
            MEAssociation requestPaginationAssociation = null;
            MEAssociation responsePaginationAssociation = null;
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp.UpdatePagination >> Requested pagination: '" + mustHavePagination + "'...");

            // First of all, check to see whether we already have pagination and work from there...
            // We itereate over all message associations and attempt to collect the existing associations...
            foreach (MEAssociation assoc in this._capabilityClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation))
            {
                if (assoc.Destination.EndPoint.Name == requestPaginationClassName)
                {
                    requestPaginationAssociation = assoc;
                    hasPagination = true;
                }
                else if (assoc.Destination.EndPoint.Name == responsePaginationClassName)
                {
                    responsePaginationAssociation = assoc;
                    hasPagination = true;
                }
            }
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp.UpdatePagination >> Current pagination: '" + hasPagination + "'...");

            if (hasPagination == mustHavePagination) return;    // We already match our post-condition, nothing to do!
            else if (hasPagination && !mustHavePagination)      // We have pagination but don't need it anymore, get rid of associations...
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp.UpdatePagination >> Deleting existing associations...");
                this._capabilityClass.DeleteAssociation(requestPaginationAssociation);
                this._capabilityClass.DeleteAssociation(responsePaginationAssociation);
                return;
            }
            else                                                //We don't have pagination but must get it now. Create the appropriate associations...
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp.UpdatePagination >> Creating new associations...");
                ModelSlt model = ModelSlt.GetModelSlt();
                var operationEndpoint = new EndpointDescriptor(this._capabilityClass, "1", this._assignedRole, null, true);
                MEClass paginationClass = model.FindClass(context.GetConfigProperty(_APISupportModelPathName),
                                                          context.GetConfigProperty(_RequestPaginationClassName));
                if (paginationClass != null)
                {
                    var paginationEndpoint = new EndpointDescriptor(paginationClass, "1", context.GetConfigProperty(_PaginationRoleName), null, true);
                    model.CreateAssociation(operationEndpoint, paginationEndpoint, MEAssociation.AssociationType.MessageAssociation);
                    paginationClass = model.FindClass(context.GetConfigProperty(_APISupportModelPathName),
                                                      context.GetConfigProperty(_ResponsePaginationClassName));
                    if (paginationClass != null)
                    {
                        paginationEndpoint = new EndpointDescriptor(paginationClass, "1", context.GetConfigProperty(_PaginationRoleName), null, true);
                        model.CreateAssociation(operationEndpoint, paginationEndpoint, MEAssociation.AssociationType.MessageAssociation);
                    }
                    else Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp.UpdatePagination >> Unable to retrieve Response Pagination class '" +
                                           context.GetConfigProperty(_APISupportModelPathName) + "/" + context.GetConfigProperty(_ResponsePaginationClassName) + "'!");

                }
                else Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp >> Unable to retrieve Request Pagination class '" +
                                       context.GetConfigProperty(_APISupportModelPathName) + "/" + context.GetConfigProperty(_RequestPaginationClassName) + "'!");
            }
        }
    }
}
