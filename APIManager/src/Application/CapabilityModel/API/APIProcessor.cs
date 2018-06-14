using System;
using System.IO;
using System.Text;
using System.Runtime.Remoting;
using System.Collections.Generic;
using Framework.Util.SchemaManagement;
using Framework.Util.SchemaManagement.XML;
using Framework.Util.SchemaManagement.JSON;
using Framework.Logging;
using Framework.Util;
using Framework.Context;
using Plugin.Application.CapabilityModel.ASCIIDoc;
using Plugin.Application.CapabilityModel.SchemaGeneration;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// The SOAP Processor is a Capability Processor that takes an Interface Capability node and creates a WSDL definition for that Interface.
    /// This WSDL contains operations (and associated schemas) for all operations that are selected by the user.
    /// </summary>
    internal partial class APIProcessor : CapabilityProcessor
    {
        /// <summary>
        /// Helper class that we're using to keep track of processed operations and the associated result per message.
        /// </summary>
        private class ProcessingContext
        {
            private OperationCapability _operation;     // Processed Operation.
            List<MessageCapability> _messageList;       // Messages that actually yielded results for this operation. 

            /// <summary>
            /// Getters for properties:
            /// Operation = processed operation capability
            /// Messages = list of all processed messages for the operation, including 'has output' indicator.
            /// </summary>
            internal OperationCapability Operation        { get { return this._operation; } }
            internal List<MessageCapability> Messages     { get { return this._messageList; } }

            /// <summary>
            /// Creates a new context entry.
            /// </summary>
            /// <param name="operation">The operation for which we're creating the context.</param>
            internal ProcessingContext(OperationCapability operation)
            {
                this._operation = operation;
                this._messageList = new List<MessageCapability>();
            }

            /// <summary>
            /// Add message processing result to this context.
            /// </summary>
            /// <param name="message">Message that has actually yielded a result.</param>
            internal void AddMessage (MessageCapability message)
            {
                this._messageList.Add(message);
            }
        }

        // Currently, the API Processor supports SOAP (WSDL) and REST (OpenAPI).
        private enum InterfaceType { SOAP, REST, Unknown}

        // Configuration properties used by this module...
        private const string _MessageAssemblyRoleName   = "MessageAssemblyRoleName";
        private const string _MessageHeaderRoleName     = "MessageHeaderRoleName";
        private const string _NSTokenTag                = "NSTokenTag";
        private const string _AccessLevelTag            = "AccessLevelTag";
        private const string _AccessLevels              = "AccessLevels";
        private const string _CommonSchemaRoleName      = "CommonSchemaRoleName";
        private const string _CommonSchemaNSToken       = "CommonSchemaNSToken";

        private ProgressPanelSlt _panel;                // Contains the progress panel that we're using to report progress.
        private int _panelIndex;                        // The Panel indentation index assigned to this processor.
        private Schema _commonSchema;                   // The Common Schema to be used (but only when specified).
        private Schema _currentSchema;                  // The Operation Schema that we're currently building.
        private InterfaceType _interfaceType;           // Defines our Interface type. 
        private string _currentAccessLevel;             // Set to the access level of the current operation.
        private string _interfaceDeclaration;           // Contains the constructed interface declaration.
        private CommonSchemaCapability _commonSchemaCapability;     // The Common Schema Capability used for this build.
        private List<Tuple<string, string>> _accessLevels;          // List of Access Levels for each operation.
        private List<ProcessingContext> _operationContextList;      // All operations that we've been building for the current Interface.

        /// <summary>
        /// Default constructor, only initializes context to 'empty'. This is the 'generic' constructor that is invoked when utilizing the Schema Processor
        /// as a 'self-containing' entity. In this setup, it does not use the Common Schema, ALL definitions are collected within the schema itself.
        /// The constructor MUST be declared public since it is called by the .Net Invocation framework, which is in another assembly!
        /// </summary>
        public APIProcessor(): base()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.APIProcessor >> Creating new processor.");
            Initialize();
        }

        /// <summary>
        /// Returns a filename that is constructed as '[Service_]Capability_vxpy.(wsdl/xsd/json)', in which: 'Service' is the verbatim name of the current
        /// Service, 'Capability' is the verbatim name of the current Capability, 'x' is the major version of that Capability and 'y' is the minor 
        /// version (e.g. 'MyCapability_v1p0.xsd').
        /// The name of the Service is ONLY included for non-Interface Capabilities (e.g. Operations).
        /// The name-constructing logic is build into the Capabilities ('BaseFileName'), which returns a name without extension.
        /// The created names have a '.wsdl' extension for SOAP Interfaces, an '.xsd' extension for XML Schemas and '.json' for either JSON Schemas
        /// or Swagger Interfaces.
        /// </summary>
        /// <returns>Capability-specific filename.</returns>
        internal override string GetCapabilityFilename()
        {
            if (this._interfaceType == InterfaceType.SOAP)
            {
                return (this._currentCapability is InterfaceCapability) ? this._currentCapability.BaseFileName + ".wsdl" :
                                                                          this._currentCapability.BaseFileName + ".xsd";
            }
            else return this._currentCapability.BaseFileName + ".json";
        }

        /// <summary>
        /// Derived classes must return a processor-specific identifier that can be shown to the user in order to facilitate selection
        /// of a specific processor from a possible list of processors. 
        /// </summary>
        /// <returns>Processor specific identifier.</returns>
        internal override string GetID()
        {
            return "API Processor.";
        }

        /// <summary>
        /// Since Capability Processors are constructed once during load time, we need an alternative method for (re-)initialization.
        /// The Initialize method is called before 'handing over' a pre-loaded processor from the ProcessorManager factory to the processing events.
        /// The method must make sure that the processor instance is "ready for use".
        /// </summary>
        internal override void Initialize()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.APIProcessor.Initialize >> Initializing processor.");
            base.Initialize();
            this._panel = null;
            this._panelIndex = 0;
            this._commonSchema = null;
            this._commonSchemaCapability = null;
            this._currentSchema = null;
            this._accessLevels = null;
            this._currentAccessLevel = string.Empty;
            this._operationContextList = null;
            this._interfaceDeclaration = null;

            ContextSlt context = ContextSlt.GetContextSlt();
            string contractTypeKey = context.GetStringSetting(FrameworkSettings._InterfaceContractType).ToLower();
            this._interfaceType = (contractTypeKey.Contains("soap")) ? InterfaceType.SOAP : InterfaceType.REST;
        }

        /// <summary>
        /// Main entry for processing of a single Operation Capability.
        /// </summary>
        /// <param name="capability">The capability to be processed.</param>
        /// <param name="stage">The current processing stage.</param>
        /// <returns>True on successfull processing, false on errors.</returns>
        internal override bool ProcessCapability(Capability capability, ProcessingStage stage)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.APIProcessor.ProcessCapability >> Processing capability: '" +
                             capability.Name + "' of type '" + capability.GetType().ToString() + "' in stage: '" + stage + "'...");

            ContextSlt context = ContextSlt.GetContextSlt();
            bool result = true;
            bool generateDocumentation = context.GetBoolSetting(FrameworkSettings._DocGenUseGenerateDoc);
            bool useCommonDocContext = context.GetBoolSetting(FrameworkSettings._DocGenUseCommon);
            bool useCommonSchema = context.GetBoolSetting(FrameworkSettings._SMCreateCommonSchema);
            this._currentCapability = capability;
            this._currentService = capability.RootService as ApplicationService;

            if (this._currentCapability == null || this._currentService == null)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.APIProcessor.processCapability >> Illegal context, aborting!");
                return false;
            }

            try
            {
                switch (stage)
                {
                    // Pre-processing stage is used to initialize the progress panel and to create the Common Schema (but only if a Common Schema
                    // Capability is defined for the Interface). Also, we attempt to create the absolute output path for the schema files and
                    // we initialize some other resources...
                    case ProcessingStage.PreProcess:
                        if (capability is InterfaceCapability)
                        {
                            // The bar-size is based on 9 steps per child plus some overhead. This is a wild guess but we don't want to spent 
                            // hours figuring out the exact scale.
                            this._panel = ProgressPanelSlt.GetProgressPanelSlt();
                            this._panel.ShowPanel("Processing Message: " + capability.Name, capability.SelectedChildrenCount * 9 + 6);
                            this._panel.WriteInfo(this._panelIndex, "Pre-processing Interface: '" + this._currentCapability.Name + "'...");
                            ClassCacheSlt.GetClassCacheSlt().Flush();   // Assures that we start with an empty cache.
                            DocManagerSlt.GetDocManagerSlt().Flush();   // Assures that 'old' documentation nodes are removed.

                            // Initialize our resources...
                            this._accessLevels = new List<Tuple<string, string>>();
                            this._operationContextList = new List<ProcessingContext>();
                            capability.CapabilityClass.SetTag(context.GetConfigProperty(_PathNameTag), this._currentService.ServiceBuildPath);
                        }
                        else if (capability is CommonSchemaCapability && useCommonSchema)
                        {
                            this._commonSchemaCapability = capability as CommonSchemaCapability;
                            string commonSchemaName = this._currentService.Name + "." + Conversions.ToPascalCase(capability.AssignedRole);
                            string alternativeNamespaceTag = this._commonSchemaCapability.AlternativeNamespaceTag;
                            string namespaceTag = (this._interfaceType == InterfaceType.SOAP) ? "SOAPOperation" : "RESTOperation";
                            if (alternativeNamespaceTag != string.Empty) namespaceTag = alternativeNamespaceTag;
                            this._panel.WriteInfo(this._panelIndex + 1, "Pre-processing Common Schema...");
                            this._commonSchema = GetSchema(Schema.SchemaType.Common, commonSchemaName,
                                                           capability.CapabilityClass.GetTag(context.GetConfigProperty(_NSTokenTag)),
                                                           this._currentService.GetFQN(namespaceTag, Conversions.ToPascalCase(capability.AssignedRole), -1),
                                                           capability.VersionString);
                            if (generateDocumentation && useCommonDocContext)
                                DocManagerSlt.GetDocManagerSlt().InitializeCommonDocContext(this._commonSchema.NSToken, commonSchemaName, 
                                                                                            MEChangeLog.GetDocumentationAsText(capability.CapabilityClass));
                        }
                        this._panel.IncreaseBar(1);
                        break;

                    // Processing stage is used to create the actual Schema instances for all Operations...
                    case ProcessingStage.Process:
                        // First of all, we check whether 'useCommonDocContext' is set and if we don't have a valid common doc context at this point, we 
                        // probably don't have a Common Schema capability as well so it has never been initialized properly. If this is the case, we
                        // create a common doc context here...
                        if (generateDocumentation && useCommonDocContext && DocManagerSlt.GetDocManagerSlt().CommonDocContext == null)
                        {
                            DocManagerSlt.GetDocManagerSlt().InitializeCommonDocContext(context.GetConfigProperty(_CommonSchemaNSToken),
                                                                                        context.GetConfigProperty(_CommonSchemaRoleName), 
                                                                                        string.Empty);
                        }

                        if (capability is OperationCapability)
                        {
                            this._panel.WriteInfo(this._panelIndex + 1, "Processing Operation '" + capability.Name + "'...");
                            result = BuildSchema(capability as OperationCapability);
                        }
                        this._panel.IncreaseBar(1); // Is called for ALL capabilities, including Messages and Common Schema!
                        break;

                    // During post-processing we're saving the generated interface as well as all documentation...
                    case ProcessingStage.PostProcess:
                        if (capability is InterfaceCapability)
                        {
                            var itf = capability as InterfaceCapability;
                            this._panel.WriteInfo(this._panelIndex, "Building Interface '" + capability.Name + "'...");
                            result = (this._interfaceType == InterfaceType.SOAP)? BuildSOAPInterface(itf): true;

                            // Save the collected documentation...
                            if (generateDocumentation) DocManagerSlt.GetDocManagerSlt().Save(this._currentService.ServiceCIPath, itf.BaseFileName, itf.Name,
                                                                                             MEChangeLog.GetDocumentationAsText(itf.CapabilityClass));
                        }
                        else if (capability is CommonSchemaCapability && useCommonSchema)
                        {
                            // If we have a Common Schema, we must save it here, but only if we actually used it...
                            if (result = SaveProcessedCapability())
                            {
                                this._panel.WriteInfo(this._panelIndex, "Interface generation has been completed successfully.");
                                this._panel.WriteInfo(this._panelIndex, "Output written to: '" + this._currentService.ServiceCIPath + "'.");
                            }
                            else this._panel.WriteError(this._panelIndex, "Unable to save Common Schema file!");
                        }
                        this._panel.IncreaseBar(1);

                        if (capability is CommonSchemaCapability || (this._commonSchema == null && capability is InterfaceCapability))
                        {
                            // End-state is post-processing of the Common Schema. In the unlikely case that we don't have a Common Schema,
                            // we will use the Interface instead. Operation Capabilities don't have post-processing.
                            // Release resources...
                            this._commonSchema = null;
                            this._panel.Done();
                            this._accessLevels = null;
                            this._interfaceDeclaration = null;
                            ClassCacheSlt.GetClassCacheSlt().Flush();   // Remove all collected resources on exit.
                        }
                        break;

                    // Cancelling stage is used to send a message and deleting the Common Schema...
                    // We close the panel on an InterfaceCapability cancel, since this is also the one that created it...
                    case ProcessingStage.Cancel:
                        if (capability is CommonSchemaCapability || (this._commonSchema == null && capability is OperationCapability))
                        {
                            this._panel.WriteWarning(this._panelIndex, "Processing cancelled!");
                            this._panel.IncreaseBar(9999);
                            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.APIProcessor.ProcessCapability >> Deallocation resources!");
                            this._commonSchema = null;
                            this._commonSchemaCapability = null;
                            this._interfaceDeclaration = null;
                            this._panel.Done();
                            this._accessLevels = null;
                            this._operationContextList = null;
                            ClassCacheSlt.GetClassCacheSlt().Flush();   // Remove resources.
                        }
                        break;

                    default:
                        break;
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.APIProcessor.ProcessCapability >> Caught exception: " + exc);
                result = false;
            }
            return result;
        }

        /// <summary>
        /// This method is invoked when we want to save the processed Capability to an output file. Actions to be taken depend on the type
        /// of Capability that we want to save. The method does not perform any control operations on the stream (e.g. does not attempt to 
        /// close the stream).
        /// </summary>
        /// <param name="stream">Stream that must receive processed Capability contents.</param>
        protected override void SaveContents(FileStream stream)
        {
            ContextSlt context = ContextSlt.GetContextSlt();

            if (this._currentCapability is InterfaceCapability)
            {
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8)) writer.Write(this._interfaceDeclaration);
            }
            else if (this._currentCapability is OperationCapability)
            {
                // The schema requires a reference to the common schema since the schema compiler must have access to ALL imported/included schema sets!!
                if (this._commonSchema != null) this._currentSchema.AddSchemaReference(this._commonSchema);
                
                // Since JSON schema does not know about comments, we skip the header in case of REST services... 
                string header = (this._interfaceType == InterfaceType.SOAP) ? BuildHeader(context.GetResourceString(FrameworkSettings._SOAPSchemaHeader)) : null;
                this._currentSchema.Save(stream, header);
            }
            else if (this._currentCapability is CommonSchemaCapability && this._commonSchema != null)
            {
                // Since JSON schema does not know about comments, we skip the header in case of REST services... 
                string header = (this._interfaceType == InterfaceType.SOAP) ? BuildHeader(context.GetResourceString(FrameworkSettings._SOAPSchemaHeader)) : null;
                this._commonSchema.Save(stream, header);
            }
        }

        /// <summary>
        /// Helper method that formats a schema- or interface header for use in export. The method replaces the header placeholders by the actual property
        /// contents retrieved from our current Service & Capability.
        /// </summary>
        /// <param name="template">Capability-specific header template.</param>
        /// <returns>Header as a formatted string</returns>
        private string BuildHeader(string template)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.APIProcessor.BuildHeader >> Going to format header for capability: " + this._currentCapability.Name);

            ContextSlt context = ContextSlt.GetContextSlt();
            MEChangeLog.MigrateLog(this._currentCapability.CapabilityClass);

            template = template.Replace("@SERVICE@", this._currentService.Name);
            template = template.Replace("@CAPABILITY@", Conversions.ToPascalCase(this._currentCapability.AssignedRole));
            template = template.Replace("@CAPABILITYTYPE@", this._currentCapability.CapabilityType);
            template = template.Replace("@AUTHOR@", this._currentCapability.Author);
            template = template.Replace("@TIMESTAMP@", DateTime.Now.ToString());
            template = template.Replace("@YEAR@", DateTime.Now.Year.ToString());
            template = template.Replace("@VERSION@", this._currentCapability.VersionString + " Build: " + this._currentService.BuildNumber);
            template = template.Replace("@ACCESSLEVEL@", this._currentAccessLevel);

            string annotation = this._currentCapability.CapabilityClass.Annotation;
            if (!string.IsNullOrEmpty(annotation))
            {
                var newLog = new MEChangeLog(context.TransformRTF(annotation, RTFDirection.ToRTF));
                template = template.Replace("@CHANGELOG@", newLog.GetLogAsText());
            }
            else template = template.Replace("@CHANGELOG@", string.Empty);

            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.APIProcessor.BuildHeader >> Created header:\n" + template);
            return template;
        }

        /// <summary>
        /// Retrieves a schema implementation for a Schema according to current interface type settings. When retrieved Ok, 
        /// the instance is explicitly initialized and returned to the caller.
        /// </summary>
        /// <param name="type">The type of schema (Collection, Operation or Message).</param>
        /// <param name="name">A meaningfull name, with which we can identify the schema.</param>
        /// <param name="namespaceToken">Namespace token.</param>
        /// <param name="ns">Schema namespace, preferably an URI.</param>
        /// <param name="version">Major, minor and build number of the schema. When omitted, the version defaults to '1.0.0'</param>
        /// <returns>Appropriate Schema implementation.</returns>
        /// <exception cref="MissingFieldException">No schema implementation has been defined for the current interface type.</exception>
        private Schema GetSchema(Schema.SchemaType type, string name, string namespaceToken, string schemaNamespace, string version = "1.0.0")
        {
            string itfTypeKey = string.Empty;
            try
            {
                ContextSlt context = ContextSlt.GetContextSlt();
                itfTypeKey = "InterfaceType:" + context.GetStringSetting(FrameworkSettings._InterfaceContractType);
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.APIProcessor.GetSchema >> Retrieving schema implementation for '" + itfTypeKey + "'...");
                ObjectHandle handle = Activator.CreateInstance(null, context.GetConfigProperty(itfTypeKey));
                var proc = handle.Unwrap() as Schema;
                if (proc != null)
                {
                    proc.Initialize(type, name, namespaceToken, schemaNamespace, version);
                    if (!(proc is XMLSchema) && !(proc is JSONSchema))
                    {
                        string message = "Unknown schema implementation '" + proc.GetType() + "'  has been defined for key '" + itfTypeKey + "'!";
                        Logger.WriteError("Plugin.Application.CapabilityModel.API.APIProcessor.GetSchema >> " + message);
                        throw new MissingFieldException(message);
                    }
                    return proc;
                }
                else
                {
                    string message = "No (valid) schema implementation has been defined for key '" + itfTypeKey + "'!";
                    Logger.WriteError("Plugin.Application.CapabilityModel.API.APIProcessor.GetSchema >> " + message);
                    throw new MissingFieldException(message);
                }
            }
            catch (Exception exc)
            {
                string message = "Caught exception when retrieving schema for key '" + itfTypeKey + "'!" + Environment.NewLine + exc.Message;
                Logger.WriteError("Plugin.Application.CapabilityModel.API.APIProcessor.GetSchema >> " + message);
                throw new MissingFieldException(message);
            }
        }
    }
}
