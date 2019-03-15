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
    class CommitServiceEvent : EventImplementation
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
        /// Processes the 'Commit Service' message, which will push service CI's to the local Configuration Management repository.
        /// If the service has not yet been processed (i.e. no CI's have been produced), you can not commit it.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.API.CommitServiceEvent.HandleEvent >> Message processing...");
            ContextSlt context = ContextSlt.GetContextSlt();
            var svcContext = new ServiceContext(this._event.Scope == TreeScope.Diagram);
            string errorMsg = string.Empty;
            Service myService = null;
            bool releaseLocked = false;

            // Perform some precondition tests...
            if (!svcContext.Valid) errorMsg = "Illegal or corrupt context, operation aborted!";
            else if (!svcContext.LockModel()) errorMsg = "Unable to lock the model!";

            if (errorMsg != string.Empty)
            {
                Logger.WriteError("Plugin.Application.Events.API.CommitServiceEvent.HandleEvent >> " + errorMsg);
                MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                svcContext.UnlockModel();
                return;
            }

            try
            {
                myService = svcContext.GetServiceInstance();
                if (myService.IsValidCMState(CMContext.CMState.Committed))
                {
                    if (myService.ConfigurationMgmtState != CMContext.CMState.Committed)
                    {
                        using (var dialog = new CMCommitMessage())
                        {
                            if (dialog.ShowDialog() == DialogResult.OK)
                            {
                                if (dialog.CommitScope == CMContext.CommitScope.Release && (releaseLocked = svcContext.LockReleaseHistory() == false))
                                {
                                    errorMsg = "Unable to lock the release history package!";
                                    Logger.WriteError("Plugin.Application.Events.API.CommitServiceEvent.HandleEvent >> " + errorMsg);
                                    MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    svcContext.UnlockModel();
                                    return;
                                }

                                if (myService.Commit(dialog.Annotation, dialog.CommitScope))
                                {
                                    MessageBox.Show("Successfully committed service '" + myService.Name + "'.",
                                                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else MessageBox.Show("Service '" + myService.Name + "' has been committed without any changes!",
                                                     "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                    else MessageBox.Show("Service is already committed!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else MessageBox.Show("Service must be new or checked-out before it can be committed!",
                                     "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exc)
            {
                string msg = "Unable to commit service '" + myService.Name + "' because: " + Environment.NewLine + exc.Message;
                MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.WriteError("Plugin.Application.Events.API.CommitServiceEvent.HandleEvent >> " + msg + Environment.NewLine + exc.ToString());
            }
            myService.Paint(context.CurrentDiagram);
            svcContext.UnlockModel();
            if (releaseLocked) svcContext.UnlockReleaseHistory();
        }
    }
}
