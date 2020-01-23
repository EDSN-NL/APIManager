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
    /// Maintains a list of Header Parameter Collections for the given service and facilitates creation/edit/retrieval of such Collections.
    /// This class is used solely for the management of "predefined" lists of header parameter collections, independent of use within operations.
    /// There exists two types of collections: Global and Local. A Global list is defined across multiple API's and can be considered
    /// an "ECDM Template" for header parameters. Local lists have API scope and must be defined explicitly at API level. These can not be
    /// re-used across API's.
    /// </summary>
    internal sealed class RESTHeaderParameterCollectionMgr: RESTCollectionMgr
    {
        // Unique key used to differentiate entries for this type in a global, static, collection...
        private const string _Token                                 = "HPCollection";

        // Configuration properties used by this module:
        private const string _HPCStereotype                         = "HPCStereotype";
        private const string _RESTHeaderParamCollectionsPathName    = "RESTHeaderParamCollectionsPathName";
        private const string _RESTHeaderParamCollectionsPkgName     = "RESTHeaderParamCollectionsPkgName";

        // Flag to force one-time load of global collections of this type...
        private static bool _mustInit                               = true;

        /// <summary>
        /// Create a new Collection Manager on behalf of the specified service. When this service is 'NULL', we're dealing with a
        /// global collection and we thus collect header parameters from our global packages only.
        /// </summary>
        /// <param name="thisService">Optional Service for which we have to maintain the collection.</param>
        internal RESTHeaderParameterCollectionMgr(Service thisService) : base(thisService)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string collectionStereotype = context.GetConfigProperty(_HPCStereotype);
            if (thisService != null)
            {
                foreach (MEClass collection in thisService.ModelPkg.FindClasses(null, collectionStereotype))
                {
                    InsertCollection(_Token, RESTCollection.CollectionScope.API, new RESTHeaderParameterCollection(null, collection));
                }
            }

            // Check whether we have 'global' collections and if so, add them to our list...
            if (_mustInit)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTHeaderParameterCollectionMgr >> Loading global collections...");
                string globalCollectionPath = context.GetConfigProperty(_RESTHeaderParamCollectionsPathName);
                string globalCollectionPkgName = context.GetConfigProperty(_RESTHeaderParamCollectionsPkgName);
                MEPackage globalCollections = ModelSlt.GetModelSlt().FindPackage(globalCollectionPath, globalCollectionPkgName);
                if (globalCollections != null)
                {
                    foreach (MEClass collection in globalCollections.FindClasses(null, collectionStereotype))
                    {
                        InsertCollection(_Token, RESTCollection.CollectionScope.Global, new RESTHeaderParameterCollection(null, collection));
                    }
                    _mustInit = false;
                }
                else Logger.WriteWarning("Could not find package '" + globalCollectionPath + ":" + globalCollectionPkgName + "', skipping global header parameter collections!");
            }
        }

        /// <summary>
        /// Facilitates creation of new collections and/or changing existing collections.
        /// </summary>
        /// <returns>Newly created collection or NULL in case of user cancel.</returns>
        internal override RESTCollection AddCollection()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string globalCollectionPath = context.GetConfigProperty(_RESTHeaderParamCollectionsPathName);
            string globalCollectionPkgName = context.GetConfigProperty(_RESTHeaderParamCollectionsPkgName);
            MEPackage globalCollections = ModelSlt.GetModelSlt().FindPackage(globalCollectionPath, globalCollectionPkgName);

            if (globalCollections == null)
            {
                Logger.WriteWarning("Could not find package '" + globalCollectionPath + ":" + globalCollectionPkgName +
                                    "', ignoring global response code collections!");
            }

            using (var editDialog = new RESTHeaderParameterCollectionEdit(null))
            {
                if (editDialog.ShowDialog() == DialogResult.OK)
                {
                    string prefix = EnumConversions<RESTCollection.CollectionScope>.EnumToString(editDialog.Scope);
                    if (HasCollection(_Token, editDialog.CollectionName, editDialog.Scope))
                    {
                        MessageBox.Show("Duplicate collection name, please try again!",
                                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        // In case of an 'add new', the user has selected the appropriate scope of the new collection....
                        if (editDialog.Scope == RESTCollection.CollectionScope.Global && globalCollections != null)
                            editDialog.Collection.Serialize(editDialog.CollectionName, globalCollections, RESTCollection.CollectionScope.Global);
                        else editDialog.Collection.Serialize(editDialog.CollectionName, OwningService.ModelPkg, RESTCollection.CollectionScope.API);
                        InsertCollection(_Token, editDialog.Scope, editDialog.Collection);
                    }
                }
                return editDialog.Collection.Scope != RESTCollection.CollectionScope.Unknown ? editDialog.Collection : null;
            }
        }

        /// <summary>
        /// Removes the collection with the specified name (and does nothing when the name does not exist in the list).
        /// </summary>
        /// <param name="thisCollection">Name of the collection that must be deleted.</param>
        /// <param name="scope">Scope of the collection to be deleted.</param>
        internal override void DeleteCollection(string thisCollection, RESTCollection.CollectionScope scope)
        {
            DeleteCollection(_Token, thisCollection, scope);    // Actual implementation is token-based in base class.
        }

        /// <summary>
        /// Presents an edit dialog that facilitates modification of an existing collection.
        /// </summary>
        /// <param name="collectionName">Name of the collection to be modified; must exist.</param>
        /// <param name="scope">Scope of the collection to be modified, the scope itself can NOT be changed!</param>
        /// <returns>The name of the modified collection (might have been changed as a result of the edit operation).</returns>
        internal override string EditCollection(string collectionName, RESTCollection.CollectionScope scope)
        {
            string scopeNm = EnumConversions<RESTCollection.CollectionScope>.EnumToString(scope);
            RESTHeaderParameterCollection thisCollection = GetCollection(_Token, collectionName, scope) as RESTHeaderParameterCollection;
            if (thisCollection == null)
            {
                MessageBox.Show("Specified collection '" + collectionName + "' with scope '" + scope + "' not found, please try again!",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }

            using (var editDialog = new RESTHeaderParameterCollectionEdit(thisCollection))
            {
                if (editDialog.ShowDialog() == DialogResult.OK)
                {
                    if (collectionName != string.Empty && editDialog.CollectionName != collectionName)
                    {
                        if (HasCollection(_Token, editDialog.CollectionName, scope))
                        {
                            MessageBox.Show("Duplicate collection name, please try again!",
                                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return string.Empty;
                        }
                        ReplaceCollection(_Token, scope, collectionName, editDialog.Collection);
                    }
                    else ReplaceCollection(_Token, scope, editDialog.CollectionName, editDialog.Collection);
                    collectionName = editDialog.CollectionName;
                }
                return collectionName;
            }
        }

        /// <summary>
        /// Is invoked when the user wants to use the contents of a Header Parameter Collection. If only a single collection exists, the contents
        /// of this collection is returned as a list of Header Parameter Descriptor objects. If multiple collections exist, the user is presented
        /// with a dialog from which to select the appropriate collection. When no collections exist (or the user does not select one), the
        /// function returns an empty list.
        /// </summary>
        /// <returns>List of Header Parameter Descriptor objects (can be empty).</returns>
        internal List<RESTHeaderParameterDescriptor> GetCollectionContents()
        {
            int APIcollectionCount = GetCollectionCount(_Token, RESTCollection.CollectionScope.API);
            int GlobalCollectionCount = GetCollectionCount(_Token, RESTCollection.CollectionScope.Global);
            if (APIcollectionCount + GlobalCollectionCount > 0)
            {
                if (APIcollectionCount == 1 && GlobalCollectionCount == 0)
                {
                    // One entry in the API collection set and no global collections...
                    var result = GetCollections(_Token, RESTCollection.CollectionScope.API)[0] as RESTHeaderParameterCollection;
                    return result.Collection != null ? result.Collection : new List<RESTHeaderParameterDescriptor>();
                }
                else if (APIcollectionCount == 0 && GlobalCollectionCount == 1)
                {
                    // One entry in the global collections and no API collections...
                    var result = GetCollections(_Token, RESTCollection.CollectionScope.Global)[0] as RESTHeaderParameterCollection;
                    return result.Collection != null ? result.Collection : new List<RESTHeaderParameterDescriptor>();
                }
                else
                {
                    // Multiple global- and API collections...
                    using (var dialog = new RESTCollectionPicker(GetCollectionNames()))
                    {
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            string key = dialog.SelectedCollection;
                            RESTCollection.CollectionScope scope;
                            if (key.Contains(_APIPrefix))
                            {
                                scope = RESTCollection.CollectionScope.API;
                                key = key.Substring(_APIPrefix.Length);
                            }
                            else
                            {
                                scope = RESTCollection.CollectionScope.Global;
                                key = key.Substring(_GlobalPrefix.Length);
                            }
                            return ((RESTHeaderParameterCollection)GetCollection(_Token, key, scope)).Collection;
                        }
                    }
                }
            }
            return new List<RESTHeaderParameterDescriptor>();
        }

        /// <summary>
        /// Returns the combined list of all collection names (global- and API-scope) that are managed by this collection manager.
        /// Each name receives a prefix that indicates the scope.
        /// </summary>
        protected override List<string> GetCollectionNames()
        {
            List<string> collections = new List<string>();
            foreach (string name in GetNames(_Token, RESTCollection.CollectionScope.API)) collections.Add(_APIPrefix + name);
            foreach (string name in GetNames(_Token, RESTCollection.CollectionScope.Global)) collections.Add(_GlobalPrefix + name);
            return collections;
        }
    }
}
