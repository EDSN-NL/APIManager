using System.Collections.Generic;
using Plugin.Application.Forms;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// Maintains a list of Collections for the given service and facilitates creation/edit/retrieval of such Collections.
    /// This class is used solely for the management of "predefined" lists collections, independent of use within operations.
    /// There exists two types of collections: Global and Local. A Global list is defined across multiple API's and can be considered
    /// an "ECDM Template" for collection items. Local lists have API scope and must be defined explicitly at API level. These can not be
    /// re-used across API's.
    /// Since collections can be of many types, this class acts as the generic base-class for different types of collections and is thus 
    /// implemented as an abstract class.
    /// Since retrieval of global template classes could be an expensive operation, these are maintained in a single static list. Each specialized
    /// collection must pass a 'Token' that is used as an additional key to retrieve the specific collections from this static list. The key
    /// for this list is thus constructed as 'token'-'name'.
    /// </summary>
    internal abstract class RESTCollectionMgr
    {
        private const string _TokenSeparator    = "-";

        // Generic prefixes that can be used by derived classes to communicate scope to users...
        protected const string _APIPrefix       = "API: ";
        protected const string _GlobalPrefix    = "Global: ";
            
        // The API-specific list is maintained on a per-instance basis and the key is the collection name.
        // The global list is maintained for all instances together (static list) and the key must contain a derived-class specific
        // token. Key construction is 'token'-'name'.
        private SortedList<string, RESTCollection> _collectionListAPI = null;               // List of API-based collections.
        private static SortedList<string, RESTCollection> _collectionListGlobal = null;     // Static list of all 'global' collections.
        private RESTService _myService = null;                                              // Service for which we maintain the API-based list.

        /// <summary>
        /// Returns the combined list of all collection names (global- and API-scope) that are managed by this collection manager.
        /// Each name receives a prefix that indicates the scope. List could be empty in case no collections are managed at all.
        /// </summary>
        internal List<string> CollectionNames
        {
            get { return GetCollectionNames(); }
        }

        /// <summary>
        /// Returns the service that 'owns' the collections managed by this Collection Manager.
        /// </summary>
        internal RESTService OwningService { get { return _myService; } }

        /// <summary>
        /// Facilitates creation of new collections and/or changing existing collections.
        /// </summary>
        /// <returns>Newly created collection or NULL in case of user cancel.</returns>
        internal abstract RESTCollection AddCollection();

        /// <summary>
        /// Removes the collection with the specified name (and does nothing when the name does not exist in the list).
        /// </summary>
        /// <param name="thisCollection">Name of the collection that must be deleted.</param>
        /// <param name="scope">Scope of the collection to be deleted.</param>
        internal abstract void DeleteCollection(string thisCollection, RESTCollection.CollectionScope scope);

        /// <summary>
        /// Presents an edit dialog that facilitates modification of an existing collection.
        /// </summary>
        /// <param name="collectionName">Name of the collection to be modified; must exist.</param>
        /// <param name="scope">Scope of the collection to be modified, the scope itself can NOT be changed!</param>
        /// <returns>The name of the modified collection (might have been changed as a result of the edit operation).</returns>
        internal abstract string EditCollection(string collectionName, RESTCollection.CollectionScope scope);

        /// <summary>
        /// Returns the combined list of all collection names (global- and API-scope) that are managed by this collection manager.
        /// Each name receives a prefix that indicates the scope.
        /// </summary>
        /// <returns>List of scope-prefixed collection names (could be an empty list).</returns>
        protected abstract List<string> GetCollectionNames();

        /// <summary>
        /// Create a new Collection Manager on behalf of the specified service. When this service is NULL, we're dealing with a 
        /// global collection set only!
        /// </summary>
        /// <param name="thisService">Optional Service for which we have to maintain the collection.</param>
        internal RESTCollectionMgr(RESTService thisService)
        {
            this._myService = thisService;
            this._collectionListAPI = new SortedList<string, RESTCollection>();
            if (_collectionListGlobal == null) _collectionListGlobal = new SortedList<string, RESTCollection>();
        }

        /// <summary>
        /// Entry point for management of the collection, using a user dialog. Facilitates add-, delete- or update of 
        /// collections.
        /// </summary>
        internal void ManageCollection()
        {
            using (var dialog = new RESTCollectionSetEdit(this)) dialog.ShowDialog();
        }

        /// <summary>
        /// Removes the collection with the specified name (and does nothing when the name does not exist in the list).
        /// </summary>
        /// <param name="token">Uniquely identifies specialized clas in case of global collections.</param>
        /// <param name="thisCollection">Name of the collection that must be deleted.</param>
        /// <param name="scope">Scope of the collection to be deleted.</param>
        protected void DeleteCollection(string token, string thisCollection, RESTCollection.CollectionScope scope)
        {
            if (scope == RESTCollection.CollectionScope.Global)
            {
                string key = token + _TokenSeparator + thisCollection;
                if (_collectionListGlobal.ContainsKey(key))
                {
                    _collectionListGlobal[key].DeleteCollection();
                    _collectionListGlobal.Remove(key);
                }
            }
            else if (scope == RESTCollection.CollectionScope.API)
            {
                if (this._collectionListAPI.ContainsKey(thisCollection))
                {
                    _collectionListAPI[thisCollection].DeleteCollection();
                    _collectionListAPI.Remove(thisCollection);
                }
            }
        }

        /// <summary>
        /// Used by derived classes to retrieve a collection from the either the API- or Global list.
        /// </summary>
        /// <param name="token">Identifies the specialized class in case of retrieval from the Global list.</param>
        /// <param name="collectionName">Name of collection to be retrieved.</param>
        /// <param name="scope">Identifies the scope of the collection as a user-readable string.</param>
        /// <returns>Collection object or NULL when not found.</returns>
        protected RESTCollection GetCollection(string token, string collectionName, RESTCollection.CollectionScope scope)
        {
            if (scope == RESTCollection.CollectionScope.Global)
            {
                string key = token + _TokenSeparator + collectionName;
                if (_collectionListGlobal.ContainsKey(key)) return _collectionListGlobal[key];
            }
            else if (scope == RESTCollection.CollectionScope.API)
            {
                if (this._collectionListAPI.ContainsKey(collectionName)) return this._collectionListAPI[collectionName];
            }
            return null;
        }

        /// <summary>
        /// Used by specialized classes to return 'their' subset of the global collections list or the API-list.
        /// If scope is not API or Global, the function does not perform any operations!
        /// </summary>
        /// <param name="token">Token identifying specialized class.</param>
        /// <param name="scope">Identifies the list to be retrieved.</param>
        /// <returns>List of global collections for this particular token.</returns>
        protected List<RESTCollection> GetCollections(string token, RESTCollection.CollectionScope scope)
        {
            List<RESTCollection> list = new List<RESTCollection>();
            if (scope == RESTCollection.CollectionScope.Global)
            {
                foreach (string key in _collectionListGlobal.Keys) if (key.Contains(token)) list.Add(_collectionListGlobal[key]);
            }
            else if (scope == RESTCollection.CollectionScope.API)
            {
                foreach (RESTCollection entry in _collectionListAPI.Values) list.Add(entry);
            }
            return list;
        }

        /// <summary>
        /// Used by specialized classes to return 'their' number of entries in either the API- or Global collection list.
        /// If scope is not API or Global, the function does not perform any operations!
        /// </summary>
        /// <param name="token">Token identifying specialized class.</param>
        /// <param name="scope">Identifies the list for which we want to retrieve the count.</param>
        /// <returns>List of global collections for this particular token.</returns>
        protected int GetCollectionCount(string token, RESTCollection.CollectionScope scope)
        {
            int counter = 0;
            if (scope == RESTCollection.CollectionScope.Global)
            {
                foreach (string key in _collectionListGlobal.Keys) if (key.Contains(token)) counter++;
            }
            else if (scope == RESTCollection.CollectionScope.API)
            {
                counter = _collectionListAPI.Count;
            }
            return counter;
        }

        /// <summary>
        /// Used by specialized classes to return 'their' list of collection names from the collection identified by 'scope'.
        /// </summary>
        /// <param name="token">Token identifying specialized class.</param>
        /// <param name="scope">Identifies which list of names to be retrieved.</param>
        /// <returns>List of names for specified collection.</returns>
        protected List<string> GetNames(string token, RESTCollection.CollectionScope scope)
        {
            List<string> keys = new List<string>();
            if (scope == RESTCollection.CollectionScope.Global)
            {
                foreach (string key in _collectionListGlobal.Keys)
                    if (key.Contains(token))
                    {
                        keys.Add(key.Substring(key.IndexOf(_TokenSeparator) + 1));
                    }
            }
            else if (scope == RESTCollection.CollectionScope.API)
            {
                foreach (string key in _collectionListAPI.Keys) keys.Add(key);
            }
            return keys;
        }

        /// <summary>
        /// Used by derived classes to check whether the collection set contains the specified collection for a given scope.
        /// </summary>
        /// <param name="token">Token identifying specialized class.</param>
        /// <param name="collectionName">Name of collection to be checked.</param>
        /// <param name="scope">Identifies the scope of the collection.</param>
        /// <returns>True on successfull insert, false when collection already exists.</returns>
        protected bool HasCollection(string token, string collectionName, RESTCollection.CollectionScope scope)
        {
            if (scope == RESTCollection.CollectionScope.Global)
            {
                string key = token + _TokenSeparator + collectionName;
                return _collectionListGlobal.ContainsKey(key);
            }
            else if (scope == RESTCollection.CollectionScope.API)
            {
                return this._collectionListAPI.ContainsKey(collectionName);
            }
            return false;
        }

        /// <summary>
        /// Used by derived classes to insert a collection in the set during initial load.
        /// </summary>
        /// <param name="token">Token identifying specialized class.</param>
        /// <param name="scope">Identifies the scope of the collection as a user-readable string.</param>
        /// <param name="newCollection">Collection to be added, must NOT yet exist in the collection!</param>
        /// <returns>True on successfull insert, false when collection already exists.</returns>
        protected bool InsertCollection(string token, RESTCollection.CollectionScope scope, RESTCollection newCollection)
        {
            bool result = false;
            if (scope == RESTCollection.CollectionScope.Global)
            {
                string key = token + _TokenSeparator + newCollection.Name;
                if (!_collectionListGlobal.ContainsKey(key))
                {
                    _collectionListGlobal.Add(key, newCollection);
                    result = true;
                }
            }
            else if (scope == RESTCollection.CollectionScope.API)
            {
                if (!this._collectionListAPI.ContainsKey(newCollection.Name))
                {
                    this._collectionListAPI.Add(newCollection.Name, newCollection);
                    result = true;
                }
            }
            return result;
        }

        /// <summary>
        /// Used by derived classes to replace an existing collection by a new one. If the new collection has a different name,
        /// we first remove the entry before adding a new entry. If the names are identical, the new collection is added with
        /// the same key.
        /// </summary>
        /// <param name="token">Token identifying specialized class.</param>
        /// <param name="scope">Identifies the scope of the collection as a user-readable string.</param>
        /// <param name="oldName">The name of the collection to replace. If the collection could not be found, the
        /// operation acts like an insert. </param>
        /// <param name="newCollection">Collection to be added, might have the same name as the 'to be replaced' collection.</param>
        /// <returns>True on successfull insert, false when collection already exists.</returns>
        protected void ReplaceCollection(string token, RESTCollection.CollectionScope scope, string oldName, RESTCollection newCollection)
        {
            if (scope == RESTCollection.CollectionScope.Global)
            {
                string newKey = token + _TokenSeparator + newCollection.Name;
                string oldKey = token + _TokenSeparator + oldName;
                if (_collectionListGlobal.ContainsKey(oldKey))
                {
                    if (oldName == newCollection.Name) _collectionListGlobal[newKey] = newCollection;
                    else
                    {
                        _collectionListGlobal.Remove(oldKey);
                        _collectionListGlobal.Add(newKey, newCollection);
                    }
                }
                else _collectionListGlobal.Add(newKey, newCollection);
            }
            else if (scope == RESTCollection.CollectionScope.API)
            {
                if (this._collectionListAPI.ContainsKey(oldName))
                {
                    if (oldName == newCollection.Name) this._collectionListAPI[oldName] = newCollection;
                    else
                    {
                        this._collectionListAPI.Remove(oldName);
                        this._collectionListAPI.Add(newCollection.Name, newCollection);
                    }
                }
                else this._collectionListAPI.Add(newCollection.Name, newCollection);
            }
        }
    }
}
