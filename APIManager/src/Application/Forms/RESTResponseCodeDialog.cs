using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Util;
using Framework.Context;
using Framework.Exceptions;
using Plugin.Application.CapabilityModel.API;
using Plugin.Application.CapabilityModel;

namespace Plugin.Application.Forms
{
    internal partial class RESTResponseCodeDialog : Form
    {
        private RESTOperationResultDescriptor _result;              // The Descriptor that we're building.
        private List<CodeDescriptor> _codeList;                     // Currently active set of response code definitions.
        private RESTResponseCodeCollection _collection;             // The collection that owns this descriptor.
        private RESTHeaderParameterCollectionMgr _headerManager;    // Header parameter manager.

        internal RESTOperationResultDescriptor OperationResult { get { return this._result; } }

        /// <summary>
        /// Dialog that facilitates creation of a new Operation Result Descriptor (or editing of an existing one).
        /// When we want to create a new descriptor, pass 'NULL' to 'descriptor'.
        /// </summary>
        /// <param name="myService">For non-template collections, this is the service that 'owns' our response codes.</param>
        /// <param name="collection">The collection that will 'own' the new descriptor.</param>
        /// <param name="descriptor">Initial declaration to use for editing.</param>
        internal RESTResponseCodeDialog(Service myService, RESTResponseCodeCollection collection, RESTOperationResultDescriptor descriptor)
        {
            InitializeComponent();
            ContextSlt context = ContextSlt.GetContextSlt();
            this._result = descriptor;
            this._collection = collection;
            this._headerManager = new RESTHeaderParameterCollectionMgr(myService);

            // 'Document' payload type is available only when we're on the 'operation' level!
            IsDocument.Enabled = collection.Scope == RESTResponseCodeCollection.CollectionScope.Operation;

            // Enable/disable OpenAPI version dependent features...
            if (context.GetStringSetting(FrameworkSettings._OpenAPIVersion) == FrameworkSettings._OpenAPIVersion20)
            {
                IsExternalLink.Enabled = false;
                IsRange.Checked = false;
                IsRange.Visible = false;
                ExternalLinkBox.Enabled = false;
            }

            if (descriptor == null)
            {
                // When we have not received an existing descriptor, we assume that we must create a new one and we start with 'default'.
                this._result = new RESTOperationResultDescriptor(collection, RESTOperationResultDescriptor._DefaultCode);
                this._result.PayloadType = RESTOperationResultDescriptor.ResultPayloadType.DefaultResponse;
                ResponseDescription.Text = this._result.Description;
                IsDefault.Checked = true;
                SetPayloadType(RESTOperationResultDescriptor.ResultPayloadType.DefaultResponse, true);
            }
            else
            {
                switch (descriptor.Category)
                {
                    case RESTOperationResultDescriptor.ResponseCategory.Informational:
                        IsInformational.Checked = true;
                        IsDefaultResponseType.Enabled = false;
                        break;

                    case RESTOperationResultDescriptor.ResponseCategory.Success:
                        IsSuccess.Checked = true;
                        IsDefaultResponseType.Enabled = false;
                        break;

                    case RESTOperationResultDescriptor.ResponseCategory.Redirection:
                        IsRedirection.Checked = true;
                        IsDefaultResponseType.Enabled = false;
                        break;

                    case RESTOperationResultDescriptor.ResponseCategory.ClientError:
                        IsClientError.Checked = true;
                        break;

                    case RESTOperationResultDescriptor.ResponseCategory.ServerError:
                        IsServerError.Checked = true;
                        break;

                    // The 'unknown' category is used for first-time creation of new descriptors. Use some sensible defaults here.
                    default:
                        this._result = new RESTOperationResultDescriptor(collection, RESTOperationResultDescriptor._DefaultCode);
                        ResponseDescription.Text = this._result.Description;
                        IsDefault.Checked = true;
                        break;
                }

                // Select the current payload type for this response code and load the payload 'name' field accordingly...
                SetPayloadType(this._result.PayloadType, true);

                // Show the associated default response document (if present)...
                if (this._result.PayloadType == RESTOperationResultDescriptor.ResultPayloadType.Document && this._result.Document != null) ResponseTypeName.Text = this._result.Document.Name;
                else if ((this._result.PayloadType == RESTOperationResultDescriptor.ResultPayloadType.DefaultResponse ||
                          this._result.PayloadType == RESTOperationResultDescriptor.ResultPayloadType.CustomResponse) && this._result.PayloadClass != null) ResponseTypeName.Text = this._result.PayloadClass.Name;
                else ResponseTypeName.Text = string.Empty;
                RspCardLo.Text = this._result.ResponseCardinality.LowerBoundaryAsString;
                RspCardHi.Text = this._result.ResponseCardinality.UpperBoundaryAsString;

                // Load header parameters (if present)...
                foreach (RESTHeaderParameterDescriptor paramDesc in descriptor.ResponseHeaders.Collection)
                {
                    if (paramDesc.IsValid)
                    {
                        ListViewItem newItem = new ListViewItem(paramDesc.Name);
                        newItem.Name = paramDesc.Name;
                        newItem.SubItems.Add(paramDesc.Description);
                        ResponseHeaderList.Items.Add(newItem);
                    }
                }
            }

            // Initialize the drop-down box with all possible codes for given category...
            this._codeList = RESTOperationResultDescriptor.GetResponseCodes(this._result.Category);
            foreach (CodeDescriptor dsc in this._codeList) ResponseCode.Items.Add(dsc.Label);
            ResponseCode.SelectedItem = CodeDescriptor.CodeToLabel(this._result.ResultCode);
            ResponseDescription.Text = (descriptor != null && !string.IsNullOrEmpty(descriptor.Description)) ? descriptor.Description : string.Empty;

            // Assign menus..
            ResponseHeaderList.ContextMenuStrip = ResponseHeaderMenuStrip;
        }

        /// <summary>
        /// This event is raised when the user clicks the 'Add Response Header' button.
        /// The method facilitates creation of an additional response header parameter.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void AddRspHeader_Click(object sender, EventArgs e)
        {
            RESTHeaderParameterDescriptor parameter = this._result.AddHeaderParameter();
            if (parameter != null && parameter.IsValid)
            {
                ListViewItem newItem = new ListViewItem(parameter.Name);
                newItem.Name = parameter.Name;
                newItem.SubItems.Add(parameter.Description);
                ResponseHeaderList.Items.Add(newItem);
            }
        }

        /// <summary>
        /// This event is raised when the user selects a previously defined response header parameter for deletion. It removes the selected
        /// header parameter.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void DeleteRspHeader_Click(object sender, EventArgs e)
        {
            if (ResponseHeaderList.SelectedItems.Count > 0)
            {
                ListViewItem key = ResponseHeaderList.SelectedItems[0];
                this._result.DeleteHeaderParameter(key.Text);
                ResponseHeaderList.Items.Remove(key);
            }
        }

        /// <summary>
        /// This event is raised when the user clicks the 'Unlink Response' button.
        /// Used to clear the current response Document Resource (if present).
        /// The response payload class is set to NULL and the cardinality is reset to default.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void RemoveResponse_Click(object sender, EventArgs e)
        {
            if (this._result.PayloadType == RESTOperationResultDescriptor.ResultPayloadType.CustomResponse ||
                this._result.PayloadType == RESTOperationResultDescriptor.ResultPayloadType.DefaultResponse) this._result.PayloadClass = null; 
            else if (this._result.PayloadType == RESTOperationResultDescriptor.ResultPayloadType.Document)   this._result.Document = null;
            else return; // 'spurious' click (in wrong context). Do nothing!

            this._result.ResponseCardinality = new Cardinality(Cardinality._Mandatory);
            RspCardLo.Text = this._result.ResponseCardinality.LowerBoundaryAsString;
            RspCardHi.Text = this._result.ResponseCardinality.UpperBoundaryAsString;
            ResponseTypeName.Clear();
        }

        /// <summary>
        /// This event is raised when the user selects a new entry from the code drop-down. The default description for the
        /// selected code is displayed in the description box and the user can optionally change this.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void ResponseCode_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = ResponseCode.SelectedIndex;
            this._result.ResultCode = CodeDescriptor.LabelToCode(ResponseCode.Items[index].ToString());
            if (IsRange.Checked) this._result.ResultCode = this._result.ResultCode[0] + RESTOperationResultDescriptor._RangePostfix;
            ResponseDescription.Text = this._result.Description;
        }

        /// <summary>
        /// This event is raised when the user clicks the 'Link Response' button.
        /// Actual operation is delegated to the OperationResultDescriptor.SetDocument method, which collects the appropriate document / class
        /// and returns its name (or empty string in case of cancel or error).
        /// Result is shown in the associated type-name field.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void SelectResponse_Click(object sender, EventArgs e)
        {
            ResponseTypeName.Text = this._result.SetDocument();
        }

        /// <summary>
        /// This event is raised when the user has modified a description. The text is copied to the Operation Result,
        /// unless it is found to be empty, in which case the box is filled again with the current description contents from the
        /// Operation Result Declaration.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void DescriptionNameFld_Leave(object sender, EventArgs e)
        {
            if (ResponseDescription.Text.Trim() == string.Empty) ResponseDescription.Text = this._result.Description;
            else this._result.Description = ResponseDescription.Text.Trim();
        }

        /// <summary>
        /// This event is raised when one of the category indicators has changed state.
        /// The method determines the new category and updates the other controls accordingly.
        /// Note that changing the category will re-initialize HTTP code and description but does not change the response payload.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void Category_CheckedChanged(object sender, EventArgs e)
        {
            RESTOperationResultDescriptor.ResponseCategory oldCategory = this._result.Category;
            RESTOperationResultDescriptor.ResponseCategory newCategory = this._result.Category;
            foreach (Control control in CategoryBox.Controls)
            {
                if (control is RadioButton && ((RadioButton)control).Checked)
                {
                    newCategory = EnumConversions<RESTOperationResultDescriptor.ResponseCategory>.StringToEnum(control.Tag.ToString());
                    break;
                }
            }

            if (newCategory != oldCategory)
            {
                this._result.Category = newCategory;
                this._codeList = RESTOperationResultDescriptor.GetResponseCodes(newCategory);
                ResponseCode.Items.Clear();
                foreach (CodeDescriptor dsc in this._codeList) ResponseCode.Items.Add(dsc.Label);

                // If we have the 'enforce range' checkbox enabled, we must enforce the 'range representation' of the category and
                // we must disable the drop-down as well...
                if (newCategory != RESTOperationResultDescriptor.ResponseCategory.Default && IsRange.Checked)
                    this._result.ResultCode = this._result.ResultCode[0] + RESTOperationResultDescriptor._RangePostfix;
                else IsRange.Checked = false;   // Default category does not support 'enforce range'.
                ResponseCode.Enabled = !IsRange.Checked;
                ResponseCode.SelectedItem = CodeDescriptor.CodeToLabel(this._result.ResultCode);
                ResponseDescription.Text = this._result.Description;

                // If we change the category to a 'non-error' category, we do not accept the default response...
                if (newCategory == RESTOperationResultDescriptor.ResponseCategory.Informational ||
                    newCategory == RESTOperationResultDescriptor.ResponseCategory.Success ||
                    newCategory == RESTOperationResultDescriptor.ResponseCategory.Redirection)
                {
                    IsDefaultResponseType.Enabled = false;
                    IsNone.Checked = true;
                    this._result.PayloadType = RESTOperationResultDescriptor.ResultPayloadType.None;
                    ResponseTypeName.Text = string.Empty;
                }
                else IsDefaultResponseType.Enabled = true;
            }
        }

        /// <summary>
        /// This event is raised when the user selects a header parameter for edit.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void EditRspHeader_Click(object sender, EventArgs e)
        {
            if (ResponseHeaderList.SelectedItems.Count > 0)
            {
                ListViewItem key = ResponseHeaderList.SelectedItems[0];
                ContextSlt context = ContextSlt.GetContextSlt();
                string originalKey = key.Text;
                RESTHeaderParameterDescriptor parameter = this._result.EditHeaderParameter(key.Text);
                if (parameter != null)
                {
                    key.SubItems[0].Text = parameter.Name;
                    key.SubItems[1].Text = parameter.Description;
                }
            }
        }

        /// <summary>
        /// This event is raised when the user clicks the 'edit header param collections' button. This brings up a subsequent dialog,
        /// which facilitates create-, delete- or edit of header parameter collections.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void EditRspHeaderCollections_Click(object sender, EventArgs e)
        {
            this._headerManager.ManageCollection();
        }

        /// <summary>
        /// This event is raised when the user selects a payload type. 
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void PayloadType_CheckedChanged(object sender, EventArgs e)
        {
            RESTOperationResultDescriptor.ResultPayloadType oldType = this._result.PayloadType;
            RESTOperationResultDescriptor.ResultPayloadType newType = this._result.PayloadType;
            foreach (Control control in PayloadTypeBox.Controls)
            {
                if (control is RadioButton && ((RadioButton)control).Checked)
                {
                    newType = EnumConversions<RESTOperationResultDescriptor.ResultPayloadType>.StringToEnum(control.Tag.ToString());
                    break;
                }
            }

            if (newType != oldType)
            {
                this._result.PayloadType = newType;
                SetPayloadType(newType, false);
            }
        }

        /// <summary>
        /// This event is raised when the user leaves either the response cardinality upper- or lower boundary field.
        /// We check the contents of the fields and create a new cardinality only if both fields have a value. In case
        /// of illegal values, we raise a pop-up error.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void RspCardinality_Leave(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(RspCardLo.Text) && !string.IsNullOrEmpty(RspCardHi.Text))
                {
                    this._result.ResponseCardinality = new Cardinality(RspCardLo.Text + ".." + RspCardHi.Text);
                }
            }
            catch (IllegalCardinalityException)
            {
                MessageBox.Show("Provided cardinality value is not correct!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Helper function that (re-)initializes, enables and/or disables parts of the dialog based on a provided payload type.
        /// When 'initButton' is set to 'true', the function also initializes the appropriate radio button.
        /// </summary>
        /// <param name="payloadType">Payload type to be processed.</param>
        /// <param name="initButton">When 'true', also set the appropriate radio button.</param>
        private void SetPayloadType(RESTOperationResultDescriptor.ResultPayloadType payloadType, bool initButton)
        {
            switch (payloadType)
            {
                case RESTOperationResultDescriptor.ResultPayloadType.CustomResponse:
                    if (!initButton)
                    {
                        if (MessageBox.Show("Warning: use of Custom Response objects might lead to unexpected results during artefact generation and " +
                                            "must be used with caution!" + Environment.NewLine +
                                            "A Custom Response may only be used when an ordinary Document Resource does not suffice." + Environment.NewLine +
                                            "Do you want to continue?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                        {
                            ResponsePayloadBox.Enabled = true;
                            ResponseTypeName.Text = this._result.PayloadClass != null ? this._result.PayloadClass.Name : string.Empty;
                            RspCardLo.Text = this._result.ResponseCardinality.LowerBoundaryAsString;
                            RspCardHi.Text = this._result.ResponseCardinality.UpperBoundaryAsString;
                        }
                        else
                        {
                            payloadType = RESTOperationResultDescriptor.ResultPayloadType.None;
                            ResponsePayloadBox.Enabled = false;
                            ResponseTypeName.Text = string.Empty;
                            IsNone.Checked = true;
                        }
                    }
                    else
                    {
                        ResponsePayloadBox.Enabled = true;
                        ResponseTypeName.Text = this._result.PayloadClass != null ? this._result.PayloadClass.Name : string.Empty;
                        RspCardLo.Text = this._result.ResponseCardinality.LowerBoundaryAsString;
                        RspCardHi.Text = this._result.ResponseCardinality.UpperBoundaryAsString;
                        IsCustomType.Checked = true;
                    }
                    ExternalLinkBox.Enabled = false;
                    ExternalLink.Text = string.Empty;
                    break;

                case RESTOperationResultDescriptor.ResultPayloadType.DefaultResponse:
                    ExternalLinkBox.Enabled = false;
                    ExternalLink.Text = string.Empty;
                    ResponsePayloadBox.Enabled = false;
                    ResponseTypeName.Text = this._result.PayloadClass != null ? this._result.PayloadClass.Name : string.Empty;
                    RspCardLo.Text = this._result.ResponseCardinality.LowerBoundaryAsString;
                    RspCardHi.Text = this._result.ResponseCardinality.UpperBoundaryAsString;
                    if (initButton) IsDefaultResponseType.Checked = true;
                    break;

                case RESTOperationResultDescriptor.ResultPayloadType.Document:
                    ExternalLinkBox.Enabled = false;
                    ExternalLink.Text = string.Empty;
                    ResponsePayloadBox.Enabled = true;
                    ResponseTypeName.Text = this._result.Document != null ? this._result.Document.Name : string.Empty;
                    RspCardLo.Text = this._result.ResponseCardinality.LowerBoundaryAsString;
                    RspCardHi.Text = this._result.ResponseCardinality.UpperBoundaryAsString;
                    if (initButton) IsDocument.Checked = true;
                    break;

                case RESTOperationResultDescriptor.ResultPayloadType.Link:
                    ExternalLinkBox.Enabled = true;
                    ExternalLink.Text = !string.IsNullOrEmpty(this._result.ExternalReference) ? this._result.ExternalReference : string.Empty;
                    ResponsePayloadBox.Enabled = false;
                    ResponseTypeName.Text = string.Empty;
                    if (initButton) IsExternalLink.Checked = true;
                    break;

                default:
                    ExternalLinkBox.Enabled = false;
                    ExternalLink.Text = string.Empty;
                    ResponsePayloadBox.Enabled = false;
                    ResponseTypeName.Text = string.Empty;
                    if (initButton) IsNone.Checked = true;
                    break;
            }
        }

        /// <summary>
        /// Invoked when the user selects- / un-selects the 'Enforce Range' checkbox.
        /// When checked, the associated response code is changed into its 'nXX' format.
        /// When un-checked, the associated response code is reset to it's generic ('n00') format.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void IsRange_CheckedChanged(object sender, EventArgs e)
        {
            // Enforce range does not work for the 'default' category, is ignored in that case.
            if (_result.Category != RESTOperationResultDescriptor.ResponseCategory.Default)
            {
                string range = this._result.ResultCode[0] + (IsRange.Checked ? RESTOperationResultDescriptor._RangePostfix :
                                                                              RESTOperationResultDescriptor._DefaultPostfix);
                this._result.ResultCode = range;
                ResponseCode.SelectedItem = CodeDescriptor.CodeToLabel(range);
                ResponseDescription.Text = this._result.Description;
                ResponseCode.Enabled = !IsRange.Checked;
            }
            else IsRange.Checked = false;
        }

        /// <summary>
        /// Invoked when the user has entered an URL in the external-link field. We perform a quick check on link format and
        /// load the result in the response descriptor.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void ExternalLink_Leave(object sender, EventArgs e)
        {
            try
            {
                var newUri = new Uri(ExternalLink.Text, UriKind.Absolute);
                this._result.ExternalReference = newUri.ToString();
                ExternalLink.Text = this._result.ExternalReference;
            }
            catch // ...and ignore the actual error.
            {
                MessageBox.Show("Link '" + ExternalLink.Text + "' has illegal (absolute) URL format, please try again!", 
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// This event is raised when the user clicks the 'use header collection' button. We return a (selected) parameter collection from
        /// the appropriate collection manager and copy all header parameters that do not yet exist in our current request list.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void UseRspHeaderCollection_Click(object sender, EventArgs e)
        {
            foreach (RESTHeaderParameterDescriptor parameter in this._headerManager.GetCollectionContents())
            {
                if (ResponseHeaderList.FindItemWithText(parameter.Name) == null && this._result.AddHeaderParameter(parameter))
                {
                    ListViewItem newItem = new ListViewItem(parameter.Name);
                    newItem.Name = parameter.Name;
                    newItem.SubItems.Add(parameter.Description);
                    ResponseHeaderList.Items.Add(newItem);
                }
            }
        }
    }
}
