using System.Windows.Forms;

namespace Plugin.Application.Forms
{
    partial class CMRepositorySetting
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
            this.EAProjectName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.EAProjectDescription = new System.Windows.Forms.TextBox();
            this.RootPathSelector = new System.Windows.Forms.FolderBrowserDialog();
            this.LocalConfigManagement = new System.Windows.Forms.GroupBox();
            this.label13 = new System.Windows.Forms.Label();
            this.GITIgnoreEntries = new System.Windows.Forms.TextBox();
            this.SelectRepoPath = new System.Windows.Forms.Button();
            this.RepoPathName = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.ConfigurationMgmtIndicator = new System.Windows.Forms.CheckBox();
            this.RemoteConfigManagement = new System.Windows.Forms.GroupBox();
            this.RepositoryNamespace = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.RepositoryBaseURL = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.Password = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.UserName = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.EMailAddress = new System.Windows.Forms.TextBox();
            this.GenericBox = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.LocalConfigManagement.SuspendLayout();
            this.RemoteConfigManagement.SuspendLayout();
            this.GenericBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(550, 298);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 6;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(469, 298);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 31);
            this.Ok.TabIndex = 7;
            this.Ok.Text = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "EA Project:";
            // 
            // EAProjectName
            // 
            this.EAProjectName.Location = new System.Drawing.Point(75, 19);
            this.EAProjectName.Name = "EAProjectName";
            this.EAProjectName.Size = new System.Drawing.Size(153, 20);
            this.EAProjectName.TabIndex = 1;
            this.EAProjectName.Leave += new System.EventHandler(this.EAProjectName_Leave);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Description:";
            // 
            // EAProjectDescription
            // 
            this.EAProjectDescription.Location = new System.Drawing.Point(75, 45);
            this.EAProjectDescription.Name = "EAProjectDescription";
            this.EAProjectDescription.Size = new System.Drawing.Size(527, 20);
            this.EAProjectDescription.TabIndex = 3;
            // 
            // LocalConfigManagement
            // 
            this.LocalConfigManagement.Controls.Add(this.label13);
            this.LocalConfigManagement.Controls.Add(this.GITIgnoreEntries);
            this.LocalConfigManagement.Controls.Add(this.SelectRepoPath);
            this.LocalConfigManagement.Controls.Add(this.RepoPathName);
            this.LocalConfigManagement.Controls.Add(this.label7);
            this.LocalConfigManagement.Location = new System.Drawing.Point(12, 96);
            this.LocalConfigManagement.Name = "LocalConfigManagement";
            this.LocalConfigManagement.Size = new System.Drawing.Size(615, 81);
            this.LocalConfigManagement.TabIndex = 4;
            this.LocalConfigManagement.TabStop = false;
            this.LocalConfigManagement.Text = "Local Configuration Management (Git)";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(6, 50);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(95, 13);
            this.label13.TabIndex = 0;
            this.label13.Text = "GIT Ignore entries:";
            // 
            // GITIgnoreEntries
            // 
            this.GITIgnoreEntries.Location = new System.Drawing.Point(107, 47);
            this.GITIgnoreEntries.Name = "GITIgnoreEntries";
            this.GITIgnoreEntries.Size = new System.Drawing.Size(495, 20);
            this.GITIgnoreEntries.TabIndex = 2;
            // 
            // SelectRepoPath
            // 
            this.SelectRepoPath.BackColor = System.Drawing.SystemColors.Control;
            this.SelectRepoPath.Location = new System.Drawing.Point(577, 20);
            this.SelectRepoPath.Name = "SelectRepoPath";
            this.SelectRepoPath.Size = new System.Drawing.Size(25, 21);
            this.SelectRepoPath.TabIndex = 1;
            this.SelectRepoPath.Text = "...";
            this.SelectRepoPath.UseVisualStyleBackColor = false;
            this.SelectRepoPath.Click += new System.EventHandler(this.SelectRepoPath_Click);
            // 
            // RepoPathName
            // 
            this.RepoPathName.BackColor = System.Drawing.SystemColors.Info;
            this.RepoPathName.Location = new System.Drawing.Point(107, 20);
            this.RepoPathName.Name = "RepoPathName";
            this.RepoPathName.ReadOnly = true;
            this.RepoPathName.Size = new System.Drawing.Size(464, 20);
            this.RepoPathName.TabIndex = 0;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(15, 23);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(86, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "Repository Root:";
            // 
            // ConfigurationMgmtIndicator
            // 
            this.ConfigurationMgmtIndicator.AutoSize = true;
            this.ConfigurationMgmtIndicator.Location = new System.Drawing.Point(237, 21);
            this.ConfigurationMgmtIndicator.Name = "ConfigurationMgmtIndicator";
            this.ConfigurationMgmtIndicator.Size = new System.Drawing.Size(244, 17);
            this.ConfigurationMgmtIndicator.TabIndex = 2;
            this.ConfigurationMgmtIndicator.Text = "Use Configuration Management for this project";
            this.ConfigurationMgmtIndicator.UseVisualStyleBackColor = true;
            this.ConfigurationMgmtIndicator.CheckedChanged += new System.EventHandler(this.ConfigurationMgmtIndicator_CheckedChanged);
            // 
            // RemoteConfigManagement
            // 
            this.RemoteConfigManagement.Controls.Add(this.RepositoryNamespace);
            this.RemoteConfigManagement.Controls.Add(this.label14);
            this.RemoteConfigManagement.Controls.Add(this.RepositoryBaseURL);
            this.RemoteConfigManagement.Controls.Add(this.label12);
            this.RemoteConfigManagement.Controls.Add(this.Password);
            this.RemoteConfigManagement.Controls.Add(this.label11);
            this.RemoteConfigManagement.Controls.Add(this.label9);
            this.RemoteConfigManagement.Controls.Add(this.UserName);
            this.RemoteConfigManagement.Controls.Add(this.label10);
            this.RemoteConfigManagement.Controls.Add(this.EMailAddress);
            this.RemoteConfigManagement.Location = new System.Drawing.Point(12, 183);
            this.RemoteConfigManagement.Name = "RemoteConfigManagement";
            this.RemoteConfigManagement.Size = new System.Drawing.Size(615, 109);
            this.RemoteConfigManagement.TabIndex = 5;
            this.RemoteConfigManagement.TabStop = false;
            this.RemoteConfigManagement.Text = "Remote Configuration Management (GitLab)";
            // 
            // RepositoryNamespace
            // 
            this.RepositoryNamespace.Location = new System.Drawing.Point(339, 74);
            this.RepositoryNamespace.Name = "RepositoryNamespace";
            this.RepositoryNamespace.Size = new System.Drawing.Size(263, 20);
            this.RepositoryNamespace.TabIndex = 5;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(234, 77);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(99, 13);
            this.label14.TabIndex = 0;
            this.label14.Text = "Repo. Namespace:";
            // 
            // RepositoryBaseURL
            // 
            this.RepositoryBaseURL.Location = new System.Drawing.Point(75, 22);
            this.RepositoryBaseURL.Name = "RepositoryBaseURL";
            this.RepositoryBaseURL.Size = new System.Drawing.Size(527, 20);
            this.RepositoryBaseURL.TabIndex = 1;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(11, 25);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(59, 13);
            this.label12.TabIndex = 0;
            this.label12.Text = "Base URL:";
            // 
            // Password
            // 
            this.Password.Location = new System.Drawing.Point(75, 74);
            this.Password.Name = "Password";
            this.Password.Size = new System.Drawing.Size(153, 20);
            this.Password.TabIndex = 4;
            this.Password.UseSystemPasswordChar = true;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(14, 77);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(56, 13);
            this.label11.TabIndex = 0;
            this.label11.Text = "Password:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 51);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(63, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "User Name:";
            // 
            // UserName
            // 
            this.UserName.Location = new System.Drawing.Point(75, 48);
            this.UserName.Name = "UserName";
            this.UserName.Size = new System.Drawing.Size(153, 20);
            this.UserName.TabIndex = 2;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(254, 51);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(79, 13);
            this.label10.TabIndex = 0;
            this.label10.Text = "E-mail Address:";
            // 
            // EMailAddress
            // 
            this.EMailAddress.Location = new System.Drawing.Point(339, 48);
            this.EMailAddress.Name = "EMailAddress";
            this.EMailAddress.Size = new System.Drawing.Size(263, 20);
            this.EMailAddress.TabIndex = 3;
            // 
            // GenericBox
            // 
            this.GenericBox.Controls.Add(this.EAProjectDescription);
            this.GenericBox.Controls.Add(this.label2);
            this.GenericBox.Controls.Add(this.ConfigurationMgmtIndicator);
            this.GenericBox.Controls.Add(this.label1);
            this.GenericBox.Controls.Add(this.EAProjectName);
            this.GenericBox.Location = new System.Drawing.Point(12, 12);
            this.GenericBox.Name = "GenericBox";
            this.GenericBox.Size = new System.Drawing.Size(613, 78);
            this.GenericBox.TabIndex = 7;
            this.GenericBox.TabStop = false;
            this.GenericBox.Text = "General settings";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 298);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(281, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Please note: Repository Root must ALWAYS be specified!";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(12, 311);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(421, 34);
            this.label4.TabIndex = 0;
            this.label4.Text = "Please note: changes to remote Configuration Management settings for \r\n          " +
    "           currently open project will become active only after restarting EA!";
            // 
            // CMRepositorySetting
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(637, 348);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.RemoteConfigManagement);
            this.Controls.Add(this.LocalConfigManagement);
            this.Controls.Add(this.Ok);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.GenericBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CMRepositorySetting";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Repository Settings";
            this.LocalConfigManagement.ResumeLayout(false);
            this.LocalConfigManagement.PerformLayout();
            this.RemoteConfigManagement.ResumeLayout(false);
            this.RemoteConfigManagement.PerformLayout();
            this.GenericBox.ResumeLayout(false);
            this.GenericBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button Ok;
        private Label label1;
        private TextBox EAProjectName;
        private Label label2;
        private TextBox EAProjectDescription;
        private FolderBrowserDialog RootPathSelector;
        private GroupBox LocalConfigManagement;
        private Label label13;
        private TextBox GITIgnoreEntries;
        private CheckBox ConfigurationMgmtIndicator;
        private Button SelectRepoPath;
        private TextBox RepoPathName;
        private Label label7;
        private GroupBox RemoteConfigManagement;
        private TextBox RepositoryNamespace;
        private Label label14;
        private TextBox RepositoryBaseURL;
        private Label label12;
        private TextBox Password;
        private Label label11;
        private Label label9;
        private TextBox UserName;
        private Label label10;
        private TextBox EMailAddress;
        private GroupBox GenericBox;
        private Label label3;
        private Label label4;
    }
}