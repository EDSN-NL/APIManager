using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Logging;
using Framework.Util;
using Framework.Model;
using Framework.Context;
using Plugin.Application.CapabilityModel.API;

namespace Plugin.Application.Forms
{
    public partial class RESTResourceDialog : Form
    {
        // Configuration properties used by this module...
        private const string _EmptyResourceName         = "EmptyResourceName";

        private RESTResourceDeclaration _resource;      // The resource declaration descriptor that we're creating / editing.
        private bool _isEdit;                           // True means we're editing a path expression, false when creating a new one.

        // Preconditions for enabling the OK button...
        private bool _hasType;
        private bool _hasName;

        /// <summary>
        /// Returns true when the user has checked the 'new minor version' checkmark.
        /// </summary>
        internal bool MinorVersionIndicator { get { return NewMinorVersion.Checked; } }

        /// <summary>
        /// Returns the constructed Resource.
        /// </summary>
        internal RESTResourceDeclaration Resource { get { return this._resource; } }

        /// <summary>
        /// Initialise the Resource dialog using the information in the provided Resource Declaration.
        /// </summary>
        /// <param name="resource">Resource declaration to use for initialisation.</param>
        internal RESTResourceDialog(RESTResourceDeclaration resource)
        {
            Logger.WriteInfo("Plugin.Application.Forms.RESTResourceDialog >> Initializing with resource: '" + resource.Name + "'...");
            InitializeComponent();

            this._isEdit = resource.Status != RESTResourceDeclaration.DeclarationStatus.Invalid;
            this.Text = this._isEdit ? "Edit Resource" : "Create new Resource";
            this._resource = resource;

            ResourceNameFld.Text = resource.Name;
            ParameterName.Text = resource.Parameter.Name;
            ParameterClassifier.Text = resource.Parameter.Classifier != null? resource.Parameter.Classifier.Name: string.Empty;

            // Load the Operations in case of Edit...
            if (this._isEdit && resource.Operations != null)
            {
                foreach (RESTOperationDeclaration operation in resource.Operations)
                {
                    if (operation.Status != RESTOperationDeclaration.DeclarationStatus.Invalid)
                    {
                        ListViewItem newItem = new ListViewItem(operation.Name);
                        newItem.SubItems.Add(operation.Archetype.ToString());
                        OperationsList.Items.Add(newItem);
                    }
                }
            }

            // Initialize the documentation boxes...
            IsTag.Checked = resource.IsTag;
            DocDescription.Text = resource.Description;
            ExternalDocDescription.Text = resource.ExternalDocDescription;
            ExternalDocURL.Text = resource.ExternalDocURL;

            // Initialize the drop-down box with the possible values of our Resource archetype enumeration...
            ResourceTypeBox.Items.AddRange(EnumConversions<RESTResourceCapability.ResourceArchetype>.GetNamesArray());
            
            // Statement below enforces a 'SelectedIndexChanges' event, which will update _currentType and set dialog items access 
            // according to the resource type...
            ResourceTypeBox.SelectedIndex = this._isEdit ? (int)resource.Archetype : 0;

            // Assign context menus to the appropriate controls...
            OperationsList.ContextMenuStrip = OperationMenuStrip;

            ResourceNameFld.Text = resource.Name;
            Ok.Enabled = this._isEdit;
            NewMinorVersion.Checked = false;
            this._hasType = (this._resource.Archetype != RESTResourceCapability.ResourceArchetype.Unknown);
            this._hasName = (resource.Name != string.Empty);

            // Initialise our tool tips...
            AddOperationToolTip.SetToolTip(AddOperation, "Add a new REST operation to this Path Expression.");
            DeleteOperationToolTip.SetToolTip(DeleteOperation, "Delete the REST operation and all associated resources from Path Expression.");
            EditOperationToolTip.SetToolTip(EditOperation, "Open the REST operation 'Edit' dialog for editing of parameters.");
        }

        /// <summary>
        /// This event is raised when the user clicks the 'add operation' button.
        /// We invoke the path.AddOperation function, which facilitates the user to create a new operation, including all parameters and
        /// configuration. On return, if we have a valid operation, we update the list in the dialog.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void AddOperation_Click(object sender, EventArgs e)
        {
            RESTOperationDeclaration operation = this._resource.AddOperation();
            if (operation != null && operation.Status != RESTOperationDeclaration.DeclarationStatus.Invalid)
            {
                ListViewItem newItem = new ListViewItem(operation.Name);
                newItem.SubItems.Add(operation.Archetype.ToString());
                OperationsList.Items.Add(newItem);
            }
        }

        /// <summary>
        /// This event is raised when the user wants to delete a defined operation declaration.
        /// We don't actually delete the associated declaration record, but mark it as 'deleted', so that we can detect the change lateron and
        /// update the model accordingly
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void DeleteOperation_Click(object sender, EventArgs e)
        {
            if (OperationsList.SelectedItems.Count > 0)
            {
                ListViewItem key = OperationsList.SelectedItems[0];
                this._resource.DeleteOperation(key.Text);
                OperationsList.Items.Remove(key);
            }
        }

        /// <summary>
        /// Suppresses the New Minor Version checkmark for those cases that this option is not required.
        /// </summary>
        internal void DisableMinorVersion()
        {
            NewMinorVersion.Visible = false;
        }

        /// <summary>
        /// This event is raised when the user selected to edit a defined operation declaration.
        /// The event retrieves the selected name and invokes the path.EditOperation method to perform the actual edit action.
        /// On return, if we have a valid result (could be unchanged), we update the view in the dialog.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void EditOperation_Click(object sender, EventArgs e)
        {
            if (OperationsList.SelectedItems.Count > 0)
            {
                ListViewItem myItem = OperationsList.SelectedItems[0];
                string originalKey = myItem.Text;
                RESTOperationDeclaration operation = this._resource.EditOperation(myItem.Text);
                if (operation != null)
                {
                    myItem.SubItems[0].Text = operation.Name;
                    myItem.SubItems[1].Text = operation.Archetype.ToString();
                }
            }
        }

        /// <summary>
        /// This event is raised when the user selects a different type of Resource.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void ResourceTypeBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = ResourceTypeBox.SelectedIndex;
            RESTResourceCapability.ResourceArchetype oldType = this._resource.Archetype;
            this._resource.Archetype = EnumConversions<RESTResourceCapability.ResourceArchetype>.StringToEnum(ResourceTypeBox.Items[ResourceTypeBox.SelectedIndex].ToString());

            // Enable/Disable all controls according to the (new) type...
            switch (this._resource.Archetype)
            {
                case RESTResourceCapability.ResourceArchetype.Collection:
                case RESTResourceCapability.ResourceArchetype.Store:
                case RESTResourceCapability.ResourceArchetype.Controller:
                    CreateDocument.Enabled = false;
                    LinkDocument.Enabled = false;
                    PropertiesBox.Enabled = false;
                    OperationsBox.Enabled = true;
                    ResourceNameFld.Enabled = true;
                    ResourceNameFld.ReadOnly = false;
                    ResourceNameFld.Clear();
                    this._resource.ClearParameter();
                    this._resource.ClearDocumentClass();
                    if (!this._isEdit)
                    {
                        this._resource.IsTag = true;
                        IsTag.Checked = true;
                    }
                    break;

                case RESTResourceCapability.ResourceArchetype.Document:
                    CreateDocument.Enabled = true;
                    LinkDocument.Enabled = true;
                    PropertiesBox.Enabled = false;
                    OperationsBox.Enabled = false;
                    OperationsList.Items.Clear();
                    ResourceNameFld.Enabled = false;
                    ResourceNameFld.ReadOnly = true;
                    ResourceNameFld.Clear();
                    this._resource.ClearParameter();
                    this._resource.IsTag = false;
                    IsTag.Checked = false;
                    break;

                case RESTResourceCapability.ResourceArchetype.Unknown:
                    CreateDocument.Enabled = false;
                    LinkDocument.Enabled = false;
                    PropertiesBox.Enabled = false;
                    OperationsBox.Enabled = false;
                    OperationsList.Items.Clear();
                    ResourceNameFld.Enabled = false;
                    ResourceNameFld.ReadOnly = false;
                    ResourceNameFld.Clear();
                    this._resource.ClearParameter();
                    this._resource.ClearDocumentClass();
                    this._resource.IsTag = false;
                    IsTag.Checked = false;
                    break;

                case RESTResourceCapability.ResourceArchetype.Identifier:
                    CreateDocument.Enabled = false;
                    LinkDocument.Enabled = false;
                    PropertiesBox.Enabled = true;
                    OperationsBox.Enabled = true;
                    ResourceNameFld.Enabled = false;
                    ResourceNameFld.ReadOnly = true;
                    ResourceNameFld.Clear();
                    ResourceNameFld.Text = this._resource.Name;
                    this._resource.ClearDocumentClass();
                    if (!this._isEdit)
                    {
                        this._resource.IsTag = true;
                        IsTag.Checked = true;
                    }
                    break;
            }
            this._hasType = this._resource.Archetype != RESTResourceCapability.ResourceArchetype.Unknown;
            this._hasName = (ResourceNameFld.Text != string.Empty);
            ValidateName();
            CheckOk();
        }

        /// <summary>
        /// This event is raised when the user has inserted or changed the name of the resource and leaves the input field.
        /// The event checks whether the provided name is a valid one in the current context.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void ResourceNameFld_Leave(object sender, EventArgs e)
        {
            ValidateName();
        }

        /// <summary>
        /// Method is called to check whether the resource name entered by the user is valid in the current context. This must be checked
        /// whenever the name input field is exited and when the resource type is changed.
        /// </summary>
        private void ValidateName()
        {
            string errorText = string.Empty;
            bool isOk = true;
            this._hasName = false;
            if (!string.IsNullOrEmpty(ResourceNameFld.Text))
            {
                if (!char.IsUpper(ResourceNameFld.Text[0]) && ResourceNameFld.Text[0] != '[')
                {
                    ResourceNameFld.Text = Conversions.ToPascalCase(ResourceNameFld.Text);
                }
                else if (ResourceNameFld.Text[0] == '[' && ResourceNameFld.Text[ResourceNameFld.Text.Length - 1] != ']')
                {
                    errorText = "Keyword '" + ResourceNameFld.Text + "' has no closing bracket, please try again!";
                    isOk = false;
                }

                if (ResourceNameFld.Text != ContextSlt.GetContextSlt().GetConfigProperty(_EmptyResourceName))
                {
                    if (isOk && this._resource.Parent != null)  // We validate the uniqueness of the name only if we have anything to check against...
                    {
                        // Check if this is a collection and if so, whether it has a unique name...
                        if (this._resource.Archetype == RESTResourceCapability.ResourceArchetype.Collection ||
                            this._resource.Archetype == RESTResourceCapability.ResourceArchetype.Store)
                        {
                            isOk = this._resource.Parent.RootService.ModelPkg.IsUniqueName(ResourceNameFld.Text);
                            if (!isOk) errorText = "Resource Collection name '" + ResourceNameFld.Text + "' is not unique, try again!";
                        }
                        else
                        {
                            // For all other resources, we check whether it is unique within the context of the owner...
                            isOk = this._resource.Parent.OwningPackage.IsUniqueName(ResourceNameFld.Text);
                            if (!isOk) errorText = "Resource name '" + ResourceNameFld.Text + "' is not unique, try again!";
                        }
                    }
                }
                if (isOk)
                {
                    this._resource.Name = ResourceNameFld.Text;
                    this._hasName = true;
                }
                else MessageBox.Show(errorText, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CheckOk();
            }
        }

        /// <summary>
        /// Simple helper that checks whether we have fulfilled all preconditions for enabling the Ok button.
        /// </summary>
        private void CheckOk()
        {
            Ok.Enabled = (this._hasName && this._hasType);
        }

        /// <summary>
        /// This event is raised when the user clicks the 'Create Document' button in order to create a new resource
        /// of type 'Document'. The user must select an existing Business Document class from the model to be used as
        /// the basis for the Document resource. The actual task of showing the dialog and validation the result is 
        /// delegated to the ResourceDeclaration object.
        /// We don't perform name validation here since the resource name is identical to the selected Business Component.
        /// Since there are valid by definition, there is no need to check.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void CreateDocument_Click(object sender, EventArgs e)
        {
            ResourceNameFld.Text = this._resource.SetDocumentClass();
            if (ResourceNameFld.Text != string.Empty)
            {
                this._resource.Name = ResourceNameFld.Text;
                this._hasName = this._hasType = true;
                CheckOk();
            }
        }

        /// <summary>
        /// This event is raised when the user clicks the 'Define Identifier' button. This will bring up the Parameter dialog, which
        /// facilitates definition of a parameter to be used as Identifier for the Resourcer.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void DefineIdentifier_Click(object sender, EventArgs e)
        {
            RESTParameterDeclaration parameter = this._resource.SetParameter();
            if (parameter != null && parameter.Status != RESTParameterDeclaration.DeclarationStatus.Invalid)
            {
                ParameterName.Text = parameter.Name;
                ParameterClassifier.Text = parameter.Classifier.Name;
                ValidateName();
            }
        }

        /// <summary>
        /// This event is raised when the user has made changes to the documentation description field.
        /// The new text is copied to the resource declaration object.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void DocDescription_Leave(object sender, EventArgs e)
        {
            this._resource.Description = DocDescription.Text;
        }

        /// <summary>
        /// This event is raised when the user has made changes to the external documentation description field.
        /// The new text is copied to the resource declaration object.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void ExternalDocDescription_Leave(object sender, EventArgs e)
        {
            this._resource.ExternalDocDescription = ExternalDocDescription.Text;
        }

        /// <summary>
        /// This event is raised when the user has made changes to the external documentation URL field.
        /// The new text is copied to the resource declaration object.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void ExternalDocURL_Leave(object sender, EventArgs e)
        {
            this._resource.ExternalDocURL = ExternalDocURL.Text;
        }

        /// <summary>
        /// This event is raised when the user changes the state of the 'isTag' checkbox.
        /// The new state is copied to the resource declaration object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsTag_CheckedChanged(object sender, EventArgs e)
        {
            this._resource.IsTag = IsTag.Checked;
        }

        /// <summary>
        /// This event is raised when the user clicks the 'Link Document' button in order to associate the parent resource
        /// with an existing Document resource. Clicking the button will present a list of existing Document resource for
        /// the user to choose from. The actual task of showing the dialog and validation the result is delegated to the
        /// ResourceDeclaration object.
        /// We don't perform name validation here since we link to an existing capability that already has been validated
        /// before.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void LinkDocument_Click(object sender, EventArgs e)
        {
            ResourceNameFld.Text = this._resource.LinkDocumentClass();
            if (ResourceNameFld.Text != string.Empty)
            {
                this._resource.Name = ResourceNameFld.Text;
                this._hasName = this._hasType = true;
                CheckOk();
            }
        }
    }
}
