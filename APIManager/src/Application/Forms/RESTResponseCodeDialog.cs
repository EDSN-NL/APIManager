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
        private const string _DefaultSuccessCode            = "DefaultSuccessCode";
        private const string _DefaultSuccessEditCode        = "DefaultSuccessEditCode";
        private const string _APISupportModelPathName       = "APISupportModelPathName";
        private const string _OperationResultClassName      = "OperationResultClassName";

        private RESTOperationResultDeclaration _result;     // The result declaration that we're building.
        private List<RESTOperationResultDeclaration.CodeDescriptor> _codeList;      // Currently active set of response code definitions.

        internal RESTOperationResultDeclaration OperationResult { get { return this._result; } }

        /// <summary>
        /// Dialog that facilitates creation of a new Operation Result Declaration (or editing of an existing one).
        /// </summary>
        /// <param name="result">Initial declaration to use for editing.</param>
        internal RESTResponseCodeDialog(RESTOperationResultDeclaration result)
        {
            InitializeComponent();
            ContextSlt context = ContextSlt.GetContextSlt();
            this._result = result;

            // Initialize the proper radio button according to the current category...
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

                // Safety catch: we should NOT invoke this dialog with Unknown/Default categories. If we DO try this, the
                // result declaration is reset to 'Informational'....
                default:
                    this._result = new RESTOperationResultDeclaration(RESTOperationResultCapability.ResponseCategory.Informational);
                    IsInformational.Checked = true;
                    break;
            }

            // Initialize the drop-down box with all possible codes for given category...
            // We skip the 'default OK' code since this may not be changed.
            this._codeList = this._result.GetResponseCodes();
            string defaultOK = context.GetConfigProperty(_DefaultSuccessCode);
            foreach (RESTOperationResultDeclaration.CodeDescriptor dsc in this._codeList)
            {
                if (dsc.Code != defaultOK) ResponseCode.Items.Add(dsc.Label);
            }
            ResponseCode.SelectedItem = RESTOperationResultDeclaration.CodeDescriptor.CodeToLabel(this._result.ResultCode);
            ResponseDescription.Text = this._result.Description;
            AssignParameterClass();
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
            this._result.ResultCode = RESTOperationResultDeclaration.CodeDescriptor.LabelToCode(ResponseCode.Items[index].ToString());
            ResponseDescription.Text = this._result.Description;
        }

        /// <summary>
        /// This event is raised whenever the user has modified a description. The text is copied to the Operation Result,
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
                // the newly selected category. This also resets the descripion and code...
                this._result = new RESTOperationResultDeclaration(newCategory);
                if (this._result.Category == RESTOperationResultCapability.ResponseCategory.Success)
                    this._result.ResultCode = ContextSlt.GetContextSlt().GetConfigProperty(_DefaultSuccessEditCode); 
                this._codeList = this._result.GetResponseCodes();
                string defaultOK = ContextSlt.GetContextSlt().GetConfigProperty(_DefaultSuccessCode);
                ResponseCode.Items.Clear();
                foreach (RESTOperationResultDeclaration.CodeDescriptor dsc in this._codeList)
                {
                    if (dsc.Code != defaultOK) ResponseCode.Items.Add(dsc.Label);
                }
                ResponseCode.SelectedItem = RESTOperationResultDeclaration.CodeDescriptor.CodeToLabel(this._result.ResultCode);
                ResponseDescription.Text = this._result.Description;
                AssignParameterClass();
            }
        }

        /// <summary>
        /// Helper method that, in case of an error response, locates the default error response parameter class and assigns this to the response
        /// declaration object. If the result declaration already constains a Parameter Class, the method does nothing.
        /// </summary>
        private void AssignParameterClass()
        {
            if (this._result.ResponseDocumentClass == null &&
                (this._result.Category == RESTOperationResultCapability.ResponseCategory.ClientError ||
                this._result.Category == RESTOperationResultCapability.ResponseCategory.ServerError))
            {
                ContextSlt context = ContextSlt.GetContextSlt();
                ModelSlt model = ModelSlt.GetModelSlt();
                MEClass resultParam = model.FindClass(context.GetConfigProperty(_APISupportModelPathName), context.GetConfigProperty(_OperationResultClassName));
                if (resultParam != null) this._result.ResponseDocumentClass = resultParam;
                else Logger.WriteError("Plugin.Application.Forms.RESTResponseCodeDialog.AssiognParameterClass >> Unable to find '" +
                                       context.GetConfigProperty(_APISupportModelPathName) + "/" + context.GetConfigProperty(_OperationResultClassName));
            }
        }
    }
}
