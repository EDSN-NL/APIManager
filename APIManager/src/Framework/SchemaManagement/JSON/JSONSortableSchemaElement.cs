using System;
using System.Collections.Generic;
using Newtonsoft.Json.Schema;
using Framework.Logging;

namespace Framework.Util.SchemaManagement.JSON
{
    /// <summary>
    /// This is a helper class that is used as a key in the creation of sorted lists, which are used by the schema logic to assure that schema 
    /// elements are sorted in the correct order within either the schema files themselves, but also within an aggregated type (element order).
    /// For JSON Schemas, we use the following order:
    /// 1) All JSON Objects marked as 'RootClass' are defined at the top of the schema.
    /// 2) All JSON constructs (Object or otherwise) that are marked as 'Type' go next in order to assure proper referencing.
    /// 3) All other JSON Objects follow next, ordered by sequence key or alphabetically.
    /// 4) Within a constructed type, Properties are ordered by either sequence key and/or name.
    /// Since Root Classes are kept in a separate list, they are not considered by the Sortable Schema Element class.
    /// </summary>
    internal class SortableSchemaElement : IComparable
    {
        /// <summary>
        /// We can sort different things:
        /// 1) Classes are constructed types that comprise the main body of the schema;
        /// 2) Types are classifiers that we want at the bottom of the schema;
        /// 3) Properties are the attributes of a class.
        /// </summary>
        internal enum SortingContext { Class, Type, Property }

        private int _sequenceKey;           // We add another key for alternative sorting order.
        private SortingContext _context;    // Defines the context of this Sortable Element.
        private string _sortingName;        // The name against which we want to sort (context dependent).

        /// <summary>
        /// Returns the name associated with this sortable element.
        /// </summary>
        internal string Name { get { return this._sortingName; } }

        /// <summary>
        /// Constructor that creates a Sortable Schema Element for a property (either Content- or Supplementary attribute,
        /// association or choice).
        /// Sorting is performed on sequence key and attribute name.
        /// </summary>
        /// <param name="attrib">The attribute to be sorted.</param>
        internal SortableSchemaElement(IJSONProperty property)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.SortableSchemaElement >> Constructing sortable element for property: '[" + 
                              property.SequenceKey + "]" + property.Name + "' in Context: 'Property'...");;
            this._sequenceKey = property.SequenceKey;
            this._context = SortingContext.Property;
            this._sortingName = property.Name;
        }

        /// <summary>
        /// Create a Sortable Schema Element for a JSchema object.
        /// </summary>
        /// <param name="schemaObject">The target object.</param>
        internal SortableSchemaElement(JSchema schemaObject)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.SortableSchemaElement >> Constructing sortable element for a schema object '" + schemaObject.Title + "'...");
            this._sequenceKey = 0;
            this._context = SortingContext.Class;
            this._sortingName = schemaObject.Title;
        }

        /// <summary>
        /// Create a Sortable Schema Element for a classifier.
        /// </summary>
        /// <param name="classifier">The target classifier.</param>
        internal SortableSchemaElement(JSONClassifier classifier)
        {
            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.SortableSchemaElement >> Constructing sortable element for a classifier '" + classifier.Name + "'...");
            this._sequenceKey = 0;
            this._context = SortingContext.Type;
            this._sortingName = classifier.Name;
        }

        /// <summary>
        /// Create a Sortable Schema Element by explicitly providing the sorting components. 
        /// </summary>
        /// <param name="name">Name on which to sort.</param>
        /// <param name="context">The sorting context.</param>
        /// <param name="sequenceKey">Sequence number.</param>
        internal SortableSchemaElement(string name, SortingContext context, int sequenceKey)
        {
            this._sequenceKey = sequenceKey;
            this._context = context;
            this._sortingName = name;
        }

        /// <summary>
        /// The compare method compares two schema objects and determines their order in a schema (sub-)hierarchy:
        /// Order is defined by Context, Sequence Key and name:
        /// 1) First, we check whether one of the objects is a Classifier. These go first.
        /// 2) Next, we check sequence keys and if these are not consistent, we check the names.
        /// </summary>
        /// <param name="other">The object to compare against.</param>
        /// <returns>-1: this precedes other; 0: this equals other, 1: this follows other.</returns>
        /// <exception cref="ArgumentException">Received object is of wrong type.</exception>
        public int CompareTo(Object other)
        {
            var otherElement = other as SortableSchemaElement;  // Prevents a whole lot of casting.
            if (otherElement == null)
            {
                throw new ArgumentException("JSONUtil.CompareTo: object '" + other.GetType() + "' is of wrong type!");
            }

            Logger.WriteInfo("Framework.Util.SchemaManagement.JSON.SortableSchemaElement.CompareTo >> Comparing this: '[" +
                             this._sequenceKey + "]" + this._sortingName + "' with other: '[" + otherElement._sequenceKey + "]" + otherElement._sortingName + "'...");

            // If I'm a Classifier and the other is not, I want to go first. This guarantees that type definitions come before Class definitons.
            if (this._context == SortingContext.Type && !(otherElement._context == SortingContext.Type)) return -1;
            else if (!(this._context == SortingContext.Type) && otherElement._context == SortingContext.Type) return 1;

            // Next, we check the sequence key. If BOTH elements have a key value > 0, this defines the order. If either has a key value == 0
            // (not defined), we ignore the key and look at the names.
            // We don't have to check for Types, this is simply 'NOT a MasterClass AND NOT a Class'.
            // And the sequenceKey / Name check also helps if both objects are of the same type.
            if (this._sequenceKey != 0 && otherElement._sequenceKey != 0)
            {
                if (this._sequenceKey < otherElement._sequenceKey) return -1;
                else if (this._sequenceKey > otherElement._sequenceKey) return 1;
            }

            // Finally, in case all previous checks failed, we look at the item names...
            // If the name starts with a '@', we're dealing with a Supplementary attribute and these must go first...
            if (this._sortingName.StartsWith("@") && !(otherElement._sortingName.StartsWith("@"))) return -1;
            else if (!(this._sortingName.StartsWith("@")) && otherElement._sortingName.StartsWith("@")) return 1;

			return string.Compare(this._sortingName, otherElement._sortingName);
        }
    }
}
