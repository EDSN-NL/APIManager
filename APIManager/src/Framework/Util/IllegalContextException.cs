using System;

namespace Framework.Exceptions
{
    /// <summary>
    /// This exception is thrown by the framework when an object is created in the wrong context.
    /// </summary>
    [Serializable]
    public sealed class IllegalContextException: Exception
    {
        [NonSerialized]
        private string _message;

        public IllegalContextException()
        {
            this._message = base.Message + " 'Framework.Exception.IllegalContextException'";
        }

        public IllegalContextException(string contextName):
            base("Framework.Exception.IllegalContextException: '" + contextName + "'.")
        {
            this._message = base.Message;
        }

        public IllegalContextException(string contextName, Exception inner):
            base("Framework.Exception.IllegalContextException: '" + contextName + "'.", inner)
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
