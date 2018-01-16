using Framework.Event;
using Framework.Logging;
using Framework.Model;
using Framework.Context;
using Plugin.Application.CapabilityModel;

namespace Plugin.Application.Events.API
{
    class ConsistencyCheckEvent : EventImplementation
    {
        // Configuration properties used by this module:
        private const string _AssociationStereotype         = "AssociationStereotype";
        private const string _MessageAssociationStereotype  = "MessageAssociationStereotype";

        private ProgressPanelSlt _panel;

        /// <summary>
        /// No specific validation, we basically accept all locations.
        /// </summary>
        /// <returns>True</returns>
        internal override bool IsValidState() {return true;}

        /// <summary>
        /// Process the "Set Metadata Event", which assigns a DEN (Dictionary Entry Name) and a UniqueID to either a single selected class, 
        /// or all classes in a selected package. When processing a class, the class itself, all attributes and all outbound associations 
        /// (with the exception of generalization) are given DEN and UniqueID. The DEN is (as much as possible) based on UN/CEFACT naming 
        /// conventions for DEN's.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.API.ConsistencyCheckEvent.HandleEvent >> Processing event...");
            var svcContext = new ServiceContext(this._event.Scope == TreeScope.Diagram);
            this._panel = ProgressPanelSlt.GetProgressPanelSlt();
            this._panel.ShowPanel("Check Association Consistency", 8);
            bool needUpdate = false;

            if (svcContext.Valid && svcContext.LockModel())
            {
                MEClass root = svcContext.ServiceClass;
                this._panel.WriteInfo(0, "Checking Service: '" + root.Name + "'...");
                this._panel.IncreaseBar(1);
                foreach (MEAssociation assoc in root.AssociationList)
                {
                    if (assoc.TypeOfAssociation == MEAssociation.AssociationType.Generalization) continue; // Skip Generalizations!
                    needUpdate = CheckInterface("Interface", assoc);
                    MEClass itf = assoc.Destination.EndPoint;
                    this._panel.WriteInfo(2, "Checking interface: '" + itf.Name + "'...");
                    foreach (MEAssociation assoc2 in itf.AssociationList)
                    {
                        if (assoc2.TypeOfAssociation == MEAssociation.AssociationType.Generalization) continue;
                        needUpdate = CheckInterface("Operation", assoc2);
                        MEClass oper = assoc2.Destination.EndPoint;
                        this._panel.WriteInfo(4, "Checking operation: '" + oper.Name + "'...");
                        foreach (MEAssociation assoc3 in oper.AssociationList)
                        {
                            if (assoc3.TypeOfAssociation == MEAssociation.AssociationType.Generalization) continue;
                            needUpdate = CheckInterface("Message", assoc3);
                            MEClass msg = assoc3.Destination.EndPoint;
                            this._panel.WriteInfo(6, "Checking message: '" + msg.Name + "'...");
                            foreach (MEAssociation assoc4 in msg.AssociationList)
                            {
                                if (assoc4.TypeOfAssociation == MEAssociation.AssociationType.Generalization) continue;
                                needUpdate = CheckInterface("Assembly", assoc4);
                            }
                        }
                    }
                }
                if (needUpdate) svcContext.Refresh();
                this._panel.Done();
            }
            svcContext.UnlockModel();
        }

        /// <summary>
        /// Helper function that performs the actual check of the interface.
        /// </summary>
        /// <param name="label">Scope identifier for reporting.</param>
        /// <param name="assoc">The association to check.</param>
        /// <returns></returns>
        private bool CheckInterface (string label, MEAssociation assoc)
        {
            bool needUpdate = false;
            ContextSlt context = ContextSlt.GetContextSlt();
            string messageStereotype = context.GetConfigProperty(_MessageAssociationStereotype);
            string assocStereotype = context.GetConfigProperty(_AssociationStereotype);

            if (assoc.HasStereotype(assocStereotype, MEAssociation.AssociationEnd.Association))
            {
                this._panel.WriteWarning(2, label + " association has wrong _ASBIE Stereotype, fixing!");
                assoc.DeleteStereotype(assocStereotype, MEAssociation.AssociationEnd.Association);
                needUpdate = true;
            }

            if (!assoc.HasStereotype(messageStereotype, MEAssociation.AssociationEnd.Association))
            {
                this._panel.WriteWarning(2, label + " association is missing ASMBIE Stereotype, fixing!");
                assoc.AddStereotype(messageStereotype, MEAssociation.AssociationEnd.Association);
                needUpdate = true;
            }

            this._panel.IncreaseBar(1);
            return needUpdate;
        }
    }
}
