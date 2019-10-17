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
    internal sealed class RESTResponseCodeCollection
    {
        /// <summary>
        /// Response code collections can have any of the following scopes:
        /// Global = ECDM-wide template that can be used for any API. These are stored as part of the Framework.
        /// API = API-specific template that can be used for any operation within the associated API. These are stored as part of the ServiceModel.
        /// Operation = Operation-specific set of response codes that will be used in the OpenAPI interface specification of the operation.
        /// If no scope is defined, the default is always 'API'. The scope is defined during construction and can not be changed afterwards!
        /// </summary>
        internal enum CollectionScope { Unknown, Global, API, Operation }

        // Configuration properties used by this module:
        private const string _RCCStereotype                         = "RCCStereotype";
        private const string _RCDStereotype                         = "RCDStereotype";
        private const string _CoreDataTypesPathName                 = "CoreDataTypesPathName";
        private const string _ServiceModelPkgName                   = "ServiceModelPkgName";
        private const string _ServiceOperationPkgStereotype         = "ServiceOperationPkgStereotype";
        private const string _ServiceModelPkgStereotype             = "ServiceModelPkgStereotype";
        private const string _RCCIDTag                              = "RCCIDTag";
        private const string _RCCScopeTag                           = "RCCScopeTag";

        private MEClass _collectionClass;                           // UML representation of the collection.
        private MEPackage _owningPackage;                           // The package in which the collection lives.
        private RESTOperationCapability _operation;                 // For operation-scoped collections, this is the parent operation.
        private List<RESTOperationResultDescriptor> _collection;    // The actual collection.
        private CollectionScope _scope;                             // Scope of this collection.
        private string _name;                                       // Collection name, identical to collectionClass name (when one is present).
        private bool _isLocked;                                     // Set to 'true' when global scope and locked.
        private string _collectionID;                               // Collection identifier.

        /// <summary>
        /// Returns the list of Operation Result Descriptors that comprises the collection.
        /// </summary>
        internal List<RESTOperationResultDescriptor> Collection { get { return this._collection; } }

        /// <summary>
        /// Returns the UML class used to represent the collection within the model.
        /// </summary>
        internal MEClass CollectionClass { get { return this._collectionClass; } }

        /// <summary>
        /// Returns the unique collection identifier.
        /// </summary>
        internal string CollectionID { get { return this.CollectionID; } }

        /// <summary>
        /// Get or Set the name of the collection. Note that, if we assign a name to an object that is not yet associated with
        /// an MEClass, we create the class and assign any attributes that are already in the collection.
        /// And if we assign a new name to a collection and a class already exists with that name, an exception will be thrown.
        /// In case we don't have a valid context (no class and package), the name is stored locally only.
        /// </summary>
        internal string Name
        {
            get { return this._collectionClass != null? this._collectionClass.Name: string.Empty; }
            set { SetName(value); }
        }

        /// <summary>
        /// Returns the package in which the collection lives.
        /// </summary>
        internal MEPackage OwningPackage { get { return this._owningPackage; } }

        /// <summary>
        /// Returns the collection scope.
        /// </summary>
        internal CollectionScope Scope {  get { return this._scope; } }

        /// <summary>
        /// Either loads an existing collection with specified name from the specified package, or creates a new, empty, collection
        /// in the specified package. If we find an existing collection, the constructor also initialises all associated Result Code
        /// Descriptors.
        /// </summary>
        /// <param name="operation">For operation-scoped collections, this is the parent operation. Must be NULL for template collections.</param>
        /// <param name="collectionName">Name to be assigned to the collection.</param>
        /// <param name="package">Package that must contain the collection. The location of the package determines the scope of the
        /// collection: if this is a ServiceModel package, the scope is 'API'. If the package is an Operation-type, the scope is 'Operation'
        /// and all others are considered 'Global'.</param>
        /// <exception cref="InvalidOperationException">Is thrown when we can't find the attribute classifier.</exception>
        internal RESTResponseCodeCollection(RESTOperationCapability operation, string collectionName, MEPackage package)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> Creating instance '" +
                             collectionName + "' in Package '" + package.Parent.Name + "/" + package.Name + "'...");

            ContextSlt context = ContextSlt.GetContextSlt();

            // first of all, we're trying to determine the scope of this collection...
            this._owningPackage = package;
            this._name = collectionName;
            this._isLocked = false;
            this._collection = new List<RESTOperationResultDescriptor>();
            this._operation = operation;

            string collectionStereotype = context.GetConfigProperty(_RCCStereotype);
            string responseDescriptorStereotype = context.GetConfigProperty(_RCDStereotype);
            this._collectionClass = package.FindClass(collectionName, collectionStereotype);
            if (this._collectionClass != null)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> Found existing collection class!");
                string attribStereotype = context.GetConfigProperty(_RCDStereotype);
                this._collectionID = this._collectionClass.GetTag(context.GetConfigProperty(_RCCIDTag));
                this._scope = EnumConversions<CollectionScope>.StringToEnum(this._collectionClass.GetTag(context.GetConfigProperty(_RCCScopeTag)));
                foreach (MEAttribute attrib in this._collectionClass.Attributes)
                {
                    if (attrib.HasStereotype(attribStereotype))
                    {
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> Found Response Code Descriptor '" +
                                         attrib.Name + "'...");
                        this._collection.Add(new RESTOperationResultDescriptor(this, attrib));
                    }
                }
            }
            else
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> No class yet, creating one...");
                this._scope = CollectionScope.Global; 
                if (package.HasStereotype(context.GetConfigProperty(_ServiceOperationPkgStereotype))) this._scope = CollectionScope.Operation;
                else if (package.Name == context.GetConfigProperty(_ServiceModelPkgName) &&
                         package.HasStereotype(context.GetConfigProperty(_ServiceModelPkgStereotype))) this._scope = CollectionScope.API;

                LockCollection();
                this._collectionClass = package.CreateClass(collectionName, collectionStereotype);
                this._collectionClass.SetTag(context.GetConfigProperty(_RCCIDTag), this._collectionClass.ElementID.ToString(), true);
                this._collectionClass.SetTag(context.GetConfigProperty(_RCCScopeTag), EnumConversions<CollectionScope>.EnumToString(this._scope));                
                UnlockCollection();
            }
        }

        /// <summary>
        /// This constructor is called with an existing Collection class and initialises the collection from that class.
        /// </summary>
        /// <param name="operation">For operation-scoped collections, this is the parent operation. Must be NULL for template collections.</param>
        /// <param name="collectionClass">Collection class.</param>
        /// <exception cref="InvalidOperationException">Thrown when a collection class is passed that is not of the correct stereotype or 
        /// when we can't find the correct attribute classifier.</exception>
        internal RESTResponseCodeCollection(RESTOperationCapability operation, MEClass collectionClass)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> Creating instance from class '" +
                              collectionClass.Name + "'...");

            ContextSlt context = ContextSlt.GetContextSlt();
            this._isLocked = false;
            this._collection = new List<RESTOperationResultDescriptor>();
            this._collectionClass = collectionClass;
            this._owningPackage = collectionClass.OwningPackage;
            this._name = collectionClass.Name;
            this._operation = operation;

            string attribStereotype = context.GetConfigProperty(_RCDStereotype);
            if (!collectionClass.HasStereotype(context.GetConfigProperty(_RCCStereotype)))
            {
                string msg = "Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> Attempt to create collection from wrong classifier '" + collectionClass.Name + "'!";
                Logger.WriteError(msg);
                throw new InvalidOperationException(msg);
            }

            this._collectionID = this._collectionClass.GetTag(context.GetConfigProperty(_RCCIDTag));
            this._scope = EnumConversions<CollectionScope>.StringToEnum(this._collectionClass.GetTag(context.GetConfigProperty(_RCCScopeTag)));
            foreach (MEAttribute attrib in this._collectionClass.Attributes)
            {
                if (attrib.HasStereotype(attribStereotype))
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> Found Response Code Descriptor '" +
                                     attrib.Name + "'...");
                    this._collection.Add(new RESTOperationResultDescriptor(this, attrib));
                }
            }
        }

        /// <summary>
        /// This function is invoked to add a new result descriptor to this collection. It displays the Response Code Dialog, which
        /// facilitates the user in creating a new result descriptor. When it is indeed a new result, the created descriptor is added to 
        /// the result list for this collection, otherwise the function does not perform any operations.
        /// </summary>
        /// <returns>Newly created result record or NULL in case of errors, duplicates or user cancel.</returns>
        internal RESTOperationResultDescriptor AddOperationResult()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            RESTOperationResultDescriptor newResult = null;
            using (var dialog = new RESTResponseCodeDialog(this, null))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (!this._collection.Contains(dialog.OperationResult))
                    {
                        newResult = dialog.OperationResult;
                        this._collection.Add(newResult);
                        if (this._collectionClass != null)
                        {
                            LockCollection();
                            newResult.CreateAttributeInCollection();
                            UnlockCollection();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Duplicate operation result code, please try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        newResult = null;
                    }
                }
                else newResult = null;
            }
            return newResult;
        }

        /// <summary>
        /// Deletes an operation result from the collection. If the collection does not contain the specified result code, the operation fails silently.
        /// </summary>
        /// <param name="code">Operation Result Code to be deleted.</param>
        internal void DeleteOperationResult(string code)
        {
            LockCollection();
            foreach (RESTOperationResultDescriptor decl in this._collection)
            {
                if (decl.ResultCode == code)
                {
                    decl.Invalidate();
                    this._collection.Remove(decl);
                    break;
                }
            }
            UnlockCollection();
        }

        /// <summary>
        /// This function is invoked when the entire collection has to be destroyed.
        /// Any exceptions are ignored and on return, the collection is no longer valid.
        /// </summary>
        internal void DeleteResources()
        {
            try
            {
                LockCollection();
                this._owningPackage.DeleteClass(this._collectionClass);
                UnlockCollection();
                this._collection = null;
                this._owningPackage = null;
                this._collectionClass = null;
                this._name = string.Empty;
            }
            catch (Exception exc)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection.DeleteResources >> Caught an exception: " + 
                                 Environment.NewLine + exc.ToString());
            }
        }

        /// <summary>
        /// This function is invoked to edit an existing result code. It displays the Response Code Dialog, which facilitates the user in 
        /// changing the result object. The updated object is added to the result list for this collection as long as
        /// it (still) has a unique code.
        /// </summary>
        /// <returns>Updated result record or NULL in case of errors or duplicates or user cancel.</returns>
        /// <exception cref="ArgumentException">Thrown when the received code does not match an existing attribute.</exception>
        internal RESTOperationResultDescriptor EditOperationResult(string code)
        {
            RESTOperationResultDescriptor originalDesc = null;
            RESTOperationResultDescriptor newDesc = null;
            foreach (RESTOperationResultDescriptor desc in this._collection)
            {
                if (desc.ResultCode == code)
                {
                    originalDesc = desc;
                    break;
                }
            }

            if (originalDesc != null)
            {
                using (var dialog = new RESTResponseCodeDialog(this, originalDesc))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        if (dialog.OperationResult.ResultCode == originalDesc.ResultCode || !this._collection.Contains(dialog.OperationResult))
                        {
                            newDesc = dialog.OperationResult;
                            if (newDesc.ResultCode != originalDesc.ResultCode)
                            {
                                this._collection.Remove(originalDesc);
                                this._collection.Add(newDesc);
                                if (myAttribute != null) myAttribute.Name = newDecl.ResultCode;
                            }
                            else originalDecl.Description = newDecl.Description; 
                        }
                        else
                        {
                            MessageBox.Show("Changing result code resulted in duplicate code, please try again!",
                                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            newDecl = null;
                        }
                    }
                    else newDecl = null;
                }
            }
            else
            {
                string msg = "Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection.EditOperationResult >> Can't find existing attribute '" + this._collectionClass.Name + "." + code + "'!";
                Logger.WriteError(msg);
                throw new ArgumentException(msg);
            }
            return newDecl;
        }

        /// <summary>
        /// If we have a global collection, the function locks the collection package for changes.
        /// </summary>
        internal void LockCollection()
        {
            if (this._scope == CollectionScope.Global && !this._isLocked)
            {
                this._owningPackage.Lock();
                this._isLocked = true;
            }
        }

        /// <summary>
        /// Should be used to change the scope of an 'unknown' collection to a valid collection scope. This function must be called
        /// ONLY on a collection that has been created earlier using a default constructor. If an attempt is made to change scope
        /// of a valid collection, an InvalidOperationException will be thrown!
        /// </summary>
        /// <param name="package">The package that must hold the collection.</param>
        /// <param name="scope">The new collection scope.</param>
        /// <exception cref="ArgumentException">Is thrown when the selected package already contains a collection with the current name.</exception>
        /// <exception cref="InvalidOperationException">Is thrown when the collection has a scope other then 'Unknown'.</exception>
        internal void SetScope(MEPackage package, CollectionScope scope)
        {
            if (this._scope == CollectionScope.Unknown)
            {
                this._owningPackage = package;
                this._scope = scope;
                SetName(this._name);
            }
            else
            {
                string msg = "Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection.SetScope >> Attempt to change scope of valid collection '" + this._collectionClass.Name + "'!";
                Logger.WriteError(msg);
                throw new InvalidOperationException(msg);
            }
        }

        /// <summary>
        /// If we have a global collection, the function unlocks the collection package (we don't check whether the package is indeed locked).
        /// </summary>
        internal void UnlockCollection()
        {
            if (this._scope == CollectionScope.Global && this._isLocked)
            {
                this._owningPackage.Unlock();
                this._isLocked = false;
            }
        }

        /// <summary>
        /// Rename the collection. If another collection class already exists with the specified name, an exception will be thrown!
        /// And if the class does not yet exists, it is created with the given name and all currently collected Response Codes will be
        /// moved to the new class. If we have incorrect / inconsistent state (no package and scope are known yet), the
        /// operation is ignored but we'll store the name for later reference.
        /// </summary>
        /// <param name="name">(new) name for the class.</param>
        /// <exception cref="ArgumentException">Will be thrown when a class already exists with the given name.</exception>
        private void SetName(string name)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string collectionStereotype = context.GetConfigProperty(_RESTResponseCodeCollectionStereotype);
            this._name = name;
            if (this._owningPackage != null)
            {
                // First of all, check spurious name assignments (nothing to do in that case)...
                if (this._collectionClass != null && this._collectionClass.Name == name) return;

                try
                {
                    LockCollection();
                    MEClass newClass = this._owningPackage.FindClass(name, collectionStereotype);
                    if (newClass != null)
                    {
                        string msg = "Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection.SetName >> New name '" + this._collectionClass.Name + "' already exists!";
                        Logger.WriteError(msg);
                        throw new ArgumentException(msg);
                    }
                    else
                    {
                        // Check whether we already own a class (with another name)...
                        if (this._collectionClass != null)
                        {
                            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> Existing class with name '" +
                                             this._collectionClass.Name + "', rename to '" + name + "'...");
                            this._collectionClass.Name = name;
                        }
                        else
                        {
                            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> No class yet, creating one...");
                            this._collectionClass = this._owningPackage.CreateClass(name, collectionStereotype);

                            if (this._collection.Count > 0)
                            {
                                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> We already have some attributes, move to new class...");
                                foreach (RESTOperationResultDescriptor attrib in this._collection)
                                {
                                    this._collectionClass.CreateAttribute(attrib.ResultCode, this._attribClassifier,
                                                                          AttributeType.Attribute, null, new Cardinality(Cardinality._Mandatory),
                                                                          false, attrib.Description);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    // Assures that the collection is unlocked, even in case of exceptions.
                    UnlockCollection();
                }
            }
        }
    }
}
