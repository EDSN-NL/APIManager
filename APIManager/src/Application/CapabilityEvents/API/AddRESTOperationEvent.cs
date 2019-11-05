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
            string errorMsg = string.Empty;
            bool isError = false;               // In case errorMsg is not empty, 'true' = error, 'false' = warning.

            // Perform a series of precondition tests...
            if (!svcContext.Valid || svcContext.MyDiagram == null || operationParent == null)
            {
                if (!svcContext.HasValidRepositoryDescriptor) errorMsg = "No valid Repository Descriptor has been defined for the currently open project!";
                else errorMsg = "Illegal or corrupt context, operation aborted!";
                isError = true;
            }
            else if (svcContext.Type != Service.ServiceArchetype.REST) errorMsg = "Operation only suitable for REST Services!";
            else if (!Service.UpdateAllowed(svcContext.ServiceClass)) errorMsg = "Service must be in checked-out state for operations to be added!";
            else if (svcContext.MyDiagram.OwningPackage != operationParent.OwningPackage) errorMsg = "Operations can only be added from the diagram of the owning resource!";
            else if (!svcContext.LockModel())
            {
                errorMsg = "Unable to lock the model!";
                isError = true;
            }

            if (errorMsg != string.Empty)
            {
                if (isError)
                {
                    Logger.WriteError("Plugin.Application.Events.API.ProcessRESTOperationEvent.HandleEvent >> " + errorMsg);
                    MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    Logger.WriteWarning(errorMsg);
                    MessageBox.Show(errorMsg, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                svcContext.UnlockModel();
                return;
            }

            // By instantiating the service, we should construct the entire capability hierarchy, which facilitates constructing
            // of 'lower level' capabilities using their Class objects...
            var myService = new RESTService(svcContext.Hierarchy, context.GetConfigProperty(_ServiceDeclPkgStereotype));
            var myResource = new RESTResourceCapability(operationParent);
            var newOperationDecl = new RESTOperationDeclaration(myResource, string.Empty, new HTTPOperation(HTTPOperation.Type.Unknown));

            using (var dialog = new RESTOperationDialog(myService, newOperationDecl, new RESTResourceDeclaration(myResource)))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    bool result = myResource.AddOperation(dialog.Operation, dialog.MinorVersionIndicator);
                    if (result)
                    {
                        // Mark service as 'modified' for configuration management and add to diagram in different color...
                        myService.Dirty();
                        myService.Paint(svcContext.ServiceDiagram);

                        // Collect the new classes and associations that must be shown on the diagram...
                        var collector = new DiagramItemsCollector(svcContext.MyDiagram);
                        myResource.Traverse(collector.Collect);

                        // This updates the selected diagram, which could be in a resource package...
                        collector.DiagramClassList.Add(newOperationDecl.ResponseCollection.CollectionClass);    // We want to show the collection on the diagram!
                        svcContext.MyDiagram.AddClassList(collector.DiagramClassList);
                        svcContext.MyDiagram.AddAssociationList(collector.DiagramAssociationList);
                        svcContext.MyDiagram.Redraw();
                        svcContext.DeclarationPackage.Refresh();
                        MessageBox.Show("Operation has been added successfully.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else MessageBox.Show("Failed to add Operation!", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                svcContext.UnlockModel();
            }
        }
    }
}
