using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Model;
using Framework.Logging;
using Framework.Context;
using Framework.Util;
using Plugin.Application.Forms;

namespace Plugin.Application.CapabilityModel.CodeList
{
    /// <summary>
    /// Definition of a single CodeList capability. Code Lists have an association with an enumerated type from the domain model
    /// that is used as the source. During class construction, the user has to explicitly assign this source and in a subsequent
    /// step, the user has to select the enumeration values to copy to the code list.
    /// </summary>
    internal class CodeListCapabilityImp: CapabilityImp
    {
        // Configuration properties used by this capability:
        private const string _CodeListClassStereotype       = "CodeListClassStereotype";
        private const string _CodeListPkgName               = "CodeListPkgName";
        private const string _FacetAttStereotype            = "FacetAttStereotype";

        // Configuration properties that define the attributes to create in the CodeType data type:
        private const string _CodeListNameAttribute         = "CodeListNameAttribute";
        private const string _CodeListAgencyIDAttribute     = "CodeListAgencyIDAttribute";
        private const string _CodeListAgencyNameAttribute   = "CodeListAgencyNameAttribute";
        private const string _CodeListVersionAttribute      = "CodeListVersionAttribute";
        private const string _CodeListURNAttribute          = "CodeListURNAttribute";
        private const string _CoreDataTypesPathName         = "CoreDataTypesPathName";
        private const string _PrimDataTypesPathName         = "PrimDataTypesPathName";

        // Some data type names that are needed by this module. These are the actual type names!
        private const string _CodeTypeParentType            = "CodeType";
        private const string _AttribClassifierType          = "NormalizedString";
        private const string _CodeListEnumClassifierType    = "CodeType";

        private MEEnumeratedType _sourceEnum = null;            // Enumeration to be used as source.
        private MEDataType _codeType = null;                    // The created CodeType, which will replace Enum in messages.
        private List<MEAttribute> _selectedAttribs = null;      // The list of enumerations associated with the CodeList.
        private string _canonicalURI = null;                    // Namespace URI without version.
        private string _canonicalVersionURI = null;             // Full namespace URI including version.
        private bool _isInitialised = false;                    // Used to determine whethere we're fully functional (lazy initialization).

        internal MEEnumeratedType SourceEnum  { get { return this._sourceEnum; } }
        internal MEDataType CodeType          { get { return this._codeType; } }
        internal string CanonicalURI          { get { return this._canonicalURI; } }
        internal string CanonicalVersionURI   { get { return this._canonicalVersionURI; } }

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
        internal CodeListCapabilityImp(CodeListService myService, string capabilityName, MEEnumeratedType sourceEnum, string agencyName, string agencyID, List<MEAttribute> copiedAttribs): base(myService)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeList.CodeListCapabilityImp (new) >> Creating new CodeList capability for service '" +
                              myService.Name + "', with name: '" + capabilityName + "'...");

            capabilityName = Conversions.ToPascalCase(capabilityName);  // Make sure that the name has the proper format!
            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            string myStereotype = context.GetConfigProperty(_CodeListClassStereotype);
            this._sourceEnum = sourceEnum;
            string versionString = myService.MajorVersion + ".0";

            try
            {
                MEPackage modelPkg = myService.ModelPkg;            // Create capability in same package as service.
                this._capabilityClass = modelPkg.CreateClass(capabilityName, myStereotype);
                this._capabilityClass.Version = myService.Version;  // We copy the version of the service.

                // Define the new CodeList capability as a composite association of my service...
                var serviceEnd = new EndpointDescriptor(myService.ServiceClass, "1", Name, null, false);
                var capabilityEnd = new EndpointDescriptor(this._capabilityClass, "1", capabilityName, null, false);
                model.CreateAssociation(serviceEnd, capabilityEnd, MEAssociation.AssociationType.Composition);

                // Create 'usage' association between the CodeList capability and the selected enumeration...
                var sourceEnd = new EndpointDescriptor(this._capabilityClass, "1", null, null, false);
                var targetEnd = new EndpointDescriptor(this._sourceEnum, "1", null, null, false);
                model.CreateAssociation(sourceEnd, targetEnd, MEAssociation.AssociationType.Usage);

                // Create a 'CodeType' data type in the appropriate package...
                MEPackage codeTypesPackage = myService.DeclarationPkg.FindPackage(context.GetConfigProperty(_CodeListPkgName));
                string typeName = capabilityName;
                if (typeName.EndsWith("Code")) typeName += "Type";
                if (!typeName.EndsWith("CodeType")) typeName += "CodeType";
                this._codeType = codeTypesPackage.CreateDataType(typeName, MEDataType.MetaDataType.ComplexType) as MEDataType;

                // Code Type must inherit from the generic CodeType CDT...
                MEDataType parentType = model.FindDataType(context.GetConfigProperty(_CoreDataTypesPathName), _CodeTypeParentType);
                var derived = new EndpointDescriptor(this._codeType);
                var parent = new EndpointDescriptor(parentType);
                model.CreateAssociation(derived, parent, MEAssociation.AssociationType.Generalization);

                // Code Type is owned by the declaration package...
                var codeTypeEnd = new EndpointDescriptor(this._codeType, "1", capabilityName, null, false);
                model.CreateAssociation(sourceEnd, codeTypeEnd, MEAssociation.AssociationType.Composition);

                // All attributes are of the same classifier: primitive String.
                MEDataType attribClassifier = model.FindDataType(context.GetConfigProperty(_PrimDataTypesPathName), _AttribClassifierType);
                var card = new Tuple<int, int>(1, 1);   // All attributes are mandatory.

                // Create the Canonical- and CanonicalVersion URI's...
                string majorVersionToken = ":" + myService.MajorVersion + ".0:";
                this._canonicalVersionURI = myService.GetFQN("CodeList", capabilityName, 0);
                this._canonicalURI = this._canonicalVersionURI.Substring(0, this._canonicalVersionURI.IndexOf(majorVersionToken));

                // Now we can create the attributes for our CodeType class:
                this._codeType.CreateAttribute(context.GetConfigProperty(_CodeListNameAttribute), attribClassifier, AttributeType.Supplementary, capabilityName, card, true);
                this._codeType.CreateAttribute(context.GetConfigProperty(_CodeListAgencyIDAttribute), attribClassifier, AttributeType.Supplementary, agencyID, card, true);
                this._codeType.CreateAttribute(context.GetConfigProperty(_CodeListAgencyNameAttribute), attribClassifier, AttributeType.Supplementary, agencyName, card, true);
                this._codeType.CreateAttribute(context.GetConfigProperty(_CodeListVersionAttribute), attribClassifier, AttributeType.Supplementary, versionString, card, true);
                this._codeType.CreateAttribute(context.GetConfigProperty(_CodeListURNAttribute), attribClassifier, AttributeType.Supplementary, this._canonicalVersionURI, card, true);

                // The associated enumeration must have 'Facet' stereotype assigned to all enumeration values. Let's check...
                string facetStereotype = context.GetConfigProperty(_FacetAttStereotype);
                foreach (MEAttribute attrib in sourceEnum.Attributes)
                {
                    if (attrib.Type != ModelElementType.Facet) attrib.AddStereotype(facetStereotype);
                }

                // Finally, we're going to create the user-selected enumeration values as attributes of the CodeList Capability class...
                this._selectedAttribs = new List<MEAttribute>();
                attribClassifier = model.FindDataType(context.GetConfigProperty(_CoreDataTypesPathName), _CodeListEnumClassifierType);

                foreach (MEAttribute attrib in copiedAttribs)
                {
                    this._selectedAttribs.Add(attrib);
                    this._capabilityClass.CreateAttribute(attrib.Name, attribClassifier, AttributeType.Facet, null, card, false);
                }

                // We create an initial audit-log entry in the capability class...
                CreateLogEntry("Initial release.");
                this._isInitialised = true;
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.CodeList.CodeListCapabilityImp (new) >> Error creating capability because: " + exc.Message);
                this._capabilityClass = null;   // Assures that instance is declared invalid.
            }
        }

        /// <summary>
        /// Generic constructor, can be used to implement operations on existing CodeLists. It generates a minimal context 
        /// and registers the CodeList with the service awaiting further instructions. Since we basically only registers the
        /// associated CodeList capability class, do not request sourceEnum and/or codeType at this point!
        /// </summary>
        /// <param name="myService">The service that 'owns' this capability.</param>
        /// <param name="codeListCapability">The class that we're using as basis for this capability.</param>
        internal CodeListCapabilityImp(Service myService, MEClass codeListCapability): base(myService)
        {
            try
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeList.CodeListCapabilityImp >> Initialize CodeList '" + codeListCapability.Name +
                                 "' for service '" + myService.Name + "'...");
                this._capabilityClass = codeListCapability;
                var itfCapability = new CodeListCapability(this);
                this._rootService.AddCapability(itfCapability);

                this._sourceEnum = null;
                this._codeType = null;
                this._canonicalURI = null;
                this._canonicalVersionURI = null;
                this._isInitialised = false;       // We only initialise the bare minimum at this point.
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.CodeList.CodeListCapabilityImp (existing) >> Error creating capability because: " + exc.Message);
                this._capabilityClass = null;   // Assures that instance is declared invalid
            }
        }

        /// <summary>
        /// Returns the file name (without extension) for this Capability. The extension is left out since this typically depends on the
        /// chosen serialization mechanism. The filename returned by this method only provides a generic name to be used for further, serialization
        /// dependent, processing.
        /// </summary>
        internal override string GetBaseFileName()
        {
            Tuple<int, int> version = this.CapabilityClass.Version;
            string postfix = Conversions.ToPascalCase(RootService.IsDefaultOperationalStatus ? string.Empty : "_" + RootService.OperationalStatus);
            return (this._rootService.UseConfigurationMgmt) ? this.Name + "CodeList" + postfix: 
                                                              this.Name + "CodeList_v" + version.Item1 + "p" + version.Item2 + postfix;
        }

        /// <summary>
        /// Returns a short textual identification of the capability type.
        /// </summary>
        /// <returns>Capability type name.</returns>
        internal override string GetCapabilityType()
        {
            return "Code List";
        }

        /// <summary>
        /// Creates an Interface object that matches the current Implementation.
        /// </summary>
        /// <returns>Interface object.</returns>
        internal override Capability GetInterface() { return new CodeListCapability(this); }

        /// <summary>
        /// This method is called whenever a (new) parent of the Capability has taken ownership of the Capability. Not implemented for CodeList
        /// Capability types, thus not performing any operations.
        /// </summary>
        /// <param name="parent">The parent Capability that has taken ownership of this Capability.</param>
        internal override void InitialiseParent(Capability parent)
        {
            // Not used.
        }

        /// <summary>
        /// The update method is invoked on an existing CodeList Capability in order to change (add/delete) enumeration values.
        /// The method uses the 'EnumPicker' dialog as a means for the user to add and/or remove enumerations.
        /// </summary>
        /// <returns>True when actually updated, false on cancel.</returns>
        internal bool Update()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeList.CodeListCapabilityImp.update >> Updating CodeList '" + this._capabilityClass.Name + "'...");
            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            MEDataType attribClassifier = model.FindDataType(context.GetConfigProperty(_CoreDataTypesPathName), _CodeListEnumClassifierType);
            var card = new Tuple<int, int>(1, 1);   // All attributes are mandatory.
            bool result = false;
            string logEntry = string.Empty;
            if (!this._isInitialised) FinalizeInit();

            using (var enumPicker = new CodeListEnumPicker())
            {
               enumPicker.LoadNodes(this._sourceEnum, this._selectedAttribs);
                if (enumPicker.ShowDialog() == DialogResult.OK)
                {
                    List<MEAttribute> newAttribList = enumPicker.GetCheckedNodes();

                    // First, we find (and delete) all attributes that have been un-checked...
                    string enumsRemoved = string.Empty;
                    bool isFirst = true;    // Little trick to get ',' at the right places.
                    foreach (MEAttribute attrib in this._selectedAttribs)
                    {
                        if (!newAttribList.Contains(attrib))
                        {
                            this._capabilityClass.DeleteAttribute(attrib);
                            enumsRemoved += (!isFirst)? ", " + attrib.Name: attrib.Name;
                            isFirst = false;
                            result = true;
                        }
                    }

                    // Next, we find (and create) all attributes that we did not have before...
                    string enumsAdded = string.Empty;
                    isFirst = true;
                    foreach (MEAttribute attrib in newAttribList)
                    {
                        if (!this._selectedAttribs.Contains(attrib))
                        {
                            this._capabilityClass.CreateAttribute(attrib.Name, attribClassifier, AttributeType.Facet, null, card, false);
                            enumsAdded += (!isFirst) ? ", " + attrib.Name : attrib.Name;
                            isFirst = false;
                            result = true;
                        }
                    }
                    this._selectedAttribs = newAttribList;  // Remember this as our new list of selected attributes.

                    if (result)
                    {
                        // We also have to update the version of the service, this class and the associated CodeType attribute!
                        this._rootService.IncrementVersion();   // Calling this will also update ALL children.
                        string newVersionString = this._capabilityClass.Version.Item1 + "." + this._capabilityClass.Version.Item2;

                        string vsnName = context.GetConfigProperty(_CodeListVersionAttribute);
                        string urnName = context.GetConfigProperty(_CodeListURNAttribute);
                        foreach (MEAttribute att in this._codeType.Attributes)
                        {
                            if (att.Name == vsnName) att.FixedValue = newVersionString;
                            else if (att.Name == urnName) att.FixedValue = this._rootService.GetFQN("CodeList", this.Name, 
                                                                                                    this._capabilityClass.Version.Item2);
                        }

                        // At least one change, create log entry for this new version.
                        string logText = string.Empty;
                        if (enumsRemoved != string.Empty) logText += "Enumerations removed: " + enumsRemoved + "; ";
                        if (enumsAdded != string.Empty) logText += "Enumerations added: " + enumsAdded;
                        CreateLogEntry(logText);

                        // An update of the CodeList must also result in an update of the associated service....
                        this._rootService.CreateLogEntry("Updated CodeList '" + this._capabilityClass.Name + "'.");

                        // To avoid issues with 'stale' data in EA classes, we remove this implementation object from the registry.
                        // This will enforce re-load of all associated EA resources on a subsequent operation.
                        Capability.UnregisterCapabilityImp(this);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// This method is used to synchronize the version of the CodeList with its parent service in case that version has changed.
        /// The method simply copies the version of the Service to my CodeList.
        /// </summary>
        internal override void VersionSync()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            if (!this._isInitialised) FinalizeInit();

            this._capabilityClass.Version = this._rootService.Version;
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeList.CodeListCapabilityImp.versionSync >> Version of class '" + Name + "' set to: " +
                             this._capabilityClass.Version.Item1 + "." + this._capabilityClass.Version.Item2 + "'.");
            this._canonicalVersionURI = this._rootService.GetFQN("CodeList", this.Name, this._rootService.Version.Item2);

            string vsnName = context.GetConfigProperty(_CodeListVersionAttribute);
            string urnName = context.GetConfigProperty(_CodeListURNAttribute);
            foreach (MEAttribute att in this._codeType.Attributes)
            {
                // We have to update the version of the CodeType, but also the URN since this contains the
                // major version!
                if (att.Name == vsnName)
                {
                    att.FixedValue = this._capabilityClass.Version.Item1 + "." + this._capabilityClass.Version.Item2;
                    CreateLogEntry("Version changed to: '" + this._capabilityClass.Version.Item1 + "." + this._capabilityClass.Version.Item2 + "'.");
                }
                else if (att.Name == urnName)
                {
                    att.FixedValue = this._canonicalVersionURI;
                }
            }
        }

        /// <summary>
        /// Process the capability (i.e. generate output according to provided processor.).
        /// </summary>
        /// <param name="stage">The processing stage we're currently in, passed verbatim to processor.</param>
        /// <returns>True when processing can commence, false on errors.</returns>
        internal override bool HandleCapability(CapabilityProcessor processor, ProcessingStage stage)
        {
            // Since all the actual work is being performed by the processor, simply pass information onwards...
            if (!this._isInitialised) FinalizeInit();
            var capabilityItf = new CodeListCapability(this);
            return processor.ProcessCapability(capabilityItf, stage);
        }

        /// <summary>
        /// This is called to finalize CodeList initialization. The method retrieves all associated child entities and attributes.
        /// Since this takes time, we attempt to postpone this until actually needed.
        /// </summary>
        private void FinalizeInit()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeList.CodeListCapabilityImp.finalizeInit >> Completing lazy initialization...");

            // Retrieve the enumeration...
            foreach (MEAssociation association in this._capabilityClass.TypedAssociations(MEAssociation.AssociationType.Usage))
            {
                // There should be only one 'usage' type of association, so we simply use the first response and ignore all others.
                this._sourceEnum = association.Destination.EndPoint as MEEnumeratedType;
                break;
            }

            // Retrieve the code type...
            foreach (MEAssociation association in this._capabilityClass.TypedAssociations(MEAssociation.AssociationType.Composition))
            {
                // We should have only one CodeType, but to be sure, we check whether the names match....
                var codeType = association.Destination.EndPoint as MEDataType;
                if (codeType != null && codeType.Name.Contains(this._capabilityClass.Name))
                {
                    this._codeType = codeType;
                    break;
                }
            }

            // Create the Canonical- and CanonicalVersion URI's...
            Tuple<int, int> capabilityVersion = this.CapabilityClass.Version;
            string majorVersionToken = ":" + capabilityVersion.Item1 + "." + capabilityVersion.Item2 + ":";
            this._canonicalVersionURI = this._rootService.GetFQN("CodeList", this.Name, capabilityVersion.Item2);
            this._canonicalURI = this._canonicalVersionURI.Substring(0, this._canonicalVersionURI.IndexOf(majorVersionToken));

            // Since the Capability class can have multiple types of attributes, we extract only the 'Facet' types since these
            // represent the selected set of enumeration values for the CodeList....
            List<MEAttribute> allAttribs = this._capabilityClass.Attributes;
            this._selectedAttribs = new List<MEAttribute>();
            foreach (MEAttribute attrib in allAttribs) if (attrib.Type == ModelElementType.Facet) this._selectedAttribs.Add(attrib);

            this._isInitialised = true;
        }
    }
}
