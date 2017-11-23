using System;
using System.IO;

namespace Framework.Exceptions
{
    /// <summary>
    /// This exception is thrown by the framework when enumeration conversions are called with invalid parameters.
    /// </summary>
    [Serializable]
    public sealed class IllegalEnumException: Exception
    {
        [NonSerialized]
        private string _message;

        public IllegalEnumException()
        {
            this._message = base.Message + " 'Framework.Exception.IllegalEnumException'";
        }

        public IllegalEnumException(string implementationName):
            base("Framework.Exception.IllegalEnumException: '" + implementationName + "'.")
        {
            this._message = base.Message;
        }

        public IllegalEnumException(string implementationName, Exception inner):
            base("Framework.Exception.IllegalEnumException: '" + implementationName + "'.", inner)
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
