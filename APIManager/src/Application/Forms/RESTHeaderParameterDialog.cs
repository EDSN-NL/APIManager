using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Plugin.Application.CapabilityModel.API;

namespace Plugin.Application.Forms
{
    public partial class RESTHeaderParameterDialog : Form
    {
        private List<RESTHeaderParameterDescriptor> _resultSet;     // The list of parameters being edited.
        private RESTServiceHeaderParameterMgr _manager;             // The parameter manager that has invoked the dialog.
        private RESTServiceHeaderParameterMgr.Scope _scope;         // Indicates whether we're editing request- or response parameters.
        private RESTHeaderParameterCollectionMgr _headerManager;    // Header parameter manager.

        /// <summary>
        /// This property is used to return the created/updated parameter list from the dialog.
        /// </summary>
        internal List<RESTHeaderParameterDescriptor> ResultSet { get { return this._resultSet; } }

        /// <summary>
        /// Dialog constructor, initialises the REST Header Parameter create/edit dialog.
        /// </summary>
        /// <param name="parameter">The parameter declaration to use in the dialog.</param>
        /// This must be a list that is sorted by parameter name.</param>
        internal RESTHeaderParameterDialog(RESTServiceHeaderParameterMgr manager, RESTServiceHeaderParameterMgr.Scope scope, List<RESTHeaderParameterDescriptor> resultSet)
        {
            InitializeComponent();

            this.Text = scope == RESTServiceHeaderParameterMgr.Scope.Request ? "Edit request header parameters." : "Edit response header parameters.";
            this._resultSet = resultSet;
            this._manager = manager;
            this._scope = scope;
            this._headerManager = new RESTHeaderParameterCollectionMgr(manager.Service);

            // Load the parameters to be edited from the result set (could be empty)...
            // For each parameter in this list that is also in the 'left pane', we remove the item from the left pane and store the name
            // in the 'copy-right' list for reference.
            // Also, we check whether this parameter still exists in the API collection and when not, we remove the parameter from the result set.
            List<RESTHeaderParameterDescriptor> deleteList = new List<RESTHeaderParameterDescriptor>();
            foreach (RESTHeaderParameterDescriptor paramDesc in this._resultSet)
            {
                if (paramDesc.IsValid && manager.HasParameter(scope, paramDesc.Name))    // Only add the parameter when it still exists in the API collection!
                {
                    ListViewItem newItem = new ListViewItem(paramDesc.Name);
                    newItem.Name = paramDesc.Name;
                    newItem.SubItems.Add(paramDesc.Description);
                    SelectedHeaderList.Items.Add(newItem);
                }
                else deleteList.Add(paramDesc);     // Since the parameter is invalid or does not exist anymore, remove from the result set!
            }
            // We must perform the delete in two steps since you can't delete from a collection while iterating through it!
            foreach (RESTHeaderParameterDescriptor paramDesc in deleteList) this._resultSet.Remove(paramDesc);

            // Load the 'master set' of parameters from the manager (could be empty). We skip all names that are
            // already in the 'right pane'...
            foreach (RESTHeaderParameterDescriptor paramDesc in manager.GetCollection(scope).Collection)
            {
                if (paramDesc.IsValid)
                {
                    if (SelectedHeaderList.FindItemWithText(paramDesc.Name) == null)
                    {
                        ListViewItem newItem = new ListViewItem(paramDesc.Name);
                        newItem.Name = paramDesc.Name;
                        newItem.SubItems.Add(paramDesc.Description);
                        APIHeaderList.Items.Add(newItem);
                    }
                }
            }
        }

        /// <summary>
        /// This event is raised when the user clicks the 'Add API Header Parameter' button.
        /// The method facilitates creation of an additional API header parameter of the appropriate scope.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void AddAPIHeaderParm_Click(object sender, EventArgs e)
        {
            RESTHeaderParameterDescriptor parameter = this._manager.AddParameter(this._scope);
            if (parameter != null && parameter.IsValid)
            {
                ListViewItem newItem = new ListViewItem(parameter.Name);
                newItem.Name = parameter.Name;
                newItem.SubItems.Add(parameter.Description);
                APIHeaderList.Items.Add(newItem);
            }
        }

        /// <summary>
        /// This event is raised when the user selects one or more header parameters for deletion. It removes the headers from
        /// the API collection, which will makes them unavailable for all operations in the API.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void DeleteAPIHeaderParm_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in APIHeaderList.SelectedItems)
            {
                this._manager.DeleteParameter(this._scope, item.Text);
                APIHeaderList.Items.Remove(item);
            }
        }

        /// <summary>
        /// This event is raised when the user selects a header parameter for edit. Although the dialog accepts multiple
        /// selected items, we can edit only a single one at a time. So, when multiple selections exist, we take the first one.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void EditAPIHeaderParm_Click(object sender, EventArgs e)
        {
            if (APIHeaderList.SelectedItems.Count > 0)
            {
                ListViewItem key = APIHeaderList.SelectedItems[0];
                string originalKey = key.Text;
                RESTHeaderParameterDescriptor parameter = this._manager.EditParameter(this._scope, key.Text);
                if (parameter != null)
                {
                    key.SubItems[0].Text = parameter.Name;
                    key.SubItems[1].Text = parameter.Description;
                }
            }
        }

        /// <summary>
        /// This event is raised when the user clicks the 'use header collection' button. We return a (selected) parameter collection from
        /// the appropriate collection manager and copy all header parameters that do not yet exist in our current list.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void UseHeaderCollection_Click(object sender, EventArgs e)
        {
            foreach (RESTHeaderParameterDescriptor parameter in this._headerManager.GetCollectionContents())
            {
                if (APIHeaderList.FindItemWithText(parameter.Name) == null &&       // It's not in our left-pane...
                    SelectedHeaderList.FindItemWithText(parameter.Name) == null &&  // And it's not in our right-pane...
                    this._manager.AddParameter(this._scope, parameter) != null)     // And it's not already in the collection...
                {
                    ListViewItem newItem = new ListViewItem(parameter.Name);
                    newItem.Name = parameter.Name;
                    newItem.SubItems.Add(parameter.Description);
                    APIHeaderList.Items.Add(newItem);
                }
            }
        }

        /// <summary>
        /// This event is raised when the user clicks the 'edit header param collections' button. This brings up a subsequent dialog,
        /// which facilitates create-, delete- or edit of header parameter collections.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void EditHeaderCollections_Click(object sender, EventArgs e)
        {
            this._headerManager.ManageCollection();
        }

        /// <summary>
        /// Selects one or more parameters from the 'left pane' and moves them to the 'right pane', i.e. adds the parameters to the 
        /// result set. We don't show the parameter in the left pane anymore to avoid confusion.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void SelectParameters_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in APIHeaderList.SelectedItems)
            {
                RESTHeaderParameterDescriptor parameter = this._manager.GetParameter(this._scope, item.Text);
                if (parameter != null)
                {
                    this._resultSet.Add(parameter);
                    APIHeaderList.Items.Remove(item);
                    SelectedHeaderList.Items.Add(item);
                }
            }
        }

        /// <summary>
        /// Selects one or more parameters from the 'right pane' and moves them back to the 'left pane', i.e. removes the parameters from the 
        /// result set. We don't show the parameter in the right pane anymore to avoid confusion.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void UnselectParameters_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in SelectedHeaderList.SelectedItems)
            {
                RESTHeaderParameterDescriptor parameter = this._manager.GetParameter(this._scope, item.Text);
                if (parameter != null)
                {
                    this._resultSet.Remove(parameter);
                    SelectedHeaderList.Items.Remove(item);
                    APIHeaderList.Items.Add(item);
                }
            }
        }
    }
}
