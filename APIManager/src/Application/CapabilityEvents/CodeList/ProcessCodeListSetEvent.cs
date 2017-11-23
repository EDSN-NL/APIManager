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
    /// Implements the 'Process CodeListSet' functionality, which writes a selection of CodeLists to processor-specific output format,
    /// including a 'Set' document that binds all CodeLists together in a single version.
    /// </summary>
    class ProcessCodeListSetEvent : EventImplementation
    {
        // Configuration properties used by this module...
        private const string _CodeListDeclPkgStereotype     = "CodeListDeclPkgStereotype";
        private const string _ServiceClassStereotype        = "ServiceClassStereotype";
        private const string _ServiceModelPkgName           = "ServiceModelPkgName";
        private const string _FileNameTag                   = "FileNameTag";
        private const string _PathNameTag                   = "PathNameTag";

        private const string _CodeListClassToken            = "CodeList";  // Processor class for CodeList processors.

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
                MEPackage declaration = context.CurrentDiagram.OwningPackage.Parent;  // Parent package of diagram package is declaration package.
                return (declaration.HasStereotype(context.GetConfigProperty(_CodeListDeclPkgStereotype)));
            }
            else return true;   // Only other possibility is package-tree and this is checked by default configuration.
        }

        /// <summary>
        /// Process the event. This method is called whenever the 'process CodeListSet' menu option is selected on a CodeList Service
        /// class on a diagram or in the package browser. The event checks how many processors are available for class 'CodeList' and 
        /// if there are more then one, let the user select the preferred processor. If there's only one, this is selected without 
        /// bothering the user. The method also counts the number of CodeLists associated with the service and presents a CodeList picker
        /// that facilitates the user to make a selection of CodeLists to process.
        /// Subsequently, the CodeLists are processed using the selected processor, which creates a series of output files according to the
        /// functionality of that processor.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.CodeList.ProcessCodeListSetEvent.handleEvent >> Processing event...");
            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            MEPackage declPackage;
            MEClass serviceClass = null;
            Diagram myDiagram = null;

            // Figure-out how we got here...
            if (this._event.Scope == TreeScope.Diagram)
            {
                myDiagram = context.CurrentDiagram;
                declPackage = myDiagram.OwningPackage.Parent;
                serviceClass = context.CurrentClass;
            }
            else // (this._event.scope == TreeScope.PackageTree), no other options possible.
            {
                declPackage = context.CurrentPackage;   // Must be activated from the Code List Declaration package!
                MEPackage serviceModel = declPackage.FindPackage(context.GetConfigProperty(_ServiceModelPkgName));
                string serviceName = declPackage.Name.Substring(0, declPackage.Name.IndexOf("_V"));
                serviceClass = serviceModel.FindClass(serviceName, context.GetConfigProperty(_ServiceClassStereotype));
                myDiagram = serviceModel.FindDiagram(serviceModel.Name);
            }

            if (serviceClass == null || myDiagram == null)
            {
                Logger.WriteError("Plugin.Application.Events.CodeList.ProcessCodeListSetEvent.handleEvent >> Illegal or corrupt context, event aborted!");
                return;
            }

            var codeListService = new CodeListService(serviceClass, context.GetConfigProperty(_CodeListDeclPkgStereotype));

            // Let's ask the user which CodeLists to process...
            using (var clPicker = new CodeListPicker())
            {
                clPicker.LoadNodes(serviceClass.Name, codeListService.Capabilities);
                if (clPicker.ShowDialog() == DialogResult.OK) codeListService.LoadSelectedCapabilities(clPicker.GetCheckedNodes());
                else
                {
                    Logger.WriteInfo("Plugin.Application.Events.CodeList.ProcessCodeListSetEvent.handleEvent >> Cancelled by user, aborting!");
                    return;
                }
            }

            // Now select the processor to use (if more then one available)...
            ProcessorManagerSlt processorMgr = ProcessorManagerSlt.GetProcessorManagerSlt();
            CapabilityProcessor processor = null;
            if (processorMgr.GetProcessorCount(_CodeListClassToken) > 1)
            {
                using (var picker = new CapabilityProcessorPicker(_CodeListClassToken))
                {
                    if (picker.ShowDialog() == DialogResult.OK) processor = picker.SelectedProcessor;
                }
            }
            else
            {
                if (processorMgr.GetProcessorCount(_CodeListClassToken) ==  1)
                    processor = processorMgr.GetProcessorByIndex(_CodeListClassToken, 0);
                else MessageBox.Show("No processors are currently defined for CodeList, aborting!");
            }
            if (processor != null) codeListService.HandleCapabilities(processor);
        }
    }
}
