using System;
using System.Windows.Forms;
using Framework.Util;
using Framework.Context;
using Framework.Model;
using Plugin.Application.CapabilityModel.API;

namespace Plugin.Application.Forms
{
    public partial class RESTHeaderParameterDialog : Form
    {
        // Configuration properties used by this module:
        private const string _LogicalDataTypeStereotype         = "LogicalDataTypeStereotype";
        private const string _CoreDataTypeEnumStereotype        = "CoreDataTypeEnumStereotype";
        private const string _BusinessDataTypeEnumStereotype    = "BusinessDataTypeEnumStereotype";

        private RESTHeaderParameterDescriptor _parameter;    // The parameter under construction (or being edited).

        // Preconditions for enabling the OK button...
        private bool _hasName;
        private bool _hasClassifier;

        /// <summary>
        /// This property is used to return the created/updated parameter declaration from the dialog.
        /// </summary>
        internal RESTHeaderParameterDescriptor Parameter { get { return this._parameter; } }

        /// <summary>
        /// Dialog constructor, initialises the REST Header Parameter create/edit dialog.
        /// </summary>
        /// <param name="parameter">The parameter declaration to use in the dialog.</param>
        /// This must be a list that is sorted by parameter name.</param>
        internal RESTHeaderParameterDialog(RESTHeaderParameterDescriptor parameter)
        {
            InitializeComponent();
            bool isEdit = parameter.Name != string.Empty;

            this.Text = isEdit ? "Edit existing header parameter" : "Create new header parameter";
            this._parameter = parameter;

            ParameterName.Text = parameter.Name;
            ParamDescription.Text = parameter.Description;
            if (parameter.Classifier != null) ParameterClassifier.Text = parameter.Classifier.Name;

            Ok.Enabled = false;                     // Block the OK button until preconditions have been satisfied.

            // If we're going to edit an existing parameter, we can set the preconditions to true!
            this._hasName = this._hasClassifier = isEdit;
        }

        /// <summary>
        /// This event is invoked when the user has changed the name of the parameter...
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void ParameterName_TextChanged(object sender, EventArgs e)
        {
            string newName = Conversions.ToPascalCase(ParameterName.Text.Trim());
            if (!newName.Contains(" "))
            {
                this._hasName = false;
                if (newName != string.Empty)
                {
                    ParameterName.Text = newName;
                    this._parameter.Name = newName;
                    this._hasName = true;
                }
                CheckOK();
            }
            else MessageBox.Show("Parameter name must NOT contain spaces, please try again!", 
                                 "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// This event is invoked when the user clicked the 'select classifier' button in order to select the
        /// datatype that we want to use as parameter type. In this case, our data type must be a primitive type.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void SelectClassifier_Click(object sender, EventArgs e)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            MEDataType newType = context.SelectDataType(false);

            // Could be NULL if user decided to cancel the type picker.
            if (newType != null)
            {
                if (newType is MEDataType && (newType.HasStereotype(context.GetConfigProperty(_LogicalDataTypeStereotype)) ||
                                              newType.HasStereotype(context.GetConfigProperty(_CoreDataTypeEnumStereotype)) ||
                                              newType.HasStereotype(context.GetConfigProperty(_BusinessDataTypeEnumStereotype))))
                {
                    this._parameter.Classifier = newType;
                    ParameterClassifier.Text = this._parameter.Classifier.Name;
                    this._hasClassifier = true;
                }
                else MessageBox.Show("Classifier must be a Primitive Data Type, please try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            CheckOK();
        }

        /// <summary>
        /// Check whether we can enable the OK button.
        /// </summary>
        private void CheckOK()
        {
            Ok.Enabled = this._hasName && this._hasClassifier;
        }

        /// <summary>
        /// This event is raised when the user entered/changed the parameter description text.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void ParamDescription_Leave(object sender, EventArgs e)
        {
            this._parameter.Description = ParamDescription.Text;
        }
    }
}
