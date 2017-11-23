using System.Xml.Schema;
using Framework.Logging;

namespace Framework.Util.SchemaManagement.XML
{
    /// <summary>
    /// A Facet represents a specific processing directive for a class attribute. Facets are supported only for primitive types. 
    /// </summary>
    internal class XMLFacet : Facet
    {
        /// <summary>
        /// Construct a new Facet object.
        /// </summary>
        /// <param name="facetName">Name of the facet according W3C XSD standard Vs 1.1.</param>
        /// <param name="facetValue">The value associated with the Facet. The constructor does NOT perform semantic validation.</param>
        internal XMLFacet(string facetName, string facetValue) : base(facetName, facetValue) { }

        /// <summary>
        /// This method returns the appropriate XmlSchemaFacet instance, depending on the type of facet. 
        /// </summary>
        /// <returns>XmlSchemaFacet instance of appropriate type or NULL in case of illegal facet.</returns>
        internal XmlSchemaFacet GetXmlFacet()
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.XML.Facet.getXmlFacet >> converting token " + this.FacetToken + " with value '" + 
                             this.FacetValue + "' to XmlSchemaFacet...");
            switch (this.FacetToken)
            {
            	case "LEN":  return new XmlSchemaLengthFacet() { Value = this.FacetValue };
                case "MINL": return new XmlSchemaMinLengthFacet() { Value = this.FacetValue };
                case "MAXL": return new XmlSchemaMaxLengthFacet() { Value = this.FacetValue };
                case "PAT":  return new XmlSchemaPatternFacet() { Value = this.FacetValue };
                case "ENUM": return new XmlSchemaEnumerationFacet() { Value = this.FacetValue };
                case "MAXI": return new XmlSchemaMaxInclusiveFacet() { Value = this.FacetValue };
                case "MAXE": return new XmlSchemaMaxExclusiveFacet() { Value = this.FacetValue };
                case "MINI": return new XmlSchemaMinInclusiveFacet() { Value = this.FacetValue };
                case "MINE": return new XmlSchemaMinExclusiveFacet() { Value = this.FacetValue };
                case "FD":   return new XmlSchemaFractionDigitsFacet() { Value = this.FacetValue };
                case "TD":   return new XmlSchemaTotalDigitsFacet() { Value = this.FacetValue };
                case "WS":   return new XmlSchemaWhiteSpaceFacet() { Value = this.FacetValue };
                
                default:
                    Logger.WriteError("Framework.Util.SchemaManagement.XML.Facet.getFacet >> Illegal facet name '" + FacetToken + "' received, instance is empty!");
                    return null;
            }
        }

        /// <summary>
        /// Very simple support function that implicitly casts an XMLFacet to its Facet base class. 
        /// Required to convert specialized lists to base-class lists.
        /// </summary>
        /// <param name="facet">Specialized facet.</param>
        /// <returns>Reference to base class.</returns>
        internal static Facet ConvertXMLFacet(XMLFacet facet)
        {
            return facet;
        }
    }
}
