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
    class EditRESTOperationEvent : EventImplementation
    {
        // Configuration properties used by this module...
        private const string _ServiceDeclPkgStereotype = "ServiceDeclPkgStereotype";

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
            Logger.WriteInfo("Plugin.Application.Events.API.EditRESTOperationEvent.HandleEvent >> Processing an edit REST Operation menu click...");

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
            var myOperation = new RESTOperationCapability(operationClass);
            var myResource = new RESTResourceCapability(myOperation.Parent.CapabilityClass);

            using (var dialog = new RESTOperationDialog(new RESTOperationDeclaration(myOperation), new RESTResourceDeclaration(myResource)))
            {
                if (svcContext.LockModel() && dialog.ShowDialog() == DialogResult.OK)
                {
                    bool result = myOperation.Edit(dialog.Operation, dialog.MinorVersionIndicator);
                    if (result)
                    {
                        // Collect the (new) classes and associations that must be shown on the diagram...
                        DiagramItemsCollector collector = new DiagramItemsCollector(svcContext.MyDiagram);
                        myOperation.Traverse(collector.Collect);

                        // This updates the selected diagram, which could be in a resource package...
                        svcContext.MyDiagram.AddClassList(collector.DiagramClassList);
                        svcContext.MyDiagram.AddAssociationList(collector.DiagramAssociationList);
                        svcContext.MyDiagram.Redraw();
                        svcContext.DeclarationPackage.Refresh();
                        MessageBox.Show("Operation has been updated successfully.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else MessageBox.Show("Failed to update Operation!", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                svcContext.UnlockModel();
            }
        }
    }
}
