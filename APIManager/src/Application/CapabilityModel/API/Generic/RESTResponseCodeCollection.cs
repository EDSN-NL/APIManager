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
    internal sealed class RESTResponseCodeCollection: RESTCollection
    {
        // Configuration properties used by this module:
        private const string _RCCStereotype                         = "RCCStereotype";
        private const string _RCDStereotype                         = "RCDStereotype";
        private const string _RCDCategoryTag                        = "RCDCategoryTag";
        private const string _RCDPayloadKindTag                     = "RCDPayloadKindTag";
        private const string _RCDReferenceURLTag                    = "RCDReferenceURLTag";
        private const string _CoreDataTypesPathName                 = "CoreDataTypesPathName";
        private const string _ServiceModelPkgName                   = "ServiceModelPkgName";
        private const string _ServiceOperationPkgStereotype         = "ServiceOperationPkgStereotype";
        private const string _ServiceModelPkgStereotype             = "ServiceModelPkgStereotype";
        private const string _RCCHeadersTag                         = "RCCHeadersTag";

        // Together with the response code, this creates a role name for the payload class associated with a response (e.g. '200-Payload')...
        private const string _ResponsePayloadRolePostfix            = "-Payload";
        private const string _CollectionRole                        = "Collection"; // Used for the collection-end of the association

        private List<RESTOperationResultDescriptor> _collection;    // The actual collection.
        private RESTResourceCapability _parentResource;             // For operation-scoped collections, this is the declaration of the parent resource that contains the operation.
        private bool _mustInit;                                     // Indicates that init. has not been taken place.

        /// <summary>
        /// Returns the list of Operation Result Descriptors that comprises the collection.
        /// </summary>
        internal List<RESTOperationResultDescriptor> Collection { get { LoadDescriptors(); return this._collection; } }

        /// <summary>
        /// Returns the REST Resource that owns the operation, which in turn owns this response collection. This property is available only for
        /// Operation-scoped collections; in all other cases, it will return NULL.
        /// </summary>
        internal RESTResourceCapability ParentResource { get { return this._parentResource; } }

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
        internal RESTResponseCodeCollection(RESTResourceCapability parent, string collectionName, MEPackage package): base(collectionName, package)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> Creating instance '" +
                             collectionName + "' in Package '" + package.Parent.Name + "/" + package.Name + "'...");

            ContextSlt context = ContextSlt.GetContextSlt();
            string collectionStereotype = context.GetConfigProperty(_RCCStereotype);
            string responseDescriptorStereotype = context.GetConfigProperty(_RCDStereotype);
            this._parentResource = parent;
            this._collection = new List<RESTOperationResultDescriptor>();
            MEClass collectionClass = package.FindClass(collectionName, collectionStereotype);
            if (collectionClass != null)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> Existing collection class, lazy init!");
                InitCollectionClass(collectionClass);
                this._mustInit = true;
            }
            else
            {
                // We could not find an existing class, so we create a new, empty, collection...
                try
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> No class yet, creating one...");
                    SetScope(CollectionScope.Global);
                    if (package.HasStereotype(context.GetConfigProperty(_ServiceOperationPkgStereotype))) SetScope(CollectionScope.Operation);
                    else if (package.Name == context.GetConfigProperty(_ServiceModelPkgName) &&
                             package.HasStereotype(context.GetConfigProperty(_ServiceModelPkgStereotype))) SetScope(CollectionScope.API);
                    LockCollection();
                    CreateCollectionClass(collectionName, collectionStereotype);
                    this._mustInit = false;
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
        internal RESTResponseCodeCollection(RESTResourceCapability parent, MEClass collectionClass): base(collectionClass)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> Creating instance from class '" +
                              collectionClass.Name + "'...");

            ContextSlt context = ContextSlt.GetContextSlt();
            if (!collectionClass.HasStereotype(context.GetConfigProperty(_RCCStereotype)))
            {
                string msg = "Attempt to create collection from class with wrong stereotype '" + collectionClass.Name + "', ignored!";
                Logger.WriteWarning(msg);
                throw new InvalidOperationException(msg);
            }

            InitCollectionClass(collectionClass);
            this._parentResource = parent;
            this._collection = null;
            this._mustInit = true;      // We use lazy initialization and postpone loading descriptors until required.
        }

        /// <summary>
        /// The default constructor is used only when we want to create new (template) collections. In this case, we want to assign owning
        /// package, name and contents iteratively.
        /// </summary>
        /// <param name="parent">For non-template collections, this is the parent resource owning the collection.</param>
        /// <param name="initialScope">Contains the initial scope for the collection.</param>
        internal RESTResponseCodeCollection(RESTResourceCapability parent, CollectionScope initialScope = CollectionScope.Unknown): base(initialScope)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection >> Default constructor.");
            this._collection = new List<RESTOperationResultDescriptor>();
            this._parentResource = parent;
            this._mustInit = false;     // Actually, there is nothing to initialize here ;-)
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
            LoadDescriptors();      // Lazy initilization: complete contents of this collection!
            using (var dialog = new RESTResponseCodeDialog(this._parentResource != null? (RESTService)this._parentResource.RootService: null, this, null))
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
                LoadDescriptors();      // Lazy initilization: complete contents of this collection!
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
            LoadDescriptors();      // Lazy initilization: complete contents of this collection!
            foreach (RESTOperationResultDescriptor desc in this._collection)
            {
                if (desc.ResultCode == code)
                {
                    // If we have a serialized collection, we remove the UML attribute, associated payload association and header parameters (if any)...
                    if (CollectionClass != null)
                    {
                        ContextSlt context = ContextSlt.GetContextSlt();
                        foreach (MEAttribute attrib in CollectionClass.Attributes)
                        {
                            if (attrib.Name == code)
                            {
                                try
                                {
                                    LockCollection();
                                    CollectionClass.DeleteAttribute(attrib);

                                    // Check whether we have an association with a payload class...
                                    if (desc.PayloadType == RESTOperationResultDescriptor.ResultPayloadType.DefaultResponse ||
                                        desc.PayloadType == RESTOperationResultDescriptor.ResultPayloadType.CustomResponse ||
                                        (Scope == CollectionScope.Operation && desc.PayloadType == RESTOperationResultDescriptor.ResultPayloadType.Document))
                                    {
                                        string endpointRole = attrib.Name + _ResponsePayloadRolePostfix;
                                        List<MEAssociation> assoc = CollectionClass.FindAssociationsByAssociationProperties(null, endpointRole);
                                        if (assoc.Count > 0) CollectionClass.DeleteAssociation(assoc[0]);
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
            LoadDescriptors();      // Lazy initilization: complete contents of this collection!
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
                using (var dialog = new RESTResponseCodeDialog(this._parentResource != null? (RESTService)this._parentResource.RootService: null, this, new RESTOperationResultDescriptor(this, currentDesc)))
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
                string msg = "Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection.EditOperationResult >> Can't find existing attribute '" + CollectionClass.Name + "." + code + "'!";
                Logger.WriteError(msg);
                throw new ArgumentException(msg);
            }
            return editDesc;
        }

        /// <summary>
        /// Is called during transformation of an old response-header structure to the new version. The function receives a list of response
        /// header parameter descriptors and assigns this list to each result code in our collection...
        /// </summary>
        /// <param name="response">Response descriptor to save</param>
        internal void EnforceResponseHeaders(List<RESTHeaderParameterDescriptor> headers)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string attribStereotype = context.GetConfigProperty(_RCDStereotype);
            string responseHeadersTag = context.GetConfigProperty(_RCCHeadersTag);
            
            // Create an ID-set for our response code attributes...
            string newIDList = string.Empty;
            bool firstOne = true;
            foreach (RESTHeaderParameterDescriptor desc in headers)
            {
                newIDList += firstOne ? desc.ID.ToString() : "," + desc.ID.ToString();
                firstOne = false;
            }

            // Next, we go through our list of result descriptors, updating each in turn...
            foreach (RESTOperationResultDescriptor resultDesc in this._collection)
            {
                resultDesc.ResponseHeaders = headers;    // Replace the existing parameter list by this one.
                foreach (MEAttribute attrib in CollectionClass.Attributes)  // Locate the attribute and update the header tag...
                {
                    if (attrib.Name == resultDesc.ResultCode && attrib.HasStereotype(attribStereotype))
                    {
                        attrib.SetTag(responseHeadersTag, newIDList, true);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Helper function that saves the specified descriptor as an attribute of the collection UML class. When the descriptor
        /// contains a payload class, we also create an association with that payload.
        /// Context must have been checked earlier, i.e. we assume that this is indeed a new attribute. The function does NOT perform
        /// any operations in case we don't (yet) have an UML class that can be used to persist the descriptor.
        /// Since the descriptor can contain header parameters, this is the place where we must serialize and associate these parameter
        /// collections.
        /// </summary>
        /// <param name="desc">Descriptor to be converted into an attribute.</param>
        /// <exception cref="InvalidOperationException">Thrown when we could not create the attribute.</exception>
        private void CreateAttributeFromDescriptor(RESTOperationResultDescriptor desc)
        {
            if (CollectionClass == null) return;

            ContextSlt context = ContextSlt.GetContextSlt();

            // Create an attribute of type 'unknown', which implies that the stereotype is left empty. This way, we can
            // assign our specific 'RCDStereotype' afterwards...
            MEAttribute newAttrib = CollectionClass.CreateAttribute(desc.ResultCode, null, AttributeType.Unknown, null, 
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
                    var sourceEndpoint = new EndpointDescriptor(CollectionClass, "1", _CollectionRole, null, false);
                    var payloadEndpoint = new EndpointDescriptor(target, desc.ResponseCardinality.ToString(), endpointRole, null, true);
                    CollectionClass.CreateAssociation(sourceEndpoint, payloadEndpoint, MEAssociation.AssociationType.MessageAssociation);
                }

                // If we have a header parameter collection, convert to tag for the new attribute...
                CreateTagFromResponseHeaders(newAttrib, desc.ResponseHeaders);
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
            List<MEAssociation> assocList;

            // Check whether we must have an association with a payload class...
            if (payloadType == RESTOperationResultDescriptor.ResultPayloadType.DefaultResponse ||
                payloadType == RESTOperationResultDescriptor.ResultPayloadType.CustomResponse ||
                (Scope == CollectionScope.Operation && payloadType == RESTOperationResultDescriptor.ResultPayloadType.Document))
            {
                string endpointRole = attrib.Name + _ResponsePayloadRolePostfix;
                assocList = CollectionClass.FindAssociationsByAssociationProperties(null, endpointRole);
                if (assocList.Count > 0)    // Must be exactly one, when more are found (which is wrong), we use the first in the list.
                {
                    payloadClass = assocList[0].Destination.EndPoint;
                    payloadCard = assocList[0].GetCardinality(MEAssociation.AssociationEnd.Destination);
                    if (assocList.Count > 1) 
                        Logger.WriteWarning("Response code '" + attrib.Name + "' in collection '" + Name + "' has multiple payload classes!");
                }

                // We must have found a payload somewhere. If it's gone, we might have deleted something that we should not have deleted!
                if (payloadClass == null)   
                {
                    Logger.WriteWarning("Missing expected payload class for response code '" + Name + "." + attrib.Name + "', payload changed to NONE!");
                    payloadType = RESTOperationResultDescriptor.ResultPayloadType.None;
                }

                // In case of 'document' payload, we must convert the class to its corresponding capability object. We can simply instantiate
                // the capability using it's MEClass constructor, since it must exist already as part of the capability hierarchy...
                if (payloadType == RESTOperationResultDescriptor.ResultPayloadType.Document)
                {
                    documentResource = new RESTResourceCapability(payloadClass);
                    payloadClass = null;
                }
            }

            // We now have enough info to actually create the result descriptor. Headers are retrieved from the API-level header parameter manager
            // by means of the 'CreateResponseHeadersFromAttribute' helper function...
            this._collection.Add(new RESTOperationResultDescriptor(this, attrib.Name, category, payloadType, attrib.Annotation, 
                                                                   CreateResponseHeadersFromAttribute(attrib),
                                                                   referenceURL, payloadClass, documentResource, payloadCard));
        }

        /// <summary>
        /// Rename the collection. If another collection class already exists with the specified name, an exception will be thrown!
        /// And if the class does not yet exists, it is created with the given name and all currently collected Response Codes will be
        /// moved to the new class. If we have incorrect / inconsistent state (no package and scope are known yet), the
        /// operation is ignored but we'll store the name for later reference.
        /// </summary>
        /// <param name="name">(new) name for the class.</param>
        /// <exception cref="ArgumentException">Will be thrown when a class already exists with the given name.</exception>
        protected override void SetName(string name)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string collectionStereotype = context.GetConfigProperty(_RCCStereotype);
            if (OwningPackage != null)
            {
                // First of all, check spurious name assignments (nothing to do in that case)...
                if (CollectionClass != null && CollectionClass.Name == name) return;

                try
                {
                    LockCollection();
                    MEClass newClass = OwningPackage.FindClass(name, collectionStereotype);
                    if (newClass != null)
                    {
                        string msg = "Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection.SetName >> New name '" + name + "' already exists!";
                        Logger.WriteError(msg);
                        throw new ArgumentException(msg);
                    }
                    else
                    {
                        // Check whether we already own a class (with another name)...
                        if (CollectionClass != null)
                        {
                            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection.SetName >> Existing class with name '" +
                                             CollectionClass.Name + "', rename to '" + name + "'...");
                            CollectionClass.Name = name;
                        }
                        else
                        {
                            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection.SetName >> No class yet, creating one...");
                            try
                            {
                                CreateCollectionClass(name, collectionStereotype);
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
        /// Helper function that receives a Response Code attribute and extracts the list of Response Header parameters from that attribute. Since we
        /// only store Header ID's, we have to reach out the the API-wide header parameter manager to retrieve the actual parameters, which might have
        /// been deleted since the attribute has last been saved. If this is the case, we update the attribute to reflect current state so all is
        /// consistent on return.
        /// </summary>
        /// <param name="attrib">The attribute to convert.</param>
        /// <returns>List of response header parameters.</returns>
        private List<RESTHeaderParameterDescriptor> CreateResponseHeadersFromAttribute(MEAttribute attrib)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string responseHeadersTag = context.GetConfigProperty(_RCCHeadersTag);
            string responseHdrIDList = attrib.GetTag(responseHeadersTag);
            List<int> idList = new List<int>();
            List<RESTHeaderParameterDescriptor> responseHeaders = new List<RESTHeaderParameterDescriptor>();
            if (!string.IsNullOrEmpty(responseHdrIDList) && this._parentResource != null)
            {
                foreach (string ID in responseHdrIDList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    int paramID;
                    if (int.TryParse(ID.Trim(), out paramID)) idList.Add(paramID);
                }
                responseHeaders = ((RESTService)this._parentResource.RootService).HeaderManager.FindParametersByID(RESTServiceHeaderParameterMgr.Scope.Response, idList);
            }

            if (responseHeaders.Count < idList.Count)
            {
                // We have received less header parameters then ID's. This means that some of our ID's have been deleted!
                // Update the tag with the current, valid, ID set...
                responseHdrIDList = string.Empty;
                bool firstOne = true;
                foreach (RESTHeaderParameterDescriptor desc in responseHeaders)
                {
                    responseHdrIDList += firstOne ? desc.ID.ToString() : "," + desc.ID.ToString();
                    firstOne = false;
                }
                attrib.SetTag(responseHeadersTag, responseHdrIDList);
            }
            return responseHeaders;
        }

        /// <summary>
        /// Helper function that takes a list of response headers for a given response code and translates this into an attribute tag.
        /// </summary>
        /// <param name="attrib">The associated response code attribute to update.</param>
        /// <param name="responseHeaders">List of response headers that need to be stored.</param>
        private void CreateTagFromResponseHeaders(MEAttribute attrib, List<RESTHeaderParameterDescriptor> responseHeaders)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string responseHdrIDList = string.Empty;
            string responseHeadersTag = context.GetConfigProperty(_RCCHeadersTag);
            bool firstOne = true;
            foreach (RESTHeaderParameterDescriptor hdrDesc in responseHeaders)
            {
                responseHdrIDList += firstOne ? hdrDesc.ID.ToString() : "," + hdrDesc.ID.ToString();
                firstOne = false;
            }
            attrib.SetTag(responseHeadersTag, responseHdrIDList, true);
        }

        /// <summary>
        /// This helper function performs the final phase of class initialization by loading all ResponseCode descriptors from the model.
        /// Since this is a fairly expensive operation, we perform a lazy initialization and postpone the load until it is really necessary 
        /// (that is, when an action on the Response Code collection is required).
        /// </summary>
        private void LoadDescriptors()
        {
            if (this._mustInit)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection.LoadDescriptors >> Lazy init: complete delayed initialization...");
                ContextSlt context = ContextSlt.GetContextSlt();
                this._collection = new List<RESTOperationResultDescriptor>();
                string attribStereotype = context.GetConfigProperty(_RCDStereotype);

                foreach (MEAttribute attrib in CollectionClass.Attributes)
                {
                    if (attrib.HasStereotype(attribStereotype))
                    {
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTResponseCodeCollection.LoadDescriptors >> Found Response Code Descriptor '" +
                                         attrib.Name + "'...");
                        CreateDescriptorFromAttribute(attrib);
                    }
                }
                this._mustInit = false;
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
            if (CollectionClass == null) return;      // ignore in case we have not serialized our collection yet.

            ContextSlt context = ContextSlt.GetContextSlt();
            foreach (MEAttribute attrib in CollectionClass.Attributes)
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
                            foreach (MEAssociation assoc in CollectionClass.AssociationList)
                            {
                                if (assoc.Destination.Role == endpointOldRole)
                                {
                                    if (oldDesc.PayloadType != newDesc.PayloadType ||
                                        oldDesc.Document != newDesc.Document ||
                                        oldDesc.PayloadClass != newDesc.PayloadClass)
                                    {
                                        CollectionClass.DeleteAssociation(assoc);
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
                            EndpointDescriptor sourceEndpoint = new EndpointDescriptor(CollectionClass, "1", _CollectionRole, null, false);
                            EndpointDescriptor payloadEndpoint = new EndpointDescriptor(newDesc.PayloadType == RESTOperationResultDescriptor.ResultPayloadType.Document ?
                                                                                        newDesc.Document.CapabilityClass : newDesc.PayloadClass,
                                                                                        newDesc.ResponseCardinality.ToString(), endpointNewRole, null, true);
                            CollectionClass.CreateAssociation(sourceEndpoint, payloadEndpoint, MEAssociation.AssociationType.MessageAssociation);
                        }
                    }
                    else if (newDesc.ResultCode != oldDesc.ResultCode && (oldDesc.PayloadClass != null || oldDesc.Document != null))
                    {
                        // Even when the payload has not changed, we might have to update the role in case the response code has been updated...
                        List<MEAssociation> targetList = CollectionClass.FindAssociationsByAssociationProperties(null, endpointOldRole);
                        if (targetList.Count > 0)
                        {
                            targetList[0].SetName(endpointNewRole, MEAssociation.AssociationEnd.Destination);

                            // Let's check whether we must update Cardinality as well...
                            if (oldDesc.ResponseCardinality != newDesc.ResponseCardinality)
                                targetList[0].SetCardinality(newDesc.ResponseCardinality, MEAssociation.AssociationEnd.Destination);
                        }
                    }
                    else if (oldDesc.ResponseCardinality != newDesc.ResponseCardinality)
                    {
                        // When the payload and role have not changed, the cardinality COULD have. So, check and adjust if necessary...
                        List<MEAssociation> targetList = CollectionClass.FindAssociationsByAssociationProperties(null, endpointNewRole);
                        if (targetList.Count > 0) targetList[0].SetCardinality(newDesc.ResponseCardinality, MEAssociation.AssociationEnd.Destination);
                    }

                    // We simply take the contents of the new response headers and persist those in the attribute tag, overwriting whatever parameters
                    // used to be in there.
                    CreateTagFromResponseHeaders(attrib, newDesc.ResponseHeaders);

                    UnlockCollection();
                    break;
                }
            }
        }
    }
}
