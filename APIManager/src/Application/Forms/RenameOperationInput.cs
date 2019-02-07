using System;
using System.Windows.Forms;
using Framework.Model;
using Framework.ConfigurationManagement;

namespace Plugin.Application.Forms
{
    internal partial class RenameOperationInput : Form
    {
        private MEPackage _parent;
        private bool _operValidation;
        private bool _newMinorVersion;
        private string _oldName;

        internal bool MinorVersionIndicator {get {return this._newMinorVersion; }}
        internal string OperationName {get {return OperationNameFld.Text; }}

        internal RenameOperationInput(string oldName, MEPackage declPackage)
        {
            InitializeComponent();
            this._parent = declPackage;
            this._operValidation = true;
            this._newMinorVersion = false;
            this._oldName = oldName;
            NewMinorVersion.Checked = false;
            ErrorLine.Visible = false;
            if (CMRepositorySlt.GetRepositorySlt().IsCMEnabled) NewMinorVersion.Visible = false;
        }

        private void OperationName_TextChanged(object sender, EventArgs e)
        {
            string errorText = string.Empty;
            this._operValidation = true;

            if (OperationNameFld.Text.Length == 0)
            {
                errorText = "Please specify a new operation name!";
                this._operValidation = false;
            }
            else
            {
                if (!char.IsUpper(OperationNameFld.Text[0]))
                {
                    errorText = "Operation name '" + OperationNameFld.Text + "' must be in PascalCase, please try again!";
                    this._operValidation = false;
                }

                // Check if the name is different from the original name...
                if (this._oldName == OperationNameFld.Text)
                {
                    errorText = "New name '" + OperationNameFld.Text + "' is identical to old name, try again!";
                    this._operValidation = false;
                }

                // Check if this is a unique name...
                if (!this._parent.IsUniqueName(OperationNameFld.Text))
                {
                    errorText = "Operation name '" + OperationNameFld.Text + "' is not unique, try again!";
                    this._operValidation = false;
                }
            }

            if (!this._operValidation)
            {
                ErrorLine.Text = errorText;
                ErrorLine.Visible = true;
            }
            else
            {
                ErrorLine.Clear();
                ErrorLine.Visible = false;
            }
            ErrorLine.Update();
            Ok.Enabled = this._operValidation;
        }

        /// <summary>
        /// Invoked whenever the user toggled the checkbox.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void NewMinorVersion_CheckedChanged(object sender, EventArgs e)
        {
            this._newMinorVersion = NewMinorVersion.Checked;
        }
    }
}
