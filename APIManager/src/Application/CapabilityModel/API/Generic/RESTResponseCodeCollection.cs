using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Framework.Model;
using Framework.Logging;
using Framework.Context;
using Framework.Util;
using Framework.Exceptions;
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
        private const string _RCCIDTag                              = "RCCIDTag";
        private const string _RCCScopeTag                           = "RCCScopeTag";
        private const string _RCDCategoryTag                        = "RCDCategoryTag";
        private const string _RCDPayloadKindTag                     = "RCDPayloadKindTag";
        private const string _RCDReferenceURLTag                    = "RCDReferenceURLTag";
        private const string _CoreDataTypesPathName                 = "CoreDataTypesPathName";
        private const string _ServiceModelPkgName                   = "ServiceModelPkgName";
        private const string _ServiceOperationPkgStereotype         = "ServiceOperationPkgStereotype";
        private const string _ServiceModelPkgStereotype             = "ServiceModelPkgStereotype";

        // Together with the response code, this creates a role name for the payload class associated with a response (e.g. '200-Payload')...
        private const string _ResponsePayloadRolePostfix            = "-Payload";
        private const string _CollectionRole                        = "Collection"; // Used for the collection-end of the association

        private MEClass _collectionClass;                           // UML representation of the collection.
        private MEPackage _owningPackage;                           // The package in which the collection lives.
        private RESTResourceCapability _parentResource;             // For operation-scoped collections, this is the declaration of the parent resource that contains the operation.
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
        internal string CollectionID { get { return this._collectionID; } }

        /// <summary>
        /// Get or Set the name of the collection. Note that, if we assign a name to an object that is not yet associated with
        /// an MEClass, we create the class and assign any attributes that are already in the collection.
        /// And if we assign a new name to a collection and a class already exists with that name, an exception will be thrown.
        /// In case we don't have a valid context (no class and package), the name is stored locally only.
        /// </summary>
        /// <exception cref="ArgumentException">Will be thrown when a class already exists with the given name.</exception>
        internal string Name
        {
            get { return this._collectionClass != null? this._collectionClass.Name: string.Empty; }
            set { SetName(value); }
        }

        /// <summary>
        /// Returns the resource that 'owns' the operation that in turn owns the collection. Valid ONLY for 'Operation' scoped collections.
        /// Returns NULL if no operation is (yet) defined.
        /// </summary>
        internal RESTResourceCapability ParentResource { get { return this._parentResource; } }

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
        /// in the specified package. If we find an existing collection, the collection is build from the existing class.
        /// </summary>
        /// <param name="parent">For operation-scoped collections, this is the resource owning the operation. Must be NULL for template collections.</param>
        /// <param name="collectionName">Name to be assigned to the collection.</param>
        /// <param name="package">Package that must contain the collection. The location of the package determines the scope of the
        /// collection: if this is a ServiceModel package, the scope is 'API'. If the package is an Operation-type, the scope is 'Operation'
        /// and all others are considered 'Global'.</param>
        /// <exception cref="InvalidOperationException">Is thrown when we can't find the attribute classifier.</exception>
        internal RESTResponseCodeCollection(RESTResourceCapability parent, string collectionName, MEPackage package)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> Creating instance '" +
                             collectionName + "' in Package '" + package.Parent.Name + "/" + package.Name + "'...");

            ContextSlt context = ContextSlt.GetContextSlt();

            this._owningPackage = package;
            this._name = collectionName;
            this._isLocked = false;
            this._collection = new List<RESTOperationResultDescriptor>();
            this._parentResource = parent;
            this._collectionID = string.Empty;

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
                        CreateDescriptorFromAttribute(attrib);
                    }
                }
            }
            else
            {
                // We could not find an existing class, so we create a new, empty, collection...
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> No class yet, creating one...");
                this._scope = CollectionScope.Global; 
                if (package.HasStereotype(context.GetConfigProperty(_ServiceOperationPkgStereotype))) this._scope = CollectionScope.Operation;
                else if (package.Name == context.GetConfigProperty(_ServiceModelPkgName) &&
                         package.HasStereotype(context.GetConfigProperty(_ServiceModelPkgStereotype))) this._scope = CollectionScope.API;

                try
                {
                    LockCollection();
                    this._collectionClass = package.CreateClass(collectionName, collectionStereotype);
                    this._collectionID = this._collectionClass.ElementID.ToString();
                    this._collectionClass.SetTag(context.GetConfigProperty(_RCCIDTag), this._collectionID, true);
                    this._collectionClass.SetTag(context.GetConfigProperty(_RCCScopeTag), EnumConversions<CollectionScope>.EnumToString(this._scope));
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
        internal RESTResponseCodeCollection(RESTResourceCapability parent, MEClass collectionClass)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> Creating instance from class '" +
                              collectionClass.Name + "'...");

            ContextSlt context = ContextSlt.GetContextSlt();
            this._isLocked = false;
            this._collection = new List<RESTOperationResultDescriptor>();
            this._collectionClass = collectionClass;
            this._owningPackage = collectionClass.OwningPackage;
            this._name = collectionClass.Name;
            this._parentResource = parent;

            if (!collectionClass.HasStereotype(context.GetConfigProperty(_RCCStereotype)))
            {
                string msg = "Attempt to create collection from class with wrong stereotype '" + collectionClass.Name + "', ignored!";
                Logger.WriteWarning(msg);
                throw new InvalidOperationException(msg);
            }

            this._collectionID = this._collectionClass.GetTag(context.GetConfigProperty(_RCCIDTag));
            string scopeTag = context.GetConfigProperty(_RCCScopeTag);
            string scopeVal = this._collectionClass.GetTag(scopeTag);
            this._scope = EnumConversions<CollectionScope>.StringToEnum(scopeVal);
            string attribStereotype = context.GetConfigProperty(_RCDStereotype);
            foreach (MEAttribute attrib in this._collectionClass.Attributes)
            {
                if (attrib.HasStereotype(attribStereotype))
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> Found Response Code Descriptor '" +
                                     attrib.Name + "'...");
                    CreateDescriptorFromAttribute(attrib);
                }
            }
        }

        /// <summary>
        /// The default constructor is used only when we want to create new (template) collections. In this case, we want to assign owning
        /// package, name and contents iteratively.
        /// </summary>
        /// <param name="parent">For non-template collections, this is the parent resource owning the collection.</param>
        /// <param name="initialScope">Contains the initial scope for the collection.</param>
        internal RESTResponseCodeCollection(RESTResourceCapability parent, CollectionScope initialScope = CollectionScope.Unknown)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> Default constructor.");

            ContextSlt context = ContextSlt.GetContextSlt();
            this._isLocked = false;
            this._collection = new List<RESTOperationResultDescriptor>();
            this._collectionClass = null;
            this._owningPackage = null;
            this._name = string.Empty;
            this._parentResource = parent;
            this._collectionID = string.Empty;
            this._scope = initialScope;
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
                    newResult = dialog.OperationResult;
                    if (!this._collection.Contains(newResult))
                    {
                        this._collection.Add(newResult);
                        CreateAttributeFromDescriptor(newResult);
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
        /// Add a descriptor from a template to our current collection. If we already have a descriptor with the same code, the operation
        /// is ignored.
        /// </summary>
        /// <param name="template">Descriptor to be copied into our collection.</param>
        /// <returns>The newly added descriptor or null on duplicate.</returns>
        internal RESTOperationResultDescriptor AddOperationResult(RESTOperationResultDescriptor template)
        {
            RESTOperationResultDescriptor newDesc = null;
            if (template.IsValid)
            {
                newDesc = new RESTOperationResultDescriptor(this, template);
                if (!this._collection.Contains(newDesc))
                {
                    this._collection.Add(newDesc);
                    CreateAttributeFromDescriptor(newDesc);
                }
                else
                {
                    Logger.WriteWarning("Attempt to add duplicate response code '" + newDesc.ResultCode + "' to collection '" + Name + "' ignored!");
                    newDesc = null;
                }
            }
            return newDesc;
        }

        /// <summary>
        /// Deletes an operation result from the collection. If the collection does not contain the specified result code, the operation fails silently.
        /// </summary>
        /// <param name="code">Operation Result Code to be deleted.</param>
        /// <returns>True when actually deleted the code, false when code was not present in the collection.</returns>
        internal bool DeleteOperationResult(string code)
        {
            bool didDelete = false;
            foreach (RESTOperationResultDescriptor desc in this._collection)
            {
                if (desc.ResultCode == code)
                {
                    // If we have a serialized collection, we remove the UML attribute and associated payload association (if any)...
                    if (this._collectionClass != null)
                    {
                        ContextSlt context = ContextSlt.GetContextSlt();
                        foreach (MEAttribute attrib in this._collectionClass.Attributes)
                        {
                            if (attrib.Name == code)
                            {
                                try
                                {
                                    LockCollection();
                                    this._collectionClass.DeleteAttribute(attrib);

                                    // Check whether we have an association with a payload class...
                                    if (desc.PayloadType == RESTOperationResultDescriptor.ResultPayloadType.DefaultResponse ||
                                        desc.PayloadType == RESTOperationResultDescriptor.ResultPayloadType.CustomResponse ||
                                        (Scope == CollectionScope.Operation && desc.PayloadType == RESTOperationResultDescriptor.ResultPayloadType.Document))
                                    {
                                        string endpointRole = attrib.Name + _ResponsePayloadRolePostfix;
                                        foreach (MEAssociation assoc in this._collectionClass.AssociationList)
                                        {
                                            if (assoc.Destination.Role == endpointRole)
                                            {
                                                this._collectionClass.DeleteAssociation(assoc);
                                                break;
                                            }
                                        }
                                    }
                                }
                                finally { UnlockCollection(); }
                                break;
                            }
                        }
                    }
                    this._collection.Remove(desc);
                    didDelete = true;
                    break;
                }
            }
            return didDelete;
        }

        /// <summary>
        /// This function is invoked when the entire collection has to be destroyed.
        /// Any exceptions are ignored and on return, the collection is no longer valid.
        /// </summary>
        internal void DeleteCollection()
        {
            try
            {
                if (this._collectionClass != null)
                {
                    LockCollection();
                    this._owningPackage.DeleteClass(this._collectionClass);
                    this._collectionClass = null;
                }
                this._collection = null;
                this._owningPackage = null;
                this._name = string.Empty;
            }
            catch (Exception exc)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection.DeleteCollection >> Caught an exception: " + 
                                 Environment.NewLine + exc.ToString());
            }
            finally { UnlockCollection(); }
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
            RESTOperationResultDescriptor currentDesc = null;
            RESTOperationResultDescriptor editDesc = null;
            foreach (RESTOperationResultDescriptor desc in this._collection)
            {
                if (desc.ResultCode == code)
                {
                    currentDesc = desc;
                    break;
                }
            }

            if (currentDesc != null)
            {
                // Create a (temporary) copy so we can properly detect changes...
                using (var dialog = new RESTResponseCodeDialog(this, new RESTOperationResultDescriptor(this, currentDesc)))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        // If the code has not been changed, or changed to a still unique code, insert the new descriptor in the collection...
                        if (dialog.OperationResult.ResultCode == currentDesc.ResultCode || !this._collection.Contains(dialog.OperationResult))
                        {
                            UpdateAttributeFromDescriptor(currentDesc, dialog.OperationResult);
                            this._collection[this._collection.IndexOf(currentDesc)] = dialog.OperationResult;
                            editDesc = dialog.OperationResult;
                        }
                        else // We have renamed the result code to an existing code, error!
                        {
                            MessageBox.Show("Changing result code resulted in duplicate code, please try again!",
                                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            editDesc = null;
                        }
                    }
                    else editDesc = null;
                }
            }
            else
            {
                string msg = "Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection.EditOperationResult >> Can't find existing attribute '" + this._collectionClass.Name + "." + code + "'!";
                Logger.WriteError(msg);
                throw new ArgumentException(msg);
            }
            return editDesc;
        }

        /// <summary>
        /// Should be used to finalize creation of a new collection and write contents to the model. This function must be called
        /// ONLY on a collection that has been created earlier using a default constructor. If an attempt is made to serialize
        /// an existing collection, an InvalidOperationException will be thrown!
        /// </summary>
        /// <param name="name">The name to be assigned to the collection.</param>
        /// <param name="package">The package that must hold the collection.</param>
        /// <param name="scope">The new collection scope.</param>
        /// <exception cref="ArgumentException">Is thrown when the selected package already contains a collection with the current name.</exception>
        /// <exception cref="InvalidOperationException">Is thrown when the collection has a scope other then 'Unknown'.</exception>
        internal void Serialize(string name, MEPackage package, CollectionScope scope)
        {
            if (this._collectionClass == null)
            {
                this._owningPackage = package;
                this._scope = scope;
                SetName(name);
            }
            else
            {
                string msg = "Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection.Serialize >> Attempt to serialize existing collection '" + this._collectionClass.Name + "'!";
                Logger.WriteError(msg);
                throw new InvalidOperationException(msg);
            }
        }

        /// <summary>
        /// Helper function that saves the specified descriptor as an attribute of the collection UML class. When the descriptor
        /// contains a payload class, we also create an association with that payload.
        /// Context must have been checked earlier, i.e. we assume that this is indeed a new attribute. The function does NOT perform
        /// any operations in case we don't (yet) have an UML class that can be used to persist the descriptor.
        /// </summary>
        /// <param name="desc">Descriptor to be converted into an attribute.</param>
        /// <exception cref="InvalidOperationException">Thrown when we could not create the attribute.</exception>
        private void CreateAttributeFromDescriptor(RESTOperationResultDescriptor desc)
        {
            if (this._collectionClass == null) return;

            ContextSlt context = ContextSlt.GetContextSlt();

            // Create an attribute of type 'unknown', which implies that the stereotype is left empty. This way, we can
            // assign our specific 'RCDStereotype' afterwards...
            MEAttribute newAttrib = this._collectionClass.CreateAttribute(desc.ResultCode, null, AttributeType.Unknown, null, 
                                                                          new Cardinality(Cardinality._Mandatory),
                                                                          false, desc.Description);
            if (newAttrib != null)
            {
                newAttrib.AddStereotype(context.GetConfigProperty(_RCDStereotype)); // Will define the tags below!
                newAttrib.SetTag(context.GetConfigProperty(_RCDReferenceURLTag), desc.ExternalReference, true);
                newAttrib.SetTag(context.GetConfigProperty(_RCDCategoryTag),
                                 EnumConversions<RESTOperationResultDescriptor.ResponseCategory>.EnumToString(desc.Category), true);
                newAttrib.SetTag(context.GetConfigProperty(_RCDPayloadKindTag),
                                 EnumConversions<RESTOperationResultDescriptor.ResultPayloadType>.EnumToString(desc.PayloadType), true);
                newAttrib.Index = desc.ToInt();

                // Check whether we must have an association with a payload class...
                if (desc.PayloadType == RESTOperationResultDescriptor.ResultPayloadType.DefaultResponse ||
                    desc.PayloadType == RESTOperationResultDescriptor.ResultPayloadType.CustomResponse ||
                    (Scope == CollectionScope.Operation && desc.PayloadType == RESTOperationResultDescriptor.ResultPayloadType.Document))
                {
                    MEClass target = desc.PayloadClass != null ? desc.PayloadClass : desc.Document.CapabilityClass; // One of these MUST be filled!
                    string endpointRole = desc.ResultCode + _ResponsePayloadRolePostfix;
                    var sourceEndpoint = new EndpointDescriptor(this._collectionClass, "1", _CollectionRole, null, false);
                    var payloadEndpoint = new EndpointDescriptor(target, desc.ResponseCardinality.ToString(), endpointRole, null, true);
                    this._collectionClass.CreateAssociation(sourceEndpoint, payloadEndpoint, MEAssociation.AssociationType.MessageAssociation);
                }
            }
            else
            {
                string message = "Unable to create attribute '" + desc.ResultCode + "' in collection '" + Name + "'!";
                Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection.CreateAttributeFromDescriptor >> " + message);
                throw new InvalidOperationException(message);
            }
        }

        /// <summary>
        /// Helper function that creates a new ResponseCodeDescriptor from an UML attribute in an existing collection. The function
        /// gathers information from the attribute as well as collection associations in order to construct the new descriptor.
        /// </summary>
        /// <param name="attrib">UML attribute to be used to create the new descriptor.</param>
        /// <exception cref="IllegalEnumException">Thrown in case we can't translate model strings to enumerations.</exception>
        /// <exception cref="InvalidOperationException">Is thrown when expected context is missing.</exception>
        private void CreateDescriptorFromAttribute(MEAttribute attrib)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string referenceURL = attrib.GetTag(context.GetConfigProperty(_RCDReferenceURLTag));
            var category = EnumConversions<RESTOperationResultDescriptor.ResponseCategory>.StringToEnum(attrib.GetTag(context.GetConfigProperty(_RCDCategoryTag)));
            var payloadType = EnumConversions<RESTOperationResultDescriptor.ResultPayloadType>.StringToEnum(attrib.GetTag(context.GetConfigProperty(_RCDPayloadKindTag)));
            MEClass payloadClass = null;
            Cardinality payloadCard = null;
            RESTResourceCapability documentResource = null;

            // Check whether we must have an association with a payload class...
            if (payloadType == RESTOperationResultDescriptor.ResultPayloadType.DefaultResponse ||
                payloadType == RESTOperationResultDescriptor.ResultPayloadType.CustomResponse ||
                (Scope == CollectionScope.Operation && payloadType == RESTOperationResultDescriptor.ResultPayloadType.Document))
            {
                string endpointRole = attrib.Name + _ResponsePayloadRolePostfix;
                foreach (MEAssociation assoc in this._collectionClass.AssociationList)
                {
                    if (assoc.Destination.Role == endpointRole)
                    {
                        payloadClass = assoc.Destination.EndPoint;
                        payloadCard = assoc.GetCardinality(MEAssociation.AssociationEnd.Destination);
                        break;
                    }
                }

                if (payloadClass == null)   // We must have found a payload somewhere...
                {
                    string message = "Missing expected payload class for response code '" + Name + "." + attrib.Name + "'!";
                    Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection.CreateDescriptorFromAttribute >> " + message);
                    throw new InvalidOperationException(message);
                }

                // In case of 'document' payload, we must convert the class to its corresponding capability object. We can simply instantiate
                // the capability using it's MEClass constructor, since it must exist already as part of the capability hierarchy...
                if (payloadType == RESTOperationResultDescriptor.ResultPayloadType.Document)
                {
                    documentResource = new RESTResourceCapability(payloadClass);
                    payloadClass = null;
                }
            }
            this._collection.Add(new RESTOperationResultDescriptor(this, attrib.Name, category, payloadType, attrib.Annotation, 
                                                                   referenceURL, payloadClass, documentResource, payloadCard));
        }

        /// <summary>
        /// If we have a global collection, the function locks the collection package for changes.
        /// </summary>
        private void LockCollection()
        {
            if (this._owningPackage != null && this._scope == CollectionScope.Global && !this._isLocked)
            {
                this._owningPackage.Lock();
                this._isLocked = true;
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
            string collectionStereotype = context.GetConfigProperty(_RCCStereotype);
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
                        string msg = "Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection.SetName >> New name '" + name + "' already exists!";
                        Logger.WriteError(msg);
                        throw new ArgumentException(msg);
                    }
                    else
                    {
                        // Check whether we already own a class (with another name)...
                        if (this._collectionClass != null)
                        {
                            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection.SetName >> Existing class with name '" +
                                             this._collectionClass.Name + "', rename to '" + name + "'...");
                            this._collectionClass.Name = name;
                        }
                        else
                        {
                            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection.SetName >> No class yet, creating one...");
                            try
                            {
                                this._collectionClass = this._owningPackage.CreateClass(name, collectionStereotype);
                                this._collectionID = this._collectionClass.ElementID.ToString();
                                this._collectionClass.SetTag(context.GetConfigProperty(_RCCIDTag), this._collectionID);
                                this._collectionClass.SetTag(context.GetConfigProperty(_RCCScopeTag), EnumConversions<CollectionScope>.EnumToString(this._scope));
                                if (this._collection.Count > 0)
                                {
                                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection.SetName >> We already have some attributes, move to new class...");
                                    foreach (RESTOperationResultDescriptor desc in this._collection) CreateAttributeFromDescriptor(desc);
                                }
                            }
                            catch (Exception exc)
                            {
                                Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection.SetName >> Error creating class '" + 
                                                  name + "' because:" + Environment.NewLine + exc.ToString());
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

        /// <summary>
        /// If we have a global collection, the function unlocks the collection package (we don't check whether the package is indeed locked).
        /// </summary>
        private void UnlockCollection()
        {
            if (this._owningPackage != null && this._scope == CollectionScope.Global && this._isLocked)
            {
                this._owningPackage.Unlock();
                this._isLocked = false;
            }
        }

        /// <summary>
        /// Helper function that is called when the user has edited an existing Operation Result Descriptor. The function
        /// locates the original attribute and replaces this with data from the new descriptor. It also renames a payload
        /// association (if one is present) or it creates a new association if the original one is no longer valid.
        /// </summary>
        /// <param name="oldDesc">The original descriptor that must be updated.</param>
        /// <param name="newDesc">The new descriptor, created by the user.</param>
        private void UpdateAttributeFromDescriptor(RESTOperationResultDescriptor oldDesc, RESTOperationResultDescriptor newDesc)
        {
            if (this._collectionClass == null) return;      // ignore in case we have not serialized our collection yet.

            ContextSlt context = ContextSlt.GetContextSlt();
            foreach (MEAttribute attrib in this._collectionClass.Attributes)
            {
                if (attrib.Name == oldDesc.ResultCode)
                {
                    LockCollection();
                    attrib.Name = newDesc.ResultCode;
                    attrib.Annotation = newDesc.Description;
                    attrib.SetTag(context.GetConfigProperty(_RCDReferenceURLTag), newDesc.ExternalReference, true);
                    attrib.SetTag(context.GetConfigProperty(_RCDCategoryTag),
                                     EnumConversions<RESTOperationResultDescriptor.ResponseCategory>.EnumToString(newDesc.Category));
                    attrib.SetTag(context.GetConfigProperty(_RCDPayloadKindTag),
                                     EnumConversions<RESTOperationResultDescriptor.ResultPayloadType>.EnumToString(newDesc.PayloadType));
                    attrib.Index = newDesc.ToInt();

                    string endpointOldRole = oldDesc.ResultCode + _ResponsePayloadRolePostfix;
                    string endpointNewRole = newDesc.ResultCode + _ResponsePayloadRolePostfix;

                    // In case the payload has been changed, replace the old- by the new code...
                    if (newDesc.PayloadChanged)
                    {
                        // If we did not previously had an association, just add the new one. Otherwise, remove old one first...
                        bool mustReplace = true;
                        if (oldDesc.PayloadClass != null || oldDesc.Document != null)
                        {
                            foreach (MEAssociation assoc in this._collectionClass.AssociationList)
                            {
                                if (assoc.Destination.Role == endpointOldRole)
                                {
                                    if (oldDesc.PayloadType != newDesc.PayloadType ||
                                        oldDesc.Document != newDesc.Document ||
                                        oldDesc.PayloadClass != newDesc.PayloadClass)
                                    {
                                        this._collectionClass.DeleteAssociation(assoc);
                                        break;
                                    }
                                    else mustReplace = false;   // This indicates that the new association is equal to the old, no need to replace!
                                }
                            }
                        }

                        // Check whether we need a new association with a different payload class...
                        if (mustReplace &&
                            (newDesc.PayloadType == RESTOperationResultDescriptor.ResultPayloadType.DefaultResponse ||
                             newDesc.PayloadType == RESTOperationResultDescriptor.ResultPayloadType.CustomResponse ||
                             (Scope == CollectionScope.Operation && newDesc.PayloadType == RESTOperationResultDescriptor.ResultPayloadType.Document)))
                        {
                            EndpointDescriptor sourceEndpoint = new EndpointDescriptor(this._collectionClass, "1", _CollectionRole, null, false);
                            EndpointDescriptor payloadEndpoint = new EndpointDescriptor(newDesc.PayloadType == RESTOperationResultDescriptor.ResultPayloadType.Document ?
                                                                                        newDesc.Document.CapabilityClass : newDesc.PayloadClass,
                                                                                        newDesc.ResponseCardinality.ToString(), endpointNewRole, null, true);
                            this._collectionClass.CreateAssociation(sourceEndpoint, payloadEndpoint, MEAssociation.AssociationType.MessageAssociation);
                        }
                    }
                    else if (newDesc.ResultCode != oldDesc.ResultCode && (oldDesc.PayloadClass != null || oldDesc.Document != null))
                    {
                        // Even when the payload has not changed, we might have to update the role in case the response code has been updated...
                        foreach (MEAssociation assoc in this._collectionClass.AssociationList)
                        {
                            if (assoc.Destination.Role == endpointOldRole)
                            {
                                assoc.SetName(endpointNewRole, MEAssociation.AssociationEnd.Destination);
                                
                                // Let's check whether we must update Cardinality as well...
                                if (oldDesc.ResponseCardinality != newDesc.ResponseCardinality)
                                    assoc.SetCardinality(newDesc.ResponseCardinality, MEAssociation.AssociationEnd.Destination);
                                break;
                            }
                        }
                    }
                    else if (oldDesc.ResponseCardinality != newDesc.ResponseCardinality)
                    {
                        // When the payload and role have not changed, the cardinality COULD have. So, check and adjust if necessary...
                        foreach (MEAssociation assoc in this._collectionClass.AssociationList)
                        {
                            if (assoc.Destination.Role == endpointNewRole)
                            {
                                assoc.SetCardinality(newDesc.ResponseCardinality, MEAssociation.AssociationEnd.Destination);
                                break;
                            }
                        }
                    }
                    UnlockCollection();
                    break;
                }
            }
        }
    }
}
