using System;
using Newtonsoft.Json.Schema;
using Framework.Logging;
using Framework.Util;

namespace Framework.Util.SchemaManagement.JSON
{
    /// <summary>
    /// An Association class represents the "target end" of an ASBIE (an ASsociated Business Information Element). In the chosen model, each ABIE is 
    /// represented by a JSON schema object that must have been created earlier in the respective root schema (reason for this is that we can 
    /// reference only to existing schemas). The way our recursive parser works guarantees that we first create the schemas that have no further
    /// associations, so this should be ok.
    /// Note that we can not process documentation for associations, since we have nothing to 'attach' the documentation to. For typical (0..1)
    /// associations, we will have a source-class property of type ["rolename", ABIE-Scheme]. ABIE-Scheme will carrie the annotation regarding
    /// the ABIE as a classifier and since this is shared by all source-classes that reference this ABIE, we can not replace the annotation by
    /// role annotation. Solution would have been to create a separate 'AllOf' schema that encapsulates the ABIE, but this would make the schema
    /// more difficult to read and comprehend.
    /// 
    /// Note: in case the cardinality of the association has an upper boundary > 1, a separate List element is created.
    /// If the cardinality lower boundary is 0, this list becomes an optional element. List contents always have a lower boundary of 1!
    /// </summary>
    internal class JSONAssociation : SchemaAssociation, IJSONProperty
    {
        private JSchema _associationClassifier;     // Represents the association as a (referenced) schema. 
        private bool _isList;                       // Set to 'true' if the association has a cardinality > 1.

        /// <summary>
        /// Implementation of the JSON Property interface:
        /// Schema = Returns the JSON Schema object that implements the interface.
        /// Name = Returns the associated property name (role name of the association).
        /// SequenceKey = Returns the sequence identifier of the association.
        /// IsMandatory = Checks whether the association must be present.
        /// </summary>
        public JSchema JSchema          { get { return this._associationClassifier; } }
        public string Name              { get { return base.RoleName; } }
        public string SchemaName        { get { return GetSchemaName(); } }
        public new int SequenceKey      { get { return base.SequenceKey; } }
        public new bool IsMandatory     { get { return base.IsMandatory; } }

        /// <summary>
        /// Creates a new Association object (ASBIE instance) within the given schema. Other arguments are the name of the target ABIE as well as the 
        /// cardinality of the association (target end). If the target of the association (the actual classifier definition) resides in another schema, 
        /// a reference to that schema MUST be passed through 'namespaceRef'. If this is NULL, it is assumed that we can locate the ABIE in our
        /// current schema (passed as 'schema').
        /// </summary>
        /// <param name="schema">The schema in which the ASBIE is being created.</param>
        /// <param name="associationName">The name that will be assigned to the element (role name of the association).</param>
        /// <param name="classifierName">The name of the target ABIE type.</param>
        /// <param name="sequenceKey">The value of sequenceKey defines the order within a list of associations. Value 0 means 'inactive'</param>
        /// <param name="choiceGroup">Optional identifier of choicegroup that this association should go to (NULL is not defined).</param>
        /// <param name="cardinality">Association cardinality. An upper boundary of '0' is interpreted as 'unbounded'.</param>
        /// <param name="classifierSchema">Optional reference to external schema. If NULL, the classifier is referenced through 'schema.'</param>
        internal JSONAssociation(JSONSchema schema, string associationName,
                                 string classifierName,
                                 int sequenceKey,
                                 ChoiceGroup choiceGroup,
                                 Cardinality cardinality,
                                 JSONSchema classifierSchema = null) : base(associationName, classifierName, sequenceKey, cardinality, choiceGroup)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONAssociation >> Creating association for '" + associationName + "' with classifier: " + classifierName +
                             " and cardinality: " + cardinality.ToString() + "...");
            this.IsValid = false;
            this._isList = false;

            JSONSchema searchSchema = classifierSchema ?? schema;
            JSchema classSchema = searchSchema.FindClass(classifierName);
            if (classSchema == null)
            {
                /**********************
                 * In plaats van deze melding moeten we deze referentie even "parkeren" en aan het eind alsnog oplossen. Wordt i.h.a. namelijk
                 * veroorzaakt door loops in het model waarbij de target nog niet is geprocessed op het moment dat de loop wordt resolved.
                 * 
                 * *********************/
                Logger.WriteError("Framework.Util.SchemaManagement.JSON.JSONAssociation >> Class '" + classifierName + "' not found in schema '" + searchSchema.Name + "'!");
                this.IsValid = false;
                return;
            }

            // If cardinality suggests a list, we have to create an array of type, instead of only the type.
            if (cardinality.IsList)
            {
                // If the classifier name ends with 'Type', we remove this before adding a new post-fix 'ListType'...
                string listType = classifierName.EndsWith("Type") ? classifierName.Substring(0, classifierName.IndexOf("Type")) : classifierName;
                listType += "ListType";
                this._associationClassifier = new JSchema()
                {
                    Title = listType,
                    Type = JSchemaType.Array
                };
                if (cardinality.IsMandatory) this._associationClassifier.MinimumItems = cardinality.LowerBoundary;
                if (cardinality.IsBoundedList) this._associationClassifier.MaximumItems = cardinality.UpperBoundary;
                this._associationClassifier.Items.Add(classSchema);
                this._isList = true;
            }
            else this._associationClassifier = classSchema;
            this.IsValid = true;
        }

        /// <summary>
        /// Helper function that returns the schema name of the association.
        /// If the name represents a list, we append 'List' to the name.
        /// </summary>
        /// <returns>Association role name to be used in schemas.</returns>
        private string GetSchemaName()
        {
            string name = base.RoleName;
            if (this._isList && !name.EndsWith("List"))
            {
                // If a role name ends with 'type', this is probably as intended and we must leave it alone!
                //if (name.EndsWith("Type")) name = name.Substring(0, name.LastIndexOf("Type"));
                name += "List";
            }
            return name;
        }
    }
}
