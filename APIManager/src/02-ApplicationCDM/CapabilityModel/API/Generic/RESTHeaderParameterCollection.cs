using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Framework.Model;
using Framework.Logging;
using Framework.Context;
using Framework.Util;
using Plugin.Application.Forms;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// Helper class that manages a collection of Header Parameter descriptors. These are used either as lists of predefined
    /// parameters that users can quickly re-assign to API's, or they are used to manage the list of either request- or 
    /// response header parameters for an API (to be used by operations to request operation-specific sub-sets).
    /// </summary>
    internal sealed class RESTHeaderParameterCollection: RESTCollection
    {
        // Configuration properties used by this module:
        private const string _HPCStereotype                         = "HPCStereotype";
        private const string _HPAStereotype                         = "HPAStereotype";
        private const string _PrimDataTypesPathName                 = "PrimDataTypesPathName";
        private const string _ServiceModelPkgName                   = "ServiceModelPkgName";
        private const string _ServiceOperationPkgStereotype         = "ServiceOperationPkgStereotype";
        private const string _ServiceModelPkgStereotype             = "ServiceModelPkgStereotype";
        private const string _HPAIDTag                              = "HPAIDTag";
        private const string _SentinelParameterName                 = "SentinelParameterName";

        // We keep two index structures in order to facilitate quick retrieval of header parameters either by name or by ID. Since the entries
        // are references to Parameter Descriptor objects, the overhead is quite small.
        private SortedList<string, RESTHeaderParameterDescriptor> _collectionByName;    // The actual collection, indexed by name.
        private SortedList<int, RESTHeaderParameterDescriptor> _collectionByID;         // The actual collection, indexed by parameter ID.
        private int _nextID;                                                            // This is the first available identifier for new parameters.
        private MEAttribute _sentinel;                                                  // We keep this around in order to quickly update sentinel value.

        /// <summary>
        /// Returns the list of Header Parameter Descriptors that comprises the collection.
        /// </summary>
        internal List<RESTHeaderParameterDescriptor> Collection { get { return new List<RESTHeaderParameterDescriptor>(this._collectionByName.Values); } }

        /// <summary>
        /// Returns true in case header parameters are present and should be used.
        /// </summary>
        internal bool UseHeaderParameters { get { return this._collectionByName.Count > 0; } }

        /// <summary>
        /// Either loads an existing collection with specified name from the specified package, or creates a new, empty, collection
        /// in the specified package. If we find an existing collection, the collection is build from the existing class.
        /// </summary>
        /// <param name="collectionName">Name to be assigned to the collection.</param>
        /// <param name="package">Package that must contain the collection. The location of the package determines the scope of the
        /// collection: if this is a ServiceModel package, the scope is 'API'. If the package is an Operation-type, the scope is 'Operation'
        /// and all others are considered 'Global'.</param>
        /// <exception cref="InvalidOperationException">Is thrown when we can't find the attribute classifier.</exception>
        internal RESTHeaderParameterCollection(string collectionName, MEPackage package): base(collectionName, package)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTHeaderParameterCollection >> Creating instance '" +
                             collectionName + "' in Package '" + package.Parent.Name + "/" + package.Name + "'...");

            ContextSlt context = ContextSlt.GetContextSlt();
            string collectionStereotype = context.GetConfigProperty(_HPCStereotype);
            MEClass collectionClass = package.FindClass(collectionName, collectionStereotype);
            if (collectionClass != null)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTHeaderParameterCollection >> Found existing collection class!");
                CreateCollectionFromClass(collectionClass);
            }
            else
            {
                // We could not find an existing class, so we create a new, empty, collection...
                try
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTHeaderParameterCollection >> No class yet, creating one...");
                    this._collectionByName = new SortedList<string, RESTHeaderParameterDescriptor>();
                    this._collectionByID = new SortedList<int, RESTHeaderParameterDescriptor>();
                    SetScope(CollectionScope.Global);
                    if (package.HasStereotype(context.GetConfigProperty(_ServiceOperationPkgStereotype))) SetScope(CollectionScope.Operation);
                    else if (package.Name == context.GetConfigProperty(_ServiceModelPkgName) &&
                             package.HasStereotype(context.GetConfigProperty(_ServiceModelPkgStereotype))) SetScope(CollectionScope.API);
                    LockCollection();
                    CreateCollectionClass(collectionName, collectionStereotype);
                    CreateSentinel(1);
                }
                finally { UnlockCollection(); }
            }
        }

        /// <summary>
        /// This constructor is called with an existing Collection class and initialises the collection from that class.
        /// </summary>
        /// <param name="collectionClass">Collection class.</param>
        /// <exception cref="InvalidOperationException">Thrown when a collection class is passed that is not of the correct stereotype or 
        /// when we can't find the correct attribute classifier.</exception>
        internal RESTHeaderParameterCollection(MEClass collectionClass): base(collectionClass)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTHeaderParameterCollection >> Creating instance from class '" +
                              collectionClass.Name + "'...");

            ContextSlt context = ContextSlt.GetContextSlt();
            if (collectionClass == null || !collectionClass.HasStereotype(context.GetConfigProperty(_HPCStereotype)))
            {
                string msg = "Attempt to create collection from class with wrong stereotype '" + (collectionClass != null? collectionClass.Name: "--NO CLASS--") + "', ignored!";
                Logger.WriteWarning(msg);
                throw new InvalidOperationException(msg);
            }
            CreateCollectionFromClass(collectionClass);
        }

        /// <summary>
        /// The default constructor is used only when we want to create new (template) collections. In this case, we want to assign owning
        /// package, name and contents iteratively.
        /// </summary>
        /// <param name="parent">For non-template collections, this is the parent resource owning the collection.</param>
        /// <param name="initialScope">Contains the initial scope for the collection.</param>
        internal RESTHeaderParameterCollection(CollectionScope initialScope = CollectionScope.Unknown): base(initialScope)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTHeaderParameterCollection >> Default constructor.");
            this._collectionByName = new SortedList<string, RESTHeaderParameterDescriptor>();
            this._collectionByID = new SortedList<int, RESTHeaderParameterDescriptor>();
            this._nextID = 1;
            this._sentinel = null;
        }

        /// <summary>
        /// This function is invoked to add a new header parameter to this collection. It displays the Parameter Dialog, which
        /// facilitates the user in creating a new parameter declaration. When it is indeed a new result, the created parameter is added to 
        /// the collection, otherwise the function does not perform any operations.
        /// </summary>
        /// <returns>Newly created descriptor or NULL in case of errors, duplicates or user cancel.</returns>
        internal RESTHeaderParameterDescriptor AddParameter()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            RESTParameterDeclaration newParam = new RESTParameterDeclaration();
            RESTHeaderParameterDescriptor newHdrParam = null;
            using (var dialog = new RESTParameterDialog(newParam))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (!this._collectionByName.ContainsKey(dialog.Parameter.Name))
                    {
                        newHdrParam = new RESTHeaderParameterDescriptor(this._nextID++, dialog.Parameter);
                        this._collectionByName.Add(newHdrParam.Name, newHdrParam);
                        this._collectionByID.Add(newHdrParam.ID, newHdrParam);
                        CreateAttributeFromDescriptor(newHdrParam);
                        
                        // Now that we have added a new parameter to the collection, we have to make sure to keep the Sentinel in sync.
                        // In theory, the Sentinal could even be absent (i.e. on new collections) so, if we can't find it, we create a new one.
                        if (this._sentinel != null) this._sentinel.SetTag(ContextSlt.GetContextSlt().GetConfigProperty(_HPAIDTag), this._nextID.ToString(), true);
                        else CreateSentinel(this._nextID);
                    }
                    else MessageBox.Show("Duplicate header parameter, please try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return newHdrParam;
        }

        /// <summary>
        /// Add a descriptor from a template to our current collection. If we already have a descriptor with the same code, the operation
        /// is ignored.
        /// </summary>
        /// <param name="template">Descriptor to be copied into our collection.</param>
        /// <returns>The newly added descriptor or null on duplicate.</returns>
        internal RESTHeaderParameterDescriptor AddParameter(RESTHeaderParameterDescriptor template)
        {
            RESTHeaderParameterDescriptor newHdrParam = null;
            if (template.IsValid)
            {
                if (!this._collectionByName.ContainsKey(template.Name))
                {
                    newHdrParam = new RESTHeaderParameterDescriptor(this._nextID++, template);
                    this._collectionByName.Add(newHdrParam.Name, newHdrParam);
                    this._collectionByID.Add(newHdrParam.ID, newHdrParam);
                    CreateAttributeFromDescriptor(newHdrParam);

                    // Now that we have added a new parameter to the collection, we have to make sure to keep the Sentinel in sync.
                    // In theory, the Sentinal could even be absent (i.e. on new collections) so, if we can't find it, we create a new one.
                    if (this._sentinel != null) this._sentinel.SetTag(ContextSlt.GetContextSlt().GetConfigProperty(_HPAIDTag), this._nextID.ToString(), true);
                    else CreateSentinel(this._nextID);

                }
                else Logger.WriteWarning("Attempt to add duplicate header parameter '" + template.Name + "' to collection '" + Name + "' ignored!");
            }
            return newHdrParam;
        }

        /// <summary>
        /// Creates a deep copy of the specified parameter sets to the current collection where all copied parameters keep their original ID. 
        /// When a collection class is already present, this is cleared and re-initialized. We also re-establish a new maximum ID number, 
        /// independent of the copied collection.
        /// </summary>
        /// <param name="other">The collection to copy from.</param>
        internal void CopyCollection(RESTHeaderParameterCollection other)
        {
            this._collectionByName = new SortedList<string, RESTHeaderParameterDescriptor>();
            this._collectionByID = new SortedList<int, RESTHeaderParameterDescriptor>();
            int maxID = 0;

            // If we already own a valid collection class, we remove all the attributes first and then replace them with the copied ones...
            // Note that this will also delete the Sentinel since we delete ALL attributes...
            if (CollectionClass != null) foreach (MEAttribute att in CollectionClass.Attributes) CollectionClass.DeleteAttribute(att);
            
            foreach (RESTHeaderParameterDescriptor otherDesc in other._collectionByName.Values)
            {
                RESTHeaderParameterDescriptor newEntry = new RESTHeaderParameterDescriptor(otherDesc.ID, otherDesc);
                this._collectionByName.Add(newEntry.Name, newEntry);
                this._collectionByID.Add(newEntry.ID, newEntry);
                if (newEntry.ID > maxID) maxID = newEntry.ID;
                CreateAttributeFromDescriptor(newEntry);
            }

            this._nextID = maxID + 1;
            CreateSentinel(this._nextID); 
        }

        /// <summary>
        /// This function is invoked when the entire collection has to be destroyed.
        /// the collection class is deleted and a new, empty, parameter list is created. 
        /// </summary>
        internal override void DeleteCollection()
        {
            base.DeleteCollection();                    // Removes the UML registration by deleting the class and clearing the name.
            this._collectionByName = new SortedList<string, RESTHeaderParameterDescriptor>();
            this._collectionByID = new SortedList<int, RESTHeaderParameterDescriptor>();
            this._nextID = 1;
            this._sentinel = null;
        }

        /// <summary>
        /// Deletes the header parameter with the specified name from the collection. Note that this has no effect on the Sentinel since
        /// removed ID's will not be re-used!
        /// </summary>
        /// <param name="name">Parameter to be deleted.</param>
        /// <returns>True when actually deleted the parameter, false when parameter was not found in the collection.</returns>
        internal bool DeleteParameter(string name)
        {
            if (!this._collectionByName.ContainsKey(name)) return false;
            if (CollectionClass != null)
            {
                foreach (MEAttribute attrib in CollectionClass.Attributes)
                {
                    if (attrib.Name == name)
                    {
                        LockCollection();
                        CollectionClass.DeleteAttribute(attrib);
                        UnlockCollection();
                        break;
                    }
                }
            }
            this._collectionByID.Remove(this._collectionByName[name].ID);
            this._collectionByName.Remove(name);
            return true;
        }

        /// <summary>
        /// This function is invoked to edit an existing header parameter. It displays the Header Parameter Dialog, which facilitates the user in 
        /// changing the result object. The updated object is added to the result list for this collection as long as
        /// it (still) has a unique name.
        /// </summary>
        /// <returns>Updated result record or NULL in case of errors or duplicates or user cancel.</returns>
        internal RESTHeaderParameterDescriptor EditParameter(string name)
        {
            RESTHeaderParameterDescriptor currentDesc = this._collectionByName.ContainsKey(name) ? this._collectionByName[name] : null;
            RESTHeaderParameterDescriptor newDesc = null;

            // If the descriptor is null, the attribute has probably been removed from the global collection and we should do the same.
            if (currentDesc != null)
            {
                // Create a (temporary) copy so we can properly detect changes...
                using (var dialog = new RESTParameterDialog(new RESTParameterDeclaration(currentDesc.Parameter)))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        newDesc = new RESTHeaderParameterDescriptor(currentDesc.ID, dialog.Parameter);
                        // If the name has not been changed, or changed to a still unique name, insert the new descriptor in the collection...
                        if (dialog.Parameter.Name == currentDesc.Name)
                        {
                            UpdateAttributeFromDescriptor(currentDesc, newDesc);
                            this._collectionByName[name] = newDesc;
                            this._collectionByID[newDesc.ID] = newDesc;
                        }
                        else if (!this._collectionByName.ContainsKey(dialog.Parameter.Name))
                        {
                            UpdateAttributeFromDescriptor(currentDesc, newDesc);
                            this._collectionByName.Remove(name);
                            this._collectionByID.Remove(newDesc.ID);
                            this._collectionByName.Add(newDesc.Name, newDesc);
                            this._collectionByID.Add(newDesc.ID, newDesc);
                        }
                        else // We have renamed the parameter to an existing parameter, error!
                        {
                            MessageBox.Show("Changing parameter name resulted in duplicate name, please try again!",
                                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }  
            return newDesc;
        }

        /// <summary>
        /// Searches the list of parameter descriptors and returns a list of all descriptors matching ID's from the IDList. This must be a comma-separated
        /// list of identifiers. All ID's that do not yield a value are simply skipped, so the caller has to check whether all requested ID's have indeed
        /// been returned.
        /// </summary>
        /// <param name="KeyList">List of parameter ID's.</param>
        /// <returns>List of parameter descriptors that match the specified ID.</returns>
        internal List<RESTHeaderParameterDescriptor> FindParametersByID(List<int> KeyList)
        {
            List<RESTHeaderParameterDescriptor> values = new List<RESTHeaderParameterDescriptor>();
            foreach (int key in KeyList)
            {
                try
                {
                    values.Add(this._collectionByID[key]);
                }
                catch (KeyNotFoundException) { /* Skip any missing keys. */ }
            }
            return values;
        }

        /// <summary>
        /// Returns a parameter descriptor for the parameter with the specified ID.
        /// </summary>
        /// <param name="parameterID">Identifier of parameter to retrieve.</param>
        /// <returns>Descriptor or NULL when not in collection.</returns>
        internal RESTHeaderParameterDescriptor GetParameter(int parameterID)
        {
            return this._collectionByID.ContainsKey(parameterID) ? this._collectionByID[parameterID] : null;
        }

        /// <summary>
        /// Returns a parameter descriptor for the parameter with the specified name.
        /// </summary>
        /// <param name="parameterName">Name of parameter to retrieve.</param>
        /// <returns>Descriptor or NULL when not in collection.</returns>
        internal RESTHeaderParameterDescriptor GetParameter(string parameterName)
        {
            return this._collectionByName.ContainsKey(parameterName) ? this._collectionByName[parameterName] : null;
        }

        /// <summary>
        /// Returns true when the collection contains a header parameter with the specified name.
        /// </summary>
        /// <param name="parameterName">Name of parameter to check.</param>
        /// <returns>True when present in collection, false otherwise.</returns>
        internal bool HasParameter(string parameterName)
        {
            return this._collectionByName.ContainsKey(parameterName);
        }

        /// <summary>
        /// Returns true when the collection contains a header parameter with the specified ID.
        /// </summary>
        /// <param name="parameterID">ID of parameter to check.</param>
        /// <returns>True when present in collection, false otherwise.</returns>
        internal bool HasParameter(int parameterID)
        {
            return this._collectionByID.ContainsKey(parameterID);
        }

        /// <summary>
        /// This method can be used to synchronize the collection with the contents of another one. On return, our collection contains only
        /// the attributes that are present in 'newContents'.
        /// </summary>
        /// <param name="newContents">The collection to copy attributes from.</param>
        internal void UpdateCollection(RESTHeaderParameterCollection newContents)
        {
            // Check what we currently have in our UML class (if any)...
            List<string> classAttribs = new List<string>();
            if (CollectionClass != null) foreach (MEAttribute attrib in CollectionClass.Attributes) classAttribs.Add(attrib.Name);

            // first of all, check what has been added or changed...
            foreach (RESTHeaderParameterDescriptor newParm in newContents.Collection)
            {
                // First of all, we either append to- or update the local collection.
                // Note that update has no effect on the Sentinel since the ID does not change.
                if (this._collectionByName.ContainsKey(newParm.Name))
                {
                    this._collectionByName[newParm.Name] = newParm;
                    this._collectionByID[newParm.ID] = newParm;
                    if (classAttribs.Contains(newParm.Name)) UpdateAttributeFromDescriptor(newParm, newParm);
                }
                else
                {
                    // When the new parameter is not yet in the collection, we have to add it using a new ID...
                    var newDesc = new RESTHeaderParameterDescriptor(this._nextID++, newParm);
                    this._collectionByName.Add(newDesc.Name, newDesc);
                    this._collectionByID.Add(newDesc.ID, newDesc);

                    CreateAttributeFromDescriptor(newDesc);
                    classAttribs.Add(newDesc.Name);

                    // Now that we have added a new parameter to the collection, we have to make sure to keep the Sentinel in sync.
                    // In theory, the Sentinel could even be absent (i.e. on new collections) so, if we can't find it, we create a new one.
                    if (this._sentinel != null) this._sentinel.SetTag(ContextSlt.GetContextSlt().GetConfigProperty(_HPAIDTag), this._nextID.ToString(), true);
                    else CreateSentinel(this._nextID);
                }
            }

            // Next, check what has been deleted...
            List<string> nameList = new List<string>(); // We can't delete while iterating, so collect the things to be deleted...
            foreach (RESTHeaderParameterDescriptor myParm in this._collectionByName.Values)
            {
                if (!newContents.Collection.Contains(myParm)) nameList.Add(myParm.Name);
            }
            try
            {
                if (CollectionClass != null) LockCollection();
                foreach (string delKey in nameList)
                {
                    if (CollectionClass != null)
                    {
                        foreach (MEAttribute attrib in CollectionClass.Attributes)
                        {
                            if (attrib.Name == delKey)
                            {
                                CollectionClass.DeleteAttribute(attrib);
                                break;
                            }
                        }
                    }
                    this._collectionByID.Remove(this._collectionByName[delKey].ID);
                    this._collectionByName.Remove(delKey);
                }
            }
            finally { if (CollectionClass != null) UnlockCollection(); }
        }

        /// <summary>
        /// (Re)name the collection. If a class with the current name already exists, the operation is ignored.
        /// And if the class does not yet exists, it is created with the given name and all currently collected Header Parameters will be
        /// moved to the new class. If we have incorrect / inconsistent state (no package and scope are known yet), the
        /// operation is ignored but we'll store the name for later reference.
        /// </summary>
        /// <param name="name">(new) name for the class.</param>
        protected override void SetName(string name)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string collectionStereotype = context.GetConfigProperty(_HPCStereotype);
            if (OwningPackage != null)
            {
                // First of all, check spurious name assignments (nothing to do in that case)...
                if (CollectionClass != null && CollectionClass.Name == name) return;

                try
                {
                    LockCollection();
                    this._name = name;
                    MEClass myClass = OwningPackage.FindClass(name, collectionStereotype);
                    if (myClass == null)
                    {
                        // Check whether we already own a class (with another name)...
                        if (CollectionClass != null)
                        {
                            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTHeaderParameterCollection.SetName >> Existing class with name '" +
                                             CollectionClass.Name + "', rename to '" + name + "'...");
                            CollectionClass.Name = name;
                        }
                        else
                        {
                            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTHeaderParameterCollection.SetName >> No class yet, creating one...");
                            try
                            {
                                CreateCollectionClass(name, collectionStereotype);
                                if (this._collectionByName.Count > 0)
                                {
                                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTHeaderParameterCollection.SetName >> We already have some attributes, move to new class...");
                                    foreach (RESTHeaderParameterDescriptor desc in this._collectionByName.Values) CreateAttributeFromDescriptor(desc);
                                }
                                CreateSentinel(this._nextID);
                            }
                            catch (Exception exc)
                            {
                                Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTHeaderParameterCollection.SetName >> Error creating class '" +
                                                  name + "' because:" + Environment.NewLine + exc.ToString());
                            }
                        }
                    }
                    else
                    {
                        // Remove all contents from the existing class so we can replace this by our current set of attributes.
                        foreach (MEAttribute att in myClass.Attributes) myClass.DeleteAttribute(att);

                        // Class already exists, but we don't know about it (yet). Associate with the class and replace its contents by my attributes...
                        InitCollectionClass(myClass);

                        // Now, copy all our current attributes to the class and create a Sentinel (the original one has been deleted above.
                        foreach (RESTHeaderParameterDescriptor paramDesc in this._collectionByName.Values) CreateAttributeFromDescriptor(paramDesc);
                        CreateSentinel(this._nextID);
                    }
                }
                finally
                {
                    // Assures that the collection is unlocked, even in case of exceptions.
                    UnlockCollection();
                }
            }
        }

        /// <summary>
        /// Helper function that saves the specified descriptor as an attribute of the collection UML class. 
        /// Context must have been checked earlier, i.e. we assume that this is indeed a new attribute. The function does NOT perform
        /// any operations in case we don't (yet) have an UML class that can be used to persist the descriptor.
        /// </summary>
        /// <param name="desc">Descriptor to be converted into an attribute.</param>
        /// <exception cref="InvalidOperationException">Thrown when we could not create the attribute.</exception>
        private void CreateAttributeFromDescriptor(RESTHeaderParameterDescriptor desc)
        {
            if (CollectionClass == null) return;

            ContextSlt context = ContextSlt.GetContextSlt();
            string stereotype = context.GetConfigProperty(_HPAStereotype);
            string IDTag = context.GetConfigProperty(_HPAIDTag);

            MEAttribute newAttrib = RESTParameterDeclaration.ConvertToAttribute(CollectionClass, desc.Parameter, stereotype);
            if (newAttrib != null)
            {
                newAttrib.SetTag(IDTag, desc.ID.ToString());
            }
            else
            {
                string message = "Unable to create attribute '" + desc.Name + "' in collection '" + Name + "'!";
                Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTHeaderParameterCollection.CreateAttributeFromDescriptor >> " + message);
                throw new InvalidOperationException(message);
            }
        }

        /// <summary>
        /// Helper function that initializes the collection from a given collection class. All properties are initialized and loaded from the class.
        /// </summary>
        /// <param name="collectionClass">Class to be used for initialization.</param>
        private void CreateCollectionFromClass(MEClass collectionClass)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string sentinelName = context.GetConfigProperty(_SentinelParameterName);
            this._collectionByName = new SortedList<string, RESTHeaderParameterDescriptor>();
            this._collectionByID = new SortedList<int, RESTHeaderParameterDescriptor>();
            this._sentinel = null;
            int maxID = 0;
            if (collectionClass != null)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTHeaderParameterCollection.CreateCollectionFromClass >> Using class '" +
                                 collectionClass.Name + "' to load collection...");
                InitCollectionClass(collectionClass);
                string attribStereotype = context.GetConfigProperty(_HPAStereotype);
                string IDTagName = context.GetConfigProperty(_HPAIDTag);
                foreach (MEAttribute attrib in collectionClass.Attributes)
                {
                    if (attrib.HasStereotype(attribStereotype))
                    {
                        if (attrib.Name != sentinelName)
                        {
                            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTHeaderParameterCollection >> Found Header Parameter Descriptor '" +
                                             attrib.Name + "'...");
                            int attribID;
                            if (int.TryParse(attrib.GetTag(IDTagName), out attribID))
                            {
                                var newParam = new RESTHeaderParameterDescriptor(attribID, attrib);
                                this._collectionByName.Add(attrib.Name, newParam);
                                this._collectionByID.Add(attribID, newParam);
                                if (attribID > maxID) maxID = attribID;
                            }
                            else Logger.WriteWarning("Unable to retrieve valid Header Parameter ID from collection attribute'" +
                                                     Name + "." + attrib.Name + "'!");
                        }
                        else
                        {
                            // The Sentinel attribute is used solely to keep track of ID's. It holds the 'next available' ID in the collection.
                            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTHeaderParameterCollection >> Found our Sentinel!");
                            int attribID;
                            if (int.TryParse(attrib.GetTag(IDTagName), out attribID))
                            {
                                this._sentinel = attrib;
                                this._nextID = attribID;
                            }
                        }
                    }
                }

                // Next, we check whether we encountered the Sentinel. If not, we create a new one...
                if (this._sentinel == null)
                {
                    LockCollection();
                    Logger.WriteWarning("Collection '" + Name + "' is missing it's Sentinel, creating new one with ID '" + maxID + 1 + "'!");
                    CreateSentinel(maxID + 1);
                    UnlockCollection();
                }
            }
        }

        /// <summary>
        /// Helper function that creates a new sentinel attribute in our collection class. It initializes this._nextID and this._sentinel.
        /// When we already have a sentinel reference, the function does not perform any operations!
        /// When the collection class is not (yet) present, the function does not perform any operations!
        /// </summary>
        /// <param name="startID">ID that must be assigned to the Sentinel.</param>
        private void CreateSentinel(int startID)
        {
            if (this._sentinel == null && CollectionClass != null)
            {
                ContextSlt context = ContextSlt.GetContextSlt();
                string sentinelName = context.GetConfigProperty(_SentinelParameterName);
                string IDTagName = context.GetConfigProperty(_HPAIDTag);
                this._nextID = startID;
                this._sentinel = CollectionClass.CreateAttribute(sentinelName, null, AttributeType.Unknown, null,
                                                                 new Cardinality(Cardinality._Mandatory),
                                                                 false, "Sentinel Attribute, do not touch!");
                if (this._sentinel != null)
                {
                    this._sentinel.AddStereotype(context.GetConfigProperty(_HPAStereotype));
                    this._sentinel.SetTag(IDTagName, this._nextID.ToString(), true);
                }
                else Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTHeaderParameterCollection.CreateSentinel >> Unable to create Sentinel attribute in collection '" + Name + "'!");
            }
        }

        /// <summary>
        /// Helper function that is called when the user has edited an existing Header Parameter Descriptor. The function
        /// locates the original attribute and replaces this with data from the new descriptor.
        /// </summary>
        /// <param name="oldDesc">The original descriptor that must be updated.</param>
        /// <param name="newDesc">The new descriptor, created by the user.</param>
        private void UpdateAttributeFromDescriptor(RESTHeaderParameterDescriptor oldDesc, RESTHeaderParameterDescriptor newDesc)
        {
            if (CollectionClass == null) return;      // ignore in case we have not serialized our collection yet.

            ContextSlt context = ContextSlt.GetContextSlt();
            foreach (MEAttribute attrib in CollectionClass.Attributes)
            {
                if (attrib.Name == oldDesc.Name)
                {
                    LockCollection();
                    CollectionClass.DeleteAttribute(attrib);    // Get rid of the old one and create new one...
                    CreateAttributeFromDescriptor(newDesc);
                    UnlockCollection();
                    break;
                }
            }
        }
    }
}
