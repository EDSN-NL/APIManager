using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Plugin.Application.CapabilityModel;

namespace Plugin.Application.Forms
{
    /// <summary>
    /// Form that facilitates selection of one or more Capabilities from a provided list
    /// </summary>
    internal partial class CapabilityPicker : Form
    {
        private List<Capability> _capabilityList;       // List of Capabilities shown in the selector box.
        private bool            _newMinorVersion;       // Reflects the state of the 'newMinorVersion' check box.

        internal bool MinorVersionIndicator { get { return this._newMinorVersion; } }

        /// <summary>
        /// Default constructor, initializes the form and displays a customized label.
        /// Indicator 'showNewMinorVersion' can be set to 'false' in order to suppress the checkbox.
        /// </summary>
        /// <param name="labelText">Text to show on the label field of the dialog.</param>
        /// <param name="capabilityList">The list of Capabilities from which to choose.</param>
        /// <param name="initialSelection">Matches the initial state of all Capability check boxes (true == checked).</param>
        /// <param name="showNewMinorVersion">Whether or not to show the 'Update minor version' check box.</param>
        internal CapabilityPicker(string labelText, List<Capability> capabilityList, bool initialSelection = true, bool showNewMinorVersion = true)
        {
            InitializeComponent();
            this._newMinorVersion = false;
            NewMinorVersion.Checked = false;
            Label.Text = labelText;
            if (!showNewMinorVersion) NewMinorVersion.Visible = false;

            this._capabilityList = capabilityList;
            CapabilityList.CheckOnClick = true;

            CapabilityList.BeginUpdate();
            CapabilityList.Items.Clear();
            foreach (Capability cap in capabilityList) CapabilityList.Items.Add(cap, initialSelection);
            CapabilityList.EndUpdate();
        }

        /// <summary>
        /// Must be called on return from the dialog in order to receive the items that the user has selected. These are copied in the same
        /// order as they were in the original sorted list. This guarantees that processing will be in the correct order.
        /// </summary>
        /// <returns>List of selected Interfaces.</returns>
        internal List<Capability> GetCheckedCapabilities()
        {
            List<Capability> selectedCapabilities = new List<Capability>();
            foreach(int checkedIndex in CapabilityList.CheckedIndices)
            {
                if (this._capabilityList[checkedIndex] != null) selectedCapabilities.Add(this._capabilityList[checkedIndex]);
            }
            return selectedCapabilities;
        }

        /// <summary>
        /// Simply selects all Capabilities.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void All_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < CapabilityList.Items.Count; CapabilityList.SetItemChecked(i++, true)) ;
        }

        /// <summary>
        /// Simply un-selects all Capabilities.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void None_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < CapabilityList.Items.Count; CapabilityList.SetItemChecked(i++, false)) ;
        }

        /// <summary>
        /// Invoked whenever the user toggled the checkbox.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e"Ignored.></param>
        private void NewMinorVersion_CheckedChanged(object sender, EventArgs e)
        {
            this._newMinorVersion = NewMinorVersion.Checked;
        }

        /// <summary>
        /// Invoked whenever the user clicks on an item in the list of Capabilities. The function toggles the state of 
        /// the associated check-box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CapabilityList_SelectedValueChanged(object sender, EventArgs e)
        {
            if (CapabilityList.SelectedIndex != -1)
            {
                bool itemChecked = CapabilityList.GetItemChecked(CapabilityList.SelectedIndex);
                CapabilityList.SetItemCheckState(CapabilityList.SelectedIndex, itemChecked ? CheckState.Checked: CheckState.Unchecked);
            }
        }
    }
}
