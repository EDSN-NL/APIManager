using System;
using System.Windows.Forms;

namespace Plugin.Application.Forms
{
    internal partial class CMChangeMessage : Form
    {
        /// <summary>
        /// Returns the annotation text entered by the user.
        /// </summary>
        internal string Annotation { get { return AnnotationFld.Text; } }

        /// <summary>
        /// Returns the value of the auto-release checkbox.
        /// </summary>
        internal bool AutoRelease { get { return DoAutoRelease.Checked; } }

        /// <summary>
        /// Dialog constructor.
        /// </summary>
        internal CMChangeMessage()
        {
            InitializeComponent();
            DoAutoRelease.Checked = false;
            AnnotationFld.Text = string.Empty;
            Ok.Enabled = false;
        }

        /// <summary>
        /// Invoked when the user leaves the annotation field. We check whether there are at least four characters in the field
        /// and if so, release the 'ok' button.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void AnnotationFld_Leave(object sender, EventArgs e)
        {
            Ok.Enabled = (AnnotationFld.Text.Length > 4);
        }
    }
}
