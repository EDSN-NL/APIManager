using System.Windows.Forms;

namespace Plugin.Application.Forms
{
    partial class CreateRESTServiceDeclaration
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreateRESTServiceDeclaration));
            this.PackageDeclarationLbl = new System.Windows.Forms.Label();
            this.APINameFld = new System.Windows.Forms.TextBox();
            this.Cancel = new System.Windows.Forms.Button();
            this.Ok = new System.Windows.Forms.Button();
            this.ResourcesBox = new System.Windows.Forms.GroupBox();
            this.ResourceList = new System.Windows.Forms.ListView();
            this.ResourceName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ResourceType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.EditResource = new System.Windows.Forms.Button();
            this.DeleteResource = new System.Windows.Forms.Button();
            this.AddResource = new System.Windows.Forms.Button();
            this.ResourceMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ContactDetailsBox = new System.Windows.Forms.GroupBox();
            this.ContactURLFld = new System.Windows.Forms.TextBox();
            this.ContactEMailFld = new System.Windows.Forms.TextBox();
            this.ContactNameFld = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.APIDescriptionFld = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.APITermsOfServiceFld = new System.Windows.Forms.TextBox();
            this.LicenseBox = new System.Windows.Forms.GroupBox();
            this.LicenseURLFld = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.LicenseNameFld = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.TicketBox = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.ProjectIDFld = new System.Windows.Forms.TextBox();
            this.TicketIDFld = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.APINamingBox = new System.Windows.Forms.GroupBox();
            this.label10 = new System.Windows.Forms.Label();
            this.ErrorLine = new System.Windows.Forms.TextBox();
            this.OperationalState = new System.Windows.Forms.ComboBox();
            this.label11 = new System.Windows.Forms.Label();
            this.ResourcesBox.SuspendLayout();
            this.ResourceMenuStrip.SuspendLayout();
            this.ContactDetailsBox.SuspendLayout();
            this.LicenseBox.SuspendLayout();
            this.TicketBox.SuspendLayout();
            this.APINamingBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // PackageDeclarationLbl
            // 
            this.PackageDeclarationLbl.AutoSize = true;
            this.PackageDeclarationLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PackageDeclarationLbl.Location = new System.Drawing.Point(24, 25);
            this.PackageDeclarationLbl.Name = "PackageDeclarationLbl";
            this.PackageDeclarationLbl.Size = new System.Drawing.Size(56, 13);
            this.PackageDeclarationLbl.TabIndex = 0;
            this.PackageDeclarationLbl.Text = "API name:";
            // 
            // APINameFld
            // 
            this.APINameFld.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.APINameFld.Location = new System.Drawing.Point(86, 20);
            this.APINameFld.Name = "APINameFld";
            this.APINameFld.Size = new System.Drawing.Size(235, 23);
            this.APINameFld.TabIndex = 1;
            this.APINameFld.Leave += new System.EventHandler(this.APIName_TextChanged);
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(565, 432);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 6;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(484, 432);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 31);
            this.Ok.TabIndex = 7;
            this.Ok.Text = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            // 
            // ResourcesBox
            // 
            this.ResourcesBox.Controls.Add(this.ResourceList);
            this.ResourcesBox.Controls.Add(this.EditResource);
            this.ResourcesBox.Controls.Add(this.DeleteResource);
            this.ResourcesBox.Controls.Add(this.AddResource);
            this.ResourcesBox.Location = new System.Drawing.Point(357, 9);
            this.ResourcesBox.Name = "ResourcesBox";
            this.ResourcesBox.Size = new System.Drawing.Size(283, 299);
            this.ResourcesBox.TabIndex = 4;
            this.ResourcesBox.TabStop = false;
            this.ResourcesBox.Text = "Associated Resources";
            // 
            // ResourceList
            // 
            this.ResourceList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ResourceName,
            this.ResourceType});
            this.ResourceList.FullRowSelect = true;
            this.ResourceList.GridLines = true;
            this.ResourceList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.ResourceList.Location = new System.Drawing.Point(9, 20);
            this.ResourceList.Name = "ResourceList";
            this.ResourceList.Size = new System.Drawing.Size(263, 233);
            this.ResourceList.TabIndex = 0;
            this.ResourceList.UseCompatibleStateImageBehavior = false;
            this.ResourceList.View = System.Windows.Forms.View.Details;
            // 
            // ResourceName
            // 
            this.ResourceName.Text = "Name";
            this.ResourceName.Width = 162;
            // 
            // ResourceType
            // 
            this.ResourceType.Text = "Type";
            this.ResourceType.Width = 93;
            // 
            // EditResource
            // 
            this.EditResource.Image = ((System.Drawing.Image)(resources.GetObject("EditResource.Image")));
            this.EditResource.Location = new System.Drawing.Point(71, 262);
            this.EditResource.Name = "EditResource";
            this.EditResource.Size = new System.Drawing.Size(25, 25);
            this.EditResource.TabIndex = 3;
            this.EditResource.UseVisualStyleBackColor = true;
            this.EditResource.Click += new System.EventHandler(this.EditResource_Click);
            // 
            // DeleteResource
            // 
            this.DeleteResource.Image = ((System.Drawing.Image)(resources.GetObject("DeleteResource.Image")));
            this.DeleteResource.Location = new System.Drawing.Point(40, 262);
            this.DeleteResource.Name = "DeleteResource";
            this.DeleteResource.Size = new System.Drawing.Size(25, 25);
            this.DeleteResource.TabIndex = 2;
            this.DeleteResource.UseVisualStyleBackColor = true;
            this.DeleteResource.Click += new System.EventHandler(this.DeleteResource_Click);
            // 
            // AddResource
            // 
            this.AddResource.Image = ((System.Drawing.Image)(resources.GetObject("AddResource.Image")));
            this.AddResource.Location = new System.Drawing.Point(9, 262);
            this.AddResource.Name = "AddResource";
            this.AddResource.Size = new System.Drawing.Size(25, 25);
            this.AddResource.TabIndex = 1;
            this.AddResource.UseVisualStyleBackColor = true;
            this.AddResource.Click += new System.EventHandler(this.AddResource_Click);
            // 
            // ResourceMenuStrip
            // 
            this.ResourceMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem,
            this.editToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.ResourceMenuStrip.Name = "ResourceMenuStrip";
            this.ResourceMenuStrip.Size = new System.Drawing.Size(108, 70);
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.addToolStripMenuItem.Text = "Add";
            this.addToolStripMenuItem.Click += new System.EventHandler(this.AddResource_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.editToolStripMenuItem.Text = "Delete";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.DeleteResource_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.deleteToolStripMenuItem.Text = "Edit";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.EditResource_Click);
            // 
            // ContactDetailsBox
            // 
            this.ContactDetailsBox.Controls.Add(this.ContactURLFld);
            this.ContactDetailsBox.Controls.Add(this.ContactEMailFld);
            this.ContactDetailsBox.Controls.Add(this.ContactNameFld);
            this.ContactDetailsBox.Controls.Add(this.label5);
            this.ContactDetailsBox.Controls.Add(this.label4);
            this.ContactDetailsBox.Controls.Add(this.label3);
            this.ContactDetailsBox.Location = new System.Drawing.Point(357, 314);
            this.ContactDetailsBox.Name = "ContactDetailsBox";
            this.ContactDetailsBox.Size = new System.Drawing.Size(283, 112);
            this.ContactDetailsBox.TabIndex = 5;
            this.ContactDetailsBox.TabStop = false;
            this.ContactDetailsBox.Text = "Contact details";
            // 
            // ContactURLFld
            // 
            this.ContactURLFld.Location = new System.Drawing.Point(50, 78);
            this.ContactURLFld.Name = "ContactURLFld";
            this.ContactURLFld.Size = new System.Drawing.Size(222, 20);
            this.ContactURLFld.TabIndex = 3;
            // 
            // ContactEMailFld
            // 
            this.ContactEMailFld.Location = new System.Drawing.Point(50, 49);
            this.ContactEMailFld.Name = "ContactEMailFld";
            this.ContactEMailFld.Size = new System.Drawing.Size(222, 20);
            this.ContactEMailFld.TabIndex = 2;
            // 
            // ContactNameFld
            // 
            this.ContactNameFld.Location = new System.Drawing.Point(50, 23);
            this.ContactNameFld.Name = "ContactNameFld";
            this.ContactNameFld.Size = new System.Drawing.Size(222, 20);
            this.ContactNameFld.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 52);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(36, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "EMail:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 81);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "URL:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 26);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Name:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 52);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Description:";
            // 
            // APIDescriptionFld
            // 
            this.APIDescriptionFld.Location = new System.Drawing.Point(86, 49);
            this.APIDescriptionFld.Multiline = true;
            this.APIDescriptionFld.Name = "APIDescriptionFld";
            this.APIDescriptionFld.Size = new System.Drawing.Size(235, 60);
            this.APIDescriptionFld.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 115);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Terms";
            // 
            // APITermsOfServiceFld
            // 
            this.APITermsOfServiceFld.Location = new System.Drawing.Point(86, 115);
            this.APITermsOfServiceFld.Multiline = true;
            this.APITermsOfServiceFld.Name = "APITermsOfServiceFld";
            this.APITermsOfServiceFld.Size = new System.Drawing.Size(235, 60);
            this.APITermsOfServiceFld.TabIndex = 3;
            // 
            // LicenseBox
            // 
            this.LicenseBox.Controls.Add(this.LicenseURLFld);
            this.LicenseBox.Controls.Add(this.label7);
            this.LicenseBox.Controls.Add(this.LicenseNameFld);
            this.LicenseBox.Controls.Add(this.label6);
            this.LicenseBox.Location = new System.Drawing.Point(12, 314);
            this.LicenseBox.Name = "LicenseBox";
            this.LicenseBox.Size = new System.Drawing.Size(334, 112);
            this.LicenseBox.TabIndex = 3;
            this.LicenseBox.TabStop = false;
            this.LicenseBox.Text = "Licensing info";
            // 
            // LicenseURLFld
            // 
            this.LicenseURLFld.Location = new System.Drawing.Point(51, 78);
            this.LicenseURLFld.Name = "LicenseURLFld";
            this.LicenseURLFld.Size = new System.Drawing.Size(270, 20);
            this.LicenseURLFld.TabIndex = 2;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 81);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(32, 13);
            this.label7.TabIndex = 2;
            this.label7.Text = "URL:";
            // 
            // LicenseNameFld
            // 
            this.LicenseNameFld.Location = new System.Drawing.Point(50, 22);
            this.LicenseNameFld.Multiline = true;
            this.LicenseNameFld.Name = "LicenseNameFld";
            this.LicenseNameFld.Size = new System.Drawing.Size(271, 47);
            this.LicenseNameFld.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 22);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(38, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "Name:";
            // 
            // TicketBox
            // 
            this.TicketBox.Controls.Add(this.label11);
            this.TicketBox.Controls.Add(this.OperationalState);
            this.TicketBox.Controls.Add(this.label8);
            this.TicketBox.Controls.Add(this.ProjectIDFld);
            this.TicketBox.Controls.Add(this.TicketIDFld);
            this.TicketBox.Controls.Add(this.label9);
            this.TicketBox.Location = new System.Drawing.Point(12, 9);
            this.TicketBox.Name = "TicketBox";
            this.TicketBox.Size = new System.Drawing.Size(334, 106);
            this.TicketBox.TabIndex = 1;
            this.TicketBox.TabStop = false;
            this.TicketBox.Text = "Administration";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(26, 25);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(54, 13);
            this.label8.TabIndex = 0;
            this.label8.Text = "Ticket ID:";
            // 
            // ProjectIDFld
            // 
            this.ProjectIDFld.Location = new System.Drawing.Point(86, 48);
            this.ProjectIDFld.Name = "ProjectIDFld";
            this.ProjectIDFld.Size = new System.Drawing.Size(235, 20);
            this.ProjectIDFld.TabIndex = 2;
            this.ProjectIDFld.Leave += new System.EventHandler(this.ProjectIDFld_Leave);
            // 
            // TicketIDFld
            // 
            this.TicketIDFld.Location = new System.Drawing.Point(86, 22);
            this.TicketIDFld.Name = "TicketIDFld";
            this.TicketIDFld.Size = new System.Drawing.Size(235, 20);
            this.TicketIDFld.TabIndex = 1;
            this.TicketIDFld.Leave += new System.EventHandler(this.TicketIDFld_Leave);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(25, 51);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(55, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "Project nr:";
            // 
            // APINamingBox
            // 
            this.APINamingBox.Controls.Add(this.label10);
            this.APINamingBox.Controls.Add(this.APITermsOfServiceFld);
            this.APINamingBox.Controls.Add(this.label2);
            this.APINamingBox.Controls.Add(this.APIDescriptionFld);
            this.APINamingBox.Controls.Add(this.label1);
            this.APINamingBox.Controls.Add(this.APINameFld);
            this.APINamingBox.Controls.Add(this.PackageDeclarationLbl);
            this.APINamingBox.Location = new System.Drawing.Point(12, 121);
            this.APINamingBox.Name = "APINamingBox";
            this.APINamingBox.Size = new System.Drawing.Size(334, 187);
            this.APINamingBox.TabIndex = 2;
            this.APINamingBox.TabStop = false;
            this.APINamingBox.Text = "API Naming";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(24, 128);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(56, 13);
            this.label10.TabIndex = 0;
            this.label10.Text = "of service:";
            // 
            // ErrorLine
            // 
            this.ErrorLine.BackColor = System.Drawing.SystemColors.Control;
            this.ErrorLine.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ErrorLine.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ErrorLine.ForeColor = System.Drawing.Color.Red;
            this.ErrorLine.Location = new System.Drawing.Point(12, 432);
            this.ErrorLine.Name = "ErrorLine";
            this.ErrorLine.Size = new System.Drawing.Size(466, 13);
            this.ErrorLine.TabIndex = 0;
            // 
            // OperationalState
            // 
            this.OperationalState.FormattingEnabled = true;
            this.OperationalState.Location = new System.Drawing.Point(156, 74);
            this.OperationalState.Name = "OperationalState";
            this.OperationalState.Size = new System.Drawing.Size(165, 21);
            this.OperationalState.TabIndex = 3;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(31, 77);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(119, 13);
            this.label11.TabIndex = 0;
            this.label11.Text = "Initial Operational State:";
            // 
            // CreateRESTServiceDeclaration
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(652, 473);
            this.Controls.Add(this.ErrorLine);
            this.Controls.Add(this.APINamingBox);
            this.Controls.Add(this.TicketBox);
            this.Controls.Add(this.LicenseBox);
            this.Controls.Add(this.ContactDetailsBox);
            this.Controls.Add(this.ResourcesBox);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.Cancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CreateRESTServiceDeclaration";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create a new API Declaration";
            this.ResourcesBox.ResumeLayout(false);
            this.ResourceMenuStrip.ResumeLayout(false);
            this.ContactDetailsBox.ResumeLayout(false);
            this.ContactDetailsBox.PerformLayout();
            this.LicenseBox.ResumeLayout(false);
            this.LicenseBox.PerformLayout();
            this.TicketBox.ResumeLayout(false);
            this.TicketBox.PerformLayout();
            this.APINamingBox.ResumeLayout(false);
            this.APINamingBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label PackageDeclarationLbl;
        private System.Windows.Forms.TextBox APINameFld;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button Ok;
        private GroupBox ResourcesBox;
        private ListView ResourceList;
        private ColumnHeader ResourceName;
        private ColumnHeader ResourceType;
        private Button EditResource;
        private Button DeleteResource;
        private Button AddResource;
        private ContextMenuStrip ResourceMenuStrip;
        private ToolStripMenuItem editToolStripMenuItem;
        private ToolStripMenuItem deleteToolStripMenuItem;
        private GroupBox ContactDetailsBox;
        private Label label1;
        private TextBox APIDescriptionFld;
        private Label label2;
        private TextBox APITermsOfServiceFld;
        private TextBox ContactURLFld;
        private TextBox ContactEMailFld;
        private TextBox ContactNameFld;
        private Label label5;
        private Label label4;
        private Label label3;
        private GroupBox LicenseBox;
        private TextBox LicenseURLFld;
        private Label label7;
        private TextBox LicenseNameFld;
        private Label label6;
        private GroupBox TicketBox;
        private Label label8;
        private TextBox ProjectIDFld;
        private TextBox TicketIDFld;
        private Label label9;
        private GroupBox APINamingBox;
        private Label label10;
        private ToolStripMenuItem addToolStripMenuItem;
        private TextBox ErrorLine;
        private ComboBox OperationalState;
        private Label label11;
    }
}