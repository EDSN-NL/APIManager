﻿using System;
using System.Collections.Generic;
using Framework.Logging;
using Framework.Model;
using Framework.Util;

namespace SparxEA.Model
{
    /// <summary>
    /// Represents an 'Object' within Sparx EA. Basically, in EA an Object is a Class (or better, an 'Element' as they are
    /// called in EA). The 'Type' of this Element is 'Object' and most operations are identical to the basis 'Element' 
    /// operations. In out model, this basic 'Element' is represented by 'Class'.
    /// </summary>
    internal sealed class EAMEIObject: MEIObject
    {
        /// <summary>
        /// In EA, an Object is basically a Class with some additional methods.
        /// </summary>
        private EAMEIClass _classPart;

        /// <summary>
        /// The internal constructor is called to initialize the repository.
        /// </summary>
        internal EAMEIObject(EAModelImplementation model, int objectID): base(model)
        {
            this._classPart = model.GetModelElementImplementation(ModelElementType.Class, objectID) as EAMEIClass;
            this._name = this._classPart.Name;
            this._elementID = this._classPart.ElementID;
            this._globalID = this._classPart.GlobalID;
            this._aliasName = this._classPart.AliasName;
        }

        /// <summary>
        /// The internal constructor is called to initialize the repository.
        /// </summary>
        internal EAMEIObject(EAModelImplementation model, string objectGUID) : base(model)
        {
            this._classPart = model.GetModelElementImplementation(ModelElementType.Class, objectGUID) as EAMEIClass;
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
        internal EAMEIObject(EAModelImplementation model, EA.Element element) : base(model)
        {
            this._classPart = model.GetModelElementImplementation(ModelElementType.Class, element.ElementID) as EAMEIClass;
            this._name = this._classPart.Name;
            this._elementID = this._classPart.ElementID;
            this._globalID = this._classPart.GlobalID;
            this._aliasName = this._classPart.AliasName;
        }

        /// <summary>
        /// Assignes the specified stereotype to this object type.
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
        /// Enumerator method that returns all ingress specialization associations of the current class.
        /// </summary>
        /// <returns>Next ingress specialization.</returns>
        internal override IEnumerable<MEAssociation> ChildAssociationList()
        {
            return this._classPart.ChildAssociationList();
        }

        /// <summary>
        /// Create a new association instance between this object and a specified target class. Note that in this case the 'source'
        /// endpoint descriptor is ONLY used to pass meta-data regarding the association. The MEClass part is IGNORED!
        /// Since objects only support association and Usage types of associations, all other types will be rejected!
        /// </summary>
        /// <param name="source">Owner-side of the association (start), class-part is ignored!.</param>
        /// <param name="target">Destination of the association (end).</param>
        /// <param name="type">The type of association (aggregation, specialization, composition, etc.).</param>
        /// <param name="name">Name of the association, could be an empty string.</param>
        /// <returns>Newly created association or NULL in case of errors.</returns>
        internal override MEAssociation CreateAssociation(EndpointDescriptor source, EndpointDescriptor destination, MEAssociation.AssociationType type, string assocName)
        {
            if (type != MEAssociation.AssociationType.Generalization && type != MEAssociation.AssociationType.MessageAssociation)
            {
                return this._classPart.CreateAssociation(source, destination, type, assocName);
            }
            else
            {
                Logger.WriteError("SparxEA.Model.EAMEIObject.createAssociation >> Attempt to create illegal association type: '" + type + "' in data type: '" + this._name + "'!");
                return null;
            }
        }

        /// <summary>
        /// Creates a new attribute in the current class using an existing MEAttribute object.
        /// </summary>
        /// <param name="attrib">The attribute object to be used as basis.</param>
        /// <exception cref="ArgumentException">Illegal or missing attribute.</exception>
        internal override void CreateAttribute(MEAttribute attrib)
        {
            this._classPart.CreateAttribute(attrib);
        }

        /// <summary>
        /// Creates a new attribute in the current object.
        /// </summary>
        /// <param name="name">Name of the attribute.</param>
        /// <param name="classifier">Attribute classifier (type of the attribute).</param>
        /// <param name="type">The type of attribute to create (regular, supplementary or facet)</param>
        /// <param name="defaultValue">An optional default value to be assignd to the attribute.</param>
        /// <param name="cardinality">Specifies lower and upper boundary of cardinality.</param>
        /// <param name="isConstant">Indicates that the attribute has a constant value. Default value must be specified in this case!</param>
        /// <param name="annotation">Annotation text for the attribute.</param>
        /// <returns>Newly created attribute object or null in case of errors.</returns>
        internal override MEAttribute CreateAttribute(string name, MEDataType classifier, AttributeType type, string defaultValue, Cardinality cardinality, bool isConstant, string annotation)
        {
            return this._classPart.CreateAttribute(name, classifier, type, defaultValue, cardinality, isConstant, annotation);
        }

        /// <summary>
        /// Deletes the specified association from the current object. Fails silently (with warning to logging) in case the
        /// association could not be found.
        /// </summary>
        /// <param name="association">Association to delete.</param>
        internal override void DeleteAssociation(MEAssociation association)
        {
            this._classPart.DeleteAssociation(association);
        }

        /// <summary>
        /// Deletes the specified attribute from the current object. Fails silently (with warning to logging) in case the
        /// attribute could not be found.
        /// </summary>
        /// <param name="attribute">Attribute to delete.</param>
        internal override void DeleteAttribute(MEAttribute attribute)
        {
            this._classPart.DeleteAttribute(attribute);
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
        /// Searches all associations on the current class for any child class with specified name and stereotype and returns the
        /// list of matching classes.
        /// Note that the function only searches for the PRIMARY stereotype and ignores any generalized stereotypes!
        /// </summary>
        /// <param name="childName">Name of child class to locate.</param>
        /// <param name="childStereotype">Child primary stereotype.</param>
        /// <returns>List of matching classes or empty list when none found.</returns>
        internal override List<MEClass> FindAssociatedClasses(string name, string stereotype)
        {
            return this._classPart.FindAssociatedClasses(name, stereotype);
        }

        /// <summary>
        /// Returns a list of all egress associations from the current class where the association name- and/or role matches a specified
        /// name pattern. The class only considers "Association" type associations. When optional field 'absoluteMatch' is specified as 'true',
        /// the names are matched exactly, otherwise we use an approximate search. Both name and role are optional. When left out, the function
        /// returns ALL egress associations from this class.
        /// </summary>
        /// <param name="assocName">Must occur in the association name.</param>
        /// <param name="assocRole">Must occur in the association role.</param>
        /// <param name="absoluteMatch">When 'true', the names must match exactly instead of approximately.</param>
        /// <returns>List of associations that match the search criteria.</returns>
        internal override List<MEAssociation> FindAssociationsByAssociationProperties(string assocName, string assocRole, bool absoluteMatch)
        {
            return this._classPart.FindAssociationsByAssociationProperties(assocName, assocRole, absoluteMatch);
        }

        /// <summary>
        /// Returns a list of all egress associations from the current class where the endpoint name- and/or stereotype matches a specified
        /// name pattern. The class only considers "Association" type associations. When optional field 'absoluteMatch' is specified as 'true',
        /// the names are matched exactly, otherwise we use an approximate search. The function MUST receive either a name or a stereotype (or
        /// both). When no valid name/stereotype is passed, an empty list is returned.
        /// </summary>
        /// <param name="className">Pattern for the class name.</param>
        /// <param name="classStereotype">Pattern for the stereotype name.</param>
        /// <param name="absoluteMatch">When 'true', the names must match exactly instead of approximately.</param>
        /// <returns>List of associations that match specified name and/or stereotype.</returns>
        internal override List<MEAssociation> FindAssociationsByEndpointProperties(string className, string classStereotype, bool absoluteMatch)
        {
            return this._classPart.FindAssociationsByEndpointProperties(className, classStereotype, absoluteMatch);
        }

        /// <summary>
        /// Returns the association that links the current class with the class specified by either classID and/or GUID. When both ID's are 
        /// specified, we're looking for a class that has BOTH properties. Otherwise, we're looking for either the database ID or global ID (GUID).
        /// When neither is specified, the function returns NULL. If there are multiple matches, we return the first one found.
        /// When no associations exist, we return NULL.
        /// </summary>
        /// <param name="classID">Database ID of target class, use negative or 0 to skip.</param>
        /// <param name="GUID">Global ID of target class, use NULL or empty string to skip.</param>
        /// <returns>First matching association or NULL when no matches are found.</returns>
        internal override MEAssociation FindAssociationByClassID(int classID, string GUID)
        {
            return this._classPart.FindAssociationByClassID(classID, GUID);
        }

        /// <summary>
        /// Searches the object for an attribute with specified name and type.
        /// </summary>
        /// <param name="name">Name of the attribute to find.</param>
        /// <param name="type">Attribute type (Attribute, Supplementary or Facet).</param>
        /// <returns>Attribute or NULL if nothing found.</returns>
        internal override MEAttribute FindAttribute(string name, AttributeType type)
        {
            return this._classPart.FindAttribute(name, type);
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
        /// Returns a list of all attributes of the object.
        /// <returns>List of all attributes for the current type (can be empty if no attributes are defined).</returns>
        /// </summary>
        internal override List<MEAttribute> GetAttributes()
        {
            return this._classPart.GetAttributes();
        }

        /// <summary>
        /// Returns the author of the Object, i.e. the user who has last made changes to the Object.
        /// </summary>
        /// <returns>Object author.</returns>
        internal override string GetAuthor()
        {
            return this._classPart.GetAuthor();
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
        /// Retrieves the complete inheritance tree of the object in a sorted list. Since Objects do not exist in a hierarchy,
        /// this method typically only returns the current object (no parents).
        /// </summary>
        /// <param name="topStereotype">If specified, the search ends at the first type that posesses this stereotype.</param>
        /// <returns>Sorted list that contains parent types + their 'distance' from this type.</returns>
        internal override SortedList<uint, MEClass> GetHierarchy(string topStereotype)
        {
            return this._classPart.GetHierarchy(topStereotype);
        }

        /// <summary>
        /// Returns the package that is 'owner' of this object, i.e. in which the object is declared.
        /// </summary>
        /// <returns>Owning package or NULL on errors.</returns>
        internal override MEPackage GetOwningPackage()
        {
            return this._classPart.GetOwningPackage();
        }

        /// <summary>
        /// Has no real use for objects since they should not have parent objects.
        /// </summary>
        /// <returns>List of parents, empty if no parent found.</returns>
        internal override List<MEClass> GetParents()
        {
            return this._classPart.GetParents();
        }

        /// <summary>
        /// Returns the run-time state of the object, which is represented by a list of properties and their current value.
        /// The run-time state in EA is represented by a single string in which each variable is enclosed in '@VAR' / '@ENDVAR'
        /// tokens. The variable string itself consists of a variable name, a value and an operator, the latter we ignore.
        /// Fields are separated by ';' characters.
        /// When the object does not have a run-time state, the function returns an empty list.
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to retrieve run-time state.</exception>
        internal override List<Tuple<string, string>> GetRuntimeState()
        {
            const string VarStartToken  = "@VAR;";
            const string VarEndToken    = "@ENDVAR;";
            const string NameToken      = "Variable=";
            const string ValueToken     = ";Value=";
           
            string runState = this._classPart.RunState;
            if (string.IsNullOrEmpty(runState))
            {
                string message = "Unable to retrieve run - state for object '" + this._name + "'!";
                Logger.WriteWarning(message);
                throw new InvalidOperationException(message);
            }

            // Collect all individual variables, which are indicated by a start- and end-token, break into separate strings...
            List<Tuple<string, string>> kvList = new List<Tuple<string, string>>();
            int startIndex = 0;
            int endIndex = 0;
            while (startIndex != -1)
            {
                startIndex = runState.IndexOf(VarStartToken);
                endIndex = runState.IndexOf(VarEndToken);
                if (startIndex >= 0 && endIndex >= 0)
                {
                    string variable = runState.Substring(startIndex + VarStartToken.Length, endIndex - startIndex - VarStartToken.Length);
                    runState = runState.Substring(endIndex + VarEndToken.Length);

                    // Break the individual variable string into a name/value tuple....
                    int startVarIndex = variable.IndexOf(NameToken);
                    int endVarIndex = variable.IndexOf(ValueToken);
                    if (startVarIndex >= 0 && endVarIndex >= 0)
                    {
                        string varName = variable.Substring(startVarIndex + NameToken.Length, endVarIndex - (startVarIndex + NameToken.Length));
                        string varValue = variable.Substring(endVarIndex + ValueToken.Length);
                        varValue = varValue.Substring(0, varValue.IndexOf(';'));    // Value is all up till first ';' char.
                        kvList.Add(new Tuple<string, string>(varName, varValue));
                    }
                }
            }
            return kvList;
        }

        /// <summary>
        /// Returns the major and minor version of the type.
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
        /// Simple method that searches a Object implementation for the occurence of a tag with specified name. 
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
        /// Searches the class (and all parent classes) for an attribute with specified name.
        /// </summary>
        /// <param name="name">Name of the attribute to find.</param>
        /// <returns>True when class (or one of the parent classes) contains an attribute with given name, false otherwise.</returns>
        internal override bool HasAttribute(string name)
        {
            return this._classPart.HasAttribute(name);
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
        /// The method checks whether one or more stereotypes from the given list of stereotypes are owned by the Object.
        /// </summary>
        /// <param name="stereotypes">List of stereotypes to check.</param>
        /// <returns>True if at least one match is found, false otherwise.</returns>
        internal override bool HasStereotype(List<string> stereotypes)
        {
            return this._classPart.HasStereotype(stereotypes);
        }

        /// <summary>
        /// The method checks whether the given stereotype is owned by the Object.
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
        /// Updates Object annotation.
        /// </summary>
        /// <param name="text">The annotation text to write.</param>
        internal override void SetAnnotation(string text)
        {
            this._classPart.SetAnnotation(text);
        }

        /// <summary>
        /// Update the alias name of the Object.
        /// </summary>
        /// <param name="newAliasName">New name to be assigned.</param>
        internal override void SetAliasName(string newAliasName)
        {
            this._classPart.SetAliasName(newAliasName);
            this._aliasName = newAliasName;
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
        /// Update the name of the Object.
        /// </summary>
        /// <param name="newName">New name to be assigned.</param>
        internal override void SetName(string newName)
        {
            this._classPart.SetName(newName);
            this._name = newName;
        }

        /// <summary>
        /// Returns the run-time state of the object, which is represented by a list of properties and their current value.
        /// The state only returns properties that have a value, empty ones are skipped!
        /// The existing run-time state of an object can be deleted by passing 'null' as a value, e.g. myObject.RuntimeState = null;
        /// </summary>
        /// <exception cref="InvalidOperationException">Unable to retrieve run-time state or invalid classifier.</exception>
        /// <exception cref="ArgumentException">New run-time state is invalid.</exception>
        internal override void SetRuntimeState(List<Tuple<string, string>> runtimeState)
        {
            const string VarStartToken = "@VAR;";
            const string VarEndToken = "@ENDVAR;";
            const string NameToken = "Variable=";
            const string ValueToken = ";Value=";

            MEClass classifier = this._classPart.Classifier > 0 ? new MEClass(this._classPart.Classifier) : null;
            if (classifier == null)
            {
                string message = "Object '" + this._name + "' does not have a valid classifier!";
                Logger.WriteError(message);
                throw new InvalidOperationException(message);
            }

            string newState = string.Empty;
            if (runtimeState != null)
            {
                foreach (Tuple<string,string> stateElement in runtimeState)
                {
                    if (classifier.HasAttribute(stateElement.Item1))
                    {
                        newState += VarStartToken + NameToken + stateElement.Item1 + ValueToken + stateElement.Item2 + VarEndToken;
                    }
                    else
                    {
                        string message = "Attribute '" + stateElement.Item1 + "' is not an attribute of classifier '" + classifier.Name + "'!";
                        Logger.WriteError(message);
                        throw new ArgumentException(message);
                    }
                }
            }
            Logger.WriteInfo("SparxEA.Model.EAMEIObject.SetRuntimeState >> Created state variable: '" + newState + "'...");
            this._classPart.RunState = newState;
        }

        /// <summary>
        /// Set the tag with specified name to the specified value for the current object. 
        /// If the tag could not be found, nothing will happen, unless the 'createIfNotExists' flag is set to true, in which case the tag will
        /// be created.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="tagValue">Value of the tag.</param>
        /// <param name="createIfNotExist">When set to true, create the tag if it's not there.</param>
        internal override void SetTag(string tagName, string tagValue, bool createIfNotExist)
        {
            this._classPart.SetTag(tagName, tagValue, createIfNotExist);
        }

        /// <summary>
        /// Returns the major and minor version of the object.
        /// </summary>
        /// <returns>Major and minor version.</returns>
        internal override void SetVersion(Tuple<int, int> version)
        {
            this._classPart.SetVersion(version);
        }
    }
}
