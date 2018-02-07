using System;
using System.Collections.Generic;
using Framework.View;

namespace Framework.Model
{
    /// <summary>
    /// Model Element Implementation Class adds another layer of abstraction between the generic Model Element Implementation
    /// and the tool-specific implementation. This facilitates implementation of Model Element type-specific methods at this layer
    /// without the bridge interface needing tool-specific implementation logic.
    /// </summary>
    internal abstract class MEIClass : ModelElementImplementation
    {
        /// <summary>
        /// Enumerator method that returns the class associations in succession.
        /// </summary>
        /// <returns>Next association.</returns>
        internal abstract IEnumerable<MEAssociation> AssociationList();

        /// <summary>
        /// Enumerator method that returns the class associations of the specified type in succession.
        /// </summary>
        /// <returns>Next association of specified type.</returns>
        internal abstract IEnumerable<MEAssociation> AssociationList(MEAssociation.AssociationType type);

        /// <summary>
        /// Create a new association instance between this class and a specified target class. Note that in this case the 'source'
        /// endpoint descriptor is ONLY used to pass meta-data regarding the association. The MEClass part is IGNORED!
        /// </summary>
        /// <param name="source">Owner-side of the association (start), class-part is ignored!.</param>
        /// <param name="destination">Destination of the association (end).</param>
        /// <param name="type">The type of association (aggregation, specialization, composition, etc.).</param>
        /// <param name="assocName">Name of the association, could be an empty string.</param>
        /// <returns>Newly created association or NULL in case of errors.</returns>
        internal abstract MEAssociation CreateAssociation(EndpointDescriptor source, EndpointDescriptor destination, MEAssociation.AssociationType type, string assocName);

        /// <summary>
        /// Creates a new attribute in the current class using an existing MEAttribute object.
        /// </summary>
        /// <param name="attrib">The attribute object to be used as basis.</param>
        /// <exception cref="ArgumentException">Illegal or missing attribute.</exception>
        internal abstract void CreateAttribute(MEAttribute attrib);

        /// <summary>
        /// Creates a new attribute in the current class.
        /// </summary>
        /// <param name="name">Name of the attribute.</param>
        /// <param name="classifier">Attribute classifier (type of the attribute).</param>
        /// <param name="type">The type of attribute to create (regular, supplementary or facet)</param>
        /// <param name="defaultValue">An optional default value to be assignd to the attribute.</param>
        /// <param name="cardinality">Specifies the lower and upper boundary of attribute cardinality.</param>
        /// <param name="isConstant">Indicates that the attribute has a constant value. Default value must be specified in this case!</param>
        /// <returns>Newly created attribute object.</returns>
        internal abstract MEAttribute CreateAttribute(string name, MEDataType classifier, AttributeType type, string defaultValue, Tuple<int, int> cardinality, bool isConstant);

        /// <summary>
        /// Delete the specified association from the class. If the association could not be found, the operation fails silently.
        /// </summary>
        /// <param name="thisOne">The association to be deleted.</param>
        internal abstract void DeleteAssociation(MEAssociation thisOne);

        /// <summary>
        /// Deletes the specified attribute from the current class. Fails silently (with warning to logging) in case the
        /// attribute could not be found.
        /// </summary>
        /// <param name="attribute">Attribute to delete.</param>
        internal abstract void DeleteAttribute(MEAttribute attribute);

        /// <summary>
        /// Searches the class for an attribute with specified name and type.
        /// </summary>
        /// <param name="name">Name of the attribute to find.</param>
        /// <param name="type">Attribute type (Attribute, Supplementary or Facet).</param>
        /// <returns>Attribute or NULL if nothing found.</returns>
        internal abstract MEAttribute FindAttribute(string name, AttributeType type);

        /// <summary>
        /// Searches all associations on the current class for a child class with specified name and stereotype and returns the
        /// association role that belongs to this child.
        /// Note that the function only searches for the PRIMARY stereotype and ignores any generealized stereotypes!
        /// </summary>
        /// <param name="childName">Name of child class to locate.</param>
        /// <param name="childStereotype">Child primary stereotype.</param>
        /// <returns>Role name or empty string if not found.</returns>
        internal abstract string FindAssociatedClassRole(string childName, string childStereotype);

        /// <summary>
        /// Composite classes are associated with a diagram that shows the class composition. This method facilitates retrieval of 
        /// an associated diagram. The function returns NULL in case no diagram is associated.
        /// </summary>
        /// <returns>Associated diagram or NULL if no such association exists.</returns>
        internal abstract Diagram GetAssociatedDiagram();

        /// <summary>
        /// Returns a list of all attributes of the class. It ONLY returns the attributes for the current class, NOT for any
        /// specialized (root) classes!
        /// </summary>
        /// <returns>List of all attributes for the current class.</returns>
        internal abstract List<MEAttribute> GetAttributes();

        /// <summary>
        /// Returns the author of the class, i.e. the repository user responsible for the last change to the class.
        /// </summary>
        /// <returns>Author name or empty string if nothing found.</returns>
        internal abstract string GetAuthor();

        /// <summary>
        /// Retrieves the current value of the build number (which is represented by the "Phase" tag in the EA element).
        /// </summary>
        /// <returns>Buils number</returns>
        internal abstract int GetBuildNumber();

        /// <summary>
        /// Retrieves the complete inheritance tree of the current class in a sorted list. The key in this list identifies
        /// the distance from the current class (0 = class itself, 1 = immediate parent, etc.).
        /// If the specified element has no root, the parentList will only contain the element itself on return (at key 0).
        /// The function correctly tracks multiple paths through the hierarchy, as long as the model is a directed graph with a
        /// single root. Behavior is undetermined in case of multiple root classes!
        /// </summary>
        /// <param name="topStereotype">If specified, the search ends at the first class that posesses this stereotype.</param>
        /// <returns>Sorted list that contains parent classes + their 'distance' from this class.</returns>
        internal abstract SortedList<uint, MEClass> GetHierarchy(string topStereotype);

        /// <summary>
        /// Retrieves a record containing class meta data.
        /// </summary>
        /// <returns>Meta data descriptor.</returns>
        internal abstract MEClassMetaData GetClassMetaData();

        /// <summary>
        /// Returns the package that is 'owner' of this class, i.e. in which the class is declared.
        /// </summary>
        /// <returns>Owning package or NULL on errors.</returns>
        internal abstract MEPackage GetOwningPackage();

        /// <summary>
        /// Retrieves the parent(s) of the current class (if any). Because we support multiple inheritance, there can be multiple 
        /// parents, hence the return list.
        /// </summary>
        /// <returns>List of parents, empty if no parent found.</returns>
        internal abstract List<MEClass> GetParents();

        /// <summary>
        /// Returns the major and minor version of the class.
        /// </summary>
        /// <returns>Major and minor version.</returns>
        internal abstract Tuple<int, int> GetVersion();

        /// <summary>
        /// This function returns true if the class contains at least one association of the specified type. If this type is 'Unknown', it checks
        /// any association that is NOT a Generalization.
        /// 'Trace' type associations are never checked!
        /// </summary>
        /// <param name="type">The type of association to check.</param>
        /// <returns>True if class has at lease one association of specified type.</returns>
        internal abstract bool HasAssociation(MEAssociation.AssociationType type);

        /// <summary>
        /// This function checks whether the class contains one or more attributes and/or associations.
        /// </summary>
        /// <returns>True is class posesses one or more attributes and/or associations.</returns>
        internal abstract bool HasContents();

        /// <summary>
        /// Helper function that enforces proper position numbering on each attribute of the current class. When parameter 'onlyCheck' is set
        /// to 'true', the function only checks the order, but does not change it. 
        /// In all cases, a return value of 'false' indicates that the order is wrong (but with 'onlyCheck = false', it will have been repaired.
        /// </summary>
        /// <param name="onlyCheck">Set to 'true' to enforce a check only.</param>
        /// <returns>Returns TRUE when the order in the class was Ok at moment of call or FALSE when attributes are/were out of order.</returns>
        internal abstract bool RepairAttributeOrder(bool onlyCheck);

        /// <summary>
        /// Composite classes are associated with a diagram that shows the class composition. This method facilitates creation of 
        /// an association between the class and a diagram. A Class can only have at most a single association, so repeated invocations
        /// will overwrite the association.
        /// </summary>
        /// <param name="diagram">Diagram to associate with the class.</param>
        internal abstract void SetAssociatedDiagram(Diagram diagram);

        /// <summary>
        /// Set the build number (which is represented by the "Phase" tag in the EA element).
        /// </summary>
        /// <param name="buildNumber">New buildnumber value.</param>
        internal abstract void SetBuildNumber(int buildNumber);

        /// <summary>
        /// Returns the major and minor version of the class.
        /// </summary>
        /// <returns>Major and minor version.</returns>
        internal abstract void SetVersion(Tuple<int, int> version);

        /// <summary>
        /// Default constructor, mainly used to pass the model instance to the base constructor and set the correct type.
        /// </summary>
        /// <param name="model">Reference to the associated model implementation object.</param>
        protected MEIClass (ModelImplementation model): base(model)
        {
            this._type = ModelElementType.Class;
        }
    }
}