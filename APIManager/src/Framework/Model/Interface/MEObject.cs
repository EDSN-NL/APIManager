using System;
using System.Collections.Generic;
using Framework.Exceptions;

namespace Framework.Model
{
    /// <summary>
    /// Representation of an UML 'Object' artifact (instance of a class). Since it shares most of its methods with the 'MEClass' 
    /// element, object is derived from class.
    /// </summary>
    internal class MEObject: MEClass
    {
        /// <summary>
        /// Returns or loads the run-time state of the object, which is represented by a list of properties and their current value.
        /// The state only returns properties that have a value, empty ones are skipped! When the object does not have a run-time
        /// state, the 'get' operation returns an empty list.
        /// For a 'set' operation, the list of provided attribute names is checked against the valid attributes of the associated class!
        /// The existing run-time state of an object can be deleted by passing 'null' as a value, e.g. myObject.RuntimeState = null;
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present.</exception>
        /// <exception cref="ArgumentException">Illegal attributes in run-time state ('set' only).</exception>
        internal List<Tuple<string, string>> RuntimeState
        {
            get
            {
                if (this._imp != null) return ((MEIObject)this._imp).GetRuntimeState();
                else throw new MissingImplementationException("MEIObject");
            }
            set
            {
                if (this._imp != null) ((MEIObject)this._imp).SetRuntimeState(value);
                else throw new MissingImplementationException("MEIObject");
            }
        }

        /// <summary>
        /// Construct a new UML 'Object' artifact by associating MEObject with the appropriate implementation object.
        /// The actual registration and construction is all performed by the base class constructor.
        /// </summary>
        /// <param name="objectID">Tool-specific instance identifier of the object artifact within the tool repository.</param>
        internal MEObject(int objectID) : base(ModelElementType.Object, objectID) { }

        /// <summary>
        /// Construct a new UML 'Object' artifact by associating MEObject with the appropriate implementation object.
        /// The actual registration and construction is all performed by the base class constructor.
        /// </summary>
        /// <param name="objectGUID">Globally unique instance identifier of the object artifact.</param>
        internal MEObject(string objectGUID) : base(ModelElementType.Object, objectGUID) { }

        /// <summary>
        /// Construct a new UML 'Object' artifact by associating MEObject with the given implementation object.
        /// The actual registration and construction is all performed by the base class constructor.
        /// </summary>
        /// <param name="imp">Object implementation object.</param>
        internal MEObject(MEIObject imp) : base(imp) { }

        /// <summary>
        /// Copy constructor creates a new Model Element object from another Model Element object.
        /// </summary>
        /// <param name="copy">Object to use as basis.</param>
        internal MEObject(MEObject copy) : base(copy) { }

        /// <summary>
        /// Default constructor creates an empty interface.
        /// </summary>
        internal MEObject() : base() { }
    }
}
