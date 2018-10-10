using System;
using System.Windows.Forms;
using Framework.Logging;
using Framework.Context;
using Framework.ConfigurationManagement;

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
            AllOfSupport.Checked                    = context.GetBoolSetting(FrameworkSettings._JSONAllOfSupport);
            RAAPIKeys.Text                          = context.GetStringSetting(FrameworkSettings._RESTAuthAPIKeys);
            RESTHostName.Text                       = context.GetStringSetting(FrameworkSettings._RESTHostName);
            RESTSchemes.Text                        = context.GetStringSetting(FrameworkSettings._RESTSchemes);
            SupplementaryPrefixCode.Text            = context.GetStringSetting(FrameworkSettings._SupplementaryPrefixCode);

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

            // Load the CM descriptor info...
            CMRepositoryDscManagerSlt dscMgr = CMRepositoryDscManagerSlt.GetRepositoryDscManagerSlt();
            foreach (RepositoryDescriptor dsc in dscMgr.RepositoryDescriptorList)
            {
                ListViewItem newItem = new ListViewItem(dsc.Name);
                newItem.SubItems.Add(dsc.Description);
                newItem.SubItems.Add(dsc.IsCMEnabled? "yes": "no");
                ResponseCodeList.Items.Add(newItem);
            }

            // Load Tool-Tips...
            AttributePrefixToolTip.SetToolTip(SupplementaryPrefixCode, "Defines the prefix that is added to a Supplementary Attribute when used as a JSON property.");
            ConfigMgmtToolTip.SetToolTip(ConfigurationManagementGroup, 
                                         @"Please configure at least one repository to be used for writing output schema's and/or interfaces");

            // Assign context menus to the appropriate controls...
            ResponseCodeList.ContextMenuStrip = CMMenuStrip;

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
            context.SetBoolSetting(FrameworkSettings._JSONAllOfSupport, AllOfSupport.Checked);

            context.SetStringSetting(FrameworkSettings._DiagramSaveType, this._imageType);
            context.SetStringSetting(FrameworkSettings._InterfaceContractType, this._interfaceType);
            context.SetStringSetting(FrameworkSettings._LogFileName, LogfileName.Text);
            context.SetStringSetting(FrameworkSettings._RESTAuthScheme, (string)RAScheme.SelectedItem);
            context.SetStringSetting(FrameworkSettings._RESTAuthOAuth2Flow, (string)RAFlow.SelectedItem);
            context.SetStringSetting(FrameworkSettings._RESTAuthAPIKeys, RAAPIKeys.Text);
            context.SetStringSetting(FrameworkSettings._RESTHostName, RESTHostName.Text);
            context.SetStringSetting(FrameworkSettings._RESTSchemes, RESTSchemes.Text);
            context.SetStringSetting(FrameworkSettings._SupplementaryPrefixCode, SupplementaryPrefixCode.Text);

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

        /// <summary>
        /// This event is raised when the user clicked the 'add new repository' button (or menu item). It invokes a user dialog to
        /// create a new repository descriptor and registers this with the repository manager.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void AddRepository_Click(object sender, EventArgs e)
        {
            CMRepositoryDscManagerSlt dscMgr = CMRepositoryDscManagerSlt.GetRepositoryDscManagerSlt();
            RepositoryDescriptor.DescriptorProperties repositoryProperties = new RepositoryDescriptor.DescriptorProperties
            {
                _name = string.Empty,
                _description = string.Empty,
                _useCM = false,
                _GITIgnore = string.Empty,
                _identity = null,
                _remotePassword = null,
                _localPath = string.Empty,
                _remoteURL = null,
                _remoteNamespace = null,
                _jiraPassword = null,
                _jiraURL = null,
                _jiraUser = string.Empty
            };

            using (var dialog = new CMRepositorySetting(repositoryProperties))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (dscMgr.AddRepositoryDescriptor(dialog.Properties))
                    {
                        ListViewItem newItem = new ListViewItem(dialog.Properties._name);
                        newItem.SubItems.Add(dialog.Properties._description);
                        newItem.SubItems.Add(dialog.Properties._useCM ? "yes" : "no");
                        ResponseCodeList.Items.Add(newItem);
                    }
                    else MessageBox.Show("A repository with this name already exists!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// This event is raised when the user clicked the 'delete existing repository' button (or menu item).
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void DeleteRepository_Click(object sender, EventArgs e)
        {
            if (ResponseCodeList.SelectedItems.Count > 0)
            {
                ListViewItem key = ResponseCodeList.SelectedItems[0];
                if (MessageBox.Show("You are about to delete repository configuration '" + key.Text + "'! Are you sure?",
                                    "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    CMRepositoryDscManagerSlt dscMgr = CMRepositoryDscManagerSlt.GetRepositoryDscManagerSlt();
                    dscMgr.DeleteDescriptor(key.Text);
                    ResponseCodeList.Items.Remove(key);
                }
            }
        }

        /// <summary>
        /// This event is raised wen the user clicked the 'edit existing repository' button (or menu item).
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void EditRepository_Click(object sender, EventArgs e)
        {
            if (ResponseCodeList.SelectedItems.Count > 0)
            {
                ListViewItem myItem = ResponseCodeList.SelectedItems[0];
                string originalKey = myItem.Text;
                CMRepositoryDscManagerSlt dscMgr = CMRepositoryDscManagerSlt.GetRepositoryDscManagerSlt();
                RepositoryDescriptor.DescriptorProperties properties = dscMgr.Find(myItem.Text).Properties;

                using (var dialog = new CMRepositorySetting(properties))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        if (dscMgr.EditDescriptor(originalKey, dialog.Properties))
                        {
                            myItem.SubItems[0].Text = dialog.Properties._name;
                            myItem.SubItems[1].Text = dialog.Properties._description;
                            myItem.SubItems[2].Text = dialog.Properties._useCM ? "yes" : "no";
                        }
                        else MessageBox.Show("Renaming repository resulted in duplicate name, please try again!", 
                                             "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// This event is raised when the user selects the 'delete all repositories' button, which will remove all descriptors.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void DeleteAllRepositories_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("This will delete ALL repository configurations! Are you sure?", 
                                "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                CMRepositoryDscManagerSlt.GetRepositoryDscManagerSlt().DeleteAllDescriptors();
                ResponseCodeList.Items.Clear();
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
