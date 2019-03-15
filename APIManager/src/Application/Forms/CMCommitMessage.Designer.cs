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
            this.ErrorText = new System.Windows.Forms.TextBox();
            this.ScopeBox = new System.Windows.Forms.GroupBox();
            this.ScopeRelease = new System.Windows.Forms.RadioButton();
            this.ScopeRemote = new System.Windows.Forms.RadioButton();
            this.ScopeLocal = new System.Windows.Forms.RadioButton();
            this.ScopeBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(518, 158);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 3;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(437, 158);
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
            // ScopeBox
            // 
            this.ScopeBox.Controls.Add(this.ScopeRelease);
            this.ScopeBox.Controls.Add(this.ScopeRemote);
            this.ScopeBox.Controls.Add(this.ScopeLocal);
            this.ScopeBox.Location = new System.Drawing.Point(12, 145);
            this.ScopeBox.Name = "ScopeBox";
            this.ScopeBox.Size = new System.Drawing.Size(400, 46);
            this.ScopeBox.TabIndex = 5;
            this.ScopeBox.TabStop = false;
            this.ScopeBox.Text = "Commit scope";
            // 
            // ScopeRelease
            // 
            this.ScopeRelease.AutoSize = true;
            this.ScopeRelease.Location = new System.Drawing.Point(204, 20);
            this.ScopeRelease.Name = "ScopeRelease";
            this.ScopeRelease.Size = new System.Drawing.Size(122, 17);
            this.ScopeRelease.TabIndex = 2;
            this.ScopeRelease.TabStop = true;
            this.ScopeRelease.Tag = "Release";
            this.ScopeRelease.Text = "Commit and Release";
            this.ScopeRelease.UseVisualStyleBackColor = true;
            this.ScopeRelease.CheckedChanged += new System.EventHandler(this.ScopeBox_CheckedChanged);
            // 
            // ScopeRemote
            // 
            this.ScopeRemote.AutoSize = true;
            this.ScopeRemote.Location = new System.Drawing.Point(101, 20);
            this.ScopeRemote.Name = "ScopeRemote";
            this.ScopeRemote.Size = new System.Drawing.Size(96, 17);
            this.ScopeRemote.TabIndex = 1;
            this.ScopeRemote.TabStop = true;
            this.ScopeRemote.Tag = "Remote";
            this.ScopeRemote.Text = "Push to remote";
            this.ScopeRemote.UseVisualStyleBackColor = true;
            this.ScopeRemote.CheckedChanged += new System.EventHandler(this.ScopeBox_CheckedChanged);
            // 
            // ScopeLocal
            // 
            this.ScopeLocal.AutoSize = true;
            this.ScopeLocal.Location = new System.Drawing.Point(7, 20);
            this.ScopeLocal.Name = "ScopeLocal";
            this.ScopeLocal.Size = new System.Drawing.Size(87, 17);
            this.ScopeLocal.TabIndex = 0;
            this.ScopeLocal.TabStop = true;
            this.ScopeLocal.Tag = "Local";
            this.ScopeLocal.Text = "Local commit";
            this.ScopeLocal.UseVisualStyleBackColor = true;
            this.ScopeLocal.CheckedChanged += new System.EventHandler(this.ScopeBox_CheckedChanged);
            // 
            // CMCommitMessage
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(608, 203);
            this.Controls.Add(this.ScopeBox);
            this.Controls.Add(this.ErrorText);
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
            this.ScopeBox.ResumeLayout(false);
            this.ScopeBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.TextBox AnnotationFld;
        private System.Windows.Forms.Label AnnotationLabel;
        private System.Windows.Forms.TextBox ErrorText;
        private System.Windows.Forms.GroupBox ScopeBox;
        private System.Windows.Forms.RadioButton ScopeRelease;
        private System.Windows.Forms.RadioButton ScopeRemote;
        private System.Windows.Forms.RadioButton ScopeLocal;
    }
}