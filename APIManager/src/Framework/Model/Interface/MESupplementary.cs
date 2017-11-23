namespace Framework.Model
{
    /// <summary>
    /// Supplementary is a specialization of attribute.
    /// </summary>
    internal class MESupplementary: MEAttribute
    {
        /// <summary>
        /// Construct a new supplementary attribute by associating MESupplementary with the appropriate implementation object.
        /// The actual registration and construction is all performed by the base class constructor.
        /// </summary>
        /// <param name="suppID">Tool-specific instance identifier of the facet artifact within the tool repository.</param>
        internal MESupplementary(int suppID) : base(ModelElementType.Supplementary, suppID) { }

        /// <summary>
        /// Construct a new supplementary attribute by associating MESupplementary with the given implementation object.
        /// The actual registration and construction is all performed by the base class constructor.
        /// </summary>
        /// <param name="imp">Class implementation object.</param>
        internal MESupplementary(MEISupplementary imp) : base(imp) { }

        /// <summary>
        /// Copy constructor creates a new supplementary attribute from another supplementary attribute.
        /// </summary>
        /// <param name="copy">Attribute to use as basis.</param>
        internal MESupplementary(MESupplementary copy) : base(copy) { }

        /// <summary>
        /// Default constructor creates an empty interface.
        /// </summary>
        internal MESupplementary() : base() { }
    }
}
