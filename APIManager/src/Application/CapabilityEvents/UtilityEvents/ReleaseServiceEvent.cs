using System;
using System.Windows.Forms;
using Framework.Event;
using Framework.Logging;
using Framework.Context;
using Plugin.Application.CapabilityModel;
using Plugin.Application.CapabilityModel.API;
using Plugin.Application.CapabilityModel.CodeList;
using Plugin.Application.Forms;
using Plugin.Application.Events.API;

namespace Plugin.Application.Events.Util
{
    /// <summary>
    /// </summary>
    class ReleaseServiceEvent : EventImplementation
    {
        // Configuration properties used by this module...
        private const string _ServiceDeclPkgStereotype      = "ServiceDeclPkgStereotype";
        private const string _CodeListDeclPkgStereotype     = "CodeListDeclPkgStereotype";
        private const string _InterfaceContractTypeTag      = "InterfaceContractTypeTag";
        private const string _CommitIDLeader                = "CommitIDLeader";

        private const bool _NOBUILDHIERARCHY = false;       // Used for CodeLists to suppress construction of complete class hierarchy.

        /// <summary>
        /// Checks whether we can process the event in the current context. Since this context is already clearly defined by the 'Service'
        /// stereotype, we only return 'false' when configuration management is generally disabled.
        /// </summary>
        /// <returns>True.</returns>
        internal override bool IsValidState()
        {
            return (ContextSlt.GetContextSlt().GetBoolSetting(FrameworkSettings._UseConfigurationManagement));
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
            Service myService;

            if (!svcContext.Valid)
            {
                Logger.WriteError("Plugin.Application.Events.API.ReleaseServiceEvent.HandleEvent >> Illegal or corrupt context, event aborted!");
                return;
            }

            try
            {
                if (svcContext.Type == ServiceContext.ServiceType.REST)
                {
                    Logger.WriteInfo("Plugin.Application.Events.API.ReleaseServiceEvent.HandleEvent >> Committing a REST Service...");
                    myService = new RESTService(svcContext.Hierarchy, context.GetConfigProperty(_ServiceDeclPkgStereotype));
                }
                else if (svcContext.Type == ServiceContext.ServiceType.CodeList)
                {
                    Logger.WriteInfo("Plugin.Application.Events.API.ReleaseServiceEvent.HandleEvent >> Committing a CodeList Service...");
                    myService = new CodeListService(svcContext.ServiceClass, context.GetConfigProperty(_CodeListDeclPkgStereotype), _NOBUILDHIERARCHY);
                }
                else    // Assume it's either SOAP or Message (which is based on SOAP)...
                {
                    Logger.WriteInfo("Plugin.Application.Events.API.ReleaseServiceEvent.HandleEvent >> Committing a SOAP/Message Service...");
                    myService = new ApplicationService(svcContext.Hierarchy, context.GetConfigProperty(_ServiceDeclPkgStereotype));
                }
            }
            catch
            {
                Logger.WriteError("Plugin.Application.Events.API.ReleaseServiceEvent.HandleEvent >> Unable to determine proper context for commit!");
                return;
            }

            if (myService.ConfigurationMgmtState == CMState.Committed)
            {
                using (var dialog = new CMChangeMessage(false))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
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
                        myService.ConfigurationMgmtState = CMState.Released;
                        myService.Paint(context.CurrentDiagram);
                    }
                }
            }
            else MessageBox.Show("Service must be 'Committed' before it can be released!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
