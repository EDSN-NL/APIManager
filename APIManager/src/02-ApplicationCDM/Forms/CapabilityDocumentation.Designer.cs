namespace Plugin.Application.Forms
{
    partial class CapabilityDocumentation
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
            this.DocumentationFld = new System.Windows.Forms.RichTextBox();
            this.FontDialog = new System.Windows.Forms.FontDialog();
            this.OpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.MenuStrip = new System.Windows.Forms.MenuStrip();
            this.FileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.FileOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.EditMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.EditCut = new System.Windows.Forms.ToolStripMenuItem();
            this.EditCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.EditPaste = new System.Windows.Forms.ToolStripMenuItem();
            this.EditDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.FormatMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.FormatBold = new System.Windows.Forms.ToolStripMenuItem();
            this.FormatItalic = new System.Windows.Forms.ToolStripMenuItem();
            this.FormatRegular = new System.Windows.Forms.ToolStripMenuItem();
            this.FormatStrikeout = new System.Windows.Forms.ToolStripMenuItem();
            this.FormatUnderline = new System.Windows.Forms.ToolStripMenuItem();
            this.FormatBullet = new System.Windows.Forms.ToolStripMenuItem();
            this.ColorDialog = new System.Windows.Forms.ColorDialog();
            this.MenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(518, 293);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 2;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(437, 293);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 31);
            this.Ok.TabIndex = 3;
            this.Ok.Text = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            // 
            // DocumentationFld
            // 
            this.DocumentationFld.AcceptsTab = true;
            this.DocumentationFld.AutoWordSelection = true;
            this.DocumentationFld.EnableAutoDragDrop = true;
            this.DocumentationFld.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DocumentationFld.Location = new System.Drawing.Point(12, 30);
            this.DocumentationFld.Name = "DocumentationFld";
            this.DocumentationFld.Size = new System.Drawing.Size(581, 250);
            this.DocumentationFld.TabIndex = 1;
            this.DocumentationFld.Text = "";
            // 
            // OpenFileDialog
            // 
            this.OpenFileDialog.Filter = "Text files|*.txt|RTF files|*.rtf";
            // 
            // MenuStrip
            // 
            this.MenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileMenu,
            this.EditMenu,
            this.FormatMenu});
            this.MenuStrip.Location = new System.Drawing.Point(0, 0);
            this.MenuStrip.Name = "MenuStrip";
            this.MenuStrip.Size = new System.Drawing.Size(608, 24);
            this.MenuStrip.TabIndex = 4;
            this.MenuStrip.Text = "MenuStrip";
            // 
            // FileMenu
            // 
            this.FileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileOpen});
            this.FileMenu.Name = "FileMenu";
            this.FileMenu.Size = new System.Drawing.Size(37, 20);
            this.FileMenu.Text = "File";
            // 
            // FileOpen
            // 
            this.FileOpen.Name = "FileOpen";
            this.FileOpen.Size = new System.Drawing.Size(112, 22);
            this.FileOpen.Text = "Open...";
            this.FileOpen.Click += new System.EventHandler(this.FileOpen_Click);
            // 
            // EditMenu
            // 
            this.EditMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.EditCut,
            this.EditCopy,
            this.EditPaste,
            this.EditDelete});
            this.EditMenu.Name = "EditMenu";
            this.EditMenu.Size = new System.Drawing.Size(39, 20);
            this.EditMenu.Text = "Edit";
            // 
            // EditCut
            // 
            this.EditCut.Name = "EditCut";
            this.EditCut.Size = new System.Drawing.Size(173, 22);
            this.EditCut.Text = "Cut               Ctrl+X";
            this.EditCut.Click += new System.EventHandler(this.EditCut_Click);
            // 
            // EditCopy
            // 
            this.EditCopy.Name = "EditCopy";
            this.EditCopy.Size = new System.Drawing.Size(173, 22);
            this.EditCopy.Text = "Copy            Ctrl+C";
            this.EditCopy.Click += new System.EventHandler(this.EditCopy_Click);
            // 
            // EditPaste
            // 
            this.EditPaste.Name = "EditPaste";
            this.EditPaste.Size = new System.Drawing.Size(173, 22);
            this.EditPaste.Text = "Paste            Ctrl+V";
            this.EditPaste.Click += new System.EventHandler(this.EditPaste_Click);
            // 
            // EditDelete
            // 
            this.EditDelete.Name = "EditDelete";
            this.EditDelete.Size = new System.Drawing.Size(173, 22);
            this.EditDelete.Text = "Delete           Del";
            this.EditDelete.Click += new System.EventHandler(this.EditDelete_Click);
            // 
            // FormatMenu
            // 
            this.FormatMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FormatBold,
            this.FormatItalic,
            this.FormatRegular,
            this.FormatStrikeout,
            this.FormatUnderline,
            this.FormatBullet});
            this.FormatMenu.Name = "FormatMenu";
            this.FormatMenu.Size = new System.Drawing.Size(57, 20);
            this.FormatMenu.Text = "Format";
            // 
            // FormatBold
            // 
            this.FormatBold.Name = "FormatBold";
            this.FormatBold.Size = new System.Drawing.Size(125, 22);
            this.FormatBold.Text = "Bold";
            this.FormatBold.Click += new System.EventHandler(this.FormatBold_Click);
            // 
            // FormatItalic
            // 
            this.FormatItalic.Name = "FormatItalic";
            this.FormatItalic.Size = new System.Drawing.Size(125, 22);
            this.FormatItalic.Text = "Italic";
            this.FormatItalic.Click += new System.EventHandler(this.FormatItalic_Click);
            // 
            // FormatRegular
            // 
            this.FormatRegular.Name = "FormatRegular";
            this.FormatRegular.Size = new System.Drawing.Size(125, 22);
            this.FormatRegular.Text = "Regular";
            this.FormatRegular.Click += new System.EventHandler(this.FormatRegular_Click);
            // 
            // FormatStrikeout
            // 
            this.FormatStrikeout.Name = "FormatStrikeout";
            this.FormatStrikeout.Size = new System.Drawing.Size(125, 22);
            this.FormatStrikeout.Text = "Strikeout";
            this.FormatStrikeout.Click += new System.EventHandler(this.FormatStrikeout_Click);
            // 
            // FormatUnderline
            // 
            this.FormatUnderline.Name = "FormatUnderline";
            this.FormatUnderline.Size = new System.Drawing.Size(125, 22);
            this.FormatUnderline.Text = "Underline";
            this.FormatUnderline.Click += new System.EventHandler(this.FormatUnderline_Click);
            // 
            // FormatBullet
            // 
            this.FormatBullet.Name = "FormatBullet";
            this.FormatBullet.Size = new System.Drawing.Size(125, 22);
            this.FormatBullet.Text = "Bullets";
            this.FormatBullet.Click += new System.EventHandler(this.FormatBullet_Click);
            // 
            // CapabilityDocumentation
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(608, 334);
            this.Controls.Add(this.DocumentationFld);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.MenuStrip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MainMenuStrip = this.MenuStrip;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CapabilityDocumentation";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Capability Documentation";
            this.MenuStrip.ResumeLayout(false);
            this.MenuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button Ok;
        private System.Windows.Forms.RichTextBox DocumentationFld;
        private System.Windows.Forms.FontDialog FontDialog;
        private System.Windows.Forms.OpenFileDialog OpenFileDialog;
        private System.Windows.Forms.MenuStrip MenuStrip;
        private System.Windows.Forms.ToolStripMenuItem FileMenu;
        private System.Windows.Forms.ToolStripMenuItem FileOpen;
        private System.Windows.Forms.ToolStripMenuItem EditMenu;
        private System.Windows.Forms.ToolStripMenuItem EditCut;
        private System.Windows.Forms.ToolStripMenuItem EditCopy;
        private System.Windows.Forms.ToolStripMenuItem EditPaste;
        private System.Windows.Forms.ToolStripMenuItem EditDelete;
        private System.Windows.Forms.ToolStripMenuItem FormatMenu;
        private System.Windows.Forms.ToolStripMenuItem FormatBold;
        private System.Windows.Forms.ToolStripMenuItem FormatItalic;
        private System.Windows.Forms.ToolStripMenuItem FormatRegular;
        private System.Windows.Forms.ToolStripMenuItem FormatStrikeout;
        private System.Windows.Forms.ToolStripMenuItem FormatUnderline;
        private System.Windows.Forms.ToolStripMenuItem FormatBullet;
        private System.Windows.Forms.ColorDialog ColorDialog;
    }
}