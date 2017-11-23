using Newtonsoft.Json.Schema;
using Framework.Logging;

namespace Framework.Util.SchemaManagement.JSON
{
    /// <summary>
    /// A Facet represents a specific processing directive for a class attribute. 
    /// JSON only supports the following facets (all others will be ignored):
    /// length, minLength, maxLength, pattern, enumeration (indirectly by type conversion), maxInclusive, maxExclusive, minInclusive, minExclusive.
    /// The fractionDigits facet is supported only for type conversion of decimal to integer.
    /// The dateType facet is supported only for type conversion of date to specialized date.
    /// </summary>
    internal class JSONFacet : Facet
    {
        /// <summary>
        /// Construct a new Facet object.
        /// </summary>
        /// <param name="facetName">Name of the facet according W3C XSD standard Vs 1.1.</param>
        /// <param name="facetValue">The value associated with the Facet. The constructor does NOT perform semantic validation.</param>
        internal JSONFacet(string facetName, string facetValue) : base(facetName, facetValue) { }

        /// <summary>
        /// This method annotates the provided schema with the current facet IF the facet is valid for JSON AND has a valid value.
        /// In all other cases, the method will not perform any actions.
        /// </summary>
        /// <param name="thisSchema">Reference to the schema that will receive the facet.</param>
        internal void AddFacet(ref JSchema thisSchema)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.AddFacet >> Annotating schema '" + thisSchema.Title + 
                             "' with facet '[" + this.FacetToken + "]" + this.FacetValue + "'...");
            switch (this.FacetToken)
            {
                case "LEN":
                case "MAXL":
                case "MINL":
                    {
            			long val;
            			if (long.TryParse(this.FacetValue, out val))
                        {
                            if (this.FacetToken == "LEN" || this.FacetToken == "MAXL") thisSchema.MaximumLength = val;
                            if (this.FacetToken == "LEN" || this.FacetToken == "MINL") thisSchema.MinimumLength = val;
                        }
                        else Logger.WriteWarning("Framework.Util.SchemaManagement.JSON.AddFacet >> Facet '[" + this.FacetToken + "]" + this.FacetValue +
                                                 "' in Schema '" + thisSchema.Title + "' can not be properly parsed; ignored!");
                    }
                    break;

                case "MAXI":
                case "MAXE":
                case "MINI":
                case "MINE":
                    {
                    	double val;
                        if (double.TryParse(this.FacetValue, out val))
                        {
                            if (this.FacetToken.StartsWith("MAX"))
                            {
                                thisSchema.Maximum = val;
                                thisSchema.ExclusiveMaximum = (this.FacetToken.EndsWith("E")) ? true : false;
                            }
                            else
                            {
                                thisSchema.Minimum = val;
                                thisSchema.ExclusiveMinimum = (this.FacetToken.EndsWith("E")) ? true : false;
                            }
                        }
                        else Logger.WriteWarning("Framework.Util.SchemaManagement.JSON.AddFacet >> Facet '[" + this.FacetToken + "]" + this.FacetValue +
                                                 "' in Schema '" + thisSchema.Title + "' can not be properly parsed; ignored!");
                    }
                    break;

                case "PAT":
                    thisSchema.Pattern = this.FacetValue;
                    break;

                default:
                    Logger.WriteWarning("Framework.Util.SchemaManagement.JSON.AddFacet >> Facet '[" + this.FacetToken + "]" + this.FacetValue + "' not valid for JSON, ignored!");
                    break;
            }
        }

        /// <summary>
        /// Very simple support function that implicitly casts a JSONFacet to its Facet base class. 
        /// Required to convert specialized lists to base-class lists.
        /// </summary>
        /// <param name="facet">Specialized facet.</param>
        /// <returns>Reference to base class.</returns>
        internal static Facet ConvertJSONFacet(JSONFacet facet)
        {
            return facet;
        }
    }
}
