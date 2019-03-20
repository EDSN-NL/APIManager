using System;
using System.Collections.Generic;
using Framework.Exceptions;
using Framework.Util;

namespace Framework.Model
{
    /// <summary>
    /// Helper class that describes a single endpoint of the association.
    /// </summary>
    internal class EndpointDescriptor
    {
        private MEClass _endPoint;      // The class associated with this endpoint.
        private string _cardinality;    // Endpoint cardinality ("1", "0", "0..*", etc.)
        private string _role;           // Role name of the endpoint.
        private string _aliasRole;      // Alias name for the role of the endpoint.
        private bool _isGeneralization; // Special case of association!
        private bool _isNavigable;        // Indicates whether this end is navigable from other end.

        // Getters for the attributes...
        internal MEClass EndPoint                     {get {return this._endPoint; }}
        internal string Cardinality                   {get {return this._cardinality; }}
        internal string Role                          {get {return this._role; }}
        internal string AliasRole                     {get {return this._aliasRole; }}
        internal bool IsGeneralization                {get {return this._isGeneralization; }}
        internal bool IsAssociation                   {get {return !this._isGeneralization; }}
        internal bool IsNavigable                     {get {return this._isNavigable; }}

        /// <summary>
        /// Constructor for the creation of a generalization endpoint. These have no properties, other then the
        /// class that is associated with the endpoint!
        /// </summary>
        /// <param name="endPoint">Class associated with endpoint.</param>
        internal EndpointDescriptor (MEClass endPoint)
        {
            this._endPoint = endPoint;
            this._cardinality = string.Empty;
            this._role = string.Empty;
            this._aliasRole = string.Empty;
            this._isGeneralization = true;
            this._isNavigable = false;
        }

        /// <summary>
        /// Standard destructor that initialised all properties of the endpoint descriptor. The parameters for role, 
        /// aliasRole and tagList can all be optional (either NULL or empty strings).
        /// Cardinality is mandatory (can never be NULL). However, it is allowed to be empty.
        /// </summary>
        /// <param name="endPoint">Class associated with endpoint.</param>
        /// <param name="cardinality">Endpoint cardinality string.</param>
        /// <param name="role">Role name of the endpoint.</param>
        /// <param name="aliasRole">An optional alternative name for the endpoint role.</param>
        /// <param name="isNavigable">Set to true if this endpoint is navigable from other side.</param>
        internal EndpointDescriptor(MEClass endPoint, string cardinality, string role, string aliasRole, bool isNavigable)
        {
            this._endPoint = endPoint;
            this._cardinality = cardinality;
            this._role = (!string.IsNullOrEmpty(role))?  role: string.Empty;
            this._aliasRole = (!string.IsNullOrEmpty(aliasRole))? aliasRole: string.Empty;
            this._isGeneralization = false;
            this._isNavigable = isNavigable;
        }
    }

    /// <summary>
    /// Representation of a generic UML association between two classes.
    /// </summary>
    internal class MEAssociation: ModelElement
    {
        /// <summary>
        /// These are the various types of associations that are supported by the model.
        /// Aggregation = owner/part relationship with independent children, unidirectional;
        /// Association = generic bi-directional assocation;
        /// Composition = owner/part relationship with dependent children, unidirectional;
        /// Generalization = target is generalization of source;
        /// MessageAssociation = generic bi-directional association used in message models;
        /// Usage = target is 'used by' source.
        /// </summary>
        internal enum AssociationType { Aggregation, Association, Composition, Generalization, MessageAssociation, Usage, Unknown }

        // Used to identify what part of an association is affected by a given operation.
        // An association is always considered FROM source, TO destination.
        // Association = association 'as a whole';
        // Source = source-end;
        // Destination = destination-end.
        internal enum AssociationEnd { Association, Source, Destination }

        /// <summary>
        /// Returns the destination endpoint descriptor.
        /// </summary>
        /// <returns>Destination endpoint descriptor.</returns>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal EndpointDescriptor Destination
        {
            get
            {
                if (this._imp != null) return ((MEIAssociation)this._imp).Destination;
                else throw new MissingImplementationException("MEIAssociation");
            }
        }

        /// <summary>
        /// Returns the source endpoint descriptor.
        /// </summary>
        /// <returns>Source endpoint descriptor.</returns>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal EndpointDescriptor Source
        {
            get
            {
                if (this._imp != null) return ((MEIAssociation)this._imp).Source;
                else throw new MissingImplementationException("MEIAssociation");
            }
        }

        /// <summary>
        /// Returns the type of the association (one of Aggregation, Association, Composition, Generalization, 
        /// MessageAssociation or Usage).
        /// </summary>
        /// <returns>Association type.</returns>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal AssociationType TypeOfAssociation
        {
            get
            {
                if (this._imp != null) return ((MEIAssociation)this._imp).GetAssociationType();
                else throw new MissingImplementationException("MEIAssociation");
            }
        }

        /// <summary>
        /// Construct a new generic UML Association artifact that connects two MEClass instances.
        /// The actual registration and construction is all performed by the base class constructor.
        /// </summary>
        /// <param name="associationID">Repository identifier of the association.</param>
        internal MEAssociation(int associationID) : base(ModelElementType.Association, associationID) { }

        /// <summary>
        /// Construct a new generic UML Association artifact that connects two MEClass instances.
        /// The actual registration and construction is all performed by the base class constructor.
        /// </summary>
        /// <param name="associationGUID">Globally-unique identifier of the association.</param>
        internal MEAssociation(string associationGUID) : base(ModelElementType.Association, associationGUID) { }

        /// <summary>
        /// Construct a new generic UML Association artifact by associating with the given implementation object.
        /// The actual registration and construction is all performed by the base class constructor.
        /// </summary>
        /// <param name="imp">Class implementation object.</param>
        internal MEAssociation(MEIAssociation imp) : base(imp) { }

        /// <summary>
        /// Copy constructor creates a new Association Model Element from a given association instance.
        /// </summary>
        /// <param name="copy">Association to use as basis.</param>
        internal MEAssociation(MEAssociation copy) : base(copy) { }

        /// <summary>
        /// Default constructor creates an empty interface.
        /// </summary>
        internal MEAssociation() : base() { }

        /// <summary>
        /// Adds the specified stereotype to the specified endpoint of the association.
        /// </summary>
        /// <param name="stereotype">Stereotype to add.</param>
        /// <param name="endpoint">Which part of the association to add the stereotype to.</param>
        /// <returns>True if a match is found, false otherwise.</returns>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal void AddStereotype(string stereotype, AssociationEnd endpoint)
        {
            if (this._imp != null) ((MEIAssociation)this._imp).AddStereotype(stereotype, endpoint);
            else throw new MissingImplementationException("MEIAssociation");
        }

        /// <summary>
        /// Deletes the specified stereotype from the association or from a specific association end.
        /// </summary>
        /// <param name="stereotype">Stereotype to check.</param>
        /// <param name="endpoint">Which part of the association to inspect.</param>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal void DeleteStereotype(string stereotype, AssociationEnd endpoint)
        {
            if (this._imp != null) ((MEIAssociation)this._imp).DeleteStereotype(stereotype, endpoint);
            else throw new MissingImplementationException("MEIAssociation");
        }

        /// <summary>
        /// Returns annotation text from the specified endpoint.
        /// </summary>
        /// <param name="endPoint">Which part of the association to check.</param>
        /// <returns>Annotation string or empty string if nothing present.</returns>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal string GetAnnotation(AssociationEnd endpoint)
        {
            if (this._imp != null) return ((MEIAssociation)this._imp).GetAnnotation(endpoint);
            else throw new MissingImplementationException("MEIAssociation");
        }

        /// <summary>
        /// Returns an integer representation of a 'Cardinality' string. In theory, this string can contain litrally anything. 
        /// In our particular case, we support:
        /// - Single value 'n' is translated to 'exactly n', i.e. minOcc = maxOcc = 'n'. Unless 'n' == 0, in which case minOcc = 0, maxOcc = 1;
        /// - Single value '*' is translated to '0 to unbounded', represented by minOcc = maxOcc = 0;
        /// - Range 'n..m' is translated to minOcc = 'n', maxOcc = 'm'. Unless 'm' = 0, in which case maxOcc = 1. If this leads to minOcc > maxOcc, both values will be swapped!
        /// - Range 'n..*' is translated to minOcc = 'n', maxOcc = 0 (maxOcc == 0 is lateron interpreted as 'unbounded').
        /// All other formates will result in a response values of minOcc = maxOcc = -1.
        /// </summary>
        /// <param name="endpoint">The endpoint to be evaluated.</param>
        /// <returns>Cardinality object containing minOCC, maxOcc. In case of errors, both will be -1.</returns>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal Cardinality GetCardinality(AssociationEnd endpoint)
        {
            if (this._imp != null) return ((MEIAssociation)this._imp).GetCardinality(endpoint);
            else throw new MissingImplementationException("MEIAssociation");
        }

        /// <summary>
        /// This method searches the specified part of the association for any tags (from the configured list of relevant 
        /// tags) that have contents. The method returns a list of MEDocumentation elements for each non-empty tag found,
        /// including the standard 'Note' tag.
        /// </summary>
        /// <param name="endpoint">Which part of the association to check.</param>
        /// <returns>List of documentation nodes or empty list on error or if no comments available.</returns>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal List<MEDocumentation> GetDocumentation(AssociationEnd endpoint)
        {
            if (this._imp != null) return ((MEIAssociation)this._imp).GetDocumentation(endpoint);
            else throw new MissingImplementationException("MEIAssociation");
        }

        /// <summary>
        /// Searches an association for the occurence of a tag with specified name. 
        /// If found, the value of the tag is returned.
        /// </summary>
        /// <param name="tagName">The name of the tag.</param>
        /// <param name="endpoint">Which part of the association to inspect.</param>
        /// <returns>Value of specified tag or empty string if nothing could be found.</returns>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal string GetTag(string tagName, AssociationEnd endpoint)
        {
            if (this._imp != null) return ((MEIAssociation)this._imp).GetTag(tagName, endpoint);
            else throw new MissingImplementationException("MEIAssociation");
        }

        /// <summary>
        /// The method checks whether one or more stereotypes from the given list of stereotypes are owned by the specified
        /// part of the association.
        /// Note that the method can only check for 'leaf' stereotypes (e.g. it will NOT find generalizations in the stereotype
        /// hierarchy). This is due to limitations in the EA API!
        /// Also note that this is a case sensitive compare!
        /// </summary>
        /// <param name="stereotypes">List of stereotypes to check.</param>
        /// <param name="endpoint">Which part of the association to inspect.</param>
        /// <returns>True if at least one match is found, false otherwise.</returns>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal bool HasStereotype(List<string> stereotypes, AssociationEnd endpoint)
        {
            if (this._imp != null) return ((MEIAssociation)this._imp).HasStereotype(stereotypes, endpoint);
            else throw new MissingImplementationException("MEIAssociation");
        }

        /// <summary>
        /// The method checks whether the given stereotype is owned by the indicated part of the association.
        /// Note that the method can only check for 'leaf' stereotypes (e.g. it will NOT find generalizations in the stereotype
        /// hierarchy). This is due to limitations in the EA API!
        /// Also note that this is a case sensitive compare!
        /// </summary>
        /// <param name="stereotype">Stereotype to check.</param>
        /// <param name="endpoint">Which part of the association to inspect.</param>
        /// <returns>True if a match is found, false otherwise.</returns>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal bool HasStereotype(string stereotype, AssociationEnd endpoint)
        {
            if (this._imp != null) return ((MEIAssociation)this._imp).HasStereotype(stereotype, endpoint);
            else throw new MissingImplementationException("MEIAssociation");
        }

        /// <summary>
        /// Either update the alias name of the association, or the role alias of the specified endpoint.
        /// </summary>
        /// <param name="newName">Alias name to be assigned to Connector or Endpoint.</param>
        /// <param name="endpoint">What to update.</param>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal void SetAliasName(string newName, MEAssociation.AssociationEnd endpoint)
        {
            if (this._imp != null) ((MEIAssociation)this._imp).SetAliasName(newName, endpoint);
            else throw new MissingImplementationException("MEIAssociation");
        }

        /// <summary>
        /// Set the cardinality of the specified endpoint. If item2 is 0, this is translated to "*". And if Item2 < Item1, the functions silently
        /// fails!
        /// </summary>
        /// <param name="card">Cardinality to set.</param>
        /// <param name="endpoint">The endpoint to be evaluated.</param>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal void SetCardinality(Cardinality card, MEAssociation.AssociationEnd endpoint)
        {
            if (this._imp != null) ((MEIAssociation)this._imp).SetCardinality(card, endpoint);
            else throw new MissingImplementationException("MEIAssociation");
        }

        /// <summary>
        /// Either update the name of the association, or the role of the specified endpoint.
        /// </summary>
        /// <param name="newName">Alias name to be assigned to Connector or Endpoint.</param>
        /// <param name="endpoint">What to update.</param>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal void SetName(string newName, MEAssociation.AssociationEnd endpoint)
        {
            if (this._imp != null) ((MEIAssociation)this._imp).SetName(newName, endpoint);
            else throw new MissingImplementationException("MEIAssociation");
        }

        /// <summary>
        /// Set the tag with specified name to the specified value. 
        /// If the tag could not be found, nothing will happen, unless the 'createIfNotExists' flag is set to true, 
        /// in which case the tag will be created.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="tagValue">Value of the tag.</param>
        /// <param name="createIfNotExist">When set to true, create the tag if it's not there.</param>
        /// <param name="endpoint">Which part of the association to inspect.</param>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal void SetTag(string tagName, string tagValue, bool createIfNotExist, AssociationEnd endpoint)
        {
            if (this._imp != null) ((MEIAssociation)this._imp).SetTag(tagName, tagValue, createIfNotExist, endpoint);
            else throw new MissingImplementationException("MEIAssociation");
        }
    }
}
