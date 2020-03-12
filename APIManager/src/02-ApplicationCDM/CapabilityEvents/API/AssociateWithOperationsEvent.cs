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
    class AssociateWithOperationsEvent : MenuEventImplementation
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
            string errorMsg = string.Empty;
            bool isError = false;               // In case errorMsg is not empty, 'true' = error, 'false' = warning.

            // Perform a series of precondition tests...
            if (!svcContext.Valid || svcContext.InterfaceClass == null)
            {
                if (!svcContext.HasValidRepositoryDescriptor) errorMsg = "No valid Repository Descriptor has been defined for the currently open project!";
                else errorMsg = "Illegal or corrupt context, operation aborted!";
                isError = true;
            }
            else if (svcContext.Type != Service.ServiceArchetype.SOAP) errorMsg = "Operation only suitable for SOAP Services!";
            else if (!Service.UpdateAllowed(svcContext.ServiceClass)) errorMsg = "Service must be in checked-out state for operations to be associated!";
            else if (!svcContext.LockModel())
            {
                errorMsg = "Unable to lock the model!";
                isError = true;
            }

            if (errorMsg != string.Empty)
            {
                if (isError)
                {
                    Logger.WriteError("Plugin.Application.Events.API.AssociateWithOperations.HandleEvent >> " + errorMsg);
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
                    if (picker.ShowDialog() == DialogResult.OK)
                    {
                        // Mark service as 'modified' for configuration management and add to diagram in different color...
                        myService.Dirty();
                        myService.Paint(svcContext.MyDiagram);

                        itfCap.AssociateOperations(picker.GetCheckedCapabilities().ConvertAll(Converter), picker.MinorVersionIndicator);
                        svcContext.Refresh();
                    }
                }
            }
            else MessageBox.Show("No operations to be associated with selected interface!", "No free operations", MessageBoxButtons.OK, MessageBoxIcon.Information);
            svcContext.UnlockModel();
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
