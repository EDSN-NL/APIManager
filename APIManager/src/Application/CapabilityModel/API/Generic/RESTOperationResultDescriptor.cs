using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Model;
using Framework.Context;
using Framework.Logging;
using Framework.Util;
using Plugin.Application.Forms;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// Defines the structure of a single response code, including a definition of the associated payload (if any).
    /// </summary>
    internal sealed class RESTOperationResultDescriptor: IEquatable<RESTOperationResultDescriptor>
    {
        // The status is used to track operational state of the descriptor. The descriptor has 'valid' state when all properties
        // are consistent in relationship with each other.
        internal enum DeclarationStatus { Invalid, Valid }

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
            Unknown = 0,        // Not yet initialized properly.
            Informational = 1,  // Request received, continuing process.
            Success = 2,        // The action was successfully received, understood, and accepted.
            Redirection = 3,    // Further action must be taken in order to complete the request.
            ClientError = 4,    // The request contains bad syntax or cannot be fulfilled.
            ServerError = 5,    // The server failed to fulfill an apparently valid request.
            Default = 6         // Will cover all not-explicitly-defined result categories/codes.
        }

        internal static string _DefaultCode     = "default";
        internal static string _DefaultCodeText = "Default catch-all response code";
        internal static string _RangePostfix    = "XX";     // Used, in combination with the category, to determine a range of codes.
        internal static string _DefaultPostfix  = "00";     // Used to reset a category to a default value.  

        // Conversion tables for each HTTP Response category..
        private static readonly string[,] _InformationResponses =
            { {"100", "Continue"},
              {"101", "Switching Protocols"},
              {"102", "Processing"},
              {"103", "Early Hints"},
              {"1XX", "Range of Information Response codes" },
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
              {"2XX", "Range of Success Response codes" },
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
              {"3XX", "Range of Redirection Response codes" },
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
              {"4XX", "Range of Client Error Response codes" },
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
              {"5XX", "Range of Server Error Response codes" },
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
        private string _resultCode;                 // Either an HTTP result code, a range (nXX) or default OpenAPI response code.
        private string _description;                // Descriptive text to go with the response.
        private bool _isTemplate;                   // Partial response code definition to be used as a template to create other instances.
        private RESTResponseCodeCollection _collection; // The collection to which this descriptor belongs.
        private ResultPayloadType _payloadType;     // Identifies the type of payload associated with this response code.
        private string _externalReference;          // URL of an external payload to be imported (not supported for all interface descriptors).
        private RESTResourceCapability _document;   // Document payload in case of document-type payload.
        private MEClass _payloadClass;              // Contains either the default- or custom response class.
        private Cardinality _responseCardinality;   // Cardinality of response payload (only if _responsePayloadClass has been defined).
        private DeclarationStatus _status;          // Descriptor status with regard to user editing.
        private bool _payloadChange;                // Set to 'true' in case payload has been changed due to edit/add operation.

        /// <summary>
        /// Returns the operation result category code.
        /// </summary>
        internal ResponseCategory Category
        {
            get { return this._category; }
            set { SetCategory(value); }
        }

        /// <summary>
        /// Returns or loads the associated description text.
        /// </summary>
        internal string Description
        {
            get { return this._description; }
            set { this._description = value; }
        }

        /// <summary>
        /// Get or set the Document payload resource.
        /// </summary>
        internal RESTResourceCapability Document
        {
            get { return this._document; }
            set
            {
                if (this._document != value)
                {
                    this._document = value;
                    DetermineStatus();
                }
            }
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
                    if (!string.IsNullOrEmpty(value))
                    {
                        this._payloadType = ResultPayloadType.Link;
                        this._status = DeclarationStatus.Valid;
                    }
                    DetermineStatus();
                }
            }
        }

        /// <summary>
        /// Returns true in case the descriptor has been created as a template descriptor, which does not accept document / custom
        /// payload types.
        /// </summary>
        internal bool IsTemplate { get { return this._isTemplate; } }

        /// <summary>
        /// Returns 'true' if the declaration record has a valid, consistent, status.
        /// </summary>
        internal bool IsValid { get { return this._status == DeclarationStatus.Valid; } }

        /// <summary>
        /// Returns 'true' in case payload has been changed due to edit- or add operation.
        /// </summary>
        internal bool PayloadChanged { get { return this._payloadChange; } }

        /// <summary>
        /// Get- or set the type of payload associated with this response code.
        /// </summary>
        internal ResultPayloadType PayloadType
        {
            get { return this._payloadType; }
            set
            {
                SetPayloadType(value);
                DetermineStatus();
            }
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
                    DetermineStatus();
                }
            }
        }

        /// <summary>
        /// Returns or loads the class that must be used as response payload in case of default / custom response class types.
        /// Returns NULL if no such resource is defined.
        /// </summary>
        internal MEClass PayloadClass
        {
            get { return this._payloadClass; }
            set
            {
                if (this._payloadClass != value)
                {
                    this._payloadClass = value;
                    DetermineStatus();
                }
            }
        }

        /// <summary>
        /// Returns or loads the HTTP result code as a string. As a side effect, default description, category and payload-type are (re-)defined
        /// as well.
        /// </summary>
        internal string ResultCode
        {
            get { return this._resultCode; }
            set
            {
                if (this._resultCode != value)
                {
                    this._resultCode = value;
                    if (!string.IsNullOrEmpty(value))
                    {
                        this._description = GetResponseDescription();
                        this._category = (value == _DefaultCode) ?
                                          ResponseCategory.Default :
                                          (ResponseCategory)int.Parse(value[0].ToString());
                        DefineResponsePayloadType();
                    }
                    DetermineStatus();
                }
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
        /// This constructor creates an Operation Result Descriptor from its set of specified components. The constructor
        /// also receives the Response Code Collection that acts as 'parent' for this descriptor.
        /// </summary>
        /// <param name="parent">Collection that 'owns' this operation result descriptor.</param>
        /// <param name="code">HTTP response code (or 'default' or range specifier nXX)</param>
        /// <param name="category">HTTP response category code.</param>
        /// <param name="payloadType">Payload type code.</param>
        /// <param name="description">Response descriptive text.</param>
        /// <param name="referenceURL">Optional external link to be used as response payload.</param>
        /// <param name="responsePayload">Optional UML class to be used as response payload.</param>
        /// <param name="document">Optional Document resource to be used as response payload.</param>
        /// <param name="responseCardinality">Optional cardinality of either 'responsePayload' or 'document'.</param>
        internal RESTOperationResultDescriptor(RESTResponseCodeCollection parent, 
                                               string code,
                                               ResponseCategory category,
                                               ResultPayloadType payloadType,
                                               string description,
                                               string referenceURL = null,
                                               MEClass responsePayload = null,
                                               RESTResourceCapability document = null,
                                               Cardinality responseCardinality = null)
        {
            this._category = category;
            this._resultCode = code;
            this._description = description;
            this._isTemplate = parent.Scope != RESTResponseCodeCollection.CollectionScope.Operation;
            this._collection = parent;
            this._payloadType = payloadType;
            this._externalReference = referenceURL != null ? referenceURL : string.Empty;
            this._payloadClass = responsePayload;
            this._document = document;
            this._responseCardinality = responseCardinality != null? responseCardinality: new Cardinality(Cardinality._Mandatory);
            this._payloadChange = false;
            DetermineStatus();
    }

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
        internal RESTOperationResultDescriptor(RESTResponseCodeCollection parent, ResponseCategory category)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationResultDescriptor >> Default constructor using category '" + category.ToString() + "'...");

            ContextSlt context = ContextSlt.GetContextSlt();
            this._category = category;
            this._responseCardinality = new Cardinality(Cardinality._Mandatory);
            this._payloadClass = null;
            this._payloadType = ResultPayloadType.Unknown;
            this._document = null;
            this._externalReference = null;
            this._collection = parent;
            this._isTemplate = parent.Scope != RESTResponseCodeCollection.CollectionScope.Operation;
            this._payloadChange = false;

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
                    break;
            }
            DefineResponsePayloadType();
            DetermineStatus();
        }

        /// <summary>
        /// Constructor that accepts a code only. The constructor retrieves the default description for this code.
        /// </summary>
        /// <param name="parent">The collection that 'owns' this result descriptor.</param>
        /// <param name="code">HTTP Result code to be associated with this result.</param>
        internal RESTOperationResultDescriptor(RESTResponseCodeCollection parent, string code)
        {
            this._resultCode = code;
            this._description = CodeDescriptor.CodeToDescription(code);
            this._payloadClass = null;
            this._payloadType = ResultPayloadType.Unknown;
            this._document = null;
            this._responseCardinality = new Cardinality(Cardinality._Mandatory);
            this._externalReference = null;
            this._category = (code == _DefaultCode) ? ResponseCategory.Default : (ResponseCategory)int.Parse(code[0].ToString());
            this._collection = parent;
            this._isTemplate = parent.Scope != RESTResponseCodeCollection.CollectionScope.Operation;
            this._payloadChange = false;
            DefineResponsePayloadType();
            DetermineStatus();
        }

        /// <summary>
        /// Constructor that creates a new descriptor from a given descriptor instance. 
        /// Be careful to use this copy constructor on the same parent so we don't get duplicate codes!
        /// The receiving descriptor may- or may not be a template, this depends on the role of the parent.
        /// </summary>
        /// <param name="parent">The collection that 'owns' this current result descriptor.</param>
        /// <param name="original">The descriptor that is used as a 'template' for construction.</param>
        internal RESTOperationResultDescriptor(RESTResponseCodeCollection parent, RESTOperationResultDescriptor original)
        {
            this._resultCode = original._resultCode;
            this._description = original._description;
            this._payloadType = original._payloadType;
            this._document = original._document;
            this._payloadClass = original._payloadClass != null ? new MEClass(original._payloadClass) : null;
            this._responseCardinality = new Cardinality(original._responseCardinality);
            this._externalReference = original._externalReference;
            this._category = original._category;
            this._collection = parent;
            this._isTemplate = parent.Scope != RESTResponseCodeCollection.CollectionScope.Operation;
            this._payloadChange = false;
            DetermineStatus();
        }

        /// <summary>
        /// Static helper function that returns a list of code descriptors for a given category. 
        /// </summary>
        /// <returns>List of code/description tuples for current category.</returns>
        static internal List<CodeDescriptor> GetResponseCodes(ResponseCategory category)
        {
            var resultList = new List<CodeDescriptor>();
            bool isOpenAPI20 = ContextSlt.GetContextSlt().GetStringSetting(FrameworkSettings._OpenAPIVersion) == FrameworkSettings._OpenAPIVersion20;

            switch (category)
            {
                case ResponseCategory.Informational:
                    for (int i = 0; i < _InformationResponses.GetLength(0); i++)
                        if (!isOpenAPI20 || !_InformationResponses[i,0].Contains(_RangePostfix))
                            resultList.Add(new CodeDescriptor(_InformationResponses[i, 0], _InformationResponses[i, 1]));
                    break;

                case ResponseCategory.Success:
                    for (int i = 0; i < _SuccessResponses.GetLength(0); i++)
                        if (!isOpenAPI20 || !_SuccessResponses[i, 0].Contains(_RangePostfix))
                            resultList.Add(new CodeDescriptor(_SuccessResponses[i, 0], _SuccessResponses[i, 1]));
                    break;

                case ResponseCategory.Redirection:
                    for (int i = 0; i < _RedirectionResponses.GetLength(0); i++)
                        if (!isOpenAPI20 || !_RedirectionResponses[i, 0].Contains(_RangePostfix))
                            resultList.Add(new CodeDescriptor(_RedirectionResponses[i, 0], _RedirectionResponses[i, 1]));
                    break;

                case ResponseCategory.ClientError:
                    for (int i = 0; i < _ClientErrorResponses.GetLength(0); i++)
                        if (!isOpenAPI20 || !_ClientErrorResponses[i, 0].Contains(_RangePostfix))
                            resultList.Add(new CodeDescriptor(_ClientErrorResponses[i, 0], _ClientErrorResponses[i, 1]));
                    break;

                case ResponseCategory.ServerError:
                    for (int i = 0; i < _ServerErrorResponses.GetLength(0); i++)
                        if (!isOpenAPI20 || !_ServerErrorResponses[i, 0].Contains(_RangePostfix))
                            resultList.Add(new CodeDescriptor(_ServerErrorResponses[i, 0], _ServerErrorResponses[i, 1]));
                    break;

                default:
                    resultList.Add(new CodeDescriptor(_DefaultCode, _DefaultCodeText));
                    break;
            }
            return resultList;
        }

        /// <summary>
        /// Depending on context, if we need a Document, we first retrieve the list of Document Resources associated with our parent resource.
        /// If there are multiple, we ask the user to select the one to use as response document. If there is only one, we take this. 
        /// If there are none, we display an error to the user.
        /// If we need a custom response, the function simply invokes the 'class selector' for the user to select any class that should
        /// be used as custom response document.
        /// Finally, if we need a default response, the function selects and loads the default response class.
        /// </summary>
        /// <returns>Name of selected resource or empty string in case of errors or cancel.</returns>
        internal string SetDocument()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            string documentName = string.Empty;

            if (this._payloadType == ResultPayloadType.Document)
            {
                this._payloadChange = this._payloadClass != null;
                this._payloadClass = null;
                int oldID = this._document != null ? this._document.CapabilityClass.ElementID : -1;

                if (this._collection.ParentResource == null) return string.Empty;    // Context incomplete or wrong. Do nothing.
                List<RESTResourceCapability> documentList = this._collection.ParentResource.ResourceList(RESTResourceCapability.ResourceArchetype.Document);
                if (documentList.Count > 0)
                {
                    // If we only have a single associated Document Resource, this is selected automatically. When there are multiple,
                    // we ask the user which one to use...
                    if (documentList.Count == 1)
                    {
                        this._document = documentList[0];
                        documentName = this._document.Name;
                        if (!this._payloadChange) this._payloadChange = this._document.CapabilityClass.ElementID != oldID;
                    }
                    else
                    {
                        List<Capability> capList = documentList.ConvertAll(x => (Capability)x);
                        using (CapabilityPicker dialog = new CapabilityPicker("Select Document resource", capList, false, false))
                        {
                            if (dialog.ShowDialog() == DialogResult.OK)
                            {
                                List<Capability> checkedCapabilities = dialog.GetCheckedCapabilities();
                                if (checkedCapabilities.Count > 0)
                                {
                                    // If the user selected multiple, we take the first one.
                                    this._document = checkedCapabilities[0] as RESTResourceCapability;
                                    documentName = this._document.Name;
                                    if (!this._payloadChange) this._payloadChange = this._document.CapabilityClass.ElementID != oldID;
                                }
                            }
                            else documentName = string.Empty;
                        }
                    }
                }
                else MessageBox.Show("No suitable Document Resources to select, add one first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (this._payloadType == ResultPayloadType.CustomResponse)
            {
                this._payloadChange = this._document != null;
                this._document = null;
                MEClass payload = context.SelectClass();
                int oldID = this._payloadClass != null ? this._payloadClass.ElementID : -1;
                if (payload != null)
                {
                    this._payloadClass = payload;
                    documentName = payload.Name;
                    this._payloadChange |= this._payloadClass == null || this._payloadClass.ElementID != oldID;
                }
            }
            else if (this._payloadType == ResultPayloadType.DefaultResponse)
            {
                this._payloadChange = this._document != null || this._payloadClass == null;
                this._document = null;
                int oldID = this._payloadClass != null ? this._payloadClass.ElementID : -1;
                this._payloadClass = model.FindClass(context.GetConfigProperty(_APISupportModelPathName), context.GetConfigProperty(_OperationResultClassName));
                if (this._payloadClass != null)
                {
                    documentName = this._payloadClass.Name;
                    this._payloadChange |= this._payloadClass.ElementID != oldID;
                }
                else Logger.WriteError("Plugin.Application.Forms.RESTOperationResultDescriptor.SetDocument >> Unable to find '" +
                                       context.GetConfigProperty(_APISupportModelPathName) + "/" + context.GetConfigProperty(_OperationResultClassName));
            }
            DetermineStatus();
            return documentName;
        }

        /// <summary>
        /// Convert the response code to an integer representation.Typically, the result is the numerical representation of the code (e.g. 404 or 500).
        /// In case of a 'range' (e.g. 4XX), the result is category * 100 + 99 (e.g. 4XX --> 499).
        /// In case of the 'default' code, the return value is 9999.
        /// In all other cases, the function returns 0.
        /// </summary>
        /// <returns>Integer representation of the response code.</returns>
        internal int ToInt()
        {
            if (this._resultCode == _DefaultCode) return 9999;
            else if (this._resultCode.Contains(_RangePostfix)) return ((int)this._category * 100) + 99;
            else 
            {
                int value;
                if (int.TryParse(this._resultCode, out value)) return value;
            }
            return 0;
        }

        /// <summary>
        /// Helper method which, in case of an undefined payload type, attempts to define the response type for this operation result and in 
        /// case of default response, locates the default error response parameter class and assigns this to the response declaration object.
        /// When the payload type is already set, the operation does not perform any actions!
        /// </summary>
        private void DefineResponsePayloadType()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            if (this._payloadType != ResultPayloadType.Unknown) return;     // Payload type already set, ignore!

            if (this._resultCode == _DefaultCode) this._payloadType = ResultPayloadType.DefaultResponse;
            else if (this._payloadClass == null && this._document == null)
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
            else if (this._document != null)
            {
                this._payloadType = ResultPayloadType.Document;
            }
            else
            {
                if (this._payloadClass.Name == context.GetConfigProperty(_OperationResultClassName)) this._payloadType = ResultPayloadType.DefaultResponse;
                else this._payloadType = ResultPayloadType.CustomResponse;
            }

            if (this._payloadType == ResultPayloadType.DefaultResponse) SetDocument();  // Assure that the default response class is present.
            else if (this._payloadType == ResultPayloadType.None || this._payloadType == ResultPayloadType.Link)
            {
                this._payloadChange = this._payloadClass != null || this._document != null;
                this._payloadClass = null;
                this._document = null;
            }
        }

        /// <summary>
        /// This function checks the values of our properties and determines the current status of the descriptor.
        /// Status will only be valid in case property values match the payload type and we have a valid code and category.
        /// </summary>
        private void DetermineStatus()
        {
            if (string.IsNullOrEmpty(this._resultCode) || this._category == ResponseCategory.Unknown)
            {
                this._status = DeclarationStatus.Invalid;
            }
            else
            {
                switch (this._payloadType)
                {
                    case ResultPayloadType.DefaultResponse:
                        if (this._category == ResponseCategory.ClientError || this._category == ResponseCategory.ServerError)
                        {
                            // In case of default response, the payload class must have been configured, even in case of template!
                            if (this._payloadClass != null && this._responseCardinality != null) this._status = DeclarationStatus.Valid;
                            else this._status = DeclarationStatus.Invalid;
                        }
                        else this._status = DeclarationStatus.Invalid;  // Default responses can only be present in client- and server errors.
                        break;

                    case ResultPayloadType.CustomResponse:
                        // Can be present in templates as well as operation responses and if present, MUST have a associated class!
                        if (this._payloadClass != null && this._responseCardinality != null) this._status = DeclarationStatus.Valid;
                        else this._status = DeclarationStatus.Invalid;
                        break;

                    case ResultPayloadType.Document:
                        // Templates ignore these response types (can be any value).
                        if (this._isTemplate) this._status = DeclarationStatus.Valid;
                        else
                        {
                            if (this._document != null && this._responseCardinality != null) this._status = DeclarationStatus.Valid;
                            else this._status = DeclarationStatus.Invalid;
                        }
                        break;

                    case ResultPayloadType.Link:
                        // A template may or may not have a link configured, we typically ignore the current value.
                        if (this._isTemplate) this._status = DeclarationStatus.Valid;
                        else
                        {
                            if (!string.IsNullOrEmpty(this._externalReference)) this._status = DeclarationStatus.Valid;
                            else this._status = DeclarationStatus.Invalid;
                        }
                        break;

                    case ResultPayloadType.None:
                        this._status = this._payloadClass == null &&
                                       this._document == null &&
                                       string.IsNullOrEmpty(this._externalReference) ? DeclarationStatus.Valid : DeclarationStatus.Invalid;
                        break;

                    default:
                        this._status = DeclarationStatus.Invalid;
                        break;
                }
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
        /// Helper function that is used to update the category of this descriptor. Changing the category might have effect on the
        /// payload type as well since not all types are supported for all categories. More specifically, the 'default' response
        /// type is supported ONLY for Default, Client- and Server error and not for the other categories.
        /// </summary>
        /// <param name="newCategory">New category for this descriptor.</param>
        private void SetCategory (ResponseCategory newCategory)
        {
            if (this._category != newCategory)
            {
                this._category = newCategory;
                switch (newCategory)
                {
                    case ResponseCategory.Informational:
                        this._resultCode = "100";
                        this._description = GetResponseDescription();
                        if (this._payloadType == ResultPayloadType.DefaultResponse) SetPayloadType(ResultPayloadType.None);
                        break;

                    case ResponseCategory.Success:
                        this._resultCode = "200";
                        this._description = GetResponseDescription();
                        if (this._payloadType == ResultPayloadType.DefaultResponse) SetPayloadType(ResultPayloadType.None);
                        break;

                    case ResponseCategory.Redirection:
                        this._resultCode = "300";
                        this._description = GetResponseDescription();
                        if (this._payloadType == ResultPayloadType.DefaultResponse) SetPayloadType(ResultPayloadType.None);
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
                        break;
                }
                DetermineStatus();
            }
        }

        /// <summary>
        /// Helper function that is used to assign a (new) value to the Result Payload Type field. Depending on the type,
        /// we might expect additional information (such as external classes) to be assigned as well before we can consider
        /// this to be a valid response code. Changing the payload type will reset ALL payload data collected so far and
        /// effectively resets the payload data to default conditions.
        /// Please be aware that template descriptors do NOT support the 'document' payload type since these are operation specific.
        /// Also, Informational, Success and Redirection categories do NOT support 'default' payload.
        /// </summary>
        /// <param name="newType">The new type to be assigned. If new type is equal to existing type, no operations are performed.</param>
        private void SetPayloadType(ResultPayloadType newType)
        {
            if (this._payloadType != newType)
            {
                if (this._isTemplate && newType == ResultPayloadType.Document)
                {
                    Logger.WriteError("Plugin.Application.Forms.RESTOperationResultDescriptor.SetPayloadType >> Template descriptor does not support 'Document' payload type!");
                    return;
                }
                else if (newType == ResultPayloadType.DefaultResponse &&
                         this._category != ResponseCategory.ClientError && this._category != ResponseCategory.ServerError &&
                         this._category != ResponseCategory.Default)
                {
                    Logger.WriteError("Plugin.Application.Forms.RESTOperationResultDescriptor.SetPayloadType >> Category '" + this._category +
                                      "' does not support 'Default Response' payload type!");
                    return;
                }

                this._payloadType = newType;
                this._externalReference = string.Empty;

                if (newType == ResultPayloadType.DefaultResponse) SetDocument();
                else if (newType == ResultPayloadType.Link || newType == ResultPayloadType.None)
                {
                    // New type will not have payload (anymore). Update payload status accordingly...
                    if (this._document != null || this._payloadClass != null) this._payloadChange = true;
                    this._document = null;
                    this._payloadClass = null;
                }
            }
        }
    }

    /// <summary>
    /// Helper class that facilitates translation between code, description and human friendly labels.
    /// </summary>
    internal sealed class CodeDescriptor
    {
        private string _code;           // Actual HTTP Code (or range or default code).
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
                if (code == RESTOperationResultDescriptor._DefaultCode) return RESTOperationResultDescriptor._DefaultCodeText;
                List<CodeDescriptor> codes = RESTOperationResultDescriptor.GetResponseCodes((RESTOperationResultDescriptor.ResponseCategory)int.Parse(code[0].ToString()));
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
