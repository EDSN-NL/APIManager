using System;
using System.IO;
using System.Text;
using Framework.Logging;
using Framework.Model;
using Framework.Context;
using ClosedXML.Excel;

namespace Plugin.Application.CapabilityModel.CodeList
{
    /// <summary>
    /// Excel exporter that writes the selected CodeList to an Excel file.
    /// </summary>
    sealed internal class ExcelExporter: CodeListExporter, IDisposable
    {
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

        private XLWorkbook _workBook;                       // Temporary Excel storage.
        private IXLWorksheet _workSheet;                    // Currently open Worksheet.
        private int _currentRow;                            // Keeps track of used rows.
        private bool _singleCodeList;                       // Set to 'true' is we're processing only a single CodeList instead of a set.
        private bool _headersDone;                          // Set to 'true' when header row has been written.
        private bool _disposed;                             // Mark myself as invalid after call to dispose!

        /// <summary>
        /// Default constructor is invoked when the ProcessorManager instantiates the ExcelExporter for the first time.
        /// It does only rudimentary initialization since it's not sure the object is actually going to be used.
        /// When an event requests a processor from the ProcessorManager, the manager invokes the 'initialize' method before
        /// returning the processor. This method must do the 'real' initialization work.
        /// The constructor MUST be declared public since it is called by the .Net Invocation framework, which is in
        /// another assembly!
        /// </summary>
        public ExcelExporter(): base()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeList.ExcelExporter >> Creating new processor.");
            this._workBook = null;
            this._currentRow = 2;
            this._singleCodeList = true;
            this._headersDone = false;
            this._workSheet = null;
        }

        /// <summary>
        /// Dispose of the Exporter resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Excel exports create filenames by appending 'CodeList_vxpybz' to the name of the current capability, in which
        /// x and y are major- and minor versions respectively and z is the build number. 
        /// The created name always have an '.xlsx' extension.
        /// </summary>
        /// <returns>Capability-specific filename.</returns>
        internal override string GetCapabilityFilename()
        {
            return this._currentCapability.BaseFileName + "b" + this._currentService.BuildNumber + ".xlsx"; ;
        }

        /// <summary>
        /// Derived classes must return a processor-specific identifier that can be shown to the user in order to facilitate selection
        /// of a specific processor from a possible list of processors. 
        /// </summary>
        /// <returns>Processor specific identifier.</returns>
        internal override string GetID()
        {
            return "Excel exporter.";
        }

        /// <summary>
        /// Excel exports create filenames by appending 'CodeListSet_vxpybz' to the name of the service, in which
        /// x and y are major- and minor versions respectively and z is the build number. If the name already ends with 'List' or 'Lists', we only
        /// append 'Set_vxpybz'.
        /// The created name always has an '.xlsx' extension.
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
            name += this._currentService.UseConfigurationMgmt? ".xlsx": 
                                                               ("_v" + version.Item1 + "p" + version.Item2 + "b" + 
                                                               this._currentService.BuildNumber + ".xlsx");
            return name;
        }

        /// <summary>
        /// The initialize method is called by the Capability Processor manager just before handing over the processor to the requesting
        /// event operation. It is used to (re-)initialize the context before being used in the event.
        /// </summary>
        internal override void Initialize()
        {
            base.Initialize();

            Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeList.ExcelExporter.initialize >> Initializing processor....");
            this._workBook = new XLWorkbook();
            this._currentRow = 2;
            this._singleCodeList = true;
            this._headersDone = false;
            this._workSheet = null;

            this._workBook.PageOptions.PageOrientation = XLPageOrientation.Landscape;
            this._workBook.PageOptions.PaperSize = XLPaperSize.A4Paper;
            this._workBook.PageOptions.Margins.Left = 1;
            this._workBook.PageOptions.Margins.Right = 1;
            this._workBook.PageOptions.Margins.Top = 1;
            this._workBook.PageOptions.Margins.Bottom = 1;
            this._workBook.PageOptions.Margins.Header = 1;
            this._workBook.PageOptions.Margins.Footer = 1;
        }

        /// <summary>
        /// Performs the actual processing on the specified capability. We implement only two stages:
        /// Pre-processing is used to assure that valid pathnames are constructed in our Service object. Since this is checked by the
        /// first capability that is processed, all capabilities in this run will share the same pathname as the name selected for
        /// the first capability.
        /// </summary>
        /// <param name="capability">The capability to be processed.</param>
        /// <param name="stage">The current processing stage.</param>
        /// <returns>True on successfull processing, false on errors.</returns>
        internal override bool ProcessCapability(Capability capability, ProcessingStage stage)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeList.ExcelExporter.processCapability >> Processing capability: '" +
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
                        if (this._currentCapability == null || this._currentService == null)
                        {
                            Logger.WriteError("Plugin.Application.CapabilityModel.CodeList.ExcelExporter.processCapability >> Illegal context, aborting!");
                            return false;
                        }
                        this._currentCapability.CapabilityClass.SetTag(context.GetConfigProperty(_PathNameTag), this._currentService.ServiceBuildPath);
                        if (this._singleCodeList) this._workSheet = this._workBook.Worksheets.Add(this._currentCapability.Name);
                        break;

                    // Processing stage is used to create the actual Excel representation and write the result to file...
                    case ProcessingStage.Process:
                        panel.WriteInfo(1, "Processing CodeList: '" + this._currentCapability.Name + "'...");
                        BuildCodeList();
                        panel.IncreaseBar(1);
                        if (this._singleCodeList)
                        {
                            this._workSheet.Columns(1, 10).AdjustToContents();
                            if (result = SaveProcessedCapability())
                            {
                                panel.WriteInfo(0, "Capability processing has been completed successfully.");
                                this._currentService.Dirty();   // Mark service as 'modified' for Configuration Management.
                            }
                            else panel.WriteError(0, "Unable to save Excel output!");
                        }
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
                Logger.WriteError("Plugin.Application.CapabilityModel.CodeList.ExcelExporter.processCapability >> Caught exception: " + exc);
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
        /// <param name="service">The service to be processed.</param>
        /// <param name="stage">The current processing stage.</param>
        /// <returns>True on successfull processing, false on errors.</returns>
        internal override bool ProcessService(Service service, ProcessingStage stage)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeList.ExcelExporter.processService >> Processing service: '" +
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
                        this._workSheet= this._workBook.Worksheets.Add(this._currentService.Name);
                        this._singleCodeList = false;
                        break;

                    // Nothing to do here, just return 'Ok'.
                    case ProcessingStage.Process:
                        break;

                    // Capabilities should have processed their output, the only thing for us to do is to save the file...
                    case ProcessingStage.PostProcess:
                        if (this._currentService == null)
                        {
                            Logger.WriteError("Plugin.Application.CapabilityModel.CodeList.ExcelExporter.processService >> Illegal context, aborting!");
                            return false;
                        }

                        this._workSheet.Columns(1, 10).AdjustToContents();
                        if (result = SaveProcessedService())
                             panel.WriteInfo(0, "Service processing has been completed successfully.");
                        else panel.WriteError(0, "Unable to save Excel output!");
                        this._currentService.Dirty();       // Mark service as 'modified' for configuration management.
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
                Logger.WriteError("Plugin.Application.CapabilityModel.CodeList.ExcelExporter.processService >> Caught exception: " + exc);
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Since the Excel library has its own save functions, we use a local method for saving the created files. 
        /// </summary>
        /// <returns>True when saved Ok, False on errors.</returns>
        internal override bool SaveProcessedCapability()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            ProgressPanelSlt panel = ProgressPanelSlt.GetProgressPanelSlt();
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeList.ExcelExporter.saveProcessedCapability >> Saving...");

            // As a safety check, we verify that my service contains a valid absolute path. If not, we attempt
            // to create one...
            string fileName = GetCapabilityFilename();
            string pathName = this._currentService.ServiceCIPath;
            bool result = false;

            try
            {
                this._workBook.SaveAs(pathName + "/" + fileName, false);

                // Next, we update the file- and path name tags in our capability class...
                this._currentCapability.CapabilityClass.SetTag(context.GetConfigProperty(_FileNameTag), fileName);
                this._currentCapability.CapabilityClass.SetTag(context.GetConfigProperty(_PathNameTag), this._currentService.ServiceBuildPath);
                result = true;
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.CodeList.CapabilityProcessor.saveProcessedCapability >> Error writing to '" +
                                  pathName + "/" + fileName + "' because:" + Environment.NewLine + exc.Message);
                panel.WriteError(0, "Error writing to '" + pathName + "/" + fileName + "' because:" + Environment.NewLine + exc.Message);
            }
            return result;
        }

        /// <summary>
        /// Since the Excel model has its own save function, we use that instead of our standard mechanism. In this case, saving service or
        /// single CodeList requires the same functionality.
        /// </summary>
        /// <returns>True when saved ok, false on errors.</returns>
        internal override bool SaveProcessedService()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            ProgressPanelSlt panel = ProgressPanelSlt.GetProgressPanelSlt();
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeList.ExcelExporter.saveProcessedService >> Saving...");

            // As a safety check, we verify that my service contains a valid absolute path. If not, we attempt
            // to create one...
            string fileName = GetServiceFilename();
            string pathName = this._currentService.ServiceCIPath;
            bool result = false;

            try
            {
                this._workBook.SaveAs(pathName + "/" + fileName, false);
                result = true;
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.CapabilityModel.CodeList.CapabilityProcessor.saveProcessedService >> Error writing to '" +
                                  pathName + "/" + fileName + "' because:" + Environment.NewLine + exc.Message);
                panel.WriteError(0, "Error writing to '" + pathName + "/" + fileName + "' because:" + Environment.NewLine + exc.Message);
            }
            return result;
        }

        /// <summary>
        /// Specialised processors must implement this method in order to write the contents of the processed capability to the provided
        /// File Stream.
        /// In this particular case, the method will not be used since we override the entire 'Save' function! We keep a sensible 
        /// implementation as safekeeping (in case the base SaveCapability is invoked, we get some results).
        /// </summary>
        /// <param name="stream">Stream that must receive processed Capability contents.</param>
        protected override void SaveContents(FileStream stream)
        {
            using (var writer = new StreamWriter(stream, Encoding.UTF8)) writer.Write(this._workBook.ToString());
        }

        /// <summary>
        /// This method performs the actual work of writing a CodeList to Excel.
        /// </summary>
        /// <returns>Genericode representation of the associated CodeList.</returns>
        private void BuildCodeList()
        {          
            ContextSlt context = ContextSlt.GetContextSlt();
            var cap = this._currentCapability as CodeListCapability;

            if (!this._headersDone)
            {
                this._workSheet.Cell("A1").Value = GetColumnName(_CodeListURNAttribute);
                this._workSheet.Cell("B1").Value = GetColumnName(_AgencyNameToken);
                this._workSheet.Cell("C1").Value = GetColumnName(_AgencyIDToken);
                this._workSheet.Cell("D1").Value = GetColumnName("Key");
                this._workSheet.Cell("E1").Value = GetColumnName(_UniqueIDTag);
                this._workSheet.Cell("F1").Value = GetColumnName(_BusinessTermNameTag);
                this._workSheet.Cell("G1").Value = GetColumnName(_DictionaryEntryNameTag);
                this._workSheet.Cell("H1").Value = GetColumnName(_DisplayNameTag);
                this._workSheet.Cell("I1").Value = GetColumnName(_DefinitionTag);
                this._workSheet.Cell("J1").Value = GetColumnName("Notes");

                IXLRange range = this._workSheet.Range("A1:J1");
                range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range.Style.Font.Bold = true;
                range.Style.Border.BottomBorder = XLBorderStyleValues.Medium;

                //this._workSheet.Column("A").Width = 80;
                this._headersDone = true;
            }

            // Some redundant columns:
            string codeListURI = string.Empty;
            string agencyName = string.Empty;
            string agencyID = string.Empty;

            // We need to retrieve some information from our CodeType...
            string vsnAttName = context.GetConfigProperty(_CodeListVersionAttribute);
            string urnAttName = context.GetConfigProperty(_CodeListURNAttribute);
            string agencyAttName = context.GetConfigProperty(_CodeListAgencyNameAttribute);
            string agencyAttID = context.GetConfigProperty(_CodeListAgencyIDAttribute);
            foreach (MEAttribute att in cap.CodeType.Attributes)
            {
                if (att.Name == urnAttName) codeListURI = att.FixedValue;
                else if (att.Name == agencyAttName) agencyName = att.FixedValue;
                else if (att.Name == agencyAttID) agencyID = att.FixedValue;
            }
            BuildRows(codeListURI, agencyName, agencyID);
        }

        /// <summary>
        /// Helper function that iterates over all Facet atrtributes in the CodeList capability. For each attribute found, it retrieves
        /// the corresponding metadata from the source Enumerated type and creates an Excel row for it. Rows are build dynamically
        /// since not all metadata might be present for all attributes.
        /// </summary>
        private void BuildRows(string codeListURI, string agencyName, string agencyID)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            var cap = this._currentCapability as CodeListCapability;

            string btNameTag = context.GetConfigProperty(_BusinessTermNameTag);
            string definitionTag = context.GetConfigProperty(_DefinitionTag);
            string dictEntryNameTag = context.GetConfigProperty(_DictionaryEntryNameTag);
            string uniqueIDTag = context.GetConfigProperty(_UniqueIDTag);
            string displayNameTag = context.GetConfigProperty(_DisplayNameTag);

            // Process the Facet attributes, yielding CodeList rows...
            foreach (MEAttribute att in cap.CapabilityClass.Attributes)
            {
                if (att.Type == ModelElementType.Facet)
                {
                    MEAttribute sourceAttrib = cap.SourceEnum.FindAttribute(att.Name, AttributeType.Facet);
                    string currValue;
                    if (sourceAttrib != null)
                    {
                        this._workSheet.Cell("A" + this._currentRow).Value = codeListURI;
                        this._workSheet.Cell("B" + this._currentRow).Value = agencyName;
                        this._workSheet.Cell("C" + this._currentRow).Value = agencyID;
                        this._workSheet.Cell("D" + this._currentRow).Value = att.Name;
                        currValue = sourceAttrib.GetTag(uniqueIDTag);
                        if (currValue != string.Empty) this._workSheet.Cell("E" + this._currentRow).Value = currValue;
                        currValue = sourceAttrib.GetTag(btNameTag);
                        if (currValue != string.Empty) this._workSheet.Cell("F" + this._currentRow).Value = currValue;
                        currValue = sourceAttrib.GetTag(dictEntryNameTag);
                        if (currValue != string.Empty) this._workSheet.Cell("G" + this._currentRow).Value = currValue;
                        currValue = sourceAttrib.GetTag(displayNameTag);
                        if (currValue != string.Empty) this._workSheet.Cell("H" + this._currentRow).Value = currValue;
                        currValue = sourceAttrib.GetTag(definitionTag);
                        if (currValue != string.Empty) this._workSheet.Cell("I" + this._currentRow).Value = currValue;
                        currValue = sourceAttrib.Annotation;
                        if (currValue != string.Empty) this._workSheet.Cell("J" + this._currentRow).Value = currValue;
                        this._currentRow++;
                    }
                }
            }
        }

        /// <summary>
        /// This is the actual disposing interface, which takes case of structural removal of the XLS Workbook instance when no longer required.
        /// </summary>
        /// <param name="disposing">Set to 'true' when called directly. Set to 'false' when called from the finalizer.</param>
        private void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                try
                {
                    if (this._workBook != null) this._workBook.Dispose();
                    this._disposed = true;
                }
                catch { };   // Ignore any exceptions, no use in processing them here.
            }
        }

        /// <summary>
        /// Helper function that translates from a component name (Tag, Notes, etc.) to the designated Excel column name.
        /// </summary>
        /// <param name="componentName">Component name to translate.</param>
        /// <returns>Associated Excel column name of empty string on errors.</returns>
        private string GetColumnName (string componentName)
        {
            // Translates a component term to the associated CodeList column name...
            string[,] translateTable =
            { {"Key",                   "KeyName" },
              {_AgencyNameToken,        "AgencyName" },
              {_AgencyIDToken,          "AgencyID" },
              {_BusinessTermNameTag,    "BusinessTermName"},
              {_CodeListURNAttribute,   "CodeListURI" },
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
