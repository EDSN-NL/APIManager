using Framework.Logging;

namespace Framework.Util.SchemaManagement
{
    /// <summary>
    /// This class represents a primitive type within a schema. The constructor accepts a CCTS PRIM type and translates 
    /// this to a schema-specific qualified primitive typename.
    /// </summary>
    internal abstract class PrimitiveType
    {
        private string _primName;             // The XSD base type name selected for this type.
        private string _classifierName;       // The name of the 'owning' classifier.
        private Schema _schema;               // The schema in which we're defining this type.

        /// <summary>
        /// Getters for the class properties:
        /// PrimitiveName = return the primitive-type name associated with this type definition.
        /// ClassifierName = returns the name of the classifier association with this type definition.
        /// Schema = returns the schema in which we are defining this type.
        /// </summary>
        internal string PrimitiveName     { get { return this._primName; } }
        internal string ClassifierName    { get { return this._classifierName; } }
        internal Schema Schema            { get { return this._schema; } }

        /// <summary>
        /// Primitive type constructor. Accepts a CCTS PRIM type and translates this to a qualified, schema-specific type name.
        /// Note: DOES NOT support Union, this must be treated elsewhere!
        /// </summary>
        /// <param name="schema">Schema in which the type must be created.</param>
        /// <param name="type">PRIM type to be translated. Supported PRIM types are: binary, boolean, date, gDay, gMonth, gMonthDay, gYear, gYearMonth, dateTime, 
        /// decimal, duration, int, integer, normalizedString, string and time.</param>
        internal PrimitiveType(Schema schema, string type)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.PrimitiveType >> Going to translate " + type + " to schema type.");
            this._classifierName = null;
            this._schema = schema;
            this._primName = TranslatePrimitive(type);
        }

        /// <summary>
        /// Primitive type constructor. Accepts a CCTS PRIM type and translates this to a qualified schema-specific typename. The constructor also
        /// receives the name of the associated classifier and creates a unique name consisting of the classifier name plus the type name.
        /// This is used mainly when constructing "alternative" primitives that are classifier-specific.
        /// Note: DOES NOT support Union, this must be treated elsewhere!
        /// Since the classifier MUST own this primitive type, the type is always created in the specified schema, which must be the schema of the owning
        /// classifier.
        /// </summary>
        /// <param name="schema">Schema in which the type must be created.</param>
        /// <param name="classifierName">The name of the 'owning' classifier.</param>
        /// <param name="type">PRIM type to be translated. Supported PRIM types are: anyType, binary, boolean, date, gDay, gMonth, gMonthDay, gYear, 
        /// gYearMonth, dateTime, decimal, duration, int, integer, normalizedString, string and time.</param>
        internal PrimitiveType(Schema schema, string classifierName, string type)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.PrimitiveType (classified) >> Going to translate " + type + " to schema type.");

            // If the classifier name ends with 'Type', we remove this since it does not add much value and takes space...
            if (classifierName.EndsWith("Type")) classifierName = classifierName.Substring(0, classifierName.Length - 4);

            this._classifierName = classifierName;
            this._schema = schema;
            this._primName = TranslatePrimitive(type);
        }

        /// <summary>
        /// Checks the provided facet token against a list of valid tokens for the associated type and returns TRUE in case the facet if valid for the 
        /// type or FALSE otherwise.
        /// </summary>
        /// <param name="facetToken">One of the supported token values.</param>
        /// <returns>TRUE if valid token for the type, FALSE otherwise.</returns>
        internal abstract bool IsValidFacet(string facetToken);

        /// <summary>
        /// This method translates a CCTS PRIM name to the corresponding schema-specific type name.
        /// </summary>
        /// <param name="primName">CCTS PRIM name</param>
        /// <returns>Schema-specific type name or empty string in case no suitable translation was possible.</returns>
        protected abstract string TranslatePrimitive(string primName);
    }
}
