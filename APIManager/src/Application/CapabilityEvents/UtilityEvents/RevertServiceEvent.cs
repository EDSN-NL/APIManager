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
    /// Processes a 'Revert Service' event, which restores a service to a previously released state.
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
            string errorMsg = string.Empty;
            Service myService = null;

            // Perform some precondition tests...
            if (!svcContext.Valid) errorMsg = "Illegal or corrupt context, operation aborted!";
            else if (!svcContext.LockModel()) errorMsg = "Unable to lock the model!";

            if (errorMsg != string.Empty)
            {
                Logger.WriteError("Plugin.Application.Events.API.RevertServiceEvent.HandleEvent >> " + errorMsg);
                MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                svcContext.UnlockModel();
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
                        using (var dialog = new CMRevertPicker(releases))
                        {
                            if (dialog.ShowDialog() == DialogResult.OK)
                            {
                                // Restore service based on selected tag.
                                // First of all, we import a (new) service package from Configuration Management.
                                // Since this might invalidate our current service tree, we then re-create the capability tree by
                                // creating a new service context based on the newly imported service.
                                // Finally, we update the version history in this tree using the version information that has been
                                // recorded in the importedService by the 'RestoreService' flow (might be different from the tag 
                                // version in case we decided to create a new version based on an existing one).
                                Tuple<int, int, int> tagVersion = ParseTagVersion(dialog.SelectedTag);
                                MEClass importedService = myService.RestoreService(dialog.SelectedTag, dialog.CreateNewFeatureTagVersion);
                                svcContext = new ServiceContext(importedService);
                                myService = svcContext.GetServiceInstance();
                                myService.UpdateVersion(importedService.Version);

                                // If we did not select to create a new version, we have to set the build number to the version indicated
                                // by the tag, since the UpdateVersion call will have reset it to default value!
                                if (!dialog.CreateNewFeatureTagVersion) myService.BuildNumber = tagVersion.Item3;
                                MessageBox.Show("Successfully reverted service '" + myService.Name + "' to version '" + 
                                                myService.Version.Item1 + "." + myService.Version.Item2 + "." + myService.BuildNumber + "'.");
                            }
                        }
                    }
                    else MessageBox.Show("Nothing to revert to!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else MessageBox.Show("Only released services can be reverted!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exc)
            {
                string msg = "Unable to revert service '" + myService.Name + "' because: " + Environment.NewLine + exc.Message;
                MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.WriteError("Plugin.Application.Events.API.RevertServiceEvent.HandleEvent >> " + msg + Environment.NewLine + exc.ToString());
            }
            myService.Paint(svcContext.MyDiagram);
            svcContext.UnlockModel();
        }

        /// <summary>
        /// Helper function that receives a tag and extracts the version information from it.
        /// </summary>
        /// <param name="tag">Tag to be parsed.</param>
        /// <returns>Version info as a triplet major,minor,build</returns>
        private static Tuple<int, int, int> ParseTagVersion(string tag)
        {
            int indexMajor = tag.LastIndexOf("_V");
            int indexMinor = tag.LastIndexOf('P');
            int indexBuild = tag.LastIndexOf('B');

            int majorNr = Int32.Parse(tag.Substring(indexMajor + 2, indexMinor - indexMajor - 2));
            int minorNr = Int32.Parse(tag.Substring(indexMinor + 1, indexBuild - indexMinor - 1));
            int buildNr = Int32.Parse(tag.Substring(indexBuild + 1));

            return new Tuple<int, int, int>(majorNr, minorNr, buildNr);
        }
    }
}
