using System.Windows.Forms;

namespace Plugin.Application.Forms
{
    partial class ClassInspector
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
            this.Ok = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.ResourceList = new System.Windows.Forms.ListView();
            this.AttribID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.AttribName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.AttribAlias = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.AttribClassifier = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.AttribCard = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.AttribType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DeleteResource = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label2 = new System.Windows.Forms.Label();
            this.ClassName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.AliasName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.StereoTypes = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.DeleteResource.SuspendLayout();
            this.SuspendLayout();
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(505, 442);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 31);
            this.Ok.TabIndex = 6;
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
            // ResourceList
            // 
            this.ResourceList.AllowColumnReorder = true;
            this.ResourceList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.AttribID,
            this.AttribName,
            this.AttribAlias,
            this.AttribClassifier,
            this.AttribCard,
            this.AttribType});
            this.ResourceList.FullRowSelect = true;
            this.ResourceList.GridLines = true;
            this.ResourceList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.ResourceList.Location = new System.Drawing.Point(12, 104);
            this.ResourceList.MultiSelect = false;
            this.ResourceList.Name = "ResourceList";
            this.ResourceList.Size = new System.Drawing.Size(568, 332);
            this.ResourceList.TabIndex = 2;
            this.ResourceList.UseCompatibleStateImageBehavior = false;
            this.ResourceList.View = System.Windows.Forms.View.Details;
            // 
            // AttribID
            // 
            this.AttribID.Text = "ID";
            this.AttribID.Width = 44;
            // 
            // AttribName
            // 
            this.AttribName.Text = "Name";
            this.AttribName.Width = 122;
            // 
            // AttribAlias
            // 
            this.AttribAlias.Text = "Alias";
            this.AttribAlias.Width = 134;
            // 
            // AttribClassifier
            // 
            this.AttribClassifier.Text = "Classifier";
            this.AttribClassifier.Width = 137;
            // 
            // AttribCard
            // 
            this.AttribCard.Text = "Cardinality";
            // 
            // AttribType
            // 
            this.AttribType.Text = "Type";
            this.AttribType.Width = 41;
            // 
            // DeleteResource
            // 
            this.DeleteResource.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteToolStripMenuItem});
            this.DeleteResource.Name = "contextMenuStrip1";
            this.DeleteResource.Size = new System.Drawing.Size(108, 26);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Class name:";
            // 
            // ClassName
            // 
            this.ClassName.Location = new System.Drawing.Point(82, 6);
            this.ClassName.Name = "ClassName";
            this.ClassName.ReadOnly = true;
            this.ClassName.Size = new System.Drawing.Size(236, 20);
            this.ClassName.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(44, 37);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Alias:";
            // 
            // AliasName
            // 
            this.AliasName.Location = new System.Drawing.Point(83, 34);
            this.AliasName.Name = "AliasName";
            this.AliasName.ReadOnly = true;
            this.AliasName.Size = new System.Drawing.Size(235, 20);
            this.AliasName.TabIndex = 10;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 64);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(69, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Stereotypes: ";
            // 
            // StereoTypes
            // 
            this.StereoTypes.Location = new System.Drawing.Point(83, 61);
            this.StereoTypes.Name = "StereoTypes";
            this.StereoTypes.ReadOnly = true;
            this.StereoTypes.Size = new System.Drawing.Size(474, 20);
            this.StereoTypes.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 88);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(54, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Attributes:";
            // 
            // ClassInspector
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(592, 485);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.StereoTypes);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.AliasName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.ClassName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ResourceList);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Ok);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ClassInspector";
            this.Text = "Class Inspector";
            this.DeleteResource.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button Ok;
        private Label label1;
        private ListView ResourceList;
        private ColumnHeader AttribID;
        private ColumnHeader AttribName;
        private ContextMenuStrip DeleteResource;
        private ToolStripMenuItem deleteToolStripMenuItem;
        private ColumnHeader AttribAlias;
        private ColumnHeader AttribClassifier;
        private ColumnHeader AttribType;
        private Label label2;
        private TextBox ClassName;
        private Label label3;
        private TextBox AliasName;
        private Label label4;
        private TextBox StereoTypes;
        private ColumnHeader AttribCard;
        private Label label5;
    }
}