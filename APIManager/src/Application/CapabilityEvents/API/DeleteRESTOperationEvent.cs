using System.Windows.Forms;
using Framework.Event;
using Framework.Logging;
using Framework.Model;
using Framework.Context;
using Plugin.Application.CapabilityModel.API;
using Plugin.Application.Forms;

namespace Plugin.Application.Events.API
{
    class DeleteRESTOperationEvent : EventImplementation
    {
        // Configuration properties used by this module...
        private const string _ServiceDeclPkgStereotype              = "ServiceDeclPkgStereotype";

        /// <summary>
        /// This event can only be applied to classes of stereotype 'RESTOperation'. There is no need to check for additional state.
        /// </summary>
        /// <returns>True.</returns>
        internal override bool IsValidState() { return true; }

        /// <summary>
        /// Processes the 'Delete REST Operation' menu click event.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.API.DeleteRESTOperationEvent.HandleEvent >> Processing a Delete REST Operation menu click...");

            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            var svcContext = new ServiceContext(this._event.Scope == TreeScope.Diagram);
            MEClass operationClass = svcContext.OperationClass;

            if (!svcContext.Valid || operationClass == null)
            {
                Logger.WriteError("Plugin.Application.Events.API.DeleteRESTOperationEvent.HandleEvent >> Illegal or corrupt context, event aborted!");
                return;
            }
            else if (svcContext.Type != ServiceContext.ServiceType.REST)
            {
                Logger.WriteWarning("Plugin.Application.Events.API.AddRESTOperationEvent.HandleEvent >> Operation only suitable for REST Services!");
                return;
            }

            // Ask the user whether he/she really wants to delete the operation...
            using (var dialog = new ConfirmOperationChanges("Are you sure you want to delete Operation '" + operationClass.Name + "'?"))
            {
                if (svcContext.LockModel() && dialog.ShowDialog() == DialogResult.OK)
                {
                    // By instantiating the service, we should construct the entire capability hierarchy, which facilitates constructing
                    // of 'lower level' capabilities using their Class objects...
                    var myService = new RESTService(svcContext.Hierarchy, context.GetConfigProperty(_ServiceDeclPkgStereotype));
                    var myOperation = new RESTOperationCapability(operationClass);
                    myOperation.Delete();
                    svcContext.Refresh();
                }
                svcContext.UnlockModel();
            }
        }
    }
}
