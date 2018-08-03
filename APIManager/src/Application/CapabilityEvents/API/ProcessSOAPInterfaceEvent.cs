using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Framework.Event;
using Framework.Logging;
using Framework.Context;
using Plugin.Application.CapabilityModel;
using Plugin.Application.CapabilityModel.API;
using Plugin.Application.Forms;

namespace Plugin.Application.Events.API
{
    /// <summary>
    /// Implements the 'Process SOAP Interface' functionality, which transforms all Operations associated with an Interface to an output format dictated by
    /// the selected Processor.
    /// </summary>
    class ProcessSOAPInterfaceEvent : EventImplementation
    {
        // Configuration properties used by this module...
        private const string _ServiceDeclPkgStereotype      = "ServiceDeclPkgStereotype";
        private const string _InterfaceContractTypeTag      = "InterfaceContractTypeTag";

        // Class token used to select appropriate message processors:
        private const string _InterfaceClassToken = "SOAPInterface";

        /// <summary>
        /// Checks whether we can process the event in the current context. In this case, we only have to check whether we are called
        /// from a diagram of the correct type, which is determined by the declaration package of which the diagram is a part.
        /// </summary>
        /// <returns>True if this is indeed a valid Interface Contract Capability, false otherwise.</returns>
        internal override bool IsValidState()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            if (context.CurrentClass != null)
            {
                // We check whether the Interface Class has the correct contract type tag. Must start with 'WSDL' to be valid...
                string contractTag = context.CurrentClass.GetTag(context.GetConfigProperty(_InterfaceContractTypeTag));
                return (!string.IsNullOrEmpty(contractTag) && contractTag.StartsWith("WSDL"));
            }
            return false;
        }

        /// <summary>
        /// Process the event. This method is called whenever the 'process Interface' menu option is selected on an Interface capability
        /// class on a diagram or in the package browser. The event checks how many processors are available for class 'SOAPInterface' and 
        /// if there are more then one, let the user select the preferred processor. If there's only one, this is selected without 
        /// bothering the user. Subsequently, the Interface is processed using the selected processor, which creates output according 
        /// to the functionality of that processor.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.API.ProcessSOAPInterfaceEvent.HandleEvent >> Message processing...");
            ContextSlt context = ContextSlt.GetContextSlt();
            var svcContext = new ServiceContext(this._event.Scope == TreeScope.Diagram);

            if (!svcContext.Valid)
            {
                Logger.WriteError("Plugin.Application.Events.API.ProcessSOAPInterfaceEvent.HandleEvent >> Illegal or corrupt context, event aborted!");
                return;
            }
            else if (svcContext.Type != ServiceContext.ServiceType.SOAP)
            {
                Logger.WriteWarning("Operation only suitable for SOAP Services!");
                return;
            }

            // Creating the SOAPService will construct the entire Capability hierarchy in memory. We can subsequently create any specialized Capability
            // object by using the 'MEClass' constructor, which fetches the appropriate implementation object from the registry...
            var myService = new ApplicationService(svcContext.Hierarchy, context.GetConfigProperty(_ServiceDeclPkgStereotype));
            var myInterface = new InterfaceCapability(svcContext.InterfaceClass);
            List<Capability> allOperations = myInterface.GetOperations().ConvertAll(BaseConverter);
            if (!myService.Checkout())
            {
                MessageBox.Show("Unable to checkout service '" + myService.Name + 
                                "' from configuration management; probably caused by uncommitted changes on branch(es): '" + 
                                CMContext.FindBranchesInState(CMState.Modified) + "'." + Environment.NewLine + 
                                "Please commit pending changes before starting work on a new service!",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var picker = new CapabilityPicker("Select Operation(s) to include in the build:", allOperations, true, false))
            {
                if (svcContext.LockModel() && picker.ShowDialog() == DialogResult.OK)
                {
                    List<Capability> selectedOperations = picker.GetCheckedCapabilities();
                    if (selectedOperations.Count == 0) return;              // Nothing selected, treat as cancel.
                    // Don't forget to explicitly add the Common Schema or it won't be processed. Adding it to the end of the list also
                    // assures that it's processed last so we can be sure that all Operations have been finished when post-processing the
                    // Common Schema!
                    selectedOperations.Add(myInterface.CommonSchema); 
                    myInterface.LoadSelectedCapabilities(selectedOperations);
                }
                else
                {
                    Logger.WriteInfo("Plugin.Application.Events.API.ProcessSOAPInterfaceEvent.HandleEvent >> Cancelling operation!");
                    svcContext.UnlockModel();
                    return;
                }
            }

            ProcessorManagerSlt processorMgr = ProcessorManagerSlt.GetProcessorManagerSlt();
            CapabilityProcessor processor = null;
            if (processorMgr.GetProcessorCount(_InterfaceClassToken) > 1)
            {
                // Ask user which processor to use.
                using (var picker = new CapabilityProcessorPicker(_InterfaceClassToken))
                {
                    if (picker.ShowDialog() == DialogResult.OK) processor = picker.SelectedProcessor;
                }
            }
            else
            {
                if (processorMgr.GetProcessorCount(_InterfaceClassToken) == 1)
                    processor = processorMgr.GetProcessorByIndex(_InterfaceClassToken, 0);
                else MessageBox.Show("No processors are currently defined for Interface processing, aborting!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (processor != null)
            {
                myInterface.HandleCapabilities(processor);

                // Mark service as 'modified' for configuration management and add to diagram in different color...
                myService.Dirty();
                myService.Paint(svcContext.MyDiagram);
            }
            svcContext.UnlockModel();
        }

        /// <summary>
        /// Helper function that aids in the conversion of base-class Capabilities to OperationCapabilities.
        /// It effectively does nothing since an OperationCapability 'is' a Capability but the Lists do not know that ;-)
        /// </summary>
        /// <param name="cap">Capability base.</param>
        /// <returns>Actual derived OperationCapability object.</returns>
        private Capability BaseConverter(OperationCapability cap) { return cap; }
    }
}
