using System.Windows.Forms;

namespace Plugin.Application.Forms
{
    partial class AddOperationInput
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
            this.OperationNamesLbl = new System.Windows.Forms.Label();
            this.CommaSeparatedLbl = new System.Windows.Forms.Label();
            this.OperationNames = new System.Windows.Forms.TextBox();
            this.ErrorLine = new System.Windows.Forms.TextBox();
            this.NewMinorVersion = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(498, 71);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 0;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(417, 71);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 31);
            this.Ok.TabIndex = 1;
            this.Ok.Text = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            // 
            // OperationNamesLbl
            // 
            this.OperationNamesLbl.AutoSize = true;
            this.OperationNamesLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OperationNamesLbl.Location = new System.Drawing.Point(12, 9);
            this.OperationNamesLbl.Name = "OperationNamesLbl";
            this.OperationNamesLbl.Size = new System.Drawing.Size(165, 17);
            this.OperationNamesLbl.TabIndex = 2;
            this.OperationNamesLbl.Text = "List of Operation Names:";
            // 
            // CommaSeparatedLbl
            // 
            this.CommaSeparatedLbl.AutoSize = true;
            this.CommaSeparatedLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.CommaSeparatedLbl.Location = new System.Drawing.Point(12, 26);
            this.CommaSeparatedLbl.Name = "CommaSeparatedLbl";
            this.CommaSeparatedLbl.Size = new System.Drawing.Size(100, 13);
            this.CommaSeparatedLbl.TabIndex = 3;
            this.CommaSeparatedLbl.Text = "(Comma Separated)";
            // 
            // OperationNames
            // 
            this.OperationNames.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.OperationNames.Location = new System.Drawing.Point(184, 9);
            this.OperationNames.Name = "OperationNames";
            this.OperationNames.Size = new System.Drawing.Size(390, 23);
            this.OperationNames.TabIndex = 4;
            this.OperationNames.Leave += new System.EventHandler(this.OperationNames_TextChanged);
            // 
            // ErrorLine
            // 
            this.ErrorLine.BackColor = System.Drawing.SystemColors.Control;
            this.ErrorLine.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ErrorLine.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ErrorLine.ForeColor = System.Drawing.Color.Red;
            this.ErrorLine.Location = new System.Drawing.Point(15, 49);
            this.ErrorLine.Name = "ErrorLine";
            this.ErrorLine.Size = new System.Drawing.Size(558, 16);
            this.ErrorLine.TabIndex = 5;
            // 
            // NewMinorVersion
            // 
            this.NewMinorVersion.AutoSize = true;
            this.NewMinorVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NewMinorVersion.Location = new System.Drawing.Point(15, 77);
            this.NewMinorVersion.Name = "NewMinorVersion";
            this.NewMinorVersion.Size = new System.Drawing.Size(168, 20);
            this.NewMinorVersion.TabIndex = 6;
            this.NewMinorVersion.Text = "Increment minor version";
            this.NewMinorVersion.UseVisualStyleBackColor = true;
            this.NewMinorVersion.CheckedChanged += new System.EventHandler(this.NewMinorVersion_CheckedChanged);
            // 
            // AddOperationInput
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(586, 113);
            this.Controls.Add(this.NewMinorVersion);
            this.Controls.Add(this.ErrorLine);
            this.Controls.Add(this.OperationNames);
            this.Controls.Add(this.CommaSeparatedLbl);
            this.Controls.Add(this.OperationNamesLbl);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.Cancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddOperationInput";
            this.Text = "Add new Service Operation(s)";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.Label OperationNamesLbl;
        private System.Windows.Forms.Label CommaSeparatedLbl;
        private System.Windows.Forms.TextBox OperationNames;
        private System.Windows.Forms.TextBox ErrorLine;
        private CheckBox NewMinorVersion;
    }
}