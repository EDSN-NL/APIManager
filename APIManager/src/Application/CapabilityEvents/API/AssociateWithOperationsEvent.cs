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
    class AssociateWithOperationsEvent : EventImplementation
    {
        // Configuration properties used by this module...
        private const string _ServiceDeclPkgStereotype  = "ServiceDeclPkgStereotype";
        private const string _OperationClassStereotype  = "OperationClassStereotype";

        /// <summary>
        /// This event can only be raised from a diagram. No further checks required.
        /// </summary>
        /// <returns>True</returns>
        internal override bool IsValidState() { return true; }

        /// <summary>
        /// Processes the 'Associate with Service Operations' menu click event.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.API.AssociateWithOperationsEvent.HandleEvent >> Processing association menu click...");
            ContextSlt context = ContextSlt.GetContextSlt();
            var svcContext = new ServiceContext(this._event.Scope == TreeScope.Diagram);
            string operationClassStereotype = context.GetConfigProperty(_OperationClassStereotype);

            if (!svcContext.Valid || svcContext.InterfaceClass == null)
            {
                Logger.WriteError("Plugin.Application.Events.API.AssociateWithOperationsEvent.HandleEvent >> Illegal context! Aborting.");
                return;
            }
            else if (svcContext.Type != ServiceContext.ServiceType.SOAP)
            {
                Logger.WriteWarning("Plugin.Application.Events.API.AssociateWithOperationsEvent.HandleEvent >> Operation only suitable for SOAP Services!");
                return;
            }

            // Creating the service will build the entire object hierarchy. Subsequently, we can create Capabilities by class alone...
            var myService = new ApplicationService(svcContext.Hierarchy, context.GetConfigProperty(_ServiceDeclPkgStereotype));
            var itfCap = new InterfaceCapability(svcContext.InterfaceClass);

            // Get a list of all operations that are NOT already associated with the selected interface...
            var allOperations = new List<Capability>();
            foreach (MEClass cls in svcContext.SVCModelPackage.GetClasses(operationClassStereotype))
            {
                if (!itfCap.HasChildClass(cls)) allOperations.Add(new OperationCapability(cls));
            }

            if (allOperations.Count > 0)
            {
                using (var picker = new CapabilityPicker("Select Operation(s) to associate:", allOperations))
                {
                    if (svcContext.LockModel() && picker.ShowDialog() == DialogResult.OK)
                    {
                        itfCap.AssociateOperations(picker.GetCheckedCapabilities().ConvertAll(Converter), picker.MinorVersionIndicator);
                        svcContext.Refresh();
                    }
                    svcContext.UnlockModel();
                }
            }
            else MessageBox.Show("No operations to be associated with selected interface!", "No free operations", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Helper function that aids in the conversion of base-class Capabilities to OperationCapabilities.
        /// </summary>
        /// <param name="cap">Capability base.</param>
        /// <returns>Actual derived OperationCapability object.</returns>
        private OperationCapability Converter(Capability cap)
        {
            return cap as OperationCapability;
        }
    }
}
