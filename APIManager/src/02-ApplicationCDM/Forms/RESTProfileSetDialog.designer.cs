using System.Windows.Forms;

namespace Plugin.Application.Forms
{
    partial class RESTProfileSetDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RESTProfileSetDialog));
            this.Cancel = new System.Windows.Forms.Button();
            this.Ok = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.ProfileSetName = new System.Windows.Forms.TextBox();
            this.SetElementsGroup = new System.Windows.Forms.GroupBox();
            this.DeleteProfile = new System.Windows.Forms.Button();
            this.AddProfile = new System.Windows.Forms.Button();
            this.ProfileList = new System.Windows.Forms.ListView();
            this.ProfileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ClassName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ClassListMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SetElementsGroup.SuspendLayout();
            this.ClassListMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(243, 279);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 3;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(162, 279);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 31);
            this.Ok.TabIndex = 4;
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
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Name:";
            // 
            // ProfileSetName
            // 
            this.ProfileSetName.Location = new System.Drawing.Point(54, 6);
            this.ProfileSetName.Name = "ProfileSetName";
            this.ProfileSetName.Size = new System.Drawing.Size(262, 20);
            this.ProfileSetName.TabIndex = 1;
            this.ProfileSetName.Leave += new System.EventHandler(this.ProfileNameFld_Leave);
            // 
            // SetElementsGroup
            // 
            this.SetElementsGroup.Controls.Add(this.DeleteProfile);
            this.SetElementsGroup.Controls.Add(this.AddProfile);
            this.SetElementsGroup.Controls.Add(this.ProfileList);
            this.SetElementsGroup.Location = new System.Drawing.Point(12, 32);
            this.SetElementsGroup.Name = "SetElementsGroup";
            this.SetElementsGroup.Size = new System.Drawing.Size(304, 241);
            this.SetElementsGroup.TabIndex = 1;
            this.SetElementsGroup.TabStop = false;
            this.SetElementsGroup.Text = "Associated Classes";
            // 
            // DeleteProfile
            // 
            this.DeleteProfile.Image = ((System.Drawing.Image)(resources.GetObject("DeleteProfile.Image")));
            this.DeleteProfile.Location = new System.Drawing.Point(42, 210);
            this.DeleteProfile.Name = "DeleteProfile";
            this.DeleteProfile.Size = new System.Drawing.Size(25, 25);
            this.DeleteProfile.TabIndex = 2;
            this.DeleteProfile.UseVisualStyleBackColor = true;
            this.DeleteProfile.Click += new System.EventHandler(this.DeleteProfile_Click);
            // 
            // AddProfile
            // 
            this.AddProfile.Image = ((System.Drawing.Image)(resources.GetObject("AddProfile.Image")));
            this.AddProfile.Location = new System.Drawing.Point(11, 210);
            this.AddProfile.Name = "AddProfile";
            this.AddProfile.Size = new System.Drawing.Size(25, 25);
            this.AddProfile.TabIndex = 1;
            this.AddProfile.UseVisualStyleBackColor = true;
            this.AddProfile.Click += new System.EventHandler(this.AddProfile_Click);
            // 
            // ProfileList
            // 
            this.ProfileList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ProfileName,
            this.ClassName});
            this.ProfileList.FullRowSelect = true;
            this.ProfileList.GridLines = true;
            this.ProfileList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.ProfileList.HideSelection = false;
            this.ProfileList.Location = new System.Drawing.Point(11, 19);
            this.ProfileList.MultiSelect = false;
            this.ProfileList.Name = "ProfileList";
            this.ProfileList.Size = new System.Drawing.Size(278, 185);
            this.ProfileList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.ProfileList.TabIndex = 0;
            this.ProfileList.UseCompatibleStateImageBehavior = false;
            this.ProfileList.View = System.Windows.Forms.View.Details;
            // 
            // ProfileName
            // 
            this.ProfileName.Text = "Profile";
            this.ProfileName.Width = 139;
            // 
            // ClassName
            // 
            this.ClassName.Text = "Root class";
            this.ClassName.Width = 139;
            // 
            // ClassListMenuStrip
            // 
            this.ClassListMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.ClassListMenuStrip.Name = "ClassListMenuStrip";
            this.ClassListMenuStrip.Size = new System.Drawing.Size(108, 48);
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.addToolStripMenuItem.Text = "Add";
            this.addToolStripMenuItem.Click += new System.EventHandler(this.AddProfile_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.DeleteProfile_Click);
            // 
            // RESTProfileSetDialog
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(328, 319);
            this.Controls.Add(this.SetElementsGroup);
            this.Controls.Add(this.ProfileSetName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.Cancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RESTProfileSetDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Profile Set";
            this.SetElementsGroup.ResumeLayout(false);
            this.ClassListMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button Ok;
        private Label label1;
        private Label label2;
        private TextBox ProfileSetName;
        private GroupBox SetElementsGroup;
        private Button DeleteProfile;
        private Button AddProfile;
        private ListView ProfileList;
        private ColumnHeader ProfileName;
        private ColumnHeader ClassName;
        private ContextMenuStrip ClassListMenuStrip;
        private ToolStripMenuItem addToolStripMenuItem;
        private ToolStripMenuItem deleteToolStripMenuItem;
    }
}