using System;
using Framework.Exceptions;
using Framework.Util;

namespace Framework.Model
{
    /// <summary>
    /// Representation of an UML 'Attribute' artifact.
    /// </summary>
    internal class MEAttribute : ModelElement
    {
        /// <summary>
        /// Returns an integer representation of a 'Cardinality' string that should be interpreted as follows:
        /// - Single value 'n' is translated to 'exactly n', i.e. minOcc = maxOcc = 'n'. Unless 'n' == 0, in which case minOcc = 0, maxOcc = 1;
        /// - Single value '*' is translated to '0 to unbounded', represented by minOcc = maxOcc = 0;
        /// - Range 'n..m' is translated to minOcc = 'n', maxOcc = 'm'. Unless 'm' = 0, in which case maxOcc = 1. If this leads to minOcc > maxOcc, both values will be swapped!
        /// - Range 'n..*' is translated to minOcc = 'n', maxOcc = 0 (maxOcc == 0 is lateron interpreted as 'unbounded').
        /// All other formates will result in a response values of minOcc = maxOcc = -1.
        /// </summary>
        /// <returns>Tuple consisting of minOCC, maxOcc. In case of errors, both will be -1.</returns>
        /// <exception cref="MissingImplementationException">No implementation object exists.</exception>
        internal Cardinality Cardinality
        {
            get
            {
                if (this._imp != null) return ((MEIAttribute)this._imp).GetCardinality();
                else throw new MissingImplementationException("MEIAttribute");
            }
        }

        /// <summary>
        /// Return the classifier of this attribute (the attribute 'type').
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal MEDataType Classifier
        {
            get
            {
                if (this._imp != null) return ((MEIAttribute)this._imp).GetClassifier();
                else throw new MissingImplementationException("MEIAttribute");
            }
        }

        /// <summary>
        /// Reads or writes the default value of the attribute (if defined).
        /// An attribute has a valid default value only if the attribute is not defined as a constant.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal string DefaultValue
        {
            get
            {
                if (this._imp != null) return ((MEIAttribute)this._imp).GetDefaultValue();
                else throw new MissingImplementationException("MEIAttribute");
            }
            set
            {
                if (this._imp != null) ((MEIAttribute)this._imp).SetDefaultValue(value);
                else throw new MissingImplementationException("MEIAttribute");
            }
        }

        /// <summary>
        /// Reads or writes the fixed (constant) value of the attribute (if defined).
        /// An attribute has a valid fixed value only if the attribute is defined as a constant.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal string FixedValue
        {
            get
            {
                if (this._imp != null) return ((MEIAttribute)this._imp).GetFixedValue();
                else throw new MissingImplementationException("MEIAttribute");
            }
            set
            {
                if (this._imp != null) ((MEIAttribute)this._imp).SetFixedValue(value);
                else throw new MissingImplementationException("MEIAttribute");
            }
        }

        /// <summary>
        /// Reads or writes the index of the attribute within the owning class.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal int Index
        {
            get
            {
                if (this._imp != null) return ((MEIAttribute)this._imp).GetIndex();
                else throw new MissingImplementationException("MEIAttribute");
            }
            set
            {
                if (this._imp != null) ((MEIAttribute)this._imp).SetIndex(value);
                else throw new MissingImplementationException("MEIAttribute");
            }
        }

        /// <summary>
        /// Returns an indication whether the attribute is optional within the owning class. This implies that the
        /// 'lower bound' of the attribute cardinality must be '0'.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal bool IsOptional
        {
            get
            {
                if (this._imp != null) return ((MEIAttribute)this._imp).IsOptional();
                else throw new MissingImplementationException("MEIAttribute");
            }
        }

        /// <summary>
        /// Returns the class in which this attribute lives, i.e. that is owner of the class.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal MEClass OwningClass
        {
            get
            {
                if (this._imp != null) return ((MEIAttribute)this._imp).GetOwningClass();
                else throw new MissingImplementationException("MEIAttribute");
            }
        }

        /// <summary>
        /// Returns the position of the attribute, relative to other attributes within the 'owning' class. The key is incremental
        /// in steps of 100 (e.g. the first attribute has key 100, the second 200, etc.). 
        /// If the class is un-ordered, the key is always 100.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal int SequenceKey
        {
            get
            {
                if (this._imp != null) return ((MEIAttribute)this._imp).GetSequenceKey();
                else throw new MissingImplementationException("MEIAttribute");
            }
        }

        /// <summary>
        /// Construct a new UML 'Attribute' artifact by associating MEAttribute with the appropriate implementation object.
        /// The actual registration and construction is all performed by the base class constructor.
        /// </summary>
        /// <param name="attribID">Tool-specific instance identifier of the attribute artifact within the tool repository.</param>
        internal MEAttribute(int attribID) : base(ModelElementType.Attribute, attribID) { }

        /// <summary>
        /// Construct a new UML 'Attribute' artifact by associating MEAttribute with the appropriate implementation object.
        /// The actual registration and construction is all performed by the base class constructor.
        /// </summary>
        /// <param name="attribGUID">Globally unique instance identifier of the attribute artifact.</param>
        internal MEAttribute(string attribGUID) : base(ModelElementType.Attribute, attribGUID) { }

        /// <summary>
        /// Construct a new UML 'Attribute' artifact by associating MEAttribute with the given implementation object.
        /// The actual registration and construction is all performed by the base class constructor.
        /// </summary>
        /// <param name="imp">Class implementation object.</param>
        internal MEAttribute(MEIAttribute imp) : base(imp) { }

        /// <summary>
        /// Copy constructor creates a new Model Element Attribute from another Model Element Attribute.
        /// </summary>
        /// <param name="copy">Attribute to use as basis.</param>
        internal MEAttribute(MEAttribute copy) : base(copy) { }

        /// <summary>
        /// Default constructor creates an empty interface.
        /// </summary>
        internal MEAttribute() : base() { }

        /// <summary>
        /// Special constructor to be used by specialized classes that have to pass alternative type identifiers.
        /// </summary>
        /// <param name="type">Type of derived class.</param>
        /// <param name="classId">Instance ID of associated repository entry.</param>
        protected MEAttribute(ModelElementType type, int classId) : base(type, classId) { }

        /// <summary>
        /// Special constructor to be used by specialized classes that have to pass alternative type identifiers.
        /// </summary>
        /// <param name="type">Type of derived class.</param>
        /// <param name="classGUID">Globally unique instance ID of associated type.</param>
        protected MEAttribute(ModelElementType type, string classGUID) : base(type, classGUID) { }
    }
}
