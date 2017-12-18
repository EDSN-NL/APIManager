using System;
using System.Collections.Generic;
using System.Linq;
using EA;
using Framework.Model;

namespace SparxEA.Model
{
    /// <summary>
    /// Represents a 'union' data type within Sparx EA. Data types are represented by components called 'Element'.
    /// </summary>
    internal sealed class EAMEIUnionType: MEIUnionType
    {
        /// <summary>
        /// Since Union is a specialization of Class, also in EA, we can re-use most of the class methods and
        /// thus can avoid a lot of duplication by keeping the class portion in a separate element (solution for
        /// lack of multiple inheritance).
        /// </summary>
        private EAMEIClass _classPart;

        /// <summary>
        /// The internal constructor is called to initialize the repository.
        /// </summary>
        internal EAMEIUnionType(EAModelImplementation model, int classID): base(model)
        {
            this._classPart = model.GetModelElementImplementation(ModelElementType.Class, classID) as EAMEIClass;
            this._name = this._classPart.Name;
            this._elementID = this._classPart.ElementID;
            this._globalID = this._classPart.GlobalID;
            this._aliasName = this._classPart.AliasName;
        }

        /// <summary>
        /// The internal constructor is called to initialize the repository.
        /// </summary>
        internal EAMEIUnionType(EAModelImplementation model, string classGUID) : base(model)
        {
            this._classPart = model.GetModelElementImplementation(ModelElementType.Class, classGUID) as EAMEIClass;
            this._name = this._classPart.Name;
            this._elementID = this._classPart.ElementID;
            this._globalID = this._classPart.GlobalID;
            this._aliasName = this._classPart.AliasName;
        }

        /// <summary>
        /// Constructor that creates a new implementation instance based on a provided EA element instance.
        /// </summary>
        /// <param name="model">The associated model implementation.</param>
        /// <param name="element">The EA Element on which the implementation is based.</param>
        internal EAMEIUnionType(EAModelImplementation model, EA.Element element) : base(model)
        {
            this._classPart = model.GetModelElementImplementation(ModelElementType.Class, element.ElementID) as EAMEIClass;
            this._name = this._classPart.Name;
            this._elementID = this._classPart.ElementID;
            this._globalID = this._classPart.GlobalID;
            this._aliasName = this._classPart.AliasName;
        }

        /// <summary>
        /// Assignes the specified stereotype to this data type.
        /// </summary>
        /// <param name="stereotype">Stereotype to be assigned.</param>
        internal override void AddStereotype(string stereotype)
        {
            this._classPart.AddStereotype(stereotype);
        }

        /// <summary>
        /// Enumerator method that returns the class associations in succession.
        /// </summary>
        /// <returns>Next association.</returns>
        internal override IEnumerable<MEAssociation> AssociationList()
        {
            return this._classPart.AssociationList();
        }

        /// <summary>
        /// Enumerator method that returns the class associations of the specified type in succession.
        /// </summary>
        /// <returns>Next association of specified type.</returns>
        internal override IEnumerable<MEAssociation> AssociationList(MEAssociation.AssociationType type)
        {
            return this._classPart.AssociationList(type);
        }

        /// <summary>
        /// Create a new association instance between this class and a specified target class. Note that in this case the 'source'
        /// endpoint descriptor is ONLY used to pass meta-data regarding the association. The MEClass part is IGNORED!
        /// </summary>
        /// <param name="source">Owner-side of the association (start), class-part is ignored!.</param>
        /// <param name="destination">Destination of the association (end).</param>
        /// <param name="type">The type of association (aggregation, specialization, composition, etc.).</param>
        /// <param name="assocName">Name of the association, could be an empty string.</param>
        /// <returns>Newly created association or NULL in case of errors.</returns>
        internal override MEAssociation CreateAssociation(EndpointDescriptor source, EndpointDescriptor destination, MEAssociation.AssociationType type, string assocName)
        {
            return this._classPart.CreateAssociation(source, destination, type, assocName);
        }

        /// <summary>
        /// Creates a new attribute in the current class.
        /// </summary>
        /// <param name="name">Name of the attribute.</param>
        /// <param name="classifier">Attribute classifier (type of the attribute).</param>
        /// <param name="type">The type of attribute to create (regular, supplementary or facet)</param>
        /// <param name="defaultValue">An optional default value to be assignd to the attribute.</param>
        /// <param name="cardinality">Specifies lower and upper boundary of cardinality.</param>
        /// <param name="isConstant">Indicates that the attribute has a constant value. Default value must be specified in this case!</param>
        /// <returns>Newly created attribute object or null in case of errors.</returns>
        internal override MEAttribute CreateAttribute(string name, MEDataType classifier, AttributeType type, string defaultValue, Tuple<int, int> cardinality, bool isConstant)
        {
            return this._classPart.CreateAttribute(name, classifier, type, defaultValue, cardinality, isConstant);
        }

        /// <summary>
        /// Deletes the specified association from the current union. Fails silently (with warning to logging) in case the
        /// association could not be found.
        /// </summary>
        /// <param name="association">Association to delete.</param>
        internal override void DeleteAssociation(MEAssociation association)
        {
            this._classPart.DeleteAssociation(association);
        }

        /// <summary>
        /// Deletes the specified attribute from the current union. Fails silently (with warning to logging) in case the
        /// attribute could not be found.
        /// </summary>
        /// <param name="attribute">Attribute to delete.</param>
        internal override void DeleteAttribute(MEAttribute attribute)
        {
            this._classPart.DeleteAttribute(attribute);
        }

        /// <summary>
        /// Searches the union for an attribute with specified name and type.
        /// </summary>
        /// <param name="name">Name of the attribute to find.</param>
        /// <param name="type">Attribute type (Attribute, Supplementary or Facet).</param>
        /// <returns>Attribute or NULL if nothing found.</returns>
        internal override MEAttribute FindAttribute(string name, AttributeType type)
        {
            return this._classPart.FindAttribute(name, type);
        }

        /// <summary>
        /// Searches all associations on the current class for a child class with specified name and stereotype and returns the
        /// association role that belongs to this child.
        /// Note that the function only searches for the PRIMARY stereotype and ignores any generealized stereotypes!
        /// </summary>
        /// <param name="childName">Name of child class to locate.</param>
        /// <param name="childStereotype">Child primary stereotype.</param>
        /// <returns>Role name or empty string if not found.</returns>
        internal override string FindAssociatedClassRole(string name, string stereotype)
        {
            return this._classPart.FindAssociatedClassRole(name, stereotype);
        }

        /// <summary>
        /// Composite classes are associated with a diagram that shows the class composition. This method facilitates retrieval of 
        /// an associated diagram. The function returns NULL in case no diagram is associated.
        /// </summary>
        /// <returns>Associated diagram or NULL if no such association exists.</returns>
        internal override Framework.View.Diagram GetAssociatedDiagram()
        {
            return this._classPart.GetAssociatedDiagram();
        }

        /// <summary>
        /// Composite classes are associated with a diagram that shows the class composition. This method facilitates creation of 
        /// an association between the class and a diagram. A Class can only have at most a single association, so repeated invocations
        /// will overwrite the association.
        /// </summary>
        /// <param name="diagram">Diagram to associate with the class.</param>
        internal override void SetAssociatedDiagram(Framework.View.Diagram diagram)
        {
            this._classPart.SetAssociatedDiagram(diagram);
        }

        /// <summary>
        /// Returns a list of all attributes of the type. It ONLY returns the attributes for the current types, NOT for any
        /// specialized parent types!
        /// </summary>
        /// <returns>List of all attributes for the current type (can be empty if no attributes are defined).</returns>
        internal override List<MEAttribute> GetAttributes()
        {
            return this._classPart.GetAttributes();
        }

        /// <summary>
        /// Retrieves the current value of the build number (which is represented by the "Phase" tag in the EA element).
        /// </summary>
        /// <returns>Buils number</returns>
        internal override int GetBuildNumber()
        {
            return this._classPart.GetBuildNumber();
        }

        /// <summary>
        /// Retrieves a record containing class meta data. This contains the name of the class (plus optional alias name), a list of class
        /// stereotypes and a list of all attributes. For each attribute, we specify the ID (sequence number in repository), the name,
        /// an optional alias name, the name of the classifier, the cardinality and type type. The latter is a single character that specifies
        /// the type of the attribute as: 'C' = Content, 'S' = Supplementary, 'F' = Facet, 'A' = Association or 'G' = Generalization (parent class).
        /// </summary>
        /// <returns>Meta data descriptor.</returns>
        internal override MEClassMetaData GetClassMetaData()
        {
            return this._classPart.GetClassMetaData();
        }

        /// <summary>
        /// Retrieves the complete inheritance tree of the union in a sorted list. The key in this list identifies
        /// the distance from the current class (0 = union itself, 1 = immediate parent, etc.).
        /// If the union has no root, the parentList will only contain the type itself on return (at key 0).
        /// The function correctly tracks multiple paths through the hierarchy, as long as the model is a directed graph with a
        /// single root. Behavior is undetermined in case of multiple root classes!
        /// </summary>
        /// <param name="topStereotype">If specified, the search ends at the first type that posesses this stereotype.</param>
        /// <returns>Sorted list that contains parent types + their 'distance' from this type.</returns>
        internal override SortedList<uint, MEClass> GetHierarchy(string topStereotype)
        {
            return this._classPart.GetHierarchy(topStereotype);
        }

        /// <summary>
        /// Returns the package that is 'owner' of this union, i.e. in which the union is declared.
        /// </summary>
        /// <returns>Owning package or NULL on errors.</returns>
        internal override MEPackage GetOwningPackage()
        {
            return this._classPart.GetOwningPackage();
        }

        /// <summary>
        /// Retrieves the parent(s) of the current union (if any). Because we support multiple inheritance, there can be multiple 
        /// parents, hence the return list.
        /// </summary>
        /// <returns>List of parents, empty if no parent found.</returns>
        internal override List<MEClass> GetParents()
        {
            return this._classPart.GetParents();
        }

        /// <summary>
        /// Returns the major and minor version of the union.
        /// </summary>
        /// <returns>Major and minor version.</returns>
        internal override Tuple<int, int> GetVersion()
        {
            return this._classPart.GetVersion();
        }

        /// <summary>
        /// Returns element annotation (if present).
        /// </summary>
        /// <returns>Element annotation or empty string if nothing present.</returns>
        internal override string GetAnnotation()
        {
            return this._classPart.GetAnnotation();
        }

        /// <summary>
        /// Returns the author of the Union, i.e. the user who has last made changes to the Union.
        /// </summary>
        /// <returns>Union author.</returns>
        internal override string GetAuthor()
        {
            return this._classPart.GetAuthor();
        }

        /// <summary>
        /// Simple method that searches the union for the occurence of a tag with specified name. 
        /// If found, the value of the tag is returned.
        /// </summary>
        /// <param name="tagName">The name of the tag.</param>
        /// <returns>Value of specified tag or empty string if nothing could be found.</returns>
        internal override string GetTag(string tagName)
        {
            return this._classPart.GetTag(tagName);
        }

        /// <summary>
        /// This function returns true if the class contains at least one association of the specified type. If this type is 'Unknown', it checks
        /// any association that is NOT a Generalization.
        /// 'Trace' type associations are never checked!
        /// </summary>
        /// <param name="type">The type of association to check.</param>
        /// <returns>True if class has at lease one association of specified type.</returns>
        internal override bool HasAssociation(MEAssociation.AssociationType type)
        {
            return this._classPart.HasAssociation(type);
        }

        /// <summary>
        /// The method checks whether one or more stereotypes from the given list of stereotypes are owned by the union.
        /// </summary>
        /// <param name="stereotypes">List of stereotypes to check.</param>
        /// <returns>True if at least one match is found, false otherwise.</returns>
        internal override bool HasStereotype(List<string> stereotypes)
        {
            return this._classPart.HasStereotype(stereotypes);
        }

        /// <summary>
        /// This function checks whether the class contains one or more attributes and/or associations.
        /// </summary>
        /// <returns>True is class posesses one or more attributes and/or associations.</returns>
        internal override bool HasContents()
        {
            return this._classPart.HasContents();
        }

        /// <summary>
        /// The method checks whether the given stereotype is owned by the union.
        /// </summary>
        /// <param name="stereotype">Stereotype to check.</param>
        /// <returns>True if a match is found, false otherwise.</returns>
        internal override bool HasStereotype(string stereotype)
        {
            return this._classPart.HasStereotype(stereotype);
        }

        /// <summary>
        /// Helper function that enforces proper position numbering on each attribute of the current class. When parameter 'onlyCheck' is set
        /// to 'true', the function only checks the order, but does not change it. 
        /// In all cases, a return value of 'false' indicates that the order is wrong (but with 'onlyCheck = false', it will have been repaired.
        /// </summary>
        /// <param name="onlyCheck">Set to 'true' to enforce a check only.</param>
        /// <returns>Returns TRUE when the order in the class was Ok at moment of call or FALSE when attributes are/were out of order.</returns>
        internal override bool RepairAttributeOrder(bool onlyCheck)
        {
            return this._classPart.RepairAttributeOrder(onlyCheck);
        }

        /// <summary>
        /// Updates union annotation.
        /// </summary>
        /// <param name="text">The annotation text to write.</param>
        internal override void SetAnnotation(string text)
        {
            this._classPart.SetAnnotation(text);
        }

        /// <summary>
        /// Update the alias name of the Union.
        /// </summary>
        /// <param name="newAliasName">New name to be assigned.</param>
        internal override void SetAliasName(string newAliasName)
        {
            this._classPart.SetAliasName(newAliasName);
            this._aliasName = newAliasName;
        }

        /// <summary>
        /// Update the name of the Union.
        /// </summary>
        /// <param name="newName">New name to be assigned.</param>
        internal override void SetName(string newName)
        {
            this._classPart.SetName(newName);
            this._name = newName;
        }

        /// <summary>
        /// Set the build number (which is represented by the "Phase" tag in the EA element).
        /// </summary>
        /// <param name="buildNumber">New buildnumber value.</param>
        internal override void SetBuildNumber(int buildNumber)
        {
            this._classPart.SetBuildNumber(buildNumber);
        }

        /// <summary>
        /// Set the tag with specified name to the specified value for the current union. 
        /// If the tag could not be found, nothing will happen, unless the 'createIfNotExists' flag is set 
        /// to true, in which case the tag will be created.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="tagValue">Value of the tag.</param>
        /// <param name="createIfNotExist">When set to true, create the tag if it's not there.</param>
        internal override void SetTag(string tagName, string tagValue, bool createIfNotExist = false)
        {
            this._classPart.SetTag(tagName, tagValue, createIfNotExist);
        }

        /// <summary>
        /// Returns the major and minor version of the union.
        /// </summary>
        /// <returns>Major and minor version.</returns>
        internal override void SetVersion(Tuple<int, int> version)
        {
            this._classPart.SetVersion(version);
        }
    }
}
