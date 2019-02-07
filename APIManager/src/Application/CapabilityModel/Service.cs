using System;
using System.IO;
using System.Configuration;
using System.Collections.Generic;
using Framework.Logging;
using Framework.Model;
using Framework.Context;
using Framework.Exceptions;
using Framework.Util;
using Framework.View;
using Framework.ConfigurationManagement;

namespace Plugin.Application.CapabilityModel
{
    /// Processing the capability tree takes three passes; first pre-processing, which can be used to initialise
    /// specific capabilities, allocate resources, etc. Next, the actual processing step is executed, followed by
    /// a post-processing step, which can be used to release resources, collect results, etc.
    /// The special Cancel stage is reserved for processing cleanup as a result of errors that have occured 
    /// during pre- or processing stage.
    internal enum ProcessingStage { PreProcess, Process, PostProcess, Cancel }

    // A service can be created in any of three states (Deprecated can only be assigned after creation and means
    // that the service should no longer be used):
    // Standard - Default value for normal, production-grade services;
    // Experimental - Used to create services for POC's or other, temporary, deployments;
    // Draft - Used to create draft services, in need for further review. Typically used to replace Standard
    // services eventually.
    internal enum OperationalState { Standard, Experimental, Deprecated, Draft }

    /// <summary>
    /// Within the model, the 'Service' represents the deliverable API, as well as the top of the capability
    /// hierarchy. This can be a SOAP- or REST service declaration, a (set of) schema(s) or something different 
    /// that we consider a 'service' delivered by the plugin.
    /// The Service acts as the base class and holder of a set of capabilities and is responsible for naming
    /// the service as well as keeping the service version and namespace.
    /// A Service 'lives' in a specifically named package and is presented by a Class symbol with 
    /// stereotype 'Service'. The package must be part of a declaration package, which in turn must be in a 
    /// functional container.
    /// Within the service, capabilities can be hierarchically organized with capabilities responsible for 
    /// processing sub-capabilities.
    /// The service provides the main entry point for capability processing (handleCapabilities method).
    /// Since processing requirements might differ, Service must be specialized according to capability requirements.
    /// </summary>
    internal abstract class Service
    {
        // These are the different 'flavors' of service archetypes that are currently known...
        // Since these are transformations of the corresponding tagname, enumeration and tag MUST be in sync at all times!
        internal enum ServiceArchetype {Unknown, SOAP, REST, Message, CodeList }

        // Represents the default operational state...
        internal const OperationalState _DefaultOperationalState = OperationalState.Standard;

        // Public Configuration properties related to Service in general...
        internal const string _CommonPkgName                    = "CommonPkgName";
        internal const string _CommonPkgStereotype              = "CommonPkgStereotype";
        internal const string _ServiceClassStereotype           = "ServiceClassStereotype";
        internal const string _ServiceModelPkgName              = "ServiceModelPkgName";
        internal const string _ServiceModelPkgStereotype        = "ServiceModelPkgStereotype";
        internal const string _ServiceContainerPkgStereotype    = "ServiceContainerPkgStereotype";
        internal const string _ServiceDeclPkgStereotype         = "ServiceDeclPkgStereotype";
        internal const string _ServiceSupportModelPathName      = "ServiceSupportModelPathName";
        internal const string _ServiceParentClassName           = "ServiceParentClassName";
        internal const string _DomainModelsRootPkgName          = "DomainModelsRootPkgName";
        internal const string _MaxOperationIDTag                = "MaxOperationIDTag";
        internal const string _ServiceArchetypeTag              = "ServiceArchetypeTag";

        // Initial build number for new services or version reset.
        private const int _DefaultBuildNr                       = 1;

        // Private configuration properties used by this service...
        private const string _BusinessFunctionIDTag             = "BusinessFunctionIDTag";
        private const string _ServiceOperationalStateTag        = "ServiceOperationalStatusTag";
        private const string _PathNameTag                       = "PathNameTag";
        private const string _ServiceModelPos                   = "ServiceModelPos";
        private const string _CommitIDLeader                    = "CommitIDLeader";
        private const string _CMArtefactsFolderName             = "CMArtefactsFolderName";
        private const string _CMDocumentsFolderName             = "CMDocumentsFolderName";
        private const string _NamespacePrefix                   = "NamespacePrefix";

        protected List<Capability> _serviceCapabilities;        // A list of all capabilities configured for this service.
        protected List<Capability> _selectedCapabilities;       // The subset of capabilities that have been selected by the user for processing.
        protected MEClass _serviceClass;                        // Services are always based on a class in the model.
        protected MEPackage _serviceDeclPackage;                // Service classes are part of a formal declaration package.
        protected MEPackage _containerPackage;                  // Service declarations are part of a functional container.
        protected MEPackage _modelPackage;                      // The package that holds the service model.
        protected ServiceArchetype _archetype;                  // Defines the archetype implemented by the current (specialized) Service class.

        // Versioning and configuration management...
        private Tuple<int,int> _version;                        // The current version of the service.
        private string _repositoryPath;                         // Absolute path to the root of our local repository.
        private string _serviceBuildPath;                       // Relative path to the location of service artifacts within the local repository
        private CMContext _CMContext;                           // Configuration management context for this service.
        private Diagram.ClassColor _representationColor;        // The color in which the Service Class must be drawn on diagrams.

        // Components that can be used to construct a namespace:
        private OperationalState _operationalState;             // Operational state as defined by a tag in the service, can be one
                                                                // of 'Standard', 'Experimental', 'Deprecated' or 'Proposed'.
        private string _businessFunctionID;                     // Current Business Function identifier associated with the service.
        private string _serviceURI;                             // Unique service identifier in URI format.

        private int _maxOperationID;                            // Current maximum operation ID, used to provide operations with a unique ID.

        /// <summary>
        /// Get or set the service build number.
        /// </summary>
        internal int BuildNumber
        {
            get { return this._serviceClass.BuildNumber; }
            set { this._serviceClass.BuildNumber = value; }
        }

        /// <summary>
        /// Returns the numeric identifier representing the Business Function associated with the service.
        /// </summary>
        internal string BusinessFunctionID { get { return this._businessFunctionID; } }

        /// <summary>
        /// Returns the list of Capability objects associated with this service.
        /// </summary>
        internal List<Capability> Capabilities { get { return this._serviceCapabilities; } }

        /// <summary>
        /// Get or set the configuration management state of the service.
        /// </summary>
        internal CMContext.CMState ConfigurationMgmtState
        {
            get { return this._CMContext.State; }
            set
            {
                if (value != this._CMContext.State)
                {
                    this._CMContext.State = value;
                    SetColor(value);
                }
            }
        }

        /// <summary>
        /// Returns the model package-object containing the service declaration (Service Container).
        /// </summary>
        internal MEPackage ContainerPkg { get { return this._containerPackage; } }

        /// <summary>
        /// Returns the model package-object containing all service components (Service Declaration).
        /// </summary>
        internal MEPackage DeclarationPkg { get { return this._serviceDeclPackage; } }

        /// <summary>
        /// Check whether our current operation state matches the default state ('Standard').
        /// </summary>
        internal bool IsDefaultOperationalState { get { return this._operationalState == _DefaultOperationalState; } }

        /// <summary>
        /// Returns the major version number for this Service.
        /// </summary>
        internal int MajorVersion { get { return this._version.Item1; } }

        /// <summary>
        /// Returns the model package-object containing the Service Capability model.
        /// </summary>
        internal MEPackage ModelPkg { get { return this._modelPackage; } }

        /// <summary>
        /// Returns the name of the service.
        /// </summary>
        internal string Name { get { return this._serviceClass.Name; } }

        /// <summary>
        /// Returns an empty string in case the Service OperationalState is 'Standard'. In all other cases, it
        /// returns the OperationState as a Pascal-Case string.
        /// </summary>
        internal string NonDefaultOperationalState
        {
            get
            {
                return IsDefaultOperationalState ? string.Empty : EnumConversions<OperationalState>.EnumToString(this._operationalState);
            }
        }

        /// <summary>
        /// Returns the Service operational state (one of 'Standard', 'Experimental', 'Deprecated' or 'Proposed').
        /// </summary>
        internal OperationalState OperationalState { get { return this._operationalState; } }

        /// <summary>
        /// Returns the list of all release tag names that have been issued for this service. The list is ordered in descending order (i.e. the
        /// latest release comes first).
        /// </summary>
        internal List<string> ReleaseTags { get { return this._CMContext.GetReleaseTagNames(); } }

        /// <summary>
        /// Returns the current Representation Color of the class, i.e. the color in which the class must be drawn on diagrams.
        /// </summary>
        internal Diagram.ClassColor RepresentationColor { get { return this._representationColor; } }

        /// <summary>
        /// Returns the list of (top-level) Capability objects that the user has most recently selected for processing.
        /// </summary>
        internal List<Capability> SelectedCapabilities { get { return this._selectedCapabilities; } }

        /// <summary>
        /// Returns the relative pathname (relative to the repository) to the location of all Configuration Items for this service. 
        /// If Configuration Management is enabled, this location is: 
        ///      (business-functionID)/(container-name)/(service-name)_[OperationalState_]V(major-version)/(artefact-folder).
        /// If Configuration Management is not active, the location is: 
        ///      (business-functionID)/(container-name)/(service-name)_[OperationalState_]V(major-version)/P(minor-version)B(build-number).
        /// </summary>
        internal string ServiceBuildPath { get { return this._serviceBuildPath; } }

        /// <summary>
        /// This is the absolute path that references the location of Service Configuration Items. 
        /// It is a combination of the RepositoryPath and the ServiceBuildPath.
        /// </summary>
        internal string ServiceCIPath { get { return this._repositoryPath + "/" + this._serviceBuildPath; } }

        /// <summary>
        /// Returns the model class-object representing this service.
        /// </summary>
        internal MEClass ServiceClass { get { return this._serviceClass; } }

        /// <summary>
        /// Returns the unique URI identifier of the service.
        /// </summary>
        internal string ServiceURI { get { return this._serviceURI; } }

        /// <summary>
        /// Returns the globally unique identifier (GUID) associated with this service. The GUID is obtained from the associated service class.
        /// </summary>
        internal string ServiceGUID { get { return this._serviceClass.GlobalID; } }

        /// <summary>
        /// Get- or set the current CM ticket associated with the servic.
        /// </summary>
        internal RMServiceTicket Ticket
        {
            get { return this._CMContext.Ticket; }
            set { this._CMContext.Ticket = value; }
        }

        /// <summary>
        /// Helper function that takes a Service Class and checks whether the Configuration Management state of the class allows updates
        /// on the service metamodel. When CM is disabled, this is always allowed. Otherwise, the Service CM state must either be Created,
        /// Checked-Out or Modified.
        /// Actual processing is delegated to CMContext, since this is where we want to keep all our CM rules and checks.
        /// </summary>
        /// <param name="serviceClass">Service to be checked.</param>
        /// <returns>True in case the meta model is allowed to change, false otherwise.</returns>
        internal static bool UpdateAllowed(MEClass serviceClass)
        {
            return CMContext.UpdateAllowed(serviceClass);
        }

        /// <summary>
        /// Returns true of Configuration Management is currently enabled for this service, false otherwise.
        /// </summary>
        internal bool UseConfigurationMgmt { get { return this._CMContext.CMEnabledService; } }

        /// <summary>
        /// Returns true if the service hierarchy has been successfully created and/or initialized, false otherwise.
        /// </summary>
        internal bool Valid { get { return this._serviceClass != null; } }

        /// <summary>
        /// Returns major- and minor version number for this Service.
        /// </summary>
        internal Tuple<int, int> Version { get { return this._version; } }

        /// <summary>
        /// Creation constructor, invoked when creating a NEW service hierarchy (Service and one or more Capabilities).
        /// Services are declared in a package that must have the appropriate declaration stereotype for the purpose
        /// of the service. The stereotype of this declaration is passed to the constructor so that the service can
        /// collect the proper context.
        /// TODO: Must accept a valid TicketID for service construction!
        /// </summary>
        /// <param name="containerPackage">The package that will hold the service declaration.</param>
        /// <param name="qualifiedServiceName">Full name of the service, including major version suffix.</param>
        /// <param name="declarationStereotype">Defines the type of service that we're constructing.</param>
        /// <param name="initialState">Initial operational state of the service.</param>
        /// <param name="remoteTicket">the ticket we use for service creation, ignored when CM is not active.</param>
        /// <param name="projectOrderID">Identifier of the project order we use for service creation, ignored when CM is not active.</param>
        /// <exception cref="ConfigurationsErrorException">Error retrieving items from configuration or configuration settings invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown when an illegal (or no) ticket is passed and CM is enabled.</exception>
        protected Service(MEPackage containerPackage, string qualifiedServiceName, string declarationStereotype, OperationalState initialState, Ticket remoteTicket, string projectOrderID)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.Service >> Creating service with name: '" + qualifiedServiceName + "' in package '" + containerPackage.Name + "'...");

            try
            {
                ContextSlt context = ContextSlt.GetContextSlt();
                ModelSlt model = ModelSlt.GetModelSlt();
                this._serviceCapabilities = new List<Capability>();
                this._selectedCapabilities = null;
                this._representationColor = Diagram.ClassColor.Default;
                this._operationalState = initialState;

                string serviceName = qualifiedServiceName.Substring(0, qualifiedServiceName.IndexOf("_V"));
                string version = qualifiedServiceName.Substring(qualifiedServiceName.IndexOf("_V") + 2);
                int majorVersion;
                if (!Int32.TryParse(version, out majorVersion)) majorVersion = 1;   // Defaults to 1 in case of errors.

                // Create the service declaration package as well as the service model package in the container...
                // We try to read the relative package position from configuration and if this failed, use '20' as default value.
                int packagePos;
                if (!int.TryParse(context.GetConfigProperty(_ServiceModelPos), out packagePos)) packagePos = 20;
                this._containerPackage = containerPackage;
                this._serviceDeclPackage = containerPackage.CreatePackage(qualifiedServiceName, declarationStereotype);
                this._modelPackage = this._serviceDeclPackage.CreatePackage(context.GetConfigProperty(_ServiceModelPkgName),
                                                                            context.GetConfigProperty(_ServiceModelPkgStereotype), packagePos);

                // Next, create the service class in the model...
                this._serviceClass = this._modelPackage.CreateClass(serviceName, context.GetConfigProperty(_ServiceClassStereotype));
                this._serviceClass.BuildNumber = _DefaultBuildNr;
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

                // Set the initial OperationalState tag...
                this._serviceClass.SetTag(context.GetConfigProperty(_ServiceOperationalStateTag), 
                                          EnumConversions<OperationalState>.EnumToString(initialState));

                // For the next set of initializations, order is important!
                // Business Function ID is required for CM State and Path Names so this must go first.
                // Configuration Management might want to clone a remote repository, which would create part of the pathnames, so this must go second
                // (otherwise, repository creation will fail with a 'existing non-empty directory' error).
                // Initialize Path will create the (remainder of) the necessary path structure and thus will go last.
                // BuildURN is not really critical, but must go after LoadBusinessFunctionID since it needs that ID.
                LoadBusinessFunctionID();
                LoadCMState(remoteTicket, projectOrderID);
                InitializePath();
                BuildURI();
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.Service >> Service creation failed because:" +
                                  Environment.NewLine + exc.ToString());
                throw;
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
            this._repositoryPath = string.Empty;
            this._serviceBuildPath = string.Empty;
            this._representationColor = Diagram.ClassColor.Default;
            this._archetype = ServiceArchetype.Unknown;

            // First of all, let's check our context...
            ContextSlt context = ContextSlt.GetContextSlt();
            if ((this._serviceClass.OwningPackage.Name != context.GetConfigProperty(_ServiceModelPkgName)) ||
                (!this._serviceClass.OwningPackage.HasStereotype(context.GetConfigProperty(_ServiceModelPkgStereotype))))
            {
                string message = "Service '" + this._serviceClass.Name + "' created in wrong context!" + Environment.NewLine +
                                 "Owning package '" + this._serviceClass.OwningPackage.Name + "' is of wrong name and/or stereotype!";
                Logger.WriteError("Plugin.Application.CapabilityModel.Service >> " + message);
                throw new IllegalContextException(message);
            }

            // Retrieve the operational state...
            this._operationalState = EnumConversions<OperationalState>.StringToEnum(this._serviceClass.GetTag(context.GetConfigProperty(_ServiceOperationalStateTag)));

            // Set model package (package in which the service lives) and the declaration package (the parent of my owning package)...
            this._modelPackage = this._serviceClass.OwningPackage;
            this._serviceDeclPackage = this._serviceClass.OwningPackage.Parent;
            if (this._serviceDeclPackage.HasStereotype(declarationStereotype))
            {
                // The name of the declaration package MUST contain the major version as a suffix ("name_Vn").
                // The major version of the service class MUST match this major version ID. We examine the version of the
                // class and if there is no match, we update the version of the class...
                // Since this is called from the constructor, there are no children to synchronise so this might required some
                // attention. Therefor, a warning message is created.
                string fullName = this._serviceDeclPackage.Name;
                string version = fullName.Substring(fullName.IndexOf("_V") + 2);
                Logger.WriteInfo("Plugin.Application.CapabilityModel.Service >> Got major version: " + version + " out of name " + fullName);
                int majorVersion;
                if (Int32.TryParse(version, out majorVersion))
                {
                    Tuple<int, int> classVersion = this._serviceClass.Version;
                    if (classVersion.Item1 != majorVersion)
                    {
                        Logger.WriteWarning("Class major version '" + classVersion.Item1 +
                                            "' differs from package version '" + majorVersion + "'. Children might be out of sync!");
                        var correctVersion = new Tuple<int, int>(majorVersion, classVersion.Item2);
                        this._serviceClass.Version = correctVersion;
                        this._version = correctVersion;
                    }
                }

                // The parent of our declaration must be the container...
                Logger.WriteInfo("Plugin.Application.CapabilityModel.Service >> Found our declaration: " + this._serviceDeclPackage.Name);
                this._containerPackage = this._serviceDeclPackage.Parent;

                // Try to obtain the next-valid Operation ID...
                string maxID = this._serviceClass.GetTag(context.GetConfigProperty(_MaxOperationIDTag));
                if (string.IsNullOrEmpty(maxID) || !int.TryParse(maxID, out this._maxOperationID))
                {
                    this._maxOperationID = 0;
                    this._serviceClass.SetTag(context.GetConfigProperty(_MaxOperationIDTag), "0", true);
                }

                // For the next set of initializations, order is important!
                // Business Function ID is required for CM State and Path Names so this must go first.
                // Configuration Management might want to clone a remote repository, which would create part of the pathnames, so this must go second
                // (otherwise, repository creation will fail with a 'existing non-empty directory' error).
                // Initialize Path will create the (remainder of) the necessary path structure and thus will go last.
                // BuildURN is not really critical, but must go after LoadBusinessFunctionID since it needs that ID.
                LoadBusinessFunctionID();
                LoadCMState(null, null);
                InitializePath();
                BuildURI();
            }
            else
            {
                string message = "Service '" + this._serviceClass.Name + "' created in wrong context!\n" +
                                 "Package '" + this._serviceClass.OwningPackage.Name + "' seems to be in wrong [part of] package tree!";
                Logger.WriteError("Plugin.Application.CapabilityModel.Service >> " + message);
                throw new IllegalContextException(message);
            }
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
        /// When CM is enabled for the service, this method assures that the CM context for the service is prepared for use. This includes 
        /// synchronisation with remote repository and creation of the appropriate working branch. The service CM state is updated to reflect 
        /// the new state. If CM is enabled in general, but not (yet) for the service, a checkout command is used to enforce the service
        /// to start using CM. In this case, we treat the service as a 'created' service in CM context.
        /// A Service can only be checked-out if we can present a valid ticket, which is used to track the issue that caused the creation-
        /// or modification of this service.
        /// When the service is already in the proper state, the function performs no actions.
        /// </summary>
        /// <param name="changeTicket">The change ticket associated with this checkout.</param>
        /// <returns>True when successfully checked-out (or CM not active for the service), false on errors.</returns>
        internal bool Checkout(RMServiceTicket changeTicket)
        {
            CMContext.CMState currentState = this._CMContext.State;
            bool result = false;
            try
            {
                if (this._CMContext.CMEnabledRepository && !this._CMContext.CMEnabledService)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.Checkout >> Enable CM for this particular service!");
                    this._CMContext.State = CMContext.CMState.Created;
                }

                if (this._CMContext.CMEnabledService)
                {
                    if (!changeTicket.Valid)
                    {
                        Logger.WriteError("Plugin.Application.CapabilityModel.Service.Checkout >> Attempt to checkout service '" + Name + 
                                          "' with invalid Ticket '" + changeTicket.ID + "'!");
                        return false;    // We must present a valid ticket for a successfull checkout!
                    }
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.Checkout >> Checking out service '" + Name + "'...");
                    this._CMContext.CheckoutService(changeTicket);
                    SetColor(this._CMContext.State);
                }
                result = true;
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.Service.Checkout >> Checkout of service '" + Name + 
                                  "' failed because: " + Environment.NewLine + exc.ToString());
                this._CMContext.State = currentState;
                SetColor(this._CMContext.State);
            }
            return result;
        }

        /// <summary>
        /// Commits all files that are in the service build path to the repository at the HEAD of the current branch. 
        /// If the user requested a an auto-release, the commit is followed by a release of the service artefacts.
        /// When CM is not active for the service, the operation won't have any effects (always returns 'true').
        /// </summary>
        /// <param name="message">Commit reporting message.</param>
        /// <param name="autoRelease">When set to 'true', the commit will be followed by a release of the service.</param>
        /// <returns>True on successfull commit (or CM not active for the service), false when there was nothing to commit (state is not
        /// updated in this case).</returns>
        /// <exception cref="InvalidOperationException">Thrown when commit could not be performed.</exception>
        internal bool Commit(string message, bool autoRelease = false)
        {
            bool result = false;
            try
            {
                if (this._CMContext.CMEnabledService)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.Commit >> Committing service '" + Name +
                                     "' with auto-release set to: '" + autoRelease + "'...");
                    string commitID = ContextSlt.GetContextSlt().GetConfigProperty(_CommitIDLeader) +
                                      this._businessFunctionID + "." + this._containerPackage.Name + ":" +
                                      this.Name + "_V" + this._version.Item1 + "P" + this._version.Item2 + "B" + this._serviceClass.BuildNumber;
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.Commit >> CommitID = '" + commitID + "'...");
                    string commitMsg = commitID + Environment.NewLine + message;
                    result = this._CMContext.CommitService(commitMsg, autoRelease);
                }
                else result = true;
            }
            catch (Exception exc)
            {
                string msg = "Commit of service '" + Name + "' failed because: " + Environment.NewLine + exc.Message;
                Logger.WriteError("Plugin.Application.CapabilityModel.Service.Commit >> " + msg + Environment.NewLine + exc.ToString());
                SetColor(this._CMContext.State);
                throw new InvalidOperationException(msg);
            }
            SetColor(this._CMContext.State);
            return result;
        }

        /// <summary>
        /// Creates a verbatim copy of the current service at the same level and in the same container as the current service.
        /// The new service will receive the specified 'newSvcName', which should typically include the major version. The operation
        /// does NOT alter te version in any way, it remains to be the responsibility of the caller to update the version of the
        /// copied service appropriately.
        /// </summary>
        /// <param name="newName">Name to be assigned to the new service.</param>
        /// <returns>The newly created service class or NULL on errors.</returns>
        internal MEClass CopyService(string newName)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.CopyService >> Copying service '" + this._serviceDeclPackage.Name + "' to '" + newName + "'...");
            string exportFile = Path.GetTempFileName();
            this._serviceDeclPackage.ExportPackage(exportFile);
            MEClass newSvcClass = null;

            Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.CopyService >> Importing to new package structure...");
            ContextSlt context = ContextSlt.GetContextSlt();
            string containerStereotype = context.GetConfigProperty(_ServiceContainerPkgStereotype);
            this._containerPackage.ImportPackage(exportFile, this._containerPackage.Name, containerStereotype, newName);
            File.Delete(exportFile);

            // Now that we have created a new package structure, retrieve the newly created service class.
            MEPackage newDeclPackage = this._containerPackage.FindPackage(newName, context.GetConfigProperty(_ServiceDeclPkgStereotype));
            if (newDeclPackage != null)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.CopyService >> Retrieved the new service package...");
                MEPackage newSvcModelPackage = newDeclPackage.FindPackage(context.GetConfigProperty(_ServiceModelPkgName), 
                                                                          context.GetConfigProperty(_ServiceModelPkgStereotype));
                if (newSvcModelPackage != null)
                {
                    newSvcClass = newSvcModelPackage.FindClass(this.Name, context.GetConfigProperty(_ServiceClassStereotype));
                    CMContext.CopyState(ServiceClass, newSvcClass, true);
                }
            }
            return newSvcClass;
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
        /// Delete all specified tags from the repository (both locally as well as remote).
        /// </summary>
        /// <param name="tagsToBeDeleted">List of tags to be deleted.</param>
        internal void DeleteTags(List<string> tagsToBeDeleted)
        {
            this._CMContext.DeleteTags(tagsToBeDeleted);
        }

        /// <summary>
        /// Force the Configuration Management state to 'modified'. Will have effect only when Configuration Management is enabled for this service.
        /// We can get an exception in case the current CM state does not allow a transition to 'dirty', e.g. in case CM is enabled and the
        /// service has not yet been checked-out, we can not modify the state.
        /// </summary>
        /// <exception cref="InvalidOperationException">Is thrown when we are not allowed to perform the transition to Modified state!</exception>
        internal void Dirty()
        {
            if (this._CMContext.CMEnabledService)
            {
                this._CMContext.State = CMContext.CMState.Modified;
                SetColor(CMContext.CMState.Modified);
            }
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
        /// @OPERSTATUS@    = The operational state of the service.
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
            string operationalState = EnumConversions<OperationalState>.EnumToString(this._operationalState);

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
                FQN = FQN.Replace("@OPERSTATUS@", operationalState);
                FQN = FQN.Replace("@YEAR@", DateTime.Now.Year.ToString());
                FQN = FQN.Replace("@MONTH@", DateTime.Now.Month.ToString());
            }
            Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.getFQN >> Constructed: '" + FQN + "'.");
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
                    Logger.WriteError("Plugin.Application.CapabilityModel.Service.handleCapabilities >> Exception caught during processing in stage '" + stage +"': " + exc.ToString());
                    HandleStage(processor, ProcessingStage.Cancel);
                    result = false;
                }
            }
            return result;
        }

        /// <summary>
        /// This function increments the minor version of the Service class and all children. As a side effect, the build number is reset to 0.
        /// Note that this function is ONLY used when CM is disabled. When CM is enabled, version updates must occur exclusively on service
        /// checkout and individual dialogs must NOT show an 'increment minor version' flag.
        /// When CM is enabled, calling this function has NO effect.
        /// </summary>
        internal void IncrementVersion()
        {
            // Function does nothing in case CM is active (in which case version management is dealt with through the 'checkout' function)...
            if (!this._CMContext.CMEnabledService)
            {
                this._serviceClass.Version = new Tuple<int, int>(this._serviceClass.Version.Item1, this._serviceClass.Version.Item2 + 1);
                this._version = this._serviceClass.Version;
                this._serviceClass.BuildNumber = _DefaultBuildNr;

                CreateLogEntry("Minor version changed to: '" + this._serviceClass.Version.Item2 + "'.");
                foreach (Capability cap in this._serviceCapabilities) cap.VersionSync();
            }
        }

        /// <summary>
        /// The path name used to store Service Configuration Items depends on configuration management state. If CM is disabled, we
        /// track each service production run in a separate directory. If CM is enabled, all runs go to the same directory and version snap-shots
        /// are managed by our version management repository (typically GIT).
        /// All CI's live in (or below) a repository directory, indicated by 'RepositoryPath'. This is an absolute path, the root of which must be set in
        /// configuration. When not set, we throw an exception.
        /// Each Service also has a 'ServiceBuildPath', which references a service-specific directory containing Service-specific configuration
        /// items. This path is relative to the repository root.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException">Is thrown when we could not retrieve a valid repository descriptor or the root path has not been set.</exception>
        internal void InitializePath()
        {
            try
            {
                // If we don't have a configured root path, throw an exception...
                ContextSlt context = ContextSlt.GetContextSlt();
                string artefactFolder = context.GetConfigProperty(_CMArtefactsFolderName);
                string documentFolder = context.GetConfigProperty(_CMDocumentsFolderName);
                if (artefactFolder != string.Empty) artefactFolder = "/" + artefactFolder;
                if (documentFolder != string.Empty) documentFolder = "/" + documentFolder;
                if (string.IsNullOrEmpty(this._repositoryPath)) throw new ConfigurationErrorsException("Configuration Root Path not set, aborting!");
                Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.initializePath >> Repository path set to '" + this._repositoryPath + "'...");

                // We might miss some levels in the structure, so construct as we go. (rootPath MUST exist)...
                this._serviceBuildPath = this._businessFunctionID + "." + this._containerPackage.Name;
                if (!Directory.Exists(this._repositoryPath + "/" + this._serviceBuildPath)) Directory.CreateDirectory(this._repositoryPath + "/" + this._serviceBuildPath);
                if (IsDefaultOperationalState) this._serviceBuildPath += "/" + Name + "_V" + Version.Item1.ToString();
                else this._serviceBuildPath += "/" + Name + "_" + EnumConversions<OperationalState>.EnumToString(this._operationalState) + "_V" + Version.Item1.ToString();
                if (!Directory.Exists(this._repositoryPath + "/" + this._serviceBuildPath)) Directory.CreateDirectory(this._repositoryPath + "/" + this._serviceBuildPath);
                if (this._CMContext.CMEnabledRepository && !Directory.Exists(this._repositoryPath + "/" + this._serviceBuildPath + artefactFolder))
                    Directory.CreateDirectory(this._repositoryPath + "/" + this._serviceBuildPath + artefactFolder);
                if (this._CMContext.CMEnabledRepository && !Directory.Exists(this._repositoryPath + "/" + this._serviceBuildPath + documentFolder))
                    Directory.CreateDirectory(this._repositoryPath + "/" + this._serviceBuildPath + documentFolder);
                this._serviceBuildPath += artefactFolder;

                // Even if CM is disabled for this particular service, we must still adhere to the path standard for a CM-enabled repository!
                // The only difference is that, when CM is not active, we don't create a separate Artefact directory.
                if (!this._CMContext.CMEnabledRepository)
                {
                    string buildDir = "/P" + Version.Item2.ToString() + "B" + this._serviceClass.BuildNumber;
                    this._serviceBuildPath += buildDir;
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.initializePath >> Creating build directory '" + this._serviceBuildPath + "'...");
                    if (!Directory.Exists(this._repositoryPath + "/" + this._serviceBuildPath)) Directory.CreateDirectory(this._repositoryPath + "/" + this._serviceBuildPath);
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.Service.initializePath >> Exception when creating path names because: " + exc.ToString());
                throw;          // Forward the exception.
            }
        }

        /// <summary>
        /// Returns true when the proposed CM state can be reached from the current CM state.
        /// </summary>
        /// <param name="newState">Proposed state.</param>
        /// <returns>True in case proposed state can be reached from current state, false otherwise.</returns>
        internal bool IsValidCMState(CMContext.CMState newState)
        {
            return this._CMContext.IsValidTransition(newState);
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
        /// Paint the Service class on the specified diagram in the current representation color. The class must be present on the specified
        /// diagram for the method to work!
        /// </summary>
        /// <param name="diagram">Diagram containing the service.</param>
        internal void Paint(Diagram diagram)
        {
            diagram.SetClassColor(this._serviceClass, this._representationColor);
        }

        /// <summary>
        /// Releases the service as a development candidate for implementation. 
        /// </summary>
        /// <param name="message">Release reporting message.</param>
        /// <exception cref="InvalidOperationException">Thrown when commit could not be performed.</exception>
        internal void Release(string message)
        {
            try
            {
                if (this._CMContext.CMEnabledService)
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.Release >> Releasing service '" + Name + "'...");
                    string releaseID = ContextSlt.GetContextSlt().GetConfigProperty(_CommitIDLeader) +
                                      this._businessFunctionID + "." + this._containerPackage.Name + ":" +
                                      this.Name + "_V" + this._version.Item1 + "P" + this._version.Item2 + "B" + this._serviceClass.BuildNumber;
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.Release >> ReleaseID = '" + releaseID + "'...");
                    string releaseMsg = releaseID + Environment.NewLine + message;
                    this._CMContext.ReleaseService(releaseMsg);
                    SetColor(this._CMContext.State);
                }
            }
            catch (Exception exc)
            {
                string msg = "Release of service '" + Name + "' failed because: " + Environment.NewLine + exc.Message;
                Logger.WriteError("Plugin.Application.CapabilityModel.Service.Release >> " + msg + Environment.NewLine + exc.Message);
                SetColor(this._CMContext.State);
                throw new InvalidOperationException(msg, exc);
            }
            SetColor(this._CMContext.State);
        }

        /// <summary>
        /// Restores the service from a Configuration Management snapshot with the given tag name. If 'newVersion' is set to 'false', the
        /// restored version will overwrite the service major version specified by the tag, restoring it exactly to the version in the snapshot.
        /// If 'newVersion' is set to 'true', the restore operation will create a new major version of the service, which is 1 higher
        /// then the highest version currently present in the service container.
        /// Note that the operation can not update the major/minor/build version numbers in the imported model since this requires a
        /// capability model tree that we can not construct at this time!
        /// If we restored the 'current service', we should reload the context at a higher level since consistency of the service
        /// model can no longer be guaranteed!
        /// </summary>
        /// <param name="featureTag">Tag name identifying the service snapshot to be restored.</param>
        /// <param name="newVersion">Set to 'true' to create a new major version using the snapshot.</param>
        /// <returns>The restored (imported) service class.</returns>
        /// <exception cref="ArgumentException">Is thrown on import errors.</exception>
        internal MEClass RestoreService(string featureTag, bool newVersion)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.RestoreService >> Restoring service from feature tag '" + 
                             featureTag + "' with 'newVersion' set to '" + newVersion + "'...");
            MEClass importedService = this._CMContext.RevertService(featureTag, newVersion);
            if (importedService.Version.Item1 == this._version.Item1) this._serviceClass = null;  // This invalidates our current model!
            return importedService;
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
        /// Must be invoked in order to change the version of the service and all registered capabilities.
        /// Also, when the new major version is updated, we also update te Service Declaration Package name to reflect this.
        /// The 'Service' object keeps a separate copy of the version property since updating this in the underlying repository takes time
        /// and might lead to mismatches when synchronising a complete capability tree. By keeping the version separate, we guarantee that
        /// child objects always get the correct version when using the 'Service' interface.
        /// When CM is enabled for the service, this is the ONLY supported mechanism for version updates.
        /// UpdateVersion changes the CM context of a service, which we could do even for committed/released services (we don't change
        /// the actual CM state of the service).
        /// Note that the operation does not check the current version of the service. It simply overwrites the entire tree with the
        /// specified version tuple. However, if the new major version is different from the current major version, it WILL change the
        /// name of the service declaration package to assure that it matches the assigned major version.
        /// </summary>
        /// <param name="newVersion">New service version.</param>
        internal void UpdateVersion(Tuple<int,int> newVersion)
        {
            Tuple<int, int> currentVersion = Version;
            this._serviceClass.Version = newVersion;
            this._version = newVersion;

            CreateLogEntry("Version set to: '" + newVersion.Item1 + "." + newVersion.Item2 + "'.");
            if (currentVersion.Item1 != newVersion.Item1)
            {
                string newPackageName = Name + "_V" + newVersion.Item1;
                this._serviceDeclPackage.Name = newPackageName;
            }

            // We enforce the specified version upon each child (might or might not be different from current)...
            foreach (Capability cap in this._serviceCapabilities) cap.VersionSync();

            if (currentVersion.Item1 != newVersion.Item1 || currentVersion.Item2 != newVersion.Item2)
            {
                // These must be updated only in case we really changed the existing version...
                InitializePath();                                       // Enforce a new path structure, based on the new version.
                this._serviceClass.BuildNumber = _DefaultBuildNr;       // And reset the build number.
                this._CMContext.UpdateVersion();
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
        /// Initialises the 'serviceURI' property, which is a unique identification of the service within the repository.
        /// Format of the uri is: 'urn:x-enexis:ecdm:service-id:[Business-Function].[Container-Package]:[Service-Name]:[Version]
        /// </summary>
        private void BuildURI()
        {
            // Create a unique identifier (in URI format) for this service...
            this._serviceURI = ContextSlt.GetContextSlt().GetConfigProperty(_NamespacePrefix) + ":service-id:" + this._businessFunctionID + "." +
                               this._containerPackage.Name + ":" + this._serviceClass.Name + ":" +
                               this._version.Item1 + "." + this._version.Item2 + "." + this.BuildNumber;
            Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.BuildURI >> Created URN: '" + this._serviceURI + "'.");
        }

        /// <summary>
        /// Helper function that searches the business function hierarchy for the proper business function ID associated with our service.
        /// </summary>
        private void LoadBusinessFunctionID()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            this._businessFunctionID = string.Empty;
            if (this._containerPackage.HasStereotype(context.GetConfigProperty(_ServiceContainerPkgStereotype)))
            {
                // Search up the hierarchy until we get the proper ID or reach the root of the package tree...
                MEPackage parentPkg = this._containerPackage.Parent;
                string rootName = context.GetConfigProperty(_DomainModelsRootPkgName);
                string IDTag = context.GetConfigProperty(_BusinessFunctionIDTag);
                while (parentPkg.Name != rootName)
                {
                    string ID = parentPkg.GetTag(IDTag);
                    if (ID != string.Empty)
                    {
                        this._businessFunctionID = ID;
                        break;
                    }
                    parentPkg = parentPkg.Parent;
                }
            }
            Logger.WriteInfo("Plugin.Application.CapabilityModel.Service.GetBusinessFunctionID >> Collected business function: '" + this._businessFunctionID + "'.");
        }

        /// <summary>
        /// Helper function, called by constructors, which determines the configuration management state of the service and loads properties accordingly.
        /// In case of a NEW service, we MUST pass the create ticket. In case of EXISTING service, the ticket MAY already be registered in the
        /// service and it MUST NOT be passed as an argument. Thus, by checking the ticket parameter, we can establish whether we're dealing
        /// with a new- or an existing service.
        /// </summary>
        /// <param name="remoteTicket">For a new service, this is the ticket used for creation. Otherwise, it must be null.</param>
        /// <param name="projectOrderID">For a new service, this is the Id of the project used for creation. Otherwise, it is ignored.</param>
        /// <exception cref="ConfigurationErrorsException">Is thrown when we could not retrieve a proper CM repository descriptor.</exception>
        private void LoadCMState(Ticket remoteTicket, string projectOrderID)
        {
            RepositoryDescriptor repoDsc = CMRepositoryDscManagerSlt.GetRepositoryDscManagerSlt().GetCurrentDescriptor();
            if (repoDsc == null)
            {
                string message = "Unable to retrieve proper CM repository descriptor, aborting!";
                Logger.WriteError("Plugin.Application.CapabilityModel.Service >> " + message);
                throw new ConfigurationErrorsException(message);
            }

            bool newService = remoteTicket != null;
            var ticket = (CMRepositorySlt.GetRepositorySlt().IsCMEnabled && newService)? new RMServiceTicket(remoteTicket, projectOrderID, this): null;
            this._CMContext = new CMContext(this, ticket);
            this._repositoryPath = repoDsc.LocalRootPath;

            // Now we set the representation color of the service according to the CM state...
            SetColor(this._CMContext.State);
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

        /// <summary>
        /// Changes the color of the Service class on diagrams based on configuration state. As long as the service has CI's that have not 
        /// (yet) been touched, the class shows up in white. When CI's have been created or changed but not committed, the class shows up in red.
        /// When the CI's have been committed, but not yet officially released, the class shows up in yellow.
        /// When the service is in 'stable' state (released to CI/CD), the class shows up in default color (green).
        /// </summary>
        /// <param name="state">State to be used for painting.</param>
        private void SetColor(CMContext.CMState state)
        {
            switch (state)
            {
                case CMContext.CMState.Created:
                case CMContext.CMState.CheckedOut:
                    this._representationColor = Diagram.ClassColor.White;
                    break;

                case CMContext.CMState.Modified:
                    this._representationColor = Diagram.ClassColor.Red;
                    break;

                case CMContext.CMState.Committed:
                    this._representationColor = Diagram.ClassColor.Yellow;
                    break;

                default:
                    this._representationColor = Diagram.ClassColor.Default;
                    break;
            }
        }
    }
}
