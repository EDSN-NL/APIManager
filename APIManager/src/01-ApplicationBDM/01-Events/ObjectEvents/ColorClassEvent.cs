﻿using System.Collections.Generic;
using Framework.Event;
using Framework.Logging;
using Framework.Model;
using Framework.View;
using Framework.Context;

namespace Plugin.ApplicationBDM.Events
{
    class ColorClassEvent : ObjectEventImplementation
    {
        // Configuration properties used by this module...
        private const string _ColorTagName          = "ColorTagName";
        private const string _ColorStereotype       = "ColorStereotype";
        private const string _ColorProtectionTag    = "ColorProtectionTag";
        private const string _ColorModelScope       = "ColorModelScope";

        /// <summary>
        /// This event is raised when a new class is created on a diagram or when a diagram object is selected. The event checks whether the
        /// class color matches the color of the owning domain and if not, updates the color.
        /// </summary>
        /// <param name="eventType">The event type that triggered the call.</param>
        /// <param name="objectType">The originating object type.</param>
        /// <param name="eventObject">The actual object that triggered the event, either a ModelElement specialization or a Diagram.</param>
        /// <param name="targetDiagram">Only in case of creation of Diagram Objects: the diagram on which the object has been created.</param>
        /// <returns>Depending on the event type, the return value is either ignored (Selected, Modified), used to specify whether the associated 
        /// object may be deleted (Deleted) or indicates whether the object has been modified by the event (Created).</returns>
        internal override bool HandleEvent(ObjectEventType eventType, ObjectType objectType, object eventObject, Diagram targetDiagram)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            Diagram myDiagram = targetDiagram != null ? targetDiagram : context.CurrentDiagram;
            string protectColorTagName = context.GetConfigProperty(_ColorProtectionTag);

            MEClass element = eventObject as MEClass;
            if (IsValidContext() && element != null)
            {
                Logger.WriteInfo("Plugin.ApplicationBDM.Events.ColorEvent.handleEvent >> Got a '" + eventType + "' event for class '" + element.Name + "'...");
                element.RefreshModelElement();
                string protectColorTag = element.GetTag(protectColorTagName);
                if (string.IsNullOrEmpty(protectColorTag) || string.Compare(protectColorTag, "false", true) == 0)
                {
                    DiagramClassRepresentation rep = myDiagram.GetRepresentation(element);
                    if (rep != null)
                    {
                        MEPackage domainPackage = element.OwningPackage.FindParentWithStereotype(context.GetConfigProperty(_ColorStereotype));
                        if (domainPackage != null)
                        {
                            int currentBGColor = DiagramClassRepresentation.ColorToInteger(rep.BackgroundColor);
                            int currentFontColor = DiagramClassRepresentation.ColorToInteger(rep.FontColor);
                            string colorDefn = domainPackage.GetTag(context.GetConfigProperty(_ColorTagName));
                            if (!string.IsNullOrEmpty(colorDefn))
                            {
                                int separatorIdx = colorDefn.IndexOf(":");
                                string bgColorStr = colorDefn.Substring(0, separatorIdx);
                                string fontColorStr = colorDefn.Substring(separatorIdx + 1);
                                int bgColor, fontColor;
                                if (int.TryParse(bgColorStr, out bgColor) && int.TryParse(fontColorStr, out fontColor) &&
                                    (currentBGColor != bgColor || currentFontColor != fontColor))
                                {
                                    rep.BackgroundColor = DiagramClassRepresentation.ColorToBytes(bgColor);
                                    rep.FontColor = DiagramClassRepresentation.ColorToBytes(fontColor);
                                    rep.Apply();
                                }
                            }
                        }
                    }
                }
            }
            return eventType == ObjectEventType.Created ? false : true;
        }

        /// <summary>
        /// Helper function that checks whether one of our open models is present in the model-scope list of the color events.
        /// </summary>
        /// <returns>True when coloring is in scope.</returns>
        private bool IsValidContext()
        {
            List<MEPackage> modelList = ModelSlt.GetModelSlt().Models;
            List<string> presentModels = new List<string>();
            foreach (MEPackage p in modelList) presentModels.Add(p.Name);
            string[] modelScopeList = ContextSlt.GetContextSlt().GetConfigProperty(_ColorModelScope).Split(',');
            foreach (string mdl in modelScopeList) if (presentModels.Contains(mdl.Trim())) return true;
            return false;
        }
    }
}
