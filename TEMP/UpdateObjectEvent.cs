using Framework.Event;
using Framework.Model;
using Framework.View;

namespace Plugin.ApplicationBDM.Events
{
    class UpdateObjectEvent : ObjectEventImplementation
    {
        /// <summary>
        /// Simple object event handler that forces the 'plugin-internal' objects to be refreshed on events and also provides some logging.
        /// Please note: eventObject and targetDiagram are managed by the event framework and should NOT be disposed in the event handler!
        /// </summary>
        /// <param name="eventType">The event type that triggered the call. For this handler, this will be "Modified".</param>
        /// <param name="objectType">The originating object type. For this handler, this will be one of "Diagram", "DiagramObject", "Class" or "Package".</param>
        /// <param name="eventObject">The actual object that triggered the event, either a ModelElement specialization or a Diagram.</param>
        /// <param name="targetDiagram">Only in case of creation of Diagram Objects: the diagram on which the object has been created.</param>
        /// <returns>Depending on the event type, the return value is either ignored (Selected, Modified), used to specify whether the associated 
        /// object may be deleted (Deleted) or indicates whether the object has been modified by the event (Created).</returns>
        internal override bool HandleEvent(ObjectEventType eventType, ObjectType objectType, object eventObject, Diagram targetDiagram)
        {
            ModelElement element = null;
            Diagram diagram = null;

            if (objectType == ObjectType.Diagram)
            {
                diagram = eventObject as Diagram;
                if (eventType == ObjectEventType.Modified && diagram != null) diagram.RefreshDiagram(); // Otherwise, we might not see what's changed.
            }
            else
            {
                element = eventObject as ModelElement;
                if (eventType == ObjectEventType.Modified && element != null) element.RefreshModelElement(); // Otherwise, we might not see what's changed.
            }

            return eventType == ObjectEventType.Created ? false : true;
        }
    }
}
