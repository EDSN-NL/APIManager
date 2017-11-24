using System;
using System.IO;
using System.Text;
using System.Runtime.Remoting;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using Framework.Util.SchemaManagement;
using Framework.Util.SchemaManagement.JSON;
using Framework.Logging;
using Framework.Util;
using Framework.Context;
using Plugin.Application.CapabilityModel.ASCIIDoc;
using Plugin.Application.CapabilityModel.SchemaGeneration;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// The OpenAPI 2.0 Processor is a Capability Processor that takes an Interface Capability node and creates an OpenAPI 2.0 definition file, 
    /// otherwise known as 'Swagger file'. It only supports processing from the Interface downwards.
    /// </summary>
    internal partial class OpenAPI20Processor : CapabilityProcessor
    {
        // Configuration properties used by this module...
        private const string _NSTokenTag                = "NSTokenTag";
        private const string _SchemaTokenName           = "SchemaTokenName";
        private const string _EmptyResourceName         = "EmptyResourceName";

        private ProgressPanelSlt _panel;                // Contains the progress panel that we're using to report progress.
        private int _panelIndex;                        // The Panel indentation index assigned to this processor.
        private Schema _schema;                         // The Schema to be used for all definitions.
        private bool _isPathInitialized;                // Global setting that assures that the export path is obtained exactly once.
        private string _APIAccessLevel;                 // Set to the access level of the API.
        private List<Tuple<string, string>> _accessLevels;          // List of Access Levels for each operation.
        private StringWriter _outputWriter;             // Will eventually generate the actual OpenAPI output file.
        private JsonTextWriter _JSONWriter;             // Used to format the JSON code for the OpenAPI output stream.
        private string _currentPath;                    // Contains the OpenAPI Path that is currently being processed.
        private RESTResourceCapability _currentResource;    // The resource that is currently being processed.

        // Since we have to terminate JSON objects properly, we must know whether we are in the last operation or operation result of a resource.
        // If we start a new resource, we might have to close the previous one. Also, we have to close the last resource but this we can handle at
        // the beginning of post processing.
        private bool _inOperation;                      // Global context: we are processing an operation.
        private bool _inOperationResult;                // Global context: we are processing an operation result.

        /// <summary>
        /// Returns a filename that is constructed as 'Capability_vxpy.json', in which: 'Capability' is the verbatim name of the current 
        /// Capability, 'x' is the major version of that Capability and 'y' is the minor version (e.g. 'MyCapability_v1p0.json').
        /// The name-constructing logic is build into the Capabilities ('BaseFileName'), which returns a name without extension.
        /// </summary>
        /// <returns>Capability-specific filename.</returns>
        internal override string GetCapabilityFilename()
        {
            return this._currentCapability.BaseFileName + ".json";
        }

        /// <summary>
        /// Derived classes must return a processor-specific identifier that can be shown to the user in order to facilitate selection
        /// of a specific processor from a possible list of processors. 
        /// </summary>
        /// <returns>Processor specific identifier.</returns>
        internal override string GetID()
        {
            return "OpenAPI 2.0 (Swagger) Processor.";
        }

        /// <summary>
        /// Since Capability Processors are constructed once during load time, we need an alternative method for (re-)initialization.
        /// The Initialize method is called before 'handing over' a pre-loaded processor from the ProcessorManager factory to the processing events.
        /// The method must make sure that the processor instance is "ready for use".
        /// </summary>
        internal override void Initialize()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.Initialize >> Initializing processor.");
            base.Initialize();
            this._panel = null;
            this._panelIndex = 0;
            this._schema = null;
            this._isPathInitialized = false;
            this._APIAccessLevel = string.Empty;
            this._accessLevels = null;
        }

        /// <summary>
        /// Main entry for processing of a single Operation Capability.
        /// </summary>
        /// <param name="capability">The capability to be processed.</param>
        /// <param name="stage">The current processing stage.</param>
        /// <returns>True on successfull processing, false on errors.</returns>
        internal override bool ProcessCapability(Capability capability, ProcessingStage stage)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.ProcessCapability >> Processing capability: '" +
                             capability.Name + "' of type '" + capability.GetType().ToString() + "' in stage: '" + stage + "'...");

            ContextSlt context = ContextSlt.GetContextSlt();
            bool result = true;
            bool useCommonDocContext = context.GetBoolSetting(FrameworkSettings._DocGenUseCommon);
            this._currentCapability = capability;
            this._currentService = capability.RootService as ApplicationService;

            if (this._currentCapability == null || this._currentService == null)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.processCapability >> Illegal context, aborting!");
                return false;
            }

            try
            {
                switch (stage)
                {
                    // Pre-processing stage is used to initialize the progress panel and to create the Common Schema (OpenAPI processors ALWAYS use the
                    // Common Schema since all type information must be collected in a single definitions section).
                    // Also, we attempt to create the absolute output path for the schema files and we initialize some other resources...
                    case ProcessingStage.PreProcess:
                        if (capability is InterfaceCapability)
                        {
                            // The bar-size is based on 6 steps per child. This is a wild guess but we don't want to spent hours figuring out the exact scale.
                            this._panel = ProgressPanelSlt.GetProgressPanelSlt();
                            this._panel.ShowPanel("Processing API: " + capability.Name, capability.SelectedChildrenCount * 6);
                            this._panel.WriteInfo(this._panelIndex, "Pre-processing Interface: '" + this._currentCapability.Name + "'...");
                            ClassCacheSlt.GetClassCacheSlt().Flush();   // Assures that we start with an empty cache.
                            DocManagerSlt.GetDocManagerSlt().Flush();   // Assures that 'old' documentation nodes are removed.
                            this._panel.IncreaseBar(1);

                            // Initialize our resources and open the JSON output stream...
                            var itf = capability as RESTInterfaceCapability;
                            this._accessLevels = new List<Tuple<string, string>>();
                            this._outputWriter = new StringWriter();
                            this._JSONWriter = new JsonTextWriter(this._outputWriter);
                            this._JSONWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
                            this._JSONWriter.WriteStartObject();
                            BuildHeader(this._JSONWriter, itf); // Prepare the generic OpenAPI data.

                            // Retrieve the schema to be used for all definitions...
                            string tokenName = context.GetConfigProperty(_SchemaTokenName);
                            this._schema = GetSchema(capability.Name, tokenName, 
                                                     this._currentService.GetFQN("RESTOperation", Conversions.ToPascalCase(capability.AssignedRole), -1),
                                                     capability.VersionString);

                            // Initialise the documentation context and other resources...
                            if (useCommonDocContext) DocManagerSlt.GetDocManagerSlt().InitializeCommonDocContext(tokenName, capability.Name, MEChangeLog.GetDocumentationAsText(capability.CapabilityClass));
 
                            this._inOperation = false;
                            this._inOperationResult = false;

                            if (!this._isPathInitialized)
                            {
                                // Below sequence assures that we have an output path for our Interface file...
                                string myPath = capability.CapabilityClass.GetTag(context.GetConfigProperty(_PathNameTag));
                                if (string.IsNullOrEmpty(myPath)) myPath = string.Empty;
                                if (result = (!string.IsNullOrEmpty(this._currentService.AbsolutePath)) || this._currentService.InitializePath(myPath))
                                {
                                    capability.CapabilityClass.SetTag(context.GetConfigProperty(_PathNameTag), capability.RootService.ComponentPath);
                                    this._isPathInitialized = true;
                                }
                            }
                        }
                        this._panel.IncreaseBar(1);
                        break;

                    // Processing stage is used to create the actual Schema instances for all Operations...
                    case ProcessingStage.Process:
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.ProcessCapability >> Processing '" + 
                                         capability.GetType().ToString() + "->" + capability.Name + "'...");
                        if (capability is RESTInterfaceCapability)
                        {
                            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.ProcessCapability >> Starting 'paths' section...");
                            this._JSONWriter.WritePropertyName("paths");
                            this._JSONWriter.WriteStartObject();        // Start 'paths' section object in the OpenAPI interface.
                            this._currentResource = null;               // Assures that no 'old' resources remain lingering around.
                            this._currentPath = string.Empty;           // Assures that the path is reset.
                        }
                        else if (capability is RESTResourceCapability)
                        {
                            this._panel.WriteInfo(this._panelIndex + 1, "Processing Resource '" + capability.Name + "'...");
                            if (this._inOperationResult)
                            {
                                this._JSONWriter.WriteEndObject();      // Close previous response parameter.
                                this._JSONWriter.WriteEndObject();      // And close the 'responses' section.
                            }
                            this._inOperationResult = false;

                            DefinePath(capability as RESTResourceCapability);
                        }
                        else if (capability is RESTOperationCapability)
                        {
                            if (this._inOperationResult)
                            {
                                this._JSONWriter.WriteEndObject();      // Close previous response parameter.
                                this._JSONWriter.WriteEndObject();      // And close the 'responses' section.
                            }
                            this._inOperationResult = false;
                            this._panel.WriteInfo(this._panelIndex + 2, "Processing Operation '" + capability.Name + "'...");

                            this._inOperation = true;
                            BuildOperation(capability as RESTOperationCapability);
                        }
                        else if (capability is RESTOperationResultCapability)
                        {
                            this._panel.WriteInfo(this._panelIndex + 3, "Processing Operation Result'" + capability.Name + "'...");
                            if (!this._inOperationResult)
                            {
                                this._JSONWriter.WritePropertyName("responses");
                                this._JSONWriter.WriteStartObject();
                                this._inOperationResult = true;
                            }
                            BuildOperationResult(capability as RESTOperationResultCapability);
                        }
                        this._panel.IncreaseBar(1);
                        break;

                    // During post-processing we're saving the generated interface as well as all documentation...
                    // Since processing stage ended with path-processing, the first thing we do here is end the 'paths' section.
                    // We use Interface Capability for re-initializing our context since that will be the first capability to be called
                    // in post-processing stage...
                    case ProcessingStage.PostProcess:
                        if (capability is RESTInterfaceCapability)
                        {
                            var itf = capability as RESTInterfaceCapability;
                            this._panel.WriteInfo(this._panelIndex, "Finalizing Interface '" + capability.Name + "'...");
                            if (this._inOperationResult)
                            {
                                this._JSONWriter.WriteEndObject();      // Close previous response parameter.
                                this._JSONWriter.WriteEndObject();      // And close the 'responses' section.
                            }
                            this._JSONWriter.WriteEndObject();          // End 'paths' section object.
                            this._JSONWriter.WriteEndObject();          // End of OpenAPI definition object.
                            this._JSONWriter.Flush();
                            result = SaveProcessedCapability();

                            // Save the collected documentation...
                            DocManagerSlt.GetDocManagerSlt().Save(this._currentService.AbsolutePath, itf.BaseFileName, itf.Name,
                                                                  MEChangeLog.GetDocumentationAsText(itf.CapabilityClass));
                        }
                        this._panel.IncreaseBar(1);

                        // Release resources...
                        this._isPathInitialized = false;
                        this._panel.Done();
                        this._JSONWriter.Close();
                        this._outputWriter.Close();
                        ClassCacheSlt.GetClassCacheSlt().Flush();   // Remove all collected resources on exit.
                        break;

                    // Cancelling stage is used to send a message and deleting the Common Schema...
                    // We close the panel on an InterfaceCapability cancel, since this is also the one that created it...
                    case ProcessingStage.Cancel:
                        // TODO: CHECK HOW TO CHANGE THIS...
                        if (capability is RESTInterfaceCapability)
                        {
                            this._panel.WriteWarning(this._panelIndex, "Processing cancelled!");
                            this._panel.IncreaseBar(9999);
                            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.ProcessCapability >> Deallocation resources!");
                            this._isPathInitialized = false;
                            this._panel.Done();
                            ClassCacheSlt.GetClassCacheSlt().Flush();   // Remove resources.
                        }
                        break;

                    default:
                        break;
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.ProcessCapability >> Caught exception: " + exc);
                result = false;
            }
            return result;
        }

        /// <summary>
        /// This method is invoked when a (new) resource is passed in the 'Processing' phase.
        /// Postcondition is a path prepared for further processing.
        /// </summary>
        /// <param name="resource">The new resource that we are going to process.</param>
        private bool DefinePath (RESTResourceCapability resource)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.DefinePath >> Received new resource '" + resource.Name + "'...");
            if (resource.IsRootLevel)
            {
                // If we have a root-level resource, this implies that we must start a new sub-API. If the current path has contents, we were processing
                // another sub-API and this must be terminated first (WriteEndObject). Next, we re-initialize the path using the current resource
                // name and start a new path object.
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.DefinePath >> Rootlevel resource, simply reset current path");
                if (this._currentPath != string.Empty) this._JSONWriter.WriteEndObject(); // Close previous path object.
                this._currentPath = "/" + RESTUtil.GetAssignedRoleName(resource.Name);
            }
            else
            {
                // If we have a non-root (= intermediate) resource, we MUST have a previous resource and a path. If not, something has gone wrong
                // in the processing sequence!
                if (this._currentResource == null || string.IsNullOrEmpty(this._currentPath))
                {
                    Logger.WriteError("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.DefinePath >> Wrong context for a non-root resource!");
                    return false;
                }

                if (this._currentResource.HasOperations()) this._JSONWriter.WriteEndObject(); // Close previous path object.

                // If the current resource is a NOT a child of the 'previous current' resource, we have reached a 'fork' in the tree and must
                // replace the last path entry by a new one.
                if (resource.Parent != this._currentResource.Implementation)
                {
                    // Remove the previous child resource...
                    this._currentPath = this._currentPath.Substring(0, this._currentPath.LastIndexOf('/'));
                }
                if (resource.Name != ContextSlt.GetContextSlt().GetConfigProperty(_EmptyResourceName))
                    this._currentPath += "/" + RESTUtil.GetAssignedRoleName(resource.Name);
            }

            // We must write the path ONLY if the new resource has operations. Otherwise, this is just an intermediate URL element...
            if (resource.HasOperations())
            {
                this._JSONWriter.WritePropertyName(this._currentPath);
                this._JSONWriter.WriteStartObject();
            }
            this._currentResource = resource;
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.DefinePath >> Current path: '" + this._currentPath + "'...");
            return true;
        }

        /// <summary>
        /// This method is invoked when we want to save the processed Interface Capability to an output file. Since all definitions are collected
        /// into a single OpenAPI 2.0 file, the Interface is in fact the ONLY capability that gets to be saved!
        /// The method does not perform any control operations on the stream (e.g. does not attempt to 
        /// close the stream).
        /// </summary>
        /// <param name="stream">Stream that must receive processed Capability contents.</param>
        protected override void SaveContents(FileStream stream)
        {
            if (this._currentCapability is RESTInterfaceCapability)
            {
                string generatedOutput = this._outputWriter.ToString();
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.BuildHeader >> Created output:" + Environment.NewLine + generatedOutput);
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8)) writer.Write(generatedOutput);
            }
        }

        /// <summary>
        /// Retrieves a schema implementation for a REST Schema. When retrieved Ok, 
        /// the instance is explicitly initialized and returned to the caller.
        /// </summary>
        /// <param name="name">A meaningfull name, with which we can identify the schema.</param>
        /// <param name="namespaceToken">Namespace token.</param>
        /// <param name="ns">Schema namespace, preferably an URI.</param>
        /// <param name="version">Major, minor and build number of the schema. When omitted, the version defaults to '1.0.0'</param>
        /// <returns>Appropriate Schema implementation.</returns>
        /// <exception cref="MissingFieldException">No schema implementation has been defined for the current interface type.</exception>
        private Schema GetSchema(string name, string namespaceToken, string schemaNamespace, string version = "1.0.0")
        {
            string itfTypeKey = string.Empty;
            try
            {
                ContextSlt context = ContextSlt.GetContextSlt();
                itfTypeKey = "InterfaceType:REST";
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.GetSchema >> Retrieving schema implementation for '" + itfTypeKey + "'...");
                ObjectHandle handle = Activator.CreateInstance(null, context.GetConfigProperty(itfTypeKey));
                var proc = handle.Unwrap() as Schema;
                if (proc != null)
                {
                    proc.Initialize(Schema.SchemaType.Collection, name, namespaceToken, schemaNamespace, version);
                    if (!(proc is JSONSchema))
                    {
                        string message = "Unknown schema implementation '" + proc.GetType() + "'  has been defined for key '" + itfTypeKey + "'!";
                        Logger.WriteError("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.GetSchema >> " + message);
                        throw new MissingFieldException(message);
                    }
                    return proc;
                }
                else
                {
                    string message = "No (valid) schema implementation has been defined for key '" + itfTypeKey + "'!";
                    Logger.WriteError("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.GetSchema >> " + message);
                    throw new MissingFieldException(message);
                }
            }
            catch (Exception exc)
            {
                string message = "Caught exception when retrieving schema for key '" + itfTypeKey + "'!" + Environment.NewLine + exc.Message;
                Logger.WriteError("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.GetSchema >> " + message);
                throw new MissingFieldException(message);
            }
        }
    }
}
