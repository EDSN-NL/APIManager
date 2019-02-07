using System;
using System.Collections.Generic;
using Framework.Logging;
using Framework.Model;
using Framework.Context;
using Framework.View;
using Framework.Util;
using Plugin.Application.Forms;

namespace Plugin.Application.CapabilityModel.CodeList
{
    /// <summary>
    /// Defines the service implementation for CodeList Services.
    /// </summary>
    internal class CodeListService: Service
    {
        // Configuration properties used by this module...
        private const string _CodeListPkgName           = "CodeListPkgName";
        private const string _CodeListPkgStereotype     = "CodeListPkgStereotype";
        private const string _CodeListClassStereotype   = "CodeListClassStereotype";

        // Keep track of classes and associations to show in the diagram...
        private List<MEClass> _diagramClassList;
        private List<MEAssociation> _diagramAssocList;

        /// <summary>
        /// This is the 'create new CodeList Service' constructor, which is invoked from the corresponding 'New CodeList Service declaration' event.
        /// The constructor creates all required packages, elements and associations and finally creates and shows the ServiceModel diagram.
        /// In case of errors, all work is rolled-back by deleting the declaration package together with all contained elements.
        /// </summary>
        internal CodeListService(MEPackage containerPackage, 
                                 string qualifiedServiceName, 
                                 SortedList<string, CodeListDirector.DirectorContext> codeLists, 
                                 string declarationStereotype,
                                 OperationalState initialState = _DefaultOperationalState): 
            base(containerPackage, qualifiedServiceName, declarationStereotype, initialState, null, null)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();

            // Create a new package for the code types...
            MEPackage codeTypePackage = this._serviceDeclPackage.CreatePackage(context.GetConfigProperty(_CodeListPkgName),
                                                                               context.GetConfigProperty(_CodeListPkgStereotype), 20);
            // We set the service archetype to 'CodeList'. 
            this._archetype = ServiceArchetype.CodeList;
            this._serviceClass.SetTag(context.GetConfigProperty(_ServiceArchetypeTag), EnumConversions<ServiceArchetype>.EnumToString(this._archetype));

            string newNames = string.Empty;
            bool isFirst = true; // Little trick to get the right amount of ',' separators.
            foreach (CodeListDirector.DirectorContext ctx in codeLists.Values)
            {
                if (ctx.CompletedIndicator)  // We only process the results that have actually been completed by the user...
                {
                    var codeListCapability = new CodeListCapability(this, ctx.name, ctx.sourceEnum, ctx.agencyName, ctx.agencyID, ctx.selectedAttribs);
                    if (codeListCapability.Valid)
                    {
                        newNames += (isFirst) ? ctx.name : ", " + ctx.name;
                        isFirst = false;
                    }
                    else
                    {
                        // Fatal CodeList creation error, roll-back and abort.
                        Logger.WriteError("Plugin.Application.CapabilityModel.CodeList.CodeListService >> Fatal error creating CodeList '" + ServiceClass.Name + ":" + ctx.name + "' Rolling back!");
                        this._serviceClass = null;                                      // Set 'Service' to invalid state.
                        containerPackage.DeletePackage(this._serviceDeclPackage);       // Should delete everything else as well!
                        return;
                    }
                }
            }
            this._serviceDeclPackage.Refresh();    // Make sure to update view to users.
            CreateLogEntry("Initial release with CodeList(s): " + newNames);

            // Create the diagram...
            Diagram myDiagram = this._modelPackage.CreateDiagram();
            myDiagram.AddDiagramProperties();
            myDiagram.ShowConnectorStereotypes(false);

            // Collect all classes and associations that must be displayed and subsequently show on diagram...
            this._diagramClassList = new List<MEClass>();
            this._diagramAssocList = new List<MEAssociation>();
            Traverse(DiagramItemsCollector);                        // Performs the actual data collection.
            myDiagram.AddClassList(this._diagramClassList);
            myDiagram.AddAssociationList(this._diagramAssocList);
            Paint(myDiagram);
            myDiagram.Show();
        }

        /// <summary>
        /// Constructor to be invoked when linking to an existing capability structure. The underlying capabilitystructure is build ONLY if the
        /// 'buildHierarchy' indicator is set to 'true'. Otherwise, we create only the service and no children.
        /// </summary>
        /// <param name="serviceClass">The class that represents the CodeList service.</param>
        /// <param name="declarationStereotype">Declaration package stereotype (defines the 'type' of service).</param>
        /// <param name="buildHierarchy">When set to 'true', the constructor creates the entire capability hierarchy instead of just the service.</param>
        internal CodeListService(MEClass serviceClass, string declarationStereotype, bool buildHierarchy = true) : base(serviceClass, declarationStereotype)
        {
            this._diagramAssocList = null;
            this._diagramClassList = null;

            if (buildHierarchy)
            {
                try
                {
                    ContextSlt context = ContextSlt.GetContextSlt();
                    string codeListStereotype = context.GetConfigProperty(_CodeListClassStereotype);
                    string archetypeStr = this._serviceClass.GetTag(context.GetConfigProperty(_ServiceArchetypeTag));
                    if (string.IsNullOrEmpty(archetypeStr))
                    {
                        // If the service does not yet possesses a proper archetype tag, we'll add it...
                        this._archetype = ServiceArchetype.CodeList;
                        this._serviceClass.SetTag(context.GetConfigProperty(_ServiceArchetypeTag), 
                                                  EnumConversions<ServiceArchetype>.EnumToString(ServiceArchetype.CodeList), true);
                    }
                    else this._archetype = EnumConversions<ServiceArchetype>.StringToEnum(archetypeStr);

                    foreach (MEAssociation association in serviceClass.TypedAssociations(MEAssociation.AssociationType.Composition))
                    {
                        // Make sure to add only the correct types of capability (just in case the class has multiple types of associations)...
                        if (association.Destination.EndPoint.HasStereotype(codeListStereotype))
                        {
                            // Constructor registers the capability with the service!
                            var cap = new CodeListCapability(this, association.Destination.EndPoint);
                            if (!cap.Valid)
                            {
                                Logger.WriteError("Plugin.Application.CapabilityModel.CodeList.CodeListService (existing) >> Error creating capability '" + association.Destination.EndPoint.Name + "'!");
                                this._serviceClass = null;
                                return;
                            }
                        }
                    }
                }
                catch (Exception exc)
                {
                    Logger.WriteError("Plugin.Application.CapabilityModel.CodeList.CodeListService (existing) >> Error creating capability structure because: " + exc.ToString());
                    this._serviceClass = null;   // Assures that instance is declared invalid.
                }
            }
        }

        /// <summary>
        /// This method collects all CodeList capabilities that are associated with the service. It must be used as a pre-processing stage
        /// before we can bulk-process all capabilities (in case we used a service constructor with 'buildHierarchy = false')!
        /// We clear the list of capabilities before constructing the tree so it's safe to call this multiple times (although a waste
        /// of resources).
        /// The method will skip any capabilities that fail construction (with an error message).
        /// Since CodeList constructors perform the necessary registration, we only have to build a set of CodeList capability instances.
        /// </summary>
        internal void BuildCapabilityTree()
        {
            try
            {
                this._serviceCapabilities = new List<Capability>();     //Will remove all existing capabilities (if present).
                string codeListStereotype = ContextSlt.GetContextSlt().GetConfigProperty(_CodeListClassStereotype);
                foreach (MEAssociation association in ServiceClass.TypedAssociations(MEAssociation.AssociationType.Composition))
                {
                    // Make sure to add only the correct types of capability (just in case the class has multiple types of associations)...
                    if (association.Destination.EndPoint.HasStereotype(codeListStereotype))
                    {
                        // Constructor registers the capability with the service!
                        var cap = new CodeListCapability(this, association.Destination.EndPoint);
                        if (!cap.Valid)
                        {
                            Logger.WriteError("Plugin.Application.CapabilityModel.CodeList.CodeListService (existing) >> Error creating capability '" + association.Destination.EndPoint.Name + "'!");
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.CodeListService (existing) >> Error creating capability structure because: " + exc.ToString());
                this._serviceClass = null;   // Assures that instance is declared invalid.
            }
        }

        /// <summary>
        /// Process the service (i.e. generate output according to provided processor.).
        /// </summary>
        /// <param name="stage">The processing stage we're currently in, passed verbatim to processor.</param>
        /// <returns>True when processing can commence, false on errors.</returns>
        protected override bool HandleCapability(CapabilityProcessor processor, ProcessingStage stage)
        {
            // Since all the actual work is being performed by the processor, simply pass information onwards...
            return processor.ProcessService(this, stage);
        }

        /// <summary>
        /// Helper function that is invoked by the capability hierarchy traversal for each node in the hierarchy, starting at the Service
        /// and subsequently invoked for each CodeList. The function collects items that must be displayed on the initial ServiceModel diagram.
        /// </summary>
        /// <param name="svc">My parent service.</param>
        /// <param name="cap">The current Capability, or NULL when invoked at Service level (very first call).</param>
        /// <returns>Always 'false', which indicates that traversal must continue until all nodes are processed.</returns>
        private bool DiagramItemsCollector(Service svc, Capability cap)
        {
            // Flags indicating what to show on the diagram apart from the Service and the CodeLists...
            ContextSlt context = ContextSlt.GetContextSlt();
            bool addSourceEnums = context.GetBoolSetting(FrameworkSettings._CLAddSourceEnumsToDiagram);
            bool addCodeTypes = context.GetBoolSetting(FrameworkSettings._CLAddCodeTypesToDiagram);

            if (cap == null)
            {
                // First invocation is for the service itself... 
                this._diagramClassList.Add(this._serviceClass);
                // Starting at service level, follow all composition associations, which yield all created CodeList elements.
                // For each CodeList, we also want to collect the constructed Types and the source enumerations...
                foreach (MEAssociation assoc in this._serviceClass.TypedAssociations(MEAssociation.AssociationType.Composition))
                {
                    MEClass currClass = assoc.Destination.EndPoint;
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeList.CodeListService.diagramItemsCollector >> Adding: " + currClass.Name);
                    this._diagramAssocList.Add(assoc);
                    this._diagramClassList.Add(currClass);
                }
            }
            else
            {
                // Subsequent invocations are for each CodeList capabilities...
                foreach (MEAssociation assoc in cap.CapabilityClass.AssociationList)
                {
                    if ((assoc.TypeOfAssociation == MEAssociation.AssociationType.Composition && addCodeTypes) ||
                        (assoc.TypeOfAssociation == MEAssociation.AssociationType.Usage && addSourceEnums))
                    {
                        MEClass subClass = assoc.Destination.EndPoint;
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeList.CodeListService.diagramItemsCollector >> Adding child: " + subClass.Name);
                        this._diagramAssocList.Add(assoc);
                        this._diagramClassList.Add(assoc.Destination.EndPoint);
                    }
                }
            }
            return false;
        }
    }
}
