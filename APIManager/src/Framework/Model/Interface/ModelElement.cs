using System;
using System.Collections.Generic;
using Framework.Logging;
using Framework.Util;
using Framework.Exceptions;

namespace Framework.Model
{
    /// <summary>
    /// These represent the various ModelElement implementation types that can exist:
    /// Association = connector between two classes, one of aggregation, composition or specialization;
    /// Attribute = a 'regular' attribute of a class;
    /// Class = an UML class;
    /// DataType = a classifier of an attribute;
    /// Enumeration = a specialized datatype that contains a given set of values;
    /// Facet = a specialized attribute that defines a facet of a class or data type;
    /// Object = an instantiation of a class;
    /// Package = a container for classes, diagrams or other packages;
    /// Profile = a model profiler (specialized class that is able to transform models);
    /// Supplementary = a specicialized attribute that defines supplementary characteristics of a class or data type;
    /// Union = a collection of attributes;
    /// Unknown = undefined type.
    /// </summary>
    internal enum ModelElementType    { Association, Attribute, Class, DataType, Enumeration, Facet, Object, Package, Profiler, Supplementary, Union, Unknown };
    internal enum AttributeType       { Attribute, Supplementary, Facet, Unknown};   // Sub-set used to define different attribute types.
    internal enum DataType            { DataType, Enumeration, Union, Unknown};      // Sub-set used to define different data types.

    /// <summary>
    /// Abstract base class for various modelling artifacts such as packages, classes, associations, attributes, etc.
    /// </summary>
    internal abstract class ModelElement: IDisposable, IEquatable<ModelElement>
    {
        // Two constants one might use in the 'SetTag' operation to clearly state intent for creation of new tag properties.
        internal const bool CreateNewTag   = true;
        internal const bool NoCreateNewTag = false;

        protected ModelElementImplementation _imp = null;   // The associated implementation object; does all the 'real' work.
        private bool _disposed;                             // Mark myself as invalid after call to dispose!

        /// <summary>
        /// Getters for some of the simple element's virtual properties...
        /// These return NULL, -1 or empty string (depending on the getter) in case of problems.
        /// </summary>
        internal int ElementID     {get { return (this._imp != null)? this._imp.ElementID : -1; }}
        internal string GlobalID   {get { return (this._imp != null)? this._imp.GlobalID : string.Empty; }}
        internal bool Valid        {get { return (this._imp != null) && this._imp.Valid; } }
        internal ModelElementType Type {get { return (this._imp != null)? this._imp.Type : ModelElementType.Unknown; }}
        internal ModelElementImplementation Implementation { get { return this._imp; } }

        /// <summary>
        /// Get- or set the alternative name of the model element.
        /// </summary>
        internal string AliasName 
        { 
            get 
            { 
                return (this._imp != null) ? this._imp.AliasName : string.Empty; 
            }
            set
            {
                if (this._imp != null) this._imp.AliasName = value;
                else throw new MissingImplementationException("ModelElementImplementation");
            }
        }

        /// <summary>
        /// Reads or writes the name of the element.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal string Name
        {
            get
            {
                if (this._imp != null) return this._imp.Name;
                else throw new MissingImplementationException("ModelElementImplementation");
            }
            set
            {
                if (this._imp != null) this._imp.Name = value;
                else throw new MissingImplementationException("ModelElementImplementation");
            }
        }

        /// <summary>
        /// Reads or write element annotation text.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal string Annotation
        {
            get
            {
                if (this._imp != null) return this._imp.GetAnnotation();
                else throw new MissingImplementationException("ModelElementImplementation");
            }
            set
            {
                if (this._imp != null) this._imp.SetAnnotation(value);
                else throw new MissingImplementationException("ModelElementImplementation");
            }
        }

        /// <summary>
        /// Returns all fully-qualified stereotype names owned by the associated model element. This could be an empty list if
        /// neither of the assigned stereotypes originates from a profile!
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal List<string> FQStereotypes
        {
            get
            {
                if (this._imp != null) return this._imp.GetFQStereotypes();
                else throw new MissingImplementationException("ModelElementImplementation");
            }
        }

        /// <summary>
        /// Adds the specified stereotype to the current model element.
        /// </summary>
        /// <param name="stereotype">Stereotype to add.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal void AddStereotype(string stereotype)
        {
            if (this._imp != null) this._imp.AddStereotype(stereotype);
            else throw new MissingImplementationException("ModelElementImplementation");
        }

        /// <summary>
        /// This is the normal entry for all users of the object that want to indicate that the interface is not required anymore.
        /// Use with extreme caution since references to the interface might still be around, which will not work anymore after calling
        /// Dispose!
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Retrieves a list of documentation nodes from the model element. The function returns null on errors.
        /// </summary>
        /// <returns>List of documentation nodes or NULL in case of errors.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal List<MEDocumentation> GetDocumentation()
        {
            if (this._imp != null) return this._imp.GetDocumentation();
            else throw new MissingImplementationException("ModelElementImplementation");
        }

        /// <summary>
        /// Retrieves the value of the tag with given name. If tag not found or no implementation is present, the function returns an
        /// empty string.
        /// </summary>
        /// <param name="tagName">Name of tag to be retrieved.</param>
        /// <returns>Value of tag or empty string in case of tag not found (or no implementation)</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal string GetTag(string tagName)
        {
            if (this._imp != null) return this._imp.GetTag(tagName);
            else throw new MissingImplementationException("ModelElementImplementation");
        }

        /// <summary>
        /// Checks whether the ModelElement has one or more of the stereotypes out of the presented list.
        /// The method returns true if at least one name exists and false when there are no matches or there is no implementation.
        /// </summary>
        /// <param name="stereotypes">List of stereotype names.</param>
        /// <returns>True if at least one of the stereotypes of the list is owned by the element, false otherwise.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal bool HasStereotype(List<string> stereotypes)
        {
            if (this._imp != null) return this._imp.HasStereotype(stereotypes);
            else throw new MissingImplementationException("ModelElementImplementation");
        }

        /// <summary>
        /// Checks whether the ModelElement has the stereotype with the given name.
        /// The method returns true if the stereotype exists and false when there are no matches or there is no implementation.
        /// </summary>
        /// <param name="stereotype">Stereotype to add.</param>
        /// <returns>True if the element owns the specified stereotype, false otherwise.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal bool HasStereotype(string stereotype)
        {
            if (this._imp != null) return this._imp.HasStereotype(stereotype);
            else throw new MissingImplementationException("ModelElementImplementation");
        }

        /// <summary>
        /// Marks the associated implementation (if present) as 'invalid'.
        /// </summary>
        internal void InValidate()
        {
            if (this._imp != null) this._imp.InValidate();
        }

        /// <summary>
        /// Instructs the model element to refresh itself, e.g. after a model change outside scope of the plugin.
        /// </summary>
        internal void RefreshModelElement()
        {
            if (this._imp != null) this._imp.RefreshModelElement();
            else throw new MissingImplementationException("ModelElementImplementation");
        }

        /// <summary>
        /// Set the value of the specified tag. If the tag does not exist (or there is no implementation), the method fails silently.
        /// If the 'createIfNotExists' flag is set to 'true' and the tag is not found, it is created with the given value.
        /// </summary>
        /// <param name="tagName">Name of tag to be set.</param>
        /// <param name="tagValue">The (new) value of the tag.</param>
        /// <param name="createIfNotExist">When set to to 'true', the tag is created if it could not be found.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal void SetTag(string tagName, string tagValue, bool createIfNotExist = NoCreateNewTag)
        {
            if (this._imp != null) this._imp.SetTag(tagName, tagValue, createIfNotExist);
            else throw new MissingImplementationException("ModelElementImplementation");
        }

        /// <summary>
        /// Override method that compares an ModelElement with another Object. Returns true if both elements are of 
        /// identical type and share the same implementation object.
        /// </summary>
        /// <param name="obj">The thing to compare against.</param>
        /// <returns>True if same object, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var objElement = obj as ModelElement;
			return (objElement != null) && Equals(objElement);
        }

        /// <summary>
        /// Compares one Model Element with another, returning true if the two elements have the same repository ID OR have the same
        /// type and name. This implies that the function checks for either identical- or similar objects.
        /// </summary>
        /// <param name="other">ModelElement to compare against.</param>
        /// <returns>TRUE is similar object, false otherwise.</returns>
        public bool Equals(ModelElement other)
        {
            if (other != null && this._imp != null && other._imp != null)
            {
                return (this.ElementID == other.ElementID) || ((this.Type == other.Type) && (this.Name == other.Name));
            }
            else return false;
        }

        /// <summary>
        /// Determine a hash of the ModelElement, which uses the hash function of the ModelElement GlobalID.
        /// </summary>
        /// <returns>Hash of ModelElement globalID</returns>
        public override int GetHashCode()
        {
            return GlobalID.GetHashCode();
        }

        /// <summary>
        /// Override of compare operator. Two ModelElement objects are equal if they share the same implementation
        /// object or if they are both NULL or if the interface objects are identical.
        /// </summary>
        /// <param name="elementa">First ModelElement to compare.</param>
        /// <param name="elementb">Second ModelElement to compare.</param>
        /// <returns>True if both elements share the same implementation object (or neither has an implementation object),
        /// false otherwise.</returns>
        public static bool operator ==(ModelElement elementa, ModelElement elementb)
        {
            // Tricky to implement correctly. These first statements make sure that we check whether we are actually
            // dealing with identical objects and/or whether one or both are NULL.
            if (ReferenceEquals(elementa, elementb)) return true;
            if (ReferenceEquals(elementa, null)) return false;
            if (ReferenceEquals(elementb, null)) return false;

            // We have two different interface instances, now check whether they share the same implementation....
            return elementa.Equals(elementb);
        }

        /// <summary>
        /// Override of compare operator. Two ModelElement objects are different if they have different implementation
        /// objects or if one of them is passed as NULL.
        /// </summary>
        /// <param name="elementa">First ModelElement to compare.</param>
        /// <param name="elementb">Second ModelElement to compare.</param>
        /// <returns>True if both elements have different implementation objects, (or one is missing an implementation 
        /// object), false otherwise.</returns>
        public static bool operator !=(ModelElement elementa, ModelElement elementb)
        {
            return !(elementa == elementb);
        }

        /// <summary>
        /// Override method, returns a string representation of the model element, which is the name of the element.
        /// </summary>
        /// <returns>Element name.</returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// The ModelElement constructor performs the work of actually obtaining the proper implementation object and
        /// registering the occurence of a new interface by incrementing the interface reference count at the implementation.
        /// </summary>
        /// <param name="type">Type of the derived ModelElement, defines the implementation object.</param>
        /// <param name="elementID">Unique ID of the object within the Tool Repository.</param>
        protected ModelElement(ModelElementType type, int elementID)
        {
            this._disposed = false;
            this._imp = ModelSlt.GetModelSlt().GetModelElementImplementation(type, elementID);
            if (this._imp != null)
            {
                // We don't have to check here for different implementations, since we have created the implementation through the model
                // interface, which checks the list of registered implementations...
                ModelElementImplementation refImp = this._imp.AddReference();
            }
            else
            {
                Logger.WriteError("Framework.Model.ModelElement >> Could not obtain implementation object of type: '" +
                                  type + "' and instance ID: '" + elementID + "'!");
            }
        }

        /// <summary>
        /// The ModelElement constructor performs the work of actually obtaining the proper implementation object and
        /// registering the occurence of a new interface by incrementing the interface reference count at the implementation.
        /// </summary>
        /// <param name="type">Type of the derived ModelElement, defines the implementation object.</param>
        /// <param name="elementGUID">Globally Unique ID of the object within the Tool Repository.</param>
        protected ModelElement(ModelElementType type, string elementGUID)
        {
            this._disposed = false;
            this._imp = ModelSlt.GetModelSlt().GetModelElementImplementation(type, elementGUID);
            if (this._imp != null)
            {
                // We don't have to check here for different implementations, since we have created the implementation through the model
                // interface, which checks the list of registered implementations...
                ModelElementImplementation refImp = this._imp.AddReference();
            }
            else
            {
                Logger.WriteError("Framework.Model.ModelElement >> Could not obtain implementation object of type: '" +
                                  type + "' and instance GUID: '" + elementGUID + "'!");
            }
        }

        /// <summary>
        /// Create a new Model Element based on a provided implementation object.
        /// </summary>
        /// <param name="imp">The implementation to be used.</param>
        protected ModelElement(ModelElementImplementation imp)
        {
            this._disposed = false;
            this._imp = imp;
            if (this._imp != null)
            {
                // The addReference method checks whether the implementation is properly registered. If a duplicate is found, this is 
                // returned instead of the original implementation and the interface must switch to assure that it used the registered
                // instance. This check is only required with implementation objects of 'unknown' source.
                ModelElementImplementation registeredImp = this._imp.AddReference();
                if (!ReferenceEquals(this._imp, registeredImp)) this._imp = registeredImp;
            }
            else
            {
                Logger.WriteError("Framework.Model.ModelElement >> Empty implementation passed!");
            }
        }

        /// <summary>
        /// Public copy constructor, creates a new Model Element based on an existing one.
        /// Assures that reference counter is updated correctly on copy.
        /// </summary>
        /// <param name="copy">Original to copy from.</param>
        protected ModelElement(ModelElement copy)
        {
            this._disposed = false;
            this._imp = copy._imp;
            if (this._imp != null)
            {
                // We don't have to check here for different implementations, since we have copied the implementation from
                // an interface object and interfaces are guaranteed to always contain a valid implementation.
                this._imp.AddReference();
            }
            else
            {
                Logger.WriteError("Framework.Model.ModelElement >> Copy constructor received empty implementation!");
            }
        }

        /// <summary>
        /// Default constructor just initialises to empty.
        /// </summary>
        protected ModelElement()
        {
            this._disposed = false;
            this._imp = null;
        }

        /// <summary>
        /// The destructor is declared as a safeguard to assure that the reference counter is decreased when the object is
        /// garbage collected. It's not failsafe since the moment of invocation is not guaranteed.
        /// </summary>
        ~ModelElement()
        {
            Dispose(false);
        }

        /// <summary>
        /// This is the actual disposing interface, which takes case of structural removal of the implementation type when no longer
        /// needed.
        /// </summary>
        /// <param name="disposing">Set to 'true' when called directly. Set to 'false' when called from the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                try
                {
                    if (this._imp != null)
                    {
                        this._imp.RemoveReference();
                        this._imp = null;
                    }
                    this._disposed = true;
                }
                catch { };   // Ignore any exceptions, no use in processing them here.
            }
        }
    }
}
