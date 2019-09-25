using System;
using System.Windows.Forms;
using Framework.Event;
using Framework.Logging;
using Framework.Context;
using Framework.Model;
using Framework.ConfigurationManagement;
using Plugin.Application.CapabilityModel;
using Plugin.Application.Events.API;
using Plugin.Application.Forms;

namespace Plugin.Application.Events.Util
{
    /// <summary>
    /// Process a 'service checkout' event.
    /// </summary>
    class CheckoutServiceEvent : EventImplementation
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
        /// Performs a service 'checkout', which prepares a service for processing. Users must perform an explicit checkout to create and assign
        /// the appropriate change-tickets and establish the necessary feature branch for processing the change.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.API.CheckoutServiceEvent.HandleEvent >> Message processing...");
            ContextSlt context = ContextSlt.GetContextSlt();
            var svcContext = new ServiceContext(this._event.Scope == TreeScope.Diagram);
            string errorMsg = string.Empty;
            Service myService = null;

            // Perform a series of precondition tests...
            if (!svcContext.Valid)
            {
                if (!svcContext.HasValidRepositoryDescriptor) errorMsg = "No valid Repository Descriptor has been defined for the currently open project!";
                else errorMsg = "Illegal or corrupt context, operation aborted!";
            }
            else if (!svcContext.LockModel()) errorMsg = "Unable to lock the model!";

            if (errorMsg != string.Empty)
            {
                Logger.WriteError("Plugin.Application.Events.API.CheckoutServiceEvent.HandleEvent >> " + errorMsg);
                MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                svcContext.UnlockModel();
                return;
            }

            try
            {
                myService = svcContext.GetServiceInstance();
                RMServiceTicket existingTicket = myService.Ticket;
                if (myService.IsValidCMState(CMContext.CMState.CheckedOut))
                {
                    if (myService.ConfigurationMgmtState != CMContext.CMState.CheckedOut)
                    {
                        using (CMCheckoutService dialog = new CMCheckoutService(myService))
                        {
                            if (dialog.ShowDialog() == DialogResult.OK)
                            {
                                bool needCheckout = true;
                                if (dialog.UseVersion)
                                {
                                    if (dialog.NewVersion.Item1 > myService.MajorVersion)
                                    {
                                        // We're going to create a new service instance with a new major version. This service is then
                                        // checked-out and we leave the original alone.
                                        svcContext = new ServiceContext(myService.CopyService(myService.DeclarationPkg.Name));
                                        myService = svcContext.GetServiceInstance();        // Instance of copied service.
                                        needCheckout = false;
                                    }
                                    if (myService.Version.Item1 != dialog.NewVersion.Item1 || 
                                        myService.Version.Item2 != dialog.NewVersion.Item2) myService.UpdateVersion(dialog.NewVersion);
                                }
                                else
                                {
                                    // Restore service based on selected tag.
                                    // First of all, we import a (new) service package from Configuration Management.
                                    // Since this might invalidate our current service tree, we then re-create the capability tree by
                                    // creating a new service context based on the newly imported service.
                                    // Finally, we update the version history in this tree using the version information that has been
                                    // recorded in the importedService by the 'RestoreService' flow (might be different from the tag 
                                    // version in case we decided to create a new version based on an existing one).
                                    Tuple<int, int, int> tagVersion = ParseTagVersion(dialog.FeatureTag);
                                    MEClass importedService = myService.RestoreService(dialog.FeatureTag, dialog.CreateNewFeatureTagVersion);
                                    svcContext = new ServiceContext(importedService);
                                    myService = svcContext.GetServiceInstance();
                                    myService.UpdateVersion(importedService.Version);

                                    // If we did not select to create a new version, we have to set the build number to the version indicated
                                    // by the tag, since the UpdateVersion call will have reset it to default value!
                                    // Otherwise, we must reset the build number.
                                    // Also, don't check-out new services since these will be in CM state 'created'.
                                    if (dialog.CreateNewFeatureTagVersion)
                                    {
                                        myService.BuildNumber = 1;
                                        needCheckout = false;
                                    }
                                    else myService.BuildNumber = tagVersion.Item3;
                                }

                                // We have to check whether:
                                // a) The user has selected a new ticket (dialog.Ticket == null and RemoteTicket contains the new ticket);
                                // b) If the user has left the ticket alone and we find the existing ticket to be different from the current
                                //    service ticket, the user has restored an 'old' service that contains an 'old' ticket and we must replace
                                //    this with the ticket from our 'current' service. Note that we can not 'trust' this old ticket since we could
                                //    have reloaded the service, destroying all existing context! Therefor, we create a new Service Ticket using
                                //    minimal data from the old one.
                                RMServiceTicket ticket = dialog.Ticket;
                                if (ticket == null) ticket = new RMServiceTicket(dialog.RemoteTicket, dialog.ProjectOrderID, myService);
                                else if (myService.Ticket != existingTicket) ticket = new RMServiceTicket(existingTicket.Ticket, existingTicket.ProjectOrderID, myService);

                                // Now that we have made sure that the service context is correctly changed, perform the actual checkout.
                                // This will create the appropriate feature branch and push stuff to remote.
                                // Only after checkout can we modify te service contents.
                                if (needCheckout)
                                {
                                    if (myService.Checkout(ticket))
                                    {
                                        MessageBox.Show("Successfully checked-out service '" + myService.Name + "'.",
                                                        "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    }
                                    else
                                    {
                                        MessageBox.Show("Unable to checkout service '" + myService.Name +
                                                        "' from configuration management; probably caused by uncommitted changes on branch(es): '" +
                                                        CMContext.FindBranchesInState(CMContext.CMState.Modified) + "'." + Environment.NewLine +
                                                        "Please commit pending changes before starting work on a new service!",
                                                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                }
                                else
                                {
                                    // Typically, the ticket is assigned during Checkout. Since we skipped that, don't forget to tell our
                                    // service about the ticket here.
                                    myService.Ticket = ticket;
                                    MessageBox.Show("Successfully created new service instance '" + myService.Name + "_V" + myService.MajorVersion,
                                                     "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                        }
                    }
                    else MessageBox.Show("Service is already checked-out!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else MessageBox.Show("Service must be committed or released before it can be checked-out!", 
                                     "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exc)
            {
                string msg = "Unable to checkout service '" + myService.Name + "' because: " + Environment.NewLine + exc.Message;
                MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.WriteError("Plugin.Application.Events.API.CheckoutServiceEvent.HandleEvent >> " + msg + Environment.NewLine + exc.ToString());
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
