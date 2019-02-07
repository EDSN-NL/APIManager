using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Framework.Model;
using Framework.Util;
using Framework.Logging;
using Framework.ConfigurationManagement;
using Plugin.Application.CapabilityModel;
using Plugin.Application.CapabilityModel.API;

namespace Plugin.Application.Forms
{
    internal partial class CreateRESTServiceDeclaration : Form
    {
        private MEPackage _parent;      // Package in which we're creating the new API.
        private SortedList<string, RESTResourceDeclaration> _resourceList;
        private Ticket _remoteTicket;   // True when a valid ticket ID has been entered.
        private bool _hasValidName;     // True when a valid API name has been entered;
        private bool _hasProjectID;     // True when a project ID has been entered;
        private bool _hasTicket;        // True when we have selected a valid ticket;

        /// <summary>
        /// The MetaData property returns a set of user-specified metadata for the new API...
        /// </summary>
        internal RESTInterfaceCapability.MetaData MetaData
        {
            get
            {
                return new RESTInterfaceCapability.MetaData
                {
                    contactEMail    = ContactEMailFld.Text,
                    contactName     = ContactNameFld.Text,
                    contactURL      = ContactURLFld.Text,
                    description     = APIDescriptionFld.Text,
                    licenseName     = LicenseNameFld.Text,
                    licenseURL      = LicenseURLFld.Text,
                    qualifiedName   = APINameFld.Text,
                    termsOfService  = APITermsOfServiceFld.Text
                };
            }
        }

        /// <summary>
        /// Returns the list of resources...
        /// </summary>
        internal List<RESTResourceDeclaration> Resources { get { return new List<RESTResourceDeclaration>(this._resourceList.Values); } }

        /// <summary>
        /// Returns the remote ticket as selected by the user.
        /// </summary>
        internal Ticket RemoteTicket { get { return this._remoteTicket; } }

        /// <summary>
        /// Returns the Project ID entered by the user.
        /// </summary>
        internal string ProjectID { get { return ProjectIDFld.Text; } }

        /// <summary>
        /// Returns the Operational State selected by the user.
        /// </summary>
        internal OperationalState SelectedState
        {
            get
            {
                return EnumConversions<OperationalState>.StringToEnum(OperationalState.Items[OperationalState.SelectedIndex].ToString());
            }
        }

        /// <summary>
        /// Initializes the dialog, disable the Ok button until we have at least a valid name and prepare for the resource list by
        /// creating an empty list to store the resource declarations.
        /// </summary>
        /// <param name="parent"></param>
        internal CreateRESTServiceDeclaration(MEPackage parent)
        {
            InitializeComponent();
            this._parent = parent;
            this._resourceList = new SortedList<string, RESTResourceDeclaration>();
            this._remoteTicket = null;

            // Assign context menus to the appropriate controls...
            ResourceList.ContextMenuStrip = ResourceMenuStrip;

            var repoDescriptor = CMRepositoryDscManagerSlt.GetRepositoryDscManagerSlt().GetCurrentDescriptor();
            if (repoDescriptor == null)
            {
                // We did not find a (proper) entry for the current project, could be an error?
                Logger.WriteWarning("We could not find a (proper) Configuration Management record for the current project, please check Settings Form for errors!");
            }
            if (repoDescriptor != null && repoDescriptor.IsCMEnabled)
            {
                this._hasProjectID = false;
                this._hasTicket = false;
                this._hasValidName = false;
            }
            else
            {
                // No CM, disable the Administration section and pretend we have a valid ticket & projectID
                // (this will enable the Ok button on name only).
                TicketBox.Enabled = false;
                this._hasTicket = true;
                this._hasProjectID = true;
            }

            // Initialize the drop-down box with all Operational States, with the exception of 'Deprecated'...
            foreach (var state in EnumConversions<OperationalState>.GetValues())
            {
                if (state != CapabilityModel.OperationalState.Deprecated)
                    OperationalState.Items.Add(EnumConversions<OperationalState>.EnumToString(state));
            }
            OperationalState.SelectedItem = EnumConversions<OperationalState>.EnumToString(Service._DefaultOperationalState);

            Ok.Enabled = false;
        }

        /// <summary>
        /// This event is raised when the user has created/modified the text in the API Name field.
        /// </summary>
        /// <param name="sender">Ignored/</param>
        /// <param name="e">Ignored.</param>
        private void APIName_TextChanged(object sender, EventArgs e)
        {
            APINameFld.Text = Conversions.ToPascalCase(APINameFld.Text);
            ErrorLine.Text = string.Empty;
            string name = APINameFld.Text.Trim();
            string errorText = string.Empty;
            bool nameValidation = true;

            // Validate input string, must be <name>_V<n>...
            name = name.Trim();
            if (name == string.Empty || !char.IsLetterOrDigit(name[0]))
            {
                errorText = "Please specify a valid API name!";
                nameValidation = false;
            }
            else if (name.Any(Char.IsWhiteSpace))
            {
                errorText = "The name may not contain whitespace, try again!";
                nameValidation = false;
            }
            else if (name.LastIndexOf("_V") <= 0)
            {
                name += "_V1";
                APINameFld.Text = name;
                APINameFld.Update();
            }

            if (nameValidation)
            {
                // Check if this is a unique name...
                if (!this._parent.IsUniqueName(name))
                {
                    errorText = "The chosen API name is not unique, try again!";
                    APINameFld.Clear();
                    nameValidation = false;
                }
            }

            if (errorText != string.Empty) ErrorLine.Text = errorText;
            this._hasValidName = nameValidation;
            CheckOK();
        }

        /// <summary>
        /// This event is raised when the user has selected the 'create new resource' button.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void AddResource_Click(object sender, EventArgs e)
        {
            RESTResourceDeclaration newResource = new RESTResourceDeclaration();
            ErrorLine.Text = string.Empty;
            using (var dialog = new RESTResourceDialog(newResource))
            {
                dialog.DisableMinorVersion();
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    newResource = dialog.Resource;
                    if (!this._resourceList.ContainsKey(newResource.Name))
                    {
                        newResource.Status = RESTResourceDeclaration.DeclarationStatus.Created;
                        this._resourceList.Add(newResource.Name, newResource);
                    }
                    else if (this._resourceList[newResource.Name].Status == RESTResourceDeclaration.DeclarationStatus.Deleted)
                    {
                        newResource.Status = RESTResourceDeclaration.DeclarationStatus.Edited;
                        this._resourceList[newResource.Name] = newResource;
                    }
                    else
                    {
                        ErrorLine.Text = "Duplicate resource name, please try again!";
                        newResource = null;
                    }
                }
            }

            if (newResource != null && newResource.Status != RESTResourceDeclaration.DeclarationStatus.Invalid)
            {
                ListViewItem newItem = new ListViewItem(newResource.Name);
                newItem.SubItems.Add(newResource.Archetype.ToString());
                ResourceList.Items.Add(newItem);
            }
        }

        /// <summary>
        /// Simple check whether we are allowed to enable the OK button.
        /// </summary>
        private void CheckOK()
        {
            Ok.Enabled = this._hasValidName && this._hasTicket && this._hasProjectID;
        }

        /// <summary>
        /// This event is raised when the user wants to delete a defined resource. The resource is removed from the list-view as well
        /// as from the internal list of resources.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void DeleteResource_Click(object sender, EventArgs e)
        {
            if (ResourceList.SelectedItems.Count > 0)
            {
                ListViewItem key = ResourceList.SelectedItems[0];
                ResourceList.Items.Remove(key);
                this._resourceList.Remove(key.Text);
            }
        }

        /// <summary>
        /// This event is raised when the user selected to edit a defined resource declaration.
        /// The event retrieves the selected name and invokes the path.EditResource method to perform the actual edit action.
        /// On return, if we have a valid result (could be unchanged), we update the view in the dialog.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void EditResource_Click(object sender, EventArgs e)
        {
            if (ResourceList.SelectedItems.Count > 0)
            {
                ErrorLine.Text = string.Empty;
                ListViewItem myItem = ResourceList.SelectedItems[0];
                string originalKey = myItem.Text;
                RESTResourceDeclaration resource = this._resourceList.ContainsKey(originalKey) ? this._resourceList[originalKey] : null;
                if (resource != null)
                {
                    using (var dialog = new RESTResourceDialog(resource))
                    {
                        dialog.DisableMinorVersion();
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            if (dialog.Resource.Name == originalKey || !this._resourceList.ContainsKey(resource.Name))
                            {
                                resource = dialog.Resource;
                                resource.Status = RESTResourceDeclaration.DeclarationStatus.Edited;

                                if (resource.Name != originalKey)
                                {
                                    this._resourceList.Remove(originalKey);
                                    this._resourceList.Add(resource.Name, resource);
                                }
                            }
                            else
                            {
                                ErrorLine.Text = "Renaming resource resulted in duplicate name, please try again!";
                                resource = null;
                            }
                        }
                    }
                }
                if (resource != null)
                {
                    myItem.SubItems[0].Text = resource.Name;
                    myItem.SubItems[1].Text = resource.Archetype.ToString();
                }
            }
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
            if (TicketIDFld.Text != string.Empty)
            {
                TicketServerSlt ticketSrv = TicketServerSlt.GetTicketServerSlt();
                Ticket remoteTicket = ticketSrv.GetTicket(TicketIDFld.Text);
                if (remoteTicket == null || ticketSrv.IsClosed(remoteTicket))
                {
                    ErrorLine.Text = "Specified ticket does not exist or is closed, please try again!";
                    TicketIDFld.Text = string.Empty;
                }
                else
                {
                    this._remoteTicket = remoteTicket;
                    this._hasTicket = true;
                    CheckOK();
                }
            }
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
    }
}
