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
    /// Helper class that maintains a series of predefined REST response codes, which the user can use to quickly
    /// assign consistent sets of response codes to REST operations. It is also used to maintain the list of response
    /// codes for a given operation.
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

        private SortedList<string, RESTHeaderParameterDescriptor> _collection;    // The actual collection.

        /// <summary>
        /// Returns the list of Header Parameter Descriptors that comprises the collection.
        /// </summary>
        internal List<RESTHeaderParameterDescriptor> Collection { get { return new List<RESTHeaderParameterDescriptor>(this._collection.Values); } }

        /// <summary>
        /// Returns true in case header parameters are present and should be used.
        /// </summary>
        internal bool UseHeaderParameters { get { return this._collection.Count > 0; } }

        /// <summary>
        /// Either loads an existing collection with specified name from the specified package, or creates a new, empty, collection
        /// in the specified package. If we find an existing collection, the collection is build from the existing class.
        /// </summary>
        /// <param name="parent">For operation-scoped collections, this is the resource owning the operation. Must be NULL for template collections.</param>
        /// <param name="collectionName">Name to be assigned to the collection.</param>
        /// <param name="package">Package that must contain the collection. The location of the package determines the scope of the
        /// collection: if this is a ServiceModel package, the scope is 'API'. If the package is an Operation-type, the scope is 'Operation'
        /// and all others are considered 'Global'.</param>
        /// <exception cref="InvalidOperationException">Is thrown when we can't find the attribute classifier.</exception>
        internal RESTHeaderParameterCollection(RESTResourceCapability parent, string collectionName, MEPackage package): base(parent, collectionName, package)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTHeaderParameterCollection >> Creating instance '" +
                             collectionName + "' in Package '" + package.Parent.Name + "/" + package.Name + "'...");

            ContextSlt context = ContextSlt.GetContextSlt();
            string collectionStereotype = context.GetConfigProperty(_HPCStereotype);
            this._collection = new SortedList<string, RESTHeaderParameterDescriptor>();
            this._collectionClass = package.FindClass(collectionName, collectionStereotype);
            if (this._collectionClass != null)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTHeaderParameterCollection >> Found existing collection class!");
                InitCollectionClass();
                string attribStereotype = context.GetConfigProperty(_HPAStereotype);
                foreach (MEAttribute attrib in this._collectionClass.Attributes)
                {
                    if (attrib.HasStereotype(attribStereotype))
                    {
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTHeaderParameterCollection >> Found Response Code Descriptor '" +
                                         attrib.Name + "'...");
                        this._collection.Add(attrib.Name, new RESTHeaderParameterDescriptor(attrib.Name, attrib.Classifier, attrib.Annotation));
                    }
                }
            }
            else
            {
                // We could not find an existing class, so we create a new, empty, collection...
                try
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTHeaderParameterCollection >> No class yet, creating one...");
                    SetScope(CollectionScope.Global);
                    if (package.HasStereotype(context.GetConfigProperty(_ServiceOperationPkgStereotype))) SetScope(CollectionScope.Operation);
                    else if (package.Name == context.GetConfigProperty(_ServiceModelPkgName) &&
                             package.HasStereotype(context.GetConfigProperty(_ServiceModelPkgStereotype))) SetScope(CollectionScope.API);
                    LockCollection();
                    CreateCollectionClass(collectionStereotype);
                }
                finally { UnlockCollection(); }
            }
        }

        /// <summary>
        /// This constructor is called with an existing Collection class and initialises the collection from that class.
        /// </summary>
        /// <param name="parent">For operation-scoped collections, this is the resource owning the operation. Must be NULL for template collections.</param>
        /// <param name="collectionClass">Collection class.</param>
        /// <exception cref="InvalidOperationException">Thrown when a collection class is passed that is not of the correct stereotype or 
        /// when we can't find the correct attribute classifier.</exception>
        internal RESTHeaderParameterCollection(RESTResourceCapability parent, MEClass collectionClass): base(parent, collectionClass)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTHeaderParameterCollection >> Creating instance from class '" +
                              collectionClass.Name + "'...");

            ContextSlt context = ContextSlt.GetContextSlt();
            if (!collectionClass.HasStereotype(context.GetConfigProperty(_HPCStereotype)))
            {
                string msg = "Attempt to create collection from class with wrong stereotype '" + collectionClass.Name + "', ignored!";
                Logger.WriteWarning(msg);
                throw new InvalidOperationException(msg);
            }

            InitCollectionClass();
            this._collection = new SortedList<string, RESTHeaderParameterDescriptor>();
            string attribStereotype = context.GetConfigProperty(_HPAStereotype);
            foreach (MEAttribute attrib in this._collectionClass.Attributes)
            {
                if (attrib.HasStereotype(attribStereotype))
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTHeaderParameterCollection >> Found Header Param Descriptor '" +
                                     attrib.Name + "'...");
                    this._collection.Add(attrib.Name, new RESTHeaderParameterDescriptor(attrib.Name, attrib.Classifier, attrib.Annotation));
                }
            }
        }

        /// <summary>
        /// The default constructor is used only when we want to create new (template) collections. In this case, we want to assign owning
        /// package, name and contents iteratively.
        /// </summary>
        /// <param name="parent">For non-template collections, this is the parent resource owning the collection.</param>
        /// <param name="initialScope">Contains the initial scope for the collection.</param>
        internal RESTHeaderParameterCollection(RESTResourceCapability parent, CollectionScope initialScope = CollectionScope.Unknown): base(parent, initialScope)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTHeaderParameterCollection >> Default constructor.");
            this._collection = new SortedList<string, RESTHeaderParameterDescriptor>();
        }

        /// <summary>
        /// This function is invoked to add a new header parameter to this collection. It displays the Header Parameter Dialog, which
        /// facilitates the user in creating a new header descriptor. When it is indeed a new result, the created descriptor is added to 
        /// the collection, otherwise the function does not perform any operations.
        /// </summary>
        /// <returns>Newly created descriptor or NULL in case of errors, duplicates or user cancel.</returns>
        internal RESTHeaderParameterDescriptor AddHeaderParameter()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            RESTHeaderParameterDescriptor newResult = new RESTHeaderParameterDescriptor(string.Empty, null, string.Empty);
            using (var dialog = new RESTHeaderParameterDialog(newResult))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    newResult = dialog.Parameter;
                    if (!this._collection.ContainsKey(newResult.Name))
                    {
                        this._collection.Add(newResult.Name, newResult);
                        CreateAttributeFromDescriptor(newResult);
                    }
                    else
                    {
                        MessageBox.Show("Duplicate header parameter, please try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        newResult = null;
                    }
                }
                else newResult = null;
            }
            return newResult;
        }

        /// <summary>
        /// Add a descriptor from a template to our current collection. If we already have a descriptor with the same code, the operation
        /// is ignored.
        /// </summary>
        /// <param name="template">Descriptor to be copied into our collection.</param>
        /// <returns>The newly added descriptor or null on duplicate.</returns>
        internal RESTHeaderParameterDescriptor AddHeaderParameter(RESTHeaderParameterDescriptor template)
        {
            RESTHeaderParameterDescriptor newDesc = null;
            if (template.IsValid)
            {
                newDesc = new RESTHeaderParameterDescriptor(template);
                if (!this._collection.ContainsKey(newDesc.Name))
                {
                    this._collection.Add(newDesc.Name, newDesc);
                    CreateAttributeFromDescriptor(newDesc);
                }
                else
                {
                    Logger.WriteWarning("Attempt to add duplicate header parameter '" + newDesc.Name + "' to collection '" + Name + "' ignored!");
                    newDesc = null;
                }
            }
            return newDesc;
        }

        /// <summary>
        /// Creates a deep copy of the specified parameter collection to the current collection.
        /// </summary>
        /// <param name="other">The collection to copy from.</param>
        internal void CopyCollection(RESTHeaderParameterCollection other)
        {
            this._collection = new SortedList<string, RESTHeaderParameterDescriptor>();
            foreach (RESTHeaderParameterDescriptor otherDesc in other._collection.Values)
            {
                this._collection.Add(otherDesc.Name, new RESTHeaderParameterDescriptor(otherDesc.Name, otherDesc.Classifier, otherDesc.Description));
            }
        }

        /// <summary>
        /// This function is invoked when the entire collection has to be destroyed.
        /// the collection class is deleted and a new, empty, parameter list is created. 
        /// </summary>
        internal override void DeleteCollection()
        {
            base.DeleteCollection();                                                    // Removes the UML registration by deleting the class and clearing the name.
            this._collection = new SortedList<string, RESTHeaderParameterDescriptor>(); // Releases the original collection and creates a new one.
        }

        /// <summary>
        /// Deletes the header parameter with the specified name from the collection.
        /// </summary>
        /// <param name="name">Parameter to be deleted.</param>
        /// <returns>True when actually deleted the parameter, false when parameter was not found in the collection.</returns>
        internal bool DeleteHeaderParameter(string name)
        {
            if (!this._collection.ContainsKey(name)) return false;
            if (this._collectionClass != null)
            {
                foreach (MEAttribute attrib in this._collectionClass.Attributes)
                {
                    if (attrib.Name == name)
                    {
                        LockCollection();
                        this._collectionClass.DeleteAttribute(attrib);
                        UnlockCollection();
                        break;
                    }
                }
            }
            this._collection.Remove(name);
            return true;
        }

        /// <summary>
        /// This function is invoked to edit an existing header parameter. It displays the Header Parameter Dialog, which facilitates the user in 
        /// changing the result object. The updated object is added to the result list for this collection as long as
        /// it (still) has a unique name.
        /// </summary>
        /// <returns>Updated result record or NULL in case of errors or duplicates or user cancel.</returns>
        /// <exception cref="ArgumentException">Thrown when the received name does not match an existing header parameter.</exception>
        internal RESTHeaderParameterDescriptor EditHeaderParameter(string name)
        {
            RESTHeaderParameterDescriptor currentDesc = this._collection.ContainsKey(name) ? this._collection[name] : null;
            RESTHeaderParameterDescriptor editDesc = null;

            if (currentDesc != null)
            {
                // Create a (temporary) copy so we can properly detect changes...
                using (var dialog = new RESTHeaderParameterDialog(new RESTHeaderParameterDescriptor(currentDesc)))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        // If the name has not been changed, or changed to a still unique name, insert the new descriptor in the collection...
                        if (dialog.Parameter.Name == currentDesc.Name)
                        {
                            UpdateAttributeFromDescriptor(currentDesc, dialog.Parameter);
                            this._collection[name] = dialog.Parameter;
                            editDesc = dialog.Parameter;
                        }
                        else if (!this._collection.ContainsKey(dialog.Parameter.Name))
                        {
                            UpdateAttributeFromDescriptor(currentDesc, dialog.Parameter);
                            this._collection.Remove(name);
                            this._collection.Add(dialog.Parameter.Name, dialog.Parameter);
                            editDesc = dialog.Parameter;
                        }
                        else // We have renamed the parameter to an existing parameter, error!
                        {
                            MessageBox.Show("Changing parameter name resulted in duplicate name, please try again!",
                                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            editDesc = null;
                        }
                    }
                    else editDesc = null;
                }
            }
            else
            {
                string msg = "Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection.EditOperationResult >> Can't find existing attribute '" + this._collectionClass.Name + "." + name + "'!";
                Logger.WriteError(msg);
                throw new ArgumentException(msg);
            }
            return editDesc;
        }

        /// <summary>
        /// This method can be used to synchronize the collection with the contents of another one. On return, our collection contains only
        /// the attributes that are present in 'newContents'.
        /// </summary>
        /// <param name="newContents"></param>
        internal void UpdateCollection(RESTHeaderParameterCollection newContents)
        {
            // Check what we currently have in our UML class (if any)...
            List<string> classAttribs = new List<string>();
            if (this._collectionClass != null) foreach (MEAttribute attrib in this._collectionClass.Attributes) classAttribs.Add(attrib.Name);

            // first of all, check what has been added or changed...
            foreach (RESTHeaderParameterDescriptor newParm in newContents.Collection)
            {
                // First of all, we either append to- or update the local collection...
                if (this._collection.ContainsKey(newParm.Name)) this._collection[newParm.Name] = newParm;
                else this._collection.Add(newParm.Name, newParm);

                // And next, we do the same for the UML class (if present)...
                if (classAttribs.Contains(newParm.Name)) UpdateAttributeFromDescriptor(this._collection[newParm.Name], newParm);
                else
                {
                    CreateAttributeFromDescriptor(newParm);
                    classAttribs.Add(newParm.Name);
                }
            }

            // Next, check what has been deleted...
            List<string> nameList = new List<string>(); // We can't delete while iterating, so collect the things to be deleted...
            foreach (RESTHeaderParameterDescriptor myParm in this._collection.Values)
            {
                if (!newContents.Collection.Contains(myParm)) nameList.Add(myParm.Name);
            }
            try
            {
                if (this._collectionClass != null) LockCollection();
                foreach (string delKey in nameList)
                {
                    if (this._collectionClass != null)
                    {
                        foreach (MEAttribute attrib in this._collectionClass.Attributes)
                        {
                            if (attrib.Name == delKey)
                            {
                                this._collectionClass.DeleteAttribute(attrib);
                                break;
                            }
                        }
                    }
                    this._collection.Remove(delKey);
                }
            }
            finally { if (this._collectionClass != null) UnlockCollection(); }
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
            if (this._collectionClass == null) return;

            ContextSlt context = ContextSlt.GetContextSlt();

            // Create an attribute of type 'unknown', which implies that the stereotype is left empty. This way, we can
            // assign our specific 'RCDStereotype' afterwards...
            MEAttribute newAttrib = this._collectionClass.CreateAttribute(desc.Name, desc.Classifier, AttributeType.Unknown, null, 
                                                                          new Cardinality(Cardinality._Mandatory),
                                                                          false, desc.Description);
            if (newAttrib != null)
            {
                newAttrib.AddStereotype(context.GetConfigProperty(_HPAStereotype));
            }
            else
            {
                string message = "Unable to create attribute '" + desc.Name + "' in collection '" + Name + "'!";
                Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTHeaderParameterCollection.CreateAttributeFromDescriptor >> " + message);
                throw new InvalidOperationException(message);
            }
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
                if (this._collectionClass != null && this._collectionClass.Name == name) return;

                try
                {
                    LockCollection();
                    this._name = name;
                    MEClass myClass = OwningPackage.FindClass(name, collectionStereotype);
                    if (myClass == null)
                    {
                        // Check whether we already own a class (with another name)...
                        if (this._collectionClass != null)
                        {
                            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTHeaderParameterCollection.SetName >> Existing class with name '" +
                                             this._collectionClass.Name + "', rename to '" + name + "'...");
                            this._collectionClass.Name = name;
                        }
                        else
                        {
                            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTHeaderParameterCollection.SetName >> No class yet, creating one...");
                            try
                            {
                                CreateCollectionClass(collectionStereotype);
                                if (this._collection.Count > 0)
                                {
                                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTHeaderParameterCollection.SetName >> We already have some attributes, move to new class...");
                                    foreach (RESTHeaderParameterDescriptor desc in this._collection.Values) CreateAttributeFromDescriptor(desc);
                                }
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
                        // Class already exists, but we don't know about it (yet). Associate with the class and add all attributes that are not yet
                        // serialized...
                        this._collectionClass = myClass;
                        List<string> attribNames = new List<string>();
                        foreach (MEAttribute attrib in myClass.Attributes) attribNames.Add(attrib.Name);
                        foreach (RESTHeaderParameterDescriptor desc in this._collection.Values) if (!attribNames.Contains(desc.Name)) CreateAttributeFromDescriptor(desc);
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
        /// Helper function that is called when the user has edited an existing Header Parameter Descriptor. The function
        /// locates the original attribute and replaces this with data from the new descriptor.
        /// </summary>
        /// <param name="oldDesc">The original descriptor that must be updated.</param>
        /// <param name="newDesc">The new descriptor, created by the user.</param>
        private void UpdateAttributeFromDescriptor(RESTHeaderParameterDescriptor oldDesc, RESTHeaderParameterDescriptor newDesc)
        {
            if (this._collectionClass == null) return;      // ignore in case we have not serialized our collection yet.

            ContextSlt context = ContextSlt.GetContextSlt();
            foreach (MEAttribute attrib in this._collectionClass.Attributes)
            {
                if (attrib.Name == oldDesc.Name)
                {
                    LockCollection();
                    attrib.Name = newDesc.Name;
                    attrib.Annotation = newDesc.Description;
                    attrib.Classifier = newDesc.Classifier;
                    UnlockCollection();
                    break;
                }
            }
        }
    }
}
