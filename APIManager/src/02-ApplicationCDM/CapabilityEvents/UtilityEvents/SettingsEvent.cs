using Framework.Logging;
using Framework.Event;
using Plugin.Application.Forms;

namespace Plugin.Application.Events.Util
{
    /// <summary>
    /// Displays the 'Settings' dialogue box that allows users to change settings dynamically. 
    /// </summary>
    class SettingsEvent: MenuEventImplementation
    {
        /// <summary>
        /// Method that checks the state of this event. For SettingsEvent, the state is always valid.
        /// </summary>
        /// <returns>True.</returns>
        internal override bool IsValidState() { return true; }

        /// <summary>
        /// The SettingsEvent facilitates dynamic propery changes. Currently, it only supports changing of logfile.
        /// </summary>
        internal override void HandleEvent()
        {
            // Create and show the settings form. This will update the user configuration settings.
            Logger.WriteInfo("Framework.Event.Util.SettingsEvent.handleEvent >> Showing settings dialogue with user...");
            using (var settingsForm = new SettingsForm())
            {
                // All necessary updates are performed by the dialog so there is no additional processing in here!
                settingsForm.ShowDialog();
            }
        }
    }
}
