namespace Framework.Model
{
    /// <summary>
    /// Supplementary is a specialization of attribute.
    /// </summary>
    internal abstract class MEISupplementary : MEIAttribute
    {
        /// <summary>
        /// Default constructor, mainly used to pass the model instance to the base constructor and set the correct type.
        /// </summary>
        /// <param name="model">Reference to the associated model implementation object.</param>
        protected MEISupplementary (ModelImplementation model): base(model)
        {
            this._type = ModelElementType.Supplementary;
        }
    }
}