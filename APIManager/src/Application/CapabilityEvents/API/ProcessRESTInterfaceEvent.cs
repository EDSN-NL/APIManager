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
    /// Implements the 'Process REST Interface' functionality, which transforms all Operations associated with an Interface to an output format dictated by
    /// the selected Processor.
    /// </summary>
    class ProcessRESTInterfaceEvent : EventImplementation
    {
        // Configuration properties used by this module...
        private const string _ServiceDeclPkgStereotype      = "ServiceDeclPkgStereotype";
        private const string _InterfaceContractTypeTag      = "InterfaceContractTypeTag";

        // Class token used to select appropriate message processors:
        private const string _InterfaceClassToken = "RESTInterface";

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
                // We check whether the Interface Class has the correct contract type tag. Must start with 'REST' to be valid...
                string contractTag = context.CurrentClass.GetTag(context.GetConfigProperty(_InterfaceContractTypeTag));
                return (!string.IsNullOrEmpty(contractTag) && contractTag.StartsWith("REST"));
            }
            return false;
        }

        /// <summary>
        /// Process the event. This method is called whenever the 'process Interface' menu option is selected on an Interface capability
        /// class on a diagram or in the package browser. The event checks how many processors are available for class 'RESTInterface' and 
        /// if there are more then one, let the user select the preferred processor. If there's only one, this is selected without 
        /// bothering the user. Subsequently, the Interface is processed using the selected processor, which creates output according 
        /// to the functionality of that processor.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.API.ProcessRESTInterfaceEvent.HandleEvent >> Message processing...");
            ContextSlt context = ContextSlt.GetContextSlt();
            var svcContext = new ServiceContext(this._event.Scope == TreeScope.Diagram);

            if (!svcContext.Valid)
            {
                Logger.WriteError("Plugin.Application.Events.API.ProcessRESTInterfaceEvent.HandleEvent >> Illegal or corrupt context, event aborted!");
                return;
            }

            // Creating the RESTService will construct the entire Capability hierarchy in memory. We can subsequently create any specialized Capability
            // object by using the 'MEClass' constructor, which fetches the appropriate implementation object from the registry...
            var myService = new RESTService(svcContext.Hierarchy, context.GetConfigProperty(_ServiceDeclPkgStereotype));
            var myInterface = new RESTInterfaceCapability(svcContext.InterfaceClass);
            List<Capability> allResources = myInterface.GetResources().ConvertAll(BaseConverter);

            using (var picker = new CapabilityPicker("Select root Resource(s) to include in the interface:", allResources, true, false))
            {
                if (picker.ShowDialog() == DialogResult.OK)
                {
                    List<Capability> selectedResources = picker.GetCheckedCapabilities();
                    if (selectedResources.Count == 0) return;              // Nothing selected, treat as cancel.
                    // Don't forget to explicitly add the Common Schema or it won't be processed. Adding it to the end of the list also
                    // assures that it's processed last so we can be sure that all Operations have been finished when post-processing the
                    // Common Schema!
                    selectedResources.Add(myInterface.CommonSchema); 
                    myInterface.LoadSelectedCapabilities(selectedResources);
                }
                else
                {
                    Logger.WriteInfo("Plugin.Application.Events.API.ProcessRESTInterfaceEvent.HandleEvent >> Cancelling operation!");
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
            if (processor != null) myInterface.HandleCapabilities(processor);
        }

        /// <summary>
        /// Helper function that aids in the conversion of base-class Capabilities to OperationCapabilities.
        /// It effectively does nothing since an OperationCapability 'is' a Capability but the Lists do not know that ;-)
        /// </summary>
        /// <param name="cap">Capability base.</param>
        /// <returns>Actual derived OperationCapability object.</returns>
        private Capability BaseConverter(RESTResourceCapability cap) { return cap; }
    }
}
