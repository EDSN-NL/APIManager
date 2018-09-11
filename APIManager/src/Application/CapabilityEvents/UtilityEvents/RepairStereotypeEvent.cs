using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Event;
using Framework.Context;
using Framework.Model;
using Framework.Logging;
using Plugin.Application.Forms;

namespace Plugin.Application.Events.Util
{
    /// <summary>
    /// Recursively parses a collection of packages, checking and optionally repairing the specified fully-qualified stereotype name. 
    /// </summary>
    class RepairStereotypeEvent: EventImplementation
    {
        /// <summary>
        /// Method that checks the state of this event. For this repair action, we must have a current package in order to succeed.
        /// </summary>
        /// <returns>True.</returns>
        internal override bool IsValidState()
        {
            return ContextSlt.GetContextSlt().CurrentPackage != null;
        }

        /// <summary>
        /// This is a simple utility event that requests a stereotype from the user and subsequently checks whether all elements
        /// in the current package hierarchy have the proper stereotype definition (that is, profile-name::stereotype-name).
        /// </summary>
        internal override void HandleEvent()
        {
            MEPackage currentPackage = ContextSlt.GetContextSlt().CurrentPackage;
            if (ModelSlt.GetModelSlt().LockModel(currentPackage))
            {
                using (var dialog = new RepairStereotypeInput())
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        currentPackage.RepairStereotype(dialog.Stereotype, dialog.CheckHierarchy);
                        MessageBox.Show("Done checking stereotypes!");
                    }
                }
                ModelSlt.GetModelSlt().UnlockModel(currentPackage);
            }
        }
    }
}
