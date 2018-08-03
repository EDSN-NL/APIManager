using Framework.Exceptions;

namespace Framework.Model
{
    /// <summary>
    /// Interface class for model profiler representations. A profiler is a specialized model element that can transform a given UML
    /// model into a subset model according to a set of transformation rules.
    /// </summary>
    internal sealed class MEProfiler: ModelElement
    {
        /// <summary>
        /// Returns the package in which this profile lives, i.e. that is owner of the profile.
        /// </summary>
        /// <returns>Owning package or NULL on errors.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the profile.</exception>
        internal MEPackage OwningPackage
        {
            get
            {
                if (this._imp != null) return ((MEIProfiler)this._imp).GetOwningPackage();
                else throw new MissingImplementationException("MEIProfiler");
            }
        }

        /// <summary>
        /// Copies the profile definition from the specified source profile to the current profile.
        /// </summary>
        /// <param name="sourceProfile">Profile to copy.</param>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the profile.</exception>
        internal void LoadProfile(MEProfiler sourceProfile)
        {
            if (this._imp != null) ((MEIProfiler)this._imp).LoadProfile(sourceProfile);
            else throw new MissingImplementationException("MEIProfiler");
        }

        /// <summary>
        /// Construct a new UML 'Profile' artifact by associating MEProfile with the appropriate implementation object.
        /// The actual registration and construction is all performed by the base class constructor.
        /// </summary>
        /// <param name="classID">Tool-specific instance identifier of the profile artifact within the tool repository.</param>
        internal MEProfiler(int classID) : base(ModelElementType.Profiler, classID) { }

        /// <summary>
        /// Construct a new UML 'Profile' artifact by associating MEProfile with the appropriate implementation object.
        /// The actual registration and construction is all performed by the base class constructor.
        /// </summary>
        /// <param name="classGUID">Globally-unique instance identifier of the profile artifact.</param>
        internal MEProfiler(string classGUID) : base(ModelElementType.Profiler, classGUID) { }

        /// <summary>
        /// Construct a new UML 'Profile' artifact by associating MEProfile with the given implementation object.
        /// The actual registration and construction is all performed by the base class constructor.
        /// </summary>
        /// <param name="imp">Profile implementation object.</param>
        internal MEProfiler(MEIProfiler imp) : base(imp) { }

        /// <summary>
        /// Copy constructor creates a new Model Element Profile from another Model Element Profile.
        /// </summary>
        /// <param name="copy">Class to use as basis.</param>
        internal MEProfiler(MEProfiler copy) : base(copy) { }

        /// <summary>
        /// Default constructor creates an empty interface.
        /// </summary>
        internal MEProfiler() : base() { }
    }
}
