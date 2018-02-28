using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Event;
using Framework.Logging;
using Framework.Model;
using Framework.Context;
using Plugin.Application.CapabilityModel;
using Plugin.Application.CapabilityModel.API;
using Plugin.Application.Forms;

namespace Plugin.Application.Events.API
{
    class EditRESTResourceEvent : EventImplementation
    {
        // Configuration properties used by this module...
        private const string _ServiceDeclPkgStereotype = "ServiceDeclPkgStereotype";

        // Keep track of (extra) classes and associations to show in the diagram...
        private List<MEClass> _diagramClassList = new List<MEClass>();
        private List<MEAssociation> _diagramAssocList = new List<MEAssociation>();

        /// <summary>
        /// The Resource.Edit operation can only be executed on Resource capabilities. Since these are uniquely identified
        /// by their stereotypes, there is no need to perform further checks. 
        /// </summary>
        /// <returns>True.</returns>
        internal override bool IsValidState() { return true; }

        /// <summary>
        /// Processes the 'Edit Resource' menu click event.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.API.EditRESTResourceEvent.HandleEvent >> Processing an edit REST Resource menu click...");

            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            var svcContext = new ServiceContext(this._event.Scope == TreeScope.Diagram);
            MEClass resourceClass = svcContext.ResourceClass;

            if (!svcContext.Valid || svcContext.MyDiagram == null || resourceClass == null)
            {
                Logger.WriteError("Plugin.Application.Events.API.EditRESTResourceEvent.HandleEvent >> Illegal or corrupt context, event aborted!");
                return;
            }
            else if (svcContext.Type != ServiceContext.ServiceType.REST)
            {
                Logger.WriteWarning("Plugin.Application.Events.API.EditRESTResourceEvent.HandleEvent >> Operation only suitable for REST Services!");
                return;
            }

            // By instantiating the service, we should construct the entire capability hierarchy, which facilitates constructing
            // of 'lower level' capabilities using their Class objects...
            var myService = new RESTService(svcContext.Hierarchy, context.GetConfigProperty(_ServiceDeclPkgStereotype));
            var myResource = new RESTResourceCapability(resourceClass);

            using (var dialog = new RESTResourceDialog(new RESTResourceDeclaration(myResource)))
            {
                if (svcContext.LockModel() && dialog.ShowDialog() == DialogResult.OK)
                {
                    bool result = myResource.Edit(dialog.Resource, dialog.MinorVersionIndicator);
                    if (result)
                    {
                        // Collect the (new) classes and associations that must be shown on the diagram...
                        this._diagramClassList = new List<MEClass>();
                        this._diagramAssocList = new List<MEAssociation>();
                        myResource.Traverse(DiagramItemsCollector);

                        // This updates the selected diagram, which could be in a resource package...
                        svcContext.MyDiagram.AddClassList(this._diagramClassList);
                        svcContext.MyDiagram.AddAssociationList(this._diagramAssocList);
                        svcContext.MyDiagram.Redraw();
                        svcContext.DeclarationPackage.Refresh();
                        MessageBox.Show("Resource has been updated successfully.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else MessageBox.Show("Failed to update Resource!", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                Logger.WriteInfo("Plugin.Application.Events.API.EditRESTResourceEvent.DiagramItemsCollector >> Traversing capability '" + cap.Name + "'...");
                this._diagramClassList.Add(cap.CapabilityClass);
                foreach (MEAssociation assoc in cap.CapabilityClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation))
                    this._diagramAssocList.Add(assoc);
            }
            return false;
        }
    }
}
