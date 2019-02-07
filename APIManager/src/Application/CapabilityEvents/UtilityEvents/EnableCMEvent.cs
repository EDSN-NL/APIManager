using System;
using System.Windows.Forms;
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
    /// Process the 'Enable CM' event for services that do not have CM enabled yet. The event requires the user to enter the initial
    /// ticket ID and project number and initialises the CM by performing a 'specialised check-out'.
    /// </summary>
    class EnableCMEvent : EventImplementation
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
        /// This operation is required to initialise CM for a service that has been created in a non-CM enabled environment. The operation
        /// collects and assigns the initial change ticket and creates the appropriate environment for processing.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.API.EnableCMEvent.HandleEvent >> Message processing...");
            ContextSlt context = ContextSlt.GetContextSlt();
            var svcContext = new ServiceContext(this._event.Scope == TreeScope.Diagram);
            string errorMsg = string.Empty;
            Service myService = null;

            // Perform a series of precondition tests...
            if (!svcContext.Valid) errorMsg = "Illegal or corrupt context, operation aborted!";
            else if (!svcContext.LockModel()) errorMsg = "Unable to lock the model!";

            if (errorMsg != string.Empty)
            {
                Logger.WriteError("Plugin.Application.Events.API.EnableCMEvent.HandleEvent >> " + errorMsg);
                MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                svcContext.UnlockModel();
                return;
            }

            try
            {
                myService = svcContext.GetServiceInstance();
                if (myService.UseConfigurationMgmt)
                {
                    MessageBox.Show("Configuration Management is already enabled for this service, use 'Checkout' instead!",
                                    "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (CMEnable dialog = new CMEnable(myService))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        // We have to explicitly assign the ticket here since UpdateVersion depends on the ticket ID to create
                        // the appropriate path- and/or branch names.
                        myService.Ticket = new RMServiceTicket(dialog.RemoteTicket, dialog.ProjectOrderID, myService);
                        if (dialog.NewVersion.Item1 > myService.MajorVersion)
                        {
                            var newSvcContext = new ServiceContext(myService.CopyService(myService.DeclarationPkg.Name));
                            myService = newSvcContext.GetServiceInstance();         // Instance of copied service.
                            myService.UpdateVersion(dialog.NewVersion);             // Updates entire hierarchy.
                            myService.Paint(newSvcContext.MyDiagram);
                        }
                        else myService.UpdateVersion(dialog.NewVersion);
                        myService.ConfigurationMgmtState = CMContext.CMState.Created;
                        MessageBox.Show("Successfully enabled Configuration Management for service '" + myService.Name + "'.",
                                        "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception exc)
            {
                string msg = "Can not enable Configuration Management for service '" + myService.Name + "' because: " + 
                             Environment.NewLine + exc.Message;
                MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.WriteError("Plugin.Application.Events.API.EnableCMEvent.HandleEvent >> " + msg + Environment.NewLine + exc.ToString());
            }
            myService.Paint(svcContext.MyDiagram);
            svcContext.UnlockModel();
        }
    }
}
