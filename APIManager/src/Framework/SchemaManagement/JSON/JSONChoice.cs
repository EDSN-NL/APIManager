using System.Collections.Generic;
using Newtonsoft.Json.Schema;
using Framework.Logging;

namespace Framework.Util.SchemaManagement.JSON
{
    /// <summary>
    /// A Choice class contains sets of JSON schema definitions that should go in various 'branches' of a 'OneOf' construct.
    /// JSON only supports ONE Choice group per source (which must be an ABIE) so even though you can define multiple choice
    /// groups in your model, you will get production errors if you try to utilize them in a JSON Scheme!
    /// </summary>
    internal class JSONChoice: Choice, IJSONProperty
    {
        private SortedList<string, SortedList<SortableSchemaElement, IJSONProperty>> _choices;  // List of lists (branches in choice group).
        private SortedList<string, bool> _mandatoryList;                                        // Used to keep track of mandatory elements.
        private bool _isMandatory;                                                              // True if the choice as a whole is mandatory.

        /// <summary>
        /// Implementation of the JSON Property interface:
        /// Schema = Returns the JSON Schema object that implements the interface.
        /// Name = Returns the associated property name (role name of the association).
        /// SequenceKey = Returns the sequence identifier of the association.
        /// IsMandatory = Returns true if the choice must be treated as a mandatory property of the owning schema. Note that this property
        /// is ONLY valid AFTER a call to FinalizeChoice!
        /// </summary>
        public JSchema JSchema          { get { return this.FinalizeChoice(); } }
        public new string Name          { get { return base.Name; } }
        public new int SequenceKey      { get { return base.SequenceKey; } }
        public bool IsMandatory         { get { return this._isMandatory; } }

        /// <summary>
        /// Getters for Choice properties:
        /// IsValid = A Choice object is valid if there are at least two entries in the sequence list (choice of 1 obviously ia not a choice).
        /// </summary>
        internal bool IsValid               { get { return this._choices.Count > 1; } }

        /// <summary>
        /// Default constructor, creates and initializes a new choice schema particle.
        /// </summary>
        /// <param name="choiceGroup">The choice group descriptor of the associated Choice Group.</param>
        internal JSONChoice(JSONSchema schema, ChoiceGroup choiceGroup) : base(schema, choiceGroup)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONChoice >> Creating choice: " + choiceGroup.GroupID);
            this._choices = new SortedList<string, SortedList<SortableSchemaElement, IJSONProperty>>();
            this._mandatoryList = new SortedList<string, bool>();
            this._isMandatory = false;
        }

        /// <summary>
        /// Add a new association (ASBIE) to this Choice. The association is inserted according to its Choice Sequence Identifier, 
        /// which groups items together within the Choice (all items with the same sequence identifier will be grouped in a sequence 
        /// within the Choice).
        /// </summary>
        /// <param name="association">The association to add.</param>
        internal override void AddAssociation(SchemaAssociation association)
        {
            var jsonAssociation = association as JSONAssociation;                 // Avoids a whole lot of casting ;-)
            var sortKey = new SortableSchemaElement(jsonAssociation);

            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONChoice.AddAssociation >> Adding association '" + jsonAssociation.ASBIEName + "'...");

            if (this.SequenceKey == 0 || (jsonAssociation.SequenceKey != 0 && jsonAssociation.SequenceKey < this.SequenceKey))
            {
                Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONChoice.AddAssociation >> Association sequence key '" + jsonAssociation.SequenceKey +
                                 "' is going to replace current choice sequence '" + this.SequenceKey + "'.");
                base.SequenceKey = jsonAssociation.SequenceKey;
            }

            if (!this._choices.ContainsKey(jsonAssociation.ChoiceGroup.SequenceID))
            {
                // We have not seen this sequence identifier before, create a new entry...
                Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONChoice.AddAssociation >> New sequence identifier: " + jsonAssociation.ChoiceGroup.SequenceID);
                var newList = new SortedList<SortableSchemaElement, IJSONProperty>
                {
                    { sortKey, jsonAssociation }
                };
                this._choices.Add(jsonAssociation.ChoiceGroup.SequenceID, newList);
                this._mandatoryList.Add(jsonAssociation.ChoiceGroup.SequenceID, jsonAssociation.IsMandatory);
            }
            else
            {
                Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONChoice.AddAssociation >> Add new element to existing sequence identifier: " + 
                                 jsonAssociation.ChoiceGroup.SequenceID);
                this._choices[jsonAssociation.ChoiceGroup.SequenceID].Add(sortKey, jsonAssociation);
                if (!this._mandatoryList[jsonAssociation.ChoiceGroup.SequenceID] && jsonAssociation.IsMandatory)
                    this._mandatoryList[jsonAssociation.ChoiceGroup.SequenceID] = true;
            }
        }

        /// <summary>
        /// Add a new content attribute to this Choice. The attribute is inserted according to its Choice Sequence Identifier, 
        /// which groups items together within the Choice (all items with the same sequence identifier will be grouped in a sequence 
        /// within the Choice).
        /// </summary>
        /// <param name="attribute">The attribute to add.</param>
        internal override void AddContentAttribute(ContentAttribute attribute)
        {
            var jsonAttribute = attribute as JSONContentAttribute;    // Avoids a whole lot of casting ;-)
            var sortKey = new SortableSchemaElement(jsonAttribute);
            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONChoice.AddContentAttribute >> Adding attribute '" + jsonAttribute.Name + "'...");

            if (this.SequenceKey == 0 || (jsonAttribute.SequenceKey != 0 && jsonAttribute.SequenceKey < this.SequenceKey))
            {
                Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONChoice.AddContentAttribute >> Attribute sequence key '" + jsonAttribute.SequenceKey +
                                 "' is going to replace current choice sequence '" + this.SequenceKey + "'.");
                base.SequenceKey = jsonAttribute.SequenceKey;
            }

            if (!this._choices.ContainsKey(jsonAttribute.ChoiceGroup.SequenceID))
            {
                // We have not seen this sequence identifier before, create a new entry...
                Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONChoice.AddContentAttribute >> New sequence identifier: " + attribute.ChoiceGroup.SequenceID);
                var newList = new SortedList<SortableSchemaElement, IJSONProperty>
                {
                    { sortKey, jsonAttribute }
                };
                this._choices.Add(jsonAttribute.ChoiceGroup.SequenceID, newList);
                this._mandatoryList.Add(jsonAttribute.ChoiceGroup.SequenceID, attribute.IsMandatory);
            }
            else
            {
                Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONChoice.AddContentAttribute >> Add new element to existing sequence identifier: " + attribute.ChoiceGroup.SequenceID);
                this._choices[jsonAttribute.ChoiceGroup.SequenceID].Add(sortKey, jsonAttribute);
                if (!this._mandatoryList[jsonAttribute.ChoiceGroup.SequenceID] && jsonAttribute.IsMandatory) this._mandatoryList[jsonAttribute.ChoiceGroup.SequenceID] = true;
            }
        }

        /// <summary>
        /// This helper method is called just before the internal schema representation is requested. The method builds the actual choice structure
        /// from the internal list of lists and returns this as a JSON Schema that must be embedded in the complex type of the 'parent' element.
        /// </summary>
        /// <returns>Initialized internal schema representation of this choice object.</returns>
        private JSchema FinalizeChoice()
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONChoice.FinalizeChoice >> Finalizing choice group '" + 
                             this.Name + "' with sequence key '" + this.SequenceKey + "'...");

            // A JSON schema can have a maximum of ONE choice per ABIE. This choice has to be a schema and we name it after the
            // choice group (e.g. a choice "Address:StreetAddress" will have a schema element named "AddressChoiceType").
            // Within the owning ABIE, this will result in a property named after the choice group, with the schema as classifier
            // (e.g. "Address", AddressChoiceType).
            var choice = new JSchema
            {
                Title = this.Name + "ChoiceType",
                Type = JSchemaType.Object,
                AllowAdditionalProperties = false
            };

            foreach (KeyValuePair<string, SortedList<SortableSchemaElement, IJSONProperty>> sequence in this._choices)
            {
                // Given the construction of choices in JSON, each 'leg' (sequence) of the choice will get it's own schema, independent of the number
                // of items in that choice sequence. We create a separate schema, which we name after the sequence name (e.g. a choice 
                // "Address:StreetAddress" will have a sequence schema named "StreetAddressChoiceSequenceType")...
                string sequenceName = sequence.Key + "ChoiceSequenceType";
                Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONChoice.FinalizeChoice >> Creating separate choice type with name: " + sequenceName);
                var requiredItems = new List<string>();
                var choiceSequence = new JSchema
                {
                    Title = sequenceName,
                    Type = JSchemaType.Object,
                    AllowAdditionalProperties = false
                };

                // Add the list of elements to the newly created type...
                foreach (KeyValuePair<SortableSchemaElement, IJSONProperty> seqElement in sequence.Value)
                {
                        Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.JSONChoice.FinalizeChoice >> Adding choice element '" +
                                         seqElement.Value.Name + "' to sequence '" + sequenceName + "'...");
                    choiceSequence.Properties.Add(seqElement.Value.Name, seqElement.Value.JSchema);
                }

                // The element is mandatory if one of the contained elements is mandatory.
                if (this._mandatoryList[sequence.Key]) requiredItems.Add(sequenceName);
                choice.OneOf.Add(choiceSequence);
            }

            // Finally, we have to determine the cardinality of the choice.
            // Cardinality of a choice is tricky. If you have an optional choice with mandatory elements, you can miss mandatory elements. 
            // And the other way around: if you have a mandatory choice with optional elements, you might have difficulty creating a valid message.
            // Our implementation checks each of the sequence groups within the choice and sets the Choice cardinality to mandatory ONLY in case EACH
            // sequence contains at least one mandatory element.
            // If the user has explicitly specified the cardinality, life is a whole lot easier since we just have to use that one.
            // For JSON, we can not explicitly specify a cardinality other then 0 or 1. We thus only look at the lower boundary and ignore
            // the upper boundary of the cardinality. This is a limitation of JSON schema (which does not really support choices at all).
            if (Cardinality == null || Cardinality.Item1 == -1)
            {
                this._isMandatory = true;
                foreach (KeyValuePair<string, bool> mandatoryFlag in this._mandatoryList)
                {
                    if (mandatoryFlag.Value == false)
                    {
                        this._isMandatory = false;
                        break;
                    }
                }
            }
            else this._isMandatory = (Cardinality.Item1 > 0);
            return choice;
        }
    }
}
