namespace Plugin.Application.Forms
{
    partial class RESTAuthEditAPIKeys
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RESTAuthEditAPIKeys));
            this.Ok = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.KeyTree = new System.Windows.Forms.TreeView();
            this.DeleteGroup = new System.Windows.Forms.Button();
            this.AddKey = new System.Windows.Forms.Button();
            this.DeleteKey = new System.Windows.Forms.Button();
            this.LocationBox = new System.Windows.Forms.GroupBox();
            this.LocationQuery = new System.Windows.Forms.RadioButton();
            this.LocationHeader = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.KeyName = new System.Windows.Forms.TextBox();
            this.KeyBox = new System.Windows.Forms.GroupBox();
            this.AddGroup = new System.Windows.Forms.Button();
            this.LocationBox.SuspendLayout();
            this.KeyBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(299, 170);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(85, 31);
            this.Ok.TabIndex = 6;
            this.Ok.Text = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(390, 170);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(85, 31);
            this.Cancel.TabIndex = 5;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // KeyTree
            // 
            this.KeyTree.Location = new System.Drawing.Point(208, 12);
            this.KeyTree.Name = "KeyTree";
            this.KeyTree.Size = new System.Drawing.Size(267, 102);
            this.KeyTree.TabIndex = 0;
            this.KeyTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.KeyTree_AfterSelect);
            // 
            // DeleteGroup
            // 
            this.DeleteGroup.Location = new System.Drawing.Point(299, 120);
            this.DeleteGroup.Name = "DeleteGroup";
            this.DeleteGroup.Size = new System.Drawing.Size(85, 25);
            this.DeleteGroup.TabIndex = 3;
            this.DeleteGroup.Text = "Delete Group";
            this.DeleteGroup.UseVisualStyleBackColor = true;
            this.DeleteGroup.Click += new System.EventHandler(this.DeleteGroup_Click);
            // 
            // AddKey
            // 
            this.AddKey.Image = ((System.Drawing.Image)(resources.GetObject("AddKey.Image")));
            this.AddKey.Location = new System.Drawing.Point(159, 57);
            this.AddKey.Name = "AddKey";
            this.AddKey.Size = new System.Drawing.Size(25, 25);
            this.AddKey.TabIndex = 3;
            this.AddKey.UseVisualStyleBackColor = true;
            this.AddKey.Click += new System.EventHandler(this.AddKey_Click);
            // 
            // DeleteKey
            // 
            this.DeleteKey.Location = new System.Drawing.Point(390, 120);
            this.DeleteKey.Name = "DeleteKey";
            this.DeleteKey.Size = new System.Drawing.Size(85, 25);
            this.DeleteKey.TabIndex = 4;
            this.DeleteKey.Text = "DeleteKey";
            this.DeleteKey.UseVisualStyleBackColor = true;
            this.DeleteKey.Click += new System.EventHandler(this.DeleteKey_Click);
            // 
            // LocationBox
            // 
            this.LocationBox.Controls.Add(this.LocationQuery);
            this.LocationBox.Controls.Add(this.LocationHeader);
            this.LocationBox.Location = new System.Drawing.Point(9, 19);
            this.LocationBox.Name = "LocationBox";
            this.LocationBox.Size = new System.Drawing.Size(144, 71);
            this.LocationBox.TabIndex = 1;
            this.LocationBox.TabStop = false;
            this.LocationBox.Text = "Location";
            // 
            // LocationQuery
            // 
            this.LocationQuery.AutoSize = true;
            this.LocationQuery.Location = new System.Drawing.Point(7, 42);
            this.LocationQuery.Name = "LocationQuery";
            this.LocationQuery.Size = new System.Drawing.Size(53, 17);
            this.LocationQuery.TabIndex = 2;
            this.LocationQuery.Tag = "query";
            this.LocationQuery.Text = "Query";
            this.LocationQuery.UseVisualStyleBackColor = true;
            this.LocationQuery.CheckedChanged += new System.EventHandler(this.LocationHeader_CheckedChanged);
            // 
            // LocationHeader
            // 
            this.LocationHeader.AutoSize = true;
            this.LocationHeader.Checked = true;
            this.LocationHeader.Location = new System.Drawing.Point(7, 19);
            this.LocationHeader.Name = "LocationHeader";
            this.LocationHeader.Size = new System.Drawing.Size(60, 17);
            this.LocationHeader.TabIndex = 1;
            this.LocationHeader.TabStop = true;
            this.LocationHeader.Tag = "header";
            this.LocationHeader.Text = "Header";
            this.LocationHeader.UseVisualStyleBackColor = true;
            this.LocationHeader.CheckedChanged += new System.EventHandler(this.LocationHeader_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 99);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name:";
            // 
            // KeyName
            // 
            this.KeyName.Location = new System.Drawing.Point(50, 96);
            this.KeyName.Name = "KeyName";
            this.KeyName.Size = new System.Drawing.Size(103, 20);
            this.KeyName.TabIndex = 2;
            // 
            // KeyBox
            // 
            this.KeyBox.Controls.Add(this.LocationBox);
            this.KeyBox.Controls.Add(this.AddKey);
            this.KeyBox.Controls.Add(this.label1);
            this.KeyBox.Controls.Add(this.KeyName);
            this.KeyBox.Location = new System.Drawing.Point(12, 13);
            this.KeyBox.Name = "KeyBox";
            this.KeyBox.Size = new System.Drawing.Size(190, 132);
            this.KeyBox.TabIndex = 1;
            this.KeyBox.TabStop = false;
            this.KeyBox.Text = "Key";
            // 
            // AddGroup
            // 
            this.AddGroup.Location = new System.Drawing.Point(208, 120);
            this.AddGroup.Name = "AddGroup";
            this.AddGroup.Size = new System.Drawing.Size(85, 25);
            this.AddGroup.TabIndex = 2;
            this.AddGroup.Text = "Add Group";
            this.AddGroup.UseVisualStyleBackColor = true;
            this.AddGroup.Click += new System.EventHandler(this.AddGroup_Click);
            // 
            // RESTAuthEditAPIKeys
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(485, 210);
            this.Controls.Add(this.AddGroup);
            this.Controls.Add(this.DeleteKey);
            this.Controls.Add(this.DeleteGroup);
            this.Controls.Add(this.KeyTree);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.KeyBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RESTAuthEditAPIKeys";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit API Keys";
            this.LocationBox.ResumeLayout(false);
            this.LocationBox.PerformLayout();
            this.KeyBox.ResumeLayout(false);
            this.KeyBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.TreeView KeyTree;
        private System.Windows.Forms.Button DeleteGroup;
        private System.Windows.Forms.Button AddKey;
        private System.Windows.Forms.Button DeleteKey;
        private System.Windows.Forms.GroupBox LocationBox;
        private System.Windows.Forms.RadioButton LocationQuery;
        private System.Windows.Forms.RadioButton LocationHeader;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox KeyName;
        private System.Windows.Forms.GroupBox KeyBox;
        private System.Windows.Forms.Button AddGroup;
    }
}