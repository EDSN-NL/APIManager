namespace Plugin.Application.Forms
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.UseLogfile = new System.Windows.Forms.CheckBox();
            this.LogfileSelector = new System.Windows.Forms.OpenFileDialog();
            this.LogfileName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SelectFile = new System.Windows.Forms.Button();
            this.Ok = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.SelectRootPath = new System.Windows.Forms.Button();
            this.RootPathName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.RootPathSelector = new System.Windows.Forms.FolderBrowserDialog();
            this.CLAddSourceEnumsToDiagram = new System.Windows.Forms.CheckBox();
            this.CodeLists = new System.Windows.Forms.GroupBox();
            this.CLAddCodeTypesToDiagram = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.AutoIncrementBuildNr = new System.Windows.Forms.CheckBox();
            this.ServiceModel = new System.Windows.Forms.GroupBox();
            this.SMAddBusinessMsgToDiagram = new System.Windows.Forms.CheckBox();
            this.SMUseSecurityLevels = new System.Windows.Forms.CheckBox();
            this.SMUseMsgHeaders = new System.Windows.Forms.CheckBox();
            this.SMCreateCmnSchema = new System.Windows.Forms.CheckBox();
            this.SMAddMessageAssemblyToDiagram = new System.Windows.Forms.CheckBox();
            this.Documentation = new System.Windows.Forms.GroupBox();
            this.DEUniqueID = new System.Windows.Forms.CheckBox();
            this.DEDefinition = new System.Windows.Forms.CheckBox();
            this.DEDictionaryEntryName = new System.Windows.Forms.CheckBox();
            this.DEBusinessTermName = new System.Windows.Forms.CheckBox();
            this.DENotes = new System.Windows.Forms.CheckBox();
            this.DiagramTypes = new System.Windows.Forms.GroupBox();
            this.SaveMsgDiagrams = new System.Windows.Forms.CheckBox();
            this.FormatLabel = new System.Windows.Forms.Label();
            this.DiagramTGA = new System.Windows.Forms.RadioButton();
            this.DiagramSVGZ = new System.Windows.Forms.RadioButton();
            this.DiagramSVG = new System.Windows.Forms.RadioButton();
            this.DiagramPNG = new System.Windows.Forms.RadioButton();
            this.DiagramPDF = new System.Windows.Forms.RadioButton();
            this.DiagramJPG = new System.Windows.Forms.RadioButton();
            this.DiagramGIF = new System.Windows.Forms.RadioButton();
            this.DiagramBMP = new System.Windows.Forms.RadioButton();
            this.InterfaceContracts = new System.Windows.Forms.GroupBox();
            this.IFCSwagger = new System.Windows.Forms.RadioButton();
            this.IFCWSDL = new System.Windows.Forms.RadioButton();
            this.DocumentationGeneration = new System.Windows.Forms.GroupBox();
            this.DocGenUseCommon = new System.Windows.Forms.CheckBox();
            this.VersionControl = new System.Windows.Forms.GroupBox();
            this.RESTAuthentication = new System.Windows.Forms.GroupBox();
            this.RAAPIKeyEdit = new System.Windows.Forms.Button();
            this.RAAPIKeys = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.RAFlow = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.RAScheme = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.RESTParameters = new System.Windows.Forms.GroupBox();
            this.RESTSchemes = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.RESTHostName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.Locking = new System.Windows.Forms.GroupBox();
            this.PersistentLocks = new System.Windows.Forms.CheckBox();
            this.AutoLocking = new System.Windows.Forms.CheckBox();
            this.CodeLists.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.ServiceModel.SuspendLayout();
            this.Documentation.SuspendLayout();
            this.DiagramTypes.SuspendLayout();
            this.InterfaceContracts.SuspendLayout();
            this.DocumentationGeneration.SuspendLayout();
            this.VersionControl.SuspendLayout();
            this.RESTAuthentication.SuspendLayout();
            this.RESTParameters.SuspendLayout();
            this.Locking.SuspendLayout();
            this.SuspendLayout();
            // 
            // UseLogfile
            // 
            this.UseLogfile.AutoSize = true;
            this.UseLogfile.Location = new System.Drawing.Point(6, 45);
            this.UseLogfile.Name = "UseLogfile";
            this.UseLogfile.Size = new System.Drawing.Size(79, 17);
            this.UseLogfile.TabIndex = 2;
            this.UseLogfile.Text = "Use Logfile";
            this.UseLogfile.UseVisualStyleBackColor = true;
            // 
            // LogfileSelector
            // 
            this.LogfileSelector.CheckFileExists = false;
            this.LogfileSelector.DefaultExt = "txt";
            this.LogfileSelector.FileName = "LogfileSelector";
            this.LogfileSelector.Title = "Select Logfile";
            // 
            // LogfileName
            // 
            this.LogfileName.Location = new System.Drawing.Point(69, 19);
            this.LogfileName.Name = "LogfileName";
            this.LogfileName.ReadOnly = true;
            this.LogfileName.Size = new System.Drawing.Size(284, 20);
            this.LogfileName.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Filename:";
            // 
            // SelectFile
            // 
            this.SelectFile.Location = new System.Drawing.Point(359, 18);
            this.SelectFile.Name = "SelectFile";
            this.SelectFile.Size = new System.Drawing.Size(41, 20);
            this.SelectFile.TabIndex = 1;
            this.SelectFile.Text = "...";
            this.SelectFile.UseVisualStyleBackColor = true;
            this.SelectFile.Click += new System.EventHandler(this.SelectLogfile_Click);
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(498, 453);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 31);
            this.Ok.TabIndex = 12;
            this.Ok.Text = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(579, 454);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 11;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // SelectRootPath
            // 
            this.SelectRootPath.Location = new System.Drawing.Point(359, 18);
            this.SelectRootPath.Name = "SelectRootPath";
            this.SelectRootPath.Size = new System.Drawing.Size(41, 20);
            this.SelectRootPath.TabIndex = 1;
            this.SelectRootPath.Text = "...";
            this.SelectRootPath.UseVisualStyleBackColor = true;
            this.SelectRootPath.Click += new System.EventHandler(this.SelectRootPath_Click);
            // 
            // RootPathName
            // 
            this.RootPathName.Location = new System.Drawing.Point(69, 19);
            this.RootPathName.Name = "RootPathName";
            this.RootPathName.ReadOnly = true;
            this.RootPathName.Size = new System.Drawing.Size(284, 20);
            this.RootPathName.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Pathname:";
            // 
            // RootPathSelector
            // 
            this.RootPathSelector.Description = "Select folder to use as root of ECDM output.";
            // 
            // CLAddSourceEnumsToDiagram
            // 
            this.CLAddSourceEnumsToDiagram.AutoSize = true;
            this.CLAddSourceEnumsToDiagram.Location = new System.Drawing.Point(6, 19);
            this.CLAddSourceEnumsToDiagram.Name = "CLAddSourceEnumsToDiagram";
            this.CLAddSourceEnumsToDiagram.Size = new System.Drawing.Size(168, 17);
            this.CLAddSourceEnumsToDiagram.TabIndex = 1;
            this.CLAddSourceEnumsToDiagram.Text = "Show source enum in diagram";
            this.CLAddSourceEnumsToDiagram.UseVisualStyleBackColor = true;
            // 
            // CodeLists
            // 
            this.CodeLists.Controls.Add(this.CLAddCodeTypesToDiagram);
            this.CodeLists.Controls.Add(this.CLAddSourceEnumsToDiagram);
            this.CodeLists.Location = new System.Drawing.Point(12, 145);
            this.CodeLists.Name = "CodeLists";
            this.CodeLists.Size = new System.Drawing.Size(218, 72);
            this.CodeLists.TabIndex = 4;
            this.CodeLists.TabStop = false;
            this.CodeLists.Text = "Code Lists";
            // 
            // CLAddCodeTypesToDiagram
            // 
            this.CLAddCodeTypesToDiagram.AutoSize = true;
            this.CLAddCodeTypesToDiagram.Location = new System.Drawing.Point(6, 42);
            this.CLAddCodeTypesToDiagram.Name = "CLAddCodeTypesToDiagram";
            this.CLAddCodeTypesToDiagram.Size = new System.Drawing.Size(156, 17);
            this.CLAddCodeTypesToDiagram.TabIndex = 2;
            this.CLAddCodeTypesToDiagram.Text = "Show CodeType in diagram";
            this.CLAddCodeTypesToDiagram.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.LogfileName);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.SelectFile);
            this.groupBox1.Controls.Add(this.UseLogfile);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(406, 68);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Logfile";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.RootPathName);
            this.groupBox2.Controls.Add(this.SelectRootPath);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Location = new System.Drawing.Point(12, 86);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(406, 53);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Root path";
            // 
            // AutoIncrementBuildNr
            // 
            this.AutoIncrementBuildNr.AutoSize = true;
            this.AutoIncrementBuildNr.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AutoIncrementBuildNr.Location = new System.Drawing.Point(6, 21);
            this.AutoIncrementBuildNr.Name = "AutoIncrementBuildNr";
            this.AutoIncrementBuildNr.Size = new System.Drawing.Size(165, 17);
            this.AutoIncrementBuildNr.TabIndex = 4;
            this.AutoIncrementBuildNr.Text = "Auto-increment build numbers";
            this.AutoIncrementBuildNr.UseVisualStyleBackColor = true;
            // 
            // ServiceModel
            // 
            this.ServiceModel.Controls.Add(this.SMAddBusinessMsgToDiagram);
            this.ServiceModel.Controls.Add(this.SMUseSecurityLevels);
            this.ServiceModel.Controls.Add(this.SMUseMsgHeaders);
            this.ServiceModel.Controls.Add(this.SMCreateCmnSchema);
            this.ServiceModel.Controls.Add(this.SMAddMessageAssemblyToDiagram);
            this.ServiceModel.Location = new System.Drawing.Point(12, 223);
            this.ServiceModel.Name = "ServiceModel";
            this.ServiceModel.Size = new System.Drawing.Size(218, 143);
            this.ServiceModel.TabIndex = 7;
            this.ServiceModel.TabStop = false;
            this.ServiceModel.Text = "Service Model";
            // 
            // SMAddBusinessMsgToDiagram
            // 
            this.SMAddBusinessMsgToDiagram.AutoSize = true;
            this.SMAddBusinessMsgToDiagram.Location = new System.Drawing.Point(6, 45);
            this.SMAddBusinessMsgToDiagram.Name = "SMAddBusinessMsgToDiagram";
            this.SMAddBusinessMsgToDiagram.Size = new System.Drawing.Size(192, 17);
            this.SMAddBusinessMsgToDiagram.TabIndex = 5;
            this.SMAddBusinessMsgToDiagram.Text = "Show BusinessMessage in diagram";
            this.SMAddBusinessMsgToDiagram.UseVisualStyleBackColor = true;
            this.SMAddBusinessMsgToDiagram.CheckedChanged += new System.EventHandler(this.SMCheckBusinessMsgDependencies);
            // 
            // SMUseSecurityLevels
            // 
            this.SMUseSecurityLevels.AutoSize = true;
            this.SMUseSecurityLevels.Location = new System.Drawing.Point(6, 114);
            this.SMUseSecurityLevels.Name = "SMUseSecurityLevels";
            this.SMUseSecurityLevels.Size = new System.Drawing.Size(120, 17);
            this.SMUseSecurityLevels.TabIndex = 4;
            this.SMUseSecurityLevels.Text = "Use Security Levels";
            this.SMUseSecurityLevels.UseVisualStyleBackColor = true;
            // 
            // SMUseMsgHeaders
            // 
            this.SMUseMsgHeaders.AutoSize = true;
            this.SMUseMsgHeaders.Location = new System.Drawing.Point(6, 91);
            this.SMUseMsgHeaders.Name = "SMUseMsgHeaders";
            this.SMUseMsgHeaders.Size = new System.Drawing.Size(134, 17);
            this.SMUseMsgHeaders.TabIndex = 3;
            this.SMUseMsgHeaders.Text = "Use Message Headers";
            this.SMUseMsgHeaders.UseVisualStyleBackColor = true;
            // 
            // SMCreateCmnSchema
            // 
            this.SMCreateCmnSchema.AutoSize = true;
            this.SMCreateCmnSchema.Location = new System.Drawing.Point(6, 68);
            this.SMCreateCmnSchema.Name = "SMCreateCmnSchema";
            this.SMCreateCmnSchema.Size = new System.Drawing.Size(131, 17);
            this.SMCreateCmnSchema.TabIndex = 2;
            this.SMCreateCmnSchema.Text = "Use Common Schema";
            this.SMCreateCmnSchema.UseVisualStyleBackColor = true;
            // 
            // SMAddMessageAssemblyToDiagram
            // 
            this.SMAddMessageAssemblyToDiagram.AutoSize = true;
            this.SMAddMessageAssemblyToDiagram.Location = new System.Drawing.Point(6, 19);
            this.SMAddMessageAssemblyToDiagram.Name = "SMAddMessageAssemblyToDiagram";
            this.SMAddMessageAssemblyToDiagram.Size = new System.Drawing.Size(194, 17);
            this.SMAddMessageAssemblyToDiagram.TabIndex = 1;
            this.SMAddMessageAssemblyToDiagram.Text = "Show MessageAssembly in diagram";
            this.SMAddMessageAssemblyToDiagram.UseVisualStyleBackColor = true;
            this.SMAddMessageAssemblyToDiagram.CheckedChanged += new System.EventHandler(this.SMCheckMSGAssemblyDependencies);
            // 
            // Documentation
            // 
            this.Documentation.Controls.Add(this.DEUniqueID);
            this.Documentation.Controls.Add(this.DEDefinition);
            this.Documentation.Controls.Add(this.DEDictionaryEntryName);
            this.Documentation.Controls.Add(this.DEBusinessTermName);
            this.Documentation.Controls.Add(this.DENotes);
            this.Documentation.Location = new System.Drawing.Point(236, 223);
            this.Documentation.Name = "Documentation";
            this.Documentation.Size = new System.Drawing.Size(182, 143);
            this.Documentation.TabIndex = 8;
            this.Documentation.TabStop = false;
            this.Documentation.Text = "Documentation Export";
            // 
            // DEUniqueID
            // 
            this.DEUniqueID.AutoSize = true;
            this.DEUniqueID.Location = new System.Drawing.Point(6, 114);
            this.DEUniqueID.Name = "DEUniqueID";
            this.DEUniqueID.Size = new System.Drawing.Size(74, 17);
            this.DEUniqueID.TabIndex = 5;
            this.DEUniqueID.Text = "Unique ID";
            this.DEUniqueID.UseVisualStyleBackColor = true;
            // 
            // DEDefinition
            // 
            this.DEDefinition.AutoSize = true;
            this.DEDefinition.Location = new System.Drawing.Point(6, 91);
            this.DEDefinition.Name = "DEDefinition";
            this.DEDefinition.Size = new System.Drawing.Size(70, 17);
            this.DEDefinition.TabIndex = 4;
            this.DEDefinition.Text = "Definition";
            this.DEDefinition.UseVisualStyleBackColor = true;
            // 
            // DEDictionaryEntryName
            // 
            this.DEDictionaryEntryName.AutoSize = true;
            this.DEDictionaryEntryName.Location = new System.Drawing.Point(6, 68);
            this.DEDictionaryEntryName.Name = "DEDictionaryEntryName";
            this.DEDictionaryEntryName.Size = new System.Drawing.Size(131, 17);
            this.DEDictionaryEntryName.TabIndex = 3;
            this.DEDictionaryEntryName.Text = "Dictionary Entry Name";
            this.DEDictionaryEntryName.UseVisualStyleBackColor = true;
            // 
            // DEBusinessTermName
            // 
            this.DEBusinessTermName.AutoSize = true;
            this.DEBusinessTermName.Location = new System.Drawing.Point(6, 45);
            this.DEBusinessTermName.Name = "DEBusinessTermName";
            this.DEBusinessTermName.Size = new System.Drawing.Size(126, 17);
            this.DEBusinessTermName.TabIndex = 2;
            this.DEBusinessTermName.Text = "Business Term Name";
            this.DEBusinessTermName.UseVisualStyleBackColor = true;
            // 
            // DENotes
            // 
            this.DENotes.AutoSize = true;
            this.DENotes.Location = new System.Drawing.Point(6, 22);
            this.DENotes.Name = "DENotes";
            this.DENotes.Size = new System.Drawing.Size(54, 17);
            this.DENotes.TabIndex = 1;
            this.DENotes.Text = "Notes";
            this.DENotes.UseVisualStyleBackColor = true;
            // 
            // DiagramTypes
            // 
            this.DiagramTypes.Controls.Add(this.SaveMsgDiagrams);
            this.DiagramTypes.Controls.Add(this.FormatLabel);
            this.DiagramTypes.Controls.Add(this.DiagramTGA);
            this.DiagramTypes.Controls.Add(this.DiagramSVGZ);
            this.DiagramTypes.Controls.Add(this.DiagramSVG);
            this.DiagramTypes.Controls.Add(this.DiagramPNG);
            this.DiagramTypes.Controls.Add(this.DiagramPDF);
            this.DiagramTypes.Controls.Add(this.DiagramJPG);
            this.DiagramTypes.Controls.Add(this.DiagramGIF);
            this.DiagramTypes.Controls.Add(this.DiagramBMP);
            this.DiagramTypes.Location = new System.Drawing.Point(427, 12);
            this.DiagramTypes.Name = "DiagramTypes";
            this.DiagramTypes.Size = new System.Drawing.Size(227, 127);
            this.DiagramTypes.TabIndex = 2;
            this.DiagramTypes.TabStop = false;
            this.DiagramTypes.Text = "Save Diagrams";
            // 
            // SaveMsgDiagrams
            // 
            this.SaveMsgDiagrams.AutoSize = true;
            this.SaveMsgDiagrams.Location = new System.Drawing.Point(10, 22);
            this.SaveMsgDiagrams.Name = "SaveMsgDiagrams";
            this.SaveMsgDiagrams.Size = new System.Drawing.Size(176, 17);
            this.SaveMsgDiagrams.TabIndex = 1;
            this.SaveMsgDiagrams.Text = "Save Msg. Diagrams (if present)";
            this.SaveMsgDiagrams.UseVisualStyleBackColor = true;
            // 
            // FormatLabel
            // 
            this.FormatLabel.AutoSize = true;
            this.FormatLabel.Location = new System.Drawing.Point(7, 46);
            this.FormatLabel.Name = "FormatLabel";
            this.FormatLabel.Size = new System.Drawing.Size(113, 13);
            this.FormatLabel.TabIndex = 0;
            this.FormatLabel.Text = "Select format for save:";
            // 
            // DiagramTGA
            // 
            this.DiagramTGA.AutoSize = true;
            this.DiagramTGA.Location = new System.Drawing.Point(157, 95);
            this.DiagramTGA.Name = "DiagramTGA";
            this.DiagramTGA.Size = new System.Drawing.Size(43, 17);
            this.DiagramTGA.TabIndex = 9;
            this.DiagramTGA.TabStop = true;
            this.DiagramTGA.Text = ".tga";
            this.DiagramTGA.UseVisualStyleBackColor = true;
            this.DiagramTGA.CheckedChanged += new System.EventHandler(this.DiagramTypes_CheckedChanged);
            // 
            // DiagramSVGZ
            // 
            this.DiagramSVGZ.AutoSize = true;
            this.DiagramSVGZ.Location = new System.Drawing.Point(109, 95);
            this.DiagramSVGZ.Name = "DiagramSVGZ";
            this.DiagramSVGZ.Size = new System.Drawing.Size(50, 17);
            this.DiagramSVGZ.TabIndex = 8;
            this.DiagramSVGZ.TabStop = true;
            this.DiagramSVGZ.Text = ".svgz";
            this.DiagramSVGZ.UseVisualStyleBackColor = true;
            this.DiagramSVGZ.CheckedChanged += new System.EventHandler(this.DiagramTypes_CheckedChanged);
            // 
            // DiagramSVG
            // 
            this.DiagramSVG.AutoSize = true;
            this.DiagramSVG.Location = new System.Drawing.Point(63, 95);
            this.DiagramSVG.Name = "DiagramSVG";
            this.DiagramSVG.Size = new System.Drawing.Size(45, 17);
            this.DiagramSVG.TabIndex = 7;
            this.DiagramSVG.TabStop = true;
            this.DiagramSVG.Text = ".svg";
            this.DiagramSVG.UseVisualStyleBackColor = true;
            this.DiagramSVG.CheckedChanged += new System.EventHandler(this.DiagramTypes_CheckedChanged);
            // 
            // DiagramPNG
            // 
            this.DiagramPNG.AutoSize = true;
            this.DiagramPNG.Location = new System.Drawing.Point(10, 95);
            this.DiagramPNG.Name = "DiagramPNG";
            this.DiagramPNG.Size = new System.Drawing.Size(46, 17);
            this.DiagramPNG.TabIndex = 6;
            this.DiagramPNG.TabStop = true;
            this.DiagramPNG.Text = ".png";
            this.DiagramPNG.UseVisualStyleBackColor = true;
            this.DiagramPNG.CheckedChanged += new System.EventHandler(this.DiagramTypes_CheckedChanged);
            // 
            // DiagramPDF
            // 
            this.DiagramPDF.AutoSize = true;
            this.DiagramPDF.Location = new System.Drawing.Point(157, 68);
            this.DiagramPDF.Name = "DiagramPDF";
            this.DiagramPDF.Size = new System.Drawing.Size(43, 17);
            this.DiagramPDF.TabIndex = 5;
            this.DiagramPDF.TabStop = true;
            this.DiagramPDF.Text = ".pdf";
            this.DiagramPDF.UseVisualStyleBackColor = true;
            this.DiagramPDF.CheckedChanged += new System.EventHandler(this.DiagramTypes_CheckedChanged);
            // 
            // DiagramJPG
            // 
            this.DiagramJPG.AutoSize = true;
            this.DiagramJPG.Location = new System.Drawing.Point(109, 68);
            this.DiagramJPG.Name = "DiagramJPG";
            this.DiagramJPG.Size = new System.Drawing.Size(42, 17);
            this.DiagramJPG.TabIndex = 4;
            this.DiagramJPG.TabStop = true;
            this.DiagramJPG.Text = ".jpg";
            this.DiagramJPG.UseVisualStyleBackColor = true;
            this.DiagramJPG.CheckedChanged += new System.EventHandler(this.DiagramTypes_CheckedChanged);
            // 
            // DiagramGIF
            // 
            this.DiagramGIF.AutoSize = true;
            this.DiagramGIF.Location = new System.Drawing.Point(63, 68);
            this.DiagramGIF.Name = "DiagramGIF";
            this.DiagramGIF.Size = new System.Drawing.Size(39, 17);
            this.DiagramGIF.TabIndex = 3;
            this.DiagramGIF.TabStop = true;
            this.DiagramGIF.Text = ".gif";
            this.DiagramGIF.UseVisualStyleBackColor = true;
            this.DiagramGIF.CheckedChanged += new System.EventHandler(this.DiagramTypes_CheckedChanged);
            // 
            // DiagramBMP
            // 
            this.DiagramBMP.AutoSize = true;
            this.DiagramBMP.Location = new System.Drawing.Point(10, 68);
            this.DiagramBMP.Name = "DiagramBMP";
            this.DiagramBMP.Size = new System.Drawing.Size(48, 17);
            this.DiagramBMP.TabIndex = 2;
            this.DiagramBMP.TabStop = true;
            this.DiagramBMP.Text = ".bmp";
            this.DiagramBMP.UseVisualStyleBackColor = true;
            this.DiagramBMP.CheckedChanged += new System.EventHandler(this.DiagramTypes_CheckedChanged);
            // 
            // InterfaceContracts
            // 
            this.InterfaceContracts.Controls.Add(this.IFCSwagger);
            this.InterfaceContracts.Controls.Add(this.IFCWSDL);
            this.InterfaceContracts.Location = new System.Drawing.Point(236, 145);
            this.InterfaceContracts.Name = "InterfaceContracts";
            this.InterfaceContracts.Size = new System.Drawing.Size(182, 72);
            this.InterfaceContracts.TabIndex = 5;
            this.InterfaceContracts.TabStop = false;
            this.InterfaceContracts.Text = "Interface Contracts";
            // 
            // IFCSwagger
            // 
            this.IFCSwagger.AutoSize = true;
            this.IFCSwagger.Location = new System.Drawing.Point(6, 41);
            this.IFCSwagger.Name = "IFCSwagger";
            this.IFCSwagger.Size = new System.Drawing.Size(149, 17);
            this.IFCSwagger.TabIndex = 1;
            this.IFCSwagger.TabStop = true;
            this.IFCSwagger.Tag = "REST";
            this.IFCSwagger.Text = "JSON Schema / OpenAPI";
            this.IFCSwagger.UseVisualStyleBackColor = true;
            this.IFCSwagger.CheckedChanged += new System.EventHandler(this.InterfaceContracts_CheckedChanges);
            // 
            // IFCWSDL
            // 
            this.IFCWSDL.AutoSize = true;
            this.IFCWSDL.Location = new System.Drawing.Point(6, 19);
            this.IFCWSDL.Name = "IFCWSDL";
            this.IFCWSDL.Size = new System.Drawing.Size(132, 17);
            this.IFCWSDL.TabIndex = 0;
            this.IFCWSDL.TabStop = true;
            this.IFCWSDL.Tag = "SOAP";
            this.IFCWSDL.Text = "XML Schema / WSDL";
            this.IFCWSDL.UseVisualStyleBackColor = true;
            this.IFCWSDL.CheckedChanged += new System.EventHandler(this.InterfaceContracts_CheckedChanges);
            // 
            // DocumentationGeneration
            // 
            this.DocumentationGeneration.Controls.Add(this.DocGenUseCommon);
            this.DocumentationGeneration.Location = new System.Drawing.Point(427, 281);
            this.DocumentationGeneration.Name = "DocumentationGeneration";
            this.DocumentationGeneration.Size = new System.Drawing.Size(227, 51);
            this.DocumentationGeneration.TabIndex = 6;
            this.DocumentationGeneration.TabStop = false;
            this.DocumentationGeneration.Text = "Documentation Generation";
            // 
            // DocGenUseCommon
            // 
            this.DocGenUseCommon.AutoSize = true;
            this.DocGenUseCommon.Location = new System.Drawing.Point(6, 20);
            this.DocGenUseCommon.Name = "DocGenUseCommon";
            this.DocGenUseCommon.Size = new System.Drawing.Size(179, 17);
            this.DocGenUseCommon.TabIndex = 0;
            this.DocGenUseCommon.Text = "Split Operations and Data Types";
            this.DocGenUseCommon.UseVisualStyleBackColor = true;
            // 
            // VersionControl
            // 
            this.VersionControl.Controls.Add(this.AutoIncrementBuildNr);
            this.VersionControl.Location = new System.Drawing.Point(427, 224);
            this.VersionControl.Name = "VersionControl";
            this.VersionControl.Size = new System.Drawing.Size(227, 51);
            this.VersionControl.TabIndex = 9;
            this.VersionControl.TabStop = false;
            this.VersionControl.Text = "Version Control";
            // 
            // RESTAuthentication
            // 
            this.RESTAuthentication.Controls.Add(this.RAAPIKeyEdit);
            this.RESTAuthentication.Controls.Add(this.RAAPIKeys);
            this.RESTAuthentication.Controls.Add(this.label5);
            this.RESTAuthentication.Controls.Add(this.RAFlow);
            this.RESTAuthentication.Controls.Add(this.label4);
            this.RESTAuthentication.Controls.Add(this.RAScheme);
            this.RESTAuthentication.Controls.Add(this.label3);
            this.RESTAuthentication.Location = new System.Drawing.Point(13, 372);
            this.RESTAuthentication.Name = "RESTAuthentication";
            this.RESTAuthentication.Size = new System.Drawing.Size(217, 113);
            this.RESTAuthentication.TabIndex = 10;
            this.RESTAuthentication.TabStop = false;
            this.RESTAuthentication.Text = "REST Authentication";
            // 
            // RAAPIKeyEdit
            // 
            this.RAAPIKeyEdit.Image = ((System.Drawing.Image)(resources.GetObject("RAAPIKeyEdit.Image")));
            this.RAAPIKeyEdit.Location = new System.Drawing.Point(186, 78);
            this.RAAPIKeyEdit.Name = "RAAPIKeyEdit";
            this.RAAPIKeyEdit.Size = new System.Drawing.Size(25, 25);
            this.RAAPIKeyEdit.TabIndex = 3;
            this.RAAPIKeyEdit.UseVisualStyleBackColor = true;
            this.RAAPIKeyEdit.Click += new System.EventHandler(this.RAAPIKeyEdit_Click);
            // 
            // RAAPIKeys
            // 
            this.RAAPIKeys.Location = new System.Drawing.Point(76, 81);
            this.RAAPIKeys.Name = "RAAPIKeys";
            this.RAAPIKeys.ReadOnly = true;
            this.RAAPIKeys.Size = new System.Drawing.Size(104, 20);
            this.RAAPIKeys.TabIndex = 0;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(2, 84);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "API Keys:";
            // 
            // RAFlow
            // 
            this.RAFlow.FormattingEnabled = true;
            this.RAFlow.Location = new System.Drawing.Point(76, 51);
            this.RAFlow.Name = "RAFlow";
            this.RAFlow.Size = new System.Drawing.Size(135, 21);
            this.RAFlow.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(2, 54);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "OAuth2 Flow:";
            // 
            // RAScheme
            // 
            this.RAScheme.FormattingEnabled = true;
            this.RAScheme.Location = new System.Drawing.Point(76, 22);
            this.RAScheme.Name = "RAScheme";
            this.RAScheme.Size = new System.Drawing.Size(135, 21);
            this.RAScheme.Sorted = true;
            this.RAScheme.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(2, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Scheme:";
            // 
            // RESTParameters
            // 
            this.RESTParameters.Controls.Add(this.RESTSchemes);
            this.RESTParameters.Controls.Add(this.label8);
            this.RESTParameters.Controls.Add(this.RESTHostName);
            this.RESTParameters.Controls.Add(this.label6);
            this.RESTParameters.Location = new System.Drawing.Point(236, 372);
            this.RESTParameters.Name = "RESTParameters";
            this.RESTParameters.Size = new System.Drawing.Size(182, 113);
            this.RESTParameters.TabIndex = 13;
            this.RESTParameters.TabStop = false;
            this.RESTParameters.Text = "REST Interface Parameters";
            // 
            // RESTSchemes
            // 
            this.RESTSchemes.Location = new System.Drawing.Point(67, 52);
            this.RESTSchemes.Name = "RESTSchemes";
            this.RESTSchemes.Size = new System.Drawing.Size(108, 20);
            this.RESTSchemes.TabIndex = 5;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(7, 55);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(54, 13);
            this.label8.TabIndex = 4;
            this.label8.Text = "Schemes:";
            // 
            // RESTHostName
            // 
            this.RESTHostName.Location = new System.Drawing.Point(67, 22);
            this.RESTHostName.Name = "RESTHostName";
            this.RESTHostName.Size = new System.Drawing.Size(108, 20);
            this.RESTHostName.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 25);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(32, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "Host:";
            // 
            // Locking
            // 
            this.Locking.Controls.Add(this.AutoLocking);
            this.Locking.Controls.Add(this.PersistentLocks);
            this.Locking.Location = new System.Drawing.Point(426, 146);
            this.Locking.Name = "Locking";
            this.Locking.Size = new System.Drawing.Size(227, 72);
            this.Locking.TabIndex = 14;
            this.Locking.TabStop = false;
            this.Locking.Text = "Model Access";
            // 
            // PersistentLocks
            // 
            this.PersistentLocks.AutoSize = true;
            this.PersistentLocks.Location = new System.Drawing.Point(6, 40);
            this.PersistentLocks.Name = "PersistentLocks";
            this.PersistentLocks.Size = new System.Drawing.Size(131, 17);
            this.PersistentLocks.TabIndex = 0;
            this.PersistentLocks.Text = "Persistent model locks";
            this.PersistentLocks.UseVisualStyleBackColor = true;
            // 
            // AutoLocking
            // 
            this.AutoLocking.AutoSize = true;
            this.AutoLocking.Location = new System.Drawing.Point(6, 18);
            this.AutoLocking.Name = "AutoLocking";
            this.AutoLocking.Size = new System.Drawing.Size(131, 17);
            this.AutoLocking.TabIndex = 1;
            this.AutoLocking.Text = "Use automatic locking";
            this.AutoLocking.UseVisualStyleBackColor = true;
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(665, 496);
            this.Controls.Add(this.Locking);
            this.Controls.Add(this.RESTParameters);
            this.Controls.Add(this.RESTAuthentication);
            this.Controls.Add(this.VersionControl);
            this.Controls.Add(this.DocumentationGeneration);
            this.Controls.Add(this.InterfaceContracts);
            this.Controls.Add(this.DiagramTypes);
            this.Controls.Add(this.Documentation);
            this.Controls.Add(this.ServiceModel);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.CodeLists);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.Ok);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "API Manager Settings";
            this.CodeLists.ResumeLayout(false);
            this.CodeLists.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ServiceModel.ResumeLayout(false);
            this.ServiceModel.PerformLayout();
            this.Documentation.ResumeLayout(false);
            this.Documentation.PerformLayout();
            this.DiagramTypes.ResumeLayout(false);
            this.DiagramTypes.PerformLayout();
            this.InterfaceContracts.ResumeLayout(false);
            this.InterfaceContracts.PerformLayout();
            this.DocumentationGeneration.ResumeLayout(false);
            this.DocumentationGeneration.PerformLayout();
            this.VersionControl.ResumeLayout(false);
            this.VersionControl.PerformLayout();
            this.RESTAuthentication.ResumeLayout(false);
            this.RESTAuthentication.PerformLayout();
            this.RESTParameters.ResumeLayout(false);
            this.RESTParameters.PerformLayout();
            this.Locking.ResumeLayout(false);
            this.Locking.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox UseLogfile;
        private System.Windows.Forms.OpenFileDialog LogfileSelector;
        private System.Windows.Forms.TextBox LogfileName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button SelectFile;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button SelectRootPath;
        private System.Windows.Forms.TextBox RootPathName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.FolderBrowserDialog RootPathSelector;
        private System.Windows.Forms.CheckBox CLAddSourceEnumsToDiagram;
        private System.Windows.Forms.GroupBox CodeLists;
        private System.Windows.Forms.CheckBox CLAddCodeTypesToDiagram;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox AutoIncrementBuildNr;
        private System.Windows.Forms.GroupBox ServiceModel;
        private System.Windows.Forms.CheckBox SMAddMessageAssemblyToDiagram;
        private System.Windows.Forms.CheckBox SMUseSecurityLevels;
        private System.Windows.Forms.CheckBox SMUseMsgHeaders;
        private System.Windows.Forms.CheckBox SMCreateCmnSchema;
        private System.Windows.Forms.GroupBox Documentation;
        private System.Windows.Forms.CheckBox DEUniqueID;
        private System.Windows.Forms.CheckBox DEDefinition;
        private System.Windows.Forms.CheckBox DEDictionaryEntryName;
        private System.Windows.Forms.CheckBox DEBusinessTermName;
        private System.Windows.Forms.CheckBox DENotes;
        private System.Windows.Forms.GroupBox DiagramTypes;
        private System.Windows.Forms.RadioButton DiagramTGA;
        private System.Windows.Forms.RadioButton DiagramSVGZ;
        private System.Windows.Forms.RadioButton DiagramSVG;
        private System.Windows.Forms.RadioButton DiagramPNG;
        private System.Windows.Forms.RadioButton DiagramPDF;
        private System.Windows.Forms.RadioButton DiagramJPG;
        private System.Windows.Forms.RadioButton DiagramGIF;
        private System.Windows.Forms.RadioButton DiagramBMP;
        private System.Windows.Forms.CheckBox SaveMsgDiagrams;
        private System.Windows.Forms.Label FormatLabel;
        private System.Windows.Forms.GroupBox InterfaceContracts;
        private System.Windows.Forms.RadioButton IFCSwagger;
        private System.Windows.Forms.RadioButton IFCWSDL;
        private System.Windows.Forms.CheckBox SMAddBusinessMsgToDiagram;
        private System.Windows.Forms.GroupBox DocumentationGeneration;
        private System.Windows.Forms.CheckBox DocGenUseCommon;
        private System.Windows.Forms.GroupBox VersionControl;
        private System.Windows.Forms.GroupBox RESTAuthentication;
        private System.Windows.Forms.Button RAAPIKeyEdit;
        private System.Windows.Forms.TextBox RAAPIKeys;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox RAFlow;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox RAScheme;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox RESTParameters;
        private System.Windows.Forms.TextBox RESTSchemes;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox RESTHostName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox Locking;
        private System.Windows.Forms.CheckBox PersistentLocks;
        private System.Windows.Forms.CheckBox AutoLocking;
    }
}