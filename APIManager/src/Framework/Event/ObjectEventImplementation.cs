using Framework.View;

namespace Framework.Event
{
    /// <summary>
    /// Object event handlers must be derived from this base class.
    /// </summary>
    internal abstract class ObjectEventImplementation
    {
        protected ObjectEvent _event;

        /// <summary>
        /// Event handlers must implement this function, which performs the actual event processing.
        /// </summary>
        /// <param name="eventType">The event type that triggered the call.</param>
        /// <param name="objectType">The originating object type.</param>
        /// <param name="eventObject">The actual object that triggered the event, either a ModelElement specialization or a Diagram.</param>
        /// <param name="targetDiagram">Only in case of creation of Diagram Objects: the diagram on which the object has been created.</param>
        /// <returns>Depending on the event type, the return value is either ignored (Selected, Modified), used to specify whether the associated 
        /// object may be deleted (Deleted) or indicates whether the object has been modified by the event (Created).</returns>
        internal abstract bool HandleEvent(ObjectEventType eventType, ObjectType objectType, object eventObject, Diagram targetDiagram);

        /// <summary>
        /// Called by the interface class to link interface and implementation together.
        /// </summary>
        /// <param name="itfEvent">The interface object associated with this implementation.</param>
        internal void BindInterface(ObjectEvent objEvent) { this._event = objEvent; }

        /// <summary>
        /// Default constructor, required for production through configuration file.
        /// </summary>
        protected ObjectEventImplementation ()
        {
            this._event = null;
        }
    }
}
