using System;
using System.IO;
using System.Configuration;
using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Logging;
using Framework.Model;
using Framework.Context;
using Framework.Exceptions;
using Framework.Util;

namespace Plugin.Application.CapabilityModel
{
    /// Processing the capability tree takes three passes; first pre-processing, which can be used to initialise
    /// specific capabilities, allocate resources, etc. Next, the actual processing step is executed, followed by
    /// a post-processing step, which can be used to release resources, collect results, etc.
    /// The special Cancel stage is reserved for processing cleanup as a result of errors that have occured 
    /// during pre- or processing stage.
    internal enum ProcessingStage { PreProcess, Process, PostProcess, Cancel }

    /// <summary>
    /// Within the model, the 'Service' represents the deliverable API, as well as the top of the capability
    /// hierarchy. This can be a SOAP- or REST service declaration, a (set of) schema(s) or something different 
    /// that we consider a 'service' delivered by the plugin.
    /// The Service acts as the base class and holder of a set of capabilities and is responsible for naming
    /// the service as well as keeping the service version and namespace.
    /// A Service 'lives' in a specifically named package and is presented by a Class symbol with 
    /// stereotype 'Service'. The package must be part of a declaration package, which in turn must be in a 
    /// functional container.
    /// Within the service, capabilities can be hierarchically organized with capabilties responsible for 
    /// processing sub-capabilities.
    /// The service provides the main entry point for capability processing (handleCapabilities method).
    /// Since processing requirements might differ, Service must be specialized according to capability requirements.
    /// </summary>
    internal abstract class Service
    {
        // Public Configuration properties related to Service in general...
        internal const string _CommonPkgName                    = "CommonPkgName";
        internal const string _CommonPkgStereotype              = "CommonPkgStereotype";
        internal const string _ServiceClassStereotype           = "ServiceClassStereotype";
        internal const string _ServiceModelPkgName              = "ServiceModelPkgName";
        internal const string _ServiceModelPkgStereotype        = "ServiceModelPkgStereotype";
        internal const string _ServiceContainerPkgStereotype    = "ServiceContainerPkgStereotype";
        internal const string _ServiceSupportModelPathName      = "ServiceSupportModelPathName";
        internal const string _ServiceParentClassName           = "ServiceParentClassName";
        internal const string _DomainModelsRootPkgName          = "DomainModelsRootPkgName";
        internal const string _MaxOperationIDTag                = "MaxOperationIDTag";

        // Other configuration properties used by this service...
        private const string _BusinessFunctionIDTag             = "BusinessFunctionIDTag";
        private const string _ServiceOperationalStatusTag       = "ServiceOperationalStatusTag";
        private const string _DefaultOperationalStatus          = "DefaultOperationalStatus";
        private const string _PathNameTag                       = "PathNameTag";
        private const string _ComponentPathTemplate             = "ComponentPathTemplate";

        protected List<Capability> _serviceCapabilities;        // A list of all capabilities configured for this service.
        protected List<Capability> _selectedCapabilities;       // The subset of capabilities that have been selected by the user for processing.
        protected MEClass _serviceClass;                        // Services are always based on a class in the model.
        protected MEPackage _serviceDeclPackage;                // Service classes are part of a formal declaration package.
        protected MEPackage _containerPackage;                  // Service declarations are part of a functional container.
        protected MEPackage _modelPackage;                      // The package that holds the service model.

        protected Tuple<int,int> _version;                      // The current version of the service.
        protected string _rootPath;                             // Generic root path (configuration item).
        protected string _componentPath;                        // Relative path to location of capability components.
        protected string _fullyQualifiedPath;                   // The combination of root- and component path.

        // Components that can be used to construct a namespace:
        private string _operationalStatus;                      // Operational status as defined by a tag in the service, can be one
                                                                // of ''Standard', 'Experimental', 'Deprecated' or 'Proposed'.
        private string _businessFunctionID;                     // Current Business Function identifier associated with the service.

        private int _maxOperationID;                            // Current maximum operation ID.

        // Getters and setters for service properties...
        internal string Name                                  { get { return this._serviceClass.Name; } }
        internal MEClass ServiceClass                         { get { return this._serviceClass; } }
        internal List<Capability> Capabilities                { get { return this._serviceCapabilities; } }
        internal List<Capability> SelectedCapabilities        { get { return this._selectedCapabilities; } }
        internal MEPackage DeclarationPkg                     { get { return this._serviceDeclPackage; } }
        internal MEPackage ContainerPkg                       { get { return this._containerPackage; } }
        internal MEPackage ModelPkg                           { get { return this._modelPackage; } }
        internal int MajorVersion                             { get { return this._version.Item1; } }
        internal Tuple<int, int> Version                      { get { return this._version; } }
        internal string OperationalStatus                     { get { return this._operationalStatus; } }
        internal string FullyQualifiedPath                    { get { return this._fullyQualifiedPath; } }
        internal string ComponentPath                         { get { return this._componentPath; } }
        internal bool Valid                                   { get { return this._serviceClass != null; } }
        internal int BuildNumber
        {
            get { return this._serviceClass.BuildNumber; }
            set { this._serviceClass.BuildNumber = value; }
        }
        internal bool IsDefaultOperationalStatus
        {
            get
            {
                return string.Compare(this._operationalStatus, ContextSlt.GetContextSlt().GetConfigProperty(_DefaultOperationalStatus), true) == 0;
            }
        }

        /// <summary>
        /// Creation constructor, invoked when creating a NEW service hierarchy (Service and one or more Capabilities).
        /// Services are declared in a package that must have the appropriate declaration stereotype for the purpose
        /// of the service. The stereotype of this declaration is passed to the constructor so that the service can
        /// collect the proper context.
        /// </summary>
        /// <param name="containerPackage">The package that will hold the service declaration.</param>
        /// <param name="qualifiedServiceName">Full name of the service, including major version suffix.</param>
        /// <param name="declarationStereotype">Defines the type of service that we're constructing.</param>
        /// <exception cref="ConfigurationsErrorException">Error retrieving items from configuration.</exception>
        protected Service(MEPackage containerPackage, string qualifiedServiceName, string declarationStereotype)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.Service >> Creating service with name: '" + qualifiedServiceName + "' in package '" + containerPackage.Name + "'...");

            try
            {
                ContextSlt context = ContextSlt.GetContextSlt();
                ModelSlt model = ModelSlt.GetModelSlt();
                this._serviceCapabilities = new List<Capability>();
                this._selectedCapabilities = null;

                string serviceName = qualifiedServiceName.Substring(0, qualifiedServiceName.IndexOf("_V"));
                string version = qualifiedServiceName.Substring(qualifiedServiceName.IndexOf("_V") + 2);
                int majorVersion;
                if (!Int32.TryParse(version, out majorVersion)) majorVersion = 1;   // Defaults to 1 in case of errors.

                // Create the service declaration package as well as the service model package in the container...
                this._containerPackage = containerPackage;
                this._serviceDeclPackage = containerPackage.CreatePackage(qualifiedServiceName, declarationStereotype);
                this._modelPackage = this._serviceDeclPackage.CreatePackage(context.GetConfigProperty(_ServiceModelPkgName),
                                                                            context.GetConfigProperty(_ServiceModelPkgStereotype), 10);

                // Next, create the service class in the model...
                this._serviceClass = this._modelPackage.CreateClass(serviceName, context.GetConfigProperty(_ServiceClassStereotype));
                this._serviceClass.BuildNumber = 1;
                this._serviceClass.Version = new Tuple<int, int>(majorVersion, 0);
                this._version = this._serviceClass.Version;

                // Service classes must inherit from generic ServiceRoot...
                MEClass serviceParent = model.FindClass(context.GetConfigProperty(_ServiceSupportModelPathName),
                                                        context.GetConfigProperty(_ServiceParentClassName));
                if (serviceParent != null)
                {
                    var derived = new EndpointDescriptor(this._serviceClass);
                    var parent = new EndpointDescriptor(serviceParent);
                    model.CreateAssociation(derived, parent, MEAssociation.AssociationType.Generalization);
                }
                else
                {
                    string message = "Configuration error: can't find the Service Support Model at: '" + context.GetConfigProperty(_ServiceSupportModelPathName) + "'!";
                    Logger.WriteError("Plugin.Application.CapabilityModel.Service >> " + message);
                    throw new ConfigurationErrorsException(message);
                }

                // Initialise the OperationID tag...
                this._maxOperationID = 0;
                this._serviceClass.SetTag(context.GetConfigProperty(_MaxOperationIDTag), "0", true);
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.Service >> Service creation failed because:" +
                                  Environment.NewLine + exc.Message);
            }
        }

        /// <summary>
        /// Constructor to be invoked on an EXISTING service. Collects the proper service context.
        /// </summary>
        /// <param name="serviceClass">The selected service class.</param>
        /// <param name="declarationStereotype">The service declaration stereotype (defines the 'type' of service).</param>
        protected Service (MEClass serviceClass, string declarationStereotype)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.Service >> Creating context for existing service '" + serviceClass.Name + "'...");
            this._serviceClass = serviceClass;
            this._serviceCapabilities = new List<Capability>();
            this._selectedCapabilities = null;
            this._version = this._serviceClass.Version;

            CollectContext(declarationStereotype);  // Builds the remainder of the properties.
        }

        /// <summary>
        /// Add a new capability to this service. Note that the method does not accept two identical capabilities to
        /// register more then once (identical capabilities are capabilities that share the same capability class).
        /// Capabilities register themselves with 'their' service, so there is NO need to directly invoke this method!!
        /// </summary>
        /// <param name="cap">Capability to be added.</param>
        internal void AddCapability(Capability cap)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.addCapability >> Add capability: " + cap.Name);
            if (!this._serviceCapabilities.Contains(cap)) this._serviceCapabilities.Add(cap);
        }

        /// <summary>
        /// Facilitates creation of log entries in the Service annotation area. It retrieves the current log, adds an
        /// entry to it and writes the log back to the annotation area of the servic.
        /// </summary>
        /// <param name="text">Text to be added to the log.</param>
        internal void CreateLogEntry(string text)
        {
            MEChangeLog.MigrateLog(this.ServiceClass);
            string annotation = this._serviceClass.Annotation;
            ContextSlt context = ContextSlt.GetContextSlt();

            MEChangeLog newLog = (!string.IsNullOrEmpty(annotation)) ? new MEChangeLog(context.TransformRTF(annotation, RTFDirection.ToRTF)) : new MEChangeLog();
            Tuple<int, int> myVersion = this._serviceClass.Version;
            string logText = "[" + myVersion.Item1 + "." + myVersion.Item2 + "]: " + text;
            newLog.AddEntry(this._serviceClass.Author, logText);
            string log = newLog.GetLog();
            this._serviceClass.Annotation = context.TransformRTF(log, RTFDirection.FromRTF);
        }

        /// <summary>
        /// This function searches the list of registered capabilities for a capability based on the specified class and returns that capability.
        /// </summary>
        /// <param name="capabilityClass">The capability class to find.</param>
        /// <returns>Associated capability object or NULL if nothing found.</returns>
        internal Capability FindCapability(MEClass capabilityClass)
        {
            foreach (Capability cap in this._serviceCapabilities) if (cap.CapabilityClass == capabilityClass) return cap;
            return null;
        }

        /// <summary>
        /// This function constructs the fully-qualified namespace of a component that is associated with the service.
        /// The namespace is constructed from a configurable template that is identified by 'tag:NSTemplate'.
        /// The template can contain a number of placeholders that are replaced by the associated run-time replacements:
        /// @MAJORVSN@      = Service Major version number;
        /// @MINORVSN@      = Component minor version number as provided as argument. If specified as '-1', the minor version of the service is used instead.
        /// @BUSINESSFN@    = The business function identifier associated with the service (identified by the 'ID' tag in a parent package).
        /// @CONTAINER@     = The name of the functional container in which the service is defined.
        /// @SERVICE@       = The name of the service.
        /// @CAPABILITY@    = The current capability name as provided as argument. The tag is removed if the argument is not provided (null or empty).
        /// @OPERSTATUS@    = The operational status of the service.
        /// @YEAR@          = The current year.
        /// @MONTH@         = The current month.
        /// </summary>
        /// <param name="tag">Configuration tag used to select the appropriate template.</param>
        /// <param name="capabilityName">Optional capability name, can be set to NULL or empty string if not required.</param>
        /// <param name="minorVersion">Optional minor version to be used, set to -1 to use service minor version instead.</param>
        /// <returns>FQN for this service and component.</returns>
        internal string GetFQN(string tag, string capabilityName, int minorVersion)
        {
            const string BSFunctionTag = "@BUSINESSFN@";
            const string CapabilityNameTag = "@CAPABILITY@";
            const string MinorVersionTag = "@MINORVSN@";

            ContextSlt context = ContextSlt.GetContextSlt();
            string templateTag = tag + ":NSTemplate";
            string FQN = context.GetConfigProperty(templateTag);

            if (!string.IsNullOrEmpty(FQN))
            {
                if (this._businessFunctionID == string.Empty && FQN.Contains(BSFunctionTag))
                {
                    // No business function, remove the tag plus subsequent separator character...
                    FQN.Remove(FQN.IndexOf(BSFunctionTag), BSFunctionTag.Length + 1);
                }
                else FQN = FQN.Replace(BSFunctionTag, this._businessFunctionID);
                FQN = FQN.Replace("@CONTAINER@", this._containerPackage.Name);
                FQN = FQN.Replace("@SERVICE@", this._serviceClass.Name);
                if (string.IsNullOrEmpty(capabilityName) && FQN.Contains(CapabilityNameTag))
                {
                    // No capability, remove the tag plus subsequent separator character...
                    FQN.Remove(FQN.IndexOf(CapabilityNameTag), CapabilityNameTag.Length + 1);
                }
                else FQN = FQN.Replace(CapabilityNameTag, capabilityName);
                if (minorVersion < 0 && FQN.Contains(MinorVersionTag))
                {
                    // Component minor version not specified, use service minor version instead...
                    FQN = FQN.Replace(MinorVersionTag, this._version.Item2.ToString());
                }
                else FQN = FQN.Replace(MinorVersionTag, minorVersion.ToString());
                FQN = FQN.Replace("@MAJORVSN@", this._version.Item1.ToString());
                FQN = FQN.Replace("@OPERSTATUS@", this._operationalStatus);
                FQN = FQN.Replace("@YEAR@", DateTime.Now.Year.ToString());
                FQN = FQN.Replace("@MONTH@", DateTime.Now.Month.ToString());
            }
            Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.getFQN >> Cpnstructed: '" + FQN + "'.");
            return FQN;
        }

        /// <summary>
        /// This function returns the next free OperationID and increments the local counter. The counter is subsequently persisted
        /// in the Service class.
        /// </summary>
        /// <returns>Next OperationID to be used for this Service.</returns>
        internal int GetNewOperationID()
        {
            if (this._maxOperationID < 0) this._maxOperationID = 0;
            int useID = this._maxOperationID++;
            this._serviceClass.SetTag(ContextSlt.GetContextSlt().GetConfigProperty(_MaxOperationIDTag), this._maxOperationID.ToString(), true);
            return useID;
        }

        /// <summary>
        /// This method iterates through all capabilities present in the 'selected capabilities' tree and executes the 
        /// 'handleCapabilities' method for each of them. If one of the calls returns a 'false' indicator, processing 
        /// is aborted.
        /// The tree is processed three times, once for pre-processing, once for processing and once for post-processing.
        /// If pre-processing or processing stage returns an error, the entire tree is processed again with 'Cancel' stage
        /// in order to facilitate proper cleanup.
        /// Errors during post-processing are ignored, all capabilities must be processed, no matter whether errors occur.
        /// If an exception is thrown from one of the stages, all capabilities are called again with the 'cancel' stage.
        /// Since this might also happen during cancel processing, a capability must be prepared to process multiple
        /// cancel stages!
        /// This mechanism allows all Capabilities to properly cleanup. It also means that each capability must keep track of 
        /// which stage it already has processed!
        /// </summary>
        /// <returns>Result of all processing, false when a child returns an error or when there are no children.</returns>
        internal virtual bool HandleCapabilities(CapabilityProcessor processor)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.handleCapabilities >> Processing capabilities one at a time...");
            bool result = false;
            ProcessingStage stage = ProcessingStage.PreProcess;
            if (this._selectedCapabilities != null)
            {
                try
                {
                    if (result = HandleStage(processor, stage))         // Pre-process.
                    {
                        stage = ProcessingStage.Process;
                        if (result = HandleStage(processor, stage))     // Process.
                        {
                            stage = ProcessingStage.PostProcess;
                            return HandleStage(processor, stage);       // Post-process (not cancelled on errors).
                        }
                    }
                    // We only end up here on errors!
                    stage = ProcessingStage.Cancel;
                    HandleStage(processor, stage);                      // Cancel.
                    result = false;
                }
                catch (Exception exc)
                {
                    Logger.WriteError("Plugin.Application.CapabilityModel.Service.handleCapabilities >> Exception caught during processing in stage '" + stage +"': " + exc.Message);
                    HandleStage(processor, ProcessingStage.Cancel);
                    result = false;
                }
            }
            return result;
        }

        /// <summary>
        /// Initialize root-path and component path. A default path could be passed, which is subsequently used as default for
        /// the user to accept. On return, the internal root- and component pathnames should contain valid values.
        /// If the user navigates away from the root path, this is set to an empty string and the selected path becomes the
        /// component path. The configuration is adjusted accordingly. Note that typically this is not a good idea, since the
        /// component path is stored inside the Capability classes and might cause conflicts with other users who DO have a
        /// valid root path!
        /// </summary>
        /// <param name="defaultPath">Optional default pathname to propose to user.</param>
        /// <returns>True on successfull completion, false when user decided to cancel.</returns>
        internal bool InitializePathOLD(string defaultPath = null)
        {
            if (!string.IsNullOrEmpty(defaultPath)) this._componentPath = defaultPath;
            bool isOk = false;

            Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.initializePath >> Root path is: " + this._rootPath);
            Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.initializePath >> Component path is: " + this._componentPath);

            // Component path must be a relative pathname. It COULD be absolute if we have a legacy pathname from the old plugin.
            // In that case, we ignore it alltogether and let the user select a new path...
            if (!string.IsNullOrEmpty(this._componentPath) && this._componentPath.Contains(":")) this._componentPath = "\\";

            using (var pathDialog = new FolderBrowserDialog())
            {
                pathDialog.RootFolder = Environment.SpecialFolder.Desktop;
                pathDialog.SelectedPath = this._rootPath + this._componentPath;
                pathDialog.ShowNewFolderButton = true;
                pathDialog.Description = "Select (or create) location to write output files...";

                if (pathDialog.ShowDialog() == DialogResult.OK)
                {
                    isOk = true;
                    this._fullyQualifiedPath = pathDialog.SelectedPath;
					if (this._fullyQualifiedPath.StartsWith(this._rootPath, StringComparison.Ordinal)) {
						this._componentPath = this._fullyQualifiedPath.Substring(this._rootPath.Length);
						Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.initializePath >> Component path set to: " + this._componentPath);
					}
                    else
                    {
						Logger.WriteWarning("Plugin.Application.CapabilityModel.Service.initializePath >> Navigated away from configured root path, settings deleted!");
						this._rootPath = string.Empty;
						this._componentPath = this._fullyQualifiedPath;
						ContextSlt context = ContextSlt.GetContextSlt();
						context.SetStringSetting(FrameworkSettings._RootPath, string.Empty);
					}
                }
            }
            return isOk;
        }

        /// <summary>
        /// Initialize the Root- and Component pathnames. The Root path is taken from configuration and identifies the root of the
        /// Sandbox (the user-specific, local, storage of artefacts). The Component path is a relative name within the Sandbox and identifies
        /// the root of the service-specific artefact storage location within the Sandbox.
        /// Sandbox layout follows the structure of the ECDM repository and is constructed as:
        ///  [Business-function-ID].[Container-name]    (obtained from Service configuration).
        ///     [Service-name]_[Major-version]          (obtained from Service configuration).
        ///        P[Minor-version].B[Build-number]     (obtained from Service configuration).
        ///           Artefacts                         (configuration constant).
        ///              [Artefact-files]               (build-specific).
        /// If we can't find a root-path, we ask the user to select one and we use that to update the root-path configuration accordingly.
        /// Otherwise, we don't bother the user with file locations since all is constructed automatically. This also assures consistent
        /// structure and contents of the Sandbox.
        /// </summary>
        /// <returns>True on successfull completion, false when user decided to cancel.</returns>
        internal bool InitializePath()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            bool isOk = true;
            Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.initializePath >> Root path is: " + this._rootPath);
            Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.initializePath >> Component path is: " + this._componentPath);

            try
            {
                // If we don't have a configured root path, we ask one from the user.
                if (string.IsNullOrEmpty(this._rootPath))
                {
                    using (var pathDialog = new FolderBrowserDialog())
                    {
                        pathDialog.RootFolder = Environment.SpecialFolder.Desktop;
                        pathDialog.ShowNewFolderButton = true;
                        pathDialog.Description = "Select (or create) the root folder of your sandbox...";
                        if (pathDialog.ShowDialog() == DialogResult.OK)
                        {
                            this._rootPath = pathDialog.SelectedPath;
                            context.SetStringSetting(FrameworkSettings._RootPath, this._rootPath);
                        }
                        else return false;
                    }
                }

                // Retrieve the template for our output structure from configuration and replace the template placeholders with the
                // actual values. Next, we construct the entire directory hierarchy dictated by the template...
                this._componentPath = string.Empty;
                this._fullyQualifiedPath = this._rootPath;
                string[] pathElements = context.GetConfigProperty(_ComponentPathTemplate).Split('/');
                foreach (string template in pathElements)
                {
                    string pathElement = string.Copy(template);
                    pathElement = pathElement.Replace("@MAJORVSN@", this.Version.Item1.ToString());
                    pathElement = pathElement.Replace("@MINORVSN@", this.Version.Item2.ToString());
                    pathElement = pathElement.Replace("@BUILD@", this.BuildNumber.ToString());
                    pathElement = pathElement.Replace("@BUSINESSFN@", this._businessFunctionID);
                    pathElement = pathElement.Replace("@CONTAINER@", this._containerPackage.Name);
                    pathElement = pathElement.Replace("@SERVICE@", this.Name);
                    pathElement = pathElement.Replace("@OPERSTATUS@", this._operationalStatus);
                    this._componentPath += "/" + pathElement;
                    this._fullyQualifiedPath = this._rootPath + this._componentPath;
                    if (!Directory.Exists(this._fullyQualifiedPath)) Directory.CreateDirectory(this._fullyQualifiedPath);
                }
                Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.initializePath >> Root path on return: " + this._rootPath);
                Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.initializePath >> Component path on return: " + this._componentPath);
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.Service.initializePath >> Exception when creating path names because: " + exc.Message);
                isOk = false;
            }
            return isOk;
        }

        /// <summary>
        /// This method is used to load a series of capabilities in the 'selected capabilities' list of the service.
        /// This list is subsequently used for the 'handleCapability' function. 
        /// Note that the method does NOT verify whether the selected capabilties originated from the original 
        /// capability tree of this service.
        /// Any original set of capabilities in the 'selected capabilties' list is destructed.
        /// </summary>
        /// <param name="selectedChildren">List of Capabilities to load.</param>
        internal void LoadSelectedCapabilities(List<Capability> selectedChildren)
        {
            this._selectedCapabilities = new List<Capability>(selectedChildren);
        }

        /// <summary>
        /// This function is called during construction of Operation Capabilities. It is used ONLY when an Operation has not yet received
        /// a valid OperationID BUT it found an existing namespace token (nsXXX). The convention in this case is to keep the existing 
        /// token but tell the service that we're doing this so the service knows that this ID is already in use.
        /// The method updates the maxOperationID of the service.
        /// </summary>
        /// <param name="usedID">ID provided by Operation.</param>
        internal void SyncOperationID(int usedID)
        {
            string maxOperationIDTag = ContextSlt.GetContextSlt().GetConfigProperty(_MaxOperationIDTag);
            if (this._maxOperationID <= usedID)
            {
                this._maxOperationID = usedID++;
                this._serviceClass.SetTag(maxOperationIDTag, this._maxOperationID.ToString(), true);
            }
        }

        /// <summary>
        /// Recursively traverses the entire capability hierarchy. For each node in this hierarchy, the provided function delegate
        /// is invoked, which receives both the service as well as the current capability as a parameter.
        /// As long as the delegate returns 'false', the traversal continues (until all nodes have been processed). It is therefor
        /// possible to abort traversal by letting the delegate return a 'true' value (as in 'done').
        /// The very first node is the 'service' itself, in which case the delegate is invoked with a 'null' for the capability!
        /// </summary>
        /// <param name="visitor">Action that must be performed on each node.</param>
        internal void Traverse(Func<Service, Capability, bool> visitor)
        {
            if (visitor(this, null)) return;
            foreach (Capability cap in this._serviceCapabilities)
            {
                if (cap.Traverse(visitor)) return;
            }
        }

        /// <summary>
        /// Must be invoked in order to change the version of the service and optionally all registered capabilities. Children will be
        /// synchronized in case the new major version is different from the current major version.
        /// The 'Service' object keeps a separate copy of the version property since updating this in the underlying repository takes time
        /// and might lead to mismatches when synchronising a complete capability tree. By keeping the version separate, we guarantee that
        /// child objects always get the correct version when using the 'Service' interface.
        /// </summary>
        /// <param name="newVersion">New service version.</param>
        internal void UpdateVersion(Tuple<int,int> newVersion)
        {
            Tuple<int, int> currentVersion = this._serviceClass.Version;
            this._serviceClass.Version = newVersion;
            this._version = newVersion;
            if (currentVersion.Item1 != newVersion.Item1)
            {
                CreateLogEntry("Major version changed to: '" + MajorVersion + ".0'.");
                foreach (Capability cap in this._serviceCapabilities) cap.VersionSync();
            }
        }

        /// <summary>
        /// Perform service capability processing for a specified stage.
        /// </summary>
        /// <param name="stage">The processing stage that must be executed. Typical processing uses pre-processing,
        /// followed by processing, followed by post-processing. If any capability issued an error during pre- or
        /// processing stages, all capabilities are processed again using a cancel stage.</param>
        /// <returns>True when processing can commence, false on errors (forces abort).</returns>
        protected abstract bool HandleCapability(CapabilityProcessor processor, ProcessingStage stage);

        /// <summary>
        /// Collect owning package, declaration package and container package and build the namespace for this 
        /// service by examining the package hierarchy, looking for a parent package that has an ID tag specified 
        /// and combining that tag with the namespace prefix, container name and service name.
        /// </summary>
        /// <exception cref="IllegalContextException">Context not correct for service creation!</exception>
        private void CollectContext(string declStereotype)
        {
            string message;

            // First of all, let's check our context...
            ContextSlt context = ContextSlt.GetContextSlt();
            if ((this._serviceClass.OwningPackage.Name != context.GetConfigProperty(_ServiceModelPkgName)) ||
                (!this._serviceClass.OwningPackage.HasStereotype(context.GetConfigProperty(_ServiceModelPkgStereotype))))
            {
                message = "Service '" + this._serviceClass.Name + "' created in wrong context!\n" +
                          "Owning package '" + this._serviceClass.OwningPackage.Name + "' is of wrong name and/or stereotype!";
                Logger.WriteError("Plugin.Application.CapabilityModel.Service.collectContext >> " + message);
                throw new IllegalContextException(message);
            }

            // Set the root path, could be an empty string if not defined. Component- and FullyQualified path name initialization is deferred
            // till the moment we actually need them.
            this._rootPath = context.GetStringSetting(FrameworkSettings._RootPath);
            if (string.IsNullOrEmpty(this._rootPath)) this._rootPath = string.Empty;
            this._componentPath = string.Empty;
            this._fullyQualifiedPath = string.Empty;

            // Retrieve the operational status...
            this._operationalStatus = this._serviceClass.GetTag(context.GetConfigProperty(_ServiceOperationalStatusTag));

            // Set model package (package in which the service lives) and the declaration package (the parent of my owning package)...
            this._modelPackage = this._serviceClass.OwningPackage;
            this._serviceDeclPackage = this._serviceClass.OwningPackage.Parent;
            if (this._serviceDeclPackage.HasStereotype(declStereotype))
            {
                // The name of the declaration package MUST contain the major version as a suffix ("name_Vn").
                // The major version of the service class MUST match this major version ID. We examine the version of the
                // class and if there is no match, we update the version of the class...
                // Since this is called from the constructor, there are no children to synchronise so this might required some
                // attention. Therefor, a warning message is created.
                string fullName = this._serviceDeclPackage.Name;
                string version = fullName.Substring(fullName.IndexOf("_V") + 2);
                Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.collectContext >> Got major version: " + version + " out of name " + fullName);
                int majorVersion;
                if (Int32.TryParse(version, out majorVersion))
                {
                    Tuple<int, int> classVersion = this._serviceClass.Version;
                    if (classVersion.Item1 != majorVersion)
                    {
                        Logger.WriteWarning("Plugin.Application.CapabilityModel.Service.collectContext >> Class major version '" + classVersion.Item1 +
                                            "' differs from package version '" + majorVersion + "'. Children might be out of sync!");
                        var correctVersion = new Tuple<int, int>(majorVersion, classVersion.Item2);
                        this._serviceClass.Version = correctVersion;
                        this._version = correctVersion;
                    }
                }

                // The parent of our declaration must be the container...
                Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.collectContext >> Found our declaration: " + this._serviceDeclPackage.Name);
                this._containerPackage = this._serviceDeclPackage.Parent;
                if (this._containerPackage.HasStereotype(context.GetConfigProperty(_ServiceContainerPkgStereotype)))
                {
                    // Search up the hierarchy until we get the proper ID or reach the root of the package tree...
                    MEPackage parentPkg = this._containerPackage.Parent;
                    string rootName = context.GetConfigProperty(_DomainModelsRootPkgName);
                    string IDTag = context.GetConfigProperty(_BusinessFunctionIDTag);
                    this._businessFunctionID = string.Empty;
                    while (parentPkg.Name != rootName)
                    {
                        string ID = parentPkg.GetTag(IDTag);
                        if (ID != string.Empty)
                        {
                            Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.collectContext >> Collected business function: '" + ID + "'.");
                            this._businessFunctionID = ID;
                            break;
                        }
                        parentPkg = parentPkg.Parent;
                    }
                }

                // Try to obtain the next-valid Operation ID...
                string maxID = this._serviceClass.GetTag(context.GetConfigProperty(_MaxOperationIDTag));
                if (string.IsNullOrEmpty(maxID) || !int.TryParse(maxID, out this._maxOperationID))
                {
                    this._maxOperationID = 0;
                    this._serviceClass.SetTag(context.GetConfigProperty(_MaxOperationIDTag), "0", true);
                }
            }
            else
            {
                message = "Service '" + this._serviceClass.Name + "' created in wrong context!\n" +
                          "Package '" + this._serviceClass.OwningPackage.Name + "' seems to be in wrong [part of] package tree!";
                Logger.WriteError("Plugin.Application.CapabilityModel.Service.collectContext >> " + message);
                throw new IllegalContextException(message);
            }
        }

        /// <summary>
        /// Processes a single stage. First, the Service is processed, followed by all Capabilities.
        /// If a single processing fails, the sequence is aborted and the function returns 'false' (not for post-processing).
        /// When invoked with 'Cancel' stage, the Service and all Capabilities are called, irrespective
        /// of processing result!
        /// </summary>
        /// <param name="processor">The processor to use for processing.</param>
        /// <param name="stage">The stage we're in.</param>
        /// <returns>True when processing completed successfully, false on errors.</returns>
        private bool HandleStage(CapabilityProcessor processor, ProcessingStage stage)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.handleStage >> Processing stage: " + stage);
            bool result;
            if (stage != ProcessingStage.Cancel)
            {
                result = this.HandleCapability(processor, stage);
                if (result || (stage == ProcessingStage.PostProcess))   // Pre- and Processing stages are aborted on error, post-processing is not.
                {
                    foreach (Capability capability in this._selectedCapabilities)
                    {
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.handleStage >> Processing: " + capability.Name);
                        result = capability.HandleCapabilities(processor, stage);
                        if (!result && (stage == ProcessingStage.PreProcess || stage == ProcessingStage.Process)) break; 
                    }
                }
            }
            else
            {
                // Cancel is treated differently!
                foreach (Capability capability in this._selectedCapabilities)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.handleStage >> Cancelling: " + capability.Name);
                    capability.HandleCapabilities(processor, ProcessingStage.Cancel);
                }
                HandleCapability(processor, ProcessingStage.Cancel);    // We cancel the service as last one.
                result = true;
            }
            Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.handleStage >> Processing result: " + result);
            return result;
        }
    }
}
