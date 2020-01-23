using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Util;
using Framework.Context;
using Framework.Model;
using Plugin.Application.CapabilityModel.API;

namespace Plugin.Application.Forms
{
    public partial class RESTParameterDialog : Form
    {
        private RESTParameterDeclaration _parameter;    // The parameter under construction (or being edited).

        // Preconditions for enabling the OK button...
        private bool _hasName;
        private bool _hasClassifier;
        private bool _hasCardinality;

        /// <summary>
        /// This property is used to return the created/updated parameter declaration from the dialog.
        /// </summary>
        internal RESTParameterDeclaration Parameter { get { return this._parameter; } }

        /// <summary>
        /// Dialog constructor, initialises the REST Parameter create/edit dialog.
        /// </summary>
        /// <param name="parameter">The parameter declaration to use in the dialog.</param>
        /// This must be a list that is sorted by parameter name.</param>
        internal RESTParameterDialog(RESTParameterDeclaration parameter)
        {
            InitializeComponent();
            bool isEdit = parameter.Name != string.Empty;

            this.Text = isEdit ? "Edit existing Parameter" : "Create new Parameter";
            this._parameter = parameter;

            ParameterName.Text = parameter.Name;
            ParamDescription.Text = parameter.Description;
            if (parameter.Classifier != null) ParameterClassifier.Text = parameter.Classifier.Name;
            ParameterDefaultValue.Text = parameter.Default;
            if (parameter.Cardinality != null)
            {
                ParameterCardLow.Text = parameter.Cardinality.LowerBoundary.ToString();
                ParameterCardHigh.Text = parameter.Cardinality.UpperBoundary == 0 ? "*" : parameter.Cardinality.UpperBoundary.ToString();
            }
            MayBeEmpty.Checked = parameter.AllowEmptyValue;

            // Initialize the drop-down box with the possible values of our QueryCollectionFormat enumeration...
            foreach (string str in Enum.GetNames(typeof(RESTParameterDeclaration.QueryCollectionFormat)))
            {
                CollectionFormat.Items.Add(str);
            }
            CollectionFormat.SelectedIndex = isEdit ? (int)parameter.CollectionFormat : (int)RESTParameterDeclaration.QueryCollectionFormat.Unknown;
            CollectionFormat.Enabled = isEdit && parameter.Cardinality.IsList;
         
            IsDataType.Checked = true;              // Set default classifier type to 'data type'.
            Ok.Enabled = false;                     // Block the OK button until preconditions have been satisfied.

            // If we're going to edit an existing parameter, we can set the preconditions to true!
            this._hasName = this._hasClassifier = this._hasCardinality = isEdit;
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
        /// This event is invoked when the user clicked the 'select classifier' button in order to select the class or 
        /// datatype that we want to use as parameter type.
        /// Since we must explicitly differentiate between classes, generic data types and enumerations, we use the
        /// set of radio buttons to request the exact type that the user desires. This affects the picker dialog that
        /// is shown to the user.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void SelectClassifier_Click(object sender, EventArgs e)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            this._hasClassifier = false;

            // Display the data-type picker, which allows the user to select the appropriate type...
            if (IsEnum.Checked)
            {
                MEEnumeratedType newEnum = context.SelectDataType(true) as MEEnumeratedType;
                this._parameter.Classifier = newEnum;
            }
            else
            {
                MEDataType newDataType = context.SelectDataType(false);
                this._parameter.Classifier = newDataType;
            }
            if (this._parameter.Classifier != null) ParameterDefaultValue.Enabled = true;

            // Could be NULL if user decided to cancel the type picker.
            if (this._parameter.Classifier != null)
            {
                ParameterClassifier.Text = this._parameter.Classifier.Name;
                this._hasClassifier = true;
                this._parameter.Default = string.Empty;
            }
            CheckOK();
        }

        /// <summary>
        /// This event is raised whenever the user changed the text in the default value box.
        /// We only perform a rudimentary validation.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void ParameterDefaultValue_TextChanged(object sender, EventArgs e)
        {
            if (this._parameter.Classifier is MEEnumeratedType)
            {
                List<MEAttribute> validValues = ((MEEnumeratedType)this._parameter.Classifier).Enumerations;
                bool foundIt = false;
                foreach (MEAttribute attrib in validValues)
                {
                    if (attrib.Name == ParameterDefaultValue.Text)
                    {
                        foundIt = true;
                        break;
                    }
                }
                if (foundIt) this._parameter.Default = ParameterDefaultValue.Text;
                else
                {
                    string values = string.Empty;
                    foreach (MEAttribute attrib in validValues) values += attrib.Name + Environment.NewLine;
                    MessageBox.Show("Illegal default value for enumeraton '" + this._parameter.Classifier.Name + 
                                    "', choose one of:" + Environment.NewLine + values, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else this._parameter.Default = ParameterDefaultValue.Text;
        }

        /// <summary>
        /// This event is invoked whenever the user changes either of the two cardinality values. Only if BOTH contain a valid
        /// value do we update the parameter data! A high value of "*" is converted to integer 0.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void ParameterCard_TextChanged(object sender, EventArgs e)
        {
            int highVal, lowVal;
            if (ParameterCardHigh.Text == "*") highVal = 0;
            else if (!int.TryParse(ParameterCardHigh.Text, out highVal)) highVal = -1;
            if (!int.TryParse(ParameterCardLow.Text, out lowVal)) lowVal = -1;
            if (highVal != -1 && lowVal != -1 && ((highVal != 0 && lowVal <= highVal) || highVal == 0))
            {
                this._parameter.Cardinality = new Cardinality(lowVal, highVal);
                this._hasCardinality = true;
            }
            else this._hasCardinality = false;

            // If we have a cardinality > 1, we enable the collection format field for the user to select the correct format.
            // If we don't have a cardinality > 1, we set the collection format to "Not Applicable".
            if (highVal == 0 || highVal > 1) CollectionFormat.Enabled = true;
            else this._parameter.CollectionFormat = RESTParameterDeclaration.QueryCollectionFormat.NA;
            CheckOK();
        }

        /// <summary>
        /// This event is invoked whenever the user changes the current CollectionFormat value.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void CollectionFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = CollectionFormat.SelectedIndex;
            this._parameter.CollectionFormat = EnumConversions<RESTParameterDeclaration.QueryCollectionFormat>.StringToEnum(CollectionFormat.Items[CollectionFormat.SelectedIndex].ToString());
        }

        /// <summary>
        /// Check whether we can enable the OK button.
        /// </summary>
        private void CheckOK()
        {
            Ok.Enabled = (this._hasName && this._hasClassifier && this._hasCardinality);
            if (Ok.Enabled) this._parameter.Status = (this._parameter.Status == RESTParameterDeclaration.DeclarationStatus.Invalid) ? 
                    RESTParameterDeclaration.DeclarationStatus.Created : 
                    RESTParameterDeclaration.DeclarationStatus.Edited;
        }

        /// <summary>
        /// This event is raised when the user changes the value of the 'may be empty' check box.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void MayBeEmpty_CheckedChanged(object sender, EventArgs e)
        {
            this._parameter.AllowEmptyValue = MayBeEmpty.Checked;
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

        /// <summary>
        /// This event is raised when the user clicked the 'Ok' button. We validate some of the provided parameters and on errors, prevent the
        /// form from closing (after displaying a suitable error message).
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void Ok_Click(object sender, EventArgs e)
        {
            bool mayClose = true;
            if (!(Parameter.Classifier is MEDataType) && !(Parameter.Classifier is MEEnumeratedType))
            {
                MessageBox.Show("Parameter must be Data Type or Enumeration, please try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                mayClose = false;
            }
            if (Parameter.Cardinality.IsList)
            {
                if (Parameter.CollectionFormat == RESTParameterDeclaration.QueryCollectionFormat.NA ||
                    Parameter.CollectionFormat == RESTParameterDeclaration.QueryCollectionFormat.Unknown)
                {
                    MessageBox.Show("Collection Format must be one of Multi, CSV, SSV, TSV or Pipes, please try again!",
                                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    mayClose = false;
                }
            }
            this.DialogResult = mayClose ? DialogResult.OK : DialogResult.None;
        }
    }
}
