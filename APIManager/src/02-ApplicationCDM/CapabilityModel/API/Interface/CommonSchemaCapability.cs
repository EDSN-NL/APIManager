using Framework.Model;
using Framework.Exceptions;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// The Common Schema capability maintains a common container for type definitions that are used for all operations of an Interface.
    /// </summary>
    internal class CommonSchemaCapability: Capability
    {
        /// <summary>
        /// Returns the namespace token used for the Common Schema. It's defined as a local property since the common schema is required by
        /// other Capabilities that need to reference the Common Schema. The token is acquired from the Repository but if not defined, we
        /// use a default 'cmn'.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal string NSToken
        {
            get
            {
                if (this._imp != null) return ((CommonSchemaCapabilityImp)this._imp).NSToken;
                else throw new MissingImplementationException("InterfaceCapabilityImp");
            }
        }

        /// <summary>
        /// Returns an alternative namespace for the Common Schema (if required). Returns empty string when standard namespace applies.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the Capability.</exception>
        internal string AlternativeNamespaceTag
        {
            get
            {
                if (this._imp != null) return ((CommonSchemaCapabilityImp)this._imp).AlternativeNamespaceTag;
                else throw new MissingImplementationException("InterfaceCapabilityImp");
            }
        }

        /// <summary>
        /// The 'new instance' constructor is used to create a new CommonSchema class in the capability model. The constructor creates a new
        /// instance in the container package of the provided parent service. It also initializes the namespace token for the common schema.
        /// </summary>
        /// <param name="myInterface">Parent service instance.</param>
        internal CommonSchemaCapability(InterfaceCapability myInterface): base()
        {
            RegisterCapabilityImp(new CommonSchemaCapabilityImp(myInterface));
        }

        /// <summary>
        /// The 'link to existing' constructor creates a CommonSchema capability based on an existing class. 
        /// </summary>
        /// <param name="myInterface">Parent service instance.</param>
        /// <param name="commonSchema">The class to be used for this common schema.</param>
        internal CommonSchemaCapability(InterfaceCapability myInterface, MEClass commonSchema): base(commonSchema.ElementID)
        {
            if (!Valid) RegisterCapabilityImp(new CommonSchemaCapabilityImp(myInterface, commonSchema));
        }

        /// <summary>
        /// Create new Common Schema Capability Interface based on existing implementation...
        /// </summary>
        /// <param name="thisImp">Implementation to use.</param>
        internal CommonSchemaCapability(CommonSchemaCapabilityImp thisImp) : base(thisImp) { }

        /// <summary>
        /// Copy Constructor, using other interface as basis.
        /// </summary>
        /// <param name="thisCap">Interface to use as basis.</param>
        internal CommonSchemaCapability(CommonSchemaCapability thisCap) : base(thisCap) { }
    }
}
