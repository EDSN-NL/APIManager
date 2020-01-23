using System.Windows.Forms;

namespace Plugin.Application.Forms
{
    partial class RESTHeaderParameterCollectionEdit
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RESTHeaderParameterCollectionEdit));
            this.Cancel = new System.Windows.Forms.Button();
            this.Ok = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.HeaderParameterGroup = new System.Windows.Forms.GroupBox();
            this.EditHeaderParameter = new System.Windows.Forms.Button();
            this.DeleteHeaderParameter = new System.Windows.Forms.Button();
            this.AddHeaderParameter = new System.Windows.Forms.Button();
            this.HeaderParameterList = new System.Windows.Forms.ListView();
            this.ResponseName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ResponseDesc = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label2 = new System.Windows.Forms.Label();
            this.CollectionNmFld = new System.Windows.Forms.TextBox();
            this.ResponseCodeMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ScopeGroup = new System.Windows.Forms.GroupBox();
            this.APIScope = new System.Windows.Forms.RadioButton();
            this.GlobalScope = new System.Windows.Forms.RadioButton();
            this.HeaderParameterGroup.SuspendLayout();
            this.ResponseCodeMenuStrip.SuspendLayout();
            this.ScopeGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(422, 309);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 4;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(341, 309);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 31);
            this.Ok.TabIndex = 5;
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
            // HeaderParameterGroup
            // 
            this.HeaderParameterGroup.Controls.Add(this.EditHeaderParameter);
            this.HeaderParameterGroup.Controls.Add(this.DeleteHeaderParameter);
            this.HeaderParameterGroup.Controls.Add(this.AddHeaderParameter);
            this.HeaderParameterGroup.Controls.Add(this.HeaderParameterList);
            this.HeaderParameterGroup.Location = new System.Drawing.Point(12, 62);
            this.HeaderParameterGroup.Name = "HeaderParameterGroup";
            this.HeaderParameterGroup.Size = new System.Drawing.Size(484, 241);
            this.HeaderParameterGroup.TabIndex = 3;
            this.HeaderParameterGroup.TabStop = false;
            this.HeaderParameterGroup.Text = "Header parameters";
            // 
            // EditHeaderParameter
            // 
            this.EditHeaderParameter.Image = ((System.Drawing.Image)(resources.GetObject("EditHeaderParameter.Image")));
            this.EditHeaderParameter.Location = new System.Drawing.Point(73, 210);
            this.EditHeaderParameter.Name = "EditHeaderParameter";
            this.EditHeaderParameter.Size = new System.Drawing.Size(25, 25);
            this.EditHeaderParameter.TabIndex = 3;
            this.EditHeaderParameter.UseVisualStyleBackColor = true;
            this.EditHeaderParameter.Click += new System.EventHandler(this.EditHeaderParameter_Click);
            // 
            // DeleteHeaderParameter
            // 
            this.DeleteHeaderParameter.Image = ((System.Drawing.Image)(resources.GetObject("DeleteHeaderParameter.Image")));
            this.DeleteHeaderParameter.Location = new System.Drawing.Point(42, 210);
            this.DeleteHeaderParameter.Name = "DeleteHeaderParameter";
            this.DeleteHeaderParameter.Size = new System.Drawing.Size(25, 25);
            this.DeleteHeaderParameter.TabIndex = 2;
            this.DeleteHeaderParameter.UseVisualStyleBackColor = true;
            this.DeleteHeaderParameter.Click += new System.EventHandler(this.DeleteHeaderParameter_Click);
            // 
            // AddHeaderParameter
            // 
            this.AddHeaderParameter.Image = ((System.Drawing.Image)(resources.GetObject("AddHeaderParameter.Image")));
            this.AddHeaderParameter.Location = new System.Drawing.Point(11, 210);
            this.AddHeaderParameter.Name = "AddHeaderParameter";
            this.AddHeaderParameter.Size = new System.Drawing.Size(25, 25);
            this.AddHeaderParameter.TabIndex = 1;
            this.AddHeaderParameter.UseVisualStyleBackColor = true;
            this.AddHeaderParameter.Click += new System.EventHandler(this.AddHeaderParameter_Click);
            // 
            // HeaderParameterList
            // 
            this.HeaderParameterList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ResponseName,
            this.ResponseDesc});
            this.HeaderParameterList.FullRowSelect = true;
            this.HeaderParameterList.GridLines = true;
            this.HeaderParameterList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.HeaderParameterList.HideSelection = false;
            this.HeaderParameterList.Location = new System.Drawing.Point(11, 19);
            this.HeaderParameterList.MultiSelect = false;
            this.HeaderParameterList.Name = "HeaderParameterList";
            this.HeaderParameterList.Size = new System.Drawing.Size(461, 185);
            this.HeaderParameterList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.HeaderParameterList.TabIndex = 0;
            this.HeaderParameterList.UseCompatibleStateImageBehavior = false;
            this.HeaderParameterList.View = System.Windows.Forms.View.Details;
            // 
            // ResponseName
            // 
            this.ResponseName.Text = "Name";
            this.ResponseName.Width = 192;
            // 
            // ResponseDesc
            // 
            this.ResponseDesc.Text = "Description";
            this.ResponseDesc.Width = 318;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Name:";
            // 
            // CollectionNmFld
            // 
            this.CollectionNmFld.Location = new System.Drawing.Point(59, 28);
            this.CollectionNmFld.Name = "CollectionNmFld";
            this.CollectionNmFld.Size = new System.Drawing.Size(283, 20);
            this.CollectionNmFld.TabIndex = 1;
            this.CollectionNmFld.Leave += new System.EventHandler(this.CollectionNmFld_Leave);
            // 
            // ResponseCodeMenuStrip
            // 
            this.ResponseCodeMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.editToolStripMenuItem});
            this.ResponseCodeMenuStrip.Name = "ResponseCodeMenuStrip";
            this.ResponseCodeMenuStrip.Size = new System.Drawing.Size(108, 70);
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.addToolStripMenuItem.Text = "Add";
            this.addToolStripMenuItem.Click += new System.EventHandler(this.AddHeaderParameter_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.DeleteHeaderParameter_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.editToolStripMenuItem.Text = "Edit";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.EditHeaderParameter_Click);
            // 
            // ScopeGroup
            // 
            this.ScopeGroup.Controls.Add(this.APIScope);
            this.ScopeGroup.Controls.Add(this.GlobalScope);
            this.ScopeGroup.Location = new System.Drawing.Point(348, 12);
            this.ScopeGroup.Name = "ScopeGroup";
            this.ScopeGroup.Size = new System.Drawing.Size(148, 44);
            this.ScopeGroup.TabIndex = 2;
            this.ScopeGroup.TabStop = false;
            this.ScopeGroup.Text = "Scope";
            // 
            // APIScope
            // 
            this.APIScope.AutoSize = true;
            this.APIScope.Location = new System.Drawing.Point(74, 19);
            this.APIScope.Name = "APIScope";
            this.APIScope.Size = new System.Drawing.Size(42, 17);
            this.APIScope.TabIndex = 2;
            this.APIScope.TabStop = true;
            this.APIScope.Text = "API";
            this.APIScope.UseVisualStyleBackColor = true;
            this.APIScope.CheckedChanged += new System.EventHandler(this.ScopeGroup_CheckedChanged);
            // 
            // GlobalScope
            // 
            this.GlobalScope.AutoSize = true;
            this.GlobalScope.Location = new System.Drawing.Point(12, 19);
            this.GlobalScope.Name = "GlobalScope";
            this.GlobalScope.Size = new System.Drawing.Size(55, 17);
            this.GlobalScope.TabIndex = 1;
            this.GlobalScope.TabStop = true;
            this.GlobalScope.Text = "Global";
            this.GlobalScope.UseVisualStyleBackColor = true;
            this.GlobalScope.CheckedChanged += new System.EventHandler(this.ScopeGroup_CheckedChanged);
            // 
            // RESTHeaderParameterCollectionEdit
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(508, 351);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.CollectionNmFld);
            this.Controls.Add(this.ScopeGroup);
            this.Controls.Add(this.HeaderParameterGroup);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.Cancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RESTHeaderParameterCollectionEdit";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create or Edit Collection";
            this.HeaderParameterGroup.ResumeLayout(false);
            this.ResponseCodeMenuStrip.ResumeLayout(false);
            this.ScopeGroup.ResumeLayout(false);
            this.ScopeGroup.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button Ok;
        private Label label1;
        private GroupBox HeaderParameterGroup;
        private Button EditHeaderParameter;
        private Button DeleteHeaderParameter;
        private Button AddHeaderParameter;
        private ListView HeaderParameterList;
        private ColumnHeader ResponseName;
        private ColumnHeader ResponseDesc;
        private Label label2;
        private TextBox CollectionNmFld;
        private ContextMenuStrip ResponseCodeMenuStrip;
        private ToolStripMenuItem addToolStripMenuItem;
        private ToolStripMenuItem deleteToolStripMenuItem;
        private ToolStripMenuItem editToolStripMenuItem;
        private GroupBox ScopeGroup;
        private RadioButton APIScope;
        private RadioButton GlobalScope;
    }
}