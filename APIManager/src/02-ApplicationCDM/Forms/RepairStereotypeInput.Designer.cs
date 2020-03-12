namespace Plugin.Application.Forms
{
    partial class RepairStereotypeInput
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
            this.label1 = new System.Windows.Forms.Label();
            this.SelectedProfileName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SelectedStereotype = new System.Windows.Forms.TextBox();
            this.EntireHierarchy = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(142, 81);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 0;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(61, 81);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 31);
            this.Ok.TabIndex = 1;
            this.Ok.Text = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Profile name:";
            // 
            // SelectedProfileName
            // 
            this.SelectedProfileName.Location = new System.Drawing.Point(77, 6);
            this.SelectedProfileName.Name = "SelectedProfileName";
            this.SelectedProfileName.Size = new System.Drawing.Size(140, 20);
            this.SelectedProfileName.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Stereotype:";
            // 
            // SelectedStereotype
            // 
            this.SelectedStereotype.Location = new System.Drawing.Point(77, 32);
            this.SelectedStereotype.Name = "SelectedStereotype";
            this.SelectedStereotype.Size = new System.Drawing.Size(140, 20);
            this.SelectedStereotype.TabIndex = 5;
            this.SelectedStereotype.Leave += new System.EventHandler(this.SelectedStereotype_Leave);
            // 
            // EntireHierarchy
            // 
            this.EntireHierarchy.AutoSize = true;
            this.EntireHierarchy.Location = new System.Drawing.Point(6, 58);
            this.EntireHierarchy.Name = "EntireHierarchy";
            this.EntireHierarchy.Size = new System.Drawing.Size(136, 17);
            this.EntireHierarchy.TabIndex = 6;
            this.EntireHierarchy.Text = "Include child packages";
            this.EntireHierarchy.UseVisualStyleBackColor = true;
            // 
            // RepairStereotypeInput
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(227, 122);
            this.Controls.Add(this.EntireHierarchy);
            this.Controls.Add(this.SelectedStereotype);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.SelectedProfileName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.Cancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RepairStereotypeInput";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "RepairStereotypeInput";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox SelectedProfileName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox SelectedStereotype;
        private System.Windows.Forms.CheckBox EntireHierarchy;
    }
}