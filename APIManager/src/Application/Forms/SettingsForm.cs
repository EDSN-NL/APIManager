﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Logging;
using Framework.Context;
using Plugin.Application.CapabilityModel.API;

namespace Plugin.Application.Forms
{
    internal partial class SettingsForm : Form
    {
        private string _logfileName;
        private string _rootPath;
        private string _imageType;
        private string _interfaceType;
        private SortedList<string, RESTParameterDeclaration> _parameterList; 

        /// <summary>
        /// the SettingsForm works in close cooperation with the FrameworkSettings component (accessed via Context) and is used to
        /// update FrameworkSettings in persistent storage through a user interface.
        /// </summary>
        internal SettingsForm()
        {
            InitializeComponent();
            ContextSlt context                      = ContextSlt.GetContextSlt();

            UseLogfile.Checked                      = context.GetBoolSetting(FrameworkSettings._UseLogFile);
            this._logfileName                       = context.GetStringSetting(FrameworkSettings._LogFileName);
            this._rootPath                          = context.GetStringSetting(FrameworkSettings._RootPath);
            this._imageType                         = context.GetStringSetting(FrameworkSettings._DiagramSaveType);
            this._interfaceType                     = context.GetStringSetting(FrameworkSettings._InterfaceContractType);

            CLAddCodeTypesToDiagram.Checked         = context.GetBoolSetting(FrameworkSettings._CLAddCodeTypesToDiagram);
            CLAddSourceEnumsToDiagram.Checked       = context.GetBoolSetting(FrameworkSettings._CLAddSourceEnumsToDiagram);
            AutoIncrementBuildNr.Checked            = context.GetBoolSetting(FrameworkSettings._AutoIncrementBuildNumbers);
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
            RAAPIKeys.Text                          = context.GetStringSetting(FrameworkSettings._RESTAuthAPIKeys);
            RESTHostName.Text                       = context.GetStringSetting(FrameworkSettings._RESTHostName);
            RESTSchemes.Text                        = context.GetStringSetting(FrameworkSettings._RESTSchemes);

            // We retrieve the set of REST header parameters from our configuration..
            this._parameterList = new SortedList<string, RESTParameterDeclaration>();
            var paramList = RESTUtil.GetHeaderParameters();
            foreach (RESTParameterDeclaration param in paramList)
            {
                this._parameterList.Add(param.Name, param);
                ListViewItem newItem = new ListViewItem(param.Name);
                newItem.SubItems.Add(param.Classifier.Name);
                ParameterList.Items.Add(newItem);
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
            foreach (Control control in InterfaceContracts.Controls)
            {
                if (control is RadioButton && (string)control.Tag == this._interfaceType)
                {
                    ((RadioButton)control).Checked = true;
                    break;
                }
            }

            LogfileName.Text    = this._logfileName;
            RootPathName.Text   = this._rootPath;
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
            context.SetStringSetting(FrameworkSettings._LogFileName, this._logfileName);
            context.SetBoolSetting(FrameworkSettings._CLAddCodeTypesToDiagram, CLAddCodeTypesToDiagram.Checked);
            context.SetBoolSetting(FrameworkSettings._CLAddSourceEnumsToDiagram, CLAddSourceEnumsToDiagram.Checked);
            context.SetBoolSetting(FrameworkSettings._AutoIncrementBuildNumbers, AutoIncrementBuildNr.Checked);
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
            context.SetStringSetting(FrameworkSettings._DiagramSaveType, this._imageType);
            context.SetStringSetting(FrameworkSettings._InterfaceContractType, this._interfaceType);
            context.SetBoolSetting(FrameworkSettings._DocGenUseCommon, DocGenUseCommon.Checked);
            context.SetStringSetting(FrameworkSettings._RESTAuthScheme, (string)RAScheme.SelectedItem);
            context.SetStringSetting(FrameworkSettings._RESTAuthOAuth2Flow, (string)RAFlow.SelectedItem);
            context.SetStringSetting(FrameworkSettings._RESTAuthAPIKeys, RAAPIKeys.Text);
            context.SetStringSetting(FrameworkSettings._RESTHostName, RESTHostName.Text);
            context.SetStringSetting(FrameworkSettings._RESTSchemes, RESTSchemes.Text);

            // Retrieve the REST Header parameters and write them to Configuration in serialized format...
            RESTUtil.SetHeaderParameters(new List<RESTParameterDeclaration>(this._parameterList.Values));

            // Check root path, should not end with separator (should not happen since user can not type the path, just to be save...)
            if (this._rootPath.EndsWith("/") || this._rootPath.EndsWith("\\"))
                this._rootPath = this._rootPath.Substring(0, this._rootPath.Length - 1);
            context.SetStringSetting(FrameworkSettings._RootPath, this._rootPath);

            // Check is we still have to use the logfile. If not, we switch to an empty filename, which effectively disables logging...
            // Note that empty files will not be persisted!
            if (UseLogfile.Checked) context.SetLogfile(this._logfileName);
            else context.SetLogfile(string.Empty);
            context.FrameworkCommit();
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
                this._logfileName = LogfileSelector.FileName;
                LogfileName.Text = this._logfileName;
                LogfileName.Update();
                Logger.WriteInfo("Custom.Event.EventImp.SettingsForm.SelectLogfile_Click >> Selected logfile: " + this._logfileName);
            }
        }

        /// <summary>
        /// Invoked when the user selected the 'root path chooser' button.
        /// </summary>
        /// <param name="sender">ignored</param>
        /// <param name="e">ignored</param>
        private void SelectRootPath_Click(object sender, EventArgs e)
        {
            if (RootPathSelector.ShowDialog() == DialogResult.OK)
            {
                this._rootPath = RootPathSelector.SelectedPath;
                RootPathName.Text = this._rootPath;
                RootPathName.Update();
                Logger.WriteInfo("Custom.Event.EventImp.SettingsForm.SelectRootPath_Click >> Selected path: " + this._rootPath);
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
            foreach (Control control in InterfaceContracts.Controls)
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
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RAAPIKeyEdit_Click(object sender, EventArgs e)
        {
            using (RESTAuthEditAPIKeys editDialog = new RESTAuthEditAPIKeys(RAAPIKeys.Text))
            {
                if (editDialog.ShowDialog() == DialogResult.OK) RAAPIKeys.Text = editDialog.APIKeys;
            }
        }

        /// <summary>
        /// This event is raised in order to add the name entered in the adjecent field to the list of REST Header Parameters.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void AddParameter_Click(object sender, EventArgs e)
        {
            RESTParameterDeclaration newParam = new RESTParameterDeclaration();
            using (RESTParameterDialog dialog = new RESTParameterDialog(newParam))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (!this._parameterList.ContainsKey(dialog.Parameter.Name))
                    {
                        this._parameterList.Add(dialog.Parameter.Name, dialog.Parameter);
                        ListViewItem newItem = new ListViewItem(dialog.Parameter.Name);
                        newItem.SubItems.Add(dialog.Parameter.Classifier.Name);
                        ParameterList.Items.Add(newItem);
                    }
                    else MessageBox.Show("Duplicate parameter, please try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// This event is raised in order to delete the selected REST Header Parameter from the list of parameters.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void DeleteParameter_Click(object sender, EventArgs e)
        {
            if (ParameterList.SelectedItems.Count > 0)
            {
                ListViewItem key = ParameterList.SelectedItems[0];
                this._parameterList.Remove(key.Text);
                ParameterList.Items.Remove(key);
            }
        }

        /// <summary>
        /// This event is raised when the user selected the 'edit parameter' button. It retrieves the parameter settings and
        /// subsequently shows the 'edit parameter' dialog that the user can utilize in order to update parameter settings.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void EditParameter_Click(object sender, EventArgs e)
        {
            if (ParameterList.SelectedItems.Count > 0)
            {
                ListViewItem key = ParameterList.SelectedItems[0];
                string originalName = key.Text;
                RESTParameterDeclaration param = this._parameterList[key.Text];
                using (RESTParameterDialog dialog = new RESTParameterDialog(param))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        if (dialog.Parameter.Name == originalName || !this._parameterList.ContainsKey(dialog.Parameter.Name))
                        {
                            param = dialog.Parameter;
                            if (param.Name != originalName)
                            {
                                this._parameterList.Remove(originalName);
                                this._parameterList.Add(param.Name, param);
                            }
                            else this._parameterList[originalName] = dialog.Parameter;
                            ParameterList.SelectedItems[0].Text = dialog.Parameter.Name;
                            ParameterList.SelectedItems[0].SubItems[1].Text = dialog.Parameter.Classifier.Name;
                        }
                        else
                        {
                            MessageBox.Show("Renaming parameter resulted in duplicate name, please try again!",
                                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }
    }
}
