using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Event;
using Framework.Context;
using Framework.Model;

namespace Plugin.Application.Events.Util
{
    /// <summary>
    /// Retrieves an fully-qualified stereotype name from a selected class and issues a 'sync-stereotype' to the repository. 
    /// </summary>
    class SyncStereotypesEvent: MenuEventImplementation
    {
        /// <summary>
        /// Method that checks the state of this event. In our case, as long as a class has been selected, we're good.
        /// </summary>
        /// <returns>True when a selected class exists.</returns>
        internal override bool IsValidState() 
        { 
            return ContextSlt.GetContextSlt().CurrentClass != null; 
        }

        /// <summary>
        /// This is a very simple utility event that retrieves the fully-qualified stereotype(s) from the currently selected
        /// class and issues a 'synchronize stereotypes' for them, updating definitions and tags.
        /// </summary>
        internal override void HandleEvent()
        {
            MEClass currentClass = ContextSlt.GetContextSlt().CurrentClass;
            if (currentClass != null)
            {
                List<string> stereoTypes = currentClass.FQStereotypes;
                string names = string.Empty;
                bool firstOne = true;
                foreach (string s in stereoTypes)
                {
                    names += firstOne ? s : (", " + s);
                    firstOne = false;
                }
                if (MessageBox.Show("Going to synchronize stereotype(s) '" + names + "' from class '" + currentClass.Name + 
                                    "'. Are you sure?", "Info", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    foreach (string s in stereoTypes) ModelSlt.GetModelSlt().SynchronizeStereotype(s);
                }
            }
        }
    }
}
