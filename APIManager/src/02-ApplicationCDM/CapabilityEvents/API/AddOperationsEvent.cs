﻿using System;
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
    class AddOperationsEvent : MenuEventImplementation
    {
        // Configuration properties used by this module...
        private const string _ServiceDeclPkgStereotype          = "ServiceDeclPkgStereotype";
        private const string _ServiceClassStereotype            = "ServiceClassStereotype";
        private const string _ServiceModelPkgName               = "ServiceModelPkgName";
        private const string _InterfaceContractClassStereotype  = "InterfaceContractClassStereotype";
        private const string _RequestMessageSuffix              = "RequestMessageSuffix";
        private const string _ResponseMessageSuffix             = "ResponseMessageSuffix";

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
        /// Processes the 'Add Operations' menu click event.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.API.AddOperationsEvent.HandleEvent >> Processing an add operations menu click...");

            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            var svcContext = new ServiceContext(this._event.Scope == TreeScope.Diagram);
            MEClass interfaceClass = svcContext.InterfaceClass;
            string interfaceContractClassStereotype = context.GetConfigProperty(_InterfaceContractClassStereotype);
            string errorMsg = string.Empty;
            bool isError = false;

            // Perform a series of precondition tests...
            if (!svcContext.Valid || svcContext.MyDiagram == null)
            {
                if (!svcContext.HasValidRepositoryDescriptor) errorMsg = "No valid Repository Descriptor has been defined for the currently open project!";
                else errorMsg = "Illegal or corrupt context, operation aborted!";
                isError = true;
            }
            else if (svcContext.Type != Service.ServiceArchetype.SOAP) errorMsg = "Operation only suitable for SOAP Services!";
            else if (!Service.UpdateAllowed(svcContext.ServiceClass)) errorMsg = "Service must be in checked-out state for operations to be added!";
            else if (!svcContext.LockModel())
            {
                errorMsg = "Unable to lock the model!";
                isError = true;
            }

            if (errorMsg != string.Empty)
            {
                if (isError)
                {
                    Logger.WriteError("Plugin.Application.Events.API.AddOperationsEvent.HandleEvent >> " + errorMsg);
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

            // If no interface has been selected, we simply take the first one available...
            if (interfaceClass == null) interfaceClass = svcContext.InterfaceList[0];

            using (var dialog = new AddOperationInput(svcContext.DeclarationPackage))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    // Creating the service will build the entire object hierarchy. Subsequently, we can create Capabilities by class alone,
                    // in which case we will associate the interface class with the existing hierarchy.
                    var myService = new ApplicationService(svcContext.Hierarchy, context.GetConfigProperty(_ServiceDeclPkgStereotype));
                    var myInterface = new InterfaceCapability(interfaceClass);
                    if (myInterface.AddOperations(dialog.OperationList, dialog.MinorVersionIndicator))
                    {
                        // Mark service as 'modified' for configuration management and add to diagram in different color...
                        myService.Dirty();
                        myService.Paint(svcContext.ServiceDiagram);

                        // Collect the classes and associations that must be shown on the diagram...
                        DiagramItemsCollector collector = new DiagramItemsCollector(svcContext.MyDiagram);
                        myInterface.Traverse(collector.Collect);

                        svcContext.MyDiagram.AddClassList(collector.DiagramClassList);
                        svcContext.MyDiagram.AddAssociationList(collector.DiagramAssociationList);
                        svcContext.MyDiagram.Redraw();
                        svcContext.DeclarationPackage.RefreshPackage();
                        MessageBox.Show("Operations have been added successfully.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else MessageBox.Show("Failed to add one or more operations!", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                svcContext.UnlockModel();
            }
        }
    }
}
