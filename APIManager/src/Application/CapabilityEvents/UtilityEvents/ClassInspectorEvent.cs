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
    /// Displays the 'Settings' dialogue box that allows users to change settings dynamically. 
    /// </summary>
    class ClassInspectorEvent: EventImplementation
    {
        /// <summary>
        /// Method that checks the state of this event. For SettingsEvent, the state is always valid.
        /// </summary>
        /// <returns>True.</returns>
        internal override bool IsValidState() { return true; }

        /// <summary>
        /// This is a very simple utility event that retrieves the meta data of the currently selected class and shows this in a dialog.
        /// </summary>
        internal override void HandleEvent()
        {
            MEClass currentClass = ContextSlt.GetContextSlt().CurrentClass;
            if (currentClass != null)
            {
                ClassInspector dialog = new ClassInspector(currentClass.Metadata);
                dialog.ShowDialog();
            }
        }
    }
}
