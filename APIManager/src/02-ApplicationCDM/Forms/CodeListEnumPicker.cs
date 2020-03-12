using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Model;
using Framework.Logging;

namespace Plugin.Application.Forms
{
    internal partial class CodeListEnumPicker : Form
    {
        private List<MEAttribute> _attribList;

        /// <summary>
        /// Default constructor, initializes the form.
        /// </summary>
        internal CodeListEnumPicker()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Receives the enumerated type that we must use to create the tree view. We pass the list of attributes that
        /// have been selected earlier so we can pre-select these. If we want to start fresh, just ignore this parameter.
        /// </summary>
        /// <param name="enumType">The enumeration that we use as source.</param>
        /// <param name="currentSet">All the attributes that have been selected before.</param>
        internal void LoadNodes(MEEnumeratedType enumType, List<MEAttribute> currentSet = null)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeListEnumPicker.loadNodes >> Building tree for enum: " + enumType.Name);

            EnumPicker.BeginUpdate();   // Suppresses painting of the tree until we have added all elements.
            EnumPicker.Nodes.Clear();   // Make sure to start with empty view.

            this._attribList = enumType.Attributes;
            Label.Text = "Select from '" + enumType.Name + "' to copy:";
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeListEnumPicker >> Showing enum: " + enumType.Name);

            if (currentSet != null)
            {
                string input = string.Empty;
                foreach (MEAttribute at in currentSet)
                {
                    input += at.Name + ", ";
                }
                Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeListEnumPicker >> Already selected: [" + input + "]...");
            }

            foreach (MEAttribute attrib in this._attribList)
            {
                TreeNode valueNode = new TreeNode(attrib.Name);
                if (currentSet != null && currentSet.Contains(attrib))
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeListEnumPicker >> CurrentSet.Contains: " + attrib.Name);
                    valueNode.Checked = true;
                }
                else Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeListEnumPicker >> CurrentSet NOT Contains: " + attrib.Name);
                EnumPicker.Nodes.Add(valueNode);
            }
            EnumPicker.EndUpdate();
        }

        /// <summary>
        /// Must be called on return from the dialog in order to receive the items that the user has selected. These are copied in the same
        /// order as they were in the original sorted list. This guarantees that processing will be in the correct order.
        /// </summary>
        /// <returns>List of selected capability items.</returns>
        internal List<MEAttribute> GetCheckedNodes()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeListEnumPicker.getCheckedNodes >> Retrieving result of dialog...");

            List<MEAttribute> selectedEnums = new List<MEAttribute>();
            foreach (TreeNode attribNode in EnumPicker.Nodes)
            {
                if (attribNode.Checked)
                {
                    int index = attribNode.Index;
                    selectedEnums.Add(this._attribList[index]);
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeListEnumPicker.getCheckedNodes >> Got attribute '" + this._attribList[index].Name + "' at index: " + index);
                }
            }
            return selectedEnums;
        }

        /// <summary>
        /// Simply selects all attributes of the enumeration.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void All_Click(object sender, EventArgs e)
        {
            foreach (TreeNode node in EnumPicker.Nodes) node.Checked = true;
        }

        /// <summary>
        /// Simply un-selects all attributes of the enumeration.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void None_Click(object sender, EventArgs e)
        {
            foreach (TreeNode node in EnumPicker.Nodes) node.Checked = false;
        }
    }
}
