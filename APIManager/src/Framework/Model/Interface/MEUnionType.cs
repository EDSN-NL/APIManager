namespace Framework.Model
{
    /// <summary>
    /// Representation of a Union data type. In our model, this is a specialization of the DataType
    /// model element.
    /// </summary>
    internal class MEUnionType: MEDataType
    {
        /// <summary>
        /// Construct a new Union data type by associating MEUnionType with the appropriate 
        /// implementation object.
        /// The actual registration and construction is all performed by the base class constructor.
        /// </summary>
        /// <param name="unionID">Tool-specific instance identifier of the union within the tool repository.</param>
        internal MEUnionType(int unionID) : base(ModelElementType.Union, unionID) { }

        /// <summary>
        /// Construct a new Union data type by associating MEUnionType with the appropriate 
        /// implementation object.
        /// The actual registration and construction is all performed by the base class constructor.
        /// </summary>
        /// <param name="unionGUID">Globally unique instance identifier of the union.</param>
        internal MEUnionType(string unionGUID) : base(ModelElementType.Union, unionGUID) { }

        /// <summary>
        /// Construct a new Union data type by associating MEUnionType with the given 
        /// implementation object.
        /// The actual registration and construction is all performed by the base class constructor.
        /// </summary>
        /// <param name="imp">Union implementation object.</param>
        internal MEUnionType(MEIUnionType imp) : base(imp) { }

        /// <summary>
        /// Copy constructor creates a new Union Model Element from another Union Model Element.
        /// </summary>
        /// <param name="copy">Union to use as basis.</param>
        internal MEUnionType(MEUnionType copy) : base(copy) { }

        /// <summary>
        /// Default constructor creates an empty interface.
        /// </summary>
        internal MEUnionType() : base() { }
    }
}
