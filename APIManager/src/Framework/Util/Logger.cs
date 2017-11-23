using System.Collections.Generic;

namespace Framework.Logging
{
    /// <summary>
    /// The Logfile class is used to create a simple tracelog and write lines to it. It offers rudimentary logging functions.
    /// This works better then the Microsoft standard logging since the latter does not work very well in a DLL environment.
    /// </summary>
    class Logger
    {
        /// <summary>
        /// Helper structure that defines a single logger.
        /// </summary>
        internal struct LogDescriptor
        {
            internal ILogger _logger;
            internal string _loggerName;
            internal bool _info;
            internal bool _warning;
            internal bool _error;

            /// <summary>
            /// Creates new descriptor instance.
            /// </summary>
            /// <param name="logger">Logger instance to register.</param>
            /// <param name="loggerName">A reference name for the logger.</param>
            /// <param name="info">True when to be used for 'info'.</param>
            /// <param name="warning">True when to be used for 'warning'.</param>
            /// <param name="error">True when to be used for 'error'.</param>
            internal LogDescriptor(ILogger logger, string loggerName, bool info, bool warning, bool error)
            {
                this._logger = logger;
                this._loggerName = loggerName;
                this._info = info;
                this._warning = warning;
                this._error = error;
            }
        }

        private static List<LogDescriptor> _loggers = new List<LogDescriptor>();

        /// <summary>
        /// Register a logger with the logging framework. Since there are three possible levels, it must be specified whether
        /// the logger must be called for each of these levels.
        /// </summary>
        /// <param name="logger">The logger to be registered.</param>
        /// <param name="name">Name used for reference.</param>
        /// <param name="info">Set to TRUE when the logger must be called for 'info' logs.</param>
        /// <param name="warning">Set to TRUE when the logger must be called for 'warning' logs.</param>
        /// <param name="error">Set to TRUE when the logger must be called for 'error' logs.</param>
        internal static void RegisterLog(ILogger logger, string name, bool info, bool warning, bool error)
        {
            foreach (LogDescriptor desc in _loggers)
            {
                if (desc._loggerName == name) return;   // We silently ignore requests for repeated registration!
            }
            _loggers.Add(new LogDescriptor(logger, name, info, warning, error));
        }

        /// <summary>
        /// Logging will not start until explicitly instructed to do so. This method simply calls the 'openLog' method
        /// for each registered logger.
        /// </summary>
        internal static void Open ()
        {
            foreach (LogDescriptor desc in _loggers)
            {
                desc._logger.OpenLog();
            }
        }

        /// <summary>
        /// Close all logs.
        /// </summary>
        internal static void Close()
        {
            foreach (LogDescriptor desc in _loggers)
            {
                desc._logger.CloseLog();
            }
        }

        /// <summary>
        /// Writes a line to each logger that has been registered for 'error' output.
        /// </summary>
        /// <param name="logLine">The error message to log.</param>
        internal static void WriteError(string logLine)
        {
            foreach (LogDescriptor desc in _loggers)
            {
                if (desc._error) desc._logger.WriteErrorLog(logLine);
            }
        }

        /// <summary>
        /// Writes a line to each logger that has been registered for 'info' output.
        /// </summary>
        /// <param name="logLine">The error message to log.</param>
        internal static void WriteInfo(string logLine)
        {
            foreach (LogDescriptor desc in _loggers)
            {
                if (desc._info) desc._logger.WriteInfoLog(logLine);
            }
        }

        /// <summary>
        /// Writes a line to each logger that has been registered for 'warning' output.
        /// </summary>
        /// <param name="logLine">The error message to log.</param>
        internal static void WriteWarning(string logLine)
        {
            foreach (LogDescriptor desc in _loggers)
            {
                if (desc._warning) desc._logger.WriteWarningLog(logLine);
            }
        }
    }
}
