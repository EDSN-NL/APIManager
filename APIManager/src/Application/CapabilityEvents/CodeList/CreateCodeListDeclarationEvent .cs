using System.Windows.Forms;
using Framework.Event;
using Framework.Logging;
using Framework.Model;
using Framework.Context;
using Plugin.Application.CapabilityModel.CodeList;
using Plugin.Application.Forms;

namespace Plugin.Application.Events.CodeList
{
    class CreateCodeListDeclarationEvent: EventImplementation
    {
        // Configuration properties used by this module...
        private const string _CodeListDeclPkgStereotype     = "CodeListDeclPkgStereotype";

        /// <summary>
        /// Checks whether we can process the event in the current context. For this particular event, we trust the
        /// configuration context settings and thus always return true.
        /// </summary>
        /// <returns>True.</returns>
        internal override bool IsValidState()
        {
            return true;
        }

        /// <summary>
        /// Process the event. This method is called whenever the 'create new code list' menu option is selected on a given
        /// package. We can be sure that the package is of the correct type and context. We show the 'create new code list'
        /// dialog to the user and request a service name and a set of code list names. Subsequently, we execute the CodeListDirector
        /// in order to let the user specify all the details.
        /// Finally, we invoke the special CodeListService 'create new' constructor, which does all the 'real' work.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.CodeList.CreateCodeListDeclarationEvent.handleEvent >> Processing event...");
            ContextSlt context = ContextSlt.GetContextSlt();
            MEPackage containerPackage = context.CurrentPackage;
            using (var dialog = new CreateCodeListDeclaration(containerPackage))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string codeListDeclStereotype = context.GetConfigProperty(_CodeListDeclPkgStereotype);

                    // Now we're going to ask the user to select enumerations based on the names entered before...
                    using (var director = new CodeListDirector(dialog.PackageName.Substring(0, dialog.PackageName.IndexOf("_V"))))
                    {
                        director.LoadRootNodes(dialog.CodeListNameList);
                        if (director.ShowDialog() == DialogResult.OK)
                        {
                            // We only have to invoke this constructor in order to create the entire service structure (we like to keep events simple :-)
                            var svc = new CodeListService(containerPackage, dialog.PackageName, director.Context, codeListDeclStereotype);
                            if (!svc.Valid) MessageBox.Show("Error creating CodeList declaration, nothing has been created!", 
                                                            "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }          
            }
        }
    }
}
