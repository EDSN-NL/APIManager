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
    internal class ApplicationService: Service
    {
        // Private configuration properties used by this service...
        private const string _CommonDefnPos     = "CommonDefnPos";
        private const string _UseSOAPFaultsTag  = "UseSOAPFaultsTag";

        // Keep track of classes and associations to show in the diagram...
        private List<MEClass> _diagramClassList;
        private List<MEAssociation> _diagramAssocList;
        private bool _useSOAPFaults;

        /// <summary>
        /// Returns an indicator stating whether the generates WSDL should include a generic SOAP Fault construct or not.
        /// </summary>
        internal bool UseSOAPFaults { get { return this._useSOAPFaults; } }

        /// <summary>
        /// 'Create new instance' constructor, creates a new API service declaration underneath the specified container package. 
        /// The name of the service is specified as 'qualifiedServiceName' (which must contain the major version as an extension, 
        /// i.e.: 'Service_V1'. 
        /// The constructor creates all necessary packages, including the initial Service Model. In order, it creates:
        /// 1) The Service declaration package, Service Model package and top-level 'Common' package;
        /// 2) The Service class;
        /// 3) The Common Schema capability;
        /// 4) The Interface capability;
        /// 5) For each specified operation name, an Operation capability (which in turn creates the associated Message Capabilities);
        /// 6) The Service Model diagram with all necessary components.
        /// The Service declaration package and Service class are created by the parent constructor.
        /// </summary>
        /// <param name="containerPackage">Name of the container that will hold the service declaration.</param>
        /// <param name="declarationStereotype">Stereotype to be used for the service declaration package.</param>
        /// <param name="initialState">Operational state in which the service will be created.</param>
        /// <param name="useSOAPFaults">Indicates, when true, that the service must support SOAP Faults (SOAP Service only).</param>
        /// <param name="useListElements">Indicates, when true, that additional List elements must be inserted for all sub-elements
        /// that have a cardinality greater then 1.</param>
        /// <param name="operationNames">List of initial operation names for the service.</param>
        /// <param name="qualifiedServiceName">Qualified name of the service (includes major version).</param>
        /// <param name="remoteTicket">Ticket used for creation, ignored when CM is not active.</param>
        /// <param name="projectOrderID">Id of the project order used for creation, ignored when CM is not active.</param>
        internal ApplicationService(MEPackage containerPackage, 
                                    string qualifiedServiceName, 
                                    List<string> operationNames, 
                                    string declarationStereotype, 
                                    OperationalState initialState,
                                    bool useSOAPFaults, bool useListElements,
                                    Ticket remoteTicket, string projectOrderID): 
            base(containerPackage, qualifiedServiceName, declarationStereotype, initialState, 
                 useListElements, remoteTicket, projectOrderID)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            this._useSOAPFaults = useSOAPFaults;

            // Create a new package for the top-level 'common' definition...
            // We try to read the relative package position from configuration and if this failed, use '50' as default value.
            int packagePos;
            if (!int.TryParse(context.GetConfigProperty(_CommonDefnPos), out packagePos)) packagePos = 50;
            MEPackage commonPackage = this._serviceDeclPackage.CreatePackage(context.GetConfigProperty(_CommonPkgName),
                                                                             context.GetConfigProperty(_CommonPkgStereotype), packagePos);

            // Create an interface and a set of operations on that interface. The operations in turn should 
            // create the remainder of the package structure with all contained classes and associations...
            var interfaceCapability = new InterfaceCapability(this, Name, operationNames);
            if (!interfaceCapability.Valid)
            {
                // Oops, something went terribly wrong during construction of the interface. Roll-back and exit!
                Logger.WriteWarning("Interface creation failed, rolling-back!");
                containerPackage.DeletePackage(this._serviceDeclPackage);
                return;
            }

            // We set the service archetype to 'SOAP' and load the 'useSOAPFaults' flag...
            this._archetype = ServiceArchetype.SOAP;
            this._serviceClass.SetTag(context.GetConfigProperty(_ServiceArchetypeTag), EnumConversions<ServiceArchetype>.EnumToString(this._archetype));
            this._serviceClass.SetTag(context.GetConfigProperty(_UseSOAPFaultsTag), this._useSOAPFaults ? "true" : "false");

            string newNames = string.Empty;
            bool isFirst = true;
            foreach (string operationName in operationNames)
            {
                string roleName = Conversions.ToCamelCase(operationName);
                newNames += isFirst ? roleName : ", " + roleName;
                isFirst = false;
            }
            this._serviceDeclPackage.Refresh();    // Make sure to update view to users.
            CreateLogEntry("Initial release with operation(s): " + newNames);

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

        /// <summary>
        /// Constructor used to create a service hierarchy based on an existing structure. The structure must have been collected
        /// earlier (e.g. by instantiating a 'ServiceContext' object) and contains all classes from the Service downwards (Service ->
        /// Interfaces -> CommonSchema + Operations -> Messages).
        /// </summary>
        /// <param name="hierarchy">Tree structure containing al MEClass elements that participate in this service hierarchy.</param>
        /// <param name="declarationStereotype"></param>
        internal ApplicationService(TreeNode<MEClass> hierarchy, string declarationStereotype) : base(hierarchy.Data, declarationStereotype)
        {
            try
            {
                ContextSlt context = ContextSlt.GetContextSlt();
                string archetypeStr = this._serviceClass.GetTag(context.GetConfigProperty(_ServiceArchetypeTag));
                if (string.IsNullOrEmpty(archetypeStr))
                {
                    // If the service does not yet possesses a proper archetype tag, we'll add it...
                    this._archetype = ServiceArchetype.SOAP;
                    this._serviceClass.SetTag(context.GetConfigProperty(_ServiceArchetypeTag),
                                              EnumConversions<ServiceArchetype>.EnumToString(this._archetype), true);
                }
                else this._archetype = EnumConversions<ServiceArchetype>.StringToEnum(archetypeStr);

                string useSOAPFaultsStr = this._serviceClass.GetTag(context.GetConfigProperty(_UseSOAPFaultsTag));
                if (string.IsNullOrEmpty(useSOAPFaultsStr))
                {
                    // If the service does not yet possesses a proper SOAP Faults tag, we'll add it...
                    this._useSOAPFaults = false;
                    this._serviceClass.SetTag(context.GetConfigProperty(_UseSOAPFaultsTag), "false", true);
                }
                else this._useSOAPFaults = string.Compare(useSOAPFaultsStr, "true", true) == 0 ? true : false;

                foreach (TreeNode<MEClass> node in hierarchy.Children) AddCapability(new InterfaceCapability(this, node));
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.APIProcessor.ApplicationService (existing) >> Error creating capability structure because: " + exc.ToString());
                this._serviceClass = null;   // Assures that instance is declared invalid.
            }
        }

        /// <summary>
        /// Removes an Interface from this Service. Removal is only allowed when there are multiple Interfaces defined for the service.
        /// If there are any 'orphaned' operations after removing the service, these will be removed as well.
        /// Note that the provided InterfaceCapability object is INVALID on return from this method (unlesss we refused to delete the Interface)!
        /// </summary>
        /// <param name="thisInterface">Interface to be deleted.</param>
        /// <param name="newMinorVersion">Set to 'true' to bump minor version on success.</param>
        /// <returns>True when the Interface has been removed, False when this is the ONLY interface and we're not allowed to remove it.</returns>
        internal bool DeleteInterface(InterfaceCapability thisInterface, bool newMinorVersion)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.APIProcessor.ApplicationService.DeleteInterface >> Removing interface '" + thisInterface.Name + "'...");
            bool result = false;

            if (this._serviceCapabilities.Count > 1)
            {
                thisInterface.Delete(); // Performs the actual delete operation of the Interface and optionally, Operations assigned solely to that Interface.
                if (newMinorVersion) IncrementVersion();
                CreateLogEntry("Deleted Interface: '" + thisInterface.Name + "'.");
                result = true;
            }
            Logger.WriteInfo("Plugin.Application.CapabilityModel.APIProcessor.ApplicationService.DeleteInterface >> Result of operation: " + result);
            return result;
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
        /// and subsequently invoked for each subordinate capability (CmnSchema, Interface, Operation and Message). 
        /// The function collects items that must be displayed on the initial ServiceModel diagram.
        /// </summary>
        /// <param name="svc">My parent service.</param>
        /// <param name="cap">The current Capability, or NULL when invoked at Service level (very first call).</param>
        /// <returns>Always 'false', which indicates that traversal must continue until all nodes are processed.</returns>
        private bool DiagramItemsCollector(Service svc, Capability cap)
        {
            if (cap == null)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.APIProcessor.ApplicationService.DiagramItemsCollector >> Traversing service '" + svc.Name + "'...");
                // First invocation is for the service itself... 
                this._diagramClassList.Add(this._serviceClass);

                // Starting at service level, follow all Message associations, which yield all created Interface capabilities.
                foreach (MEAssociation assoc in ServiceClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation)) this._diagramAssocList.Add(assoc);
            }
            else
            {
                // Subsequent invocations are for the Common Schema(s), Interfaces, Operations and Messages (in unspecified order)...
                Logger.WriteInfo("Plugin.Application.CapabilityModel.APIProcessor.ApplicationService.DiagramItemsCollector >> Traversing capability '" + cap.Name + "'...");
                if (cap is MessageCapability)
                {
                    if (ContextSlt.GetContextSlt().GetBoolSetting(FrameworkSettings._SMAddBusinessMsgToDiagram))
                    {
                        this._diagramClassList.Add(cap.CapabilityClass);
                        // We're at the message level, we might (optionally) collect the Message Assembly component(s)...
                        if (ContextSlt.GetContextSlt().GetBoolSetting(FrameworkSettings._SMAddMessageAssemblyToDiagram))
                        {
                            foreach (MEAssociation assoc in cap.CapabilityClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation))
                            {
                                this._diagramAssocList.Add(assoc);
                                this._diagramClassList.Add(assoc.Destination.EndPoint);
                            }
                        }
                    }
                }
                else
                {
                    // In all other cases, we simply collect child associations if available...
                    this._diagramClassList.Add(cap.CapabilityClass);
                    foreach (MEAssociation assoc in cap.CapabilityClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation)) this._diagramAssocList.Add(assoc);
                }
            }
            return false;
        }
    }
}
