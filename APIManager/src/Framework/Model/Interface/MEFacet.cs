namespace Framework.Model
{
    /// <summary>
    /// Facet is a specialization of attribute.
    /// </summary>
    internal class MEFacet: MEAttribute
    {
        /// <summary>
        /// Construct a new UML 'Facet' artifact by associating MEFacet with the appropriate implementation object.
        /// The actual registration and construction is all performed by the base class constructor.
        /// </summary>
        /// <param name="facetID">Tool-specific instance identifier of the facet artifact within the tool repository.</param>
        internal MEFacet(int facetID) : base(ModelElementType.Facet, facetID) { }

        /// <summary>
        /// Construct a new UML 'Facet' artifact by associating MEFacet with the given implementation object.
        /// The actual registration and construction is all performed by the base class constructor.
        /// </summary>
        /// <param name="imp">Class implementation object.</param>
        internal MEFacet(MEIFacet imp) : base(imp) { }

        /// <summary>
        /// Copy constructor creates a new Model Element Facet from another Model Element Facet.
        /// </summary>
        /// <param name="copy">Attribute to use as basis.</param>
        internal MEFacet(MEFacet copy) : base(copy) { }

        /// <summary>
        /// Default constructor creates an empty interface.
        /// </summary>
        internal MEFacet() : base() { }
    }
}
