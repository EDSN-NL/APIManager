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
            this.RespCode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.RespDescription = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
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
            this.RemoveRequest = new System.Windows.Forms.Button();
            this.RequestTypeName = new System.Windows.Forms.TextBox();
            this.SelectRequest = new System.Windows.Forms.Button();
            this.RequestHeaderGroup = new System.Windows.Forms.GroupBox();
            this.UseReqHeaderCollection = new System.Windows.Forms.Button();
            this.EditReqHeaderCollections = new System.Windows.Forms.Button();
            this.EditReqHeader = new System.Windows.Forms.Button();
            this.DeleteReqHeader = new System.Windows.Forms.Button();
            this.AddReqHeader = new System.Windows.Forms.Button();
            this.RequestHeaderList = new System.Windows.Forms.ListView();
            this.ReqHdrName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ReqHdrDescription = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.RequestHeaderMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.useCollectionToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.manageCollectionsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.OperationGroup.SuspendLayout();
            this.FilterGroup.SuspendLayout();
            this.ResponseCodeGroup.SuspendLayout();
            this.FilterParametersMenuStrip.SuspendLayout();
            this.ResponseCodeMenuStrip.SuspendLayout();
            this.MimeBox.SuspendLayout();
            this.DocumentationBox.SuspendLayout();
            this.RequestParamBox.SuspendLayout();
            this.ReqCardinalityGroup.SuspendLayout();
            this.RequestHeaderGroup.SuspendLayout();
            this.RequestHeaderMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(523, 567);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 11;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(442, 567);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 31);
            this.Ok.TabIndex = 12;
            this.Ok.Text = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
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
            this.OperationNameFld.Size = new System.Drawing.Size(235, 20);
            this.OperationNameFld.TabIndex = 1;
            this.OperationNameFld.Leave += new System.EventHandler(this.OperationNameFld_Leave);
            // 
            // OperationTypeFld
            // 
            this.OperationTypeFld.FormattingEnabled = true;
            this.OperationTypeFld.Location = new System.Drawing.Point(351, 17);
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
            this.label2.Location = new System.Drawing.Point(311, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Type:";
            // 
            // OperationGroup
            // 
            this.OperationGroup.Controls.Add(this.OperationNamesLbl);
            this.OperationGroup.Controls.Add(this.label2);
            this.OperationGroup.Controls.Add(this.OperationTypeFld);
            this.OperationGroup.Controls.Add(this.OperationNameFld);
            this.OperationGroup.Location = new System.Drawing.Point(12, 12);
            this.OperationGroup.Name = "OperationGroup";
            this.OperationGroup.Size = new System.Drawing.Size(586, 49);
            this.OperationGroup.TabIndex = 1;
            this.OperationGroup.TabStop = false;
            this.OperationGroup.Text = "Operation";
            // 
            // HasPagination
            // 
            this.HasPagination.AutoSize = true;
            this.HasPagination.Location = new System.Drawing.Point(12, 408);
            this.HasPagination.Name = "HasPagination";
            this.HasPagination.Size = new System.Drawing.Size(97, 17);
            this.HasPagination.TabIndex = 7;
            this.HasPagination.Text = "Use pagination";
            this.HasPagination.UseVisualStyleBackColor = true;
            this.HasPagination.CheckedChanged += new System.EventHandler(this.Indicator_CheckedChanged);
            // 
            // NewMinorVersion
            // 
            this.NewMinorVersion.AutoSize = true;
            this.NewMinorVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NewMinorVersion.Location = new System.Drawing.Point(12, 567);
            this.NewMinorVersion.Name = "NewMinorVersion";
            this.NewMinorVersion.Size = new System.Drawing.Size(138, 17);
            this.NewMinorVersion.TabIndex = 10;
            this.NewMinorVersion.Text = "Increment minor version";
            this.NewMinorVersion.UseVisualStyleBackColor = true;
            // 
            // FilterGroup
            // 
            this.FilterGroup.Controls.Add(this.EditParameter);
            this.FilterGroup.Controls.Add(this.DeleteFilter);
            this.FilterGroup.Controls.Add(this.FilterParameterList);
            this.FilterGroup.Controls.Add(this.AddFilter);
            this.FilterGroup.Location = new System.Drawing.Point(12, 67);
            this.FilterGroup.Name = "FilterGroup";
            this.FilterGroup.Size = new System.Drawing.Size(296, 180);
            this.FilterGroup.TabIndex = 2;
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
            this.FilterParameterList.HideSelection = false;
            this.FilterParameterList.Location = new System.Drawing.Point(11, 19);
            this.FilterParameterList.MultiSelect = false;
            this.FilterParameterList.Name = "FilterParameterList";
            this.FilterParameterList.Size = new System.Drawing.Size(274, 121);
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
            this.ResponseCodeGroup.Location = new System.Drawing.Point(314, 67);
            this.ResponseCodeGroup.Name = "ResponseCodeGroup";
            this.ResponseCodeGroup.Size = new System.Drawing.Size(284, 180);
            this.ResponseCodeGroup.TabIndex = 3;
            this.ResponseCodeGroup.TabStop = false;
            this.ResponseCodeGroup.Text = "Response codes";
            // 
            // UseCollection
            // 
            this.UseCollection.Image = ((System.Drawing.Image)(resources.GetObject("UseCollection.Image")));
            this.UseCollection.Location = new System.Drawing.Point(105, 146);
            this.UseCollection.Name = "UseCollection";
            this.UseCollection.Size = new System.Drawing.Size(25, 25);
            this.UseCollection.TabIndex = 4;
            this.UseCollection.UseVisualStyleBackColor = true;
            this.UseCollection.Click += new System.EventHandler(this.UseCollection_Click);
            // 
            // EditCollections
            // 
            this.EditCollections.Image = ((System.Drawing.Image)(resources.GetObject("EditCollections.Image")));
            this.EditCollections.Location = new System.Drawing.Point(136, 146);
            this.EditCollections.Name = "EditCollections";
            this.EditCollections.Size = new System.Drawing.Size(25, 25);
            this.EditCollections.TabIndex = 5;
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
            this.RespCode,
            this.RespDescription});
            this.ResponseCodeList.FullRowSelect = true;
            this.ResponseCodeList.GridLines = true;
            this.ResponseCodeList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.ResponseCodeList.HideSelection = false;
            this.ResponseCodeList.Location = new System.Drawing.Point(12, 19);
            this.ResponseCodeList.MultiSelect = false;
            this.ResponseCodeList.Name = "ResponseCodeList";
            this.ResponseCodeList.Size = new System.Drawing.Size(261, 121);
            this.ResponseCodeList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.ResponseCodeList.TabIndex = 0;
            this.ResponseCodeList.UseCompatibleStateImageBehavior = false;
            this.ResponseCodeList.View = System.Windows.Forms.View.Details;
            // 
            // RespCode
            // 
            this.RespCode.Text = "Code";
            this.RespCode.Width = 58;
            // 
            // RespDescription
            // 
            this.RespDescription.Text = "Description";
            this.RespDescription.Width = 197;
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
            this.manageCollectionsToolStripMenuItem.Click += new System.EventHandler(this.EditCollections_Click);
            // 
            // MimeBox
            // 
            this.MimeBox.Controls.Add(this.ConsumesMIME);
            this.MimeBox.Controls.Add(this.label4);
            this.MimeBox.Controls.Add(this.ProducesMIME);
            this.MimeBox.Controls.Add(this.label3);
            this.MimeBox.Location = new System.Drawing.Point(12, 327);
            this.MimeBox.Name = "MimeBox";
            this.MimeBox.Size = new System.Drawing.Size(296, 75);
            this.MimeBox.TabIndex = 6;
            this.MimeBox.TabStop = false;
            this.MimeBox.Text = "MIME types";
            // 
            // ConsumesMIME
            // 
            this.ConsumesMIME.Location = new System.Drawing.Point(63, 42);
            this.ConsumesMIME.Name = "ConsumesMIME";
            this.ConsumesMIME.Size = new System.Drawing.Size(222, 20);
            this.ConsumesMIME.TabIndex = 2;
            this.ConsumesMIME.TextChanged += new System.EventHandler(this.ConsumesMIME_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 45);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Consumes:";
            // 
            // ProducesMIME
            // 
            this.ProducesMIME.Location = new System.Drawing.Point(63, 16);
            this.ProducesMIME.Name = "ProducesMIME";
            this.ProducesMIME.Size = new System.Drawing.Size(222, 20);
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
            this.DocumentationBox.Location = new System.Drawing.Point(12, 439);
            this.DocumentationBox.Name = "DocumentationBox";
            this.DocumentationBox.Size = new System.Drawing.Size(586, 122);
            this.DocumentationBox.TabIndex = 9;
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
            this.RequestParamBox.Controls.Add(this.RemoveRequest);
            this.RequestParamBox.Controls.Add(this.RequestTypeName);
            this.RequestParamBox.Controls.Add(this.SelectRequest);
            this.RequestParamBox.Location = new System.Drawing.Point(12, 253);
            this.RequestParamBox.Name = "RequestParamBox";
            this.RequestParamBox.Size = new System.Drawing.Size(296, 68);
            this.RequestParamBox.TabIndex = 4;
            this.RequestParamBox.TabStop = false;
            this.RequestParamBox.Text = "Request payload";
            // 
            // ReqCardinalityGroup
            // 
            this.ReqCardinalityGroup.Controls.Add(this.label6);
            this.ReqCardinalityGroup.Controls.Add(this.ReqCardHi);
            this.ReqCardinalityGroup.Controls.Add(this.ReqCardLo);
            this.ReqCardinalityGroup.Location = new System.Drawing.Point(215, 11);
            this.ReqCardinalityGroup.Name = "ReqCardinalityGroup";
            this.ReqCardinalityGroup.Size = new System.Drawing.Size(70, 47);
            this.ReqCardinalityGroup.TabIndex = 3;
            this.ReqCardinalityGroup.TabStop = false;
            this.ReqCardinalityGroup.Text = "Cardinality";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(25, 13);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(21, 20);
            this.label6.TabIndex = 0;
            this.label6.Text = "...";
            // 
            // ReqCardHi
            // 
            this.ReqCardHi.Location = new System.Drawing.Point(45, 19);
            this.ReqCardHi.Name = "ReqCardHi";
            this.ReqCardHi.Size = new System.Drawing.Size(20, 20);
            this.ReqCardHi.TabIndex = 2;
            this.ReqCardHi.Leave += new System.EventHandler(this.ReqCardinality_Leave);
            // 
            // ReqCardLo
            // 
            this.ReqCardLo.Location = new System.Drawing.Point(6, 19);
            this.ReqCardLo.Name = "ReqCardLo";
            this.ReqCardLo.Size = new System.Drawing.Size(20, 20);
            this.ReqCardLo.TabIndex = 1;
            this.ReqCardLo.Leave += new System.EventHandler(this.ReqCardinality_Leave);
            // 
            // RemoveRequest
            // 
            this.RemoveRequest.Image = ((System.Drawing.Image)(resources.GetObject("RemoveRequest.Image")));
            this.RemoveRequest.Location = new System.Drawing.Point(42, 26);
            this.RemoveRequest.Name = "RemoveRequest";
            this.RemoveRequest.Size = new System.Drawing.Size(25, 25);
            this.RemoveRequest.TabIndex = 2;
            this.RemoveRequest.UseVisualStyleBackColor = true;
            this.RemoveRequest.Click += new System.EventHandler(this.RemoveRequest_Click);
            // 
            // RequestTypeName
            // 
            this.RequestTypeName.Location = new System.Drawing.Point(73, 30);
            this.RequestTypeName.Name = "RequestTypeName";
            this.RequestTypeName.ReadOnly = true;
            this.RequestTypeName.Size = new System.Drawing.Size(141, 20);
            this.RequestTypeName.TabIndex = 0;
            // 
            // SelectRequest
            // 
            this.SelectRequest.Image = ((System.Drawing.Image)(resources.GetObject("SelectRequest.Image")));
            this.SelectRequest.Location = new System.Drawing.Point(11, 26);
            this.SelectRequest.Name = "SelectRequest";
            this.SelectRequest.Size = new System.Drawing.Size(25, 25);
            this.SelectRequest.TabIndex = 1;
            this.SelectRequest.UseVisualStyleBackColor = true;
            this.SelectRequest.Click += new System.EventHandler(this.SelectRequest_Click);
            // 
            // RequestHeaderGroup
            // 
            this.RequestHeaderGroup.Controls.Add(this.UseReqHeaderCollection);
            this.RequestHeaderGroup.Controls.Add(this.EditReqHeaderCollections);
            this.RequestHeaderGroup.Controls.Add(this.EditReqHeader);
            this.RequestHeaderGroup.Controls.Add(this.DeleteReqHeader);
            this.RequestHeaderGroup.Controls.Add(this.AddReqHeader);
            this.RequestHeaderGroup.Controls.Add(this.RequestHeaderList);
            this.RequestHeaderGroup.Location = new System.Drawing.Point(314, 253);
            this.RequestHeaderGroup.Name = "RequestHeaderGroup";
            this.RequestHeaderGroup.Size = new System.Drawing.Size(284, 180);
            this.RequestHeaderGroup.TabIndex = 5;
            this.RequestHeaderGroup.TabStop = false;
            this.RequestHeaderGroup.Text = "Request headers";
            // 
            // UseReqHeaderCollection
            // 
            this.UseReqHeaderCollection.Image = ((System.Drawing.Image)(resources.GetObject("UseReqHeaderCollection.Image")));
            this.UseReqHeaderCollection.Location = new System.Drawing.Point(105, 146);
            this.UseReqHeaderCollection.Name = "UseReqHeaderCollection";
            this.UseReqHeaderCollection.Size = new System.Drawing.Size(25, 25);
            this.UseReqHeaderCollection.TabIndex = 4;
            this.UseReqHeaderCollection.UseVisualStyleBackColor = true;
            this.UseReqHeaderCollection.Click += new System.EventHandler(this.UseReqHeaderCollection_Click);
            // 
            // EditReqHeaderCollections
            // 
            this.EditReqHeaderCollections.Image = ((System.Drawing.Image)(resources.GetObject("EditReqHeaderCollections.Image")));
            this.EditReqHeaderCollections.Location = new System.Drawing.Point(136, 146);
            this.EditReqHeaderCollections.Name = "EditReqHeaderCollections";
            this.EditReqHeaderCollections.Size = new System.Drawing.Size(25, 25);
            this.EditReqHeaderCollections.TabIndex = 5;
            this.EditReqHeaderCollections.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.EditReqHeaderCollections.UseVisualStyleBackColor = true;
            this.EditReqHeaderCollections.Click += new System.EventHandler(this.EditReqHeaderCollections_Click);
            // 
            // EditReqHeader
            // 
            this.EditReqHeader.Image = ((System.Drawing.Image)(resources.GetObject("EditReqHeader.Image")));
            this.EditReqHeader.Location = new System.Drawing.Point(74, 146);
            this.EditReqHeader.Name = "EditReqHeader";
            this.EditReqHeader.Size = new System.Drawing.Size(25, 25);
            this.EditReqHeader.TabIndex = 3;
            this.EditReqHeader.UseVisualStyleBackColor = true;
            this.EditReqHeader.Click += new System.EventHandler(this.EditReqHeader_Click);
            // 
            // DeleteReqHeader
            // 
            this.DeleteReqHeader.Image = ((System.Drawing.Image)(resources.GetObject("DeleteReqHeader.Image")));
            this.DeleteReqHeader.Location = new System.Drawing.Point(43, 146);
            this.DeleteReqHeader.Name = "DeleteReqHeader";
            this.DeleteReqHeader.Size = new System.Drawing.Size(25, 25);
            this.DeleteReqHeader.TabIndex = 2;
            this.DeleteReqHeader.UseVisualStyleBackColor = true;
            this.DeleteReqHeader.Click += new System.EventHandler(this.DeleteReqHeader_Click);
            // 
            // AddReqHeader
            // 
            this.AddReqHeader.Image = ((System.Drawing.Image)(resources.GetObject("AddReqHeader.Image")));
            this.AddReqHeader.Location = new System.Drawing.Point(12, 146);
            this.AddReqHeader.Name = "AddReqHeader";
            this.AddReqHeader.Size = new System.Drawing.Size(25, 25);
            this.AddReqHeader.TabIndex = 1;
            this.AddReqHeader.UseVisualStyleBackColor = true;
            this.AddReqHeader.Click += new System.EventHandler(this.AddReqHeader_Click);
            // 
            // RequestHeaderList
            // 
            this.RequestHeaderList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ReqHdrName,
            this.ReqHdrDescription});
            this.RequestHeaderList.FullRowSelect = true;
            this.RequestHeaderList.GridLines = true;
            this.RequestHeaderList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.RequestHeaderList.HideSelection = false;
            this.RequestHeaderList.Location = new System.Drawing.Point(12, 19);
            this.RequestHeaderList.MultiSelect = false;
            this.RequestHeaderList.Name = "RequestHeaderList";
            this.RequestHeaderList.Size = new System.Drawing.Size(261, 121);
            this.RequestHeaderList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.RequestHeaderList.TabIndex = 0;
            this.RequestHeaderList.UseCompatibleStateImageBehavior = false;
            this.RequestHeaderList.View = System.Windows.Forms.View.Details;
            // 
            // ReqHdrName
            // 
            this.ReqHdrName.Text = "Name";
            this.ReqHdrName.Width = 128;
            // 
            // ReqHdrDescription
            // 
            this.ReqHdrDescription.Text = "Description";
            this.ReqHdrDescription.Width = 197;
            // 
            // RequestHeaderMenuStrip
            // 
            this.RequestHeaderMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem2,
            this.deleteToolStripMenuItem2,
            this.editToolStripMenuItem2,
            this.useCollectionToolStripMenuItem1,
            this.manageCollectionsToolStripMenuItem1});
            this.RequestHeaderMenuStrip.Name = "contextMenuStrip1";
            this.RequestHeaderMenuStrip.Size = new System.Drawing.Size(177, 114);
            // 
            // addToolStripMenuItem2
            // 
            this.addToolStripMenuItem2.Name = "addToolStripMenuItem2";
            this.addToolStripMenuItem2.Size = new System.Drawing.Size(176, 22);
            this.addToolStripMenuItem2.Text = "Add";
            this.addToolStripMenuItem2.Click += new System.EventHandler(this.AddReqHeader_Click);
            // 
            // deleteToolStripMenuItem2
            // 
            this.deleteToolStripMenuItem2.Name = "deleteToolStripMenuItem2";
            this.deleteToolStripMenuItem2.Size = new System.Drawing.Size(176, 22);
            this.deleteToolStripMenuItem2.Text = "Delete";
            this.deleteToolStripMenuItem2.Click += new System.EventHandler(this.DeleteReqHeader_Click);
            // 
            // editToolStripMenuItem2
            // 
            this.editToolStripMenuItem2.Name = "editToolStripMenuItem2";
            this.editToolStripMenuItem2.Size = new System.Drawing.Size(176, 22);
            this.editToolStripMenuItem2.Text = "Edit";
            this.editToolStripMenuItem2.Click += new System.EventHandler(this.EditReqHeader_Click);
            // 
            // useCollectionToolStripMenuItem1
            // 
            this.useCollectionToolStripMenuItem1.Name = "useCollectionToolStripMenuItem1";
            this.useCollectionToolStripMenuItem1.Size = new System.Drawing.Size(176, 22);
            this.useCollectionToolStripMenuItem1.Text = "UseCollection";
            this.useCollectionToolStripMenuItem1.Click += new System.EventHandler(this.UseReqHeaderCollection_Click);
            // 
            // manageCollectionsToolStripMenuItem1
            // 
            this.manageCollectionsToolStripMenuItem1.Name = "manageCollectionsToolStripMenuItem1";
            this.manageCollectionsToolStripMenuItem1.Size = new System.Drawing.Size(176, 22);
            this.manageCollectionsToolStripMenuItem1.Text = "ManageCollections";
            this.manageCollectionsToolStripMenuItem1.Click += new System.EventHandler(this.EditReqHeaderCollections_Click);
            // 
            // RESTOperationDialog
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(611, 612);
            this.Controls.Add(this.RequestHeaderGroup);
            this.Controls.Add(this.RequestParamBox);
            this.Controls.Add(this.HasPagination);
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
            this.RequestHeaderGroup.ResumeLayout(false);
            this.RequestHeaderMenuStrip.ResumeLayout(false);
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
        private ColumnHeader RespCode;
        private ColumnHeader RespDescription;
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
        private TextBox RequestTypeName;
        private Button SelectRequest;
        private Button RemoveRequest;
        private Button UseCollection;
        private Button EditCollections;
        private ToolStripMenuItem addToolStripMenuItem;
        private ToolStripMenuItem useCollectionToolStripMenuItem;
        private ToolStripMenuItem addToolStripMenuItem1;
        private ToolStripMenuItem manageCollectionsToolStripMenuItem;
        private GroupBox ReqCardinalityGroup;
        private Label label6;
        private TextBox ReqCardHi;
        private TextBox ReqCardLo;
        private GroupBox RequestHeaderGroup;
        private Button UseReqHeaderCollection;
        private Button EditReqHeaderCollections;
        private Button EditReqHeader;
        private Button DeleteReqHeader;
        private Button AddReqHeader;
        private ListView RequestHeaderList;
        private ColumnHeader ReqHdrName;
        private ColumnHeader ReqHdrDescription;
        private ContextMenuStrip RequestHeaderMenuStrip;
        private ToolStripMenuItem addToolStripMenuItem2;
        private ToolStripMenuItem deleteToolStripMenuItem2;
        private ToolStripMenuItem editToolStripMenuItem2;
        private ToolStripMenuItem useCollectionToolStripMenuItem1;
        private ToolStripMenuItem manageCollectionsToolStripMenuItem1;
    }
}