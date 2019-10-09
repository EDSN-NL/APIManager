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
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RESTResponseCodeDialog));
            this.Cancel = new System.Windows.Forms.Button();
            this.Ok = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.CategoryBox = new System.Windows.Forms.GroupBox();
            this.IsServerError = new System.Windows.Forms.RadioButton();
            this.IsClientError = new System.Windows.Forms.RadioButton();
            this.IsRedirection = new System.Windows.Forms.RadioButton();
            this.IsSuccess = new System.Windows.Forms.RadioButton();
            this.IsInformational = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.ResponseCode = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.ResponseDescription = new System.Windows.Forms.TextBox();
            this.ResponseRefTypeBox = new System.Windows.Forms.GroupBox();
            this.IsNone = new System.Windows.Forms.RadioButton();
            this.IsExternalLink = new System.Windows.Forms.RadioButton();
            this.IsDefaultResponseType = new System.Windows.Forms.RadioButton();
            this.IsCustomType = new System.Windows.Forms.RadioButton();
            this.IsDocument = new System.Windows.Forms.RadioButton();
            this.LocalReferenceBox = new System.Windows.Forms.GroupBox();
            this.RspCardinalityGroup = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.RspCardHi = new System.Windows.Forms.TextBox();
            this.RspCardLo = new System.Windows.Forms.TextBox();
            this.RemoveResponse = new System.Windows.Forms.Button();
            this.SelectResponse = new System.Windows.Forms.Button();
            this.ResponseTypeName = new System.Windows.Forms.TextBox();
            this.ExternalLinkBox = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ExternalLink = new System.Windows.Forms.TextBox();
            this.CategoryBox.SuspendLayout();
            this.ResponseRefTypeBox.SuspendLayout();
            this.LocalReferenceBox.SuspendLayout();
            this.RspCardinalityGroup.SuspendLayout();
            this.ExternalLinkBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(304, 346);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 6;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(223, 346);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 31);
            this.Ok.TabIndex = 7;
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
            this.CategoryBox.Controls.Add(this.IsServerError);
            this.CategoryBox.Controls.Add(this.IsClientError);
            this.CategoryBox.Controls.Add(this.IsRedirection);
            this.CategoryBox.Controls.Add(this.IsSuccess);
            this.CategoryBox.Controls.Add(this.IsInformational);
            this.CategoryBox.Location = new System.Drawing.Point(12, 12);
            this.CategoryBox.Name = "CategoryBox";
            this.CategoryBox.Size = new System.Drawing.Size(90, 166);
            this.CategoryBox.TabIndex = 1;
            this.CategoryBox.TabStop = false;
            this.CategoryBox.Text = "Category";
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
            this.IsInformational.Size = new System.Drawing.Size(46, 17);
            this.IsInformational.TabIndex = 1;
            this.IsInformational.TabStop = true;
            this.IsInformational.Tag = "Informational";
            this.IsInformational.Text = "Info.";
            this.IsInformational.UseVisualStyleBackColor = true;
            this.IsInformational.CheckedChanged += new System.EventHandler(this.Category_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 245);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "HTTP code:";
            // 
            // ResponseCode
            // 
            this.ResponseCode.FormattingEnabled = true;
            this.ResponseCode.Location = new System.Drawing.Point(84, 242);
            this.ResponseCode.Name = "ResponseCode";
            this.ResponseCode.Size = new System.Drawing.Size(295, 21);
            this.ResponseCode.TabIndex = 4;
            this.ResponseCode.SelectedIndexChanged += new System.EventHandler(this.ResponseCode_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 272);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Description:";
            // 
            // ResponseDescription
            // 
            this.ResponseDescription.Location = new System.Drawing.Point(84, 269);
            this.ResponseDescription.Multiline = true;
            this.ResponseDescription.Name = "ResponseDescription";
            this.ResponseDescription.Size = new System.Drawing.Size(295, 71);
            this.ResponseDescription.TabIndex = 5;
            this.ResponseDescription.Leave += new System.EventHandler(this.DescriptionNameFld_Leave);
            // 
            // ResponseRefTypeBox
            // 
            this.ResponseRefTypeBox.Controls.Add(this.IsNone);
            this.ResponseRefTypeBox.Controls.Add(this.IsExternalLink);
            this.ResponseRefTypeBox.Controls.Add(this.IsDefaultResponseType);
            this.ResponseRefTypeBox.Controls.Add(this.IsCustomType);
            this.ResponseRefTypeBox.Controls.Add(this.IsDocument);
            this.ResponseRefTypeBox.Location = new System.Drawing.Point(108, 12);
            this.ResponseRefTypeBox.Name = "ResponseRefTypeBox";
            this.ResponseRefTypeBox.Size = new System.Drawing.Size(284, 92);
            this.ResponseRefTypeBox.TabIndex = 2;
            this.ResponseRefTypeBox.TabStop = false;
            this.ResponseRefTypeBox.Text = "Response payload type";
            // 
            // IsNone
            // 
            this.IsNone.AutoSize = true;
            this.IsNone.Location = new System.Drawing.Point(8, 65);
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
            this.IsExternalLink.Location = new System.Drawing.Point(8, 42);
            this.IsExternalLink.Name = "IsExternalLink";
            this.IsExternalLink.Size = new System.Drawing.Size(82, 17);
            this.IsExternalLink.TabIndex = 3;
            this.IsExternalLink.TabStop = true;
            this.IsExternalLink.Tag = "Link";
            this.IsExternalLink.Text = "External link";
            this.IsExternalLink.UseVisualStyleBackColor = true;
            this.IsExternalLink.CheckedChanged += new System.EventHandler(this.PayloadType_CheckedChanged);
            // 
            // IsDefaultResponseType
            // 
            this.IsDefaultResponseType.AutoSize = true;
            this.IsDefaultResponseType.Location = new System.Drawing.Point(122, 42);
            this.IsDefaultResponseType.Name = "IsDefaultResponseType";
            this.IsDefaultResponseType.Size = new System.Drawing.Size(128, 17);
            this.IsDefaultResponseType.TabIndex = 4;
            this.IsDefaultResponseType.TabStop = true;
            this.IsDefaultResponseType.Tag = "DefaultResponse";
            this.IsDefaultResponseType.Text = "Default response type";
            this.IsDefaultResponseType.UseVisualStyleBackColor = true;
            this.IsDefaultResponseType.CheckedChanged += new System.EventHandler(this.PayloadType_CheckedChanged);
            // 
            // IsCustomType
            // 
            this.IsCustomType.AutoSize = true;
            this.IsCustomType.Location = new System.Drawing.Point(122, 19);
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
            this.IsDocument.Location = new System.Drawing.Point(8, 19);
            this.IsDocument.Name = "IsDocument";
            this.IsDocument.Size = new System.Drawing.Size(74, 17);
            this.IsDocument.TabIndex = 1;
            this.IsDocument.TabStop = true;
            this.IsDocument.Tag = "Document";
            this.IsDocument.Text = "Document";
            this.IsDocument.UseVisualStyleBackColor = true;
            this.IsDocument.CheckedChanged += new System.EventHandler(this.PayloadType_CheckedChanged);
            // 
            // LocalReferenceBox
            // 
            this.LocalReferenceBox.Controls.Add(this.RspCardinalityGroup);
            this.LocalReferenceBox.Controls.Add(this.RemoveResponse);
            this.LocalReferenceBox.Controls.Add(this.SelectResponse);
            this.LocalReferenceBox.Controls.Add(this.ResponseTypeName);
            this.LocalReferenceBox.Location = new System.Drawing.Point(108, 110);
            this.LocalReferenceBox.Name = "LocalReferenceBox";
            this.LocalReferenceBox.Size = new System.Drawing.Size(284, 68);
            this.LocalReferenceBox.TabIndex = 8;
            this.LocalReferenceBox.TabStop = false;
            this.LocalReferenceBox.Text = "Document / Custom response type";
            // 
            // RspCardinalityGroup
            // 
            this.RspCardinalityGroup.Controls.Add(this.label7);
            this.RspCardinalityGroup.Controls.Add(this.RspCardHi);
            this.RspCardinalityGroup.Controls.Add(this.RspCardLo);
            this.RspCardinalityGroup.Location = new System.Drawing.Point(206, 12);
            this.RspCardinalityGroup.Name = "RspCardinalityGroup";
            this.RspCardinalityGroup.Size = new System.Drawing.Size(70, 47);
            this.RspCardinalityGroup.TabIndex = 6;
            this.RspCardinalityGroup.TabStop = false;
            this.RspCardinalityGroup.Text = "Cardinality";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(25, 13);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(21, 20);
            this.label7.TabIndex = 0;
            this.label7.Text = "...";
            // 
            // RspCardHi
            // 
            this.RspCardHi.Location = new System.Drawing.Point(45, 19);
            this.RspCardHi.Name = "RspCardHi";
            this.RspCardHi.Size = new System.Drawing.Size(20, 20);
            this.RspCardHi.TabIndex = 2;
            // 
            // RspCardLo
            // 
            this.RspCardLo.Location = new System.Drawing.Point(6, 19);
            this.RspCardLo.Name = "RspCardLo";
            this.RspCardLo.Size = new System.Drawing.Size(20, 20);
            this.RspCardLo.TabIndex = 1;
            // 
            // RemoveResponse
            // 
            this.RemoveResponse.Image = ((System.Drawing.Image)(resources.GetObject("RemoveResponse.Image")));
            this.RemoveResponse.Location = new System.Drawing.Point(39, 27);
            this.RemoveResponse.Name = "RemoveResponse";
            this.RemoveResponse.Size = new System.Drawing.Size(25, 25);
            this.RemoveResponse.TabIndex = 2;
            this.RemoveResponse.UseVisualStyleBackColor = true;
            // 
            // SelectResponse
            // 
            this.SelectResponse.Image = ((System.Drawing.Image)(resources.GetObject("SelectResponse.Image")));
            this.SelectResponse.Location = new System.Drawing.Point(8, 27);
            this.SelectResponse.Name = "SelectResponse";
            this.SelectResponse.Size = new System.Drawing.Size(25, 25);
            this.SelectResponse.TabIndex = 1;
            this.SelectResponse.UseVisualStyleBackColor = true;
            // 
            // ResponseTypeName
            // 
            this.ResponseTypeName.Location = new System.Drawing.Point(70, 30);
            this.ResponseTypeName.Name = "ResponseTypeName";
            this.ResponseTypeName.ReadOnly = true;
            this.ResponseTypeName.Size = new System.Drawing.Size(130, 20);
            this.ResponseTypeName.TabIndex = 0;
            // 
            // ExternalLinkBox
            // 
            this.ExternalLinkBox.Controls.Add(this.ExternalLink);
            this.ExternalLinkBox.Controls.Add(this.label2);
            this.ExternalLinkBox.Location = new System.Drawing.Point(12, 184);
            this.ExternalLinkBox.Name = "ExternalLinkBox";
            this.ExternalLinkBox.Size = new System.Drawing.Size(380, 52);
            this.ExternalLinkBox.TabIndex = 9;
            this.ExternalLinkBox.TabStop = false;
            this.ExternalLinkBox.Text = "External link";
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
            // ExternalLink
            // 
            this.ExternalLink.Location = new System.Drawing.Point(42, 22);
            this.ExternalLink.Name = "ExternalLink";
            this.ExternalLink.Size = new System.Drawing.Size(325, 20);
            this.ExternalLink.TabIndex = 1;
            // 
            // RESTResponseCodeDialog
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(405, 389);
            this.Controls.Add(this.ExternalLinkBox);
            this.Controls.Add(this.LocalReferenceBox);
            this.Controls.Add(this.ResponseRefTypeBox);
            this.Controls.Add(this.ResponseDescription);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.ResponseCode);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.CategoryBox);
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
            this.ResponseRefTypeBox.ResumeLayout(false);
            this.ResponseRefTypeBox.PerformLayout();
            this.LocalReferenceBox.ResumeLayout(false);
            this.LocalReferenceBox.PerformLayout();
            this.RspCardinalityGroup.ResumeLayout(false);
            this.RspCardinalityGroup.PerformLayout();
            this.ExternalLinkBox.ResumeLayout(false);
            this.ExternalLinkBox.PerformLayout();
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
        private GroupBox ResponseRefTypeBox;
        private RadioButton IsNone;
        private RadioButton IsExternalLink;
        private RadioButton IsDefaultResponseType;
        private RadioButton IsCustomType;
        private RadioButton IsDocument;
        private GroupBox LocalReferenceBox;
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
    }
}