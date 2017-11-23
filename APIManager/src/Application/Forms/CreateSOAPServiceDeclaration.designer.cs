using System.Windows.Forms;

namespace Plugin.Application.Forms
{
    partial class CreateSOAPServiceDeclaration
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
            this.PackageDeclarationLbl = new System.Windows.Forms.Label();
            this.PackageNameFld = new System.Windows.Forms.TextBox();
            this.OperationNamesLbl = new System.Windows.Forms.Label();
            this.CommaSeparatedLbl = new System.Windows.Forms.Label();
            this.OperationNames = new System.Windows.Forms.TextBox();
            this.Cancel = new System.Windows.Forms.Button();
            this.Ok = new System.Windows.Forms.Button();
            this.ErrorLine = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // PackageDeclarationLbl
            // 
            this.PackageDeclarationLbl.AutoSize = true;
            this.PackageDeclarationLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PackageDeclarationLbl.Location = new System.Drawing.Point(2, 12);
            this.PackageDeclarationLbl.Name = "PackageDeclarationLbl";
            this.PackageDeclarationLbl.Size = new System.Drawing.Size(235, 17);
            this.PackageDeclarationLbl.TabIndex = 0;
            this.PackageDeclarationLbl.Text = "Service Declaration Package Name:";
            // 
            // PackageNameFld
            // 
            this.PackageNameFld.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PackageNameFld.Location = new System.Drawing.Point(243, 9);
            this.PackageNameFld.Name = "PackageNameFld";
            this.PackageNameFld.Size = new System.Drawing.Size(331, 23);
            this.PackageNameFld.TabIndex = 1;
            this.PackageNameFld.Leave += new System.EventHandler(this.PackageName_TextChanged);
            // 
            // OperationNamesLbl
            // 
            this.OperationNamesLbl.AutoSize = true;
            this.OperationNamesLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OperationNamesLbl.Location = new System.Drawing.Point(72, 42);
            this.OperationNamesLbl.Name = "OperationNamesLbl";
            this.OperationNamesLbl.Size = new System.Drawing.Size(165, 17);
            this.OperationNamesLbl.TabIndex = 0;
            this.OperationNamesLbl.Text = "List of Operation Names:";
            // 
            // CommaSeparatedLbl
            // 
            this.CommaSeparatedLbl.AutoSize = true;
            this.CommaSeparatedLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CommaSeparatedLbl.Location = new System.Drawing.Point(72, 59);
            this.CommaSeparatedLbl.Name = "CommaSeparatedLbl";
            this.CommaSeparatedLbl.Size = new System.Drawing.Size(97, 13);
            this.CommaSeparatedLbl.TabIndex = 0;
            this.CommaSeparatedLbl.Text = "(comma separated)";
            // 
            // OperationNames
            // 
            this.OperationNames.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OperationNames.Location = new System.Drawing.Point(243, 40);
            this.OperationNames.Name = "OperationNames";
            this.OperationNames.Size = new System.Drawing.Size(331, 23);
            this.OperationNames.TabIndex = 2;
            this.OperationNames.Leave += new System.EventHandler(this.OperationNames_TextChanged);
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(499, 119);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 3;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(418, 119);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 31);
            this.Ok.TabIndex = 4;
            this.Ok.Text = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            // 
            // ErrorLine
            // 
            this.ErrorLine.BackColor = System.Drawing.SystemColors.Control;
            this.ErrorLine.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ErrorLine.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ErrorLine.ForeColor = System.Drawing.Color.Red;
            this.ErrorLine.Location = new System.Drawing.Point(12, 83);
            this.ErrorLine.Name = "ErrorLine";
            this.ErrorLine.Size = new System.Drawing.Size(566, 16);
            this.ErrorLine.TabIndex = 7;
            // 
            // CreateSOAPServiceDeclaration
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(586, 162);
            this.Controls.Add(this.ErrorLine);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.OperationNames);
            this.Controls.Add(this.CommaSeparatedLbl);
            this.Controls.Add(this.OperationNamesLbl);
            this.Controls.Add(this.PackageNameFld);
            this.Controls.Add(this.PackageDeclarationLbl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CreateSOAPServiceDeclaration";
            this.Text = "Create a new Service Declaration";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label PackageDeclarationLbl;
        private System.Windows.Forms.TextBox PackageNameFld;
        private System.Windows.Forms.Label OperationNamesLbl;
        private System.Windows.Forms.Label CommaSeparatedLbl;
        private System.Windows.Forms.TextBox OperationNames;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.TextBox ErrorLine;
    }
}