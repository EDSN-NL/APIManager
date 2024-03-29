﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Framework.Util;
using Framework.Exceptions;
using Framework.ConfigurationManagement;
using Plugin.Application.CapabilityModel.API;

namespace Plugin.Application.Forms
{
    internal partial class RESTOperationDialog : Form
    {
        private RESTOperationDeclaration _operation;    // The operation we're constructing or editing.
        private bool _hasName;                          // Set to true if we have a valid name.
        private bool _hasType;                          // Set to true if we have a valid type.
        private bool _createMode;                       // 'True' in case of editing operation, false on create.
        private bool _dirty;                            // 'True' when stuff has changed.
        private bool _initializing;                     // Set to 'true' during construction to suppress unwanted events.
        private RESTResourceDeclaration _parent;        // Parent resource of this operation.
        private RESTResponseCodeCollectionMgr _responseManager;     // Response code collection manager.

        /// <summary>
        /// Returns 'true' when Operation Minor Version must be updated.
        /// </summary>
        internal bool MinorVersionIndicator             { get { return NewMinorVersion.Checked; } }
        
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
        /// Creates a new dialog that facilitates either creation or editing of an operation, including all of its components.
        /// </summary>
        /// <param name="myService">The service that owns the operation.</param>
        /// <param name="operation">Either an empty descriptor (in case of new operation), or operation properties in case
        /// we're editing an existing operation.</param>
        /// <param name="parent">The capability that will act as the parent for the new resource(s).</param>
        internal RESTOperationDialog(RESTService myService, RESTOperationDeclaration operation, RESTResourceDeclaration parent)
        {
            InitializeComponent();
            this._initializing = true;
            NewMinorVersion.Checked = false;
            this._operation = operation;
            this._createMode = operation.Name == string.Empty;
            this.Text = this._createMode ? "Create new Operation": "Edit existing Operation";
            this.OperationNameFld.Text = operation.Name;
            this._dirty = false;
            this._parent = parent;
            var unknownOperation = new HTTPOperation();
            this._responseManager = new RESTResponseCodeCollectionMgr(myService);

            SummaryText.Text = operation.Summary;
            Description.Text = operation.Description;

            // Show the associated default request document (if present)...
            if (operation.RequestDocument == null) this._operation.RequestCardinality = new Cardinality(Cardinality._Mandatory);
            else RequestTypeName.Text = operation.RequestDocument.Name;
            ReqCardLo.Text = this._operation.RequestCardinality.LowerBoundaryAsString;
            ReqCardHi.Text = this._operation.RequestCardinality.UpperBoundaryAsString;

            // Set remaining indicators according to current settings...
            HasPagination.Checked       = operation.PaginationIndicator;

            // Initialize the MIME-type fields...
            string MIMETypes = string.Empty;
            bool firstOne = true;
            if (operation.ProducedMIMETypes.Count > 0)
            {
                foreach (string producedMIMEType in operation.ProducedMIMETypes)
                {
                    MIMETypes += firstOne ? producedMIMEType : ", " + producedMIMEType;
                    firstOne = false;
                }
                ProducesMIME.Text = MIMETypes;
            }
            if (operation.ConsumedMIMETypes.Count > 0)
            {
                MIMETypes = string.Empty;
                firstOne = true;
                foreach (string consumedMIMEType in operation.ConsumedMIMETypes)
                {
                    MIMETypes += firstOne ? consumedMIMEType : ", " + consumedMIMEType;
                    firstOne = false;
                }
                ConsumesMIME.Text = MIMETypes;
            }

            // Since the 'AvailableOperationsList' does not contain our own operation, we will add this separately.
            // But only if it represents a valid operation.
            if (operation.OperationType != unknownOperation) OperationTypeFld.Items.Add(operation.OperationType);

            // Initialize the drop-down box with our list of available operations.
            // Note that changing the selected index WILL trigger an event.
            foreach (HTTPOperation operationType in parent.AvailableOperationsList)
            {
                OperationTypeFld.Items.Add(operationType);
            }

            if (this._operation.OperationType == unknownOperation && parent.AvailableOperationsList.Count > 0)
                this._operation.OperationType = parent.AvailableOperationsList[0];

            OperationTypeFld.SelectedIndex = 0;

            // Load the list of Filter Parameters...
            foreach (RESTParameterDeclaration queryParam in operation.QueryParameters)
            {
                if (queryParam.Status != RESTParameterDeclaration.DeclarationStatus.Invalid)
                {
                    ListViewItem newItem = new ListViewItem(queryParam.Name);
                    newItem.SubItems.Add(queryParam.Classifier.Name);
                    FilterParameterList.Items.Add(newItem);
                }
            }

            // Load the result codes from our collection...
            foreach (RESTOperationResultDescriptor resultDesc in operation.ResponseCollection.Collection)
            {
                if (resultDesc.IsValid)
                {
                    ListViewItem newItem = new ListViewItem(resultDesc.ResultCode);
                    newItem.Name = resultDesc.ResultCode;
                    newItem.SubItems.Add(resultDesc.Description);
                    ResponseCodeList.Items.Add(newItem);
                }
            }

            // Load the request header parameters from our collection...
            foreach (RESTHeaderParameterDescriptor paramDesc in operation.RequestHeaderParameters)
            {
                if (paramDesc.IsValid)
                {
                    ListViewItem newItem = new ListViewItem(paramDesc.Name);
                    newItem.Name = paramDesc.Name;
                    newItem.SubItems.Add(paramDesc.Description);
                    RequestHeaderList.Items.Add(newItem);
                }
            }

            // If CM is enabled, we have to suppress the 'minor version' checkbox...
            if (CMRepositorySlt.GetRepositorySlt().IsCMEnabled) NewMinorVersion.Visible = false;

            // Check whether we may enable the OK button...
            this._hasName = !string.IsNullOrEmpty(operation.Name);
            this._hasType = operation.OperationType != unknownOperation;
            CheckOk();
            this._initializing = false;
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
            if (!this._initializing)
            {
                // This part is skipped when still in initialization phase!
                int index = OperationTypeFld.SelectedIndex;
                HTTPOperation oldType = this._operation.OperationType;
                HTTPOperation unknownType = new HTTPOperation();
                this._operation.OperationType = OperationTypeFld.Items[index] as HTTPOperation;
                this._hasType = this._operation.OperationType != unknownType;
                if (this._hasType) this._dirty = true;
                CheckOk();
            }

            // Depending on the selected operation type, some parameters / settings should be enabled and/or disabled...
            switch (this._operation.OperationType.TypeEnum)
            {
                case HTTPOperation.Type.Delete:
                case HTTPOperation.Type.Head:
                    FilterGroup.Enabled = true;
                    RequestParamBox.Enabled = true;
                    HasPagination.Enabled = false;
                    HasPagination.Checked = false;
                    break;

                case HTTPOperation.Type.Get:
                    FilterGroup.Enabled = true;
                    RequestParamBox.Enabled = false;
                    HasPagination.Enabled = true;
                    break;

                case HTTPOperation.Type.Patch:
                case HTTPOperation.Type.Post:
                case HTTPOperation.Type.Put:
                    FilterGroup.Enabled = true;
                    RequestParamBox.Enabled = true;
                    HasPagination.Enabled = true;
                    HasPagination.Checked = false;
                    break;

                default:
                    FilterGroup.Enabled = false;
                    RequestParamBox.Enabled = false;
                    HasPagination.Enabled = false;
                    HasPagination.Checked = false;
                    break;
            }
        }

        /// <summary>
        /// This event is raised whenever the user has entered an operation name and leaves the input field. The name must
        /// now be validated.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void OperationNameFld_Leave(object sender, EventArgs e)
        {
            if (this._initializing) return;   // Ignore event during initialization.
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
        /// This event is raised when the user clicks the 'Add Request Header' button. The method invokes the API-level 'manage header parameters'
        /// dialog, which facilitates creation/modification/removal of request header parameters for the API as well as making selections from
        /// the global list to the operation-specific list. On return, we simply replace the contents of our header parameter list with the
        /// contents of the updated list.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void AddReqHeader_Click(object sender, EventArgs e)
        {
            List<RESTHeaderParameterDescriptor> parameters = this._operation.AddHeaderParameters();
            RequestHeaderList.Items.Clear();
            foreach (RESTHeaderParameterDescriptor paramDesc in parameters)
            {
                if (paramDesc.IsValid)
                {
                    ListViewItem newItem = new ListViewItem(paramDesc.Name);
                    newItem.Name = paramDesc.Name;
                    newItem.SubItems.Add(paramDesc.Description);
                    RequestHeaderList.Items.Add(newItem);
                }
            }
            this._dirty = true;
        }

        /// <summary>
        /// This event is raised when the user clicks the 'Add Response Code' button.
        /// The method facilitates creation of an additional response code (an HTTP response code with associated metadata such as
        /// payload, description, cardinality, etc.).
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void AddResponseCode_Click(object sender, EventArgs e)
        {
            RESTOperationResultDescriptor result = this._operation.AddOperationResult();
            if (result != null && result.IsValid)
            {
                ListViewItem newItem = new ListViewItem(result.ResultCode);
                newItem.Name = result.ResultCode;
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
        /// This event is raised when the user selects a previously defined request header parameter for deletion. It removes the selected
        /// header parameter.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void DeleteReqHeader_Click(object sender, EventArgs e)
        {
            if (RequestHeaderList.SelectedItems.Count > 0)
            {
                ListViewItem key = RequestHeaderList.SelectedItems[0];
                this._operation.DeleteHeaderParameter(key.Text);
                RequestHeaderList.Items.Remove(key);
                this._dirty = true;
            }
        }

        /// <summary>
        /// This event is raised when the user selects a response code for deletion.
        /// The response code record is marked as 'deleted'.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void DeleteResponseCode_Click(object sender, EventArgs e)
        {
            if (ResponseCodeList.SelectedItems.Count > 0)
            {
                ListViewItem key = ResponseCodeList.SelectedItems[0];
                this._operation.DeleteOperationResult(key.Text);
                ResponseCodeList.Items.Remove(key);
                this._dirty = true;
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
        /// This event is raised when the user selects a header parameter for edit.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void EditReqHeader_Click(object sender, EventArgs e)
        {
            if (RequestHeaderList.SelectedItems.Count > 0)
            {
                ListViewItem key = RequestHeaderList.SelectedItems[0];
                string originalKey = key.Text;
                RESTHeaderParameterDescriptor parameter = this._operation.EditHeaderParameter(key.Text);
                if (parameter != null)
                {
                    key.SubItems[0].Text = parameter.Name;
                    key.SubItems[1].Text = parameter.Description;
                    this._dirty = true;
                }
            }
        }

        /// <summary>
        /// This event is raised when the user selects a response code for edit.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void EditResponseCode_Click(object sender, EventArgs e)
        {
            if (ResponseCodeList.SelectedItems.Count > 0)
            {
                ListViewItem key = ResponseCodeList.SelectedItems[0];
                string originalKey = key.Text;
                RESTOperationResultDescriptor result = this._operation.EditOperationResult(key.Text);
                if (result != null)
                {
                    key.SubItems[0].Text = result.ResultCode;
                    key.SubItems[1].Text = result.Description;
                    this._dirty = true;
                }
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
            if (this._initializing) return;   // Ignore event during initialization.
            this._operation.PaginationIndicator = HasPagination.Checked;
            this._dirty = true;
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
            if (this._initializing) return;   // Ignore event during initialization.
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
            if (this._initializing) return;   // Ignore event during initialization.
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
            if (this._initializing) return;   // Ignore event during initialization.
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
            if (this._initializing) return;   // Ignore event during initialization.
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
            this._operation.ClearDocument();
            RequestTypeName.Clear();
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
            RequestTypeName.Text = this._operation.SetDocument();
            if (RequestTypeName.Text != string.Empty) this._dirty = true;
        }

        /// <summary>
        /// This event is raised when the user clicks the 'use collection' button. We return a (selected) collection
        /// from the collection manager and copy all result codes that do not yet exist in our current list.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void UseCollection_Click(object sender, EventArgs e)
        {
            foreach (RESTOperationResultDescriptor result in this._responseManager.GetCollectionContents())
            {
                if (ResponseCodeList.FindItemWithText(result.ResultCode) == null && this._operation.AddOperationResult(result))
                {
                    ListViewItem newItem = new ListViewItem(result.ResultCode);
                    newItem.Name = result.ResultCode;
                    newItem.SubItems.Add(result.Description);
                    ResponseCodeList.Items.Add(newItem);
                    this._dirty = true;
                }
            }
        }

        /// <summary>
        /// This event is raised when the user clicks the 'edit collections' button. This brings up a subsequent dialog,
        /// which facilitates create- delete- or edit of response code collections.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void EditCollections_Click(object sender, EventArgs e)
        {
            this._responseManager.ManageCollection();
        }

        /// <summary>
        /// This event is raised when the user leaves either the request cardinality upper- or lower boundary field.
        /// We check the contents of the fields and create a new cardinality only if both fields have a value. In case
        /// of illegal values, we raise a pop-up error.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void ReqCardinality_Leave(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(ReqCardLo.Text) && !string.IsNullOrEmpty(ReqCardHi.Text))
                {
                    this._operation.RequestCardinality = new Cardinality(ReqCardLo.Text + ".." + ReqCardHi.Text);
                }
            }
            catch (IllegalCardinalityException)
            {
                MessageBox.Show("Provided cardinality value is not correct!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// On exit, we check the consistency of the response code list:
        /// - At least one 'Success' result;
        /// - At least one 'Client Error' result;
        /// - All response codes in 'valid' state.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void Ok_Click(object sender, EventArgs e)
        {
            string errorCodes = string.Empty;
            string message = "Response code collection not yet correct because:" + Environment.NewLine;
            int defaultLen = message.Length;
            bool foundOk = false;
            bool foundError = false;
            bool firstOne = true;
            foreach (RESTOperationResultDescriptor response in this._operation.ResponseCollection.Collection)
            {
                if (response.Category == RESTOperationResultDescriptor.ResponseCategory.Success)
                {
                    foundOk = true;
                    if (response.ResultCode == "200" && 
                        response.Document == null && 
                        response.PayloadClass == null && 
                        MessageBox.Show("Success code 200 typically implies a payload, do you want to assign one?", 
                                        "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                        this.DialogResult = DialogResult.None;
                }
                else if (response.Category == RESTOperationResultDescriptor.ResponseCategory.ClientError) foundError = true;
                if (!response.IsValid)
                {
                    errorCodes += firstOne ? response.ResultCode : ", " + response.ResultCode;
                    firstOne = false;
                }
            }
            if (!foundOk) message += "No success code has been defined;" + Environment.NewLine;
            if (!foundError) message += "No client error code has been defined;" + Environment.NewLine;
            if (errorCodes != string.Empty) message += "The following response codes don't (yet) have a valid context: " + errorCodes;
            
            if (message.Length > defaultLen)
            {
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.None;
            }
        }
    }
}
