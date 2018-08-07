using System;
using Framework.Event;
using Framework.Logging;
using Framework.Model;
using Framework.Context;
using Framework.View;
using Plugin.Application.CapabilityModel.CodeList;

namespace Plugin.Application.Events.CodeList
{
    /// <summary>
    /// Implements the 'Update CodeList' functionality. Presents the user with a list box of all enumeration values from the associated
    /// enumerated types in which the currently selected values are 'pre-selected' and allows the user to add- or delete values.
    /// On OK, the capability class is updated with the new list.
    /// </summary>
    class UpdateCodeListEvent : EventImplementation
    {
        // Configuration properties used by this module...
        private const string _CodeListDeclPkgStereotype     = "CodeListDeclPkgStereotype";
        private const string _ServiceClassStereotype        = "ServiceClassStereotype";

        private const bool _NOBUILDHIERARCHY = false;

        /// <summary>
        /// Checks whether we can process the event in the current context. In this case, we always return 'true' since the correct
        /// context has already been checked by our default mechanism (checking stereotypes).
        /// </summary>
        /// <returns>True.</returns>
        internal override bool IsValidState()
        {
            return true;
        }

        /// <summary>
        /// Process the event. This method is called whenever the 'update code list' menu option is selected on a given
        /// CodeList capability class.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.CodeList.UpdateCodeListEvent.handleEvent >> Processing event...");
            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            MEPackage serviceModel = null;
            MEClass codeListClass = context.CurrentClass;
            Diagram myDiagram = null;

            // Figure-out how we got here...
            if (this._event.Scope == TreeScope.Diagram)
            {
                myDiagram = context.CurrentDiagram;
                serviceModel = myDiagram.OwningPackage;
            }
            else // (this._event.scope == TreeScope.PackageTree), no other options possible.
            {
                serviceModel = context.CurrentPackage;   // Must be activated from the ServiceModel package!
                myDiagram = serviceModel.FindDiagram(serviceModel.Name);
            }

            if (model.LockModel(serviceModel.Parent))
            {
                string serviceName = serviceModel.Parent.Name.Substring(0, serviceModel.Parent.Name.IndexOf("_V"));
                MEClass serviceClass = serviceModel.FindClass(serviceName, context.GetConfigProperty(_ServiceClassStereotype));
                if (serviceClass == null || myDiagram == null)
                {
                    Logger.WriteError("Plugin.Application.Events.CodeList.UpdateCodeListEvent.handleEvent >> Illegal or corrupt context, event aborted!");
                    return;
                }

                // the NoBuildHierarchy indicator avoids construction of the entire CodeList set (since we're only interested in one single CodeList)...
                var codeListService = new CodeListService(serviceClass, context.GetConfigProperty(_CodeListDeclPkgStereotype), _NOBUILDHIERARCHY);
                var codeListCapability = new CodeListCapability(codeListService, codeListClass);
                if (codeListCapability.Update())    // Performs the actual update operation.
                {
                    // Only in case something has actually been updated do we refresh the diagram and package views...
                    codeListService.Dirty();
                    codeListService.Paint(myDiagram);
                    serviceModel.Refresh();
                    myDiagram.Refresh();
                }
            }
            model.UnlockModel(serviceModel.Parent);
        }
    }
}
