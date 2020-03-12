using System.Windows.Forms;
using Framework.Event;
using Framework.Logging;
using Framework.ConfigurationManagement;

namespace Plugin.Application.Events.Util
{
    /// <summary>
    /// Processes a 'Synchronise Tags' event, which removes ALL tags from the local repository and subsequently loads a new set from remote.
    /// </summary>
    class SynchroniseTagsEvent : MenuEventImplementation
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
        /// Generic repository maintenance event: deletes all local tags and subsequently reads a fresh set from remote.
        /// We link the event to a Service, not because we need the service, but because ALL CM operations are service-specific and we
        /// like to be consistent in behavior.
        /// If Configuration Management is disabled, the operation has no effect.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.API.RevertServiceEvent.SynchroniseTags >> Message processing...");
            CMRepositorySlt repo = CMRepositorySlt.GetRepositorySlt();

            if (repo.IsCMEnabled)
            {
                if (MessageBox.Show("This deletes ALL local tags and fetches a new set from remote. Are you sure?", 
                                    "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    Logger.WriteInfo("Plugin.Application.Events.API.RevertServiceEvent.SynchroniseTags >> Synchronising tags...");
                    repo.SynchroniseTags();
                    MessageBox.Show("Finished synchronising tags.");
                }
            }
        }
    }
}
