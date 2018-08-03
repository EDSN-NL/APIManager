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
    class RenameOperationsEvent : EventImplementation
    {
        // Configuration properties used by this module...
        private const string _ServiceDeclPkgStereotype          = "ServiceDeclPkgStereotype";
        private const string _InterfaceContractClassStereotype  = "InterfaceContractClassStereotype";

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
        /// Processes the 'Rename Operation' menu click event.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.API.RenameOperationsEvent.HandleEvent >> Processing a rename operation menu click...");
            ContextSlt context = ContextSlt.GetContextSlt();
            var svcContext = new ServiceContext(this._event.Scope == TreeScope.Diagram);
            string interfaceContractClassStereotype = context.GetConfigProperty(_InterfaceContractClassStereotype);

            if (!svcContext.Valid || svcContext.OperationClass == null)
            {
                Logger.WriteError("Plugin.Application.Events.API.RenameOperationsEvent.HandleEvent >> Illegal context! Aborting.");
                return;
            }
            else if (svcContext.Type != ServiceContext.ServiceType.SOAP)
            {
                Logger.WriteWarning("Operation only suitable for SOAP Services!");
                return;
            }

            string oldName = svcContext.OperationClass.Name;
            using (var dialog = new RenameOperationInput(oldName, svcContext.DeclarationPackage))
            {
                if (svcContext.LockModel() && dialog.ShowDialog() == DialogResult.OK)
                {
                    var myService = new ApplicationService(svcContext.Hierarchy, context.GetConfigProperty(_ServiceDeclPkgStereotype));

                    // Search the model for all interfaces that have an association with the operation to be renamed. If found, we instruct
                    // the interface to rename the operation. If there are multiple, the first will actually perform the work
                    // and all subsequent interfaces will only have to adjust their role name...
                    foreach (Capability cap in myService.Capabilities)
                    {
                        if (cap.CapabilityClass.HasStereotype(interfaceContractClassStereotype) && 
                            svcContext.HasOperation(cap.CapabilityClass, svcContext.OperationClass))
                        {
                            var itfCap = new InterfaceCapability(cap.CapabilityClass);
                            itfCap.RenameOperation(svcContext.OperationClass, oldName, dialog.OperationName, dialog.MinorVersionIndicator);
                        }
                    }

                    if (dialog.MinorVersionIndicator) myService.UpdateVersion(new Tuple<int, int>(myService.Version.Item1, myService.Version.Item2 + 1));
                    myService.CreateLogEntry("Renamed operation: '" + oldName + "' to: '" + svcContext.OperationClass.Name + "'.");

                    // Mark service as 'modified' for configuration management and add to diagram in different color...
                    myService.Dirty();
                    myService.Paint(svcContext.MyDiagram);

                    svcContext.Refresh();
                }
                svcContext.UnlockModel();
            }
        }
    }
}
