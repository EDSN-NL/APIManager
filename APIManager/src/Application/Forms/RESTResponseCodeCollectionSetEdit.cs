using System;
using System.Windows.Forms;
using Plugin.Application.CapabilityModel.API;

namespace Plugin.Application.Forms
{
    /// <summary>
    /// This dialog facilitates add-, delete- or update of response code collections.
    /// </summary>
    internal partial class RESTResponseCodeCollectionSetEdit : Form
    {
        private RESTResponseCodeCollectionMgr _manager;

        /// <summary>
        /// This dalog presents the list of Response Code Collections for the specified manager and facilitates add-, delete- or update operations
        /// on that set.
        /// </summary>
        /// <param name="setManager">The Collection Set Manager used to manage the created lists.</param>
        internal RESTResponseCodeCollectionSetEdit(RESTResponseCodeCollectionMgr setManager)
        {
            InitializeComponent();
            this._manager = setManager;
            foreach (string name in setManager.CollectionNames) CollectionListFld.Items.Add(name);

            // Assign context menus to the appropriate controls...
            CollectionListFld.ContextMenuStrip = CollectionListMenuStrip;
        }

        /// <summary>
        /// This event is raised when the user clicks the 'add collection' button (or menu entry).
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void AddCollection_Click(object sender, EventArgs e)
        {
            string collectionName = this._manager.AddCollection();
            if (collectionName != string.Empty) CollectionListFld.Items.Add(collectionName);
        }

        /// <summary>
        /// This event is raised when the user clicks the 'delete collection' button (or menu entry).
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void DeleteCollection_Click(object sender, EventArgs e)
        {
            string selectedName = CollectionListFld.SelectedItem as string;
            this._manager.DeleteCollection(selectedName);
            CollectionListFld.Items.Remove(selectedName);
        }

        /// <summary>
        /// This event is raised when the user clicks the 'edit collection' button (or menu entry).
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void EditCollection_Click(object sender, EventArgs e)
        {
            string selectedName = CollectionListFld.SelectedItem as string;
            string updatedName = this._manager.EditCollection(selectedName);
            if (updatedName != string.Empty && updatedName != selectedName)
            {
                CollectionListFld.Items.Remove(selectedName);
                CollectionListFld.Items.Add(updatedName);
            }
        }
    }
}
