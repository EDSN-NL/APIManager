﻿using System;
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
        /// Returns the run-time state of the object, which is represented by a list of properties and their current value.
        /// The state only returns properties that have a value, empty ones are skipped!
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present.</exception>
        internal List<Tuple<string, string>> RunTimeState
        {
            get
            {
                if (this._imp != null) return ((MEIObject)this._imp).GetRunTimeState();
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
