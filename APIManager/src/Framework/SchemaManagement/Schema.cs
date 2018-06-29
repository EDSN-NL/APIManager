using System;
using System.IO;
using System.Collections.Generic;
using Framework.Logging;

namespace Framework.Util.SchemaManagement
{
    /// <summary>
    /// Schema is the abstract root class for various specialized schema implementations. These implementations must at least support the
    /// abstract functions defined by the implementation base class.
    /// </summary>
    internal abstract class Schema
    {
        // We support three different types of Schemas:
        // 1) Common = schema contains simply a list of type definitions, to be used for reference by other schemas (used primarily by CommonSchema).
        // 2) Operation = defines an 'operation' object, consisting of an optional request and optional response (plus optionally a collection of shared objects).
        // 3) Message = defines a single 'message' object (plus optionaly a collection of shared objects).
        // 4) Definitions = similar to 'Common', but used only as a collection of type-defs, NEVER used as/with common schema.
        // 4) Unknown = schema type unknown (not initialized).
        internal enum SchemaType { Unknown, Common, Operation, Message, Definitions }

        private string _nsToken;        // Short, mnemonic name that is used to make members of the schema unique outside the namespace.
        private string _namespace;      // Full namespace identifier.
        private string _lastError;      // Stores last error message in case of exceptions.
        private string _version;        // Schema major version, minor version and build number, separated by "." (e.g. "1.2.6").
        private SchemaType _type;       // Identifies the type of schema.
        private string _name;           // Identifying name assigned to the schema.
        private bool _isInitialized;    // Set to TRUE after initialization.

        /// <summary>
        /// Getters for properties of Schema:
        /// NSToken = Returns the Schema Namespace token.
        /// SchemaNamespace = Returns the full Schema Namespace URI.
        /// SchemaVersion = Returns the version string of this Schema.
        /// LastError = In case of building errors, this property contains the most recent error message (if any).
        /// Name = returns the schema name.
        /// Type = returns the schema type (Collection, Operation or Message).
        /// IsInitialized = protected check on proper initialization.
        /// </summary>
        internal string NSToken         { get { return this._nsToken; } }
        internal string SchemaNamespace { get { return this._namespace; } }
        internal string SchemaVersion   { get { return this._version; } }
        internal string LastError       { get { return this._lastError; } }
        internal string Name            { get { return this._name; } }
        internal SchemaType Type
        {
            get { return this._type; }
            set { this._type = value; }
        }
        protected bool IsInitialized    { get { return this._isInitialized; } }

        /// <summary>
        /// Creates a new ABIE type and adds the list of content- and supplementary attributes as well as a list of associated classes to this type. 
        /// An association (ASBIE) is constructed as a sub-element of the type of the associated class (e.g. the ABIE is considered the 'source' of the association).
        /// An ABIE thus consists of a complex type, containing a sequence of content attribute elements as well as a sequence of associated classes (ASBIE's).
        /// The ABIE supports three sets of attributes:
        /// - Content Attributes - will be translated to a sequence of elements.
        ///                        Each element must be of a CDT (or BDT) type that must have been created earlier using one of the 'addxxxBBIEType' operations.
        /// - Supplementary Attributes - will be translated to attributes, the mechanism of which is schema dependent.
        /// Both sets of attributes are passed as a single list and correct processing is managed by the Schema implementation.
        /// If an ABIE with the specified name already exists, no processing takes place and 'true' is returned.
        /// </summary>
        /// <param name="className">The name of the business component (ABIE name).</param>
        /// <param name="annotation">Optional comment for this element (empty list in case of no comment).</param>
        /// <param name="attributeList">List of content- and supplementary attributes.</param>
        /// <param name="associationList">List of associated classes (ASBIE).</param>
        /// <returns>TRUE in case of success, FALSE on errors. If the type already exists, no action is performed and the operation returns TRUE.</returns>
        internal abstract bool AddABIEType(string className, List<MEDocumentation> annotation, List<SchemaAttribute> attributeList, List<SchemaAssociation> associationList);

        /// <summary>
        /// This operation is used to create a type definition for a complex-type Basic Business Information Entity (BBIE). These are always of type CDT (or BDT derived from CDT).
        /// The classifierName MUST be a PRIM type, so the caller must have parsed the entire class hierarchy until arrival at PRIM level.
        /// Note that we're in fact creating classifiers here (these are definitions used as classifiers in attributes of other elements). 
        /// To differentiate between the 'regular' primitive types and facetted types, we add the classifier name as a prefix to the primitive (since different classifiers
        /// might use different facets for the same primitive).
        /// </summary>
        /// <param name="classifierName">The name of the classifier to be constructed. The name must be unique within the current namespace. If not, it will be skipped!</param>
        /// <param name="primType">Associated PRIM type</param>
        /// <param name="annotation">Optional comment for the classifier (empty list in case of no comment).</param>
        /// <param name="attribList">Optional list of attributes to be assigned to this type</param>
        /// <param name="facetList">Optional list of facets that must be applied to the primitive type of the classifier.</param>
        /// <returns>TRUE in case of success, FALSE on errors. If the type already exists, no action is performed and the operation returns TRUE.</returns>
        internal abstract bool AddComplexClassifier(string classifierName, string primType, List<MEDocumentation> annotation, List<SupplementaryAttribute> attribList, List<Facet> facetList);

        /// <summary>
        /// This operation is used to create a type definition for an enumerated-type Basic Business Information Entity (BBIE). 
        /// Enumerations might have attributes (which is schema implementation dependent) but no facets (other then the enumeration values).
        /// </summary>
        /// <param name="classifierName">BBIE classifier name. The name must be unique within the current namespace. If not, it will be skipped!</param>
        /// <param name="annotation">Optional classifier documentation, empty list in case of no comments.</param>
        /// <param name="attribList">Optional list of attributes to be assigned to this type</param>
        /// <param name="enumList">The list of enumeration values.</param>
        /// <returns>TRUE in case of success, FALSE on errors. If the type already exists, no action is performed and the operation returns TRUE.</returns>
        internal abstract bool AddEnumClassifier(string classifierName, List<MEDocumentation> annotation, List<SupplementaryAttribute> attribList, List<EnumerationItem> enumList);

        /// <summary>
        /// Add a typedef that contains elements from an external schema. The schema details are passed as arguments to the call. The type might either contain
        /// elements of a specified base-type, or, if the name of the base-type is "Any", the type can contain any elements defined by the external schema.
        /// </summary>
        /// <param name="classifierName">Name of the classifier.</param>
        /// <param name="annotation">Any comment from the UML model or empty list in case of no comments.</param>
        /// <param name="ns">Namespace string.</param>
        /// <param name="nsToken">Namespace token to be used when referring to 'ns'.</param>
        /// <param name="schema">Fully qualified schema filename.</param>
        /// <param name="baseType">Name of base type, or 'Any' in case of any type.</param>
        /// <param name="cardinality">Classifier cardinality, an upper boundary of '0' is interpreted as 'unbounded'.</param>
        /// <returns>True when defined Ok, false on errors.</returns>
        internal abstract bool AddExternalClassifier(string classifierName, List<MEDocumentation> annotation,
                                                   string ns, string nsToken,
                                                   string schema, string baseType,
                                                   Tuple<int, int> cardinality);

        /// <summary>
        /// Adds an element to the schema. The element name must be unique within the schema and the classifier must have been defined earlier, either in the
        /// current schema or in the schema identified by the specified namespace.
        /// An element is defined as a structured property of a given type (classifier). The implementation is schema dependent.
        /// </summary>
        /// <param name="elementName">Name of the element.</param>
        /// <param name="classifierNamespace">Namespace of the element classifier.</param>
        /// <param name="classifierName">Name of the element classifier (type of the element).</param>
        /// <param name="annotation">Optional comment for the element (empty list in case of no comment).</param>
        /// <returns>True on success or false on errors.</returns>
        internal abstract bool AddElement(string elementName, string classifierNamespace, string classifierName, List<MEDocumentation> annotation);

        /// <summary>
        /// This method adds an external namespace to the schema. Regarding the location: it is most convenient to keep all schemas together, in which case this
        /// argument will look something like: './schemaname.xsd'. Location could by NULL if no explicit import is required.
        /// The method will silently fail (with a logged error) in case of exceptions.
        /// </summary>
        /// <param name="nsToken">Namespace token as used throughout the schema to reference this external namespace.</param>
        /// <param name="nsName">The fully qualified namespace name, typically an URI or URL.</param>
        /// <param name="nsLocation">The location of the namespace as used during import of the namespace. Must refer to a valid location. NULL if no explicit import required.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the schema.</exception>
        internal abstract void AddNamespace(string nsToken, string nsName, string nsLocation);

        /// <summary>
        /// Adds an external schema to the set of schemas that are referenced from the current schema.
        /// </summary>
        /// <param name="referencedSchema">The external schema that is referenced by the current schema.</param>
        internal abstract void AddSchemaReference(Schema referencedSchema);

        /// <summary>
        /// This operation is used to create a type definition for a simple-type Basic Business Information Entity (BBIE). These are always of type CDT (or derived from CDT).
        /// The typeName MUST be a PRIM type, so the caller must have parsed the entire class hierarchy until arrival at PRIM level.
        /// Simple types are allowed to have facets and the derivation method of a simple type that is derived from a primitive is always a restriction, a list or a union!
        /// Enumerations and Unions must NOT be added using this method, but must use methods 'addEnumClassifier' and 'addUnionClassifier' respectively.
        /// If an attribute list is specified, the simple type becomes a complex type (holding the attributes) around a simple type (optionally holding the facets). This
        /// is implemented by simply invoking the 'addComplexClassifier' method.
        /// </summary>
        /// <param name="classifierName">BBIE classifier name</param>
        /// <param name="typeName">Associated PRIM type</param>
        /// <param name="annotation">Optional comment for the classifier (empty list in case of no comment).</param>
        /// <param name="attribList">Optional list of attributes to be assigned tot the classifier. Since a simple classifier can not directly contain attributes,
        /// the type is converted into a complex type instead!</param>
        /// <param name="facetList">Optional list of facets to be applied to this type</param>
        /// <returns>TRUE in case of success, FALSE on errors. If the type already exists, no action is performed and the operation returns TRUE.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the schema.</exception>
        internal abstract bool AddSimpleClassifier(string classifierName, string typeName, List<MEDocumentation> annotation, List<SupplementaryAttribute> attribList, List<Facet> facetList);

        /// <summary>
        /// Returns a schema-specific file extension.
        /// </summary>
        /// <returns>Schema-specific file extension, including separator character (e.g. ".xsd").</returns>
        internal abstract string GetSchemaFileExtension();

        /// <summary>
        /// Schema initializer, assigns some of the generic properties. Typically, schemas MUST be initialized once before being used.
        /// </summary>
        /// <param name="type">Identifies the type of schema that we're constructing.</param>
        /// <param name="name">Schema identifying name.</param>
        /// <param name="nsToken">A short, mnemonic name that is used as a namespace token as well as a prefix for element- and typenames to assure that they are unique.</param>
        /// <param name="ns">Schema namespace, preferably a URI.</param>
        /// <param name="version">Major, minor and build number of the schema.</param>
        internal virtual void Initialize(SchemaType type, string name, string nsToken, string ns, string version)
        {
            this._type = type;
            this._name = name;
            this._nsToken = nsToken;
            this._namespace = ns;
            this._version = version;
            this._lastError = string.Empty;
            this._isInitialized = true;
        }

        /// <summary>
        /// Merges all the objects in the provided schema (other) with our current schema. If either schema is NULL or the schemas are of different types,
        /// an exception is thrown.
        /// </summary>
        /// <param name="other">The schema to copy from.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the schema.</exception>
        /// <exception cref="ArgumentException">Schemas are of different types.</exception>
        internal abstract void Merge(Schema other);

        /// <summary>
        /// This method must be implemented by specialized schema implementations and is used to write the contents of the schema to the specified stream.
        /// </summary>
        /// <param name="stream">Schema contents must be written to this stream.</param>
        /// <param name="header">Formatted header text for the schema.</param>
        internal abstract void Save(Stream stream, string header);

        /// <summary>
        /// Sorts the current schema according to the following rules:
        /// 1) Elements before types;
        /// 2) Lower sequence key before higher sequence key (only for key values != 0);
        /// 3) Names in alphabetical order (if no key is defined).
        /// </summary>
        internal abstract void Sort();

        /// <summary>
        /// Schema default constructor. Set properties to sensible defaults.
        /// </summary>
        protected Schema()
        {
            this._type = SchemaType.Unknown;
            this._name = "NAME-NOT-SET";
            this._nsToken = string.Empty;
            this._namespace = string.Empty;
            this._version = "1.0.0";
            this._lastError = string.Empty;
            this._isInitialized = false;
        }

        /// <summary>
        /// Set last error property to provided value.
        /// </summary>
        /// <param name="error">Last error string.</param>
        protected void SetLastError (string error) { this._lastError = error; }
    }
}
