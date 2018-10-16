namespace Plugin.Application.Forms
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
            this.TagList = new System.Windows.Forms.ListBox();
            this.VersionBox = new System.Windows.Forms.GroupBox();
            this.BuildNumber = new System.Windows.Forms.TextBox();
            this.MinorVersion = new System.Windows.Forms.TextBox();
            this.MajorVersion = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.VersionBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(303, 355);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 3;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(222, 355);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 31);
            this.Ok.TabIndex = 4;
            this.Ok.Text = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            // 
            // TagList
            // 
            this.TagList.FormattingEnabled = true;
            this.TagList.Location = new System.Drawing.Point(13, 13);
            this.TagList.Name = "TagList";
            this.TagList.Size = new System.Drawing.Size(365, 277);
            this.TagList.Sorted = true;
            this.TagList.TabIndex = 1;
            this.TagList.SelectedValueChanged += new System.EventHandler(this.TagList_SelectedValueChanged);
            // 
            // VersionBox
            // 
            this.VersionBox.Controls.Add(this.BuildNumber);
            this.VersionBox.Controls.Add(this.MinorVersion);
            this.VersionBox.Controls.Add(this.MajorVersion);
            this.VersionBox.Controls.Add(this.label3);
            this.VersionBox.Controls.Add(this.label2);
            this.VersionBox.Controls.Add(this.label1);
            this.VersionBox.Location = new System.Drawing.Point(13, 296);
            this.VersionBox.Name = "VersionBox";
            this.VersionBox.Size = new System.Drawing.Size(365, 52);
            this.VersionBox.TabIndex = 2;
            this.VersionBox.TabStop = false;
            this.VersionBox.Text = "Assign version";
            // 
            // BuildNumber
            // 
            this.BuildNumber.Location = new System.Drawing.Point(221, 19);
            this.BuildNumber.Name = "BuildNumber";
            this.BuildNumber.Size = new System.Drawing.Size(40, 20);
            this.BuildNumber.TabIndex = 3;
            // 
            // MinorVersion
            // 
            this.MinorVersion.Location = new System.Drawing.Point(136, 20);
            this.MinorVersion.Name = "MinorVersion";
            this.MinorVersion.Size = new System.Drawing.Size(40, 20);
            this.MinorVersion.TabIndex = 2;
            // 
            // MajorVersion
            // 
            this.MajorVersion.Location = new System.Drawing.Point(48, 20);
            this.MajorVersion.Name = "MajorVersion";
            this.MajorVersion.Size = new System.Drawing.Size(40, 20);
            this.MajorVersion.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(182, 23);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(33, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Build:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(94, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Minor:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Major:";
            // 
            // CMRevertPicker
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(390, 398);
            this.Controls.Add(this.VersionBox);
            this.Controls.Add(this.TagList);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.Cancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CMRevertPicker";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Select Release to switch to";
            this.VersionBox.ResumeLayout(false);
            this.VersionBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.ListBox TagList;
        private System.Windows.Forms.GroupBox VersionBox;
        private System.Windows.Forms.TextBox BuildNumber;
        private System.Windows.Forms.TextBox MinorVersion;
        private System.Windows.Forms.TextBox MajorVersion;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
    }
}