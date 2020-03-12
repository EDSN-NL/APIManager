using System;
using Framework.Model;
using Framework.Logging;
using Framework.Context;
using Framework.Util;

namespace Plugin.Application.CapabilityModel.API
{
    /// <summary>
    /// The SOAP Common Schema capability maintains a common container for type definitions that are used for all operations of an Interface.
    /// </summary>
    internal class CommonSchemaCapabilityImp: CapabilityImp
    {
        // Configuration properties used by this module:
        private const string _CommonSchemaClassName         = "CommonSchemaClassName";
        private const string _CommonSchemaClassStereotype   = "CommonSchemaClassStereotype";
        private const string _CommonSchemaRoleName          = "CommonSchemaRoleName";
        private const string _CommonSchemaNSToken           = "CommonSchemaNSToken";
        private const string _NSTokenTag                    = "NSTokenTag";
        private const string _UseAlternativeNamingTag       = "UseAlternativeNamingTag";
        private const string _AlternativeCommonNS           = "AlternativeCommonNS";

        private InterfaceCapability _interfaceCapability;   // Represents the 'owning' interface.
        private bool _useAlternativeNaming;                 // Set to 'true' to support the 'old' naming convention.
        private string _alternativeNamespaceTag;            // Stores namespace construction tag when AlternativeNamespace is true.

        /// <summary>
        /// Returns an alternative namespace for the Common Schema (if required). Returns empty string when standard namespace applies.
        /// </summary>
        internal string AlternativeNamespaceTag             { get { return this._alternativeNamespaceTag; } }

        /// <summary>
        /// Returns the namespace token used for the Common Schema. It's defined as a local property since the common schema is required by
        /// other Capabilities that need to reference the Common Schema. The token is acquired from the Repository but if not defined, we
        /// use a default 'cmn'.
        /// </summary>
        internal string NSToken
        {
            get
            {
                string nsTokenTag = this._capabilityClass.GetTag(ContextSlt.GetContextSlt().GetConfigProperty(_NSTokenTag));
                return (nsTokenTag != string.Empty) ? nsTokenTag : "cmn";
            }
        }

        /// <summary>
        /// The 'new instance' constructor is used to create a new CommonSchema class in the capability model. The constructor creates a new
        /// instance in the container package of the provided parent service. It also initializes the namespace token for the common schema.
        /// By default, the 'useAlternativeNaming' property is set to 'false'. The only way to change this is for the user to manually edit
        /// the property using the UML tool user interface.
        /// </summary>
        /// <param name="myInterface">Parent service instance.</param>
        internal CommonSchemaCapabilityImp(InterfaceCapability myInterface): base(myInterface.RootService)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            this._interfaceCapability = myInterface;
            this._capabilityClass = myInterface.RootService.ModelPkg.CreateClass(context.GetConfigProperty(_CommonSchemaClassName), 
                                                                                 context.GetConfigProperty(_CommonSchemaClassStereotype));
            this._capabilityClass.SetTag(context.GetConfigProperty(_NSTokenTag), context.GetConfigProperty(_CommonSchemaNSToken), true);
            this._capabilityClass.SetTag(context.GetConfigProperty(_UseAlternativeNamingTag), "false", true);
            this._capabilityClass.Version = myInterface.RootService.Version;
            this._alternativeNamespaceTag = string.Empty;

            // Create the associations with our interface...
            this._assignedRole = context.GetConfigProperty(_CommonSchemaRoleName);
            var interfaceEndpoint = new EndpointDescriptor(myInterface.CapabilityClass, "1", Name, null, false);
            var commonSchema = new EndpointDescriptor(this._capabilityClass, "1", this._assignedRole, null, true);
            model.CreateAssociation(interfaceEndpoint, commonSchema, MEAssociation.AssociationType.MessageAssociation);

            // We create an initial audit-log entry in the interface capability class...
            CreateLogEntry("Initial release.");
        }

        /// <summary>
        /// The 'link to existing' constructor creates a CommonSchema capability based on an existing class.
        /// </summary>
        /// <param name="myInterface">Parent interface instance.</param>
        /// <param name="commonSchema">The class to be used for this common schema.</param>
        internal CommonSchemaCapabilityImp(InterfaceCapability myInterface, MEClass commonSchema): base(myInterface.RootService)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            this._interfaceCapability = myInterface;
            this._capabilityClass = commonSchema;
            this._assignedRole = myInterface.FindChildClassRole(context.GetConfigProperty(_CommonSchemaClassName), context.GetConfigProperty(_CommonSchemaClassStereotype));
            string alternativeNamingTag = commonSchema.GetTag(context.GetConfigProperty(_UseAlternativeNamingTag));
            this._useAlternativeNaming = !string.IsNullOrEmpty(alternativeNamingTag) && string.Compare(alternativeNamingTag, "true", true) == 0;
            this._alternativeNamespaceTag = this._useAlternativeNaming ? _AlternativeCommonNS : string.Empty;
        }

        /// <summary>
        /// Returns the file name (without extension) for this Capability. The extension is left out since this typically depends on the
        /// chosen serialization mechanism. The filename returned by this method only provides a generic name to be used for further, serialization
        /// dependent, processing.
        /// If the OperationalStatus of the service is not equal to the default, we also include the OperationalStatus in the filename.
        /// </summary>
        internal override string GetBaseFileName()
        {
            Tuple<int, int> version = this.CapabilityClass.Version;
            string postfix = RootService.IsDefaultOperationalState ? string.Empty : "_" + RootService.NonDefaultOperationalState;
            string baseName = this._useAlternativeNaming ? this._rootService.Name + "_" + this._assignedRole : this.Name;
            return this._rootService.UseConfigurationMgmt? (baseName + postfix): (baseName + "_v" + version.Item1 + "p" + version.Item2 + postfix);
        }

        /// <summary>
        /// Returns a short textual identification of the capability type.
        /// </summary>
        /// <returns>Capability type name.</returns>
        internal override string GetCapabilityType()
        {
            return "Common Schema";
        }

        /// <summary>
        /// Creates an Interface object that matches the current Implementation.
        /// </summary>
        /// <returns>Interface object.</returns>
        internal override Capability GetInterface() { return new CommonSchemaCapability(this); }

        /// <summary>
        /// This method is called whenever a (new) parent of the Capability has taken ownership of the Capability. If this parent is an Interface,
        /// we have to register the current instance with that Interface.
        /// </summary>
        /// <param name="parent">The parent Capability that has taken ownership of this Capability.</param>
        internal override void InitialiseParent(Capability parent)
        {
            if (parent is InterfaceCapability) parent.AddChild(new CommonSchemaCapability(this));
        }

        /// <summary>
        /// Process the capability (i.e. generate output according to provided processor.).
        /// </summary>
        /// <param name="processor">Capability processor that must be used.</param>
        /// <param name="stage">The processing stage we're currently in, passed verbatim to processor.</param>
        /// <returns>True when processing can commence, false on errors.</returns>
        internal override bool HandleCapability(CapabilityProcessor processor, ProcessingStage stage)
        {
            // Since all the actual work is being performed by the processor, simply pass information onwards...
            return processor.ProcessCapability(new CommonSchemaCapability(this), stage);
        }
    }
}
