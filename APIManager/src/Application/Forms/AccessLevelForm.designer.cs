namespace Plugin.Application.Forms
{
    partial class AccessLevelForm
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
            this.AccessLevelGroup = new System.Windows.Forms.Panel();
            this.RBSecret = new System.Windows.Forms.RadioButton();
            this.RBConfidential = new System.Windows.Forms.RadioButton();
            this.RBInternalUse = new System.Windows.Forms.RadioButton();
            this.RBPublic = new System.Windows.Forms.RadioButton();
            this.Done = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.AccessLevelGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // AccessLevelGroup
            // 
            this.AccessLevelGroup.Controls.Add(this.RBSecret);
            this.AccessLevelGroup.Controls.Add(this.RBConfidential);
            this.AccessLevelGroup.Controls.Add(this.RBInternalUse);
            this.AccessLevelGroup.Controls.Add(this.RBPublic);
            this.AccessLevelGroup.Location = new System.Drawing.Point(3, 46);
            this.AccessLevelGroup.Name = "AccessLevelGroup";
            this.AccessLevelGroup.Size = new System.Drawing.Size(166, 96);
            this.AccessLevelGroup.TabIndex = 0;
            // 
            // RBSecret
            // 
            this.RBSecret.AutoSize = true;
            this.RBSecret.Location = new System.Drawing.Point(16, 72);
            this.RBSecret.Name = "RBSecret";
            this.RBSecret.Size = new System.Drawing.Size(56, 17);
            this.RBSecret.TabIndex = 3;
            this.RBSecret.TabStop = true;
            this.RBSecret.Text = "Secret";
            this.RBSecret.UseVisualStyleBackColor = true;
            this.RBSecret.CheckedChanged += new System.EventHandler(this.RBSecret_CheckedChanged);
            // 
            // RBConfidential
            // 
            this.RBConfidential.AutoSize = true;
            this.RBConfidential.Location = new System.Drawing.Point(16, 49);
            this.RBConfidential.Name = "RBConfidential";
            this.RBConfidential.Size = new System.Drawing.Size(80, 17);
            this.RBConfidential.TabIndex = 2;
            this.RBConfidential.TabStop = true;
            this.RBConfidential.Text = "Confidential";
            this.RBConfidential.UseVisualStyleBackColor = true;
            this.RBConfidential.CheckedChanged += new System.EventHandler(this.RBConfidential_CheckedChanged);
            // 
            // RBInternalUse
            // 
            this.RBInternalUse.AutoSize = true;
            this.RBInternalUse.Location = new System.Drawing.Point(16, 26);
            this.RBInternalUse.Name = "RBInternalUse";
            this.RBInternalUse.Size = new System.Drawing.Size(82, 17);
            this.RBInternalUse.TabIndex = 1;
            this.RBInternalUse.TabStop = true;
            this.RBInternalUse.Text = "Internal Use";
            this.RBInternalUse.UseVisualStyleBackColor = true;
            this.RBInternalUse.CheckedChanged += new System.EventHandler(this.RBInternalUse_CheckedChanged);
            // 
            // RBPublic
            // 
            this.RBPublic.AutoSize = true;
            this.RBPublic.Location = new System.Drawing.Point(16, 3);
            this.RBPublic.Name = "RBPublic";
            this.RBPublic.Size = new System.Drawing.Size(54, 17);
            this.RBPublic.TabIndex = 0;
            this.RBPublic.TabStop = true;
            this.RBPublic.Text = "Public";
            this.RBPublic.UseVisualStyleBackColor = true;
            this.RBPublic.CheckedChanged += new System.EventHandler(this.RBPublic_CheckedChanged);
            // 
            // Done
            // 
            this.Done.Location = new System.Drawing.Point(94, 148);
            this.Done.Name = "Done";
            this.Done.Size = new System.Drawing.Size(75, 31);
            this.Done.TabIndex = 1;
            this.Done.Text = "Done";
            this.Done.UseVisualStyleBackColor = true;
            this.Done.Click += new System.EventHandler(this.Done_Click);
            // 
            // textBox1
            // 
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Location = new System.Drawing.Point(12, 1);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(157, 47);
            this.textBox1.TabIndex = 2;
            this.textBox1.Text = "Operation does not specify an Access Level!\r\nPlease select one below:";
            // 
            // AccessLevelForm
            // 
            this.AcceptButton = this.Done;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(181, 191);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.Done);
            this.Controls.Add(this.AccessLevelGroup);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AccessLevelForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.AccessLevelGroup.ResumeLayout(false);
            this.AccessLevelGroup.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel AccessLevelGroup;
        private System.Windows.Forms.RadioButton RBSecret;
        private System.Windows.Forms.RadioButton RBConfidential;
        private System.Windows.Forms.RadioButton RBInternalUse;
        private System.Windows.Forms.RadioButton RBPublic;
        private System.Windows.Forms.Button Done;
        private System.Windows.Forms.TextBox textBox1;
    }
}