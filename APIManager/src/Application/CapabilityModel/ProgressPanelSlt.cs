using System;
using System.Diagnostics;
using Framework.Logging;
using Plugin.Application.Forms;

namespace Plugin.Application.CapabilityModel
{
    /// <summary>
    /// This implements the Processor Manager, a component that creates a list of defined Capability Processor objects. These can 
    /// subsequently be retrieved by searching for their class name (a grouping of processors) and the Processor ID.
    /// </summary>
    internal sealed class ProgressPanelSlt : IDisposable
    {
        // This is the actual Progress Panel singleton. It is created automatically on first load.
        private static readonly ProgressPanelSlt _progressPanelSlt = new ProgressPanelSlt();

        private Stopwatch _stopwatch;       // Used to time a complete sequence from creation to 'done'.
        private ProgressPanel _panel;

        /// <summary>
        /// Controlled dispose of progress panel.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Stops the stopwatch, write a closing line to the panel and unlocks the 'Done' button so the user can close the window.
        /// </summary>
        internal void Done()
        {
            if (this._panel != null)
            {
                this._panel.Activate();
                this._stopwatch.Stop();
                TimeSpan ts = this._stopwatch.Elapsed;
                string elapsedTime = String.Format(" in {0:00}:{1:00}.{2:00} <<", ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                this._panel.WriteText(Environment.NewLine + ">> Finished in " + elapsedTime + " <<");
                this._panel.Done();
                this._panel = null;
            }
        }

        /// <summary>
        /// Public Progress Panel"factory" method. Simply returns the static instance. Also assures that the instance is initialized.
        /// </summary>
        /// <returns>Processor Manager singleton object</returns>
        internal static ProgressPanelSlt GetProgressPanelSlt()
        {
            return _progressPanelSlt;
        }

        /// <summary>
        /// Advances the progress bar with defined number of steps.
        /// </summary>
        /// <param name="steps">Number of steps that we want to bar to advance.</param>
        internal void IncreaseBar(int steps)
        {
            if (this._panel != null) this._panel.IncreaseBar(steps);
        }

        /// <summary>
        /// Prepares the progress panel for use. It creates a new instance and initializes the progress bar with the provided maximum 
        /// number of steps as well as the number of steps to advance on each 'increaseBar' command.
        /// Subsequently, the panel thread is created and started, activating the panel.
        /// </summary>
        /// <param name="title">Title to show on the progress panel window.</param>
        /// <param name="maxSteps">Maximum number of steps to perform in order to reach 100%.</param>
        internal void ShowPanel(string title, int maxSteps)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.ProgressPanelSlt.showPanel >> Showing panel with title '" + title +
                             "' and stepsize '" + maxSteps + "'...");
            this._panel = new ProgressPanel(title, maxSteps);
            this._panel.Show();
            this._stopwatch = new Stopwatch();
            this._stopwatch.Reset();
            this._stopwatch.Start();
        }

        /// <summary>
        /// Write an error message to the progress panel. Each line is preceded by the current time and an error indicator.
        /// </summary>
        /// <param name="index">Number of spaces to add in front of the text.</param>
        /// <param name="text">Text to write.</param>
        internal void WriteError(int index, string text)
        {
            if (this._panel != null)
            {
                this._panel.Activate();
                string prefix = " ";
                for (int i = 0; i < index; i++, prefix += "   ") ;
                this._panel.WriteText(DateTime.Now.ToString() + " **ERROR:" + prefix + " " + text + Environment.NewLine);
            }
        }

        /// <summary>
        /// Write an informational line of text to the progress panel. Each line is preceded by the current time.
        /// </summary>
        /// <param name="index">Number of spaces to add in front of the text.</param>
        /// <param name="text">Text to write.</param>
        internal void WriteInfo(int index, string text)
        {
            if (this._panel != null)
            {
                this._panel.Activate();
                string prefix = " ";
                for (int i = 0; i < index; i++, prefix += "   ") ;
                this._panel.WriteText(DateTime.Now.ToString() + " Info:" + prefix + " " + text + Environment.NewLine);
            }
        }

        /// <summary>
        /// Write a warningmessage to the progress panel. Each line is preceded by the current time and a warning indicator.
        /// </summary>
        /// <param name="index">Number of spaces to add in front of the text.</param>
        /// <param name="text">Text to write.</param>
        internal void WriteWarning(int index, string text)
        {
            if (this._panel != null)
            {
                this._panel.Activate();
                string prefix = " ";
                for (int i = 0; i < index; i++, prefix += "   ") ;
                this._panel.WriteText(DateTime.Now.ToString() + " **WARNING:" + prefix + " " + text + Environment.NewLine);
            }
        }
        
        /// <summary>
        /// Called on exit and used to explicitly dispose of the file handle.
        /// </summary>
        /// <param name="disposing">True when called from Dispose method.</param>
        private void Dispose(bool disposing)
        {
            if (disposing) this._panel.Dispose();
        }

        /// <summary>
        /// The private constructor is called once on initial load and assures that exactly one valid object is present at all times.
        /// </summary>
        private ProgressPanelSlt()
        {
            this._panel = null;
        }
    }
}
