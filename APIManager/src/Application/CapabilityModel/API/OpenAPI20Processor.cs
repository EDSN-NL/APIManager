using System;
using System.IO;
using System.Text;
using System.Runtime.Remoting;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Framework.Util.SchemaManagement;
using Framework.Util.SchemaManagement.JSON;
using Framework.Logging;
using Framework.Util;
using Framework.Context;
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
        private const string _NSTokenTag                        = "NSTokenTag";
        private const string _SchemaTokenName                   = "SchemaTokenName";
        private const string _EmptyResourceName                 = "EmptyResourceName";
        private const string _RESTHeaderParamClassName          = "RESTHeaderParamClassName";
        private const string _RESTHeaderParamClassStereotype    = "RESTHeaderParamClassStereotype";

        private ProgressPanelSlt _panel;                // Contains the progress panel that we're using to report progress.
        private int _panelIndex;                        // The Panel indentation index assigned to this processor.
        private SchemaProcessor _schema;                // One single Schema Processor will process all our schema stuff.
        private bool _isPathInitialized;                // Global setting that assures that the export path is obtained exactly once.
        private string _APIAccessLevel;                 // Set to the access level of the API.
        private List<Tuple<string, string>> _accessLevels;          // List of Access Levels for each operation.
        private StringWriter _outputWriter;             // Will eventually generate the actual OpenAPI output file.
        private JsonTextWriter _JSONWriter;             // Used to format the JSON code for the OpenAPI output stream.
        private string _currentPath;                    // Contains the OpenAPI Path that is currently being processed.
        private RESTResourceCapability _currentResource;            // The resource that is currently being processed.
        private RESTOperationCapability _currentOperation;          // The operation that is currently being processed.
        private int _capabilityCounter;                 // The total number of capabilities (itf, resource, operation, result) to process.
        private string _defaultResponseClassifier;      // Contains the classifier name of the default response once processed.
        private List<RESTResourceCapability> _identifierList;       // Contains all identifiers detected in the current path. 

        // Since we have to terminate JSON objects properly, we must know whether we are in the last operation result of a resource.
        // If we start a new resource, we might have to close the previous one. Also, we have to close the last resource but this we can handle at
        // the beginning of post processing.    
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
            this._defaultResponseClassifier = string.Empty;
            this._identifierList = null;
            this._currentResource = null;
            this._currentOperation = null;
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
            this._currentCapability = capability;
            this._currentService = capability.RootService as RESTService;

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
                            // We first of all collect a count of all capabilities (excluding our Service). For the bar length, we
                            // use 2x the number of capabilities since that seems to work out fine.
                            this._capabilityCounter = 0;
                            capability.TraverseSelected(ItemCollector);
                            this._panel = ProgressPanelSlt.GetProgressPanelSlt();
                            this._panel.ShowPanel("Processing API: " + capability.Name, this._capabilityCounter * 2);
                            this._panel.WriteInfo(this._panelIndex, "Pre-processing Interface: '" + this._currentCapability.Name + "'...");
                            ClassCacheSlt.GetClassCacheSlt().Flush();       // Assures that we start with an empty cache (no data definitions).
                            RESTSecuritySlt.GetRESTSecuritySlt().Reload();  // Assures that we start with a fresh set of definitions.

                            // Initialize our resources and open the JSON output stream...
                            var itf = capability as RESTInterfaceCapability;
                            this._accessLevels = new List<Tuple<string, string>>();
                            this._currentOperation = null;
                            this._identifierList = new List<RESTResourceCapability>();
                            this._outputWriter = new StringWriter();
                            this._JSONWriter = new JsonTextWriter(this._outputWriter);
                            this._JSONWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
                            this._JSONWriter.WriteStartObject();
                            BuildHeader(this._JSONWriter, itf);     // Prepare the generic OpenAPI data.

                            // Retrieve the schema to be used for all definitions...
                            string tokenName = context.GetConfigProperty(_SchemaTokenName);
                            JSONSchema theSchema = GetSchema(capability.Name, tokenName, 
                                                             this._currentService.GetFQN("RESTOperation", Conversions.ToPascalCase(capability.AssignedRole), -1),
                                                             capability.VersionString) as JSONSchema;
                            // Create global schema processor in 'ad-hoc external schema' mode.
                            if (theSchema != null) this._schema = new SchemaProcessor(theSchema, this._panelIndex + 3);
                            else
                            {
                                Logger.WriteError("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.ProcessCapability >> Unable to create JSON Schema instance!");
                                return false;
                            }

                            // Initialise the documentation context and other resources...
                            this._inOperationResult = false;
                            this._defaultResponseClassifier = string.Empty;

                            if (!this._isPathInitialized)
                            {
                                capability.CapabilityClass.SetTag(context.GetConfigProperty(_PathNameTag), capability.RootService.ServiceBuildPath);
                                this._isPathInitialized = true;
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
                            // We MUST NOT process Document Resources, these are just placeholders for the message schemas!
                            if (((RESTResourceCapability)capability).Archetype != RESTResourceCapability.ResourceArchetype.Document)
                            {
                                this._panel.WriteInfo(this._panelIndex + 1, "Processing Resource '" + capability.Name + "'...");
                                if (this._inOperationResult)
                                {
                                    this._JSONWriter.WriteEndObject();      // Close previous response parameter.
                                    this._JSONWriter.WriteEndObject();      // And close the 'responses' section.
                                    this._currentOperation = null;          // Remove the previous Operation capability.
                                }
                                this._inOperationResult = false;
                                DefinePath(capability as RESTResourceCapability);
                            }
                        }
                        else if (capability is RESTOperationCapability)
                        {
                            if (this._inOperationResult)
                            {
                                this._JSONWriter.WriteEndObject();      // Close previous response parameter.
                                this._JSONWriter.WriteEndObject();      // And close the 'responses' section.
                            }
                            this._inOperationResult = false;
                            this._currentOperation = capability as RESTOperationCapability;
                            this._panel.WriteInfo(this._panelIndex + 2, "Processing Operation '" + capability.Name + "'...");
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
                    // We use Interface Capability for clearing our context since that will be the first capability to be called
                    // in post-processing stage...
                    // Basically, all other capabilities are ignored here (except for increasing the progress bar)...
                    case ProcessingStage.PostProcess:
                        if (capability is RESTInterfaceCapability)
                        {
                            var itf = capability as RESTInterfaceCapability;
                            this._panel.WriteInfo(this._panelIndex, "Finalizing Interface '" + capability.Name + "'...");
                            if (this._inOperationResult)
                            {
                                this._JSONWriter.WriteEndObject();      // Close previous response parameter.
                                this._JSONWriter.WriteEndObject();      // And close the 'responses' section.
                                this._JSONWriter.WriteEndObject();      // And close the last 'path' section.
                            }
                            this._JSONWriter.WriteEndObject();          // End 'paths' section object.

                            // Retrieve all definitions and write the definitions section...
                            string definitionsObject = ((JSONSchema)(this._schema.MessageSchema)).GetDefinitionsObject();
                            definitionsObject = definitionsObject.Substring(1, definitionsObject.Length - 2);   // Remove the enclosing braces.
                            this._JSONWriter.WriteRaw("," + definitionsObject);

                            this._JSONWriter.WriteEndObject();          // End of OpenAPI definition object.
                            this._JSONWriter.Flush();
                            result = SaveProcessedCapability();
                            if (result == true)
                            {
                                this._panel.WriteInfo(this._panelIndex, "Interface generation has been completed successfully.");
                                this._panel.WriteInfo(this._panelIndex, "Output written to: '" + this._currentService.ServiceCIPath + "'.");
                            }

                            // Release resources...
                            this._isPathInitialized = false;
                            this._panel.Done();
                            this._JSONWriter.Close();
                            this._outputWriter.Close();
                            ClassCacheSlt.GetClassCacheSlt().Flush();   // Remove all collected resources on exit.
                        }
                        this._panel.IncreaseBar(1);
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
                            this._JSONWriter.Close();
                            this._outputWriter.Close();
                            ClassCacheSlt.GetClassCacheSlt().Flush();   // Remove all collected resources on exit.
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
                // The following assures that the generated output is validated and re-formatted again so that all is properly indented...
                using (var stringReader = new StringReader(this._outputWriter.ToString()))
                using (var stringWriter = new StringWriter())
                {
                    var jsonReader = new JsonTextReader(stringReader);
                    var jsonWriter = new JsonTextWriter(stringWriter) { Formatting = Formatting.Indented };
                    jsonWriter.WriteToken(jsonReader);
                    string api = stringWriter.ToString();
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.BuildHeader >> Created output:" + Environment.NewLine + api);
                    using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8)) writer.Write(api);
                }
            }
        }

        /// <summary>
        /// This method is invoked when a (new) resource is passed in the 'Processing' phase. First of all we check whether the new resource is a
        /// root-level resource (a 'sub-API'). If this is the case, we simply discard all current context and start from scratch.
        /// If we are not a root-level resource, we must check whether we start a new 'fork'. This is the case if the parent of the new resource
        /// is different from the current resource (e.g. the one that we processed last time). If not, we simply append the new resource to the
        /// current path and be done.
        /// In case of a fork, we must back-track in the class hierarchy until we find our parent. The means that we remove the 'tail-end' of the
        /// path until the parents match again. This also requires removing Identifier Resources from the Identifiers list if these happen to be
        /// nodes in the path that we don't use anymore.
        /// Lucky for us, the order in which resources are processed is predetermined. It is a recursive process in which we process a resource,
        /// then process al child resources of that resource. In other words: there will never be 'jumps' across the hierarchy, processing resembles
        /// a stack-model.
        /// Postcondition is a path prepared for either subsequence resources or operations on that part of the path.
        /// </summary>
        /// <param name="resource">The new resource that we are going to process.</param>
        private bool DefinePath(RESTResourceCapability resource)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.DefinePath >> Received new resource '" + resource.Name + "'...");
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.DefinePath >> Current path is: '" + this._currentPath + "'...");

            if (resource.IsRootLevel)
            {
                // If we have a root-level resource, this implies that we must start a new sub-API. If the current path has contents, we were processing
                // another sub-API and this must be terminated first (WriteEndObject). Next, we re-initialize the path using the current resource
                // name and start a new path object.
                // If the resource is an Identifier, we have to add it to the list of Identifiers.
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.DefinePath >> Rootlevel resource, simply reset current path");
                if (this._currentPath != string.Empty)
                {
                    // Close previous path object.
                    this._JSONWriter.WriteEndObject();
                    this._identifierList.Clear();
                }
                if (resource.Name != ContextSlt.GetContextSlt().GetConfigProperty(_EmptyResourceName))
                    this._currentPath = "/" + RESTUtil.GetAssignedRoleName(resource.Name);
                else this._currentPath = "/";
                if (resource.Archetype == RESTResourceCapability.ResourceArchetype.Identifier) this._identifierList.Add(resource);
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

                if (this._currentResource.HasOperations()) this._JSONWriter.WriteEndObject();   // Close previous path object.

                // If the current resource is a NOT a child of the 'previous current' resource, we have reached a 'fork' in the tree and must
                // 'back-track' to the correct parent node. If we encounter Identifiers along the way, these have to be removed as well...
                if (resource.Parent != this._currentResource.Implementation)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.DefinePath >> Fork detected!");
                    CapabilityImp currentNode = this._currentResource.Implementation;   // This is the current node in the path.
                    while (currentNode != resource.Parent)
                    {
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.DefinePath >> checking '" +
                                         currentNode.Name + "' against '" + resource.Parent.Name + "'...");
                        this._currentPath = this._currentPath.Substring(0, this._currentPath.LastIndexOf('/'));
                        if (this._identifierList.Count > 0 && this._identifierList[this._identifierList.Count - 1].Implementation == currentNode)
                            this._identifierList.RemoveAt(this._identifierList.Count - 1);
                        currentNode = currentNode.Parent;
                    }
                }
                if (resource.Name != ContextSlt.GetContextSlt().GetConfigProperty(_EmptyResourceName))
                    this._currentPath += "/" + RESTUtil.GetAssignedRoleName(resource.Name);
                if (resource.Archetype == RESTResourceCapability.ResourceArchetype.Identifier) this._identifierList.Add(resource);
            }

            // We must write the path ONLY if the new resource has operations. Otherwise, this is just an intermediate URL element...
            if (resource.HasOperations())
            {
                this._JSONWriter.WritePropertyName(this._currentPath);
                this._JSONWriter.WriteStartObject();
            }
            this._currentResource = resource;
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.OpenAPI20Processor.DefinePath >> New path: '" + this._currentPath + "'...");
            return true;
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
                    proc.Initialize(Schema.SchemaType.Definitions, name, namespaceToken, schemaNamespace, version);
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

        /// <summary>
        /// Helper function that simply counts all capabilties encountered.
        /// </summary>
        /// <param name="svc">Ignored.</param>
        /// <param name="cap">The current Capability, or NULL when invoked at Service level (very first call).</param>
        /// <returns>Always 'false', which indicates that traversal must continue until all nodes are processed.</returns>
        private bool ItemCollector(Service svc, Capability cap)
        {
            if (cap != null) this._capabilityCounter++;
            return false;
        }
    }
}
