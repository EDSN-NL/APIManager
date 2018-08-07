using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Framework.Event;
using Framework.Logging;
using Framework.Context;
using Framework.ConfigurationManagement;
using Plugin.Application.CapabilityModel;
using Plugin.Application.CapabilityModel.API;
using Plugin.Application.CapabilityModel.CodeList;
using Plugin.Application.Events.API;
using Plugin.Application.Forms;

namespace Plugin.Application.Events.Util
{
    /// <summary>
    /// Process an explicit 'service checkout' event.
    /// </summary>
    class RevertServiceEvent : EventImplementation
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
        /// Performs a service 'checkout', which prepares a service for processing. Typically, this is done explicitly when performing a
        /// 'process service' event. However, users can perform an explicit checkout to assure that CM contains the appropriate branches and
        /// to check whether no pending changes from other services prevent processing of the selected service.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.API.RevertServiceEvent.HandleEvent >> Message processing...");
            ContextSlt context = ContextSlt.GetContextSlt();
            var svcContext = new ServiceContext(this._event.Scope == TreeScope.Diagram);

            if (!svcContext.Valid)
            {
                Logger.WriteError("Plugin.Application.Events.API.RevertServiceEvent.HandleEvent >> Illegal or corrupt context, event aborted!");
                return;
            }

            try
            {
                if (svcContext.LockModel())
                {
                    Service myService = svcContext.GetServiceInstance();
                    List<string> releases = myService.CMContext.GetReleaseTags();
                    if (releases.Count > 0)
                    {
                        using (var pickerDialog = new CMRevertPicker(releases))
                        {
                            if (pickerDialog.ShowDialog() == DialogResult.OK)
                            {
                                MessageBox.Show("Selected tag = " + pickerDialog.SelectedTag + ", with version: " + pickerDialog.AssignedVersion.Item1 + "." + pickerDialog.AssignedVersion.Item2 + "." + pickerDialog.AssignedVersion.Item3);
                            }
                        }
                    }
                    else MessageBox.Show("Nothing to revert to!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    svcContext.UnlockModel();
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.Events.API.RevertServiceEvent.HandleEvent >> Caught an exception during service creation: " + Environment.NewLine + exc.Message);
                return;
            }
        }
    }
}
