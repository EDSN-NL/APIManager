using System.Collections.Generic;
using Framework.Exceptions;

namespace Framework.Model
{
    /// <summary>
    /// Representation of an UML 'Enumerated Type' artifact. In our model, this is a specialization of the DataType
    /// model element.
    /// </summary>
    internal class MEEnumeratedType : MEDataType
    {
        /// <summary>
        /// Returns a list of all enumeration attributes of the type. These are the attributes that are NOT marked Supplementary or Facet.
        /// If the current class does not have any enumerated values, the method searches up the hierarchy until it finds a parent that does. Searching
        /// stops at that level, e.g. the 'most specialized class that contains enumerated values' wins.
        /// </summary>
        /// <returns>List of all attributes for the current type (can be empty if no attributes are defined).</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal List<MEAttribute> Enumerations
        {
            get
            {
                if (this._imp != null) return ((MEIEnumeratedType)this._imp).GetEnumerations();
                else throw new MissingImplementationException("MEIEnumeratedType");
            }
        }

        /// <summary>
        /// Construct a new UML 'Enumerated Type' artifact by associating MEEnumeratedType with the appropriate 
        /// implementation object.
        /// The actual registration and construction is all performed by the base class constructor.
        /// </summary>
        /// <param name="enumID">Tool-specific instance identifier of the enumeration artifact within the tool repository.</param>
        internal MEEnumeratedType(int enumID) : base(ModelElementType.Enumeration, enumID) { }

        /// <summary>
        /// Construct a new UML 'Enumerated Type' artifact by associating MEEnumeratedType with the given 
        /// implementation object.
        /// The actual registration and construction is all performed by the base class constructor.
        /// </summary>
        /// <param name="imp">Enumeration implementation object.</param>
        internal MEEnumeratedType(MEIEnumeratedType imp) : base(imp) { }

        /// <summary>
        /// Copy constructor creates a new Model Element enumeration from another Model Element emumeration.
        /// </summary>
        /// <param name="copy">Enumeration to use as basis.</param>
        internal MEEnumeratedType(MEEnumeratedType copy) : base(copy) { }

        /// <summary>
        /// Default constructor creates an empty interface.
        /// </summary>
        internal MEEnumeratedType() : base() { }
    }
}
