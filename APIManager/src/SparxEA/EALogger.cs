using System;
using EA;
using Framework.Logging;

namespace SparxEA.Logging
{
    /// <summary>
    /// The EA logger is a simple class that provides access to the EA System Output window. We use this primarily to log
    /// Error and Warning messages.
    /// </summary>
    sealed internal class EALogger: ILogger
    {
        private bool _isOpen;
        private Repository _repository;
        private string _tabName;

        /// <summary>
        /// Initializes the class by loading the tab name and repository to use. The logging is NOT started until the 'open'
        /// method is called though. When no tab name is specified, the class will use the default "System" tab.
        /// </summary>
        /// <param name="repository">Default repository to use.</param>
        /// <param name="tabName">Name of the output tab in the system output window. Has default value of "System".</param>
        internal EALogger(Repository repository, string tabName = "System")
        {
            this._repository = repository;
            this._tabName = tabName;
            this._isOpen = false;
        }

        /// <summary>
        /// The openLog method creates an output tab and assures that it is visible to the user. In case the method is 
        /// called on an already open log, the tab is closed first, which will effectively clear the output.
        /// </summary>
        public void OpenLog ()
        {
            try
            {
                // When called on an open logger, the tab is removed first.
                if (_isOpen) this._repository.RemoveOutputTab(this._tabName);
                this._repository.CreateOutputTab(this._tabName);
                this._repository.EnsureOutputVisible(this._tabName);
                this._repository.WriteOutput(this._tabName, DateTime.Now.ToString() + " INFO: SparxEA.Logging.openLog >> EA Logging initiated!", 0);
                this._isOpen = true;
            }
            catch
            {
                this._isOpen = false;
            }
        }

        /// <summary>
        /// Simply set logging status to 'closed'. 
        /// Will NOT remove the tab, since this will also remove all collected logging info ;-)
        /// </summary>
        public void CloseLog()
        {
            this._repository.WriteOutput(this._tabName, DateTime.Now.ToString() + " INFO: SparxEA.Logging.openLog >> EA Logging has stopped!", 0);
            this._isOpen = false;
        }

        /// <summary>
        /// Write a line to the log.
        /// The operations adds a timestamp and the text '**ERROR' in front of each line and a newline at the end.
        /// If the log is not open, nothing will happen.
        /// </summary>
        /// <param name="logLine">Text to write to logging window.</param>
        public void WriteErrorLog(string logLine)
        {
            if (this._isOpen)
            {
                this._repository.EnsureOutputVisible(this._tabName);
                this._repository.WriteOutput(this._tabName, DateTime.Now.ToString() + " **ERROR: " + logLine, 0);
            }
        }

        /// <summary>
        /// Write a line to the log.
        /// The operations adds a timestamp and the text ' INFO' in front of each line and a newline at the end.
        /// If the log is not open, nothing will happen.
        /// Since output to this logstream is not considered to be terribly important, we do not invoke the 'EnsureOutputVisible'
        /// operation for info. messages.
        /// </summary>
        /// <param name="logLine">Text to write to logging window.</param>
        public void WriteInfoLog(string logLine)
        {
            if (_isOpen)
            {
                this._repository.WriteOutput(this._tabName, DateTime.Now.ToString() + " INFO: " + logLine, 0);
            }
        }

        /// <summary>
        /// Write a line to the log.
        /// The operations adds a timestamp and the text ' **WARNING' in front of each line and a newline at the end.
        /// If the log is not open, nothing will happen.
        /// </summary>
        /// <param name="logLine">Text to write to logging window.</param>
        public void WriteWarningLog(string logLine)
        {
            if (_isOpen)
            {
                this._repository.EnsureOutputVisible(this._tabName);
                this._repository.WriteOutput(this._tabName, DateTime.Now.ToString() + " **WARNING: " + logLine, 0);
            }
        }
    }
}
