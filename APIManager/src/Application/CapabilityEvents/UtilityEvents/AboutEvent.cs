using Framework.Event;
using Plugin.Application.Forms;

namespace Plugin.Application.Events.Util
{
    /// <summary>
    /// Simple event implementation that displays the "about box" dialogue.
    /// </summary>
    class AboutEvent: EventImplementation
    {
        /// <summary>
        /// Method that checks the state of this event. For AboutEvent, the state is always valid.
        /// </summary>
        /// <returns>True.</returns>
        internal override bool IsValidState() { return true; }

        /// <summary>
        /// The AboutEvent is a very simple event that just displays the 'about' dialogue box. No further dialog is required.
        /// </summary>
        internal override void HandleEvent()
        {
            using (var box = new AboutBox()) box.ShowDialog();
        }
    }
}
