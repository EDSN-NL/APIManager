using System.Windows.Forms;
using Framework.Event;
using Framework.Context;
using Framework.Util;
using Plugin.Application.Forms;

namespace Plugin.Application.Events.Util
{
    class CapabilityDocumentationEvent : EventImplementation
    {
        /// <summary>
        /// Applicability has already been checked by the basic Event framework. No additional checks are required.
        /// </summary>
        /// <returns></returns>
        internal override bool IsValidState() { return true; }

        /// <summary>
        /// This event retrieves the -RTF-formatted- documentation section of the current Capability class and presents this in a
        /// simple editor dialog. After modification by the user, the updated documentation is written back to the class.
        /// </summary>
        internal override void HandleEvent()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string caption = "Edit documentation for class: '" + context.CurrentClass.Name + "'...";
            using (var capDoc = new CapabilityDocumentation(caption, MEChangeLog.GetRTFDocumentation(context.CurrentClass)))
            {
                if (capDoc.ShowDialog() == DialogResult.OK) MEChangeLog.SetRTFDocumentation(context.CurrentClass, capDoc.Documentation);
            }
        }
    }
}
