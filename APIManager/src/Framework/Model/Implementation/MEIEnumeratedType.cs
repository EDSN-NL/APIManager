using System.Collections.Generic;

namespace Framework.Model
{
    /// <summary>
    /// Implementation of enumerated type is based on implementation of data type.
    /// </summary>
    internal abstract class MEIEnumeratedType : MEIDataType
    {
        /// <summary>
        /// Returns a list of all enumeration attributes of the type. These are the attributes that are NOT marked Supplementary or Facet.
        /// If the current class does not have any enumerated values, the method searches up the hierarchy until it finds a parent that does. Searching
        /// stops at that level, e.g. the 'most specialized class that contains enumerated values' wins.
        /// </summary>
        /// <returns>List of all attributes for the current type (can be empty if no attributes are defined).</returns>
        internal abstract List<MEAttribute> GetEnumerations();

        /// <summary>
        /// Checks whether we must treat this enumeration as a simple string, i.e. ignore the list of enumeration values!
        /// </summary>
        /// <returns>True in case we must treat the enumeration as a simple string, false for 'normal' enum behavior.</returns>
        internal abstract bool MustSuppressEnumeration();

        /// <summary>
        /// Default constructor, initializes Model Element type and Data Type meta-type.
        /// </summary>
        /// <param name="model">Reference to the associated model implementation object.</param>
        protected MEIEnumeratedType (ModelImplementation model): base(model)
        {
            this._type = ModelElementType.Enumeration;          // Defines the Model Element type.
            this._metaType = MEDataType.MetaDataType.Enumeration;   // Defines the Data Type meta-type.
        }
    }
}