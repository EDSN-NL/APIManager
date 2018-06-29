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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.UseLogfile = new System.Windows.Forms.CheckBox();
            this.LogfileSelector = new System.Windows.Forms.OpenFileDialog();
            this.LogfileName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SelectFile = new System.Windows.Forms.Button();
            this.Ok = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.CLAddSourceEnumsToDiagram = new System.Windows.Forms.CheckBox();
            this.CodeLists = new System.Windows.Forms.GroupBox();
            this.CLAddCodeTypesToDiagram = new System.Windows.Forms.CheckBox();
            this.LogFile = new System.Windows.Forms.GroupBox();
            this.LocalConfigManagement = new System.Windows.Forms.GroupBox();
            this.label13 = new System.Windows.Forms.Label();
            this.GITIgnoreEntries = new System.Windows.Forms.TextBox();
            this.ConfigurationMgmtIndicator = new System.Windows.Forms.CheckBox();
            this.SelectRepoPath = new System.Windows.Forms.Button();
            this.RepoPathName = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.RemoteConfigManagement = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.UseProxy = new System.Windows.Forms.CheckBox();
            this.RepositoryNamespace = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.RepositoryBaseURL = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.AccessToken = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.UserName = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.EMailAddress = new System.Windows.Forms.TextBox();
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
            this.SchemaGeneration = new System.Windows.Forms.GroupBox();
            this.IFCSwagger = new System.Windows.Forms.RadioButton();
            this.IFCWSDL = new System.Windows.Forms.RadioButton();
            this.DocumentationGeneration = new System.Windows.Forms.GroupBox();
            this.DocGenGenerate = new System.Windows.Forms.CheckBox();
            this.DocGenUseCommon = new System.Windows.Forms.CheckBox();
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
            this.AutoLocking = new System.Windows.Forms.CheckBox();
            this.PersistentLocks = new System.Windows.Forms.CheckBox();
            this.JSONSpecifics = new System.Windows.Forms.GroupBox();
            this.SupplementaryPrefixCode = new System.Windows.Forms.TextBox();
            this.SupplementaryPrefixLabel = new System.Windows.Forms.Label();
            this.AttributePrefixToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.RepoPathSelector = new System.Windows.Forms.FolderBrowserDialog();
            this.ConfigMgmtToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.RepositoryRootToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.GITIgnoreToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.ProxyServerName = new System.Windows.Forms.TextBox();
            this.ProxyServer = new System.Windows.Forms.GroupBox();
            this.label15 = new System.Windows.Forms.Label();
            this.ProxyServerPort = new System.Windows.Forms.TextBox();
            this.CodeLists.SuspendLayout();
            this.LogFile.SuspendLayout();
            this.LocalConfigManagement.SuspendLayout();
            this.RemoteConfigManagement.SuspendLayout();
            this.ServiceModel.SuspendLayout();
            this.Documentation.SuspendLayout();
            this.DiagramTypes.SuspendLayout();
            this.SchemaGeneration.SuspendLayout();
            this.DocumentationGeneration.SuspendLayout();
            this.RESTAuthentication.SuspendLayout();
            this.RESTParameters.SuspendLayout();
            this.Locking.SuspendLayout();
            this.JSONSpecifics.SuspendLayout();
            this.ProxyServer.SuspendLayout();
            this.SuspendLayout();
            // 
            // UseLogfile
            // 
            this.UseLogfile.AutoSize = true;
            this.UseLogfile.Location = new System.Drawing.Point(6, 21);
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
            this.LogfileName.Location = new System.Drawing.Point(168, 19);
            this.LogfileName.Name = "LogfileName";
            this.LogfileName.ReadOnly = true;
            this.LogfileName.Size = new System.Drawing.Size(403, 20);
            this.LogfileName.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(110, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Filename:";
            // 
            // SelectFile
            // 
            this.SelectFile.Location = new System.Drawing.Point(577, 19);
            this.SelectFile.Name = "SelectFile";
            this.SelectFile.Size = new System.Drawing.Size(25, 21);
            this.SelectFile.TabIndex = 1;
            this.SelectFile.Text = "...";
            this.SelectFile.UseVisualStyleBackColor = true;
            this.SelectFile.Click += new System.EventHandler(this.SelectLogfile_Click);
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(1094, 358);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 31);
            this.Ok.TabIndex = 15;
            this.Ok.Text = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(1175, 358);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 14;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
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
            this.CodeLists.Location = new System.Drawing.Point(633, 131);
            this.CodeLists.Name = "CodeLists";
            this.CodeLists.Size = new System.Drawing.Size(218, 72);
            this.CodeLists.TabIndex = 5;
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
            // LogFile
            // 
            this.LogFile.Controls.Add(this.LogfileName);
            this.LogFile.Controls.Add(this.label1);
            this.LogFile.Controls.Add(this.SelectFile);
            this.LogFile.Controls.Add(this.UseLogfile);
            this.LogFile.Location = new System.Drawing.Point(12, 12);
            this.LogFile.Name = "LogFile";
            this.LogFile.Size = new System.Drawing.Size(615, 51);
            this.LogFile.TabIndex = 1;
            this.LogFile.TabStop = false;
            this.LogFile.Text = "Logfile";
            // 
            // LocalConfigManagement
            // 
            this.LocalConfigManagement.Controls.Add(this.label13);
            this.LocalConfigManagement.Controls.Add(this.GITIgnoreEntries);
            this.LocalConfigManagement.Controls.Add(this.ConfigurationMgmtIndicator);
            this.LocalConfigManagement.Controls.Add(this.SelectRepoPath);
            this.LocalConfigManagement.Controls.Add(this.RepoPathName);
            this.LocalConfigManagement.Controls.Add(this.label7);
            this.LocalConfigManagement.Location = new System.Drawing.Point(12, 74);
            this.LocalConfigManagement.Name = "LocalConfigManagement";
            this.LocalConfigManagement.Size = new System.Drawing.Size(615, 81);
            this.LocalConfigManagement.TabIndex = 3;
            this.LocalConfigManagement.TabStop = false;
            this.LocalConfigManagement.Text = "Local Configuration Management (Git)";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(67, 50);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(95, 13);
            this.label13.TabIndex = 0;
            this.label13.Text = "GIT Ignore entries:";
            // 
            // GITIgnoreEntries
            // 
            this.GITIgnoreEntries.Location = new System.Drawing.Point(168, 47);
            this.GITIgnoreEntries.Name = "GITIgnoreEntries";
            this.GITIgnoreEntries.Size = new System.Drawing.Size(434, 20);
            this.GITIgnoreEntries.TabIndex = 3;
            this.GITIgnoreEntries.Leave += new System.EventHandler(this.Repoconfig_Leave);
            // 
            // ConfigurationMgmtIndicator
            // 
            this.ConfigurationMgmtIndicator.AutoSize = true;
            this.ConfigurationMgmtIndicator.Location = new System.Drawing.Point(6, 22);
            this.ConfigurationMgmtIndicator.Name = "ConfigurationMgmtIndicator";
            this.ConfigurationMgmtIndicator.Size = new System.Drawing.Size(64, 17);
            this.ConfigurationMgmtIndicator.TabIndex = 1;
            this.ConfigurationMgmtIndicator.Text = "Use CM";
            this.ConfigurationMgmtIndicator.UseVisualStyleBackColor = true;
            this.ConfigurationMgmtIndicator.CheckedChanged += new System.EventHandler(this.ConfigurationMgmtIndicator_CheckedChanged);
            this.ConfigurationMgmtIndicator.Leave += new System.EventHandler(this.Repoconfig_Leave);
            // 
            // SelectRepoPath
            // 
            this.SelectRepoPath.Location = new System.Drawing.Point(577, 20);
            this.SelectRepoPath.Name = "SelectRepoPath";
            this.SelectRepoPath.Size = new System.Drawing.Size(25, 21);
            this.SelectRepoPath.TabIndex = 2;
            this.SelectRepoPath.Text = "...";
            this.SelectRepoPath.UseVisualStyleBackColor = true;
            this.SelectRepoPath.Click += new System.EventHandler(this.SelectLocalRepoPath_Click);
            // 
            // RepoPathName
            // 
            this.RepoPathName.Location = new System.Drawing.Point(168, 20);
            this.RepoPathName.Name = "RepoPathName";
            this.RepoPathName.ReadOnly = true;
            this.RepoPathName.Size = new System.Drawing.Size(403, 20);
            this.RepoPathName.TabIndex = 0;
            this.RepoPathName.Leave += new System.EventHandler(this.Repoconfig_Leave);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(76, 23);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(86, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "Repository Root:";
            // 
            // RemoteConfigManagement
            // 
            this.RemoteConfigManagement.Controls.Add(this.ProxyServer);
            this.RemoteConfigManagement.Controls.Add(this.RepositoryNamespace);
            this.RemoteConfigManagement.Controls.Add(this.label14);
            this.RemoteConfigManagement.Controls.Add(this.RepositoryBaseURL);
            this.RemoteConfigManagement.Controls.Add(this.label12);
            this.RemoteConfigManagement.Controls.Add(this.AccessToken);
            this.RemoteConfigManagement.Controls.Add(this.label11);
            this.RemoteConfigManagement.Controls.Add(this.label9);
            this.RemoteConfigManagement.Controls.Add(this.UserName);
            this.RemoteConfigManagement.Controls.Add(this.label10);
            this.RemoteConfigManagement.Controls.Add(this.EMailAddress);
            this.RemoteConfigManagement.Location = new System.Drawing.Point(12, 161);
            this.RemoteConfigManagement.Name = "RemoteConfigManagement";
            this.RemoteConfigManagement.Size = new System.Drawing.Size(615, 166);
            this.RemoteConfigManagement.TabIndex = 4;
            this.RemoteConfigManagement.TabStop = false;
            this.RemoteConfigManagement.Text = "Remote Configuration Management (GitLab)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(137, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Name:";
            // 
            // UseProxy
            // 
            this.UseProxy.AutoSize = true;
            this.UseProxy.Location = new System.Drawing.Point(6, 21);
            this.UseProxy.Name = "UseProxy";
            this.UseProxy.Size = new System.Drawing.Size(108, 17);
            this.UseProxy.TabIndex = 3;
            this.UseProxy.Text = "Use Proxy Server";
            this.UseProxy.UseVisualStyleBackColor = true;
            // 
            // RepositoryNamespace
            // 
            this.RepositoryNamespace.Location = new System.Drawing.Point(390, 74);
            this.RepositoryNamespace.Name = "RepositoryNamespace";
            this.RepositoryNamespace.Size = new System.Drawing.Size(212, 20);
            this.RepositoryNamespace.TabIndex = 5;
            this.RepositoryNamespace.Leave += new System.EventHandler(this.Repoconfig_Leave);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(285, 77);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(99, 13);
            this.label14.TabIndex = 0;
            this.label14.Text = "Repo. Namespace:";
            // 
            // RepositoryBaseURL
            // 
            this.RepositoryBaseURL.Location = new System.Drawing.Point(91, 22);
            this.RepositoryBaseURL.Name = "RepositoryBaseURL";
            this.RepositoryBaseURL.Size = new System.Drawing.Size(511, 20);
            this.RepositoryBaseURL.TabIndex = 1;
            this.RepositoryBaseURL.Leave += new System.EventHandler(this.Repoconfig_Leave);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(26, 25);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(59, 13);
            this.label12.TabIndex = 0;
            this.label12.Text = "Base URL:";
            // 
            // AccessToken
            // 
            this.AccessToken.Location = new System.Drawing.Point(91, 74);
            this.AccessToken.Name = "AccessToken";
            this.AccessToken.Size = new System.Drawing.Size(188, 20);
            this.AccessToken.TabIndex = 4;
            this.AccessToken.Leave += new System.EventHandler(this.Repoconfig_Leave);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 77);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(79, 13);
            this.label11.TabIndex = 0;
            this.label11.Text = "Access Token:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(22, 51);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(63, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "User Name:";
            // 
            // UserName
            // 
            this.UserName.Location = new System.Drawing.Point(91, 48);
            this.UserName.Name = "UserName";
            this.UserName.Size = new System.Drawing.Size(188, 20);
            this.UserName.TabIndex = 2;
            this.UserName.Leave += new System.EventHandler(this.Repoconfig_Leave);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(305, 51);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(79, 13);
            this.label10.TabIndex = 0;
            this.label10.Text = "E-mail Address:";
            // 
            // EMailAddress
            // 
            this.EMailAddress.Location = new System.Drawing.Point(390, 48);
            this.EMailAddress.Name = "EMailAddress";
            this.EMailAddress.Size = new System.Drawing.Size(212, 20);
            this.EMailAddress.TabIndex = 3;
            this.EMailAddress.Leave += new System.EventHandler(this.Repoconfig_Leave);
            // 
            // ServiceModel
            // 
            this.ServiceModel.Controls.Add(this.SMAddBusinessMsgToDiagram);
            this.ServiceModel.Controls.Add(this.SMUseSecurityLevels);
            this.ServiceModel.Controls.Add(this.SMUseMsgHeaders);
            this.ServiceModel.Controls.Add(this.SMCreateCmnSchema);
            this.ServiceModel.Controls.Add(this.SMAddMessageAssemblyToDiagram);
            this.ServiceModel.Location = new System.Drawing.Point(633, 209);
            this.ServiceModel.Name = "ServiceModel";
            this.ServiceModel.Size = new System.Drawing.Size(218, 143);
            this.ServiceModel.TabIndex = 8;
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
            this.SMUseMsgHeaders.Size = new System.Drawing.Size(159, 17);
            this.SMUseMsgHeaders.TabIndex = 3;
            this.SMUseMsgHeaders.Text = "Generate Message Headers";
            this.SMUseMsgHeaders.UseVisualStyleBackColor = true;
            // 
            // SMCreateCmnSchema
            // 
            this.SMCreateCmnSchema.AutoSize = true;
            this.SMCreateCmnSchema.Location = new System.Drawing.Point(6, 68);
            this.SMCreateCmnSchema.Name = "SMCreateCmnSchema";
            this.SMCreateCmnSchema.Size = new System.Drawing.Size(180, 17);
            this.SMCreateCmnSchema.TabIndex = 2;
            this.SMCreateCmnSchema.Text = "Use/Generate Common Schema";
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
            this.Documentation.Location = new System.Drawing.Point(857, 209);
            this.Documentation.Name = "Documentation";
            this.Documentation.Size = new System.Drawing.Size(182, 143);
            this.Documentation.TabIndex = 9;
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
            this.DiagramTypes.Location = new System.Drawing.Point(1045, 12);
            this.DiagramTypes.Name = "DiagramTypes";
            this.DiagramTypes.Size = new System.Drawing.Size(205, 115);
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
            this.DiagramTGA.Location = new System.Drawing.Point(157, 91);
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
            this.DiagramSVGZ.Location = new System.Drawing.Point(109, 91);
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
            this.DiagramSVG.Location = new System.Drawing.Point(63, 91);
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
            this.DiagramPNG.Location = new System.Drawing.Point(10, 91);
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
            // SchemaGeneration
            // 
            this.SchemaGeneration.Controls.Add(this.IFCSwagger);
            this.SchemaGeneration.Controls.Add(this.IFCWSDL);
            this.SchemaGeneration.Location = new System.Drawing.Point(857, 133);
            this.SchemaGeneration.Name = "SchemaGeneration";
            this.SchemaGeneration.Size = new System.Drawing.Size(182, 72);
            this.SchemaGeneration.TabIndex = 6;
            this.SchemaGeneration.TabStop = false;
            this.SchemaGeneration.Text = "Schema Generation";
            // 
            // IFCSwagger
            // 
            this.IFCSwagger.AutoSize = true;
            this.IFCSwagger.Location = new System.Drawing.Point(6, 41);
            this.IFCSwagger.Name = "IFCSwagger";
            this.IFCSwagger.Size = new System.Drawing.Size(95, 17);
            this.IFCSwagger.TabIndex = 1;
            this.IFCSwagger.TabStop = true;
            this.IFCSwagger.Tag = "REST";
            this.IFCSwagger.Text = "JSON Schema";
            this.IFCSwagger.UseVisualStyleBackColor = true;
            this.IFCSwagger.CheckedChanged += new System.EventHandler(this.InterfaceContracts_CheckedChanges);
            // 
            // IFCWSDL
            // 
            this.IFCWSDL.AutoSize = true;
            this.IFCWSDL.Location = new System.Drawing.Point(6, 19);
            this.IFCWSDL.Name = "IFCWSDL";
            this.IFCWSDL.Size = new System.Drawing.Size(89, 17);
            this.IFCWSDL.TabIndex = 0;
            this.IFCWSDL.TabStop = true;
            this.IFCWSDL.Tag = "SOAP";
            this.IFCWSDL.Text = "XML Schema";
            this.IFCWSDL.UseVisualStyleBackColor = true;
            this.IFCWSDL.CheckedChanged += new System.EventHandler(this.InterfaceContracts_CheckedChanges);
            // 
            // DocumentationGeneration
            // 
            this.DocumentationGeneration.Controls.Add(this.DocGenGenerate);
            this.DocumentationGeneration.Controls.Add(this.DocGenUseCommon);
            this.DocumentationGeneration.Location = new System.Drawing.Point(1044, 209);
            this.DocumentationGeneration.Name = "DocumentationGeneration";
            this.DocumentationGeneration.Size = new System.Drawing.Size(206, 65);
            this.DocumentationGeneration.TabIndex = 10;
            this.DocumentationGeneration.TabStop = false;
            this.DocumentationGeneration.Text = "Documentation Generation";
            // 
            // DocGenGenerate
            // 
            this.DocGenGenerate.AutoSize = true;
            this.DocGenGenerate.Location = new System.Drawing.Point(5, 19);
            this.DocGenGenerate.Name = "DocGenGenerate";
            this.DocGenGenerate.Size = new System.Drawing.Size(143, 17);
            this.DocGenGenerate.TabIndex = 1;
            this.DocGenGenerate.Text = "Generate documentation";
            this.DocGenGenerate.UseVisualStyleBackColor = true;
            // 
            // DocGenUseCommon
            // 
            this.DocGenUseCommon.AutoSize = true;
            this.DocGenUseCommon.Location = new System.Drawing.Point(5, 42);
            this.DocGenUseCommon.Name = "DocGenUseCommon";
            this.DocGenUseCommon.Size = new System.Drawing.Size(179, 17);
            this.DocGenUseCommon.TabIndex = 0;
            this.DocGenUseCommon.Text = "Split Operations and Data Types";
            this.DocGenUseCommon.UseVisualStyleBackColor = true;
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
            this.RESTAuthentication.Location = new System.Drawing.Point(633, 12);
            this.RESTAuthentication.Name = "RESTAuthentication";
            this.RESTAuthentication.Size = new System.Drawing.Size(217, 113);
            this.RESTAuthentication.TabIndex = 12;
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
            this.RESTParameters.Location = new System.Drawing.Point(857, 14);
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
            this.Locking.Location = new System.Drawing.Point(1045, 133);
            this.Locking.Name = "Locking";
            this.Locking.Size = new System.Drawing.Size(205, 72);
            this.Locking.TabIndex = 7;
            this.Locking.TabStop = false;
            this.Locking.Text = "Model Access";
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
            // PersistentLocks
            // 
            this.PersistentLocks.AutoSize = true;
            this.PersistentLocks.Location = new System.Drawing.Point(6, 40);
            this.PersistentLocks.Name = "PersistentLocks";
            this.PersistentLocks.Size = new System.Drawing.Size(100, 17);
            this.PersistentLocks.TabIndex = 0;
            this.PersistentLocks.Text = "Persistent locks";
            this.PersistentLocks.UseVisualStyleBackColor = true;
            // 
            // JSONSpecifics
            // 
            this.JSONSpecifics.Controls.Add(this.SupplementaryPrefixCode);
            this.JSONSpecifics.Controls.Add(this.SupplementaryPrefixLabel);
            this.JSONSpecifics.Location = new System.Drawing.Point(1045, 280);
            this.JSONSpecifics.Name = "JSONSpecifics";
            this.JSONSpecifics.Size = new System.Drawing.Size(205, 72);
            this.JSONSpecifics.TabIndex = 11;
            this.JSONSpecifics.TabStop = false;
            this.JSONSpecifics.Text = "JSON Settings";
            // 
            // SupplementaryPrefixCode
            // 
            this.SupplementaryPrefixCode.Location = new System.Drawing.Point(111, 16);
            this.SupplementaryPrefixCode.Name = "SupplementaryPrefixCode";
            this.SupplementaryPrefixCode.Size = new System.Drawing.Size(35, 20);
            this.SupplementaryPrefixCode.TabIndex = 1;
            // 
            // SupplementaryPrefixLabel
            // 
            this.SupplementaryPrefixLabel.AutoSize = true;
            this.SupplementaryPrefixLabel.Location = new System.Drawing.Point(2, 19);
            this.SupplementaryPrefixLabel.Name = "SupplementaryPrefixLabel";
            this.SupplementaryPrefixLabel.Size = new System.Drawing.Size(109, 13);
            this.SupplementaryPrefixLabel.TabIndex = 0;
            this.SupplementaryPrefixLabel.Text = "Supplementary Prefix:";
            // 
            // RepoPathSelector
            // 
            this.RepoPathSelector.Description = "Select folder that is the root of the local GIT Repository";
            // 
            // ProxyServerName
            // 
            this.ProxyServerName.Location = new System.Drawing.Point(181, 19);
            this.ProxyServerName.Name = "ProxyServerName";
            this.ProxyServerName.Size = new System.Drawing.Size(265, 20);
            this.ProxyServerName.TabIndex = 1;
            // 
            // ProxyServer
            // 
            this.ProxyServer.Controls.Add(this.ProxyServerPort);
            this.ProxyServer.Controls.Add(this.label15);
            this.ProxyServer.Controls.Add(this.UseProxy);
            this.ProxyServer.Controls.Add(this.ProxyServerName);
            this.ProxyServer.Controls.Add(this.label2);
            this.ProxyServer.Location = new System.Drawing.Point(9, 100);
            this.ProxyServer.Name = "ProxyServer";
            this.ProxyServer.Size = new System.Drawing.Size(593, 56);
            this.ProxyServer.TabIndex = 6;
            this.ProxyServer.TabStop = false;
            this.ProxyServer.Text = "Proxy Server";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(452, 22);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(29, 13);
            this.label15.TabIndex = 10;
            this.label15.Text = "Port:";
            // 
            // ProxyServerPort
            // 
            this.ProxyServerPort.Location = new System.Drawing.Point(487, 19);
            this.ProxyServerPort.Name = "ProxyServerPort";
            this.ProxyServerPort.Size = new System.Drawing.Size(100, 20);
            this.ProxyServerPort.TabIndex = 2;
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(1264, 402);
            this.Controls.Add(this.RemoteConfigManagement);
            this.Controls.Add(this.JSONSpecifics);
            this.Controls.Add(this.Locking);
            this.Controls.Add(this.RESTParameters);
            this.Controls.Add(this.RESTAuthentication);
            this.Controls.Add(this.DocumentationGeneration);
            this.Controls.Add(this.SchemaGeneration);
            this.Controls.Add(this.DiagramTypes);
            this.Controls.Add(this.Documentation);
            this.Controls.Add(this.ServiceModel);
            this.Controls.Add(this.LocalConfigManagement);
            this.Controls.Add(this.LogFile);
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
            this.LogFile.ResumeLayout(false);
            this.LogFile.PerformLayout();
            this.LocalConfigManagement.ResumeLayout(false);
            this.LocalConfigManagement.PerformLayout();
            this.RemoteConfigManagement.ResumeLayout(false);
            this.RemoteConfigManagement.PerformLayout();
            this.ServiceModel.ResumeLayout(false);
            this.ServiceModel.PerformLayout();
            this.Documentation.ResumeLayout(false);
            this.Documentation.PerformLayout();
            this.DiagramTypes.ResumeLayout(false);
            this.DiagramTypes.PerformLayout();
            this.SchemaGeneration.ResumeLayout(false);
            this.SchemaGeneration.PerformLayout();
            this.DocumentationGeneration.ResumeLayout(false);
            this.DocumentationGeneration.PerformLayout();
            this.RESTAuthentication.ResumeLayout(false);
            this.RESTAuthentication.PerformLayout();
            this.RESTParameters.ResumeLayout(false);
            this.RESTParameters.PerformLayout();
            this.Locking.ResumeLayout(false);
            this.Locking.PerformLayout();
            this.JSONSpecifics.ResumeLayout(false);
            this.JSONSpecifics.PerformLayout();
            this.ProxyServer.ResumeLayout(false);
            this.ProxyServer.PerformLayout();
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
        private System.Windows.Forms.CheckBox CLAddSourceEnumsToDiagram;
        private System.Windows.Forms.GroupBox CodeLists;
        private System.Windows.Forms.CheckBox CLAddCodeTypesToDiagram;
        private System.Windows.Forms.GroupBox LogFile;
        private System.Windows.Forms.GroupBox LocalConfigManagement;
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
        private System.Windows.Forms.GroupBox SchemaGeneration;
        private System.Windows.Forms.RadioButton IFCSwagger;
        private System.Windows.Forms.RadioButton IFCWSDL;
        private System.Windows.Forms.CheckBox SMAddBusinessMsgToDiagram;
        private System.Windows.Forms.GroupBox DocumentationGeneration;
        private System.Windows.Forms.CheckBox DocGenUseCommon;
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
        private System.Windows.Forms.GroupBox JSONSpecifics;
        private System.Windows.Forms.TextBox SupplementaryPrefixCode;
        private System.Windows.Forms.Label SupplementaryPrefixLabel;
        private System.Windows.Forms.ToolTip AttributePrefixToolTip;
        private System.Windows.Forms.CheckBox DocGenGenerate;
        private System.Windows.Forms.Button SelectRepoPath;
        private System.Windows.Forms.TextBox RepoPathName;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox RemoteConfigManagement;
        private System.Windows.Forms.TextBox AccessToken;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox EMailAddress;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox UserName;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox RepositoryBaseURL;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.FolderBrowserDialog RepoPathSelector;
        private System.Windows.Forms.CheckBox ConfigurationMgmtIndicator;
        private System.Windows.Forms.ToolTip ConfigMgmtToolTip;
        private System.Windows.Forms.TextBox GITIgnoreEntries;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.ToolTip RepositoryRootToolTip;
        private System.Windows.Forms.ToolTip GITIgnoreToolTip;
        private System.Windows.Forms.TextBox RepositoryNamespace;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox UseProxy;
        private System.Windows.Forms.GroupBox ProxyServer;
        private System.Windows.Forms.TextBox ProxyServerPort;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox ProxyServerName;
    }
}