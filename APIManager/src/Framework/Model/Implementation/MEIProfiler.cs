namespace Framework.Model
{
    /// <summary>
    /// Implementation base class for model profiler representations. A profiles is a specialized model element that can transform a given UML
    /// model into a subset model according to a set of transformation rules.
    /// </summary>
    internal abstract class MEIProfiler : ModelElementImplementation
    {
        /// <summary>
        /// Returns the package that is 'owner' of this profile, i.e. in which the profile is declared.
        /// </summary>
        /// <returns>Owning package or NULL on errors.</returns>
        internal abstract MEPackage GetOwningPackage();

        /// <summary>
        /// Copies the profile definition from the specified source profile to the current profile.
        /// </summary>
        /// <param name="sourceProfile">Profile to copy.</param>
        internal abstract void LoadProfile(MEProfiler sourceProfile);

        /// <summary>
        /// Default constructor, mainly used to pass the model instance to the base constructor and set the correct type.
        /// </summary>
        /// <param name="model">Reference to the associated model implementation object.</param>
        protected MEIProfiler (ModelImplementation model): base(model)
        {
            this._type = ModelElementType.Profiler;
        }
    }
}