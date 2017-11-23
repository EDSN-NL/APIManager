using System.Windows.Forms;

namespace Plugin.Application.Forms
{
    partial class AddCodeListInput
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
            this.CodeListNamesLbl = new System.Windows.Forms.Label();
            this.CommaSeparatedLbl = new System.Windows.Forms.Label();
            this.CodeListNames = new System.Windows.Forms.TextBox();
            this.ErrorLine = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(499, 119);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 0;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(418, 119);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 31);
            this.Ok.TabIndex = 1;
            this.Ok.Text = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            // 
            // CodeListNamesLbl
            // 
            this.CodeListNamesLbl.AutoSize = true;
            this.CodeListNamesLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CodeListNamesLbl.Location = new System.Drawing.Point(12, 9);
            this.CodeListNamesLbl.Name = "CodeListNamesLbl";
            this.CodeListNamesLbl.Size = new System.Drawing.Size(159, 17);
            this.CodeListNamesLbl.TabIndex = 2;
            this.CodeListNamesLbl.Text = "List of Code List names:";
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
            // CodeListNames
            // 
            this.CodeListNames.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.CodeListNames.Location = new System.Drawing.Point(184, 9);
            this.CodeListNames.Name = "CodeListNames";
            this.CodeListNames.Size = new System.Drawing.Size(390, 23);
            this.CodeListNames.TabIndex = 4;
            this.CodeListNames.Leave += new System.EventHandler(this.CodeListNames_TextChanged);
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
            // AddCodeListInput
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(586, 162);
            this.Controls.Add(this.ErrorLine);
            this.Controls.Add(this.CodeListNames);
            this.Controls.Add(this.CommaSeparatedLbl);
            this.Controls.Add(this.CodeListNamesLbl);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.Cancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddCodeListInput";
            this.Text = "Add new Code List(s)";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.Label CodeListNamesLbl;
        private System.Windows.Forms.Label CommaSeparatedLbl;
        private System.Windows.Forms.TextBox CodeListNames;
        private System.Windows.Forms.TextBox ErrorLine;
    }
}