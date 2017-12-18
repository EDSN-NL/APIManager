using System;
using System.IO;
using System.Xml.Serialization;
using Framework.Model;
using Framework.Logging;
using Framework.Context;
using Framework.Util;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// Helper class for proper XML serialization of Parameter Declaration objects (for this to work, all data must be public)...
    /// </summary>
    public sealed class RESTParameterState
    {
        public string _name;                    // Property name.
        public string _classifierGUID;          // Global ID of the classifier type.
        public string _defaultValue;            // Default value (if applicable).
        public string _description;             // Property notes.
        public int[]  _cardinality;             // Property cardinality, item2 == 0 --> Undefined.
        public string _collectionFormat;        // In case of upper-limit cardinality >1: how do we separate values in the input?
        public string _scope;                   // Parameter scope.
        public string _status;                  // Status of this declaration record.
        public bool _allowEmptyValue;           // True when parameter can have 'name only'.    
    }

    /// <summary>
    /// A simple helper class that bundles the components that make up a REST parameter (attributes of a Path Expression or operation).
    /// </summary>
    internal sealed class RESTParameterDeclaration
    {
        // Configuration properties used by this module...
        private const string _RESTParameterStereotype       = "RESTParameterStereotype";
        private const string _ParameterScopeTag             = "ParameterScopeTag";
        private const string _CollectionFormatTag           = "CollectionFormatTag";
        private const string _AllowEmptyParameterValueTag   = "AllowEmptyParameterValueTag";

        // The status is used to track operations on the declaration.
        internal enum DeclarationStatus { Invalid, Stable, Created, Edited, Deleted }

        // Indicates Parameter scope...
        // Form = Parameter is passed as a form (in the body).
        // Header = Parameter is passed in the HTTP header.
        // Path = Parameter is passed as part of the URL.
        // Query = Parameter is passed as part of the query-section of the URL ("?.....").
        // RequestBody = Parameter is a body parameter in a request message.
        // ResponseBody = Parameter is a body parameter in a response message.
        internal enum ParameterScope { Form, Header, Path, Query, RequestBody, ResponseBody, Unknown }

        /// <summary>
        /// QueryCollectionFormat represents a REST OpenAPI collection format definition for query parameters:
        /// - CSV = Comma separated value list;
        /// - SSV = Space separated value list;
        /// - TSV = Tab (/t) separated value list;
        /// - Pipes = Pipe (|) separated value list;
        /// - Multi = The same query parameter can appear multiple times with different values.
        /// - NA = Not Applicable (not a collection).
        /// </summary>
        internal enum QueryCollectionFormat { CSV, SSV, TSV, PIPES, Multi, NA, Unknown }

        private string _name;                   // Property name.
        private MEClass _classifier;            // Type of the property.
        private string _defaultValue;           // Default value (if applicable).
        private string _description;            // Property notes.
        private Tuple<int, int> _cardinality;   // Property cardinality, item2 == 0 --> Undefined.
        private QueryCollectionFormat _collectionFormat; // In case of upper-limit cardinality >1: how do we separate values in the input?
        private ParameterScope _scope;          // Parameter scope.
        private DeclarationStatus _status;      // Status of this declaration record.
        private bool _allowEmptyValue;          // True when parameter can have 'name only'.

        /// <summary>
        /// Get or set the 'Allow Empty Value' parameter property.
        /// </summary>
        internal bool AllowEmptyValue
        {
            get { return this._allowEmptyValue; }
            set
            {
                if (this._allowEmptyValue != value)
                {
                    this._allowEmptyValue = value;
                    this._status = (this._status == DeclarationStatus.Invalid || 
                                    this._status == DeclarationStatus.Created) ? DeclarationStatus.Created : DeclarationStatus.Edited; 
                }
            }
        }

        /// <summary>
        /// Parameter lower- and higher cardinality boundaries. A high value of 0 is interpreted as 'infinite'.
        /// </summary>
        internal Tuple<int, int> Cardinality
        {
            get { return this._cardinality; }
            set
            {
                if (this._cardinality != value)
                {
                    this._cardinality = value;
                    this._status = (this._status == DeclarationStatus.Invalid ||
                                    this._status == DeclarationStatus.Created) ? DeclarationStatus.Created : DeclarationStatus.Edited;
                }
            }
        }

        /// <summary>
        ///  Get or set the parameter default value (empty string if no default is defined).
        /// </summary>
        internal string Default
        {
            get { return this._defaultValue; }
            set
            {
                if (this._defaultValue != value)
                {
                    this._defaultValue = value;
                    this._status = (this._status == DeclarationStatus.Invalid ||
                                    this._status == DeclarationStatus.Created) ? DeclarationStatus.Created : DeclarationStatus.Edited;
                }
            }
        }

        /// <summary>
        ///  Get or set the parameter annotation text.
        /// </summary>
        internal string Description
        {
            get { return this._description; }
            set
            {
                if (this._description != value)
                {
                    this._description = value;
                    this._status = (this._status == DeclarationStatus.Invalid ||
                                    this._status == DeclarationStatus.Created) ? DeclarationStatus.Created : DeclarationStatus.Edited;
                }
            }
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
                    this._status = (this._status == DeclarationStatus.Invalid ||
                                    this._status == DeclarationStatus.Created) ? DeclarationStatus.Created : DeclarationStatus.Edited;
                }
            }
        }

        /// <summary>
        /// Get or set the parameter scope.
        /// </summary>
        internal ParameterScope Scope
        {
            get { return this._scope; }
            set
            {
                if (this._scope != value)
                {
                    this._scope = value;
                    this._status = (this._status == DeclarationStatus.Invalid ||
                                    this._status == DeclarationStatus.Created) ? DeclarationStatus.Created : DeclarationStatus.Edited;
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
        /// The assigned data type for the parameter. Please note that, unlike most ECDM classifiers, REST parameters DO accept a 
        /// constructed type as classifier. This is required for body-type parameters where the parameter type represents a complete message instead
        /// of a primitive. For ALL other parameter types, the Cardinality must represent an MEDataType!
        /// </summary>
        internal MEClass Classifier
        {
            get { return this._classifier; }
            set
            {
                if (this._classifier != value)
                {
                    this._classifier = value;
                    this._status = (this._status == DeclarationStatus.Invalid ||
                                    this._status == DeclarationStatus.Created) ? DeclarationStatus.Created : DeclarationStatus.Edited;
                }
            }
        }

        /// <summary>
        /// In case of cardinality upper boundary >1, this represents the mechanism used to separate values in the input.
        /// </summary>
        internal QueryCollectionFormat CollectionFormat
        {
            get { return this._collectionFormat; }
            set
            {
                if (this._collectionFormat != value)
                {
                    this._collectionFormat = value;
                    this._status = (this._status == DeclarationStatus.Invalid ||
                                    this._status == DeclarationStatus.Created) ? DeclarationStatus.Created : DeclarationStatus.Edited;
                }
            }
        }

        /// <summary>
        /// Default constructor creates an empty, illegal, parameter declaration.
        /// </summary>
        internal RESTParameterDeclaration()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTParameterDeclaration >> Creating empty declaration.");
            this._name = string.Empty;
            this._defaultValue = string.Empty;
            this._cardinality = null;
            this._classifier = null;
            this._description = string.Empty;
            this._collectionFormat = QueryCollectionFormat.Unknown;
            this._scope = ParameterScope.Unknown;
            this._status = DeclarationStatus.Invalid;
            this._allowEmptyValue = false;
        }

        /// <summary>
        /// Default class constructor. Accepts all the components that make up a REST parameter. CollectionFmt is optional and must only be provided in
        /// case we have a cardinality of which the upper boundary is 0 (undefined) or >1.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <param name="classifier">Parameter type.</param>
        /// <param name="deflt">Optional default value, can be NULL or empty string if not defined.</param>
        /// <param name="description">Parameter description.</param>
        /// <param name="card">Cardinality of the parameter occurance.</param>
        /// <param name="allowEmptyValue">Set to true to allow the parameter to be specified by 'name only'.</param>
        /// <param name="scope">Scope of this parameter (path, body, query or header)</param>
        /// <param name="collectionFmt">Mechanism used to separate values in case cardinality upper boundary >1.</param>
        internal RESTParameterDeclaration(string name, MEClass classifier, string deflt, string description, Tuple<int,int> card, bool allowEmptyValue, ParameterScope scope, QueryCollectionFormat collectionFmt = QueryCollectionFormat.Unknown)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTParameterDeclaration >> Creating new declaration with name '" + name + "' and classifier '" + classifier.Name + "'...");
            this._name = name;
            this._classifier = classifier;
            this._description = description;
            this._defaultValue = string.IsNullOrEmpty(deflt)? string.Empty: deflt;
            this._cardinality = card;
            this._collectionFormat = collectionFmt;
            this._scope = scope;
            this._allowEmptyValue = allowEmptyValue;
            this._status = (name != string.Empty && classifier != null && scope != ParameterScope.Unknown)? DeclarationStatus.Stable: DeclarationStatus.Invalid;
        }

        /// <summary>
        /// Class constructor that creates a parameter declaration from an existing Attribute object. It retrieves all necessary information from that attribute.
        /// </summary>
        /// <param name="attrib">Attribute to be parsed.</param>
        internal RESTParameterDeclaration(MEAttribute attrib)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTParameterDeclaration >> Creating parameter declaration from attribute '" + attrib + "'...");
            ModelSlt model = ModelSlt.GetModelSlt();
            ContextSlt context = ContextSlt.GetContextSlt();
            this._name = attrib.Name;
            this._classifier = attrib.Classifier;
            this._description = attrib.Annotation;
            this._defaultValue = !string.IsNullOrEmpty(attrib.DefaultValue) ? attrib.DefaultValue : attrib.FixedValue;
            this._cardinality = attrib.Cardinality;
            this._status = DeclarationStatus.Stable;

            string collectionFmt = attrib.GetTag(context.GetConfigProperty(_CollectionFormatTag));
            string scope = attrib.GetTag(context.GetConfigProperty(_ParameterScopeTag));
            string emptyParam = attrib.GetTag(context.GetConfigProperty(_AllowEmptyParameterValueTag));
            this._scope = !string.IsNullOrEmpty(scope) ? EnumConversions<ParameterScope>.StringToEnum(scope) : ParameterScope.Unknown;
            this._collectionFormat = !string.IsNullOrEmpty(collectionFmt) ? EnumConversions<QueryCollectionFormat>.StringToEnum(collectionFmt) : QueryCollectionFormat.Unknown;
            this._allowEmptyValue = !string.IsNullOrEmpty(emptyParam) ? string.Compare(emptyParam, "true", true) == 0 : false;
        }

        /// <summary>
        /// A static helper function that transforms the Parameter declaration to an attribute of the specified class. Depending on the status
        /// of the parameter, an attribute is created, removed or replaced in the parent class.
        /// </summary>
        /// <param name="parent">Class in which we're going to create the attribute.</param>
        /// <param name="param">The parameter declaration to transform.</param>
        /// <returns>Created attribute or NULL in case of errors.</returns>
        internal static MEAttribute ConvertToAttribute(MEClass parent, RESTParameterDeclaration param)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            MEAttribute newAttrib = null;
            // Note that an Enumeration is also a MEDataType, so this would cover both flavors...
            if (param.Classifier is MEDataType && param.Status == DeclarationStatus.Created)
            {
                newAttrib = parent.CreateAttribute(param.Name, (MEDataType)param.Classifier, AttributeType.Attribute, param.Default, param.Cardinality, false);
                newAttrib.AddStereotype(context.GetConfigProperty(_RESTParameterStereotype));
                newAttrib.SetTag(context.GetConfigProperty(_ParameterScopeTag), param._scope.ToString(), true);
                newAttrib.SetTag(context.GetConfigProperty(_CollectionFormatTag), param._collectionFormat.ToString(), true);
                newAttrib.SetTag(context.GetConfigProperty(_AllowEmptyParameterValueTag), param._allowEmptyValue.ToString(), true);
                newAttrib.Annotation = param.Description;
            }
            else if (param.Status == DeclarationStatus.Deleted)
            {
                foreach (MEAttribute attrib in parent.Attributes)
                {
                    if (attrib.Name == param.Name)
                    {
                        parent.DeleteAttribute(attrib);
                        break;
                    }
                }
            }
            else if (param.Status == DeclarationStatus.Edited)
            {
                bool foundIt = false;
                foreach (MEAttribute attrib in parent.Attributes)
                {
                    if (attrib.Name == param.Name)
                    {
                        parent.DeleteAttribute(attrib);
                        newAttrib = parent.CreateAttribute(param.Name, (MEDataType)param.Classifier, AttributeType.Attribute, param.Default, param.Cardinality, false);
                        newAttrib.AddStereotype(context.GetConfigProperty(_RESTParameterStereotype));
                        newAttrib.SetTag(context.GetConfigProperty(_ParameterScopeTag), param._scope.ToString(), true);
                        newAttrib.SetTag(context.GetConfigProperty(_CollectionFormatTag), param._collectionFormat.ToString(), true);
                        newAttrib.SetTag(context.GetConfigProperty(_AllowEmptyParameterValueTag), param._allowEmptyValue.ToString(), true);
                        newAttrib.Annotation = param.Description;
                        foundIt = true;
                        break;
                    }
                }
                if (!foundIt)
                {
                    // Since it is difficult to get the status correct between Created and Edited, we assume that an 'Edited' status on a
                    // non-existing attribute actually means that we want to create it. Anyway, it has a valid status and is not present in the
                    // class, so change to 'Created' and re-invoke the method...
                    param.Status = DeclarationStatus.Created;
                    return ConvertToAttribute(parent, param);
                }
            }
            
            // Parameters of type Class must be treated differently since an attribute can only be a simple type!
            if (!(param.Classifier is MEDataType) && !(param.Classifier is MEEnumeratedType))
            {
                Logger.WriteWarning("Plugin.Application.CapabilityModel.API.RestParameterDeclaration.ConvertToAttribute >> Attempt to convert class-based attribute '" + 
                                    param.Name + "' to an attribute of class '" + parent.Name + "' is not supported!");
            }
            return newAttrib;
        }

        /// <summary>
        /// Serialize the specified parameter declaration object to an XML string that can be stored and later used to 
        /// reconstruct the instance.
        /// </summary>
        /// <returns>Object state as an XML string or empty string on errors.</returns>
        internal static string Serialize(RESTParameterDeclaration param)
        {
            try
            {
                var stateObject = new RESTParameterState
                {
                    _name = param._name,
                    _classifierGUID = param._classifier != null ? param._classifier.GlobalID : string.Empty,
                    _defaultValue = param._defaultValue,
                    _description = param._description,
                    _cardinality = new int[] { param._cardinality.Item1, param._cardinality.Item2 },
                    _collectionFormat = param._collectionFormat.ToString(),
                    _scope = param._scope.ToString(),
                    _status = param._status.ToString(),
                    _allowEmptyValue = param._allowEmptyValue
                };

                XmlSerializer serializer = new XmlSerializer(typeof(RESTParameterState));
                using (StringWriter textWriter = new StringWriter())
                {
                    serializer.Serialize(textWriter, stateObject);
                    return Compression.Zip(textWriter.ToString());
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.RestParameterDeclaration.Serialize >> Serialization failed because: " + exc.Message);
            }
            return string.Empty;
        }

        /// <summary>
        /// Reconstruct (deserialize) a RESTParameterDeclaration object from specified serialized string.
        /// </summary>
        /// <param name="stateString">Must be obtained from earlier call to Serialize.</param>
        /// <returns>Associated parameter declaration object or null on errors.</returns>
        internal static RESTParameterDeclaration Deserialize(string stateString)
        {
            try
            {
                RESTParameterState stateObject;
                XmlSerializer serializer = new XmlSerializer(typeof(RESTParameterState));
                using (StringReader textReader = new StringReader(Compression.Unzip(stateString)))
                {
                    stateObject = (RESTParameterState)serializer.Deserialize(textReader);
                }

                RESTParameterDeclaration param = new RESTParameterDeclaration
                {
                    _name = stateObject._name,
                    _classifier = stateObject._classifierGUID != string.Empty ? ModelSlt.GetModelSlt().GetDataType(stateObject._classifierGUID) : null,
                    _defaultValue = stateObject._defaultValue,
                    _description = stateObject._description,
                    _cardinality = new Tuple<int, int>(stateObject._cardinality[0], stateObject._cardinality[1]),
                    _collectionFormat = EnumConversions<QueryCollectionFormat>.StringToEnum(stateObject._collectionFormat),
                    _scope = EnumConversions<ParameterScope>.StringToEnum(stateObject._scope),
                    _status = EnumConversions<DeclarationStatus>.StringToEnum(stateObject._status),
                    _allowEmptyValue = stateObject._allowEmptyValue
                };
                return param;
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.API.RestParameterDeclaration.Deserialize >> Deserialization failed because: " + exc.Message);
            }
            return null;
        }
    }
}
