using System;
using System.IO;

namespace Framework.Logging
{
    /// <summary>
    /// The Logfile class is used to create a simple tracelog and write lines to it. It offers rudimentary logging functions.
    /// This works better then the Microsoft standard logging since the latter does not work very well in a DLL environment.
    /// </summary>
    sealed public class FileLogger: ILogger, IDisposable
    {
        private StreamWriter    _logFile = null;
        private string          _fileName = string.Empty;
        private bool            _isOpen = false;

        /// <summary>
        /// Just loads the file name and initializes other properties. The file is not actually opened until the 'open' method
        /// is called.
        /// </summary>
        /// <param name="fileName">File to be used for logging.</param>
        public FileLogger (string fileName)
        {
            this._logFile = null;
            this._fileName = fileName;
            this._isOpen = false;
        }

        /// <summary>
        /// Close the logfile. Any write operations after calling this method will fail to produce output.
        /// </summary>
        public void CloseLog()
        {
            if (this._isOpen)
            {
                try
                {
                    this.WriteInfoLog("Framework.Logging.FileLogger >> Logging closed in file '" + this._fileName + "'.");
                    this._logFile.Flush();
                    this._logFile.Dispose();
                }
                catch { }   //Ignore IO errors. 
                this._isOpen = false;
            }
        }

        /// <summary>
        /// We have to implement this since the class manages a file handle, which is a disposable object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The static open method initialises the logger singleton with a new filename. If a file is already open, the old one is flushed and
        /// closed first, before a new file is opened.
        /// </summary>
        public void OpenLog ()
        {
            try
            {
                if (this._isOpen)
                {
                    this._logFile.Flush();       // Make sure that all contents have been written.
                    this._logFile.Dispose();     // Force release of an existing logfile.
                }

                if (!string.IsNullOrEmpty(this._fileName))
                {
                    this._logFile = new StreamWriter(this._fileName);
                    this._isOpen = true;
                    this.WriteInfoLog("Framework.Logging.FileLogger >> Logging started in file '" + this._fileName + "'.");
                }
                else
                {
                    this._logFile = null;
                    this._isOpen = false;
                }
            }
            catch
            {
                this._isOpen = false;
            }
        }

        /// <summary>
        /// Is called when we want to write to a different filename. If an existing logfile is open, this is flushed and closed
        /// before the new file is opened. When an empty name is passed, the logging is stopped.
        /// <param name="newName">Fully qualified logfile name.</param>
        /// </summary>
        public void SetFileName(string newName)
        {
            try
            {
                if (this._isOpen)
                {
                    this._logFile.Flush();       // Make sure that all contents have been written.
                    this._logFile.Dispose();     // Force release of an existing logfile.
                }

                if (!string.IsNullOrEmpty(newName))
                {
                    this._fileName = newName;
                    this._logFile = new StreamWriter(this._fileName);
                    this._isOpen = true;
                    this.WriteInfoLog("Framework.Logging.FileLogger >> Logging started in file '" + this._fileName + "'.");
                }
                else
                {
                    this._logFile = null;
                    this._isOpen = false;
                }
            }
            catch
            {
                this._isOpen = false;
            }
        }

        /// <summary>
        /// Write a line to the logfile and immediately flushes the file. This assures that the output is written, even if the program crashes immediately afterwards.
        /// Not the most efficient mechanism, but surely the safest in case of error logs...
        /// The operations adds a timestamp and the text '**ERROR' in front of each line and a newline at the end.
        /// On errors, the file is closed and the logwrites ceases operations.
        /// Parameters are:
        /// logLine = string to be written.
        /// </summary>
        /// <param name="logLine"></param>
        public void WriteErrorLog(string logLine)
        {
            if (this._isOpen)
            {
                try
                {
                    this._logFile.WriteLine(DateTime.Now.ToString() + " **ERROR: " + logLine);
                    this._logFile.Flush();
                }
                catch
                {
                    this._logFile.Close();
                    this._isOpen = false;
                }
            }
        }

        /// <summary>
        /// Write a line to the logfile and immediately flushes the file. This assures that the output is written, even if the program crashes immediately afterwards.
        /// Not the most efficient mechanism, but surely the safest in case of error logs...
        /// The operations adds a timestamp in front of each line and a newline at the end.
        /// On errors, the file is closed and the logwrites ceases operations.
        /// Parameters are:
        /// logLine = string to be written.
        /// </summary>
        /// <param name="logLine"></param>
        public void WriteInfoLog(string logLine)
        {
            if (this._isOpen)
            {
                try
                {
                    this._logFile.WriteLine(DateTime.Now.ToString() + " INFO: " + logLine);
                    this._logFile.Flush();
                }
                catch
                {
                    this._logFile.Close();
                    this._isOpen = false;
                }
            }
        }

        /// <summary>
        /// Write a line to the logfile and immediately flushes the file. This assures that the output is written, even if the program crashes immediately afterwards.
        /// Not the most efficient mechanism, but surely the safest in case of error logs...
        /// The operations adds a timestamp and the text '**WARNING' in front of each line and a newline at the end.
        /// On errors, the file is closed and the logwrites ceases operations.
        /// Parameters are:
        /// logLine = string to be written.
        /// </summary>
        /// <param name="logLine"></param>
        public void WriteWarningLog(string logLine)
        {
            if (this._isOpen)
            {
                try
                {
                    this._logFile.WriteLine(DateTime.Now.ToString() + " **WARNING: " + logLine);
                    this._logFile.Flush();
                }
                catch
                {
                    this._logFile.Close();
                    this._isOpen = false;
                }
            }
        }

        /// <summary>
        /// Called on exit and used to explicitly dispose of the file handle.
        /// </summary>
        /// <param name="disposing">True when called from Dispose method.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._logFile.Dispose();
            }
        }
    }
}
