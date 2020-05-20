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
        private bool _useListPostfix;               // Set to 'true' when the Schema forces use of "List" postfix in case cardinality > 1.
        private bool _isDeferred;                   // Set to 'true' when we were not able to solve the target of the association at constructor time!
                                                    // The association properties are kept by the Deferred Association singleton at this time.

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
        /// Returns true in case the association is on the 'deferred' list, which must be resolved at a later time...
        /// </summary>
        internal bool IsDeferred { get { return this._isDeferred; } }

        /// <summary>
        /// Creates a new Association object (ASBIE instance) within the given schema. Other arguments are the name of the target ABIE as well as the 
        /// cardinality of the association (target end). If the target of the association (the actual classifier definition) resides in another schema, 
        /// a reference to that schema MUST be passed through 'namespaceRef'. If this is NULL, it is assumed that we can locate the ABIE in our
        /// current schema (passed as 'schema').
        /// </summary>
        /// <param name="nodeID">The unique identifier of the source node (i.e. class that is the originator of the association)</param>
        /// <param name="schema">The schema in which the ASBIE is being created.</param>
        /// <param name="associationName">The name that will be assigned to the element (role name of the association).</param>
        /// <param name="classifierName">The name of the target ABIE type.</param>
        /// <param name="sequenceKey">The value of sequenceKey defines the order within a list of associations. Value 0 means 'inactive'</param>
        /// <param name="choiceGroup">Optional identifier of choicegroup that this association should go to (NULL is not defined).</param>
        /// <param name="cardinality">Association cardinality. An upper boundary of '0' is interpreted as 'unbounded'.</param>
        /// <param name="classifierSchema">Optional reference to external schema. If NULL, the classifier is referenced through 'schema.'</param>
        internal JSONAssociation(int nodeID, JSONSchema schema, string associationName,
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
            this._useListPostfix = schema.UseLists && cardinality.UseLists;
            this._isDeferred = false;

            JSONSchema searchSchema = classifierSchema ?? schema;
            JSchema classSchema = searchSchema.FindClass(classifierName);
            if (classSchema == null)
            {
                // No target schema present yet. We probably are dealing with a self-reference! Defer this association till we're done processing
                // all classes...
                var properties = new JSONDeferredAssociationSlt.AssociationProperties(schema, associationName, classifierName, sequenceKey, choiceGroup, cardinality, classifierSchema);
                if (!JSONDeferredAssociationSlt.GetJSONDeferredAssociationSlt().AddAssociationRequest(nodeID, properties))
                {
                    // This time, the target is really missing!
                    Logger.WriteError("Framework.Util.SchemaManagement.JSON.JSONAssociation >> Class '" + classifierName + "' not found in schema '" + searchSchema.Name + "'!");
                    this.IsValid = false;
                }
                this._isDeferred = true;    
                return;     // We keep the 'isValid' status set to 'false', since deferred association objects are in fact invalid.
            }

            // If cardinality suggests a list, we have to create an array of type, instead of only the type.
            // We will add a new postfix for this type, EXCEPT when the schema- and cardinality settings indicate this should not happen.
            if (cardinality.IsList)
            {
                // If the classifier name ends with 'Type', we remove this before adding a new post-fix 'ListType'...
                string listType = classifierName;
                if (schema.UseLists && cardinality.UseLists)
                {
                    listType = classifierName.EndsWith("Type") ? classifierName.Substring(0, classifierName.IndexOf("Type")) : classifierName;
                    listType += "ListType";
                }
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
        /// If the name represents a list, we append 'List' to the name (but ONLY when the Schema settings allow this).
        /// </summary>
        /// <returns>Association role name to be used in schemas.</returns>
        private string GetSchemaName()
        {
            string name = base.RoleName;
            if (this._useListPostfix && this._isList && !name.EndsWith("List")) name += "List";
            return name;
        }
    }
}
