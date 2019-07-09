using System.Windows.Forms;

namespace Plugin.Application.Forms
{
    partial class RESTResponseCodeCollectionEdit
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RESTResponseCodeCollectionEdit));
            this.Cancel = new System.Windows.Forms.Button();
            this.Ok = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.ResponseCodeGroup = new System.Windows.Forms.GroupBox();
            this.EditResponseCode = new System.Windows.Forms.Button();
            this.DeleteResponseCode = new System.Windows.Forms.Button();
            this.AddResponseCode = new System.Windows.Forms.Button();
            this.ResponseCodeList = new System.Windows.Forms.ListView();
            this.Code = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
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
            this.ResponseCodeGroup.SuspendLayout();
            this.ResponseCodeMenuStrip.SuspendLayout();
            this.ScopeGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(219, 274);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 3;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(138, 274);
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
            // ResponseCodeGroup
            // 
            this.ResponseCodeGroup.Controls.Add(this.EditResponseCode);
            this.ResponseCodeGroup.Controls.Add(this.DeleteResponseCode);
            this.ResponseCodeGroup.Controls.Add(this.AddResponseCode);
            this.ResponseCodeGroup.Controls.Add(this.ResponseCodeList);
            this.ResponseCodeGroup.Location = new System.Drawing.Point(12, 88);
            this.ResponseCodeGroup.Name = "ResponseCodeGroup";
            this.ResponseCodeGroup.Size = new System.Drawing.Size(282, 180);
            this.ResponseCodeGroup.TabIndex = 2;
            this.ResponseCodeGroup.TabStop = false;
            this.ResponseCodeGroup.Text = "Response codes";
            // 
            // EditResponseCode
            // 
            this.EditResponseCode.Image = ((System.Drawing.Image)(resources.GetObject("EditResponseCode.Image")));
            this.EditResponseCode.Location = new System.Drawing.Point(74, 146);
            this.EditResponseCode.Name = "EditResponseCode";
            this.EditResponseCode.Size = new System.Drawing.Size(25, 25);
            this.EditResponseCode.TabIndex = 4;
            this.EditResponseCode.UseVisualStyleBackColor = true;
            this.EditResponseCode.Click += new System.EventHandler(this.EditResponseCode_Click);
            // 
            // DeleteResponseCode
            // 
            this.DeleteResponseCode.Image = ((System.Drawing.Image)(resources.GetObject("DeleteResponseCode.Image")));
            this.DeleteResponseCode.Location = new System.Drawing.Point(43, 146);
            this.DeleteResponseCode.Name = "DeleteResponseCode";
            this.DeleteResponseCode.Size = new System.Drawing.Size(25, 25);
            this.DeleteResponseCode.TabIndex = 3;
            this.DeleteResponseCode.UseVisualStyleBackColor = true;
            this.DeleteResponseCode.Click += new System.EventHandler(this.DeleteResponseCode_Click);
            // 
            // AddResponseCode
            // 
            this.AddResponseCode.Image = ((System.Drawing.Image)(resources.GetObject("AddResponseCode.Image")));
            this.AddResponseCode.Location = new System.Drawing.Point(12, 146);
            this.AddResponseCode.Name = "AddResponseCode";
            this.AddResponseCode.Size = new System.Drawing.Size(25, 25);
            this.AddResponseCode.TabIndex = 2;
            this.AddResponseCode.UseVisualStyleBackColor = true;
            this.AddResponseCode.Click += new System.EventHandler(this.AddResponseCode_Click);
            // 
            // ResponseCodeList
            // 
            this.ResponseCodeList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Code,
            this.ResponseDesc});
            this.ResponseCodeList.FullRowSelect = true;
            this.ResponseCodeList.GridLines = true;
            this.ResponseCodeList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.ResponseCodeList.Location = new System.Drawing.Point(12, 19);
            this.ResponseCodeList.MultiSelect = false;
            this.ResponseCodeList.Name = "ResponseCodeList";
            this.ResponseCodeList.Size = new System.Drawing.Size(256, 121);
            this.ResponseCodeList.TabIndex = 1;
            this.ResponseCodeList.UseCompatibleStateImageBehavior = false;
            this.ResponseCodeList.View = System.Windows.Forms.View.Details;
            // 
            // Code
            // 
            this.Code.Text = "Code";
            this.Code.Width = 58;
            // 
            // ResponseDesc
            // 
            this.ResponseDesc.Text = "Description";
            this.ResponseDesc.Width = 197;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Name:";
            // 
            // CollectionNmFld
            // 
            this.CollectionNmFld.Location = new System.Drawing.Point(59, 12);
            this.CollectionNmFld.Name = "CollectionNmFld";
            this.CollectionNmFld.Size = new System.Drawing.Size(235, 20);
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
            this.addToolStripMenuItem.Click += new System.EventHandler(this.AddResponseCode_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.DeleteResponseCode_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.editToolStripMenuItem.Text = "Edit";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.EditResponseCode_Click);
            // 
            // ScopeGroup
            // 
            this.ScopeGroup.Controls.Add(this.APIScope);
            this.ScopeGroup.Controls.Add(this.GlobalScope);
            this.ScopeGroup.Location = new System.Drawing.Point(12, 38);
            this.ScopeGroup.Name = "ScopeGroup";
            this.ScopeGroup.Size = new System.Drawing.Size(282, 44);
            this.ScopeGroup.TabIndex = 11;
            this.ScopeGroup.TabStop = false;
            this.ScopeGroup.Text = "Scope";
            // 
            // APIScope
            // 
            this.APIScope.AutoSize = true;
            this.APIScope.Location = new System.Drawing.Point(74, 19);
            this.APIScope.Name = "APIScope";
            this.APIScope.Size = new System.Drawing.Size(42, 17);
            this.APIScope.TabIndex = 1;
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
            this.GlobalScope.TabIndex = 0;
            this.GlobalScope.TabStop = true;
            this.GlobalScope.Text = "Global";
            this.GlobalScope.UseVisualStyleBackColor = true;
            this.GlobalScope.CheckedChanged += new System.EventHandler(this.ScopeGroup_CheckedChanged);
            // 
            // RESTResponseCodeCollectionEdit
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(308, 318);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.CollectionNmFld);
            this.Controls.Add(this.ScopeGroup);
            this.Controls.Add(this.ResponseCodeGroup);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.Cancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RESTResponseCodeCollectionEdit";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create or Edit Colection";
            this.ResponseCodeGroup.ResumeLayout(false);
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
        private GroupBox ResponseCodeGroup;
        private Button EditResponseCode;
        private Button DeleteResponseCode;
        private Button AddResponseCode;
        private ListView ResponseCodeList;
        private ColumnHeader Code;
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