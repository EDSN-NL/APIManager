using System;
using System.Windows.Forms;
using Framework.Event;
using Framework.Logging;
using Framework.Context;
using Framework.ConfigurationManagement;
using Plugin.Application.CapabilityModel;
using Plugin.Application.Forms;
using Plugin.Application.Events.API;

namespace Plugin.Application.Events.Util
{
    /// <summary>
    /// </summary>
    class ReleaseServiceEvent : EventImplementation
    {
        /// <summary>
        /// Checks whether we can process the event in the current context. Since this context is already clearly defined by the 'Service'
        /// stereotype, we only return 'false' when configuration management is generally disabled.
        /// </summary>
        /// <returns>True.</returns>
        internal override bool IsValidState()
        {
            RepositoryDescriptor repoDsc = CMRepositoryDscManagerSlt.GetRepositoryDscManagerSlt().GetCurrentDescriptor();
            if (repoDsc == null) Logger.WriteWarning("Unable to retrieve a matching CM Repository configuration!");
            return (repoDsc != null && repoDsc.IsCMEnabled) ? true : false;
        }

        /// <summary>
        /// Processes the 'Release Service' message, which will push service CI's to the central Configuration Management repository.
        /// If the service has not yet been committed (to the local repository), you can not release it!
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.API.ReleaseServiceEvent.HandleEvent >> Message processing...");
            ContextSlt context = ContextSlt.GetContextSlt();
            var svcContext = new ServiceContext(this._event.Scope == TreeScope.Diagram);
            string errorMsg = string.Empty;
            Service myService = null;

            // Perform a series of precondition tests...
            if (!svcContext.Valid) errorMsg = "Illegal or corrupt context, operation aborted!";
            else if (!svcContext.LockModel()) errorMsg = "Unable to lock the model!";

            if (errorMsg != string.Empty)
            {
                Logger.WriteError("Plugin.Application.Events.API.ReleaseServiceEvent.HandleEvent >> " + errorMsg);
                MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                svcContext.UnlockModel();
                return;
            }

            try
            {
                myService = svcContext.GetServiceInstance();
                if (myService.IsValidCMState(CMContext.CMState.Released))
                {
                    if (myService.ConfigurationMgmtState != CMContext.CMState.Released)
                    {
                        using (var dialog = new CMReleaseMessage())
                        {
                            if (dialog.ShowDialog() == DialogResult.OK)
                            {
                                myService.Release(dialog.Annotation);
                                MessageBox.Show("Successfully released service '" + myService.Name + "'.",
                                                "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                    else MessageBox.Show("Service is already released!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else MessageBox.Show("Service must be committed before it can be released!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exc)
            {
                string msg = "Unable to release service '" + myService.Name + "' because: " + Environment.NewLine + exc.Message;
                MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.WriteError("Plugin.Application.Events.API.ReleaseServiceEvent.HandleEvent >> " + msg + Environment.NewLine + exc.ToString());
            }
            myService.Paint(context.CurrentDiagram);
            svcContext.UnlockModel();
        }
    }
}
