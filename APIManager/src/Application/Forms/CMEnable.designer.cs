using System.Windows.Forms;

namespace Plugin.Application.Forms
{
    partial class CMEnable
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
            this.ErrorLine = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.ProjectIDFld = new System.Windows.Forms.TextBox();
            this.TicketIDFld = new System.Windows.Forms.TextBox();
            this.TicketBox = new System.Windows.Forms.GroupBox();
            this.VersionBox = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.NewVersionFld = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.ExistingVersionFld = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.TicketBox.SuspendLayout();
            this.VersionBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(299, 115);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 3;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(218, 115);
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
            this.ErrorLine.Location = new System.Drawing.Point(15, 49);
            this.ErrorLine.Name = "ErrorLine";
            this.ErrorLine.Size = new System.Drawing.Size(558, 16);
            this.ErrorLine.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(33, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Ticket ID:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Project number:";
            // 
            // ProjectIDFld
            // 
            this.ProjectIDFld.Location = new System.Drawing.Point(93, 53);
            this.ProjectIDFld.Name = "ProjectIDFld";
            this.ProjectIDFld.Size = new System.Drawing.Size(105, 20);
            this.ProjectIDFld.TabIndex = 2;
            this.ProjectIDFld.Leave += new System.EventHandler(this.Keyfield_Leave);
            // 
            // TicketIDFld
            // 
            this.TicketIDFld.Location = new System.Drawing.Point(93, 22);
            this.TicketIDFld.Name = "TicketIDFld";
            this.TicketIDFld.Size = new System.Drawing.Size(105, 20);
            this.TicketIDFld.TabIndex = 1;
            this.TicketIDFld.Leave += new System.EventHandler(this.Keyfield_Leave);
            // 
            // TicketBox
            // 
            this.TicketBox.Controls.Add(this.label1);
            this.TicketBox.Controls.Add(this.ProjectIDFld);
            this.TicketBox.Controls.Add(this.TicketIDFld);
            this.TicketBox.Controls.Add(this.label2);
            this.TicketBox.Location = new System.Drawing.Point(12, 12);
            this.TicketBox.Name = "TicketBox";
            this.TicketBox.Size = new System.Drawing.Size(210, 97);
            this.TicketBox.TabIndex = 1;
            this.TicketBox.TabStop = false;
            this.TicketBox.Text = "Administration";
            // 
            // VersionBox
            // 
            this.VersionBox.Controls.Add(this.label5);
            this.VersionBox.Controls.Add(this.NewVersionFld);
            this.VersionBox.Controls.Add(this.label4);
            this.VersionBox.Controls.Add(this.ExistingVersionFld);
            this.VersionBox.Controls.Add(this.label3);
            this.VersionBox.Location = new System.Drawing.Point(228, 12);
            this.VersionBox.Name = "VersionBox";
            this.VersionBox.Size = new System.Drawing.Size(146, 97);
            this.VersionBox.TabIndex = 2;
            this.VersionBox.TabStop = false;
            this.VersionBox.Text = "Version information";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 76);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(122, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Format: <major>.<minor>";
            // 
            // NewVersionFld
            // 
            this.NewVersionFld.Location = new System.Drawing.Point(66, 53);
            this.NewVersionFld.Name = "NewVersionFld";
            this.NewVersionFld.Size = new System.Drawing.Size(67, 20);
            this.NewVersionFld.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(28, 56);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "New:";
            // 
            // ExistingVersionFld
            // 
            this.ExistingVersionFld.Location = new System.Drawing.Point(66, 22);
            this.ExistingVersionFld.Name = "ExistingVersionFld";
            this.ExistingVersionFld.ReadOnly = true;
            this.ExistingVersionFld.Size = new System.Drawing.Size(67, 20);
            this.ExistingVersionFld.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Existing:";
            // 
            // EnableCM
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(387, 158);
            this.Controls.Add(this.VersionBox);
            this.Controls.Add(this.TicketBox);
            this.Controls.Add(this.ErrorLine);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.Cancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EnableCM";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Enable Configuration Management";
            this.TicketBox.ResumeLayout(false);
            this.TicketBox.PerformLayout();
            this.VersionBox.ResumeLayout(false);
            this.VersionBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.TextBox ErrorLine;
        private Label label1;
        private Label label2;
        private TextBox ProjectIDFld;
        private TextBox TicketIDFld;
        private GroupBox TicketBox;
        private GroupBox VersionBox;
        private Label label5;
        private TextBox NewVersionFld;
        private Label label4;
        private TextBox ExistingVersionFld;
        private Label label3;
    }
}