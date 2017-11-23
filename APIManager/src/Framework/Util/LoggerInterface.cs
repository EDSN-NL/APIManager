namespace Framework.Logging
{
    /// <summary>
    /// The framework accepts multiple logger implementations that can all be registered with the static logger.
    /// These logger instances can be registered dynamically and will be called in turn.
    /// </summary>
    interface ILogger
    {
        void OpenLog();
        void CloseLog();
        void WriteErrorLog(string logLine);
        void WriteInfoLog(string logLine);
        void WriteWarningLog(string logLine);
    }
}
