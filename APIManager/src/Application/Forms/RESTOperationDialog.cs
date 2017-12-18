using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Util;
using Framework.Logging;
using Framework.Context;
using Plugin.Application.CapabilityModel;
using Plugin.Application.CapabilityModel.API;

namespace Plugin.Application.Forms
{
    internal partial class RESTOperationDialog : Form
    {
        // Configuration properties used by this module:
        private const string _DefaultSuccessCode    = "DefaultSuccessCode";
        private const string _DefaultResponseCode   = "DefaultResponseCode";

        private RESTOperationDeclaration _operation;    // The operation we're constructing or editing.
        private bool _hasName;                          // Set to true if we have a valid name.
        private bool _hasType;                          // Set to true if we have a valid type.
        private bool _createMode;                       // 'True' in case of editing operation, false on create.
        private bool _dirty;                            // 'True' when stuff has changed.

        /// <summary>
        /// Returns 'true' when Operation Minor Version must be updated.
        /// </summary>
        internal bool MinorVersionIndicator         { get { return NewMinorVersion.Checked; }}
        
        /// <summary>
        /// Returns the Operation Declaration created/edited by the dialog.
        /// </summary>
        internal RESTOperationDeclaration Operation
        {
            get
            {
                if (this._dirty)
                {
                    // We only update the status when stuff has actually changed...
                    this._operation.Status = this._createMode ? RESTOperationDeclaration.DeclarationStatus.Created :
                                                                RESTOperationDeclaration.DeclarationStatus.Edited;
                }
                return this._operation;
            }
        }

        /// <summary>
        /// Creates a new dialog that facilitates creation of a series of resources. Each resource is represented in the dialog
        /// as a tuple of resource name and archetype.
        /// </summary>
        /// <param name="parent">The capability that will act as the parent for the new resource(s).</param>
        internal RESTOperationDialog(RESTOperationDeclaration operation)
        {
            InitializeComponent();
            NewMinorVersion.Checked = false;
            this._operation = operation;
            this._createMode = (operation.Name == string.Empty);
            this.Text = this._createMode ? "Create new Operation": "Edit existing Operation";
            this.OperationNameFld.Text = operation.Name;
            this._dirty = false;

            SummaryText.Text = operation.Summary;
            Description.Text = operation.Description;

            // Show the associated default request- and response documents (if present)...
            if (operation.RequestDocument != null) RequestTypeName.Text = operation.RequestDocument.Name;
            if (operation.ResponseDocument != null) ResponseTypeName.Text = operation.ResponseDocument.Name;

            // Set indicators according to current settings...
            HasPagination.Checked = this._operation.PaginationIndicator;
            RequestMultiple.Checked = this._operation.RequestBodyCardinalityIndicator;
            ResponseMultiple.Checked = this._operation.ResponseBodyCardinalityIndicator;
            OverrideSecurity.Checked = this._operation.PublicAccessIndicator;
            UseHeaderParameters.Checked = this._operation.UseHeaderParametersIndicator;

            // Initialize the drop-down box with a human-friendly label of our OperationType enumeration...
            foreach (RESTOperationCapability.OperationType type in Enum.GetValues(typeof(RESTOperationCapability.OperationType)))
            {
                OperationTypeFld.Items.Add(RESTOperationCapability.OperationTypeToLabel(type));
            }

            if (!string.IsNullOrEmpty(operation.Name))
            {
                OperationTypeFld.SelectedIndex = (int)operation.Archetype;
            }
            else
            {
                OperationTypeFld.SelectedIndex = 0;
                this._operation.Archetype = RESTOperationCapability.LabelToOperationType(OperationTypeFld.Items[0].ToString());
            }

            // Load the list of Filter Parameters...
            foreach (RESTParameterDeclaration queryParam in operation.Parameters)
            {
                if (queryParam.Status != RESTParameterDeclaration.DeclarationStatus.Invalid)
                {
                    ListViewItem newItem = new ListViewItem(queryParam.Name);
                    newItem.SubItems.Add(queryParam.Classifier.Name);
                    FilterParameterList.Items.Add(newItem);
                }
            }

            // Load the result codes from the received operation declaration...
            foreach (RESTOperationResultDeclaration resultDecl in operation.OperationResults)
            {
                if (resultDecl.Status != RESTOperationResultDeclaration.DeclarationStatus.Invalid)
                {
                    ListViewItem newItem = new ListViewItem(resultDecl.ResultCode);
                    newItem.SubItems.Add(resultDecl.Description);
                    ResponseCodeList.Items.Add(newItem);
                }
            }

            // Check whether we may enable the OK button...
            this._hasName = !string.IsNullOrEmpty(operation.Name);
            this._hasType = operation.Archetype != RESTOperationCapability.OperationType.Unknown;
            CheckOk();
        }

        /// <summary>
        /// Suppresses the New Minor Version checkmark for those cases that this option is not required.
        /// </summary>
        internal void DisableMinorVersion()
        {
            NewMinorVersion.Visible = false;
        }

        /// <summary>
        /// This event is raised whenever the user changes the Operation Type drop-down. Depending on the selected type,
        /// some or all of the other properties are selected, deselected, enabled or disabled.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void OperationTypeFld_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = OperationTypeFld.SelectedIndex;
            RESTOperationCapability.OperationType oldType = this._operation.Archetype;
            this._operation.Archetype = RESTOperationCapability.LabelToOperationType(OperationTypeFld.Items[index].ToString());

            // Depending on the selected operation type, some parameters / settings should be enabled and/or disabled...
            switch (this._operation.Archetype)
            {
                case RESTOperationCapability.OperationType.Delete:
                case RESTOperationCapability.OperationType.Head:
                    FilterGroup.Enabled = false;
                    RequestParamBox.Enabled = true;
                    ResponseParamBox.Enabled = false;
                    HasPagination.Enabled = false;
                    HasPagination.Checked = false;
                    break;

                case RESTOperationCapability.OperationType.Get:
                    FilterGroup.Enabled = true;
                    RequestParamBox.Enabled = true;
                    ResponseParamBox.Enabled = true;
                    HasPagination.Enabled = true;
                    break;

                case RESTOperationCapability.OperationType.Patch:
                case RESTOperationCapability.OperationType.Post:
                case RESTOperationCapability.OperationType.Put:
                    FilterGroup.Enabled = false;
                    RequestParamBox.Enabled = true;
                    ResponseParamBox.Enabled = true;
                    HasPagination.Enabled = false;
                    HasPagination.Checked = false;
                    break;

                default:
                    FilterGroup.Enabled = false;
                    RequestParamBox.Enabled = false;
                    ResponseParamBox.Enabled = false;
                    HasPagination.Enabled = false;
                    HasPagination.Checked = false;
                    break;
            }
            this._hasType = this._operation.Archetype != RESTOperationCapability.OperationType.Unknown;
            if (this._hasType) this._dirty = true;
            CheckOk();
        }

        /// <summary>
        /// This event is raised whenever the user has entered an operation name and leaves the input field. The name must
        /// now be validated.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void OperationNameFld_Leave(object sender, EventArgs e)
        {
            this._hasName = false;
            if (OperationNameFld.Text != string.Empty)
            {
                OperationNameFld.Text = Conversions.ToPascalCase(OperationNameFld.Text);
                if (this._createMode && !this._operation.IsValidName(OperationNameFld.Text))
                {
                    MessageBox.Show("Operation '" + OperationNameFld.Text + "' is not unique, try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    this._operation.Name = OperationNameFld.Text;
                    this._hasName = true;
                    this._dirty = true;
                    CheckOk();
                }
            }
        }

        /// <summary>
        /// This event is raised when the user clicks the 'Add Filter Parameter' button.
        /// The method facilitates creation of a new URL query parameter.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void AddFilter_Click(object sender, EventArgs e)
        {
            RESTParameterDeclaration parameter = this._operation.AddParameter();
            if (parameter != null && parameter.Status != RESTParameterDeclaration.DeclarationStatus.Invalid)
            {
                ListViewItem newItem = new ListViewItem(parameter.Name);
                newItem.SubItems.Add(parameter.Classifier.Name);
                FilterParameterList.Items.Add(newItem);
                this._dirty = true;
            }
        }

        /// <summary>
        /// This event is raised when the user clicks the 'Add Response Code' button.
        /// The method facilitates creation of an additional response code (link between a HTTP response code and an
        /// associated data type). The dialog creates two response codes by default: 
        /// - Default OK response (HTTP code 200);
        /// - Default Error response (OpenAPI 'default' response);
        /// Any additional response codes can be defined using this dialog.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void AddResponseCode_Click(object sender, EventArgs e)
        {
            RESTOperationResultDeclaration result = this._operation.AddOperationResult();
            if (result != null && result.Status != RESTOperationResultDeclaration.DeclarationStatus.Invalid)
            {
                ListViewItem newItem = new ListViewItem(result.ResultCode);
                newItem.SubItems.Add(result.Description);
                ResponseCodeList.Items.Add(newItem);
                this._dirty = true;
            }
        }

        /// <summary>
        /// This event is raised when the user selects a previously defined URL query parameter for deletion.
        /// It marks the selected parameter as 'deleted'.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void DeleteFilter_Click(object sender, EventArgs e)
        {
            if (FilterParameterList.SelectedItems.Count > 0)
            {
                ListViewItem key = FilterParameterList.SelectedItems[0];
                this._operation.DeleteParameter(key.Text);
                FilterParameterList.Items.Remove(key);
                this._dirty = true;
            }
        }

        /// <summary>
        /// This event is raised when the user selects a response code for deletion. This is only allowed for response
        /// codes that have been added by the user, i.e. the standard responses (HTTP 200 and 'default') can NOT be deleted.
        /// The response code record is marked as 'deleted'.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void DeleteResponseCode_Click(object sender, EventArgs e)
        {
            if (ResponseCodeList.SelectedItems.Count > 0)
            {
                ListViewItem key = ResponseCodeList.SelectedItems[0];
                ContextSlt context = ContextSlt.GetContextSlt();
                if (key.Text != context.GetConfigProperty(_DefaultResponseCode) && key.Text != context.GetConfigProperty(_DefaultSuccessCode))
                {
                    this._operation.DeleteOperationResult(key.Text);
                    ResponseCodeList.Items.Remove(key);
                    this._dirty = true;
                }
                else MessageBox.Show("You are not allowed to remove this response code!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// This event is raised when the user selects a predefined URL query parameter for edit. It facilitates changes of 
        /// name, type or other properties of the parameter.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void EditFilter_Click(object sender, EventArgs e)
        {
            if (FilterParameterList.SelectedItems.Count > 0)
            {
                ListViewItem key = FilterParameterList.SelectedItems[0];
                ContextSlt context = ContextSlt.GetContextSlt();
                string originalKey = key.Text;
                RESTParameterDeclaration param = this._operation.EditParameter(key.Text);
                if (param != null)
                {
                    key.SubItems[0].Text = param.Name;
                    key.SubItems[1].Text = param.Classifier.Name;
                    this._dirty = true;
                }
            }
        }

        /// <summary>
        /// This event is raised when the user selects a response code for edit. All codes can be edited, however, the user
        /// can NOT change the name of the default response (HTTP 200) and the default response ('default'). For these, only
        /// the associated response data type can be changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void EditResponseCode_Click(object sender, EventArgs e)
        {
            if (ResponseCodeList.SelectedItems.Count > 0)
            {
                ListViewItem key = ResponseCodeList.SelectedItems[0];
                ContextSlt context = ContextSlt.GetContextSlt();
                string originalKey = key.Text;
                if (key.Text != context.GetConfigProperty(_DefaultResponseCode) && key.Text != context.GetConfigProperty(_DefaultSuccessCode))
                {
                    RESTOperationResultDeclaration result = this._operation.EditOperationResult(key.Text);
                    if (result != null)
                    {
                        key.SubItems[0].Text = result.ResultCode;
                        key.SubItems[1].Text = result.Description;
                        this._dirty = true;
                    }
                }
                else MessageBox.Show("You are not allowed to edit this response code!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// This event is raised when one of the operation indicators has changed state.
        /// The method collects and updates the associated indicators in the operation declaration.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void Indicator_CheckedChanged(object sender, EventArgs e)
        {
            this._operation.PaginationIndicator = HasPagination.Checked;
            this._operation.PublicAccessIndicator = OverrideSecurity.Checked;
            this._operation.RequestBodyCardinalityIndicator = RequestMultiple.Checked;
            this._operation.ResponseBodyCardinalityIndicator = ResponseMultiple.Checked;
            this._operation.UseHeaderParametersIndicator = UseHeaderParameters.Checked;
            this._dirty = true;

            Logger.WriteInfo("Plugin.Application.Forms.RESTOperationDialog.IndicatorCheckedChanged >> Collected indicators: " + 
                             "MultipleRequestIndicator =" + this._operation.RequestBodyCardinalityIndicator + Environment.NewLine +
                             "MultipleResponseIndicator =" + this._operation.ResponseBodyCardinalityIndicator + Environment.NewLine +
                             "PaginationIndicator = " + this._operation.PaginationIndicator + Environment.NewLine +
                             "UseHeaderParametersIndicator = " + this._operation.UseHeaderParametersIndicator + Environment.NewLine +
                             "PublicAccessIndicator = " + this._operation.PublicAccessIndicator);
        }

        /// <summary>
        /// Simple helper that checks whether we have fulfilled all preconditions for enabling the Ok button.
        /// </summary>
        private void CheckOk()
        {
            Ok.Enabled = this._hasName && this._hasType;
        }

        /// <summary>
        /// This event is raised when the user made changes to the 'Consumes' MIME List.
        /// We split the entry in separate MIME type, separated by ',' character, and add each one to the 
        /// 'Consumed' list. This will REPLACE the current list in the Operation Declaration.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void ConsumesMIME_TextChanged(object sender, EventArgs e)
        {
            this._operation.ClearConsumedMIMETypes();
            string[] MIMEList = ConsumesMIME.Text.Split(',');
            foreach (string MIMEEntry in MIMEList) this._operation.AddConsumedMIMEType(MIMEEntry.Trim());
            this._dirty = true;
        }

        /// <summary>
        /// This event is raised when the user made changes to the 'Produces' MIME List.
        /// We split the entry in separate MIME type, separated by ',' character, and add each one to the 
        /// 'Produced' list. This will REPLACE the current list in the Operation Declaration.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void ProducesMIME_TextChanged(object sender, EventArgs e)
        {
            this._operation.ClearProducedMIMETypes();
            string[] MIMEList = ProducesMIME.Text.Split(',');
            foreach (string MIMEEntry in MIMEList) this._operation.AddProducedMIMEType(MIMEEntry.Trim());
            this._dirty = true;
        }

        /// <summary>
        /// This event is raised when the user made changes to the Summary Text field.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void SummaryText_Leave(object sender, EventArgs e)
        {
            this._operation.Summary = SummaryText.Text;
            this._dirty = true;
        }

        /// <summary>
        /// This event is raised when the user made changes to the Description Text field.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void Description_Leave(object sender, EventArgs e)
        {
            this._operation.Description = Description.Text;
            this._dirty = true;
        }

        /// <summary>
        /// This event is raised when the user clicks the 'Unlink Request' button.
        /// Used to clear the current request Document Resource (if present).
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void RemoveRequest_Click(object sender, EventArgs e)
        {
            this._operation.ClearDocument(RESTOperationDeclaration._RequestIndicator);
            RequestTypeName.Clear();
            this._dirty = true;
        }

        /// <summary>
        /// This event is raised when the user clicks the 'Unlink Response' button.
        /// Used to clear the current response Document Resource (if present).
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void RemoveResponse_Click(object sender, EventArgs e)
        {
            this._operation.ClearDocument(RESTOperationDeclaration._ResponseIndicator);
            ResponseTypeName.Clear();
            this._dirty = true;
        }

        /// <summary>
        /// This event is raised when the user clicks the 'Link Request' button.
        /// Actual operation is delegated to the OperationDeclaration.SetDocument method.
        /// Result is shown in the associated type-name field.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void SelectRequest_Click(object sender, EventArgs e)
        {
            RequestTypeName.Text = this._operation.SetDocument(RESTOperationDeclaration._RequestIndicator);
            if (RequestTypeName.Text != string.Empty) this._dirty = true;
        }

        /// <summary>
        /// This event is raised when the user clicks the 'Link Response' button.
        /// Actual operation is delegated to the OperationDeclaration.SetDocument method.
        /// Result is shown in the associated type-name field.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void SelectResponse_Click(object sender, EventArgs e)
        {
            ResponseTypeName.Text = this._operation.SetDocument(RESTOperationDeclaration._ResponseIndicator);
            if (ResponseTypeName.Text != string.Empty) this._dirty = true;
        }
    }
}
