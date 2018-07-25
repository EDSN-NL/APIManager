using System;

namespace Framework.ConfigurationManagement
{
    /// <summary>
    /// This exception is thrown by the Configuration Management tools when it is discovered that the local repository is no longer in sync
    /// with the remote repository.
    /// </summary>
    [Serializable]
    public sealed class CMOutOfSyncException: Exception
    {
        [NonSerialized]
        private string _message;

        public CMOutOfSyncException()
        {
            this._message = base.Message + " 'Framework.ConfigurationManagement.CMOutOfSyncException'";
        }

        public CMOutOfSyncException(string additionalInfo):
            base("Framework.ConfigurationManagement.CMOutOfSyncException: '" + additionalInfo + "'.")
        {
            this._message = base.Message;
        }

        public CMOutOfSyncException(string additionalInfo, Exception inner):
            base("Framework.ConfigurationManagement.CMOutOfSyncException: '" + additionalInfo + "'.", inner)
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
