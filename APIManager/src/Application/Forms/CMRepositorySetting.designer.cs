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
            this.components = new System.ComponentModel.Container();
            this.Cancel = new System.Windows.Forms.Button();
            this.Ok = new System.Windows.Forms.Button();
            this.label1_1 = new System.Windows.Forms.Label();
            this.EAProjectName = new System.Windows.Forms.TextBox();
            this.label1_2 = new System.Windows.Forms.Label();
            this.EAProjectDescription = new System.Windows.Forms.TextBox();
            this.RootPathSelector = new System.Windows.Forms.FolderBrowserDialog();
            this.LocalConfigManagement = new System.Windows.Forms.GroupBox();
            this.label2_2 = new System.Windows.Forms.Label();
            this.GITIgnoreEntries = new System.Windows.Forms.TextBox();
            this.SelectRepoPath = new System.Windows.Forms.Button();
            this.RepoPathName = new System.Windows.Forms.TextBox();
            this.label2_1 = new System.Windows.Forms.Label();
            this.ConfigurationMgmtIndicator = new System.Windows.Forms.CheckBox();
            this.RemoteConfigManagement = new System.Windows.Forms.GroupBox();
            this.RepositoryNamespace = new System.Windows.Forms.TextBox();
            this.label3_5 = new System.Windows.Forms.Label();
            this.RepositoryBaseURL = new System.Windows.Forms.TextBox();
            this.label3_1 = new System.Windows.Forms.Label();
            this.Password = new System.Windows.Forms.TextBox();
            this.label3_4 = new System.Windows.Forms.Label();
            this.label3_2 = new System.Windows.Forms.Label();
            this.UserName = new System.Windows.Forms.TextBox();
            this.label3_3 = new System.Windows.Forms.Label();
            this.EMailAddress = new System.Windows.Forms.TextBox();
            this.GenericBox = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.JIRAGroup = new System.Windows.Forms.GroupBox();
            this.JiraPassword = new System.Windows.Forms.TextBox();
            this.label4_3 = new System.Windows.Forms.Label();
            this.JiraUserName = new System.Windows.Forms.TextBox();
            this.label4_2 = new System.Windows.Forms.Label();
            this.JiraURL = new System.Windows.Forms.TextBox();
            this.label4_1 = new System.Windows.Forms.Label();
            this.JiraTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.LocalConfigManagement.SuspendLayout();
            this.RemoteConfigManagement.SuspendLayout();
            this.GenericBox.SuspendLayout();
            this.JIRAGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(552, 413);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 31);
            this.Cancel.TabIndex = 5;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // Ok
            // 
            this.Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Ok.Location = new System.Drawing.Point(471, 413);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 31);
            this.Ok.TabIndex = 6;
            this.Ok.Text = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // label1_1
            // 
            this.label1_1.AutoSize = true;
            this.label1_1.Location = new System.Drawing.Point(9, 22);
            this.label1_1.Name = "label1_1";
            this.label1_1.Size = new System.Drawing.Size(60, 13);
            this.label1_1.TabIndex = 0;
            this.label1_1.Text = "EA Project:";
            // 
            // EAProjectName
            // 
            this.EAProjectName.Location = new System.Drawing.Point(75, 19);
            this.EAProjectName.Name = "EAProjectName";
            this.EAProjectName.Size = new System.Drawing.Size(153, 20);
            this.EAProjectName.TabIndex = 1;
            this.EAProjectName.Leave += new System.EventHandler(this.EAProjectName_Leave);
            // 
            // label1_2
            // 
            this.label1_2.AutoSize = true;
            this.label1_2.Location = new System.Drawing.Point(6, 48);
            this.label1_2.Name = "label1_2";
            this.label1_2.Size = new System.Drawing.Size(63, 13);
            this.label1_2.TabIndex = 0;
            this.label1_2.Text = "Description:";
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
            this.LocalConfigManagement.Controls.Add(this.label2_2);
            this.LocalConfigManagement.Controls.Add(this.GITIgnoreEntries);
            this.LocalConfigManagement.Controls.Add(this.SelectRepoPath);
            this.LocalConfigManagement.Controls.Add(this.RepoPathName);
            this.LocalConfigManagement.Controls.Add(this.label2_1);
            this.LocalConfigManagement.Location = new System.Drawing.Point(12, 96);
            this.LocalConfigManagement.Name = "LocalConfigManagement";
            this.LocalConfigManagement.Size = new System.Drawing.Size(615, 81);
            this.LocalConfigManagement.TabIndex = 2;
            this.LocalConfigManagement.TabStop = false;
            this.LocalConfigManagement.Text = "Local Configuration Management (Git)";
            // 
            // label2_2
            // 
            this.label2_2.AutoSize = true;
            this.label2_2.Location = new System.Drawing.Point(6, 50);
            this.label2_2.Name = "label2_2";
            this.label2_2.Size = new System.Drawing.Size(95, 13);
            this.label2_2.TabIndex = 0;
            this.label2_2.Text = "GIT Ignore entries:";
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
            // label2_1
            // 
            this.label2_1.AutoSize = true;
            this.label2_1.Location = new System.Drawing.Point(15, 23);
            this.label2_1.Name = "label2_1";
            this.label2_1.Size = new System.Drawing.Size(86, 13);
            this.label2_1.TabIndex = 0;
            this.label2_1.Text = "Repository Root:";
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
            this.RemoteConfigManagement.Controls.Add(this.label3_5);
            this.RemoteConfigManagement.Controls.Add(this.RepositoryBaseURL);
            this.RemoteConfigManagement.Controls.Add(this.label3_1);
            this.RemoteConfigManagement.Controls.Add(this.Password);
            this.RemoteConfigManagement.Controls.Add(this.label3_4);
            this.RemoteConfigManagement.Controls.Add(this.label3_2);
            this.RemoteConfigManagement.Controls.Add(this.UserName);
            this.RemoteConfigManagement.Controls.Add(this.label3_3);
            this.RemoteConfigManagement.Controls.Add(this.EMailAddress);
            this.RemoteConfigManagement.Location = new System.Drawing.Point(12, 183);
            this.RemoteConfigManagement.Name = "RemoteConfigManagement";
            this.RemoteConfigManagement.Size = new System.Drawing.Size(615, 109);
            this.RemoteConfigManagement.TabIndex = 3;
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
            // label3_5
            // 
            this.label3_5.AutoSize = true;
            this.label3_5.Location = new System.Drawing.Point(234, 77);
            this.label3_5.Name = "label3_5";
            this.label3_5.Size = new System.Drawing.Size(99, 13);
            this.label3_5.TabIndex = 0;
            this.label3_5.Text = "Repo. Namespace:";
            // 
            // RepositoryBaseURL
            // 
            this.RepositoryBaseURL.Location = new System.Drawing.Point(75, 22);
            this.RepositoryBaseURL.Name = "RepositoryBaseURL";
            this.RepositoryBaseURL.Size = new System.Drawing.Size(527, 20);
            this.RepositoryBaseURL.TabIndex = 1;
            // 
            // label3_1
            // 
            this.label3_1.AutoSize = true;
            this.label3_1.Location = new System.Drawing.Point(11, 25);
            this.label3_1.Name = "label3_1";
            this.label3_1.Size = new System.Drawing.Size(59, 13);
            this.label3_1.TabIndex = 0;
            this.label3_1.Text = "Base URL:";
            // 
            // Password
            // 
            this.Password.Location = new System.Drawing.Point(75, 74);
            this.Password.Name = "Password";
            this.Password.Size = new System.Drawing.Size(153, 20);
            this.Password.TabIndex = 4;
            this.Password.UseSystemPasswordChar = true;
            // 
            // label3_4
            // 
            this.label3_4.AutoSize = true;
            this.label3_4.Location = new System.Drawing.Point(14, 77);
            this.label3_4.Name = "label3_4";
            this.label3_4.Size = new System.Drawing.Size(56, 13);
            this.label3_4.TabIndex = 0;
            this.label3_4.Text = "Password:";
            // 
            // label3_2
            // 
            this.label3_2.AutoSize = true;
            this.label3_2.Location = new System.Drawing.Point(6, 51);
            this.label3_2.Name = "label3_2";
            this.label3_2.Size = new System.Drawing.Size(63, 13);
            this.label3_2.TabIndex = 0;
            this.label3_2.Text = "User Name:";
            // 
            // UserName
            // 
            this.UserName.Location = new System.Drawing.Point(75, 48);
            this.UserName.Name = "UserName";
            this.UserName.Size = new System.Drawing.Size(153, 20);
            this.UserName.TabIndex = 2;
            // 
            // label3_3
            // 
            this.label3_3.AutoSize = true;
            this.label3_3.Location = new System.Drawing.Point(254, 51);
            this.label3_3.Name = "label3_3";
            this.label3_3.Size = new System.Drawing.Size(79, 13);
            this.label3_3.TabIndex = 0;
            this.label3_3.Text = "E-mail Address:";
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
            this.GenericBox.Controls.Add(this.label1_2);
            this.GenericBox.Controls.Add(this.ConfigurationMgmtIndicator);
            this.GenericBox.Controls.Add(this.label1_1);
            this.GenericBox.Controls.Add(this.EAProjectName);
            this.GenericBox.Location = new System.Drawing.Point(12, 12);
            this.GenericBox.Name = "GenericBox";
            this.GenericBox.Size = new System.Drawing.Size(613, 78);
            this.GenericBox.TabIndex = 1;
            this.GenericBox.TabStop = false;
            this.GenericBox.Text = "General settings";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 410);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(281, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Please note: Repository Root must ALWAYS be specified!";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(9, 427);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(421, 34);
            this.label6.TabIndex = 0;
            this.label6.Text = "Please note: changes to remote Configuration Management settings for \r\n          " +
    "           currently open project will become active only after restarting EA!";
            // 
            // JIRAGroup
            // 
            this.JIRAGroup.Controls.Add(this.JiraPassword);
            this.JIRAGroup.Controls.Add(this.label4_3);
            this.JIRAGroup.Controls.Add(this.JiraUserName);
            this.JIRAGroup.Controls.Add(this.label4_2);
            this.JIRAGroup.Controls.Add(this.JiraURL);
            this.JIRAGroup.Controls.Add(this.label4_1);
            this.JIRAGroup.Location = new System.Drawing.Point(12, 298);
            this.JIRAGroup.Name = "JIRAGroup";
            this.JIRAGroup.Size = new System.Drawing.Size(615, 109);
            this.JIRAGroup.TabIndex = 4;
            this.JIRAGroup.TabStop = false;
            this.JIRAGroup.Text = "JIRA Access";
            // 
            // JiraPassword
            // 
            this.JiraPassword.Location = new System.Drawing.Point(75, 72);
            this.JiraPassword.Name = "JiraPassword";
            this.JiraPassword.Size = new System.Drawing.Size(153, 20);
            this.JiraPassword.TabIndex = 3;
            this.JiraPassword.UseSystemPasswordChar = true;
            // 
            // label4_3
            // 
            this.label4_3.AutoSize = true;
            this.label4_3.Location = new System.Drawing.Point(13, 75);
            this.label4_3.Name = "label4_3";
            this.label4_3.Size = new System.Drawing.Size(56, 13);
            this.label4_3.TabIndex = 0;
            this.label4_3.Text = "Password:";
            // 
            // JiraUserName
            // 
            this.JiraUserName.Location = new System.Drawing.Point(75, 46);
            this.JiraUserName.Name = "JiraUserName";
            this.JiraUserName.Size = new System.Drawing.Size(153, 20);
            this.JiraUserName.TabIndex = 2;
            // 
            // label4_2
            // 
            this.label4_2.AutoSize = true;
            this.label4_2.Location = new System.Drawing.Point(6, 49);
            this.label4_2.Name = "label4_2";
            this.label4_2.Size = new System.Drawing.Size(63, 13);
            this.label4_2.TabIndex = 0;
            this.label4_2.Text = "User Name:";
            // 
            // JiraURL
            // 
            this.JiraURL.Location = new System.Drawing.Point(75, 17);
            this.JiraURL.Name = "JiraURL";
            this.JiraURL.Size = new System.Drawing.Size(527, 20);
            this.JiraURL.TabIndex = 1;
            // 
            // label4_1
            // 
            this.label4_1.AutoSize = true;
            this.label4_1.Location = new System.Drawing.Point(3, 20);
            this.label4_1.Name = "label4_1";
            this.label4_1.Size = new System.Drawing.Size(66, 13);
            this.label4_1.TabIndex = 0;
            this.label4_1.Text = "Server URL:";
            // 
            // JiraTooltip
            // 
            this.JiraTooltip.IsBalloon = true;
            // 
            // CMRepositorySetting
            // 
            this.AcceptButton = this.Ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(637, 462);
            this.Controls.Add(this.JIRAGroup);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
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
            this.JIRAGroup.ResumeLayout(false);
            this.JIRAGroup.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button Ok;
        private Label label1_1;
        private TextBox EAProjectName;
        private Label label1_2;
        private TextBox EAProjectDescription;
        private FolderBrowserDialog RootPathSelector;
        private GroupBox LocalConfigManagement;
        private Label label2_2;
        private TextBox GITIgnoreEntries;
        private CheckBox ConfigurationMgmtIndicator;
        private Button SelectRepoPath;
        private TextBox RepoPathName;
        private Label label2_1;
        private GroupBox RemoteConfigManagement;
        private TextBox RepositoryNamespace;
        private Label label3_5;
        private TextBox RepositoryBaseURL;
        private Label label3_1;
        private TextBox Password;
        private Label label3_4;
        private Label label3_2;
        private TextBox UserName;
        private Label label3_3;
        private TextBox EMailAddress;
        private GroupBox GenericBox;
        private Label label5;
        private Label label6;
        private GroupBox JIRAGroup;
        private TextBox JiraPassword;
        private Label label4_3;
        private TextBox JiraUserName;
        private Label label4_2;
        private TextBox JiraURL;
        private Label label4_1;
        private ToolTip JiraTooltip;
    }
}