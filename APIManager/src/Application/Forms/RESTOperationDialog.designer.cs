using System.Windows.Forms;

namespace Plugin.Application.Forms
{
    partial class RESTOperationDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RESTOperationDialog));
            this.Cancel = new System.Windows.Forms.Button();
            this.Ok = new System.Windows.Forms.Button();
            this.OperationNamesLbl = new System.Windows.Forms.Label();
            this.OperationNameFld = new System.Windows.Forms.TextBox();
            this.OperationTypeFld = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.OperationGroup = new System.Windows.Forms.GroupBox();
            this.UseHeaderParameters = new System.Windows.Forms.CheckBox();
            this.HasPagination = new System.Windows.Forms.CheckBox();
            this.NewMinorVersion = new System.Windows.Forms.CheckBox();
            this.FilterGroup = new System.Windows.Forms.GroupBox();
            this.EditParameter = new System.Windows.Forms.Button();
            this.DeleteFilter = new System.Windows.Forms.Button();
            this.FilterParameterList = new System.Windows.Forms.ListView();
            this.ParamName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ParamType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.AddFilter = new System.Windows.Forms.Button();
            this.ResponseCodeGroup = new System.Windows.Forms.GroupBox();
            this.UseCollection = new System.Windows.Forms.Button();
            this.EditCollections = new System.Windows.Forms.Button();
            this.EditResponseCode = new System.Windows.Forms.Button();
            this.DeleteResponseCode = new System.Windows.Forms.Button();
            this.AddResponseCode = new System.Windows.Forms.Button();
            this.ResponseCodeList = new System.Windows.Forms.ListView();
            this.Code = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ResponseDesc = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.FilterParametersMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ResponseCodeMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.useCollectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageCollectionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MimeBox = new System.Windows.Forms.GroupBox();
            this.ConsumesMIME = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.ProducesMIME = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.DocumentationBox = new System.Windows.Forms.GroupBox();
            this.Description = new System.Windows.Forms.TextBox();
            this.SummaryText = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.RequestParamBox = new System.Windows.Forms.GroupBox();
            this.ReqCardinalityGroup = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.ReqCardHi = new System.Windows.Forms.TextBox();
            this.ReqCardLo = new System.Windows.Forms.TextBox();
            this.ReqSecurityGroup = new System.Windows.Forms.GroupBox();
            this.ReqEncryption = new System.Windows.Forms.CheckBox();
            this.ReqSigning = new System.Windows.Forms.CheckBox();
            this.RemoveRequest = new System.Windows.Forms.Button();
            this.RequestTypeName = new System.Windows.Forms.TextBox();
            this.SelectRequest = new System.Windows.Forms.Button();
            this.ResponseParamBox = new System.Windows.Forms.GroupBox();
            this.RspCardinalityGroup = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.RspCardHi = new System.Windows.Forms.TextBox();
            this.RspCardLo = new System.Windows.Forms.TextBox();
            this.RspSecurityGroup = new System.Windows.Forms.GroupBox();
            this.RspEncryption = new System.Windows.Forms.CheckBox();
            this.RspSigning = new System.Windows.Forms.CheckBox();
            this.RemoveResponse = new System.Windows.Forms.Button();
            this.SelectResponse = new System.Windows.Forms.Button();
            this.ResponseTypeName = new System.Windows.Forms.TextBox();
            this.OperationGroup.SuspendLayout();
            this.FilterGroup.SuspendLayout();
            this.ResponseCodeGroup.SuspendLayout();
            this.FilterParametersMenuStrip.SuspendLayout();
            this.ResponseCodeMenuStrip.SuspendLayout();
            this.MimeBox.SuspendLayout();
            this.DocumentationBox.SuspendLayout();
            this.RequestParamBox.SuspendLayout();
            this.ReqCardinalityGroup.SuspendLayout();
            this.ReqSecurityGroup.SuspendLayout();
            this.ResponseParamBox.SuspendLayout();
            this.RspCardinalityGroup.SuspendLayout();
            this.RspSecurityGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(523, 558);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 12;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(442, 558);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 31);
            this.Ok.TabIndex = 13;
            this.Ok.Text = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            // 
            // OperationNamesLbl
            // 
            this.OperationNamesLbl.AutoSize = true;
            this.OperationNamesLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OperationNamesLbl.Location = new System.Drawing.Point(6, 21);
            this.OperationNamesLbl.Name = "OperationNamesLbl";
            this.OperationNamesLbl.Size = new System.Drawing.Size(38, 13);
            this.OperationNamesLbl.TabIndex = 0;
            this.OperationNamesLbl.Text = "Name:";
            // 
            // OperationNameFld
            // 
            this.OperationNameFld.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OperationNameFld.Location = new System.Drawing.Point(50, 18);
            this.OperationNameFld.Name = "OperationNameFld";
            this.OperationNameFld.Size = new System.Drawing.Size(234, 20);
            this.OperationNameFld.TabIndex = 1;
            this.OperationNameFld.Leave += new System.EventHandler(this.OperationNameFld_Leave);
            // 
            // OperationTypeFld
            // 
            this.OperationTypeFld.FormattingEnabled = true;
            this.OperationTypeFld.Location = new System.Drawing.Point(352, 18);
            this.OperationTypeFld.Name = "OperationTypeFld";
            this.OperationTypeFld.Size = new System.Drawing.Size(223, 21);
            this.OperationTypeFld.TabIndex = 2;
            this.OperationTypeFld.SelectedIndexChanged += new System.EventHandler(this.OperationTypeFld_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.label1.Location = new System.Drawing.Point(13, 52);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 16);
            this.label1.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(312, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Type:";
            // 
            // OperationGroup
            // 
            this.OperationGroup.Controls.Add(this.UseHeaderParameters);
            this.OperationGroup.Controls.Add(this.OperationNamesLbl);
            this.OperationGroup.Controls.Add(this.label2);
            this.OperationGroup.Controls.Add(this.OperationTypeFld);
            this.OperationGroup.Controls.Add(this.OperationNameFld);
            this.OperationGroup.Controls.Add(this.HasPagination);
            this.OperationGroup.Location = new System.Drawing.Point(12, 4);
            this.OperationGroup.Name = "OperationGroup";
            this.OperationGroup.Size = new System.Drawing.Size(584, 64);
            this.OperationGroup.TabIndex = 1;
            this.OperationGroup.TabStop = false;
            this.OperationGroup.Text = "Operation";
            // 
            // UseHeaderParameters
            // 
            this.UseHeaderParameters.AutoSize = true;
            this.UseHeaderParameters.Location = new System.Drawing.Point(9, 44);
            this.UseHeaderParameters.Name = "UseHeaderParameters";
            this.UseHeaderParameters.Size = new System.Drawing.Size(116, 17);
            this.UseHeaderParameters.TabIndex = 4;
            this.UseHeaderParameters.Text = "Header parameters";
            this.UseHeaderParameters.UseVisualStyleBackColor = true;
            this.UseHeaderParameters.CheckedChanged += new System.EventHandler(this.Indicator_CheckedChanged);
            // 
            // HasPagination
            // 
            this.HasPagination.AutoSize = true;
            this.HasPagination.Location = new System.Drawing.Point(131, 44);
            this.HasPagination.Name = "HasPagination";
            this.HasPagination.Size = new System.Drawing.Size(76, 17);
            this.HasPagination.TabIndex = 6;
            this.HasPagination.Text = "Pagination";
            this.HasPagination.UseVisualStyleBackColor = true;
            this.HasPagination.CheckedChanged += new System.EventHandler(this.Indicator_CheckedChanged);
            // 
            // NewMinorVersion
            // 
            this.NewMinorVersion.AutoSize = true;
            this.NewMinorVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NewMinorVersion.Location = new System.Drawing.Point(12, 566);
            this.NewMinorVersion.Name = "NewMinorVersion";
            this.NewMinorVersion.Size = new System.Drawing.Size(138, 17);
            this.NewMinorVersion.TabIndex = 11;
            this.NewMinorVersion.Text = "Increment minor version";
            this.NewMinorVersion.UseVisualStyleBackColor = true;
            // 
            // FilterGroup
            // 
            this.FilterGroup.Controls.Add(this.EditParameter);
            this.FilterGroup.Controls.Add(this.DeleteFilter);
            this.FilterGroup.Controls.Add(this.FilterParameterList);
            this.FilterGroup.Controls.Add(this.AddFilter);
            this.FilterGroup.Location = new System.Drawing.Point(12, 189);
            this.FilterGroup.Name = "FilterGroup";
            this.FilterGroup.Size = new System.Drawing.Size(284, 180);
            this.FilterGroup.TabIndex = 7;
            this.FilterGroup.TabStop = false;
            this.FilterGroup.Text = "Filter parameters";
            // 
            // EditParameter
            // 
            this.EditParameter.Image = ((System.Drawing.Image)(resources.GetObject("EditParameter.Image")));
            this.EditParameter.Location = new System.Drawing.Point(73, 146);
            this.EditParameter.Name = "EditParameter";
            this.EditParameter.Size = new System.Drawing.Size(25, 25);
            this.EditParameter.TabIndex = 3;
            this.EditParameter.UseVisualStyleBackColor = true;
            this.EditParameter.Click += new System.EventHandler(this.EditFilter_Click);
            // 
            // DeleteFilter
            // 
            this.DeleteFilter.Image = ((System.Drawing.Image)(resources.GetObject("DeleteFilter.Image")));
            this.DeleteFilter.Location = new System.Drawing.Point(42, 146);
            this.DeleteFilter.Name = "DeleteFilter";
            this.DeleteFilter.Size = new System.Drawing.Size(25, 25);
            this.DeleteFilter.TabIndex = 2;
            this.DeleteFilter.UseVisualStyleBackColor = true;
            this.DeleteFilter.Click += new System.EventHandler(this.DeleteFilter_Click);
            // 
            // FilterParameterList
            // 
            this.FilterParameterList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ParamName,
            this.ParamType});
            this.FilterParameterList.FullRowSelect = true;
            this.FilterParameterList.GridLines = true;
            this.FilterParameterList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.FilterParameterList.Location = new System.Drawing.Point(11, 19);
            this.FilterParameterList.MultiSelect = false;
            this.FilterParameterList.Name = "FilterParameterList";
            this.FilterParameterList.Size = new System.Drawing.Size(260, 121);
            this.FilterParameterList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.FilterParameterList.TabIndex = 0;
            this.FilterParameterList.UseCompatibleStateImageBehavior = false;
            this.FilterParameterList.View = System.Windows.Forms.View.Details;
            // 
            // ParamName
            // 
            this.ParamName.Text = "Name";
            this.ParamName.Width = 150;
            // 
            // ParamType
            // 
            this.ParamType.Text = "Type";
            this.ParamType.Width = 105;
            // 
            // AddFilter
            // 
            this.AddFilter.Image = ((System.Drawing.Image)(resources.GetObject("AddFilter.Image")));
            this.AddFilter.Location = new System.Drawing.Point(11, 146);
            this.AddFilter.Name = "AddFilter";
            this.AddFilter.Size = new System.Drawing.Size(25, 25);
            this.AddFilter.TabIndex = 1;
            this.AddFilter.UseVisualStyleBackColor = true;
            this.AddFilter.Click += new System.EventHandler(this.AddFilter_Click);
            // 
            // ResponseCodeGroup
            // 
            this.ResponseCodeGroup.Controls.Add(this.UseCollection);
            this.ResponseCodeGroup.Controls.Add(this.EditCollections);
            this.ResponseCodeGroup.Controls.Add(this.EditResponseCode);
            this.ResponseCodeGroup.Controls.Add(this.DeleteResponseCode);
            this.ResponseCodeGroup.Controls.Add(this.AddResponseCode);
            this.ResponseCodeGroup.Controls.Add(this.ResponseCodeList);
            this.ResponseCodeGroup.Location = new System.Drawing.Point(314, 189);
            this.ResponseCodeGroup.Name = "ResponseCodeGroup";
            this.ResponseCodeGroup.Size = new System.Drawing.Size(284, 180);
            this.ResponseCodeGroup.TabIndex = 8;
            this.ResponseCodeGroup.TabStop = false;
            this.ResponseCodeGroup.Text = "Response codes";
            // 
            // UseCollection
            // 
            this.UseCollection.Image = ((System.Drawing.Image)(resources.GetObject("UseCollection.Image")));
            this.UseCollection.Location = new System.Drawing.Point(105, 146);
            this.UseCollection.Name = "UseCollection";
            this.UseCollection.Size = new System.Drawing.Size(25, 25);
            this.UseCollection.TabIndex = 5;
            this.UseCollection.UseVisualStyleBackColor = true;
            this.UseCollection.Click += new System.EventHandler(this.UseCollection_Click);
            // 
            // EditCollections
            // 
            this.EditCollections.Image = ((System.Drawing.Image)(resources.GetObject("EditCollections.Image")));
            this.EditCollections.Location = new System.Drawing.Point(136, 146);
            this.EditCollections.Name = "EditCollections";
            this.EditCollections.Size = new System.Drawing.Size(25, 25);
            this.EditCollections.TabIndex = 4;
            this.EditCollections.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.EditCollections.UseVisualStyleBackColor = true;
            this.EditCollections.Click += new System.EventHandler(this.EditCollections_Click);
            // 
            // EditResponseCode
            // 
            this.EditResponseCode.Image = ((System.Drawing.Image)(resources.GetObject("EditResponseCode.Image")));
            this.EditResponseCode.Location = new System.Drawing.Point(74, 146);
            this.EditResponseCode.Name = "EditResponseCode";
            this.EditResponseCode.Size = new System.Drawing.Size(25, 25);
            this.EditResponseCode.TabIndex = 3;
            this.EditResponseCode.UseVisualStyleBackColor = true;
            this.EditResponseCode.Click += new System.EventHandler(this.EditResponseCode_Click);
            // 
            // DeleteResponseCode
            // 
            this.DeleteResponseCode.Image = ((System.Drawing.Image)(resources.GetObject("DeleteResponseCode.Image")));
            this.DeleteResponseCode.Location = new System.Drawing.Point(43, 146);
            this.DeleteResponseCode.Name = "DeleteResponseCode";
            this.DeleteResponseCode.Size = new System.Drawing.Size(25, 25);
            this.DeleteResponseCode.TabIndex = 2;
            this.DeleteResponseCode.UseVisualStyleBackColor = true;
            this.DeleteResponseCode.Click += new System.EventHandler(this.DeleteResponseCode_Click);
            // 
            // AddResponseCode
            // 
            this.AddResponseCode.Image = ((System.Drawing.Image)(resources.GetObject("AddResponseCode.Image")));
            this.AddResponseCode.Location = new System.Drawing.Point(12, 146);
            this.AddResponseCode.Name = "AddResponseCode";
            this.AddResponseCode.Size = new System.Drawing.Size(25, 25);
            this.AddResponseCode.TabIndex = 1;
            this.AddResponseCode.UseVisualStyleBackColor = true;
            this.AddResponseCode.Click += new System.EventHandler(this.AddResponseCode_Click);
            // 
            // ResponseCodeList
            // 
            this.ResponseCodeList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Code,
            this.ResponseDesc});
            this.ResponseCodeList.FullRowSelect = true;
            this.ResponseCodeList.GridLines = true;
            this.ResponseCodeList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.ResponseCodeList.Location = new System.Drawing.Point(12, 19);
            this.ResponseCodeList.MultiSelect = false;
            this.ResponseCodeList.Name = "ResponseCodeList";
            this.ResponseCodeList.Size = new System.Drawing.Size(261, 121);
            this.ResponseCodeList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.ResponseCodeList.TabIndex = 0;
            this.ResponseCodeList.UseCompatibleStateImageBehavior = false;
            this.ResponseCodeList.View = System.Windows.Forms.View.Details;
            // 
            // Code
            // 
            this.Code.Text = "Code";
            this.Code.Width = 58;
            // 
            // ResponseDesc
            // 
            this.ResponseDesc.Text = "Description";
            this.ResponseDesc.Width = 197;
            // 
            // FilterParametersMenuStrip
            // 
            this.FilterParametersMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem1,
            this.deleteToolStripMenuItem,
            this.editToolStripMenuItem});
            this.FilterParametersMenuStrip.Name = "FilterParametersMenuStrip";
            this.FilterParametersMenuStrip.Size = new System.Drawing.Size(108, 70);
            this.FilterParametersMenuStrip.Click += new System.EventHandler(this.AddFilter_Click);
            // 
            // addToolStripMenuItem1
            // 
            this.addToolStripMenuItem1.Name = "addToolStripMenuItem1";
            this.addToolStripMenuItem1.Size = new System.Drawing.Size(107, 22);
            this.addToolStripMenuItem1.Text = "Add";
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.DeleteFilter_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.editToolStripMenuItem.Text = "Edit";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.EditFilter_Click);
            // 
            // ResponseCodeMenuStrip
            // 
            this.ResponseCodeMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem,
            this.deleteToolStripMenuItem1,
            this.editToolStripMenuItem1,
            this.useCollectionToolStripMenuItem,
            this.manageCollectionsToolStripMenuItem});
            this.ResponseCodeMenuStrip.Name = "ResponseCodeMenuStrip";
            this.ResponseCodeMenuStrip.Size = new System.Drawing.Size(180, 114);
            this.ResponseCodeMenuStrip.Click += new System.EventHandler(this.EditCollections_Click);
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.addToolStripMenuItem.Text = "Add";
            this.addToolStripMenuItem.Click += new System.EventHandler(this.AddResponseCode_Click);
            // 
            // deleteToolStripMenuItem1
            // 
            this.deleteToolStripMenuItem1.Name = "deleteToolStripMenuItem1";
            this.deleteToolStripMenuItem1.Size = new System.Drawing.Size(179, 22);
            this.deleteToolStripMenuItem1.Text = "Delete";
            this.deleteToolStripMenuItem1.Click += new System.EventHandler(this.DeleteResponseCode_Click);
            // 
            // editToolStripMenuItem1
            // 
            this.editToolStripMenuItem1.Name = "editToolStripMenuItem1";
            this.editToolStripMenuItem1.Size = new System.Drawing.Size(179, 22);
            this.editToolStripMenuItem1.Text = "Edit";
            this.editToolStripMenuItem1.Click += new System.EventHandler(this.EditResponseCode_Click);
            // 
            // useCollectionToolStripMenuItem
            // 
            this.useCollectionToolStripMenuItem.Name = "useCollectionToolStripMenuItem";
            this.useCollectionToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.useCollectionToolStripMenuItem.Text = "Use Collection";
            this.useCollectionToolStripMenuItem.Click += new System.EventHandler(this.UseCollection_Click);
            // 
            // manageCollectionsToolStripMenuItem
            // 
            this.manageCollectionsToolStripMenuItem.Name = "manageCollectionsToolStripMenuItem";
            this.manageCollectionsToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.manageCollectionsToolStripMenuItem.Text = "Manage Collections";
            // 
            // MimeBox
            // 
            this.MimeBox.Controls.Add(this.ConsumesMIME);
            this.MimeBox.Controls.Add(this.label4);
            this.MimeBox.Controls.Add(this.ProducesMIME);
            this.MimeBox.Controls.Add(this.label3);
            this.MimeBox.Location = new System.Drawing.Point(12, 375);
            this.MimeBox.Name = "MimeBox";
            this.MimeBox.Size = new System.Drawing.Size(586, 49);
            this.MimeBox.TabIndex = 9;
            this.MimeBox.TabStop = false;
            this.MimeBox.Text = "MIME types";
            // 
            // ConsumesMIME
            // 
            this.ConsumesMIME.Location = new System.Drawing.Point(358, 16);
            this.ConsumesMIME.Name = "ConsumesMIME";
            this.ConsumesMIME.Size = new System.Drawing.Size(217, 20);
            this.ConsumesMIME.TabIndex = 3;
            this.ConsumesMIME.TextChanged += new System.EventHandler(this.ConsumesMIME_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(293, 19);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Consumes:";
            // 
            // ProducesMIME
            // 
            this.ProducesMIME.Location = new System.Drawing.Point(63, 16);
            this.ProducesMIME.Name = "ProducesMIME";
            this.ProducesMIME.Size = new System.Drawing.Size(213, 20);
            this.ProducesMIME.TabIndex = 1;
            this.ProducesMIME.TextChanged += new System.EventHandler(this.ProducesMIME_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 19);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Produces:";
            // 
            // DocumentationBox
            // 
            this.DocumentationBox.Controls.Add(this.Description);
            this.DocumentationBox.Controls.Add(this.SummaryText);
            this.DocumentationBox.Controls.Add(this.label5);
            this.DocumentationBox.Location = new System.Drawing.Point(12, 430);
            this.DocumentationBox.Name = "DocumentationBox";
            this.DocumentationBox.Size = new System.Drawing.Size(586, 122);
            this.DocumentationBox.TabIndex = 10;
            this.DocumentationBox.TabStop = false;
            this.DocumentationBox.Text = "Documentation";
            // 
            // Description
            // 
            this.Description.Location = new System.Drawing.Point(12, 47);
            this.Description.Multiline = true;
            this.Description.Name = "Description";
            this.Description.Size = new System.Drawing.Size(563, 59);
            this.Description.TabIndex = 2;
            this.Description.Leave += new System.EventHandler(this.Description_Leave);
            // 
            // SummaryText
            // 
            this.SummaryText.Location = new System.Drawing.Point(63, 16);
            this.SummaryText.Name = "SummaryText";
            this.SummaryText.Size = new System.Drawing.Size(512, 20);
            this.SummaryText.TabIndex = 1;
            this.SummaryText.Leave += new System.EventHandler(this.SummaryText_Leave);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 19);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Summary:";
            // 
            // RequestParamBox
            // 
            this.RequestParamBox.Controls.Add(this.ReqCardinalityGroup);
            this.RequestParamBox.Controls.Add(this.ReqSecurityGroup);
            this.RequestParamBox.Controls.Add(this.RemoveRequest);
            this.RequestParamBox.Controls.Add(this.RequestTypeName);
            this.RequestParamBox.Controls.Add(this.SelectRequest);
            this.RequestParamBox.Location = new System.Drawing.Point(12, 74);
            this.RequestParamBox.Name = "RequestParamBox";
            this.RequestParamBox.Size = new System.Drawing.Size(284, 107);
            this.RequestParamBox.TabIndex = 2;
            this.RequestParamBox.TabStop = false;
            this.RequestParamBox.Text = "Request";
            // 
            // ReqCardinalityGroup
            // 
            this.ReqCardinalityGroup.Controls.Add(this.label6);
            this.ReqCardinalityGroup.Controls.Add(this.ReqCardHi);
            this.ReqCardinalityGroup.Controls.Add(this.ReqCardLo);
            this.ReqCardinalityGroup.Location = new System.Drawing.Point(176, 47);
            this.ReqCardinalityGroup.Name = "ReqCardinalityGroup";
            this.ReqCardinalityGroup.Size = new System.Drawing.Size(102, 47);
            this.ReqCardinalityGroup.TabIndex = 6;
            this.ReqCardinalityGroup.TabStop = false;
            this.ReqCardinalityGroup.Text = "Cardinality";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(42, 20);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(16, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "...";
            // 
            // ReqCardHi
            // 
            this.ReqCardHi.Location = new System.Drawing.Point(64, 19);
            this.ReqCardHi.Name = "ReqCardHi";
            this.ReqCardHi.Size = new System.Drawing.Size(30, 20);
            this.ReqCardHi.TabIndex = 1;
            this.ReqCardHi.Leave += new System.EventHandler(this.ReqCardinality_Leave);
            // 
            // ReqCardLo
            // 
            this.ReqCardLo.Location = new System.Drawing.Point(6, 19);
            this.ReqCardLo.Name = "ReqCardLo";
            this.ReqCardLo.Size = new System.Drawing.Size(30, 20);
            this.ReqCardLo.TabIndex = 0;
            this.ReqCardLo.Leave += new System.EventHandler(this.ReqCardinality_Leave);
            // 
            // ReqSecurityGroup
            // 
            this.ReqSecurityGroup.Controls.Add(this.ReqEncryption);
            this.ReqSecurityGroup.Controls.Add(this.ReqSigning);
            this.ReqSecurityGroup.Location = new System.Drawing.Point(8, 47);
            this.ReqSecurityGroup.Name = "ReqSecurityGroup";
            this.ReqSecurityGroup.Size = new System.Drawing.Size(161, 47);
            this.ReqSecurityGroup.TabIndex = 5;
            this.ReqSecurityGroup.TabStop = false;
            this.ReqSecurityGroup.Text = "Security";
            // 
            // ReqEncryption
            // 
            this.ReqEncryption.AutoSize = true;
            this.ReqEncryption.Location = new System.Drawing.Point(73, 19);
            this.ReqEncryption.Name = "ReqEncryption";
            this.ReqEncryption.Size = new System.Drawing.Size(76, 17);
            this.ReqEncryption.TabIndex = 6;
            this.ReqEncryption.Text = "Encryption";
            this.ReqEncryption.UseVisualStyleBackColor = true;
            this.ReqEncryption.CheckedChanged += new System.EventHandler(this.Indicator_CheckedChanged);
            // 
            // ReqSigning
            // 
            this.ReqSigning.AutoSize = true;
            this.ReqSigning.Location = new System.Drawing.Point(6, 19);
            this.ReqSigning.Name = "ReqSigning";
            this.ReqSigning.Size = new System.Drawing.Size(61, 17);
            this.ReqSigning.TabIndex = 5;
            this.ReqSigning.Text = "Signing";
            this.ReqSigning.UseVisualStyleBackColor = true;
            this.ReqSigning.CheckedChanged += new System.EventHandler(this.Indicator_CheckedChanged);
            // 
            // RemoveRequest
            // 
            this.RemoveRequest.Image = ((System.Drawing.Image)(resources.GetObject("RemoveRequest.Image")));
            this.RemoveRequest.Location = new System.Drawing.Point(39, 19);
            this.RemoveRequest.Name = "RemoveRequest";
            this.RemoveRequest.Size = new System.Drawing.Size(25, 25);
            this.RemoveRequest.TabIndex = 2;
            this.RemoveRequest.UseVisualStyleBackColor = true;
            this.RemoveRequest.Click += new System.EventHandler(this.RemoveRequest_Click);
            // 
            // RequestTypeName
            // 
            this.RequestTypeName.Location = new System.Drawing.Point(70, 21);
            this.RequestTypeName.Name = "RequestTypeName";
            this.RequestTypeName.ReadOnly = true;
            this.RequestTypeName.Size = new System.Drawing.Size(201, 20);
            this.RequestTypeName.TabIndex = 0;
            // 
            // SelectRequest
            // 
            this.SelectRequest.Image = ((System.Drawing.Image)(resources.GetObject("SelectRequest.Image")));
            this.SelectRequest.Location = new System.Drawing.Point(8, 19);
            this.SelectRequest.Name = "SelectRequest";
            this.SelectRequest.Size = new System.Drawing.Size(25, 25);
            this.SelectRequest.TabIndex = 1;
            this.SelectRequest.UseVisualStyleBackColor = true;
            this.SelectRequest.Click += new System.EventHandler(this.SelectRequest_Click);
            // 
            // ResponseParamBox
            // 
            this.ResponseParamBox.Controls.Add(this.RspCardinalityGroup);
            this.ResponseParamBox.Controls.Add(this.RspSecurityGroup);
            this.ResponseParamBox.Controls.Add(this.RemoveResponse);
            this.ResponseParamBox.Controls.Add(this.SelectResponse);
            this.ResponseParamBox.Controls.Add(this.ResponseTypeName);
            this.ResponseParamBox.Location = new System.Drawing.Point(314, 74);
            this.ResponseParamBox.Name = "ResponseParamBox";
            this.ResponseParamBox.Size = new System.Drawing.Size(284, 107);
            this.ResponseParamBox.TabIndex = 3;
            this.ResponseParamBox.TabStop = false;
            this.ResponseParamBox.Text = "Response";
            // 
            // RspCardinalityGroup
            // 
            this.RspCardinalityGroup.Controls.Add(this.label7);
            this.RspCardinalityGroup.Controls.Add(this.RspCardHi);
            this.RspCardinalityGroup.Controls.Add(this.RspCardLo);
            this.RspCardinalityGroup.Location = new System.Drawing.Point(175, 47);
            this.RspCardinalityGroup.Name = "RspCardinalityGroup";
            this.RspCardinalityGroup.Size = new System.Drawing.Size(102, 47);
            this.RspCardinalityGroup.TabIndex = 6;
            this.RspCardinalityGroup.TabStop = false;
            this.RspCardinalityGroup.Text = "Cardinality";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(44, 20);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(16, 13);
            this.label7.TabIndex = 2;
            this.label7.Text = "...";
            // 
            // RspCardHi
            // 
            this.RspCardHi.Location = new System.Drawing.Point(66, 17);
            this.RspCardHi.Name = "RspCardHi";
            this.RspCardHi.Size = new System.Drawing.Size(30, 20);
            this.RspCardHi.TabIndex = 1;
            this.RspCardHi.Leave += new System.EventHandler(this.RspCardinality_Leave);
            // 
            // RspCardLo
            // 
            this.RspCardLo.Location = new System.Drawing.Point(6, 17);
            this.RspCardLo.Name = "RspCardLo";
            this.RspCardLo.Size = new System.Drawing.Size(30, 20);
            this.RspCardLo.TabIndex = 0;
            this.RspCardLo.Leave += new System.EventHandler(this.RspCardinality_Leave);
            // 
            // RspSecurityGroup
            // 
            this.RspSecurityGroup.Controls.Add(this.RspEncryption);
            this.RspSecurityGroup.Controls.Add(this.RspSigning);
            this.RspSecurityGroup.Location = new System.Drawing.Point(8, 47);
            this.RspSecurityGroup.Name = "RspSecurityGroup";
            this.RspSecurityGroup.Size = new System.Drawing.Size(161, 47);
            this.RspSecurityGroup.TabIndex = 5;
            this.RspSecurityGroup.TabStop = false;
            this.RspSecurityGroup.Text = "Security";
            // 
            // RspEncryption
            // 
            this.RspEncryption.AutoSize = true;
            this.RspEncryption.Location = new System.Drawing.Point(73, 19);
            this.RspEncryption.Name = "RspEncryption";
            this.RspEncryption.Size = new System.Drawing.Size(76, 17);
            this.RspEncryption.TabIndex = 1;
            this.RspEncryption.Text = "Encryption";
            this.RspEncryption.UseVisualStyleBackColor = true;
            this.RspEncryption.CheckedChanged += new System.EventHandler(this.Indicator_CheckedChanged);
            // 
            // RspSigning
            // 
            this.RspSigning.AutoSize = true;
            this.RspSigning.Location = new System.Drawing.Point(6, 19);
            this.RspSigning.Name = "RspSigning";
            this.RspSigning.Size = new System.Drawing.Size(61, 17);
            this.RspSigning.TabIndex = 0;
            this.RspSigning.Text = "Signing";
            this.RspSigning.UseVisualStyleBackColor = true;
            this.RspSigning.CheckedChanged += new System.EventHandler(this.Indicator_CheckedChanged);
            // 
            // RemoveResponse
            // 
            this.RemoveResponse.Image = ((System.Drawing.Image)(resources.GetObject("RemoveResponse.Image")));
            this.RemoveResponse.Location = new System.Drawing.Point(39, 19);
            this.RemoveResponse.Name = "RemoveResponse";
            this.RemoveResponse.Size = new System.Drawing.Size(25, 25);
            this.RemoveResponse.TabIndex = 2;
            this.RemoveResponse.UseVisualStyleBackColor = true;
            this.RemoveResponse.Click += new System.EventHandler(this.RemoveResponse_Click);
            // 
            // SelectResponse
            // 
            this.SelectResponse.Image = ((System.Drawing.Image)(resources.GetObject("SelectResponse.Image")));
            this.SelectResponse.Location = new System.Drawing.Point(8, 19);
            this.SelectResponse.Name = "SelectResponse";
            this.SelectResponse.Size = new System.Drawing.Size(25, 25);
            this.SelectResponse.TabIndex = 1;
            this.SelectResponse.UseVisualStyleBackColor = true;
            this.SelectResponse.Click += new System.EventHandler(this.SelectResponse_Click);
            // 
            // ResponseTypeName
            // 
            this.ResponseTypeName.Location = new System.Drawing.Point(70, 20);
            this.ResponseTypeName.Name = "ResponseTypeName";
            this.ResponseTypeName.ReadOnly = true;
            this.ResponseTypeName.Size = new System.Drawing.Size(203, 20);
            this.ResponseTypeName.TabIndex = 0;
            // 
            // RESTOperationDialog
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(607, 601);
            this.Controls.Add(this.ResponseParamBox);
            this.Controls.Add(this.RequestParamBox);
            this.Controls.Add(this.DocumentationBox);
            this.Controls.Add(this.MimeBox);
            this.Controls.Add(this.ResponseCodeGroup);
            this.Controls.Add(this.FilterGroup);
            this.Controls.Add(this.NewMinorVersion);
            this.Controls.Add(this.OperationGroup);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.Cancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RESTOperationDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add / Edit Operation";
            this.OperationGroup.ResumeLayout(false);
            this.OperationGroup.PerformLayout();
            this.FilterGroup.ResumeLayout(false);
            this.ResponseCodeGroup.ResumeLayout(false);
            this.FilterParametersMenuStrip.ResumeLayout(false);
            this.ResponseCodeMenuStrip.ResumeLayout(false);
            this.MimeBox.ResumeLayout(false);
            this.MimeBox.PerformLayout();
            this.DocumentationBox.ResumeLayout(false);
            this.DocumentationBox.PerformLayout();
            this.RequestParamBox.ResumeLayout(false);
            this.RequestParamBox.PerformLayout();
            this.ReqCardinalityGroup.ResumeLayout(false);
            this.ReqCardinalityGroup.PerformLayout();
            this.ReqSecurityGroup.ResumeLayout(false);
            this.ReqSecurityGroup.PerformLayout();
            this.ResponseParamBox.ResumeLayout(false);
            this.ResponseParamBox.PerformLayout();
            this.RspCardinalityGroup.ResumeLayout(false);
            this.RspCardinalityGroup.PerformLayout();
            this.RspSecurityGroup.ResumeLayout(false);
            this.RspSecurityGroup.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.Label OperationNamesLbl;
        private System.Windows.Forms.TextBox OperationNameFld;
        private ComboBox OperationTypeFld;
        private Label label1;
        private Label label2;
        private GroupBox OperationGroup;
        private CheckBox NewMinorVersion;
        private GroupBox FilterGroup;
        private Button EditParameter;
        private Button DeleteFilter;
        private ListView FilterParameterList;
        private ColumnHeader ParamName;
        private ColumnHeader ParamType;
        private Button AddFilter;
        private CheckBox HasPagination;
        private GroupBox ResponseCodeGroup;
        private Button EditResponseCode;
        private Button DeleteResponseCode;
        private Button AddResponseCode;
        private ListView ResponseCodeList;
        private ColumnHeader Code;
        private ColumnHeader ResponseDesc;
        private ContextMenuStrip FilterParametersMenuStrip;
        private ToolStripMenuItem deleteToolStripMenuItem;
        private ToolStripMenuItem editToolStripMenuItem;
        private ContextMenuStrip ResponseCodeMenuStrip;
        private ToolStripMenuItem deleteToolStripMenuItem1;
        private ToolStripMenuItem editToolStripMenuItem1;
        private GroupBox MimeBox;
        private TextBox ConsumesMIME;
        private Label label4;
        private TextBox ProducesMIME;
        private Label label3;
        private GroupBox DocumentationBox;
        private TextBox Description;
        private TextBox SummaryText;
        private Label label5;
        private GroupBox RequestParamBox;
        private GroupBox ResponseParamBox;
        private TextBox RequestTypeName;
        private Button SelectRequest;
        private Button SelectResponse;
        private TextBox ResponseTypeName;
        private Button RemoveRequest;
        private Button RemoveResponse;
        private CheckBox UseHeaderParameters;
        private Button UseCollection;
        private Button EditCollections;
        private ToolStripMenuItem addToolStripMenuItem;
        private ToolStripMenuItem useCollectionToolStripMenuItem;
        private ToolStripMenuItem addToolStripMenuItem1;
        private ToolStripMenuItem manageCollectionsToolStripMenuItem;
        private GroupBox ReqCardinalityGroup;
        private GroupBox ReqSecurityGroup;
        private CheckBox ReqEncryption;
        private CheckBox ReqSigning;
        private GroupBox RspCardinalityGroup;
        private GroupBox RspSecurityGroup;
        private Label label6;
        private TextBox ReqCardHi;
        private TextBox ReqCardLo;
        private Label label7;
        private TextBox RspCardHi;
        private TextBox RspCardLo;
        private CheckBox RspEncryption;
        private CheckBox RspSigning;
    }
}