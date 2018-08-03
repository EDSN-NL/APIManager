using System;
using System.Collections.Generic;
using Framework.Logging;
using Framework.Util.SchemaManagement;
using Framework.Util.SchemaManagement.XML;
using Framework.Util.SchemaManagement.JSON;
using Framework.Model;
using Framework.Context;
using Framework.Exceptions;
using Plugin.Application.CapabilityModel.ASCIIDoc;

namespace Plugin.Application.CapabilityModel.SchemaGeneration
{
    /// <summary>
    /// This is the 'second part' of the SchemaProcessor Class definition, which contains all the code responsible for parsing a Class Hierarchy and
    /// transforming this into an XML Schema definition.
    /// </summary>
    internal partial class SchemaProcessor: CapabilityProcessor
    {
        // Configuration properties used by this module...
        private const string _SequenceKeyTag                        = "SequenceKeyTag";
        private const string _ChoiceKeyTag                          = "ChoiceKeyTag";
        private const string _NillableTag                           = "NillableTag";
        private const string _BDTUnionStereotype                    = "BDTUnionStereotype";
        private const string _HierarchyOffset                       = "HierarchyOffset";
        private const string _PrimitiveDataTypeStereotype           = "PrimitiveDataTypeStereotype";
        private const string _AssociationStereotype                 = "AssociationStereotype";
        private const string _MessageAssociationStereotype          = "MessageAssociationStereotype";

        // Static properties that are used across a series of schema generation operations...
        private ClassCacheSlt _cache;                               // Cache singleton, used to administer processed entities across builds.
        private static string _majorVersion = string.Empty;         // Major version to be used for all capabilities.
        private static string _buildNumber = string.Empty;          // Build number to be used for all capabilities.
        private static string _operationalStatus = string.Empty;    // Service operational status to be used by all capabilities.

        /// <summary>
        /// Processes all associations in the given class (as long as these are of type 'Association'). For each association, the 
        /// 'processClass' method is called, which in turn will process attributes and associations of that class, etc.
        /// The function keeps track of FQN's of all classes that already have been processed in order to avoid duplicate work.
        /// As a result, the method returns the list of all Association objects (i.e. child elements) for the specified element.
        /// </summary>
        /// <param name="node">The element to process.</param>
        /// <param name="sequenceBase">Reflects position of attribute set in a classhierarchy, used to properly offset attributes within the hierarchy.</param>
        /// <param name="attribList">Since associations could in fact result in additional attributes (e.g. use of Unions), we need a pointer to the attribute list for the current class.</param>
        /// <param name="inCommonSchema">Set to 'true' if node is defined in Common Schema.</param>
        /// <param name="unqualifiedNodeName">Identifies the name under which the node type is registered in the schema (unique type key).</param>
        /// <param name="nodeDocScope">Identifies the documentation scope of the current class (can be different from schema scope).</param>
        /// <returns>List of all Association objects for the element, is empty in case of errors (or no associations).</returns>
        /// <exception cref="IllegalCardinalityException">In case of destination role cardinalities.</exception>
        private List<SchemaAssociation> ProcessAssociations(MEClass node, bool inCommonSchema, int sequenceBase, ref List<SchemaAttribute> attribList, string unqualifiedNodeName, ClassifierContext.DocScopeCode nodeDocScope)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.ProcessAssociations >> Processing class: " + node.Name + "...");
            var associationList = new List<SchemaAssociation>();
            ContextSlt context = ContextSlt.GetContextSlt();
            string associationStereotype = context.GetConfigProperty(_AssociationStereotype);
            string msgAssociationStereotype = context.GetConfigProperty(_MessageAssociationStereotype);
            OperationDocContext.Chapter currentChapter = this._useDocContext ? this._currentOperationDocContext.ActiveChapter : OperationDocContext.Chapter.Unknown;

            try
            {
                foreach (MEAssociation assoc in node.AssociationList)
                {
                    // We are ONLY processing associations of the correct type...
                    if (assoc.TypeOfAssociation == MEAssociation.AssociationType.Composition ||
                        assoc.TypeOfAssociation == MEAssociation.AssociationType.MessageAssociation ||
                        assoc.TypeOfAssociation == MEAssociation.AssociationType.Aggregation ||
                        assoc.TypeOfAssociation == MEAssociation.AssociationType.Association)
                    {
                        MEClass target = assoc.Destination.EndPoint;
                        string associationName = (assoc.Destination.AliasRole != string.Empty) ? assoc.Destination.AliasRole : assoc.Destination.Role;

                        Tuple<int, int> cardinality = assoc.GetCardinality(MEAssociation.AssociationEnd.Destination);
                        if (cardinality.Item1 == -1)
                        {
                            // In case of errors, we throw an exception, no need to write an error to the log, that has already been done by the Association.
                            string message = "Cardinality format error in association: '" + assoc.Source.EndPoint.Name + "-->" + assoc.Destination.EndPoint.Name + "'!";
                            if (this._panel != null) this._panel.WriteError(this._panelIndex + 2, message);
                            throw new IllegalCardinalityException(message);
                        }

                        // Note: Sequence- & Choice key tags are on the Association itself, NOT on an endpoint!
                        string choiceKey = assoc.GetTag(context.GetConfigProperty(_ChoiceKeyTag), MEAssociation.AssociationEnd.Association);
                        string stringSequenceKey = assoc.GetTag(context.GetConfigProperty(_SequenceKeyTag), MEAssociation.AssociationEnd.Association);
                        int sequenceKey = (stringSequenceKey != string.Empty) ? Convert.ToInt32(stringSequenceKey) : 0;
                        sequenceKey += sequenceBase;

                        // Let's check whether the target of the association is a Union. In this case, we're not dealing with a 'real' association but with a specialised
                        // Data Type! This requires different processing and results in an Attribute to be added to the attribute-list of the source class!
                        if (target.HasStereotype(context.GetConfigProperty(_BDTUnionStereotype)))
                        {
                            Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.ProcessAssociations >> Found association with Union '" + target.Name + "'.");
                            ContentAttribute newAttrib = ProcessUnion(node, target, associationName, cardinality, choiceKey, sequenceKey);
                            if (newAttrib != null) attribList.Add(newAttrib);
                        }
                        else
                        {
                            Tuple<ClassifierContext.ScopeCode, ClassifierContext.DocScopeCode> scope = ClassifierContext.GetScope(target, this._commonSchema != null, this._commonDocContext != null);
                            ClassifierContext.DocScopeCode docScope = scope.Item2;
                            ClassifierContext.ScopeCode schemaScope = scope.Item1;

                            // Make sure that our association has a valid name, even if role happens to be empty!
                            // If our class type name ends with 'Type', we remove this...
                            if (associationName == string.Empty)
                            {
                                Logger.WriteWarning("Association to element '" + target.Name + "' has no name, using target name instead!");
                                if (this._panel != null) this._panel.WriteWarning(this._panelIndex + 2, "Association to element '" + target.Name + "' has no name, using target name instead!");
                                associationName = (target.Name.EndsWith("Type")) ? target.Name.Substring(0, target.Name.LastIndexOf("Type")) : target.Name;
                            }

                            // We need to create a unique key for this particular class, independent of the class name. Names might be duplicated across different
                            // messages with different scope identifiers. Thus, we create a prefix that is scope dependent.....
                            string classKeyToken;
                            if (schemaScope == ClassifierContext.ScopeCode.Interface)    classKeyToken = this._commonSchema.NSToken;
                            else if (schemaScope == ClassifierContext.ScopeCode.Message) classKeyToken = this._schema.NSToken + "_" + this._currentCapability.AssignedRole;
                            else if (schemaScope == ClassifierContext.ScopeCode.Remote)  classKeyToken = "remote_";
                            else                                                         classKeyToken = this._schema.NSToken;
                            classKeyToken += ":" + target.Name;
                            Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.ProcessAssociations >> Association stored with key: " + classKeyToken);

                            string qualifiedTargetName;
                            if (this._panel != null) this._panel.WriteInfo(this._panelIndex + 2, "Processing associated class: " + target.Name + "...");

                            if (this._cache.HasClassKey(classKeyToken))
                            {
                                qualifiedTargetName = this._cache.GetQualifiedClassName(classKeyToken);
                                Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.ProcessAssociations >> Retrieved from cache an association with: " + qualifiedTargetName);
                            }
                            else
                            {
                                // It is the responsibility of processClass to add the class name to the classCache in order to avoid loops!
                                // The qualified target name aleady contains the appropriate namespace token and name formatted according to scope...
                                qualifiedTargetName = ProcessClass(target, classKeyToken, scope);
                                Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.ProcessAssociations >> Done processing association with: " + qualifiedTargetName);
                            }

                            ChoiceGroup choiceGroup = null;
                            if (choiceKey != string.Empty)
                            {
                                try
                                {
                                    // We found a choice key. This should consist of a tuple: <groupID>:<sequenceID>.
                                    // The groupID can optionally contain a cardinality for the choice: <groupID>[low..high]
                                    // On errors, we pretend that the choice does not exist.
                                    Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.ProcessAssociations >> Detected choice key: " + choiceKey);
                                    string groupID = choiceKey.Substring(0, choiceKey.IndexOf(':'));
                                    string sequenceID = choiceKey.Substring(choiceKey.IndexOf(':') + 1);
                                    choiceGroup = new ChoiceGroup(groupID, sequenceID);
                                }
                                catch
                                {
                                    if (this._panel != null) this._panel.WriteError(this._panelIndex + 2, "Illegal 'choice group' detected in association FROM element '" +
                                                                                    node.Name + "' TO element '" + target.Name + "'!");
                                }
                            }

                            string unqualifiedTargetName = qualifiedTargetName.Substring(qualifiedTargetName.IndexOf(':') + 1);     // Remove namespace token (if present).
                            bool targetInCommonSchema = ((schemaScope == ClassifierContext.ScopeCode.Remote) || (schemaScope == ClassifierContext.ScopeCode.Interface));
                            Schema targetSchema = targetInCommonSchema ? this._commonSchema: this._schema;
                            if (targetSchema is XMLSchema)
                            {
                                string classNSName = targetInCommonSchema ? this._commonSchema.SchemaNamespace : null;
                                associationList.Add(new XMLAssociation((XMLSchema)targetSchema, associationName, unqualifiedTargetName, sequenceKey, choiceGroup,
                                                                       cardinality, assoc.GetDocumentation(MEAssociation.AssociationEnd.Destination), classNSName));
                            }
                            else
                            {
                                JSONSchema classSchema = (targetInCommonSchema) ? (JSONSchema)this._commonSchema : null;
                                associationList.Add(new JSONAssociation((JSONSchema)targetSchema, associationName, unqualifiedTargetName, sequenceKey, choiceGroup,
                                                                        cardinality, classSchema));

                            }
                            
                            // Add the association to the correct documentation context. Make sure to activate our original class, since recursive
                            // parsing will have changed it!
                            if (this._useDocContext)  // We don't always have documentation context enabled!!
                            {
                                string sourceScope = string.Empty;
                                string contextID = docScope == ClassifierContext.DocScopeCode.Common ? this._commonDocContext.ContextID : this._currentOperationDocContext.ContextID;
                                string targetNSToken = targetSchema.NSToken;
                                if (nodeDocScope == ClassifierContext.DocScopeCode.Common)
                                {
                                    this._commonDocContext.SwitchClass(unqualifiedNodeName, null, false);
                                    sourceScope = "Common:";
                                    this._commonDocContext.AddAssociation(associationName, unqualifiedTargetName, contextID,
                                                                          cardinality, assoc.GetAnnotation(MEAssociation.AssociationEnd.Destination));
                                }
                                else
                                {
                                    sourceScope = (targetInCommonSchema ? (this._currentOperationDocContext.Name + ":") :string.Empty) + this._currentCapability.Name + ".";
                                    this._currentOperationDocContext.SwitchClass(currentChapter, unqualifiedNodeName, null, false);
                                    this._currentOperationDocContext.AddAssociation(associationName, unqualifiedTargetName, contextID,
                                                                                    cardinality, assoc.GetAnnotation(MEAssociation.AssociationEnd.Destination));
                                }

                                // The documentation XREF must be added to the documentation context of the TARGET class...
                                if (docScope == ClassifierContext.DocScopeCode.Common)
                                {
                                    this._commonDocContext.AddXREF(unqualifiedTargetName, 
                                                                   ((nodeDocScope == ClassifierContext.DocScopeCode.Common) ? this._commonDocContext.ContextID: this._currentOperationDocContext.ContextID), 
                                                                   sourceScope, unqualifiedNodeName);
                                }
                                else this._currentOperationDocContext.AddXREF(unqualifiedTargetName, this._currentOperationDocContext.ContextID, sourceScope, unqualifiedNodeName);
                            }
                            Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.ProcessAssociations >> Created association.");
                        }
                    }
                }
                this._lastError = string.Empty;
            }
            catch (Exception exc)
            {
                string schemaError = this._schema.LastError;
                string message = (schemaError != string.Empty) ? schemaError + " -> " + exc.Message : exc.Message;
                Logger.WriteError("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.ProcessAssociations >> Caught an exception: " + message);
                if (this._panel != null) this._panel.WriteError(this._panelIndex + 2, "Caught an exception:" + Environment.NewLine + message);
                this._lastError = message;
            }
            return associationList;
        }

        /// <summary>
        /// This function is used by 'ProcessClass' to process the attributes of a given element.
        /// For each attribute, the function collects metadata along the classifier hierarchy and finally creates an Attribute object of the appropriate 
        /// type. The function also creates the associated type definitions in either the common schema (if specified) or in the current schema. Finally, the 
        /// list of attribute definitions for the specified class is returned.
        /// 
        /// By definition, the type of an attribute MUST (eventually) be a CDT type. Therefore, they are all considered 'common' definitions and will thus be 
        /// stored in the common schema if possible. The method parses the attribute classifier and follows the type hierarchy until it has dicovered both 
        /// the CDT typename as well as the PRIM datatype that is associated with that typename.
        /// </summary>
        /// <param name="node">The class for which we're collecting attributes.</param>
        /// <param name="sequenceBase">Reflects position of attribute set in a classhierarchy, used to properly offset attributes within the hierarchy.</param>
        /// <param name="inCommonSchema">Set to 'true' is node is defined in Common Schema.</param>
        /// <param name="unqualifiedNodeName">Identifies the name under which the node type is registered in the schema (unique type key).</param>
        /// <param name="nodeDocScope">Identifies the documentation scope of the current class (can be different from schema scope).</param>
        /// <returns>List of attribute declarations for the element or empty list in case of errors (or no attributes).</returns>
        /// <exception cref="IllegalCardinalityException">In case of illegal attribute cardinalities.</exception>
        private List<SchemaAttribute> ProcessAttributes(MEClass node, bool inCommonSchema, int sequenceBase, string unqualifiedNodeName, ClassifierContext.DocScopeCode nodeDocScope)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.ProcessAttributes >> Processing class: " + node.Name + "...");
            this._lastError = string.Empty;
            ContextSlt context = ContextSlt.GetContextSlt();
            DocManagerSlt docMgr = DocManagerSlt.GetDocManagerSlt();
            var attribList = new List<SchemaAttribute>();

            // Attribute documentation should go to the documentation node of the parent class.
            DocContext attribDocCtx = this._currentOperationDocContext;
            if (nodeDocScope == ClassifierContext.DocScopeCode.Common) attribDocCtx= this._commonDocContext;

            try
            {
                foreach (MEAttribute attribute in node.Attributes)
                {
                    // Attribute classifiers are, by definition, BDT's or CDT's. These are 'common' definitions by default and thus are all stored in the common
                    // schema in case this is specified. If no common schema is passed, all classifiers go the to current schema...
                    MEDataType classifier = attribute.Classifier;

                    Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.ProcessAttributes >> Processing attribute: " + attribute.Name + " with classifier: " + classifier.Name);
                    if (this._panel != null) this._panel.WriteInfo(this._panelIndex + 2, "Processing attribute: '" + attribute.Name + "' with classifier: '" + classifier.Name + "'..");

                    // Assures that we have an attribute classifier definition and we know where to store it and where to document it...
                    ClassifierContext classifierCtx = ProcessClassifier(classifier);
                    DocContext classifierDocCtx = this._commonDocContext;
                    if (!classifierCtx.IsInCommonDocContext) classifierDocCtx = this._currentOperationDocContext;
                    Schema targetSchema = (classifierCtx.IsInCommonSchema) ? this._commonSchema : this._schema;

                    string attributeName = (attribute.AliasName != string.Empty) ? attribute.AliasName : attribute.Name;
                    if (attribute.Type == ModelElementType.Supplementary)
                    {
                        // This is a supplementary attribute
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.ProcessAttributes >> Supplementary attribute.");
                        if (classifierCtx.ContentType == ClassifierContext.ContentTypeCode.Simple || classifierCtx.ContentType == ClassifierContext.ContentTypeCode.Enum)
                        {
                            string defaultVal = attribute.DefaultValue;
                            bool isOptional = attribute.IsOptional;
                            bool classifierInSchemaNS = classifierCtx.SchemaScope != ClassifierContext.ScopeCode.Remote;

                            SupplementaryAttribute suppAttrib = null;
                            if (targetSchema is XMLSchema)
                            {
                                suppAttrib = new XMLSupplementaryAttribute((XMLSchema)targetSchema, attributeName, classifierCtx.Name, classifierInSchemaNS, isOptional,
                                                                           attribute.GetDocumentation(), defaultVal, string.Empty);
                            }
                            else
                            {
                                suppAttrib = new JSONSupplementaryAttribute((JSONSchema)targetSchema, attributeName, classifierCtx.Name, isOptional,
                                                                            attribute.GetDocumentation(), defaultVal, string.Empty);
                            }
                            attribList.Add(suppAttrib);
                            string targetNSToken = targetSchema.NSToken;
                            Tuple<int, int> cardinality = new Tuple<int, int>(isOptional ? 0 : 1, 1);
                            if (this._useDocContext) classifierDocCtx.AddAttribute(attributeName, AttributeType.Supplementary, classifierCtx.Name, targetNSToken, cardinality, attribute.Annotation);
                        }
                        else
                        {
                            this._lastError = "Attribute '" + attribute.Name + "' with classifier '" + classifier.Name +
                                              "' has incompatible content type for a Supplementary Attribute (must be Primitive or Enumeration)!";
                            Logger.WriteError("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.ProcessAttributes >> " + this._lastError);
                            if (this._panel != null) this._panel.WriteError(this._panelIndex + 2, "this._lastError");
                        }
                    }
                    else if (attribute.Type == ModelElementType.Facet)
                    {
                        // This is a facet-type attribute. These are currently not supported on 'regular' class attributes!
                        Logger.WriteWarning("Facet-type attribute '" + attribute.Name + "' currently not supported!");
                        this._panel.WriteWarning(this._panelIndex + 2, "Facet-type attribute '" + attribute.Name + "' currently not supported!");
                    }
                    else
                    {
                        // All others are considered 'regular' attributes.
                        Tuple<int, int> cardinality = attribute.Cardinality;
                        if (cardinality.Item1 == -1)
                        {
                            // In case of errors, we throw an exception, no need to write an error to the log, that has already been done by the Attribute.
                            string message = "Cardinality format error in: '" + node.Name + "." + attribute.Name + "'!";
                            if (this._panel != null) this._panel.WriteError(this._panelIndex + 2, message);
                            throw new IllegalCardinalityException(message);
                        }
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.ProcessAttributes >> Content attribute, default= '" + attribute.DefaultValue +
                                         "', minOcc = '" + cardinality.Item1 + "', maxOcc = '" + cardinality.Item2 + "'.");
                        string choiceKey = attribute.GetTag(context.GetConfigProperty(_ChoiceKeyTag));
                        ChoiceGroup choiceGroup = null;
                        if (choiceKey != string.Empty)
                        {
                            try
                            {
                                // We found a choice key. This should consist of a tuple: <groupID>:<sequenceID>.
                                // GroupID might have an optional cardinality (<groupID>[n..m]).
                                // On errors, we pretend that the choice does not exist.
                                Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.ProcessAttributes >> Detected choice key: " + choiceKey);
                                string groupID = choiceKey.Substring(0, choiceKey.IndexOf(':'));
                                string sequenceID = choiceKey.Substring(choiceKey.IndexOf(':') + 1);
                                choiceGroup = new ChoiceGroup(groupID, sequenceID);
                            }
                            catch
                            {
                                if (this._panel != null) this._panel.WriteError(this._panelIndex + 2, "Illegal Choice Group detected in attribute '" + 
                                                                                attribute.Name + "' of class '" + node.Name + "'!");
                            }
                        }

                        // The sequence key for an attribute is equal to the position within the class (as shown on the diagram) times 100. The latter
                        // is done so that we have space to 'squeeze' other elements in between (such as classes and associations).
                        // Since numbering starts at 0, we add 1 before multiplication so that we now start at 100 instead.
                        // For those rare occasions where we have to deal with class hierarchies, we also add a base number that reflects the position
                        // of each attribute within the entire hierarchy.
                        int sequenceKey = attribute.SequenceKey + sequenceBase;
                        ContentAttribute contentAttrib = null;
                        string nillableTag = attribute.GetTag(context.GetConfigProperty(_NillableTag));
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.ProcessAttributes >> Read tag '" + context.GetConfigProperty(_NillableTag) + "' and got: '" + nillableTag + "'...");
                        bool isNillable = !string.IsNullOrEmpty(nillableTag) && string.Compare(nillableTag, "true", true) == 0;
                        Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.ProcessAttributes >> Attribute '" + 
                                         attribute.Name + "' has sequence: " + sequenceKey + " and nillable-indicator: " + isNillable);
                        if (targetSchema is XMLSchema)
                        {
                            contentAttrib = new XMLContentAttribute((XMLSchema)targetSchema, attributeName, classifierCtx.Name, sequenceKey, choiceGroup, cardinality,
                                                                    attribute.GetDocumentation(), attribute.DefaultValue, string.Empty, isNillable);
                        }
                        else
                        {
                            contentAttrib = new JSONContentAttribute((JSONSchema)targetSchema, attributeName, classifierCtx.Name, sequenceKey, choiceGroup, cardinality,
                                                                     attribute.GetDocumentation(), attribute.DefaultValue, string.Empty, isNillable);
                        }
                        attribList.Add(contentAttrib);
                        if (this._useDocContext)
                        {
                            string classifierCtxID = classifierCtx.IsInCommonDocContext ? this._commonDocContext.ContextID : this._currentOperationDocContext.ContextID;
                            attribDocCtx.AddAttribute(attributeName, AttributeType.Attribute, classifierCtx.Name, classifierCtxID, cardinality, attribute.Annotation);
                        }
                    }

                    // The documentation XREF must be added to the documentation context of the TARGET classifier...
                    if (this._useDocContext)
                    {
                        string sourceScope = "Common:";
                        if (nodeDocScope != ClassifierContext.DocScopeCode.Common)
                        {
                            if (classifierCtx.DocScope == ClassifierContext.DocScopeCode.Local)
                            {
                                sourceScope = this._currentCapability.Name + ".";
                            }
                            else sourceScope = this._currentOperationDocContext.Name + ":" + this._currentCapability.Name + ".";
                        }

                        if (classifierCtx.IsInCommonDocContext)
                        {
                            // In this case, we need the ContextID (and scope) of the SOURCE, not the Target....
                            this._commonDocContext.AddXREF(classifierCtx.Name,
                                                           ((nodeDocScope == ClassifierContext.DocScopeCode.Common) ? this._commonDocContext.ContextID : this._currentOperationDocContext.ContextID),
                                                           sourceScope, unqualifiedNodeName);
                        }
                        else this._currentOperationDocContext.AddXREF(classifierCtx.Name, this._currentOperationDocContext.ContextID, sourceScope, unqualifiedNodeName);
                    }
                }
            }
            catch (Exception exc)
            {
                string schemaError = this._schema.LastError;
                string message = (schemaError != string.Empty) ? schemaError + " -> " + exc.Message : exc.Message;
                Logger.WriteError("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.ProcessAttributes >> Caught an exception: " + message);
                if (this._panel != null) this._panel.WriteError(this._panelIndex + 2, "Caught an exception: " + Environment.NewLine + message);
                this._lastError = message;
            }
            return attribList;
        }

        /// <summary>
        /// This function processes a single class. The function retrieves the class hierarchy and subsequently parses the attribute- and association 
        /// list of that hierarchy. If a common schema is specified, this is taken into account when processing the 
        /// items in the class. Whether or not the class is added to the current- or the local schema depends on the "scope" argument. 
        /// The function recursively processes all associated classes, so typically it is called only once for the root message assembly class in a message.
        /// The qualified name of the class is returned (as in: nsToken:classname), even if the class is defined in the local schema!
        /// </summary>
        /// <param name="currentClass">The current Class to be parsed.</param>
        /// <param name="classKeyToken">This is a unique identification of this class in the current processing context and is used to detect which 
        /// classes have been processed already.</param>
        /// <param name="scope">Defines in which schema and documentation context we're going to put this class.</param>
        /// <returns>Fully qualified class name (token:name) or empty string in case of errors.</returns>
        private string ProcessClass(MEClass currentClass, string classKeyToken, Tuple<ClassifierContext.ScopeCode, ClassifierContext.DocScopeCode> scope)
        {
            if (currentClass == null)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.processClass >> No class defined, abort!");
                return string.Empty;
            }
            Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.processClass >> Processing class: " + currentClass.Name + "...");
            ContextSlt context = ContextSlt.GetContextSlt();

            try
            {
                if (this._panel != null) this._panel.WriteInfo(this._panelIndex + 1, "Processing class: " + currentClass.Name);

                SortedList<uint, MEClass> classHierarchy = currentClass.GetHierarchy();
                var attribList = new List<SchemaAttribute>();
                var associationList = new List<SchemaAssociation>();
                ClassifierContext.ScopeCode schemaScope = scope.Item1;
                ClassifierContext.DocScopeCode docScope = scope.Item2;
                Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.processClass >> Schema scope: '" + 
                                 schemaScope + "', Doc. scope: '" + docScope + "'.");

                // Determine number of levels in my Class hierarchy, this defines the sequence number offsets...
                int step = Int16.Parse(context.GetConfigProperty(_HierarchyOffset));
                int seqBase = (classHierarchy.Count - 1) * step;
                Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.processClass >> Sequence base = " + seqBase + " stepping down by: " + step);

                // We do not have to test commonSchema against null since this has already been taken care of when we determined the schema scope.
                string qualifiedClassName = GetQualifiedClassName(currentClass, schemaScope);
                string unqualifiedName = qualifiedClassName.Substring(qualifiedClassName.IndexOf(':') + 1);

                Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.processClass >> Class: " + 
                                 currentClass.Name + " has received Qualified Name: " + qualifiedClassName + "...");
                if (this._panel != null) this._panel.WriteInfo(this._panelIndex + 1, "Class: " + currentClass.Name + " has been assigned Qualified Name: " + qualifiedClassName);

                // Safety catch for loops. Should not happen since inserting the name in the cache down here should prevent this method to be called a second time for the
                // same class. However, when we use the Schema Processor to generate JSON Schema's for OpenAPI, we call the processor on individual classes and it might
                // happen that the parser meets the same class multiple times in the same context. In those cases, simply abort further processing since it has already
                // been processed before!
                if (this._cache.HasClassKey(classKeyToken)) return qualifiedClassName;
                else this._cache.AddQualifiedClassName(classKeyToken, qualifiedClassName);     // Make sure to remember that we have processed this class!

                // Prepare documentation. It is important to do this BEFORE we're adding attributes and associations so that the documentation context
                // is properly set-up for the current class.
                if (this._useDocContext)
                {
                    // First of all, for documentation purposes we must check whether the class we're processing has contents 'somewhere' in its hierarchy
                    // Contents meaning: one or more local attributes and/or associations originating from the class. If not, we have to suppress some output!
                    bool isEmpty = true;
                    foreach (MEClass node in classHierarchy.Values)
                    {
                        if (node.HasContents())
                        {
                            isEmpty = false;
                            break;
                        }
                    }

                    if (docScope == ClassifierContext.DocScopeCode.Common)
                    {
                        // Since we have already tested the availability of commonDocContext when determining the documentation scope, we can be sure
                        // that commonDocContext is available in case of 'Common' doc context!
                        this._commonDocContext.SwitchClass(unqualifiedName, currentClass.Annotation, isEmpty);
                    }
                    else
                    {
                        // The class can have either local documentation scope (belongs to one specific message) 
                        // or common scope (common across all messages in a single operation).
                        var chapter = (docScope == ClassifierContext.DocScopeCode.Local) ? OperationDocContext.Chapter.MessageClasses :
                                                                                           OperationDocContext.Chapter.CommonClasses;
                        this._currentOperationDocContext.SwitchClass(chapter, unqualifiedName, currentClass.Annotation, isEmpty);
                    }
                }

                bool inCommonSchema = ((schemaScope == ClassifierContext.ScopeCode.Remote) || (schemaScope == ClassifierContext.ScopeCode.Interface));
                foreach (MEClass node in classHierarchy.Values)                           // Process one node at a time (node == element in class hierarchy).
                {
                    attribList.AddRange(ProcessAttributes(node, inCommonSchema, seqBase, unqualifiedName, docScope));
                    associationList.AddRange(ProcessAssociations(node, inCommonSchema, seqBase, ref attribList, unqualifiedName, docScope));
                    seqBase -= step;
                }

                Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.processClass >> Adding class '" + unqualifiedName + "' to Schema...");
                Schema targetSchema = inCommonSchema? this._commonSchema : this._schema;
                if (targetSchema.AddABIEType(unqualifiedName, currentClass.GetDocumentation(), attribList, associationList))
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.processClass >> Returning FQN: " + qualifiedClassName);
                    return qualifiedClassName;
                }
                else
                {
                    Logger.WriteError("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.processClass >> Adding ABIE type failed because: " + this._schema.LastError);
                    if (this._panel != null) this._panel.WriteError(this._panelIndex + 1, "Error adding complex type:" + Environment.NewLine + this._schema.LastError);
                    return string.Empty;
                }
            }
            catch (Exception exc)
            {
                string schemaError = this._schema.LastError;
                string message = (schemaError != string.Empty) ? schemaError + " -> " + exc.Message : exc.Message;
                Logger.WriteError("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.processClass >> Caught an exception: " + message);
                if (this._panel != null) this._panel.WriteError(this._panelIndex + 1, "Caught an exception:" + Environment.NewLine + message);
                this._lastError = message;
            }
            return string.Empty;
        }

        /// <summary>
        /// This helper function is called for each attribute classifier that needs a definition. It checks whether the classifier has already been
        /// processed before and if so, the associated type designator is returned from the cache. If it has not been processed before, a unique key 
        /// is constructed (consisting of the namespace token and the classifier name) and the classifier definition is created using 'defineClassifier'. 
        /// Subsequently, the type designator is stored in the classifier cache using the unique key in order to mark this classifier as 'resolved'.
        /// </summary>
        /// <param name="classifier">The classifier element.</param>
        /// <returns>Classifier Context</returns>
        private ClassifierContext ProcessClassifier(MEDataType classifier)
        {
            // In case of primitive types, we do not have to create an explicit type definition since these will be translated to "built-in" XSD primitives!
            if (classifier.HasStereotype(ContextSlt.GetContextSlt().GetConfigProperty(_PrimitiveDataTypeStereotype)))
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.ProcessClassifier >> This is a PRIM type, do nothing!");
                var primScope = new Tuple<ClassifierContext.ScopeCode, ClassifierContext.DocScopeCode>
                (
                    ((this._commonSchema != null) ? ClassifierContext.ScopeCode.Interface : ClassifierContext.ScopeCode.Operation),
                    ((this._commonDocContext != null) ? ClassifierContext.DocScopeCode.Common : ClassifierContext.DocScopeCode.Local)
                );
                return new ClassifierContext(ClassifierContext.ContentTypeCode.Simple, classifier.Name, primScope);
            }

            // We need to create a unique key for this particular classifier, independent of the classifier name. Names might be duplicated across
            // different messages with different scope identifiers. Thus, we create a prefix that is scope dependent.....
            Tuple<ClassifierContext.ScopeCode, ClassifierContext.DocScopeCode> scope = ClassifierContext.GetScope(classifier, this._commonSchema != null, this._commonDocContext != null);
            ClassifierContext context;
            ClassifierContext.ScopeCode schemaScope = scope.Item1;
            ClassifierContext.DocScopeCode docScope = scope.Item2;

            string classifKey;
            if (schemaScope == ClassifierContext.ScopeCode.Interface)    classifKey = this._commonSchema.NSToken;
            else if (schemaScope == ClassifierContext.ScopeCode.Message) classifKey = this._schema.NSToken + "_" + this._currentCapability.AssignedRole;
            else if (schemaScope == ClassifierContext.ScopeCode.Remote)  classifKey = "remote_";
            else                                                         classifKey = this._schema.NSToken;
            classifKey += ":" + classifier.Name;
            Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.ProcessClassifier >> Processing classifier '"
                             + classifier.Name + "' with key '" + classifKey + "'...");

            if (this._cache.HasClassifierKey(classifKey))
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.ProcessClassifier >> Existing classifier, type designator retrieved from cache.");
                context = this._cache.GetClassifierContext(classifKey);
            }
            else
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.ProcessClassifier >> New classifier, going to create definition and store in cache...");
                context = DefineClassifier(classifier, scope);
                this._cache.AddClassifierContext(classifKey, context);  // Remember as processed so we do not perform any redundant work.
            }
            return context;
        }

        /// <summary>
        /// This helper method is invoked from the processAssociation method whenever an association is discovered with a Union instead of a Business Component. 
        /// The method will construct a Content Attribute using the classifier of the FIRST attribute found in the Union. This attribute will receive the name 
        /// passed to the method, which is typically the role name of the association.
        /// The method also receives the cardinality, choice-key and sequence-key retrieved from the association. It creates a classifier and an attribute in 
        /// the appropriate schema and returns the ContentAttribute object created. This has to be included in the attribute-list of the source class.
        /// </summary>
        /// <param name="source">Source class of the association, has to receive the new attribute.</param>
        /// <param name="target">The Union class.</param>
        /// <param name="attributeName">Name to be assigned to the new attribute.</param>
        /// <param name="cardinality">Cardinality of the attribute.</param>
        /// <param name="choiceKey">Choice-key if applicable (empty string otherwise)</param>
        /// <param name="sequenceKey">Sequence-key to be assigned to the attribute.</param>
        /// <returns>Created ContentAttribute object or null in case of errors.</returns>
        private ContentAttribute ProcessUnion(MEClass source, MEClass target, string attributeName, Tuple<int, int> cardinality, string choiceKey, int sequenceKey)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.ProcessUnion >> Union '" + target.Name + "' associated with '" + 
                             source.Name + "' as attribute '" + attributeName + "'...");
            try
            {
                foreach (MEAttribute attribute in target.Attributes)
                {
                    // The union MUST have ONE Attribute, which will be used as the classifier for the attribute we're going to add to the source class.
                    // If there are more attributes in here, we will IGNORE these.
                    MEDataType classifier = attribute.Classifier;

                    Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.ProcessUnion >> Going to use classifier: " + classifier.Name);
                    if (this._panel != null) this._panel.WriteInfo(this._panelIndex + 3, "Processing Union: '" + target.Name + "', using classifier: '" + classifier.Name + "'..");

                    ClassifierContext classifierCtx = ProcessClassifier(classifier); // Assures that we have a classifier definition.

                    Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.ProcessUnion >> Attribute, default= '" + attribute.DefaultValue +
                                     "', minOcc = '" + cardinality.Item1 + "', maxOcc = '" + cardinality.Item2 + "'.");
                    string defaultVal = attribute.DefaultValue;

                    ChoiceGroup choiceGroup = null;
                    if (choiceKey != string.Empty)
                    {
                        try
                        {
                            // We found a choice key. This should consist of a tuple: <groupID>:<sequenceID>....
                            // On errors, we pretend that the choice does not exist.
                            Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.ProcessUnion >> Detected choice key: " + choiceKey);
                            string groupID = choiceKey.Substring(0, choiceKey.IndexOf(':'));
                            string sequenceID = choiceKey.Substring(choiceKey.IndexOf(':') + 1);
                            choiceGroup = new ChoiceGroup(groupID, sequenceID);
                        }
                        catch
                        {
                            if (this._panel != null) this._panel.WriteError(this._panelIndex + 3, "Illegal Choice Group detected in aassociation between '" + 
                                                                            source.Name + "' and '" + target.Name + "'!");
                        }
                    }

                    Schema targetSchema = classifierCtx.IsInCommonSchema? this._commonSchema : this._schema;
                    ContentAttribute contentAttrib = null;
                    string nillableTag = attribute.GetTag(ContextSlt.GetContextSlt().GetConfigProperty(_NillableTag));
                    bool isNillable = !string.IsNullOrEmpty(nillableTag) && string.Compare(nillableTag, "true", true) == 0;
                    if (targetSchema is XMLSchema)
                    {
                        contentAttrib = new XMLContentAttribute((XMLSchema)targetSchema, attributeName, classifierCtx.Name, sequenceKey, choiceGroup,
                                                                cardinality, attribute.GetDocumentation(), defaultVal, string.Empty, isNillable);
                    }
                    else
                    {
                        contentAttrib = new JSONContentAttribute((JSONSchema)targetSchema, attributeName, classifierCtx.Name, sequenceKey, choiceGroup,
                                                                 cardinality, attribute.GetDocumentation(), defaultVal, string.Empty, isNillable);
                    }
                    return contentAttrib;
                }
            }
            catch (Exception exc)
            {
                string schemaError = this._schema.LastError;
                string message = (schemaError != string.Empty) ? schemaError + " -> " + exc.Message : exc.Message;
                Logger.WriteError("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.ProcessUnion >> Caught an exception: " + message);
                if (this._panel != null) this._panel.WriteError(this._panelIndex + 1, "Caught an exception: " + Environment.NewLine + message);
                this._lastError = message;
            }
            return null;
        }
    }
}
