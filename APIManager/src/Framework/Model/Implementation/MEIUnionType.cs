using Framework.Logging;

namespace Framework.Model
{
    /// <summary>
    /// Implementation of Union type is based on implementation of data type.
    /// </summary>
    internal abstract class MEIUnionType : MEIDataType
    {
        /// <summary>
        /// Default constructor, initializes Model Element type and Data Type meta-type.
        /// </summary>
        /// <param name="model">Reference to the associated model implementation object.</param>
        protected MEIUnionType (ModelImplementation model): base(model)
        {
            this._type = ModelElementType.Union;            // Defines the Model Element type.
            this._metaType = MEDataType.MetaDataType.Union;     // Defines the Data Type meta-type.
        }
    }
}