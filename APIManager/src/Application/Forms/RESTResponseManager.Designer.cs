namespace APIManager.SparxEA.src.Application.Forms
{
    partial class RESTResponseManager
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RESTResponseManager));
            this.ResponseGroup = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ResultCodeComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.RspClassifierName = new System.Windows.Forms.TextBox();
            this.ClassifierSelector = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.RspCommentBox = new System.Windows.Forms.TextBox();
            this.ResponseEntries = new System.Windows.Forms.ListBox();
            this.RspAdd = new System.Windows.Forms.Button();
            this.RspDelete = new System.Windows.Forms.Button();
            this.ResponseHeaders = new System.Windows.Forms.GroupBox();
            this.ResponseHeaderEntries = new System.Windows.Forms.ListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.HdrName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.HdrClassifierName = new System.Windows.Forms.TextBox();
            this.HdrClassifierSelector = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.HdrCommentBox = new System.Windows.Forms.TextBox();
            this.HdrAdd = new System.Windows.Forms.Button();
            this.HdrDelete = new System.Windows.Forms.Button();
            this.ResponseGroup.SuspendLayout();
            this.ResponseHeaders.SuspendLayout();
            this.SuspendLayout();
            // 
            // ResponseGroup
            // 
            this.ResponseGroup.Controls.Add(this.ResponseHeaders);
            this.ResponseGroup.Controls.Add(this.RspDelete);
            this.ResponseGroup.Controls.Add(this.RspAdd);
            this.ResponseGroup.Controls.Add(this.ResponseEntries);
            this.ResponseGroup.Controls.Add(this.RspCommentBox);
            this.ResponseGroup.Controls.Add(this.label3);
            this.ResponseGroup.Controls.Add(this.ClassifierSelector);
            this.ResponseGroup.Controls.Add(this.RspClassifierName);
            this.ResponseGroup.Controls.Add(this.label2);
            this.ResponseGroup.Controls.Add(this.ResultCodeComboBox);
            this.ResponseGroup.Controls.Add(this.label1);
            this.ResponseGroup.Location = new System.Drawing.Point(13, 26);
            this.ResponseGroup.Name = "ResponseGroup";
            this.ResponseGroup.Size = new System.Drawing.Size(568, 270);
            this.ResponseGroup.TabIndex = 0;
            this.ResponseGroup.TabStop = false;
            this.ResponseGroup.Text = "Operation Responses";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Result Code:";
            // 
            // ResultCodeComboBox
            // 
            this.ResultCodeComboBox.FormattingEnabled = true;
            this.ResultCodeComboBox.Location = new System.Drawing.Point(80, 28);
            this.ResultCodeComboBox.Name = "ResultCodeComboBox";
            this.ResultCodeComboBox.Size = new System.Drawing.Size(121, 21);
            this.ResultCodeComboBox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Classifier:";
            // 
            // RspClassifierName
            // 
            this.RspClassifierName.Location = new System.Drawing.Point(80, 54);
            this.RspClassifierName.Name = "RspClassifierName";
            this.RspClassifierName.ReadOnly = true;
            this.RspClassifierName.Size = new System.Drawing.Size(120, 20);
            this.RspClassifierName.TabIndex = 3;
            // 
            // ClassifierSelector
            // 
            this.ClassifierSelector.Location = new System.Drawing.Point(206, 54);
            this.ClassifierSelector.Name = "ClassifierSelector";
            this.ClassifierSelector.Size = new System.Drawing.Size(29, 20);
            this.ClassifierSelector.TabIndex = 4;
            this.ClassifierSelector.Text = "...";
            this.ClassifierSelector.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Comment:";
            // 
            // RspCommentBox
            // 
            this.RspCommentBox.Location = new System.Drawing.Point(81, 81);
            this.RspCommentBox.Name = "RspCommentBox";
            this.RspCommentBox.Size = new System.Drawing.Size(154, 20);
            this.RspCommentBox.TabIndex = 6;
            // 
            // ResponseEntries
            // 
            this.ResponseEntries.FormattingEnabled = true;
            this.ResponseEntries.Location = new System.Drawing.Point(283, 19);
            this.ResponseEntries.MultiColumn = true;
            this.ResponseEntries.Name = "ResponseEntries";
            this.ResponseEntries.Size = new System.Drawing.Size(267, 95);
            this.ResponseEntries.TabIndex = 7;
            // 
            // RspAdd
            // 
            this.RspAdd.Image = ((System.Drawing.Image)(resources.GetObject("RspAdd.Image")));
            this.RspAdd.Location = new System.Drawing.Point(252, 41);
            this.RspAdd.Name = "RspAdd";
            this.RspAdd.Size = new System.Drawing.Size(25, 25);
            this.RspAdd.TabIndex = 8;
            this.RspAdd.UseVisualStyleBackColor = true;
            // 
            // RspDelete
            // 
            this.RspDelete.Image = ((System.Drawing.Image)(resources.GetObject("RspDelete.Image")));
            this.RspDelete.Location = new System.Drawing.Point(252, 72);
            this.RspDelete.Name = "RspDelete";
            this.RspDelete.Size = new System.Drawing.Size(25, 25);
            this.RspDelete.TabIndex = 9;
            this.RspDelete.UseVisualStyleBackColor = true;
            // 
            // ResponseHeaders
            // 
            this.ResponseHeaders.Controls.Add(this.HdrDelete);
            this.ResponseHeaders.Controls.Add(this.HdrAdd);
            this.ResponseHeaders.Controls.Add(this.HdrCommentBox);
            this.ResponseHeaders.Controls.Add(this.label6);
            this.ResponseHeaders.Controls.Add(this.HdrClassifierSelector);
            this.ResponseHeaders.Controls.Add(this.HdrClassifierName);
            this.ResponseHeaders.Controls.Add(this.label5);
            this.ResponseHeaders.Controls.Add(this.HdrName);
            this.ResponseHeaders.Controls.Add(this.label4);
            this.ResponseHeaders.Controls.Add(this.ResponseHeaderEntries);
            this.ResponseHeaders.Location = new System.Drawing.Point(9, 132);
            this.ResponseHeaders.Name = "ResponseHeaders";
            this.ResponseHeaders.Size = new System.Drawing.Size(553, 123);
            this.ResponseHeaders.TabIndex = 10;
            this.ResponseHeaders.TabStop = false;
            this.ResponseHeaders.Text = "Response Headers";
            // 
            // ResponseHeaderEntries
            // 
            this.ResponseHeaderEntries.FormattingEnabled = true;
            this.ResponseHeaderEntries.Location = new System.Drawing.Point(274, 19);
            this.ResponseHeaderEntries.MultiColumn = true;
            this.ResponseHeaderEntries.Name = "ResponseHeaderEntries";
            this.ResponseHeaderEntries.Size = new System.Drawing.Size(267, 95);
            this.ResponseHeaderEntries.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(27, 20);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Name:";
            // 
            // HdrName
            // 
            this.HdrName.Location = new System.Drawing.Point(71, 20);
            this.HdrName.Name = "HdrName";
            this.HdrName.Size = new System.Drawing.Size(154, 20);
            this.HdrName.TabIndex = 2;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 47);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "Classifier:";
            // 
            // HdrClassifierName
            // 
            this.HdrClassifierName.Location = new System.Drawing.Point(71, 46);
            this.HdrClassifierName.Name = "HdrClassifierName";
            this.HdrClassifierName.ReadOnly = true;
            this.HdrClassifierName.Size = new System.Drawing.Size(120, 20);
            this.HdrClassifierName.TabIndex = 4;
            // 
            // HdrClassifierSelector
            // 
            this.HdrClassifierSelector.Location = new System.Drawing.Point(197, 47);
            this.HdrClassifierSelector.Name = "HdrClassifierSelector";
            this.HdrClassifierSelector.Size = new System.Drawing.Size(29, 20);
            this.HdrClassifierSelector.TabIndex = 5;
            this.HdrClassifierSelector.Text = "...";
            this.HdrClassifierSelector.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(11, 75);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(54, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Comment:";
            // 
            // HdrCommentBox
            // 
            this.HdrCommentBox.Location = new System.Drawing.Point(71, 72);
            this.HdrCommentBox.Name = "HdrCommentBox";
            this.HdrCommentBox.Size = new System.Drawing.Size(155, 20);
            this.HdrCommentBox.TabIndex = 7;
            // 
            // HdrAdd
            // 
            this.HdrAdd.Image = ((System.Drawing.Image)(resources.GetObject("HdrAdd.Image")));
            this.HdrAdd.Location = new System.Drawing.Point(243, 36);
            this.HdrAdd.Name = "HdrAdd";
            this.HdrAdd.Size = new System.Drawing.Size(25, 25);
            this.HdrAdd.TabIndex = 8;
            this.HdrAdd.UseVisualStyleBackColor = true;
            // 
            // HdrDelete
            // 
            this.HdrDelete.Image = ((System.Drawing.Image)(resources.GetObject("HdrDelete.Image")));
            this.HdrDelete.Location = new System.Drawing.Point(243, 67);
            this.HdrDelete.Name = "HdrDelete";
            this.HdrDelete.Size = new System.Drawing.Size(25, 25);
            this.HdrDelete.TabIndex = 9;
            this.HdrDelete.UseVisualStyleBackColor = true;
            // 
            // RESTResponseManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(593, 336);
            this.Controls.Add(this.ResponseGroup);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RESTResponseManager";
            this.Text = "RESTResponseManager";
            this.ResponseGroup.ResumeLayout(false);
            this.ResponseGroup.PerformLayout();
            this.ResponseHeaders.ResumeLayout(false);
            this.ResponseHeaders.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox ResponseGroup;
        private System.Windows.Forms.Button RspAdd;
        private System.Windows.Forms.ListBox ResponseEntries;
        private System.Windows.Forms.TextBox RspCommentBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button ClassifierSelector;
        private System.Windows.Forms.TextBox RspClassifierName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox ResultCodeComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button RspDelete;
        private System.Windows.Forms.GroupBox ResponseHeaders;
        private System.Windows.Forms.Button HdrClassifierSelector;
        private System.Windows.Forms.TextBox HdrClassifierName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox HdrName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListBox ResponseHeaderEntries;
        private System.Windows.Forms.Button HdrDelete;
        private System.Windows.Forms.Button HdrAdd;
        private System.Windows.Forms.TextBox HdrCommentBox;
        private System.Windows.Forms.Label label6;
    }
}