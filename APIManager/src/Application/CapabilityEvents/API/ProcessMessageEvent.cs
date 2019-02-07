using System;
using System.Windows.Forms;
using Framework.Event;
using Framework.Logging;
using Framework.Context;
using Framework.Model;
using Plugin.Application.CapabilityModel;
using Plugin.Application.CapabilityModel.API;
using Plugin.Application.Forms;

namespace Plugin.Application.Events.API
{
    /// <summary>
    /// Implements the 'Process Message' functionality, which transforms an UML message using any defined processor.
    /// </summary>
    class ProcessMessageEvent : EventImplementation
    {
        // Configuration properties used by this module...
        private const string _ServiceDeclPkgStereotype          = "ServiceDeclPkgStereotype";
        private const string _BusinessMessageClassStereotype    = "BusinessMessageClassStereotype";

        // Class token used to select appropriate message processors:
        private const string _MessageClassToken = "Message";  // Processor class for Message processors.

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
        /// Process the event. This method is called whenever the 'process Message' menu option is selected on a Business Message capability
        /// class on a diagram or in the package browser. The event checks how many processors are available for class 'Message' and 
        /// if there are more then one, let the user select the preferred processor. If there's only one, this is selected without 
        /// bothering the user. Subsequently, the Message is processed using the selected processor, which creates output according 
        /// to the functionality of that processor.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.API.ProcessMessageEvent.HandleEvent >> Message processing...");
            ContextSlt context = ContextSlt.GetContextSlt();
            var svcContext = new ServiceContext(this._event.Scope == TreeScope.Diagram);



            string errorMsg = string.Empty;
            bool isError = false;

            // Perform a series of precondition tests...
            if (!svcContext.Valid)
            {
                errorMsg = "Illegal or corrupt context, operation aborted!";
                isError = true;
            }
            else if (svcContext.Type != Service.ServiceArchetype.SOAP) errorMsg = "Operation only suitable for SOAP Services!";
            else if (!Service.UpdateAllowed(svcContext.ServiceClass)) errorMsg = "Service must be in checked-out state for messages to be processed!";
            else if (!svcContext.LockModel())
            {
                errorMsg = "Unable to lock the model!";
                isError = true;
            }

            if (errorMsg != string.Empty)
            {
                if (isError)
                {
                    Logger.WriteError("Plugin.Application.Events.API.ProcessMessageEvent.HandleEvent >> " + errorMsg);
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

            // Creating the ApplicationService will construct the entire Capability hierarchy in memory. We can subsequently create any specialized Capability
            // object by using the 'MEClass' constructor, which fetches the appropriate implementation object from the registry...
            var myService = new ApplicationService(svcContext.Hierarchy, context.GetConfigProperty(_ServiceDeclPkgStereotype));
            var myMessage = new MessageCapability(context.CurrentClass);

            ProcessorManagerSlt processorMgr = ProcessorManagerSlt.GetProcessorManagerSlt();
            CapabilityProcessor processor = null;

            if (processorMgr.GetProcessorCount(_MessageClassToken) > 1)
            {
                // Ask user which processor to use.
                using (var picker = new CapabilityProcessorPicker(_MessageClassToken))
                {
                    if (picker.ShowDialog() == DialogResult.OK) processor = picker.SelectedProcessor;
                }
            }
            else
            {
                if (processorMgr.GetProcessorCount(_MessageClassToken) == 1)
                    processor = processorMgr.GetProcessorByIndex(_MessageClassToken, 0);
                else MessageBox.Show("No processors are currently defined for Messaging, aborting!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            if (processor != null) myMessage.HandleCapabilities(processor);

            // Mark service as 'modified' for configuration management and add to diagram in different color...
            myService.Dirty();
            myService.Paint(svcContext.MyDiagram);
            svcContext.UnlockModel();
        }
    }
}
