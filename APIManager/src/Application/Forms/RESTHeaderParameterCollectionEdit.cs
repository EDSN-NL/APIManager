using System;
using System.Windows.Forms;
using Framework.Context;
using Framework.Util;
using Plugin.Application.CapabilityModel.API;

namespace Plugin.Application.Forms
{
    /// <summary>
    /// Presents a user dialog that facilitates either creation of a new header parameter collection or changing an existing one.
    /// </summary>
    internal partial class RESTHeaderParameterCollectionEdit : Form
    {
        private RESTHeaderParameterCollection _collection;
        private RESTCollection.CollectionScope _scope;

        /// <summary>
        /// Returns the collection name as assigned by the user.
        /// </summary>
        internal string CollectionName { get { return CollectionNmFld.Text; } }

        /// <summary>
        /// Returns the created or modified collection.
        /// </summary>
        internal RESTHeaderParameterCollection Collection { get { return this._collection; } }

        /// <summary>
        /// Returns the user-selected scope of the collection.
        /// </summary>
        internal RESTCollection.CollectionScope Scope { get { return this._scope; } }

        /// <summary>
        /// Dialog that facilitates creation of a new Operation Result Declaration (or editing of an existing one).
        /// </summary>
        /// <param name="result">Initial declaration to use for editing.</param>
        internal RESTHeaderParameterCollectionEdit(RESTHeaderParameterCollection thisCollection)
        {
            InitializeComponent();
            ContextSlt context = ContextSlt.GetContextSlt();

            if (thisCollection != null)
            {
                this._collection = thisCollection;
                this.Text = "Edit existing Collection";
                CollectionNmFld.Text = thisCollection.Name;
                this._scope = thisCollection.Scope;
                ScopeGroup.Enabled = false;             // You can NOT change the scope of an existing collection!

                // Load the result codes from the existing collection...
                foreach (RESTHeaderParameterDescriptor resultDesc in thisCollection.Collection)
                {
                    if (resultDesc.IsValid)
                    {
                        ListViewItem newItem = new ListViewItem(resultDesc.Name);
                        newItem.SubItems.Add(resultDesc.Description);
                        HeaderParameterList.Items.Add(newItem);
                    }
                }
            }
            else
            {
                this.Text = "Create new Collection";
                this.Ok.Enabled = false;
                this._scope = RESTCollection.CollectionScope.API;
                this._collection = new RESTHeaderParameterCollection(null, this._scope);
            }

            // Assign context menus to the appropriate controls...
            HeaderParameterList.ContextMenuStrip = ResponseCodeMenuStrip;

            // Find out which radio button to select for the scope...
            foreach (Control control in ScopeGroup.Controls)
            {
                if (control is RadioButton && string.Compare(control.Text, this._scope.ToString(), true) == 0)
                {
                    ((RadioButton)control).Checked = true;
                    break;
                }
            }
        }

        /// <summary>
        /// This event is raised when the user clicks the 'Add Header Parameter' button.
        /// The method facilitates creation of an additional header parameter.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void AddHeaderParameter_Click(object sender, EventArgs e)
        {
            RESTHeaderParameterDescriptor result = this._collection.AddHeaderParameter();
            if (result != null && result.IsValid)
            {
                ListViewItem newItem = new ListViewItem(result.Name);
                newItem.SubItems.Add(result.Description);
                HeaderParameterList.Items.Add(newItem);
            }
        }

        /// <summary>
        /// This event is raised when the user selects a header parameter for deletion.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void DeleteHeaderParameter_Click(object sender, EventArgs e)
        {
            if (HeaderParameterList.SelectedItems.Count > 0)
            {
                ListViewItem key = HeaderParameterList.SelectedItems[0];
                this._collection.DeleteHeaderParameter(key.Text);
                HeaderParameterList.Items.Remove(key);
            }
        }

        /// <summary>
        /// This event is raised when the user selects a header parameter for edit.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void EditHeaderParameter_Click(object sender, EventArgs e)
        {
            if (HeaderParameterList.SelectedItems.Count > 0)
            {
                ListViewItem key = HeaderParameterList.SelectedItems[0];
                string originalKey = key.Text;
                RESTHeaderParameterDescriptor result = this._collection.EditHeaderParameter(key.Text);
                if (result != null)
                {
                    key.SubItems[0].Text = result.Name;
                    key.SubItems[1].Text = result.Description;
                    HeaderParameterList.Sort();
                }
            }
        }

        /// <summary>
        /// This event is raised when the user leaves the parameter name field. It checks whether the field has some
        /// contents and if so, enables the Ok key.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void CollectionNmFld_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(CollectionNmFld.Text) && CollectionNmFld.Text.Length > 1)
            {
                this._collection.Name = CollectionNmFld.Text;
                this.Ok.Enabled = true;
            }
        }

        /// <summary>
        /// We invoke this handler whenever one of the Control Scope radio buttons is clicked.
        /// The function converts the selected button to an enumeration value.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void ScopeGroup_CheckedChanged(object sender, EventArgs e)
        {
            foreach (Control control in ScopeGroup.Controls)
            {
                if (control is RadioButton && ((RadioButton)control).Checked)
                {
                    this._scope = EnumConversions<RESTCollection.CollectionScope>.StringToEnum(control.Text);
                    break;
                }
            }
        }
    }
}
