using System;
using System.Windows.Forms;
using Framework.Event;
using Framework.Logging;
using Framework.ConfigurationManagement;

namespace Plugin.Application.Events.Util
{
    /// <summary>
    /// Performs a 'pull' of the remote repository to the local release branch.
    /// </summary>
    class SynchroniseRepositoryEvent : MenuEventImplementation
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
        /// Processes the 'Synchronise Repository' message, which will pull any changed content from the remote repository.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.API.SynchroniseRepositoryEvent.HandleEvent >> Message processing...");

            try
            {
                CMRepositorySlt repository = CMRepositorySlt.GetRepositorySlt();
                repository.GotoBranch(repository.ReleaseBranchName);
                repository.Pull();
            }
            catch (Exception exc)
            {
                string msg = "Unable to synchronise repository because: " + Environment.NewLine + exc.Message;
                MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.WriteError("Plugin.Application.Events.API.SynchroniseRepositoryEvent.HandleEvent >> " + msg + Environment.NewLine + exc.ToString());
            }
        }
    }
}
