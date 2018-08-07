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
        // Configuration properties used by this module...
        private const string _CommitIDLeader = "CommitIDLeader";

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

            if (!svcContext.Valid)
            {
                Logger.WriteError("Plugin.Application.Events.API.ReleaseServiceEvent.HandleEvent >> Illegal or corrupt context, event aborted!");
                return;
            }

            try
            {
                if (svcContext.LockModel())
                {
                    Service myService = svcContext.GetServiceInstance();
                    if (myService.ConfigurationMgmtState == CMState.Committed)
                    {
                        using (var dialog = new CMChangeMessage(false))
                        {
                            if (dialog.ShowDialog() == DialogResult.OK)
                            {
                                try
                                {
                                    string releaseID = context.GetConfigProperty(_CommitIDLeader) +
                                                       myService.BusinessFunctionID + ":" +
                                                       myService.ContainerPkg.Name + ":" +
                                                       myService.Name + ":" +
                                                       myService.Version.Item1 + ":" +
                                                       myService.Version.Item2 + ":" +
                                                       myService.BuildNumber;
                                    Logger.WriteInfo("Plugin.Application.Events.API.ReleaseServiceEvent.HandleEvent >> ReleaseID = '" + releaseID + "'...");
                                    myService.CMContext.ReleaseService(releaseID + Environment.NewLine + dialog.Annotation);
                                    myService.Paint(context.CurrentDiagram);
                                }
                                catch (CMOutOfSyncException)
                                {
                                    MessageBox.Show("Unable to release service to remote repository because the build number '" + myService.BuildNumber +
                                                    "'  has already been used before!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                    else MessageBox.Show("Service must be 'Committed' before it can be released!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    svcContext.UnlockModel();
                }
            }
            catch
            {
                Logger.WriteError("Plugin.Application.Events.API.ReleaseServiceEvent.HandleEvent >> Unable to determine proper context for commit!");
                return;
            }
        }
    }
}
