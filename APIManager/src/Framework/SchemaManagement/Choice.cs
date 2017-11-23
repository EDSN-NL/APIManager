using Framework.Logging;

namespace Framework.Util.SchemaManagement
{
    /// <summary>
    /// This helper class provides support for the implementation of choices in the output schema. A choice is implemented in the UML model by means of 
    /// 'Choice Group' identifiers (which contains a unique identifier for a 'set of choices' within an element). Within each Choice Group, the
    /// 'Sequence ID' identifier specifies which attributes / associations should be grouped together within the associated Choice Group.
    /// All attributes / associations with the same Sequence ID go to a single sequence within the associated Choice.
    /// </summary>
    internal class ChoiceGroup
    {
        private string _groupID;
        private string _sequenceID;

        /// <summary>
        /// Getters for ChoiceGroup properties:
        /// GroupID = Identifier of the Choice element within the schema.
        /// SequenceID = Identifier for one of the 'branches' within that Choice element.
        /// </summary>
        internal string GroupID       { get { return this._groupID; } }
        internal string SequenceID    { get { return this._sequenceID; } }

        /// <summary>
        /// Default constructor, creates a new Choice Group.
        /// </summary>
        /// <param name="group">Group identifier (opaque string).</param>
        /// <param name="sequence">Sequence identifier (opaque string).</param>
        internal ChoiceGroup(string group, string sequence)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.ChoiceGroup >> Creating choice group '" + group + "' with sequence ID: " + sequence);
            this._groupID = group;
            this._sequenceID = sequence;
        }
    }

    /// <summary>
    /// A Choice class represents the actual Choice Schema Particle (part of a constructed type definition). Since different schema types require different
    /// implementations, Choice is implemented as an abstract base class that must be specialized by the various schema implementations.
    /// </summary>
    internal abstract class Choice
    {
        private Schema _schema;             // The schema in which we're creating the choice.
        private int _sequenceKey;           // Set to the lowest value of the sequence keys of all choice elements.
        private string _name;               // This is the Choice Group name assigned to the choice group. Used for sorting.

        /// <summary>
        /// Getters for Choice properties:
        /// Schema = The schema in which we're creating the choi
        /// IsValid = A Choice object is valid if there are at least two entries in the sequence list (choice of 1 obviously ia not a choice).
        /// Name = Corresponds with the Choice Group ID and represents the unique name of this Choice element.
        /// SequenceKey = The sequence key of the XML Schema Choice Element which is calculated by looking for the lowest key value of all elements that
        /// comprise this choice.
        /// </summary>
        internal Schema Schema                    { get { return this._schema; } }
        internal string Name                      { get { return this._name; } }
        internal int SequenceKey
        {
            get { return this._sequenceKey; }
            set { this._sequenceKey = value; }
        }

        /// <summary>
        /// Default constructor, creates and initializes our generic properties.
        /// </summary>
        /// <param name="schema">The schema in which we are creating the choice.</param>
        /// <param name="choiceGroupID">The identifier of the associated Choice Group.</param>
        internal Choice(Schema schema, string choiceGroupID)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.Choice >> Creating choice: " + choiceGroupID);
            this._schema = schema;
            this._sequenceKey = 0;
            this._name = choiceGroupID;
        }

        /// <summary>
        /// Add a new association (constructed element) to this Choice. The association is inserted according to its Choice Sequence Identifier, 
        /// which groups elements together within the Choice (all elements with the same sequence identifier will be grouped in a sequence 
        /// within the Choice).
        /// </summary>
        /// <param name="association">The association to add.</param>
        internal abstract void AddAssociation(SchemaAssociation association);

        /// <summary>
        /// Add a new attribute (simple element) to this Choice. The attribute is inserted according to its Choice Sequence Identifier, 
        /// which groups elements together within the Choice (all elements with the same sequence identifier will be grouped in a sequence 
        /// within the Choice).
        /// </summary>
        /// <param name="attribute">The attribute to add.</param>
        internal abstract void AddContentAttribute(ContentAttribute attribute);
    }
}
