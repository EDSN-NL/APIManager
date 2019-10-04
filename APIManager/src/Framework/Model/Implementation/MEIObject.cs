﻿using System;
using System.Collections.Generic;

namespace Framework.Model
{
   /// <summary>
    /// Model Element Implementation Class adds another layer of abstraction between the generic Model Element Implementation
    /// and the tool-specific implementation. This facilitates implementation of Model Element type-specific methods at this layer
    /// without the bridge interface needing tool-specific implementation logic.
    /// </summary>
    internal abstract class MEIObject : MEIClass
    {
        /// <summary>
        /// Returns the run-time state of the object, which is represented by a list of properties and their current value.
        /// The state only returns properties that have a value, empty ones are skipped!
        /// When no run-time state has been defined, the function returns an empty list.
        /// </summary>
        internal abstract List<Tuple<string, string>> GetRuntimeState();

        /// <summary>
        /// Returns the run-time state of the object, which is represented by a list of properties and their current value.
        /// The state only returns properties that have a value, empty ones are skipped!
        /// The existing run-time state of an object can be deleted by passing 'null' as a value, e.g. myObject.RuntimeState = null;
        /// </summary>
        /// <exception cref="ArgumentException">Illegal attributes in run-time state.</exception>
        internal abstract void SetRuntimeState(List<Tuple<string, string>> runtimeState);

        /// <summary>
        /// Default constructor, mainly used to pass the model instance to the base constructor and set the correct type.
        /// </summary>
        /// <param name="model">Reference to the associated model implementation object.</param>
        protected MEIObject (ModelImplementation model): base(model)
        {
            this._type = ModelElementType.Object;
        }
    }
}