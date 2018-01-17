using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using Framework.Logging;

namespace Framework.Util.SchemaManagement.XML
{
    /// <summary>
    /// A Choice class represents the actual XSD Choice Schema Particle (part of a constructed type definition).
    /// </summary>
    internal class XMLChoice: Choice
    {
        private SortedList<string, SortedList<SortableSchemaElement, XmlSchemaElement>> _choiceSequences;   // List of lists...
        private SortedList<string, bool> _mandatoryList;    // Used to keep track of mandatory elements.

        /// <summary>
        /// Getters for Choice properties:
        /// IsValid = A Choice object is valid if there are at least two entries in the sequence list (choice of 1 obviously ia not a choice).
        /// SchemaObject = The choice encoded as an XML Schema Element.
        /// </summary>
        internal bool IsValid                     { get { return this._choiceSequences.Count > 1; } }
        internal XmlSchemaChoice SchemaObject     { get { return this.FinalizeChoice(); } }

        /// <summary>
        /// Default constructor, creates and initializes a new choice schema particle.
        /// </summary>
        /// <param name="choiceGroup">The choice group descriptor of the associated Choice Group.</param>
        internal XMLChoice(XMLSchema schema, ChoiceGroup choiceGroup) : base(schema, choiceGroup)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLChoice >> Creating choice: " + choiceGroup.GroupID);
            this._choiceSequences = new SortedList<string, SortedList<SortableSchemaElement, XmlSchemaElement>>();
            this._mandatoryList = new SortedList<string, bool>();
        }

        /// <summary>
        /// Add a new association (constructed element) to this Choice. The association is inserted according to its Choice Sequence Identifier, 
        /// which groups elements together within the Choice (all elements with the same sequence identifier will be grouped in a sequence 
        /// within the Choice).
        /// </summary>
        /// <param name="association">The association to add.</param>
        internal override void AddAssociation(SchemaAssociation association)
        {
            var xmlAssociation = association as XMLAssociation;                 // Avoids a whole lot of casting ;-)
            Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLChoice.AddAssociation >> Adding association '" + xmlAssociation.SchemaElement.Name + "'...");
            XmlSchemaElement schemaElement = xmlAssociation.SchemaElement;
            var sortKey = new SortableSchemaElement(schemaElement, xmlAssociation.SequenceKey);

            if (this.SequenceKey == 0 || (xmlAssociation.SequenceKey != 0 && xmlAssociation.SequenceKey < this.SequenceKey))
            {
                Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLChoice.AddAssociation >> Association sequence key '" + xmlAssociation.SequenceKey +
                                 "' is going to replace current choice sequence '" + this.SequenceKey + "'.");
                this.SequenceKey = xmlAssociation.SequenceKey;
            }

            if (!this._choiceSequences.ContainsKey(xmlAssociation.ChoiceGroup.SequenceID))
            {
                // We have not seen this sequence identifier before, create a new entry...
                Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLChoice.AddAssociation >> New sequence identifier: " + xmlAssociation.ChoiceGroup.SequenceID);
                var newList = new SortedList<SortableSchemaElement, XmlSchemaElement>
                {
                    { sortKey, schemaElement }
                };
                this._choiceSequences.Add(xmlAssociation.ChoiceGroup.SequenceID, newList);
                this._mandatoryList.Add(xmlAssociation.ChoiceGroup.SequenceID, xmlAssociation.IsMandatory);
            }
            else
            {
                Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLChoice.AddAssociation >> Add new element to existing sequence identifier: " + xmlAssociation.ChoiceGroup.SequenceID);
                this._choiceSequences[xmlAssociation.ChoiceGroup.SequenceID].Add(sortKey, schemaElement);
                if (!this._mandatoryList[xmlAssociation.ChoiceGroup.SequenceID] && xmlAssociation.IsMandatory) this._mandatoryList[xmlAssociation.ChoiceGroup.SequenceID] = true;
            }
        }

        /// <summary>
        /// Add a new attribute (simple element) to this Choice. The attribute is inserted according to its Choice Sequence Identifier, 
        /// which groups elements together within the Choice (all elements with the same sequence identifier will be grouped in a sequence 
        /// within the Choice).
        /// </summary>
        /// <param name="attribute">The attribute to add.</param>
        internal override void AddContentAttribute(ContentAttribute attribute)
        {
            var xmlAttribute = attribute as XMLContentAttribute;    // Avoids a whole lot of casting ;-)
            Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLChoice.AddContentAttribute >> Adding attribute '" + xmlAttribute.SchemaElement.Name + "'...");
            Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLChoice.AddContentAttribute >> Choice sequence key: " + this.SequenceKey + " and attribute key: " + xmlAttribute.SequenceKey);

            XmlSchemaElement schemaElement = xmlAttribute.SchemaElement;
            var sortKey = new SortableSchemaElement(schemaElement, xmlAttribute.SequenceKey);

            if (this.SequenceKey == 0 || (xmlAttribute.SequenceKey != 0 && xmlAttribute.SequenceKey < this.SequenceKey))
            {
                Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLChoice.AddContentAttribute >> Attribute sequence key '" + xmlAttribute.SequenceKey +
                                 "' is going to replace current choice sequence '" + this.SequenceKey + "'.");
                this.SequenceKey = xmlAttribute.SequenceKey;
            }

            if (!this._choiceSequences.ContainsKey(xmlAttribute.ChoiceGroup.SequenceID))
            {
                // We have not seen this sequence identifier before, create a new entry...
                Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLChoice.AddContentAttribute >> New sequence identifier: " + attribute.ChoiceGroup.SequenceID);
                var newList = new SortedList<SortableSchemaElement, XmlSchemaElement>
                {
                    { sortKey, schemaElement }
                };
                this._choiceSequences.Add(xmlAttribute.ChoiceGroup.SequenceID, newList);
                this._mandatoryList.Add(xmlAttribute.ChoiceGroup.SequenceID, attribute.IsMandatory);
            }
            else
            {
                Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLChoice.AddContentAttribute >> Add new element to existing sequence identifier: " + xmlAttribute.ChoiceGroup.SequenceID);
                this._choiceSequences[xmlAttribute.ChoiceGroup.SequenceID].Add(sortKey, schemaElement);
                if (!this._mandatoryList[xmlAttribute.ChoiceGroup.SequenceID] && xmlAttribute.IsMandatory) this._mandatoryList[xmlAttribute.ChoiceGroup.SequenceID] = true;
            }
        }

        /// <summary>
        /// This helper method is called just before the internal XSD schema representation is requested. The method builds the actual choice structure
        /// from the internal list of lists and returns this as a choice type that must be embedded in the complex type of the 'parent' element.
        /// </summary>
        /// <returns>Initialized internal schema representation of this choice object.</returns>
        private XmlSchemaChoice FinalizeChoice()
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLChoice.FinalizeChoice >> Finalizing choice group '" + 
                             this.Name + "' with sequence key '" + this.SequenceKey + "'...");
            var choice = new XmlSchemaChoice();
            foreach (KeyValuePair<string, SortedList<SortableSchemaElement, XmlSchemaElement>> sequence in this._choiceSequences)
            {
                if (sequence.Value.Count > 1)
                {
                    // This particular choice sequence has more then 1 element. In order to avoid problems with schema parsing, we're going to create a separate
                    // type for this sequence and insert an element of that type...
                    // Note that, for this model to work, the Choice Group Name must be unique within the schema! We create a name by combining the choice group
                    // name with the choice group sequence name (e.g. a choice "Address:StreetAddress" will become "Address_StreetAddress")...
                    string choiceElementName = this.Name + "_" + sequence.Key;
                    string choiceTypeName = choiceElementName + "Type";
                    Logger.WriteInfo("Framework.Util.SchemaManagement.XML.XMLChoice.FinalizeChoice >> Creating separate choice type with name: " + choiceTypeName);

                    var choiceType = new XmlSchemaComplexType() { Name = choiceTypeName };
                    var choiceSequence = new XmlSchemaSequence();
                    choiceType.Particle = choiceSequence;
                    ((XMLSchema)this.Schema).AddToSchema(choiceType);

                    // Add the list of elements to the newly created type...
                    foreach (KeyValuePair<SortableSchemaElement, XmlSchemaElement> seqElement in sequence.Value)
                    {
                        choiceSequence.Items.Add(seqElement.Value);
                    }

                    // Now, create an element of the newly created type and add this to the choice. 
                    // The element is mandatory if one of the contained elements is mandatory.
                    var choiceElement = new XmlSchemaElement()
                    {
                        Name = choiceElementName,
                        SchemaTypeName = new XmlQualifiedName(choiceTypeName, this.Schema.SchemaNamespace)
                    };
                    choice.Items.Add(choiceElement);
                    choiceElement.MinOccurs = (_mandatoryList[sequence.Key]) ? 1 : 0;
                    choiceElement.MaxOccurs = 1;
                }
                else
                {
                    // Only one element, this can be added directly...
                    choice.Items.Add(sequence.Value.Values[0]);
                }
            }

            // Finally, we have to determine the cardinality of the choice.
            // Cardinality of a choice is tricky. If you have an optional choice with mandatory elements, you can miss mandatory elements. 
            // And the other way around: if you have a mandatory choice with optional elements, you might have difficulty creating a valid XML.
            // Our implementation checks each of the sequence groups within the choice and sets the Choice cardinality to mandatory ONLY in case EACH
            // sequence contains at least one mandatory element.
            // If the user has explicitly specified the cardinality, life is a whole lot easier since we just have to use that one.
            if (Cardinality == null || Cardinality.Item1 == -1)
            {
                bool mandatoryChoice = true;
                foreach (KeyValuePair<string, bool> mandatoryFlag in this._mandatoryList)
                {
                    if (mandatoryFlag.Value == false)
                    {
                        mandatoryChoice = false;
                        break;
                    }
                }
                choice.MinOccurs = (mandatoryChoice) ? 1 : 0;
                choice.MaxOccurs = 1;
            }
            else
            {
                choice.MinOccurs = Cardinality.Item1;
                if (Cardinality.Item2 == 0) choice.MaxOccursString = "unbounded";
                else choice.MaxOccurs = Cardinality.Item2;
            }
            return choice;
        }
    }
}
