using System.Windows.Forms;

namespace Plugin.Application.Forms
{
    partial class RESTResponseCodeDialog
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
            this._result = null;    // We don't 'OWN' this class, so don't call dispose on it!
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RESTResponseCodeDialog));
            this.Cancel = new System.Windows.Forms.Button();
            this.Ok = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.CategoryBox = new System.Windows.Forms.GroupBox();
            this.IsDefault = new System.Windows.Forms.RadioButton();
            this.IsServerError = new System.Windows.Forms.RadioButton();
            this.IsClientError = new System.Windows.Forms.RadioButton();
            this.IsRedirection = new System.Windows.Forms.RadioButton();
            this.IsSuccess = new System.Windows.Forms.RadioButton();
            this.IsInformational = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.ResponseCode = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.ResponseDescription = new System.Windows.Forms.TextBox();
            this.PayloadTypeBox = new System.Windows.Forms.GroupBox();
            this.IsNone = new System.Windows.Forms.RadioButton();
            this.IsExternalLink = new System.Windows.Forms.RadioButton();
            this.IsDefaultResponseType = new System.Windows.Forms.RadioButton();
            this.IsCustomType = new System.Windows.Forms.RadioButton();
            this.IsDocument = new System.Windows.Forms.RadioButton();
            this.ResponsePayloadBox = new System.Windows.Forms.GroupBox();
            this.RspCardinalityGroup = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.RspCardHi = new System.Windows.Forms.TextBox();
            this.RspCardLo = new System.Windows.Forms.TextBox();
            this.RemoveResponse = new System.Windows.Forms.Button();
            this.SelectResponse = new System.Windows.Forms.Button();
            this.ResponseTypeName = new System.Windows.Forms.TextBox();
            this.ExternalLinkBox = new System.Windows.Forms.GroupBox();
            this.ExternalLink = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.IsRange = new System.Windows.Forms.CheckBox();
            this.ResponseHeaderGroup = new System.Windows.Forms.GroupBox();
            this.EditReqHeader = new System.Windows.Forms.Button();
            this.DeleteReqHeader = new System.Windows.Forms.Button();
            this.AddReqHeader = new System.Windows.Forms.Button();
            this.ResponseHeaderList = new System.Windows.Forms.ListView();
            this.ReqHdrName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ReqHdrDescription = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.CategoryCodeGroup = new System.Windows.Forms.GroupBox();
            this.PayloadGroup = new System.Windows.Forms.GroupBox();
            this.ResponseHeaderMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CategoryBox.SuspendLayout();
            this.PayloadTypeBox.SuspendLayout();
            this.ResponsePayloadBox.SuspendLayout();
            this.RspCardinalityGroup.SuspendLayout();
            this.ExternalLinkBox.SuspendLayout();
            this.ResponseHeaderGroup.SuspendLayout();
            this.CategoryCodeGroup.SuspendLayout();
            this.PayloadGroup.SuspendLayout();
            this.ResponseHeaderMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(711, 353);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 4;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(630, 353);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 31);
            this.Ok.TabIndex = 5;
            this.Ok.Text = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
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
            // CategoryBox
            // 
            this.CategoryBox.Controls.Add(this.IsDefault);
            this.CategoryBox.Controls.Add(this.IsServerError);
            this.CategoryBox.Controls.Add(this.IsClientError);
            this.CategoryBox.Controls.Add(this.IsRedirection);
            this.CategoryBox.Controls.Add(this.IsSuccess);
            this.CategoryBox.Controls.Add(this.IsInformational);
            this.CategoryBox.Location = new System.Drawing.Point(6, 19);
            this.CategoryBox.Name = "CategoryBox";
            this.CategoryBox.Size = new System.Drawing.Size(101, 164);
            this.CategoryBox.TabIndex = 1;
            this.CategoryBox.TabStop = false;
            this.CategoryBox.Text = "Category";
            // 
            // IsDefault
            // 
            this.IsDefault.AutoSize = true;
            this.IsDefault.Location = new System.Drawing.Point(6, 134);
            this.IsDefault.Name = "IsDefault";
            this.IsDefault.Size = new System.Drawing.Size(59, 17);
            this.IsDefault.TabIndex = 6;
            this.IsDefault.TabStop = true;
            this.IsDefault.Tag = "Default";
            this.IsDefault.Text = "Default";
            this.IsDefault.UseVisualStyleBackColor = true;
            this.IsDefault.CheckedChanged += new System.EventHandler(this.Category_CheckedChanged);
            // 
            // IsServerError
            // 
            this.IsServerError.AutoSize = true;
            this.IsServerError.Location = new System.Drawing.Point(6, 111);
            this.IsServerError.Name = "IsServerError";
            this.IsServerError.Size = new System.Drawing.Size(80, 17);
            this.IsServerError.TabIndex = 5;
            this.IsServerError.TabStop = true;
            this.IsServerError.Tag = "ServerError";
            this.IsServerError.Text = "Server error";
            this.IsServerError.UseVisualStyleBackColor = true;
            this.IsServerError.CheckedChanged += new System.EventHandler(this.Category_CheckedChanged);
            // 
            // IsClientError
            // 
            this.IsClientError.AutoSize = true;
            this.IsClientError.Location = new System.Drawing.Point(6, 88);
            this.IsClientError.Name = "IsClientError";
            this.IsClientError.Size = new System.Drawing.Size(75, 17);
            this.IsClientError.TabIndex = 4;
            this.IsClientError.TabStop = true;
            this.IsClientError.Tag = "ClientError";
            this.IsClientError.Text = "Client error";
            this.IsClientError.UseVisualStyleBackColor = true;
            this.IsClientError.CheckedChanged += new System.EventHandler(this.Category_CheckedChanged);
            // 
            // IsRedirection
            // 
            this.IsRedirection.AutoSize = true;
            this.IsRedirection.Location = new System.Drawing.Point(6, 65);
            this.IsRedirection.Name = "IsRedirection";
            this.IsRedirection.Size = new System.Drawing.Size(79, 17);
            this.IsRedirection.TabIndex = 3;
            this.IsRedirection.TabStop = true;
            this.IsRedirection.Tag = "Redirection";
            this.IsRedirection.Text = "Redirection";
            this.IsRedirection.UseVisualStyleBackColor = true;
            this.IsRedirection.CheckedChanged += new System.EventHandler(this.Category_CheckedChanged);
            // 
            // IsSuccess
            // 
            this.IsSuccess.AutoSize = true;
            this.IsSuccess.Location = new System.Drawing.Point(6, 42);
            this.IsSuccess.Name = "IsSuccess";
            this.IsSuccess.Size = new System.Drawing.Size(66, 17);
            this.IsSuccess.TabIndex = 2;
            this.IsSuccess.TabStop = true;
            this.IsSuccess.Tag = "Success";
            this.IsSuccess.Text = "Success";
            this.IsSuccess.UseVisualStyleBackColor = true;
            this.IsSuccess.CheckedChanged += new System.EventHandler(this.Category_CheckedChanged);
            // 
            // IsInformational
            // 
            this.IsInformational.AutoSize = true;
            this.IsInformational.Location = new System.Drawing.Point(6, 19);
            this.IsInformational.Name = "IsInformational";
            this.IsInformational.Size = new System.Drawing.Size(77, 17);
            this.IsInformational.TabIndex = 1;
            this.IsInformational.TabStop = true;
            this.IsInformational.Tag = "Informational";
            this.IsInformational.Text = "Information";
            this.IsInformational.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.IsInformational.UseVisualStyleBackColor = true;
            this.IsInformational.CheckedChanged += new System.EventHandler(this.Category_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(113, 19);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "HTTP code:";
            // 
            // ResponseCode
            // 
            this.ResponseCode.FormattingEnabled = true;
            this.ResponseCode.Location = new System.Drawing.Point(182, 16);
            this.ResponseCode.Name = "ResponseCode";
            this.ResponseCode.Size = new System.Drawing.Size(293, 21);
            this.ResponseCode.TabIndex = 2;
            this.ResponseCode.SelectedIndexChanged += new System.EventHandler(this.ResponseCode_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(113, 63);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Description:";
            // 
            // ResponseDescription
            // 
            this.ResponseDescription.Location = new System.Drawing.Point(182, 66);
            this.ResponseDescription.Multiline = true;
            this.ResponseDescription.Name = "ResponseDescription";
            this.ResponseDescription.Size = new System.Drawing.Size(293, 117);
            this.ResponseDescription.TabIndex = 4;
            this.ResponseDescription.Leave += new System.EventHandler(this.DescriptionNameFld_Leave);
            // 
            // PayloadTypeBox
            // 
            this.PayloadTypeBox.Controls.Add(this.IsNone);
            this.PayloadTypeBox.Controls.Add(this.IsExternalLink);
            this.PayloadTypeBox.Controls.Add(this.IsDefaultResponseType);
            this.PayloadTypeBox.Controls.Add(this.IsCustomType);
            this.PayloadTypeBox.Controls.Add(this.IsDocument);
            this.PayloadTypeBox.Location = new System.Drawing.Point(6, 19);
            this.PayloadTypeBox.Name = "PayloadTypeBox";
            this.PayloadTypeBox.Size = new System.Drawing.Size(145, 142);
            this.PayloadTypeBox.TabIndex = 1;
            this.PayloadTypeBox.TabStop = false;
            this.PayloadTypeBox.Text = "Payload type";
            // 
            // IsNone
            // 
            this.IsNone.AutoSize = true;
            this.IsNone.Location = new System.Drawing.Point(6, 111);
            this.IsNone.Name = "IsNone";
            this.IsNone.Size = new System.Drawing.Size(51, 17);
            this.IsNone.TabIndex = 5;
            this.IsNone.TabStop = true;
            this.IsNone.Tag = "None";
            this.IsNone.Text = "None";
            this.IsNone.UseVisualStyleBackColor = true;
            this.IsNone.CheckedChanged += new System.EventHandler(this.PayloadType_CheckedChanged);
            // 
            // IsExternalLink
            // 
            this.IsExternalLink.AutoSize = true;
            this.IsExternalLink.Location = new System.Drawing.Point(6, 88);
            this.IsExternalLink.Name = "IsExternalLink";
            this.IsExternalLink.Size = new System.Drawing.Size(82, 17);
            this.IsExternalLink.TabIndex = 4;
            this.IsExternalLink.TabStop = true;
            this.IsExternalLink.Tag = "Link";
            this.IsExternalLink.Text = "External link";
            this.IsExternalLink.UseVisualStyleBackColor = true;
            this.IsExternalLink.CheckedChanged += new System.EventHandler(this.PayloadType_CheckedChanged);
            // 
            // IsDefaultResponseType
            // 
            this.IsDefaultResponseType.AutoSize = true;
            this.IsDefaultResponseType.Location = new System.Drawing.Point(6, 65);
            this.IsDefaultResponseType.Name = "IsDefaultResponseType";
            this.IsDefaultResponseType.Size = new System.Drawing.Size(128, 17);
            this.IsDefaultResponseType.TabIndex = 3;
            this.IsDefaultResponseType.TabStop = true;
            this.IsDefaultResponseType.Tag = "DefaultResponse";
            this.IsDefaultResponseType.Text = "Default response type";
            this.IsDefaultResponseType.UseVisualStyleBackColor = true;
            this.IsDefaultResponseType.CheckedChanged += new System.EventHandler(this.PayloadType_CheckedChanged);
            // 
            // IsCustomType
            // 
            this.IsCustomType.AutoSize = true;
            this.IsCustomType.Location = new System.Drawing.Point(6, 42);
            this.IsCustomType.Name = "IsCustomType";
            this.IsCustomType.Size = new System.Drawing.Size(129, 17);
            this.IsCustomType.TabIndex = 2;
            this.IsCustomType.TabStop = true;
            this.IsCustomType.Tag = "CustomResponse";
            this.IsCustomType.Text = "Custom response type";
            this.IsCustomType.UseVisualStyleBackColor = true;
            this.IsCustomType.CheckedChanged += new System.EventHandler(this.PayloadType_CheckedChanged);
            // 
            // IsDocument
            // 
            this.IsDocument.AutoSize = true;
            this.IsDocument.Location = new System.Drawing.Point(6, 19);
            this.IsDocument.Name = "IsDocument";
            this.IsDocument.Size = new System.Drawing.Size(130, 17);
            this.IsDocument.TabIndex = 1;
            this.IsDocument.TabStop = true;
            this.IsDocument.Tag = "Document";
            this.IsDocument.Text = "Document / ProfileSet";
            this.IsDocument.UseVisualStyleBackColor = true;
            this.IsDocument.CheckedChanged += new System.EventHandler(this.PayloadType_CheckedChanged);
            // 
            // ResponsePayloadBox
            // 
            this.ResponsePayloadBox.Controls.Add(this.RspCardinalityGroup);
            this.ResponsePayloadBox.Controls.Add(this.RemoveResponse);
            this.ResponsePayloadBox.Controls.Add(this.SelectResponse);
            this.ResponsePayloadBox.Controls.Add(this.ResponseTypeName);
            this.ResponsePayloadBox.Location = new System.Drawing.Point(157, 19);
            this.ResponsePayloadBox.Name = "ResponsePayloadBox";
            this.ResponsePayloadBox.Size = new System.Drawing.Size(319, 68);
            this.ResponsePayloadBox.TabIndex = 2;
            this.ResponsePayloadBox.TabStop = false;
            this.ResponsePayloadBox.Text = "Document / Custom response type";
            // 
            // RspCardinalityGroup
            // 
            this.RspCardinalityGroup.Controls.Add(this.label7);
            this.RspCardinalityGroup.Controls.Add(this.RspCardHi);
            this.RspCardinalityGroup.Controls.Add(this.RspCardLo);
            this.RspCardinalityGroup.Location = new System.Drawing.Point(240, 11);
            this.RspCardinalityGroup.Name = "RspCardinalityGroup";
            this.RspCardinalityGroup.Size = new System.Drawing.Size(70, 47);
            this.RspCardinalityGroup.TabIndex = 3;
            this.RspCardinalityGroup.TabStop = false;
            this.RspCardinalityGroup.Text = "Cardinality";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(26, 18);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(19, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "...";
            // 
            // RspCardHi
            // 
            this.RspCardHi.Location = new System.Drawing.Point(45, 19);
            this.RspCardHi.Name = "RspCardHi";
            this.RspCardHi.Size = new System.Drawing.Size(20, 20);
            this.RspCardHi.TabIndex = 2;
            this.RspCardHi.Leave += new System.EventHandler(this.RspCardinality_Leave);
            // 
            // RspCardLo
            // 
            this.RspCardLo.Location = new System.Drawing.Point(6, 19);
            this.RspCardLo.Name = "RspCardLo";
            this.RspCardLo.Size = new System.Drawing.Size(20, 20);
            this.RspCardLo.TabIndex = 1;
            this.RspCardLo.Leave += new System.EventHandler(this.RspCardinality_Leave);
            // 
            // RemoveResponse
            // 
            this.RemoveResponse.Image = ((System.Drawing.Image)(resources.GetObject("RemoveResponse.Image")));
            this.RemoveResponse.Location = new System.Drawing.Point(39, 27);
            this.RemoveResponse.Name = "RemoveResponse";
            this.RemoveResponse.Size = new System.Drawing.Size(25, 25);
            this.RemoveResponse.TabIndex = 2;
            this.RemoveResponse.UseVisualStyleBackColor = true;
            this.RemoveResponse.Click += new System.EventHandler(this.RemoveResponse_Click);
            // 
            // SelectResponse
            // 
            this.SelectResponse.Image = ((System.Drawing.Image)(resources.GetObject("SelectResponse.Image")));
            this.SelectResponse.Location = new System.Drawing.Point(8, 27);
            this.SelectResponse.Name = "SelectResponse";
            this.SelectResponse.Size = new System.Drawing.Size(25, 25);
            this.SelectResponse.TabIndex = 1;
            this.SelectResponse.UseVisualStyleBackColor = true;
            this.SelectResponse.Click += new System.EventHandler(this.SelectResponse_Click);
            // 
            // ResponseTypeName
            // 
            this.ResponseTypeName.Location = new System.Drawing.Point(70, 30);
            this.ResponseTypeName.Name = "ResponseTypeName";
            this.ResponseTypeName.ReadOnly = true;
            this.ResponseTypeName.Size = new System.Drawing.Size(164, 20);
            this.ResponseTypeName.TabIndex = 0;
            // 
            // ExternalLinkBox
            // 
            this.ExternalLinkBox.Controls.Add(this.ExternalLink);
            this.ExternalLinkBox.Controls.Add(this.label2);
            this.ExternalLinkBox.Location = new System.Drawing.Point(157, 93);
            this.ExternalLinkBox.Name = "ExternalLinkBox";
            this.ExternalLinkBox.Size = new System.Drawing.Size(318, 68);
            this.ExternalLinkBox.TabIndex = 3;
            this.ExternalLinkBox.TabStop = false;
            this.ExternalLinkBox.Text = "External link";
            // 
            // ExternalLink
            // 
            this.ExternalLink.Location = new System.Drawing.Point(42, 22);
            this.ExternalLink.Name = "ExternalLink";
            this.ExternalLink.Size = new System.Drawing.Size(262, 20);
            this.ExternalLink.TabIndex = 1;
            this.ExternalLink.Leave += new System.EventHandler(this.ExternalLink_Leave);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Link:";
            // 
            // IsRange
            // 
            this.IsRange.AutoSize = true;
            this.IsRange.Location = new System.Drawing.Point(182, 43);
            this.IsRange.Name = "IsRange";
            this.IsRange.Size = new System.Drawing.Size(122, 17);
            this.IsRange.TabIndex = 3;
            this.IsRange.Text = "Enforce range (nXX)";
            this.IsRange.UseVisualStyleBackColor = true;
            this.IsRange.CheckedChanged += new System.EventHandler(this.IsRange_CheckedChanged);
            // 
            // ResponseHeaderGroup
            // 
            this.ResponseHeaderGroup.Controls.Add(this.EditReqHeader);
            this.ResponseHeaderGroup.Controls.Add(this.DeleteReqHeader);
            this.ResponseHeaderGroup.Controls.Add(this.AddReqHeader);
            this.ResponseHeaderGroup.Controls.Add(this.ResponseHeaderList);
            this.ResponseHeaderGroup.Location = new System.Drawing.Point(502, 12);
            this.ResponseHeaderGroup.Name = "ResponseHeaderGroup";
            this.ResponseHeaderGroup.Size = new System.Drawing.Size(284, 193);
            this.ResponseHeaderGroup.TabIndex = 3;
            this.ResponseHeaderGroup.TabStop = false;
            this.ResponseHeaderGroup.Text = "Response headers";
            // 
            // EditReqHeader
            // 
            this.EditReqHeader.Image = ((System.Drawing.Image)(resources.GetObject("EditReqHeader.Image")));
            this.EditReqHeader.Location = new System.Drawing.Point(74, 155);
            this.EditReqHeader.Name = "EditReqHeader";
            this.EditReqHeader.Size = new System.Drawing.Size(25, 25);
            this.EditReqHeader.TabIndex = 3;
            this.EditReqHeader.UseVisualStyleBackColor = true;
            this.EditReqHeader.Click += new System.EventHandler(this.EditRspHeader_Click);
            // 
            // DeleteReqHeader
            // 
            this.DeleteReqHeader.Image = ((System.Drawing.Image)(resources.GetObject("DeleteReqHeader.Image")));
            this.DeleteReqHeader.Location = new System.Drawing.Point(43, 155);
            this.DeleteReqHeader.Name = "DeleteReqHeader";
            this.DeleteReqHeader.Size = new System.Drawing.Size(25, 25);
            this.DeleteReqHeader.TabIndex = 2;
            this.DeleteReqHeader.UseVisualStyleBackColor = true;
            this.DeleteReqHeader.Click += new System.EventHandler(this.DeleteRspHeader_Click);
            // 
            // AddReqHeader
            // 
            this.AddReqHeader.Image = ((System.Drawing.Image)(resources.GetObject("AddReqHeader.Image")));
            this.AddReqHeader.Location = new System.Drawing.Point(12, 155);
            this.AddReqHeader.Name = "AddReqHeader";
            this.AddReqHeader.Size = new System.Drawing.Size(25, 25);
            this.AddReqHeader.TabIndex = 1;
            this.AddReqHeader.UseVisualStyleBackColor = true;
            this.AddReqHeader.Click += new System.EventHandler(this.AddRspHeader_Click);
            // 
            // ResponseHeaderList
            // 
            this.ResponseHeaderList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ReqHdrName,
            this.ReqHdrDescription});
            this.ResponseHeaderList.FullRowSelect = true;
            this.ResponseHeaderList.GridLines = true;
            this.ResponseHeaderList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.ResponseHeaderList.HideSelection = false;
            this.ResponseHeaderList.Location = new System.Drawing.Point(12, 19);
            this.ResponseHeaderList.MultiSelect = false;
            this.ResponseHeaderList.Name = "ResponseHeaderList";
            this.ResponseHeaderList.Size = new System.Drawing.Size(261, 130);
            this.ResponseHeaderList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.ResponseHeaderList.TabIndex = 0;
            this.ResponseHeaderList.UseCompatibleStateImageBehavior = false;
            this.ResponseHeaderList.View = System.Windows.Forms.View.Details;
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
            // CategoryCodeGroup
            // 
            this.CategoryCodeGroup.Controls.Add(this.CategoryBox);
            this.CategoryCodeGroup.Controls.Add(this.label3);
            this.CategoryCodeGroup.Controls.Add(this.IsRange);
            this.CategoryCodeGroup.Controls.Add(this.ResponseCode);
            this.CategoryCodeGroup.Controls.Add(this.ResponseDescription);
            this.CategoryCodeGroup.Controls.Add(this.label4);
            this.CategoryCodeGroup.Location = new System.Drawing.Point(12, 12);
            this.CategoryCodeGroup.Name = "CategoryCodeGroup";
            this.CategoryCodeGroup.Size = new System.Drawing.Size(484, 193);
            this.CategoryCodeGroup.TabIndex = 1;
            this.CategoryCodeGroup.TabStop = false;
            this.CategoryCodeGroup.Text = "Response type definition";
            // 
            // PayloadGroup
            // 
            this.PayloadGroup.Controls.Add(this.PayloadTypeBox);
            this.PayloadGroup.Controls.Add(this.ResponsePayloadBox);
            this.PayloadGroup.Controls.Add(this.ExternalLinkBox);
            this.PayloadGroup.Location = new System.Drawing.Point(12, 211);
            this.PayloadGroup.Name = "PayloadGroup";
            this.PayloadGroup.Size = new System.Drawing.Size(484, 173);
            this.PayloadGroup.TabIndex = 2;
            this.PayloadGroup.TabStop = false;
            this.PayloadGroup.Text = "Payload definition";
            // 
            // ResponseHeaderMenuStrip
            // 
            this.ResponseHeaderMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.editToolStripMenuItem});
            this.ResponseHeaderMenuStrip.Name = "ResponseHeaderMenuStrip";
            this.ResponseHeaderMenuStrip.Size = new System.Drawing.Size(108, 70);
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.addToolStripMenuItem.Text = "Add";
            this.addToolStripMenuItem.Click += new System.EventHandler(this.AddRspHeader_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.DeleteRspHeader_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.editToolStripMenuItem.Text = "Edit";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.EditRspHeader_Click);
            // 
            // RESTResponseCodeDialog
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(797, 393);
            this.Controls.Add(this.PayloadGroup);
            this.Controls.Add(this.CategoryCodeGroup);
            this.Controls.Add(this.ResponseHeaderGroup);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.Cancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RESTResponseCodeDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Operation Result";
            this.CategoryBox.ResumeLayout(false);
            this.CategoryBox.PerformLayout();
            this.PayloadTypeBox.ResumeLayout(false);
            this.PayloadTypeBox.PerformLayout();
            this.ResponsePayloadBox.ResumeLayout(false);
            this.ResponsePayloadBox.PerformLayout();
            this.RspCardinalityGroup.ResumeLayout(false);
            this.RspCardinalityGroup.PerformLayout();
            this.ExternalLinkBox.ResumeLayout(false);
            this.ExternalLinkBox.PerformLayout();
            this.ResponseHeaderGroup.ResumeLayout(false);
            this.CategoryCodeGroup.ResumeLayout(false);
            this.CategoryCodeGroup.PerformLayout();
            this.PayloadGroup.ResumeLayout(false);
            this.ResponseHeaderMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button Ok;
        private Label label1;
        private GroupBox CategoryBox;
        private RadioButton IsServerError;
        private RadioButton IsClientError;
        private RadioButton IsRedirection;
        private RadioButton IsSuccess;
        private RadioButton IsInformational;
        private Label label3;
        private ComboBox ResponseCode;
        private Label label4;
        private TextBox ResponseDescription;
        private GroupBox PayloadTypeBox;
        private RadioButton IsNone;
        private RadioButton IsExternalLink;
        private RadioButton IsDefaultResponseType;
        private RadioButton IsCustomType;
        private RadioButton IsDocument;
        private GroupBox ResponsePayloadBox;
        private GroupBox RspCardinalityGroup;
        private Label label7;
        private TextBox RspCardHi;
        private TextBox RspCardLo;
        private Button RemoveResponse;
        private Button SelectResponse;
        private TextBox ResponseTypeName;
        private GroupBox ExternalLinkBox;
        private TextBox ExternalLink;
        private Label label2;
        private RadioButton IsDefault;
        private CheckBox IsRange;
        private GroupBox ResponseHeaderGroup;
        private Button EditReqHeader;
        private Button DeleteReqHeader;
        private Button AddReqHeader;
        private ListView ResponseHeaderList;
        private ColumnHeader ReqHdrName;
        private ColumnHeader ReqHdrDescription;
        private GroupBox CategoryCodeGroup;
        private GroupBox PayloadGroup;
        private ContextMenuStrip ResponseHeaderMenuStrip;
        private ToolStripMenuItem addToolStripMenuItem;
        private ToolStripMenuItem deleteToolStripMenuItem;
        private ToolStripMenuItem editToolStripMenuItem;
    }
}