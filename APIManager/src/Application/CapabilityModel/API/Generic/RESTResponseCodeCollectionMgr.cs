using System.Windows.Forms;
using System.Collections.Generic;
using Framework.Model;
using Framework.Context;
using Framework.Util;
using Framework.Logging;
using Plugin.Application.Forms;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// Maintains a list of Response Code Collections for the given service and facilitates creation/edit/retrieval of Response Code Collections.
    /// This class is used solely for the management of "predefined" lists of response codes, independent of use within operations.
    /// There exists two types of response code lists: Global and Local. A Global list is defined across multiple API's and can be considered
    /// an "ECDM Template" for response codes. Local lists have API scope and must be defined explicitly at API level. These can not be
    /// re-used across API's.
    /// </summary>
    internal sealed class RESTResponseCodeCollectionMgr
    {
        // Configuration properties used by this module:
        private const string _RESTResponseCodeCollectionStereotype      = "RESTResponseCodeCollectionStereotype";
        private const string _RESTResponseCodeCollectionsPathName       = "RESTResponseCodeCollectionsPathName";
        private const string _RESTResponseCodeCollectionsPkgName        = "RESTResponseCodeCollectionsPkgName";

        private const string _ScopeSeparator = ": ";

        // The key into the list must have format <scope>:<name>, in which 'scope' is one of 'Global' or 'API'...
        private static SortedList<string, RESTResponseCodeCollection> _collectionList = null;   // List of collections.
        private static Service _myService = null;                                               // Service for which we maintain the list.

        /// <summary>
        /// Returns a list of all the collection names managed by this Response Code Collection Manager.
        /// The returned key lists have format SCOPE: NAME.
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
                ContextSlt context = ContextSlt.GetContextSlt();
                string APIString = EnumConversions<RESTResponseCodeCollection.CollectionScope>.EnumToString(RESTResponseCodeCollection.CollectionScope.API);
                string GlobalString = EnumConversions<RESTResponseCodeCollection.CollectionScope>.EnumToString(RESTResponseCodeCollection.CollectionScope.Global);
                string globalCollectionPath = context.GetConfigProperty(_RESTResponseCodeCollectionsPathName);
                string globalCollectionPkgName = context.GetConfigProperty(_RESTResponseCodeCollectionsPkgName);

                // Create a collection manager for another service, we have to switch contexts...
                _myService = thisService;
                _collectionList = new SortedList<string, RESTResponseCodeCollection>();
                string collectionStereotype = context.GetConfigProperty(_RESTResponseCodeCollectionStereotype);
                foreach (MEClass collection in thisService.ModelPkg.FindClasses(null, collectionStereotype))
                {
                    _collectionList.Add(APIString + _ScopeSeparator + collection.Name, new RESTResponseCodeCollection(collection));
                }

                // Check whether we have 'global' collections and if so, add them to our list...
                MEPackage globalCollections = ModelSlt.GetModelSlt().FindPackage(globalCollectionPath, globalCollectionPkgName);
                if (globalCollections != null)
                {
                    foreach (MEClass collection in globalCollections.FindClasses(null, collectionStereotype))
                    {
                        _collectionList.Add(GlobalString + _ScopeSeparator + collection.Name, new RESTResponseCodeCollection(collection));
                    }
                }
                else Logger.WriteWarning("Could not find package '" + globalCollectionPath + ":" + globalCollectionPkgName + "', skipping global response code collections!");
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
        /// <returns>Newly created collection or NULL in case of user cancel.</returns>
        internal RESTResponseCodeCollection AddCollection()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string globalCollectionPath = context.GetConfigProperty(_RESTResponseCodeCollectionsPathName);
            string globalCollectionPkgName = context.GetConfigProperty(_RESTResponseCodeCollectionsPkgName);
            MEPackage globalCollections = ModelSlt.GetModelSlt().FindPackage(globalCollectionPath, globalCollectionPkgName);

            if (globalCollections == null)
            {
                Logger.WriteWarning("Could not find package '" + globalCollectionPath + ":" + globalCollectionPkgName +
                                    "', ignoring global response code collections!");
            }

            RESTResponseCodeCollectionEdit editDialog = new RESTResponseCodeCollectionEdit(null);
            if (editDialog.ShowDialog() == DialogResult.OK)
            {
                string prefix = EnumConversions<RESTResponseCodeCollection.CollectionScope>.EnumToString(editDialog.Scope);
                if (_collectionList.ContainsKey(prefix + _ScopeSeparator + editDialog.CollectionName))
                {
                    MessageBox.Show("Duplicate collection name, please try again!",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    // In case of an 'add new', the user has selected the appropriate scope of the new collection....
                    if (editDialog.Scope == RESTResponseCodeCollection.CollectionScope.Global && globalCollections != null)
                         editDialog.Collection.SetScope(globalCollections, RESTResponseCodeCollection.CollectionScope.Global);
                    else editDialog.Collection.SetScope(_myService.ModelPkg, RESTResponseCodeCollection.CollectionScope.API);
                    _collectionList.Add(prefix + _ScopeSeparator + editDialog.CollectionName, editDialog.Collection);
                }
            }
            return editDialog.Collection.Scope != RESTResponseCodeCollection.CollectionScope.Unknown? editDialog.Collection: null;
        }

        /// <summary>
        /// Removes the collection with the specified name (and does nothing when the name does not exist in the list).
        /// </summary>
        /// <param name="thisCollection">Name of the collection that must be deleted.</param>
        /// <param name="scope">Scope of the collection to be deleted.</param>
        internal void DeleteCollection(string thisCollection, RESTResponseCodeCollection.CollectionScope scope)
        {
            string prefix = EnumConversions<RESTResponseCodeCollection.CollectionScope>.EnumToString(scope);
            string key = prefix + _ScopeSeparator + thisCollection;
            if (_collectionList.ContainsKey(key))
            {
                RESTResponseCodeCollection collection = _collectionList[key];
                collection.DeleteResources();
                _collectionList.Remove(prefix + _ScopeSeparator + thisCollection);
            }
        }

        /// <summary>
        /// Presents an edit dialog that facilitates modification of an existing collection.
        /// </summary>
        /// <param name="collectionName">Name of the collection to be modified; must exist.</param>
        /// <param name="scope">Scope of the collection to be modified, the scope itself can NOT be changed!</param>
        /// <returns>The name of the modified collection (might have been changed as a result of the edit operation).</returns>
        internal string EditCollection(string collectionName, RESTResponseCodeCollection.CollectionScope scope)
        {
            RESTResponseCodeCollection thisCollection = null;
            string prefix = EnumConversions<RESTResponseCodeCollection.CollectionScope>.EnumToString(scope);
            string key = prefix + _ScopeSeparator + collectionName;
            if (_collectionList.ContainsKey(key)) thisCollection = _collectionList[key];
            else
            {
                MessageBox.Show("Specified collection '" + collectionName + "' with scope '" + scope + "' not found, please try again!",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }

            thisCollection.LockCollection();
            RESTResponseCodeCollectionEdit editDialog = new RESTResponseCodeCollectionEdit(thisCollection);
            if (editDialog.ShowDialog() == DialogResult.OK)
            {
                if (collectionName != string.Empty && editDialog.CollectionName != collectionName)
                {
                    if (_collectionList.ContainsKey(prefix + _ScopeSeparator + editDialog.CollectionName))
                    {
                        MessageBox.Show("Duplicate collection name, please try again!",
                                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return string.Empty;
                    }
                    _collectionList.Remove(key);
                    _collectionList.Add(prefix + _ScopeSeparator + editDialog.CollectionName, editDialog.Collection);
                }
                else _collectionList[prefix + _ScopeSeparator + editDialog.CollectionName] = editDialog.Collection;
                collectionName = editDialog.CollectionName;
                editDialog.Collection.Name = collectionName;
            }
            thisCollection.UnlockCollection();
            return collectionName;
        }

        /// <summary>
        /// Is invoked when the user wants to use the contents of a Response Code Collection. If only a single collection exists, the contents
        /// of this collection is returned as a list of OperationResultDeclaration objects. If multiple collections exist, the user is presented
        /// with a dialog from which to select the appropriate collection. When no collections exist (or the user does not select one), the
        /// function returns an empty list.
        /// </summary>
        /// <returns>List of Operation Result Declaration objects (can be empty).</returns>
        internal List<RESTOperationResultDescriptor> GetCollectionContents()
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
            return new List<RESTOperationResultDescriptor>();
        }
    }
}
