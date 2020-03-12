using System.Collections.Generic;
using Framework.Context;
using Framework.Model;
using Framework.View;

namespace Framework.Event
{
    /// <summary>
    /// Object Event acts as common base class for specialized object event types. An object event is associated with one or more event types (e.g. Created,
    /// Selected, Deleted, Modified) and one or more object types (e.g. Class, Attribute, Connector, etc.).
    /// The main purpose of this base class is to manage the association with the event implementation object, which is responsible for all the 'real work'.
    /// </summary>
    internal class ObjectEvent
    {
        protected ObjectEventImplementation _eventImp;      // The actual event handler instance.
           
        internal string Name { get; }                       // The name of the configured event as specified in the configuration file.
        private List<ObjectEventType> _eventTypes;          // Event scope is limited to this set of event types.
        private List<ObjectType> _objectTypes;              // Event scope is limited to this set of object types.
        private List<string> _stereotypes;                  // Event scope is limited to this set of stereotypes.

        /// <summary>
        /// The constructor creates a new object event interface using the contents from the associated configuration section.
        /// </summary>
        /// <param name="name">Event name as specified in the configuration file.</param>
        /// <param name="eventTypes">Optional list of event types for which this event is valid.</param>
        /// <param name="objectTypes">Optional list of object types for wich this event is valid.</param>
        /// <param name="stereotypes">Optional list of stereotypes for which this event is valid.</param>
        internal ObjectEvent (string name, ObjectEventType eventType, List<ObjectType> objectTypes, List<string> stereotypes)
        {
            Name = name;
            this._eventImp = null;
            this._eventTypes = new List<ObjectEventType>();
            this._eventTypes.Add(eventType);
            this._objectTypes = objectTypes;
            this._stereotypes = stereotypes;
        }

        /// <summary>
        /// Adds an additional event type to the list of event types that this handler will support. If the type already exists, the 
        /// function does not perform any operations.
        /// </summary>
        /// <param name="newType">Type to be added.</param>
        internal void AddEventType(ObjectEventType newType)
        {
            if (!this._eventTypes.Contains(newType)) this._eventTypes.Add(newType);
        }

        /// <summary>
        /// This method is used to (optionally) bind an event handler implementation to the event. When such an implementation is absent, the
        /// event will not be able to perform any operational activities.
        /// </summary>
        /// <param name="imp">Event implementation object.</param>
        internal void Bind(ObjectEventImplementation imp)
        {
            this._eventImp = imp;
            if (imp != null) imp.BindInterface(this);
        }

        /// <summary>
        /// Method must be called to actually process the event. Processing is delegated to the implementation and silently fails if no
        /// such implementation is present!
        /// </summary>
        /// <param name="eventType">The event type that triggered the call.</param>
        /// <param name="objectType">The originating object type.</param>
        /// <param name="eventObject">The actual object that triggered the event, either a ModelElement specialization or a Diagram.</param>
        /// <param name="targetDiagram">Only in case of creation of Diagram Objects: the diagram on which the object has been created.</param>
        /// <returns>Depending on the event type, the return value is either ignored (Selected, Modified), used to specify whether the associated 
        /// object may be deleted (Deleted) or indicates whether the object has been modified by the event (Created).</returns>
        internal bool HandleEvent(ObjectEventType eventType, ObjectType objectType, object eventObject, Diagram targetDiagram) 
        { 
            if (this._eventImp != null) return this._eventImp.HandleEvent(eventType, objectType, eventObject, targetDiagram);
            else return eventType == ObjectEventType.Created ? false : true;
        }

        /// <summary>
        /// This event has valid state ONLY on the following conditions:
        /// The currently active object has a stereotype that exists in the list of stereotypes (only in case of non-diagram objects);
        /// The event type matches one of the event types configured for this handler;
        /// The event object type matches one of the object types configured for this handler.
        /// </summary>
        /// <param name="eventType">The event type that triggered the call.</param>
        /// <param name="objectType">The originating object type.</param>
        /// <param name="eventObject">The actual object that triggered the event, either a ModelElement specialization or a Diagram.</param>
        /// <returns>Result of event implementation context check or 'false' if no implementation present.</returns>
        internal bool IsValidState(ObjectEventType eventType, ObjectType objectType, object eventObject)
        {
            ContextSlt context = ContextSlt.GetContextSlt();          
            bool result = true;

            if (this._eventImp != null)
            {
                if (this._eventTypes != null)  result &= this._eventTypes.Contains(eventType);
                if (this._objectTypes != null) result &= this._objectTypes.Contains(objectType);

                if (this._stereotypes != null)  // Checked only for non-diagram types.
                {
                    ModelElement modelElement = null;
                    if (objectType != ObjectType.Diagram) modelElement = eventObject as ModelElement;
                    if (modelElement != null) result &= modelElement.HasStereotype(this._stereotypes);
                }
            }
            else result = false;        // No implementation, don't bother...
            return result;
        }
    }
}
