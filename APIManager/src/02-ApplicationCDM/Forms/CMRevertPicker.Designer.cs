﻿namespace Plugin.Application.Forms
{
    partial class CMRevertPicker
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
            this.DoCreateNewVersion = new System.Windows.Forms.CheckBox();
            this.FeatureTags = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(187, 253);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 3;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(106, 253);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 31);
            this.Ok.TabIndex = 4;
            this.Ok.Text = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            // 
            // DoCreateNewVersion
            // 
            this.DoCreateNewVersion.AutoSize = true;
            this.DoCreateNewVersion.Location = new System.Drawing.Point(13, 230);
            this.DoCreateNewVersion.Name = "DoCreateNewVersion";
            this.DoCreateNewVersion.Size = new System.Drawing.Size(117, 17);
            this.DoCreateNewVersion.TabIndex = 2;
            this.DoCreateNewVersion.Text = "Create new version";
            this.DoCreateNewVersion.UseVisualStyleBackColor = true;
            // 
            // FeatureTags
            // 
            this.FeatureTags.Location = new System.Drawing.Point(13, 12);
            this.FeatureTags.Name = "FeatureTags";
            this.FeatureTags.PathSeparator = "/";
            this.FeatureTags.Size = new System.Drawing.Size(249, 212);
            this.FeatureTags.TabIndex = 1;
            this.FeatureTags.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.FeatureTags_AfterSelect);
            // 
            // CMRevertPicker
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(276, 296);
            this.Controls.Add(this.FeatureTags);
            this.Controls.Add(this.DoCreateNewVersion);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.Cancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CMRevertPicker";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Select Release to revert to";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.CheckBox DoCreateNewVersion;
        private System.Windows.Forms.TreeView FeatureTags;
    }
}