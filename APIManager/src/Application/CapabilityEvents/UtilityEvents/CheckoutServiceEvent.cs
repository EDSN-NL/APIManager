using System;
using System.Windows.Forms;
using Framework.Event;
using Framework.Logging;
using Framework.Context;
using Framework.ConfigurationManagement;
using Plugin.Application.CapabilityModel;
using Plugin.Application.Events.API;

namespace Plugin.Application.Events.Util
{
    /// <summary>
    /// Process an explicit 'service checkout' event.
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
        /// Performs a service 'checkout', which prepares a service for processing. Typically, this is done explicitly when performing a
        /// 'process service' event. However, users can perform an explicit checkout to assure that CM contains the appropriate branches and
        /// to check whether no pending changes from other services prevent processing of the selected service.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.API.CheckoutServiceEvent.HandleEvent >> Message processing...");
            ContextSlt context = ContextSlt.GetContextSlt();
            var svcContext = new ServiceContext(this._event.Scope == TreeScope.Diagram);

            if (!svcContext.Valid)
            {
                Logger.WriteError("Plugin.Application.Events.API.CheckoutServiceEvent.HandleEvent >> Illegal or corrupt context, event aborted!");
                return;
            }

            try
            {
                if (svcContext.LockModel())
                {
                    Service myService = svcContext.GetServiceInstance();
                    if (!myService.Checkout())
                    {
                        MessageBox.Show("Unable to checkout service '" + myService.Name +
                                        "' from configuration management; probably caused by uncommitted changes on branch(es): '" +
                                        CMContext.FindBranchesInState(CMState.Modified) + "'." + Environment.NewLine +
                                        "Please commit pending changes before starting work on a new service!",
                                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show("Successfully checked-out service '" + myService.Name + "'.",
                                        "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        myService.Paint(svcContext.MyDiagram);
                    }
                    svcContext.UnlockModel();
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.Events.API.CheckoutServiceEvent.HandleEvent >> Caught an exception during service creation: " + Environment.NewLine + exc.Message);
                return;
            }
        }
    }
}
