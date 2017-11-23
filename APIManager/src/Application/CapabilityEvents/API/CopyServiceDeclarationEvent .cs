using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Framework.Event;
using Framework.Logging;
using Framework.Model;
using Framework.Context;

namespace Plugin.Application.Events.API
{
    class CopyServiceDeclarationEvent: EventImplementation
    {
        internal override bool IsValidState()
        {
            Logger.WriteInfo("CPlugin.Application.Events.API.CopyServiceDeclarationEvent.IsValidState >> Checking context...");
            return true;
        }

        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.API.CopyServiceDeclarationEvent.HandleEvent >> Processing event...");
            MessageBox.Show("CopyServiceDeclarationEvent");
        }
    }
}
