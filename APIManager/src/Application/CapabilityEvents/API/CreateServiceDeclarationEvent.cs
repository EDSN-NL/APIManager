using System.Windows.Forms;
using Framework.Event;
using Framework.Logging;
using Framework.Model;
using Framework.Context;
using Framework.View;
using Plugin.Application.Forms;
using Plugin.Application.CapabilityModel.API;

namespace Plugin.Application.Events.API
{
    /// <summary>
    /// Checks whether we can process the event in the current context. For this particular event, we trust the
    /// configuration context settings and thus always return true.
    /// </summary>
    class CreateServiceDeclarationEvent : EventImplementation
    {
        // Configuration properties used by this module...
        private const string _ServiceDeclPkgStereotype = "ServiceDeclPkgStereotype";

        /// <summary>
        /// Checks whether we're in the correct context for processing this event. Since this only depends on configuration in this case,
        /// we can simply return 'true' since the configuration has been checked elsewhere.
        /// </summary>
        /// <returns>True</returns>
        internal override bool IsValidState() { return true; }

        /// <summary>
        /// Process the event. This method is called whenever the 'create new service declaration' menu option is selected on a given
        /// package. We can be sure that the package is of the correct type and context. We show the 'create new service declaration'
        /// dialog to the user and request a service name and a set of operation names.
        /// Subsequently, the method invokes the special 'Create new SOAP Service' constructor of SOAPService in order to get all the
        /// actual work done.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.API.CreateServiceDeclarationEvent.HandleEvent >> Processing event...");
            ContextSlt context = ContextSlt.GetContextSlt();
            MEPackage containerPackage = context.CurrentPackage;
            if (ModelSlt.GetModelSlt().LockModel(containerPackage))
            {
                using (var dialog = new CreateSOAPServiceDeclaration(containerPackage))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        // We only have to invoke this constructor in order to create the entire service structure...
                        var svc = new ApplicationService(containerPackage, dialog.PackageName, dialog.OperationList,
                                                         context.GetConfigProperty(_ServiceDeclPkgStereotype));
                        if (svc.Valid) svc.Paint(svc.ModelPkg.FindDiagram(svc.ModelPkg.Name));
                        else MessageBox.Show("Error creating Service declaration, nothing has been created!",
                                             "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                ModelSlt.GetModelSlt().UnlockModel(containerPackage);
            }
        }
    }
}
