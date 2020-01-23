namespace Plugin.Application.Forms
{
    partial class RESTHeaderParameterDialog
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
            this.Ok = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.ParameterName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ParameterClassifier = new System.Windows.Forms.TextBox();
            this.SelectClassifier = new System.Windows.Forms.Button();
            this.ClassifierBox = new System.Windows.Forms.GroupBox();
            this.ParamDescription = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ClassifierBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(333, 150);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 31);
            this.Ok.TabIndex = 5;
            this.Ok.Text = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(414, 150);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 4;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(37, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name:";
            // 
            // ParameterName
            // 
            this.ParameterName.Location = new System.Drawing.Point(81, 27);
            this.ParameterName.Name = "ParameterName";
            this.ParameterName.Size = new System.Drawing.Size(166, 20);
            this.ParameterName.TabIndex = 1;
            this.ParameterName.Leave += new System.EventHandler(this.ParameterName_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Select:";
            // 
            // ParameterClassifier
            // 
            this.ParameterClassifier.Location = new System.Drawing.Point(52, 19);
            this.ParameterClassifier.Name = "ParameterClassifier";
            this.ParameterClassifier.ReadOnly = true;
            this.ParameterClassifier.Size = new System.Drawing.Size(146, 20);
            this.ParameterClassifier.TabIndex = 0;
            // 
            // SelectClassifier
            // 
            this.SelectClassifier.Location = new System.Drawing.Point(204, 19);
            this.SelectClassifier.Name = "SelectClassifier";
            this.SelectClassifier.Size = new System.Drawing.Size(25, 21);
            this.SelectClassifier.TabIndex = 1;
            this.SelectClassifier.Text = "...";
            this.SelectClassifier.UseVisualStyleBackColor = true;
            this.SelectClassifier.Click += new System.EventHandler(this.SelectClassifier_Click);
            // 
            // ClassifierBox
            // 
            this.ClassifierBox.Controls.Add(this.ParameterClassifier);
            this.ClassifierBox.Controls.Add(this.label2);
            this.ClassifierBox.Controls.Add(this.SelectClassifier);
            this.ClassifierBox.Location = new System.Drawing.Point(253, 9);
            this.ClassifierBox.Name = "ClassifierBox";
            this.ClassifierBox.Size = new System.Drawing.Size(235, 53);
            this.ClassifierBox.TabIndex = 2;
            this.ClassifierBox.TabStop = false;
            this.ClassifierBox.Text = "Classifier (CDT or BDT)";
            // 
            // ParamDescription
            // 
            this.ParamDescription.Location = new System.Drawing.Point(81, 68);
            this.ParamDescription.Multiline = true;
            this.ParamDescription.Name = "ParamDescription";
            this.ParamDescription.Size = new System.Drawing.Size(408, 76);
            this.ParamDescription.TabIndex = 3;
            this.ParamDescription.Leave += new System.EventHandler(this.ParamDescription_Leave);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Description:";
            // 
            // RESTHeaderParameterDialog
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(501, 193);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.ParamDescription);
            this.Controls.Add(this.ClassifierBox);
            this.Controls.Add(this.ParameterName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.Ok);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RESTHeaderParameterDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add/Edit Header Parameter";
            this.ClassifierBox.ResumeLayout(false);
            this.ClassifierBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox ParameterName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox ParameterClassifier;
        private System.Windows.Forms.Button SelectClassifier;
        private System.Windows.Forms.GroupBox ClassifierBox;
        private System.Windows.Forms.TextBox ParamDescription;
        private System.Windows.Forms.Label label3;
    }
}