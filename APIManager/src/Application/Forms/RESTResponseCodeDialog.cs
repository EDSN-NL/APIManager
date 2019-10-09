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

        private RESTOperationResultDescriptor _result;     // The result declaration that we're building.
        private List<RESTOperationResultDescriptor.CodeDescriptor> _codeList;      // Currently active set of response code definitions.

        internal RESTOperationResultDescriptor OperationResult { get { return this._result; } }

        /// <summary>
        /// Dialog that facilitates creation of a new Operation Result Declaration (or editing of an existing one).
        /// </summary>
        /// <param name="result">Initial declaration to use for editing.</param>
        internal RESTResponseCodeDialog(RESTOperationResultDescriptor result)
        {
            InitializeComponent();
            ContextSlt context = ContextSlt.GetContextSlt();
            this._result = result;

            // Initialize the proper category button according to our current category...
            switch (result.Category)
            {
                case RESTOperationResultCapability.ResponseCategory.Informational:
                    IsInformational.Checked = true;
                    break;

                case RESTOperationResultCapability.ResponseCategory.Success:
                    IsSuccess.Checked = true;
                    break;

                case RESTOperationResultCapability.ResponseCategory.Redirection:
                    IsRedirection.Checked = true;
                    break;

                case RESTOperationResultCapability.ResponseCategory.ClientError:
                    IsClientError.Checked = true;
                    break;

                case RESTOperationResultCapability.ResponseCategory.ServerError:
                    IsServerError.Checked = true;
                    break;

                // Safety catch: we should NOT invoke this dialog with Unknown categories. If we DO try this, the
                // result declaration is reset to 'Informational'....
                default:
                    this._result = new RESTOperationResultDescriptor(RESTOperationResultCapability.ResponseCategory.Informational);
                    ResponseDescription.Text = this._result.Description;
                    IsInformational.Checked = true;
                    break;
            }

            // Select the current payload type for this response code and load the payload 'name' field accordingly...
            switch (result.PayloadType)
            {
                case RESTOperationResultDescriptor.ResultPayloadType.CustomResponse:
                    PayloadBox.Text = result.ResponsePayloadClass != null? result.ResponsePayloadClass.Name: string.Empty;
                    IsCustomType.Checked = true;
                    break;

                case RESTOperationResultDescriptor.ResultPayloadType.DefaultResponse:
                    PayloadBox.Text = result.ResponsePayloadClass != null? result.ResponsePayloadClass.Name: string.Empty;
                    IsDefaultResponseType.Checked = true;
                    break;

                case RESTOperationResultDescriptor.ResultPayloadType.Document:
                    PayloadBox.Text = result.ResponsePayloadClass != null ? result.ResponsePayloadClass.Name : string.Empty;
                    IsDocument.Checked = true;
                    break;

                case RESTOperationResultDescriptor.ResultPayloadType.Link:
                    PayloadBox.Text = !string.IsNullOrEmpty(result.ExternalReference) ? result.ExternalReference : string.Empty;
                    IsExternalLink.Checked = true;
                    break;

                default:
                    PayloadBox.Text = string.Empty;
                    IsNone.Checked = true;
                    break;
            }

            // Initialize the drop-down box with all possible codes for given category...
            this._codeList = this._result.GetResponseCodes();
            foreach (RESTOperationResultDescriptor.CodeDescriptor dsc in this._codeList)
            {
                ResponseCode.Items.Add(dsc.Label);
            }
            ResponseCode.SelectedItem = RESTOperationResultDescriptor.CodeDescriptor.CodeToLabel(this._result.ResultCode);
            ResponseDescription.Text = (result != null && !string.IsNullOrEmpty(result.Description)) ? result.Description : string.Empty;
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
