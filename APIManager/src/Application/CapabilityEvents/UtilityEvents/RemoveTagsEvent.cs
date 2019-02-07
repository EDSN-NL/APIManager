using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Framework.Model;
using Framework.Event;
using Framework.Logging;
using Framework.Context;
using Framework.ConfigurationManagement;
using Plugin.Application.CapabilityModel;
using Plugin.Application.Events.API;
using Plugin.Application.Forms;

namespace Plugin.Application.Events.Util
{
    /// <summary>
    /// Processes a 'Remove Tags' event, which facilitates removal of release tags for a selected Service.
    /// </summary>
    class RemoveTagsEvent : EventImplementation
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
            Logger.WriteInfo("Plugin.Application.Events.API.RevertServiceEvent.RemoveTags >> Message processing...");
            ContextSlt context = ContextSlt.GetContextSlt();
            var svcContext = new ServiceContext(this._event.Scope == TreeScope.Diagram);
            string errorMsg = string.Empty;
            Service myService = null;

            if (!svcContext.Valid)
            {
                errorMsg = "Illegal or corrupt context, operation aborted!";
                Logger.WriteError("Plugin.Application.Events.API.RemoveTagsEvent.HandleEvent >> " + errorMsg);
                MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                myService = svcContext.GetServiceInstance();
                if (myService.ConfigurationMgmtState == CMContext.CMState.Released)
                {
                    List<string> releases = myService.ReleaseTags;
                    if (releases.Count > 0)
                    {
                        using (var dialog = new CMRemoveTags(releases))
                        {
                            if (dialog.ShowDialog() == DialogResult.OK)
                            {
                                List<string> tagList = dialog.SelectedTags;
                                string tags = string.Empty;
                                foreach (string tag in tagList) tags += tag + Environment.NewLine;
                                if (MessageBox.Show("The following tags will be deleted, are you sure?" + Environment.NewLine + tags, 
                                                    "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                                {
                                    myService.DeleteTags(tagList);
                                    MessageBox.Show("Tags have been successfully deleted!");
                                }
                            }
                        }
                    }
                    else MessageBox.Show("No release tags found!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else MessageBox.Show("Service must be released for tags to be removed!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exc)
            {
                string msg = "Unable to remove tags for service '" + myService.Name + "' because: " + Environment.NewLine + exc.Message;
                MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.WriteError("Plugin.Application.Events.API.RemoveTagsEvent.HandleEvent >> " + msg + Environment.NewLine + exc.ToString());
            }
            myService.Paint(svcContext.MyDiagram);
            svcContext.UnlockModel();
        }
    }
}
