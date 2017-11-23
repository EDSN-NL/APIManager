using System;
using System.IO;

namespace Framework.Exceptions
{
    /// <summary>
    /// This exception is thrown by the framework when cardinality settings for an attribute or association is wrong.
    /// </summary>
    [Serializable]
    public sealed class IllegalCardinalityException: Exception
    {
        [NonSerialized]
        private string _message;

        public IllegalCardinalityException()
        {
            this._message = base.Message + " 'Framework.Exception.IllegalCardinalityException'";
        }

        public IllegalCardinalityException(string implementationName):
            base("Framework.Exception.IllegalCardinalityException: '" + implementationName + "'.")
        {
            this._message = base.Message;
        }

        public IllegalCardinalityException(string implementationName, Exception inner):
            base("Framework.Exception.IllegalCardinalityException: '" + implementationName + "'.", inner)
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
