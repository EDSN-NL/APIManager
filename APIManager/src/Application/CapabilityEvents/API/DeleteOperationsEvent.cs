using System;
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
    class DeleteOperationsEvent : EventImplementation
    {
        // Configuration properties used by this module...
        private const string _ServiceDeclPkgStereotype          = "ServiceDeclPkgStereotype";
        private const string _OperationClassStereotype          = "OperationClassStereotype";
        private const string _InterfaceContractClassStereotype  = "InterfaceContractClassStereotype";
        private const string _ServiceClassStereotype            = "ServiceClassStereotype";

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
        /// Processes the 'Delete Operation' menu click event.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.API.DeleteOperationsEvent.handleEvent >> Processing a delete operation menu click...");
            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            var svcContext = new ServiceContext(this._event.Scope == TreeScope.Diagram);
            string interfaceContractClassStereotype = context.GetConfigProperty(_InterfaceContractClassStereotype);
            string interfaceNames = string.Empty;
            bool deleteResources = true;

            if (!svcContext.Valid && svcContext.OperationClass == null)
            {
                Logger.WriteError("Plugin.Application.Events.API.DeleteOperationsEvent.handleEvent >> Illegal context! Aborting.");
                return;
            }
            else if (svcContext.Type != ServiceContext.ServiceType.SOAP)
            {
                Logger.WriteWarning("Operation only suitable for SOAP Services!");
                return;
            }

            // Ask the user whether he/she really wants to delete the operation...
            using (var dialog = new ConfirmOperationChanges("Are you sure you want to delete Operation '" + svcContext.OperationClass.Name + "'?"))
            {
                if (svcContext.LockModel() && dialog.ShowDialog() == DialogResult.OK)
                {
                    var myService = new ApplicationService(svcContext.Hierarchy, context.GetConfigProperty(_ServiceDeclPkgStereotype));

                    // Search the model for all interfaces that have an association with the operation to be deleted...
                    var affectedInterfaces = new List<Capability>();
                    foreach (Capability cap in myService.Capabilities)
                    {
                        if (cap.CapabilityClass.HasStereotype(interfaceContractClassStereotype))
                        {
                            foreach (Capability itfCap in cap.Children)
                            {
                                if (itfCap.CapabilityClass.HasStereotype(context.GetConfigProperty(_OperationClassStereotype)) && itfCap.CapabilityClass.Name == svcContext.OperationClass.Name)
                                {
                                    Logger.WriteInfo("Plugin.Application.Events.API.DeleteOperationsEvent.handleEvent >> Found affected interface '" + cap.Name + "'...");
                                    affectedInterfaces.Add(cap);
                                    break;
                                }
                            }
                        }
                    }

                    // Now that we have a list of owning interfaces, we tell the first one in the list to delete the operation. This will remove all packages,
                    // elements and associations for all other interfaces as well...
                    if (affectedInterfaces.Count > 0)
                    {
                        if (affectedInterfaces.Count > 1)
                        {
                            // This operation is associated with multiple interfaces, we have to ask the user whether it must be deleted from ALL
                            // or SOME interfaces...
                            using (var picker = new CapabilityPicker("Select Interface(s) from which to delete operation:", affectedInterfaces, false, false))
                            {
                                if (picker.ShowDialog() == DialogResult.OK)
                                {
                                    List<InterfaceCapability> selectedInterfaces = picker.GetCheckedCapabilities().ConvertAll(Converter);
                                    if (selectedInterfaces.Count == 0) return;    // Nothing selected, treat as cancel.
                                    if (selectedInterfaces.Count < affectedInterfaces.Count)
                                    {
                                        // Only remove from SOME interfaces means that we must not actually delete the operation resources!
                                        Logger.WriteInfo("Plugin.Application.Events.API.DeleteOperationsEvent.handleEvent >> Going to disassociate instead of delete!");
                                        deleteResources = false;
                                        affectedInterfaces = selectedInterfaces.ConvertAll(BaseConverter);
                                    }
                                }
                                else
                                {
                                    Logger.WriteInfo("Plugin.Application.Events.API.DeleteOperationsEvent.handleEvent >> Cancelling operation!");
                                    return;
                                }
                            }
                        }

                        // We tell each interface in turn to delete the operation. The first one will perform the actual delete, all the others
                        // will find the operation classes to have gone already and just update their versions as well as logging info.
                        bool isFirst = true;
                        foreach (InterfaceCapability itf in affectedInterfaces)
                        {
                            itf.DeleteOperation(svcContext.OperationClass, dialog.MinorVersionIndicator, deleteResources);
                            interfaceNames += (!isFirst) ? ", " + itf.Name : itf.Name;
                            isFirst = false;
                        }
                        if (dialog.MinorVersionIndicator) myService.UpdateVersion(new Tuple<int, int>(myService.Version.Item1, myService.Version.Item2 + 1));
                        myService.CreateLogEntry("Deleted operation: '" + svcContext.OperationClass.Name + "' from interface(s): '" + interfaceNames + "'.");
                        
                        // Mark service as 'modified' for configuration management and add to diagram in different color...
                        myService.Dirty();
                        myService.Paint(svcContext.MyDiagram);
                        svcContext.Refresh();
                    }
                    else Logger.WriteError("Plugin.Application.Events.API.DeleteOperationsEvent.handleEvent >> Unable to delete operation '" + 
                                           svcContext.OperationClass.Name + "', aborting!");
                }
                svcContext.UnlockModel();
            }
        }

        /// <summary>
        /// Helper function that aids in the conversion of base-class Capabilities to InterfaceCapabilities.
        /// It effectively does nothing since an InterfaceCapability 'is' a Capability but the Lists do not know that ;-)
        /// </summary>
        /// <param name="cap">Capability base.</param>
        /// <returns>Actual derived InterfaceCapability object.</returns>
        private Capability BaseConverter(InterfaceCapability cap)
        {
            return cap;
        }

        /// <summary>
        /// Helper function that aids in the conversion of base-class Capabilities to InterfaceCapabilities.
        /// </summary>
        /// <param name="cap">Capability base.</param>
        /// <returns>Actual derived InterfaceCapability object.</returns>
        private InterfaceCapability Converter(Capability cap)
        {
            return cap as InterfaceCapability;
        }
    }
}
