using System.Collections.Generic;
using Framework.Context;
using Framework.Model;

namespace Framework.Event
{
    /// <summary>
    /// Event acts as common base class for specialized interface event types. The specialized types are associated with different event scopes,
    /// which currently are 'Controller' (general purpose events), 'Diagram' (events emitted from diagram objects) and 'PackageTree' (events
    /// emitted from the package structure). The main purpose of this base class is to manage the association with the event implementation
    /// object, which is responsible for all the 'real work'.
    /// </summary>
    internal class Event
    {
        protected EventImplementation _eventImp;    // The actual event handler instance.
        private string _name;                       // The name of the configured event.
        private TreeScope _scope;                   // Identifies the event tree in which this event lives.
        private List<string> _packageNames;         // Event scope is limited to this set of package names.
        private List<string> _stereotypes;          // Event scope is limited to this set of stereotypes.
        private List<string> _classNames;           // Event scope is limited to this set of class names.

        internal string Name {get {return this._name; }}
        internal TreeScope Scope                      // Scope is a read/write attribute!
        {
            get { return this._scope; }
            set { this._scope = value; }
        }

        /// <summary>
        /// The constructor creates a new event implementation using the contents from the provided configuration section.
        /// </summary>
        /// <param name="name">Event name.</param>
        /// <param name="packages">Optional list of packages for which this event is valid.</param>
        /// <param name="stereotypes">Optional list of stereotypes for wich this event is valid.</param>
        /// <param name="classes">Optional list of classes for which this event is valid.</param>
        internal Event (string name, List<string> packages, List<string> stereotypes, List<string> classes)
        {
            this._name = name;
            this._eventImp = null;
            this._packageNames = packages;
            this._stereotypes = stereotypes;
            this._classNames = classes;
            this._scope = TreeScope.Undefined;
        }

        /// <summary>
        /// This method is used to (optionally) bind an event handler implementation to the event. When such an implementation is absent, the
        /// event will not be able to perform any operational activities.
        /// </summary>
        /// <param name="imp"></param>
        internal void Bind(EventImplementation imp)
        {
            this._eventImp = imp;
            if (imp != null) imp.BindInterface(this);
        }

        /// <summary>
        /// Method must be called to actually process the event. Processing is delegated to the implementation and silently fails if no
        /// such implementation is present!
        /// </summary>
        internal void HandleEvent() { if (this._eventImp != null) this._eventImp.HandleEvent(); }

        /// <summary>
        /// This event has valid state ONLY on the following conditions:
        /// The current package exists in the list of package names (or when this list is empty).
        /// The current class exists in the list of class names (or when this list is empty).
        /// The currently active object (either a class or a package) has a stereotype that exists in the list of stereotypes.
        /// The event has an implementation and the implementation reports a valid state.
        /// </summary>
        /// <returns>Result of event implementation context check or 'false' if no implementation present.</returns>
        internal bool IsValidState()
        {
            ContextSlt context = ContextSlt.GetContextSlt();          
            bool result = true;

            if (this._eventImp != null)
            {
                if (this._packageNames != null)
                {
                    MEPackage currentPackage = context.CurrentPackage;
                    result &= ((currentPackage != null) && (this._packageNames.Contains(currentPackage.Name)));
                }

                if (this._classNames != null)
                {
                    MEClass currentClass = context.CurrentClass;
                    result &= ((currentClass != null) && (this._classNames.Contains(currentClass.Name)));
                }

                if (this._stereotypes != null)
                {
                    ModelElement activeElement = context.GetActiveElement();
                    if (activeElement != null) result &= activeElement.HasStereotype(this._stereotypes);
                }
                result &= this._eventImp.IsValidState();
            }
            else result = false;        // Missing implementation is show stopper!
            return result;
        }
    }
}
