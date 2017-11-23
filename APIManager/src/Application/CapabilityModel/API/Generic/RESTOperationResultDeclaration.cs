using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework.Model;
using Framework.Context;
using Framework.Logging;
using Framework.Util;

namespace Plugin.Application.CapabilityModel.API
{
    internal sealed class RESTOperationResultDeclaration
    {
        /// <summary>
        /// Helper class that facilitates translation between code, description and human friendly labels.
        /// </summary>
        internal sealed class CodeDescriptor
        {
            private string _code;           // Actual HTTP Code.
            private string _description;    // Descriptive text for the code.

            internal string Code        { get { return this._code; } }
            internal string Description { get { return this._description; } }
            internal string Label       { get { return this._code + " - " + this._description; } }

            /// <summary>
            /// Create a new CodeDescriptor object based on Code and Description.
            /// </summary>
            /// <param name="code">HTTP Response Code.</param>
            /// <param name="description">Associated description.</param>
            internal CodeDescriptor (string code, string description)
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
                return label.Substring(0, label.IndexOf(" - "));
            }

            /// <summary>
            /// Helper function that takes a code and translates this into a human-friendly label.
            /// </summary>
            /// <param name="code">Code to translate.</param>
            /// <returns>Associated label or empty string in case of illegal codes.</returns>
            static internal string CodeToLabel(string code)
            {
                var category = (RESTOperationResultCapability.ResponseCategory)(int.Parse(code[0].ToString()));
                var result = new RESTOperationResultDeclaration(category);
                List<CodeDescriptor> codes = result.GetResponseCodes();
                foreach (CodeDescriptor dsc in codes)
                {
                    if (dsc.Code == code) return code + " - " + dsc.Description;
                }
                return string.Empty;
            }
        }

        // The status is used to track operations on the declaration.
        internal enum DeclarationStatus { Invalid, Created, Edited, Deleted }

        // Conversion tables for each HTTP Response category..
        private static readonly string[,] _InformationResponses =
            { {"100", "Continue"},
              {"101", "Switching Protocols"},
              {"102", "Processing"},
              {"103", "Early Hints"} };

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
              {"226", "IM Used"} };

        private static readonly string[,] _RedirectionResponses =
            { {"300", "Multiple Choices"},
              {"301", "Moved Permanently"},
              {"302", "Found"},
              {"303", "See Other"},
              {"304", "Not Modified"},
              {"305", "Use Proxy"},
              {"307", "Temporary Redirect"},
              {"308", "Permanent Redirect"} };

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
              {"451", "Unavailable For Legal Reasons"} };

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
              {"511", "Network Authentication Required"} };

        // Configuration properties used by this module...
        private const string _OperationResultPrefix             = "OperationResultPrefix";
        private const string _DefaultResponseCode               = "DefaultResponseCode";
        private const string _DefaultResponseDescription        = "DefaultResponseDescription";
        private const string _DefaultSuccessCode                = "DefaultSuccessCode";
        private const string _DefaultClientErrorCode            = "DefaultClientErrorCode";
        private const string _DefaultServerErrorCode            = "DefaultServerErrorCode";
        private const string _APISupportModelPathName           = "APISupportModelPathName";
        private const string _CoreDataTypesPathName             = "CoreDataTypesPathName";
        private const string _RESTOperationResultStereotype     = "RESTOperationResultStereotype";
        private const string _ResultCodeAttributeName           = "ResultCodeAttributeName";
        private const string _ResultCodeAttributeClassifier     = "ResultCodeAttributeClassifier";

        private string _defaultResponseCode;        // Contains the OpenAPI default response code (typically, this is 'default').
        private RESTOperationResultCapability.ResponseCategory _category;   // Operation result category code.
        private string _resultCode;                 // Either an HTTP result code or default OpenAPI response code.
        private string _description;                // Descriptive text to go with the response.
        private MEClass _responseType;              // In case of response schemas, this class represents the root of that schema.
        private DeclarationStatus _status;          // Status of this declaration record.
        private DeclarationStatus _initialStatus;   // Original status of this declaration record.

        /// <summary>
        /// Returns the operation result category code.
        /// </summary>
        internal RESTOperationResultCapability.ResponseCategory Category { get { return this._category; } }

        /// <summary>
        /// Returns or loads the associated description text.
        /// </summary>
        internal string Description
        {
            get { return this._description; }
            set { this._description = value; }
        }

        /// <summary>
        /// Returns or loads the optional result parameters. Returns NULL if no parameters are defined.
        /// </summary>
        internal MEClass Parameters
        {
            get { return this._responseType; }
            set
            {
                this._responseType = value;
                if (this._initialStatus == DeclarationStatus.Invalid && this._resultCode != string.Empty) this._status = DeclarationStatus.Created;
                else if (this._initialStatus != DeclarationStatus.Invalid) this._status = DeclarationStatus.Edited;
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
        /// Default constructor, which accepts only a response category code and no further information. This constructs a descriptor of which
        /// the values depend on the category that has been passed:
        /// Success - Default response descriptor, no body, only a result code and default OK description.
        /// ClientError - Default client error code, standard Error body and default Client Error description.
        /// ServerError - Default server error code, standard Error body and default Server Error description.
        /// All others - descriptor containing 'default' code and default category-dependent description.
        /// </summary>
        /// <param name="category">The HTTP Result Code category (first digit in response code).</param>
        internal RESTOperationResultDeclaration(RESTOperationResultCapability.ResponseCategory category)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationResultDeclaration >> Default constructor using category '" + category.ToString() + "'...");

            ContextSlt context = ContextSlt.GetContextSlt();
            this._defaultResponseCode = context.GetConfigProperty(_DefaultResponseCode);
            this._responseType = null;
            this._category = category;

            switch (category)
            {
                case RESTOperationResultCapability.ResponseCategory.Informational:
                    this._resultCode = "100";
                    this._description = GetResponseDescription();
                    this._status = this._initialStatus = DeclarationStatus.Created;
                    break;

                case RESTOperationResultCapability.ResponseCategory.Success:
                    this._resultCode = context.GetConfigProperty(_DefaultSuccessCode);
                    this._description = GetResponseDescription();
                    this._status = this._initialStatus = DeclarationStatus.Created;
                    break;

                case RESTOperationResultCapability.ResponseCategory.Redirection:
                    this._resultCode = "300";
                    this._description = GetResponseDescription();
                    this._status = this._initialStatus = DeclarationStatus.Created;
                    break;

                case RESTOperationResultCapability.ResponseCategory.ClientError:
                    this._resultCode = context.GetConfigProperty(_DefaultClientErrorCode);
                    this._description = GetResponseDescription();
                    this._status = this._initialStatus = DeclarationStatus.Created;
                    break;

                case RESTOperationResultCapability.ResponseCategory.ServerError:
                    this._resultCode = context.GetConfigProperty(_DefaultServerErrorCode);
                    this._description = GetResponseDescription();
                    this._status = this._initialStatus = DeclarationStatus.Created;
                    break;

                case RESTOperationResultCapability.ResponseCategory.Default:
                    this._resultCode = this._defaultResponseCode;
                    this._description = context.GetConfigProperty(_DefaultResponseDescription);
                    this._status = this._initialStatus = DeclarationStatus.Created;
                    break;

                default:
                    this._resultCode = this._defaultResponseCode;
                    this._description = string.Empty;
                    this._status = this._initialStatus = DeclarationStatus.Invalid;
                    break;
            }
        }

        /// <summary>
        /// Constructor that accepts code, description and an optional data-type response. In case of normal successfull completion of a request that
        /// accepts response parameters, an OK Result Declaration must be created that contains a class that specifies these parameters.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="description"></param>
        /// <param name="type"></param>
        internal RESTOperationResultDeclaration(string code, string description, MEClass type = null)
        {
            this._resultCode = code;
            this._description = description;
            this._responseType = type;
            this._category = (RESTOperationResultCapability.ResponseCategory)(int.Parse(code[0].ToString()));
            this._defaultResponseCode = ContextSlt.GetContextSlt().GetConfigProperty(_DefaultResponseCode);
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
            if (this._status != RESTOperationResultDeclaration.DeclarationStatus.Deleted)
            {
                string name = context.GetConfigProperty(_OperationResultPrefix) + this._resultCode;
                Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationResultDeclaration.AssignResultClass >> Creating class with name '" + name + "'...");
                resultClass = owningPackage.CreateClass(name, context.GetConfigProperty(_RESTOperationResultStereotype));
                MEDataType classifier = model.FindDataType(context.GetConfigProperty(_CoreDataTypesPathName), context.GetConfigProperty(_ResultCodeAttributeClassifier));
                if (classifier != null)
                {
                    MEAttribute newAttrib = resultClass.CreateAttribute(context.GetConfigProperty(_ResultCodeAttributeName), classifier,
                                                                        AttributeType.Attribute, this._resultCode, new Tuple<int, int>(1, 1), true);
                    resultClass.Annotation = this._description;
                    if (this._responseType != null)
                    {
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTOperationResultDeclaration.AssignResultClass >> Associating with response type '" + this._responseType.Name + "'...");
                        string roleName = Conversions.ToPascalCase(this._responseType.Name);
                        if (roleName.EndsWith("Type")) roleName = roleName.Substring(0, roleName.IndexOf("Type"));
                        var parentEndpoint = new EndpointDescriptor(resultClass, "1", resultClass.Name, null, false);
                        var typeEndpoint = new EndpointDescriptor(this._responseType, "1", roleName, null, true);
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
                case RESTOperationResultCapability.ResponseCategory.Informational:
                    for (int i = 0; i < _InformationResponses.GetLength(0); i++)
                        resultList.Add(new CodeDescriptor(_InformationResponses[i, 0], _InformationResponses[i, 1]));
                    break;

                case RESTOperationResultCapability.ResponseCategory.Success:
                    for (int i = 0; i < _SuccessResponses.GetLength(0); i++)
                        resultList.Add(new CodeDescriptor(_SuccessResponses[i, 0], _SuccessResponses[i, 1]));
                    break;

                case RESTOperationResultCapability.ResponseCategory.Redirection:
                    for (int i = 0; i < _RedirectionResponses.GetLength(0); i++)
                        resultList.Add(new CodeDescriptor(_RedirectionResponses[i, 0], _RedirectionResponses[i, 1]));
                    break;

                case RESTOperationResultCapability.ResponseCategory.ClientError:
                    for (int i = 0; i < _ClientErrorResponses.GetLength(0); i++)
                        resultList.Add(new CodeDescriptor(_ClientErrorResponses[i, 0], _ClientErrorResponses[i, 1]));
                    break;

                case RESTOperationResultCapability.ResponseCategory.ServerError:
                    for (int i = 0; i < _ServerErrorResponses.GetLength(0); i++)
                        resultList.Add(new CodeDescriptor(_ServerErrorResponses[i, 0], _ServerErrorResponses[i, 1]));
                    break;

                default:
                    ContextSlt context = ContextSlt.GetContextSlt();
                    resultList.Add(new CodeDescriptor(context.GetConfigProperty(_DefaultResponseCode), 
                                                      context.GetConfigProperty(_DefaultResponseDescription)));
                    break;
            }
            return resultList;
        }

        /// <summary>
        /// Returns the description that belongs the current category and code. In case of 'Default' category,
        /// the function returns the 'Default Response Description'.
        /// </summary>
        /// <returns>Description for current category + code.</returns>
        private string GetResponseDescription()
        {
            switch (this._category)
            {
                case RESTOperationResultCapability.ResponseCategory.Informational:
                    for (int i = 0; i < _InformationResponses.GetLength(0); i++)
                        if (string.Compare(_InformationResponses[i, 0], this._resultCode, true) == 0) return _InformationResponses[i, 1];
                    break;

                case RESTOperationResultCapability.ResponseCategory.Success:
                    for (int i = 0; i < _SuccessResponses.GetLength(0); i++)
                        if (string.Compare(_SuccessResponses[i, 0], this._resultCode, true) == 0) return _SuccessResponses[i, 1];
                    break;

                case RESTOperationResultCapability.ResponseCategory.Redirection:
                    for (int i = 0; i < _RedirectionResponses.GetLength(0); i++)
                        if (string.Compare(_RedirectionResponses[i, 0], this._resultCode, true) == 0) return _RedirectionResponses[i, 1];
                    break;

                case RESTOperationResultCapability.ResponseCategory.ClientError:
                    for (int i = 0; i < _ClientErrorResponses.GetLength(0); i++)
                        if (string.Compare(_ClientErrorResponses[i, 0], this._resultCode, true) == 0) return _ClientErrorResponses[i, 1];
                    break;

                case RESTOperationResultCapability.ResponseCategory.ServerError:
                    for (int i = 0; i < _ServerErrorResponses.GetLength(0); i++)
                        if (string.Compare(_ServerErrorResponses[i, 0], this._resultCode, true) == 0) return _ServerErrorResponses[i, 1];
                    break;

                default:
                    break;
            }
            return ContextSlt.GetContextSlt().GetConfigProperty(_DefaultResponseDescription);
        }
    }
}
