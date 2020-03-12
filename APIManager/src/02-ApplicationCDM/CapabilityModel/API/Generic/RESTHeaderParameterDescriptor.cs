using System;
using Framework.Model;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// Defines the structure of a single header parameter.
    /// </summary>
    internal sealed class RESTHeaderParameterDescriptor : IEquatable<RESTHeaderParameterDescriptor>
    {
        // The status is used to track operational state of the descriptor. The descriptor has 'valid' state when all properties
        // are consistent in relationship with each other.
        internal enum DeclarationStatus { Invalid, Valid }

        private int _ID;                                // Unique parameter identifier (within collection).
        private RESTParameterDeclaration _parameter;    // The actual parameter is represented by this parameter declaration object.
        private DeclarationStatus _status;              // Descriptor status with regard to user editing.

        /// <summary>
        /// Returns the parameter description, taken directly from the parameter declaration object.
        /// </summary>
        internal string Description { get { return this._parameter.Description; } }

        /// <summary>
        /// Returns the unique identifier of this header parameter.
        /// </summary>
        internal int ID { get { return this._ID; } }

        /// <summary>
        /// Get or set the name of the descriptor (identifies the header parameter). The name is taken directly from the
        /// parameter declaration object.
        /// </summary>
        internal string Name
        {
            get { return this._parameter.Name; }
            set
            {
                if (this._parameter.Name != value)
                {
                    this._parameter.Name = value;
                    DetermineStatus();
                }
            }
        }

        /// <summary>
        /// Returns the actual parameter object.
        /// </summary>
        internal RESTParameterDeclaration Parameter
        {
            get { return this._parameter; }
        }

        /// <summary>
        /// Returns 'true' if the declaration record has a valid, consistent, status.
        /// </summary>
        internal bool IsValid { get { return this._status == DeclarationStatus.Valid; } }

        /// <summary>
        /// Compares the descriptor against another object. If the other object is also a Header Parameter Descriptor,
        /// the function returns true if both descriptors have the same name. In all other cases, the
        /// function returns false.
        /// </summary>
        /// <param name="obj">The thing to compare against.</param>
        /// <returns>True if same object, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var objElement = obj as RESTHeaderParameterDescriptor;
            return (objElement != null) && Equals(objElement);
        }

        /// <summary>
        /// Compares the descriptor against another descriptor of same type. The function returns true 
        /// if both descriptors have identical parameter declaration objects.
        /// </summary>
        /// <param name="other">The descriptor to compare against.</param>
        /// <returns>True if same object, false otherwise.</returns>
        public bool Equals(RESTHeaderParameterDescriptor other)
        {
            return other != null && other._parameter == this._parameter;
        }

        /// <summary>
        /// Returns a hashcode that is associated with the descriptor. The hash code
        /// is derived from the parameter declaration object.
        /// </summary>
        /// <returns>Hashcode for this descriptor.</returns>
        public override int GetHashCode()
        {
            return this._parameter.GetHashCode();
        }

        /// <summary>
        /// Override of compare operator. Two descriptor objects are equal if they have the same name
        /// or if they are both NULL.
        /// </summary>
        /// <param name="elementa">First descriptor to compare.</param>
        /// <param name="elementb">Second descriptor to compare.</param>
        /// <returns>True if the descriptors are equal.</returns>
        public static bool operator ==(RESTHeaderParameterDescriptor elementa, RESTHeaderParameterDescriptor elementb)
        {
            // Tricky to implement correctly. These first statements make sure that we check whether we are actually
            // dealing with identical objects and/or whether one or both are NULL.
            if (ReferenceEquals(elementa, elementb)) return true;
            if (ReferenceEquals(elementa, null)) return false;
            if (ReferenceEquals(elementb, null)) return false;
            return elementa.Equals(elementb);
        }

        /// <summary>
        /// Override of compare operator. Two descriptor objects are different if they have different names or one of them is NULL...
        /// </summary>
        /// <param name="elementa">First descriptor to compare.</param>
        /// <param name="elementb">Second descriptor to compare.</param>
        /// <returns>True if the descriptors are different.</returns>
        public static bool operator !=(RESTHeaderParameterDescriptor elementa, RESTHeaderParameterDescriptor elementb)
        {
            return !(elementa == elementb);
        }

        /// <summary>
        /// This constructor creates a Header Parameter Descriptor from its set of specified components.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <param name="classifier">Parameter classifier (must be a primitive data type).</param>
        /// <param name="description">Response descriptive text.</param>
        internal RESTHeaderParameterDescriptor(int ID, RESTParameterDeclaration parameter)
        {
            this._ID = ID;
            this._parameter = parameter;
            DetermineStatus();
        }

        /// <summary>
        /// Create a new parameter descriptor from an UML attribute.
        /// </summary>
        /// <param name="ID">ID to be assigned to this parameter.</param>
        /// <param name="attribute">The attribute to use as an initializer.</param>
        internal RESTHeaderParameterDescriptor(int ID, MEAttribute attribute)
        {
            this._ID = ID;
            this._parameter = new RESTParameterDeclaration(attribute);
            DetermineStatus();
        }

        /// <summary>
        /// This constructor creates a Header Parameter Descriptor as a copy of a provided (template) instance.
        /// The new parameter receives the specified ID, the ID from the template is ignored.
        /// </summary>
        /// <param name="other">Instance to be used to copy from.</param>
        internal RESTHeaderParameterDescriptor(int ID, RESTHeaderParameterDescriptor other)
        {
            this._ID = ID;
            this._parameter = new RESTParameterDeclaration(other._parameter);
            DetermineStatus();
        }

        /// <summary>
        /// The descriptor is valid when we have a name and a classifier. We're note going to check whether the name is part of a standard set.
        /// </summary>
        private void DetermineStatus()
        {
            this._status = this._parameter.Status != RESTParameterDeclaration.DeclarationStatus.Invalid ? DeclarationStatus.Valid : DeclarationStatus.Invalid;
        }
    }
}
