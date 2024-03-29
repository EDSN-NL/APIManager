﻿using System.Collections.Generic;
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
    class AddInterfaceEvent : MenuEventImplementation
    {
        // Configuration properties used by this module...
        private const string _OperationClassStereotype = "OperationClassStereotype";
        private const string _ServiceDeclPkgStereotype = "ServiceDeclPkgStereotype";

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
        /// Processes the 'Add Interface' menu click event.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.SOAP.AddInterfaceEvent.handleEvent >> Processing an add interface menu click...");

            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            var svcContext = new ServiceContext(this._event.Scope == TreeScope.Diagram);
            string errorMsg = string.Empty;
            bool isError = false;

            // Perform a series of precondition tests...
            if (!svcContext.Valid || svcContext.SVCModelPackage == null)
            {
                if (!svcContext.HasValidRepositoryDescriptor) errorMsg = "No valid Repository Descriptor has been defined for the currently open project!";
                else errorMsg = "Illegal or corrupt context, operation aborted!";
                isError = true;
            }
            else if (svcContext.Type != Service.ServiceArchetype.SOAP) errorMsg = "Operation only suitable for SOAP Services!";
            else if (!Service.UpdateAllowed(svcContext.ServiceClass)) errorMsg = "Service must be in checked-out state for interfaces to be added!";
            else if (!svcContext.LockModel())
            {
                errorMsg = "Unable to lock the model!";
                isError = true;
            }

            if (errorMsg != string.Empty)
            {
                if (isError)
                {
                    Logger.WriteError("Plugin.Application.Events.API.AddInterfaceEvent.HandleEvent >> " + errorMsg);
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

            using (var dialog = new AddInterfaceInput(svcContext.SVCModelPackage, 
                                                      svcContext.SVCModelPackage.GetClasses(context.GetConfigProperty(_OperationClassStereotype))))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var myService = new ApplicationService(svcContext.Hierarchy, context.GetConfigProperty(_ServiceDeclPkgStereotype));
                    var itfCap = new InterfaceCapability(myService, dialog.InterfaceName, null);
                    itfCap.AssociateOperations(dialog.SelectedOperations, dialog.MinorVersionIndicator);
                    
                    // Mark service as 'modified' for configuration management and add to diagram in different color...
                    myService.Dirty();
                    myService.Paint(svcContext.MyDiagram);

                    // Next, collect classes and associations that have to be added to the diagram...
                    var diagramClassList = new List<MEClass>();
                    var diagramAssocList = new List<MEAssociation>();
                    diagramClassList.Add(itfCap.CapabilityClass);
                    diagramClassList.Add(itfCap.CommonSchema.CapabilityClass);

                    // Search for the association between Service and new Interface...
                    foreach (MEAssociation assoc in myService.ServiceClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation))
                    {
                        if (assoc.Destination.EndPoint == itfCap.CapabilityClass)
                        {
                            diagramAssocList.Add(assoc);
                            break;
                        }
                    }

                    // Add all associations from the new Interface (to Common Schema and Operations)...
                    foreach (MEAssociation assoc in itfCap.CapabilityClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation))
                    {
                        diagramAssocList.Add(assoc);
                    }
                    svcContext.MyDiagram.AddClassList(diagramClassList);
                    svcContext.MyDiagram.AddAssociationList(diagramAssocList);
                    svcContext.MyDiagram.Redraw();
                    svcContext.Refresh();
                }
                svcContext.UnlockModel();
            }
        }
    }
}
