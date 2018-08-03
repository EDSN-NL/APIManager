using System;
using System.Xml;
using System.Xml.Schema;
using System.Collections.Generic;
using Framework.Logging;
using Framework.Context;

namespace Framework.Util.SchemaManagement.XML
{
    /// <summary>
    /// This class represents a primitive type within a schema. 
    /// The constructor accepts a CCTS PRIM type and translates this to a XSD qualified simple typename.
    /// </summary>
    internal class XMLPrimitiveType: PrimitiveType
    {
        // Configuration properties used by this module:
        private const string _XMLSchemaStdNamespace = "XMLSchemaStdNamespace";

        private XmlQualifiedName _qualifiedName;
        private XmlSchemaSimpleType _facettedSimpleType;

        /// <summary>
        /// Getters for properties of SimpleType:
        /// IsFacettedType = Check whether this is a facetted simple type or not. If so, the type is represented by an XmlSchemaType instead of just an XmlQualifiedName.
        /// QualifiedType = Returns qualified name (namespace:name) or NULL in case the instance does not hold a valid type.
        /// XmlSimpleType = Returns the primitive type as an XSD type definition (valid only for facetted simple types).
        /// </summary>
        internal bool IsFacettedType                  { get { return this._facettedSimpleType != null; } }
        internal XmlQualifiedName QualifiedType       { get { return this._qualifiedName; } }
        internal XmlSchemaSimpleType XmlSimpleType    { get { return this._facettedSimpleType; } }

        /// <summary>
        /// Primitive type constructor. Accepts a CCTS PRIM type and translates this to a qualified XSD typename.
        /// Note: DOES NOT support Union, this must be treated elsewhere!
        /// </summary>
        /// <param name="schema">Schema in which the type must be created.</param>
        /// <param name="type">PRIM type to be translated. Supported PRIM types are: binary, boolean, date, gDay, gMonth, gMonthDay, gYear, gYearMonth, dateTime, 
        /// decimal, duration, int, integer, normalizedString, string and time.</param>
        internal XMLPrimitiveType(XMLSchema schema, string type): base(schema, type)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLPrimitiveType >> Going to translate " + type + " to schema type.");
            this._facettedSimpleType = null;
            if (this.PrimitiveName != string.Empty)
            {
                this._qualifiedName = new XmlQualifiedName(this.PrimitiveName, ContextSlt.GetContextSlt().GetConfigProperty(_XMLSchemaStdNamespace));
                Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLPrimitiveType >> Type set to: " + this._qualifiedName);
            }
            else
            {
                // If we end up here, the classifier is NOT a primitive type. This COULD be, because it is an enumeration passed in an attribute.
                // Any way, we're going to use the schema namespace and create a type in there....
                this._qualifiedName = new XmlQualifiedName(type, schema.SchemaNamespace);
                Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLPrimitiveType >> NOT a primitive type, created '" + schema.SchemaNamespace + ":" + type + "' instead!");
            }
        }

        /// <summary>
        /// Primitive type constructor. Accepts a CCTS PRIM type with a set of facets and translates this to a qualified XSD typename. In order to create
        /// a unique name, the XSD type name is prefixed with the owning classifier name.
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
        internal XMLPrimitiveType(XMLSchema schema, string classifierName, string type, List<Facet> facetList): base(schema, classifierName, type)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLPrimitiveType (facetted) >> Going to translate " + type + " to schema type.");
            string typeName = string.Empty;
            XmlQualifiedName qName;

            if (this.PrimitiveName != string.Empty)
            {
                qName = new XmlQualifiedName(this.PrimitiveName, ContextSlt.GetContextSlt().GetConfigProperty(_XMLSchemaStdNamespace)); // This is the actual base primitive type.
                typeName = classifierName + Conversions.ToPascalCase(this.PrimitiveName);
                Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLPrimitiveType (facetted) >> Type set to: " + typeName);
            }
            else
            {
                // If we end up here, the classifier is NOT a primitive type. This COULD be, because it is an enumeration passed in an attribute.
                // Any way, we're going to use the original name instead and define it in the current schema...
                qName = new XmlQualifiedName(type, schema.SchemaNamespace);
                typeName = classifierName + Conversions.ToPascalCase(type);
                Logger.WriteInfo("Framework.Util.SchemaManagement.XML.PrimitiveType (facetted) >> NOT a primitive type, created '" + typeName + "' instead!");
            }

            // Now we have to create an actual type to be used instead of the original qualified name...
            this._qualifiedName = new XmlQualifiedName(typeName, schema.SchemaNamespace);
            this._facettedSimpleType = new XmlSchemaSimpleType() { Name = typeName };
            if (schema.AddToSchema(this._facettedSimpleType))
            {
                Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLPrimitiveType (facetted) >> Successfully added classifier '" + typeName + "' with type '" + this.PrimitiveName + "'.");
            }
            else
            {
                Logger.WriteWarning("Warning: duplicate type '" + typeName + "' in schema with namespace token '" + schema.NSToken + "' skipped.");
                return;
            }

            // The derivation method for these simple types is always a restriction!
            Logger.WriteInfo("Framework.Util.SchemaManagement.XMLPrimitiveType (facetted) >> Creating restricted derivation for simple type...");
            var restriction = new XmlSchemaSimpleTypeRestriction();
            this._facettedSimpleType.Content = restriction;
            restriction.BaseTypeName = qName;
            foreach (XMLFacet facet in facetList)
            {
                if (facet.IsValid(this)) restriction.Facets.Add(facet.GetXmlFacet());
            }
        }

        /// <summary>
        /// Checks the provided facet token against a list of valid tokens for the associated XSD type and returns TRUE in case the facet if valid for the type or FALSE otherwise.
        /// </summary>
        /// <param name="facetToken">One of the supported token values from CDMMgr.XMLSchema.Token.</param>
        /// <returns>TRUE if valid token for the type, FALSE otherwise.</returns>
        internal override bool IsValidFacet(string facetToken)
        {
            // This table consists of tuples <XSD name> - <Allowed facet tokens>
            string[,] validationTable =
                 {{"base64Binary",       "LEN,MINL,MAXL"},
                 {"boolean",            ""},
                 {"date",               "MINI,MAXI,MINE,MAXE,ENUM"},
                 {"datetime",           "MINI,MAXI,MINE,MAXE,ENUM"},
                 {"decimal",            "MINI,MAXI,MINE,MAXE,ENUM,TD,FD"},
                 {"duration",           "MINI,MAXI,MINE,MAXE,ENUM"},
                 {"gDay",               "MINI,MAXI,MINE,MAXE,ENUM"},
                 {"gMonth",             "MINI,MAXI,MINE,MAXE,ENUM"},
                 {"gMonthDay",          "MINI,MAXI,MINE,MAXE,ENUM"},
                 {"gYear",              "MINI,MAXI,MINE,MAXE,ENUM"},
                 {"gYearMonth",         "MINI,MAXI,MINE,MAXE,ENUM"},
                 {"int",                "MINI,MAXI,MINE,MAXE,ENUM,TF,FD"},
                 {"integer",            "MINI,MAXI,MINE,MAXE,ENUM,TF,FD"},
                 {"normalizedstring",   "LEN,MINL,MAXL,ENUM"},
                 {"string",             "LEN,MINL,MAXL,ENUM"},
                 {"time",               "MINI,MAXI,MINE,MAXE,ENUM"}};

            Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLPrimitiveType.isValidFacet >> Going to check facet token " + facetToken + " for type " + this._qualifiedName.Name);
            bool facetValid = false;
            facetToken = facetToken.ToUpper();
            string xsdName = !string.IsNullOrEmpty(this.PrimitiveName) ? this.PrimitiveName : this._qualifiedName.Name;

            if (facetToken == "PAT" || facetToken == "WS")
            {
                Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLPrimitiveType.isValidFacet >> PAT or WS are always valid.");
                facetValid = true;
            }
            else
            {
                for (int i = 0; i < validationTable.GetLength(0); i++)
                {
                    if (String.Compare(xsdName, validationTable[i, 0], StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLPrimitiveType >> Found primitive, now going to check facet...");
                        facetValid = validationTable[i, 1].Contains(facetToken);
                        break;
                    }
                }
            }
            Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLPrimitiveType.isValidFacet >> Returns: " + facetValid);
            return facetValid;
        }

        /// <summary>
        /// This method translates a CCTS PRIM name to the corresponding XSD schema type name.
        /// </summary>
        /// <param name="primName">CCTS PRIM name</param>
        /// <returns>XSD type name or empty string in case no suitable translation was possible.</returns>
        protected override string TranslatePrimitive(string primName)
        {
            // This table consists of tuples <PRIM name> - <Corresponding xsd name>
            string[,] translateTable =
                 {{"anytype",           "anyType"},
                 {"binary",             "base64Binary"},
                 {"boolean",            "boolean"},
                 {"date",               "date"},
                 {"datetime",           "dateTime"},
                 {"decimal",            "decimal"},
                 {"duration",           "duration"},
                 {"gDay",               "gDay"},
                 {"gMonth",             "gMonth"},
                 {"gMonthDay",          "gMonthDay"},
                 {"gYear",              "gYear"},
                 {"gYearMonth",         "gYearMonth"},
                 {"int",                "int"},
                 {"integer",            "integer"},
                 {"normalizedstring",   "normalizedString"},
                 {"enumeration",        "string" },
                 {"string",             "string"},
                 {"time",               "time"}};

            Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLPrimitiveType.translatePrim >> Going to translate " + primName + " to schema type.");

            for (int i = 0; i < translateTable.GetLength(0); i++)
            {
                if (String.Compare(primName, translateTable[i, 0], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLPrimitiveType.translatePrim >> Found primitive, going to swap against schema type: " + translateTable[i, 1]);
                    return translateTable[i, 1];
                }
            }
            return string.Empty;
        }
    }
}
