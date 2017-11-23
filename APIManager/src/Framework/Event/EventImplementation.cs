namespace Framework.Event
{
    internal abstract class EventImplementation
    {
        protected Event _event;

        internal abstract bool IsValidState();
        internal abstract void HandleEvent();

        /// <summary>
        /// Called by the interface class to link interface and implementation together.
        /// </summary>
        /// <param name="itfEvent">The interface object associated with this implementation.</param>
        internal void BindInterface(Event itfEvent) { this._event = itfEvent; }

        /// <summary>
        /// Default constructor, required for production through configuration file.
        /// </summary>
        protected EventImplementation ()
        {
            this._event = null;
        }
    }
}
