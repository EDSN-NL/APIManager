using System;
using System.Xml.Schema;

namespace Framework.Util.SchemaManagement.XML
{
    /// <summary>
    /// This is a helper class that is used by the schema logic to assure that types and elements are sorted in the correct order within either the schema 
    /// files themselves, but also within an aggregated type (element order).
    /// </summary>
    internal class SortableSchemaElement : IComparable
    {
        private XmlSchemaObject _schemaObject;      // Used in case of Element or Schema Type.
        private XMLChoice _choice;                  // Used in case of Choice.
        private bool _schemaObjectIsElement;
        private string _schemaObjectName;
        private int _sequenceKey;

        /// <summary>
        /// Returns the sequence key associated with this sortable element.
        /// </summary>
        internal int SequenceKey              { get { return this._sequenceKey; } }

        /// <summary>
        /// Returns the associated internal schema representation. In case of 'Choice', we use our representation of Choice, thus we have to ask that
        /// object to return the schema representation. 
        /// </summary>
        internal XmlSchemaObject SchemaObject { get { return (this._choice != null) ? this._choice.SchemaObject : this._schemaObject; } }

        /// <summary>
        /// Returns 'true' if this SortableSchemaElement is associated with a Choice element.
        /// </summary>
        internal bool IsChoice                { get { return (this._choice != null); } }

        /// <summary>
        /// Constructor that creates a Sortable Schema Element from an Element declaration.
        /// </summary>
        /// <param name="schemaElement">The element to process</param>
        /// <param name="sequenceKey">Optional element sequence key (a value of 0 indicates that the key is undefined.</param>
        internal SortableSchemaElement(XmlSchemaElement schemaElement, int sequenceKey)
        {
            this._schemaObject = schemaElement;
            this._schemaObjectName = schemaElement.Name;
            this._schemaObjectIsElement = true;
            this._choice = null;
            this._sequenceKey = sequenceKey;
        }

        /// <summary>
        /// Constructor that creates a Sortable Schema Element from a Type definition.
        /// </summary>
        /// <param name="schemaType">The Type to process.</param>
        /// <param name="sequenceKey">Optional type sequence key (a value of 0 indicates that the key is undefined).</param>
        internal SortableSchemaElement(XmlSchemaType schemaType, int sequenceKey)
        {
            this._schemaObject = schemaType;
            this._schemaObjectName = schemaType.Name;
            this._schemaObjectIsElement = false;
            this._choice = null;
            this._sequenceKey = sequenceKey;
        }

        /// <summary>
        /// Constructor that creates a Sortable Schema Element from a Choice definition. A 'formal' XSD choice does not have a name, since it is
        /// represented by an XmlSchemaParticle. Therefor, we use our 'internal' Choice object here, since this contains a name (derived from the
        /// original 'ChoiceGroup' name assigned by the model).
        /// </summary>
        /// <param name="choice">The Choice to process.</param>
        /// <param name="sequenceKey">Optional type sequence key (a value of 0 indicates that the key is undefined.</param>
        internal SortableSchemaElement(XMLChoice choice, int sequenceKey)
        {
            this._schemaObject = null;
            this._schemaObjectName = choice.Name;
            this._schemaObjectIsElement = true;  //Choices end up in the element list of a class and thus have to be treated as elements.
            this._choice = choice;
            this._sequenceKey = sequenceKey;
        }

        /// <summary>
        /// The compare method compares two schema objects and determines their order in a schema (sub-)hierarchy:
        /// 1) First check is on Element versus Types: Elements always come first.
        /// 2) In case both objects are of equal type (both are element or both are types), we check whether Sequence Keys are
        ///    defined for both. A valid Sequence Key has a value != 0. If this is the case, the object with the lowest key
        ///    value comes first.
        /// 3) If neither of the other methods have resulted in an order, the sorting order is based on the name.
        /// </summary>
        /// <param name="other">The object to compare against.</param>
        /// <returns>-1: this precedes other; 0: this equals other, 1: this follows other.</returns>
        /// <exception cref="ArgumentException">Received object is of wrong type.</exception>
        public int CompareTo(Object other)
        {
            var otherElement = other as SortableSchemaElement;  // Prevents a whole lot of casting.
            if (otherElement == null)
            {
                throw new ArgumentException("XMLUtil.CompareTo: object '" + other.GetType() + "' is of wrong type!");
            }

            // First check is based om element vs. types. Elements should always come first! 
            // If I'm an element and the other is not, I will be first.
            // If the other is an element and I am not, I will be last.
            if (this._schemaObjectIsElement && !otherElement._schemaObjectIsElement) return -1;
            else if (!this._schemaObjectIsElement && otherElement._schemaObjectIsElement) return 1;

            // Next, we check the sequence key. If BOTH elements have a key value > 0, this defines the order. If either has a key value == 0
            // (not defined), we ignore the key and look at the names.
            if (this._sequenceKey != 0 && otherElement._sequenceKey != 0)
            {
                if (this._sequenceKey < otherElement._sequenceKey) return -1;
                else if (this._sequenceKey > otherElement._sequenceKey) return 1;
            }

            // In case either sequence key == 0 or their values are otherwise identical, we look at the name...
			return string.Compare(this._schemaObjectName, otherElement._schemaObjectName);
        }
    }
}
