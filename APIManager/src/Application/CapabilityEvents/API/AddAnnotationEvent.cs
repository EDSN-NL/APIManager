﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Event;
using Framework.Logging;
using Framework.Model;
using Framework.Context;
using Framework.Util;
using Plugin.Application.Forms;

namespace Plugin.Application.Events.API
{
    class AddAnnotationEvent : EventImplementation
    {
        // Configuration properties used by this module...
        private const string _ServiceDeclPkgStereotype          = "ServiceDeclPkgStereotype";
        private const string _OperationClassStereotype          = "OperationClassStereotype";
        private const string _InterfaceContractClassStereotype  = "InterfaceContractClassStereotype";
        private const string _CommonSchemaClassStereotype       = "CommonSchemaClassStereotype";
        private const string _ServiceClassStereotype            = "ServiceClassStereotype";

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
                if (context.CurrentDiagram != null)
                {
                    MEPackage declaration = context.CurrentDiagram.OwningPackage.Parent;  // Parent package of diagram package is declaration package.
                    return (declaration.HasStereotype(context.GetConfigProperty(_ServiceDeclPkgStereotype)));
                }
                else return false;
            }
            else return true;   // Only other possibility is package-tree and this is checked by default configuration.
        }

        /// <summary>
        /// Processes an Add Annotation event at the InterfaceClass level. The method collects a 'change log' message from
        /// the user and assigns this to all selected operations, as well as the Interface and Common Schema.
        /// When requested to do so, the minor version of the operations, Interface and Common Schema is increased.
        /// </summary>
        internal override void HandleEvent()
        {
            Logger.WriteInfo("Plugin.Application.Events.API.AddAnnotationEvent.handleEvent >> Processing an add operations menu click...");

            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            var svcContext = new ServiceContext(this._event.Scope == TreeScope.Diagram);
            var operationList = new List<MEClass>();
            MEClass commonSchemaClass = null;

            string operationStereotype = context.GetConfigProperty(_OperationClassStereotype);
            string commonSchemaStereotype = context.GetConfigProperty(_CommonSchemaClassStereotype);

            if (!svcContext.Valid || svcContext.InterfaceClass == null)
            {
                Logger.WriteError("Plugin.Application.Events.API.AddAnnotationEvent.handleEvent >> Illegal context! Aborting.");
                return;
            }

            // Collect all possible operations on this interface so the user can indicate what operations have been modified...
            foreach (MEAssociation assoc in svcContext.InterfaceClass.TypedAssociations(MEAssociation.AssociationType.Composition))
            {
                MEClass cl = assoc.Destination.EndPoint;
                if (cl.HasStereotype(operationStereotype)) operationList.Add(cl);
                else if (cl.HasStereotype(commonSchemaStereotype)) commonSchemaClass = cl;
            }

            using (var dialog = new CollectAnnotation())
            {
                dialog.LoadNodes("Operations on '" + svcContext.InterfaceClass.Name + "' to annotate:", operationList);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    operationList = dialog.GetCheckedNodes();
                    foreach (MEClass cl in operationList)
                    {
                        if (dialog.MinorVersionIndicator)
                        {
                            Logger.WriteInfo("Plugin.Application.Events.API.AddAnnotationEvent.handleEvent >> Updating version for class: '" + cl.Name + "'...");
                            var newVersion = new Tuple<int, int>(cl.Version.Item1, cl.Version.Item2 + 1);
                            cl.Version = newVersion;
                        }
                        CreateLogEntry(cl, dialog.Annotation);
                    }
                    if (dialog.MinorVersionIndicator)
                    {
                        var newVersion = new Tuple<int, int>(svcContext.InterfaceClass.Version.Item1, svcContext.InterfaceClass.Version.Item2 + 1);
                        svcContext.InterfaceClass.Version = newVersion;
                        commonSchemaClass.Version = newVersion;
                        newVersion = new Tuple<int, int>(svcContext.ServiceClass.Version.Item1, svcContext.ServiceClass.Version.Item2 + 1);
                        svcContext.ServiceClass.Version = newVersion;
                    }
                    if (svcContext.InterfaceList.Count > 1) UpdateInterfaces(svcContext, operationList, dialog.Annotation, dialog.MinorVersionIndicator);
                    CreateLogEntry(svcContext.InterfaceClass, dialog.Annotation);
                    CreateLogEntry(commonSchemaClass, dialog.Annotation);
                    CreateLogEntry(svcContext.ServiceClass, dialog.Annotation);
                }
            }
        }

        /// <summary>
        /// Helper method that creates a new log entry in the specified class. The log entry is preceded by the date, 
        /// author and class version.
        /// </summary>
        /// <param name="cl"></param>
        /// <param name="text"></param>
        private void CreateLogEntry(MEClass cl, string text)
        {
            string annotation = cl.Annotation;
            ContextSlt context = ContextSlt.GetContextSlt();
            MEChangeLog.MigrateLog(cl);                            // Assures that log has correct format.

            MEChangeLog newLog = (!string.IsNullOrEmpty(annotation)) ? new MEChangeLog(context.TransformRTF(annotation, RTFDirection.ToRTF)) : new MEChangeLog();
            Tuple<int, int> myVersion = cl.Version;
            string logText = "[" + myVersion.Item1 + "." + myVersion.Item2 + "]: " + text;
            newLog.AddEntry(cl.Author, logText);
            string log = newLog.GetLog();
            cl.Annotation = context.TransformRTF(log, RTFDirection.FromRTF);
        }

        /// <summary>
        /// If the service model has multiple interfaces, we have to check whether one of the updates operations has a relationship with any of
        /// the interfaces different from the selected interface. If so, we have to update comment (and optionally version) of these interfaces
        /// as well!
        /// </summary>
        /// <param name="svcContext">Service-model context.</param>
        /// <param name="operationList">List of operations that have received annotation.</param>
        /// <param name="annotation">The annotation text.</param>
        /// <param name="newMinorVersion">Indicates whether minor version has to be bumped.</param>
        private void UpdateInterfaces (ServiceContext svcContext, List<MEClass> operationList, string annotation, bool newMinorVersion)
        {
            Logger.WriteInfo("Plugin.Application.Events.API.AddAnnotationEvent.UpdateInterfaces >> Update other interfaces with annotation and version");
            
            foreach(MEClass itf in svcContext.InterfaceList)
            {
                if (itf != svcContext.InterfaceClass) // Make sure to skip the 'original' interface, since this has already been processed.
                {
                    Logger.WriteInfo("Plugin.Application.Events.API.AddAnnotationEvent.UpdateInterfaces >> Updating interface '" + itf.Name + "' ...");
                    if (svcContext.HasAnyOperation(itf, operationList))
                    {
                        Logger.WriteInfo("Plugin.Application.Events.API.AddAnnotationEvent.UpdateInterfaces >> Affected interface, perform update...");
                        MEClass commonSchema = svcContext.GetCommonSchema(itf);
                        CreateLogEntry(itf, annotation);
                        CreateLogEntry(commonSchema, annotation);
                        if (newMinorVersion)
                        {
                            var newVersion = new Tuple<int, int>(itf.Version.Item1, itf.Version.Item2 + 1);
                            itf.Version = newVersion;
                            commonSchema.Version = newVersion;
                        }
                    }
                }
            }
        }
    }
}
