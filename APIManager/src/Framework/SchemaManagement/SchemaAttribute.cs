using System;
using System.Collections.Generic;
using Framework.Logging;
using Framework.Util;

namespace Framework.Util.SchemaManagement
{
    /// <summary>
    /// Abstract base class for both content- and supplementary attributes. Contains some generic properties and methods.
    /// </summary>
    internal abstract class SchemaAttribute
    {
        internal enum AttributeTypeCode { Supplementary, Content };

        private bool _isValid;
        private string _attName;                    // The name of the attribute, must NOT end with 'Type'.
        private string _attClassifier;              // The name of the classifier, MUST end with 'Type'.
        private string _attClassifierNs;            // The namespace in which the classifier is defined.
        private string _NSToken;                    // Namespace token, identifying the namespace in which the attribute is defined. 
        private AttributeTypeCode _attType;         // Indicates whether this is a content- or supplementary attribute.
        private bool _isOptional;                   // Indicates whether the attribute is mandatory or optional.
        private int _sequenceKey;                   // Sorting key.
        private Schema _schema;                     // The schema in which we're creating the attribute.
        private bool _listRequired;                 // Set to 'true' in case attribute cardinality suggests an array to be created.
        private List<MEDocumentation> _annotation;  // Attribute annotation.
        private string _defaultValue;               // Attribute default value, mutual exclusive with fixed value!
        private string _fixedValue;                 // Attribute fixed value, mutual exclusive with default value!
        private Cardinality _cardinality;           // Attribute cardinality. Upper limit = 0 --> Unbounded.

        /// <summary>
        /// Getters for properties of Attribute:
        /// IsValid = Provides read/write access to the 'IsValid' attribute.
        /// IsOptional = Provides read/write access to the 'IsOptional' attribute.
        /// IsMandatory = The inverse of IsOptional, sometimes checking this is more obvious then using '!IsOptional'.
        /// AttributeType = Returns the attribute type designator, which can be either Supplementary or Content.
        /// SequenceKey = Retrieve the sequence key for this attribute. Is used for sorting of elements within a constructed type (ABIE).
        /// Name = The attribute name.
        /// Classifier = The name of the attribute classifier.
        /// ClassifierNS = The namespace in which the classifier is defined.
        /// NSToken = Token identifying the namespace in which the attribute is defined (e.g. "ns0", "cmn", etc,).
        /// IsListRequired = True in case attribute cardinality suggests that we must create an array of attributes.
        /// DefaultValue = The default value of the attribute (if defined).
        /// FixedValue = The read-only, fixed, value of the attribute (if defined).
        /// Cardinality = Returns attribute cardinality as two integers (lower and upper boundary). Upper = 0 --> Unbounded.
        /// </summary>
        internal bool IsValid
        {
            get { return this._isValid; }
            set { this._isValid = value; }
        }
        internal bool IsOptional
        {
            get { return this._isOptional; }
            set { this._isOptional = value; }
        }
        internal bool IsMandatory
        {
            get { return !this._isOptional; }
            set { this._isOptional = !value; }
        }
        internal AttributeTypeCode AttributeType  { get { return _attType; } }
        internal int SequenceKey                  { get { return this._sequenceKey; } } 
        internal string Name                      { get { return this._attName; } }
        internal string Classifier                { get { return this._attClassifier; } }
        internal string ClassifierNS              { get { return this._attClassifierNs; } }
        internal string NSToken                   { get { return this._NSToken; } }
        internal bool IsListRequired              { get { return this._listRequired; } }
        internal string DefaultValue              { get { return this._defaultValue; } }
        internal string FixedValue                { get { return this._fixedValue; } }
        internal Cardinality Cardinality          { get { return this._cardinality; } }

        /// <summary>
        /// Inherited from base class, will return the name and classifier of the attribute in format 'name: classifier'
        /// </summary>
        /// <returns>Attribute name.</returns>
        public override string ToString()
        {
            return this._attName + ": " + this._attClassifier;
        }

        /// <summary>
        /// Default constructor, simply initialized local attributes.
        /// </summary>
        /// <param name="schema">The schema in which the attribute is created (which is identical to the schema in which the Classifier is created).</param>
        /// <param name="name">Name of the attribute as defined in the ABIE.</param>
        /// <param name="classifier">Classifier name as defined in the ABIE.</param>
        /// <param name="sequenceKey">When specified (not 0), this indicates the sorting order within a list of attributes and associations.</param>
        /// <param name="cardinality">Attribute cardinality. Upper boundary 0 is interpreted as 'unbounded'.</param>
        /// <param name="annotation">Optional comment for the content element. Empty list in case no comment is present.</param>
        /// <param name="defaultValue">Optional default value.</param>
        /// <param name="fixedValue">Optional fixed value.</param
        protected SchemaAttribute(Schema schema, string name, string classifier, int sequenceKey, Cardinality cardinality, List<MEDocumentation> annotation,
                                  string defaultValue, string fixedValue, AttributeTypeCode attType)
        {
            this._isOptional = cardinality.IsOptional;
            this._attClassifier = classifier;
            this._attClassifierNs = schema.SchemaNamespace;
            this._listRequired = cardinality.IsList;
            this._attName = name;
            this._attType = attType;
            this._sequenceKey = sequenceKey;
            this._NSToken = schema.NSToken;
            this._annotation = annotation;
            this._defaultValue = defaultValue;
            this._fixedValue = fixedValue;
            this._schema = schema;
            this._isValid = false;
            this._cardinality = cardinality;

            if (defaultValue != string.Empty)
            {
                Logger.WriteInfo("Framework.Util.SchemaManagement.SchemaAttribute >> Attribute has default value: " + defaultValue);
                this._defaultValue = defaultValue;
                this._fixedValue = string.Empty;
                if (fixedValue != string.Empty)
                {
                    Logger.WriteWarning("Attribute can not have both default- and fixed values, fixed value '" + fixedValue + "' has been ignored!");
                }
            }
            else if (fixedValue != string.Empty)
            {
                Logger.WriteInfo("Framework.Util.SchemaManagement.SchemaAttribute >> Attribute has fixed value: " + fixedValue);
                this._fixedValue = fixedValue;
                this._defaultValue = string.Empty;
            }
        }

        /// <summary>
        /// Since we don't want everybody to change the attribute name, we made a internal Getter and a protected Setter.
        /// </summary>
        /// <param name="newName">New attribute name.</param>
        protected void SetAttributeName (string newName) { this._attName = newName; }

        /// <summary>
        /// Since we don't want everybody to change the classifier namespace, we made a internal Getter and a protected Setter.
        /// </summary>
        /// <param name="newNS">New classifier namespace.</param>
        protected void SetClassifierNS (string newNS) { this._attClassifierNs = newNS; }
    }

    /// <summary>
    /// Specialization of 'Attribute' for Content-type attributes. These are translated to elements that are associated with the type-object of the specified 
    /// classifier. These elements in turn must be included as a Sequence within the associated ABIE.
    /// The constructor performs most of the work in creating the appropriate element(s). In case of a cardinality >1, a list-element is constructed to contain the actual content elements.
    /// The class contains an overloaded helper operation that is used to add the element to the appropriate container of the owning ABIE.
    /// </summary>
    internal abstract class ContentAttribute : SchemaAttribute
    {
        internal ChoiceGroup _choiceGroup;      // Identification of (optional) choice group that this attribute belongs to. 
        internal bool _nillable;                // Set to 'true' to indicate that the attribute supports a NULL value.

        /// <summary>
        /// Returns the ChoiceGroup object associated with this attribute (NULL if undefined).
        /// </summary>
        internal ChoiceGroup ChoiceGroup { get { return this._choiceGroup; } }

        /// <summary>
        /// Returns 'true' is this attribute is part of a Choice group.
        /// </summary>
        internal bool IsChoiceElement { get { return this._choiceGroup != null; } }

        /// <summary>
        /// Returns 'true' if the attribute supports a NULL value.
        /// </summary>
        internal bool IsNillable { get { return this._nillable; } }

        /// <summary>
        /// Construct a new content attribute. These attributes MUST be used in the context of the associated ABIE of which they are declared as attributes.
        /// Content attributes have a name (the attribute name as defined in the containing ABIE) and a classifier that must be a CDT (or derived BDT).
        /// The classifier is created in the specified schema and will use the namespace of that schema.
        /// Attributes have a cardinality specified by minOccurs and maxOccurs, both must be specified. If maxOccurs is '0', this is interpreted as 'unbounded'. 
        /// In case the cardinality of the attribute has an upper boundary > 1, a separate List element is created.
        /// If the cardinality lower boundary is 0, this list becomes an optional element. List contents always have a lower boundary of 1!
        /// The attribute can also have EITHER a default- or a fixed value, if both are specified, only the default value will be used.
        /// Note that the constructor creates a 'dangling' attribute, e.g. it is not assigned to an ABIE yet. It is the responsibility of the caller to pass this attribute to the associated
        /// ABIE element in due time.
        /// </summary>
        /// <param name="schema">The schema in which the attribute is created.</param>
        /// <param name="name">Name of the attribute as defined in the ABIE.</param>
        /// <param name="classifier">Classifier name as defined in the ABIE.</param>
        /// <param name="isComplex">Set to 'true' to indicate that classifier is a complex type rather then a simple type.</param>
        /// <param name="sequenceKey">When specified (not 0), this indicates the sorting order within a list of attributes and associations.</param>
        /// <param name="choiceGroup">Optional identifier for the choice group that this attribute should go to or NULL if not defined.</param>
        /// <param name="cardinality">Attribute cardinality. Upper boundary 0 is interpreted as 'unbounded'.</param>
        /// <param name="annotation">Optional comment for the content element. Empty list in case no comment is present.</param>
        /// <param name="defaultValue">Optional default value.</param>
        /// <param name="fixedValue">Optional fixed value.</param>
        /// <param name="isNillable">Set to 'true' to indicate that the attribute supports a NULL value.</param>
        internal ContentAttribute(Schema schema,
                                  string name,
                                  string classifier,
                                  int sequenceKey,
                                  ChoiceGroup choiceGroup,
                                  Cardinality cardinality,
                                  List<MEDocumentation> annotation,
                                  string defaultValue, string fixedValue,
                                  bool isNillable): 
            base(schema, name, classifier, sequenceKey, cardinality, annotation,defaultValue, fixedValue, AttributeTypeCode.Content)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.ContentAttribute >> Creating attribute '" + name + "' with classifier '" + classifier +
                             "' and cardinality '" + cardinality.ToString() + "'.");
            this._choiceGroup = choiceGroup;
            this._nillable = isNillable;
        }
    }

    /// <summary>
    /// Specialization of 'Attribute' for Supplementary-type attributes. These are considered to contain meta-data of the owning ABIE.
    /// Implementation depends on the exact schema type, hence implementation as an abstract class.
    /// </summary>
    internal abstract class SupplementaryAttribute : SchemaAttribute
    {
        /// <summary>
        /// Construct a new supplementary attribute. The attribute must have a name, a type and a usage flag (true in case of optional 
        /// attribute, false in case of mandatory attribute).
        /// The attribute can also have EITHER a default- or a fixed value, if both are specified, only the default value will be used.
        /// The attribute classifier must be CCTS PRIM datatype name. UNION is NOT supported for attributes and will lead to an invalid object.
        /// </summary>
        /// <param name="schema">The schema in which the attribute is defined.</param>
        /// <param name="name">Name of the attribute</param>
        /// <param name="classifier">Type of the attribute, MUST be a PRIM type, CAN be an enumeration!</param>
        /// <param name="isOptional">TRUE if this is an optional attribute, FALSE if mandatory</param>
        /// <param name="annotation">Optional comment for the attribute (empty list in case of no comment).</param>
        /// <param name="defaultValue">Default value of the attribute</param>
        /// <param name="fixedValue">Fixed value of the attribute</param>
        internal SupplementaryAttribute(Schema schema,
                                        string name,
                                        string classifier,
                                        bool isOptional,
                                        List<MEDocumentation> annotation,
                                        string defaultValue, string fixedValue): 
            base(schema, name, classifier, 0, new Cardinality(isOptional? 0: 1, 1), annotation, defaultValue, fixedValue, AttributeTypeCode.Supplementary)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.SupplementaryAttribute >> Creating attribute '" + name + "' of type '" + classifier + "'.");
        }
    }
}
