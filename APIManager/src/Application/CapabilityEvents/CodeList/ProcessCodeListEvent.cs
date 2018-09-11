using System;
using System.Windows.Forms;
using Framework.Event;
using Framework.Logging;
using Framework.Model;
using Framework.Context;
using Framework.View;
using Plugin.Application.CapabilityModel;
using Plugin.Application.CapabilityModel.CodeList;
using Plugin.Application.Forms;

namespace Plugin.Application.Events.CodeList
{
    /// <summary>
    /// Implements the 'Process CodeList' functionality, which writes the CodeList to a processor-specific output file.
    /// </summary>
    class ProcessCodeListEvent : EventImplementation
    {
        // Configuration properties used by this module...
        private const string _CodeListDeclPkgStereotype     = "CodeListDeclPkgStereotype";
        private const string _ServiceClassStereotype        = "ServiceClassStereotype";
        private const string _FileNameTag                   = "FileNameTag";
        private const string _PathNameTag                   = "PathNameTag";

        private const bool _NOBUILDHIERARCHY                = false;

        private const string _CodeListClassToken = "CodeList";  // Processor class for CodeList processors.

        /// <summary>
        /// Checks whether we can process the event in the current context. In this case, we always return 'true' since the correct
        /// context has already been checked by our default mechanism (checking stereotypes).
        /// </summary>
        /// <returns>True.</returns>
        internal override bool IsValidState()
        {
            return true;
        }

        /// <summary>
        /// Process the event. This method is called whenever the 'process CodeList' menu option is selected on a CodeList capability
        /// class on a diagram or in the package browser. The event checks how many processors are available for class 'CodeList' and 
        /// if there are more then one, let the user select the preferred processor. If there's only one, this is selected without 
        /// bothering the user.
        /// Subsequently, the CodeList is processed using the selected processor, which creates an output file according to the
        /// functionality of that processor.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.CodeList.ProcessCodeListEvent.handleEvent >> CodeList processing...");
            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            MEPackage serviceModel = null;
            MEClass codeListClass = context.CurrentClass;
            Diagram myDiagram = null;

            // Figure-out how we got here...
            if (this._event.Scope == TreeScope.Diagram)
            {
                myDiagram = context.CurrentDiagram;
                serviceModel = myDiagram.OwningPackage;
            }
            else // (this._event.scope == TreeScope.PackageTree), no other options possible.
            {
                serviceModel = context.CurrentPackage;   // Must be activated from the ServiceModel package!
                myDiagram = serviceModel.FindDiagram(serviceModel.Name);
            }

            string serviceName = serviceModel.Parent.Name.Substring(0, serviceModel.Parent.Name.IndexOf("_V"));
            MEClass serviceClass = serviceModel.FindClass(serviceName, context.GetConfigProperty(_ServiceClassStereotype));
            if (serviceClass == null || myDiagram == null)
            {
                Logger.WriteError("Plugin.Application.Events.CodeList.ProcessCodeListEvent.handleEvent >> Illegal or corrupt context, event aborted!");
                return;
            }

            if (model.LockModel(serviceModel.Parent))
            {
                // The NoBuildHierarchy indicator avoids that the entire structure is constructed (since we're only interested in one single CodeList)...
                var codeListService = new CodeListService(serviceClass, context.GetConfigProperty(_CodeListDeclPkgStereotype), _NOBUILDHIERARCHY);
                var codeListCapability = new CodeListCapability(codeListService, codeListClass);
                ProcessorManagerSlt processorMgr = ProcessorManagerSlt.GetProcessorManagerSlt();
                CapabilityProcessor processor = null;

                if (processorMgr.GetProcessorCount(_CodeListClassToken) > 1)
                {
                    // Ask user which processor to use.
                    using (var picker = new CapabilityProcessorPicker(_CodeListClassToken))
                    {
                        if (picker.ShowDialog() == DialogResult.OK) processor = picker.SelectedProcessor;
                    }
                }
                else
                {
                    if (processorMgr.GetProcessorCount(_CodeListClassToken) == 1)
                        processor = processorMgr.GetProcessorByIndex(_CodeListClassToken, 0);
                    else MessageBox.Show("No processors are currently defined for CodeList, aborting!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (processor != null && codeListCapability.HandleCapabilities(processor))
                {
                    string message = "Processing of CodeList '" + codeListCapability.Name + "' finished." + Environment.NewLine;
                    message += "Output path: '" + codeListService.ServiceCIPath + "'." + Environment.NewLine + "Filename: '" + processor.GetCapabilityFilename() + "'.";
                    MessageBox.Show(message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    codeListService.Paint(myDiagram);
                }
            }
            model.UnlockModel(serviceModel.Parent);
        }
    }
}
