using System;
using Framework.Logging;

namespace Framework.Util.SchemaManagement
{
    /// <summary>
    /// A Facet represents a specific processing directive for a class attribute. For most schemas, Facets are supported only for primitive types. 
    /// Since implementation of Facets depends on the type of Schema, it's defined as an abstract class and thus required specialised implementations.
    /// </summary>
    internal abstract class Facet : IEquatable<Facet>
    {
        private string _facetToken;
        private string _value;

        /// <summary>
        /// Construct a new Facet object.
        /// </summary>
        /// <param name="facetName">Name of the facet according W3C XSD standard Vs 1.1.</param>
        /// <param name="facetValue">The value associated with the Facet. The constructor does NOT perform semantic validation.</param>
        internal Facet(string facetName, string facetValue)
        {
            // This table is used to validate facet names and translate them to tokens for further processing.
            string[,] translateTable =
                 {{"length",        "LEN"},
                 {"minlength",      "MINL"},
                 {"maxlength",      "MAXL"},
                 {"pattern",        "PAT"},
                 {"enumeration",    "ENUM"},
                 {"maxinclusive",   "MAXI"},
                 {"maxexclusive",   "MAXE"},
                 {"mininclusive",   "MINI"},
                 {"minexclusive",   "MINE"},
                 {"fractiondigits", "FD"},
                 {"totaldigits",    "TD"},
                 {"whitespace",     "WS"},
                 {"datetype",       "DT"}};

            Logger.WriteInfo("Framework.Util.SchemaManagement.Facet >> Creating facet " + facetName + " with value " + facetValue);

            this._facetToken = string.Empty;
            this._value = string.Empty;

            for (int i = 0; i < translateTable.GetLength(0); i++)
            {
                if (string.Compare(facetName, translateTable[i, 0], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.Facet >> Facet found, using token: " + translateTable[i, 1]);
                    this._facetToken = translateTable[i, 1];
                    this._value = facetValue;
                    break;
                }
            }

            if (this._facetToken == string.Empty)
            {
                Logger.WriteWarning("Illegal facet name '" + facetName + "' received, instance is empty!");
                return;
            }
        }

        /// <summary>
        /// This method checks whether we're dealing with a type-replacement Facet. These types of Facets are used to generate a primitive type different from the 
        /// original base type. Currently, we support two such Facet types:
        /// 1) fractionDigits = 0 --> supported for primitive type 'decimal' and replaces 'decimal' by 'integer';
        /// 2) dateType = [value from DateTypeCode] --> replaces 'date' by a specialised date variant;
        /// </summary>
        /// <param name="typeName">The type name to check</param>
        /// <returns>Replacement type or empty string if nothing to replace.</returns>
        internal string CheckReplaceType(string typeName)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.Facet.checkReplaceType >> Checking type: " + typeName + " on Facet: " + 
                             this._facetToken + " with value: " + this._value);

            string retVal = string.Empty;
            if (string.Compare(typeName, "decimal", StringComparison.OrdinalIgnoreCase) == 0)
            {
                // If we have a base-type of 'decimal' and a Facet 'fractionDigits=0', we replace 'decimal' by 'integer'...
                retVal = (this._facetToken == "FD" && this._value == "0") ? "integer" : string.Empty;
            }
            else if (string.Compare(typeName, "date", StringComparison.OrdinalIgnoreCase) == 0)
            {
                // If we have a base-type of 'date' and a Facet 'dateType' with a value other then 'date', we return the associated date specialization...
                if (this._facetToken == "DT")
                {
                    retVal = this._value;
                }
            }
            Logger.WriteInfo("Framework.Util.SchemaManagement.Facet.checkReplaceType >> Returning '" + retVal + "'...");
            return retVal;
        }

        /// <summary>
        /// Implements the 'Equals' interface in order to search for facets in lists.
        /// Two facets are considered equal if their tokens (facet names) are identical.
        /// </summary>
        /// <param name="other">Facet to compare against.</param>
        /// <returns></returns>
        public bool Equals(Facet other)
        {
            return this._facetToken == other.FacetToken;
        }

        /// <summary>
        /// Returns a short mnemonic name that represents this facet. Supported values are:
        /// length -> LEN,
        /// minLength       -> MINL,
        /// maxLength       -> MAXL,
        /// pattern         -> PAT,
        /// enumeration     -> ENUM,
        /// maxInclusive    -> MAXI,
        /// maxExclusive    -> MAXE,
        /// minInclusive    -> MINI,
        /// minExclusive    -> MINE,
        /// fractionDigits  -> FD,
        /// totalDigits     -> TF,
        /// whiteSpace      -> WS
        /// <returns>tokenstring</returns>
        internal string FacetToken
        {
            get { return this._facetToken; }
        }

        /// <summary>
        /// Returns the assigned facet value, which is a fixed value.
        /// </summary>
        internal string FacetValue
        {
            get { return this._value; }
        }

        /// <summary>
        /// The method checks whether the facet instance is valid and whether the facet is valid for the provided data type.
        /// </summary>
        /// <param name="thisType">Must be a PRIM datatype name.</param>
        /// <returns>TRUE is both facet has valid contents and is a valid facet for the provided type, FALSE otherwise.</returns>
        internal bool IsValid(PrimitiveType thisType)
        {
            bool validFacet = false;
            if (this._facetToken != string.Empty && thisType.IsValidFacet(FacetToken)) validFacet = true;
            return validFacet;
        }
    }
}
