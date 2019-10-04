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
            this.DefaultResponse = new System.Windows.Forms.CheckBox();
            this.ResponseRefTypeBox = new System.Windows.Forms.GroupBox();
            this.IsDocument = new System.Windows.Forms.RadioButton();
            this.IsCustomType = new System.Windows.Forms.RadioButton();
            this.IsDefaultResponseType = new System.Windows.Forms.RadioButton();
            this.IsExternalLink = new System.Windows.Forms.RadioButton();
            this.IsNone = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SelectTarget = new System.Windows.Forms.Button();
            this.CategoryBox.SuspendLayout();
            this.ResponseRefTypeBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(275, 259);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 4;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(194, 259);
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
            this.CategoryBox.Controls.Add(this.IsServerError);
            this.CategoryBox.Controls.Add(this.IsClientError);
            this.CategoryBox.Controls.Add(this.IsRedirection);
            this.CategoryBox.Controls.Add(this.IsSuccess);
            this.CategoryBox.Controls.Add(this.IsInformational);
            this.CategoryBox.Location = new System.Drawing.Point(12, 12);
            this.CategoryBox.Name = "CategoryBox";
            this.CategoryBox.Size = new System.Drawing.Size(99, 139);
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
            this.IsInformational.Size = new System.Drawing.Size(85, 17);
            this.IsInformational.TabIndex = 1;
            this.IsInformational.TabStop = true;
            this.IsInformational.Tag = "Informational";
            this.IsInformational.Text = "Informational";
            this.IsInformational.UseVisualStyleBackColor = true;
            this.IsInformational.CheckedChanged += new System.EventHandler(this.Category_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 160);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(112, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "HTTP response code:";
            // 
            // ResponseCode
            // 
            this.ResponseCode.FormattingEnabled = true;
            this.ResponseCode.Location = new System.Drawing.Point(122, 157);
            this.ResponseCode.Name = "ResponseCode";
            this.ResponseCode.Size = new System.Drawing.Size(228, 21);
            this.ResponseCode.TabIndex = 2;
            this.ResponseCode.SelectedIndexChanged += new System.EventHandler(this.ResponseCode_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(53, 187);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Description:";
            // 
            // ResponseDescription
            // 
            this.ResponseDescription.Location = new System.Drawing.Point(122, 184);
            this.ResponseDescription.Name = "ResponseDescription";
            this.ResponseDescription.Size = new System.Drawing.Size(228, 20);
            this.ResponseDescription.TabIndex = 3;
            this.ResponseDescription.Leave += new System.EventHandler(this.DescriptionNameFld_Leave);
            // 
            // DefaultResponse
            // 
            this.DefaultResponse.AutoSize = true;
            this.DefaultResponse.Location = new System.Drawing.Point(7, 236);
            this.DefaultResponse.Name = "DefaultResponse";
            this.DefaultResponse.Size = new System.Drawing.Size(147, 17);
            this.DefaultResponse.TabIndex = 7;
            this.DefaultResponse.Text = "Default response override";
            this.DefaultResponse.UseVisualStyleBackColor = true;
            this.DefaultResponse.CheckedChanged += new System.EventHandler(this.DefaultResponse_CheckedChanged);
            // 
            // ResponseRefTypeBox
            // 
            this.ResponseRefTypeBox.Controls.Add(this.IsNone);
            this.ResponseRefTypeBox.Controls.Add(this.IsExternalLink);
            this.ResponseRefTypeBox.Controls.Add(this.IsDefaultResponseType);
            this.ResponseRefTypeBox.Controls.Add(this.IsCustomType);
            this.ResponseRefTypeBox.Controls.Add(this.IsDocument);
            this.ResponseRefTypeBox.Location = new System.Drawing.Point(122, 12);
            this.ResponseRefTypeBox.Name = "ResponseRefTypeBox";
            this.ResponseRefTypeBox.Size = new System.Drawing.Size(228, 139);
            this.ResponseRefTypeBox.TabIndex = 8;
            this.ResponseRefTypeBox.TabStop = false;
            this.ResponseRefTypeBox.Text = "Response payload";
            // 
            // IsDocument
            // 
            this.IsDocument.AutoSize = true;
            this.IsDocument.Location = new System.Drawing.Point(6, 19);
            this.IsDocument.Name = "IsDocument";
            this.IsDocument.Size = new System.Drawing.Size(74, 17);
            this.IsDocument.TabIndex = 0;
            this.IsDocument.TabStop = true;
            this.IsDocument.Text = "Document";
            this.IsDocument.UseVisualStyleBackColor = true;
            // 
            // IsCustomType
            // 
            this.IsCustomType.AutoSize = true;
            this.IsCustomType.Location = new System.Drawing.Point(6, 42);
            this.IsCustomType.Name = "IsCustomType";
            this.IsCustomType.Size = new System.Drawing.Size(129, 17);
            this.IsCustomType.TabIndex = 1;
            this.IsCustomType.TabStop = true;
            this.IsCustomType.Text = "Custom response type";
            this.IsCustomType.UseVisualStyleBackColor = true;
            // 
            // IsDefaultResponseType
            // 
            this.IsDefaultResponseType.AutoSize = true;
            this.IsDefaultResponseType.Location = new System.Drawing.Point(6, 65);
            this.IsDefaultResponseType.Name = "IsDefaultResponseType";
            this.IsDefaultResponseType.Size = new System.Drawing.Size(128, 17);
            this.IsDefaultResponseType.TabIndex = 2;
            this.IsDefaultResponseType.TabStop = true;
            this.IsDefaultResponseType.Text = "Default response type";
            this.IsDefaultResponseType.UseVisualStyleBackColor = true;
            // 
            // IsExternalLink
            // 
            this.IsExternalLink.AutoSize = true;
            this.IsExternalLink.Location = new System.Drawing.Point(6, 88);
            this.IsExternalLink.Name = "IsExternalLink";
            this.IsExternalLink.Size = new System.Drawing.Size(82, 17);
            this.IsExternalLink.TabIndex = 3;
            this.IsExternalLink.TabStop = true;
            this.IsExternalLink.Text = "External link";
            this.IsExternalLink.UseVisualStyleBackColor = true;
            // 
            // IsNone
            // 
            this.IsNone.AutoSize = true;
            this.IsNone.Location = new System.Drawing.Point(6, 111);
            this.IsNone.Name = "IsNone";
            this.IsNone.Size = new System.Drawing.Size(51, 17);
            this.IsNone.TabIndex = 4;
            this.IsNone.TabStop = true;
            this.IsNone.Text = "None";
            this.IsNone.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(43, 213);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Target object:";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(122, 210);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(192, 20);
            this.textBox1.TabIndex = 10;
            // 
            // SelectTarget
            // 
            this.SelectTarget.Location = new System.Drawing.Point(320, 210);
            this.SelectTarget.Name = "SelectTarget";
            this.SelectTarget.Size = new System.Drawing.Size(30, 23);
            this.SelectTarget.TabIndex = 11;
            this.SelectTarget.Text = "...";
            this.SelectTarget.UseVisualStyleBackColor = true;
            // 
            // RESTResponseCodeDialog
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(364, 301);
            this.Controls.Add(this.SelectTarget);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ResponseRefTypeBox);
            this.Controls.Add(this.DefaultResponse);
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
        private CheckBox DefaultResponse;
        private GroupBox ResponseRefTypeBox;
        private RadioButton IsNone;
        private RadioButton IsExternalLink;
        private RadioButton IsDefaultResponseType;
        private RadioButton IsCustomType;
        private RadioButton IsDocument;
        private Label label2;
        private TextBox textBox1;
        private Button SelectTarget;
    }
}