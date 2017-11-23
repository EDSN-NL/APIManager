﻿using System;
using System.Collections.Generic;
using System.Linq;
using EA;
using Framework.Exceptions;
using Framework.Logging;
using Framework.Model;

namespace SparxEA.Model
{
    /// <summary>
    /// Represents a 'data type' within Sparx EA. Data types are represented by components called 'Element'.
    /// </summary>
    internal sealed class EAMEIAttribute: MEIAttribute
    {
        private EA.Attribute _attribute;    // Repository attribute representation.

        /// <summary>
        /// Fetches attribute information from EA repository and initializes the implementation object.
        /// </summary>
        internal EAMEIAttribute(EAModelImplementation model, int attribID): base(model)
        {
            this._attribute = model.Repository.GetAttributeByID(attribID);
            if (this._attribute == null)
            {
                Logger.WriteError("SparxEA.Model.EAMEIAttribute >> Failed to retrieve EA Attribute with ID: " + attribID);
            }
            else
            {
                this._name = this._attribute.Name;
                this._elementID = attribID;
                this._globalID = this._attribute.AttributeGUID;
				this._aliasName = this._attribute.Alias ?? string.Empty;
            }
        }

        /// <summary>
        /// Constructor that creates a new implementation instance based on a provided EA attribute instance.
        /// </summary>
        /// <param name="model">The associated model implementation.</param>
        /// <param name="attribute">The EA Attribute on which the implementation is based.</param>
        internal EAMEIAttribute(EAModelImplementation model, EA.Attribute attribute) : base(model)
        {
            this._attribute = attribute;
            if (this._attribute == null)
            {
                Logger.WriteError("SparxEA.Model.EAMEIAttribute >>  Constructor based on empty element!");
            }
            else
            {
                this._name = this._attribute.Name;
                this._elementID = attribute.AttributeID;
                this._globalID = attribute.AttributeGUID;
				this._aliasName = attribute.Alias ?? string.Empty;
            }
        }

        /// <summary>
        /// Assignes the specified stereotype to this attribute.
        /// </summary>
        /// <param name="stereotype">Stereotype to be assigned.</param>
        internal override void AddStereotype(string stereotype)
        {
            if (!string.IsNullOrEmpty(stereotype))
            {
                string stereoTypes = this._attribute.StereotypeEx;
                if (this._attribute.Stereotype != string.Empty && stereoTypes == string.Empty)
                {
                    // Special condition in case of enumerations, which use some sort of 'fake' stereotype.
                    // We have to copy the 'main' stereotype to the EX field, otherwise we loose it on the update.
                    stereoTypes = this._attribute.Stereotype;
                }

                string checkType = (stereotype.Contains("::")) ? stereotype.Substring(stereotype.IndexOf("::") + 2) : stereotype;
                if (!stereoTypes.Contains(checkType))
                {
                    stereoTypes += (stereoTypes.Length > 0) ? "," + stereotype : stereotype;
                    this._attribute.StereotypeEx = stereoTypes;
                    this._attribute.Update();
                }
            }
        }

        /// <summary>
        /// Returns element annotation (if present).
        /// </summary>
        /// <returns>Element annotation or empty string if nothing present.</returns>
        internal override string GetAnnotation()
        {
            string notes = string.Empty;
            // Due to a bug (or 'feature') in EA, Notes that are updated within the same program, but using different references
            // to the same repository element, do not show up in other references. So, to be sure, we reload the attribute instance
            // before retrieving our notes...
            this._attribute = ((EAModelImplementation)this._model).Repository.GetAttributeByID(this._attribute.AttributeID);
            if (!string.IsNullOrEmpty(this._attribute.Notes)) notes = this._attribute.Notes;
            return notes;
        }

        /// <summary>
        /// Returns an integer representation of a 'Cardinality' string that should be interpreted as follows:
        /// - Lower bound MUST be numeric. This is copied verbatim, all other values are rejected. A value smaller then 0 is interpreted as 0.
        /// - Upper bound MUST be >= 1 or either "*" or "n".  "*"/"n" is translated to 0, which is lateron interpreted as 'unbounded'.
        /// If, after conversion, upper bound is smaller then lower bound, both values are swapped!
        /// All other formates will result in a response values of minOcc = maxOcc = -1.
        /// </summary>
        /// <returns>Tuple consisting of minOCC, maxOcc. In case of errors, both will be -1.</returns>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal override Tuple<int, int> GetCardinality()
        {
            int minOcc, maxOcc;

            try
            {
                minOcc = Convert.ToInt16(this._attribute.LowerBound);
                if (minOcc < 0) minOcc = 0;

                string upperBound = this._attribute.UpperBound;
                if (upperBound.Contains("*") || upperBound.Contains("n") || upperBound.Contains("N")) maxOcc = 0;
                else maxOcc = Convert.ToInt16(upperBound);

                // If maxOcc < minOcc (and not 'unbounded'), we return an error as well.
                if ((maxOcc > 0) && (maxOcc < minOcc)) minOcc = maxOcc = -1;
                return new Tuple<int, int>(minOcc, maxOcc);
            }
            catch (FormatException exc)
            {
                Logger.WriteError("SparxEA.Model.EAMEIAttribute.GetCardinality >> Unsupported format in cardinality field of attribute: '" + 
                                  this._attribute.Name + "'!" + Environment.NewLine + exc.Message);
            }
            return new Tuple<int, int>(-1, -1);
        }

        /// <summary>
        /// Returns the classifier of this attribute (the 'type' of the attribute).
        /// This can either be a 'regular' Data Type, an Enumeration or a Union. In all other cases, the method will
        /// return NULL!
        /// </summary>
        /// <returns>Attribute classifier as a data type instance or NULL on errors.</returns>
        internal override MEDataType GetClassifier()
        {
            MEDataType classifierType = null;
            int classifierID = this._attribute.ClassifierID;
            if (classifierID > 0) classifierType = ModelSlt.GetModelSlt().GetDataType(classifierID);
            return classifierType;
        }

        /// <summary>
        /// Returns the default value of the attribute (if defined).
        /// A default attribute value is defined as valid if:
        /// The 'Default' property has a value AND
        /// The 'IsConst' indicator is set to FALSE.
        /// </summary>
        /// <returns>Default value or empty string if no such value exists (or attribute is constant).</returns>
        internal override string GetDefaultValue()
        {
            return !(this._attribute.IsConst) ? this._attribute.Default : string.Empty;
        }

        /// <summary>
        /// Returns the fixed (constant) value of the attribute (if defined).
        /// A fixed attribute value is defined as valid if:
        /// The 'Default' property has a value AND
        /// The 'IsConst' indicator is set to TRUE.
        /// </summary>
        /// <returns>Default value or empty string if no such value exists (or attribute is not constant).</returns>
        internal override string GetFixedValue()
        {
            return (this._attribute.IsConst) ? this._attribute.Default : string.Empty;
        }

        /// <summary>
        /// Returns the class that is 'owner' of the attribute, i.e. in which the attribute is declared.
        /// </summary>
        /// <returns>Owning class or NULL on errors.</returns>
        internal override MEClass GetOwningClass()
        {
            return new MEClass(this._attribute.ParentID);
        }

        /// <summary>
        /// Returns the position of the attribute, relative to other attributes within the 'owning' class. The key is incremental
        /// in steps of 100 (e.g. the first attribute has key 100, the second 200, etc.). 
        /// If the class is un-ordered, the key is always 100.
        /// </summary>
        /// <returns>Sequence key of attribute, starting at 100 or 0 if unordered.</returns>
        internal override int GetSequenceKey()
        {
            int pos = (this._attribute.Pos + 1) * 100;
            Logger.WriteInfo("SparxEA.Model.EAMEIAttribute.GetSequenceKey >> Found key: '" + pos + "'.");
            return pos;
        }

        /// <summary>
        /// Simple method that searches the attribute implementation for the occurence of a tag with specified name. 
        /// If found, the value of the tag is returned.
        /// </summary>
        /// <param name="tagName">The name of the tag.</param>
        /// <returns>Value of specified tag or empty string if nothing could be found.</returns>
        internal override string GetTag(string tagName)
        {
            try
            {
                foreach (AttributeTag t in this._attribute.TaggedValues)
                { 
                    if (String.Compare(t.Name, tagName, StringComparison.OrdinalIgnoreCase) == 0) return t.Value;
                }
            }
            catch { /* & ignore all errors */ }
            return string.Empty;
        }

        /// <summary>
        /// The method checks whether one or more stereotypes from the given list of stereotypes are owned by the Attribute.
        /// Since attributes only support 'non-qualified' stereotype names, we remove any possible profile prefix.
        /// </summary>
        /// <param name="stereotypes">List of stereotypes to check.</param>
        /// <returns>True if at least one match is found, false otherwise.</returns>
        internal override bool HasStereotype(List<string> stereotypes)
        {
            foreach (string st in stereotypes)
            {
                string normalizedType = (st.Contains("::")) ? st.Substring(st.IndexOf("::") + 2) : st;
                if (this._attribute.StereotypeEx.Contains(normalizedType)) return true;
            }
            return false;
        }

        /// <summary>
        /// The method checks whether the given stereotype is owned by the Attribute.
        /// Since attributes only support 'non-qualified' stereotype names, we remove any possible profile prefix.
        /// </summary>
        /// <param name="stereotype">Stereotype to check.</param>
        /// <returns>True if a match is found, false otherwise.</returns>
        internal override bool HasStereotype(string stereotype)
        {
            string normalizedType = (stereotype.Contains("::")) ? stereotype.Substring(stereotype.IndexOf("::") + 2) : stereotype;
            return this._attribute.StereotypeEx.Contains(normalizedType);
        }

        /// <summary>
        /// Returns an indication whether the attribute is optional within the owning class. This implies that the
        /// 'lower bound' of the attribute cardinality must be '0'.
        /// </summary>
        /// <returns>True is this is an optional attribute, false otherwise.</returns>
        internal override bool IsOptional()
        {
            return (this._attribute.LowerBound == "0") ? true : false;
        }

        /// <summary>
        /// Updates attribute annotation.
        /// </summary>
        /// <param name="text">The annotation text to write.</param>
        internal override void SetAnnotation(string text)
        {
            this._attribute.Notes = text;
            this._attribute.Update();
        }

        /// <summary>
        /// Update the alias name of the attribute.
        /// </summary>
        /// <param name="newAliasName">New name to be assigned.</param>
        internal override void SetAliasName(string newAliasName)
        {
            this._attribute.Alias = newAliasName;
            this._aliasName = newAliasName;
            this._attribute.Update();
        }

        /// <summary>
        /// Update the name of the attribute.
        /// </summary>
        /// <param name="newName">New name to be assigned.</param>
        internal override void SetName (string newName)
        {
            this._attribute.Name = newName;
            this._name = newName;
            this._attribute.Update();
        }

        /// <summary>
        /// Writes the default value of the attribute (if defined).
        /// A default attribute value is defined as valid if the 'IsConst' indicator is set to FALSE.
        /// If the attribute is not defined as a default attribute, the update is ignored!
        /// </summary>
        /// <param name="value">The new default value.</param>
        internal override void SetDefaultValue(string value)
        {
            if (!this._attribute.IsConst)
            {
                this._attribute.Default = value;
                this._attribute.Update();
            }
        }

        /// <summary>
        /// Writes the fixed value of the attribute (if defined).
        /// A fixed attribute value is defined as valid if the 'IsConst' indicator is set to TRUE.
        /// If the attribute is not defined as a fixed attribute, the update is ignored!
        /// </summary>
        /// <param name="value">The new default value.</param>
        internal override void SetFixedValue(string value)
        {
            if (this._attribute.IsConst)
            {
                this._attribute.Default = value;
                this._attribute.Update();
            }
        }

        /// <summary>
        /// Set the tag with specified name to the specified value for the current class. 
        /// If the tag could not be found, nothing will happen, unless the 'createIfNotExists' flag is set to true, in which case the tag will
        /// be created.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="tagValue">Value of the tag.</param>
        /// <param name="createIfNotExist">When set to true, create the tag if it's not there.</param>
        internal override void SetTag(string tagName, string tagValue, bool createIfNotExist = false)
        {
            try
            {
                foreach (AttributeTag t in this._attribute.TaggedValues)
                {
                    if (String.Compare(t.Name, tagName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        t.Value = tagValue;
                        t.Update();
                        return;
                    }
                }

                // Tag not found, create new one if instructed to do so...
                if (createIfNotExist)
                {
                    var newTag = this._attribute.TaggedValues.AddNew(tagName, "TaggedValue") as AttributeTag;
                    newTag.Value = tagValue;
                    newTag.Update();
                    this._attribute.TaggedValues.Refresh();
                }
            }
            catch { /* & ignore all errors */ }
        }
    }
}
