namespace Plugin.Application.Forms
{
    partial class ProgressPanel
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
            this.progressBox = new System.Windows.Forms.TextBox();
            this.DoneBar = new System.Windows.Forms.Button();
            this.ProgressBar = new System.Windows.Forms.ProgressBar();
            this.CopyToClipboard = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // progressBox
            // 
            this.progressBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.progressBox.Location = new System.Drawing.Point(16, 14);
            this.progressBox.Multiline = true;
            this.progressBox.Name = "progressBox";
            this.progressBox.ReadOnly = true;
            this.progressBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.progressBox.Size = new System.Drawing.Size(1137, 422);
            this.progressBox.TabIndex = 0;
            // 
            // DoneBar
            // 
            this.DoneBar.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.DoneBar.Location = new System.Drawing.Point(16, 462);
            this.DoneBar.Name = "DoneBar";
            this.DoneBar.Size = new System.Drawing.Size(996, 31);
            this.DoneBar.TabIndex = 1;
            this.DoneBar.Text = "Done";
            this.DoneBar.UseVisualStyleBackColor = true;
            this.DoneBar.Click += new System.EventHandler(this.Done_Click);
            // 
            // ProgressBar
            // 
            this.ProgressBar.Location = new System.Drawing.Point(16, 442);
            this.ProgressBar.Name = "ProgressBar";
            this.ProgressBar.Size = new System.Drawing.Size(1137, 14);
            this.ProgressBar.TabIndex = 0;
            // 
            // CopyToClipboard
            // 
            this.CopyToClipboard.Location = new System.Drawing.Point(1018, 462);
            this.CopyToClipboard.Name = "CopyToClipboard";
            this.CopyToClipboard.Size = new System.Drawing.Size(135, 31);
            this.CopyToClipboard.TabIndex = 2;
            this.CopyToClipboard.Text = "Copy to Clipboard";
            this.CopyToClipboard.UseVisualStyleBackColor = true;
            this.CopyToClipboard.Click += new System.EventHandler(this.CopyToClipboard_Click);
            // 
            // ProgressPanel
            // 
            this.AcceptButton = this.DoneBar;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1169, 505);
            this.Controls.Add(this.CopyToClipboard);
            this.Controls.Add(this.ProgressBar);
            this.Controls.Add(this.DoneBar);
            this.Controls.Add(this.progressBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProgressPanel";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Processing progress...";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox progressBox;
        private System.Windows.Forms.Button DoneBar;
        private System.Windows.Forms.ProgressBar ProgressBar;
        private System.Windows.Forms.Button CopyToClipboard;
    }
}