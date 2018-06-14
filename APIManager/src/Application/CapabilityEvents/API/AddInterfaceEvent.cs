using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Event;
using Framework.Logging;
using Framework.Model;
using Framework.Context;
using Plugin.Application.CapabilityModel.API;
using Plugin.Application.Forms;

namespace Plugin.Application.Events.API
{
    class AddInterfaceEvent : EventImplementation
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
            if (svcContext.Type != ServiceContext.ServiceType.SOAP)
            {
                Logger.WriteWarning("Plugin.Application.Events.API.AddInterfaceEvent.HandleEvent >> Operation only suitable for SOAP Services!");
                return;
            }

            if (!svcContext.Valid || svcContext.SVCModelPackage == null)
            {
                Logger.WriteError("Plugin.Application.Events.API.AddInterfaceEvent.HandleEvent >> Illegal or corrupt context, event aborted!");
                return;
            }
            else if (svcContext.Type != ServiceContext.ServiceType.SOAP)
            {
                Logger.WriteWarning("Plugin.Application.Events.API.AddInterfaceEvent.HandleEvent >> Operation only suitable for SOAP Services!");
                return;
            }

            using (var dialog = new AddInterfaceInput(svcContext.SVCModelPackage, 
                                                      svcContext.SVCModelPackage.GetClasses(context.GetConfigProperty(_OperationClassStereotype))))
            {
                if (svcContext.LockModel() && dialog.ShowDialog() == DialogResult.OK)
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
