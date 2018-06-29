using System;
using System.Windows.Forms;
using Framework.Logging;
using Framework.Context;

namespace Plugin.Application.Forms
{
    internal partial class SettingsForm : Form
    {
        private string _imageType;
        private string _interfaceType;
        private bool _userNotifiedCMChanges;

        /// <summary>
        /// the SettingsForm works in close cooperation with the FrameworkSettings component (accessed via Context) and is used to
        /// update FrameworkSettings in persistent storage through a user interface.
        /// </summary>
        internal SettingsForm()
        {
            InitializeComponent();
            ContextSlt context                      = ContextSlt.GetContextSlt();
            this._userNotifiedCMChanges             = false;

            UseLogfile.Checked                      = context.GetBoolSetting(FrameworkSettings._UseLogFile);
            LogfileName.Text                        = context.GetStringSetting(FrameworkSettings._LogFileName);
            this._imageType                         = context.GetStringSetting(FrameworkSettings._DiagramSaveType);
            this._interfaceType                     = context.GetStringSetting(FrameworkSettings._InterfaceContractType);

            CLAddCodeTypesToDiagram.Checked         = context.GetBoolSetting(FrameworkSettings._CLAddCodeTypesToDiagram);
            CLAddSourceEnumsToDiagram.Checked       = context.GetBoolSetting(FrameworkSettings._CLAddSourceEnumsToDiagram);
            SMAddMessageAssemblyToDiagram.Checked   = context.GetBoolSetting(FrameworkSettings._SMAddMessageAssemblyToDiagram);
            SMAddBusinessMsgToDiagram.Checked       = context.GetBoolSetting(FrameworkSettings._SMAddBusinessMsgToDiagram);
            SMCreateCmnSchema.Checked               = context.GetBoolSetting(FrameworkSettings._SMCreateCommonSchema);
            SMUseMsgHeaders.Checked                 = context.GetBoolSetting(FrameworkSettings._SMUseMessageHeaders);
            SMUseSecurityLevels.Checked             = context.GetBoolSetting(FrameworkSettings._SMUseSecurityLevels);
            DEBusinessTermName.Checked              = context.GetBoolSetting(FrameworkSettings._DEBusinessTermName);
            DEDefinition.Checked                    = context.GetBoolSetting(FrameworkSettings._DEDefinition);
            DEDictionaryEntryName.Checked           = context.GetBoolSetting(FrameworkSettings._DEDictionaryEntryName);
            DENotes.Checked                         = context.GetBoolSetting(FrameworkSettings._DENotes);
            DEUniqueID.Checked                      = context.GetBoolSetting(FrameworkSettings._DEUniqueID);
            SaveMsgDiagrams.Checked                 = context.GetBoolSetting(FrameworkSettings._SaveMessageDiagrams);
            DocGenUseCommon.Checked                 = context.GetBoolSetting(FrameworkSettings._DocGenUseCommon);
            DocGenGenerate.Checked                  = context.GetBoolSetting(FrameworkSettings._DocGenUseGenerateDoc);
            AutoLocking.Checked                     = context.GetBoolSetting(FrameworkSettings._UseAutomaticLocking);
            PersistentLocks.Checked                 = context.GetBoolSetting(FrameworkSettings._PersistentModelLocks);
            ConfigurationMgmtIndicator.Checked      = context.GetBoolSetting(FrameworkSettings._UseConfigurationManagement);
            UseProxy.Checked                        = context.GetBoolSetting(FrameworkSettings._GITUseProxy);
            RAAPIKeys.Text                          = context.GetStringSetting(FrameworkSettings._RESTAuthAPIKeys);
            RESTHostName.Text                       = context.GetStringSetting(FrameworkSettings._RESTHostName);
            RESTSchemes.Text                        = context.GetStringSetting(FrameworkSettings._RESTSchemes);
            SupplementaryPrefixCode.Text            = context.GetStringSetting(FrameworkSettings._SupplementaryPrefixCode);
            UserName.Text                           = context.GetStringSetting(FrameworkSettings._GLUserName);
            AccessToken.Text                        = context.GetStringSetting(FrameworkSettings._GLAccessToken, true);
            EMailAddress.Text                       = context.GetStringSetting(FrameworkSettings._GLEMail);
            RepositoryBaseURL.Text                  = context.GetStringSetting(FrameworkSettings._GLRepositoryBaseURL);
            RepoPathName.Text                       = context.GetStringSetting(FrameworkSettings._RepositoryRootPath);
            RepositoryNamespace.Text                = context.GetStringSetting(FrameworkSettings._GLRepositoryNamespace);
            GITIgnoreEntries.Text                   = context.GetStringSetting(FrameworkSettings._GITIgnoreEntries);

            string proxyServer = context.GetStringSetting(FrameworkSettings._GITProxyServer);
            if (!string.IsNullOrEmpty(proxyServer))
            {
                ProxyServerName.Text = proxyServer.Substring(0, proxyServer.IndexOf(':'));
                ProxyServerPort.Text = proxyServer.Substring(proxyServer.IndexOf(':') + 1);
            }

            RAScheme.Items.AddRange(new object[]
            {
                FrameworkSettings._RESTAuthSchemeAPIKey,
                FrameworkSettings._RESTAuthSchemeBasic,
                FrameworkSettings._RESTAuthSchemeNone,
                FrameworkSettings._RESTAuthSchemeOAuth2
            });
            RAScheme.SelectedItem = context.GetStringSetting(FrameworkSettings._RESTAuthScheme);

            RAFlow.Items.AddRange(new object[]
            {
                FrameworkSettings._RESTAuthOAuth2FlowAuthCode,
                FrameworkSettings._RESTAuthOAuth2FlowClientCredentials,
                FrameworkSettings._RESTAuthOAuth2FlowImplicit,
                FrameworkSettings._RESTAuthOAuth2FlowPassword
            });
            RAFlow.SelectedItem = context.GetStringSetting(FrameworkSettings._RESTAuthOAuth2Flow);

            RESTAuthentication.Enabled = false; // For the time being, REST Authentication setup is disabled!

            // Find out which radio button to select for diagram...
            foreach (Control control in DiagramTypes.Controls)
            {
                if (control is RadioButton && control.Text == this._imageType)
                {
                    ((RadioButton)control).Checked = true;
                    break;
                }
            }

            // Find out which radio button to select for interface type...
            foreach (Control control in SchemaGeneration.Controls)
            {
                if (control is RadioButton && (string)control.Tag == this._interfaceType)
                {
                    ((RadioButton)control).Checked = true;
                    break;
                }
            }

            // Enable/disable repository-related controls.
            // Note that repository-root is required at all times. If we don't use configuration management, it acts as
            // the root of the local file store for output files. If configuration management is enabled, it acts as
            // the root of our local GIT repository.
            RemoteConfigManagement.Enabled = ConfigurationMgmtIndicator.Checked;
            if (!RemoteConfigManagement.Enabled)
            {
                UserName.Text      = string.Empty;
                AccessToken.Text   = string.Empty;
                EMailAddress.Text  = string.Empty;
                RepositoryBaseURL.Text = string.Empty;
            }

            // Load Tool-Tips...
            AttributePrefixToolTip.SetToolTip(SupplementaryPrefixCode, "Defines the prefix that is added to a Supplementary Attribute when used as a JSON property.");
            ConfigMgmtToolTip.SetToolTip(ConfigurationMgmtIndicator, 
                                         @"When enabled, the local repository path acts as the root of a local GIT repository and the remote " +
                                           "repository must be configured. When disabled, the remote repository is disabled and the repository path " +
                                           "is the root of our local file store.");
            GITIgnoreToolTip.SetToolTip(GITIgnoreEntries, "Comma-separated list of entries for Git-Ignore");
            RepositoryRootToolTip.SetToolTip(RepositoryBaseURL, "Our GITLab URL.");
        }

        /// <summary>
        /// Invoked on OK event, indicating that the current settings must be transferred to configuration.
        /// </summary>
        /// <param name="sender">ignored</param>
        /// <param name="e">ignored</param>
        private void Ok_Click(object sender, EventArgs e)
        {
            Logger.WriteInfo("Custom.Event.EventImp.SettingsForm.SettingsForm.Ok_Click >> We're done!");
            ContextSlt context = ContextSlt.GetContextSlt();
            context.FrameworkStartTransaction();

            context.SetBoolSetting(FrameworkSettings._UseLogFile, UseLogfile.Checked);
            context.SetBoolSetting(FrameworkSettings._CLAddCodeTypesToDiagram, CLAddCodeTypesToDiagram.Checked);
            context.SetBoolSetting(FrameworkSettings._CLAddSourceEnumsToDiagram, CLAddSourceEnumsToDiagram.Checked);
            context.SetBoolSetting(FrameworkSettings._SMAddMessageAssemblyToDiagram, SMAddMessageAssemblyToDiagram.Checked);
            context.SetBoolSetting(FrameworkSettings._SMAddBusinessMsgToDiagram, SMAddBusinessMsgToDiagram.Checked);
            context.SetBoolSetting(FrameworkSettings._SMCreateCommonSchema, SMCreateCmnSchema.Checked);
            context.SetBoolSetting(FrameworkSettings._SMUseMessageHeaders, SMUseMsgHeaders.Checked);
            context.SetBoolSetting(FrameworkSettings._SMUseSecurityLevels, SMUseSecurityLevels.Checked);
            context.SetBoolSetting(FrameworkSettings._DEBusinessTermName, DEBusinessTermName.Checked);
            context.SetBoolSetting(FrameworkSettings._DEDefinition, DEDefinition.Checked);
            context.SetBoolSetting(FrameworkSettings._DEDictionaryEntryName, DEDictionaryEntryName.Checked);
            context.SetBoolSetting(FrameworkSettings._DENotes, DENotes.Checked);
            context.SetBoolSetting(FrameworkSettings._DEUniqueID, DEUniqueID.Checked);
            context.SetBoolSetting(FrameworkSettings._SaveMessageDiagrams, SaveMsgDiagrams.Checked);
            context.SetBoolSetting(FrameworkSettings._DocGenUseCommon, DocGenUseCommon.Checked);
            context.SetBoolSetting(FrameworkSettings._DocGenUseGenerateDoc, DocGenGenerate.Checked);
            context.SetBoolSetting(FrameworkSettings._UseAutomaticLocking, AutoLocking.Checked);
            context.SetBoolSetting(FrameworkSettings._PersistentModelLocks, PersistentLocks.Checked);
            context.SetBoolSetting(FrameworkSettings._UseConfigurationManagement, ConfigurationMgmtIndicator.Checked);
            context.SetBoolSetting(FrameworkSettings._GITUseProxy, UseProxy.Checked);

            context.SetStringSetting(FrameworkSettings._DiagramSaveType, this._imageType);
            context.SetStringSetting(FrameworkSettings._InterfaceContractType, this._interfaceType);
            context.SetStringSetting(FrameworkSettings._LogFileName, LogfileName.Text);
            context.SetStringSetting(FrameworkSettings._RESTAuthScheme, (string)RAScheme.SelectedItem);
            context.SetStringSetting(FrameworkSettings._RESTAuthOAuth2Flow, (string)RAFlow.SelectedItem);
            context.SetStringSetting(FrameworkSettings._RESTAuthAPIKeys, RAAPIKeys.Text);
            context.SetStringSetting(FrameworkSettings._RESTHostName, RESTHostName.Text);
            context.SetStringSetting(FrameworkSettings._RESTSchemes, RESTSchemes.Text);
            context.SetStringSetting(FrameworkSettings._SupplementaryPrefixCode, SupplementaryPrefixCode.Text);
            context.SetStringSetting(FrameworkSettings._GLUserName, UserName.Text);
            context.SetStringSetting(FrameworkSettings._GLAccessToken, AccessToken.Text, true);
            context.SetStringSetting(FrameworkSettings._GLEMail, EMailAddress.Text);
            context.SetStringSetting(FrameworkSettings._GITIgnoreEntries, GITIgnoreEntries.Text);
            context.SetStringSetting(FrameworkSettings._GITProxyServer, ProxyServerName.Text + ":" + ProxyServerPort.Text);

            // Check repository root path, should not end with separator (should not happen since user can not type the path, just to be save...)
            string thePath = RepoPathName.Text;
            if (thePath.EndsWith("/") || thePath.EndsWith("\\"))
                thePath = thePath.Substring(0, thePath.Length - 1);
            context.SetStringSetting(FrameworkSettings._RepositoryRootPath, thePath);

            // Check Repository namespace, should not start- or end with separator...
            thePath = RepositoryNamespace.Text;
            if (thePath.EndsWith("/") || thePath.EndsWith("\\"))
                thePath = thePath.Substring(0, thePath.Length - 1);
            if (thePath.StartsWith("/") || thePath.StartsWith("\\"))
                thePath = thePath.Substring(1, thePath.Length - 1);
            context.SetStringSetting(FrameworkSettings._GLRepositoryNamespace, thePath);

            // Check Repository base URL, should not end with separator...
            thePath = RepositoryBaseURL.Text;
            if (thePath.EndsWith("/") || thePath.EndsWith("\\"))
                thePath = thePath.Substring(0, thePath.Length - 1);
            context.SetStringSetting(FrameworkSettings._GLRepositoryBaseURL, thePath);

            // Check is we still have to use the logfile. If not, we switch to an empty filename, which effectively disables logging...
            // Note that empty files will not be persisted!
            if (UseLogfile.Checked) context.SetLogfile(LogfileName.Text);
            else context.SetLogfile(string.Empty);
            context.FrameworkCommit();

            SettingsEvent.OnSettingsChanged(this);  // Raise the 'settings changed' event.
        }

        /// <summary>
        /// Invoked on CANCEL event, does not perform any operation (i.e. leaves all settings the way they were).
        /// </summary>
        /// <param name="sender">ignored</param>
        /// <param name="e">ignored</param>
        private void Cancel_Click(object sender, EventArgs e)
        {
            Logger.WriteInfo("Custom.Event.EventImp.SettingsForm.Cancel_Click >> We're giving up!");
        }

        /// <summary>
        /// Invoked when the user selected the 'logfile chooser' button.
        /// </summary>
        /// <param name="sender">ignored</param>
        /// <param name="e">ignored</param>
        private void SelectLogfile_Click(object sender, EventArgs e)
        {
            if (LogfileSelector.ShowDialog() == DialogResult.OK)
            {
                LogfileName.Text = LogfileSelector.FileName;
                LogfileName.Update();
                Logger.WriteInfo("Custom.Event.EventImp.SettingsForm.SelectLogfile_Click >> Selected logfile: " + LogfileName.Text);
            }
        }

        /// <summary>
        /// Invoked when the user selected the 'local repository path chooser' button.
        /// </summary>
        /// <param name="sender">ignored</param>
        /// <param name="e">ignored</param>
        private void SelectLocalRepoPath_Click(object sender, EventArgs e)
        {
            if (RepoPathSelector.ShowDialog() == DialogResult.OK)
            {
                RepoPathName.Text = RepoPathSelector.SelectedPath;
                RepoPathName.Update();
                Logger.WriteInfo("Custom.Event.EventImp.SettingsForm.SelectLocalRepoPath >> Selected path: " + RepoPathName.Text);
            }
        }

        /// <summary>
        /// We invoke this handler whenever one of the Diagram Types radio buttons is clicked.
        /// The function loads the string value of the currently selected radio button.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void DiagramTypes_CheckedChanged(object sender, EventArgs e)
        {
            foreach(Control control in DiagramTypes.Controls)
            {
                if (control is RadioButton && ((RadioButton)control).Checked)
                {
                    this._imageType = control.Text;
                    break;
                }
            }
        }

        /// <summary>
        /// We invoke this handler whenever one of the Interface Types radio buttons is clicked.
        /// Since string values for these buttons are meant as indicators for the user and not 
        /// su much usable from code, a textual Tag is defined per button. We use the tag
        /// value as labels.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void InterfaceContracts_CheckedChanges(object sender, EventArgs e)
        {
            foreach (Control control in SchemaGeneration.Controls)
            {
                if (control is RadioButton && ((RadioButton)control).Checked)
                {
                    this._interfaceType = control.Tag as string;
                    break;
                }
            }
        }

        /// <summary>
        /// This event is raised whenever the Add Message Assembly to Diagram changes state.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void SMCheckMSGAssemblyDependencies(object sender, EventArgs e)
        {
            if (SMAddMessageAssemblyToDiagram.Checked) SMAddBusinessMsgToDiagram.Checked = true;
        }

        /// <summary>
        /// This event is raised whenever the Add Business Message to Diagram changes state.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void SMCheckBusinessMsgDependencies(object sender, EventArgs e)
        {
            if (!SMAddBusinessMsgToDiagram.Checked) SMAddMessageAssemblyToDiagram.Checked = false;
        }

        /// <summary>
        /// This event is raised whenever the 'Edit REST Authentication API Keys' button is clicked.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void RAAPIKeyEdit_Click(object sender, EventArgs e)
        {
            using (RESTAuthEditAPIKeys editDialog = new RESTAuthEditAPIKeys(RAAPIKeys.Text))
            {
                if (editDialog.ShowDialog() == DialogResult.OK) RAAPIKeys.Text = editDialog.APIKeys;
            }
        }

        /// <summary>
        /// This event is raised whenever the user changed the value of the 'Use Configuration Management' checkmark.
        /// The event method enables or disables the remote repository configuration accordingly.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void ConfigurationMgmtIndicator_CheckedChanged(object sender, EventArgs e)
        {
            RemoteConfigManagement.Enabled = ConfigurationMgmtIndicator.Checked;
        }

        /// <summary>
        /// Issues a warning to the user when one of the CM settings is initialized and/or changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void Repoconfig_Leave(object sender, EventArgs e)
        {
            if (!this._userNotifiedCMChanges)
            {
                MessageBox.Show("(Changes to) Configuration Management Settings will ONLY become active after a restart of Enterprise Architect!", 
                                "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this._userNotifiedCMChanges = true;
            }
        }
    }

    /// <summary>
    /// Static helper class for broadcasting of settings-changed events. This is an event that has NO data!
    /// </summary>
    internal static class SettingsEvent
    {
        // We declare an event that can be used by other framework components to detect that settings have been changed...
        public static event EventHandler SettingsChanged;

        /// <summary>
        /// The (static) method to be called, which will raise the event.
        /// </summary>
        /// <param name="form">SettingsForm instance that raised the event.</param>
        public static void OnSettingsChanged(SettingsForm form)
        {
        	var handler = SettingsChanged;
        	if (handler != null) handler(form, null);
        }
    }
}
