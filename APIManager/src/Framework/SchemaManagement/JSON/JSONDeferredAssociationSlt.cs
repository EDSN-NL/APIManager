using System.Collections.Generic;

namespace Framework.Util.SchemaManagement.JSON
{
    /// <summary>
    /// The DeferredAssociation singleton is used to temporary store associations to classes that can not be resolved at the time
    /// of parsing the hierarchy. This is typically the result of "self associations". We temporary keep all details of the association
    /// in a separate structure till we fully parsed all classes in the hierarchy, after which we again attempt to create these
    /// associations. We keep a separate list per 'node' (in our context, 'node' is an arbitrary numeric identifier, which in practice
    /// will be the element-ID of the original source class.
    /// </summary>
    internal class JSONDeferredAssociationSlt
    {
        /// <summary>
        /// The actual data structure used to keep association properties
        /// </summary>
        internal struct AssociationProperties
        {
            /// <summary>
            /// The schema used to define the association
            /// </summary>
            internal JSONSchema Schema { get; }

            /// <summary>
            /// The endpoint role name (i.e. the source JSON attribute name).
            /// </summary>
            internal string AssociationName { get; }

            /// <summary>
            /// The name of the target ABIE
            /// </summary>
            internal string ClassifierName { get; }

            /// <summary>
            /// The association order identifier
            /// </summary>
            internal int SequenceKey { get; }

            /// <summary>
            /// Identification of the choice group to which the association belongs (if any).
            /// </summary>
            internal ChoiceGroup ChoiceGroup { get; }

            /// <summary>
            /// Cardinality of the association
            /// </summary>
            internal Cardinality Cardinality { get; }

            /// <summary>
            /// The schema in which the classifier has been defined (in case this differs from the source schema).
            /// </summary>
            internal JSONSchema ClassifierSchema { get; }

            /// <summary>
            /// Creates a cached set of association properties for an unresovable association, which we want to retry at a later time.
            /// </summary>
            /// <param name="schema">The schema in which the ASBIE is being created.</param>
            /// <param name="associationName">The name that will be assigned to the element (role name of the association).</param>
            /// <param name="classifierName">The name of the target ABIE type.</param>
            /// <param name="sequenceKey">The value of sequenceKey defines the order within a list of associations. Value 0 means 'inactive'</param>
            /// <param name="choiceGroup">Optional identifier of choicegroup that this association should go to (NULL is not defined).</param>
            /// <param name="cardinality">Association cardinality. An upper boundary of '0' is interpreted as 'unbounded'.</param>
            /// <param name="classifierSchema">Optional reference to external schema. If NULL, the classifier is referenced through 'schema.'</param>
            internal AssociationProperties(JSONSchema schema, string associationName, string classifierName, int sequenceKey,
                                           ChoiceGroup choiceGroup, Cardinality cardinality, JSONSchema classifierSchema)
            {
                this.Schema = schema;
                this.AssociationName = associationName;
                this.ClassifierName = classifierName;
                this.SequenceKey = sequenceKey;
                this.ChoiceGroup = choiceGroup;
                this.Cardinality = cardinality;
                this.ClassifierSchema = classifierSchema;
            }
        }

        // This is the actual Context singleton. It is created automatically on first load.
        private static readonly JSONDeferredAssociationSlt _deferredAssociationSlt = new JSONDeferredAssociationSlt();

        private SortedList<int, List<AssociationProperties>> _requestList;  // Stores all deferred association requests by NodeID;
        private bool _initialized = false;                                  // Keeps track of our state.
        private bool _purgingList = false;                                  // Set to true when we're processing the list.

        /// <summary>
        /// Public Context "factory" method. Simply returns the static instance.
        /// </summary>
        /// <returns>Context singleton object</returns>
        internal static JSONDeferredAssociationSlt GetJSONDeferredAssociationSlt() { return _deferredAssociationSlt; }

        /// <summary>
        /// Append a new association request to the list of deferred requests. We do NOT check whether we already have a similar request,
        /// since we assume that the original requests were unique in the first place.
        /// </summary>
        /// <param name="properties">Request properties to store.</param>
        /// <returns>True when the request has been added to our list, false when we ignored the request due to state error.</returns>
        internal bool AddAssociationRequest(int nodeID, AssociationProperties properties)
        {
            bool result = false;
            if (!this._initialized)
            {
                this._requestList = new SortedList<int, List<AssociationProperties>>();
                this._initialized = true;
                this._purgingList = false;
            }

            // We will only add requests when we're NOT processing our list in order to avoid endless loops when association
            // targets are 'really' missing.
            // Also, we keep a single list for each unique node ID
            if (!this._purgingList)
            {
                if (!this._requestList.ContainsKey(nodeID)) this._requestList.Add(nodeID, new List<AssociationProperties>());
                this._requestList[nodeID].Add(properties);
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Process the list of deferred associations for the given node ID, attempting to create valid associations for each entry.
        /// </summary>
        /// <param name="nodeID">The node for which we must process the associations</param>
        /// <returns>List of created associations, null in case of state errors (no list present).</returns>
        internal List<JSONAssociation> ProcessList(int nodeID)
        {
            if (this._initialized && this._requestList.ContainsKey(nodeID))
            {
                this._purgingList = true;
                var associations = new List<JSONAssociation>();
                foreach (var properties in this._requestList[nodeID])
                {
                    associations.Add(new JSONAssociation(nodeID, properties.Schema, properties.AssociationName, properties.ClassifierName, 
                                                         properties.SequenceKey, properties.ChoiceGroup, properties.Cardinality, 
                                                         properties.ClassifierSchema));
                }
                this._requestList.Remove(nodeID);
                this._purgingList = false;
                return associations;
            }
            return null;
        }

        /// <summary>
        /// Default constructor is private and thus can not be invoked directly!
        /// </summary>
        private JSONDeferredAssociationSlt()
        {
            this._requestList = null;
            this._initialized = false;
        }
    }
}
