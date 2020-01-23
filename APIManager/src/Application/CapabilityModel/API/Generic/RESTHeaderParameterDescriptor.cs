using System;
using Framework.Model;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// Defines the structure of a single header parameter.
    /// </summary>
    internal sealed class RESTHeaderParameterDescriptor : IEquatable<RESTHeaderParameterDescriptor>, IDisposable
    {
        // The status is used to track operational state of the descriptor. The descriptor has 'valid' state when all properties
        // are consistent in relationship with each other.
        internal enum DeclarationStatus { Invalid, Valid }

        private MEDataType _classifier;             // [Primitive] data type of this header parameter.
        private string _name;                       // Parameter name;
        private string _description;                // Descriptive text to go with the parameter.
        private DeclarationStatus _status;          // Descriptor status with regard to user editing.
        private bool _disposed;                     // Mark myself as invalid after call to dispose!

        /// <summary>
        /// Get or set the parameter classifier.
        /// </summary>
        internal MEDataType Classifier
        {
            get { return this._classifier; }
            set
            {
                if (this._classifier != value)
                {
                    this._classifier = value;
                    DetermineStatus();
                }
            }
        }

        /// <summary>
        /// This is the normal entry for all users of the object that want to indicate that the resource description is not required anymore.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Returns or loads the associated description text.
        /// </summary>
        internal string Description
        {
            get { return this._description; }
            set { this._description = value; }
        }

        /// <summary>
        /// Get or set the name of the descriptor (identifies the header parameter).
        /// </summary>
        internal string Name
        {
            get { return this._name; }
            set
            {
                if (this._name != value)
                {
                    this._name = value;
                    DetermineStatus();
                }
            }
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
        /// if both descriptors have identical names. In all other cases, the function returns false.
        /// </summary>
        /// <param name="other">The descriptor to compare against.</param>
        /// <returns>True if same object, false otherwise.</returns>
        public bool Equals(RESTHeaderParameterDescriptor other)
        {
            return other != null && other._name == this._name;
        }

        /// <summary>
        /// Returns a hashcode that is associated with the descriptor. The hash code
        /// is derived from the descriptor name.
        /// </summary>
        /// <returns>Hashcode for this descriptor.</returns>
        public override int GetHashCode()
        {
            return this._name.GetHashCode();
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
        internal RESTHeaderParameterDescriptor(string name,
                                               MEDataType classifier,
                                               string description)
        {
            this._name = name;
            this._classifier = classifier;
            this._description = description;
            this._disposed = false;
            DetermineStatus();
        }

        /// <summary>
        /// This constructor creates a Header Parameter Descriptor as a copy of a provided (template) instance.
        /// </summary>
        /// <param name="other">Instance to be used to copy from.</param>
        internal RESTHeaderParameterDescriptor(RESTHeaderParameterDescriptor other)
        {
            this._name = other._name;
            this._classifier = new MEDataType(other._classifier);
            this._description = other._description;
            this._disposed = false;
            DetermineStatus();
        }

        /// <summary>
        /// The descriptor is valid when we have a name and a classifier. We're note going to check whether the name is part of a standard set.
        /// </summary>
        private void DetermineStatus()
        {
            this._status = !string.IsNullOrEmpty(this._name) && this._classifier != null ? DeclarationStatus.Valid : DeclarationStatus.Invalid;
        }

        /// <summary>
        /// This is the actual disposing interface, which takes case of structural removal of the object when no longer needed.
        /// </summary>
        /// <param name="disposing">Set to 'true' when called directly. Set to 'false' when called from the finalizer.</param>
        private void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                try
                {
                    if (this._classifier != null) this._classifier.Dispose();
                    this._classifier = null;
                    this._disposed = true;
                }
                catch { };   // Ignore any exceptions, no use in processing them here.
            }
        }
    }
}
