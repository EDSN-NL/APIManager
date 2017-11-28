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
    internal sealed class RESTOperationDeclaration
    {
        // Configuration properties used by this module:
        private const string _APISupportModelPathName   = "APISupportModelPathName";
        private const string _OperationResultClassName  = "OperationResultClassName";

        // Separator between summary text and description text
        private const string _Summary                   = "summary: ";

        // The status is used to track operations on the declaration.
        internal enum DeclarationStatus { Invalid, Created, Edited, Deleted }

        private string _name;                                       // Operation name.
        private RESTOperationCapability.OperationType _archetype;   // Associated HTTP type.
        private Capability _parent;                                 // Represents the 'owner' of the operation.
        private MEClass _existingOperation;                         // Contains class in case operation has been constructed from existing class.
        private DeclarationStatus _status;                          // Status of this declaration record.
        private DeclarationStatus _initialStatus;                   // Original status of this declaration record.
        private bool _hasRequestParameters;                         // A message definition is present in the request body.
        private bool _hasResponseParameters;                        // A message definition is present in the response body.
        private bool _hasPagination;                                // Operation must implement default pagination mechanism.
        private bool _publicAccess;                                 // Security must be overruled for this operation.
        private SortedList<string, RESTOperationResultDeclaration> _resultList;     // Set of result declarations (one for each unique HTTP result code).
        private List<string> _producedMIMEList;                     // Non-standard MIME types produced by the operation.
        private List<string> _consumedMIMEList;                     // Non-standard MIME types consumed by the operation.
        private string _summaryText;                                // Short description of operation.
        private string _description;                                // Long description of operation.

        /// <summary>
        /// Get or set the long description of the operation.
        /// </summary>
        internal string Description
        {
            get { return this._description; }
            set { this._description = value; }
        }

        /// <summary>
        /// If the operation declaration is based on an existing operation, this property returns the associated operation class.
        /// </summary>
        internal MEClass OperationClass { get { return this._existingOperation; } }

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
        /// Returns the list of MIME types produced by the operation. Empty if only default MIME types are produced.
        /// </summary>
        internal List<string> ProducedMIMETypes { get { return this._producedMIMEList; } }

        /// <summary>
        /// Returns the list of MIME types consumed by the operation. Empty of only default MIME types are consumed.
        /// </summary>
        internal List<string> ConsumedMIMETypes { get { return this._consumedMIMEList; } }

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
        /// Get or set the short operation description.
        /// </summary>
        internal string Summary
        {
            get { return this._summaryText; }
            set { this._summaryText = value; }
        }

        /// <summary>
        /// Get or set the 'has request body' indicator.
        /// </summary>
        internal bool RequestBodyIndicator
        {
            get { return this._hasRequestParameters; }
            set
            {
                if (this._hasRequestParameters != value)
                {
                    this._hasRequestParameters = value;
                    if (this._initialStatus != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                }
            }
        }

        /// <summary>
        /// Get or set the 'has response body' indicator.
        /// </summary>
        internal bool ResponseBodyIndicator
        {
            get { return this._hasResponseParameters; }
            set
            {
                if (this._hasResponseParameters != value)
                {
                    this._hasResponseParameters = value;
                    if (this._initialStatus != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                }
            }
        }

        /// <summary>
        /// Returns the list of operation result declarations...
        /// </summary>
        internal List<RESTOperationResultDeclaration> OperationResults
        {
            get { return new List<RESTOperationResultDeclaration>(this._resultList.Values); }
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
                    if (this._initialStatus == DeclarationStatus.Invalid && this._archetype != RESTOperationCapability.OperationType.Unknown) this._status = DeclarationStatus.Created;
                    else if (this._initialStatus != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                }
            }
        }

        /// <summary>
        /// Get or set the archetype of this operation, i.e. the associated HTTP operation.
        /// </summary>
        internal RESTOperationCapability.OperationType Archetype
        {
            get { return this._archetype; }
            set
            {
                if (this._archetype != value)
                {
                    this._archetype = value;
                    if (this._initialStatus == DeclarationStatus.Invalid && this._name != string.Empty) this._status = DeclarationStatus.Created;
                    else if (this._initialStatus != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                }
            }
        }

        /// <summary>
        /// Default constructor creates an empty, illegal, operation declaration using "a" capability as parent.
        /// </summary>
        internal RESTOperationDeclaration(Capability parent)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationDeclaration >> Creating empty declaration.");
            this._name = string.Empty;
            this._parent = parent;
            this._existingOperation = null;
            this._archetype = RESTOperationCapability.OperationType.Unknown;
            this._status = this._initialStatus = DeclarationStatus.Invalid;
            this._hasRequestParameters = false;
            this._hasResponseParameters = false;
            this._hasPagination = false;
            this._publicAccess = false;
            this._producedMIMEList = new List<string>();
            this._consumedMIMEList = new List<string>();
            this._description = string.Empty;
            this._summaryText = string.Empty;

            CreateDefaultResults();
        }

        /// <summary>
        /// Creates a new declaration descriptor using the provided parameters.
        /// </summary>
        /// <param name="name">The name of the operation (unique within the API).</param>
        /// <param name="archetype">The archetype of the operation (the associated HTTP operation).</param>
        internal RESTOperationDeclaration(RESTResourceCapability parent, string name, RESTOperationCapability.OperationType archetype)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationDeclaration >> Creating new declaration with name '" + name + "' and type '" + archetype + "'...");
            this._name = name;
            this._parent = parent;
            this._archetype = archetype;
            this._existingOperation = null;
            this._status = DeclarationStatus.Invalid;
            if (this._name != string.Empty && this._archetype != RESTOperationCapability.OperationType.Unknown) this._status = DeclarationStatus.Created;
            this._initialStatus = this.Status;
            this._hasRequestParameters = false;
            this._hasResponseParameters = false;
            this._hasPagination = false;
            this._publicAccess = false;
            this._producedMIMEList = new List<string>();
            this._consumedMIMEList = new List<string>();
            this._description = string.Empty;
            this._summaryText = string.Empty;

            CreateDefaultResults();
        }

        /// <summary>
        /// This constructor creates a new operation declaration descriptor using an existing Operation Capability.
        /// TO DO: NOT ALL SETTINGS ARE PROPERLY COPIED FROM THE CAPABILITY!!!!
        /// </summary>
        /// <param name="operation">Operation Capability to use for initialisation.</param>
        internal RESTOperationDeclaration(RESTOperationCapability operation)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationDeclaration (capability) >> Creating new declaration using capability '" + operation.Name + "'...");
            this._name = operation.Name;
            this._parent = new RESTResourceCapability(operation.Parent as RESTResourceCapabilityImp);
            this._existingOperation = operation.CapabilityClass;
            this._archetype = operation.HTTPType;
            this._status = DeclarationStatus.Created;
            this._initialStatus = this.Status;
            this._hasRequestParameters = false;
            this._hasResponseParameters = false;
            this._hasPagination = false;
            this._publicAccess = false;
            this._producedMIMEList = operation.ProducedMIMEList;
            this._consumedMIMEList = operation.ConsumedMIMEList;
            
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
            CreateDefaultResults();
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
        /// This function is invoked to add a new Operation Result Declaration to this operation. It displays the Response Code Dialog, which
        /// facilitates the user in creating a new result object. The created object is added to the result list for this operation as long as
        /// it has either a unique code, or there exists a record with the same code and status 'deleted'. In the latter case, this record
        /// is overwritten and set to 'edited'.
        /// </summary>
        /// <returns>Newly created result record or NULL in case of errors or duplicates or user cancel.</returns>
        internal RESTOperationResultDeclaration AddOperationResult()
        {
            var newResult = new RESTOperationResultDeclaration(RESTOperationResultCapability.ResponseCategory.Unknown);
            using (var dialog = new RESTResponseCodeDialog(newResult))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (!this._resultList.ContainsKey(dialog.OperationResult.ResultCode))
                    {
                        dialog.OperationResult.Status = RESTOperationResultDeclaration.DeclarationStatus.Created;
                        this._resultList.Add(dialog.OperationResult.ResultCode, dialog.OperationResult);
                        if (this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                        newResult = dialog.OperationResult;
                    }
                    else if (this._resultList[dialog.OperationResult.ResultCode].Status == RESTOperationResultDeclaration.DeclarationStatus.Deleted)
                    {
                        dialog.OperationResult.Status = RESTOperationResultDeclaration.DeclarationStatus.Edited;
                        this._resultList[dialog.OperationResult.ResultCode] = dialog.OperationResult;
                        if (this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                        newResult = dialog.OperationResult;
                    }
                    else
                    {
                        MessageBox.Show("Duplicate operation result code, please try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        newResult = null;
                    }
                }
            }
            return newResult;
        }

        /// <summary>
        /// Assigns the specified parameter class to the result declaration identified by the specified code. If no result declaration exists
        /// for that code, the function returns NULL. Otherwise, it returns the affected Result Declaration object.
        /// Assigning a parameter class will overwrite any existing parameters for the result declaration!
        /// </summary>
        /// <param name="code">Result code to update.</param>
        /// <param name="parameterClass">The parameter class to assign to the result declaration.</param>
        /// <returns></returns>
        internal RESTOperationResultDeclaration AddResultParameters(string code, MEClass parameterClass)
        {
            if (this._resultList.ContainsKey(code))
            {
                this._resultList[code].Parameters = parameterClass;
                return this._resultList[code];
            }
            return null;
        }

        /// <summary>
        /// Remove the current Consumed MIME Types list.
        /// </summary>
        internal void ClearConsumedMIMETypes()
        {
            this._consumedMIMEList = new List<string>();
        }

        /// <summary>
        /// Remove the current Consumed MIME Types list.
        /// </summary>
        internal void ClearProducedMIMETypes()
        {
            this._producedMIMEList = new List<string>();
        }

        /// <summary>
        /// Deletes an operation result from the list. In fact, the record is not actually removed but marked as 'deleted'.
        /// </summary>
        /// <param name="code">Operation Result Code to be deleted.</param>
        internal void DeleteOperationResult(string code)
        {
            if (this._resultList.ContainsKey(code))
            {
                this._resultList[code].Status = RESTOperationResultDeclaration.DeclarationStatus.Deleted;
                if (this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
            }
        }

        /// <summary>
        /// This function is invoked to edit an existing Operation Result Declaration for this operation. It displays the Response Code Dialog, which
        /// facilitates the user in changing the result object. The updated object is added to the result list for this operation as long as
        /// it has either a unique code, or there exists a record with the same code and status 'deleted'. In the latter case, this record
        /// is overwritten and set to 'edited'.
        /// </summary>
        /// <returns>Updated result record or NULL in case of errors or duplicates or user cancel.</returns>
        internal RESTOperationResultDeclaration EditOperationResult(string code)
        {
            string originalKey = code;
            RESTOperationResultDeclaration result = null;
            if (this._resultList.ContainsKey(code))
            {
                result = this._resultList[code];
                using (var dialog = new RESTResponseCodeDialog(result))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        if (dialog.OperationResult.ResultCode == originalKey || !this._resultList.ContainsKey(dialog.OperationResult.ResultCode))
                        {
                            result = dialog.OperationResult;
                            result.Status = RESTOperationResultDeclaration.DeclarationStatus.Edited;

                            if (result.ResultCode != originalKey)
                            {
                                this._resultList.Remove(originalKey);
                                this._resultList.Add(result.ResultCode, result);
                            }
                            if (this._status != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                        }
                        else
                        {
                            MessageBox.Show("Changing result code resulted in duplicate code, please try again!",
                                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            result = null;
                        }
                    }
                }
            }
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
        /// Helper method that initialises the Operation Result list with four default responses: Default OK, Default Client Error, 
        /// Default Server Error and Generic (default) Error.
        /// </summary>
        private void CreateDefaultResults()
        {
            // Simplified response: only OK and default Error supported by default.
            this._resultList = new SortedList<string, RESTOperationResultDeclaration>();
            var defaultOk = new RESTOperationResultDeclaration(RESTOperationResultCapability.ResponseCategory.Success);
            var defaultResponse = new RESTOperationResultDeclaration(RESTOperationResultCapability.ResponseCategory.Default);
            this._resultList.Add(defaultOk.ResultCode, defaultOk);
            this._resultList.Add(defaultResponse.ResultCode, defaultResponse);

            // Associate the default error responses with a parameter type...
            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            MEClass resultParam = model.FindClass(context.GetConfigProperty(_APISupportModelPathName), context.GetConfigProperty(_OperationResultClassName));
            if (resultParam != null) defaultResponse.Parameters = resultParam;
            else Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTOperationDeclaration.CreateDefaultResults >> Unable to find '" + 
                                   context.GetConfigProperty(_APISupportModelPathName) + "/" + context.GetConfigProperty(_OperationResultClassName));
        }
    }
}
