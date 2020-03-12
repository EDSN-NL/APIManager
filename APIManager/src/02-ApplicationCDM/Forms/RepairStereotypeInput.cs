using System;
using System.Windows.Forms;

namespace Plugin.Application.Forms
{
    public partial class RepairStereotypeInput : Form
    {
        /// <summary>
        /// Returns the fully qualified stereotype name entered by the user.
        /// </summary>
        internal string Stereotype { get { return SelectedProfileName.Text + "::" + SelectedStereotype.Text; } }

        /// <summary>
        /// Returns true if we have to check the entire package hierarchy, false for current package only.
        /// </summary>
        internal bool CheckHierarchy { get { return EntireHierarchy.Checked; } }

        public RepairStereotypeInput()
        {
            InitializeComponent();

            SelectedProfileName.Text = "Enexis ECDM";
            SelectedStereotype.Text = string.Empty;
            EntireHierarchy.Checked = false;
            Ok.Enabled = false;
        }

        /// <summary>
        /// Raised when the user finishes editing the stereotype name field. If there is any text in there
        /// (length > 0), we enable the Ok button.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void SelectedStereotype_Leave(object sender, EventArgs e)
        {
            if (SelectedStereotype.Text.Length > 0) Ok.Enabled = true;
        }
    }
}
