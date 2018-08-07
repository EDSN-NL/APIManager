using System;
using Framework.Event;
using Framework.Logging;
using Framework.Context;
using Plugin.Application.CapabilityModel;

namespace Plugin.Application.Events.API
{
    /// <summary>
    /// Copies an entire Service metamodel to a new, higher, major version.
    /// </summary>
    class CopyServiceDeclarationEvent: EventImplementation
    {
        /// <summary>
        /// Always returns 'true', since validity of the call can be established by checking stereotypes only.
        /// </summary>
        /// <returns>True</returns>
        internal override bool IsValidState() { return true; }

        /// <summary>
        /// Copies an entire service model hierarchy to a new major version of that service.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.API.CopyServiceDeclarationEvent.HandleEvent >> Message processing...");
            ContextSlt context = ContextSlt.GetContextSlt();
            var svcContext = new ServiceContext(this._event.Scope == TreeScope.Diagram);

            if (!svcContext.Valid)
            {
                Logger.WriteError("Plugin.Application.Events.API.CopyServiceDeclarationEvent.HandleEvent >> Illegal or corrupt context, event aborted!");
                return;
            }

            try
            {
                if (svcContext.LockModel(true))
                {
                    // Initially, we copy the service with the same name as the original (this because a lot of load-checks are based
                    // on the version). Next, we perform an update to a new major version, which should update all relevant version
                    // information, including the Service Declaration Package name...
                    Service myService = svcContext.GetServiceInstance();    // Instance of current service.
                    var newSvcContext = new ServiceContext(myService.CopyService(myService.DeclarationPkg.Name));
                    myService = newSvcContext.GetServiceInstance();         // Instance of copied service.
                    myService.UpdateVersion(new Tuple<int, int>(svcContext.ServiceClass.Version.Item1 + 1, 0)); // Updates entire hierarchy.
                    myService.Paint(newSvcContext.MyDiagram);
                    svcContext.UnlockModel();
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.Events.API.CopyServiceDeclarationEvent.HandleEvent >> Unable to copy service because: " + Environment.NewLine + exc.Message);
                return;
            }
        }
    }
}
