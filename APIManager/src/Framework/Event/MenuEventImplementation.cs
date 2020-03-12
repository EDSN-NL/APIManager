namespace Framework.Event
{
    internal abstract class MenuEventImplementation
    {
        protected MenuEvent _event;

        internal abstract bool IsValidState();
        internal abstract void HandleEvent();

        /// <summary>
        /// Called by the interface class to link interface and implementation together.
        /// </summary>
        /// <param name="itfEvent">The interface object associated with this implementation.</param>
        internal void BindInterface(MenuEvent itfEvent) { this._event = itfEvent; }

        /// <summary>
        /// Default constructor, required for production through configuration file.
        /// </summary>
        protected MenuEventImplementation ()
        {
            this._event = null;
        }
    }
}
