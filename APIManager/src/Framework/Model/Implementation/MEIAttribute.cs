using System;
using Framework.Util;

namespace Framework.Model
{
    /// <summary>
    /// Model Element Implementation Attribute adds another layer of abstraction between the generic Model Element Implementation
    /// and the tool-specific implementation. This facilitates implementation of Model Element type-specific methods at this layer
    /// without the bridge interface needing tool-specific implementation logic.
    /// </summary>
    internal abstract class MEIAttribute : ModelElementImplementation
    { 
        /// <summary>
        /// Returns the classifier of this attribute (the 'type' of the attribute).
        /// </summary>
        /// <returns>Attribute classifier as a data type instance.</returns>
        internal abstract MEDataType GetClassifier();

        /// <summary>
        /// Returns an integer representation of a 'Cardinality' string that should be interpreted as follows:
        /// - Single value 'n' is translated to 'exactly n', i.e. minOcc = maxOcc = 'n'. Unless 'n' == 0, in which case minOcc = 0, maxOcc = 1;
        /// - Single value '*' is translated to '0 to unbounded', represented by minOcc = maxOcc = 0;
        /// - Range 'n..m' is translated to minOcc = 'n', maxOcc = 'm'. Unless 'm' = 0, in which case maxOcc = 1. If this leads to minOcc > maxOcc, both values will be swapped!
        /// - Range 'n..*' is translated to minOcc = 'n', maxOcc = 0 (maxOcc == 0 is lateron interpreted as 'unbounded').
        /// All other formates will result in an IllegalCardinlity exception.
        /// </summary>
        /// <returns>Tuple consisting of minOCC, maxOcc. In case of errors, both will be -1.</returns>)]
        internal abstract Cardinality GetCardinality();

        /// <summary>
        /// Reads or writes the default value of the attribute (if defined).
        /// An attribute has a valid default value only if the attribute is not defined as a constant.
        /// </summary>
        /// <returns>Default value or empty string if no such value exists.</returns>
        internal abstract string GetDefaultValue();
        internal abstract void SetDefaultValue(string value);

        /// <summary>
        /// Returns the fixed (constant) value of the attribute (if defined).
        /// An attribute has a valid fixed value only if the attribute is defined as a constant.
        /// </summary>
        /// <returns>Default value or empty string if no such value exists.</returns>
        internal abstract string GetFixedValue();
        internal abstract void SetFixedValue(string value);

        /// <summary>
        /// Returns the index of the attribute within the owning class.
        /// </summary>
        /// <returns>The index of the attribute within the owning class.</returns>
        internal abstract int GetIndex();

        /// <summary>
        /// Returns the class that is 'owner' of this attribute, i.e. in which the attribute is declared.
        /// </summary>
        /// <returns>Owning class or NULL on errors.</returns>
        internal abstract MEClass GetOwningClass();

        /// <summary>
        /// Returns the position of the attribute, relative to other attributes within the 'owning' class. The key is incremental
        /// in steps of 100 (e.g. the first attribute has key 100, the second 200, etc.). 
        /// If the class is un-ordered, the key is always 100.
        /// </summary>
        /// <returns>Sequence key of attribute, starting at 100 or 0 if unordered.</returns>
        internal abstract int GetSequenceKey();

        /// <summary>
        /// Returns an indication whether the attribute is optional within the owning class. This implies that the
        /// 'lower bound' of the attribute cardinality must be '0'.
        /// </summary>
        /// <returns>True is this is an optional attribute, false otherwise.</returns>
        internal abstract bool IsOptional();

        /// <summary>
        /// Updates the index of the attribute within the owning class. Index must be >= 0, otherwise no action is performed.
        /// </summary>
        /// <param name="index">New index of attribute within owning class.</param>
        internal abstract void SetIndex(int index);

        /// <summary>
        /// Default constructor, mainly used to pass the model instance to the base constructor and set the correct type.
        /// </summary>
        /// <param name="model">Reference to the associated model implementation object.</param>
        protected MEIAttribute (ModelImplementation model): base(model)
        {
            this._type = ModelElementType.Attribute;
        }
    }
}