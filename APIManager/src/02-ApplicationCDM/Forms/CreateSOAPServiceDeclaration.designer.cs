﻿using System.Windows.Forms;

namespace Plugin.Application.Forms
{
    partial class CreateSOAPServiceDeclaration
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
            this.PackageDeclarationLbl = new System.Windows.Forms.Label();
            this.ServiceNameFld = new System.Windows.Forms.TextBox();
            this.OperationNamesLbl = new System.Windows.Forms.Label();
            this.CommaSeparatedLbl = new System.Windows.Forms.Label();
            this.OperationNames = new System.Windows.Forms.TextBox();
            this.Cancel = new System.Windows.Forms.Button();
            this.Ok = new System.Windows.Forms.Button();
            this.ErrorLine = new System.Windows.Forms.TextBox();
            this.TicketBox = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.ProjectIDFld = new System.Windows.Forms.TextBox();
            this.TicketIDFld = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.ServiceDetailsBox = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.OperationalState = new System.Windows.Forms.ComboBox();
            this.UseSOAPFaultsIndicator = new System.Windows.Forms.CheckBox();
            this.UseListElementsIndicator = new System.Windows.Forms.CheckBox();
            this.TicketBox.SuspendLayout();
            this.ServiceDetailsBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // PackageDeclarationLbl
            // 
            this.PackageDeclarationLbl.AutoSize = true;
            this.PackageDeclarationLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PackageDeclarationLbl.Location = new System.Drawing.Point(21, 20);
            this.PackageDeclarationLbl.Name = "PackageDeclarationLbl";
            this.PackageDeclarationLbl.Size = new System.Drawing.Size(77, 13);
            this.PackageDeclarationLbl.TabIndex = 0;
            this.PackageDeclarationLbl.Text = "Service Name:";
            // 
            // ServiceNameFld
            // 
            this.ServiceNameFld.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ServiceNameFld.Location = new System.Drawing.Point(104, 15);
            this.ServiceNameFld.Name = "ServiceNameFld";
            this.ServiceNameFld.Size = new System.Drawing.Size(165, 20);
            this.ServiceNameFld.TabIndex = 1;
            this.ServiceNameFld.Leave += new System.EventHandler(this.ServiceName_TextChanged);
            // 
            // OperationNamesLbl
            // 
            this.OperationNamesLbl.AutoSize = true;
            this.OperationNamesLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OperationNamesLbl.Location = new System.Drawing.Point(6, 47);
            this.OperationNamesLbl.Name = "OperationNamesLbl";
            this.OperationNamesLbl.Size = new System.Drawing.Size(92, 13);
            this.OperationNamesLbl.TabIndex = 0;
            this.OperationNamesLbl.Text = "Operation Names:";
            // 
            // CommaSeparatedLbl
            // 
            this.CommaSeparatedLbl.AutoSize = true;
            this.CommaSeparatedLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CommaSeparatedLbl.Location = new System.Drawing.Point(1, 61);
            this.CommaSeparatedLbl.Name = "CommaSeparatedLbl";
            this.CommaSeparatedLbl.Size = new System.Drawing.Size(97, 13);
            this.CommaSeparatedLbl.TabIndex = 0;
            this.CommaSeparatedLbl.Text = "(comma separated)";
            // 
            // OperationNames
            // 
            this.OperationNames.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OperationNames.Location = new System.Drawing.Point(104, 44);
            this.OperationNames.Name = "OperationNames";
            this.OperationNames.Size = new System.Drawing.Size(452, 20);
            this.OperationNames.TabIndex = 3;
            this.OperationNames.Leave += new System.EventHandler(this.OperationNames_TextChanged);
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(499, 191);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 3;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(418, 191);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 31);
            this.Ok.TabIndex = 4;
            this.Ok.Text = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            // 
            // ErrorLine
            // 
            this.ErrorLine.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ErrorLine.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ErrorLine.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ErrorLine.ForeColor = System.Drawing.Color.Red;
            this.ErrorLine.Location = new System.Drawing.Point(12, 10);
            this.ErrorLine.Name = "ErrorLine";
            this.ErrorLine.Size = new System.Drawing.Size(562, 16);
            this.ErrorLine.TabIndex = 0;
            // 
            // TicketBox
            // 
            this.TicketBox.Controls.Add(this.label8);
            this.TicketBox.Controls.Add(this.ProjectIDFld);
            this.TicketBox.Controls.Add(this.TicketIDFld);
            this.TicketBox.Controls.Add(this.label9);
            this.TicketBox.Location = new System.Drawing.Point(12, 39);
            this.TicketBox.Name = "TicketBox";
            this.TicketBox.Size = new System.Drawing.Size(562, 53);
            this.TicketBox.TabIndex = 1;
            this.TicketBox.TabStop = false;
            this.TicketBox.Text = "Administration";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(44, 21);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(54, 13);
            this.label8.TabIndex = 0;
            this.label8.Text = "Ticket ID:";
            // 
            // ProjectIDFld
            // 
            this.ProjectIDFld.Location = new System.Drawing.Point(391, 18);
            this.ProjectIDFld.Name = "ProjectIDFld";
            this.ProjectIDFld.Size = new System.Drawing.Size(165, 20);
            this.ProjectIDFld.TabIndex = 2;
            this.ProjectIDFld.Leave += new System.EventHandler(this.ProjectIDFld_Leave);
            // 
            // TicketIDFld
            // 
            this.TicketIDFld.Location = new System.Drawing.Point(104, 18);
            this.TicketIDFld.Name = "TicketIDFld";
            this.TicketIDFld.Size = new System.Drawing.Size(165, 20);
            this.TicketIDFld.TabIndex = 1;
            this.TicketIDFld.Leave += new System.EventHandler(this.TicketIDFld_Leave);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(330, 21);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(55, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "Project nr:";
            // 
            // ServiceDetailsBox
            // 
            this.ServiceDetailsBox.Controls.Add(this.label1);
            this.ServiceDetailsBox.Controls.Add(this.OperationalState);
            this.ServiceDetailsBox.Controls.Add(this.PackageDeclarationLbl);
            this.ServiceDetailsBox.Controls.Add(this.OperationNamesLbl);
            this.ServiceDetailsBox.Controls.Add(this.CommaSeparatedLbl);
            this.ServiceDetailsBox.Controls.Add(this.ServiceNameFld);
            this.ServiceDetailsBox.Controls.Add(this.OperationNames);
            this.ServiceDetailsBox.Location = new System.Drawing.Point(12, 98);
            this.ServiceDetailsBox.Name = "ServiceDetailsBox";
            this.ServiceDetailsBox.Size = new System.Drawing.Size(562, 87);
            this.ServiceDetailsBox.TabIndex = 2;
            this.ServiceDetailsBox.TabStop = false;
            this.ServiceDetailsBox.Text = "Service Details";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(294, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Initial Oper. State:";
            // 
            // OperationalState
            // 
            this.OperationalState.FormattingEnabled = true;
            this.OperationalState.Location = new System.Drawing.Point(391, 15);
            this.OperationalState.Name = "OperationalState";
            this.OperationalState.Size = new System.Drawing.Size(165, 21);
            this.OperationalState.Sorted = true;
            this.OperationalState.TabIndex = 2;
            // 
            // UseSOAPFaultsIndicator
            // 
            this.UseSOAPFaultsIndicator.AutoSize = true;
            this.UseSOAPFaultsIndicator.Location = new System.Drawing.Point(12, 191);
            this.UseSOAPFaultsIndicator.Name = "UseSOAPFaultsIndicator";
            this.UseSOAPFaultsIndicator.Size = new System.Drawing.Size(126, 17);
            this.UseSOAPFaultsIndicator.TabIndex = 5;
            this.UseSOAPFaultsIndicator.Text = "Use SOAP 1.1 Faults";
            this.UseSOAPFaultsIndicator.UseVisualStyleBackColor = true;
            // 
            // UseListElementsIndicator
            // 
            this.UseListElementsIndicator.AutoSize = true;
            this.UseListElementsIndicator.Location = new System.Drawing.Point(144, 191);
            this.UseListElementsIndicator.Name = "UseListElementsIndicator";
            this.UseListElementsIndicator.Size = new System.Drawing.Size(110, 17);
            this.UseListElementsIndicator.TabIndex = 6;
            this.UseListElementsIndicator.Text = "Use List Elements";
            this.UseListElementsIndicator.UseVisualStyleBackColor = true;
            // 
            // CreateSOAPServiceDeclaration
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(586, 233);
            this.Controls.Add(this.UseListElementsIndicator);
            this.Controls.Add(this.UseSOAPFaultsIndicator);
            this.Controls.Add(this.ServiceDetailsBox);
            this.Controls.Add(this.TicketBox);
            this.Controls.Add(this.ErrorLine);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.Cancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CreateSOAPServiceDeclaration";
            this.Text = "Create a new Service Declaration";
            this.TicketBox.ResumeLayout(false);
            this.TicketBox.PerformLayout();
            this.ServiceDetailsBox.ResumeLayout(false);
            this.ServiceDetailsBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label PackageDeclarationLbl;
        private System.Windows.Forms.TextBox ServiceNameFld;
        private System.Windows.Forms.Label OperationNamesLbl;
        private System.Windows.Forms.Label CommaSeparatedLbl;
        private System.Windows.Forms.TextBox OperationNames;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.TextBox ErrorLine;
        private GroupBox TicketBox;
        private Label label8;
        private TextBox ProjectIDFld;
        private TextBox TicketIDFld;
        private Label label9;
        private GroupBox ServiceDetailsBox;
        private Label label1;
        private ComboBox OperationalState;
        private CheckBox UseSOAPFaultsIndicator;
        private CheckBox UseListElementsIndicator;
    }
}