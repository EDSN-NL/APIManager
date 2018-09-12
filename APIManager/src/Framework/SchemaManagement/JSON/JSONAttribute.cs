﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Framework.Logging;
using Framework.Context;

namespace Framework.Util.SchemaManagement.JSON
{
    /// <summary>
    /// Specialization of 'Attribute' for Content-type attributes. These are translated as a name (stored in the base class) and a JSON Schema 
    /// that represents the classifier.
    /// In JSON Schema, attributes are ALWAYS constructed as a key/value tuple, in which the key is the attribute name and the value a JSON
    /// schema defining the attribute classifier. For simple (non-constructed) classifiers, the schema is constructed in-line (we do not reference
    /// simple types). For constructed classifiers (including enumerated types), we use an "AllOf" JSON construct, which facilitates a reference
    /// to the classifier while still facilitating annotation with default values, array types, etc.
    /// Fixed-value attributes are ALWAYS constructed as a string enumeration with a single value. All other properties are ignored in this case
    /// (with the exception of lower-bound cardinality, we can still created an optional fixed-value attribute).
    /// </summary>
    internal class JSONContentAttribute : ContentAttribute, IJSONProperty
    {
        private JSchema _attributeClassifier;       // Defines the classifier of the attribute within the JSON schema.
        private JSchema _simpleAttributeClassifier; // Simplified definition, created for use of attributes as Properties in special contexts.
        private JSONClassifier _classifier;         // The associated classifier object in case of complex types.
        private string _annotation;                 // Contains the annotation text of the attribute.
        private string _example;                    // Optionally, the annotation could contain examples of use. We extract these separately.

        /// <summary>
        /// Implementation of the JSON Property interface:
        /// JSchema = Returns the JSON Schema object that implements the content attribute.
        /// Name = Returns the associated property name.
        /// SequenceKey = Returns the sequence identifier of the content property.
        /// IsMandatory = Checks whether the attribute must be present.
        /// </summary>
        public JSchema JSchema                      { get { return this._attributeClassifier; } }
        public new string Name                      { get { return base.Name; } }
        public new int SequenceKey                  { get { return base.SequenceKey; } }
        public new bool IsMandatory                 { get { return base.IsMandatory; } }

        /// <summary>
        /// Returns the annotation text of the attribute.
        /// </summary>
        internal string Annotation    { get { return this._annotation; } }

        /// <summary>
        /// Returns the example text for the attribute (empty string if none found).
        /// </summary>
        internal string ExampleText { get { return this._example; } }

        /// <summary>
        /// Construct a new content attribute. These attributes MUST be used in the context of the associated ABIE of which they are declared as 
        /// attributes. Content attributes have a name (the attribute name as defined in the containing ABIE) and a classifier that must be a CDT (or 
        /// derived BDT). Since our JSON schema requires classifiers to be concrete JSchema objects, we use the name of the classifier as an index 
        /// in the schema to retrieve the classifier definition (MUST have been created earlier) and work from there. If the classifier is a constructed
        /// type (including enumerations), it is referenced, if it's a simple type, the contents are copied over to a local Schema instance.
        /// Attributes have a cardinality specified by minOccurs and maxOccurs, both must be specified. If maxOccurs is '0', this is interpreted as 
        /// 'unbounded'. In case the cardinality of the attribute has an upper boundary > 1, the schema is created as an array of classifier.
        /// If the cardinality lower boundary is 0, this list becomes optional. List contents always have a lower boundary of 1!
        /// The attribute can also have EITHER a default- or a fixed value, if both are specified, only the fixed value will be used. In this case, most
        /// parameters will be ignored and the attribute is created as a simple string enumeration with a single value (i.e. the 'fixed' value).
        /// Fixed value attributes thus have a cardinality of 0 or 1 and a base-type of String, irrespective of the 'real' classifier type.
        /// Note that the constructor creates a 'dangling' attribute, e.g. it is not assigned to an ABIE yet (or registered anywhere for that matter). 
        /// It is the responsibility of the caller to pass this attribute to the associated ABIE element in due time.
        /// </summary>
        /// <param name="schema">The schema in which the attribute is created.</param>
        /// <param name="attributeName">Name of the attribute as defined in the ABIE.</param>
        /// <param name="classifierName">Classifier name as defined in the ABIE.</param>
        /// <param name="sequenceKey">When specified (not 0), this indicates the sorting order within a list of attributes and associations.</param>
        /// <param name="choiceGroup">Optional identifier for the choice group that this attribute should go to or NULL if not defined.</param>
        /// <param name="cardinality">Attribute cardinality. Upper boundary 0 is interpreted as 'unbounded'.</param>
        /// <param name="annotation">Optional comment for the content element. Empty list in case no comment is present.</param>
        /// <param name="defaultValue">Optional default value.</param>
        /// <param name="fixedValue">Optional fixed value.</param>
        /// <param name="isNillable">Set to 'true' to indicate that the attribute supports a NULL value.</param>
        internal JSONContentAttribute(JSONSchema schema,
                                      string attributeName,
                                      string classifierName,
                                      int sequenceKey,
                                      ChoiceGroup choiceGroup,
                                      Tuple<int, int> cardinality,
                                      List<MEDocumentation> annotation,
                                      string defaultValue, string fixedValue,
                                      bool isNillable) :
            base(schema, attributeName, classifierName, sequenceKey, choiceGroup, cardinality, annotation, defaultValue, fixedValue, isNillable)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONContentAttribute >> Creating attribute '" + attributeName + "' with classifier '" + classifierName +
                             "' and cardinality '" + cardinality.Item1 + "-" + cardinality.Item2 + "'.");

            if (!string.IsNullOrEmpty(fixedValue))
            {
                // If this is an attribute with a fixed value, we ignore most of the arguments and simply create an enumeration with a single value.
                Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONContentAttribute >> Fixed value '" + fixedValue + "' specified, treat as enumeration!");
                this._attributeClassifier = new JSchema
                {
                    Title = classifierName,
                    Type = JSchemaType.String,
                    Enum = { fixedValue }
                };
                this._simpleAttributeClassifier = new JSchema
                {
                    Type = JSchemaType.String,
                    Enum = { fixedValue }
                };
                this.IsValid = true;

                if (!string.IsNullOrEmpty(defaultValue))
                {
                    Logger.WriteWarning("Attribute '" + attributeName + "' of type '" +
                                        classifierName + "' has BOTH default- and fixed value, default is ignored!");
                }
            }
            else
            {
                JSONClassifier classifier = schema.FindClassifier(classifierName);
                if (classifier == null)
                {
                    Logger.WriteError("Framework.Util.SchemaManagement.JSON.JSONContentAttribute >> Attribute '" + attributeName + "' has missing classifier definition for '" + classifierName + "'!");
                    this.IsValid = false;
                    return;
                }

                // Unfortunately, since JSchema does not implement a proper copy constructor, we have to create two entirely different attribute
                // objects so we can differentiate between the 'rich' version (including a Title, AdditionalItems, Description and Example clause) and the simple
                // version, which does not have these. We can't assign one to the other since these are all pointers!
                var attribClassifier = new JSchema { Title = classifierName };
                var simpleAttributeClassifier = new JSchema();
                this._classifier = classifier;

                // In case of 'any' type, we don't make any changes to this base type, it MUST be an empty schema in order to work properly.
                if (classifier.BaseType.Classifier != JSchemaType.None)
                {
                    if (classifier.IsReferenceType)
                    {
                        // Complex classifier, we need to extend this instead of copy the contents...
                        //@@@@@@@@@ ISSUE: Some frameworks don't like the 'AllOf' construction!
                        // We use this so we can extend the existing type definition with our own Title, Description and other Facets.
                        // There are two solutions:
                        // 1) Use a simple REF (like all other external references). But in this case, NO facets other then the REF may exist.
                        // 2) Use in-line extension (copy all components from the original). This will grow the schema significantly.
                        // Or... use (1) for the simple cases when no facets have been defined and use (2) when we must extend the type.
                        Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONContentAttribute >> Constructed classifier, creating extension...");
                        attribClassifier.AllOf.Add(classifier.ReferenceClassifier);
                        simpleAttributeClassifier.AllOf.Add(classifier.ReferenceClassifier);
                    }
                    else
                    {
                        Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONContentAttribute >> Simple classifier, creating inline typedef...");
                        attribClassifier.Type = classifier.BaseType.Classifier;
                        simpleAttributeClassifier.Type = classifier.BaseType.Classifier;
                        if (classifier.BaseType.Format != string.Empty)
                        {
                            attribClassifier.Format = classifier.BaseType.Format;
                            simpleAttributeClassifier.Format = classifier.BaseType.Format;
                        }
                        if (classifier.BaseType.IsFacetted)
                        {
                            foreach (JSONFacet facet in classifier.BaseType.FacetList)
                            {
                                facet.AddFacet(ref attribClassifier);
                                facet.AddFacet(ref simpleAttributeClassifier);
                            }
                        }
                    }

                    if (defaultValue != string.Empty)
                    {
                        Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONContentAttribute >> Attribute has default value: " + defaultValue);
                        attribClassifier.Default = new JValue(defaultValue);
                        simpleAttributeClassifier.Default = new JValue(defaultValue);
                    }
                }

                // If upper boundary of cardinality > 1 (or 0, which means unbounded), we have to create an array of type, instead of only the type.
                if (cardinality.Item2 == 0 || cardinality.Item2 > 1)
                {
                    // If the classifier name ends with 'Type', we remove this before adding a new post-fix 'ListType'...
                    string listType = (classifierName.EndsWith("Type")) ? classifierName.Substring(0, classifierName.IndexOf("Type")) : classifierName;
                    listType += "ListType";
                    this._attributeClassifier = new JSchema
                    {
                        Title = listType,
                        Type = JSchemaType.Array,
                        //AllowAdditionalItems = false,  Only use this when different array elements use different schemas.
                        // For 'normal' classifiers, a list must have at least one element and the list as a whole can be made optional in the
                        // context of the 'owning' attribute...
                        MinimumItems = (cardinality.Item1 == 0) ? 1 : cardinality.Item1,  
                    };
                    this._simpleAttributeClassifier = new JSchema { Type = JSchemaType.Array };
                    if (cardinality.Item1 > 0) this._simpleAttributeClassifier.MinimumItems = cardinality.Item1;
                    if (cardinality.Item2 > 1)
                    {
                        this._attributeClassifier.MaximumItems = cardinality.Item2;
                        this._simpleAttributeClassifier.MaximumItems = cardinality.Item2;
                    }
                    this._attributeClassifier.Items.Add(attribClassifier);
                    this._simpleAttributeClassifier.Items.Add(simpleAttributeClassifier);
                }
                else
                {
                    this._attributeClassifier = attribClassifier;
                    this._simpleAttributeClassifier = simpleAttributeClassifier;
                }
            }

            // Build a description block for the element...
            // Since newlines don't work very well in JSON, we replace line breaks by spaces.
            this._annotation = string.Empty;
            this._example = string.Empty;
            bool firstOne = true;
            if (annotation.Count > 0)
            {
                Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONContentAttribute >> Adding annotation to attribute...");
                foreach (MEDocumentation docNode in annotation)
                {
                    this._annotation += firstOne ? docNode.BodyText : "  " + docNode.BodyText;

                    // There should be only one body text with one example and all others should return an empty string.
                    // If we found example text, this is stored in an extension property...
                    if (docNode.ExampleText != string.Empty)
                    {
                        this._example = docNode.ExampleText;
                        this._attributeClassifier.ExtensionData.Add("example", this._example);
                    }
                    firstOne = false;
                }
                this._attributeClassifier.Description = this._annotation;
            }

            if (this._example == string.Empty)
            {
                // We did not find an example in our attribute documentation. Check documentation of the Classifier...
                foreach (MEDocumentation docNode in this._classifier.Documentation)
                {
                    if (docNode.ExampleText != string.Empty)
                    {
                        this._example = docNode.ExampleText;
                        this._attributeClassifier.ExtensionData.Add("example", this._example);
                        break;
                    }
                }
            }

            // If the attribute is nillable, we must add another type qualifier. 
            // We ignore this for the 'simple' classifiers since the context in which these are used
            // does not allow the 'null' value anyway...
            if (isNillable)
            {
                Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONContentAttribute >> Nillable attribute!");
                this._attributeClassifier.Type |= JSchemaType.Null;
            }

            this.IsValid = true;
            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONContentAttribute >> Successfully created attribute.");
        }

        /// <summary>
        /// This function returns a formatted attribute classifier definition in JSON Schema that can be used to reference the attribute 
        /// definition from JSON Text files such as OpenAPI. In this case, we consider attributes as properties of an API operation instead 
        /// of using them as attributes of a class. By processing the attributes separately we can use them as stand-alone type definitions.
        /// If the attribute is a primitive type, the returned definition is a 'stand-alone' section (e.g. does not contain any external references).
        /// In case of enumerations, we take the actual enumerated-value list from the schema and translate this into a local string array.
        /// All other constructed types are returned as references to a schema (which may or may not work, depending on the scope of the
        /// classifier definition at the caller-end).
        /// When expanded in-line, the method returns the 'bare minimum' schema text (no Title, no AdditionalItems and no description).
        /// </summary>
        /// <returns>Formatted text string that describes the attribute in JSON Schema, alas WITHOUT surrounding braces!.</returns>
        internal string GetClassifierAsJSONSchemaText()
        {
            string schemaString = string.Empty;
            string enumList = this._attributeClassifier.ToString();
            if (enumList.Contains("enum"))
            {
                Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONContentAttribute.GetClassifierAsJSONSchemaText >> Got me an enumeration!");
                enumList = enumList.Substring(enumList.IndexOf("\"enum\":"));
                enumList = enumList.Substring(0, enumList.IndexOf(']') + 1);

                schemaString = "\"type\": ";
                if (this.Cardinality.Item2 == 0 || this.Cardinality.Item2 > 1)
                {
                    schemaString += "\"array\", \"items\": {\"type\": \"string\", " + enumList;
                    if (!string.IsNullOrEmpty(DefaultValue)) schemaString += ", \"default\": \"" + DefaultValue + "\"";
                    schemaString += "}";
                    if (this.Cardinality.Item1 > 0) schemaString += ", \"minItems\": " + this.Cardinality.Item1;
                    if (this.Cardinality.Item2 != 0) schemaString += ", \"maxItems\": " + this.Cardinality.Item2;
                }
                else
                {
                    schemaString += "\"string\", " + enumList;
                    if (!string.IsNullOrEmpty(DefaultValue)) schemaString += ", \"default\": \"" + DefaultValue + "\"";
                }
            }
            else
            {
                // This covers all 'non-enumerated type' complex classifiers. No guarantee that this reference will work in all contexts!...
                if (this._classifier != null && this._classifier.IsReferenceType)
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONContentAttribute.GetClassifierAsJSONSchemaText >> Got me a schema reference!");
                    schemaString = "\"schema\": ";
                    string typeRef = "{\"$ref\": \"#/definitions/" + this._classifier.Name + "\"}";
                    if (this.Cardinality.Item2 == 0 || this.Cardinality.Item2 > 1)
                    {
                        schemaString = schemaString + "{\"type\": \"array\", \"items\": " + typeRef + "}";
                        if (this.Cardinality.Item1 > 0) schemaString += ", \"minItems\": " + this.Cardinality.Item1;
                        if (this.Cardinality.Item2 != 0) schemaString += ", \"maxItems\": " + this.Cardinality.Item2;
                    }
                    else schemaString += typeRef;
                }
                else
                {
                    schemaString = this._simpleAttributeClassifier.ToString();
                    if (schemaString[0] == '{') schemaString = schemaString.Substring(1, schemaString.LastIndexOf('}') - 1);
                }
            }
            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONContentAttribute.GetClassifierAsJSONSchemaText >> Constructed schema '" + schemaString + "'.");
            return schemaString;
        }
    }

    /// <summary>
    /// Specialization of 'Attribute' for Supplementary-type attributes. JSON does not really know any difference between a Content- and a Supplementary
    /// attributes. However, the usage pattern is different, as are the additional (meta-) properties of the attribute.
    /// A Supplementary attributes can not be extended beyond the parameters provided to the constructor so we can store the supplementary as a combination
    /// of the attribute name and a JSON Schema.
    /// </summary>
    internal class JSONSupplementaryAttribute : SupplementaryAttribute, IJSONProperty
    {
        private JSchema _classifier;    // The associated schema for the attribute classifier.
        private string _annotation;     // Contains the annotation text of the attribute.
        private string _example;        // Optionally, the annotation could contain examples of use. We extract these separately.

        /// <summary>
        /// Implementation of the JSON Property interface:
        /// JSchema = Returns the JSON Schema object that implements the content attribute.
        /// Name = Returns the associated property name.
        /// SequenceKey = Returns the sequence identifier of the content property.
        /// IsMandatory = Returns true if this attribute must be present.
        /// </summary>
        public JSchema JSchema          { get { return this._classifier; } }
        public new string Name          { get { return base.Name; } }
        public new int SequenceKey      { get { return base.SequenceKey; } }
        public new bool IsMandatory     { get { return base.IsMandatory; } }

        /// <summary>
        /// Returns the annotation text of the attribute.
        /// </summary>
        internal string Annotation      { get { return this._annotation; } }

        /// <summary>
        /// Returns the example text for the attribute (empty string if none found).
        /// </summary>
        internal string ExampleText     { get { return this._example; } }

        /// <summary>
        /// Getters for properties of SupplementaryAttribute:
        /// JSchema = Returns the JSON schema object for this attribute.
        /// Mandatory = Returns TRUE if the attribute is mandatory.
        /// </summary>
        internal JSchema AttributeClassifier     { get { return this._classifier; } }

        /// <summary>
        /// Construct a new supplementary attribute. The attribute must have a name, a type and a usage flag (true in case of optional attribute, 
        /// false in case of mandatory attribute). The attribute can also have EITHER a default- or a fixed value, if both are specified, only the 
        /// fixed value will be used!
        /// The attribute classifier must be CCTS PRIM datatype name. UNION is NOT supported for attributes and will lead to an invalid object.
        /// </summary>
        /// <param name="schema">The schema in which the attribute is defined.</param>
        /// <param name="name">Name of the attribute, will be prefixed with configurable prefix code to indicate that this is a supplementary attribute.</param>
        /// <param name="classifier">Type of the attribute, MUST be a PRIM type, CAN be an enumeration!</param>
        /// <param name="isOptional">TRUE if this is an optional attribute, FALSE if mandatory</param>
        /// <param name="annotation">Optional comment for the attribute (empty list in case of no comment).</param>
        /// <param name="defaultValue">Default value of the attribute</param>
        /// <param name="fixedValue">Fixed value of the attribute</param>
        internal JSONSupplementaryAttribute(JSONSchema schema,
                                            string name,
                                            string classifier,
                                            bool isOptional,
                                            List<MEDocumentation> annotation,
                                            string defaultValue, string fixedValue): 
            base(schema, ContextSlt.GetContextSlt().GetStringSetting(FrameworkSettings._SupplementaryPrefixCode) + name, classifier, isOptional, annotation, defaultValue, fixedValue)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSupplementaryAttribute >> Creating attribute '" + name + "' of type '" + classifier + "'.");
            this.IsValid = true;

            if (!string.IsNullOrEmpty(fixedValue))
            {
                // In this case, we treat the attribute as an enumeration with a single value...
                Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSupplementaryAttribute >> Fixed value '" + fixedValue + "' specified, treat as enumeration!");
                this._classifier = new JSchema
                {
                    Title = classifier,
                    Type = JSchemaType.String,
                    Enum = { fixedValue }
                };
                this.IsValid = true;

                if (!string.IsNullOrEmpty(defaultValue))
                {
                    Logger.WriteWarning("Attribute '" + name + "' of type '" +
                                        classifier + "' has BOTH default- and fixed value, default is ignored!");
                }
            }
            else
            {
                JSONPrimitiveType baseType = schema.FindPrimitiveType(classifier);
                if (baseType == null)
                {
                    // Now, it CAN be that this is actually an enumeration instead of a primitive type. Check if we have a registered classifier...
                    JSONClassifier attribClassifier = schema.FindClassifier(classifier);
                    if (attribClassifier != null && attribClassifier.IsReferenceType)
                    {
                        // This is probably an enum. Create a reference to it...
                        this._classifier = new JSchema { Title = classifier };
                        this._classifier.AllOf.Add(attribClassifier.ReferenceClassifier);
                        this.IsValid = true;
                    }
                    else
                    {
                        // Primitive type not yet registered or a complex classifier that we don't want in JSON. 
                        // Create and register as primitive type...
                        baseType = new JSONPrimitiveType(schema, classifier);
                        schema.AddPrimitiveType(baseType);
                        this._classifier = baseType.ConvertToSchema();
                        this.IsValid = baseType.Classifier != JSchemaType.Null;
                    }
                }
                else
                {
                    // Existing type, use this for my attribute schema...
                    this._classifier = baseType.ConvertToSchema();
                    this.IsValid = baseType.Classifier != JSchemaType.Null;
                }
                if (!string.IsNullOrEmpty(defaultValue)) this._classifier.Default = new JValue(defaultValue);
            }

            // Build documentation block. Since newlines don't work very well in JSON, we replace line breaks by spaces.
            this._annotation = string.Empty;
            this._example = string.Empty;
            if (annotation != null && annotation.Count > 0)
            {
                bool firstOne = true;
                foreach (MEDocumentation docNode in annotation)
                {
                    this._annotation += firstOne ? docNode.BodyText : "  " + docNode.BodyText;

                    // There should be only one body text with one example and all others should return an empty string.
                    if (docNode.ExampleText != string.Empty)
                    {
                        this._example = docNode.ExampleText;
                        this._classifier.ExtensionData.Add("example", this._example);
                    }
                    firstOne = false;
                }
                this._classifier.Description = this._annotation;
            }
        }
    }
}
