using System;

namespace Framework.Util.SchemaManagement
{
    /// <summary>
    /// An Association class represents the "target end" of an ASBIE (an ASsociated Business Information Element). In the chosen model, each ABIE is 
    /// represented by a separate complex type and instances (elements) are only created for association elements. The only exception is the "root" 
    /// Message Assembly element. This method keeps the number of elements in a schema as low as possible, while still facilitating efficient mapping 
    /// since all elements share a common type model.
    /// SchemaAssociation is an abstract class since different schema types require different implementations.
    /// 
    /// Note: in case the cardinality of the association has an upper boundary > 1, a separate List element is created.
    /// If the cardinality lower boundary is 0, this list becomes an optional element. List contents always have a lower boundary of 1!
    /// </summary>
    internal abstract class SchemaAssociation
    {
        private bool _isValid;
        private string _ASBIEName;
        private string _roleName;
        private int _sequenceKey;
        private bool _isOptional;          // Set to 'true' in case minOccurs == 0.
        private ChoiceGroup _choiceGroup;

        /// <summary>
        /// Getters for Association properties:
        /// ChoiceGroup = Returns the Choice Group of this association (if any, NULL if undefined).
        /// IsChoiceElement = Returns 'true' is this association is part of a Choice group.
        /// IsOptional = Returns true if the association is optional (minOccurs == 0). This is the reverse operator of IsMandatory.
        /// IsMandatory = Returns true if the association must exist (minOccurs > 0). This is the reverse operator of IsOptional.
        /// ASBIEName = Returns the name of the association end (in fact, this is the classifier name).
        /// SequenceKey = Retrieve the sequence key for this association. Is used for sorting of elements within a constructed type (ABIE).
        /// Valid = identifies whether the association object is in valid state.
        /// </summary>
        internal ChoiceGroup ChoiceGroup
        {
            get { return this._choiceGroup; }
            set { this._choiceGroup = value; }
        }
        internal bool IsChoiceElement               { get { return this._choiceGroup != null; } }
        internal int SequenceKey                    { get { return this._sequenceKey; } }
        internal string RoleName                    { get { return this._roleName; } }
        internal bool IsOptional
        {
            get { return this._isOptional; }
            set { this._isOptional = value; }
        }
        internal bool IsMandatory
        {
            get { return !this._isOptional; }
            set { this._isOptional = !value; }
        }
        internal string ASBIEName
        {
            get { return _ASBIEName; }
            set { this._ASBIEName = value; }
        }
        internal bool IsValid
        {
            get { return this._isValid; }
            set { this._isValid = value; }
        }

        /// <summary>
        /// Default constructor, initializes local properties to provided values.
        /// </summary>
        protected SchemaAssociation(string roleName, string classifier, int sequenceKey, Tuple<int, int> cardinality, ChoiceGroup choiceGroup)
        {
            this._isValid = false;
            this._ASBIEName = classifier;
            this._sequenceKey = sequenceKey;
            this._isOptional = cardinality.Item1 == 0;
            this._choiceGroup = choiceGroup;
            this._roleName = roleName;
        }
    }
}
