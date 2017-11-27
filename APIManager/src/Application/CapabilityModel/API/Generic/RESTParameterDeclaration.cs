using System;
using System.Collections.Generic;
using Framework.Model;
using Framework.Logging;
using Framework.Context;
using Framework.Util;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// A simple helper class that bundles the components that make up a REST parameter (attributes of a Path Expression or operation).
    /// </summary>
    internal sealed class RESTParameterDeclaration
    {
        // Configuration properties used by this module...
        private const string _RESTParameterStereotype   = "RESTParameterStereotype";
        private const string _ParameterScopeTag         = "ParameterScopeTag";
        private const string _CollectionFormatTag       = "CollectionFormatTag";

        // The status is used to track operations on the declaration.
        internal enum DeclarationStatus { Invalid, Created, Edited, Deleted }

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

        /// <summary>
        /// Parameter lower- and higher cardinality boundaries. A high value of 0 is interpreted as 'infinite'.
        /// </summary>
        internal Tuple<int, int> Cardinality
        {
            get { return this._cardinality; }
            set { this._cardinality = value; }
        }

        /// <summary>
        ///  Get or set the parameter default value (empty string if no default is defined).
        /// </summary>
        internal string Default
        {
            get { return this._defaultValue; }
            set { this._defaultValue = value; }
        }

        /// <summary>
        ///  Get or set the parameter annotation text.
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
            set { this._name = value; }
        }

        /// <summary>
        /// Get or set the parameter scope.
        /// </summary>
        internal ParameterScope Scope
        {
            get { return this._scope; }
            set { this._scope = value; }
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
            set { this._classifier = value; }
        }

        /// <summary>
        /// In case of cardinality upper boundary >1, this represents the mechanism used to separate values in the input.
        /// </summary>
        internal QueryCollectionFormat CollectionFormat
        {
            get { return this._collectionFormat; }
            set { this._collectionFormat = value; }
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
        /// <param name="collectionFmt">Mechanism used to separate values in case cardinality upper boundary >1.</param>
        internal RESTParameterDeclaration(string name, MEClass classifier, string deflt, string description, Tuple<int,int> card, ParameterScope scope, QueryCollectionFormat collectionFmt = QueryCollectionFormat.Unknown)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.API.RESTParameterDeclaration >> Creating new declaration with name '" + name + "' and classifier '" + classifier.Name + "'...");
            this._name = name;
            this._classifier = classifier;
            this._description = description;
            this._defaultValue = string.IsNullOrEmpty(deflt)? string.Empty: deflt;
            this._cardinality = card;
            this._collectionFormat = collectionFmt;
            this._scope = scope;
            this._status = (name != string.Empty && classifier != null && scope != ParameterScope.Unknown)? DeclarationStatus.Created: DeclarationStatus.Invalid;
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
            this._status = DeclarationStatus.Created;

            string collectionFmt = attrib.GetTag(context.GetConfigProperty(_CollectionFormatTag));
            string scope = attrib.GetTag(context.GetConfigProperty(_ParameterScopeTag));
            this._scope = !string.IsNullOrEmpty(scope) ? EnumConversions<ParameterScope>.StringToEnum(scope) : ParameterScope.Unknown;
            this._collectionFormat = !string.IsNullOrEmpty(collectionFmt) ? EnumConversions<QueryCollectionFormat>.StringToEnum(collectionFmt) : QueryCollectionFormat.Unknown;
        }

        /// <summary>
        /// A static helper function that transforms the Parameter declaration to an attribute of the specified class.
        /// </summary>
        /// <param name="parent">Class in which we're going to create the attribute.</param>
        /// <param name="param">The parameter declaration to transform.</param>
        /// <returns>Created attribute or NULL in case of errors.</returns>
        internal static MEAttribute ConvertToAttribute(MEClass parent, RESTParameterDeclaration param)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            MEAttribute newAttrib = null;
            if (param.Classifier is MEDataType && param.Status != RESTParameterDeclaration.DeclarationStatus.Deleted)
            {
                newAttrib = parent.CreateAttribute(param.Name, (MEDataType)param.Classifier, AttributeType.Attribute, param.Default, param.Cardinality, false);
                newAttrib.AddStereotype(context.GetConfigProperty(_RESTParameterStereotype));
                newAttrib.SetTag(context.GetConfigProperty(_ParameterScopeTag), param._scope.ToString(), true);
                newAttrib.SetTag(context.GetConfigProperty(_CollectionFormatTag), param._collectionFormat.ToString(), true);
                newAttrib.Annotation = param.Description;
            }
            
            // Parameters of type Class must be treated differently since an attribute can only be a simple type!
            if (!(param.Classifier is MEDataType) && !(param.Classifier is MEEnumeratedType))
            {
                Logger.WriteWarning("Plugin.Application.CapabilityModel.API.RestParameterDeclaration.ConvertToAttribute >> Attempt to convert class-based attribute '" + 
                                    param.Name + "' to an attribute of class '" + parent.Name + "' is not supported!");
            }
            return newAttrib;
        }
    }
}
