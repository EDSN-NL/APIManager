using System;
using System.Collections.Generic;
using Framework.Exceptions;
using Framework.Util;

namespace Framework.Model
{
    /// <summary>
    /// In our model, data types are specializations of the 'Class' artifact.
    /// </summary>
    internal class MEDataType : MEClass
    {
        /// <summary>
        /// Identifies the meta-type of this data type (Defined by an UML Profile):
        /// ComplexType = A data type that can be extended or reduced and can have mixed content;
        /// Enumeration = An enumerated data type;
        /// ExtSchemaType = specialized version of Complex Type, only used for external schema references;
        /// SimpleType = A data type that is associated with a primitive type that can not be extended or reduced;
        /// Union = A union data type (choice of different types);
        /// Unknown = Uninitialized type.
        /// </summary>
        internal enum MetaDataType { ComplexType, Enumeration, ExtSchemaType, SimpleType, Union, Unknown }

        /// <summary>
        /// Data types can have attributes, which are restricted to 'regular' types or enumerations. The 'getMetaData' method
        /// collects all meta data attributes on a given data type hierarchy according to some specific rules and returns the
        /// collected attribute data as a list of MetaDataDescriptors. This is effectively a 'stripped-down' portion of the
        /// attribute with a limited set of pre-fetched values.
        /// </summary>
        internal class MetaDataDescriptor
        {
            private string _name;
            private AttributeType _type;
            private MEDataType _classifier;
            bool _isOptional;
            List<MEDocumentation> _documentation;
            string _defaultValue;
            string _fixedValue;

            // Set of getters for retrieval of properties...
            internal string Name                          { get { return this._name; } }
            internal AttributeType Type                   { get { return this._type; } }
            internal MEDataType Classifier                { get { return this._classifier; } }
            internal bool IsOptional                      { get { return this._isOptional; } }
            internal List<MEDocumentation> Documentation  { get { return this._documentation; } }
            internal string DefaultValue                  { get { return this._defaultValue; } }
            internal string FixedValue                    { get { return this._fixedValue; } }

            /// <summary>
            /// Constructor initialises the descriptor with appropriate values.
            /// </summary>
            /// <param name="attrib">The attribute used as basis for the descriptor.</param>
            internal MetaDataDescriptor(MEAttribute attrib)
            {
                this._name = attrib.Name;
                this._type = ConvertModelElementType(attrib.Type);
                this._classifier = attrib.Classifier;
                this._isOptional = attrib.IsOptional;
                this._documentation = attrib.GetDocumentation();

                // Make sure that no leading/trailing whitespace remains (these are human input fields, anything can happen)...
                this._defaultValue = attrib.DefaultValue.Trim();
                this._fixedValue = attrib.FixedValue.Trim();
            }

            /// <summary>
            /// Since you can't directly cast ModelElementType to AttributeType (without loosing valid values), we need a small
            /// conversion function.
            /// </summary>
            /// <param name="meType">ModelElementType to convert.</param>
            /// <returns>Valid AttributeType.</returns>
            private AttributeType ConvertModelElementType (ModelElementType meType)
            {
                switch (meType)
                {
                    case ModelElementType.Attribute:        return AttributeType.Attribute;
                    case ModelElementType.Supplementary:    return AttributeType.Supplementary;
                    case ModelElementType.Facet:            return AttributeType.Facet;
                    default:                                return AttributeType.Unknown;
                }
            }
        }

        /// <summary>
        /// Return the meta-type of this data type.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal MetaDataType MetaType
        {
            get
            {
                if (this._imp != null) return ((MEIDataType)this._imp).MetaType;
                else throw new MissingImplementationException("MEIAttribute");
            }
        }

        /// <summary>
        /// Parses the class hierarchy of this data type and extracts all attributes (up to the PRIM level in the hierarchy)
        /// that comply with the following rules:
        /// - We select only Supplementary and Facet attributes that have either a fixed- or default value;
        /// - If an attribute is redefined at a lower level in the hierarchy, this is used instead of the more abstract versions;
        /// - If the data type has the 'useSupplementaries' tag set to 'true', all Supplementary and Facet attributes are copied
        /// for that class (as long as the previous rule is not broken).
        /// </summary>
        /// <returns>List of MetaDataDescriptor objects or empty list when nothing found.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal List<MetaDataDescriptor> GetMetaData()
        {
            if (this._imp != null) return ((MEIDataType)this._imp).GetMetaData();
            else throw new MissingImplementationException("MEIAttribute");
        }

        /// <summary>
        /// This method searches the class hierarchy for the Core Data Type (stereotype = CDT) of the current data type. It then
        /// returns the meta-type of this CDT as well as the name of the associated primitive data type (PRIM).
        /// The reason for this split is that, even though the primitive type itself is a simple type, the derived data type might be
        /// a complex type and we want to keep this knowledge (complex + simple = complex).
        /// </summary>
        /// <returns>Tuple containing both the meta-type and the type name.</returns>
        internal Tuple<MEDataType.MetaDataType, string> GetPrimitiveTypeName()
        {
            if (this._imp != null) return ((MEIDataType)this._imp).GetPrimitiveTypeName();
            else throw new MissingImplementationException("MEIAttribute");
        }

        /// <summary>
        /// Construct a new UML 'Data Type' artifact by associating MEDataType with the appropriate 
        /// implementation object.
        /// The actual registration and construction is all performed by the base class constructor.
        /// </summary>
        /// <param name="classID">Tool-specific instance identifier of the data type artifact within the tool repository.</param>
        internal MEDataType(int typeID) : base(ModelElementType.DataType, typeID) { }

        /// <summary>
        /// Construct a new UML 'Data Type' artifact by associating MEDataType with the given implementation object.
        /// The actual registration and construction is all performed by the base class constructor.
        /// </summary>
        /// <param name="imp">DataType implementation object.</param>
        internal MEDataType(MEIDataType imp) : base(imp) { }

        /// <summary>
        /// Copy constructor creates a new Model Element DataType from another Model Element DataType.
        /// </summary>
        /// <param name="copy">DataType to use as basis.</param>
        internal MEDataType(MEDataType copy) : base(copy) { }

        /// <summary>
        /// Default constructor creates an empty interface.
        /// </summary>
        internal MEDataType() : base() { }

        /// <summary>
        /// Special constructor to be used by specialized classes that have to pass alternative type identifiers.
        /// </summary>
        /// <param name="type">Type of derived class.</param>
        /// <param name="classId">Instance ID of associated repository entry.</param>
        protected MEDataType(ModelElementType type, int classId) : base(type, classId) { }
    }
}
