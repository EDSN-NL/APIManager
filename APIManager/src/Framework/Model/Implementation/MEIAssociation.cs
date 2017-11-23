using System;
using System.Collections.Generic;
using Framework.Util;
using Framework.Exceptions;

namespace Framework.Model
{
    /// <summary>
    /// Model Element Implementation Class adds another layer of abstraction between the generic Model Element Implementation
    /// and the tool-specific implementation. This facilitates implementation of Model Element type-specific methods at this layer
    /// without the bridge interface needing tool-specific implementation logic.
    /// </summary>
    internal abstract class MEIAssociation : ModelElementImplementation
    {
        /// <summary>
        /// Retrieves the source endpoint descriptor of the association from the repository.
        /// </summary>
        internal EndpointDescriptor Source { get { return GetSourceDescriptor(); } }

        /// <summary>
        /// Retrieves the destination endpoint descriptor of the association from the repository.
        /// </summary>
        internal EndpointDescriptor Destination { get { return GetDestinationDescriptor(); } }

        /// <summary>
        /// Adds the specified stereotype to the destination end of the association.
        /// These are the override methods from the parent class that work on the DESTINATION end of the association
        /// only! The class provides additional methods in which the endpoint can be specified explicitly.
        /// </summary>
        /// <param name="stereotype">Stereotype to add.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal override void AddStereotype(string stereotype)
        {
            AddStereotype(stereotype, MEAssociation.AssociationEnd.Destination);
        }

        /// <summary>
        /// Retrieves annotation for the destination end of the association.
        /// These are the override methods from the parent class that work on the DESTINATION end of the association
        /// only! The class provides additional methods in which the endpoint can be specified explicitly.
        /// </summary>
        /// <returns>Annotation text or empty string if nothing there.</returns>
        internal override string GetAnnotation()
        {
            return GetAnnotation(MEAssociation.AssociationEnd.Destination);
        }

        /// <summary>
        /// Returns the type of the association (one of Aggregation, Association, Composition, Generalization, 
        /// MessageAssociation or Usage).
        /// </summary>
        /// <returns>Association type.</returns>
        internal abstract MEAssociation.AssociationType GetAssociationType();

        /// <summary>
        /// Retrieves documentation items from the destination end of the association.
        /// These are the override methods from the parent class that work on the DESTINATION end of the association
        /// only! The class provides additional methods in which the endpoint can be specified explicitly.
        /// </summary>
        /// <returns>List of documentation elements or NULL if nothing there.</returns>
        internal override List<MEDocumentation> GetDocumentation()
        {
            return GetDocumentation(MEAssociation.AssociationEnd.Destination);
        }

        /// <summary>
        /// Read the value of the specified tag from the destination end of the association.
        /// These are the override methods from the parent class that work on the DESTINATION end of the association
        /// only! The class provides additional methods in which the endpoint can be specified explicitly.
        /// </summary>
        /// <param name="tagName">Tag to be retrieved.</param>
        /// <returns>Tag value or empty string if not found.</returns>
        internal override string GetTag(string tagName)
        {
            return GetTag(tagName, MEAssociation.AssociationEnd.Destination);
        }

        /// <summary>
        /// Set the alias name of the connector destination end.
        /// These are the override methods from the parent class that work on the DESTINATION end of the association
        /// only! The class provides additional methods in which the endpoint can be specified explicitly.
        /// </summary>
        /// <param name="newName">Name to be assigned to destination.</param>
        internal override void SetAliasName(string newName)
        {
            SetAliasName(newName, MEAssociation.AssociationEnd.Destination);
        }

        /// <summary>
        /// Set the name of the connector destination end.
        /// These are the override methods from the parent class that work on the DESTINATION end of the association
        /// only! The class provides additional methods in which the endpoint can be specified explicitly.
        /// </summary>
        /// <param name="newName">Name to be assigned to destination.</param>
        internal override void SetName(string newName)
        {
            SetName(newName, MEAssociation.AssociationEnd.Destination);
        }

        /// <summary>
        /// Set the tag with specified name to the specified value at the destination end of the association. 
        /// If the tag could not be found, nothing will happen, unless the 'createIfNotExists' flag is set to true, in which case the tag will
        /// be created.
        /// These are the override methods from the parent class that work on the DESTINATION end of the association
        /// only! The class provides additional methods in which the endpoint can be specified explicitly.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="tagValue">Value of the tag.</param>
        /// <param name="createIfNotExists">When set to true, create the tag if it's not there.</param>
        internal override void SetTag(string tagName, string tagValue, bool createIfNotExists)
        {
            SetTag(tagName, tagValue, createIfNotExists, MEAssociation.AssociationEnd.Destination);
        }

        /// <summary>
        /// Checks whether the association destination endpoint contains one of the specified stereotypes.
        /// These are the override methods from the parent class that work on the DESTINATION end of the association
        /// only! The class provides additional methods in which the endpoint can be specified explicitly.
        /// </summary>
        /// <param name="stereotypes">List of stereotypes to check.</param>
        /// <returns>True if at least one stereotype exists for the Destination endpoint.</returns>
        internal override bool HasStereotype(List<string> stereotypes)
        {
            return HasStereotype(stereotypes, MEAssociation.AssociationEnd.Destination);
        }

        /// <summary>
        /// Checks whether the association destination endpoint contains the specified stereotype.
        /// These are the override methods from the parent class that work on the DESTINATION end of the association
        /// only! The class provides additional methods in which the endpoint can be specified explicitly.
        /// </summary>
        /// <param name="stereotype">Stereotype to check.</param>
        /// <returns>True if at least one stereotype exists for the Destination endpoint.</returns>
        internal override bool HasStereotype(string stereotype)
        {
            return HasStereotype(stereotype, MEAssociation.AssociationEnd.Destination);
        }

        /// <summary>
        /// Writes annotation for the destination end of the association.
        /// These are the override methods from the parent class that work on the DESTINATION end of the association
        /// only! The class provides additional methods in which the endpoint can be specified explicitly.
        /// </summary>
        /// <returns>Annotation text or empty string if nothing there.</returns>
        internal override void SetAnnotation(string text)
        {
            SetAnnotation(text, MEAssociation.AssociationEnd.Destination);
        }

        /// <summary>
        /// Returns an integer representation of a 'Cardinality' string. In theory, this string can contain litrally anything. In our particular case, we support:
        /// - Single value 'n' is translated to 'exactly n', i.e. minOcc = maxOcc = 'n'. Unless 'n' == 0, in which case minOcc = 0, maxOcc = 1;
        /// - Single value '*' is translated to '0 to unbounded', represented by minOcc = maxOcc = 0;
        /// - Range 'n..m' is translated to minOcc = 'n', maxOcc = 'm'. Unless 'm' = 0, in which case maxOcc = 1. If this leads to minOcc > maxOcc, both values will be swapped!
        /// - Range 'n..*' is translated to minOcc = 'n', maxOcc = 0 (maxOcc == 0 is lateron interpreted as 'unbounded').
        /// All other formates will result in a response values of minOcc = maxOcc = -1.
        /// </summary>
        /// <param name="endpoint">The endpoint to be evaluated.</param>
        /// <returns>Tuple consisting of minOCC, maxOcc. In case of errors, both will be -1.</returns>
        internal abstract Tuple<int, int> GetCardinality(MEAssociation.AssociationEnd endpoint);

        // These are association-specific operations on tags and stereotype, in which the association end can be 
        // explicitly specified...
        internal abstract void AddStereotype(string stereotype, MEAssociation.AssociationEnd endpoint);
        internal abstract void DeleteStereotype(string stereotype, MEAssociation.AssociationEnd endpoint);
        internal abstract string GetAnnotation(MEAssociation.AssociationEnd endpoint);
        internal abstract void SetAnnotation(string text, MEAssociation.AssociationEnd endpoint);
        internal abstract List<MEDocumentation> GetDocumentation(MEAssociation.AssociationEnd endpoint);
        internal abstract string GetTag(string tagName, MEAssociation.AssociationEnd endpoint);
        internal abstract void SetTag(string tagName, string tagValue, bool createIfNotExists, MEAssociation.AssociationEnd endpoint);
        internal abstract bool HasStereotype(List<string> stereotypes, MEAssociation.AssociationEnd endpoint);
        internal abstract bool HasStereotype(string stereotype, MEAssociation.AssociationEnd endpoint);
        internal abstract void SetAliasName(string newName, MEAssociation.AssociationEnd endpoint);
        internal abstract void SetName(string newName, MEAssociation.AssociationEnd endpoint);

        // Perform actual retrieval operations, which are repository specific.
        protected abstract EndpointDescriptor GetSourceDescriptor();
        protected abstract EndpointDescriptor GetDestinationDescriptor();

        /// <summary>
        /// Default constructor, mainly used to pass the model instance to the base constructor and set the correct type.
        /// </summary>
        /// <param name="model">Reference to the associated model implementation object.</param>
        protected MEIAssociation(ModelImplementation model) : base(model)
        {
            this._type = ModelElementType.Association;
        }
    }
}