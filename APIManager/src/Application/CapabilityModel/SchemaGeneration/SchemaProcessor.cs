using System;
using System.Collections.Generic;
using Framework.Model;
using Framework.Logging;
using Framework.Util;
using Framework.Util.SchemaManagement;
using Framework.Context;
using Plugin.Application.CapabilityModel.API;
using Plugin.Application.CapabilityModel.ASCIIDoc;

namespace Plugin.Application.CapabilityModel.SchemaGeneration
{
    /// <summary>
    /// The Schema Processor is a Capability Processor that takes an UML 'root' node and creates an XML Schema from that node and all child nodes.
    /// This module is the 'first part' of the Class Definition and contains the code related to the Capability Processor (i.e. the interface with the
    /// processing context).
    /// </summary>
    internal partial class SchemaProcessor : CapabilityProcessor
    {
        // Configuration properties used by this module...
        private const string _MessageAssemblyRoleName   = "MessageAssemblyRoleName";
        private const string _MessageHeaderRoleName     = "MessageHeaderRoleName";
        protected const string _DocgenSourcePrefix      = "DocgenSourcePrefix";
        protected const string _CommonSchemaNSToken     = "CommonSchemaNSToken";

        private Schema _schema;                     // Will receive the schema representation of the node.
        private Schema _commonSchema;               // References the Common Schema associated with Interface (when present, otherwise this is NULL).
        private ProgressPanelSlt _panel;            // Contains the progress panel that we're using to report progress.
        private int _panelIndex;                    // The Panel indentation index assigned to this processor.
        private string _lastError;                  // Used to save last error in case of exceptions.

        private string _qualifiedClassName;         // Contains the final message name, prefixed with the namespace(token) in which it is defined (token:name).
        private string _messageName;                // The name of the constructed message element.
        private string _messageNamespace;           // The namespace in which the message has been constructed.
        private string _messageNsToken;             // The namespace token for the namespace in which the message has been constructed.
        private bool _standAlone;                   // Set to true if this processor runs in 'stand alone' mode, i.e. not called from an Interface Processor.
        private bool _extSchema;                    // Set to true if the schema is managed outside the scope of the processor, we have limited functionality in this case.
        private bool _generatedOutput;              // Set to true in case processing has yielded some results.
        private OperationDocContext _currentOperationDocContext;    // Currently active documentation context.
        private CommonDocContext _commonDocContext;                 // Documentation context for common definitions.
        private bool _useDocContext;                                // We ONLY use documentation context when not in stand-alone mode.

        /// <summary>
        /// Some getters for private properties:
        /// MessageSchema = The schema that has been generated after parsing the class hierarchy.
        /// QualifiedClassName = Contains the final message name, prefixed with the namespace(token) in which it is defined (token:name).
        /// MessageName = The name of the constructed message element.
        /// MessageNamespace = The namespace in which the message has been constructed.
        /// MessageNSToken = The namespace token for the namespace in which the message has been constructed.
        /// HasGeneratedOutput = Processing has yielded a valid schema (containing at least one entry).
        /// </summary>
        internal Schema MessageSchema                 { get { return this._schema; } }
        internal string QualifiedClassName            { get { return this._qualifiedClassName; } }
        internal string MessageName                   { get { return this._messageName; } }
        internal string MessageNamespace              { get { return this._messageNamespace; } }
        internal string MessageNSToken                { get { return this._messageNsToken; } }
        internal bool HasGeneratedOutput              { get { return this._generatedOutput; } }

        /// <summary>
        /// Default constructor, only initializes context to 'empty'. This is the 'generic' constructor that is invoked when utilizing the Schema Processor
        /// as a 'self-containing' entity. In this setup, it does not use the Common Schema, ALL definitions are collected within the schema itself.
        /// The constructor MUST be declared public since it is called by the .Net Invocation framework, which is in another assembly!
        /// </summary>
        public SchemaProcessor(): base()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor >> Creating new processor (stand-alone).");
            Initialize();
        }

        /// <summary>
        /// Constructor receiving a ready-made schema to be used for all processing. This variant behaves like a stand-alone schema processor,
        /// it does not support documentation generation and does not use a common schema.
        /// We set the mode to 'extSchema' to indicate that this schema is not linked to interactive means and must also not create its own schema.
        /// </summary>
        /// <param name="thisSchema">The schema to be used for processing.</param>
        internal SchemaProcessor(Schema thisSchema): base()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor >> Creating new processor (schema).");
            Initialize();
            this._schema = thisSchema;
            this._extSchema = true;
        }

        /// <summary>
        /// Specialised constructor that is used to create a Schema Processor for processing one or more Messages that are part of an Operation, which in
        /// turn is part of an Interface. This constructor is typically used to create context for processing a series of Operations that share a common
        /// Schema (maintained by the Interface Processor).
        /// </summary>
        /// <param name="commonSchema">Reference to the Common Schema, could be NULL if no Common Schema is used.</param>
        /// <param name="panelIndex">Progress Panel index assigned to this processor.</param>
        internal SchemaProcessor(Schema commonSchema, int panelIndex): base()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor >> Creating new processor (operation).");
            Initialize();
            this._commonSchema = commonSchema;
            this._panelIndex = panelIndex;
            this._standAlone = false;
            this._commonDocContext = DocManagerSlt.GetDocManagerSlt().CommonDocContext;
            this._useDocContext = false;
        }

        /// <summary>
        /// Returns a filename that is constructed as 'Capability_vxpy.extension', in which: 'Capability' is the verbatim name of the current 
        /// Capability, 'x' is the major version of that Capability, 'y' is the minor version and 'extension' is a schema-specific extension
        /// (e.g. 'MyCapability_v1p0.xsd').
        /// Please be aware that this function is ONLY invoked when running the Schema Processor in 'stand-alone' mode!
        /// </summary>
        /// <returns>Capability-specific filename.</returns>
        internal override string GetCapabilityFilename()
        {
            return this._currentCapability.BaseFileName + ((this._schema != null) ? this._schema.GetSchemaFileExtension() : string.Empty);
        }

        /// <summary>
        /// Derived classes must return a processor-specific identifier that can be shown to the user in order to facilitate selection
        /// of a specific processor from a possible list of processors. 
        /// </summary>
        /// <returns>Processor specific identifier.</returns>
        internal override string GetID()
        {
            return "Schema Processor.";
        }

        /// <summary>
        /// Since processors are created once and subsequently re-used, we have to implement this initialization function in order to be sure
        /// that the processor is properly 'reset' for repeated use.
        /// </summary>
        internal override void Initialize()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.Initialize >> Initializing processor.");
            base.Initialize();
            this._schema = null;
            this._commonSchema = null;
            this._panel = null;
            this._lastError = null;
            this._panelIndex = 0;
            this._qualifiedClassName = null;
            this._messageName = null;
            this._messageNamespace = null;
            this._messageNsToken = null;
            this._standAlone = true;
            this._extSchema = false;
            this._generatedOutput = false;
            this._currentOperationDocContext = null;
            this._commonDocContext = null;
            this._useDocContext = false;
        }

        /// <summary>
        /// Main entry for serializing a message model to a schema.
        /// </summary>
        /// <param name="capability">The capability to be processed.</param>
        /// <param name="stage">The current processing stage.</param>
        /// <returns>True on successfull processing, false on errors.</returns>
        internal override bool ProcessCapability(Capability capability, ProcessingStage stage)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.ProcessCapability >> Processing capability: '" +
                             capability.Name + "' in stage: '" + stage + "'...");

            ContextSlt context = ContextSlt.GetContextSlt();
            bool result = true;
            this._currentCapability = capability as MessageCapability;
            this._currentService = capability.RootService;

            try
            {
                switch (stage)
                {
                    // Pre-processing stage is used to check my context and creates a blank Schema instance...
                    // Schema namespace and version are retrieved from our parent Operation object...
                    // In background mode we must not use a panel.
                    case ProcessingStage.PreProcess:
                        if (!this._extSchema) this._panel = ProgressPanelSlt.GetProgressPanelSlt();
                        this._cache = ClassCacheSlt.GetClassCacheSlt();     // Obtain an instance of the cache singleton. In stand-alone mode, 
                                                                            // we must flush it, otherwise we continue where we left of.
                        if (this._standAlone)
                        {
                            this._panel.ShowPanel("Building schema for message: " + capability.Name, 3);
                            this._panel.WriteInfo(this._panelIndex, "Message processing started.");
                            this._cache.Flush();                                            // Assures that we start with an empty cache.
                        }
                        else if (!this._extSchema) this._panel.WriteInfo(this._panelIndex, "Pre-processing Message: '" + capability.Name + "'...");
                        if (!this._extSchema) this._panel.IncreaseBar(1);

                        _majorVersion = capability.RootService.MajorVersion.ToString();     // Major version to be used for all capabilities.
                        _buildNumber = capability.RootService.BuildNumber.ToString();       // Build number to be used for all capabilities.
                        _operationalStatus = capability.RootService.OperationalStatus;      // Operational status to be used for all capabilities.
                        this._lastError = null;

                        if (this._currentCapability == null || this._currentService == null)
                        {
                            Logger.WriteError("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.processCapability >> Illegal context, aborting!");
                            return false;
                        }

                        if (this._standAlone)
                        {
                            // Below sequence assures that we have a valid pathname for the schema file.
                            // In case of non-stand-alone use, this should have been initialized elsewhere.
                            string myPath = this._currentCapability.CapabilityClass.GetTag(context.GetConfigProperty(_PathNameTag));
                            if (string.IsNullOrEmpty(myPath)) myPath = string.Empty;
                            result = (!string.IsNullOrEmpty(this._currentService.AbsolutePath)) || this._currentService.InitializePath(myPath);
                        }

                        if (!this._extSchema)
                        {
                            // Create the new schema at the end of pre-processing stage, so it is available for 'manipulations' if required.
                            // If we're in background mode, the schema must have been passed to the constructor and we can skip this part.
                            OperationCapability parent = ((MessageCapability)this._currentCapability).Parent;
                            string schemaName = this._standAlone ? this._currentCapability.Name : parent.Name;
                            this._schema = GetSchema(Schema.SchemaType.Operation, schemaName, parent.NSToken, parent.FQName, parent.VersionString);
                        }
                        break;

                    // Processing stage is used to create the actual Schema instance...
                    case ProcessingStage.Process:
                        if (!this._extSchema) this._panel.WriteInfo(this._panelIndex, "Processing Message: '" + this._currentCapability.Name + "'...");
                        result = BuildSchema();
                        if (!this._extSchema) this._panel.IncreaseBar(1);
                        break;

                    // Cancelling stage is used to send a message and deleting the Schema...
                    case ProcessingStage.Cancel:
                        if (!this._extSchema)
                        {
                            this._panel.WriteWarning(this._panelIndex, "Cancelling Message: '" + this._currentCapability.Name + "'...");
                            this._schema = null;
                            this._panel.IncreaseBar(1);
                            ClassCacheSlt.GetClassCacheSlt().Flush();
                            if (this._standAlone) this._panel.Done();
                        }
                        break;

                    // Postprocessing is used to save the generated schema, reset the reference count so we're ready for the next round...
                    // Saving etc. is done only in stand-alone mode. In other cases, the created schema will be collected by a processor
                    // higher-up in the chain.
                    // In background mode, we simply keep the state around since the Schema is now managed outside our scope and we just
                    // have to fill it.
                    case ProcessingStage.PostProcess:
                        if (!this._extSchema)
                        {
                            if (this._standAlone)
                            {
                                if (result = SaveProcessedCapability())
                                {
                                    this._panel.WriteInfo(this._panelIndex, "Message processing has been completed successfully.");
                                    this._panel.WriteInfo(this._panelIndex, "Output written to: '" + this._currentService.AbsolutePath + "'.");
                                    if (context.GetBoolSetting(FrameworkSettings._AutoIncrementBuildNumbers)) this._currentService.BuildNumber++;
                                }
                                else this._panel.WriteError(this._panelIndex, "Unable to save Schema file!");
                                this._panel.Done();
                                ClassCacheSlt.GetClassCacheSlt().Flush();   // Release all collected resources (don't need them anymore).
                            }
                            else
                            {
                                this._panel.WriteInfo(this._panelIndex, "Post-processing Message: '" + this._currentCapability.Name + "'...");
                            }
                        }
                        break;

                    default:
                        break;
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.processCapability >> Caught exception: " + exc);
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Processes the entire class hierarchy specified by the given class and stores all generated definitions and declarations in the current
        /// schema. The method returns the qualified name of the processed class on success.
        /// </summary>
        /// <param name="messageClass">Class to be processed.</param>
        /// <param name="token">This is a unique identification of the class in the current processing context and is used to detect which 
        /// classes have been processed already. It must therefor be unique across all classes in the current schema!</param>
        /// <returns>Fully qualified class name (token:name) or empty string in case of errors.</returns>
        internal string ProcessClass(MEClass messageClass, string token)
        {
            if (this._cache == null) this._cache = ClassCacheSlt.GetClassCacheSlt(); // Obtain a valid context for processing.
            return ProcessClass(messageClass, token, 
                                new Tuple<ClassifierContext.ScopeCode, ClassifierContext.DocScopeCode>(ClassifierContext.ScopeCode.Operation, ClassifierContext.DocScopeCode.Common));
        }

        /// <summary>
        /// Special processing function that processes the attributes of the specified class and returns this as a list of Attribute Definitions
        /// that are valid for the current schema type. Since the method uses the 'regular' processing function, attribute classifiers are properly
        /// registered in the schema and can be referred to if needed.
        /// </summary>
        /// <param name="propertyClass">Class to be processed.</param>
        /// <returns>List of attributes from processed class.</returns>
        internal List<SchemaAttribute> ProcessProperties(MEClass propertyClass)
        {
            if (this._cache == null) this._cache = ClassCacheSlt.GetClassCacheSlt(); // Obtain a valid context for property processing.
            return ProcessAttributes(propertyClass, false, 0, propertyClass.Name, ClassifierContext.DocScopeCode.Common);
        }

        /// <summary>
        /// Recursively parses a class hierarchy and creates a schema representation in the local 'Schema' object. If 'CommonSchema' is specified, all 
        /// data type definitions and other classes marked as such will be defined in the CommonSchema. Otherwise, the schema will be 'self-supporting'.
        /// </summary>
        /// <returns>True if generation successfull, false otherwise.</returns>
        private bool BuildSchema()
        {
            bool success                = false;
            OperationCapability parent  = ((MessageCapability)this._currentCapability).Parent;
            ContextSlt context          = ContextSlt.GetContextSlt();
            string headerRole           = context.GetConfigProperty(_MessageHeaderRoleName);
            string bodyRole             = context.GetConfigProperty(_MessageAssemblyRoleName);

            try
            {
                // Retrieve documentation context, which must have been initialized earlier. In stand-alone mode, it will always return NULL.
                // When used in combination with APIProcessor, the context has been correctly set-up and the documentation scope is set to
                // the current message.
                // The documentation context for the Common schema has been created in the constructor since this does not change during the
                // entire processing run (operations obviously will change as we switch from one to the next).
                this._currentOperationDocContext = DocManagerSlt.GetDocManagerSlt().GetOperationDocContext(parent.NSToken);
                this._useDocContext = this._currentOperationDocContext != null;

                Logger.WriteInfo("Plugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.BuildSchema >> Building schema for: " + 
                                 this._currentCapability.Name);

                // If we find a Generalization association at this level, we assume this class is derived from the header root class.
                // Otherwise, if we have a MessageAssociation with 'Body' role, we have the body class, which we want to use as root in case there is no
                // header. This way, we got rid of the Header/Body construct in case of 'plain old message' constructs...
                // The Message Capability class is used to name the message element and type.
                MEClass bodyClass = null;
                bool hasHeader = false;
                foreach (MEAssociation assoc in this._currentCapability.CapabilityClass.AssociationList)
                {
                    if (assoc.TypeOfAssociation == MEAssociation.AssociationType.MessageAssociation &&
                        bodyRole.Equals(assoc.Destination.Role, StringComparison.OrdinalIgnoreCase)) bodyClass = assoc.Destination.EndPoint;
                    else if (assoc.TypeOfAssociation == MEAssociation.AssociationType.Generalization) hasHeader = true;
                }

                // In case of headers, we MUST have something to process (the header). When we don't use headers, it can be so that there is nothing for us
                // to do here! In that case, simply return 'true' to enforce processing to continue with the next message...
                if (!hasHeader && bodyClass == null)
                {
                    Logger.WriteInfo("CPlugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.BuildSchema >> Message type undefined for operation, skip!");
                    this._generatedOutput = false;
                    return true;
                }

                // The first class to be parsed MUST have message scope. This implies a document scope of 'Local'.
                MEClass targetClass = hasHeader ? this._currentCapability.CapabilityClass : bodyClass;
                var scope = new Tuple<ClassifierContext.ScopeCode, ClassifierContext.DocScopeCode>(ClassifierContext.ScopeCode.Message, ClassifierContext.DocScopeCode.Local);
                this._qualifiedClassName = ProcessClass(targetClass, parent.NSToken + ":" + this._currentCapability.Name, scope);
                if (this._qualifiedClassName != string.Empty)
                {
                    // Message parsed ok, now add the message element...
                    // The element name will be a concatenation of the role of the parent and the role of the current message...
                    // We don't have to create documentation for the message element since this has been taken care of by APIProcessor.
                    string unqualifiedClassName = this._qualifiedClassName.Substring(this._qualifiedClassName.IndexOf(':') + 1);  // Remove namespace token.
                    bool isCommonDecl = (!this._qualifiedClassName.Contains(this._schema.NSToken) && this._commonSchema != null) ? true : false;
                    string elementName = Conversions.ToPascalCase(parent.AssignedRole) + Conversions.ToPascalCase(this._currentCapability.AssignedRole);
                    string elementNamespace = (isCommonDecl) ? this._commonSchema.SchemaNamespace : this._schema.SchemaNamespace;
                    success = this._schema.AddElement(elementName, elementNamespace, unqualifiedClassName, GetCapabilityDocumentation(this._currentCapability.CapabilityClass));
                    this._messageName = elementName;
                    this._messageNsToken = (isCommonDecl) ? this._commonSchema.NSToken : this._schema.NSToken;
                    this._messageNamespace = (isCommonDecl) ? this._commonSchema.SchemaNamespace : this._schema.SchemaNamespace;
                    this._generatedOutput = true;
                }

                Logger.WriteInfo("CPlugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.BuildSchema >> Result of operation is: " + success);
                return success;
            }
            catch (Exception exc)
            {
                string msg = "Caught exception while building schema:" + Environment.NewLine + exc.Message;
                Logger.WriteError("CPlugin.Application.CapabilityModel.SchemaGeneration.SchemaProcessor.BuildSchema >> " + msg);
                return false;
            }
        }

        /// <summary>
        /// We can't use the default 'GetDocumentation' function for Capability classes because of the alternative use of the Notes
        /// section. Therefor, we use this replacement function that properly extracts and processes the Documentation text from the 
        /// Notes field without copying all additional contents.
        /// </summary>
        /// <param name="capabilityClass">Capability class we want to process.</param>
        /// <returns>Documentation as a list.</returns>
        private List<MEDocumentation> GetCapabilityDocumentation(MEClass capabilityClass)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string sourceID = context.GetConfigProperty(_DocgenSourcePrefix) + "annotation";
            List<MEDocumentation> capabilityDocList = new List<MEDocumentation>();
            string notes = MEChangeLog.GetDocumentationAsText(capabilityClass);
            if (notes != string.Empty)
            {
                if (notes[0] == '[')
                {
                    // We have a language tag-code, use the contents as language settings...
                    int endBracket = notes.IndexOf(']');
                    string actualNotes = notes.Substring(endBracket + 1);
                    string newLanguageCode = notes.Substring(1, endBracket - 1);
                    // Only accept 2-character language codes, anything else is considered 'no language code'!
                    if (newLanguageCode.Length != 2)
                    {
                        actualNotes = notes;
                        newLanguageCode = "en";
                    }
                    capabilityDocList.Add(new MEDocumentation(sourceID, actualNotes, newLanguageCode));
                }
                else capabilityDocList.Add(new MEDocumentation(sourceID, notes, "en"));
            }
            return capabilityDocList;
        }
    }
}
