﻿namespace Plugin.Application.Forms
{
    partial class CodeListPicker
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
            this.Label = new System.Windows.Forms.Label();
            this.CodeListSet = new System.Windows.Forms.TreeView();
            this.Cancel = new System.Windows.Forms.Button();
            this.Ok = new System.Windows.Forms.Button();
            this.All = new System.Windows.Forms.Button();
            this.None = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Label
            // 
            this.Label.AutoSize = true;
            this.Label.CausesValidation = false;
            this.Label.Enabled = false;
            this.Label.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label.Location = new System.Drawing.Point(6, 6);
            this.Label.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.Label.Name = "Label";
            this.Label.Size = new System.Drawing.Size(187, 17);
            this.Label.TabIndex = 0;
            this.Label.Text = "Select CodeLists to process:";
            // 
            // CodeListSet
            // 
            this.CodeListSet.CausesValidation = false;
            this.CodeListSet.CheckBoxes = true;
            this.CodeListSet.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CodeListSet.Location = new System.Drawing.Point(6, 32);
            this.CodeListSet.Margin = new System.Windows.Forms.Padding(2);
            this.CodeListSet.Name = "CodeListSet";
            this.CodeListSet.Size = new System.Drawing.Size(361, 201);
            this.CodeListSet.TabIndex = 1;
            this.CodeListSet.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.CodeListSet_AfterCheck);
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(292, 238);
            this.Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 2;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(209, 238);
            this.Ok.Margin = new System.Windows.Forms.Padding(2);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 31);
            this.Ok.TabIndex = 3;
            this.Ok.Text = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            // 
            // All
            // 
            this.All.Location = new System.Drawing.Point(6, 238);
            this.All.Name = "All";
            this.All.Size = new System.Drawing.Size(50, 31);
            this.All.TabIndex = 4;
            this.All.Text = "All";
            this.All.UseVisualStyleBackColor = true;
            this.All.Click += new System.EventHandler(this.All_Click);
            // 
            // None
            // 
            this.None.Location = new System.Drawing.Point(62, 238);
            this.None.Name = "None";
            this.None.Size = new System.Drawing.Size(50, 31);
            this.None.TabIndex = 5;
            this.None.Text = "None";
            this.None.UseVisualStyleBackColor = true;
            this.None.Click += new System.EventHandler(this.None_Click);
            // 
            // CodeListPicker
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(372, 278);
            this.Controls.Add(this.None);
            this.Controls.Add(this.All);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.CodeListSet);
            this.Controls.Add(this.Label);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CodeListPicker";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CodeList Chooser";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label Label;
        private System.Windows.Forms.TreeView CodeListSet;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.Button All;
        private System.Windows.Forms.Button None;
    }
}