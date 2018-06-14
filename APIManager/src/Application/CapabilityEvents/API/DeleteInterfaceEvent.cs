using System.Windows.Forms;
using Framework.Event;
using Framework.Logging;
using Framework.Model;
using Framework.Context;
using Plugin.Application.CapabilityModel.API;
using Plugin.Application.Forms;

namespace Plugin.Application.Events.API
{
    class DeleteInterfaceEvent : EventImplementation
    {
        // Configuration properties used by this module...
        private const string _OperationClassStereotype          = "OperationClassStereotype";
        private const string _ServiceDeclPkgStereotype          = "ServiceDeclPkgStereotype";
        private const string _ServiceClassStereotype            = "ServiceClassStereotype";
        private const string _ServiceModelPkgName               = "ServiceModelPkgName";
        private const string _InterfaceContractClassStereotype  = "InterfaceContractClassStereotype";
        private const string _RequestMessageSuffix              = "RequestMessageSuffix";
        private const string _ResponseMessageSuffix             = "ResponseMessageSuffix";


        /// <summary>
        /// Checks whether we can process the event in the current context. In this case, we only have to check whether we are called
        /// from a diagram of the correct type, which is determined by the declaration package of which the diagram is a part.
        /// </summary>
        /// <returns>True on correct context, false otherwise.</returns>
        internal override bool IsValidState()
        {
            if (this._event.Scope == TreeScope.Diagram)
            {
                ContextSlt context = ContextSlt.GetContextSlt();
                if (context.CurrentDiagram != null)
                {
                    MEPackage declaration = context.CurrentDiagram.OwningPackage.Parent;  // Parent package of diagram package is declaration package.
                    return (declaration.HasStereotype(context.GetConfigProperty(_ServiceDeclPkgStereotype)));
                }
                else return false;
            }
            else return true;   // Only other possibility is package-tree and this is checked by default configuration.
        }

        /// <summary>
        /// Processes the 'Add Interface' menu click event.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.API.DeleteInterfaceEvent.HandleEvent >> Processing a delete interface menu click...");

            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            var svcContext = new ServiceContext(this._event.Scope == TreeScope.Diagram);

            if (!svcContext.Valid || svcContext.InterfaceClass == null || svcContext.SVCModelPackage == null)
            {
                Logger.WriteError("Plugin.Application.Events.API.DeleteInterfaceEvent.HandleEvent >> Illegal or corrupt context, event aborted!");
                return;
            }
            else if (svcContext.Type != ServiceContext.ServiceType.SOAP)
            {
                Logger.WriteWarning("Plugin.Application.Events.API.DeleteInterfaceEvent.HandleEvent >> Operation only suitable for SOAP Services!");
                return;
            }

            using (var dialog = new ConfirmOperationChanges("Are you sure you want to delete Interface '" + svcContext.InterfaceClass.Name + "'?"))
            {
                if (svcContext.LockModel() && dialog.ShowDialog() == DialogResult.OK)
                {
                    // Creating the service will build the entire object hierarchy. Subsequently, we can create Capabilities by class alone...
                    var myService = new ApplicationService(svcContext.Hierarchy, context.GetConfigProperty(_ServiceDeclPkgStereotype));
                    if (!myService.DeleteInterface(new InterfaceCapability(svcContext.InterfaceClass), dialog.MinorVersionIndicator))
                    {
                        Logger.WriteWarning("Plugin.Application.Events.API.DeleteInterfaceEvent.handleEvent >> Attempt to delete only Interface!");
                        MessageBox.Show("It is not allowed to delete the only Interface!", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        // Mark service as 'modified' for configuration management and add to diagram in different color...
                        myService.Dirty();
                        myService.Paint(svcContext.MyDiagram);
                        svcContext.Refresh();
                    }
                }
                svcContext.UnlockModel();
            }
        }
    }
}
