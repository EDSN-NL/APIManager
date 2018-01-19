using System;
using System.Collections.Generic;
using Framework.Model;
using Framework.Logging;
using Framework.Context;
using Framework.Util;

namespace Plugin.Application.CapabilityModel.API
{
    internal class RESTOperationCapabilityImp: OperationCapabilityImp
    {
        // Configuration properties used by this module:
        private const string _RESTOperationClassStereotype      = "RESTOperationClassStereotype";
        private const string _RESTOperationPkgStereotype        = "RESTOperationPkgStereotype";
        private const string _RESTOperationResultStereotype     = "RESTOperationResultStereotype";
        private const string _ResourceClassStereotype           = "ResourceClassStereotype";
        private const string _RequestPkgName                    = "RequestPkgName";
        private const string _ResponsePkgName                   = "ResponsePkgName";
        private const string _RequestPkgStereotype              = "RequestPkgStereotype";
        private const string _ResponsePkgStereotype             = "ResponsePkgStereotype";
        private const string _ArchetypeTag                      = "ArchetypeTag";
        private const string _RequestPaginationClassName        = "RequestPaginationClassName";
        private const string _ResponsePaginationClassName       = "ResponsePaginationClassName";
        private const string _PaginationRoleName                = "PaginationRoleName";
        private const string _APISupportModelPathName           = "APISupportModelPathName";
        private const string _DefaultSuccessCode                = "DefaultSuccessCode";
        private const string _DefaultResponseCode               = "DefaultResponseCode";
        private const string _ConsumesMIMEListTag               = "ConsumesMIMEListTag";
        private const string _ProducesMIMEListTag               = "ProducesMIMEListTag";
        private const string _RESTUseHeaderParametersTag        = "RESTUseHeaderParametersTag";
        private const string _RESTUseLinkHeaderTag              = "RESTUseLinkHeaderTag";

        private RESTResourceCapability _parent;                 // Parent resource capability that owns this operation.
        private RESTOperationCapability.OperationType _archetype;   // The HTTP operation type associated with the operation.
        private List<string> _producedMIMETypes;                // List of non-standard MIME types produced by the operation.
        private List<string> _consumedMIMETypes;                // List of non-standard MIME types consumed by the operation.
        private RESTResourceCapability _requestBodyDocument;    // If the operation has a request body, this is the associated Document Resource.
        private RESTResourceCapability _responseBodyDocument;   // If the operation has a response body, this is the associated Document Resource.
        private bool _hasMultipleResponses;                     // True if the default Ok response has multiple response documents.
        private bool _useHeaderParameters;                      // Set to 'true' when operation muse use configured Header Parameters.
        private bool _useLinkHeaders;                           // Set to 'true' when the response must contain a definition for Link Headers.

        /// <summary>
        /// Getters for class properties:
        /// HTTPType = Returns the HTTP operation type that is associated with this REST operation (as an enumeration).
        /// HTTPTypeName = Returns the name of the HTTP operation as a lowercase string.
        /// UseHeaderParameters = Returns true if the operation muse use configured Header Parameters.
        /// UseLinkHeaders = Returns true if the operation Ok response must contain a definition for Link Headers.
        /// ConsumedMIMEList = Returns list of non-standard MIME types consumed by the operation.
        /// ProducedMIMEList = Returns list of non-standard MIME types produced by the operation.
        /// ParentResource = The resource that 'owns' this operation.
        /// RequestBodyDocument = If the operation has a request body, this returns the associated Document Resource.
        /// ResponseBodyDocument = If the operation has a default Ok response body, this gets/sets the associated Document Resource.
        /// HasMultipleResponses = True if the default OK response has multiple response body elements.
        /// </summary>
        internal RESTOperationCapability.OperationType HTTPType { get { return this._archetype; } }
        internal string HTTPTypeName                            { get { return this._archetype.ToString("G").ToLower(); } }
        internal bool UseHeaderParameters                       { get { return this._useHeaderParameters; } }
        internal bool UseLinkHeaders                            { get { return this._useLinkHeaders; } }
        internal List<string> ConsumedMIMEList                  { get { return this._consumedMIMETypes; } }
        internal List<string> ProducedMIMEList                  { get { return this._producedMIMETypes; } }
        internal RESTResourceCapability ParentResource          { get { return this._parent; } }
        internal RESTResourceCapability RequestBodyDocument     { get { return this._requestBodyDocument; } }
        internal RESTResourceCapability ResponseBodyDocument
        {
            get { return this._responseBodyDocument; }
            set { this._responseBodyDocument = value; }
        }
        internal bool HasMultipleResponses                      { get { return this._hasMultipleResponses; } }

        /// <summary>
        /// Returns the list of Operation Result capabilities for this Operation.
        /// </summary>
        internal List<RESTOperationResultCapability> OperationResultList
        {
            get
            {
                var resultList = new List<RESTOperationResultCapability>();
                foreach (Capability child in GetChildren())
                {
                    if (child is RESTOperationResultCapability) resultList.Add(child as RESTOperationResultCapability);
                }
                return resultList;
            }
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
                this._archetype = operation.Archetype;
                var myInterface = new RESTOperationCapability(this);
                this._consumedMIMETypes = operation.ConsumedMIMETypes;
                this._producedMIMETypes = operation.ProducedMIMETypes;
                this._requestBodyDocument = operation.RequestDocument;
                this._responseBodyDocument = operation.ResponseDocument;
                this._useHeaderParameters = operation.UseHeaderParametersIndicator;

                this._capabilityClass = OperationPackage.CreateClass(operation.Name, context.GetConfigProperty(_RESTOperationClassStereotype));
                if (this._capabilityClass != null)
                {
                    this._capabilityClass.SetTag(context.GetConfigProperty(_ArchetypeTag), operation.Archetype.ToString(), true);
                    this._capabilityClass.SetTag(context.GetConfigProperty(_RESTUseHeaderParametersTag), operation.UseHeaderParametersIndicator.ToString(), true);
                    this._capabilityClass.SetTag(context.GetConfigProperty(_RESTUseLinkHeaderTag), operation.UseLinkHeaderIndicator.ToString(), true);
                    this._capabilityClass.Version = new Tuple<int, int>(parentResource.RootService.MajorVersion, 0);
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

                    // Each operation will receive by default a private packages for operation specific request- and response data models.
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
                        string cardinality = operation.RequestBodyCardinalityIndicator ? "1..*" : "1";
                        var componentEndpoint = new EndpointDescriptor(this._requestBodyDocument.CapabilityClass, cardinality, roleName, null, true);
                        model.CreateAssociation(operationEndpoint, componentEndpoint, MEAssociation.AssociationType.MessageAssociation);
                    }

                    // Create Response Object classes for each operation result declaration...
                    string defaultSuccess = context.GetConfigProperty(_DefaultSuccessCode);
                    foreach (RESTOperationResultDeclaration result in operation.OperationResults)
                    {
                        if (this._responseBodyDocument != null && result.ResultCode == defaultSuccess)
                        {
                            // If we need a response body, this must be linked to the default success result capability.
                            // We assign the associated document class with the result parameter so it will be linked to the result class.
                            result.HasMultipleResponses = operation.ResponseBodyCardinalityIndicator;
                            result.ResponseDocumentClass = this._responseBodyDocument.CapabilityClass;
                        }
                        RESTOperationResultCapability newResult = new RESTOperationResultCapability(myInterface, result);
                        newResult.InitialiseParent(myInterface);
                    }

                    // Check whether we have to use Pagination. If so, we first attempt to create an association with the Request Pagination parameters,
                    // followed by the Response Pagination parameters...
                    if (operation.PaginationIndicator)
                    {
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
                            else Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp (declaration) >> Unable to retrieve Response Pagination class '" +
                                                   context.GetConfigProperty(_APISupportModelPathName) + "/" + context.GetConfigProperty(_ResponsePaginationClassName) + "'!");

                        }
                        else Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp (declaration) >> Unable to retrieve Request Pagination class '" + 
                                               context.GetConfigProperty(_APISupportModelPathName) + "/" + context.GetConfigProperty(_RequestPaginationClassName) + "'!");
                    }
                    CreateLogEntry("Initial release.");
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp (declaration) >> Error creating operation: " + exc.Message);
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
                this._assignedRole = parentResource.FindChildClassRole(this._capabilityClass.Name, context.GetConfigProperty(_RESTOperationClassStereotype));
                string operationArchetype = this._capabilityClass.GetTag(context.GetConfigProperty(_ArchetypeTag));
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp (existing) >> Operation is of archetype: '" + operationArchetype + "'...");
                this._archetype = EnumConversions<RESTOperationCapability.OperationType>.StringToEnum(operationArchetype);
                this._useHeaderParameters = string.Compare(this._capabilityClass.GetTag(context.GetConfigProperty(_RESTUseHeaderParametersTag)), "true", true) == 0;
                this._useLinkHeaders = string.Compare(this._capabilityClass.GetTag(context.GetConfigProperty(_RESTUseLinkHeaderTag)), "true", true) == 0;

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

                // Construct all associated operation results...
                string operationResultStereotype = context.GetConfigProperty(_RESTOperationResultStereotype);
                string resourceStereotype = context.GetConfigProperty(_ResourceClassStereotype);
                string defaultSuccess = ContextSlt.GetContextSlt().GetConfigProperty(_DefaultSuccessCode);
                var myInterface = new RESTOperationCapability(this);
                foreach (TreeNode<MEClass> node in hierarchy.Children)
                {
                    if (node.Data.HasStereotype(operationResultStereotype))
                    {
                        var resultCap = new RESTOperationResultCapability(myInterface, node);
                        resultCap.InitialiseParent(myInterface);
                        if (!resultCap.Valid)
                        {
                            Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp >> Error creating Operation Result '" + node.Data.Name + "'!");
                            this._capabilityClass = null;
                            return;
                        }
                        if (resultCap.ResultCode == defaultSuccess) this._hasMultipleResponses = resultCap.HasMultipleResponses;
                    }
                    else if (node.Data.HasStereotype(resourceStereotype))
                    {
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp >> Found request body '" +
                                         node.Data.Name + "'...");
                        // Here we can initialize by MEClass only since the capability must have been created earlier (in my parent Resource).
                        this._requestBodyDocument = new RESTResourceCapability(node.Data);
                    }
                    else
                    {
                        Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp >> Unknown child type '" +
                                          node.GetType() + "' with name '" + node.Data.Name + "'!");
                        this._capabilityClass = null;
                        return;
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp (existing) >> Error creating capability because: " + exc.Message);
                this._capabilityClass = null;   // Assures that instance is declared invalid.
            }
        }

        /// <summary>
        /// Overrides the default Capability.delete in order to assure that the operation is deleted as well as the operation package.
        /// On return, all operation resources, including the package tree, are deleted and the Capability is INVALID.
        /// </summary>
        internal override void Delete()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp.Delete >> Deleting the operation and all associated resources...");

            this._parent.RemoveChild(new OperationCapability(this));                // Detaches the operation from the parent.
            base.Delete();                                                          // Deletes the class structure and package.
        }

        /// <summary>
        /// This method is invoked when the user has made one or more changes to an Operation Capability. The method receives an
        /// Operation Declaration object that contains the (updated) information for the Operation. The method updates metadata and
        /// associations where appropriate.
        /// </summary>
        /// <param name="operation">Updated Operation properties.</param>
        /// <param name="minorVersionUpdate">Set to true to force update of API minor version.</param>
        /// <returns>True on successfull completion, false on errors.</returns>
        internal bool Edit(RESTOperationDeclaration operation, bool minorVersionUpdate)
        {
            if (operation.Status == RESTOperationDeclaration.DeclarationStatus.Edited)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp.EditOperation >> Editing '" + operation.Name + "'...");
                ContextSlt context = ContextSlt.GetContextSlt();
                ModelSlt model = ModelSlt.GetModelSlt();

                // Check whether our type has changed...
                if (this._archetype != operation.Archetype)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp.EditOperation >> Changed archetype from '" + 
                                     this._archetype + "' to '" + operation.Archetype + "'!");
                    this._archetype = operation.Archetype;
                    this._capabilityClass.SetTag(context.GetConfigProperty(_ArchetypeTag), operation.Archetype.ToString());
                }
                
                // Check whether our name has changed...
                if (this.Name != operation.Name)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp.EditOperation >> Changed name from '" +
                                     this.Name + "' to '" + operation.Name + "'!");
                    if (this._capabilityClass.OwningPackage.Parent.FindPackage(operation.Name, context.GetConfigProperty(_RESTOperationPkgStereotype)) == null)
                    {
                        this._capabilityClass.OwningPackage.Name = operation.Name;
                        this._capabilityClass.Name = operation.Name;
                        this._assignedRole = operation.Name;
                    }
                    else
                    {
                        Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp.EditOperation >> Operation rename from '" +
                                           this.Name + "' to '" + operation.Name + "' failed: name already in use!");
                        return false;
                    }
                }

                // Check changes to 'use header parameters' and 'use link header'...
                if (operation.UseHeaderParametersIndicator != this._useHeaderParameters)
                {
                    this._useHeaderParameters = operation.UseHeaderParametersIndicator;
                    this._capabilityClass.SetTag(context.GetConfigProperty(_RESTUseHeaderParametersTag), operation.UseHeaderParametersIndicator.ToString());
                }
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
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp.EditOperation >> MIME Types have changed!");
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

                // Replace the request- and response body elements...
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
                        string roleName = RESTUtil.GetAssignedRoleName(this._requestBodyDocument.CapabilityClass.Name);
                        if (roleName.EndsWith("Type")) roleName = roleName.Substring(0, roleName.IndexOf("Type"));
                        string cardinality = operation.RequestBodyCardinalityIndicator ? "1..*" : "1";
                        var componentEndpoint = new EndpointDescriptor(this._requestBodyDocument.CapabilityClass, cardinality, roleName, null, true);
                        model.CreateAssociation(operationEndpoint, componentEndpoint, MEAssociation.AssociationType.MessageAssociation);
                    }
                }
                
                // (Re-)Load documentation...
                List<string> documentation = new List<string>();
                if (!string.IsNullOrEmpty(operation.Summary)) documentation.Add("Summary: " + operation.Summary);
                if (!string.IsNullOrEmpty(operation.Description)) documentation.Add(operation.Description);
                if (documentation.Count > 0) MEChangeLog.SetRTFDocumentation(this._capabilityClass, documentation);

                // (re-)Define all query parameter attributes (ConvertToAttribute properly handles existing attributes)...
                foreach (RESTParameterDeclaration param in operation.Parameters) RESTParameterDeclaration.ConvertToAttribute(this._capabilityClass, param);

                // Create Response Object classes for each operation result declaration...
                string defaultSuccess = context.GetConfigProperty(_DefaultSuccessCode);
                this._responseBodyDocument = operation.ResponseDocument;
                foreach (RESTOperationResultDeclaration result in operation.OperationResults)
                {
                    // We need to perform a little trick in case of response body definitions: the associated Document Resource has been assigned
                    // to the Operation and not to the Operation Result. However, it must be passed to the appropriate result in order to properly
                    // establish the association. So, we must check whether we have 'caught' the default OK response and if so, patch the Result
                    // Declaration object so it holds the class reference...
                    if (result.ResultCode == defaultSuccess)
                    {
                        MEClass newResponseClass = operation.ResponseDocument != null ? operation.ResponseDocument.CapabilityClass : null;
                        if (result.ResponseDocumentClass != newResponseClass)
                        {
                            result.ResponseDocumentClass = newResponseClass;
                            result.Status = RESTOperationResultDeclaration.DeclarationStatus.Edited;
                        }
                        if (result.HasMultipleResponses != operation.ResponseBodyCardinalityIndicator)
                        {
                            result.HasMultipleResponses = operation.ResponseBodyCardinalityIndicator;
                            result.Status = RESTOperationResultDeclaration.DeclarationStatus.Edited;
                        }
                    }
                    UpdateOperationResult(result);
                    this._responseBodyDocument = operation.ResponseDocument;
                }
                
                if (minorVersionUpdate)
                {
                    var newVersion = new Tuple<int, int>(this._capabilityClass.Version.Item1, this._capabilityClass.Version.Item2 + 1);
                    this._capabilityClass.Version = newVersion;
                }
                CreateLogEntry("Changes made to Operation.");
            }
            return true;
        }

        /// <summary>
        /// Returns the file name (without extension) for this Capability. The extension is left out since this typically depends on the
        /// chosen serialization mechanism. The filename returned by this method only provides a generic name to be used for further, serialization
        /// dependent, processing.
        /// </summary>
        internal override string GetBaseFileName()
        {
            Tuple<int, int> version = this.CapabilityClass.Version;
            return this._rootService.Name + "_" + this.Name + "_v" + version.Item1 + "p" + version.Item2;
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

        /// <summary>
        /// This helper method is used to update the Operation Result object specified by the 'result' parameter.
        /// Depending on the result status, a new entry is created, an existing entry removed or an existing entry is updated.
        /// </summary>
        /// <param name="result">Operation Result declaration metadata.</param>
        private void UpdateOperationResult(RESTOperationResultDeclaration result)
        {
            string defaultResponse = ContextSlt.GetContextSlt().GetConfigProperty(_DefaultResponseCode);
            var myInterface = new RESTOperationCapability(this);
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp.UpdateOperationResult >> Updating result code '" + result.ResultCode + "'...");

            // Default response can not be edited and invalid status should be ignored...
            if (result.ResultCode == defaultResponse || result.Status == RESTOperationResultDeclaration.DeclarationStatus.Invalid) return;
            else
            {
                if (result.Status == RESTOperationResultDeclaration.DeclarationStatus.Created)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp.UpdateOperationResult >> Creating new result object...");
                    RESTOperationResultCapability newResult = new RESTOperationResultCapability(myInterface, result);
                    newResult.InitialiseParent(myInterface);
                }
                else if (result.Status == RESTOperationResultDeclaration.DeclarationStatus.Deleted)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp.UpdateOperationResult >> Removing result object...");
                    foreach (Capability cap in GetChildren())
                    {
                        if (cap is RESTOperationResultCapability && ((RESTOperationResultCapability)cap).ResultCode == result.ResultCode)
                        {
                            DeleteChild(cap.Implementation, true);
                            return;
                        }
                    }
                }
                else if (result.Status == RESTOperationResultDeclaration.DeclarationStatus.Edited)
                {
                    foreach (Capability cap in GetChildren())
                    {
                        if (cap is RESTOperationResultCapability && ((RESTOperationResultCapability)cap).ResultCode == result.OriginalCode)
                        {
                            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp.UpdateOperationResult >> Edit result object...");
                            ((RESTOperationResultCapability)cap).Edit(result);
                            return;
                        }
                    }
                }
            }
        }
    }
}
