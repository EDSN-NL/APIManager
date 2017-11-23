using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Plugin.Application.Forms
{
    internal partial class ProgressPanel : Form
    {
        /// <summary>
        /// Creates a new ProgressPanel window and initialize the progress bar.
        /// </summary>
        /// <param name="title">Title to assign to the progress panel window.</param>
        /// <param name="maxSteps">Number of steps until bar reaches 100%.</param>
        internal ProgressPanel(string title, int maxSteps)
        {
            InitializeComponent();
            ProgressBar.Minimum = 1;
            ProgressBar.Maximum = maxSteps;
            ProgressBar.Step = 1;
            DoneBar.Visible = false;
            this.Text = title;
        }

        /// <summary>
        /// Unlocks the 'Done' button, which allows the user to close the window.
        /// </summary>
        internal void Done()
        {
            DoneBar.Visible = true;
        }

        /// <summary>
        /// Advances the progress bar by the configured step size.
        /// </summary>
        internal void IncreaseBar(int steps)
        {
            ProgressBar.Increment(steps);
            this.Refresh();
        }

        /// <summary>
        /// Writes a text-string to the box without any formatting.
        /// </summary>
        /// <param name="text">Text string to display.</param>
        internal void WriteText (string text)
        {
            progressBox.AppendText(text);
            this.Refresh();
        }

        /// <summary>
        /// Executed when the user clicks on the 'Done' button. Closes the form and release all resources.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void Done_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Activated whenever the user clicks the 'copy to clipboard' button. Copies the current contents of the progress panel
        /// to the clipboard for use by other applications.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void CopyToClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(progressBox.Text);
        }
    }
}
