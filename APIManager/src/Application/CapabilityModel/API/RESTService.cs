using System;
using System.Collections.Generic;
using Framework.Logging;
using Framework.Model;
using Framework.Context;
using Framework.View;
using Framework.Util;
using Framework.ConfigurationManagement;

namespace Plugin.Application.CapabilityModel.API
{
    internal class RESTService: Service
    {
        // Configuration properties used by this module...
        private const string _ResourceClassStereotype               = "ResourceClassStereotype";
        private const string _InterfaceContractClassStereotype      = "InterfaceContractClassStereotype";
        private const string _CommonSchemaClassStereotype           = "CommonSchemaClassStereotype";
        private const string _DataModelPkgName                      = "DataModelPkgName";
        private const string _DataModelPkgStereotype                = "DataModelPkgStereotype";
        private const string _ServiceArchetypeREST                  = "ServiceArchetypeREST";
        private const string _DataModelPos                          = "DataModelPos";

        private List<RESTResourceCapability> _tagList;              // The list of REST Resources that have tags defined for them.
        private List<RESTResourceCapability> _rootLevelTagList;     // The list of Root-level REST Resources that have tags defined for them.
        private List<RESTResourceCapability> _documentList;         // The list of REST Document resources for this API.

        // Keep track of classes and associations to show in the diagram...
        private List<MEClass> _diagramClassList;
        private List<MEAssociation> _diagramAssocList;

        /// <summary>
        /// Returns the list of Document Resources for this API.
        /// </summary>
        internal List<RESTResourceCapability> DocumentList { get { return this._documentList; } }

        /// <summary>
        /// Returns the list of Resources that have one or more assigned tag names. This list ALWAYS contains all root-level resources first,
        /// followed by 'other' resources.
        /// </summary>
        internal List<RESTResourceCapability> TagList
        {
            get
            {
                List<RESTResourceCapability> allTagResources = new List<RESTResourceCapability>(this._rootLevelTagList);
                allTagResources.AddRange(this._tagList);
                return allTagResources;
            }
        }

        /// <summary>
        /// 'Create new instance' constructor, creates a new API service declaration below the specified container package. 
        /// The name of the service is specified as 'qualifiedServiceName' (which must contain the major version as an extension, 
        /// i.e.: 'Service_V1'. 
        /// The constructor creates all necessary packages, including the initial Service Model. In order, it creates:
        /// 1) The Service declaration package, Service Model package and top-level 'Common' package;
        /// 2) The Service class;
        /// 3) The Common Schema capability;
        /// 4) The Interface capability;
        /// 5) An in initial set of resource collections;
        /// 6) The Service Model diagram with all necessary components.
        /// The declaration package and service class are constructed by the top-level constructor.
        /// </summary>
        /// <param name="containerPackage">The container in which we're creating the resource.</param>
        /// <param name="metaData">Set of user-defined API meta data (see above).</param>
        /// <param name="resources">List of root-level Resource collection declarations.</param>
        /// <param name="declarationStereotype">The stereotype to apply to the created API.</param>
        /// <param name="initialState">Initial operational state to be assigned to the service.</param>
        /// <param name="remoteTicket">The ticket used for creation, ignored when CM is not enabled.</param>
        /// <param name="projectOrderID">Identifier of the project order used for creation, ignored when CM is not enabled.</param>
        internal RESTService(MEPackage containerPackage, 
                             RESTInterfaceCapability.MetaData metaData, 
                             List<RESTResourceDeclaration> resources, 
                             string declarationStereotype,
                             OperationalState initialState,
                             Ticket remoteTicket, string projectOrderID): 
            base(containerPackage, metaData.qualifiedName, declarationStereotype, initialState, remoteTicket, projectOrderID)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            this._tagList = new List<RESTResourceCapability>();
            this._rootLevelTagList = new List<RESTResourceCapability>();
            this._documentList = new List<RESTResourceCapability>();

            try
            {
                // REST API's use a single data model. Here we create the data model package...
                // We try to read the relative package position from configuration and if this failed, use '50' as default value.
                int packagePos;
                if (!int.TryParse(context.GetConfigProperty(_DataModelPos), out packagePos)) packagePos = 50;
                this._serviceDeclPackage.CreatePackage(context.GetConfigProperty(_DataModelPkgName),
                                                       context.GetConfigProperty(_DataModelPkgStereotype), packagePos);

                // Create an interface and a set of top-level resource collections on that interface. The remainder of the REST service
                // structure must be added separately by means of specialized 'edit' dialogues.
                var interfaceCapability = new RESTInterfaceCapability(this, metaData, resources);
                if (!interfaceCapability.Valid)
                {
                    // Oops, something went terribly wrong during construction of the interface. Roll-back and exit!
                    Logger.WriteWarning("Interface creation failed, rolling-back!");
                    this._serviceDeclPackage.Parent.DeletePackage(this._serviceDeclPackage);
                    return;
                }

                // We set the service archetype to 'REST'. 
                this._archetype = ServiceArchetype.REST;
                this._serviceClass.SetTag(context.GetConfigProperty(_ServiceArchetypeTag), EnumConversions<ServiceArchetype>.EnumToString(this._archetype), true);

                string newNames = string.Empty;
                bool isFirst = true;
                foreach (RESTResourceDeclaration resource in resources)
                {
                    string roleName = RESTUtil.GetAssignedRoleName(resource.Name);
                    newNames += isFirst ? roleName : ", " + roleName;
                    isFirst = false;
                }
                this._serviceDeclPackage.Refresh();    // Make sure to update view to users.
                CreateLogEntry("Initial release with resource collection(s): " + newNames);

                // Create the diagram...
                Diagram myDiagram = this._modelPackage.CreateDiagram();
                myDiagram.AddDiagramProperties();
                myDiagram.ShowConnectorStereotypes(false);

                // Collect all classes and associations that must be shown on the diagram...
                this._diagramClassList = new List<MEClass>();
                this._diagramAssocList = new List<MEAssociation>();
                Traverse(DiagramItemsCollector);                        // Performs the actual data collection.
                myDiagram.AddClassList(this._diagramClassList);
                myDiagram.AddAssociationList(this._diagramAssocList);
                Paint(myDiagram);
                myDiagram.Show();
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.APIProcessor.RESTService >> Service creation failed because:" + 
                                  Environment.NewLine + exc.ToString());
            }
        }

        /// <summary>
        /// Constructor used to create a service hierarchy based on an existing structure. The structure must have been collected
        /// earlier (e.g. by instantiating a 'ServiceContext' object) and contains all classes from the Service downwards (Service ->
        /// Interfaces -> CommonSchema + ResourceCollection -> Paths/Operations/ResourceCollections).
        /// </summary>
        /// <param name="hierarchy">Tree structure containing al MEClass elements that participate in this service hierarchy.</param>
        /// <param name="declarationStereotype"></param>
        internal RESTService(TreeNode<MEClass> hierarchy, string declarationStereotype) : base(hierarchy.Data, declarationStereotype)
        {
            try
            {
                ContextSlt context = ContextSlt.GetContextSlt();
                this._tagList = new List<RESTResourceCapability>();
                this._rootLevelTagList = new List<RESTResourceCapability>();
                this._documentList = new List<RESTResourceCapability>();
                string archetypeStr = this._serviceClass.GetTag(context.GetConfigProperty(_ServiceArchetypeTag));
                if (string.IsNullOrEmpty(archetypeStr))
                {
                    // If the service does not yet possesses a proper archetype tag, we'll add it...
                    this._archetype = ServiceArchetype.REST;
                    this._serviceClass.SetTag(context.GetConfigProperty(_ServiceArchetypeTag),
                                              EnumConversions<ServiceArchetype>.EnumToString(this._archetype), true);
                }
                else this._archetype = EnumConversions<ServiceArchetype>.StringToEnum(archetypeStr);

                foreach (TreeNode<MEClass> node in hierarchy.Children) AddCapability(new RESTInterfaceCapability(this, node));
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.APIProcessor.RESTService (existing) >> Error creating capability structure because: " + exc.ToString());
                this._serviceClass = null;   // Assures that instance is declared invalid.
            }
        }

        /// <summary>
        /// Searches the list of registered Document resource for a resource with the given name. If found, associated
        /// capability is removed from the list. If not found, the method fails silently.
        /// </summary>
        /// <param name="name">Name of resource to be removed.</param>
        internal void DeleteDocumentResource(string name)
        {
            for (int i=0; i<this._documentList.Count; i++)
            {
                if (this._documentList[i].Name == name)
                {
                    this._documentList.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// Searches the list of registered Document resource for a resource with the given name. If found, the capability is returned.
        /// </summary>
        /// <param name="name">Name to be found.</param>
        /// <returns>Document resource with given name or NULL if not found.</returns>
        internal RESTResourceCapability FindDocumentResource(string name)
        {
            foreach (RESTResourceCapability cap in this._documentList)
            {
                if (cap.Name == name) return cap;
            }
            return null;
        }

        /// <summary>
        /// Registers the provided resource as an element of the document list. We register the resource only if there is not yet a 
        /// similarly named resource in the list.
        /// </summary>
        /// <param name="documentResource">The resource to be registered.</param>
        internal void RegisterDocument(RESTResourceCapability documentResource)
        {
            if (!this._documentList.Contains(documentResource)) this._documentList.Add(documentResource);
        }

        /// <summary>
        /// Registers the provided resource as an element of the tag list. This implies that metadata of the resource will be used to construct
        /// tag entries in an OpenAPI interface specification. We register the resource only if there is not yet a similarly named resource in the list.
        /// The list actually consists of two separate lists: one for root-level resources holding tags and one for all other resources.
        /// </summary>
        /// <param name="resource">The resource to be registered.</param>
        internal void RegisterTag(RESTResourceCapability resource)
        {
            if (resource.IsRootLevel && !this._rootLevelTagList.Contains(resource)) this._rootLevelTagList.Add(resource);
            else if (!this._tagList.Contains(resource)) this._tagList.Add(resource);
        }

        /// <summary>
        /// Process the service (i.e. generate output according to provided processor.).
        /// </summary>
        /// <param name="processor">The Capability processor to use.</param>
        /// <param name="stage">The processing stage we're currently in, passed verbatim to processor.</param>
        /// <returns>True when processing can commence, false on errors.</returns>
        protected override bool HandleCapability(CapabilityProcessor processor, ProcessingStage stage)
        {
            // Since all the actual work is being performed by the processor, simply pass information onwards...
            return processor.ProcessService(this, stage);
        }

        /// <summary>
        /// Helper function that is invoked by the capability hierarchy traversal for each node in the hierarchy, starting at the Service
        /// and subsequently invoked for each subordinate capability (CmnSchema, Interface, Resource Collection). 
        /// The function collects items that must be displayed on the initial ServiceModel diagram.
        /// </summary>
        /// <param name="svc">My parent service.</param>
        /// <param name="cap">The current Capability, or NULL when invoked at Service level (very first call).</param>
        /// <returns>Always 'false', which indicates that traversal must continue until all nodes are processed.</returns>
        private bool DiagramItemsCollector(Service svc, Capability cap)
        {
            if (cap == null)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.APIProcessor.RESTService.DiagramItemsCollector >> Traversing service '" + svc.Name + "'...");
                // First invocation is for the service itself... 
                this._diagramClassList.Add(this._serviceClass);

                // Starting at service level, follow all Message associations, which yield all created Interface capabilities.
                foreach (MEAssociation assoc in ServiceClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation)) this._diagramAssocList.Add(assoc);
            }
            else
            {
                // Subsequent invocations are for the Common Schema(s), Interfaces and resource collections (in unspecified order)...
                Logger.WriteInfo("Plugin.Application.CapabilityModel.APIProcessor.RESTService.DiagramItemsCollector >> Traversing capability '" + cap.Name + "'...");
                ContextSlt context = ContextSlt.GetContextSlt();
                if (cap.CapabilityClass.HasStereotype(context.GetConfigProperty(_ResourceClassStereotype)) ||
                    cap.CapabilityClass.HasStereotype(context.GetConfigProperty(_InterfaceContractClassStereotype)) ||
                    cap.CapabilityClass.HasStereotype(context.GetConfigProperty(_CommonSchemaClassStereotype)))
                {
                    this._diagramClassList.Add(cap.CapabilityClass);
                    foreach (MEAssociation assoc in cap.CapabilityClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation)) this._diagramAssocList.Add(assoc);
                }
            }
            return false;
        }
    }
}