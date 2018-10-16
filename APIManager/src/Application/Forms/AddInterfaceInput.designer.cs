using System.Windows.Forms;

namespace Plugin.Application.Forms
{
    partial class AddInterfaceInput
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
            this.OperationNamesLbl = new System.Windows.Forms.Label();
            this.InterfaceNameFld = new System.Windows.Forms.TextBox();
            this.ErrorLine = new System.Windows.Forms.TextBox();
            this.NewMinorVersion = new System.Windows.Forms.CheckBox();
            this.OperationList = new System.Windows.Forms.CheckedListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.All = new System.Windows.Forms.Button();
            this.None = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(341, 344);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 4;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(260, 344);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 31);
            this.Ok.TabIndex = 5;
            this.Ok.Text = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            // 
            // OperationNamesLbl
            // 
            this.OperationNamesLbl.AutoSize = true;
            this.OperationNamesLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OperationNamesLbl.Location = new System.Drawing.Point(12, 9);
            this.OperationNamesLbl.Name = "OperationNamesLbl";
            this.OperationNamesLbl.Size = new System.Drawing.Size(108, 17);
            this.OperationNamesLbl.TabIndex = 0;
            this.OperationNamesLbl.Text = "Interface Name:";
            // 
            // InterfaceNameFld
            // 
            this.InterfaceNameFld.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.InterfaceNameFld.Location = new System.Drawing.Point(126, 9);
            this.InterfaceNameFld.Name = "InterfaceNameFld";
            this.InterfaceNameFld.Size = new System.Drawing.Size(292, 23);
            this.InterfaceNameFld.TabIndex = 1;
            this.InterfaceNameFld.Leave += new System.EventHandler(this.OperationNames_TextChanged);
            // 
            // ErrorLine
            // 
            this.ErrorLine.BackColor = System.Drawing.SystemColors.Control;
            this.ErrorLine.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ErrorLine.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ErrorLine.ForeColor = System.Drawing.Color.Red;
            this.ErrorLine.Location = new System.Drawing.Point(15, 45);
            this.ErrorLine.Multiline = true;
            this.ErrorLine.Name = "ErrorLine";
            this.ErrorLine.Size = new System.Drawing.Size(403, 39);
            this.ErrorLine.TabIndex = 0;
            // 
            // NewMinorVersion
            // 
            this.NewMinorVersion.AutoSize = true;
            this.NewMinorVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NewMinorVersion.Location = new System.Drawing.Point(15, 350);
            this.NewMinorVersion.Name = "NewMinorVersion";
            this.NewMinorVersion.Size = new System.Drawing.Size(168, 20);
            this.NewMinorVersion.TabIndex = 3;
            this.NewMinorVersion.Text = "Increment minor version";
            this.NewMinorVersion.UseVisualStyleBackColor = true;
            this.NewMinorVersion.CheckedChanged += new System.EventHandler(this.NewMinorVersion_CheckedChanged);
            // 
            // OperationList
            // 
            this.OperationList.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OperationList.FormattingEnabled = true;
            this.OperationList.Location = new System.Drawing.Point(15, 107);
            this.OperationList.Name = "OperationList";
            this.OperationList.Size = new System.Drawing.Size(401, 191);
            this.OperationList.TabIndex = 2;
            this.OperationList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.OperationList_ItemCheck);
            this.OperationList.SelectedValueChanged += new System.EventHandler(this.OperationList_SelectedValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 87);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(321, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select Operations to associate with the new Interface:";
            // 
            // All
            // 
            this.All.Location = new System.Drawing.Point(15, 305);
            this.All.Name = "All";
            this.All.Size = new System.Drawing.Size(50, 31);
            this.All.TabIndex = 6;
            this.All.Text = "All";
            this.All.UseVisualStyleBackColor = true;
            this.All.Click += new System.EventHandler(this.All_Click);
            // 
            // None
            // 
            this.None.Location = new System.Drawing.Point(71, 305);
            this.None.Name = "None";
            this.None.Size = new System.Drawing.Size(50, 31);
            this.None.TabIndex = 7;
            this.None.Text = "None";
            this.None.UseVisualStyleBackColor = true;
            this.None.Click += new System.EventHandler(this.None_Click);
            // 
            // AddInterfaceInput
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(431, 387);
            this.Controls.Add(this.None);
            this.Controls.Add(this.All);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.OperationList);
            this.Controls.Add(this.NewMinorVersion);
            this.Controls.Add(this.ErrorLine);
            this.Controls.Add(this.InterfaceNameFld);
            this.Controls.Add(this.OperationNamesLbl);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.Cancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddInterfaceInput";
            this.Text = "Add a new Interface";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.Label OperationNamesLbl;
        private System.Windows.Forms.TextBox InterfaceNameFld;
        private System.Windows.Forms.TextBox ErrorLine;
        private CheckBox NewMinorVersion;
        private CheckedListBox OperationList;
        private Label label1;
        private Button All;
        private Button None;
    }
}