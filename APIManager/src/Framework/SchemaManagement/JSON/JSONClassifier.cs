using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using Framework.Logging;
using Framework.Context;

namespace Framework.Util.SchemaManagement.JSON
{
    /// <summary>
    /// This class represents a classifier definition that is used in JSON schema. Unlike XML Schema where classifiers are types that are referenced and
    /// can be extended, JSON classifiers are almost always expanded 'in-line' as part of the attribute that uses them.
    /// However, if the classifier has attributes, it becomes a JSON Object that is defined in the 'definition' section of the Schema and has to be
    /// referenced instead of expanded.
    /// </summary>
    internal class JSONClassifier
    {
        private string _name;                           // Classifier name.
        private JSONPrimitiveType _baseType;            // The primitive type that the classifier is based on.
        private bool _isConstructed;                    // If the classifier has attributes, it becomes an object and has to be referenced.
        private JSchema _constructedClassifier;         // The associated schema object in case of constructed classifiers.
        private List<MEDocumentation> _documentation;   // Classifier annotation. Is part of the object in case of constructed classifiers.

        /// <summary>
        /// Getters for the class properties:
        /// Name = Returns the classifier name.
        /// BaseType = Returns the primitive type descriptor for the classifier.
        /// ReferenceType = Returns TRUE if this is in fact an object that has to be referenced instead of expanded.
        /// ReferenceClassifier = Reference to classifier object.
        /// Documentation = List of documentation objects for this classifier.
        /// </summary>
        internal string Name                              { get { return this._name; } }
        internal JSONPrimitiveType BaseType               { get { return this._baseType; } }
        internal bool IsReferenceType                     { get { return this._isConstructed; } }
        internal JSchema ReferenceClassifier              { get { return this._constructedClassifier; } }
        internal List<MEDocumentation> Documentation      { get { return this._documentation; } }

        /// <summary>
        /// Creates a new classifier definition and registers it in the specified schema. Typically, a classifier is a primitive type and is expanded whenever
        /// we want to create an attribute of that type. However, if the classifier has attributes, it is created as an object in de 'definitions' section of
        /// the schema and must be referenced instead of expanded in-line!
        /// Constructed classifiers have a predefined property name 'content', which must hold the actual contents. Since all other properties are
        /// supplementary attributes, which receive a '@' as a suffix, we won't have name clashes.
        /// </summary>
        /// <param name="schema">The schema in which we define the classifier.</param>
        /// <param name="classifierName">The original name of the classifier.</param>
        /// <param name="primType">The classifier type, MUST be a PRIM type.</param>
        /// <param name="annotation">Classifier annotation.</param>
        /// <param name="attribList">Optional list of supplementary attributes for the classifier.</param>
        /// <param name="facetList">List of facets for the classifier.</param>
        internal JSONClassifier(JSONSchema schema, string classifierName, string primType, List<MEDocumentation> annotation, List<SupplementaryAttribute> attribList, List<Facet> facetList)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONClassifier >> Creating classifier '" + classifierName + "' of type '" + primType + "'...");

            CreateClassifier(schema, classifierName, primType, annotation, attribList, facetList);
        }

        /// <summary>
        /// Creates a new enumeration classifier definition and registers it in the specified schema. For JSON, enumerations are always a constructed type
        /// of type string and can have NO facets (if the model specifies them, they are ignored). In theory, an enumeration CAN have attributes. Since
        /// it's a constructed type anyway, adding these attributes won't change the overall use of the classifier (it's still a referenced type).
        /// </summary>
        /// <param name="schema">The schema in which we define the classifier.</param>
        /// <param name="classifierName">The original name of the classifier.</param>
        /// <param name="annotation">Classifier annotation.</param>
        /// <param name="attribList">Optional list of supplementary attributes for the classifier.</param>
        /// <param name="enumList">List of enumeration values.</param>
        internal JSONClassifier(JSONSchema schema, string classifierName, List<MEDocumentation> annotation, List<SupplementaryAttribute> attribList, List<EnumerationItem> enumList)
        {
            const string EnumPrim = "String";

            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONClassifier >> Creating enumeration classifier '" + classifierName + "'...");

            // If we have an enumeration without values (which happens occasionally), we 'demote' the classifier to a simple string type...
            if (enumList.Count == 0) CreateClassifier(schema, classifierName, EnumPrim, annotation, attribList, null);
            else
            {
                // Make sure we have a valid base type (String in this case)...
                this._baseType = schema.FindPrimitiveType(EnumPrim);
                if (this._baseType == null)
                {
                    // Primitive not yet registered in schema. Create and register...
                    this._baseType = new JSONPrimitiveType(schema, EnumPrim);
                    schema.AddPrimitiveType(this._baseType);
                }
                this._name = classifierName;
                this._documentation = annotation;
                this._isConstructed = true;             // We don't want enumerations to be expanded in-line, so mark it as constructed.

                this._constructedClassifier = new JSchema
                {
                    Title = classifierName,
                    Type = (attribList.Count > 0) ? JSchemaType.Object : JSchemaType.String,
                    AllowAdditionalProperties = false
                };

                foreach (EnumerationItem enumItem in enumList) this._constructedClassifier.Enum.Add(new JValue(enumItem.Name));

                if (attribList.Count > 0)
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONClassifier >> Enumeration with additional properties...");
                    foreach (JSONSupplementaryAttribute attrib in attribList)
                    {
                        if (attrib.IsValid) this._constructedClassifier.Properties.Add(attrib.Name, attrib.AttributeClassifier);
                    }
                }
                schema.AddClassifier(this);
            }
        }

        /// <summary>
        /// Creation method as a separate method so we can call this from multiple constructors!
        /// </summary>
        /// <param name="schema">The schema in which we define the classifier.</param>
        /// <param name="classifierName">The original name of the classifier.</param>
        /// <param name="primType">The classifier type, MUST be a PRIM type.</param>
        /// <param name="annotation">Classifier annotation.</param>
        /// <param name="attribList">Optional list of supplementary attributes for the classifier.</param>
        /// <param name="facetList">List of facets for the classifier.</param>
        private void CreateClassifier(JSONSchema schema, string classifierName, string primType, List<MEDocumentation> annotation, List<SupplementaryAttribute> attribList, List<Facet> facetList)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONClassifier.CreateClassifier >> Creating classifier '" + classifierName + "' of type '" + primType + "'...");

            // First of all, we have to check the facetList for a possible type replacement (might happen for e.g. decimals, which 
            // could be transformed into integers)...
            if (facetList != null && facetList.Count > 0)
            {
                foreach (Facet facet in facetList)
                {
                    string replacementType = facet.CheckReplaceType(primType);
                    if (replacementType != string.Empty)
                    {
                        Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONClassifier.CreateClassifier >> Facet forced replacement from '" +
                                         primType + "' to '" + replacementType + "'!");
                        primType = replacementType;
                        facetList.Remove(facet);        // Remove from list, has fulfilled its purpose.
                        break;
                    }
                }
            }

            // We have to check count again since the previous code block could have reduced the list...
            if (facetList == null || facetList.Count == 0)
            {
                this._baseType = schema.FindPrimitiveType(primType);
                if (this._baseType == null)
                {
                    // Primitive not yet registered in schema. Create and register...
                    this._baseType = new JSONPrimitiveType(schema, primType);
                    schema.AddPrimitiveType(this._baseType);
                }
            }
            else
            {
                // Since we have facets, the name of the type will be based on the classifier (different classifiers can use the same primitive
                // type with different sets of facets). We simply create the primitive and then check whether the schema already knows about it...
                this._baseType = new JSONPrimitiveType(schema, classifierName, primType, facetList);
                if (!schema.HasPrimitiveType(this._baseType.TypeName)) schema.AddPrimitiveType(this._baseType);
            }
            this._name = classifierName;
            this._documentation = annotation;

            if (attribList.Count > 0)
            {
                Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONClassifier >> Classifier has properties, convert to structured type...");
                this._isConstructed = true;
                this._constructedClassifier = new JSchema
                {
                    Title = this._name,
                    Type = JSchemaType.Object,
                    AllowAdditionalProperties = false
                };
                this._constructedClassifier.Properties.Add("content", this._baseType.ConvertToSchema());

                foreach (JSONSupplementaryAttribute attrib in attribList)
                {
                    if (attrib.IsValid) this._constructedClassifier.Properties.Add(attrib.Name, attrib.AttributeClassifier);
                }
            }
            else
            {
                Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONClassifier >> Simple classifier.");
                this._isConstructed = false;
                this._constructedClassifier = null;
            }
            schema.AddClassifier(this);
        }
    }
}
