using System;
using System.Collections.Generic;
using Framework.Logging;
using Framework.Model;
using Framework.Util;

namespace SparxEA.Model
{
    /// <summary>
    /// Represents the repository-specific implementation of a Facet attribute type.
    /// </summary>
    internal sealed class EAMEIFacet: MEIFacet
    {
        /// <summary>
        /// Since Facet is a specialization of Attribute, we can re-use most of the attribute methods and
        /// thus can avoid a lot of duplication by keeping the attribute portion in a separate element (solution for
        /// lack of multiple inheritance).
        /// </summary>
        private EAMEIAttribute _attributePart;

        /// <summary>
        /// Fetches attribute information from EA repository and initializes the implementation object.
        /// </summary>
        internal EAMEIFacet(EAModelImplementation model, int attribID): base(model)
        {
            this._attributePart = model.GetModelElementImplementation(ModelElementType.Attribute, attribID) as EAMEIAttribute;
            this._name = this._attributePart.Name;
            this._elementID = attribID;
            this._globalID = this._attributePart.GlobalID;
            this._aliasName = this._attributePart.AliasName;
        }

        /// <summary>
        /// Fetches attribute information from EA repository and initializes the implementation object.
        /// </summary>
        internal EAMEIFacet(EAModelImplementation model, string attribGUID) : base(model)
        {
            this._attributePart = model.GetModelElementImplementation(ModelElementType.Attribute, attribGUID) as EAMEIAttribute;
            this._name = this._attributePart.Name;
            this._elementID = this._attributePart.ElementID;
            this._globalID = this._attributePart.GlobalID;
            this._aliasName = this._attributePart.AliasName;
        }

        /// <summary>
        /// Constructor that creates a new implementation instance based on a provided EA attribute instance.
        /// </summary>
        /// <param name="model">The associated model implementation.</param>
        /// <param name="attribute">The EA Attribute on which the implementation is based.</param>
        internal EAMEIFacet(EAModelImplementation model, EA.Attribute attribute) : base(model)
        {
            Logger.WriteInfo("SparxEA.Model.EAMEIFacet >> Creating Facet based on: " + attribute.Name);
            this._attributePart = model.GetModelElementImplementation(ModelElementType.Attribute, attribute.AttributeID) as EAMEIAttribute;
            this._name = this._attributePart.Name;
            this._elementID = this._attributePart.ElementID;
            this._globalID = this._attributePart.GlobalID;
            this._aliasName = this._attributePart.AliasName;
        }

        /// <summary>
        /// Assignes the specified stereotype to this attribute type.
        /// </summary>
        /// <param name="stereotype">Stereotype to be assigned.</param>
        internal override void AddStereotype(string stereotype)
        {
            this._attributePart.AddStereotype(stereotype);
        }

        /// <summary>
        /// Returns facet annotation (if present).
        /// </summary>
        /// <returns>Element annotation or empty string if nothing present.</returns>
        internal override string GetAnnotation()
        {
            return this._attributePart.GetAnnotation();
        }

        /// <summary>
        /// Returns the cardinality of this attribute.
        /// </summary>
        /// <returns>Cardinality as tuple lowerBound, upperBound (upperBound 0 is defined as unbounded).</returns>
        internal override Cardinality GetCardinality()
        {
            return this._attributePart.GetCardinality();
        }

        /// <summary>
        /// Returns the classifier of this facet (the 'type' of the facet).
        /// </summary>
        /// <returns>Attribute classifier as a data type instance or NULL on errors.</returns>
        internal override MEDataType GetClassifier()
        {
            return this._attributePart.GetClassifier();
        }

        /// <summary>
        /// Returns the default value of the facet (if defined).
        /// A facet has a valid default value only if the facet is not defined as a constant.
        /// </summary>
        /// <returns>Default value or empty string if no such value exists.</returns>
        internal override string GetDefaultValue()
        {
            return this._attributePart.GetDefaultValue();
        }

        /// <summary>
        /// Returns the fixed (constant) value of the facet (if defined).
        /// A facet has a valid fixed value only if the facet is defined as a constant.
        /// </summary>
        /// <returns>Default value or empty string if no such value exists.</returns>
        internal override string GetFixedValue()
        {
            return this._attributePart.GetFixedValue();
        }

        /// <summary>
        /// Returns the index of the attribute within the owning class.
        /// </summary>
        /// <returns>The index of the attribute within the owning class.</returns>
        internal override int GetIndex()
        {
            return this._attributePart.GetIndex();
        }

        /// <summary>
        /// Returns the class that is 'owner' of the facet, i.e. in which the facet is declared.
        /// </summary>
        /// <returns>Owning class or NULL on errors.</returns>
        internal override MEClass GetOwningClass()
        {
            return this._attributePart.GetOwningClass();
        }

        /// <summary>
        /// Returns the position of the attribute, relative to other attributes within the 'owning' class. The key is incremental
        /// in steps of 100 (e.g. the first attribute has key 100, the second 200, etc.). 
        /// If the class is un-ordered, the key is always 100.
        /// </summary>
        /// <returns>Sequence key of attribute.</returns>
        internal override int GetSequenceKey()
        {
            return this._attributePart.GetSequenceKey();
        }

        /// <summary>
        /// Simple method that searches the facet implementation for the occurence of a tag with specified name. 
        /// If found, the value of the tag is returned.
        /// </summary>
        /// <param name="tagName">The name of the tag.</param>
        /// <returns>Value of specified tag or empty string if nothing could be found.</returns>
        internal override string GetTag(string tagName)
        {
            return this._attributePart.GetTag(tagName);
        }

        /// <summary>
        /// The method checks whether one or more stereotypes from the given list of stereotypes are owned by the Facet.
        /// </summary>
        /// <param name="stereotypes">List of stereotypes to check.</param>
        /// <returns>True if at least one match is found, false otherwise.</returns>
        internal override bool HasStereotype(List<string> stereotypes)
        {
            return this._attributePart.HasStereotype(stereotypes);
        }

        /// <summary>
        /// The method checks whether the given stereotype is owned by the Facet.
        /// </summary>
        /// <param name="stereotype">Stereotype to check.</param>
        /// <returns>True if a match is found, false otherwise.</returns>
        internal override bool HasStereotype(string stereotype)
        {
            return this._attributePart.HasStereotype(stereotype);
        }

        /// <summary>
        /// Returns an indication whether the attribute is optional within the owning class. This implies that the
        /// 'lower bound' of the attribute cardinality must be '0'.
        /// </summary>
        /// <returns>True is this is an optional attribute, false otherwise.</returns>
        internal override bool IsOptional()
        {
            return this._attributePart.IsOptional();
        }

        /// <summary>
        /// Updates Facet annotation.
        /// </summary>
        /// <param name="text">The annotation text to write.</param>
        internal override void SetAnnotation(string text)
        {
            this._attributePart.SetAnnotation(text);
        }

        /// <summary>
        /// Update the alias name of the Facet.
        /// </summary>
        /// <param name="newAliasName">New name to be assigned.</param>
        internal override void SetAliasName(string newAliasName)
        {
            this._attributePart.SetAliasName(newAliasName);
            this._aliasName = newAliasName;
        }

        /// <summary>
        /// Updates the index of the attribute within the owning class. Index must be >= 0, otherwise no action is performed.
        /// </summary>
        /// <param name="index">New index of attribute within owning class.</param>
        internal override void SetIndex(int index)
        {
            this._attributePart.SetIndex(index);
        }

        /// <summary>
        /// Update the name of the Facet.
        /// </summary>
        /// <param name="newName">New name to be assigned.</param>
        internal override void SetName(string newName)
        {
            this._attributePart.SetName(newName);
            this._name = newName;
        }

        /// <summary>
        /// Writes the default value of the attribute (if defined).
        /// A default attribute value is defined as valid if the 'IsConst' indicator is set to FALSE.
        /// If the attribute is not defined as a default attribute, the update is ignored!
        /// </summary>
        /// <param name="value">The new default value.</param>
        internal override void SetDefaultValue(string value)
        {
            this._attributePart.SetDefaultValue(value);
        }

        /// <summary>
        /// Writes the fixed value of the attribute (if defined).
        /// A fixed attribute value is defined as valid if the 'IsConst' indicator is set to TRUE.
        /// If the attribute is not defined as a fixed attribute, the update is ignored!
        /// </summary>
        /// <param name="value">The new default value.</param>
        internal override void SetFixedValue(string value)
        {
            this._attributePart.SetFixedValue(value);
        }

        /// <summary>
        /// Set the tag with specified name to the specified value for the current facet. 
        /// If the tag could not be found, nothing will happen, unless the 'createIfNotExists' flag is set to true, in which case the tag will
        /// be created.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="tagValue">Value of the tag.</param>
        /// <param name="createIfNotExist">When set to true, create the tag if it's not there.</param>
        internal override void SetTag(string tagName, string tagValue, bool createIfNotExist = false)
        {
            this._attributePart.SetTag(tagName, tagValue, createIfNotExist);
        }
    }
}
