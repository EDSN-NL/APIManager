using System;
using System.Windows.Forms;
using LibGit2Sharp;
using Framework.Util;
using Framework.ConfigurationManagement;

namespace Plugin.Application.Forms
{
    internal partial class CMRepositorySetting : Form
    {
        private RepositoryDescriptor.DescriptorProperties _repositoryProperties;
        private bool _hasName;
        private bool _hasRootPath;
        private bool _isCreate;
        private string _originalName;

        internal RepositoryDescriptor.DescriptorProperties Properties { get { return this._repositoryProperties; } }

        /// <summary>
        /// Creates a new dialog that facilitates creation of a configuration management descriptor.
        /// </summary>
        /// <param name="properties">Either a new descriptor or an existing one that muse be edited.</param>
        internal CMRepositorySetting(RepositoryDescriptor.DescriptorProperties properties)
        {
            InitializeComponent();

            this._repositoryProperties              = properties;
            EAProjectName.Text                      = properties._name;
            EAProjectDescription.Text               = properties._description;
            ConfigurationMgmtIndicator.Checked      = properties._useCM;
            RepoPathName.Text                       = properties._localPath;
            GITIgnoreEntries.Text                   = properties._GITIgnore;
            RepositoryBaseURL.Text                  = properties._remoteURL != null ? properties._remoteURL.AbsoluteUri : string.Empty; 
            UserName.Text                           = properties._identity != null? properties._identity.Name: string.Empty;
            EMailAddress.Text                       = properties._identity != null? properties._identity.Email: string.Empty;
            Password.Text                           = CryptString.ToPlainString(properties._remotePassword);
            RepositoryNamespace.Text                = properties._remoteNamespace != null? properties._remoteNamespace.OriginalString: string.Empty;

            RemoteConfigManagement.Enabled          = properties._useCM;
            GITIgnoreEntries.Enabled                = properties._useCM;
            JIRAGroup.Enabled                       = properties._useCM;

            JiraURL.Text                            = properties._jiraURL != null ? properties._jiraURL.AbsoluteUri : string.Empty;
            JiraUserName.Text                       = properties._jiraUser;
            JiraPassword.Text                       = CryptString.ToPlainString(properties._jiraPassword);

            this._hasName = !string.IsNullOrEmpty(properties._name);
            this._hasRootPath = !string.IsNullOrEmpty(properties._localPath);
            this._isCreate = !this._hasName;
            this._originalName = properties._name;

            // Initialise our tool tips...
            JiraTooltip.SetToolTip(JIRAGroup, "Server URL must NOT include the API-path and Username is the e-mail address of the Jira user.");

            CheckOk();
        }

        /// <summary>
        /// This event is raised when te 'use CM' indicator changes state. This affects some of our dialogs.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void ConfigurationMgmtIndicator_CheckedChanged(object sender, EventArgs e)
        {
            RemoteConfigManagement.Enabled = ConfigurationMgmtIndicator.Checked;
            GITIgnoreEntries.Enabled = ConfigurationMgmtIndicator.Checked;
        }

        /// <summary>
        /// This event is invoked when the user exists the dialog using 'Ok'. In this case, all fields are written back to
        /// the descriptors for collection by the caller.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void Ok_Click(object sender, EventArgs e)
        {
            this._repositoryProperties._name          = EAProjectName.Text;
            this._repositoryProperties._description   = EAProjectDescription.Text;
            this._repositoryProperties._useCM         = ConfigurationMgmtIndicator.Checked;
            this._repositoryProperties._GITIgnore     = GITIgnoreEntries.Text;

            // Check repository root path, should not end with separator (should not happen since user can not type the path, just to be save...)
            string thePath = RepoPathName.Text;
            if (thePath.EndsWith("/") || thePath.EndsWith("\\"))
                thePath = thePath.Substring(0, thePath.Length - 1);
            this._repositoryProperties._localPath = thePath;

            if (ConfigurationMgmtIndicator.Checked)
            {
                // Absolute URL's will always end with a '/', no matter whether the user has entered this.
                this._repositoryProperties._remoteURL = new Uri(RepositoryBaseURL.Text, UriKind.Absolute);
                this._repositoryProperties._jiraURL = new Uri(JiraURL.Text, UriKind.Absolute);

                if (!string.IsNullOrEmpty(UserName.Text) && !string.IsNullOrEmpty(EMailAddress.Text))
                    this._repositoryProperties._identity = new Identity(UserName.Text, EMailAddress.Text);
                if (!string.IsNullOrEmpty(Password.Text))
                    this._repositoryProperties._remotePassword = CryptString.ToSecureString(Password.Text);

                // Check Repository namespace, should not start- or end with separator...
                thePath = RepositoryNamespace.Text;
                if (!string.IsNullOrEmpty(thePath))
                {
                    if (thePath.EndsWith("/") || thePath.EndsWith("\\"))
                        thePath = thePath.Substring(0, thePath.Length - 1);
                    if (thePath.StartsWith("/") || thePath.StartsWith("\\"))
                        thePath = thePath.Substring(1, thePath.Length - 1);
                    this._repositoryProperties._remoteNamespace = new Uri(thePath, UriKind.Relative);
                }

                this._repositoryProperties._jiraUser = JiraUserName.Text;
                if (!string.IsNullOrEmpty(JiraPassword.Text)) this._repositoryProperties._jiraPassword = CryptString.ToSecureString(JiraPassword.Text);
            }
            else // NO Configuration management, clear all unused properties...
            {
                this._repositoryProperties._remoteURL       = null;
                this._repositoryProperties._jiraURL         = null;
                this._repositoryProperties._identity        = null;
                this._repositoryProperties._remotePassword  = null;
                this._repositoryProperties._remoteNamespace = null;
                this._repositoryProperties._jiraUser        = null;
                this._repositoryProperties._jiraPassword    = null;
            }
        }

        /// <summary>
        /// Invoked when the user selected the 'local repository path chooser' button.
        /// </summary>
        /// <param name="sender">ignored</param>
        /// <param name="e">ignored</param>
        private void SelectRepoPath_Click(object sender, EventArgs e)
        {
            if (RootPathSelector.ShowDialog() == DialogResult.OK)
            {
                RepoPathName.Text = RootPathSelector.SelectedPath;
                RepoPathName.Update();
                this._hasRootPath = true;
                CheckOk();
            }
        }

        /// <summary>
        /// Helper function that checks whether we have reached a state in which the OK button can be enabled.
        /// </summary>
        private void CheckOk()
        {
            Ok.Enabled = this._hasName && this._hasRootPath;
        }

        /// <summary>
        /// This event is raised when the user is finished editing the name. If te name is already registered as a repository name, we issue a 
        /// warning since adding a duplicate repository is not allowed.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void EAProjectName_Leave(object sender, EventArgs e)
        {
            this._hasName = !string.IsNullOrEmpty(EAProjectName.Text);
            CMRepositoryDscManagerSlt dscMgr = CMRepositoryDscManagerSlt.GetRepositoryDscManagerSlt();
            if (dscMgr.Find(EAProjectName.Text) != null && (this._isCreate || this._originalName != EAProjectName.Text))
            {
                MessageBox.Show("Duplicate repository name, please choose another one!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                EAProjectName.Text = this._repositoryProperties._name;
                this._hasName = !string.IsNullOrEmpty(this._originalName);
            }
            CheckOk();
        }
    }
}
