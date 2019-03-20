using System;
using System.IO;
using System.Collections.Generic;
using Framework.Model;
using Framework.Util;
using Framework.Util.SchemaManagement;

namespace Plugin.Application.CapabilityModel.ASCIIDoc
{
    /// <summary>
    /// A documentation context can be considered a 'chapter' in a document. Each context contains class- as well as classifier documentation.
    /// A context is formatted as:
    /// - Title.
    /// - Header documentation.
    /// - Class documentation (tree of class-doc. nodes).
    /// - Classifiers defined within this context (data types).
    /// </summary>
    internal abstract class DocContext: IComparable<DocContext>
    {
        protected ClassDocNode _currentNode;            // The currently active documentation node (class that's currently being processed).
        protected string _contextID;                    // Unique identification, used to create the correct anchors.

        protected SortedList<string, ClassifierDocNode> _classifiers;   // Contains all classifiers documentation for the current context.
        protected SortedList<string, CrossReference> _xrefList;         // All cross-references for this documentation context.
        protected string _name;                         // Context name (chapter title).
        protected string _headerText;                   // Introduction text.
        protected int _level;                           // Indentation level of this particular context.

        /// <summary>
        /// Returns the name of the operation documentation context (typically, this is the associated Operation name).
        /// </summary>
        internal string Name { get { return this._name; } }

        /// <summary>
        /// Returns the unique context identifier of this documentation context.
        /// </summary>
        internal string ContextID { get { return this._contextID; } }

        /// <summary>
        /// This method adds an association with another class as an ASCIIDoc attribute description (since associations can be considered
        /// structured attributes). The classifier is formatted as an anchor since we want to be able to drill-down.
        /// </summary>
        /// <param name="name">Role name of the association, defines the 'attribute name' in the source class.</param>
        /// <param name="classifierName">Name of the target class, which acts as a classifier for the attribute.</param>
        /// <param name="classifierContextID">The context in which this class is defined (by definition, namespace token of target context).</param>
        /// <param name="cardinality">Cardinality of the association as a string.</param>
        /// <param name="notes">Notes associated with the role.</param>
        internal void AddAssociation(string name, string classifierName, string classifierContextID, Cardinality cardinality, string notes)
        {
            if (this._currentNode != null) this._currentNode.AddAssociation(name, classifierName, classifierContextID, cardinality, notes);
        }

        /// <summary>
        /// This method adds a simple attribute as an ASCIIDoc attribute description. The classifier is formatted as an anchor 
        /// since we want to be able to drill-down.
        /// </summary>
        /// <param name="name">Attribute name.</param>
        /// <param name="type">Metatype of attribute.</param>
        /// <param name="classifierName">Attribute classifier name.</param>
        /// <param name="classifierContextID">The context in which the classifier is defined (by definition, namespace token of target context).</param>
        /// <param name="cardinality">Cardinality of the attribute as a string.</param>
        /// <param name="notes">Notes associated with the attribute.</param>
        internal void AddAttribute(string name, AttributeType type, string classifierName, string classifierContextID, Cardinality cardinality, string notes)
        {
           if (this._currentNode != null) this._currentNode.AddAttribute(name, type, classifierName, classifierContextID, cardinality, notes);
        }

        /// <summary>
        /// Create a new ASCIIDoc documentation entry for a classifier. Each instantiated ClassifierDocNode represents a single classifier.
        /// The contextID is used to create context-bound anchors for the classifier, which facilitates cross-referencing between different contexts.
        /// Anchors are defined as 'id_name' (e.g. 'cmn_countertype').
        /// </summary>
        /// <param name="contextID">Context identifier, by definition, this is the namespace token of the namespace associated with the context.</param>
        /// <param name="classifier">Classifier definition.</param>
        /// <param name="facetList">Optional list of facets for this classifier.</param>
        /// <param name="attribList">Optional list of supplementary attributes for this classifier.</param>
        /// <param name="primName">Name of the primary type associated with the classifier.</param>
        /// <param name="qualifiedName">The classifier name that we must use in the documentation (if NULL, use classifier.Name instead).</param>
        /// <exception cref="ArgumentException">Class is already in list for this context.</exception>
        internal void AddClassifier(MEDataType classifier, List<Facet> facetList, List<SupplementaryAttribute> attribList, string primName, string qualifiedName = null)
        {
            if (!this._classifiers.ContainsKey(classifier.Name))
            {
                this._classifiers.Add(classifier.Name, new ClassifierDocNode(this._contextID, classifier, facetList, attribList, primName, qualifiedName, GetClassifierLevel()));
            }
            else
            {
               // Logger.WriteError("Plugin.Application.CapabilityModel.ASCIIDoc.DocContext.AddClassifier >> Duplicate classifier '" + classifier.Name +
               //                   "' in context '" + this._contextID + ":" + this._name + "'!");
               // throw new ArgumentException("Duplicate classifier '" + classifier.Name + "' in context '" + this._contextID + ":" + this._name + "'!");
            }
        }

        /// <summary>
        /// Add a cross-referenced item to the current documentation context. If the item has not been referenced before, it is created. Otherwise,
        /// the specified source is added as a new cross-reference. 
        /// The provided sourceScope should have a suitable suffix that allows it to be appended to the sourceName without additional linking
        /// characters.
        /// <param name="target">The name of the referenced item.</param>
        /// <param name="sourceContextID">By definition, this is the NS token of the source class.</param>
        /// <param name="sourceScope">Human-readable identification of the source scope (where is it defined).</param>
        /// <param name="sourceName">Source human-readable name.</param>
        internal void AddXREF(string target, string sourceContextID, string sourceScope, string sourceName)
        {
            string anchor = sourceContextID.ToLower() + "_" + sourceName.ToLower();
            if (!this._xrefList.ContainsKey(target)) this._xrefList.Add(target, new CrossReference(target));
            this._xrefList[target].AddReference(anchor, sourceScope + sourceName);
        }

        /// <summary>
        /// Can be used to compare two DocContext objects for sorting. Compare is based on the name of the DocContext.
        /// </summary>
        /// <param name="other">Object to compare against.</param>
        /// <returns>0 if both are equal, 1 in case this object is greater then other, -1 in case this is smaller then other.</returns>
        public int CompareTo(DocContext other)
        {
            return this._name.CompareTo(other._name);
        }

        /// <summary>
        /// Specialized classes must implement this method, which writes the ASCIIDoc formatted contents to the specified file stream.
        /// </summary>
        /// <param name="stream">Stream that must receive the ASCIIDoc contents.</param>
        internal abstract void Save(StreamWriter stream);

        /// <summary>
        /// Creates a new context with specified ID, name and header text.
        /// </summary>
        /// <param name="contextID">Unique ID of this context. By definition, this must be the namespace token of the associated namespace.</param>
        /// <param name="contextName">The name of the context.</param>
        /// <param name="headerText">Header text for the context.</param>
        /// <param name="level">Defines the indentation level of this documentation context within the overall document.</param>
        protected DocContext(string contextID, string contextName, string headerText, int level)
        {
            this._classifiers = new SortedList<string, ClassifierDocNode>();
            this._xrefList = new SortedList<string, CrossReference>();
            this._currentNode = null;
            this._name = contextName;
            this._headerText = headerText;
            this._contextID = contextID;
            this._level = level;
        }

        /// <summary>
        /// Since Classifiers can be created at different indentation levels, we leave the construction of the correct level to the
        /// specialized classes.
        /// </summary>
        /// <returns>Indentation level for current classifier.</returns>
        protected abstract int GetClassifierLevel();
    }
}
