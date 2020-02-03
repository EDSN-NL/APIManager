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
    /// A simple helper class that bundles the components that make up a REST parameter (attributes of a Path Expression or operation).
    /// </summary>
    internal sealed class RESTOperationDeclaration: IEquatable<RESTOperationDeclaration>, IDisposable
    {
        // Configuration properties used by this module:
        private const string _RESTParameterStereotype       = "RESTParameterStereotype";

        private const string _Summary                       = "summary: ";      // Separator between summary text and description text.
        private const string _RspCollectionPostfix          = "Responses";      // Added to operation name to create the response collection name.

        // The status is used to track operations on the declaration.
        internal enum DeclarationStatus { Invalid, Created, Stable, Edited, Deleted }

        private string _name;                                       // Operation name.
        private HTTPOperation _operationType;                       // Associated HTTP operation type.
        private RESTResourceCapability _parent;                     // Represents the 'owner' of the operation.
        private MEClass _existingOperation;                         // Contains class in case operation has been constructed from existing class.
        private DeclarationStatus _status;                          // Status of this declaration record.
        private DeclarationStatus _initialStatus;                   // Original status of this declaration record.
        private RESTResourceCapability _requestDocument;            // Resource document to be used for request. This can be either a Document or a ProfileSet!.
        private Cardinality _requestCardinality;                    // Request cardinality.
        private bool _hasPagination;                                // Operation must implement default pagination mechanism.
        private bool _publicAccess;                                 // Security must be overruled for this operation.
        private bool _useLinkHeaders;                               // Operations uses response Link Headers.
        private RESTResponseCodeCollection _responseCollection;     // Contains the set of response codes for this operation.
        private List<RESTHeaderParameterDescriptor> _requestHeaderCollection;   // Contains the set of request header parameters.
        private List<string> _producedMIMEList;                     // Non-standard MIME types produced by the operation.
        private List<string> _consumedMIMEList;                     // Non-standard MIME types consumed by the operation.
        private string _summaryText;                                // Short description of operation.
        private string _description;                                // Long description of operation.
        private SortedList<string, RESTParameterDeclaration> _queryParams;          // List of user-defined query parameters for this operation.
        private bool _disposed;                                     // Mark myself as invalid after call to dispose!

        /// <summary>
        /// This is the normal entry for all users of the object that want to indicate that the resource declaration is not required anymore.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Get or set the associated HTTP operation for this operation declaration.
        /// </summary>
        internal HTTPOperation OperationType
        {
            get { return this._operationType; }
            set
            {
                if (this._operationType != value)
                {
                    this._operationType = value;
                    if (this._initialStatus == DeclarationStatus.Invalid && this._name != string.Empty) this._status = DeclarationStatus.Created;
                    else if (this._initialStatus != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                }
            }
        }

        /// <summary>
        /// Returns the list of MIME types consumed by the operation. Empty of only default MIME types are consumed.
        /// </summary>
        internal List<string> ConsumedMIMETypes { get { return this._consumedMIMEList; } }

        /// <summary>
        /// Get or set the long description of the operation.
        /// </summary>
        internal string Description
        {
            get { return this._description; }
            set { this._description = value; }
        }

        /// <summary>
        /// Parameter name.
        /// </summary>
        internal string Name
        {
            get { return this._name; }
            set
            {
                if (this._name != value)
                {
                    this._name = value;
                    if (this._initialStatus == DeclarationStatus.Invalid && this._operationType.TypeEnum != HTTPOperation.Type.Unknown) this._status = DeclarationStatus.Created;
                    else if (this._initialStatus != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                }
            }
        }

        /// <summary>
        /// If the operation declaration is based on an existing operation, this property returns the associated operation class.
        /// </summary>
        internal MEClass OperationClass { get { return this._existingOperation; } }

        /// <summary>
        /// Returns the list of request header parameters for the operation (if any)...
        /// </summary>
        internal List<RESTHeaderParameterDescriptor> RequestHeaderParameters { get { return this._requestHeaderCollection; } }

        /// <summary>
        /// Returns the list of operation response codes with their metadata.
        /// </summary>
        internal RESTResponseCodeCollection ResponseCollection { get { return this._responseCollection; } }

        /// <summary>
        /// Returns the resource capability that 'owns' this operation.
        /// </summary>
        internal RESTResourceCapability Parent { get { return this._parent; } }

        /// <summary>
        /// Get or set the 'has pagination' indicator
        /// </summary>
        internal bool PaginationIndicator
        {
            get { return this._hasPagination; }
            set
            {
                if (this._hasPagination != value)
                {
                    this._hasPagination = value;
                    if (this._initialStatus != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                }
            }
        }

        /// <summary>
        /// Returns the list of all query parameters defined for this operation.
        /// </summary>
        internal List<RESTParameterDeclaration> QueryParameters { get { return new List<RESTParameterDeclaration>(this._queryParams.Values); } }

        /// <summary>
        /// Returns the list of MIME types produced by the operation. Empty if only default MIME types are produced.
        /// </summary>
        internal List<string> ProducedMIMETypes { get { return this._producedMIMEList; } }

        /// <summary>
        /// Get or set the 'has public access' indicator
        /// </summary>
        internal bool PublicAccessIndicator
        {
            get { return this._publicAccess; }
            set
            {
                if (this._publicAccess != value)
                {
                    this._publicAccess = value;
                    if (this._initialStatus != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                }
            }
        }

        /// <summary>
        /// Get or set the Document Resource to be used as request body. Note that this can be either a Document or a Profile Set resource.
        /// </summary>
        internal RESTResourceCapability RequestDocument
        {
            get { return this._requestDocument; }
            set
            {
                if (this._requestDocument != value)
                {
                    this._requestDocument = value;
                    if (this._initialStatus != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                }
            }
        }

        /// <summary>
        /// Get or set the request Cardinality.
        /// </summary>
        internal Cardinality RequestCardinality
        {
            get { return this._requestCardinality; }
            set
            {
                if (this._requestCardinality != value)
                {
                    this._requestCardinality = value;
                    if (this._initialStatus != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                }
            }
        }

        /// <summary>
        /// Get or set the short operation description.
        /// </summary>
        internal string Summary
        {
            get { return this._summaryText; }
            set { this._summaryText = value; }
        }

        /// <summary>
        /// Get or set the status of this declaration record.
        /// </summary>
        internal DeclarationStatus Status
        {
            get { return this._status; }
            set { this._status = value; }
        }

        /// <summary>
        ///  Get or set the 'Use Link Headers' indicator.
        /// </summary>
        internal bool UseLinkHeaderIndicator
        {
            get { return this._useLinkHeaders; }
            set
            {
                if (this._useLinkHeaders != value)
                {
                    this._useLinkHeaders = value;
                    if (this._initialStatus != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                }
            }
        }

        /// <summary>
        /// Compares the Operation Declaration against another object. If the other object is also an Operation Declaration, the 
        /// function returns true if both Declarations are of the same operation type and have the same name. In all other cases,
        /// the function returns false.
        /// </summary>
        /// <param name="obj">The thing to compare against.</param>
        /// <returns>True if same object, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var objElement = obj as RESTOperationDeclaration;
            return (objElement != null) && Equals(objElement);
        }

        /// <summary>
        /// Compares the Operation Declaration against another Operation Declaration. The function returns true if both 
        /// Declarations are of the same operation type and have the same name. In all other cases, the function returns false.
        /// </summary>
        /// <param name="other">The Operation Declaration to compare against.</param>
        /// <returns>True if same object, false otherwise.</returns>
        public bool Equals(RESTOperationDeclaration other)
        {
            return other != null && other._operationType == this._operationType && other._name == this._name;
        }

        /// <summary>
        /// Returns a hashcode that is associated with the Operation Declaration. The hash code
        /// is derived from the Operation name and operation type.
        /// </summary>
        /// <returns>Hashcode according to Operation Declaration.</returns>
        public override int GetHashCode()
        {
            return this._name.GetHashCode() ^ this._operationType.GetHashCode();
        }

        /// <summary>
        /// Override of compare operator. Two Operation Declaration objects are equal if they are of the same HTTP Operator type,
        /// the same name or if they are both NULL.
        /// </summary>
        /// <param name="elementa">First Operation Declaration to compare.</param>
        /// <param name="elementb">Second Operation Declaration to compare.</param>
        /// <returns>True if the Operation Declarations are equal.</returns>
        public static bool operator ==(RESTOperationDeclaration elementa, RESTOperationDeclaration elementb)
        {
            // Tricky to implement correctly. These first statements make sure that we check whether we are actually
            // dealing with identical objects and/or whether one or both are NULL.
            if (ReferenceEquals(elementa, elementb)) return true;
            if (ReferenceEquals(elementa, null)) return false;
            if (ReferenceEquals(elementb, null)) return false;
            return elementa.Equals(elementb);
        }

        /// <summary>
        /// Override of compare operator. Two Operation Declaration objects are different if they have different HTTP Operator types,
        /// different names or one of them is NULL..
        /// </summary>
        /// <param name="elementa">First Operation Declaration to compare.</param>
        /// <param name="elementb">Second Operation Declaration to compare.</param>
        /// <returns>True if the Operation Declarations are different.</returns>
        public static bool operator !=(RESTOperationDeclaration elementa, RESTOperationDeclaration elementb)
        {
            return !(elementa == elementb);
        }

        /// <summary>
        /// Creates a new, incomplete, declaration descriptor using the provided parameters.
        /// </summary>
        /// <param name="name">The name of the operation (unique within the API).</param>
        /// <param name="archetype">The archetype of the operation (the associated HTTP operation).</param>
        internal RESTOperationDeclaration(RESTResourceCapability parent, string name, HTTPOperation operationType)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationDeclaration >> Creating new declaration with name '" + name + "' and type '" + operationType + "'...");
            this._name = name;
            this._parent = parent;
            this._operationType = operationType;
            this._existingOperation = null;
            this._status = DeclarationStatus.Invalid;
            this._initialStatus = this.Status;
            this._requestDocument = null;
            this._requestCardinality = new Cardinality(Cardinality._Mandatory);
            this._responseCollection = new RESTResponseCodeCollection(parent, RESTCollection.CollectionScope.Operation);
            this._requestHeaderCollection = new List<RESTHeaderParameterDescriptor>();
            this._hasPagination = false;
            this._publicAccess = false;
            this._producedMIMEList = new List<string>();
            this._consumedMIMEList = new List<string>();
            this._queryParams = new SortedList<string, RESTParameterDeclaration>();
            this._description = string.Empty;
            this._summaryText = string.Empty;
            this._useLinkHeaders = false;
            this._disposed = false;
        }

        /// <summary>
        /// This constructor creates a new operation declaration descriptor using an existing Operation Capability.
        /// </summary>
        /// <param name="operation">Operation Capability to use for initialisation.</param>
        internal RESTOperationDeclaration(RESTOperationCapability operation)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationDeclaration (capability) >> Creating new declaration using capability '" + operation.Name + "'...");
            this._name = operation.Name;
            this._parent = new RESTResourceCapability(operation.Parent as RESTResourceCapabilityImp);
            this._existingOperation = operation.CapabilityClass;
            this._operationType = operation.HTTPOperationType;
            this._status = DeclarationStatus.Stable;
            this._initialStatus = DeclarationStatus.Stable;
            this._hasPagination = operation.UsePagination;
            this._publicAccess = false;                                     // Not yet implemented!
            this._producedMIMEList = operation.ProducedMIMEList;
            this._consumedMIMEList = operation.ConsumedMIMEList;
            this._useLinkHeaders = operation.UseLinkHeaders;
            this._queryParams = new SortedList<string, RESTParameterDeclaration>();
            this._requestHeaderCollection = operation.RequestHeaders;

            // We create the collections in this manner (by instantiating from the model) to assure we get a separate copy from our Operation capability
            // that we can edit and otherwise modify without affecting the current operation settings. Please note that, in case the collection has been serialized,
            // We will STILL SHARE the UML class (when present) and thus updates WILL affect this class (but not the in-memory copy)!
            this._responseCollection = new RESTResponseCodeCollection(this._parent, this._name + _RspCollectionPostfix, operation.CapabilityClass.OwningPackage);

            this._summaryText = string.Empty;
            this._description = string.Empty;
            this._disposed = false;

            // Extract documentation from the class. This can be a multi-line object in which the first line might be the 'summary' description...
            List<string> documentation = MEChangeLog.GetDocumentationAsTextLines(operation.CapabilityClass);
            if (documentation.Count > 0)
            {
                if (documentation[0].StartsWith(_Summary, StringComparison.OrdinalIgnoreCase))
                {
                    this._summaryText = documentation[0].Substring(_Summary.Length);
                }
                if (documentation.Count > 1)
                {
                    this._description = string.Empty;
                    // We already copied the first line, so now append all additional lines...
                    // Since multi-line documentation does not really work well in JSON, we replace line breaks by spaces.
                    bool firstLine = true;
                    for (int i = 1; i < documentation.Count; i++)
                    {
                        this._description += firstLine ? documentation[i] : "  " + documentation[i];
                        firstLine = false;
                    }
                }
            }

            // Extract query parameters from the class. These are all attributes that have a scope of type 'Query'...
            string RESTParameterStereotype = ContextSlt.GetContextSlt().GetConfigProperty(_RESTParameterStereotype);
            foreach (MEAttribute attrib in operation.CapabilityClass.Attributes)
            {
                if (attrib.HasStereotype(RESTParameterStereotype))
                {
                    var paramDeclaration = new RESTParameterDeclaration(attrib);
                    if (paramDeclaration.Scope == RESTParameterDeclaration.ParameterScope.Query)
                    {
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationDeclaration(capability) >> Found query parameter '" + attrib.Name + "'...");
                        this._queryParams.Add(attrib.Name, paramDeclaration);
                    }
                }
            }

            // Load information regarding the request payload (response payload is part of the response collection).
            this._requestDocument = operation.RequestBodyDocument;
            this._requestCardinality = operation.RequestCardinality;
        }

        /// <summary>
        /// Add a new entry to the 'Consumed' MIME list. New entry is added only if not yet present in the list.
        /// </summary>
        /// <param name="MIMEType">Type to be added.</param>
        internal void AddConsumedMIMEType(string MIMEType)
        {
            if (!this._consumedMIMEList.Contains(MIMEType)) this._consumedMIMEList.Add(MIMEType);
        }

        /// <summary>
        /// Add a new entry to the 'Produced' MIME list. New entry is added only if not yet present in the list.
        /// </summary>
        /// <param name="MIMEType">Type to be added.</param>
        internal void AddProducedMIMEType(string MIMEType)
        {
            if (!this._producedMIMEList.Contains(MIMEType)) this._producedMIMEList.Add(MIMEType);
        }

        /// <summary>
        /// This function is invoked to add a new operation result to the operation. The actual dialog with the
        /// user is delegated to the collection.
        /// </summary>
        /// <returns>Newly created result descriptor or NULL in case of errors or duplicates or user cancel.</returns>
        internal RESTOperationResultDescriptor AddOperationResult()
        {
            RESTOperationResultDescriptor newDesc = this._responseCollection.AddOperationResult();
            if (newDesc != null && this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
            return newDesc;
        }

        /// <summary>
        /// This function receives a predefined OperationResultDescriptor and inserts the object in my result list (if no such code
        /// yet exists).
        /// </summary>
        /// <param name="result">The operation result to insert.</param>
        /// <returns>True if successfully inserted, false on duplicate code.</returns>
        internal bool AddOperationResult(RESTOperationResultDescriptor result)
        {
            RESTOperationResultDescriptor newDesc = this._responseCollection.AddOperationResult(result);
            if (newDesc != null && this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
            return newDesc != null;
        }

        /// <summary>
        /// This function is invoked to add one or more request header parameters to the operation. The actual dialog with the
        /// user is delegated to the header parameter manager at service level.
        /// </summary>
        /// <returns>Updated set of header parameters.</returns>
        internal List<RESTHeaderParameterDescriptor> AddHeaderParameters()
        {
            this._requestHeaderCollection = ((RESTService)this._parent.RootService).HeaderManager.ManageParameters(RESTServiceHeaderParameterMgr.Scope.Request, 
                                                                                                                   this._requestHeaderCollection);
            if (this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
            return this._requestHeaderCollection;
        }

        /// <summary>
        /// The method call will display the 'Edit Parameter' dialog so that the user can create a new parameter declaration object. 
        /// On return from the dialog, if the parameter settings are Ok, the parameter declaration is initialized and the created 
        /// parameter declaration object is returned to the caller.
        /// </summary>
        /// <returns>Created parameter declaration or NULL on cancel/errors.</returns>
        internal RESTParameterDeclaration AddParameter()
        {
            var newParam = new RESTParameterDeclaration();
            using (var dialog = new RESTParameterDialog(newParam))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (dialog.Parameter.Cardinality.IsList)
                    {
                        if (!(dialog.Parameter.Classifier is MEDataType) && !(dialog.Parameter.Classifier is MEEnumeratedType))
                        {
                            MessageBox.Show("Parameter must be Data Type or Enumeration, please try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return null;
                        }
                        if (dialog.Parameter.CollectionFormat == RESTParameterDeclaration.QueryCollectionFormat.NA ||
                            dialog.Parameter.CollectionFormat == RESTParameterDeclaration.QueryCollectionFormat.Unknown)
                        {
                            MessageBox.Show("Collection Format must be one of Multi, CSV, SSV, TSV or Pipes, please try again!",
                                            "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return null;
                        }
                    }
                    else dialog.Parameter.CollectionFormat = RESTParameterDeclaration.QueryCollectionFormat.NA;
                    newParam = dialog.Parameter;
                    newParam.Scope = RESTParameterDeclaration.ParameterScope.Query;
                    if (this._initialStatus != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                    this._queryParams.Add(newParam.Name, newParam);
                }
                else newParam = null;
            }
            return newParam;
        }

        /// <summary>
        /// Deletes a parameter from the list. In fact, the record is not actually removed but marked as 'deleted'.
        /// </summary>
        /// <param name="paramName">Name of the parameter to be deleted.</param>
        internal void DeleteParameter(string paramName)
        {
            if (this._queryParams.ContainsKey(paramName))
            {
                this._queryParams[paramName].Status = RESTParameterDeclaration.DeclarationStatus.Deleted;
                if (this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
            }
        }

        /// <summary>
        /// The method receives the name of an existing Query Parameter. It checks whether the operation indeed knows this parameter and if so,
        /// it presents the current contents in a dialog box for the user to edit. On return, the new parameter is written to the query list.
        /// If the parameter has been renamed by the user, the original record is deleted and replaced by the new record. If renaming resulted
        /// in a name clash, the operation is aborted.
        /// </summary>
        /// <returns>Updated parameter declaration or NULL on cancel/errors.</returns>
        internal RESTParameterDeclaration EditParameter(string paramName)
        {
            RESTParameterDeclaration editParam = null;
            if (this._queryParams.ContainsKey(paramName))
            {
                string originalKey = paramName;
                editParam = this._queryParams[paramName];
                using (var dialog = new RESTParameterDialog(editParam))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        if (!(dialog.Parameter.Classifier is MEDataType) && !(dialog.Parameter.Classifier is MEEnumeratedType))
                        {
                            MessageBox.Show("Parameter must be Data Type or Enumeration, please try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return null;
                        }
                        if (dialog.Parameter.Cardinality.IsList)
                        {
                            if (dialog.Parameter.CollectionFormat == RESTParameterDeclaration.QueryCollectionFormat.NA ||
                                dialog.Parameter.CollectionFormat == RESTParameterDeclaration.QueryCollectionFormat.Unknown)
                            {
                                MessageBox.Show("Collection Format must be one of Multi, CSV, SSV, TSV or Pipes, please try again!",
                                                "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return null;
                            }
                        }
                        else dialog.Parameter.CollectionFormat = RESTParameterDeclaration.QueryCollectionFormat.NA;
                        dialog.Parameter.Scope = RESTParameterDeclaration.ParameterScope.Query;

                        if (dialog.Parameter.Name == originalKey || !this._queryParams.ContainsKey(dialog.Parameter.Name))
                        {
                            editParam = dialog.Parameter;
                            if (this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;

                            if (editParam.Name != originalKey)
                            {
                                this._queryParams.Remove(originalKey);
                                this._queryParams.Add(editParam.Name, editParam);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Renaming parameter resulted in duplicate name, please try again!",
                                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            editParam = null;
                        }
                    }
                    else editParam = null;
                }
            }
            return editParam;
        }

        /// <summary>
        /// Remove the current Consumed MIME Types list.
        /// </summary>
        internal void ClearConsumedMIMETypes()
        {
            this._consumedMIMEList = new List<string>();
        }

        /// <summary>
        /// Remove the request document.
        /// </summary>
        internal void ClearDocument()
        {
            this._requestDocument = null;
        }

        /// <summary>
        /// Remove the current Consumed MIME Types list.
        /// </summary>
        internal void ClearProducedMIMETypes()
        {
            this._producedMIMEList = new List<string>();
        }

        /// <summary>
        /// Deletes an operation result from the list.
        /// </summary>
        /// <param name="code">Operation Result Code to be deleted.</param>
        internal void DeleteOperationResult(string code)
        {
            if (this._responseCollection.DeleteOperationResult(code) && this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
        }

        /// <summary>
        /// Deletes the specified request header parameter from the list.
        /// </summary>
        /// <param name="paramName">Header parameter to be deleted.</param>
        internal void DeleteHeaderParameter(string paramName)
        {
            foreach (RESTHeaderParameterDescriptor param in this._requestHeaderCollection)
            {
                if (param.Name == paramName)
                {
                    this._requestHeaderCollection.Remove(param);
                    if (this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                    break;
                }
            }
        }

        /// <summary>
        /// This function is invoked to edit an existing request header parameter for this operation. It displays the Header Parameter Dialog, which
        /// facilitates the user in changing the parameter. The updated object is added to the request parameter list for this operation as long as
        /// it has a unique name. The actual edit processing is 'delegated' to the header parameter manager at service level.
        /// </summary>
        /// <returns>Updated result record or NULL in case of errors or duplicates or user cancel.</returns>
        internal RESTHeaderParameterDescriptor EditHeaderParameter(string paramName)
        {
            RESTHeaderParameterDescriptor parameter = ((RESTService)this._parent.RootService).HeaderManager.EditParameter(RESTServiceHeaderParameterMgr.Scope.Request, paramName);
            if (parameter != null)
            {
                // Successfull edit, update our entry...
                for (int i = 0; i < this._requestHeaderCollection.Count; i++)
                {
                    if (this._requestHeaderCollection[i].Name == paramName)
                    {
                        this._requestHeaderCollection[i] = parameter;
                        break;
                    }
                }
                if (this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
            }
            return parameter;
        }

        /// <summary>
        /// This function is invoked to edit an existing Operation Result Declaration for this operation. It displays the Response Code Dialog, which
        /// facilitates the user in changing the result object. The updated object is added to the result list for this operation as long as
        /// it has a unique code. The actual edit processing is 'delegated' to our response code collection.
        /// </summary>
        /// <returns>Updated result record or NULL in case of errors or duplicates or user cancel.</returns>
        internal RESTOperationResultDescriptor EditOperationResult(string code)
        {
            RESTOperationResultDescriptor result = this._responseCollection.EditOperationResult(code);
            if (result != null && this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
            return result;
        }

        /// <summary>
        /// Checks whether the provided name is correct in the current context. If we have a valid parent capability, the name is
        /// checked against that capability. If we don't have a parent, we assume we are in 'create mode', in which case names are
        /// valid (since nothing has been created yet).
        /// </summary>
        /// <param name="operationName">Name to check.</param>
        /// <returns>True if no parent or name is correct in current context, false otherwise.</returns>
        internal bool IsValidName(string operationName)
        {
            return (this._parent != null) ? this._parent.RootService.DeclarationPkg.IsUniqueName(operationName) : true;
        }

        /// <summary>
        /// Assign the request payload document. We first retrieve the list of Document- and/or Profile Set Resources associated with 
        /// our parent resource. If there are multiple, we ask the user to select the one to use as request document. If there is only 
        /// one, we take this. If there are none, we display an error to the user.
        /// In short: We can ONLY link a Document / Profile Set resource to an operation AFTER the user has associated the operation 
        /// resource with at least one such resource!
        /// </summary>
        /// <returns>Name of selected resource or empty string in case of errors or cancel.</returns>
        internal string SetDocument()
        {
            string documentName = string.Empty;
            if (this._parent == null) return string.Empty;  // Nothing (yet) to do.

            // We retrieve the list(s) of Document- and Profile Set resources from our parent and merge them into a single list...
            List<RESTResourceCapability> targetResourceList = this._parent.ResourceList(RESTResourceCapability.ResourceArchetype.Document);
            List<RESTResourceCapability> profileList = this._parent.ResourceList(RESTResourceCapability.ResourceArchetype.ProfileSet);
            foreach (var resource in profileList) targetResourceList.Add(resource);
            if (targetResourceList.Count > 0)
            {
                // If we only have a single associated resource, this is selected automatically. When there are multiple,
                // we ask the user which one to use...
                if (targetResourceList.Count == 1)
                {
                    RESTResourceCapability document = targetResourceList[0];
                    this._requestDocument = document;
                    if (document != null) documentName = document.Name;
                }
                else
                {
                    List<Capability> capList = targetResourceList.ConvertAll(x => (Capability)x);
                    using (CapabilityPicker dialog = new CapabilityPicker("Select Document- / Profile Set resource", capList, false, false))
                    {
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            List<Capability> checkedCapabilities = dialog.GetCheckedCapabilities();
                            if (checkedCapabilities.Count > 0)
                            {
                                // If the user selected multiple, we take the first one.
                                RESTResourceCapability resource = checkedCapabilities[0] as RESTResourceCapability;
                                this._requestDocument = resource;
                                if (resource != null) documentName = resource.Name;
                            }
                        }
                        else documentName = string.Empty;
                    }
                }
            }
            else MessageBox.Show("No suitable Document- or Profile Set Resources to select, add one first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return documentName;
        }

        /// <summary>
        /// This is the actual disposing interface, which takes case of structural removal of the implementation type when no longer
        /// needed.
        /// </summary>
        /// <param name="disposing">Set to 'true' when called directly. Set to 'false' when called from the finalizer.</param>
        private void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                try
                {
                    if (this._existingOperation != null) this._existingOperation.Dispose();
                    this._parent = null;
                    this._existingOperation = null;
                    this._requestDocument = null;
                    this._responseCollection = null;
                    this._requestHeaderCollection = null;
                    this._producedMIMEList = null;
                    this._consumedMIMEList = null;
                    this._queryParams = null;
                    this._disposed = true;
                }
                catch { };   // Ignore any exceptions, no use in processing them here.
            }
        }
    }
}
