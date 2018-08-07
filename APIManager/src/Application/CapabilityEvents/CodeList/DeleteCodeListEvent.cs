using System;
using System.Windows.Forms;
using Framework.Event;
using Framework.Logging;
using Framework.Model;
using Framework.Context;
using Framework.View;
using Plugin.Application.CapabilityModel.CodeList;

namespace Plugin.Application.Events.CodeList
{
    /// <summary>
    /// Implements the 'Delete CodeList' functionality: ask the user for confirmation and subsequently deletes the generated CodeType
    /// element as well as the CodeList capability element.
    /// </summary>
    class DeleteCodeListEvent : EventImplementation
    {
        // Configuration properties used by this module...
        private const string _CodeListDeclPkgStereotype     = "CodeListDeclPkgStereotype";
        private const string _ServiceClassStereotype        = "ServiceClassStereotype";

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
        /// Process the event. This method is called whenever the 'delete code list' menu option is selected on a given
        /// CodeList capability. We ask the user for permission before deleting the associated CodeType and the capability class.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.CodeList.DeleteCodeListEvent.handleEvent >> Processing event...");
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

            // Ask the user whether he/she really wants to delete this...
            if (model.LockModel(serviceModel.Parent) &&  MessageBox.Show("Are you sure you want to delete Code List '" + 
                                                                         codeListClass.Name + "'?", "Delete Code List",
                                                                         MessageBoxButtons.YesNo, 
                                                                         MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                foreach(MEAssociation assoc in codeListClass.TypedAssociations(MEAssociation.AssociationType.Composition))
                {
                    MEClass child = assoc.Destination.EndPoint;
                    Logger.WriteInfo("Plugin.Application.Events.CodeList.DeleteCodeListEvent.handleEvent >> Deleting child: '" + child.Name + "'...");
                    child.OwningPackage.DeleteClass(child);
                }
                serviceModel.DeleteClass(codeListClass);
                serviceModel.Refresh();
                myDiagram.Refresh();

                // Associated service gets an updated minor version as well as a new log entry telling us why...
                string serviceName = serviceModel.Parent.Name.Substring(0, serviceModel.Parent.Name.IndexOf("_V"));
                MEClass serviceClass = serviceModel.FindClass(serviceName, context.GetConfigProperty(_ServiceClassStereotype));
                var codeListService = new CodeListService(serviceClass, context.GetConfigProperty(_CodeListDeclPkgStereotype));
                codeListService.UpdateVersion(new Tuple<int, int>(codeListService.Version.Item1, codeListService.Version.Item2 + 1));
                codeListService.CreateLogEntry("Deleted CodeList: " + codeListClass.Name);
                codeListService.Dirty();
                codeListService.Paint(myDiagram);
            }
            model.UnlockModel(serviceModel.Parent);
        }
    }
}
