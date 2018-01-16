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
    class EditRESTOperationEvent : EventImplementation
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
        /// The Operation.Edit operation can only be executed on Operation capabilities. Since these are uniquely identified
        /// by their stereotypes, there is no need to perform further checks. 
        /// </summary>
        /// <returns>True.</returns>
        internal override bool IsValidState() { return true; }

        /// <summary>
        /// Processes the 'Edit Operations' menu click event.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.API.EditRESTOperationEvent.HandleEvent >> Processing an add REST Operation menu click...");

            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            var svcContext = new ServiceContext(this._event.Scope == TreeScope.Diagram);
            MEClass operationClass = svcContext.OperationClass;

            if (!svcContext.Valid || svcContext.MyDiagram == null || operationClass == null)
            {
                Logger.WriteError("Plugin.Application.Events.API.EditRESTOperationEvent.HandleEvent >> Illegal or corrupt context, event aborted!");
                return;
            }
            else if (svcContext.Type != ServiceContext.ServiceType.REST)
            {
                Logger.WriteWarning("Plugin.Application.Events.API.EditRESTOperationEvent.HandleEvent >> Operation only suitable for REST Services!");
                return;
            }

            // By instantiating the service, we should construct the entire capability hierarchy, which facilitates constructing
            // of 'lower level' capabilities using their Class objects...
            var myService = new RESTService(svcContext.Hierarchy, context.GetConfigProperty(_ServiceDeclPkgStereotype));
            RESTOperationCapability myOperation = new RESTOperationCapability(operationClass);

            var operationDecl = new RESTOperationDeclaration(myOperation);
            using (var dialog = new RESTOperationDialog(operationDecl))
            {
                if (svcContext.LockModel() && dialog.ShowDialog() == DialogResult.OK)
                {
                    bool result = myOperation.Edit(dialog.Operation, dialog.MinorVersionIndicator);
                    if (result)
                    {
                        // Collect the (new) classes and associations that must be shown on the diagram...
                        this._diagramClassList = new List<MEClass>();
                        this._diagramAssocList = new List<MEAssociation>();
                        myOperation.Traverse(DiagramItemsCollector);

                        // This updates the selected diagram, which could be in a resource package...
                        svcContext.MyDiagram.AddClassList(this._diagramClassList);
                        svcContext.MyDiagram.AddAssociationList(this._diagramAssocList);
                        svcContext.MyDiagram.Redraw();
                        svcContext.DeclarationPackage.Refresh();
                        MessageBox.Show("Operation has been updated successfully.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else MessageBox.Show("Failed to update Operation!", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                Logger.WriteInfo("Plugin.Application.Events.API.EditResourcesEvent.DiagramItemsCollector >> Traversing capability '" + cap.Name + "'...");
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
