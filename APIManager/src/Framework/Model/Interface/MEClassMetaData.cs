using System;
using System.Collections.Generic;
using Framework.Util;

namespace Framework.Model
{
    /// <summary>
    /// Helper class that manages attribute meta data.
    /// </summary>
    class MEAttributeMetaData: IComparable
    {
        private int _identifier;
        private string _name;
        private string _alias;
        private string _classifier;
        private Cardinality _cardinality;
        private char _type;

        /// <summary>
        /// Getters for the various properties.
        /// </summary>
        internal int Identifier     { get { return this._identifier; } }
        internal string Name        { get { return this._name; } }
        internal string Alias       { get { return this._alias; } }
        internal string Classifier  { get { return this._classifier; } }
        internal string Cardinality { get { return this._cardinality.ToString(); } }
        internal char Type          { get { return this._type; } }

        /// <summary>
        /// Create a new meta data record.
        /// </summary>
        /// <param name="ID">Attribute identifier, derived from position in repository and user-defined sequence numbers.</param>
        /// <param name="name">Attribute name.</param>
        /// <param name="alias">Attribute alias if defined.</param>
        /// <param name="classifier">Attribute classifier name.</param>
        /// <param name="card">Cardinality, second value '0' == infinite.</param>
        /// <param name="type">The type of attribute: 'C' = Content, 'F' = Facet, 'S' = Supplementary, 'A' = Association, 'G' = Generalization</param>
        internal MEAttributeMetaData(int ID, string name, string alias, string classifier, Cardinality card, char type)
        {
            this._identifier = ID;
            this._name = name;
            this._alias = alias;
            this._classifier = classifier;
            this._cardinality = card;
            this._type = type;
        }

        /// <summary>
        /// Define order of attribute metadata: first order by ID, then by name.
        /// </summary>
        /// <param name="other">Attribute Meta Data to compare against.</param>
        /// <returns>-1: this precedes other; 0: this equals other, 1: this follows other.</returns>
        /// <exception cref="ArgumentException">Received object is of wrong type.</exception>
        public int CompareTo(Object other)
        {
            var otherAttribMetaData = other as MEAttributeMetaData;  // Prevents a whole lot of casting.
            if (otherAttribMetaData == null)
            {
                throw new ArgumentException("Framework.Model.MEAttributeMetaData.CompareTo: object '" + other.GetType() + "' is of wrong type!");
            }

            // We first check the ID's, the lowest one wins. If ID's are equal, we compare the types, finally we compare names.
            if (this._identifier != otherAttribMetaData._identifier)
            {
                return this._identifier < otherAttribMetaData._identifier ? -1 : 1;
            }
            else if (this._type != otherAttribMetaData._type)
            {
                // Order by type, order is G -> C -> S -> F -> A
                if (this._type == 'G' && otherAttribMetaData._type != 'G') return -1;
                else if (this._type != 'G' && otherAttribMetaData._type == 'G') return 1;
                else if (this._type == 'C' && otherAttribMetaData._type != 'C') return -1;
                else if (this._type != 'C' && otherAttribMetaData._type == 'C') return 1;
                else if (this._type == 'S' && otherAttribMetaData._type != 'S') return -1;
                else if (this._type != 'S' && otherAttribMetaData._type == 'S') return 1;
                else if (this._type == 'F' && otherAttribMetaData._type != 'F') return -1;
                else if (this._type != 'F' && otherAttribMetaData._type == 'F') return 1;
                else if (this._type == 'A' && otherAttribMetaData._type != 'A') return -1;
                else return 1;
            }
            else return string.Compare(this._name, otherAttribMetaData._name);
        }
    }

    /// <summary>
    /// Simple helper class that is used to collect meta data of a Model Repository Class. This in turn can be used to provide insight in
    /// class properties such as Alias names, Stereotypes, Attribute- and Association order, etc.
    /// </summary>
    internal class MEClassMetaData
    {
        private string _className;
        private string _classAlias;
        private string _classStereoTypes;
        private SortedSet<MEAttributeMetaData> _attributes;

        /// <summary>
        /// Getters for the class properties.
        /// </summary>
        internal string Name                                { get { return this._className; } }
        internal string Alias                               { get { return this._classAlias; } }
        internal string Stereotypes                         { get { return this._classStereoTypes; } }
        internal SortedSet<MEAttributeMetaData> Attributes  { get { return this._attributes; } }

        /// <summary>
        /// Creates a new instance with given class properties.
        /// </summary>
        /// <param name="name">Class name.</param>
        /// <param name="alias">Optional class alias name.</param>
        /// <param name="stereotypes">Comma separated list of stereotypes for the class.</param>
        internal MEClassMetaData(string name, string alias, string stereotypes)
        {
            this._className = name;
            this._classAlias = alias;
            this._classStereoTypes = stereotypes;
            this._attributes = new SortedSet<MEAttributeMetaData>();
        }

        /// <summary>
        /// Creates a new attribute meta data record and registers it with the class. Attributes are ordered by ID, then by Type and
        /// finally by name.
        /// </summary>
        /// <param name="ID">Attribute identifier.</param>
        /// <param name="name">Attribute name.</param>
        /// <param name="alias">Attribute alias name.</param>
        /// <param name="classifier">Attribute classifier name.</param>
        /// <param name="card">Attribute cardinality tuple.</param>
        /// <param name="type">Attribute type (content, facet, etc.).</param>
        internal void AddAttribute(int ID, string name, string alias, string classifier, Cardinality card, char type)
        {
            this._attributes.Add(new MEAttributeMetaData(ID, name, alias, classifier, card, type));
        }
    }
}
