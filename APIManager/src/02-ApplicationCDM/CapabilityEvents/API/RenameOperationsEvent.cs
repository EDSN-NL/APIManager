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
    class RenameOperationsEvent : MenuEventImplementation
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
            string errorMsg = string.Empty;
            bool isError = false;

            // Perform a series of precondition tests...
            if (!svcContext.Valid || svcContext.OperationClass == null)
            {
                if (!svcContext.HasValidRepositoryDescriptor) errorMsg = "No valid Repository Descriptor has been defined for the currently open project!";
                else errorMsg = "Illegal or corrupt context, operation aborted!";
                isError = true;
            }
            else if (svcContext.Type != Service.ServiceArchetype.SOAP) errorMsg = "Operation only suitable for SOAP Services!";
            else if (!Service.UpdateAllowed(svcContext.ServiceClass)) errorMsg = "Service must be in checked-out state for operations to be renamed!";
            else if (!svcContext.LockModel())
            {
                errorMsg = "Unable to lock the model!";
                isError = true;
            }

            if (errorMsg != string.Empty)
            {
                if (isError)
                {
                    Logger.WriteError("Plugin.Application.Events.API.RenameOperationsEvent.HandleEvent >> " + errorMsg);
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

            string oldName = svcContext.OperationClass.Name;
            using (var dialog = new RenameOperationInput(oldName, svcContext.DeclarationPackage))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
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
