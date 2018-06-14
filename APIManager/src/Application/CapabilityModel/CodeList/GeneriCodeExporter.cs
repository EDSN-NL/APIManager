using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Framework.Logging;
using Framework.Util;
using Framework.Util.SchemaManagement.XML;
using Framework.Model;
using Framework.Context;

namespace Plugin.Application.CapabilityModel.CodeList
{
    /// <summary>
    /// CodeList exporter that writes the selected CodeList to a Genericode XML file.
    /// </summary>
    internal class GenericodeExporter: CodeListExporter
    {
        private const string _SPACING = "    ";

        // Configuration properties used by this module:
        private const string _BusinessTermNameTag           = "BusinessTermNameTag";
        private const string _DefinitionTag                 = "DefinitionTag";
        private const string _DictionaryEntryNameTag        = "DictionaryEntryNameTag";
        private const string _UniqueIDTag                   = "UniqueIDTag";
        private const string _DisplayNameTag                = "DisplayNameTag";
        private const string _CodeListNameAttribute         = "CodeListNameAttribute";
        private const string _CodeListAgencyIDAttribute     = "CodeListAgencyIDAttribute";
        private const string _CodeListAgencyNameAttribute   = "CodeListAgencyNameAttribute";
        private const string _CodeListVersionAttribute      = "CodeListVersionAttribute";
        private const string _CodeListURNAttribute          = "CodeListURNAttribute";
        private const string _FrameworkRootPath             = "FrameworkRootPath";
        private const string _IdentifiersPkgName            = "IdentifiersPkgName";
        private const string _EnexisIdentifierName          = "EnexisIdentifierName";
        private const string _AgencyIDToken                 = "AgencyIDToken";
        private const string _AgencyNameToken               = "AgencyNameToken";

        private string _XMLContents;                        // Temporary storage for Genericode output;

        /// <summary>
        /// Default constructor, called during dynamic creation in order to create a new object.
        /// The constructor MUST be declared public since it is called by the .Net Invocation framework, which is in
        /// another assembly!
        /// </summary>
        public GenericodeExporter(): base()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeList.GeneriCodeExporter >> Creating new processor.");
        }

        /// <summary>
        /// Genericode exports create filenames by appending 'CodeList_vxpy' to the name of the current capability, in which
        /// x and y are major- and minor versions respectively. 
        /// The created name always has an '.xml' extension.
        /// When configuration management is enabled, filenames will NOT have a version!
        /// </summary>
        /// <returns>Capability-specific filename.</returns>
        internal override string GetCapabilityFilename()
        {
            return this._currentCapability.BaseFileName + ".xml";
        }

        /// <summary>
        /// Derived classes must return a processor-specific identifier that can be shown to the user in order to facilitate selection
        /// of a specific processor from a possible list of processors. 
        /// </summary>
        /// <returns>Processor specific identifier.</returns>
        internal override string GetID()
        {
            return "GeneriCode exporter.";
        }

        /// <summary>
        /// Genericode exports create filenames by appending 'CodeListSet_vxpy' to the name of the service, in which
        /// x and y are major- and minor versions respectively. If the name already ends with 'List' or 'Lists', we only
        /// append 'Set_vxpy'.
        /// When configuration management is enabled, filenames will NOT have a version!
        /// The created name always has an '.xml' extension.
        /// </summary>
        /// <returns>Capability-specific filename.</returns>
        internal override string GetServiceFilename()
        {
            Tuple<int, int> version = this._currentService.Version;
            string name = this._currentService.Name;
            if (this._currentService.Name.EndsWith("List")) name += "Set";
            else if (this._currentService.Name.EndsWith("Lists"))
            {
                name = name.Substring(0, name.Length - 1);  // Remove last character
                name += "Set";
            }
            else if (!this._currentService.Name.EndsWith("Set")) name += "CodeListSet";
            name += this._currentService.UseConfigurationMgmt? ".xml": ("_v" + version.Item1 + "p" + version.Item2 + ".xml");
            return name;
        }

        /// <summary>
        /// Performs the actual processing on the specified capability. We implement only two stages:
        /// Pre-processing is used to assure that valid pathnames are constructed in our Service object. Since this is checked by the
        /// first capability that is processed, all capabilities in this run will share the same pathname as the name selected for
        /// the first capability. This name is verified by the user and if the user decides to cancel, no work will be lost since
        /// nothing has been processed at that time.
        /// </summary>
        /// <param name="capability">The capability to be processed.</param>
        /// <param name="stage">The current processing stage.</param>
        /// <returns>True on successfull processing, false on errors.</returns>
        internal override bool ProcessCapability(Capability capability, ProcessingStage stage)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeList.GenericodeExporter.processCapability >> Processing capability: '" +
                             capability.Name + "' in stage: '" + stage + "'...");

            // If a panel is active, we use it. However, we do not initialize the panel, that task belongs to Service.
            ProgressPanelSlt panel = ProgressPanelSlt.GetProgressPanelSlt();
            ContextSlt context = ContextSlt.GetContextSlt();
            this._currentCapability = capability as CodeListCapability;
            this._currentService = capability.RootService as CodeListService;
            bool result = true;

            try
            {
                switch (stage)
                {
                    // Pre-processing stage is used to check my context and assures that pathnames are properly initialized...
                    case ProcessingStage.PreProcess:
                        panel.WriteInfo(1, "Pre-processing CodeList: '" + this._currentCapability.Name + "'...");
                        panel.IncreaseBar(1);
                        this._XMLContents = string.Empty;
                        if (this._currentCapability == null || this._currentService == null)
                        {
                            Logger.WriteError("Plugin.Application.CapabilityModel.CodeList.GenericodeExporter.processCapability >> Illegal context, aborting!");
                            return false;
                        }

                        this._currentCapability.CapabilityClass.SetTag(context.GetConfigProperty(_PathNameTag), this._currentService.ServiceBuildPath);
                        break;

                    // Processing stage is used to create the actual Genericode representation and write the result to file...
                    case ProcessingStage.Process:
                        panel.WriteInfo(1, "Processing CodeList: '" + this._currentCapability.Name + "'...");
                        this._XMLContents = BuildCodeList();
                        panel.IncreaseBar(1);
                        result = SaveProcessedCapability();
                        this._currentService.Dirty();   // Mark service as 'modified' for configuration management.
                        break;

                    // Cancelling stage is used only to send a message...
                    case ProcessingStage.Cancel:
                        panel.WriteWarning(1, "Cancelling CodeList: '" + this._currentCapability.Name + "'...");
                        panel.IncreaseBar(1);
                        break;

                    default:
                        // All other stages (post-processing, cancel) do not require any work in this case.
                        break;
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.CodeList.GenericodeExporter.processCapability >> Caught exception: " + exc);
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Performs the actual processing on the specified capability. We implement only two stages:
        /// Pre-processing is used to assure that valid pathnames are constructed in our Service object. Since this is checked by the
        /// first capability that is processed, all capabilities in this run will share the same pathname as the name selected for
        /// the first capability. This name is verified by the user and if the user decides to cancel, no work will be lost since
        /// nothing has been processed at that time.
        /// </summary>
        /// <param name="thisOne"></param>
        /// <returns>True on successfull processing, false on errors.</returns>
        internal override bool ProcessService(Service service, ProcessingStage stage)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeList.GenericodeExporter.processService >> Processing service: '" +
                             service.Name + "' in stage: '" + stage + "'...");

            // Create a progress panel for CodeLists to write to.
            ProgressPanelSlt panel = ProgressPanelSlt.GetProgressPanelSlt();
            ContextSlt context = ContextSlt.GetContextSlt();
            this._currentService = service as CodeListService;
            bool result = true;

            try
            {
                switch (stage)
                {
                    // Initialize and show the panel in Pre-Process stage...
                    case ProcessingStage.PreProcess:
                        panel.ShowPanel("Processing CodeList Set: " + this._currentService.Name, this._currentService.SelectedCapabilities.Count * 2);
                        panel.WriteInfo(0, "Service processing started.");
                        break;

                    // Nothing to do here, just return 'Ok'.
                    case ProcessingStage.Process:
                        break;

                    // Capabilities should have processed their output, collect results here.
                    case ProcessingStage.PostProcess:
                        if (this._currentService == null)
                        {
                            Logger.WriteError("Plugin.Application.CapabilityModel.CodeList.GenericodeExporter.processService >> Illegal context, aborting!");
                            return false;
                        }
                        this._XMLContents = BuildCodeListSet();

                        if (result = SaveProcessedService())
                             panel.WriteInfo(0, "Service processing has been completed successfully.");
                        else panel.WriteError(0, "**ERROR: Unable to save CodeListSet output!");
                        this._currentService.Dirty();   // Mark service as 'modified' for configuration management.
                        panel.Done();
                        break;

                    default:
                        // Cancel, set panel to done...
                        panel.WriteWarning(0, "Service processing has been cancelled!");
                        panel.Done();
                        break;
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.CodeList.GenericodeExporter.processService >> Caught exception: " + exc);
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Specialised processors must implement this method in order to write the contents of the processed capability to the provided
        /// File Stream.
        /// </summary>
        /// <param name="stream">Stream that must receive processed Capability contents.</param>
        protected override void SaveContents(FileStream stream)
        {
            using (var writer = new StreamWriter(stream, Encoding.UTF8)) writer.Write(this._XMLContents);
        }

        /// <summary>
        /// This method performs the actual work of creating a CodeList. The created Genericode string is returned for subsequent 
        /// storage.
        /// </summary>
        /// <returns>Genericode representation of the associated CodeList.</returns>
        private string BuildCodeList()
        {          
            ContextSlt context = ContextSlt.GetContextSlt();
            var cap = this._currentCapability as CodeListCapability;
            string template = context.GetResourceString(FrameworkSettings._GenericodeTemplate);
            string enumAnnotation = cap.SourceEnum.Annotation;
            template = template.Replace("@HEADER@", BuildCapabilityHeader());
            template = template.Replace("@NAME@", cap.Name);
            template = template.Replace("@NOTES@", !string.IsNullOrEmpty(enumAnnotation) ? XmlTextEncoder.Encode(enumAnnotation) : cap.Name);

            // We need to retrieve some information from our CodeType...
            string vsnName = context.GetConfigProperty(_CodeListVersionAttribute);
            string urnName = context.GetConfigProperty(_CodeListURNAttribute);
            string agencyName = context.GetConfigProperty(_CodeListAgencyNameAttribute);
            string agencyID = context.GetConfigProperty(_CodeListAgencyIDAttribute);
            foreach (MEAttribute att in cap.CodeType.Attributes)
            {
                if (att.Name == urnName)
                {
                    template = template.Replace("@CANONICALURI@", cap.CanonicalURI);
                    template = template.Replace("@CANONICALVSURI@", att.FixedValue);
                }
                else if (att.Name == vsnName) template = template.Replace("@VERSION@", att.FixedValue);
                else if (att.Name == agencyName) template = template.Replace("@AGENCYNAME@", att.FixedValue);
                else if (att.Name == agencyID) template = template.Replace("@AGENCYID@", att.FixedValue);
            }
            template = template.Replace("@ROWSET@", BuildRows());
            //Logger.writeInfo("Plugin.Application.CapabilityModel.CodeList.GenericodeExporter.buildCodeList >> Genericode output:\n" + template);
            return template;
        }

        /// <summary>
        /// This method performs the actual work of creating a CodeListSet. The created Genericode string is returned for subsequent 
        /// storage.
        /// </summary>
        /// <returns>Genericode representation of the associated CodeListSet.</returns>
        private string BuildCodeListSet()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            var svc = this._currentService as CodeListService;

            string template = context.GetResourceString(FrameworkSettings._GenericodeSetTemplate);
            template = template.Replace("@HEADER@", BuildServiceHeader());
            template = template.Replace("@NAME@", svc.Name);
            template = template.Replace("@VERSION@", svc.Version.Item1 + "." + svc.Version.Item2);
            template = template.Replace("@CANONICALURI@", svc.GetFQN("CodeListSet", svc.Name, -1));
            template = template.Replace("@CANONICALVSURI@", svc.GetFQN("CodeListSet", null, -1));

            string agencyNameToken = context.GetConfigProperty(_AgencyNameToken);
            string agencyIDToken = context.GetConfigProperty(_AgencyIDToken);
            string identifierPath = context.GetConfigProperty(_FrameworkRootPath) + ":" + context.GetConfigProperty(_IdentifiersPkgName);
            MEObject identifier = model.FindObject(identifierPath, context.GetConfigProperty(_EnexisIdentifierName));
            if (identifier != null)
            {
                List<Tuple<string, string>> stateVars = identifier.RunTimeState;
                for (int i = 0; i < stateVars.Count; i++)
                {
                    if (stateVars[i].Item1 == agencyIDToken) template = template.Replace("@AGENCYID@", stateVars[i].Item2);
                    if (stateVars[i].Item1 == agencyNameToken) template = template.Replace("@AGENCYNAME@", stateVars[i].Item2);
                }
            }
            else
            {
                template = template.Replace("@AGENCYID@", "-unspecified-");
                template = template.Replace("@AGENCYNAME@", "-unspecified-");
            }
            template = template.Replace("@REFSET@", BuildReferences());
            return template;
        }

        /// <summary>
        /// Retrieves and formats the CodeList header. Result is returned as a string.
        /// </summary>
        /// <returns>Formatted header string.</returns>
        private string BuildCapabilityHeader()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string header = context.GetResourceString(FrameworkSettings._CodeListHeader);

            header = header.Replace("@NAME@", this._currentCapability.Name);
            header = header.Replace("@AUTHOR@", this._currentCapability.CapabilityClass.Author);
            header = header.Replace("@TIMESTAMP@", DateTime.Now.ToString());
            header = header.Replace("@YEAR@", DateTime.Now.Year.ToString());
            header = header.Replace("@VERSION@", this._currentCapability.CapabilityClass.Version.Item1 +
                                    "." + this._currentCapability.CapabilityClass.Version.Item2);

            string annotation = this._currentCapability.CapabilityClass.Annotation;
            if (!string.IsNullOrEmpty(annotation))
            {
                var newLog = new MEChangeLog(context.TransformRTF(annotation, RTFDirection.ToRTF));
                header = header.Replace("@CHANGELOG@", newLog.GetLogAsText());
            }
            else header = header.Replace("@CHANGELOG@", string.Empty);
            return header;
        }

        /// <summary>
        /// Helper function for Service processing. The function returnes the, Genericode formatted, list of all CodeList capabilities
        /// that are part of this CodeListSet.
        /// </summary>
        /// <returns>Set of CodeList references in Genericode format.</returns>
        private string BuildReferences()
        {
            string refList = string.Empty;
            string EOL = Environment.NewLine;

            foreach (CodeListCapability capability in this._currentService.SelectedCapabilities)
            {
                this._currentCapability = capability; // Setup context, required for getCapabilityFilename!;
                refList += _SPACING + "<CodeListRef>" + EOL + _SPACING + _SPACING +
                           "<CanonicalUri>" + capability.CanonicalURI + "</CanonicalUri>" + EOL + _SPACING + _SPACING +
                           "<CanonicalVersionUri>" + capability.CanonicalVersionURI + "</CanonicalVersionUri>" + EOL + _SPACING + _SPACING +
                           "<LocationUri>file:" + GetCapabilityFilename() + "</LocationUri>" + EOL + _SPACING + "</CodeListRef>" + EOL;
            }
            return refList;
        }

        /// <summary>
        /// Helper function that iterates over all Facet atrtributes in the CodeList capability. For each attribute found, it retrieves
        /// the corresponding metadata from the source Enumerated type and creates a Genericode row for it. Rows are build dynamically
        /// since not all metadata might be present for all attributes.
        /// </summary>
        /// <returns>Set of rows in Genericode format.</returns>
        private string BuildRows()
        {
            var rowList = new List<string>();
            ContextSlt context = ContextSlt.GetContextSlt();
            string btNameTag = context.GetConfigProperty(_BusinessTermNameTag);
            string definitionTag = context.GetConfigProperty(_DefinitionTag);
            string dictEntryNameTag = context.GetConfigProperty(_DictionaryEntryNameTag);
            string uniqueIDTag = context.GetConfigProperty(_UniqueIDTag);
            string displayNameTag = context.GetConfigProperty(_DisplayNameTag);

            // Process the Facet attributes, yielding CodeList rows...
            foreach (MEAttribute att in this._currentCapability.CapabilityClass.Attributes)
            {
                if (att.Type == ModelElementType.Facet)
                {
                    MEAttribute sourceAttrib = ((CodeListCapability)this._currentCapability).SourceEnum.FindAttribute(att.Name, AttributeType.Facet);
                    if (sourceAttrib != null)
                    {
                        string row = _SPACING + _SPACING + "<Row>" + Environment.NewLine;
                        var values = new List<string>();
                        string currValue;
                        values.Add(FormatRow(GetColumnName("Key"), att.Name));
                        currValue = sourceAttrib.GetTag(uniqueIDTag);
                        if (currValue != string.Empty) values.Add(FormatRow(GetColumnName(_UniqueIDTag), XmlTextEncoder.Encode(currValue)));
                        currValue = sourceAttrib.GetTag(btNameTag);
                        if (currValue != string.Empty) values.Add(FormatRow(GetColumnName(_BusinessTermNameTag), XmlTextEncoder.Encode(currValue)));
                        currValue = sourceAttrib.GetTag(dictEntryNameTag);
                        if (currValue != string.Empty) values.Add(FormatRow(GetColumnName(_DictionaryEntryNameTag), XmlTextEncoder.Encode(currValue)));
                        currValue = sourceAttrib.GetTag(displayNameTag);
                        if (currValue != string.Empty) values.Add(FormatRow(GetColumnName(_DisplayNameTag), XmlTextEncoder.Encode(currValue)));
                        currValue = sourceAttrib.GetTag(definitionTag);
                        if (currValue != string.Empty) values.Add(FormatRow(GetColumnName(_DefinitionTag), XmlTextEncoder.Encode(currValue)));
                        currValue = sourceAttrib.Annotation;
                        if (currValue != string.Empty) values.Add(FormatRow(GetColumnName("Notes"), XmlTextEncoder.Encode(currValue)));

                        foreach (string value in values) row += value;
                        row += _SPACING + _SPACING + "</Row>" + Environment.NewLine;
                        rowList.Add(row);
                    }
                }
            }
            string rowSet = string.Empty;
            foreach (string row in rowList) rowSet += row;
            return rowSet.Substring(0, rowSet.Length - Environment.NewLine.Length);    // Gets rid of the last newline!
        }

        /// <summary>
        /// Retrieves and formats the Service header. Result is returned as a string.
        /// </summary>
        /// <returns>Formatted header string.</returns>
        private string BuildServiceHeader()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string header = context.GetResourceString(FrameworkSettings._CodeListHeader);

            header = header.Replace("@NAME@", this._currentService.Name);
            header = header.Replace("@AUTHOR@", this._currentService.ServiceClass.Author);
            header = header.Replace("@TIMESTAMP@", DateTime.Now.ToString());
            header = header.Replace("@YEAR@", DateTime.Now.Year.ToString());
            header = header.Replace("@VERSION@", this._currentService.Version.Item1 + "." + 
                                    this._currentService.Version.Item2 + " Build: " + this._currentService.BuildNumber);

            string annotation = this._currentService.ServiceClass.Annotation;
            if (!string.IsNullOrEmpty(annotation))
            {
                var newLog = new MEChangeLog(context.TransformRTF(annotation, RTFDirection.ToRTF));
                header = header.Replace("@CHANGELOG@", newLog.GetLogAsText());
            }
            else header = header.Replace("@CHANGELOG@", string.Empty);
            return header;
        }

        /// <summary>
        /// Helper function that returns a Genericode row value based on key/value pair.
        /// </summary>
        /// <param name="key">Genericode Column name</param>
        /// <param name="value">Contents</param>
        /// <returns>Formatted value row.</returns>
        private string FormatRow(string key, string value)
        {
            string EOL = Environment.NewLine;
            string row = _SPACING + _SPACING + _SPACING + "<Value ColumnRef=\"" + key + "\">" + EOL + _SPACING + _SPACING + _SPACING + _SPACING + "<SimpleValue>" + value +
                         "</SimpleValue>" + EOL + _SPACING + _SPACING + _SPACING + "</Value>" + EOL;
            return row;
        }

        /// <summary>
        /// Helper function that translates from a component name (Tag, Notes, etc.) to the designated Genericode column name.
        /// </summary>
        /// <param name="componentName">Component name to translate.</param>
        /// <returns>Associated Genericode column name of empty string on errors.</returns>
        private string GetColumnName (string componentName)
        {
            // Translates a component term to the associated CodeList column name...
            string[,] translateTable =
            { {"Key",                   "KeyName" },
              {_BusinessTermNameTag,    "BusinessTermName"},
              {_DefinitionTag,          "Definition" },
              {_DictionaryEntryNameTag, "DictionaryEntryName" },
              {_UniqueIDTag,            "UniqueID" },
              {_DisplayNameTag,         "DisplayName"},
              {"Notes",                 "Annotation" } };

            for (int i = 0; i < translateTable.GetLength(0); i++)
            {
                if (String.Compare(componentName, translateTable[i, 0], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return translateTable[i, 1];
                }
            }
            return string.Empty;
        }
    }
}
