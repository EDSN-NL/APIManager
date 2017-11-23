namespace Plugin.Application.Forms
{
    partial class CollectAnnotation
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
            this.AnnotationFld = new System.Windows.Forms.TextBox();
            this.OperationTree = new System.Windows.Forms.TreeView();
            this.AnnotationLabel = new System.Windows.Forms.Label();
            this.OperationsLabel = new System.Windows.Forms.Label();
            this.NewMinorVersion = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(518, 293);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 4;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(437, 293);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 31);
            this.Ok.TabIndex = 5;
            this.Ok.Text = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            // 
            // AnnotationFld
            // 
            this.AnnotationFld.AcceptsReturn = true;
            this.AnnotationFld.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AnnotationFld.Location = new System.Drawing.Point(12, 200);
            this.AnnotationFld.Multiline = true;
            this.AnnotationFld.Name = "AnnotationFld";
            this.AnnotationFld.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.AnnotationFld.Size = new System.Drawing.Size(581, 78);
            this.AnnotationFld.TabIndex = 2;
            // 
            // OperationTree
            // 
            this.OperationTree.CheckBoxes = true;
            this.OperationTree.Location = new System.Drawing.Point(12, 31);
            this.OperationTree.Name = "OperationTree";
            this.OperationTree.Size = new System.Drawing.Size(581, 143);
            this.OperationTree.TabIndex = 1;
            this.OperationTree.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.OperationTree_AfterCheck);
            // 
            // AnnotationLabel
            // 
            this.AnnotationLabel.AutoSize = true;
            this.AnnotationLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AnnotationLabel.Location = new System.Drawing.Point(12, 178);
            this.AnnotationLabel.Name = "AnnotationLabel";
            this.AnnotationLabel.Size = new System.Drawing.Size(141, 16);
            this.AnnotationLabel.TabIndex = 0;
            this.AnnotationLabel.Text = "Description of change:";
            // 
            // OperationsLabel
            // 
            this.OperationsLabel.AutoSize = true;
            this.OperationsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OperationsLabel.Location = new System.Drawing.Point(12, 9);
            this.OperationsLabel.Name = "OperationsLabel";
            this.OperationsLabel.Size = new System.Drawing.Size(132, 16);
            this.OperationsLabel.TabIndex = 0;
            this.OperationsLabel.Text = "Selected operations:";
            // 
            // NewMinorVersion
            // 
            this.NewMinorVersion.AutoSize = true;
            this.NewMinorVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NewMinorVersion.Location = new System.Drawing.Point(15, 299);
            this.NewMinorVersion.Name = "NewMinorVersion";
            this.NewMinorVersion.Size = new System.Drawing.Size(168, 20);
            this.NewMinorVersion.TabIndex = 5;
            this.NewMinorVersion.Text = "Increment minor version";
            this.NewMinorVersion.UseVisualStyleBackColor = true;
            this.NewMinorVersion.CheckedChanged += new System.EventHandler(this.NewMinorVersion_CheckedChanged);
            // 
            // CollectAnnotation
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(608, 334);
            this.Controls.Add(this.NewMinorVersion);
            this.Controls.Add(this.OperationsLabel);
            this.Controls.Add(this.AnnotationLabel);
            this.Controls.Add(this.OperationTree);
            this.Controls.Add(this.AnnotationFld);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.Cancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CollectAnnotation";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Change log";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.TextBox AnnotationFld;
        private System.Windows.Forms.TreeView OperationTree;
        private System.Windows.Forms.Label AnnotationLabel;
        private System.Windows.Forms.Label OperationsLabel;
        private System.Windows.Forms.CheckBox NewMinorVersion;
    }
}