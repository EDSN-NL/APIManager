using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Logging;
using Framework.Util;
using Framework.Context;
using Framework.ConfigurationManagement;
using Plugin.Application.CapabilityModel.API;

namespace Plugin.Application.Forms
{
    public partial class RESTResourceDialog : Form
    {
        // Configuration properties used by this module...
        private const string _EmptyResourceName         = "EmptyResourceName";

        private RESTResourceDeclaration _resource;      // The resource declaration descriptor that we're creating / editing.
        private bool _isEdit;                           // True means we're editing a path expression, false when creating a new one.
        private bool _initializing;                     // Set to 'true' during construction to suppress unwanted events.

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
            this._initializing = true;

            this._isEdit = resource.Status != RESTResourceDeclaration.DeclarationStatus.Invalid;
            this.Text = this._isEdit ? "Edit Resource" : "Create new Resource";
            this._resource = resource;

            ResourceNameFld.Text = this._isEdit ? resource.Name : string.Empty;
            if (resource.Parameter != null)
            {
                ParameterName.Text = resource.Parameter.Name;
                ParameterClassifier.Text = resource.Parameter.Classifier != null ? resource.Parameter.Classifier.Name : string.Empty;
            }

            string tagList = string.Empty;
            bool firstOne = true;
            foreach (string tagName in resource.TagNames)
            {
                tagList += firstOne ? tagName : ", " + tagName;
                firstOne = false;
            }
            TagNames.Text = tagList;

            // Load the Operations in case of Edit...
            if (this._isEdit && resource.Operations != null)
            {
                foreach (RESTOperationDeclaration operation in resource.Operations)
                {
                    if (operation.Status != RESTOperationDeclaration.DeclarationStatus.Invalid)
                    {
                        ListViewItem newItem = new ListViewItem(operation.Name);
                        newItem.SubItems.Add(operation.OperationType.ToString());
                        OperationsList.Items.Add(newItem);
                    }
                }
            }

            // Initialize the documentation boxes...
            DocDescription.Text = resource.Description;
            ExternalDocDescription.Text = resource.ExternalDocDescription;
            ExternalDocURL.Text = resource.ExternalDocURL;

            // Initialize the drop-down box with the possible values of our Resource archetype enumeration...
            // Setting the ResourceTypeBox.SelectedIndex enforces a 'SelectedIndexChanged' event, which will perform all dialog
            // housekeeping that is resource-type specific.
            // If we're in create-mode, we enforce a default type of 'Collection' in our drop-down and make sure that the resource
            // is still at 'Unknown'.
            // In Edit mode, we don't allow most transitions. If we have an Identifier, Document or Controller, we can't change the
            // archetype at all. If we have a Collection or Store, we can change one into the other.
            // In other words: you can't change a collection into a Document (or vice-versa), but you CAN change a Collection into
            // a Store...
            int typeIndex = 0;
            if (this._isEdit)
            {
                if (resource.Archetype == RESTResourceCapability.ResourceArchetype.Document || 
                    resource.Archetype == RESTResourceCapability.ResourceArchetype.Identifier ||
                    resource.Archetype == RESTResourceCapability.ResourceArchetype.Controller)
                {
                    ResourceTypeBox.Items.Add(EnumConversions<RESTResourceCapability.ResourceArchetype>.EnumToString(resource.Archetype));
                    ResourceTypeBox.Enabled = false;
                    typeIndex = 0;
                }
                else
                {
                    ResourceTypeBox.Items.Add("Collection");
                    ResourceTypeBox.Items.Add("Store");
                    typeIndex = (resource.Archetype == RESTResourceCapability.ResourceArchetype.Collection) ? 0 : 1;
                }
            }
            else
            {
                this._resource.Archetype = RESTResourceCapability.ResourceArchetype.Unknown;
                typeIndex = (int)RESTResourceCapability.ResourceArchetype.Collection;
                ResourceTypeBox.Items.AddRange(EnumConversions<RESTResourceCapability.ResourceArchetype>.GetNamesArray());
            }
            ResourceTypeBox.SelectedIndex = typeIndex;

            // Assign context menus to the appropriate controls...
            OperationsList.ContextMenuStrip = OperationMenuStrip;

            // If CM is enabled, we have to suppress the 'minor version' checkbox...
            if (CMRepositorySlt.GetRepositorySlt().IsCMEnabled) NewMinorVersion.Visible = false;

            Ok.Enabled = this._isEdit;
            NewMinorVersion.Checked = false;
            this._hasType = (this._resource.Archetype != RESTResourceCapability.ResourceArchetype.Unknown);
            this._hasName = (resource.Name != string.Empty);

            // Since the 'SelectedIndexChanged' event is only partly processed in case of 'edit', we enforce the selection of controls
            // explicitly from the constructor to be sure that all control are properly initialized in both 'edit' and 'create' modes.
            if (this._isEdit) SetAvailableControls();

            // Initialise our tool tips...
            AddOperationToolTip.SetToolTip(AddOperation, "Add a new REST operation to this Path Expression.");
            DeleteOperationToolTip.SetToolTip(DeleteOperation, "Delete the REST operation and all associated resources from Path Expression.");
            EditOperationToolTip.SetToolTip(EditOperation, "Open the REST operation 'Edit' dialog for editing of parameters.");
            this._initializing = false;
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
                newItem.SubItems.Add(operation.OperationType.ToString());
                OperationsList.Items.Add(newItem);
                this._resource.Status = this._isEdit ? RESTResourceDeclaration.DeclarationStatus.Edited : RESTResourceDeclaration.DeclarationStatus.Created;
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
                this._resource.Status = this._isEdit ? RESTResourceDeclaration.DeclarationStatus.Edited : RESTResourceDeclaration.DeclarationStatus.Created;
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
                    myItem.SubItems[1].Text = operation.OperationType.ToString();
                    this._resource.Status = this._isEdit ? RESTResourceDeclaration.DeclarationStatus.Edited : RESTResourceDeclaration.DeclarationStatus.Created;
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
            if (this._isEdit && oldType == this._resource.Archetype) return;    // Edit and no change, do nothing!

            // If we change the resource type, we remove all existing operations that do not match the new type.
            // Note that iterating the items list in reverse allows us to remove items from the list while iterating over it...
            for (int i=OperationsList.Items.Count-1; i >=0; i--)
            {
                if (!this._resource.HasOperation(OperationsList.Items[i].Text)) OperationsList.Items.RemoveAt(i);
            }

            SetAvailableControls();

            this._hasType = this._resource.Archetype != RESTResourceCapability.ResourceArchetype.Unknown;
            this._resource.Status = this._isEdit ? RESTResourceDeclaration.DeclarationStatus.Edited : RESTResourceDeclaration.DeclarationStatus.Created;
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
            if (this._initializing) return;     // No actions during initialization.
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
            if (!string.IsNullOrEmpty(ResourceNameFld.Text) && ResourceNameFld.Text != "{}")
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
                            if (!isOk)
                            {
                                if (MessageBox.Show("Resource Collection name '" + ResourceNameFld.Text +
                                                    "' is not unique, are you sure?", "Warning", MessageBoxButtons.YesNo,
                                                    MessageBoxIcon.Warning) == DialogResult.Yes) isOk = true;
                                else return;
                            }
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
                    this._resource.Status = this._isEdit ? RESTResourceDeclaration.DeclarationStatus.Edited : RESTResourceDeclaration.DeclarationStatus.Created;
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
                this._resource.Status = this._isEdit ? RESTResourceDeclaration.DeclarationStatus.Edited : RESTResourceDeclaration.DeclarationStatus.Created;
                this._hasName = this._hasType = true;
                CheckOk();
            }
        }

        /// <summary>
        /// This event is raised when the user clicks the 'Define Identifier' button. This will bring up the Parameter dialog, which
        /// facilitates definition of a parameter to be used as Identifier for the Resource.
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
                ResourceNameFld.Text = this._resource.Name;
                this._hasName = this._hasType = true;
                CheckOk();
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
            if (this._initializing) return;     // No actions during initialization.
            this._resource.Description = DocDescription.Text;
            this._resource.Status = this._isEdit ? RESTResourceDeclaration.DeclarationStatus.Edited : RESTResourceDeclaration.DeclarationStatus.Created;
        }

        /// <summary>
        /// This event is raised when the user has made changes to the external documentation description field.
        /// The new text is copied to the resource declaration object.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void ExternalDocDescription_Leave(object sender, EventArgs e)
        {
            if (this._initializing) return;     // No actions during initialization.
            this._resource.ExternalDocDescription = ExternalDocDescription.Text;
            this._resource.Status = this._isEdit ? RESTResourceDeclaration.DeclarationStatus.Edited : RESTResourceDeclaration.DeclarationStatus.Created;
        }

        /// <summary>
        /// This event is raised when the user has made changes to the external documentation URL field.
        /// The new text is copied to the resource declaration object.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void ExternalDocURL_Leave(object sender, EventArgs e)
        {
            if (this._initializing) return;     // No actions during initialization.
            this._resource.ExternalDocURL = ExternalDocURL.Text;
            this._resource.Status = this._isEdit ? RESTResourceDeclaration.DeclarationStatus.Edited : RESTResourceDeclaration.DeclarationStatus.Created;
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
                this._resource.Status = this._isEdit ? RESTResourceDeclaration.DeclarationStatus.Edited : RESTResourceDeclaration.DeclarationStatus.Created;
                this._hasName = this._hasType = true;
                CheckOk();
            }
        }

        /// <summary>
        /// Helper function that enables or disables controls based on the current archetype.
        /// </summary>
        private void SetAvailableControls()
        {
            // Enable/Disable all controls according to the Resource type...
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
                    if (!this._isEdit)
                    {
                        ResourceNameFld.Clear();
                        this._resource.ClearParameter();
                        this._resource.ClearDocumentClass();
                    }
                    break;

                case RESTResourceCapability.ResourceArchetype.Document:
                    CreateDocument.Enabled = true;
                    LinkDocument.Enabled = true;
                    PropertiesBox.Enabled = false;
                    OperationsBox.Enabled = false;
                    ResourceNameFld.Enabled = false;
                    ResourceNameFld.ReadOnly = true;
                    if (!this._isEdit)
                    {
                        ResourceNameFld.Clear();
                        this._resource.ClearParameter();
                        this._resource.TagNames = new List<string>();
                        TagNames.Text = string.Empty;
                    }
                    break;

                case RESTResourceCapability.ResourceArchetype.Unknown:
                    CreateDocument.Enabled = false;
                    LinkDocument.Enabled = false;
                    PropertiesBox.Enabled = false;
                    OperationsBox.Enabled = false;
                    ResourceNameFld.Enabled = false;
                    ResourceNameFld.ReadOnly = false;
                    if (!this._isEdit)
                    {
                        ResourceNameFld.Clear();
                        this._resource.ClearParameter();
                        this._resource.ClearDocumentClass();
                        this._resource.TagNames = new List<string>();
                        TagNames.Text = string.Empty;
                    }
                    break;

                case RESTResourceCapability.ResourceArchetype.Identifier:
                    CreateDocument.Enabled = false;
                    LinkDocument.Enabled = false;
                    PropertiesBox.Enabled = true;
                    OperationsBox.Enabled = true;
                    ResourceNameFld.Enabled = false;
                    ResourceNameFld.ReadOnly = true;
                    if (!this._isEdit)
                    {
                        ResourceNameFld.Clear();
                        this._resource.ClearDocumentClass();
                    }
                    ResourceNameFld.Text = this._resource.Name;
                    break;
            }
        }

        /// <summary>
        /// This event is raised when the user made changes to the list of tag names. The event removes the contents of the current
        /// tag list in the declaration object and replaces it with the contents of the new list.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void TagNames_Leave(object sender, EventArgs e)
        {
            this._resource.TagNames = new List<string>();
            if (!string.IsNullOrEmpty(TagNames.Text))
            {
                string[] tagArray = TagNames.Text.Split(',');
                foreach (string tagName in tagArray) this._resource.TagNames.Add(tagName.Trim());
                this._resource.Status = this._isEdit ? RESTResourceDeclaration.DeclarationStatus.Edited : RESTResourceDeclaration.DeclarationStatus.Created;
            }
        }
    }
}
