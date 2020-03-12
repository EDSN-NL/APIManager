using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Model;
using Framework.Context;
using Plugin.Application.CapabilityModel.API;

namespace Plugin.Application.Forms
{
    /// <summary>
    /// Dialog that facilitates creation of a new Profile Set or editing of an existing Profile Set.
    /// </summary>
    internal partial class RESTProfileSetDialog : Form
    {
        // Configuration properties used by this module:
        private const string _EmptyResourceName = "EmptyResourceName";

        private RESTResourceDeclaration _resourceDeclaration;

        /// <summary>
        /// Returns the user-assigned name of the Profile Set.
        /// </summary>
        internal string SetName { get { return ProfileSetName.Text; } }

        /// <summary>
        /// Dialog that facilitates creation of a new Profile Set or editing of an existing one, is used from within a Resource Declaration object.
        /// </summary>
        /// <param name="resourceDecl">Resource declaration that we're either creating or editing</param>
        internal RESTProfileSetDialog(RESTResourceDeclaration resourceDecl)
        {
            InitializeComponent();
            this._resourceDeclaration = resourceDecl;

            if (string.IsNullOrEmpty(resourceDecl.Name) || resourceDecl.Name == ContextSlt.GetContextSlt().GetConfigProperty(_EmptyResourceName))
            {
                this.Text = "Create new Profile Set";
                ProfileSetName.Text = string.Empty;
                this._resourceDeclaration.Name = string.Empty;
                Ok.Enabled = false;
            }
            else
            {
                this.Text = "Edit existing Profile Set";
                ProfileSetName.Text = resourceDecl.Name;
                Ok.Enabled = true;

                // Load the existing list of profiles...
                foreach (KeyValuePair<string, MEClass> profile in resourceDecl.ProfileSet)
                {
                    ListViewItem newItem = new ListViewItem(profile.Key);
                    newItem.Name = profile.Key;
                    newItem.SubItems.Add(profile.Value.Name);
                    ProfileList.Items.Add(newItem);
                }
            }

            // Assign context menus to the appropriate controls...
            ProfileList.ContextMenuStrip = ClassListMenuStrip;
        }

        /// <summary>
        /// This event is raised when the user has entered a name for the profile set. We check whether this is indeed a
        /// unique name and if so, assign the name to the Resource Declaration object. A valid name 'releases' the OK button.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void ProfileNameFld_Leave(object sender, EventArgs e)
        {
            string profileName = ProfileSetName.Text.Trim();
            if (!string.IsNullOrEmpty(profileName) && ((RESTService)this._resourceDeclaration.Parent.RootService).FindProfileSet(profileName) == null)
            {
                this._resourceDeclaration.Name = profileName;
                Ok.Enabled = true;
            }
            else
            {
                MessageBox.Show("Duplicate Profile Set name, please try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Ok.Enabled = false;
            }
        }

        /// <summary>
        /// This event is raised when the user clicks the 'Add Profile' button or selects the 'Add' menu operation. 
        /// We invoke the 'AddProfile' method in the resource declaration to actually register the new profile. The result is 
        /// shown in the dialog window.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void AddProfile_Click(object sender, EventArgs e)
        {
            Tuple<string, string> newProfile = this._resourceDeclaration.AddProfile();
            if (newProfile != null)
            {
                ListViewItem newItem = new ListViewItem(newProfile.Item1);
                newItem.Name = newProfile.Item1;
                newItem.SubItems.Add(newProfile.Item2);
                ProfileList.Items.Add(newItem);
            }
        }

        /// <summary>
        /// This event is raised when the user clicks the 'Delete Profile' button or selects 'Delete' menu operation. 
        /// We invoke the 'DeleteProfile' method in the resource declaration to actually remove the selected profile. The result is 
        /// shown in the dialog window.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void DeleteProfile_Click(object sender, EventArgs e)
        {
            if (ProfileList.SelectedItems.Count > 0)
            {
                ListViewItem key = ProfileList.SelectedItems[0];
                this._resourceDeclaration.DeleteProfile(key.Text);
                ProfileList.Items.Remove(key);
            }
        }
    }
}
