using System;
using System.Windows.Forms;
using Framework.Event;
using Framework.Logging;
using Framework.Model;
using Framework.View;
using Framework.Context;
using Plugin.Application.CapabilityModel.CodeList;
using Plugin.Application.Forms;

namespace Plugin.Application.Events.CodeList
{
    /// <summary>
    /// Implements the 'ChangeMajorVersion' operation, which adjusts the major version of the Service according to user input and updates
    /// the entire Capability hierarchy accordingly. The event is raised from the declaration package in the package browser, OR from a
    /// service class on a diagram.
    /// </summary>
    class CodeListMajorVersionEvent : EventImplementation
    {
        // Configuration properties used by this module...
        private const string _CodeListDeclPkgStereotype = "CodeListDeclPkgStereotype";
        private const string _ServiceClassStereotype = "ServiceClassStereotype";
        private const string _ServiceModelPkgName = "ServiceModelPkgName";

        /// <summary>
        /// Checks whether we can process the event in the current context. In this case, we only have to check whether we are called
        /// from a diagram of the correct type, which is determined by the declaration package of which the diagram is a part.
        /// </summary>
        /// <returns>True on correct context, false otherwise.</returns>
        internal override bool IsValidState()
        {
            if (this._event.Scope == TreeScope.Diagram)
            {
                ContextSlt context = ContextSlt.GetContextSlt();
                MEPackage declaration = context.CurrentDiagram.OwningPackage.Parent;  // Parent package of diagram package is declaration package.
                return (declaration.HasStereotype(context.GetConfigProperty(_CodeListDeclPkgStereotype)));
            }
            else return true;
        }

        /// <summary>
        /// Process the event. The method is either invoked from the declaration package level, or from a Service class on a 
        /// diagram. We locate the Service class, create a Service capability, ask the user for a new major version and update
        /// the Service class, all capabilities and associated CodeTypes as well as the declaration package name.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.CodeList.CodeListVersionSyncEvent.handleEvent >> Processing event...");
            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            MEClass serviceClass;
            MEPackage declPackage;
            Diagram myDiagram;

            // Figure-out how we got here...
            if (this._event.Scope == TreeScope.Diagram)
            {
                serviceClass = context.CurrentClass;
                declPackage = serviceClass.OwningPackage.Parent;
                myDiagram = context.CurrentDiagram;
            }
            else // (this._event.scope == TreeScope.PackageTree), no other options possible.
            {
                declPackage = context.CurrentPackage;   // Must be activated from the Code List Declaration package!
                MEPackage serviceModel = declPackage.FindPackage(context.GetConfigProperty(_ServiceModelPkgName));
                string serviceName = declPackage.Name.Substring(0, declPackage.Name.IndexOf("_V"));
                serviceClass = serviceModel.FindClass(serviceName, context.GetConfigProperty(_ServiceClassStereotype));
                myDiagram = serviceModel.FindDiagram(serviceModel.Name);
            }

            int currentMajor = serviceClass.Version.Item1;
            Logger.WriteInfo("Plugin.Application.Events.CodeList.CodeListVersionSyncEvent.handleEvent >> Current major version: " + currentMajor);
            using (var dialog = new ChangeMajorVersion(currentMajor))
            {
                if (model.LockModel(declPackage) && dialog.ShowDialog() == DialogResult.OK)
                {
                    Logger.WriteInfo("Plugin.Application.Events.CodeList.CodeListVersionSyncEvent.handleEvent >> Updating all CodeLists to version: '" + dialog.MajorVersion + "'...");
                    var codeListService = new CodeListService(serviceClass, context.GetConfigProperty(_CodeListDeclPkgStereotype));

                    // This will automatically synchronize all children if the major version is different from the current version...
                    codeListService.UpdateVersion(new Tuple<int, int>(dialog.MajorVersion, 0));
                    codeListService.Dirty();
                    if (myDiagram != null) codeListService.Paint(myDiagram);

                    // Finally, update the package version.
                    string packageName = declPackage.Name.Substring(0, declPackage.Name.IndexOf("_V") + 2);
                    packageName += dialog.MajorVersion;
                    declPackage.Name = packageName;
                }
                model.UnlockModel(declPackage);
            }
        }
    }
}
