using System.Windows.Forms;
using Framework.Event;
using Framework.Context;
using Framework.Util;
using Framework.Logging;
using Plugin.Application.Forms;
using Plugin.Application.CapabilityModel;
using Plugin.Application.Events.API;


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
            var svcContext = new ServiceContext(this._event.Scope == TreeScope.Diagram);
            string errorMsg = string.Empty;
            bool isError = false;

            // Perform a series of precondition tests...
            if (!svcContext.Valid)
            {
                errorMsg = "Illegal or corrupt context, operation aborted!";
                isError = true;
            }
            else if (!Service.UpdateAllowed(svcContext.ServiceClass)) errorMsg = "Service must be in checked-out state for documentation to be updated!";
            else if (!svcContext.LockModel())
            {
                errorMsg = "Unable to lock the model!";
                isError = true;
            }

            if (errorMsg != string.Empty)
            {
                if (isError)
                {
                    Logger.WriteError("Plugin.Application.Events.API.CapabilityDocumentationEvent.HandleEvent >> " + errorMsg);
                    MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    Logger.WriteWarning(errorMsg);
                    MessageBox.Show(errorMsg, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                string caption = "Edit documentation for class: '" + context.CurrentClass.Name + "'...";
                using (var capDoc = new CapabilityDocumentation(caption, MEChangeLog.GetRTFDocumentation(context.CurrentClass)))
                {
                    if (capDoc.ShowDialog() == DialogResult.OK) MEChangeLog.SetRTFDocumentation(context.CurrentClass, capDoc.Documentation);
                }
            }
            svcContext.UnlockModel();
        }
    }
}
