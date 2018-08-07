using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Event;
using Framework.Logging;
using Framework.Model;
using Framework.Context;
using Framework.View;
using Plugin.Application.CapabilityModel.CodeList;
using Plugin.Application.Forms;

namespace Plugin.Application.Events.CodeList
{
    class AddCodeListsEvent : EventImplementation
    {
        // Configuration properties used by this module...
        private const string _CodeListDeclPkgStereotype     = "CodeListDeclPkgStereotype";
        private const string _ServiceClassStereotype        = "ServiceClassStereotype";
        private const string _ServiceModelPkgName           = "ServiceModelPkgName";

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
            else return true;   // Only other possibility is package-tree and this is checked by default configuration.
        }

        /// <summary>
        /// Process the event. This method is called whenever the 'create new code list' menu option is selected on a given
        /// package. We can be sure that the package is of the correct type and context. We show the 'create new code list'
        /// dialog to the user and request a service name and a set of code list names.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.CodeList.AddCodeListsEvent.handleEvent >> Processing event...");
            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            MEPackage declPackage;
            MEClass serviceClass = null;
            Diagram myDiagram = null;
            bool addSourceEnums = context.GetBoolSetting(FrameworkSettings._CLAddSourceEnumsToDiagram);
            bool addCodeTypes = context.GetBoolSetting(FrameworkSettings._CLAddCodeTypesToDiagram);

            // Figure-out how we got here...
            if (this._event.Scope == TreeScope.Diagram)
            {
                myDiagram = context.CurrentDiagram;
                declPackage = myDiagram.OwningPackage.Parent;
                serviceClass = context.CurrentClass;
            }
            else // (this._event.scope == TreeScope.PackageTree), no other options possible.
            {
                declPackage = context.CurrentPackage;   // Must be activated from the Code List Declaration package!
                MEPackage serviceModel = declPackage.FindPackage(context.GetConfigProperty(_ServiceModelPkgName));
                string serviceName = declPackage.Name.Substring(0, declPackage.Name.IndexOf("_V"));
                serviceClass = serviceModel.FindClass(serviceName, context.GetConfigProperty(_ServiceClassStereotype));
                if (serviceClass != null) myDiagram = serviceModel.FindDiagram(serviceModel.Name);
            }

            if (serviceClass == null || myDiagram == null)
            {
                Logger.WriteError("Plugin.Application.Events.CodeList.AddCodeListsEvent.handleEvent >> Illegal or corrupt context, event aborted!");
                return;
            }

            if (!model.LockModel(declPackage)) return;      // If we can't lock the package, we can't continue.

            using (var dialog = new AddCodeListInput(serviceClass))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    List<string> nameList = dialog.CodeListNameList;

                    // Now we're going to ask the user to select enumerations based on the names entered before...
                    var director = new CodeListDirector(serviceClass.Name);
                    director.LoadRootNodes(nameList);
                    if (director.ShowDialog() == DialogResult.OK)
                    {
                        // Keep track of (extra) classes and associations to show in the diagram...
                        var diagramClassList = new List<MEClass>();
                        var diagramAssocList = new List<MEAssociation>();

                        var codeListService = new CodeListService(serviceClass, context.GetConfigProperty(_CodeListDeclPkgStereotype));
                        string newNames = string.Empty;
                        bool isFirst = true; // Little trick to get the right amount of ',' separators.
                        SortedList<string, CodeListDirector.DirectorContext> resultSet = director.Context;
                        foreach (CodeListDirector.DirectorContext ctx in resultSet.Values)
                        {
                            if (ctx.CompletedIndicator)  // We only process the results that have actually been completed by the user...
                            {
                                var codeListCapability = new CodeListCapability(codeListService, ctx.name, ctx.sourceEnum, ctx.agencyName, ctx.agencyID, ctx.selectedAttribs);
                                if (codeListCapability.Valid)
                                {
                                    MEClass capabilityClass = codeListCapability.CapabilityClass;
                                    diagramClassList.Add(capabilityClass);
                                    newNames += (!isFirst)? ", " + capabilityClass.Name: capabilityClass.Name;
                                    isFirst = false;
                                    foreach (MEAssociation assoc in capabilityClass.AssociationList)
                                    {
                                        if ((assoc.TypeOfAssociation == MEAssociation.AssociationType.Composition && addCodeTypes) ||
                                            (assoc.TypeOfAssociation == MEAssociation.AssociationType.Usage && addSourceEnums))
                                        {
                                            MEClass subClass = assoc.Destination.EndPoint;
                                            Logger.WriteInfo("Plugin.Application.Events.CodeList.AddCodeListsEvent.handleEvent >> Adding child: " + subClass.Name);
                                            diagramAssocList.Add(assoc);
                                            diagramClassList.Add(assoc.Destination.EndPoint);
                                        }
                                    }
                                    foreach (MEAssociation assoc in serviceClass.TypedAssociations(MEAssociation.AssociationType.Composition))
                                    {
                                        if (assoc.Destination.EndPoint == capabilityClass)
                                        {
                                            diagramAssocList.Add(assoc);
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    // Oops, creation of capability structure failed, ignore and continue with next.
                                    Logger.WriteWarning("Adding CodeList '" + ctx.name + "' failed!");
                                }
                            }
                        }
                        // Associated service gets an updated minor version as well as a new log entry telling us why...
                        codeListService.UpdateVersion(new Tuple<int, int>(codeListService.Version.Item1, codeListService.Version.Item2 + 1));
                        codeListService.CreateLogEntry("Added CodeList(s): " + newNames);

                        myDiagram.AddClassList(diagramClassList);
                        myDiagram.AddAssociationList(diagramAssocList);
                        codeListService.Dirty();
                        codeListService.Paint(myDiagram);
                        myDiagram.Redraw();
                        declPackage.Refresh();
                    }
                }
            }
            model.UnlockModel(declPackage);
        }
    }
}
