using System;
using System.Collections.Generic;
using Framework.Context;
using Framework.Event;
using Framework.Model;
using Framework.Logging;

namespace Framework.Controller
{
    /// <summary>
    /// The controller singleton is responsible for initialization of the framework and dispatching of events.
    /// </summary>
    sealed class ControllerSlt
    {
        // This is the actual Controller singleton. It is created automatically on first load.
        private static readonly ControllerSlt _controller = new ControllerSlt();
        private EventManager _eventManager; // The Event Manager is wrapped and controlled by the ControllerSlt and must not be used directly!
        private bool _enableScopeSwitch;    // Set to 'true' to process Scope Switches, 'false' to ignore them. 
        private bool _enableEvents;         // Set to 'true' to allow event processing.

        /// <summary>
        /// Enables or disables event processing. Since EA generates events at, sometimes, inconvenient moments (i.e. right in  the middle of an
        /// model element creation), we must have a means to 'defer' event processing until the objects are stable. The low-level model code
        /// uses this function to enable or disable event processing until the system is in a stable state.
        /// </summary>
        internal bool EnableEvents { set { this._enableEvents = value; } }

        /// <summary>
        /// Enables or disables scope switches. Since EA generates scope switches whenever a new object is created and we want to defer the switch
        /// until we are certain that the created object has reached a valid state, we can use this property to block scope switch processing.
        /// </summary>
        internal bool EnableScopeSwitch   { set { this._enableScopeSwitch = value; } }

        /// <summary>
        /// Removes all current context from both the model and the context environments. Can be used to force a 'clean slate'.
        /// The current class and current package are re-initialised after the flush in order to keep the context valid.
        /// </summary>
        internal void Flush ()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            Logger.WriteInfo("Framework.Controller.ControllerSlt.flush >>  Flushing current context...");

            // In order to facilitate reconstruction of current package and class after a flush, we save copies of these
            // (context returns new instances not references) and use these to simulate a scope switch.
            MEPackage currentPackage = context.CurrentPackage;
            MEClass currentClass = context.CurrentClass;
            View.Diagram currentDiagram = context.CurrentDiagram;

            string diagramName = currentDiagram != null ? currentDiagram.Name : "-No diagram-";
            string packageName = currentPackage != null ? currentPackage.Name : "-No package-";
            string className = currentClass != null ? currentClass.Name : "-No class-";

            context.Flush();                   // Clean context first (might use the model).
            ModelSlt.GetModelSlt().Flush();    // Clean the model context.

            // Simulate a scope switch in order to get the context back to a consistent state...
            try
            {
                Logger.WriteInfo("Framework.Controller.ControllerSlt.flush >> Restoring current diagram (" +
                                 diagramName + "), package (" + packageName + ") and class (" + className + ")...");
                if (currentPackage != null && currentPackage.Valid) SwitchScope(ContextScope.Package, currentPackage.ElementID, currentPackage.GlobalID);
                if (currentClass != null && currentClass.Valid) SwitchScope(ContextScope.Class, currentClass.ElementID, currentClass.GlobalID);
                if (currentDiagram != null && currentDiagram.Valid) SwitchScope(ContextScope.Diagram, currentDiagram.DiagramID, currentDiagram.GlobalID);
            }
            catch (Exception exc)
            {
                Logger.WriteInfo("Framework.Controller.ControllerSlt.flush >> Got an exception during context restore, DB might have changed!" + 
                                 Environment.NewLine + exc.ToString());
            }
        }

        /// <summary>
        /// Public Controller "factory" method. Simply returns the static instance.
        /// </summary>
        /// <returns>Context singleton object</returns>
        internal static ControllerSlt GetControllerSlt() { return _controller; }

        /// <summary>
        /// Delegate method that requests the list of configured events from the event manager.
        /// The method returns all events that are defined immediately below the node with specified name.
        /// </summary>
        /// <param name="scope">Which event list to retrieve.</param>
        /// <param name="rootName">Where we want to start looking.</param>
        /// <returns>List of events or NULL in case of errors.</returns>
        internal List<EventManager.MenuEventNode> GetEventList(TreeScope scope, string rootName)
        {
            return (this._eventManager != null) ? this._eventManager.GetMenuEventList(scope, rootName) : null;
        }

        /// <summary>
        /// Delegate method that attempts to process the specified event at the event manager. When no manager is available, the method
        /// silently fails.
        /// </summary>
        /// <param name="scope">Which event list to search for the event.</param>
        /// <param name="parentName">Parent of the specified event.</param>
        /// <param name="eventName">Name of the event to execute.</param>
        internal void HandleEvent(TreeScope scope, string parentName, string eventName)
        {
            if (this._eventManager != null) this._eventManager.HandleMenuEvent(scope, parentName, eventName);
        }

        /// <summary>
        /// Delegate method that attempts to process the specified object event at the event manager. When no manager is available, the method
        /// silently fails. This object event handler is called when the repository raises an event on a repository object. 
        /// </summary>
        /// <param name="eventType">The type of event that has been raised (one of Created, Deleted, Selected or Modified).</param>
        /// <param name="objectType">Identifies the type of object represented by either objectID or objectGUID.</param>
        /// <param name="objectID">Repository-specific object identifier, contains -1 when unknown.</param>
        /// <param name="objectGUID">Repository-independent, global, object identifier, contains empty string when unknown.</param>
        /// <param name="diagramID">In case of a create new Diagram Object, this parameter contains the ID of the diagram on which the object is created.</param>
        /// <returns>Depending on the eventType, the return value has different meaning: for Modified/Selected events, the return value should be 'true', 
        /// in case of a 'Created' event, the return value indicates whether or not the object has been modified by the event handler(s) and in case of 
        /// a 'Deleted' event, the return value indicates whether or not the object is allowed to be deleted.</returns>
        internal bool HandleObjectEvent(ObjectEventType eventType, ObjectType objectType, int objectID, string objectGUID, int diagramID = -1)
        {
            bool operationResult = eventType == ObjectEventType.Created ? false : true;
            if (this._eventManager != null && this._enableEvents && (objectID > 0 || objectGUID != string.Empty))
            {
                ModelElement subjectElement = null;
                View.Diagram subjectDiagram = null;
                View.Diagram targetDiagram = diagramID > 0 ? new View.Diagram(diagramID) : null;
                switch (objectType)
                {
                    case ObjectType.Attribute:
                        subjectElement = objectID > 0 ? new MEAttribute(objectID) : new MEAttribute(objectGUID);
                        break;

                    case ObjectType.Class:
                    case ObjectType.DiagramObject:
                        subjectElement = objectID > 0 ? new MEClass(objectID) : new MEClass(objectGUID);
                        break;

                    case ObjectType.Object:
                        subjectElement = objectID > 0 ? new MEObject(objectID) : new MEObject(objectGUID);
                        break;

                    case ObjectType.Connector:
                        subjectElement = objectID > 0 ? new MEAssociation(objectID) : new MEAssociation(objectGUID);
                        break;

                    case ObjectType.Diagram:
                        subjectDiagram = objectID > 0 ? new View.Diagram(objectID) : new View.Diagram(objectGUID);
                        break;

                    case ObjectType.Package:
                        subjectElement = objectID > 0 ? new MEPackage(objectID) : new MEPackage(objectGUID);
                        break;

                    default:
                        break;  // Unknown/undefined object type, no action!
                }

                // Since we create the event object in this function, we should also dispose of them when done...
                if (subjectElement != null)
                {
                    operationResult = this._eventManager.HandleObjectEvent(eventType, objectType, subjectElement, targetDiagram);
                    if (targetDiagram != null) targetDiagram.Dispose();
                    subjectElement.Dispose();
                }
                else if (subjectDiagram != null)
                {
                    operationResult = this._eventManager.HandleObjectEvent(eventType, objectType, subjectDiagram);
                    subjectDiagram.Dispose();
                }
            }
            return operationResult;
        }

        /// <summary>
        /// This method is called during startup of the plugin and must initialize all controller-specific stuff.
        /// Note that we can't write to the log file until after initialization is complete!
        /// </summary>
        internal void Initialize(ContextImplementation contextImp, ModelImplementation modelImp)
        {
            ModelSlt.GetModelSlt().Initialize(modelImp);
            ContextSlt.GetContextSlt().Initialize(contextImp);
            this._eventManager = new EventManager();    // Will create all events as defined in configuration file.
            this._enableScopeSwitch = true;
            this._enableEvents = true;
        }

        /// <summary>
        /// Delegate method that checks whether the event indicated by scope, parent and name has valid state. If the event is a group, the state
        /// is valid only if at least one child event within the group is valid.
        /// The method returns false when there is no event manager or the event reports an invalid state.
        /// </summary>
        /// <param name="scope">Which event tree to search.</param>
        /// <param name="parentName">The event that acts as root for the specified event.</param>
        /// <param name="eventName">The event to be checked.</param>
        /// <returns>True in case of valid state, false on invalid state or errors.</returns>
        internal bool IsValidState(TreeScope scope, string parentName, string eventName)
        {
			return(this._eventManager != null) && this._eventManager.IsValidState(scope, parentName, eventName);
        }

        /// <summary>
        /// This method is called during plugin shutdown and must release resources...
        /// </summary>
        internal void ShutDown()
        {
            this._eventManager = null;
            this._enableScopeSwitch = false;
            this._enableEvents = false;
            ContextSlt.GetContextSlt().ShutDown();  // Since context uses the model, close this one first!
            ModelSlt.GetModelSlt().ShutDown();
        }

        /// <summary>
        /// Called when the user has clicked on a modelling element, a diagram or a package. The method is used to pass
        /// this information to the context so that the current scope can be updated. 
        /// The context switch is only executed when 'EnableScopeSwitch' is set to 'true'.
        /// </summary>
        /// <param name="newScope">The type of element that has received focus.</param>
        /// <param name="itemID">Identification of that element within the tool repository (tool-dependent format)</param>
        /// <param name="uniqueID">Globally-unique ID of the item.</param>
        internal void SwitchScope(ContextScope newScope, int itemID, string uniqueID)
        {
            if (this._enableScopeSwitch) ContextSlt.GetContextSlt().SwitchScope(newScope, itemID, uniqueID);
        }

        /// <summary>
        /// The private constructor is called once on initial load and initializes all controller properties.
        /// </summary>
        private ControllerSlt()
        {
            this._eventManager = null;
        }
    }
}
