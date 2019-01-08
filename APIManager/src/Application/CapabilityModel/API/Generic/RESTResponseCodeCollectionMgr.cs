using System.Windows.Forms;
using System.Collections.Generic;
using Framework.Model;
using Framework.Context;
using Plugin.Application.Forms;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// Maintains a list of Response Code Collections for the given service and facilitates creation/edit/retrieval of Response Code Collections.
    /// </summary>
    internal sealed class RESTResponseCodeCollectionMgr
    {
        // Configuration properties used by this module:
        private const string _RESTResponseCodeCollectionStereotype = "RESTResponseCodeCollectionStereotype";

        private static SortedList<string, RESTResponseCodeCollection> _collectionList = null;   // List of collections.
        private static Service _myService = null;                                               // Service for which we maintain the list.

        /// <summary>
        /// Returns a list of all the collection names managed by this Response Code Collection Manager.
        /// </summary>
        internal IList<string> CollectionNames { get { return _collectionList.Keys; } }

        /// <summary>
        /// Returns the service that 'owns' the collections managed by this Collection Manager.
        /// </summary>
        internal Service OwningService { get { return _myService; } }

        /// <summary>
        /// Create a new Collection Manager on behalf of the specified service. To increase efficiency, we keep the actual collection as a 
        /// static list, which implies that it exists as long as the process lives. This also implies that the user might switch
        /// between services within a session. Therefor, we also keep a static service reference and check whether we pass a new
        /// service, in which case we have to reload the collection list.
        /// The list itself is safe to keep in memory, since the user should not manually change the contents of the collection classes!
        /// </summary>
        /// <param name="thisService">Service for which we have to maintain the collection.</param>
        internal RESTResponseCodeCollectionMgr(Service thisService)
        {
            if (_myService == null || thisService.ServiceClass.GlobalID != _myService.ServiceClass.GlobalID)
            {
                // Create a collection manager for another service, we have to switch contexts...
                _myService = thisService;
                _collectionList = new SortedList<string, RESTResponseCodeCollection>();
                ContextSlt context = ContextSlt.GetContextSlt();
                string collectionStereotype = context.GetConfigProperty(_RESTResponseCodeCollectionStereotype);
                foreach (MEClass collection in thisService.ModelPkg.FindClasses(null, collectionStereotype))
                {
                    _collectionList.Add(collection.Name, new RESTResponseCodeCollection(collection, thisService));
                }
            }
        }

        /// <summary>
        /// Entry point for management of the collection, using a user dialog. Facilitates add-, delete- or update of 
        /// collections.
        /// </summary>
        internal void ManageCollection()
        {
            RESTResponseCodeCollectionSetEdit dialog = new RESTResponseCodeCollectionSetEdit(this);
            dialog.ShowDialog();
        }

        /// <summary>
        /// Facilitates creation of new collections and/or changing existing collections.
        /// </summary>
        internal string AddCollection()
        {
            string collectionName = string.Empty;
            RESTResponseCodeCollection newCollection = new RESTResponseCodeCollection(_myService);
            RESTResponseCodeCollectionEdit editDialog = new RESTResponseCodeCollectionEdit(newCollection);
            if (editDialog.ShowDialog() == DialogResult.OK)
            {
                if (_collectionList.ContainsKey(editDialog.CollectionName))
                {
                    MessageBox.Show("Duplicate collection name, please try again!",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    _collectionList.Add(editDialog.CollectionName, editDialog.Collection);
                    collectionName = editDialog.CollectionName;
                }
            }
            return collectionName;
        }

        /// <summary>
        /// Removes the collection with the specified name (and does nothing when the name does not exist in the list).
        /// </summary>
        internal void DeleteCollection(string thisCollection)
        {
            if (_collectionList.ContainsKey(thisCollection)) _collectionList.Remove(thisCollection);
        }

        /// <summary>
        /// Presents an edit dialog that facilitates modification of an existing collection.
        /// </summary>
        /// <returns>The name of the modified collection (might have been changed as a result of the edit operation).</returns>
        internal string EditCollection(string collectionName)
        {
            RESTResponseCodeCollection thisCollection = null;
            if (_collectionList.ContainsKey(collectionName)) thisCollection = _collectionList[collectionName];
            else
            {
                MessageBox.Show("Specified collection '" + collectionName + "' not found, please try again!",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }

            RESTResponseCodeCollectionEdit editDialog = new RESTResponseCodeCollectionEdit(thisCollection);
            if (editDialog.ShowDialog() == DialogResult.OK)
            {
                if (collectionName != string.Empty && editDialog.CollectionName != collectionName)
                {
                    if (_collectionList.ContainsKey(editDialog.CollectionName))
                    {
                        MessageBox.Show("Duplicate collection name, please try again!",
                                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return string.Empty;
                    }
                    _collectionList.Remove(collectionName);
                    _collectionList.Add(editDialog.CollectionName, editDialog.Collection);
                }
                else _collectionList[editDialog.CollectionName] = editDialog.Collection;
                collectionName = editDialog.CollectionName;
            }
            return collectionName;
        }

        /// <summary>
        /// Is invoked when the user wants to use the contents of a Response Code Collection. If only a single collection exists, the contents
        /// of this collection is returned as a list of OperationResultDeclaration objects. If multiple collections exist, the user is presented
        /// with a dialog from which to select the appropriate collection. When no collections exist (or the user does not select one), the
        /// function returns an empty list.
        /// </summary>
        /// <returns>List of Operation Result Declaration objects (can be empty).</returns>
        internal List<RESTOperationResultDeclaration> GetCollectionContents()
        {
            if (_collectionList.Count > 0)
            {
                if (_collectionList.Count == 1) return _collectionList.Values[0].Collection;
                else
                {
                    RESTOperationResultCodeCollectionPicker dialog = new RESTOperationResultCodeCollectionPicker(_collectionList.Keys);
                    if (dialog.ShowDialog() == DialogResult.OK) return _collectionList[dialog.SelectedCollection].Collection;
                }
            }
            return new List<RESTOperationResultDeclaration>();
        }
    }
}
