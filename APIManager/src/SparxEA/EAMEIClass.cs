using System;
using System.Collections.Generic;
using System.Xml;
using EA;
using Framework.Context;
using Framework.Logging;
using Framework.Model;

namespace SparxEA.Model
{
    /// <summary>
    /// Represents a 'class' within Sparx EA. Classes are represented by components called 'Element'.
    /// </summary>
    internal sealed class EAMEIClass : MEIClass
    {
        private EA.Element _element;                        // EA Class representation.

        // Configuration properties used to check attribute types...
        private const string _SupplementaryAttStereotype    = "SupplementaryAttStereotype";
        private const string _FacetAttStereotype            = "FacetAttStereotype";
        private const string _ContentAttStereotype          = "ContentAttStereotype";

        // Some of the default stereotypes required when manipulating associations...
        private const string _GeneralizationStereotype      = "GeneralizationStereotype";
        private const string _AssociationStereotype         = "AssociationStereotype";
        private const string _MessageAssociationStereotype  = "MessageAssociationStereotype";

        // Other configuration properties...
        private const string _SequenceKeyTag                = "SequenceKeyTag";
        private const string _TraceAssociationStereotype    = "TraceAssociationStereotype";

        /// <summary>
        /// Getter that returns the attribute list of the class as a repository-specific collection.
        /// It is meant to be used by other EA implementation methods that require access to the
        /// attribute data, without a need to directly expose the EA object.
        /// </summary>
        internal EA.Collection AttributeList { get { return this._element.Attributes; } }

        /// <summary>
        /// Getter that returns the meta type of the class as a repository-specific string.
        /// It is meant to be used by other EA implementation methods that require access to the
        /// meta type, without a need to directly expose the EA object.
        /// </summary>
        internal string MetaType { get { return this._element.MetaType; } }

        /// <summary>
        /// Getter that returns the run-time state of the element as a repository-specific string.
        /// It is meant to be used by other EA implementation methods that require access to the
        /// meta type, without a need to directly expose the EA object.
        /// </summary>
        internal string RunState { get { return this._element.RunState; } }

        /// <summary>
        /// Creates a new EA Class implementation based on repository ID.
        /// </summary>
        internal EAMEIClass(EAModelImplementation model, int classID) : base(model)
        {
            this._element = model.Repository.GetElementByID(classID);
            if (this._element != null)
            {
                this._name = this._element.Name;
                this._elementID = classID;
                this._globalID = this._element.ElementGUID;
				this._aliasName = this._element.Alias ?? string.Empty;
            }
            else
            {
                Logger.WriteError("SparxEA.Model.EAMEIClass >> Failed to retrieve EA Element with ID: " + classID);
            }
        }

        /// <summary>
        /// Creates a new EA Class implementation based on repository GUID.
        /// </summary>
        internal EAMEIClass(EAModelImplementation model, string classGUID) : base(model)
        {
            this._element = model.Repository.GetElementByGuid(classGUID);
            if (this._element != null)
            {
                this._name = this._element.Name;
                this._elementID = this._element.ElementID;
                this._globalID = classGUID;
                this._aliasName = this._element.Alias ?? string.Empty;
            }
            else
            {
                Logger.WriteError("SparxEA.Model.EAMEIClass >> Failed to retrieve EA Element with GUID: " + classGUID);
            }
        }

        /// <summary>
        /// Constructor that creates a new implementation instance based on a provided EA element instance.
        /// </summary>
        /// <param name="model">The associated model implementation.</param>
        /// <param name="element">The EA Element on which the implementation is based.</param>
        internal EAMEIClass(EAModelImplementation model, EA.Element element) : base(model)
        {
            this._element = element;
            if (this._element != null)
            {
                this._name = this._element.Name;
                this._elementID = element.ElementID;
                this._globalID = this._element.ElementGUID;
				this._aliasName = this._element.Alias ?? string.Empty;
            }
            else
            {
                Logger.WriteError("SparxEA.Model.EAMEIClass >>  Constructor based on empty element!");
            }
        }

        /// <summary>
        /// Assignes the specified stereotype to this class.
        /// </summary>
        /// <param name="stereotype">Stereotype to be assigned.</param>
        internal override void AddStereotype(string stereotype)
        {
            if (!string.IsNullOrEmpty(stereotype))
            {
                string stereoTypes = this._element.StereotypeEx;

                if (!HasStereotype(stereotype))
                {
                    stereoTypes += (stereoTypes.Length > 0) ? "," + stereotype : stereotype;
                    this._element.StereotypeEx = stereoTypes;
                    this._element.Update();
                    ((EAModelImplementation)this._model).Repository.AdviseElementChange(this._element.ElementID);
                }
            }
        }

        /// <summary>
        /// Enumerator method that returns the class associations in succession.
        /// </summary>
        /// <returns>Next association.</returns>
        internal override IEnumerable<MEAssociation> AssociationList()
        {
            this._element.Connectors.Refresh();
            string traceStereotype = ContextSlt.GetContextSlt().GetConfigProperty(_TraceAssociationStereotype);

            for (short i = 0; i < this._element.Connectors.Count; i++)
            {
                var connector = this._element.Connectors.GetAt(i) as EA.Connector;
                // Make sure to return only connectors that originate from the current class and are NOT 'trace' associations...
                if (connector.ClientID == this._elementID && !connector.StereotypeEx.Contains(traceStereotype))
                {
                    var association = this._model.GetModelElementImplementation(ModelElementType.Association, connector.ConnectorID) as EAMEIAssociation;
                    yield return new MEAssociation(association);
                }
            }
        }

        /// <summary>
        /// Enumerator method that returns the class associations of the specified type in succession.
        /// </summary>
        /// <returns>Next association of specified type.</returns>
        internal override IEnumerable<MEAssociation> AssociationList(MEAssociation.AssociationType type)
        {
            this._element.Connectors.Refresh();
            string traceStereotype = ContextSlt.GetContextSlt().GetConfigProperty(_TraceAssociationStereotype);

            for (short i = 0; i < this._element.Connectors.Count; i++)
            {
                var connector = this._element.Connectors.GetAt(i) as EA.Connector;
                // Make sure to return only connectors that originate from the current class and are NOT 'trace' associations...
                if (connector.ClientID == this._elementID && !connector.StereotypeEx.Contains(traceStereotype))
                {
                    var association = this._model.GetModelElementImplementation(ModelElementType.Association, connector.ConnectorID) as EAMEIAssociation;
                    if (association.GetAssociationType() == type) yield return new MEAssociation(association);
                }
            }
        }

        /// <summary>
        /// Create a new association instance between source and target classes. Note that in this case the 'source'
        /// endpoint descriptor is ONLY used to pass meta-data regarding the association. The MEClass part is IGNORED!
        /// MessageAssociation is treated similar to Composition. alas with a different stereotype.
        /// </summary>
        /// <param name="source">Owner-side of the association (start), class-part is ignored!.</param>
        /// <param name="destination">Destination of the association (end).</param>
        /// <param name="type">The type of association (aggregation, specialization, composition, etc.).</param>
        /// <param name="assocName">Name of the association, could be an empty string.</param>
        /// <returns>Newly created association or NULL in case of errors.</returns>
        internal override MEAssociation CreateAssociation(EndpointDescriptor source, EndpointDescriptor destination, MEAssociation.AssociationType type, string assocName)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string stereotype = string.Empty;
            if (string.IsNullOrEmpty(Name)) assocName = string.Empty;
            EA.Connector connector = null;
            switch (type)
            {
                case MEAssociation.AssociationType.Generalization:
                    connector = (EA.Connector)this._element.Connectors.AddNew(assocName, "Generalization");
                    stereotype = context.GetConfigProperty(_GeneralizationStereotype);
                    break;

                case MEAssociation.AssociationType.Usage:
                    connector = (EA.Connector)this._element.Connectors.AddNew(assocName, "Usage");
                    stereotype = string.Empty;  // Usage-type connectors do not have a stereotype!
                    connector.Direction = "Source -> Destination";
                    break;

                case MEAssociation.AssociationType.MessageAssociation:
                    connector = (EA.Connector)this._element.Connectors.AddNew(assocName, "Association");
                    stereotype = context.GetConfigProperty(_MessageAssociationStereotype);
                    break;

                default:
                    connector = (EA.Connector)this._element.Connectors.AddNew(assocName, "Association");
                    stereotype = context.GetConfigProperty(_AssociationStereotype);
                    break;
            }

            connector.SupplierID = destination.EndPoint.ElementID;
            connector.StereotypeEx = stereotype;
            connector.Update();
            this._element.Connectors.Refresh();

            EA.ConnectorEnd sourceEnd = connector.ClientEnd;
            EA.ConnectorEnd destinationEnd = connector.SupplierEnd;

            if (type == MEAssociation.AssociationType.Generalization)
            {
                sourceEnd.Aggregation = 0;
                destinationEnd.Aggregation = 0;
                sourceEnd.Navigable = "Unspecified";
                destinationEnd.Navigable = "Unspecified";
            }
            else
            {
                // All 'general purpose' associations (no inheritance)...
                if (type != MEAssociation.AssociationType.Composition && type != MEAssociation.AssociationType.MessageAssociation)
                {
                    if (source.Cardinality != string.Empty) sourceEnd.Cardinality = source.Cardinality;
                    if (source.Role != string.Empty) sourceEnd.Role = source.Role;
                    if (source.AliasRole != string.Empty) sourceEnd.Alias = source.AliasRole;
                    sourceEnd.Navigable = (source.IsNavigable) ? "Navigable" : "Non-Navigable";
                }

                if (destination.Cardinality != string.Empty) destinationEnd.Cardinality = destination.Cardinality;
                if (destination.Role != string.Empty) destinationEnd.Role = destination.Role;
                if (destination.AliasRole != string.Empty) destinationEnd.Alias = destination.AliasRole;
                destinationEnd.Navigable = (destination.IsNavigable) ? "Navigable" : "Non-Navigable";
                destinationEnd.Aggregation = 0;
                if (type == MEAssociation.AssociationType.Aggregation) sourceEnd.Aggregation = 1;
                else if (type == MEAssociation.AssociationType.Composition || type == MEAssociation.AssociationType.MessageAssociation) sourceEnd.Aggregation = 2;
                else sourceEnd.Aggregation = 0;
                if (type == MEAssociation.AssociationType.Usage) destinationEnd.Navigable = "Navigable";
            }

            sourceEnd.Update();
            destinationEnd.Update();
            ((EAModelImplementation)this._model).Repository.AdviseElementChange(this._element.ElementID);
            return new MEAssociation(connector.ConnectorID);
        }

        /// <summary>
        /// Creates a new attribute in the current class using an existing MEAttribute object.
        /// </summary>
        /// <param name="attrib">The attribute object to be used as basis.</param>
        /// <exception cref="ArgumentException">Illegal or missing attribute.</exception>
        internal override void CreateAttribute(MEAttribute attrib)
        {
            if (attrib == null)
            {
                string message = "SparxEA.Model.EAMEIClass.createAttribute >> Attempt to create a 'null' attribute '" + this._name + "'!";
                Logger.WriteError(message);
                throw new ArgumentException(message);
            }

            bool isConstant = !string.IsNullOrEmpty(attrib.FixedValue);
            AttributeType attributeType;
            switch (attrib.Type)
            {
                case ModelElementType.Facet:
                    attributeType = AttributeType.Facet;
                    break;

                case ModelElementType.Supplementary:
                    attributeType = AttributeType.Supplementary;
                    break;

                case ModelElementType.Attribute:
                    attributeType = AttributeType.Attribute;
                    break;

                default:
                    attributeType = AttributeType.Unknown;
                    break;
            }
            string value = !string.IsNullOrEmpty(attrib.DefaultValue)? attrib.DefaultValue: attrib.FixedValue;
            CreateAttribute(attrib.Name, attrib.Classifier, attributeType, value, attrib.Cardinality, isConstant, attrib.Annotation);
        }

        /// <summary>
        /// Creates a new attribute in the current class.
        /// <param name="name">Name of the attribute.</param>
        /// <param name="classifier">Attribute classifier (type of the attribute).</param>
        /// <param name="type">The type of attribute to create (regular, supplementary or facet)</param>
        /// <param name="defaultValue">An optional default value to be assignd to the attribute.</param>
        /// <param name="cardinality">Specifies lower and upper boundary of cardinality.</param>
        /// <param name="isConstant">Indicates that the attribute has a constant value. Default value must be specified in this case!</param>
        /// <param name="annotation">Annotation text for the attribute.</param>
        /// </summary>
        /// <returns>Newly created attribute object.</returns>
        /// <exception cref="ArgumentException">Illegal or missing name.</exception>
        internal override MEAttribute CreateAttribute(string attribName, MEDataType classifier, AttributeType type, string defaultValue, Tuple<int, int> cardinality, bool isConstant, string annotation)
        {
            if (string.IsNullOrEmpty(attribName))
            {
                string message = "SparxEA.Model.EAMEIClass.createAttribute >> Attempt to create an attribute without name in class '" + this._name + "'!";
                Logger.WriteError(message);
                throw new ArgumentException(message);
            }
            if (classifier == null || classifier.Type == ModelElementType.Unknown)
            {
                string message = "SparxEA.Model.EAMEIClass.createAttribute >> Attempt to create attribute '" + attribName + "' with illegal classifier in class '" + this.Name + "'!";
                Logger.WriteError(message);
                throw new ArgumentException(message);
            }

            ContextSlt context = ContextSlt.GetContextSlt();
            var newAttribute = this._element.Attributes.AddNew(attribName, "Attribute") as EA.Attribute;
            newAttribute.Update();    // Update immediately to properly finish the creation!
            this._element.Attributes.Refresh();

            // We use 'type' to determine the appropriate stereotype...
            switch (type)
            {
                case AttributeType.Facet:
                    newAttribute.StereotypeEx = context.GetConfigProperty(_FacetAttStereotype);
                    break;

                case AttributeType.Supplementary:
                    newAttribute.StereotypeEx = context.GetConfigProperty(_SupplementaryAttStereotype);
                    break;

                default:
                    newAttribute.StereotypeEx = context.GetConfigProperty(_ContentAttStereotype);
                    break;
            }

            if (!string.IsNullOrEmpty(defaultValue)) newAttribute.Default = defaultValue;
            newAttribute.ClassifierID = classifier.ElementID;
            newAttribute.Type = classifier.Name;
            newAttribute.LowerBound = cardinality.Item1.ToString();
            newAttribute.UpperBound = (cardinality.Item2 == 0)? "*": cardinality.Item2.ToString();
            newAttribute.Visibility = "Public";
            if (isConstant && string.IsNullOrEmpty(defaultValue))
            {
                Logger.WriteWarning("Attribute: '" + attribName + "' in class: '" + this._element.Name +
                                    "' marked constant without specified default value, skipped!");
            }
            else newAttribute.IsConst = isConstant;

            // Add annotation to attribute...
            if (!string.IsNullOrEmpty(annotation)) newAttribute.Notes = annotation;

            newAttribute.Update();
            ((EAModelImplementation)this._model).Repository.AdviseElementChange(this._element.ElementID);
            return new MEAttribute(newAttribute.AttributeID);
        }

        /// <summary>
        /// Delete the specified association from the class. If the association could not be found, the operation fails silently.
        /// </summary>
        /// <param name="thisOne">The association to be deleted.</param>
        internal override void DeleteAssociation(MEAssociation association)
        {
            for (short i = 0; i < this._element.Connectors.Count; i++)
            {
                var currConnector = this._element.Connectors.GetAt(i) as EA.Connector;
                if (currConnector.ConnectorID == association.ElementID)
                {
                    this._element.Connectors.DeleteAt(i, true); // Refresh options currently does not work.
                    this._element.Connectors.Refresh();
                    association.InValidate();                   // Make sure to mark associated implementation as invalid!
                    ((EAModelImplementation)this._model).Repository.AdviseElementChange(this._element.ElementID);
                    return;
                }
            }
            Logger.WriteWarning("Attempt to delete '" + association.ElementID + "' from class: '" + this._element.Name + 
                                "' failed; association not found!");
        }

        /// <summary>
        /// Deletes the specified attribute from the current class. Fails silently (with warning to logging) in case the
        /// attribute could not be found.
        /// <param name="attribute">Attribute to delete.</param>
        /// </summary>
        internal override void DeleteAttribute(MEAttribute attribute)
        {
            for (short i=0; i<this._element.Attributes.Count; i++)
            {
                var attrib = this._element.Attributes.GetAt(i) as EA.Attribute;
                if (attrib.AttributeID == attribute.ElementID)
                {
                    this._element.Attributes.DeleteAt(i, true); // Refresh options currently does not work.
                    this._element.Attributes.Refresh();
                    attribute.InValidate();                     // Make sure to mark associated implementation as invalid!
                    ((EAModelImplementation)this._model).Repository.AdviseElementChange(this._element.ElementID);
                    return;
                }
            }
            Logger.WriteWarning("Attempt to delete '" + attribute.Name + "' from class: '" + this._element.Name + "' failed; attribute not found!");
        }

        /// <summary>
        /// Searches the class for an attribute with specified name and type.
        /// </summary>
        /// <param name="name">Name of the attribute to find.</param>
        /// <param name="type">Attribute type (Attribute, Supplementary or Facet).</param>
        /// <returns>Attribute or NULL if nothing found.</returns>
        internal override MEAttribute FindAttribute(string name, AttributeType type)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string facetStereotype = context.GetConfigProperty(_FacetAttStereotype);
            string suppStereotype =  context.GetConfigProperty(_SupplementaryAttStereotype);

            // Reading StereotypeEX does not return fully qualified stereotype names so we remove the profile name if specified....
            facetStereotype = (facetStereotype.Contains("::")) ? facetStereotype.Substring(facetStereotype.IndexOf("::") + 2) : facetStereotype;
            suppStereotype = (suppStereotype.Contains("::")) ? suppStereotype.Substring(suppStereotype.IndexOf("::") + 2) : suppStereotype;

            foreach (EA.Attribute attribute in this._element.Attributes)
            {
                if (attribute.Name == name)
                {
                    if (type == AttributeType.Supplementary && attribute.StereotypeEx.Contains(suppStereotype))
                    {
                        return new MESupplementary(attribute.AttributeID);
                    }
                    else if (type == AttributeType.Facet && attribute.StereotypeEx.Contains(facetStereotype))
                    {
                        return new MEFacet(attribute.AttributeID);
                    }
                    else if (type == AttributeType.Attribute) 
                    {
                        return new MEAttribute(attribute.AttributeID);
                    }
                }
            }
            Logger.WriteInfo("SparxEA.Model.EAMEIClass.FindAttribute >> Requested attribute '" + name + "' not found in class '" + this._element.Name + "'!");
            return null;
        }

        /// <summary>
        /// Searches all associations on the current class for a child class with specified name and stereotype and returns the
        /// association role that belongs to this child.
        /// Note that the function only searches for the PRIMARY stereotype and ignores any generalized stereotypes!
        /// </summary>
        /// <param name="childName">Name of child class to locate.</param>
        /// <param name="childStereotype">Child primary stereotype.</param>
        /// <returns>Role name or empty string if not found.</returns>
        internal override string FindAssociatedClassRole (string childName, string childStereotype)
        {
            EA.Repository repository = ((EAModelImplementation)this._model).Repository;
            bool isLocalDB = ModelSlt.GetModelSlt().ModelRepositoryType == ModelSlt.RepositoryType.Local;
            string likeClause = isLocalDB ? "LIKE '*" : "LIKE '%";  // EAP files use different syntax for 'like'!
            string checkType = (childStereotype.Contains("::")) ? childStereotype.Substring(childStereotype.IndexOf("::") + 2) : childStereotype;
            string query = @"SELECT c.DestRole AS DestRole FROM
                            ((t_connector c INNER JOIN t_object o ON c.Start_Object_ID = o.Object_ID)
                            LEFT JOIN t_object o2 ON c.End_Object_ID = o2.Object_ID)
                            WHERE o.Object_ID = " + this._elementID + " AND o2.Name = '" + childName + 
                            "' AND o2.Stereotype " + likeClause + checkType + "'";

            var queryResult = new XmlDocument();                            // Repository query will return an XML Document.
            queryResult.LoadXml(repository.SQLQuery(query));                // Execute query and store result in XML Document.
            XmlNodeList elements = queryResult.GetElementsByTagName("Row");

            return (elements.Count > 0) ? elements[0]["DestRole"].InnerText.Trim() : string.Empty;
        }

        /// <summary>
        /// Searches all associations on the current class for any child class with specified name and stereotype and returns the
        /// list of matching classes.
        /// Note that the function only searches for the PRIMARY stereotype and ignores any generalized stereotypes!
        /// </summary>
        /// <param name="childName">Name of child class to locate.</param>
        /// <param name="childStereotype">Child primary stereotype.</param>
        /// <returns>List of matching classes or empty list when none found.</returns>
        internal override List<MEClass> FindAssociatedClasses(string childName, string childStereotype)
        {
            EA.Repository repository = ((EAModelImplementation)this._model).Repository;
            bool isLocalDB = ModelSlt.GetModelSlt().ModelRepositoryType == ModelSlt.RepositoryType.Local;
            string likeClause = isLocalDB ? "LIKE '*" : "LIKE '%";  // EAP files use different syntax for 'like'!
            string checkType = (childStereotype.Contains("::")) ? childStereotype.Substring(childStereotype.IndexOf("::") + 2) : childStereotype;
            string query = @"SELECT o2.Object_ID AS DestObject FROM
                            ((t_connector c INNER JOIN t_object o ON c.Start_Object_ID = o.Object_ID)
                            LEFT JOIN t_object o2 ON c.End_Object_ID = o2.Object_ID)
                            WHERE o.Object_ID = " + this._elementID + " AND o2.Name = '" + childName +
                            "' AND o2.Stereotype " + likeClause + checkType + "'";

            var queryResult = new XmlDocument();                            // Repository query will return an XML Document.
            queryResult.LoadXml(repository.SQLQuery(query));                // Execute query and store result in XML Document.
            var matches = new List<MEClass>();
            foreach (XmlNode node in queryResult.GetElementsByTagName("Row"))
            {
                int objectID = 0;
                if (int.TryParse(node["DestObject"].InnerText.Trim(), out objectID)) matches.Add(new MEClass(objectID));
            }
            return matches;
        }

        /// <summary>
        /// Composite classes are associated with a diagram that shows the class composition. This method facilitates retrieval of 
        /// this associated diagram. The function returns NULL in case no diagram is associated.
        /// </summary>
        /// <returns>Associated diagram or NULL if no such association exists.</returns>
        internal override Framework.View.Diagram GetAssociatedDiagram()
        {
            EA.Diagram diagram = this._element.CompositeDiagram as EA.Diagram;
            return (diagram != null)? new Framework.View.Diagram(diagram.DiagramID): null;
        }

        /// <summary>
        /// Returns a list of all attributes of the class. It ONLY returns the attributes for the current class, NOT for any
        /// specialized (root) classes!
        /// <returns>List of all attributes for the current class (can be empty if no attributes are defined).</returns>
        /// </summary>
        internal override List<MEAttribute> GetAttributes()
        {
            var attribList = new List<MEAttribute>();
            ContextSlt context = ContextSlt.GetContextSlt();
            string suppStereotype = context.GetConfigProperty(_SupplementaryAttStereotype);
            string facetStereotype = context.GetConfigProperty(_FacetAttStereotype);
            suppStereotype = (suppStereotype.Contains("::")) ? suppStereotype.Substring(suppStereotype.IndexOf("::") + 2) : suppStereotype;
            facetStereotype = (facetStereotype.Contains("::")) ? facetStereotype.Substring(facetStereotype.IndexOf("::") + 2) : facetStereotype;

            foreach (EA.Attribute attribute in this._element.Attributes)
            {
                if (attribute.StereotypeEx.Contains(suppStereotype))
                {
                    attribList.Add(new MESupplementary(attribute.AttributeID));
                }
                else if (attribute.StereotypeEx.Contains(facetStereotype))
                {
                    attribList.Add(new MEFacet(attribute.AttributeID));
                }
                else // Attributes that are either explicitly marked as 'Content' or that have no stereotype...
                {
                    attribList.Add(new MEAttribute(attribute.AttributeID));
                }
            }
            return attribList;
        }

        /// <summary>
        /// Retrieve the build number (which is represented by the "Phase" tag in the EA element).
        /// </summary>
        internal override int GetBuildNumber()
        {
            // Try parsing the build number field to an integer. Must be a round number to succeed!
            int buildNumber;
            if (!Int32.TryParse(this._element.Phase, out buildNumber)) buildNumber = -1;
            return buildNumber;
        }

        /// <summary>
        /// Returns element annotation (if present).
        /// </summary>
        /// <returns>Element annotation or empty string if nothing present.</returns>
        internal override string GetAnnotation()
        {
            string notes = this._element.Notes;
            return (!string.IsNullOrEmpty(notes))? notes.Trim(): string.Empty;
        }

        /// <summary>
        /// Returns the author of the Class, i.e. the user who has last made changes to the Class.
        /// </summary>
        /// <returns>Class author.</returns>
        internal override string GetAuthor()
        {
            string author = this._element.Author;
            return (!string.IsNullOrEmpty(author)) ? author : string.Empty;
        }

        /// <summary>
        /// Retrieves the complete inheritance tree of the current class in a sorted list. The key in this list identifies
        /// the distance from the current class (0 = class itself).
        /// If the specified element has no root, the parentList will only contain the element itself on return (at key 0).
        /// The function correctly tracks multiple paths through the hierarchy, as long as the model is a directed graph with a
        /// single root. Behavior is undetermined in case of multiple root classes!
        /// The method is actually a wrapper around the 'traverseParents' method, which does all the 'real' work.
        /// </summary>
        /// <param name="topStereotype">If specified, the search ends at the first class that posesses this stereotype.</param>
        internal override SortedList<uint, MEClass> GetHierarchy(string topStereotype)
        {
            var parentList = new SortedList<uint, MEClass>();
            var myClass = new MEClass(this);
            if (!string.IsNullOrEmpty(topStereotype) || !HasStereotype(topStereotype)) TraverseParents(myClass, ref parentList, 10, topStereotype);
            parentList.Add(0, myClass);
            return parentList;
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
            MEClassMetaData metaData = new MEClassMetaData(this._element.Name, this._element.Alias, this._element.StereotypeEx);
            ContextSlt context = ContextSlt.GetContextSlt();
            string facetStereotype = context.GetConfigProperty(_FacetAttStereotype);
            string suppStereotype = context.GetConfigProperty(_SupplementaryAttStereotype);
            int ID;
            string name;
            string alias;
            Tuple<int, int> card;
            char type;

            // First, we collect the 'regular' attributes...
            foreach (EA.Attribute att in this._element.Attributes)
            {
                ID = (att.Pos + 1) * 100;
                name = att.Name;
                alias = att.Alias ?? string.Empty;

                // Create Cardinality...
                try
                {
                    int maxOcc;
                    int minOcc = Convert.ToInt16(att.LowerBound);
                    if (minOcc < 0) minOcc = 0;
                    if (att.UpperBound.Contains("*") || att.UpperBound.Contains("n") || att.UpperBound.Contains("N")) maxOcc = 0;
                    else maxOcc = Convert.ToInt16(att.UpperBound);
                    card = new Tuple<int, int>(minOcc, maxOcc);
                }
                catch // Conversion error, create an invalid cardinality tuple...
                {
                    card = new Tuple<int, int>(-1, -1);
                }

                // Determine the attribute type.
                type = 'C';    // Default is most generic type.
                if (HasStereotype(att, facetStereotype)) type = 'F';
                else if (HasStereotype(att, suppStereotype)) type = 'S';

                metaData.AddAttribute(ID, name, alias, att.Type, card, type);
            }

            // Next, we collect associations...
            foreach (MEAssociation association in AssociationList())
            {
                string IDTag = association.GetTag(context.GetConfigProperty(_SequenceKeyTag), MEAssociation.AssociationEnd.Association);
                MEClass target = association.Destination.EndPoint;
                if (target.Type == ModelElementType.Class)
                {
                    ID = 0;
                    if (!string.IsNullOrEmpty(IDTag) && !int.TryParse(IDTag, out ID)) ID = -1;      // Set to illegal value when illegal contents.
                    type = (association.TypeOfAssociation == MEAssociation.AssociationType.Generalization) ? 'G' : 'A';
                    name = association.Destination.Role;
                    alias = association.Destination.AliasRole;
                    card = association.GetCardinality(MEAssociation.AssociationEnd.Destination);
                    metaData.AddAttribute(ID, name, alias, association.Destination.EndPoint.Name, card, type);
                }
            }
            return metaData;
        }

        /// <summary>
        /// Retrieve the package in which this class is declared.
        /// </summary>
        /// <returns>Owning package.</returns>
        internal override MEPackage GetOwningPackage()
        {
            return new MEPackage(this._element.PackageID);
        }

        /// <summary>
        /// Retrieves the parent(s) of the current class (if any). Because we support multiple inheritance, there can be multiple 
        /// parents, hence the return list.
        /// This method is a wrapper around the 'private' variant that retrieves the direct parents of the local class.
        /// </summary>
        /// <returns>List of parents, empty if no parent found.</returns>
        internal override List<MEClass> GetParents()
        {
            return this.GetParents(new MEClass(this));
        }

        /// <summary>
        /// Simple method that searches a Class implementation for the occurence of a tag with specified name. 
        /// If found, the value of the tag is returned.
        /// </summary>
        /// <param name="tagName">The name of the tag.</param>
        /// <returns>Value of specified tag or empty string if nothing could be found.</returns>
        internal override string GetTag(string tagName)
        {
            string retVal = string.Empty;
            try
            {
                foreach (TaggedValue t in this._element.TaggedValues)
                {
                    if (String.Compare(t.Name, tagName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        retVal = t.Value;
                        break;
                    }
                }
            }
            catch { /* & ignore all errors */ }
            return retVal;
        }

        /// <summary>
        /// Retrieve the major- and minor version components of the class as a tuple of two integers.
        /// If no version is set, or on illegal formats, the function returns the equivalent of "1.0".
        /// </summary>
        /// <returns>Major- and minor version numbers of the class.</returns>
        internal override Tuple<int, int> GetVersion()
        {
            string version = this._element.Version;
            int major = 1;
            int minor = 0;
            if (version != string.Empty)
            {
                string[] components = version.Split(new char[] { '.' });
                Int32.TryParse(components[0], out major);   // Returns 0 on errors.
                Int32.TryParse(components[1], out minor);   // Returns 0 on errors.
                if (major == 0) major = 1;
            }
            return new Tuple<int, int>(major, minor);
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
            this._element.Connectors.Refresh();
            string traceStereotype = ContextSlt.GetContextSlt().GetConfigProperty(_TraceAssociationStereotype);

            for (short i = 0; i < this._element.Connectors.Count; i++)
            {
                var connector = this._element.Connectors.GetAt(i) as EA.Connector;
                // Make sure to return only connectors that originate from the current class and are NOT 'trace' associations...
                if (connector.ClientID == this._elementID && !connector.StereotypeEx.Contains(traceStereotype))
                {
                    var association = this._model.GetModelElementImplementation(ModelElementType.Association, connector.ConnectorID) as EAMEIAssociation;
                    if (type == MEAssociation.AssociationType.Unknown && association.GetAssociationType() != MEAssociation.AssociationType.Generalization) return true;
                    else if (type != MEAssociation.AssociationType.Unknown && association.GetAssociationType() == type) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// This function checks whether the class contains one or more attributes and/or associations.
        /// </summary>
        /// <returns>True is class posesses one or more attributes and/or associations.</returns>
        internal override bool HasContents()
        {
            bool hasContents = this._element.Attributes.Count > 0;
            if (!hasContents)
            {
                this._element.Connectors.Refresh();
                for (short i = 0; i < this._element.Connectors.Count; i++)
                {
                    var connector = this._element.Connectors.GetAt(i) as EA.Connector;
                    if (connector.ClientID == this._elementID) // Make sure to test only connectors that originate from the current class!
                    {
                        hasContents = true;
                        break;
                    }
                }
            }
            return hasContents;
        }

        /// <summary>
        /// The method checks whether one or more stereotypes from the given list of stereotypes are owned by the Class.
        /// </summary>
        /// <param name="stereotypes">List of stereotypes to check.</param>
        /// <returns>True if at least one match is found, false otherwise.</returns>
        internal override bool HasStereotype(List<string> stereotypes)
        {
            foreach (string stereotype in stereotypes)
            {
                if (this._element.HasStereotype(stereotype)) return true;
            }
            return false;
        }

        /// <summary>
        /// The method checks whether the given stereotype is owned by the Class.
        /// </summary>
        /// <param name="stereotype">Stereotype to check.</param>
        /// <returns>True if a match is found, false otherwise.</returns>
        internal override bool HasStereotype(string stereotype)
        {
            bool hasStereotype = this._element.HasStereotype(stereotype);
            return hasStereotype;
        }

        /// <summary>
        /// Helper function that enforces proper position numbering on each attribute of the current class. When parameter 'onlyCheck' is set
        /// to 'true', the function only checks the order, but does not change it. 
        /// In all cases, a return value of 'false' indicates that the order is wrong (but with 'onlyCheck = false', it will have been repaired).
        /// </summary>
        /// <param name="onlyCheck">Set to 'true' to enforce a check only.</param>
        /// <returns>Returns TRUE when the order in the class was Ok at moment of call or FALSE when attributes are/were out of order.</returns>
        internal override bool RepairAttributeOrder(bool onlyCheck)
        {
            int pos = 0;
            bool repairNeeded = false;

            foreach (EA.Attribute attrib in this._element.Attributes)
            {
                if (attrib.Pos != pos++)
                {
                    repairNeeded = true;
                    break;
                }
            }

            if (!onlyCheck && repairNeeded)
            {
                pos = 0;
                foreach (EA.Attribute attrib in this._element.Attributes)
                {
                    attrib.Pos = pos++;
                    attrib.Update();
                }
                this._element.Refresh();
                ((EAModelImplementation)this._model).Repository.AdviseElementChange(this._element.ElementID);
            }
            return !repairNeeded;
        }

        /// <summary>
        /// Updates Class annotation.
        /// </summary>
        /// <param name="text">The annotation text to write.</param>
        internal override void SetAnnotation(string text)
        {
            this._element.Notes = text;
            this._element.Update();
            ((EAModelImplementation)this._model).Repository.AdviseElementChange(this._element.ElementID);
        }

        /// <summary>
        /// Update the alias name of the Class.
        /// </summary>
        /// <param name="newAliasName">New name to be assigned.</param>
        internal override void SetAliasName(string newAliasName)
        {
            this._element.Alias = newAliasName;
            this._aliasName = newAliasName;
            this._element.Update();
            ((EAModelImplementation)this._model).Repository.AdviseElementChange(this._element.ElementID);
        }

        /// <summary>
        /// Composite classes are associated with a diagram that shows the class composition. This method facilitates creation of 
        /// an association between the class and a diagram. A Class can only have at most a single association, so repeated invocations
        /// will overwrite the association.
        /// </summary>
        /// <param name="diagram">Diagram to associate with the class.</param>
        internal override void SetAssociatedDiagram(Framework.View.Diagram diagram)
        {
            if (!this._element.SetCompositeDiagram(diagram.GlobalID))
            {
                Logger.WriteError("SparxEA.Model.EAMEIClass.SetAssociatedDiagram >> Failed to associate class '" + this._name + 
                                  "' with diagram '" + diagram.Name + "'!");
            }
        }

        /// <summary>
        /// Update the name of the Class.
        /// </summary>
        /// <param name="newName">New name to be assigned.</param>
        internal override void SetName(string newName)
        {
            this._element.Name = newName;
            this._name = newName;
            this._element.Update();
            ((EAModelImplementation)this._model).Repository.AdviseElementChange(this._element.ElementID);
        }

        /// <summary>
        /// Set the build number (which is represented by the "Phase" tag in the EA element).
        /// </summary>
        /// <param name="buildNumber">New buildnumber value.</param>
        internal override void SetBuildNumber(int buildNumber)
        {
            this._element.Phase = buildNumber.ToString();
            this._element.Update();
            ((EAModelImplementation)this._model).Repository.AdviseElementChange(this._element.ElementID);
        }

        /// <summary>
        /// Overwrites the current version of the class with provided values. 
        /// The version string will be formatted as major "." minor.
        /// </summary>
        /// <param name="version">New version.</param>
        internal override void SetVersion(Tuple<int, int> version)
        {
            this._element.Version = version.Item1 + "." + version.Item2;
            this._element.Update();
            ((EAModelImplementation)this._model).Repository.AdviseElementChange(this._element.ElementID);
        }

        /// <summary>
        /// Set the tag with specified name to the specified value for the current class. 
        /// If the tag could not be found, nothing will happen, unless the 'createIfNotExists' flag is set to true, in which case the tag will
        /// be created.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="tagValue">Value of the tag.</param>
        /// <param name="createIfNotExist">When set to true, create the tag if it's not there.</param>
        internal override void SetTag(string tagName, string tagValue, bool createIfNotExist)
        {
            try
            {
                foreach (TaggedValue t in this._element.TaggedValues)
                {
                    if (String.Compare(t.Name, tagName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        t.Value = tagValue;
                        t.Update();
                        this._element.TaggedValues.Refresh();
                        ((EAModelImplementation)this._model).Repository.AdviseElementChange(this._element.ElementID);
                        return;
                    }
                }

                // Element tag not found, create new one if instructed to do so...
                if (createIfNotExist)
                {
                    var newTag = this._element.TaggedValues.AddNew(tagName, "TaggedValue") as TaggedValue;
                    newTag.Value = tagValue;
                    newTag.Update();
                    this._element.TaggedValues.Refresh();
                    ((EAModelImplementation)this._model).Repository.AdviseElementChange(this._element.ElementID);
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("SparxEA.Model.EAMEIClass.SetTag >> Unable to update tag '" + tagName + "' in class '" + this.Name + 
                                  "' because: " + Environment.NewLine + exc.ToString());
            }
        }

        /// <summary>
        /// Retrieves the parent(s) of the provided class (if any). Because we support multiple inheritance, there can be multiple 
        /// parents, hence the return list.
        /// The method uses a direct SQL query on the repository, since some elements might have a large number of associated 
        /// connectors. Using a lineair iteration consumes too many resources in this case.
        /// </summary>
        /// <param name="thisClass">The class for which we want to retrieve parent(s).</param>
        /// <returns>List of parents, empty if no parent found.</returns>
        private List<MEClass> GetParents(MEClass thisClass)
        {
            EA.Repository repository = ((EAModelImplementation)this._model).Repository;

            string query = @"SELECT o2.Object_ID AS ParentID
                            FROM ((t_connector c INNER JOIN t_object o ON c.Start_Object_ID = o.Object_ID) 
                            LEFT JOIN t_object o2 ON c.End_Object_ID = o2.Object_ID)
                            WHERE o.Object_ID = " + thisClass.ElementID + " AND c.Connector_Type = 'Generalization';";

            var queryResult = new XmlDocument();                    // Repository query will return an XML Document.
            queryResult.LoadXml(repository.SQLQuery(query));                // Execute query and store result in XML Document.
            XmlNodeList elements = queryResult.GetElementsByTagName("Row"); // Retrieve parent class name and package in which class resides.
            var parentList = new List<MEClass>();

            // Class could have multiple parents, check each in turn...
            for (int i = 0; i < elements.Count; i++)
            {
                int parentID = Convert.ToInt32(elements[i]["ParentID"].InnerText.Trim());
                parentList.Add(new MEClass(parentID));
            }
            return parentList;
        }

        /// <summary>
        /// The method checks whether the given attribute contains the given stereotype.
        /// </summary>
        /// <param name="attribute">Attribute to check.</param>
        /// <param name="stereotype">Stereotype to check.</param>
        /// <returns>True if a match is found, false otherwise.</returns>
        private bool HasStereotype(EA.Attribute attribute, string stereotype)
        {
            string normalizedType = (stereotype.Contains("::")) ? stereotype.Substring(stereotype.IndexOf("::") + 2) : stereotype;
            return attribute.StereotypeEx.Contains(normalizedType);
        }

        /// <summary>
        /// Helper method that recursively traverses the class hierarchy, looking for the top-most class (the one that does not have a parent).
        /// The function recursively traverses all possible paths through the hierarchy, storing all intermediate elements in the 'parentList' reference 
        /// (including the root). 
        /// If a class has multiple roots, the order of the hierarchy list is undefined (but should still work ok).
        /// </summary>
        /// <param name="thisClass">The class for which we want to get the root.</param>
        /// <param name="parentList">Will be used to store all parent objects.</param>
        /// <param name="level">Specifies the current level in the hierarchy, 0 being the initial level.</param>
        /// <param name="topStereotype">When specified, this parameter states that we have to stop at the first class that posesses this stereotype.</param>
        private void TraverseParents(MEClass thisClass, ref SortedList<uint, MEClass> parentList, uint level, string topStereotype)
        {
            List<MEClass> currentParents = GetParents(thisClass);

            if (currentParents != null && currentParents.Count > 0)
            {
                // We found parents, remember and try again...
                uint counter = 0;  // Keeps track of parent classes on same level;

                foreach (MEClass parent in currentParents)
                {
                    bool isInList = false;
                    foreach (KeyValuePair<uint, MEClass> p in parentList)
                    {
                        if (p.Value.ElementID == parent.ElementID)
                        {
                            isInList = true;
                            break;
                        }
                    }

                    if (!isInList)
                    {
                        parentList.Add(level + counter++, parent);
                        if (topStereotype == null || !parent.HasStereotype(topStereotype))
                        {
                            TraverseParents(parent, ref parentList, level + 100, topStereotype);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}
