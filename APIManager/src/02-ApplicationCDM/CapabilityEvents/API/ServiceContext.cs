﻿using System.Collections.Generic;
using Framework.Logging;
using Framework.Model;
using Framework.Context;
using Framework.View;
using Framework.Exceptions;
using Framework.Util;
using Framework.ConfigurationManagement;
using Plugin.Application.CapabilityModel;
using Plugin.Application.CapabilityModel.API;
using Plugin.Application.CapabilityModel.CodeList;

namespace Plugin.Application.Events.API
{
    /// <summary>
    /// Instances of this class represent the current context in which we must process our user events. The context contains the current
    /// Service Model and all relevant classes and packages, as far as we can determine them given the state of the repository. The constructor
    /// attempts to figure out how we have been activated (from a diagram or the package tree) and where we are on the diagram or package tree.
    /// Before using the contents, check the 'Valid' property to see whether a valid context has indeed been established!
    /// </summary>
    internal class ServiceContext
    {
        // Configuration properties used by this module...
        private const string _RootPkgName                       = "RootPkgName";
        private const string _ServiceDeclPkgStereotype          = "ServiceDeclPkgStereotype";
        private const string _CodeListDeclPkgStereotype         = "CodeListDeclPkgStereotype";
        private const string _ServiceOperationPkgStereotype     = "ServiceOperationPkgStereotype";
        private const string _GenericMessagePkgStereotype       = "GenericMessagePkgStereotype";
        private const string _ServiceClassStereotype            = "ServiceClassStereotype";
        private const string _ServiceModelPkgName               = "ServiceModelPkgName";
        private const string _ServiceModelDiagramName           = "ServiceModelDiagramName";
        private const string _InterfaceContractClassStereotype  = "InterfaceContractClassStereotype";
        private const string _CommonSchemaClassStereotype       = "CommonSchemaClassStereotype";
        private const string _OperationClassStereotype          = "OperationClassStereotype";
        private const string _MessageAssociationStereotype      = "MessageAssociationStereotype";
        private const string _BusinessMessageClassStereotype    = "BusinessMessageClassStereotype";
        private const string _ResourceCollectionPkgStereotype   = "ResourceCollectionPkgStereotype";
        private const string _ResourceClassStereotype           = "ResourceClassStereotype";
        private const string _RESTOperationClassStereotype      = "RESTOperationClassStereotype";
        private const string _DataModelPkgName                  = "DataModelPkgName";
        private const string _DataModelPkgStereotype            = "DataModelPkgStereotype";
        private const string _ServiceArchetypeTag               = "ServiceArchetypeTag";
        private const string _ServiceCapabilityClassBaseStereotype = "ServiceCapabilityClassBaseStereotype";
        private const string _RMPackageStereotype               = "RMPackageStereotype";
        private const string _RMPackageName                     = "RMPackageName";
        private const string _RMReleasePackageParentPath        = "RMReleasePackageParentPath";
        private const string _RMReleasePackageParentName        = "RMReleasePackageParentName";
        private const string _RMReleasePackagePath              = "RMReleasePackagePath";

        private const bool _NOBUILDHIERARCHY = false;           // Used for CodeLists to suppress construction of complete class hierarchy.

        private MEClass         _serviceClass;                  // Identifies the service.
        private MEClass         _interfaceClass;                // Contains interface class currently selected by user (if applicable).
        private MEClass         _operationClass;                // Contains operation class currently selected by user (if applicable).
        private MEClass         _resourceClass;                 // Contains resource class currently selected by user (if applicable).
        private MEPackage       _declarationPackage;            // Service declaration package.
        private MEPackage       _serviceModelPackage;           // Package containing the service model.
        private MEPackage       _operationPackage;              // Contains operation package if selected by user.
        private MEPackage       _resourceCollectionPackage;     // Resource collection package is selected by user.
        private MEPackage       _releaseHistoryPackage;         // Release history package is selected by user.
        private Diagram         _diagram;                       // Currently active servicemodel diagram.
        private Diagram         _serviceDiagram;                // The diagram that contains the Service class.
        private MEClass         _treeNodeTarget;                // Search target when browsing the tree hierarchy.
        private TreeNode<MEClass> _treeNodeResult;              // Node that is associated with treeNodeTarget.
        private TreeNode<MEClass> _hierarchy;                   // Contains the hierarchy from Service down to Message Capabilities.
        private Service.ServiceArchetype  _type;                // Defines whether we're in REST or SOAP 'mode' (or Unknown).
        private bool            _lockContainer;                 // Indicates that 'lockModel' has a broader scope (container instead of decl.).
        private bool            _hasValidRepositoryDsc;         // True when we have found a valid repository descriptor.

        /// <summary>
        /// Returns the collected context elements...
        /// </summary>
        internal MEClass ServiceClass                   { get { return this._serviceClass; } }
        internal MEClass InterfaceClass                 { get { return this._interfaceClass; } }
        internal MEClass OperationClass                 { get { return this._operationClass; } }
        internal MEClass CommonSchemaClass              { get { return GetCommonSchema(this._interfaceClass); } }
        internal MEClass ResourceClass                  { get { return this._resourceClass; } }
        internal List<MEClass> InterfaceList            { get { return this._hierarchy.ChildObjects; } }
        internal MEPackage DeclarationPackage           { get { return this._declarationPackage; } }
        internal MEPackage OperationPackage             { get { return this._operationPackage; } }
        internal MEPackage ResourceCollectionPackage    { get { return this._resourceCollectionPackage; } }
        internal MEPackage ReleaseHistoryPackage        { get { return this._releaseHistoryPackage; } }
        internal MEPackage SVCModelPackage              { get { return this._serviceModelPackage; } }
        internal Diagram MyDiagram                      { get { return this._diagram; } }
        internal Diagram ServiceDiagram                 { get { return this._serviceDiagram; } }
        internal TreeNode<MEClass> Hierarchy            { get { return this._hierarchy; } }

        /// <summary>
        /// Getters for additional properties:
        /// Valid = Context is assumed to be valid if the service class has been properly initialized or we have a release history package.
        /// Type = Returns the type of Service.
        /// HasValidRepositoryDescriptor = Returns true if we have a valid repository descriptor.
        /// </summary>
        internal bool Valid                             { get { return this._hasValidRepositoryDsc && (this._serviceClass != null || this._releaseHistoryPackage != null); } }
        internal Service.ServiceArchetype Type          { get { return this._type; } }
        internal bool HasValidRepositoryDescriptor      { get { return this._hasValidRepositoryDsc; } }

        /// <summary>
        /// Creates the context based on either selected diagram or entry in package tree. The constructor collects contextual information and
        /// attempts to intialize the most important contextual elements (declaration package, service class, list of interfaces and the active
        /// diagram). It also attempts to initialize currently selected classes and/or packages.
        /// Determining context is tricky since the current elements are not necessarily reliable. Depending on whether we have been triggered
        /// from diagram or package tree, different elements might have been selected by the user and current class, current diagram and current
        /// package might still contain values from earlier selections (e.g. switching between diagram tabs without actually clicking on the
        /// diagram will NOT result in 'currentDiagram' to point to the diagram the user is looking at and the same is true for 'currentPackage'
        /// and 'currentClass'). 
        /// </summary>
        /// <param name="isDiagram">True if event raised from a diagram context, false on package tree.</param>
        internal ServiceContext (bool isDiagram)
        {
            Logger.WriteInfo("Plugin.Application.Events.API.ServiceContext >> Building context for " + (isDiagram ? "Diagram..." : "Package Tree..."));

            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            Capability.FlushRegistry();             // Make sure to clean-up collected classes from previous Event, could contain stale contents!

            // Check whether we have a valid repository descriptor for the current project. If not, there is not much we can do...
            this._hasValidRepositoryDsc = CMRepositoryDscManagerSlt.GetRepositoryDscManagerSlt().GetCurrentDescriptor() != null;

            // Retrieve a bunch of configuration properties...
            string interfaceContractClassStereotype = context.GetConfigProperty(_InterfaceContractClassStereotype);
            string operationPkgStereotype           = context.GetConfigProperty(_ServiceOperationPkgStereotype);
            string serviceDeclPkgStereotype         = context.GetConfigProperty(_ServiceDeclPkgStereotype);
            string codeListDeclPkgStereotype        = context.GetConfigProperty(_CodeListDeclPkgStereotype);
            string serviceClassStereotype           = context.GetConfigProperty(_ServiceClassStereotype);
            string operationClassStereotype         = context.GetConfigProperty(_OperationClassStereotype);
            string operationClassRESTStereotype     = context.GetConfigProperty(_RESTOperationClassStereotype);
            string serviceModelPkgName              = context.GetConfigProperty(_ServiceModelPkgName);
            string resourceClassStereotype          = context.GetConfigProperty(_ResourceClassStereotype);
            string resourceCollectionPkgStereotype  = context.GetConfigProperty(_ResourceCollectionPkgStereotype);
            string rootPackageName                  = context.GetConfigProperty(_RootPkgName);
            string dataModelPkgName                 = context.GetConfigProperty(_DataModelPkgName);
            string dataModelPkgStereotype           = context.GetConfigProperty(_DataModelPkgStereotype);
            string releaseMgmtPkgStereotype         = context.GetConfigProperty(_RMPackageStereotype);
            string releaseMgmtPkgName               = context.GetConfigProperty(_RMPackageName);

            // Check what the Repository knows about our context...
            MEClass currentClass            = context.CurrentClass;
            MEPackage currentPackage        = context.CurrentPackage;
            Diagram currentDiagram          = context.CurrentDiagram;

            // Initialize all context items to NULL for the time being...
            this._diagram                   = null;
            this._serviceDiagram            = null;
            this._declarationPackage        = null;
            this._serviceClass              = null;
            this._serviceModelPackage       = null;
            this._operationClass            = null;
            this._operationPackage          = null;
            this._interfaceClass            = null;
            this._resourceCollectionPackage = null;
            this._releaseHistoryPackage     = null;
            this._resourceClass             = null;
            this._type                      = Service.ServiceArchetype.Unknown;

            if (isDiagram && currentDiagram != null)
            {
                // If we're on a diagram, we CAN NOT trust 'current package' and 'current class' since these are  not necessarily initialized 
                // properly at this time. We DO assume that the current diagram is properly initialized and reflects the diagram that the user 
                // is looking at. If not, there is nothing we can do and the context will remain invalid.
                // Therefor, we're looking at CurrentDiagram, get the owning packages and continue from there...
                // In case of REST services, we COULD be at a Resource Collection diagram, which is one level down from the Service Model...
                // Since we don't know the names of the Resource Collection diagrams, but we DO know the name of the Service Model diagram, let's
                // check that one and act accordingly...
                this._diagram = currentDiagram;
                if (currentDiagram.Name == context.GetConfigProperty(_ServiceModelDiagramName))
                {
                    // If we're on a service model diagram, we can extract the service model package and take its parent to get to the declaration
                    // package. Next, we check whether the declaration package contains a 'DataModel' package, which should typically be there in case
                    // of REST services...
                    this._serviceModelPackage = currentDiagram.OwningPackage;
                    this._type = (this._serviceModelPackage.Parent.FindPackage(dataModelPkgName, dataModelPkgStereotype) != null) ? Service.ServiceArchetype.REST : Service.ServiceArchetype.SOAP;
                }
                else // We must be on a Resource Collection diagram...
                {
                    this._resourceCollectionPackage = currentDiagram.OwningPackage;
                    this._serviceModelPackage = currentDiagram.OwningPackage.Parent;
                    this._type = Service.ServiceArchetype.REST;
                }
                this._declarationPackage = this._serviceModelPackage.Parent;
                if (this._declarationPackage.HasStereotype(codeListDeclPkgStereotype)) this._type = Service.ServiceArchetype.CodeList;

                if (currentClass != null)
                {
                    if (currentClass.HasStereotype(operationClassStereotype) || currentClass.HasStereotype(operationClassRESTStereotype))
                    {
                        this._operationClass = currentClass;
                        this._operationPackage = this._declarationPackage.FindPackage(this._operationClass.Name, operationPkgStereotype);
                    }
                    else if (currentClass.HasStereotype(interfaceContractClassStereotype))  this._interfaceClass = currentClass;
                    else if (currentClass.HasStereotype(resourceClassStereotype))           this._resourceClass = currentClass;
                }
            }
            else if (currentPackage != null) //In package browser or menu bar..
            {
                // In this case, we CAN NOT trust 'current class', nor 'current diagram' since we have only selected a package for sure.
                if (currentPackage.HasStereotype(serviceDeclPkgStereotype))
                {
                    // We're at the Service Declaration package itself. Not more to detect up here...
                    this._declarationPackage = currentPackage;
                }
                else if (currentPackage.HasStereotype(codeListDeclPkgStereotype))
                {
                    // We're at a CodeList Declaration package.
                    this._declarationPackage = currentPackage;
                    this._type = Service.ServiceArchetype.CodeList;
                }
                else if (currentPackage.HasStereotype(operationPkgStereotype))
                {
                    // We're at an operation package. We can find the declaration package as well as the operation package...
                    this._operationPackage = currentPackage;
                    this._declarationPackage = currentPackage.Parent;
                }
                else if (currentPackage.HasStereotype(resourceCollectionPkgStereotype))
                {
                    // We're at a resource collection package, which is a child of the service model.
                    this._resourceCollectionPackage = currentPackage;
                    this._serviceModelPackage = currentPackage.Parent;
                    this._declarationPackage = this._serviceModelPackage.Parent;
                }
                else if (currentPackage.HasStereotype(releaseMgmtPkgStereotype) && currentPackage.Name == releaseMgmtPkgName)
                {
                    // We're at a release history package, which is either a child of the service model or a child of the Domain Model..
                    string releasePackageParentPath = context.GetConfigProperty(_RMReleasePackageParentPath);
                    string releasePackageParentName = context.GetConfigProperty(_RMReleasePackageParentName);
                    MEPackage releasePackageParent = model.FindPackage(releasePackageParentPath, releasePackageParentName);
                    this._releaseHistoryPackage = currentPackage;
                    if (releasePackageParent == null || releasePackageParent != currentPackage.Parent)
                    {
                        // Here we assume we're in a Service package structure...
                        this._releaseHistoryPackage = currentPackage;
                        this._declarationPackage = currentPackage.Parent;
                    }
                }
                else if (currentPackage.Parent.HasStereotype(serviceDeclPkgStereotype))
                {
                    // We're at "some" specialized package underneath the Service Declaration.
                    this._declarationPackage = currentPackage.Parent;
                }
                else if (currentPackage.HasStereotype(context.GetConfigProperty(_GenericMessagePkgStereotype)))
                {
                    // We're at 'some' message package, but we don't have an idea about the structure, other then that it must be part of
                    // a service declaration since we have the proper stereotype. Looking upwards for the declaration package...
                    // If we hit the repository root package, we can be sure that we're at the wrong place.
                    MEPackage package = currentPackage.Parent;
                    while (!package.HasStereotype(serviceDeclPkgStereotype) && package.Name != rootPackageName) package = package.Parent;
                    if (package.Name != rootPackageName) this._declarationPackage = package;
                }

                if (this._declarationPackage != null && this._serviceModelPackage == null)
                {
                    this._serviceModelPackage = this._declarationPackage.FindPackage(serviceModelPkgName);
                }

                if (this._operationPackage != null && this._serviceModelPackage != null)
                {
                    this._operationClass = (this._resourceCollectionPackage != null)? 
                        this._resourceCollectionPackage.FindClass(this._operationPackage.Name, operationClassRESTStereotype):
                        this._serviceModelPackage.FindClass(this._operationPackage.Name, operationClassStereotype);
                }
            }

            // If we have not been able to detect these packages, the context is REALLY wrong.
            if (this._declarationPackage == null || this._serviceModelPackage == null)
            {
                if (this._releaseHistoryPackage == null)
                {
                    Logger.WriteError("Plugin.Application.Events.API.ServiceContext >> Illegal context, throwing exception!");
                    throw new IllegalContextException("No valid packages in context");
                }
                else
                {
                    Logger.WriteInfo("Plugin.Application.Events.API.ServiceContext >> Top-level Release History, only partial context available!");
                    if (this._diagram == null) this._diagram = this._releaseHistoryPackage.FindDiagram();
                    return;
                }
            }

            // Now we should be able to find our Service package and class and deduct the type from there. If the service class does not have
            // an archetype, we use the preliminary value to assign one and send a warning to the user that it has to be verified...
            string serviceName = this._declarationPackage.Name.Substring(0, this._declarationPackage.Name.IndexOf("_V"));
            this._serviceClass = this._serviceModelPackage.FindClass(serviceName, serviceClassStereotype);
            if (this._serviceClass != null)
            {
                string svcArchetypeTag = context.GetConfigProperty(_ServiceArchetypeTag);
                string svcArchetype = this._serviceClass.GetTag(svcArchetypeTag);
                if (svcArchetype != string.Empty) this._type = EnumConversions<Service.ServiceArchetype>.StringToEnum(svcArchetype);
                else if (this._type == Service.ServiceArchetype.Unknown)
                {
                    if (this._declarationPackage.HasStereotype(codeListDeclPkgStereotype)) this._type = Service.ServiceArchetype.CodeList;
                    else if (this._declarationPackage.FindPackage(dataModelPkgName, dataModelPkgStereotype) != null)
                        this._type = Service.ServiceArchetype.REST;
                    else this._type = Service.ServiceArchetype.SOAP;
                    this._serviceClass.SetTag(svcArchetypeTag, EnumConversions<Service.ServiceArchetype>.EnumToString(this._type), true);
                    Logger.WriteWarning("Found Service class without archetype; setting archetype to '" +
                                        this._type + "', please verify correctness!");
                }
            }

            // If we were not able to reliably detect Interface- or Operation classes before, we perform one more attempt
            // by looking at the context of the current class, which must be reliable by now...
            if (this._interfaceClass == null && currentClass != null &&
                currentClass.HasStereotype(interfaceContractClassStereotype) &&
                currentClass.OwningPackage == this._serviceModelPackage)
            {
                this._interfaceClass = currentClass;
            }

            // Looking for an operation class. Operations are always defined in the context of their owning resources and these in turn
            // must be in a child package of the Service Model Package.
            if (this._operationClass == null && currentClass != null &&
                (currentClass.HasStereotype(operationClassStereotype) || currentClass.HasStereotype(operationClassRESTStereotype)) &&
                currentClass.OwningPackage.Parent == this._serviceModelPackage)
            {
                this._operationClass = currentClass;
            }

            // Looking for a resource class. Resources are either defined in the context of their owning resources or they have their own
            // package. In either case, these must be in a child package of the Service Model Package.
            if (this._resourceClass == null && currentClass != null && currentClass.HasStereotype(resourceClassStereotype) &&
                currentClass.OwningPackage.Parent == this._serviceModelPackage)
            {
                this._resourceClass = currentClass;
            }

            // Resources are ALWAYS defined in the package of the owning Resource Collection. This implies that the parent of ANY Resource class
            // is ALWAYS a Resource Collection package. Therefor, we don't have to check the archetype of the Resource Class...
            if (this._resourceCollectionPackage == null && this._resourceClass != null) this._resourceCollectionPackage = this._resourceClass.OwningPackage;

            if (this._diagram == null)
            {
                if (this._resourceClass != null)
                {
                    // If we have selected a Resource class, but not on a diagram, we're going to select the associated 
                    // diagram. If the resource is a collection, this is the diagram of that collection. Otherwise, it will be the
                    // diagram of the resource collection that 'owns' the currently selected resource.
                    // Typically, this is the only diagram in the package...
                    this._diagram = this._resourceClass.OwningPackage.FindDiagram();
                }
                else if (this._operationClass != null && this._type == Service.ServiceArchetype.REST)
                {
                    // If we have selected a REST operation, we select the associated diagram (typically, the only one in package)...
                    this._diagram = this._operationClass.OwningPackage.FindDiagram();
                }
                else this._diagram = this._serviceModelPackage.FindDiagram(this._serviceModelPackage.Name);
            }

            // If we are on a diagram that does not contain the Service class, this._diagram refers to "some" diagram.
            // Let's not assume anything and just search the service model package for the service diagram, in which case
            // we are sure we have the correct diagram...
            this._serviceDiagram = this._serviceModelPackage.FindDiagram(this._serviceModelPackage.Name);

            if (this._serviceClass != null) BuildHierarchy();

            Logger.WriteInfo("Plugin.Application.Events.API.ServiceContext >> Service type is: '" + this._type + "'.");
            if (this._diagram != null) Logger.WriteInfo("Plugin.Application.Events.API.ServiceContext >> Found diagram: " + this._diagram.Name);
            if (this._serviceClass != null) Logger.WriteInfo("Plugin.Application.Events.API.ServiceContext >> Selected service class: " + this._serviceClass.Name);
            if (this._interfaceClass != null) Logger.WriteInfo("Plugin.Application.Events.API.ServiceContext >> Selected interface class: " + this._interfaceClass.Name);
            if (this._operationClass != null) Logger.WriteInfo("Plugin.Application.Events.API.ServiceContext >> Selected operation class: " + this._operationClass.Name);
            if (this._resourceClass != null) Logger.WriteInfo("Plugin.Application.Events.API.ServiceContext >> Found resource: " + this._resourceClass.Name);
            if (this._declarationPackage != null) Logger.WriteInfo("Plugin.Application.Events.API.ServiceContext >> Found declaration package: " + this._declarationPackage.Name);
            if (this._operationPackage != null) Logger.WriteInfo("Plugin.Application.Events.API.ServiceContext >> Selected operation package: " + this._operationPackage.Name);
            if (this._serviceModelPackage != null) Logger.WriteInfo("Plugin.Application.Events.API.ServiceContext >> Found service model package: " + this._serviceModelPackage.Name);
            if (this._resourceCollectionPackage != null) Logger.WriteInfo("Plugin.Application.Events.API.ServiceContext >> Found resource collection package: " + this._resourceCollectionPackage.Name);
        }

        /// <summary>
        /// Creates context for a given Service Class. This constructor is invoked on an existing Service hierarchy, 'outside' the scope of a UI
        /// event. It receives the service class as an argument (which must be part of an existing service hierarchy) and constructs the proper 
        /// context from there. Since we start from a service class, the context will only contain those elements that we can derive from a
        /// service class (i.e. service type, declaration- and model packages and the model diagram)
        /// </summary>
        /// <param name="svcClass">The Service class on which we are going to base our context.</param>
        internal ServiceContext(MEClass svcClass)
        {
            Logger.WriteInfo("Plugin.Application.Events.API.ServiceContext >> Building context for service '" + svcClass.Name + "'...");

            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            Capability.FlushRegistry();             // Make sure to clean-up collected classes from previous Event, could contain stale contents!

            // Check whether we have a valid repository descriptor for the current project. If not, there is not much we can do...
            this._hasValidRepositoryDsc = CMRepositoryDscManagerSlt.GetRepositoryDscManagerSlt().GetCurrentDescriptor() != null;

            // Retrieve a bunch of configuration properties...
            string serviceDeclPkgStereotype = context.GetConfigProperty(_ServiceDeclPkgStereotype);
            string codeListDeclPkgStereotype = context.GetConfigProperty(_CodeListDeclPkgStereotype);
            string serviceModelPkgName = context.GetConfigProperty(_ServiceModelPkgName);
            string dataModelPkgName = context.GetConfigProperty(_DataModelPkgName);
            string dataModelPkgStereotype = context.GetConfigProperty(_DataModelPkgStereotype);

            // Initialize the context items...
            this._serviceClass = svcClass;
            this._serviceModelPackage = svcClass.OwningPackage;
            this._declarationPackage = this._serviceModelPackage.Parent;
            this._serviceDiagram = this._serviceModelPackage.FindDiagram(this._serviceModelPackage.Name);
            this._diagram = this._serviceDiagram;
            this._type = Service.ServiceArchetype.Unknown;

            // These will not be available in this context...
            this._operationClass = null;
            this._operationPackage = null;
            this._interfaceClass = null;
            this._resourceCollectionPackage = null;
            this._resourceClass = null;

            string svcArchetypeTag = context.GetConfigProperty(_ServiceArchetypeTag);
            string svcArchetype = this._serviceClass.GetTag(svcArchetypeTag);
            if (!string.IsNullOrEmpty(svcArchetype)) this._type = EnumConversions<Service.ServiceArchetype>.StringToEnum(svcArchetype);
            else
            {
                if (this._declarationPackage.HasStereotype(codeListDeclPkgStereotype))
                {
                    this._type = Service.ServiceArchetype.CodeList;
                }
                else if (this._declarationPackage.FindPackage(dataModelPkgName, dataModelPkgStereotype) != null)
                {
                    this._type = Service.ServiceArchetype.REST;
                }
                else this._type = Service.ServiceArchetype.SOAP;
                this._serviceClass.SetTag(svcArchetypeTag, EnumConversions<Service.ServiceArchetype>.EnumToString(this._type), true);
                Logger.WriteWarning("Found Service class without archetype; setting archetype to '" + this._type + 
                                    "', please verify correctness!");
            }
            BuildHierarchy();

            Logger.WriteInfo("Plugin.Application.Events.API.ServiceContext >> Service type is: '" + this._type + "'.");
            Logger.WriteInfo("Plugin.Application.Events.API.ServiceContext >> Found diagram: " + this._serviceDiagram.Name);
            Logger.WriteInfo("Plugin.Application.Events.API.ServiceContext >> Selected service class: " + this._serviceClass.Name);
            Logger.WriteInfo("Plugin.Application.Events.API.ServiceContext >> Found declaration package: " + this._declarationPackage.Name);
            Logger.WriteInfo("Plugin.Application.Events.API.ServiceContext >> Found service model package: " + this._serviceModelPackage.Name);
        }

        /// <summary>
        /// This function returns the class that 'owns' the specified dependent class. Ownership in this context means that the dependent class is a composite
        /// of the owning class (there exists a Composite Association between Owner and Dependant).
        /// </summary>
        /// <param name="dependentClass">The dependent class.</param>
        /// <returns>Owner class or NULL if no owner could be found.</returns>
        internal MEClass FindOwner(MEClass dependentClass)
        {
            this._treeNodeTarget = dependentClass;
            this._treeNodeResult = null;
            this._hierarchy.Traverse(this._hierarchy, FindNode);
            return (this._treeNodeResult != null) ? this._treeNodeResult.Data : null;
        }

        /// <summary>
        /// Retrieve the common schema for the specified interface class.
        /// </summary>
        /// <param name="itf">Interface class.</param>
        /// <returns>Common schema class or NULL if not found.</returns>
        internal MEClass GetCommonSchema (MEClass itf)
        {
            string commonSchemaStereotype = ContextSlt.GetContextSlt().GetConfigProperty(_CommonSchemaClassStereotype);
            if (itf == null) return null;

            foreach (TreeNode<MEClass> itfNode in this._hierarchy.Children)
            {
                if (itfNode.Data == itf)
                {
                    foreach (MEClass operClass in itfNode.ChildObjects)
                    {
                        if (operClass.HasStereotype(commonSchemaStereotype)) return operClass;
                    }
                    break;
                }
            }
            return null;
        }

        /// <summary>
        /// Based on the current context, the function determines the correct type of Service implementation required and creates
        /// an instance of that type, which is subsequently returned to the caller.
        /// </summary>
        /// <returns>Service instance compatible with current context.</returns>
        /// <exception cref="IllegalContextException">Thrown when attempting to create a Service on an invalid context.</exception>
        internal Service GetServiceInstance()
        {
            Service myService = null;
            ContextSlt context = ContextSlt.GetContextSlt();
            if (!Valid) throw new IllegalContextException("Unable to create Service instance since context is invalid!");
            
            switch (this._type)
            {
                // REST Service context...
                case Service.ServiceArchetype.REST:
                    myService = new RESTService(this._hierarchy, context.GetConfigProperty(_ServiceDeclPkgStereotype));
                    break;

                // CodeList Service context...
                case Service.ServiceArchetype.CodeList:
                    myService = new CodeListService(this._serviceClass, context.GetConfigProperty(_CodeListDeclPkgStereotype), _NOBUILDHIERARCHY);
                    break;

                // Either SOAP or Message (the latter using the same Service type as the former)...
                default:
                    myService = new ApplicationService(this._hierarchy, context.GetConfigProperty(_ServiceDeclPkgStereotype));
                    break;
            }
            return myService;
        }

        /// <summary>
        /// Checks whether the specified interface class has an association with any operation that is present in the specified operations list.
        /// </summary>
        /// <param name="itf">Interface class to be checked.</param>
        /// <param name="operationList">List of operations to be checked.</param>
        /// <returns>True if interface has an association with one or more operations from the list.</returns>
        internal bool HasAnyOperation(MEClass itf, List<MEClass> operationList)
        {
            if (itf == null) return false;
            foreach (TreeNode<MEClass> itfNode in this._hierarchy.Children)
            {
                if (itfNode.Data == itf)
                {
                    foreach (MEClass operClass in itfNode.ChildObjects)
                    {
                        if (operationList.Contains(operClass))
                        {
                            Logger.WriteInfo("Plugin.Application.Events.API.ServiceContext.HasAnyOperation >> Operation found!");
                            return true;
                        }
                    }
                    break;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks whether the specified interface class has an association with the specified operation.
        /// </summary>
        /// <param name="itf">Interface class to be checked.</param>
        /// <param name="operation">Operation to be checked.</param>
        /// <returns>True if interface has an association with the specified operation.</returns>
        internal bool HasOperation(MEClass itf, MEClass operation)
        {
            if (itf == null || operation == null) return false;
            foreach (TreeNode<MEClass> itfNode in this._hierarchy.Children)
            {
                if (itfNode.Data == itf)
                {
                    if (itfNode.ChildObjects.Contains(operation))
                    {
                        Logger.WriteInfo("Plugin.Application.Events.API.ServiceContext.HasOperation >> Operation found!");
                        return true;
                    }
                    break;
                }
            }
            Logger.WriteInfo("Plugin.Application.Events.API.ServiceContext.HasOperation >> Operation NOT found!");
            return false;
        }

        /// <summary>
        /// Attempts to lock the current message model. If this fails, we can't continue until the user has solved this.
        /// This function is a very simple pass-through to the actual locking function, which is part of the Model singleton.
        /// When optional parameter 'lockContainer' is set to 'true', the function locks the entire service container instead of 
        /// just the service declaration.
        /// </summary>
        /// <param name="lockContainer">When set to 'true', lock the container package instead of the declaration package.</param>
        /// <returns>True if locked successfully.</returns>
        internal bool LockModel(bool lockContainer = false)
        {
            this._lockContainer = lockContainer;
            return ModelSlt.GetModelSlt().LockModel(lockContainer? this._declarationPackage.Parent: this._declarationPackage);
        }

        /// <summary>
        /// Attempts to lock the release history package. If this fails, we can't continue until the user has solved this.
        /// This function is a very simple pass-through to the actual locking function, which is part of the Model singleton.
        /// </summary>
        /// <returns>True if package is locked successfully.</returns>
        internal bool LockReleaseHistory()
        {
            return ModelSlt.GetModelSlt().LockPackage(ContextSlt.GetContextSlt().GetConfigProperty(_RMReleasePackagePath), true);
        }

        /// <summary>
        /// Refreshes an entire service declaration environment after applying updates, assures that user-view is updated accordingly.
        /// </summary>
        internal void Refresh()
        {
            // We only refresh the Declaration Package, which in turn refreshes all subordinate packages and diagrams.
            Logger.WriteInfo("Plugin.Application.Events.API.ServiceContext.RefreshContext >> Refreshing packages and diagram...");
            this._declarationPackage.RefreshPackage();
        }

        /// <summary>
        /// Unlocks the current message model, ignoring any warnings / errors.
        /// This function is a very simple pass-through to the actual unlocking function, which is part of the Model singleton.
        /// </summary>
        internal void UnlockModel()
        {
            ModelSlt.GetModelSlt().UnlockModel(this._lockContainer? this._declarationPackage.Parent: this._declarationPackage);
        }

        /// <summary>
        /// Unlocks the release history package, ignoring any warnings / errors.
        /// This function is a very simple pass-through to the actual unlocking function, which is part of the Model singleton.
        /// </summary>
        /// <returns>True if package is locked successfully.</returns>
        internal void UnlockReleaseHistory()
        {
            ModelSlt.GetModelSlt().UnlockPackage(ContextSlt.GetContextSlt().GetConfigProperty(_RMReleasePackagePath), true);
        }

        /// <summary>
        /// Helper method that constructs the entire class hierarchy for the current service. On return, the hierarchy is available in
        /// the 'hierarchy' local property.
        /// </summary>
        private void BuildHierarchy()
        {
            Logger.WriteInfo("Plugin.Application.Events.API.ServiceContext.BuildHierarchy >> Building Capability Hierarchy...");
            ModelSlt model = ModelSlt.GetModelSlt();
            ContextSlt context = ContextSlt.GetContextSlt();
            string interfaceContractClassStereotype = context.GetConfigProperty(_InterfaceContractClassStereotype);
            string businessMessageClassStereotype = context.GetConfigProperty(_BusinessMessageClassStereotype);
            string operationClassStereotype = context.GetConfigProperty(_OperationClassStereotype);
            string operationClassRESTStereotype = context.GetConfigProperty(_RESTOperationClassStereotype);
            string commonSchemaStereotype = ContextSlt.GetContextSlt().GetConfigProperty(_CommonSchemaClassStereotype);
            string assocStereotype = ContextSlt.GetContextSlt().GetConfigProperty(_MessageAssociationStereotype);
            string resourceClassStereotype = context.GetConfigProperty(_ResourceClassStereotype);

            this._hierarchy = new TreeNode<MEClass>(this._serviceClass);

            List<MEClass> itfList = model.GetAssociatedClasses(this._serviceClass, assocStereotype);
            foreach (MEClass itfClass in itfList)
            {
                if (!itfClass.HasStereotype(interfaceContractClassStereotype)) continue;
                Logger.WriteInfo("Plugin.Application.Events.API.ServiceContext.BuildHierarchy >> Adding Interface '" + itfClass.Name + "'...");
                TreeNode<MEClass> itfNode = this._hierarchy.AddChild(itfClass);

                List<MEClass> childList = model.GetAssociatedClasses(itfClass, assocStereotype);
                foreach (MEClass childClass in childList)
                {
                    if (childClass.HasStereotype(commonSchemaStereotype))
                    {
                        Logger.WriteInfo("Plugin.Application.Events.API.ServiceContext.BuildHierarchy >> Adding Common Schema...");
                        itfNode.AddChild(childClass);
                    }
                    else if (childClass.HasStereotype(operationClassStereotype))
                    {
                        Logger.WriteInfo("Plugin.Application.Events.API.ServiceContext.BuildHierarchy >> Adding Operation '" +
                                         itfClass.Name + "." + childClass.Name + "'...");
                        TreeNode<MEClass> operNode = itfNode.AddChild(childClass);

                        List<MEClass> msgList = model.GetAssociatedClasses(childClass, assocStereotype);
                        foreach (MEClass msgClass in msgList)
                        {
                            if (!msgClass.HasStereotype(businessMessageClassStereotype)) continue;
                            Logger.WriteInfo("Plugin.Application.Events.API.ServiceContext.BuildHierarchy >> Adding Message '" +
                                             itfClass.Name + "." + childClass.Name + "." + msgClass.Name + "'...");
                            operNode.AddChild(msgClass);
                        }
                    }
                    else if (childClass.HasStereotype(resourceClassStereotype)) ParseResourceTree(childClass, ref itfNode);
                }
            }
        }

        /// <summary>
        /// Each root-level resource can have any number of child resources associated with them. This method recursively parses the flow 
        /// from the provided resource downwards and collects all other resources for that particular path.
        /// We don't check the archetype of resources that are collected 'on the way', since it is assumed that the model is constructed correctly
        /// (e.g. Resource Collections can not be children of Document Resources etc.).
        /// </summary>
        /// <param name="resourceClass">Class associated with the current resource.</param>
        /// <param name="resourceNode">Reference to TreeNode in parent resource that should receive results.</param>
        private void ParseResourceTree (MEClass resourceClass, ref TreeNode<MEClass> resourceNode)
        {
            ModelSlt model = ModelSlt.GetModelSlt();
            ContextSlt context = ContextSlt.GetContextSlt();
            string assocStereotype = context.GetConfigProperty(_MessageAssociationStereotype);
            string capabilityBaseStereotype = context.GetConfigProperty(_ServiceCapabilityClassBaseStereotype);

            Logger.WriteInfo("Plugin.Application.Events.API.ServiceContext.ParseResourceTree >> Parsing resource '" + resourceClass.Name + "' with parent '" + resourceNode.Data.Name + "'...");

            TreeNode<MEClass> node = resourceNode.AddChild(resourceClass);
            List<MEClass> children = model.GetAssociatedClasses(resourceClass, assocStereotype);
            foreach (MEClass child in children)
            {
                if (child.HasStereotype(capabilityBaseStereotype))
                {
                    Logger.WriteInfo("Plugin.Application.Events.API.ServiceContext.ParseResourceTree >> Going to parse sub-tree for resource '" + child.Name + "'...");
                    ParseResourceTree(child, ref node);
                }
            }
        }

        /// <summary>
        /// Helper function that is used to browse the Class hierarchy, looking for specific nodes.
        /// </summary>
        /// <param name="node">Node that is currently being investigated.</param>
        /// <returns>True when done browsing, false when we need to continue.</returns>
        private bool FindNode(TreeNode<MEClass> node)
        {
            bool result = false;
            if (node.Data == this._treeNodeTarget)
            {
                this._treeNodeResult = node;
                result = true;
            }
            return result;
        }
    }
}