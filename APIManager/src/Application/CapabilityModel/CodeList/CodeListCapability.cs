using System.Collections.Generic;
using Framework.Model;
using Framework.Logging;
using Framework.Exceptions;

namespace Plugin.Application.CapabilityModel.CodeList
{
    /// <summary>
    /// Definition of a single CodeList capability. Code Lists have an association with an enumerated type from the domain model
    /// that is used as the source. During class construction, the user has to explicitly assign this source and in a subsequent
    /// step, the user has to select the enumeration values to copy to the code list.
    /// </summary>
    internal class CodeListCapability: Capability
    {
        /// <summary>
        /// Returns the Enumerated Type that has been used as source for this CodeList.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal MEEnumeratedType SourceEnum
        {
            get
            {
                if (this._imp != null) return ((CodeListCapabilityImp)this._imp).SourceEnum;
                else throw new MissingImplementationException("CapabilityImp");
            }
        }

        /// <summary>
        /// Returns the CodeType object that is associated with the CodeList.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal MEDataType CodeType
        {
            get
            {
                if (this._imp != null) return ((CodeListCapabilityImp)this._imp).CodeType;
                else throw new MissingImplementationException("CapabilityImp");
            }
        }

        /// <summary>
        /// Returns the canonical URI for this CodeList (this is the URI without version information).
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal string CanonicalURI
        {
            get
            {
                if (this._imp != null) return ((CodeListCapabilityImp)this._imp).CanonicalURI;
                else throw new MissingImplementationException("CapabilityImp");
            }
        }

        /// <summary>
        /// Returns the full URI (including version) for this CodeList.
        /// </summary>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal string CanonicalVersionURI
        {
            get
            {
                if (this._imp != null) return ((CodeListCapabilityImp)this._imp).CanonicalVersionURI;
                else throw new MissingImplementationException("CapabilityImp");
            }
        }

        /// <summary>
        /// Create constructor, used to create a new instance of a Code List. The constructor assumes that the package structure
        /// exists and that there exists a service to which we can connect the new capability. The constructor creates the
        /// appropriate model elements in the correct packages and links stuff together.
        /// </summary>
        /// <paramref name="myService">All capabilities are, directly or indirectly, always associated with a single Service.</param>
        /// <paramref name="capabilityName">The name of this code list.</paramref>
        /// <paramref name="sourceEnum">The associated enumerated type.</paramref>
        /// <paramref name="agencyName">The human-readable namne of the agency responsible for this code list.</paramref>
        /// <paramref name="agencyID">The URI identifier of the agency responsible for this code list.</paramref>
        /// <paramref name="copiedAttribs">The set of enumeration values to be copied from the source enum.</paramref>
        internal CodeListCapability(CodeListService myService, string capabilityName, MEEnumeratedType sourceEnum, string agencyName, string agencyID, List<MEAttribute> copiedAttribs): base()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeList.CodeListCapability (new) >> Creating new CodeList capability for Service '" +
                              myService.Name + "', with name: '" + capabilityName + "'...");
            RegisterCapabilityImp(new CodeListCapabilityImp(myService, capabilityName, sourceEnum, agencyName, agencyID, copiedAttribs));
        }

        /// <summary>
        /// Generic constructor, can be used to implement operations on existing CodeLists. It generates a minimal context 
        /// and registers the CodeList with the service awaiting further instructions. Since we basically only registers the
        /// associated CodeList capability class, do not request sourceEnum and/or codeType at this point!
        /// </summary>
        /// <param name="myService">The service that 'owns' this capability.</param>
        /// <param name="codeListCapability">The class that we're using as basis for this capability.</param>
        internal CodeListCapability(Service myService, MEClass codeListCapability): base(codeListCapability.ElementID)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeList.CodeListCapability  >> Creating CodeList capability for Service '" + 
                             myService.Name + "', based on class '" + codeListCapability.Name + "'...");
            if (!Valid)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeList.CodeListCapability  >> No implementation yet exists, creating one...");
                RegisterCapabilityImp(new CodeListCapabilityImp(myService, codeListCapability));
            }
        }

        /// <summary>
        /// Create new CodeList Capability Interface based on existing implementation...
        /// </summary>
        /// <param name="thisImp">Implementation to use.</param>
        internal CodeListCapability(CodeListCapabilityImp thisImp) : base(thisImp)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeList.CodeListCapability >> Creating new Interface based on Implementation '" + thisImp.Name + "'...");
        }

        /// <summary>
        /// Copy Constructor, using other interface as basis.
        /// </summary>
        /// <param name="thisImp">Interface to use as basis.</param>
        internal CodeListCapability(CodeListCapability thisCap) : base(thisCap)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeList.CodeListCapability >> Copy Constructor using '" + thisCap.Name + "'...");
        }

        /// <summary>
        /// The update method is invoked on an existing CodeList Capability in order to change (add/delete) enumeration values.
        /// The method uses the 'EnumPicker' dialog as a means for the user to add and/or remove enumerations.
        /// </summary>
        /// <returns>True when actually updated, false on cancel.</returns>
        /// <exception cref="MissingImplementationException">When no implementation object is present for the model.</exception>
        internal bool Update()
        {
            if (this._imp != null) return ((CodeListCapabilityImp)this._imp).Update();
            else throw new MissingImplementationException("CapabilityImp");
        }
    }
}
