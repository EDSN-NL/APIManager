using System.Collections.Generic;
using Framework.Event;
using Framework.Logging;
using Framework.Model;
using Framework.View;
using Framework.Context;

namespace Plugin.ApplicationBDM.Events
{
    class ColorDiagramEvent : ObjectEventImplementation
    {
        // Configuration properties used by this module...
        private const string _ColorTagName          = "ColorTagName";
        private const string _ColorStereotype       = "ColorStereotype";
        private const string _ColorModelScope       = "ColorModelScope";

        /// <summary>
        /// This event is raised when a (new) diagram is opened. We check whether the context of the diagram is correct and if so, update the colors
        /// of all diagram objects to the color of their respective domains.
        /// </summary>
        /// <param name="eventType">The event type that triggered the call. For this handler, this can only be "Opened".</param>
        /// <param name="objectType">The originating object type. For this handler, this can only be "Diagram".</param>
        /// <param name="eventObject">The actual object that triggered the event, either a ModelElement specialization or a Diagram.</param>
        /// <param name="targetDiagram">Only in case of creation of Diagram Objects: the diagram on which the object has been created.</param>
        /// <returns>Depending on the event type, the return value is either ignored (Selected, Modified), used to specify whether the associated 
        /// object may be deleted (Deleted) or indicates whether the object has been modified by the event (Created).</returns>
        internal override bool HandleEvent(ObjectEventType eventType, ObjectType objectType, object eventObject, Diagram targetDiagram)
        {
            MEPackage currentParent = null;
            Diagram openedDiagram = eventObject as Diagram;
            int newBGColor = -1;
            int newFontColor = -1;

            if (IsValidContext() && openedDiagram != null)
            {
                Logger.WriteInfo("Plugin.ApplicationBDM.Events.ColorDiagramEvent.handleEvent >> Got a '" + eventType + "' event for diagram '" + openedDiagram.Name + "'...");
                foreach (DiagramClassRepresentation rep in openedDiagram.GetRepresentations())
                {
                    Logger.WriteInfo("Plugin.ApplicationBDM.Events.ColorDiagramEvent.handleEvent >> Processing diagram object '" + rep.AssociatedClass.Name + "'...");
                    int currentBGColor = rep.BackgroundColorInt;
                    int currentFontColor = rep.FontColorInt;

                    if (currentParent == null || currentParent.ElementID != rep.AssociatedClass.OwningPackage.ElementID)
                    {
                        // In case we have not yet retrieved our owning package, or the current class is in a different package, retrieve colors.
                        // By 'caching' our domain package, we prevent looking for it for each subsequent class on the diagram (assumption is that most
                        // classes on the diagram are in the same domain)...
                        UpdateColors(rep.AssociatedClass.OwningPackage, out newBGColor, out newFontColor);
                        currentParent = rep.AssociatedClass.OwningPackage;
                    }
                    if (newBGColor != -1 && newFontColor != -1 && (currentBGColor != newBGColor || currentFontColor != newFontColor))
                    {
                        rep.BackgroundColorInt = newBGColor;
                        rep.FontColorInt = newFontColor;
                        rep.Apply();        // Re-draws the element on the diagram according to representation settings.
                    }
                }
            }
            return true;    // Return value does not really matter for this event since it's not used.
        }

        /// <summary>
        /// Helper function that checks whether one of our open models is present in the model-scope list of the color events.
        /// This prevents us from looking for packages that are not in scope anyway.
        /// Models are the "root" packages in the repository browser.
        /// The configuration contains a comma-separated list of the root models that are in scope for coloring. 
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

        /// <summary>
        /// Helper function that retrieves the new background color and font color from the domain package of the specified location package.
        /// </summary>
        /// <param name="classLocation">Package that 'owns' the currently selected class on the diagram.</param>
        /// <param name="newBGColor">Will receive new background color or -1 if unable to locate.</param>
        /// <param name="newFontColor">Will receive new font color or -1 if unable to locate.</param>
        private void UpdateColors(MEPackage classLocation, out int newBGColor, out int newFontColor)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            newBGColor = newFontColor = -1;
            MEPackage domainPackage = classLocation.FindParentWithStereotype(context.GetConfigProperty(_ColorStereotype));
            if (domainPackage != null)
            {
                string colorDefn = domainPackage.GetTag(context.GetConfigProperty(_ColorTagName));
                if (!string.IsNullOrEmpty(colorDefn))
                {
                    int separatorIdx = colorDefn.IndexOf(":");
                    string bgColorStr = colorDefn.Substring(0, separatorIdx);
                    string fontColorStr = colorDefn.Substring(separatorIdx + 1);
                    int.TryParse(bgColorStr, out newBGColor);
                    int.TryParse(fontColorStr, out newFontColor);
                }
            }
        }
    }
}
