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
    /// This class represents the definition of a primitive type within a JSON schema. Unlike XML schema, the JSON primitives are not referenced by a 
    /// namespace but are constructed for each classifier that requires them. What we keep here are simply the 'building blocks' that facilitate type
    /// construction 'on-demand'. The constructor accepts a CCTS PRIM type and translates this to a JSON typename.
    /// </summary>
    internal class JSONPrimitiveType: PrimitiveType
    {
        private string _typeName;           // The type name to be used when referencing the type. Must be unique across the schema.
        private string _format;             // Type extension descriptor.
        private JSchemaType _classifier;    // Classifier for this type, one of the supported JSON types.
        private bool _isFacetted;           // When 'true', this indicates that we have added one or more facets to the type.
        private List<Facet> _facetList;     // List of facets in case this is a facetted type.

        /// <summary>
        /// Getters for the class properties:
        /// TypeName = Returns the associated JSON type as a JSchema object.
        /// Classifier = Returns the JSON base-type classifier.
        /// IsFacetted = Returns true if this is a facetted type.
        /// FacetList = Returns the list of facets (if IsFacetted is true).
        /// Format = Returns the specialized type-format string.
        /// </summary>
        internal string TypeName              { get { return this._typeName; } }
        internal JSchemaType Classifier       { get { return this._classifier; } }
        internal bool IsFacetted              { get { return this._isFacetted; } }
        internal List<Facet> FacetList        { get { return this._facetList; } }
        internal string Format                { get { return this._format; } }

        /// <summary>
        /// Simple type constructor. Accepts a CCTS PRIM type and translates this to a qualified JSON schema. The title of this schema will be the original
        /// type name.
        /// Note: DOES NOT support Union, this must be treated elsewhere!
        /// </summary>
        /// <param name="schema">Schema in which the type must be created.</param>
        /// <param name="type">PRIM type to be translated. Supported PRIM types are: binary, boolean, date, gDay, gMonth, gMonthDay, gYear, gYearMonth, dateTime, 
        /// decimal, duration, int, integer, normalizedString, string and time.</param>
        internal JSONPrimitiveType(JSONSchema schema, string type): base(schema, type)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONPrimitiveType >> Going to translate '" + type + "' to schema type.");
            this._typeName = type;
            this._isFacetted = false;
            this._facetList = null;
            this._format = GetFormat(type);
            this._classifier = GetClassifier(this.PrimitiveName);
        }

        /// <summary>
        /// Simple type constructor. Accepts a CCTS PRIM type with a set of facets and translates this to a qualified JSON schema. In order to create
        /// a unique name, the type name is prefixed with the owning classifier name.
        /// Note: DOES NOT support Union, this must be treated elsewhere!
        /// Since the classifier MUST own the facetted type, the type is always created in the specified schema, which must be the schema of the owning
        /// classifier.
        /// </summary>
        /// <param name="schema">Schema in which the type must be created.</param>
        /// <param name="classifierName">The name of the 'owning' classifier.</param>
        /// <param name="type">PRIM type to be translated. Supported PRIM types are: binary, boolean, date, gDay, gMonth, gMonthDay, gYear, gYearMonth, dateTime, 
        /// decimal, duration, int, integer, normalizedString, string and time.</param>
        /// <param name="facetList">List of facets to be assigned to the type. Not all facets are valid for all types so the constructor discards any facet that
        /// does not match the type.</param>
        internal JSONPrimitiveType(JSONSchema schema, string classifierName, string type, List<Facet> facetList): base(schema, classifierName, type)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONPrimitiveType (facetted) >> Going to translate " + type + " to schema type.");

            this._format = GetFormat(type);
            this._classifier = GetClassifier(this.PrimitiveName);

            if (facetList == null || facetList.Count == 0)
            {
                Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONPrimitiveType (facetted) >> No facets in list, treat as simple type...");
                this._typeName = type;
                this._isFacetted = false;
                this._facetList = null;
            }
            else
            {
                Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONPrimitiveType (facetted) >> Simple type with facets!");
                this._typeName = this.ClassifierName + Conversions.ToPascalCase(type);
                this._isFacetted = true;
                this._facetList = facetList;
            }
        }

        /// <summary>
        /// Converts the type descriptor into a corresponding JSON Schema.
        /// If we have a 'file' primitive type, this will be translated to a NULL schema with 'Format' set to 'file' so we can recognize
        /// this as a special case when needed. 
        /// </summary>
        /// <returns>JSON Schema that represents this primitive type.</returns>
        internal JSchema ConvertToSchema()
        {
            if (this._classifier != JSchemaType.None)
            {
                var primType = new JSchema
                {
                    Title = this._typeName,
                    Type = this._classifier
                };
                if (this._format != string.Empty) primType.Format = this._format;
                if (this._isFacetted) foreach (JSONFacet facet in this._facetList) facet.AddFacet(ref primType);
                return primType;
            }
            else return new JSchema();
        }

        /// <summary>
        /// Checks the provided facet token against a list of valid tokens for the associated JSON type and returns TRUE in case the facet 
        /// is valid for the type or FALSE otherwise.
        /// </summary>
        /// <param name="facetToken">One of the supported token values from Token.</param>
        /// <returns>TRUE if valid token for the type, FALSE otherwise.</returns>
        internal override bool IsValidFacet(string facetToken)
        {
            // This table consists of tuples <JSON name> - <Allowed facet tokens>
            string[,] validationTable =
               { {"boolean",    ""},
                 {"number",     "MINI,MAXI,MINE,MAXE"},
                 {"integer",    "MINI,MAXI,MINE,MAXE"},
                 {"string",     "LEN,MINL,MAXL,ENUM,PAT"} };

            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONPrimitiveType.isValidFacet >> Going to check facet token " + facetToken + " for type " + this._typeName);
            bool typeValid = false;
            facetToken = facetToken.ToUpper();
            string jsonName = !string.IsNullOrEmpty(this.PrimitiveName) ? this.PrimitiveName : "string";

            for (int i = 0; i < validationTable.GetLength(0); i++)
            {
                if (String.Compare(jsonName, validationTable[i, 0], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONPrimitiveType.isValidFacet >> Found primitive, now going to check facet...");
                    typeValid = validationTable[i, 1].Contains(facetToken);
                    break;
                }
            }
            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONPrimitiveType.isValidFacet >> Returns: " + typeValid);
            return typeValid;
        }

        /// <summary>
        /// This method translates a CCTS PRIM name to the corresponding JSON schema type name. The 'any' type is returned 'as-is' and required special
        /// attention by the schema processor. Same holds for the "file" type, which also requires special attention.
        /// Note that we do NOT support the JSON 'Format' qualifier since the formal set of options is severely restricted.
        /// </summary>
        /// <param name="primName">CCTS PRIM name</param>
        /// <returns>JSON type name or empty string in case no suitable translation was possible.</returns>
        protected override string TranslatePrimitive(string primName)
        {
            // This table consists of tuples <PRIM name> - <Corresponding JSON name>
            string[,] translateTable =
                 {{"anytype",           "anyType"},
                 {"binary",             "string"},
                 {"boolean",            "boolean"},
                 {"date",               "string"},
                 {"datetime",           "string"},
                 {"decimal",            "number"},
                 {"duration",           "string"},
                 {"file",               "file"},
                 {"gDay",               "string"},
                 {"gMonth",             "string"},
                 {"gMonthDay",          "string"},
                 {"gYear",              "string"},
                 {"gYearMonth",         "string"},
                 {"int",                "integer"},
                 {"integer",            "integer"},
                 {"normalizedstring",   "string"},
                 {"enumeration",        "string" },
                 {"string",             "string"},
                 {"time",               "string"}};

            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONPrimitiveType.translatePrim >> Going to translate " + primName + " to schema type.");

            for (int i = 0; i < translateTable.GetLength(0); i++)
            {
                if (String.Compare(primName, translateTable[i, 0], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONPrimitiveType.translatePrim >> Found primitive, going to swap against schema type: " + translateTable[i, 1]);
                    return translateTable[i, 1];
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Helper function that determines the correct JSON Type based on the primitive type name that has been constructed by the base class.
        /// If we don't have a valid primitive type name, the function return the NULL type.
        /// Empty primitive names can occur in case we use an enumeration as a supplementary attribute. In that case, we assume the primitive
        /// type to be a 'String'. The 'anyType' is translated to schema type 'None'.
        /// </summary>
        /// <param name="primitiveTypeName">JSON type string.</param>
        /// <returns>JSON type enumeration or the NULL type in case of unrecognized types.</returns>
        private JSchemaType GetClassifier(string primitiveTypeName)
        {
            if (primitiveTypeName != string.Empty)
            {
                switch (primitiveTypeName)
                {
                    case "string":  return JSchemaType.String;
                    case "boolean": return JSchemaType.Boolean;
                    case "integer": return JSchemaType.Integer;
                    case "number":  return JSchemaType.Number;
                    case "anyType": return JSchemaType.None;
                    default:        return JSchemaType.Null;
                }
            }
            else return JSchemaType.String;
        }

        /// <summary>
        /// JSON only has a small number of 'base primitives'. Some implementations, such as OpenAPI (Swagger) therefor use the JSON 'format'
        /// property to define specializations of a primitive. This method uses the OpenAPI specification to retrieve the format string for
        /// a given CCTS Primitive when applicable. Since 'format' is a 'free-text' field, we use this to specify some of the other CCTS
        /// primitives, even though these might not be recognized by existing tooling. We also use this to keep the 'file' special type around
        /// since the base-type will be translated to NULL (unknown base type). By keeping 'file' around as a format, we are able to 
        /// properly process the type when needed.
        /// </summary>
        /// <param name="primName">CCTS PRIM name</param>
        /// <returns>JSON format string, empty string if not applicable for given type.</returns>
        private string GetFormat(string primName)
        {
            // This table consists of tuples <PRIM name> - <Corresponding JSON name>
            string[,] translateTable =
                 {{"anytype",           ""},
                 {"binary",             "binary"},
                 {"boolean",            ""},
                 {"date",               "date"},
                 {"datetime",           "date-time"},
                 {"decimal",            ""},
                 {"duration",           "duration"},
                 {"file",               "file"},
                 {"gDay",               "day"},
                 {"gMonth",             "month"},
                 {"gMonthDay",          "month-day"},
                 {"gYear",              "year"},
                 {"gYearMonth",         "year-month"},
                 {"int",                ""},
                 {"integer",            ""},
                 {"normalizedstring",   ""},
                 {"enumeration",        "" },
                 {"string",             ""},
                 {"time",               "time"}};

            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONPrimitiveType.GetFormat >> Going to translate " + primName + " to format name.");

            for (int i = 0; i < translateTable.GetLength(0); i++)
            {
                if (String.Compare(primName, translateTable[i, 0], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONPrimitiveType.GetFormat >> Found primitive, going to swap against format string: " + translateTable[i, 1]);
                    return translateTable[i, 1];
                }
            }
            return string.Empty;
        }
    }
}
