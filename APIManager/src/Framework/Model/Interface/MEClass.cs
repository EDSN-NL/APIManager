using System;
using System.Collections.Generic;
using Framework.Exceptions;
using Framework.View;
using Framework.Util;

namespace Framework.Model
{
    /// <summary>
    /// Representation of an UML 'Class' artifact.
    /// </summary>
    internal class MEClass: ModelElement
    {
        /// <summary>
        /// Composite classes are associated with a diagram that shows the class composition. This property facilitates either assignment
        /// of an associated diagram or retrieval of existing associated diagram.
        /// </summary>
        internal Diagram AssociatedDiagram
        {
            get
            {
                if (this._imp != null) return ((MEIClass)this._imp).GetAssociatedDiagram();
                else throw new MissingImplementationException("MEIClass");
            }
            set
            {
                if (this._imp != null) ((MEIClass)this._imp).SetAssociatedDiagram(value);
                else throw new MissingImplementationException("MEIClass");
            }
        }

        /// <summary>
        /// Returns an enumerator that lists all associations that have the current class as a 'source', e.g. that
        /// 'depart' from the class.
        /// </summary>
        /// <returns>Zero to many associations associated with the class.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal IEnumerable<MEAssociation> AssociationList
        {
            get
            {
                if (this._imp == null) throw new MissingImplementationException("MEIClass");
                else
                {
                    foreach (MEAssociation association in ((MEIClass)this._imp).AssociationList())
                    {
                        yield return association;
                    }
                }
            }
        }

        /// <summary>
        /// Returns a list of all attributes of the class. It ONLY returns the attributes for the current class, NOT for any
        /// specialized (root) classes!
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal List<MEAttribute> Attributes
        {
            get
            {
                if (this._imp != null) return ((MEIClass)this._imp).GetAttributes();
                else throw new MissingImplementationException("MEIClass");
            }
        }

        /// <summary>
        /// Returns the author of the class, i.e. the repository user responsible for the last change to the class.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal string Author
        {
            get
            {
                if (this._imp != null) return ((MEIClass)this._imp).GetAuthor();
                else throw new MissingImplementationException("MEIClass");
            }
        }

        /// <summary>
        /// Getter and setter for the build number ("virtual property").
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal int BuildNumber
        {
            get
            {
                if (this._imp != null) return ((MEIClass)this._imp).GetBuildNumber();
                else throw new MissingImplementationException("MEIClass");
            }
            set
            {
                if (this._imp != null) ((MEIClass)this._imp).SetBuildNumber(value);
                else throw new MissingImplementationException("MEIClass");
            }
        }

        /// <summary>
        /// Returns a record with meta data of the class (information regarding the class and its attributes).
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal MEClassMetaData Metadata
        {
            get
            {
                if (this._imp != null) return ((MEIClass)this._imp).GetClassMetaData();
                else throw new MissingImplementationException("MEIClass");
            }
        }

        /// <summary>
        /// Returns the package in which this class lives, i.e. that is owner of the class.
        /// </summary>
        /// <returns>Owning package or NULL on errors.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal MEPackage OwningPackage
        {
            get
            {
                if (this._imp != null) return ((MEIClass)this._imp).GetOwningPackage();
                else throw new MissingImplementationException("MEIClass");
            }
        }

        /// <summary>
        /// Retrieves the parent(s) of the current class (if any). Because we support multiple inheritance, there can be multiple 
        /// parents, hence the return list.
        /// </summary>
        /// <returns>List of parents, empty if no parent found.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal List<MEClass> Parents
        {
            get
            {
                if (this._imp != null) return ((MEIClass)this._imp).GetParents();
                else throw new MissingImplementationException("MEIClass");
            }
        }

        /// <summary>
        /// Getter and setter for the major- and minor version of this class.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal Tuple<int, int> Version
        {
            get
            {
                if (this._imp != null) return ((MEIClass)this._imp).GetVersion();
                else throw new MissingImplementationException("MEIClass");
            }
            set
            {
                if (this._imp != null) ((MEIClass)this._imp).SetVersion(value);
                else throw new MissingImplementationException("MEIClass");
            }
        }

        /// <summary>
        /// Construct a new UML 'Class' artifact by associating MEClass with the appropriate implementation object.
        /// The actual registration and construction is all performed by the base class constructor.
        /// </summary>
        /// <param name="classID">Tool-specific instance identifier of the class artifact within the tool repository.</param>
        internal MEClass(int classID) : base(ModelElementType.Class, classID) { }

        /// <summary>
        /// Construct a new UML 'Class' artifact by associating MEClass with the appropriate implementation object.
        /// The actual registration and construction is all performed by the base class constructor.
        /// </summary>
        /// <param name="classGUID">Globally-unique instance identifier of the class artifact.</param>
        internal MEClass(string classGUID) : base(ModelElementType.Class, classGUID) { }

        /// <summary>
        /// Construct a new UML 'Class' artifact by associating MEClass with the given implementation object.
        /// The actual registration and construction is all performed by the base class constructor.
        /// </summary>
        /// <param name="imp">Class implementation object.</param>
        internal MEClass(MEIClass imp) : base(imp) { }

        /// <summary>
        /// Copy constructor creates a new Model Element class from another Model Element class.
        /// </summary>
        /// <param name="copy">Class to use as basis.</param>
        internal MEClass(MEClass copy) : base(copy) { }

        /// <summary>
        /// Default constructor creates an empty interface.
        /// </summary>
        internal MEClass() : base() { }

        /// <summary>
        /// Enumerator method that returns all ingress specialization associations of the current class.
        /// </summary>
        /// <returns>Next ingress specialization.</returns>
        internal IEnumerable<MEAssociation> ChildAssociationList()
        {
            if (this._imp != null) return ((MEIClass)this._imp).ChildAssociationList();
            else throw new MissingImplementationException("MEIClass");
        }

        /// <summary>
        /// Create a new association instance between this class and a specified target class. Note that in this case the 'source'
        /// endpoint descriptor is ONLY used to pass meta-data regarding the association. The MEClass part is IGNORED!
        /// </summary>
        /// <param name="source">Owner-side of the association (start), class-part is ignored!.</param>
        /// <param name="destination">Destination of the association (end).</param>
        /// <param name="type">The type of association (aggregation, specialization, composition, etc.).</param>
        /// <param name="assocName">Optional Name of the association, could be left out when not needed.</param>
        /// <returns>Newly created association or NULL in case of errors.</returns>
        internal MEAssociation CreateAssociation(EndpointDescriptor source, EndpointDescriptor destination, MEAssociation.AssociationType type, string assocName = null)
        {
            if (this._imp != null) return ((MEIClass)this._imp).CreateAssociation(source, destination, type, assocName);
            else throw new MissingImplementationException("MEIClass");
        }

        /// <summary>
        /// Creates a new attribute in the current class using an existing MEAttribute object.
        /// </summary>
        /// <param name="attrib">The attribute object to be used as basis.</param>
        /// <exception cref="ArgumentException">Illegal or missing attribute.</exception>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal void CreateAttribute(MEAttribute attrib)
        {
            if (this._imp != null) ((MEIClass)this._imp).CreateAttribute(attrib);
            else throw new MissingImplementationException("MEIClass");
        }

        /// <summary>
        /// Creates a new attribute in the current class.
        /// </summary>
        /// <param name="name">Name of the attribute.</param>
        /// <param name="classifier">Attribute classifier (type of the attribute).</param>
        /// <param name="type">The type of attribute to create (regular, supplementary or facet)</param>
        /// <param name="defaultValue">An optional default value to be assignd to the attribute.</param>
        /// <param name="cardinality">Specifies lower and upper boundaries of attribute cardinality.</param>
        /// <param name="isConstant">Indicates that the attribute has a constant value. Default value must be specified in this case!</param>
        /// <param name="annotation">Optional annotation text for the attribute.</param>
        /// <returns>Newly created attribute object.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal MEAttribute CreateAttribute(string name, MEDataType classifier, AttributeType type, string defaultValue, Cardinality cardinality, bool isConstant, string annotation = null)
        {
            if (this._imp != null) return ((MEIClass)this._imp).CreateAttribute(name, classifier, type, defaultValue, cardinality, isConstant, annotation);
            else throw new MissingImplementationException("MEIClass");
        }

        /// <summary>
        /// Delete the specified association from the class. If the association could not be found, the operation fails silently.
        /// </summary>
        /// <param name="thisOne">The association to be deleted.</param>
        internal void DeleteAssociation(MEAssociation thisOne)
        {
            if (this._imp != null)((MEIClass)this._imp).DeleteAssociation(thisOne);
            else throw new MissingImplementationException("MEIClass");
        }

        /// <summary>
        /// Deletes the specified attribute from the current class. Fails silently (with warning to logging) in case the
        /// attribute could not be found.
        /// </summary>
        /// <param name="attribute">Attribute to delete.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal void DeleteAttribute(MEAttribute attribute)
        {
            if (this._imp != null) ((MEIClass)this._imp).DeleteAttribute(attribute);
            else throw new MissingImplementationException("MEIClass");
        }

        /// <summary>
        /// Searches the class for an attribute with specified name and optional type.
        /// </summary>
        /// <param name="name">Name of the attribute to find.</param>
        /// <param name="type">Attribute type (Attribute, Supplementary or Facet). If not specified, we're looking for 'regular' attributes.</param>
        /// <returns>Attribute or NULL if nothing found.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal MEAttribute FindAttribute(string name, AttributeType type = AttributeType.Attribute)
        {
            if (this._imp != null) return ((MEIClass)this._imp).FindAttribute(name, type);
            else throw new MissingImplementationException("MEIClass");
        }
        /// <summary>
        /// Searches all associations on the current class for a child class with specified name and stereotype and returns the
        /// association role that belongs to this child.
        /// Note that the function only searches for the PRIMARY stereotype and ignores any generealized stereotypes!
        /// </summary>
        /// <param name="childName">Name of child class to locate.</param>
        /// <param name="childStereotype">Child primary stereotype.</param>
        /// <returns>Role name or empty string if not found.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal string FindAssociatedClassRole(string childName, string childStereotype)
        {
            if (this._imp != null) return ((MEIClass)this._imp).FindAssociatedClassRole(childName, childStereotype);
            else throw new MissingImplementationException("MEIClass");
        }

        /// <summary>
        /// Searches all associations on the current class for any child class with specified name and stereotype and returns the
        /// list of matching classes.
        /// Note that the function only searches for the PRIMARY stereotype and ignores any generealized stereotypes!
        /// </summary>
        /// <param name="childName">Name of child class to locate.</param>
        /// <param name="childStereotype">Child primary stereotype.</param>
        /// <returns>List of matching classes or empty list when none found.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal List<MEClass> FindAssociatedClasses(string childName, string childStereotype)
        {
            if (this._imp != null) return ((MEIClass)this._imp).FindAssociatedClasses(childName, childStereotype);
            else throw new MissingImplementationException("MEIClass");
        }

        /// <summary>
        /// Returns a list of associations of the given type that have the current class as a 'source', e.g. that
        /// 'depart' from the class.
        /// </summary>
        /// <param name="type">The type of association to look for</param>
        /// <returns>Zero to many associations of the specified type that are associated with the class.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal IEnumerable<MEAssociation> TypedAssociations(MEAssociation.AssociationType type)
        {
            if (this._imp == null) throw new MissingImplementationException("MEIClass");
            else
            {
                var imp = this._imp as MEIClass;
                foreach (MEAssociation association in imp.AssociationList(type))
                {
                    yield return association;
                }
            }
        }

        /// <summary>
        /// Retrieves the complete inheritance tree of the current class in a sorted list. The key in this list identifies
        /// the distance from the current class (0 = class itself, 1 = immediate parent, etc.).
        /// If the specified element has no root, the parentList will only contain the element itself on return (at key 0).
        /// The function correctly tracks multiple paths through the hierarchy, as long as the model is a directed graph with a
        /// single root. Behavior is undetermined in case of multiple root classes!
        /// </summary>
        /// <param name="topStereotype">If specified, the search ends at the first class that posesses this stereotype.</param>
        /// <returns>Sorted list that contains parent classes + their 'distance' from this class.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal SortedList<uint, MEClass> GetHierarchy(string topStereotype = null)
        {
            if (this._imp != null) return ((MEIClass)this._imp).GetHierarchy(topStereotype);
            else throw new MissingImplementationException("MEIClass");
        }

        /// <summary>
        /// Helper function that enforces proper position numbering on each attribute of the current class. When parameter 'onlyCheck' is set
        /// to 'true', the function only checks the order, but does not change it. 
        /// In all cases, a return value of 'false' indicates that the order is wrong (but with 'onlyCheck = false', it will have been repaired.
        /// </summary>
        /// <param name="onlyCheck">Set to 'true' to enforce a check only.</param>
        /// <returns>Returns TRUE when the order in the class was Ok at moment of call or FALSE when attributes are/were out of order.</returns>
        internal bool RepairAttributeOrder(bool onlyCheck)
        {
            if (this._imp != null) return ((MEIClass)this._imp).RepairAttributeOrder(onlyCheck);
            else throw new MissingImplementationException("MEIClass");
        }

        /// <summary>
        /// This function returns true if the class contains at least one association of the specified type. If no type is specified, it checks
        /// any association that is NOT a Generalization.
        /// 'Trace' type associations are never checked!
        /// </summary>
        /// <param name="type">Optional type of association to check.</param>
        /// <returns>True if class has at lease one association of specified type.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal bool HasAssociation(MEAssociation.AssociationType type = MEAssociation.AssociationType.Unknown)
        {
            if (this._imp != null) return ((MEIClass)this._imp).HasAssociation(type);
            else throw new MissingImplementationException("MEIClass");
        }

        /// <summary>
        /// Searches the class (and all parent classes) for an attribute with specified name.
        /// </summary>
        /// <param name="name">Name of the attribute to find.</param>
        /// <returns>True when class (or one of the parent classes) contains an attribute with given name, false otherwise.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal bool HasAttribute(string name)
        {
            if (this._imp != null) return ((MEIClass)this._imp).HasAttribute(name);
            else throw new MissingImplementationException("MEIClass");
        }

        /// <summary>
        /// This function checks whether the class contains one or more attributes and/or associations.
        /// </summary>
        /// <returns>True is class posesses one or more attributes and/or associations.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal bool HasContents()
        {
            if (this._imp != null) return ((MEIClass)this._imp).HasContents();
            else throw new MissingImplementationException("MEIClass");
        }

        /// <summary>
        /// Special constructor to be used by specialized classes that have to pass alternative type identifiers.
        /// </summary>
        /// <param name="type">Type of derived class.</param>
        /// <param name="classId">Instance ID of associated repository entry.</param>
        protected MEClass(ModelElementType type, int classId) : base(type, classId) { }

        /// <summary>
        /// Special constructor to be used by specialized classes that have to pass alternative type identifiers.
        /// </summary>
        /// <param name="type">Type of derived class.</param>
        /// <param name="classGUID">Globally unique instance ID of associated type.</param>
        protected MEClass(ModelElementType type, string classGUID) : base(type, classGUID) { }
    }
}
