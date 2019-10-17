using System;
using System.Collections.Generic;
using Framework.Model;
using Framework.Context;
using Framework.Logging;
using Framework.Util;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// Defines the structure of a single response code, including a definition of the associated payload (if any).
    /// </summary>
    internal sealed class RESTOperationResultDescriptor: IEquatable<RESTOperationResultDescriptor>
    {
        // The status is used to track operational state of the descriptor.
        internal enum DeclarationStatus { Invalid, Created, Stable, Edited, Deleted }

        // This enumeration is used to associate a type of response payload with the result code.
        internal enum ResultPayloadType
        {
            Unknown = 0,        // Not defined yet.
            None = 1,           // No payload, only a response code.
            Document = 2,       // Associated with a Document resource.
            Link = 3,           // Associated with an external (absolute) URL.
            CustomResponse = 4, // A custom class to be used as the payload.
            DefaultResponse = 5 // Default error response payload.
        }

        // Identifies the various HTTP Operation Result categories (Unknown is added only to specify an 'unknown' category and must never
        // be used in actual responses)...
        // We explicitly assign numeric values to the enumeration so that they match the HTTP prefix codes.
        internal enum ResponseCategory
        {
            Unknown = 0,        // 
            Informational = 1,  // Request received, continuing process.
            Success = 2,        // The action was successfully received, understood, and accepted.
            Redirection = 3,    // Further action must be taken in order to complete the request.
            ClientError = 4,    // The request contains bad syntax or cannot be fulfilled.
            ServerError = 5,    // The server failed to fulfill an apparently valid request.
            Default = 6         // Will cover all not-explicitly-defined result categories/codes.
        }

        internal static string _DefaultCode     = "default";
        internal static string _DefaultCodeText = "Default catch-all response code";

        // Conversion tables for each HTTP Response category..
        private static readonly string[,] _InformationResponses =
            { {"100", "Continue"},
              {"101", "Switching Protocols"},
              {"102", "Processing"},
              {"103", "Early Hints"},
              {_DefaultCode, _DefaultCodeText } };

        private static readonly string[,] _SuccessResponses =
            { {"200", "OK"},
              {"201", "Created"},
              {"202", "Accepted"},
              {"203", "Non-Authoritative Information"},
              {"204", "No Content"},
              {"205", "Reset Content"},
              {"206", "Partial Content"},
              {"207", "Multi-Status"},
              {"208", "Already Reported"},
              {"226", "IM Used"},
              {_DefaultCode, _DefaultCodeText } };

        private static readonly string[,] _RedirectionResponses =
            { {"300", "Multiple Choices"},
              {"301", "Moved Permanently"},
              {"302", "Found"},
              {"303", "See Other"},
              {"304", "Not Modified"},
              {"305", "Use Proxy"},
              {"307", "Temporary Redirect"},
              {"308", "Permanent Redirect"},
              {_DefaultCode, _DefaultCodeText } };

        private static readonly string[,] _ClientErrorResponses =
            { {"400", "Bad Request"},
              {"401", "Unauthorized"},
              {"402", "Payment Required"},
              {"403", "Forbidden"},
              {"404", "Not Found"},
              {"405", "Method Not Allowed"},
              {"406", "Not Acceptable"},
              {"407", "Proxy Authentication Required"},
              {"408", "Request Timeout"},
              {"409", "Conflict"},
              {"410", "Gone"},
              {"411", "Length Required"},
              {"412", "Precondition Failed"},
              {"413", "Payload Too Large"},
              {"414", "URI Too Long"},
              {"415", "Unsupported Media Type"},
              {"416", "Range Not Satisfiable"},
              {"417", "Expectation Failed"},
              {"421", "Misdirected Request"},
              {"422", "Unprocessable Entity"},
              {"423", "Locked"},
              {"424", "Failed Dependency"},
              {"426", "Upgrade Required"},
              {"428", "Precondition Required"},
              {"429", "Too Many Requests"},
              {"431", "Request Header Fields Too Large"},
              {"451", "Unavailable For Legal Reasons"},
              {_DefaultCode, _DefaultCodeText } };

        private static readonly string[,] _ServerErrorResponses =
            { {"500", "Internal Server Error"},
              {"501", "Not Implemented"},
              {"502", "Bad Gateway"},
              {"503", "Service Unavailable"},
              {"504", "Gateway Timeout"},
              {"505", "HTTP Version Not Supported"},
              {"506", "Variant Also Negotiates"},
              {"507", "Insufficient Storage"},
              {"508", "Loop Detected"},
              {"510", "Not Extended"},
              {"511", "Network Authentication Required"},
              {_DefaultCode, _DefaultCodeText } };

        // Configuration properties used by this module...
        private const string _OperationResultPrefix         = "OperationResultPrefix";
        private const string _APISupportModelPathName       = "APISupportModelPathName";
        private const string _CoreDataTypesPathName         = "CoreDataTypesPathName";
        private const string _RESTOperationResultStereotype = "RESTOperationResultStereotype";
        private const string _ResultCodeAttributeName       = "ResultCodeAttributeName";
        private const string _ResultCodeAttributeClassifier = "ResultCodeAttributeClassifier";
        private const string _ResourceClassStereotype       = "ResourceClassStereotype";
        private const string _OperationResultClassName      = "OperationResultClassName";

        private ResponseCategory _category;         // Operation result category code.
        private string _resultCode;                 // Either an HTTP result code or default OpenAPI response code.
        private string _originalCode;               // In case of Edit: if we replace the code by a new one, this contains the original code.
        private string _description;                // Descriptive text to go with the response.
        private bool _isTemplate;                   // Partial response code definition to be used as a template to create other instances.
        private RESTResponseCodeCollection _collection; // The collection to which this descriptor belongs.
        private ResultPayloadType _payloadType;     // Identifies the type of payload associated with this response code.
        private string _externalReference;          // URL of an external payload to be imported (not supported for all interface descriptors).
        private MEClass _responsePayloadClass;      // Contains the class that is assigned to this response as a payload.
        private Cardinality _responseCardinality;   // Cardinality of response payload (only if _responsePayloadClass has been defined).
        private DeclarationStatus _status;          // Descriptor status with regard to user editing.
        private DeclarationStatus _initialStatus;   // Original status with regard to user editing (used to detect changes).

        /// <summary>
        /// Returns the operation result category code.
        /// </summary>
        internal ResponseCategory Category { get { return this._category; } }

        /// <summary>
        /// Returns or loads the associated description text.
        /// </summary>
        internal string Description
        {
            get { return this._description; }
            set { this._description = value; }
        }

        /// <summary>
        /// Get or set an external reference associated with this response code. This must be an URL referencing some external schema fragment.
        /// </summary>
        internal string ExternalReference
        {
            get { return this._externalReference; }
            set
            {
                if (this._externalReference != value)
                {
                    this._externalReference = value;
                    if (this._initialStatus == DeclarationStatus.Invalid && this._resultCode != string.Empty) this._status = DeclarationStatus.Created;
                    else if (this._initialStatus != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                }
            }
        }

        /// <summary>
        /// Returns true in case the descriptor has been created as a template descriptor, which does not accept document / custom
        /// payload types.
        /// </summary>
        internal bool IsTemplate { get { return this._isTemplate; } }

        /// <summary>
        /// Get- or set the type of payload associated with this response code.
        /// </summary>
        internal ResultPayloadType PayloadType
        {
            get { return this._payloadType; }
            set { SetPayloadType(value); }
        }

        /// <summary>
        /// Get of set the cardinality of the response object.
        /// </summary>
        internal Cardinality ResponseCardinality
        {
            get { return this._responseCardinality; }
            set
            {
                if (this._responseCardinality != value)
                {
                    this._responseCardinality = value;
                    if (this._initialStatus == DeclarationStatus.Invalid && this._resultCode != string.Empty) this._status = DeclarationStatus.Created;
                    else if (this._initialStatus != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                }
            }
        }

        /// <summary>
        /// Get the Code value as it was BEFORE an edit operation on the result. Can be used to determine whether the object has been renamed.
        /// </summary>
        internal string OriginalCode { get { return this._originalCode; } }

        /// <summary>
        /// Returns or loads the class that must be used as response payload.
        /// Returns NULL if no such resource is defined.
        /// </summary>
        internal MEClass ResponsePayloadClass
        {
            get { return this._responsePayloadClass; }
            set
            {
                if (this._responsePayloadClass != value)
                {
                    this._responsePayloadClass = value;
                    if (this._initialStatus == DeclarationStatus.Invalid && this._resultCode != string.Empty) this._status = DeclarationStatus.Created;
                    else if (this._initialStatus != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                }
            }
        }

        /// <summary>
        /// Returns or loads the HTTP result code as a string. As a side effect, the associated description is loaded as well.
        /// </summary>
        internal string ResultCode
        {
            get { return this._resultCode; }
            set
            {
                if (this._resultCode != value)
                {
            ALS RESULTCODE EEN ANDERE WAARDE KRIJGT MOET OOK HET ATTRIBUUT IN DE COLLECTIE WORDEN AANGEPAST EN OOK ALLE ASSOCIATIES!!!!!!
                    this._resultCode = value;
                    this._description = GetResponseDescription();
                    if (this._initialStatus == DeclarationStatus.Invalid && this._resultCode != string.Empty) this._status = DeclarationStatus.Created;
                    else if (this._initialStatus != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                }
            }
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
        /// Returns 'true' if the declaration record has a valid status.
        /// </summary>
        internal bool IsValid
        {
            get
            {
                return this._status == DeclarationStatus.Created ||
                       this._status == DeclarationStatus.Edited ||
                       this._status == DeclarationStatus.Stable;
            }
        }

        /// <summary>
        /// Compares the Operation Result Declaration against another object. If the other object is also an Operation Result 
        /// Declaration, the function returns true if both Declarations have the same result code. In all other cases, the
        /// function returns false.
        /// </summary>
        /// <param name="obj">The thing to compare against.</param>
        /// <returns>True if same object, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var objElement = obj as RESTOperationResultDescriptor;
            return (objElement != null) && Equals(objElement);
        }

        /// <summary>
        /// Compares the Operation Result Declaration against another Operation Result Declaration. The function returns true 
        /// if both Declarations have identical result codes. In all other cases, the function returns false.
        /// </summary>
        /// <param name="other">The Operation Result Declaration to compare against.</param>
        /// <returns>True if same object, false otherwise.</returns>
        public bool Equals(RESTOperationResultDescriptor other)
        {
            return other != null && other._resultCode == this._resultCode;
        }

        /// <summary>
        /// Returns a hashcode that is associated with the Operation Result Declaration. The hash code
        /// is derived from the result code.
        /// </summary>
        /// <returns>Hashcode according to Operation Result Declaration.</returns>
        public override int GetHashCode()
        {
            return this._resultCode.GetHashCode();
        }

        /// <summary>
        /// Override of compare operator. Two Operation Result Declaration objects are equal if they have the same result code
        /// or if they are both NULL.
        /// </summary>
        /// <param name="elementa">First Operation Result Declaration to compare.</param>
        /// <param name="elementb">Second Operation Result Declaration to compare.</param>
        /// <returns>True if the Operation Result Declarations are equal.</returns>
        public static bool operator ==(RESTOperationResultDescriptor elementa, RESTOperationResultDescriptor elementb)
        {
            // Tricky to implement correctly. These first statements make sure that we check whether we are actually
            // dealing with identical objects and/or whether one or both are NULL.
            if (ReferenceEquals(elementa, elementb)) return true;
            if (ReferenceEquals(elementa, null)) return false;
            if (ReferenceEquals(elementb, null)) return false;
            return elementa.Equals(elementb);
        }

        /// <summary>
        /// Override of compare operator. Two Operation Result Declaration objects are different if they have different 
        /// result codes or one of them is NULL..
        /// </summary>
        /// <param name="elementa">First Operation Result Declaration to compare.</param>
        /// <param name="elementb">Second Operation Result Declaration to compare.</param>
        /// <returns>True if the Operation Declarations are different.</returns>
        public static bool operator !=(RESTOperationResultDescriptor elementa, RESTOperationResultDescriptor elementb)
        {
            return !(elementa == elementb);
        }

        /// <summary>
        /// This constructor creates an Operation Result Descriptor from an UML Attribute. The constructor
        /// also receives the Response Code Collection that acts as 'parent' for this descriptor.
        /// </summary>
        /// <param name="parent">Collection that 'owns' this operation result descriptor.</param>
        /// <param name="resultAttribute">UML attribute that describes this result descriptor.</param>
        internal RESTOperationResultDescriptor(RESTResponseCodeCollection parent, MEAttribute resultAttribute)
        {
            // TO BE IMPLEMENTED!
            // note: y/n template is defined by scope of parent.
        }

        /*********************
        /// <summary>
        /// This constructor creates a new operation result declaration descriptor using an existing Operation Result Capability.
        /// It copies attributes from the capability and if we have a response body, locates the associated Document Resource in order
        /// to set the 'cardinality indicator' to the appropriate value.
        /// </summary>
        /// <param name="resultCap">Operation Result Capability to use for initialisation.</param>
        internal RESTOperationResultDescriptor(RESTOperationResultCapability resultCap)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            this._category = resultCap.Category;
            this._description = MEChangeLog.GetDocumentationAsText(resultCap.CapabilityClass);
            this._status = DeclarationStatus.Stable;
            this._initialStatus = DeclarationStatus.Stable;
            this._responsePayloadClass = resultCap.ResponseBodyClass;
            this._externalReference = null;
            this._responseCardinality = new Cardinality();
            this._resultCode = this._originalCode = resultCap.ResultCode;
            this._payloadType = ResultPayloadType.Unknown;

            // If we have a response type, we locate the association and inspect its target cardinality...
            if (this._responsePayloadClass != null)
            {
                foreach (MEAssociation association in resultCap.CapabilityClass.TypedAssociations(MEAssociation.AssociationType.MessageAssociation))
                {
                    if (association.Destination.EndPoint.Name == this._responsePayloadClass.Name)
                    {
                        this._responseCardinality = association.GetCardinality(MEAssociation.AssociationEnd.Destination);
                        break;
                    }
                }
            }
            else DefineResponsePayloadType();    // If we did not get an explicit class from the result capabilty, we assign our default!
        }
        *************************/

        /// <summary>
        /// Default constructor, which accepts only a response category code and no further information. This constructs a descriptor of which
        /// the values depend on the category that has been passed:
        /// Success - Default response descriptor, no body, only a result code and default OK description.
        /// ClientError - Default client error code, standard Error body and default Client Error description.
        /// ServerError - Default server error code, standard Error body and default Server Error description.
        /// All others - descriptor containing 'default' code and default category-dependent description.
        /// </summary>
        /// <param name="parent">The collection that 'owns' this result descriptor.</param>
        /// <param name="category">The HTTP Result Code category (first digit in response code).</param>
        internal RESTOperationResultDescriptor(RESTResponseCodeCollection parent,  ResponseCategory category)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationResultDeclaration >> Default constructor using category '" + category.ToString() + "'...");

            ContextSlt context = ContextSlt.GetContextSlt();
            this._category = category;
            this._responseCardinality = new Cardinality();
            this._status = this._initialStatus = DeclarationStatus.Stable;
            this._externalReference = null;
            this._collection = parent;

            switch (category)
            {
                case ResponseCategory.Informational:
                    this._resultCode = "100";
                    this._description = GetResponseDescription();
                    break;

                case ResponseCategory.Success:
                    this._resultCode = "200";
                    this._description = GetResponseDescription();
                    break;

                case ResponseCategory.Redirection:
                    this._resultCode = "300";
                    this._description = GetResponseDescription();
                    break;

                case ResponseCategory.ClientError:
                    this._resultCode = "400";
                    this._description = GetResponseDescription();
                    break;

                case ResponseCategory.ServerError:
                    this._resultCode = "500";
                    this._description = GetResponseDescription();
                    break;

                case ResponseCategory.Default:
                    this._resultCode = _DefaultCode;
                    this._description = _DefaultCodeText;
                    break;

                default:
                    this._resultCode = _DefaultCode;
                    this._description = _DefaultCodeText;
                    this._status = this._initialStatus = DeclarationStatus.Invalid;
                    break;
            }
            this._originalCode = this._resultCode;
            DefineResponsePayloadType();
        }

        /// <summary>
        /// Constructor that accepts code, description and an optional payload (either a link or a class). 
        /// In case of normal successfull completion of a request that accepts response parameters, an OK Result Declaration 
        /// must be created that contains a class that specifies these parameters.
        /// Note that externalRef and payloadClass are mutually exclusive and when both are specified, payloadClass has precedence.
        /// </summary>
        /// <param name="parent">The collection that 'owns' this result descriptor.</param>
        /// <param name="code">HTTP Result code to be associated with this result.</param>
        /// <param name="description">Textual description.</param>
        /// <param name="externalRef">Optional URL that identifies an 'external' payload reference.</param>
        /// <param name="payloadClass">An optional response body payload class.</param>
        internal RESTOperationResultDescriptor(RESTResponseCodeCollection parent, string code, string description, string externalRef = null, MEClass payloadClass = null)
        {
            this._resultCode = this._originalCode = code;
            this._description = description;
            this._responsePayloadClass = payloadClass;
            this._externalReference = externalRef;
            this._category = (code == _DefaultCode)? 
                ResponseCategory.Default :
               (ResponseCategory)int.Parse(code[0].ToString());
            this._collection = parent;
            this._status = this._initialStatus = DeclarationStatus.Stable;
            this._responseCardinality = new Cardinality();
            DefineResponsePayloadType();
        }

        /// <summary>
        /// Constructor that accepts a code only. The constructor retrieves the default description for this code.
        /// </summary>
        /// <param name="parent">The collection that 'owns' this result descriptor.</param>
        /// <param name="code">HTTP Result code to be associated with this result.</param>
        internal RESTOperationResultDescriptor(RESTResponseCodeCollection parent, string code)
        {
            this._resultCode = this._originalCode = code;
            this._description = CodeDescriptor.CodeToDescription(code);
            this._responsePayloadClass = null;
            this._externalReference = null;
            this._category = (code == _DefaultCode) ?
                ResponseCategory.Default :
               (ResponseCategory)int.Parse(code[0].ToString());
            this._collection = parent;
            this._status = this._initialStatus = DeclarationStatus.Stable;
            this._responseCardinality = new Cardinality();
            DefineResponsePayloadType();
        }

        /// <summary>
        /// Convert the Operation Result Descriptor into an attribute of the Collection UML Class. When the attribute has a payload
        /// class, an association between the collection and that payload class is also created, where the association role corresponds
        /// with the operation result code.
        /// When the collection already contains an attribute with the same code, this is removed first (as is the optional association
        /// with the payload class).
        /// </summary>
        /// <returns>Newly created attribute.</returns>
        internal void CreateAttributeInCollection()
        {

        }

        /// <summary>
        /// This helper function transforms the current operation declaration to an Operation Result Class. The class is created as a child of the 
        /// specified owning package.
        /// </summary>
        /// <param name="parent">Package in which we're going to create the result Class.</param>
        /// <returns>Created result Class or NULL in case of errors.</returns>
        internal MEClass ConvertToClass(MEPackage owningPackage)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            MEClass resultClass = null;
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationResultDeclaration.AssignResultClass >> Creating class in parent '" + owningPackage.Name + "'...");
            if (this._status != DeclarationStatus.Deleted && this._status != DeclarationStatus.Invalid)
            {
                string name = context.GetConfigProperty(_OperationResultPrefix) + this._resultCode;
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationResultDeclaration.AssignResultClass >> Creating class with name '" + name + "'...");
                resultClass = owningPackage.CreateClass(name, context.GetConfigProperty(_RESTOperationResultStereotype));
                MEDataType classifier = model.FindDataType(context.GetConfigProperty(_CoreDataTypesPathName), context.GetConfigProperty(_ResultCodeAttributeClassifier));
                if (classifier != null)
                {
                    MEAttribute newAttrib = resultClass.CreateAttribute(context.GetConfigProperty(_ResultCodeAttributeName), classifier,
                                                                        AttributeType.Attribute, this._resultCode, new Cardinality(Cardinality._Mandatory), true);
                    resultClass.Annotation = this._description;
                    if (this._responsePayloadClass != null)
                    {
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationResultDeclaration.AssignResultClass >> Associating with response type '" + this._responsePayloadClass.Name + "'...");
                        string roleName = Conversions.ToPascalCase(this._responsePayloadClass.Name);
                        if (roleName.EndsWith("Type")) roleName = roleName.Substring(0, roleName.IndexOf("Type"));
                        var parentEndpoint = new EndpointDescriptor(resultClass, "1", resultClass.Name, null, false);
                        var typeEndpoint = new EndpointDescriptor(this._responsePayloadClass, "1", roleName, null, true);
                        model.CreateAssociation(parentEndpoint, typeEndpoint, MEAssociation.AssociationType.MessageAssociation);
                    }
                }
                else Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTOperationResultDeclaration.AssignResultClass >> Error retrieving result classifier '" +
                                       context.GetConfigProperty(_ResultCodeAttributeClassifier) + "' from path '" + context.GetConfigProperty(_CoreDataTypesPathName) + "'!");
            }
            else Logger.WriteError("Plugin.Application.CapabilityModel.API.RESTOperationResultDeclaration.AssignResultClass >> Declaration is in deleted status, can't convert to class!");
            return resultClass;
        }

        /// <summary>
        /// Returns a list of code descriptors for the current response category. 
        /// </summary>
        /// <returns>List of code/description tuples for current category.</returns>
        internal List<CodeDescriptor> GetResponseCodes()
        {
            var resultList = new List<CodeDescriptor>();
            switch(this._category)
            {
                case ResponseCategory.Informational:
                    for (int i = 0; i < _InformationResponses.GetLength(0); i++)
                        resultList.Add(new CodeDescriptor(_InformationResponses[i, 0], _InformationResponses[i, 1]));
                    break;

                case ResponseCategory.Success:
                    for (int i = 0; i < _SuccessResponses.GetLength(0); i++)
                        resultList.Add(new CodeDescriptor(_SuccessResponses[i, 0], _SuccessResponses[i, 1]));
                    break;

                case ResponseCategory.Redirection:
                    for (int i = 0; i < _RedirectionResponses.GetLength(0); i++)
                        resultList.Add(new CodeDescriptor(_RedirectionResponses[i, 0], _RedirectionResponses[i, 1]));
                    break;

                case ResponseCategory.ClientError:
                    for (int i = 0; i < _ClientErrorResponses.GetLength(0); i++)
                        resultList.Add(new CodeDescriptor(_ClientErrorResponses[i, 0], _ClientErrorResponses[i, 1]));
                    break;

                case ResponseCategory.ServerError:
                    for (int i = 0; i < _ServerErrorResponses.GetLength(0); i++)
                        resultList.Add(new CodeDescriptor(_ServerErrorResponses[i, 0], _ServerErrorResponses[i, 1]));
                    break;

                default:
                    resultList.Add(new CodeDescriptor(_DefaultCode, _DefaultCodeText));
                    break;
            }
            return resultList;
        }

        /// <summary>
        /// This function is invoked on deletion of the response descriptor from the parent collection. The function deletes associations
        /// between the paren collection and payload classes 'owned' by this result and it removes the UML attribute from the collection
        /// class. On return, one should NOT use this descriptor anymore!
        /// </summary>
        internal void Invalidate()
        {
            // DO DELETE STUFF HERE
            this._responsePayloadClass = null;
            this._externalReference = null;
            this._category = ResponseCategory.Unknown;
            this._collection = null;
            this._status = this._initialStatus = DeclarationStatus.Invalid;
            this._responseCardinality = null;
        }

        /// <summary>
        /// Helper method that attempts to define the response type for this operation result and, in case of default response,
        /// locates the default error response parameter class and assigns this to the response declaration object. 
        /// </summary>
        private void DefineResponsePayloadType()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            if (this._resultCode == _DefaultCode) this._payloadType = ResultPayloadType.DefaultResponse;
            else if (this._responsePayloadClass == null)
            {
                if (string.IsNullOrEmpty(this._externalReference)) 
                {
                    if (this._category == ResponseCategory.ClientError ||
                        this._category == ResponseCategory.ServerError)
                    {
                        this._payloadType = ResultPayloadType.DefaultResponse;
                    }
                    else this._payloadType = ResultPayloadType.None;
                }
                else this._payloadType = ResultPayloadType.Link;
            }
            else
            {
                string resourceStereotype = context.GetConfigProperty(_ResourceClassStereotype);
                string defaultResponseName = context.GetConfigProperty(_OperationResultClassName);
                if (this._responsePayloadClass.HasStereotype(resourceStereotype)) this._payloadType = ResultPayloadType.Document;
                else if (this._responsePayloadClass.Name == defaultResponseName) this._payloadType = ResultPayloadType.DefaultResponse;
                else this._payloadType = ResultPayloadType.CustomResponse;
            }

            if (this._payloadType == ResultPayloadType.DefaultResponse && this._responsePayloadClass == null)
            {
                ModelSlt model = ModelSlt.GetModelSlt();
                this._responsePayloadClass = model.FindClass(context.GetConfigProperty(_APISupportModelPathName), context.GetConfigProperty(_OperationResultClassName));
                if (this._responsePayloadClass == null)
                    Logger.WriteError("Plugin.Application.Forms.RESTOperationResultDeclaration.AssignParameterClass >> Unable to find '" +
                                       context.GetConfigProperty(_APISupportModelPathName) + "/" + context.GetConfigProperty(_OperationResultClassName));
            }
        }

        /// <summary>
        /// Returns the description that belongs the current category and code.
        /// </summary>
        /// <returns>Description for current category + code.</returns>
        private string GetResponseDescription()
        {
            string description = string.Empty;
            switch (this._category)
            {
                case ResponseCategory.Informational:
                    for (int i = 0; i < _InformationResponses.GetLength(0); i++)
                        if (string.Compare(_InformationResponses[i, 0], this._resultCode, true) == 0) description= _InformationResponses[i, 1];
                    break;

                case ResponseCategory.Success:
                    for (int i = 0; i < _SuccessResponses.GetLength(0); i++)
                        if (string.Compare(_SuccessResponses[i, 0], this._resultCode, true) == 0) description = _SuccessResponses[i, 1];
                    break;

                case ResponseCategory.Redirection:
                    for (int i = 0; i < _RedirectionResponses.GetLength(0); i++)
                        if (string.Compare(_RedirectionResponses[i, 0], this._resultCode, true) == 0) description = _RedirectionResponses[i, 1];
                    break;

                case ResponseCategory.ClientError:
                    for (int i = 0; i < _ClientErrorResponses.GetLength(0); i++)
                        if (string.Compare(_ClientErrorResponses[i, 0], this._resultCode, true) == 0) description = _ClientErrorResponses[i, 1];
                    break;

                case ResponseCategory.ServerError:
                    for (int i = 0; i < _ServerErrorResponses.GetLength(0); i++)
                        if (string.Compare(_ServerErrorResponses[i, 0], this._resultCode, true) == 0) description = _ServerErrorResponses[i, 1];
                    break;

                default:
                    description = _DefaultCodeText;
                    break;
            }
            return description;
        }

        /// <summary>
        /// Helper function that is used to assign a (new) value to the Result Payload Type field. Depending on the type,
        /// we might expect additional information (such as external classes) to be assigned as well before we can consider
        /// this to be a valid response code.
        /// </summary>
        /// <param name="newType">The new type to be assigned. If new type is equal to existing type, no operations are performed.</param>
        private void SetPayloadType(ResultPayloadType newType)
        {
            if (this._payloadType != newType)
            {
                this._payloadType = newType;
                switch (newType)
                {
                    case ResultPayloadType.CustomResponse:
                    case ResultPayloadType.Document:
                    case ResultPayloadType.Link:
                        this._externalReference = string.Empty;
                        this._responsePayloadClass = null;
                        this._status = DeclarationStatus.Invalid;   // We must assign a valid payload/link to get a valid response.
                        break;

                    case ResultPayloadType.DefaultResponse:
                        if (this._category == ResponseCategory.ClientError ||
                            this._category == ResponseCategory.ServerError)
                            DefineResponsePayloadType();
                        else this._responsePayloadClass = null;
                        this._externalReference = string.Empty;
                        if (this._initialStatus == DeclarationStatus.Invalid && this._resultCode != string.Empty) this._status = DeclarationStatus.Created;
                        else if (this._initialStatus != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                        break;

                    case ResultPayloadType.None:
                        this._externalReference = string.Empty;
                        this._responsePayloadClass = null;
                        if (this._initialStatus == DeclarationStatus.Invalid && this._resultCode != string.Empty) this._status = DeclarationStatus.Created;
                        else if (this._initialStatus != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Helper class that facilitates translation between code, description and human friendly labels.
    /// </summary>
    internal sealed class CodeDescriptor
    {
        private string _code;           // Actual HTTP Code.
        private string _description;    // Descriptive text for the code.

        internal string Code { get { return this._code; } }
        internal string Description { get { return this._description; } }
        internal string Label { get { return this._code + " - " + this._description; } }

        /// <summary>
        /// Create a new CodeDescriptor object based on Code and Description.
        /// </summary>
        /// <param name="code">HTTP Response Code.</param>
        /// <param name="description">Associated description.</param>
        internal CodeDescriptor(string code, string description)
        {
            this._code = code;
            this._description = description;
        }

        /// <summary>
        /// Helper function that returns the original Code for a given Label.
        /// </summary>
        /// <param name="label">Label to convert.</param>
        /// <returns>Code that corresponds with the label.</returns>
        static internal string LabelToCode(string label)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            return label.Substring(0, label.IndexOf(" - "));
        }

        /// <summary>
        /// Helper function that takes a code and fetches the default description text for that code.
        /// </summary>
        /// <param name="code">Code to translate.</param>
        /// <returns>Associated description or empty string in case of illegal codes.</returns>
        static internal string CodeToDescription(string code)
        {
            try
            {
                // Since this is non-numeric, we must test this one explicitly...
                if (code == _DefaultCode) return _DefaultCodeText;

                var category = (RESTOperationResultCapability.ResponseCategory)int.Parse(code[0].ToString());
                var result = new RESTOperationResultDescriptor(category);
                List<CodeDescriptor> codes = result.GetResponseCodes();
                foreach (CodeDescriptor dsc in codes) if (dsc.Code == code) return dsc.Description;
                return string.Empty;
            }
            catch { return string.Empty; }
        }

        /// <summary>
        /// Helper function that takes a code and translates this into a human-friendly label.
        /// </summary>
        /// <param name="code">Code to translate.</param>
        /// <returns>Associated label or empty string in case of illegal codes.</returns>
        static internal string CodeToLabel(string code)
        {
            string description = CodeToDescription(code);
            return description != string.Empty ? code + " - " + description : string.Empty;
        }
    }
}
