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
    class AddOperationsEvent : EventImplementation
    {
        // Configuration properties used by this module...
        private const string _ServiceDeclPkgStereotype          = "ServiceDeclPkgStereotype";
        private const string _ServiceClassStereotype            = "ServiceClassStereotype";
        private const string _ServiceModelPkgName               = "ServiceModelPkgName";
        private const string _InterfaceContractClassStereotype  = "InterfaceContractClassStereotype";
        private const string _RequestMessageSuffix              = "RequestMessageSuffix";
        private const string _ResponseMessageSuffix             = "ResponseMessageSuffix";

        // Keep track of (extra) classes and associations to show in the diagram...
        private List<MEClass> _diagramClassList = new List<MEClass>();
        private List<MEAssociation> _diagramAssocList = new List<MEAssociation>();

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

            if (!svcContext.Valid || svcContext.MyDiagram == null)
            {
                Logger.WriteError("Plugin.Application.Events.API.AddOperationsEvent.HandleEvent >> Illegal or corrupt context, event aborted!");
                return;
            }
            else if (svcContext.Type != ServiceContext.ServiceType.SOAP)
            {
                Logger.WriteWarning("Plugin.Application.Events.API.AddOperationsEvent.HandleEvent >> Operation only suitable for SOAP Services!");
                return;
            }

            // If no interface has been selected, we simply take the first one available...
            if (interfaceClass == null) interfaceClass = svcContext.InterfaceList[0];   

            using (var dialog = new AddOperationInput(svcContext.DeclarationPackage))
            {
                if (svcContext.LockModel() && dialog.ShowDialog() == DialogResult.OK)
                {
                    // Creating the service will build the entire object hierarchy. Subsequently, we can create Capabilities by class alone,
                    // in which case we will associate the interface class with the existing hierarchy.
                    var myService = new ApplicationService(svcContext.Hierarchy, context.GetConfigProperty(_ServiceDeclPkgStereotype));
                    var myInterface = new InterfaceCapability(interfaceClass);

                    if (myInterface.AddOperations(dialog.OperationList, dialog.MinorVersionIndicator))
                    {
                        // Collect the classes and associations that must be shown on the diagram...
                        this._diagramClassList = new List<MEClass>();
                        this._diagramAssocList = new List<MEAssociation>();
                        myInterface.Traverse(DiagramItemsCollector);

                        svcContext.MyDiagram.AddClassList(this._diagramClassList);
                        svcContext.MyDiagram.AddAssociationList(this._diagramAssocList);
                        svcContext.MyDiagram.Redraw();
                        svcContext.DeclarationPackage.Refresh();
                        MessageBox.Show("Operations have been added successfully.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else MessageBox.Show("Failed to add one or more operations!", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                svcContext.UnlockModel();
            }
        }

        /// <summary>
        /// Helper function that is invoked by the capability hierarchy traversal for each node in the hierarchy, starting at the Interface
        /// and subsequently invoked for each subordinate capability (Operation and Message). 
        /// The function collects items that must be displayed on the updated ServiceModel diagram.
        /// </summary>
        /// <param name="svc">My parent service.</param>
        /// <param name="cap">The current Capability.</param>
        /// <returns>Always 'false', which indicates that traversal must continue until all nodes are processed.</returns>
        private bool DiagramItemsCollector(Service svc, Capability cap)
        {
            if (cap != null) // Safety catch, must not be NULL since we start at capability level.   
            {
                Logger.WriteInfo("Plugin.Application.Events.API.AddOperationsEvent.DiagramItemsCollector >> Traversing capability '" + cap.Name + "'...");
                if (cap is MessageCapability)
                {
                    if (ContextSlt.GetContextSlt().GetBoolSetting(FrameworkSettings._SMAddBusinessMsgToDiagram))
                    {
                        this._diagramClassList.Add(cap.CapabilityClass);
                        // We're at the message level, we might (optionally) collect the Message Assembly component(s)...
                        if (ContextSlt.GetContextSlt().GetBoolSetting(FrameworkSettings._SMAddMessageAssemblyToDiagram))
                        {
                            foreach (MEAssociation assoc in cap.CapabilityClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation))
                            {
                                this._diagramAssocList.Add(assoc);
                                this._diagramClassList.Add(assoc.Destination.EndPoint);
                            }
                        }
                    }
                }
                else
                {
                    // In all other cases, we simply collect child associations if available...
                    this._diagramClassList.Add(cap.CapabilityClass);
                    foreach (MEAssociation assoc in cap.CapabilityClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation)) this._diagramAssocList.Add(assoc);
                }
            }
            return false;
        }
    }
}
