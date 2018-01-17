using System;
using Framework.Logging;
using Framework.Util;

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
        private string _groupID;                // Identifies the choice group as a whole.
        private string _sequenceID;             // Identifies an item (or set of items) within the choice.
        private Tuple<int, int> _cardinality;   // Choice cardinality, only specified when part of the group, otherwise, this will contain -1, -1.

        /// <summary>
        /// Getters for ChoiceGroup properties:
        /// GroupID = Identifier of the Choice element within the schema.
        /// SequenceID = Identifier for one of the 'branches' within that Choice element.
        /// Cardinality = optional cardinality of choice group.
        /// </summary>
        internal string GroupID                 { get { return this._groupID; } }
        internal string SequenceID              { get { return this._sequenceID; } }
        internal Tuple<int,int> Cardinality     { get { return this._cardinality; } }

        /// <summary>
        /// Default constructor, creates a new Choice Group.
        /// </summary>
        /// <param name="group">Group identifier (opaque string).</param>
        /// <param name="sequence">Sequence identifier (opaque string).</param>
        internal ChoiceGroup(string group, string sequence)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.ChoiceGroup >> Creating choice group '" + group + "' with sequence ID: " + sequence);
            if (group.Contains("["))
            {
                if (!group.Contains("]") || !group.Contains(".."))
                {
                    Logger.WriteError("Framework.Util.SchemaManagement.ChoiceGroup >> Illegal choice group format in group '" + group + "'!");
                    this._groupID = group.Substring(0, group.IndexOf('['));
                    this._cardinality = new Tuple<int, int>(-1, -1);
                }
                else
                {
                    this._groupID = Conversions.ToPascalCase(group.Substring(0, group.IndexOf('[')));
                    string card = group.Substring(group.IndexOf('['));
                    string lowBoundary = card.Substring(1, card.IndexOf("..") - 1);
                    string highBoundary = card.Substring(card.IndexOf("..") + 2);
                    highBoundary = highBoundary.Substring(0, highBoundary.IndexOf("]"));
                    int low;
                    int high;
                    if (!int.TryParse(lowBoundary, out low))
                    {
                        Logger.WriteError("Framework.Util.SchemaManagement.ChoiceGroup >> Illegal low boundary '" + lowBoundary + "' ignored!");
                        low = 0;
                    }
                    if (highBoundary == "*" || highBoundary == "n" || highBoundary == "N") high = 0;
                    else if (!int.TryParse(highBoundary, out high))
                    {
                        Logger.WriteError("Framework.Util.SchemaManagement.ChoiceGroup >> Illegal high boundary '" + highBoundary + "' ignored!");
                        high = 1;
                    }
                    if (high != 0 && high < low)
                    {
                        Logger.WriteError("Framework.Util.SchemaManagement.ChoiceGroup >> High/Low relationship '" + lowBoundary + ".." + highBoundary + "' is incorrect!");
                        high = 1;
                    }
                    this._cardinality = new Tuple<int, int>(low, high);
                    Logger.WriteInfo("Framework.Util.SchemaManagement.ChoiceGroup >> Cardinality of choice set to: '" + low + ".." + high + "'.");
                }

            }
            else this._groupID = Conversions.ToPascalCase(group);
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
        private Tuple<int, int> _cardinality;   // Choice cardinality (optional).

        /// <summary>
        /// Getters for Choice properties:
        /// Schema = The schema in which we're creating the choi
        /// IsValid = A Choice object is valid if there are at least two entries in the sequence list (choice of 1 obviously ia not a choice).
        /// Name = Corresponds with the Choice Group ID and represents the unique name of this Choice element.
        /// Cardinality = Optional cardinality of the choice group. Contains -1, -1 when unspecified.
        /// SequenceKey = The sequence key of the XML Schema Choice Element which is calculated by looking for the lowest key value of all elements that
        /// comprise this choice.
        /// </summary>
        internal Schema Schema                      { get { return this._schema; } }
        internal string Name                        { get { return this._name; } }
        internal Tuple<int, int> Cardinality        { get { return this._cardinality; } }
        internal int SequenceKey
        {
            get { return this._sequenceKey; }
            set { this._sequenceKey = value; }
        }

        /// <summary>
        /// Default constructor, creates and initializes our generic properties.
        /// </summary>
        /// <param name="schema">The schema in which we are creating the choice.</param>
        /// <param name="choiceGroup">The choice group descriptor for this particular choice object.</param>
        internal Choice(Schema schema, ChoiceGroup choiceGroup)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.Choice >> Creating choice: " + choiceGroup.GroupID);
            this._schema = schema;
            this._sequenceKey = 0;
            this._name = choiceGroup.GroupID;
            this._cardinality = choiceGroup.Cardinality;
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
