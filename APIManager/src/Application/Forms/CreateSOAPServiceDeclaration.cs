using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Framework.Model;
using Framework.ConfigurationManagement;
using Plugin.Application.CapabilityModel;

namespace Plugin.Application.Forms
{
    internal partial class CreateSOAPServiceDeclaration : Form
    {
        private MEPackage _parent;
        private bool _hasValidOperations;   // True when at least one valid operation name has been entered.
        private bool _hasTicket;            // True when a valid ticket ID has been entered.
        private bool _hasValidName;         // True when a valid API name has been entered;
        private bool _hasProjectID;         // True when a project ID has been entered;

        internal string PackageName
        {
            get { return this.PackageNameFld.Text; }
        }

        internal List<string> OperationList
        {
            get { return GetOperationList(); }
        }

        internal CreateSOAPServiceDeclaration(MEPackage parent)
        {
            InitializeComponent();
            this._parent = parent;
            ErrorLine.Visible = false;
            this._hasValidName = false;
            this._hasValidOperations = false;

            if (CMRepositoryDscManagerSlt.GetRepositoryDscManagerSlt().GetCurrentDescriptor().IsCMEnabled)
            {
                this._hasProjectID = false;
                this._hasTicket = false;
            }
            else
            {
                // No CM, disable the Administration section and pretend we have a valid ticket & projectID
                // (this will enable the Ok button on name only).
                TicketBox.Enabled = false;
                this._hasTicket = true;
                this._hasProjectID = true;
            }
            Ok.Enabled = false;
        }

        /// <summary>
        /// Returns the operation names as a list of strings.
        /// </summary>
        /// <returns>List of operation names entered by the user.</returns>
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
        /// This event is raised when the user entered text in the package name field (= service name).
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void PackageName_TextChanged(object sender, EventArgs e)
        {
            string name = PackageNameFld.Text.Trim();
            string errorText = string.Empty;
            this._hasValidName = true;

            // Validate input string, must be <name>_V<n>...
            name = name.Trim();
            if (name == string.Empty)
            {
                errorText = "Please specify a Service name!";
                this._hasValidName = false;
            }
            else if (!char.IsUpper(name[0]))
            {
                errorText = "Name must be in PascalCase, try again!";
                this._hasValidName = false;
            }
            else if (name.Any(Char.IsWhiteSpace))
            {
                errorText = "The name may not contain whitespace, try again!";
                this._hasValidName = false;
            }
            else if (name.LastIndexOf("_V") <= 0)
            {
                errorText = "Could not detect major version suffix ('_Vn'), V1 assumed!";
                name += "_V1";
                PackageNameFld.Text = name;
                PackageNameFld.Update();
            }

            if (this._hasValidName)
            {
                // Check if this is a unique name...
                if (!this._parent.IsUniqueName(name))
                {
                    errorText = "The chosen Service name is not unique, try again!";
                    PackageNameFld.Clear();
                    this._hasValidName = false;
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
            CheckOK();
        }

        /// <summary>
        /// This event is raised when the user leaves the operation names field. There must be at least one operation.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void OperationNames_TextChanged(object sender, EventArgs e)
        {
            string errorText = string.Empty;
            this._hasValidOperations = true;

            string[] operations = OperationNames.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (operations.Length == 0)
            {
                errorText = "Please specify at least one operation name!";
                this._hasValidOperations = false;
            }
            else
            {
                foreach (string operation in operations)
                {
                    string validatedOperation = operation.Trim();
                    if (!char.IsUpper(validatedOperation[0]))
                    {
                        errorText = "Operation name '" + operation + "' must be in PascalCase, please try again!";
                        this._hasValidOperations = false;
                        break;
                    }
                }
            }

            if (!this._hasValidOperations)
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
            CheckOK();
        }

        /// <summary>
        /// This event is raised when the user enters a new ticket number. We check whether the ID represents a
        /// valid ticket (present in Jira with a status not equal to 'open').
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void TicketIDFld_Leave(object sender, EventArgs e)
        {
            ErrorLine.Text = string.Empty;
            this._hasTicket = RMTicket.IsValidID(TicketIDFld.Text);
            if (!this._hasTicket)
            {
                ErrorLine.Text = "Provided ID does not identify a valid open ticket, please try again!";
            }
            else CheckOK();
        }

        /// <summary>
        /// This event is raised when the user entered some text in the project ID field. Since we currently have
        /// no means to validate the project number, we ONLY check whether there are at least 3 characters in the field.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void ProjectIDFld_Leave(object sender, EventArgs e)
        {
            ErrorLine.Text = string.Empty;
            this._hasProjectID = ProjectIDFld.Text.Trim().Length >= 3;
            if (!this._hasProjectID)
            {
                ErrorLine.Text = "Provided ID does not identify a valid project ID (at least 3 characters), please try again!";
            }
            else CheckOK();
        }

        /// <summary>
        /// Simple check whether we are allowed to enable the OK button.
        /// </summary>
        private void CheckOK()
        {
            Ok.Enabled = this._hasValidName && 
                         this._hasTicket && 
                         this._hasProjectID && 
                         this._hasValidOperations;
        }
    }
}
