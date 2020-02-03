namespace Plugin.Application.Forms
{
    partial class RESTHeaderParameterDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RESTHeaderParameterDialog));
            this.Ok = new System.Windows.Forms.Button();
            this.APIHeaderGroup = new System.Windows.Forms.GroupBox();
            this.UseHeaderCollection = new System.Windows.Forms.Button();
            this.EditHeaderCollections = new System.Windows.Forms.Button();
            this.EditAPIHeaderParm = new System.Windows.Forms.Button();
            this.DeleteAPIHeaderParm = new System.Windows.Forms.Button();
            this.AddAPIHeaderParm = new System.Windows.Forms.Button();
            this.APIHeaderList = new System.Windows.Forms.ListView();
            this.APIHdrName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.APIHdrDescription = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.APIParametersMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.useCollectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageCollectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SelectedHeaderGroup = new System.Windows.Forms.GroupBox();
            this.SelectedHeaderList = new System.Windows.Forms.ListView();
            this.SelectedHdrName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SelectedHdrDescription = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.UnselectParameters = new System.Windows.Forms.Button();
            this.SelectParameters = new System.Windows.Forms.Button();
            this.APIHeaderGroup.SuspendLayout();
            this.APIParametersMenuStrip.SuspendLayout();
            this.SelectedHeaderGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(578, 315);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 31);
            this.Ok.TabIndex = 4;
            this.Ok.Text = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            // 
            // APIHeaderGroup
            // 
            this.APIHeaderGroup.Controls.Add(this.UseHeaderCollection);
            this.APIHeaderGroup.Controls.Add(this.EditHeaderCollections);
            this.APIHeaderGroup.Controls.Add(this.EditAPIHeaderParm);
            this.APIHeaderGroup.Controls.Add(this.DeleteAPIHeaderParm);
            this.APIHeaderGroup.Controls.Add(this.AddAPIHeaderParm);
            this.APIHeaderGroup.Controls.Add(this.APIHeaderList);
            this.APIHeaderGroup.Location = new System.Drawing.Point(12, 12);
            this.APIHeaderGroup.Name = "APIHeaderGroup";
            this.APIHeaderGroup.Size = new System.Drawing.Size(284, 297);
            this.APIHeaderGroup.TabIndex = 1;
            this.APIHeaderGroup.TabStop = false;
            this.APIHeaderGroup.Text = "Available header Parameters";
            // 
            // UseHeaderCollection
            // 
            this.UseHeaderCollection.Image = ((System.Drawing.Image)(resources.GetObject("UseHeaderCollection.Image")));
            this.UseHeaderCollection.Location = new System.Drawing.Point(105, 266);
            this.UseHeaderCollection.Name = "UseHeaderCollection";
            this.UseHeaderCollection.Size = new System.Drawing.Size(25, 25);
            this.UseHeaderCollection.TabIndex = 4;
            this.UseHeaderCollection.UseVisualStyleBackColor = true;
            this.UseHeaderCollection.Click += new System.EventHandler(this.UseHeaderCollection_Click);
            // 
            // EditHeaderCollections
            // 
            this.EditHeaderCollections.Image = ((System.Drawing.Image)(resources.GetObject("EditHeaderCollections.Image")));
            this.EditHeaderCollections.Location = new System.Drawing.Point(136, 266);
            this.EditHeaderCollections.Name = "EditHeaderCollections";
            this.EditHeaderCollections.Size = new System.Drawing.Size(25, 25);
            this.EditHeaderCollections.TabIndex = 5;
            this.EditHeaderCollections.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.EditHeaderCollections.UseVisualStyleBackColor = true;
            this.EditHeaderCollections.Click += new System.EventHandler(this.EditHeaderCollections_Click);
            // 
            // EditAPIHeaderParm
            // 
            this.EditAPIHeaderParm.Image = ((System.Drawing.Image)(resources.GetObject("EditAPIHeaderParm.Image")));
            this.EditAPIHeaderParm.Location = new System.Drawing.Point(74, 266);
            this.EditAPIHeaderParm.Name = "EditAPIHeaderParm";
            this.EditAPIHeaderParm.Size = new System.Drawing.Size(25, 25);
            this.EditAPIHeaderParm.TabIndex = 3;
            this.EditAPIHeaderParm.UseVisualStyleBackColor = true;
            this.EditAPIHeaderParm.Click += new System.EventHandler(this.EditAPIHeaderParm_Click);
            // 
            // DeleteAPIHeaderParm
            // 
            this.DeleteAPIHeaderParm.Image = ((System.Drawing.Image)(resources.GetObject("DeleteAPIHeaderParm.Image")));
            this.DeleteAPIHeaderParm.Location = new System.Drawing.Point(43, 266);
            this.DeleteAPIHeaderParm.Name = "DeleteAPIHeaderParm";
            this.DeleteAPIHeaderParm.Size = new System.Drawing.Size(25, 25);
            this.DeleteAPIHeaderParm.TabIndex = 2;
            this.DeleteAPIHeaderParm.UseVisualStyleBackColor = true;
            this.DeleteAPIHeaderParm.Click += new System.EventHandler(this.DeleteAPIHeaderParm_Click);
            // 
            // AddAPIHeaderParm
            // 
            this.AddAPIHeaderParm.Image = ((System.Drawing.Image)(resources.GetObject("AddAPIHeaderParm.Image")));
            this.AddAPIHeaderParm.Location = new System.Drawing.Point(12, 266);
            this.AddAPIHeaderParm.Name = "AddAPIHeaderParm";
            this.AddAPIHeaderParm.Size = new System.Drawing.Size(25, 25);
            this.AddAPIHeaderParm.TabIndex = 1;
            this.AddAPIHeaderParm.UseVisualStyleBackColor = true;
            this.AddAPIHeaderParm.Click += new System.EventHandler(this.AddAPIHeaderParm_Click);
            // 
            // APIHeaderList
            // 
            this.APIHeaderList.BackColor = System.Drawing.Color.GhostWhite;
            this.APIHeaderList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.APIHdrName,
            this.APIHdrDescription});
            this.APIHeaderList.ContextMenuStrip = this.APIParametersMenuStrip;
            this.APIHeaderList.FullRowSelect = true;
            this.APIHeaderList.GridLines = true;
            this.APIHeaderList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.APIHeaderList.HideSelection = false;
            this.APIHeaderList.Location = new System.Drawing.Point(12, 19);
            this.APIHeaderList.Name = "APIHeaderList";
            this.APIHeaderList.Size = new System.Drawing.Size(261, 241);
            this.APIHeaderList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.APIHeaderList.TabIndex = 0;
            this.APIHeaderList.UseCompatibleStateImageBehavior = false;
            this.APIHeaderList.View = System.Windows.Forms.View.Details;
            // 
            // APIHdrName
            // 
            this.APIHdrName.Text = "Name";
            this.APIHdrName.Width = 128;
            // 
            // APIHdrDescription
            // 
            this.APIHdrDescription.Text = "Description";
            this.APIHdrDescription.Width = 197;
            // 
            // APIParametersMenuStrip
            // 
            this.APIParametersMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.editToolStripMenuItem,
            this.useCollectionToolStripMenuItem,
            this.manageCollectionToolStripMenuItem});
            this.APIParametersMenuStrip.Name = "APIParametersMenuStrip";
            this.APIParametersMenuStrip.Size = new System.Drawing.Size(180, 114);
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.addToolStripMenuItem.Text = "Add";
            this.addToolStripMenuItem.Click += new System.EventHandler(this.AddAPIHeaderParm_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.DeleteAPIHeaderParm_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.editToolStripMenuItem.Text = "Edit";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.EditAPIHeaderParm_Click);
            // 
            // useCollectionToolStripMenuItem
            // 
            this.useCollectionToolStripMenuItem.Name = "useCollectionToolStripMenuItem";
            this.useCollectionToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.useCollectionToolStripMenuItem.Text = "Use Collection";
            this.useCollectionToolStripMenuItem.Click += new System.EventHandler(this.UseHeaderCollection_Click);
            // 
            // manageCollectionToolStripMenuItem
            // 
            this.manageCollectionToolStripMenuItem.Name = "manageCollectionToolStripMenuItem";
            this.manageCollectionToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.manageCollectionToolStripMenuItem.Text = "Manage Collections";
            this.manageCollectionToolStripMenuItem.Click += new System.EventHandler(this.EditHeaderCollections_Click);
            // 
            // SelectedHeaderGroup
            // 
            this.SelectedHeaderGroup.Controls.Add(this.SelectedHeaderList);
            this.SelectedHeaderGroup.Location = new System.Drawing.Point(369, 12);
            this.SelectedHeaderGroup.Name = "SelectedHeaderGroup";
            this.SelectedHeaderGroup.Size = new System.Drawing.Size(284, 297);
            this.SelectedHeaderGroup.TabIndex = 0;
            this.SelectedHeaderGroup.TabStop = false;
            this.SelectedHeaderGroup.Text = "Selected header parameters";
            // 
            // SelectedHeaderList
            // 
            this.SelectedHeaderList.BackColor = System.Drawing.Color.GhostWhite;
            this.SelectedHeaderList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.SelectedHdrName,
            this.SelectedHdrDescription});
            this.SelectedHeaderList.FullRowSelect = true;
            this.SelectedHeaderList.GridLines = true;
            this.SelectedHeaderList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.SelectedHeaderList.HideSelection = false;
            this.SelectedHeaderList.Location = new System.Drawing.Point(12, 19);
            this.SelectedHeaderList.Name = "SelectedHeaderList";
            this.SelectedHeaderList.Size = new System.Drawing.Size(261, 241);
            this.SelectedHeaderList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.SelectedHeaderList.TabIndex = 0;
            this.SelectedHeaderList.UseCompatibleStateImageBehavior = false;
            this.SelectedHeaderList.View = System.Windows.Forms.View.Details;
            // 
            // SelectedHdrName
            // 
            this.SelectedHdrName.Text = "Name";
            this.SelectedHdrName.Width = 128;
            // 
            // SelectedHdrDescription
            // 
            this.SelectedHdrDescription.Text = "Description";
            this.SelectedHdrDescription.Width = 197;
            // 
            // UnselectParameters
            // 
            this.UnselectParameters.Image = ((System.Drawing.Image)(resources.GetObject("UnselectParameters.Image")));
            this.UnselectParameters.Location = new System.Drawing.Point(319, 168);
            this.UnselectParameters.Name = "UnselectParameters";
            this.UnselectParameters.Size = new System.Drawing.Size(25, 65);
            this.UnselectParameters.TabIndex = 3;
            this.UnselectParameters.UseVisualStyleBackColor = true;
            this.UnselectParameters.Click += new System.EventHandler(this.UnselectParameters_Click);
            // 
            // SelectParameters
            // 
            this.SelectParameters.Image = ((System.Drawing.Image)(resources.GetObject("SelectParameters.Image")));
            this.SelectParameters.Location = new System.Drawing.Point(319, 87);
            this.SelectParameters.Name = "SelectParameters";
            this.SelectParameters.Size = new System.Drawing.Size(25, 65);
            this.SelectParameters.TabIndex = 2;
            this.SelectParameters.UseVisualStyleBackColor = true;
            this.SelectParameters.Click += new System.EventHandler(this.SelectParameters_Click);
            // 
            // RESTHeaderParameterDialog
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(666, 358);
            this.Controls.Add(this.SelectedHeaderGroup);
            this.Controls.Add(this.UnselectParameters);
            this.Controls.Add(this.APIHeaderGroup);
            this.Controls.Add(this.SelectParameters);
            this.Controls.Add(this.Ok);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RESTHeaderParameterDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add/Edit Header Parameter";
            this.APIHeaderGroup.ResumeLayout(false);
            this.APIParametersMenuStrip.ResumeLayout(false);
            this.SelectedHeaderGroup.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.GroupBox APIHeaderGroup;
        private System.Windows.Forms.Button UseHeaderCollection;
        private System.Windows.Forms.Button EditHeaderCollections;
        private System.Windows.Forms.Button EditAPIHeaderParm;
        private System.Windows.Forms.Button DeleteAPIHeaderParm;
        private System.Windows.Forms.Button AddAPIHeaderParm;
        private System.Windows.Forms.ListView APIHeaderList;
        private System.Windows.Forms.ColumnHeader APIHdrName;
        private System.Windows.Forms.ColumnHeader APIHdrDescription;
        private System.Windows.Forms.GroupBox SelectedHeaderGroup;
        private System.Windows.Forms.ListView SelectedHeaderList;
        private System.Windows.Forms.ColumnHeader SelectedHdrName;
        private System.Windows.Forms.ColumnHeader SelectedHdrDescription;
        private System.Windows.Forms.Button UnselectParameters;
        private System.Windows.Forms.Button SelectParameters;
        private System.Windows.Forms.ContextMenuStrip APIParametersMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem useCollectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manageCollectionToolStripMenuItem;
    }
}