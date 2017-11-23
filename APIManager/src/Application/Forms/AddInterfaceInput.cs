using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Model;
using Framework.Context;
using Framework.Logging;

namespace Plugin.Application.Forms
{
    internal partial class AddInterfaceInput : Form
    {
        //Configuration properties used by this module...
        private const string _InterfaceContractClassStereotype  = "InterfaceContractClassStereotype";

        private MEPackage _serviceModelPackage;
        private bool _operValidation;
        private bool _newMinorVersion;
        private bool _validName;
        private bool _validOperation;
        private List<MEClass> _operationList;

        internal string InterfaceName { get { return InterfaceNameFld.Text; } }
        internal bool MinorVersionIndicator { get { return this._newMinorVersion; } }
        internal List<MEClass> SelectedOperations { get { return GetSelectedOperations(); } }

        /// <summary>
        /// Constructor that creates a new dialog for creating an Interface in model 'serviceModelPackage'. The Interface
        /// might be associated with one or more operations specified in 'operationList'.
        /// </summary>
        /// <param name="serviceModelPackage">Service model in which we're going to create the new Interface.</param>
        /// <param name="operationList">List of available operations to assign to the new Interface.</param>
        internal AddInterfaceInput(MEPackage serviceModelPackage, List<MEClass> operationList)
        {
            InitializeComponent();
            this._serviceModelPackage = serviceModelPackage;
            ErrorLine.Visible = false;
            this._operValidation = true;
            this._newMinorVersion = false;
            this._validName = false;
            this._validOperation = false;
            Ok.Enabled = false;

            this._operationList = operationList;
            NewMinorVersion.Checked = false;
            OperationList.Sorted = true;
            OperationList.CheckOnClick = true;

            OperationList.BeginUpdate();
            OperationList.Items.Clear();
            foreach (MEClass operation in operationList) OperationList.Items.Add(operation, false);
            OperationList.EndUpdate();
        }

        /// <summary>
        /// Retrieves the collection of selected operations from the listbox and converts from internal collection
        /// to a List of MEClass elements.
        /// </summary>
        /// <returns>List of selected elements.</returns>
        private List<MEClass> GetSelectedOperations()
        {
            List<MEClass> selectedOperations = new List<MEClass>();
            foreach (MEClass selectedOperation in OperationList.CheckedItems)
            {
                selectedOperations.Add(selectedOperation);
            }
            return selectedOperations;
        }

        /// <summary>
        /// Invoked whenever the user made changes to the text field. Validates the Interface name...
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void OperationNames_TextChanged(object sender, EventArgs e)
        {
            string errorText = string.Empty;
            this._operValidation = true;

            string interfaceName = InterfaceNameFld.Text;
            if (interfaceName.Length == 0)
            {
                errorText = "Please specify an Interface name!";
                this._operValidation = false;
            }
            else
            {
                if (!char.IsUpper(interfaceName[0]))
                {
                    errorText = "Name '" + interfaceName + "' must be in PascalCase, please try again!";
                    this._operValidation = false;
                }

                // Check if this is a unique name...
                if (this._serviceModelPackage.FindClass(interfaceName, ContextSlt.GetContextSlt().GetConfigProperty(_InterfaceContractClassStereotype)) != null)
                {
                    errorText = "Name '" + interfaceName + "' is not unique, try again!";
                    this._operValidation = false;
                }
            }

            if (!this._operValidation)
            {
                ErrorLine.Text = errorText;
                ErrorLine.Visible = true;
            }
            else
            {
                ErrorLine.Clear();
                ErrorLine.Visible = false;
            }
            ErrorLine.Update();
            this._validName = this._operValidation;
            Ok.Enabled = (this._validName && this._validOperation);
        }

        /// <summary>
        /// Called whenever the user changes the state of the 'minor version' checkbox.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>m>
        private void NewMinorVersion_CheckedChanged(object sender, EventArgs e)
        {
            this._newMinorVersion = NewMinorVersion.Checked;
        }

        /// <summary>
        /// Invoked when user clicks the "All" button, selects all Operations.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void All_Click(object sender, EventArgs e)
        {
            Logger.WriteInfo("Plugin.Application.Forms.AddInterfaceInput.All_Click >> Set all as selected");
            for (int i = 0; i < OperationList.Items.Count; OperationList.SetItemChecked(i++, true)) ;
            this._validOperation = true;
            Ok.Enabled = (this._validName && this._validOperation);
        }

        /// <summary>
        /// Invoked when user clicks the "None" button, un-selects all Operations.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void None_Click(object sender, EventArgs e)
        {
            Logger.WriteInfo("Plugin.Application.Forms.AddInterfaceInput.None_Click >> Set all as unselected");
            for (int i = 0; i < OperationList.Items.Count; OperationList.SetItemChecked(i++, false)) ;
            this._validOperation = false;
            Ok.Enabled = (this._validName && this._validOperation);
        }

        /// <summary>
        /// Invoked whenever the user selects- or un-selects an Operation in the Operation list.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void OperationList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (OperationList.CheckedItems.Count > 0) this._validOperation = true;
            Ok.Enabled = (this._validName && this._validOperation);
        }

        /// <summary>
        /// Invoked whenever the user selects an item in the Operation list. The operation automatically toggles the associated
        /// check-box for the item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OperationList_SelectedValueChanged(object sender, EventArgs e)
        {
            if (OperationList.SelectedIndex != -1)
            {
                bool itemChecked = OperationList.GetItemChecked(OperationList.SelectedIndex);
                OperationList.SetItemCheckState(OperationList.SelectedIndex, itemChecked ? CheckState.Checked : CheckState.Unchecked);
                if (OperationList.CheckedItems.Count > 0) this._validOperation = true;
                Ok.Enabled = (this._validName && this._validOperation);
            }
        }
    }
}
