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
        private const string _RequestMessageAssemblyClassName   = "RequestMessageAssemblyClassName";
        private const string _ResponseMessageAssemblyClassName  = "ResponseMessageAssemblyClassName";
        private const string _RequestMessageRoleName            = "RequestMessageRoleName";
        private const string _MessageAssemblyClassStereotype    = "MessageAssemblyClassStereotype";
        private const string _RequestPkgName                    = "RequestPkgName";
        private const string _ResponsePkgName                   = "ResponsePkgName";
        private const string _RequestPkgStereotype              = "RequestPkgStereotype";
        private const string _ResponsePkgStereotype             = "ResponsePkgStereotype";
        private const string _ArchetypeTag                      = "ArchetypeTag";
        private const string _PaginationClassName               = "PaginationClassName";
        private const string _PaginationRoleName                = "PaginationRoleName";
        private const string _APISupportModelPathName           = "APISupportModelPathName";
        private const string _DefaultSuccessCode                = "DefaultSuccessCode";
        private const string _ConsumesMIMEListTag               = "ConsumesMIMEListTag";
        private const string _ProducesMIMEListTag               = "ProducesMIMEListTag";

        private RESTResourceCapability _parent;                     // Parent resource capability that owns this operation.
        private RESTOperationCapability.OperationType _archetype;   // The HTTP operation type associated with the operation.
        private List<string> _producedMIMETypes;                    // List of non-standard MIME types produced by the operation.
        private List<string> _consumedMIMETypes;                    // List of non-standard MIME types consumed by the operation.

        /// <summary>
        /// Getters for class properties:
        /// HTTPType = Returns the HTTP operation type that is associated with this REST operation (as an enumeration).
        /// HTTPTypeName = Returns the name of the HTTP operation as a lowercase string.
        /// ConsumedMIMEList = Returns list of non-standard MIME types consumed by the operation.
        /// ProducedMIMEList = Returns list of non-standard MIME types produced by the operation.
        /// ParentResource = The resource that 'owns' this operation.
        /// </summary>
        internal RESTOperationCapability.OperationType HTTPType { get { return this._archetype; } }
        internal string HTTPTypeName                            { get { return this._archetype.ToString("G").ToLower(); } }
        internal List<string> ConsumedMIMEList                  { get { return this._consumedMIMETypes; } }
        internal List<string> ProducedMIMEList                  { get { return this._producedMIMETypes; } }
        internal RESTResourceCapability ParentResource          { get { return this._parent; } }

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

                this._capabilityClass = OperationPackage.CreateClass(operation.Name, context.GetConfigProperty(_RESTOperationClassStereotype));
                if (this._capabilityClass != null)
                {
                    this._capabilityClass.SetTag(context.GetConfigProperty(_ArchetypeTag), operation.Archetype.ToString());
                    this._capabilityClass.Version = new Tuple<int, int>(parentResource.RootService.MajorVersion, 0);
                    this._assignedRole = operation.Name;
                    this._capabilityClass.SetTag(context.GetConfigProperty(_ArchetypeTag), this._archetype.ToString());

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

                    // Load the documentation...
                    List<string> documentation = new List<string>();
                    if (!string.IsNullOrEmpty(operation.Summary)) documentation.Add("Summary: " + operation.Summary);
                    if (!string.IsNullOrEmpty(operation.Description)) documentation.Add(operation.Description);
                    if (documentation.Count > 0) MEChangeLog.SetRTFDocumentation(this._capabilityClass, documentation);

                    // Explicitly request a new Operation ID for this class (could not do this in the parent constructor since the capabilityClass
                    // object has not been initialized yet at that point).
                    AssignNewOperationID();

                    // Establish link with our Parent...
                    var parentEndpoint = new EndpointDescriptor(parentResource.CapabilityClass, "1", parentResource.AssignedRole, null, false);
                    var operationEndpoint = new EndpointDescriptor(this._capabilityClass, "1", this._assignedRole, null, true);
                    model.CreateAssociation(parentEndpoint, operationEndpoint, MEAssociation.AssociationType.MessageAssociation);
                    InitialiseParent(parentResource);

                    // Check whether we must create an association with a request body...
                    // Since all body classes end up in the same namespace, we create a unique name by combining the name of the operation with
                    // the default name of the Request Message Assembly.
                    // For a request Message Assembly, this results in '<Operation>RequestBodyType' as a resulting name.
                    string classStereotype = context.GetConfigProperty(_MessageAssemblyClassStereotype);
                    if (operation.RequestBodyIndicator)
                    {
                        string pkgName = context.GetConfigProperty(_RequestPkgName);
                        string pkgStereotype = context.GetConfigProperty(_RequestPkgStereotype);
                        string className = this.Name + context.GetConfigProperty(_RequestMessageAssemblyClassName);
                        string cardinality = operation.RequestBodyCardinalityIndicator ? "1..*" : "1";
                        MEPackage msgPackage = OperationPackage.FindPackage(pkgName, pkgStereotype);
                        if (msgPackage == null) msgPackage = OperationPackage.CreatePackage(pkgName, pkgStereotype);
                        MEClass msgClass = msgPackage.FindClass(className, classStereotype);
                        if (msgClass == null) msgClass = msgPackage.CreateClass(className, classStereotype);

                        // Now we can create the association...
                        var msgEndpoint = new EndpointDescriptor(msgClass, cardinality, context.GetConfigProperty(_RequestMessageRoleName), null, true);
                        ModelSlt.GetModelSlt().CreateAssociation(operationEndpoint, msgEndpoint, MEAssociation.AssociationType.MessageAssociation);
                    }

                    // Create Response Object classes for each operation result declaration...
                    // Since all body classes end up in the same namespace, we create a unique name by combining the name of the operation with
                    // the operation result code and the default name of the Response Message Assembly.
                    // For a success response Message Assembly, this results in '<Operation>200ResponseBodyType' as a resulting name.
                    // Since we can also have a result code 'default', we translate all codes to Pascal Case ('Default').
                    string defaultSuccess = context.GetConfigProperty(_DefaultSuccessCode);
                    foreach (RESTOperationResultDeclaration result in operation.OperationResults)
                    {
                        if (operation.ResponseBodyIndicator && result.ResultCode == defaultSuccess)
                        {
                            // We need to link the default result descriptor with a result body. First of all, we must create a 
                            // package and a class for this. They might already exist so check first...
                            string pkgName = context.GetConfigProperty(_ResponsePkgName);
                            string pkgStereotype = context.GetConfigProperty(_ResponsePkgStereotype);
                            string className = this.Name + Conversions.ToPascalCase(result.ResultCode) + 
                                               context.GetConfigProperty(_ResponseMessageAssemblyClassName);
                            MEPackage msgPackage = OperationPackage.FindPackage(pkgName, pkgStereotype);
                            if (msgPackage == null) msgPackage = OperationPackage.CreatePackage(pkgName, pkgStereotype);
                            MEClass msgClass = msgPackage.FindClass(className, classStereotype);
                            if (msgClass == null) msgClass = msgPackage.CreateClass(className, classStereotype);
                            result.HasMultipleResponses = operation.ResponseBodyCardinalityIndicator;
                            result.Parameters = msgClass;   // Assign as result parameters so it will be linked to the result class.
                        }
                        RESTOperationResultCapability newResult = new RESTOperationResultCapability(myInterface, result);
                        newResult.InitialiseParent(myInterface);
                    }

                    // Check whether we have to use Pagination...
                    if (operation.PaginationIndicator)
                    {
                        MEClass paginationClass = model.FindClass(context.GetConfigProperty(_APISupportModelPathName), 
                                                                  context.GetConfigProperty(_PaginationClassName));
                        if (paginationClass != null)
                        {
                            var paginationEndpoint = new EndpointDescriptor(paginationClass, "1", context.GetConfigProperty(_PaginationRoleName), null, true);
                            model.CreateAssociation(operationEndpoint, paginationEndpoint, MEAssociation.AssociationType.MessageAssociation);
                        }
                        else Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp (declaration) >> Unable to retrieve Pagination class '" + 
                                               context.GetConfigProperty(_APISupportModelPathName) + "/" + context.GetConfigProperty(_PaginationClassName) + "'!");
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
                string operationArchetype = this._capabilityClass.GetTag(ContextSlt.GetContextSlt().GetConfigProperty(_ArchetypeTag));
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp (existing) >> Operation is of archetype: '" + operationArchetype + "'...");
                this._archetype = EnumConversions<RESTOperationCapability.OperationType>.StringToEnum(operationArchetype);

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
                var myInterface = new RESTOperationCapability(this);
                foreach (TreeNode<MEClass> node in hierarchy.Children)
                {
                    if (node.Data.HasStereotype(operationResultStereotype))
                    {
                        var resultCap = new RESTOperationResultCapability(myInterface, node.Data);
                        resultCap.InitialiseParent(myInterface);
                        if (!resultCap.Valid)
                        {
                            Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp >> Error creating Operation Result '" + node.Data.Name + "'!");
                            this._capabilityClass = null;
                            return;
                        }
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
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationCapabilityImp.delete >> Deleting the operation and all associated resources...");

            this._parent.RemoveChild(new OperationCapability(this));                // Detaches the operation from the parent.
            base.Delete();                                                          // Deletes the class structure and package.
        }

        /// <summary>
        /// This method is invoked when the user has made one or more changes to an Operation Capability. The method receives an
        /// Operation Declaration object that contains the (updated) information for the Operation.
        /// </summary>
        /// <param name="operation">Updated Operation properties.</param>
        internal void EditOperation(RESTOperationDeclaration operation)
        {
            // NOT YET IMPLEMENTED
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
    }
}
