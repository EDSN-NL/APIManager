using System;
using System.Windows.Forms;
using Plugin.Application.CapabilityModel;
using Framework.Util;

namespace Plugin.Application.Forms
{
    internal partial class CMCommitMessage : Form
    {
        private CMContext.CommitScope _commitScope;

        /// <summary>
        /// Returns the annotation text entered by the user.
        /// </summary>
        internal string Annotation { get { return AnnotationFld.Text; } }

        /// <summary>
        /// Returns the scope of the commit operation as an enumerated type.
        /// </summary>
        internal CMContext.CommitScope CommitScope { get { return this._commitScope; } }

        /// <summary>
        /// Dialog constructor.
        /// </summary>
        internal CMCommitMessage()
        {
            InitializeComponent();
            AnnotationFld.Select();

            // Find out which radio button to select for commit scope...
            this._commitScope = CMContext.CommitScope.Local;
            string defaultScope = EnumConversions<CMContext.CommitScope>.EnumToString(this._commitScope);
            foreach (Control control in ScopeBox.Controls)
            {
                if (control is RadioButton && (string)control.Tag == defaultScope)
                {
                    ((RadioButton)control).Checked = true;
                    break;
                }
            }
            Ok.Enabled = false;
        }

        /// <summary>
        /// Invoked when the user leaves the annotation field. We check whether there are an arbitrary number of characters in the field
        /// and if so, release the 'ok' button.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void AnnotationFld_Leave(object sender, EventArgs e)
        {
            Ok.Enabled = AnnotationFld.Text.Length >= 8;
            ErrorText.Text = Ok.Enabled ? string.Empty : "Please enter at least an 8-character description!";
        }

        /// <summary>
        /// Invoked when the user selects one of the possible commit scope radio buttons.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void ScopeBox_CheckedChanged(object sender, EventArgs e)
        {
            foreach (Control control in ScopeBox.Controls)
            {
                if (control is RadioButton && ((RadioButton)control).Checked)
                {
                    this._commitScope = EnumConversions<CMContext.CommitScope>.StringToEnum((string)control.Tag);
                    break;
                }
            }
        }
    }
}
