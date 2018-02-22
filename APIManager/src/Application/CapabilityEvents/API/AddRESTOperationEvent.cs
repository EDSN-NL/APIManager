using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Event;
using Framework.Logging;
using Framework.Model;
using Framework.Context;
using Framework.Util;
using Plugin.Application.CapabilityModel;
using Plugin.Application.CapabilityModel.API;
using Plugin.Application.Forms;

namespace Plugin.Application.Events.API
{
    class AddRESTOperationEvent : EventImplementation
    {
        // Configuration properties used by this module...
        private const string _ResourceClassStereotype               = "ResourceClassStereotype";
        private const string _ServiceModelPkgName                   = "ServiceModelPkgName";
        private const string _ServiceDeclPkgStereotype              = "ServiceDeclPkgStereotype";
        private const string _ServiceCapabilityClassBaseStereotype  = "ServiceCapabilityClassBaseStereotype";
        private const string _ArchetypeTag                          = "ArchetypeTag";
        private const string _MessageAssemblyClassStereotype        = "MessageAssemblyClassStereotype";

        // Keep track of (extra) classes and associations to show in the diagram...
        private List<MEClass> _diagramClassList = new List<MEClass>();
        private List<MEAssociation> _diagramAssocList = new List<MEAssociation>();

        /// <summary>
        /// Operations can be added to resources, but only if the resource is NOT a Document.
        /// The default event context checker has verified the Stereotype of the selected class to be a Resource.
        /// We have to check whether we have the correct archetype...
        /// </summary>
        /// <returns>True on correct context, false otherwise.</returns>
        internal override bool IsValidState()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            MEClass currentClass = context.CurrentClass;
            try
            {
                if (currentClass != null && currentClass.HasStereotype(context.GetConfigProperty(_ResourceClassStereotype)))
                {
                    string typeTagValue = currentClass.GetTag(context.GetConfigProperty(_ArchetypeTag));
                    if (!string.IsNullOrEmpty(typeTagValue))
                    {
                        var type = EnumConversions<RESTResourceCapability.ResourceArchetype>.StringToEnum(typeTagValue);
                        return type != RESTResourceCapability.ResourceArchetype.Document;
                    }
                }
            }
            catch
            {
                // We could get exceptions if the context is wrong and we get the wrong enumeration. Ignore all those!
            }
            return false;
        }

        /// <summary>
        /// Processes the 'Add Operations' menu click event.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.API.AddRESTOperationEvent.HandleEvent >> Processing an add REST Operation menu click...");

            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            var svcContext = new ServiceContext(this._event.Scope == TreeScope.Diagram);
            MEClass operationParent = svcContext.ResourceClass;

            if (!svcContext.Valid || svcContext.MyDiagram == null || operationParent == null)
            {
                Logger.WriteError("Plugin.Application.Events.API.AddRESTOperationEvent.HandleEvent >> Illegal or corrupt context, event aborted!");
                return;
            }
            else if (svcContext.Type != ServiceContext.ServiceType.REST)
            {
                Logger.WriteWarning("Plugin.Application.Events.API.AddRESTOperationEvent.HandleEvent >> Operation only suitable for REST Services!");
                return;
            }

            // Check what type of diagram has been selected, must be the owner of the resource...
            if (svcContext.MyDiagram.OwningPackage != operationParent.OwningPackage)
            {
                MessageBox.Show("Operations can only be added from the diagram of the owning resource!",
                                "Wrong Diagram", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // By instantiating the service, we should construct the entire capability hierarchy, which facilitates constructing
            // of 'lower level' capabilities using their Class objects...
            var myService = new RESTService(svcContext.Hierarchy, context.GetConfigProperty(_ServiceDeclPkgStereotype));
            var myResource = new RESTResourceCapability(operationParent);
            var newOperationDecl = new RESTOperationDeclaration(myResource, string.Empty, new HTTPOperation(HTTPOperation.Type.Unknown));

            using (var dialog = new RESTOperationDialog(newOperationDecl, new RESTResourceDeclaration(myResource)))
            {
                if (svcContext.LockModel() && dialog.ShowDialog() == DialogResult.OK)
                {
                    bool result = myResource.AddOperation(dialog.Operation, dialog.MinorVersionIndicator);
                    if (result)
                    {
                        // Collect the new classes and associations that must be shown on the diagram...
                        this._diagramClassList = new List<MEClass>();
                        this._diagramAssocList = new List<MEAssociation>();
                        myResource.Traverse(DiagramItemsCollector);

                        // This updates the selected diagram, which could be in a resource package...
                        svcContext.MyDiagram.AddClassList(this._diagramClassList);
                        svcContext.MyDiagram.AddAssociationList(this._diagramAssocList);
                        svcContext.MyDiagram.Redraw();
                        svcContext.DeclarationPackage.Refresh();
                        MessageBox.Show("Operation has been added successfully.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else MessageBox.Show("Failed to add Operation!", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                svcContext.UnlockModel();
            }
        }

        /// <summary>
        /// Helper function that is invoked by the capability hierarchy traversal for each node in the hierarchy, starting at the Resource
        /// and subsequently invoked for each subordinate capability (Operation). 
        /// The function collects items that must be displayed on the updated ServiceModel diagram. It simply collects ALL classes and associations,
        /// irrespective whether they were already on the diagram before. Superfluous elements are properly handled by the View code, so this is
        /// not an issue and makes the code at this level a lot simpler.
        /// </summary>
        /// <param name="svc">My parent service, we ignore this here.</param>
        /// <param name="cap">The current Capability.</param>
        /// <returns>Always 'false', which indicates that traversal must continue until all nodes are processed.</returns>
        private bool DiagramItemsCollector(Service svc, Capability cap)
        {
            if (cap != null) // Safety catch, must not be NULL since we start at capability level.   
            {
                ContextSlt context = ContextSlt.GetContextSlt();
                string capabilityBaseStereotype = context.GetConfigProperty(_ServiceCapabilityClassBaseStereotype);
                string messageAssemblyStereotype = context.GetConfigProperty(_MessageAssemblyClassStereotype);
                bool mustShowMsgAssembly = context.GetBoolSetting(FrameworkSettings._SMAddMessageAssemblyToDiagram);
                Logger.WriteInfo("Plugin.Application.Events.API.AddResourcesEvent.DiagramItemsCollector >> Traversing capability '" + cap.Name + "'...");
                this._diagramClassList.Add(cap.CapabilityClass);
                foreach (MEAssociation assoc in cap.CapabilityClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation))
                {
                    this._diagramAssocList.Add(assoc);
                    // If the endpoint of the association is a Message Assembly, we MIGHT have to add it to the diagram manually...
                    if (assoc.Destination.EndPoint.HasStereotype(messageAssemblyStereotype) && mustShowMsgAssembly)
                    {
                        this._diagramClassList.Add(assoc.Destination.EndPoint);
                    }
                }
            }
            return false;
        }
    }
}
