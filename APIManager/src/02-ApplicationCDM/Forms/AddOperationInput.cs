using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Model;
using Framework.ConfigurationManagement;

namespace Plugin.Application.Forms
{
    internal partial class AddOperationInput : Form
    {
        private MEPackage _parent;
        private bool _operValidation;
        private bool _newMinorVersion;

        internal List<string> OperationList
        {
            get { return GetOperationList(); }
        }

        internal bool MinorVersionIndicator
        {
            get { return this._newMinorVersion; }
        }

        internal AddOperationInput(MEPackage parent)
        {
            InitializeComponent();
            this._parent = parent;
            ErrorLine.Visible = false;
            this._operValidation = true;
            this._newMinorVersion = false;
            NewMinorVersion.Checked = false;

            if (CMRepositorySlt.GetRepositorySlt().IsCMEnabled) NewMinorVersion.Visible = false;
        }

        /// <summary>
        /// Returns the list of operation names entered by the user.
        /// </summary>
        /// <returns>List of names in Pascal Case.</returns>
        private List<string> GetOperationList()
        {
            string[] operations = this.OperationNames.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> operationList = new List<string>();
            if (operations.Length != 0)
            {
                foreach (string operation in operations)
                {
                    operationList.Add(operation.Trim());
                }
            }
            return operationList;
        }

        /// <summary>
        /// Invoked whenever the user made changes to the text field. Validates the operation names...
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void OperationNames_TextChanged(object sender, EventArgs e)
        {
            string errorText = string.Empty;
            this._operValidation = true;

            string[] operations = OperationNames.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (operations.Length == 0)
            {
                errorText = "Please specify at least one operation name!";
                this._operValidation = false;
            }
            else
            {
                string checkedStrings = string.Empty;
                foreach (string operation in operations)
                {
                    string validatedOperation = operation.Trim();
                    if (!char.IsUpper(validatedOperation[0]))
                    {
                        errorText = "Operation name '" + validatedOperation + "' must be in PascalCase, please try again!";
                        this._operValidation = false;
                        break;
                    }

                    // Check if this is a unique name...
                    if (!this._parent.IsUniqueName(validatedOperation))
                    {
                        errorText = "Operation name '" + validatedOperation + "' is not unique, try again!";
                        this._operValidation = false;
                        break;
                    }
                    if (checkedStrings.Contains(validatedOperation))
                    {
                        errorText = "Operation name '" + validatedOperation + "' is specified multiple times, try again!";
                        this._operValidation = false;
                        break;
                    }
                    checkedStrings += "," + validatedOperation;
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
        /// Called whenever the user changes the state of the checkbox.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>m>
        private void NewMinorVersion_CheckedChanged(object sender, EventArgs e)
        {
            this._newMinorVersion = NewMinorVersion.Checked;
        }
    }
}
