namespace Plugin.Application.Forms
{
    partial class CMCommitMessage
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
            this.AnnotationFld = new System.Windows.Forms.TextBox();
            this.AnnotationLabel = new System.Windows.Forms.Label();
            this.CreateRelease = new System.Windows.Forms.CheckBox();
            this.ErrorText = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(518, 144);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 3;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(437, 144);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 31);
            this.Ok.TabIndex = 4;
            this.Ok.Text = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            // 
            // AnnotationFld
            // 
            this.AnnotationFld.AcceptsReturn = true;
            this.AnnotationFld.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AnnotationFld.Location = new System.Drawing.Point(12, 32);
            this.AnnotationFld.Multiline = true;
            this.AnnotationFld.Name = "AnnotationFld";
            this.AnnotationFld.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.AnnotationFld.Size = new System.Drawing.Size(581, 106);
            this.AnnotationFld.TabIndex = 1;
            this.AnnotationFld.Leave += new System.EventHandler(this.AnnotationFld_Leave);
            // 
            // AnnotationLabel
            // 
            this.AnnotationLabel.AutoSize = true;
            this.AnnotationLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AnnotationLabel.Location = new System.Drawing.Point(12, 9);
            this.AnnotationLabel.Name = "AnnotationLabel";
            this.AnnotationLabel.Size = new System.Drawing.Size(141, 16);
            this.AnnotationLabel.TabIndex = 0;
            this.AnnotationLabel.Text = "Description of change:";
            // 
            // CreateRelease
            // 
            this.CreateRelease.AutoSize = true;
            this.CreateRelease.Location = new System.Drawing.Point(12, 144);
            this.CreateRelease.Name = "CreateRelease";
            this.CreateRelease.Size = new System.Drawing.Size(151, 17);
            this.CreateRelease.TabIndex = 2;
            this.CreateRelease.Text = "Commit and create release";
            this.CreateRelease.UseVisualStyleBackColor = true;
            // 
            // ErrorText
            // 
            this.ErrorText.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ErrorText.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ErrorText.ForeColor = System.Drawing.Color.Red;
            this.ErrorText.Location = new System.Drawing.Point(185, 11);
            this.ErrorText.Name = "ErrorText";
            this.ErrorText.ReadOnly = true;
            this.ErrorText.Size = new System.Drawing.Size(408, 15);
            this.ErrorText.TabIndex = 0;
            // 
            // CMCommitMessage
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(608, 191);
            this.Controls.Add(this.ErrorText);
            this.Controls.Add(this.CreateRelease);
            this.Controls.Add(this.AnnotationLabel);
            this.Controls.Add(this.AnnotationFld);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.Cancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CMCommitMessage";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Change log";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.TextBox AnnotationFld;
        private System.Windows.Forms.Label AnnotationLabel;
        private System.Windows.Forms.CheckBox CreateRelease;
        private System.Windows.Forms.TextBox ErrorText;
    }
}