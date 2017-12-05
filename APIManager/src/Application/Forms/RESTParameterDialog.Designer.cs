namespace Plugin.Application.Forms
{
    partial class RESTParameterDialog
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
            this.CardinalityGroup = new System.Windows.Forms.GroupBox();
            this.ParameterCardHigh = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.ParameterCardLow = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.ParameterDefaultValue = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.CollectionFormat = new System.Windows.Forms.ComboBox();
            this.ClassifierBox = new System.Windows.Forms.GroupBox();
            this.IsEnum = new System.Windows.Forms.RadioButton();
            this.IsDataType = new System.Windows.Forms.RadioButton();
            this.DescriptionBox = new System.Windows.Forms.GroupBox();
            this.ParamDescription = new System.Windows.Forms.TextBox();
            this.MayBeEmpty = new System.Windows.Forms.CheckBox();
            this.CardinalityGroup.SuspendLayout();
            this.ClassifierBox.SuspendLayout();
            this.DescriptionBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(333, 218);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 31);
            this.Ok.TabIndex = 9;
            this.Ok.Text = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(414, 218);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 8;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name:";
            // 
            // ParameterName
            // 
            this.ParameterName.Location = new System.Drawing.Point(64, 24);
            this.ParameterName.Name = "ParameterName";
            this.ParameterName.Size = new System.Drawing.Size(183, 20);
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
            this.SelectClassifier.TabIndex = 3;
            this.SelectClassifier.Text = "...";
            this.SelectClassifier.UseVisualStyleBackColor = true;
            this.SelectClassifier.Click += new System.EventHandler(this.SelectClassifier_Click);
            // 
            // CardinalityGroup
            // 
            this.CardinalityGroup.Controls.Add(this.ParameterCardHigh);
            this.CardinalityGroup.Controls.Add(this.label4);
            this.CardinalityGroup.Controls.Add(this.ParameterCardLow);
            this.CardinalityGroup.Controls.Add(this.label3);
            this.CardinalityGroup.Location = new System.Drawing.Point(16, 107);
            this.CardinalityGroup.Name = "CardinalityGroup";
            this.CardinalityGroup.Size = new System.Drawing.Size(235, 50);
            this.CardinalityGroup.TabIndex = 5;
            this.CardinalityGroup.TabStop = false;
            this.CardinalityGroup.Text = "Cardinality";
            // 
            // ParameterCardHigh
            // 
            this.ParameterCardHigh.Location = new System.Drawing.Point(179, 16);
            this.ParameterCardHigh.Name = "ParameterCardHigh";
            this.ParameterCardHigh.Size = new System.Drawing.Size(50, 20);
            this.ParameterCardHigh.TabIndex = 2;
            this.ParameterCardHigh.Leave += new System.EventHandler(this.ParameterCard_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(141, 19);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "High:";
            // 
            // ParameterCardLow
            // 
            this.ParameterCardLow.Location = new System.Drawing.Point(42, 16);
            this.ParameterCardLow.Name = "ParameterCardLow";
            this.ParameterCardLow.Size = new System.Drawing.Size(50, 20);
            this.ParameterCardLow.TabIndex = 1;
            this.ParameterCardLow.Leave += new System.EventHandler(this.ParameterCard_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 19);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(30, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Low:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 58);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(73, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Default value:";
            // 
            // ParameterDefaultValue
            // 
            this.ParameterDefaultValue.Location = new System.Drawing.Point(91, 55);
            this.ParameterDefaultValue.Name = "ParameterDefaultValue";
            this.ParameterDefaultValue.Size = new System.Drawing.Size(156, 20);
            this.ParameterDefaultValue.TabIndex = 3;
            this.ParameterDefaultValue.Leave += new System.EventHandler(this.ParameterDefaultValue_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 166);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(88, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "Collection format:";
            // 
            // CollectionFormat
            // 
            this.CollectionFormat.FormattingEnabled = true;
            this.CollectionFormat.Location = new System.Drawing.Point(107, 163);
            this.CollectionFormat.Name = "CollectionFormat";
            this.CollectionFormat.Size = new System.Drawing.Size(140, 21);
            this.CollectionFormat.TabIndex = 6;
            this.CollectionFormat.SelectedIndexChanged += new System.EventHandler(this.CollectionFormat_SelectedIndexChanged);
            // 
            // ClassifierBox
            // 
            this.ClassifierBox.Controls.Add(this.IsEnum);
            this.ClassifierBox.Controls.Add(this.IsDataType);
            this.ClassifierBox.Controls.Add(this.ParameterClassifier);
            this.ClassifierBox.Controls.Add(this.label2);
            this.ClassifierBox.Controls.Add(this.SelectClassifier);
            this.ClassifierBox.Location = new System.Drawing.Point(253, 9);
            this.ClassifierBox.Name = "ClassifierBox";
            this.ClassifierBox.Size = new System.Drawing.Size(235, 69);
            this.ClassifierBox.TabIndex = 2;
            this.ClassifierBox.TabStop = false;
            this.ClassifierBox.Text = "Classifier";
            // 
            // IsEnum
            // 
            this.IsEnum.AutoSize = true;
            this.IsEnum.Location = new System.Drawing.Point(133, 45);
            this.IsEnum.Name = "IsEnum";
            this.IsEnum.Size = new System.Drawing.Size(84, 17);
            this.IsEnum.TabIndex = 2;
            this.IsEnum.TabStop = true;
            this.IsEnum.Text = "Enumeration";
            this.IsEnum.UseVisualStyleBackColor = true;
            // 
            // IsDataType
            // 
            this.IsDataType.AutoSize = true;
            this.IsDataType.Location = new System.Drawing.Point(52, 45);
            this.IsDataType.Name = "IsDataType";
            this.IsDataType.Size = new System.Drawing.Size(75, 17);
            this.IsDataType.TabIndex = 1;
            this.IsDataType.TabStop = true;
            this.IsDataType.Text = "Data Type";
            this.IsDataType.UseVisualStyleBackColor = true;
            // 
            // DescriptionBox
            // 
            this.DescriptionBox.Controls.Add(this.ParamDescription);
            this.DescriptionBox.Location = new System.Drawing.Point(253, 84);
            this.DescriptionBox.Name = "DescriptionBox";
            this.DescriptionBox.Size = new System.Drawing.Size(235, 128);
            this.DescriptionBox.TabIndex = 7;
            this.DescriptionBox.TabStop = false;
            this.DescriptionBox.Text = "Description";
            // 
            // ParamDescription
            // 
            this.ParamDescription.Location = new System.Drawing.Point(11, 16);
            this.ParamDescription.Multiline = true;
            this.ParamDescription.Name = "ParamDescription";
            this.ParamDescription.Size = new System.Drawing.Size(218, 99);
            this.ParamDescription.TabIndex = 1;
            this.ParamDescription.Leave += new System.EventHandler(this.ParamDescription_Leave);
            // 
            // MayBeEmpty
            // 
            this.MayBeEmpty.AutoSize = true;
            this.MayBeEmpty.Location = new System.Drawing.Point(91, 84);
            this.MayBeEmpty.Name = "MayBeEmpty";
            this.MayBeEmpty.Size = new System.Drawing.Size(111, 17);
            this.MayBeEmpty.TabIndex = 4;
            this.MayBeEmpty.Text = "Allow empty value";
            this.MayBeEmpty.UseVisualStyleBackColor = true;
            this.MayBeEmpty.CheckedChanged += new System.EventHandler(this.MayBeEmpty_CheckedChanged);
            // 
            // RESTParameterDialog
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(501, 257);
            this.Controls.Add(this.MayBeEmpty);
            this.Controls.Add(this.DescriptionBox);
            this.Controls.Add(this.ClassifierBox);
            this.Controls.Add(this.CollectionFormat);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.ParameterDefaultValue);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.CardinalityGroup);
            this.Controls.Add(this.ParameterName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.Ok);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RESTParameterDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add/Edit Parameter";
            this.CardinalityGroup.ResumeLayout(false);
            this.CardinalityGroup.PerformLayout();
            this.ClassifierBox.ResumeLayout(false);
            this.ClassifierBox.PerformLayout();
            this.DescriptionBox.ResumeLayout(false);
            this.DescriptionBox.PerformLayout();
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
        private System.Windows.Forms.GroupBox CardinalityGroup;
        private System.Windows.Forms.TextBox ParameterCardHigh;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox ParameterCardLow;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox ParameterDefaultValue;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox CollectionFormat;
        private System.Windows.Forms.GroupBox ClassifierBox;
        private System.Windows.Forms.RadioButton IsEnum;
        private System.Windows.Forms.RadioButton IsDataType;
        private System.Windows.Forms.GroupBox DescriptionBox;
        private System.Windows.Forms.TextBox ParamDescription;
        private System.Windows.Forms.CheckBox MayBeEmpty;
    }
}