using System;
using System.Windows.Forms;
using Framework.Context;
using Plugin.Application.CapabilityModel;
using Plugin.Application.CapabilityModel.API;

namespace Plugin.Application.Forms
{
    /// <summary>
    /// Presents a user dialog that facilitates either creation of a new collection of changing an existing one.
    /// </summary>
    internal partial class RESTResponseCodeCollectionEdit : Form
    {
        private RESTResponseCodeCollection _collection;

        /// <summary>
        /// Returns the collection name as assigned by the user.
        /// </summary>
        internal string CollectionName { get { return CollectionNmFld.Text; } }

        /// <summary>
        /// Returns the created or modified collection.
        /// </summary>
        internal RESTResponseCodeCollection Collection { get { return this._collection; } }

        /// <summary>
        /// Dialog that facilitates creation of a new Operation Result Declaration (or editing of an existing one).
        /// </summary>
        /// <param name="result">Initial declaration to use for editing.</param>
        internal RESTResponseCodeCollectionEdit(RESTResponseCodeCollection thisCollection)
        {
            InitializeComponent();
            ContextSlt context = ContextSlt.GetContextSlt();
            this._collection = thisCollection;

            if (thisCollection != null)
            {
                this.Text = "Edit existing Collection";
                CollectionNmFld.Text = thisCollection.Name;

                // Load the result codes from the existing collection...
                foreach (RESTOperationResultDeclaration resultDecl in thisCollection.Collection)
                {
                    if (resultDecl.Status != RESTOperationResultDeclaration.DeclarationStatus.Invalid)
                    {
                        ListViewItem newItem = new ListViewItem(resultDecl.ResultCode);
                        newItem.SubItems.Add(resultDecl.Description);
                        ResponseCodeList.Items.Add(newItem);
                    }
                }
            }
            else
            {
                this.Text = "Create new Collection";
                this.Ok.Enabled = false;
            }

            // Assign context menus to the appropriate controls...
            ResponseCodeList.ContextMenuStrip = ResponseCodeMenuStrip;
        }

        /// <summary>
        /// This event is raised when the user clicks the 'Add Response Code' button.
        /// The method facilitates creation of an additional response code (link between a HTTP response code and an
        /// associated data type).
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void AddResponseCode_Click(object sender, EventArgs e)
        {
            RESTOperationResultDeclaration result = this._collection.AddOperationResult();
            if (result != null && result.Status != RESTOperationResultDeclaration.DeclarationStatus.Invalid)
            {
                ListViewItem newItem = new ListViewItem(result.ResultCode);
                newItem.SubItems.Add(result.Description);
                ResponseCodeList.Items.Add(newItem);
            }
        }

        /// <summary>
        /// This event is raised when the user selects a response code for deletion.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void DeleteResponseCode_Click(object sender, EventArgs e)
        {
            if (ResponseCodeList.SelectedItems.Count > 0)
            {
                ListViewItem key = ResponseCodeList.SelectedItems[0];
                ContextSlt context = ContextSlt.GetContextSlt();
                this._collection.DeleteOperationResult(key.Text);
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
                ContextSlt context = ContextSlt.GetContextSlt();
                string originalKey = key.Text;
                RESTOperationResultDeclaration result = this._collection.EditOperationResult(key.Text);
                if (result != null)
                {
                    key.SubItems[0].Text = result.ResultCode;
                    key.SubItems[1].Text = result.Description;
                }
            }
        }

        /// <summary>
        /// This event is raised when the user leaves the collection name field. It checks whether the field has some
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
    }
}
