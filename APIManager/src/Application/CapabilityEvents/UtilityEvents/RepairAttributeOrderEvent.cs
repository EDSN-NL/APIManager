using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Event;
using Framework.Context;
using Framework.Model;
using Framework.Logging;

namespace Plugin.Application.Events.Util
{
    /// <summary>
    /// Displays the 'Settings' dialogue box that allows users to change settings dynamically. 
    /// </summary>
    class RepairAttributeOrderEvent: EventImplementation
    {
        private List<string> _processedClasses;

        /// <summary>
        /// Method that checks the state of this event. For SettingsEvent, the state is always valid.
        /// </summary>
        /// <returns>True.</returns>
        internal override bool IsValidState() { return true; }

        /// <summary>
        /// This is a simple utility event that enforces all attributes in the selected class, or hierarchy of classes, to have a proper position identifier.
        /// </summary>
        internal override void HandleEvent()
        {
            MEClass currentClass = ContextSlt.GetContextSlt().CurrentClass;
            string message = "Process entire hierarchy from '" + currentClass.Name + "' onwards?";
            if (MessageBox.Show(message, "Repair Scope", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this._processedClasses = new List<string>();
                ProcessClass(currentClass);
                MessageBox.Show("All classes have been checked and repaired where needed.");
            }
            else
            {
                // Single class processing, first we check the status of the class by invoking the repair method with 'checkOnly = true'...
                if (!currentClass.RepairAttributeOrder(true))
                {
                    message = "Attributes of class '" + currentClass.Name + "', are out of order! Repair?";
                    if (MessageBox.Show(message, "Repair attribute order", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                        currentClass.RepairAttributeOrder(false);
                }
                else MessageBox.Show("Attributes of class '" + currentClass.Name + "' are in order, no repair necessary!");
            }
        }

        /// <summary>
        /// Determine the (list of) classes connected to the provided class and invoke ProcessClass on each of them (unless whe have processed the class before).
        /// </summary>
        /// <param name="node">Current position in class model.</param>
        private void ProcessAssociations(MEClass node)
        {
            Logger.WriteInfo("Plugin.Application.Events.Util.RepairAttributeOrderEvent.ProcessAssociations >> Processing class: " + node.Name + "...");
            foreach (MEAssociation assoc in node.AssociationList)
            {
                // We are ONLY processing associations of the correct type...
                if (assoc.TypeOfAssociation == MEAssociation.AssociationType.Composition ||
                    assoc.TypeOfAssociation == MEAssociation.AssociationType.MessageAssociation ||
                    assoc.TypeOfAssociation == MEAssociation.AssociationType.Aggregation ||
                    assoc.TypeOfAssociation == MEAssociation.AssociationType.Association)
                {
                    MEClass target = assoc.Destination.EndPoint;
                    if (!this._processedClasses.Contains(target.Name)) ProcessClass(assoc.Destination.EndPoint);
                }
            }
        }

        /// <summary>
        /// First check the attributes of the current class and repair if necessary. Then, iterate through all associations...
        /// </summary>
        /// <param name="currentClass">Class to be checked.</param>
        private void ProcessClass(MEClass currentClass)
        {
            SortedList<uint, MEClass> classHierarchy = currentClass.GetHierarchy();
            foreach (MEClass node in classHierarchy.Values)
            {
                Logger.WriteWarning("RepairAttributeOrder >> Checking class: " + node.Name + "...");
                this._processedClasses.Add(node.Name);  // Indicates that we have 'done' this one.
                if (!node.RepairAttributeOrder(true))
                {
                    Logger.WriteWarning("    RepairAttributeOrder >> Attributes of class '" + node.Name + "' are out of order, repairing...");
                    node.RepairAttributeOrder(false);
                }
                ProcessAssociations(node);
            }
        }
    }
}
