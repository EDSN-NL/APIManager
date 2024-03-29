﻿namespace Plugin.Application.Forms
{
    partial class RESTResourceDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RESTResourceDialog));
            this.Ok = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.PathExpressionLbl = new System.Windows.Forms.Label();
            this.OperationsBox = new System.Windows.Forms.GroupBox();
            this.OperationsList = new System.Windows.Forms.ListView();
            this.OperationName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.OperationType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.EditOperation = new System.Windows.Forms.Button();
            this.DeleteOperation = new System.Windows.Forms.Button();
            this.AddOperation = new System.Windows.Forms.Button();
            this.AddOperationToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.DeleteOperationToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.EditOperationToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.PropertiesBox = new System.Windows.Forms.GroupBox();
            this.SelectClassifier = new System.Windows.Forms.Button();
            this.ParameterClassifier = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ParameterName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ResourceTypeBox = new System.Windows.Forms.ComboBox();
            this.OperationMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.NewMinorVersion = new System.Windows.Forms.CheckBox();
            this.ResourceNameFld = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.DocumentationBox = new System.Windows.Forms.GroupBox();
            this.TagNames = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.ExternalDocURL = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.ExternalDocDescription = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.DocDescription = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.DefineIdentifierToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.CreateDocument = new System.Windows.Forms.Button();
            this.LinkDocument = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.OperationsBox.SuspendLayout();
            this.PropertiesBox.SuspendLayout();
            this.OperationMenuStrip.SuspendLayout();
            this.DocumentationBox.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(370, 346);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 31);
            this.Ok.TabIndex = 10;
            this.Ok.Text = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(451, 346);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 9;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // PathExpressionLbl
            // 
            this.PathExpressionLbl.AutoSize = true;
            this.PathExpressionLbl.Location = new System.Drawing.Point(2, 16);
            this.PathExpressionLbl.Name = "PathExpressionLbl";
            this.PathExpressionLbl.Size = new System.Drawing.Size(79, 13);
            this.PathExpressionLbl.TabIndex = 0;
            this.PathExpressionLbl.Text = "Resource type:";
            // 
            // OperationsBox
            // 
            this.OperationsBox.Controls.Add(this.OperationsList);
            this.OperationsBox.Controls.Add(this.EditOperation);
            this.OperationsBox.Controls.Add(this.DeleteOperation);
            this.OperationsBox.Controls.Add(this.AddOperation);
            this.OperationsBox.Location = new System.Drawing.Point(260, 8);
            this.OperationsBox.Name = "OperationsBox";
            this.OperationsBox.Size = new System.Drawing.Size(266, 190);
            this.OperationsBox.TabIndex = 5;
            this.OperationsBox.TabStop = false;
            this.OperationsBox.Text = "Associated operations";
            // 
            // OperationsList
            // 
            this.OperationsList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.OperationName,
            this.OperationType});
            this.OperationsList.FullRowSelect = true;
            this.OperationsList.GridLines = true;
            this.OperationsList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.OperationsList.HideSelection = false;
            this.OperationsList.Location = new System.Drawing.Point(7, 19);
            this.OperationsList.MultiSelect = false;
            this.OperationsList.Name = "OperationsList";
            this.OperationsList.Size = new System.Drawing.Size(250, 130);
            this.OperationsList.TabIndex = 0;
            this.OperationsList.UseCompatibleStateImageBehavior = false;
            this.OperationsList.View = System.Windows.Forms.View.Details;
            // 
            // OperationName
            // 
            this.OperationName.Text = "Name";
            this.OperationName.Width = 149;
            // 
            // OperationType
            // 
            this.OperationType.Text = "Type";
            this.OperationType.Width = 92;
            // 
            // EditOperation
            // 
            this.EditOperation.Image = ((System.Drawing.Image)(resources.GetObject("EditOperation.Image")));
            this.EditOperation.Location = new System.Drawing.Point(68, 155);
            this.EditOperation.Name = "EditOperation";
            this.EditOperation.Size = new System.Drawing.Size(25, 25);
            this.EditOperation.TabIndex = 3;
            this.EditOperation.UseVisualStyleBackColor = true;
            this.EditOperation.Click += new System.EventHandler(this.EditOperation_Click);
            // 
            // DeleteOperation
            // 
            this.DeleteOperation.Image = ((System.Drawing.Image)(resources.GetObject("DeleteOperation.Image")));
            this.DeleteOperation.Location = new System.Drawing.Point(37, 155);
            this.DeleteOperation.Name = "DeleteOperation";
            this.DeleteOperation.Size = new System.Drawing.Size(25, 25);
            this.DeleteOperation.TabIndex = 2;
            this.DeleteOperation.UseVisualStyleBackColor = true;
            this.DeleteOperation.Click += new System.EventHandler(this.DeleteOperation_Click);
            // 
            // AddOperation
            // 
            this.AddOperation.Image = ((System.Drawing.Image)(resources.GetObject("AddOperation.Image")));
            this.AddOperation.Location = new System.Drawing.Point(6, 155);
            this.AddOperation.Name = "AddOperation";
            this.AddOperation.Size = new System.Drawing.Size(25, 25);
            this.AddOperation.TabIndex = 1;
            this.AddOperation.UseVisualStyleBackColor = true;
            this.AddOperation.Click += new System.EventHandler(this.AddOperation_Click);
            // 
            // AddOperationToolTip
            // 
            this.AddOperationToolTip.IsBalloon = true;
            this.AddOperationToolTip.ToolTipTitle = "Add Operation";
            // 
            // DeleteOperationToolTip
            // 
            this.DeleteOperationToolTip.IsBalloon = true;
            this.DeleteOperationToolTip.ToolTipTitle = "Delete Operation";
            // 
            // EditOperationToolTip
            // 
            this.EditOperationToolTip.IsBalloon = true;
            this.EditOperationToolTip.ToolTipTitle = "Edit Operation";
            // 
            // PropertiesBox
            // 
            this.PropertiesBox.Controls.Add(this.SelectClassifier);
            this.PropertiesBox.Controls.Add(this.ParameterClassifier);
            this.PropertiesBox.Controls.Add(this.label3);
            this.PropertiesBox.Controls.Add(this.ParameterName);
            this.PropertiesBox.Controls.Add(this.label2);
            this.PropertiesBox.Location = new System.Drawing.Point(5, 107);
            this.PropertiesBox.Name = "PropertiesBox";
            this.PropertiesBox.Size = new System.Drawing.Size(249, 91);
            this.PropertiesBox.TabIndex = 6;
            this.PropertiesBox.TabStop = false;
            this.PropertiesBox.Text = "Identifier Properties";
            // 
            // SelectClassifier
            // 
            this.SelectClassifier.Image = ((System.Drawing.Image)(resources.GetObject("SelectClassifier.Image")));
            this.SelectClassifier.Location = new System.Drawing.Point(208, 25);
            this.SelectClassifier.Name = "SelectClassifier";
            this.SelectClassifier.Size = new System.Drawing.Size(25, 25);
            this.SelectClassifier.TabIndex = 1;
            this.SelectClassifier.UseVisualStyleBackColor = true;
            this.SelectClassifier.Click += new System.EventHandler(this.DefineIdentifier_Click);
            // 
            // ParameterClassifier
            // 
            this.ParameterClassifier.Location = new System.Drawing.Point(61, 59);
            this.ParameterClassifier.Name = "ParameterClassifier";
            this.ParameterClassifier.ReadOnly = true;
            this.ParameterClassifier.Size = new System.Drawing.Size(172, 20);
            this.ParameterClassifier.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 62);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(51, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Classifier:";
            // 
            // ParameterName
            // 
            this.ParameterName.Location = new System.Drawing.Point(61, 28);
            this.ParameterName.Name = "ParameterName";
            this.ParameterName.ReadOnly = true;
            this.ParameterName.Size = new System.Drawing.Size(141, 20);
            this.ParameterName.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Name:";
            // 
            // ResourceTypeBox
            // 
            this.ResourceTypeBox.Location = new System.Drawing.Point(87, 13);
            this.ResourceTypeBox.Name = "ResourceTypeBox";
            this.ResourceTypeBox.Size = new System.Drawing.Size(167, 21);
            this.ResourceTypeBox.TabIndex = 1;
            this.ResourceTypeBox.SelectedIndexChanged += new System.EventHandler(this.ResourceTypeBox_SelectedIndexChanged);
            // 
            // OperationMenuStrip
            // 
            this.OperationMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editToolStripMenuItem,
            this.deleteToolStripMenuItem1});
            this.OperationMenuStrip.Name = "OperationMenuStrip";
            this.OperationMenuStrip.Size = new System.Drawing.Size(108, 48);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.editToolStripMenuItem.Text = "Delete";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.DeleteOperation_Click);
            // 
            // deleteToolStripMenuItem1
            // 
            this.deleteToolStripMenuItem1.Name = "deleteToolStripMenuItem1";
            this.deleteToolStripMenuItem1.Size = new System.Drawing.Size(107, 22);
            this.deleteToolStripMenuItem1.Text = "Edit";
            this.deleteToolStripMenuItem1.Click += new System.EventHandler(this.EditOperation_Click);
            // 
            // NewMinorVersion
            // 
            this.NewMinorVersion.AutoSize = true;
            this.NewMinorVersion.Location = new System.Drawing.Point(5, 346);
            this.NewMinorVersion.Name = "NewMinorVersion";
            this.NewMinorVersion.Size = new System.Drawing.Size(138, 17);
            this.NewMinorVersion.TabIndex = 8;
            this.NewMinorVersion.Text = "Increment minor version";
            this.NewMinorVersion.UseVisualStyleBackColor = true;
            // 
            // ResourceNameFld
            // 
            this.ResourceNameFld.Location = new System.Drawing.Point(112, 25);
            this.ResourceNameFld.Name = "ResourceNameFld";
            this.ResourceNameFld.Size = new System.Drawing.Size(131, 20);
            this.ResourceNameFld.TabIndex = 2;
            this.ResourceNameFld.Leave += new System.EventHandler(this.ResourceNameFld_Leave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(68, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name:";
            // 
            // DocumentationBox
            // 
            this.DocumentationBox.Controls.Add(this.TagNames);
            this.DocumentationBox.Controls.Add(this.label8);
            this.DocumentationBox.Controls.Add(this.ExternalDocURL);
            this.DocumentationBox.Controls.Add(this.label7);
            this.DocumentationBox.Controls.Add(this.label6);
            this.DocumentationBox.Controls.Add(this.ExternalDocDescription);
            this.DocumentationBox.Controls.Add(this.label5);
            this.DocumentationBox.Controls.Add(this.DocDescription);
            this.DocumentationBox.Controls.Add(this.label4);
            this.DocumentationBox.Location = new System.Drawing.Point(5, 204);
            this.DocumentationBox.Name = "DocumentationBox";
            this.DocumentationBox.Size = new System.Drawing.Size(521, 136);
            this.DocumentationBox.TabIndex = 7;
            this.DocumentationBox.TabStop = false;
            this.DocumentationBox.Text = "Documentation properties";
            // 
            // TagNames
            // 
            this.TagNames.Location = new System.Drawing.Point(75, 103);
            this.TagNames.Name = "TagNames";
            this.TagNames.Size = new System.Drawing.Size(427, 20);
            this.TagNames.TabIndex = 4;
            this.TagNames.Leave += new System.EventHandler(this.TagNames_Leave);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(29, 106);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(40, 13);
            this.label8.TabIndex = 0;
            this.label8.Text = "Tag(s):";
            // 
            // ExternalDocURL
            // 
            this.ExternalDocURL.Location = new System.Drawing.Point(299, 77);
            this.ExternalDocURL.Name = "ExternalDocURL";
            this.ExternalDocURL.Size = new System.Drawing.Size(203, 20);
            this.ExternalDocURL.TabIndex = 3;
            this.ExternalDocURL.Leave += new System.EventHandler(this.ExternalDocURL_Leave);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(261, 80);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(32, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "URL:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(245, 35);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(57, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "document:";
            // 
            // ExternalDocDescription
            // 
            this.ExternalDocDescription.Location = new System.Drawing.Point(299, 22);
            this.ExternalDocDescription.Multiline = true;
            this.ExternalDocDescription.Name = "ExternalDocDescription";
            this.ExternalDocDescription.Size = new System.Drawing.Size(203, 46);
            this.ExternalDocDescription.TabIndex = 2;
            this.ExternalDocDescription.Leave += new System.EventHandler(this.ExternalDocDescription_Leave);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(245, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(45, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "External";
            // 
            // DocDescription
            // 
            this.DocDescription.Location = new System.Drawing.Point(75, 22);
            this.DocDescription.Multiline = true;
            this.DocDescription.Name = "DocDescription";
            this.DocDescription.Size = new System.Drawing.Size(164, 75);
            this.DocDescription.TabIndex = 1;
            this.DocDescription.Leave += new System.EventHandler(this.DocDescription_Leave);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Description:";
            // 
            // DefineIdentifierToolTip
            // 
            this.DefineIdentifierToolTip.ToolTipTitle = "Define Identifier";
            // 
            // CreateDocument
            // 
            this.CreateDocument.Image = ((System.Drawing.Image)(resources.GetObject("CreateDocument.Image")));
            this.CreateDocument.Location = new System.Drawing.Point(9, 22);
            this.CreateDocument.Name = "CreateDocument";
            this.CreateDocument.Size = new System.Drawing.Size(25, 25);
            this.CreateDocument.TabIndex = 3;
            this.CreateDocument.UseVisualStyleBackColor = true;
            this.CreateDocument.Click += new System.EventHandler(this.CreateDocument_Click);
            // 
            // LinkDocument
            // 
            this.LinkDocument.Image = ((System.Drawing.Image)(resources.GetObject("LinkDocument.Image")));
            this.LinkDocument.Location = new System.Drawing.Point(40, 22);
            this.LinkDocument.Name = "LinkDocument";
            this.LinkDocument.Size = new System.Drawing.Size(25, 25);
            this.LinkDocument.TabIndex = 10;
            this.LinkDocument.UseVisualStyleBackColor = true;
            this.LinkDocument.Click += new System.EventHandler(this.LinkDocument_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.CreateDocument);
            this.groupBox1.Controls.Add(this.LinkDocument);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.ResourceNameFld);
            this.groupBox1.Location = new System.Drawing.Point(5, 40);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(249, 61);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Document / Profile properties";
            // 
            // RESTResourceDialog
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(534, 387);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.DocumentationBox);
            this.Controls.Add(this.NewMinorVersion);
            this.Controls.Add(this.ResourceTypeBox);
            this.Controls.Add(this.PropertiesBox);
            this.Controls.Add(this.OperationsBox);
            this.Controls.Add(this.PathExpressionLbl);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.Ok);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RESTResourceDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add/Edit Resource";
            this.OperationsBox.ResumeLayout(false);
            this.PropertiesBox.ResumeLayout(false);
            this.PropertiesBox.PerformLayout();
            this.OperationMenuStrip.ResumeLayout(false);
            this.DocumentationBox.ResumeLayout(false);
            this.DocumentationBox.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Label PathExpressionLbl;
        private System.Windows.Forms.GroupBox OperationsBox;
        private System.Windows.Forms.Button EditOperation;
        private System.Windows.Forms.Button DeleteOperation;
        private System.Windows.Forms.Button AddOperation;
        private System.Windows.Forms.ToolTip AddOperationToolTip;
        private System.Windows.Forms.ToolTip DeleteOperationToolTip;
        private System.Windows.Forms.ToolTip EditOperationToolTip;
        private System.Windows.Forms.GroupBox PropertiesBox;
        private System.Windows.Forms.ListView OperationsList;
        private System.Windows.Forms.ColumnHeader OperationName;
        private System.Windows.Forms.ColumnHeader OperationType;
        private System.Windows.Forms.ComboBox ResourceTypeBox;
        private System.Windows.Forms.ContextMenuStrip OperationMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem1;
        private System.Windows.Forms.CheckBox NewMinorVersion;
        private System.Windows.Forms.TextBox ResourceNameFld;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox ParameterName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox DocumentationBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox ExternalDocDescription;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox DocDescription;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox ExternalDocURL;
        private System.Windows.Forms.Button SelectClassifier;
        private System.Windows.Forms.TextBox ParameterClassifier;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ToolTip DefineIdentifierToolTip;
        private System.Windows.Forms.Button CreateDocument;
        private System.Windows.Forms.Button LinkDocument;
        private System.Windows.Forms.TextBox TagNames;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}