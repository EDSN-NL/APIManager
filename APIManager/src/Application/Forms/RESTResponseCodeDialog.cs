using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Util;
using Framework.Logging;
using Framework.Context;
using Framework.Model;
using Plugin.Application.CapabilityModel.API;

namespace Plugin.Application.Forms
{
    internal partial class RESTResponseCodeDialog : Form
    {
        // Configuration codes used by this module:
        private const string _APISupportModelPathName = "APISupportModelPathName";

        private RESTOperationResultDescriptor _result;                          // The Descriptor that we're building.
        private List<RESTOperationResultDescriptor.CodeDescriptor> _codeList;   // Currently active set of response code definitions.
        private RESTResponseCodeCollection _collection;                         // The collection that will hold the descriptor.

        internal RESTOperationResultDescriptor OperationResult { get { return this._result; } }

        /// <summary>
        /// Dialog that facilitates creation of a new Operation Result Descriptor (or editing of an existing one).
        /// When we want to create a new descriptor, pass 'NULL' to 'descriptor'.
        /// </summary>
        /// <param name="collection">The collection that will 'own' the new descriptor.</param>
        /// <param name="descriptor">Initial declaration to use for editing.</param>
        internal RESTResponseCodeDialog(RESTResponseCodeCollection collection, RESTOperationResultDescriptor descriptor)
        {
            InitializeComponent();
            ContextSlt context = ContextSlt.GetContextSlt();
            this._result = descriptor;
            this._collection = collection;

            if (descriptor == null)
            {
                DO SOMETHING
            }
            else
            {
                switch (descriptor.Category)
                {
                    case RESTOperationResultDescriptor.ResponseCategory.Informational:
                        IsInformational.Checked = true;
                        break;

                    case RESTOperationResultDescriptor.ResponseCategory.Success:
                        IsSuccess.Checked = true;
                        break;

                    case RESTOperationResultDescriptor.ResponseCategory.Redirection:
                        IsRedirection.Checked = true;
                        break;

                    case RESTOperationResultDescriptor.ResponseCategory.ClientError:
                        IsClientError.Checked = true;
                        break;

                    case RESTOperationResultDescriptor.ResponseCategory.ServerError:
                        IsServerError.Checked = true;
                        break;

                    // The 'unknown' category is used for first-time creation of new descriptors. Use some sensible defaults here.
                    default:
                        this._result = new RESTOperationResultDescriptor(RESTOperationResultDescriptor.ResponseCategory.Informational);
                        ResponseDescription.Text = this._result.Description;
                        IsInformational.Checked = true;
                        break;
                }

                // Select the current payload type for this response code and load the payload 'name' field accordingly...
                switch (descriptor.PayloadType)
                {
                    case RESTOperationResultDescriptor.ResultPayloadType.CustomResponse:
                        PayloadBox.Text = descriptor.ResponsePayloadClass != null ? descriptor.ResponsePayloadClass.Name : string.Empty;
                        IsCustomType.Checked = true;
                        break;

                    case RESTOperationResultDescriptor.ResultPayloadType.DefaultResponse:
                        PayloadBox.Text = descriptor.ResponsePayloadClass != null ? descriptor.ResponsePayloadClass.Name : string.Empty;
                        IsDefaultResponseType.Checked = true;
                        break;

                    case RESTOperationResultDescriptor.ResultPayloadType.Document:
                        PayloadBox.Text = descriptor.ResponsePayloadClass != null ? descriptor.ResponsePayloadClass.Name : string.Empty;
                        IsDocument.Checked = true;
                        break;

                    case RESTOperationResultDescriptor.ResultPayloadType.Link:
                        PayloadBox.Text = !string.IsNullOrEmpty(descriptor.ExternalReference) ? descriptor.ExternalReference : string.Empty;
                        IsExternalLink.Checked = true;
                        break;

                    default:
                        PayloadBox.Text = string.Empty;
                        IsNone.Checked = true;
                        break;
                }
            }

            // Initialize the drop-down box with all possible codes for given category...
            this._codeList = this._result.GetResponseCodes();
            foreach (RESTOperationResultDescriptor.CodeDescriptor dsc in this._codeList)
            {
                ResponseCode.Items.Add(dsc.Label);
            }
            ResponseCode.SelectedItem = RESTOperationResultDescriptor.CodeDescriptor.CodeToLabel(this._result.ResultCode);
            ResponseDescription.Text = (descriptor != null && !string.IsNullOrEmpty(descriptor.Description)) ? descriptor.Description : string.Empty;
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
            this._result.ResultCode = RESTOperationResultDescriptor.CodeDescriptor.LabelToCode(ResponseCode.Items[index].ToString());
            ResponseDescription.Text = this._result.Description;
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
        /// Note that changing the category will re-initialize the associated operation result declaration.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void Category_CheckedChanged(object sender, EventArgs e)
        {
            RESTOperationResultCapability.ResponseCategory oldCategory = this._result.Category;
            RESTOperationResultCapability.ResponseCategory newCategory = this._result.Category;
            foreach (Control control in CategoryBox.Controls)
            {
                if (control is RadioButton && ((RadioButton)control).Checked)
                {
                    newCategory = EnumConversions<RESTOperationResultCapability.ResponseCategory>.StringToEnum(control.Tag.ToString());
                    break;
                }
            }

            if (newCategory != oldCategory)
            {
                // Changing the category means that we're going to replace the current result object by a new one according to 
                // the newly selected category. This also resets the payload type, descripion and code...
                this._result = new RESTOperationResultDescriptor(newCategory);
                this._codeList = this._result.GetResponseCodes();
                ResponseCode.Items.Clear();
                foreach (RESTOperationResultDescriptor.CodeDescriptor dsc in this._codeList) ResponseCode.Items.Add(dsc.Label);
                ResponseCode.SelectedItem = RESTOperationResultDescriptor.CodeDescriptor.CodeToLabel(this._result.ResultCode);
                ResponseDescription.Text = this._result.Description;
                PayloadBox.Text = string.Empty;

                // Select the current payload type for this response code...
                switch (this._result.PayloadType)
                {
                    case RESTOperationResultDescriptor.ResultPayloadType.CustomResponse:
                        IsCustomType.Checked = true;
                        break;

                    case RESTOperationResultDescriptor.ResultPayloadType.DefaultResponse:
                        IsDefaultResponseType.Checked = true;
                        break;

                    case RESTOperationResultDescriptor.ResultPayloadType.Document:
                        IsDocument.Checked = true;
                        break;

                    case RESTOperationResultDescriptor.ResultPayloadType.Link:
                        IsExternalLink.Checked = true;
                        break;

                    default:
                        IsNone.Checked = true;
                        break;
                }
            }
        }

        /// <summary>
        /// This event is raised when the user clicks the 'select payload' button. Actions depend on the currently
        /// selected payload type.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectPayload_Click(object sender, EventArgs e)
        {

        }

        private void PayloadType_CheckedChanged(object sender, EventArgs e)
        {
            RESTOperationResultDescriptor.ResultPayloadType oldType = this._result.PayloadType;
            RESTOperationResultDescriptor.ResultPayloadType newType = this._result.PayloadType;
            foreach (Control control in ResponseRefTypeBox.Controls)
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
                if (newType != RESTOperationResultDescriptor.ResultPayloadType.None)
                {
                    if (newType == RESTOperationResultDescriptor.ResultPayloadType.Link)
                        PayloadBox.Text = !string.IsNullOrEmpty(this._result.ExternalReference) ? this._result.ExternalReference : string.Empty;
                    else
                        PayloadBox.Text = this._result.ResponsePayloadClass != null ? this._result.ResponsePayloadClass.Name : string.Empty;
                }
                else PayloadBox.Text = string.Empty;
            }
        }
    }
}
