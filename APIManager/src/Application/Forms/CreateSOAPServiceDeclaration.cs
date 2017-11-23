﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Framework.Model;

namespace Plugin.Application.Forms
{
    internal partial class CreateSOAPServiceDeclaration : Form
    {
        private MEPackage _parent;
        private bool _nameValidation;
        private bool _operValidation;

        internal CreateSOAPServiceDeclaration(MEPackage parent)
        {
            InitializeComponent();
            this._parent = parent;
            ErrorLine.Visible = false;
            this._nameValidation = true;
            this._operValidation = true;
        }

        internal string PackageName
        {
            get { return this.PackageNameFld.Text; }
        }

        internal List<string> OperationList
        {
            get { return GetOperationList(); }
        }

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

        private void PackageName_TextChanged(object sender, EventArgs e)
        {
            string name = PackageNameFld.Text.Trim();
            string errorText = string.Empty;
            this._nameValidation = true;

            // Validate input string, must be <name>_V<n>...
            name = name.Trim();
            if (name == string.Empty)
            {
                errorText = "Please specify a Service name!";
                this._nameValidation = false;
            }
            else if (!char.IsUpper(name[0]))
            {
                errorText = "Name must be in PascalCase, try again!";
                this._nameValidation = false;
            }
            else if (name.Any(Char.IsWhiteSpace))
            {
                errorText = "The name may not contain whitespace, try again!";
                this._nameValidation = false;
            }
            else if (name.LastIndexOf("_V") <= 0)
            {
                errorText = "Could not detect major version suffix ('_Vn'), V1 assumed!";
                name += "_V1";
                PackageNameFld.Text = name;
                PackageNameFld.Update();
            }

            if (this._nameValidation)
            {
                // Check if this is a unique name...
                if (!this._parent.IsUniqueName(name))
                {
                    errorText = "The chosen Service name is not unique, try again!";
                    PackageNameFld.Clear();
                    this._nameValidation = false;
                }
            }

            if (errorText != string.Empty)
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
            Ok.Enabled = this._nameValidation && this._operValidation;
        }

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
                foreach (string operation in operations)
                {
                    string validatedOperation = operation.Trim();
                    if (!char.IsUpper(validatedOperation[0]))
                    {
                        errorText = "Operation name '" + operation + "' must be in PascalCase, please try again!";
                        this._operValidation = false;
                        break;
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
            Ok.Enabled = this._nameValidation && this._operValidation;
        }
    }
}
