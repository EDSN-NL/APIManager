using System.Windows.Forms;
using Framework.Event;
using Framework.Logging;
using Framework.Model;
using Framework.Context;
using Plugin.Application.CapabilityModel.API;
using Plugin.Application.Forms;

namespace Plugin.Application.Events.API
{
    class EditRESTResourceEvent : EventImplementation
    {
        // Configuration properties used by this module...
        private const string _ServiceDeclPkgStereotype = "ServiceDeclPkgStereotype";

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
            
            // Check if we are on the owning diagram of the resource...  
            if (svcContext.MyDiagram.OwningPackage != svcContext.ResourceClass.OwningPackage)
            {
                MessageBox.Show("A Resource can only be edited from its owning diagram!",
                                "Wrong Diagram", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        // Mark service as 'modified' for configuration management and add to diagram in different color...
                        myService.Dirty();
                        myService.Paint(svcContext.MyDiagram);

                        // Collect the (new) classes and associations that must be shown on the diagram...
                        DiagramItemsCollector collector = new DiagramItemsCollector(svcContext.MyDiagram);
                        myResource.Traverse(collector.Collect);

                        // This updates the selected diagram, which could be in a resource package...
                        svcContext.MyDiagram.AddClassList(collector.DiagramClassList);
                        svcContext.MyDiagram.AddAssociationList(collector.DiagramAssociationList);
                        svcContext.MyDiagram.Redraw();
                        svcContext.DeclarationPackage.Refresh();
                        MessageBox.Show("Resource has been updated successfully.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else MessageBox.Show("Failed to update Resource!", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                svcContext.UnlockModel();
            }
        }
    }
}
