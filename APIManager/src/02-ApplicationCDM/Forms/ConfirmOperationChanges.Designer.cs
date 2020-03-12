namespace Plugin.Application.Forms
{
    partial class ConfirmOperationChanges
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
            this.Warning = new System.Windows.Forms.PictureBox();
            this.NewMinorVersion = new System.Windows.Forms.CheckBox();
            this.Cancel = new System.Windows.Forms.Button();
            this.OK = new System.Windows.Forms.Button();
            this.Label = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.Warning)).BeginInit();
            this.SuspendLayout();
            // 
            // Warning
            // 
            this.Warning.Location = new System.Drawing.Point(12, 22);
            this.Warning.Name = "Warning";
            this.Warning.Size = new System.Drawing.Size(37, 36);
            this.Warning.TabIndex = 0;
            this.Warning.TabStop = false;
            // 
            // NewMinorVersion
            // 
            this.NewMinorVersion.AutoSize = true;
            this.NewMinorVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NewMinorVersion.Location = new System.Drawing.Point(12, 84);
            this.NewMinorVersion.Name = "NewMinorVersion";
            this.NewMinorVersion.Size = new System.Drawing.Size(168, 20);
            this.NewMinorVersion.TabIndex = 0;
            this.NewMinorVersion.Text = "Increment minor version";
            this.NewMinorVersion.UseVisualStyleBackColor = true;
            this.NewMinorVersion.CheckedChanged += new System.EventHandler(this.NewMinorVersion_CheckedChanged);
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(267, 78);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 1;
            this.Cancel.Text = "No";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // OK
            // 
            this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OK.Location = new System.Drawing.Point(186, 78);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(75, 31);
            this.OK.TabIndex = 2;
            this.OK.Text = "Yes";
            this.OK.UseVisualStyleBackColor = true;
            // 
            // Label
            // 
            this.Label.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Label.CausesValidation = false;
            this.Label.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.Label.Location = new System.Drawing.Point(56, 22);
            this.Label.Multiline = true;
            this.Label.Name = "Label";
            this.Label.ReadOnly = true;
            this.Label.Size = new System.Drawing.Size(287, 50);
            this.Label.TabIndex = 0;
            this.Label.TabStop = false;
            // 
            // ConfirmOperationChanges
            // 
            this.AcceptButton = this.OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(355, 121);
            this.Controls.Add(this.Label);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.NewMinorVersion);
            this.Controls.Add(this.Warning);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfirmOperationChanges";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Confirm Operation Change";
            this.Load += new System.EventHandler(this.ConfirmOperationChanges_Load);
            ((System.ComponentModel.ISupportInitialize)(this.Warning)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox Warning;
        private System.Windows.Forms.CheckBox NewMinorVersion;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.TextBox Label;
    }
}