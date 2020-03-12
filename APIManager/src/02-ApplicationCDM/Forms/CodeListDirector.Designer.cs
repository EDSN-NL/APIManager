namespace Plugin.Application.Forms
{
    partial class CodeListDirector
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
            this.TopLabel = new System.Windows.Forms.Label();
            this.CodeListProgress = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(440, 420);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 2;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(359, 420);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 31);
            this.Ok.TabIndex = 1;
            this.Ok.Text = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            // 
            // TopLabel
            // 
            this.TopLabel.AutoSize = true;
            this.TopLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TopLabel.Location = new System.Drawing.Point(9, 9);
            this.TopLabel.Name = "TopLabel";
            this.TopLabel.Size = new System.Drawing.Size(139, 16);
            this.TopLabel.TabIndex = 0;
            this.TopLabel.Text = "Code List Declaration:";
            // 
            // CodeListProgress
            // 
            this.CodeListProgress.CheckBoxes = true;
            this.CodeListProgress.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CodeListProgress.Location = new System.Drawing.Point(12, 30);
            this.CodeListProgress.Name = "CodeListProgress";
            this.CodeListProgress.Size = new System.Drawing.Size(502, 384);
            this.CodeListProgress.TabIndex = 0;
            this.CodeListProgress.BeforeCheck += new System.Windows.Forms.TreeViewCancelEventHandler(this.CodeListProgress_BeforeCheck);
            this.CodeListProgress.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.CodeListProgress_AfterCheck);
            this.CodeListProgress.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.CodeListProgress_NodeMouseClick);
            // 
            // CodeListDirector
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(526, 463);
            this.Controls.Add(this.CodeListProgress);
            this.Controls.Add(this.TopLabel);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.Cancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CodeListDirector";
            this.Text = "CodeListDirector";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.Label TopLabel;
        private System.Windows.Forms.TreeView CodeListProgress;
    }
}