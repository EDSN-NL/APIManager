using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Model;

namespace Plugin.Application.Forms
{
    internal partial class AddCodeListInput : Form
    {
        private MEClass _service;       // The service that 'owns' the set of Code Lists to be extended.
        private bool _operValidation;

        /// <summary>
        /// Returns the list of added Code List names.
        /// </summary>
        internal List<string> CodeListNameList
        {
            get
            {
                List<string> nameList = new List<string>();
                string[] names = this.CodeListNames.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (names.Length != 0) foreach (string name in names) nameList.Add(name.Trim());
                return nameList;
            }
        }

        /// <summary>
        /// Default constructor, receives the service class that will be owner of the new code lists.
        /// </summary>
        /// <param name="service">Owner of the CodeList(s).</param>
        internal AddCodeListInput(MEClass service)
        {
            InitializeComponent();
            this._service = service;
            ErrorLine.Visible = false;
            this._operValidation = true;
        }

        /// <summary>
        /// Invoked whenever the user has changes something in the name input field. Check to make sure that correct and unique
        /// names are entered.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CodeListNames_TextChanged(object sender, EventArgs e)
        {
            string errorText = string.Empty;
            this._operValidation = true;

            string[] names = CodeListNames.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (names.Length == 0)
            {
                errorText = "Please specify at least one Code List name!";
                this._operValidation = false;
            }
            else
            {
                foreach (string name in names)
                {
                    string validatedOperation = name.Trim();
                    if (!char.IsUpper(validatedOperation[0]))
                    {
                        errorText = "Code List name '" + name + "' must be in PascalCase, please try again!";
                        this._operValidation = false;
                        break;
                    }

                    // Check if this is a unique name...
                    if (this._service.FindAssociationsByEndpointProperties(name, null).Count > 0)
                    {
                        errorText = "Code List name '" + name + "' is not unique, try again!";
                        this._operValidation = false;
                    }
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
    }
}
