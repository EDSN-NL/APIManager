using System;
using System.Windows.Forms;
using Plugin.Application.CapabilityModel.API;
using Framework.Util;

namespace Plugin.Application.Forms
{
    /// <summary>
    /// This dialog facilitates add-, delete- or update of collections.
    /// </summary>
    internal partial class RESTCollectionSetEdit : Form
    {
        private RESTCollectionMgr _manager;

        // Separates scope from name in collections...
        private const string _ScopeSeparator = ": ";

        /// <summary>
        /// This dalog presents the list of Response Code Collections for the specified manager and facilitates add-, delete- or update operations
        /// on that set.
        /// </summary>
        /// <param name="setManager">The Collection Set Manager used to manage the created lists.</param>
        internal RESTCollectionSetEdit(RESTCollectionMgr setManager)
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
            RESTCollection newCollection = this._manager.AddCollection();
            if (newCollection != null)
                CollectionListFld.Items.Add(EnumConversions<RESTCollection.CollectionScope>.EnumToString(newCollection.Scope) + 
                                            _ScopeSeparator + newCollection.Name);
        }

        /// <summary>
        /// This event is raised when the user clicks the 'delete collection' button (or menu entry).
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void DeleteCollection_Click(object sender, EventArgs e)
        {
            string selectedName = CollectionListFld.SelectedItem as string;
            if (!string.IsNullOrEmpty(selectedName))
            {
                string scope = selectedName.Substring(0, selectedName.IndexOf(_ScopeSeparator));
                string collection = selectedName.Substring(selectedName.IndexOf(_ScopeSeparator) + _ScopeSeparator.Length);
                this._manager.DeleteCollection(collection, EnumConversions<RESTCollection.CollectionScope>.StringToEnum(scope));
                CollectionListFld.Items.Remove(selectedName);
            }
        }

        /// <summary>
        /// This event is raised when the user clicks the 'edit collection' button (or menu entry).
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void EditCollection_Click(object sender, EventArgs e)
        {
            string selectedName = CollectionListFld.SelectedItem as string;
            if (!string.IsNullOrEmpty(selectedName))
            {
                string scope = selectedName.Substring(0, selectedName.IndexOf(_ScopeSeparator));
                string collection = selectedName.Substring(selectedName.IndexOf(_ScopeSeparator) + _ScopeSeparator.Length);
                string updatedName = this._manager.EditCollection(collection, EnumConversions<RESTCollection.CollectionScope>.StringToEnum(scope));
                if (updatedName != string.Empty)
                {
                    updatedName = scope + _ScopeSeparator + updatedName;
                    if (updatedName != selectedName)
                    {
                        CollectionListFld.Items.Remove(selectedName);
                        CollectionListFld.Items.Add(updatedName);
                    }
                }
            }
        }
    }
}
