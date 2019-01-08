using System.Windows.Forms;

namespace Plugin.Application.Forms
{
    partial class RESTResponseCodeCollectionSetEdit
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RESTResponseCodeCollectionSetEdit));
            this.Cancel = new System.Windows.Forms.Button();
            this.Ok = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.EditResponseCode = new System.Windows.Forms.Button();
            this.DeleteResponseCode = new System.Windows.Forms.Button();
            this.AddResponseCode = new System.Windows.Forms.Button();
            this.CollectionListFld = new System.Windows.Forms.ListBox();
            this.CollectionListMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addCollectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteCollectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editCollectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CollectionListMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(183, 139);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(50, 25);
            this.Cancel.TabIndex = 4;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(127, 139);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(50, 25);
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
            // EditResponseCode
            // 
            this.EditResponseCode.Image = ((System.Drawing.Image)(resources.GetObject("EditResponseCode.Image")));
            this.EditResponseCode.Location = new System.Drawing.Point(74, 139);
            this.EditResponseCode.Name = "EditResponseCode";
            this.EditResponseCode.Size = new System.Drawing.Size(25, 25);
            this.EditResponseCode.TabIndex = 3;
            this.EditResponseCode.UseVisualStyleBackColor = true;
            this.EditResponseCode.Click += new System.EventHandler(this.EditCollection_Click);
            // 
            // DeleteResponseCode
            // 
            this.DeleteResponseCode.Image = ((System.Drawing.Image)(resources.GetObject("DeleteResponseCode.Image")));
            this.DeleteResponseCode.Location = new System.Drawing.Point(43, 139);
            this.DeleteResponseCode.Name = "DeleteResponseCode";
            this.DeleteResponseCode.Size = new System.Drawing.Size(25, 25);
            this.DeleteResponseCode.TabIndex = 2;
            this.DeleteResponseCode.UseVisualStyleBackColor = true;
            this.DeleteResponseCode.Click += new System.EventHandler(this.DeleteCollection_Click);
            // 
            // AddResponseCode
            // 
            this.AddResponseCode.Image = ((System.Drawing.Image)(resources.GetObject("AddResponseCode.Image")));
            this.AddResponseCode.Location = new System.Drawing.Point(12, 139);
            this.AddResponseCode.Name = "AddResponseCode";
            this.AddResponseCode.Size = new System.Drawing.Size(25, 25);
            this.AddResponseCode.TabIndex = 1;
            this.AddResponseCode.UseVisualStyleBackColor = true;
            this.AddResponseCode.Click += new System.EventHandler(this.AddCollection_Click);
            // 
            // CollectionListFld
            // 
            this.CollectionListFld.FormattingEnabled = true;
            this.CollectionListFld.Location = new System.Drawing.Point(12, 12);
            this.CollectionListFld.Name = "CollectionListFld";
            this.CollectionListFld.Size = new System.Drawing.Size(221, 121);
            this.CollectionListFld.Sorted = true;
            this.CollectionListFld.TabIndex = 4;
            // 
            // CollectionListMenuStrip
            // 
            this.CollectionListMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addCollectionToolStripMenuItem,
            this.deleteCollectionToolStripMenuItem,
            this.editCollectionToolStripMenuItem});
            this.CollectionListMenuStrip.Name = "CollectionListMenuStrip";
            this.CollectionListMenuStrip.Size = new System.Drawing.Size(165, 92);
            // 
            // addCollectionToolStripMenuItem
            // 
            this.addCollectionToolStripMenuItem.Name = "addCollectionToolStripMenuItem";
            this.addCollectionToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.addCollectionToolStripMenuItem.Text = "Add Collection";
            this.addCollectionToolStripMenuItem.Click += new System.EventHandler(this.AddCollection_Click);
            // 
            // deleteCollectionToolStripMenuItem
            // 
            this.deleteCollectionToolStripMenuItem.Name = "deleteCollectionToolStripMenuItem";
            this.deleteCollectionToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.deleteCollectionToolStripMenuItem.Text = "Delete Collection";
            this.deleteCollectionToolStripMenuItem.Click += new System.EventHandler(this.DeleteCollection_Click);
            // 
            // editCollectionToolStripMenuItem
            // 
            this.editCollectionToolStripMenuItem.Name = "editCollectionToolStripMenuItem";
            this.editCollectionToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.editCollectionToolStripMenuItem.Text = "Edit Collection";
            this.editCollectionToolStripMenuItem.Click += new System.EventHandler(this.EditCollection_Click);
            // 
            // RESTResponseCodeCollectionSetEdit
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(245, 177);
            this.Controls.Add(this.CollectionListFld);
            this.Controls.Add(this.AddResponseCode);
            this.Controls.Add(this.DeleteResponseCode);
            this.Controls.Add(this.EditResponseCode);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.Cancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RESTResponseCodeCollectionSetEdit";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Manage Response Code Collections";
            this.CollectionListMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button Ok;
        private Label label1;
        private Button EditResponseCode;
        private Button DeleteResponseCode;
        private Button AddResponseCode;
        private ListBox CollectionListFld;
        private ContextMenuStrip CollectionListMenuStrip;
        private ToolStripMenuItem addCollectionToolStripMenuItem;
        private ToolStripMenuItem deleteCollectionToolStripMenuItem;
        private ToolStripMenuItem editCollectionToolStripMenuItem;
    }
}