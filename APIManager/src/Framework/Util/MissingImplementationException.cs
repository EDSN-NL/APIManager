using System;
using System.IO;

namespace Framework.Exceptions
{
    /// <summary>
    /// This exception is thrown by the framework when an implementation object could not be found.
    /// </summary>
    [Serializable]
    public sealed class MissingImplementationException: Exception
    {
        [NonSerialized]
        private string _message;

        public MissingImplementationException()
        {
            this._message = base.Message + " 'Framework.Exception.MissingImplementationException'";
        }

        public MissingImplementationException(string implementationName):
            base("Framework.Exception.MissingImplementationException: '" + implementationName + "'.")
        {
            this._message = base.Message;
        }

        public MissingImplementationException(string implementationName, Exception inner):
            base("Framework.Exception.MissingImplementationException: '" + implementationName + "'.", inner)
        {
            this._message = base.Message;
        }

        public override string Message
        {
            get
            {
                return this._message;
            }
        }
    }
}
