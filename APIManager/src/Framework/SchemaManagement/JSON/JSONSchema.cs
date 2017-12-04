using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using Framework.Context;
using Framework.Logging;

namespace Framework.Util.SchemaManagement.JSON
{
    internal class JSONSchema: Schema
    {
        // Configuration properties used by this module:
        private const string _JSONSchemaStdNamespace = "JSONSchemaStdNamespace";

        private JSchema _schema;                                        // The eventual (root) schema.
        private JSchema _skeletonSchema;                                // Contains the 'skeleton' schema as created by 'Initialize', to be completed by invoking 'Build'.
        private Uri _id;                                                // The Schema ID.
        private List<JSONSchema> _externalSchemas;                      // Used to store pointers to external schemas that are referenced by this particular schema.
        private SortedList<string, JSONClassifier> _classifiers;        // Contains all the classifiers defined in this schema.
        private SortedList<SortableSchemaElement, JSchema> _classes;    // Contains all referenced class (ABIE) definitions in the schema. 
        private SortedList<string, JSONPrimitiveType> _primitiveTypes;  // These are all primitive types defined for this schema.
        private SortedList<string, JSchema> _rootClasses;               // These are all root class properties, added through 'addElement'.

        /// <summary>
        /// Schema constructor.
        /// Since schemas are constructed dynamically, the constructor does not perform any operations. After construction, the schema MUST be explicitly
        /// initialized via a call to 'Initialize'. Until that has been done, object state is indeterminate.
        /// The constructor MUST be declared public since it is called by the .Net Invocation framework, which is in another assembly!
        /// </summary>
        public JSONSchema()
        {
            this._schema = null;
            this._id = null;
            this._externalSchemas = null;
            this._classes = null;
            this._classifiers = null;
            this._primitiveTypes = null;
            this._rootClasses = null;
        }

        /// <summary>
        /// Creates a new ABIE type and adds the list of content- and supplementary attributes as well as a list of associated classes to this type. 
        /// An association (ASBIE) is constructed as a property of the type of the associated class (e.g. the ABIE is considered the 'source' of the association).
        /// An ABIE thus consists of a complex type, containing a sequence of content attribute elements as well as a sequence of associated classes (ASBIE's).
        /// Since JSON Schema does not have a difference between 'Content' and 'Regular' attributes, these are treated equal, except that the names of the
        /// Supplementary attributes will be prefixed by a '@' character. This assures uniqueness, assures that they are grouped together and makes them
        /// recognizable as a supplementary attribute.
        /// Attributes will be translated to a sequence of JSON Schema Properties and each property must be of a CDT (or BDT) type that must have been 
        /// created earlier using one of the 'addxxxBBIEType' operations.
        /// Associations are created as references to the target objects, which must have been created earlier, that is, must already been stored
        /// in the schema-wide list of objects.
        /// Both Supplementary- and Content attributes are passed as a single list and correct processing is managed by the Schema implementation.
        /// If an ABIE with the specified name already exists, no processing takes place and 'true' is returned.
        /// </summary>
        /// <param name="className">The name of the business component (ABIE name).</param>
        /// <param name="annotation">Optional comment for this element (empty list in case of no comment).</param>
        /// <param name="attributeList">List of content- and supplementary attributes.</param>
        /// <param name="associationList">List of associated classes (ASBIE).</param>
        /// <returns>TRUE in case of success, FALSE on errors. If the type already exists, no action is performed and the operation returns TRUE.</returns>
        internal override bool AddABIEType(string className, List<MEDocumentation> annotation, List<SchemaAttribute> attributeList, List<SchemaAssociation> associationList)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.addABIEType >> Adding class " + className);
            var propertyList = new SortedList<SortableSchemaElement, IJSONProperty>();      // All properties of this class.
            var choiceList = new SortedList<string, JSONChoice>();                          // All choices of this class.
            var mandatoryProperties = new List<string>();                                   // Names of all properties marked 'mandatory'.
            bool result = true;

            var newType = new JSchema()
            {
                Title = className,
                Type = JSchemaType.Object,
                AllowAdditionalProperties = false
            };
            var classKey = new SortableSchemaElement(newType);

            if (!this._classes.ContainsKey(classKey))
            {
                this._classes.Add(classKey, newType);
                Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.addABIEType >> Successfully registered class '" + className + "'.");
            }
            else
            {
                Logger.WriteWarning("Framework.Util.SchemaManagement.JSON.JSONSchema.addABIEType >> Warning: duplicate class '" + className + "' in schema '" + this._id + "' skipped.");
                SetLastError(string.Empty);
                return true;
            }

            // Build a description block for the element...
            if (annotation.Count > 0)
            {
                Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.addABIEType >> Adding annotation to class...");
                string description = string.Empty;
                foreach (MEDocumentation docNode in annotation) description += docNode.BodyText + Environment.NewLine;
                newType.Description = description;
            }

            // Iterate through list of attributes (simple types) for this class...
            foreach (SchemaAttribute att in attributeList)
            {
                if (att.IsValid && att is JSONContentAttribute)
                {
                    var contentAttrib = att as JSONContentAttribute;     // Want to avoid a lot of explicit casts ;-)
                    if (att.IsMandatory) mandatoryProperties.Add(att.Name);

                    // Check whether this attribute is part of a choice group. If so, check whether we have created an instance of this
                    // choice group. If not, first create the new choice object. Finally, add the attribute to the choice.
                    if (contentAttrib.IsChoiceElement)
                    {
                        if (!choiceList.ContainsKey(contentAttrib.ChoiceGroupID))
                        {
                            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.addABIEType >> Creating new Choice with name: " + contentAttrib.ChoiceGroupID);
                            var newChoice = new JSONChoice(this, contentAttrib.ChoiceGroupID);
                            choiceList.Add(contentAttrib.ChoiceGroupID, newChoice);
                        }
                        Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.addABIEType >> Adding attribute '" + contentAttrib.Name +
                                         "' to Choice Group '" + contentAttrib.ChoiceGroupID + "'...");
                        choiceList[contentAttrib.ChoiceGroupID].AddContentAttribute(contentAttrib);
                    }
                    else
                    {
                        propertyList.Add(new SortableSchemaElement(contentAttrib), contentAttrib);
                        Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.addABIEType >> Added content attribute '" + contentAttrib.Name + "'.");
                    }
                }
                else if (att.IsValid && att is JSONSupplementaryAttribute)
                {
                    var suppAttrib = att as JSONSupplementaryAttribute;
                    if (att.IsMandatory) mandatoryProperties.Add(att.Name);
                    propertyList.Add(new SortableSchemaElement(suppAttrib), suppAttrib);
                    Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.addABIEType >> Added supplementary attribute '" + suppAttrib.Name + "'.");
                }
                else
                {
                    Logger.WriteError("Framework.Util.SchemaManagement.JSON.JSONSchema.addABIEType >> Illegal attribute type detected!");
                    result = false;
                }
            }

            // Iterate through list of associations (constructed elements) for this class...
            foreach (JSONAssociation associatedClass in associationList)
            {
                if (associatedClass.IsValid)
                {
                    if (associatedClass.IsMandatory) mandatoryProperties.Add(associatedClass.Name);
                    // Check whether this association (constructed element) is part of a choice group. If so, check whether we have created an instance of this
                    // choice object. If not, first create the new choice object. Finally, add the association to the choice.
                    if (associatedClass.IsChoiceElement)
                    {
                        if (!choiceList.ContainsKey(associatedClass.ChoiceGroupID))
                        {
                            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.addABIEType >> Creating new Choice with name: " + associatedClass.ChoiceGroupID);
                            var newChoice = new JSONChoice(this, associatedClass.ChoiceGroupID);
                            choiceList.Add(associatedClass.ChoiceGroupID, newChoice);
                        }
                        Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.addABIEType >> Adding association '" + associatedClass.ASBIEName +
                                         "' to Choice Group '" + associatedClass.ChoiceGroupID + "'...");
                        choiceList[associatedClass.ChoiceGroupID].AddAssociation(associatedClass);
                    }
                    else
                    {
                        propertyList.Add(new SortableSchemaElement(associatedClass), associatedClass);
                        Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.addABIEType >> Added association '" + associatedClass.ASBIEName + "'.");
                    }
                }
            }

            // If we have a choice, we insert this as a property. Since JSON Schema only supported ONE choice per ABIE, we generate an error if we have
            // multiple choice groups.
            if (choiceList.Count > 0)
            {
                if (choiceList.Values[0].IsValid)
                {
                    propertyList.Add(new SortableSchemaElement(choiceList.Values[0]), choiceList.Values[0]);
                    if (choiceList.Values[0].IsMandatory) mandatoryProperties.Add(choiceList.Values[0].Name);
                    Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.addABIEType >> Added choice '" + choiceList.Values[0].Name + "'.");
                }
                else
                {
                    string lastError = "Choice Group '" + choiceList.Values[0].Name + "' in class '" + className + "' only has a single element!";
                    Logger.WriteWarning("Framework.Util.SchemaManagement.JSON.JSONSchema.addABIEType >>  " + lastError);
                }

                if (choiceList.Count > 1)
                {
                    string lastError = "Class '" + className + "' defines multiple Choice Groups, JSON only supports ONE. All but first group are ignored!";
                    Logger.WriteWarning("Framework.Util.SchemaManagement.JSON.JSONSchema.addABIEType >>  " + lastError);
                }
            }

            // Now that our property list is complete (and sorted), we can copy the contents to the ABIE schema...
            foreach (IJSONProperty property in propertyList.Values)
            {
                Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.addABIEType >> Adding property '" + property.Name + "' to ABIE...");
                newType.Properties.Add(property.Name, property.JSchema);
            }
            foreach (string mandatoryProperty in mandatoryProperties) newType.Required.Add(mandatoryProperty);
            return result;
        }

        /// <summary>
        /// This function attempts to add a class to the list of classes defined in this schema. If the class is not already in there, it will be added.
        /// </summary>
        /// <param name="thisClass">The class to be added.</param>
        /// <returns>True is added successfully, FALSE if class is already in there.</returns>
        internal bool AddClass(JSchema thisClass)
        {
            SortableSchemaElement key = new SortableSchemaElement(thisClass);
            if (this._classes.ContainsKey(key)) return false;
            this._classes.Add(key, thisClass);
            return true;
        }

        /// <summary>
        /// This function attempts to add a primitive type to the list of primitive types. If the type is not already in there, it will be added.
        /// </summary>
        /// <param name="thisType">The type to be added.</param>
        /// <returns>True is added successfully, FALSE if type is already in there.</returns>
        internal bool AddClassifier(JSONClassifier classifier)
        {
            if (this._classifiers.ContainsKey(classifier.Name)) return false;
            this._classifiers.Add(classifier.Name, classifier);
            return true;
        }

        /// <summary>
        /// This operation is used to create a type definition for a complex-type Basic Business Information Entity (BBIE). These are always of type CDT (or BDT derived from CDT).
        /// The classifierName MUST be a PRIM type, so the caller must have parsed the entire class hierarchy until arrival at PRIM level.
        /// Note that we're in fact creating classifiers here (these are definitions used as classifiers in attributes of other elements). 
        /// The Classifier registers itself with the schema, so we don't have to do this here, making this a very simple function.
        /// </summary>
        /// <param name="classifierName">The name of the classifier to be constructed. The name must be unique within the current namespace. If not, it will be skipped!</param>
        /// <param name="primType">Associated PRIM type</param>
        /// <param name="annotation">Optional comment for the classifier (empty list in case of no comment).</param>
        /// <param name="attribList">Optional list of attributes to be assigned to this type</param>
        /// <param name="facetList">Optional list of facets that must be applied to the primitive type of the classifier.</param>
        /// <returns>TRUE in case of success, FALSE on errors. If the type already exists, no action is performed and the operation returns TRUE.</returns>
        internal override bool AddComplexClassifier(string classifierName, string primType, List<MEDocumentation> annotation, List<SupplementaryAttribute> attribList, List<Facet> facetList)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.addComplexClassifier >> Adding classifier '" + classifierName + "' of type '" + primType + "'...");
            JSONClassifier newClassifier = new JSONClassifier(this, classifierName, primType, annotation, attribList, facetList);
            return true;
        }

        /// <summary>
        /// This operation is used to create a type definition for an enumerated-type Basic Business Information Entity (BBIE). 
        /// Enumerations might have attributes (which is schema implementation dependent) but no facets (other then the enumeration values).
        /// </summary>
        /// <param name="classifierName">BBIE classifier name. The name must be unique within the current namespace. If not, it will be skipped!</param>
        /// <param name="annotation">Optional classifier documentation, empty list in case of no comments.</param>
        /// <param name="attribList">Optional list of attributes to be assigned to this type</param>
        /// <param name="enumList">The list of enumeration values.</param>
        /// <returns>TRUE in case of success, FALSE on errors. If the type already exists, no action is performed and the operation returns TRUE.</returns>
        internal override bool AddEnumClassifier(string classifierName, List<MEDocumentation> annotation, List<SupplementaryAttribute> attribList, List<EnumerationItem> enumList)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.addEnumClassifier >> Adding enumeration classifier '" + classifierName + "'...");
            JSONClassifier newClassifier = new JSONClassifier(this, classifierName, annotation, attribList, enumList);
            return true;
        }

        /// <summary>
        /// Add a typedef that contains elements from an external schema. The schema details are passed as arguments to the call. The type might either contain
        /// elements of a specified base-type, or, if the name of the base-type is "Any", the type can contain any elements defined by the external schema.
        /// </summary>
        /// <param name="classifierName">Name of the classifier.</param>
        /// <param name="annotation">Any comment from the UML model or empty list in case of no comments.</param>
        /// <param name="ns">Namespace string.</param>
        /// <param name="nsToken">Namespace token to be used when referring to 'ns'.</param>
        /// <param name="schema">Fully qualified schema filename.</param>
        /// <param name="baseType">Name of base type, or 'Any' in case of any type.</param>
        /// <param name="cardinality">Classifier cardinality, an upper boundary of '0' is interpreted as 'unbounded'.</param>
        /// <returns>True when defined Ok, false on errors.</returns>
        internal override bool AddExternalClassifier(string classifierName, List<MEDocumentation> annotation,
                                                     string ns, string nsToken,
                                                     string schema, string baseType,
                                                     Tuple<int, int> cardinality)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.addExternalClassifier >> Operation to add external classifier '" + classifierName + "' not (yet) supported for JSON!");
            return false;
        }

        /// <summary>
        /// Adds an element to the schema. Since JSON does not support the difference between 'elements' and 'types', we use this function to add
        /// root classes to schemas of type Operation or Message. A root class must be of an ABIE type that has been added earlier (using the
        /// AddABIEType operation).
        /// Since we have no placeholder for the annotation, this is ignored.
        /// </summary>
        /// <param name="elementName">Name of the element.</param>
        /// <param name="classifierNamespace">Namespace of the element classifier.</param>
        /// <param name="classifierName">Name of the element classifier (type of the element).</param>
        /// <param name="annotation">Optional comment for the element (empty list in case of no comment).</param>
        /// <returns>True on success or false on errors.</returns>
        internal override bool AddElement(string elementName, string classifierNamespace, string classifierName, List<MEDocumentation> annotation)
        {
            bool result = false;
            if (this.Type == SchemaType.Message || this.Type == SchemaType.Operation)
            {
                JSONSchema targetSchema = this;
                if (classifierNamespace != this.SchemaNamespace)
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.addElement >> Classifier is in other namespace '" + classifierNamespace + "'! Searching...");
                    foreach (JSONSchema extSchema in this._externalSchemas)
                    {
                        if (extSchema.SchemaNamespace == classifierNamespace)
                        {
                            targetSchema = extSchema;
                            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.addElement >> Found external schema!");
                            break;
                        }
                    }
                }
                JSchema classifier = targetSchema.FindClass(classifierName);
                if (classifier != null && !this._rootClasses.ContainsKey(elementName))
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.addElement >> Adding element '" + elementName + "' to rootClasses...");
                    this._rootClasses.Add(elementName, classifier);
                    result = true;
                }
                else Logger.WriteError("Framework.Util.SchemaManagement.JSON.JSONSchema.addElement >> Failed to add root class '" +
                                       elementName + ":" + classifierName + "', classifier not found or duplicate element!");
            }
            else Logger.WriteError("Framework.Util.SchemaManagement.JSON.JSONSchema.addElement >> Schema type unsuitable for adding root classes!");
            return result;
        }

        /// <summary>
        /// Since JSON does not support the notion of namespaces as such, this method has no effect. External schemas can be added instead by means
        /// of the 'AddSchemaReference' operation.
        /// </summary>
        /// <param name="nsToken">Namespace token as used throughout the schema to reference this external namespace.</param>
        /// <param name="nsName">The fully qualified namespace name, typically an URI or URL.</param>
        /// <param name="nsLocation">The location of the namespace as used during import of the namespace. Must refer to a valid location. NULL if no explicit import required.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the schema.</exception>
        internal override void AddNamespace(string nsToken, string nsName, string nsLocation)
        {
            Logger.WriteWarning("Framework.Util.SchemaManagement.JSON.JSONSchema.AddNamespace >> Method not supported for JSON schema.");
        }

        /// <summary>
        /// This function attempts to add a primitive type to the list of primitive types. If the type is not already in there, it will be added.
        /// </summary>
        /// <param name="thisType">The type to be added.</param>
        /// <returns>True is added successfully, FALSE if type is already in there.</returns>
        internal bool AddPrimitiveType(JSONPrimitiveType thisType)
        {
            if (this._primitiveTypes.ContainsKey(thisType.TypeName)) return false;
            this._primitiveTypes.Add(thisType.TypeName, thisType);
            return true;
        }

        /// <summary>
        /// Adds an external schema to the set of schemas referenced by the current schema.
        /// </summary>
        /// <param name="referencedSchema">The external schema that is referenced by the current schema.</param>
        internal override void AddSchemaReference(Schema referencedSchema)
        {
            // We only copy the schema if it is a valid JSON Schema and it's not my own schema (that would not make sense ;-)
            if (referencedSchema != null && referencedSchema is JSONSchema && ((JSONSchema)referencedSchema)._id != this._id)
            {
                // Check if entry with given ID is already in the list. If so, silently ignore. 
                foreach (JSONSchema extSchema in this._externalSchemas) if (extSchema._id == ((JSONSchema)referencedSchema)._id) return;
                this._externalSchemas.Add(referencedSchema as JSONSchema);
            }
        }

        /// <summary>
        /// This operation is used to create a type definition for a simple-type Basic Business Information Entity (BBIE). These are always of type CDT (or derived from CDT).
        /// The typeName MUST be a PRIM type, so the caller must have parsed the entire class hierarchy until arrival at PRIM level.
        /// Enumerations and Unions must NOT be added using this method, but must use methods 'addEnumClassifier' and 'addUnionClassifier' respectively..
        /// </summary>
        /// <param name="classifierName">BBIE classifier name</param>
        /// <param name="primType">Associated PRIM type</param>
        /// <param name="annotation">Optional comment for the classifier (empty list in case of no comment).</param>
        /// <param name="attribList">Optional list of attributes to be assigned tot the classifier. Since a simple classifier can not directly contain attributes,
        /// the type is converted into a complex type instead!</param>
        /// <param name="facetList">Optional list of facets to be applied to this type</param>
        /// <returns>TRUE in case of success, FALSE on errors. If the type already exists, no action is performed and the operation returns TRUE.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the schema.</exception>
        internal override bool AddSimpleClassifier(string classifierName, string primType, List<MEDocumentation> annotation, List<SupplementaryAttribute> attribList, List<Facet> facetList)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.addSimpleClassifier >> Adding classifier '" + classifierName + "' of type '" + primType + "'...");
            JSONClassifier newClassifier = new JSONClassifier(this, classifierName, primType, annotation, attribList, facetList);
            return true;
        }

        /// <summary>
        /// This method takes the separate components that comprise a JSchema and creates an initialized JSchema object. This object is a 
        /// 'snapshot' of the schema, since more elements might be added lateron. Each invocation of 'Build' creates a 'more complete' schema
        /// object. Build can be invoked separately in order to create a schema snapshot. It is also invoked from within the 'Save' method
        /// in order to create a schema that we can actually save.
        /// </summary>
        /// <exception cref="SystemException">Saving schema object failed.</exception>
        internal void Build()
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.Build >> Building schema '" + this.Name + "'...");
            try
            {
                this._schema = new JSchema()
                {
                    SchemaVersion = this._skeletonSchema.SchemaVersion,
                    Id = this._skeletonSchema.Id,
                    Title = this._skeletonSchema.Title,
                    AllowAdditionalProperties = false
                };
                if (this._skeletonSchema.Type != JSchemaType.Null) this._schema.Type = this._skeletonSchema.Type; 

                // If this is an Operation, we have to create properties out of the root classes...
                // If this is a Message, the root class itself becomes the Schema...
                if (this.Type == SchemaType.Operation)
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.Build >> Operation, creating properties...");
                    foreach (string property in this._rootClasses.Keys) this._schema.Properties.Add(property, this._rootClasses[property]);
                }
                else if (this.Type == SchemaType.Message)
                {
                    if (this._rootClasses.Count > 0)
                    {
                        Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.Build >> Message, promoting root class to schema...");
                        JSchema messageSchema = this._rootClasses.Values[0];
                        messageSchema.SchemaVersion = this._schema.SchemaVersion;
                        messageSchema.Id = this._schema.Id;
                        this._schema = messageSchema;

                        if (this._rootClasses.Count > 1)
                            Logger.WriteWarning("Framework.Util.SchemaManagement.JSON.JSONSchema.Build >> Multiple message root classes defined, all other then first are ignored!");
                    }
                    else Logger.WriteError("Framework.Util.SchemaManagement.JSON.JSONSchema.Build >> Message schema has no message root class defined!");
                }

                // Create the 'definitions' section and add all constructed classifiers and classes (in that order)...
                // We MUST create classifiers first since this is the only way to guarantee that external references are resolved correctly
                // (if we put the classes first, we run the risk that an external reference is resolved by referring to a class property instead
                // of the associated type definition, this might be a bug in the Json.Net library).
                JObject definitions = new JObject();
                this._schema.ExtensionData.Add("definitions", definitions);
                foreach (string classifier in this._classifiers.Keys)
                    if (this._classifiers[classifier].IsReferenceType) definitions.Add(classifier, this._classifiers[classifier].ReferenceClassifier);
                foreach (SortableSchemaElement key in this._classes.Keys) definitions.Add(key.Name, this._classes[key]);
            }
            catch (SystemException exc)
            {
                Logger.WriteError("Framework.Util.SchemaManagement.JSON.JSONSchema.Build >> Building schema failed because: " + exc.Message);
                SetLastError(exc.Message);
                throw;
            }
        }

        /// <summary>
        /// This function searches the list of registered classes for a class with specified name. If found, the associated schema is
        /// returned. If not found, the function returns NULL.
        /// </summary>
        /// <param name="className">Name of class we're looking for.</param>
        /// <returns>Class schema or NULL if not found.</returns>
        internal JSchema FindClass(string className)
        {
            SortableSchemaElement key = new SortableSchemaElement(className, SortableSchemaElement.SortingContext.Class, 0);
            return this._classes.ContainsKey(key)? this._classes[key]: null;
        }

        /// <summary>
        /// Searches the list of all classifiers defined for this schema for a classifier with the given name. If present, the associated Classifier
        /// descriptor is returned. When not found, the function returns NULL.
        /// </summary>
        /// <param name="classifierName">Classifier we're looking for.</param>
        /// <returns>Classifier descriptor or NULL when not found.</returns>
        internal JSONClassifier FindClassifier(string classifierName)
        {
            return this._classifiers.ContainsKey(classifierName)? this._classifiers[classifierName]: null;   
        }

        /// <summary>
        /// This function checks whether a primitive type with the given type name exists and if so, returns the type object.
        /// </summary>
        /// <param name="typeName">Type to look for. Note that this is the UML Model type name and NOT the JSON type!</param>
        /// <returns>Associated type object or NULL if nothing found.</returns>
        internal JSONPrimitiveType FindPrimitiveType(string typeName)
        {
            return this._primitiveTypes.ContainsKey(typeName)? this._primitiveTypes[typeName]: null;
        }

        /// <summary>
        /// Creates a JSON definitions object that contains all global type- and class definitions. The method returns a string that has
        /// the following format:
        /// {
        ///     "definitions": {
        ///        [All 'reference type' type definitions]
        ///        [All 'global object definitions]
        ///     }
        /// }
        /// </summary>
        /// <returns>Definitions object as a JSON Schema object string.</returns>
        internal string GetDefinitionsObject()
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.GetDefinitionsObject >> Retrieving 'definitions' object...");
            JSchema definitionsSchema = new JSchema();
            JObject definitions = new JObject();
            definitionsSchema.ExtensionData.Add("definitions", definitions);
            foreach (string classifier in this._classifiers.Keys)
                if (this._classifiers[classifier].IsReferenceType) definitions.Add(classifier, this._classifiers[classifier].ReferenceClassifier);
            foreach (SortableSchemaElement key in this._classes.Keys) definitions.Add(key.Name, this._classes[key]);
            string definitionsString = definitionsSchema.ToString();
            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.GetDefinitionsObject >> Got 'definitions':" + Environment.NewLine + definitionsString);
            return definitionsString;
        }

        /// <summary>
        /// Returns a schema-specific file extension.
        /// </summary>
        /// <returns>Schema-specific file extension, including separator character (e.g. ".xsd").</returns>
        internal override string GetSchemaFileExtension()
        {
            return ".json";
        }

        /// <summary>
        /// This function checks whether a class with the specified name exists in the list of registered classes. If so, it returns
        /// true. If not found, the function returns false.
        /// </summary>
        /// <param name="className">Name of class we're looking for.</param>
        /// <returns>True if present in schema, false otherwise.</returns>
        internal bool HasClass(string className)
        {
            SortableSchemaElement key = new SortableSchemaElement(className, SortableSchemaElement.SortingContext.Class, 0);
            return this._classes.ContainsKey(key);
        }

        /// <summary>
        /// This function checks whether a classifier with the given name is present in this schema.
        /// </summary>
        /// <param name="classifierName">The classifier to be checked.</param>
        /// <returns>True if present in schema, false otherwise.</returns>
        internal bool HasClassifier(string classifierName)
        {
            return this._classifiers.ContainsKey(classifierName);
        }

        /// <summary>
        /// This function checks whether a primitive type with the given name is present in this schema.
        /// </summary>
        /// <param name="typeName">The type to be checked.</param>
        /// <returns>True if present in schema, false otherwise.</returns>
        internal bool HasPrimitiveType(string typeName)
        {
            return this._primitiveTypes.ContainsKey(typeName);
        }

        /// <summary>
        /// Schema initializer.
        /// The JSONSchema class is responsible for the construction of a JSON 'Root' schemafile for a Service model. One class instance corresponds to 
        /// one service model. We recognize three different types of schemas:
        /// 1) Collection: has no explicit type or title, but consists merely of a 'definitions' section containing JSchema objects that can be referenced
        /// from other schemas.
        /// 2) Operation: has one root object (representing the operation), that has a number of object properties (the messages). These are added using 
        /// the 'AddElement' operation. Operation messages typically consists of a header and a body. 
        /// 3) Message: has one root object (representing the message) and the properties are the properties of the MessageAssembly that is used to represent this
        /// message. Messages do not have a header, unless this is explicitly added to the MessageAssembly type. The Message is defined using the 'AddElement'
        /// operation.
        /// The initializer accepts the schema type and name, a namespaceToken (mnemonic name that guarantees that elements within the schema are unique), 
        /// the namespace to be used for the schema and an optional schema version. When omitted, the version is set to '1.0.0'.
        /// </summary>
        /// <param name="type">Identifies the type of schema that we're building.</param>
        /// <param name="name">A meaningfull name, with which we can identify the schema.</param>
        /// <param name="nsToken">Namespace token, not used for JSON schemas.</param>
        /// <param name="ns">Schema namespace, must adhere to JSON Schema 'Id' specifications.</param>
        /// <param name="version">Major, minor and build number of the schema. When omitted, the version defaults to '1.0.0'</param>
        internal override void Initialize(SchemaType type, string name, string nsToken, string ns, string version)
        {
            if (!IsInitialized)
            {
                base.Initialize(type, name, nsToken, ns, version);

                Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.Initialize >> Creating new schema of type: '" + type + "', in namespace: '" +
                                 ns + "' and version: '" + version + "'...");

                ContextSlt context = ContextSlt.GetContextSlt();
                this._id = new Uri(ns, UriKind.Absolute);
                this._skeletonSchema = new JSchema()
                {
                    SchemaVersion = new Uri(context.GetConfigProperty(_JSONSchemaStdNamespace)),
                    Id = this._id,
                    Title = name,
                    AllowAdditionalProperties = false
                };

                if (type != SchemaType.Collection)
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.Initialize >> Schema has title: '" + name + "'.");
                    this._skeletonSchema.Type = JSchemaType.Object;
                }

                this._classifiers = new SortedList<string, JSONClassifier>();
                this._rootClasses = new SortedList<string, JSchema>();
                this._classes = new SortedList<SortableSchemaElement, JSchema>();
                this._primitiveTypes = new SortedList<string, JSONPrimitiveType>();
                this._externalSchemas = new List<JSONSchema>();
            }
        }

        /// <summary>
        /// Merges all the objects in the provided schema (other) with our current schema. Primary characteristics of the current schema (such as ID, Title,
        /// Description, etc.) are NOT affected by this operation.
        /// </summary>
        /// <param name="other">The schema to copy from.</param>
        internal override void Merge(Schema other)
        {
            if (other != null && other is JSONSchema)
            {
                var jsonSchema = other as JSONSchema;

                // Merge primitive types...
                foreach (string primType in jsonSchema._primitiveTypes.Keys)
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.Merge >> Merging primitive type '" + primType + "'...");
                    if (!this._primitiveTypes.ContainsKey(primType)) this._primitiveTypes.Add(primType, jsonSchema._primitiveTypes[primType]);
                }

                // Merge Classifiers...
                foreach (string classifier in jsonSchema._classifiers.Keys)
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.Merge >> Merging classifier '" + classifier + "'...");
                    if (!this._classifiers.ContainsKey(classifier)) this._classifiers.Add(classifier, jsonSchema._classifiers[classifier]);
                }

                // Merge Class definitions...
                foreach (SortableSchemaElement key in jsonSchema._classes.Keys)
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.Merge >> Merging class '" + key.Name + "'...");
                    if (!this._classes.ContainsKey(key)) this._classes.Add(key, jsonSchema._classes[key]);
                }

                // Merge Root Classes...
                foreach (string rootClass in jsonSchema._rootClasses.Keys)
                {
                    Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.Merge >> Merging root class '" + rootClass + "'...");
                    if (!this._rootClasses.ContainsKey(rootClass)) this._rootClasses.Add(rootClass, jsonSchema._rootClasses[rootClass]);
                }

                // Merge external schemas...
                foreach(JSONSchema extSchema in jsonSchema._externalSchemas)
                {
                    bool unique = true;
                    foreach (JSONSchema myExtSchema in this._externalSchemas)
                    {
                        if (myExtSchema._id == extSchema._id)
                        {
                            unique = false;
                            break;
                        }
                    }
                    if (unique) this._externalSchemas.Add(extSchema);
                }
            }
            else Logger.WriteWarning("Framework.Util.SchemaManagement.JSON.JSONSchema.Merge >> Attempt to merge empty or incompatible schema!");
        }

        /// <summary>
        /// This method first adds the provided header to the schema and then writes the schema as an indented JSON document to the
        /// provided stream. The produced schema adheres to JSON Schema Draft 4 standards.
        /// </summary>
        /// <param name="stream">Schema contents must be written to this stream.</param>
        /// <param name="header">Since JSON does NOT support proper comment blocks, the header is ignored!</param>
        /// <exception cref="SystemException">Saving schema object failed.</exception>
        internal override void Save(Stream stream, string header)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.Save >> Saving schema '" + this.Name + "'..."); 
            try
            {
                // Build the list of external schema references...
                List<ExternalSchema> extSchemaList = new List<ExternalSchema>();
                foreach (JSONSchema extSchema in this._externalSchemas)
                {
                    extSchema.Build();      // Creates a snapshot that should contain all definitions required by the current schema.
                    extSchemaList.Add(new ExternalSchema(extSchema._schema));
                }

                this.Build();   // Make sure that we have something to save.

                // Next, we actually are going to create the schema...
                using (var streamWriter = new StreamWriter(stream, Encoding.UTF8))
                using (var jsonWriter = new JsonTextWriter(streamWriter))
                {
                    jsonWriter.Formatting = Formatting.Indented;
                    jsonWriter.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                    jsonWriter.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                    this._schema.WriteTo(jsonWriter, new JSchemaWriterSettings
                    {
                        Version = Newtonsoft.Json.Schema.SchemaVersion.Draft4,
                        ExternalSchemas = extSchemaList,
                        ReferenceHandling = JSchemaWriterReferenceHandling.Always
                    });
                }
            }
            catch (SystemException exc)
            {
                Logger.WriteError("Framework.Util.SchemaManagement.JSON.JSONSchema.Save >> Generating schema failed because: " + exc.Message);
                SetLastError(exc.Message);
                throw;
            }
        }

        /// <summary>
        /// Since JSON Schema elements are always kept in sorted order, the method has no effect for JSON schema.
        /// </summary>
        internal override void Sort()
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONSchema.Sort >> Sorting JSON has no effect!");
        }
    }
}
