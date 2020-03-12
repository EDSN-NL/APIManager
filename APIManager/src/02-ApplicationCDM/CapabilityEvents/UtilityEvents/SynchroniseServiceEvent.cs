using System;
using System.Windows.Forms;
using Framework.Event;
using Framework.Logging;
using Framework.ConfigurationManagement;
using Framework.Context;
using Plugin.Application.CapabilityModel;
using Plugin.Application.Events.API;

namespace Plugin.Application.Events.Util
{
    /// <summary>
    /// Activates the current service branch (if one is present) and performs a pull from remote.
    /// </summary>
    class SynchroniseServiceEvent : MenuEventImplementation
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
        /// Processes the 'Synchronise Service' message, which will pull any changed content from the service branch on the remote
        /// repository, but only when such a branch exists.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.API.SynchroniseServiceEvent.HandleEvent >> Message processing...");

            ContextSlt context = ContextSlt.GetContextSlt();
            var svcContext = new ServiceContext(this._event.Scope == TreeScope.Diagram);
            string errorMsg = string.Empty;
            Service myService = null;

            // Perform some precondition tests...
            if (!svcContext.Valid)
            {
                if (!svcContext.HasValidRepositoryDescriptor) errorMsg = "No valid Repository Descriptor has been defined for the currently open project!";
                else errorMsg = "Illegal or corrupt context, operation aborted!";
            }
            else if (!svcContext.LockModel()) errorMsg = "Unable to lock the model!";

            if (errorMsg != string.Empty)
            {
                Logger.WriteError("Plugin.Application.Events.API.SynchroniseServiceEvent.HandleEvent >> " + errorMsg);
                MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                svcContext.UnlockModel();
                return;
            }

            try
            {
                svcContext.GetServiceInstance().SyncWithRemote();
                MessageBox.Show("Service has been synchronised successfully.");
            }
            catch (Exception exc)
            {
                string msg = "Unable to synchronise service '" + myService.Name + "' because: " + Environment.NewLine + exc.Message;
                MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.WriteError("Plugin.Application.Events.API.SynchroniseServiceEvent.HandleEvent >> " + msg + Environment.NewLine + exc.ToString());
            }
            svcContext.UnlockModel();
        }
    }
}
