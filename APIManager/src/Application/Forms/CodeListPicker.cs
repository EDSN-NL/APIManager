using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Plugin.Application.CapabilityModel;

namespace Plugin.Application.Forms
{
    /// <summary>
    /// Form that facilitates selection of one or more CodeList capabilities from a provided list.
    /// </summary>
    internal partial class CodeListPicker : Form
    {
        private List<Capability> _codeLists;    // All CodeList capabilities available for this service.

        /// <summary>
        /// Default constructor, initializes the form.
        /// </summary>
        internal CodeListPicker()
        {
            InitializeComponent();
            Ok.Enabled = false;
        }

        /// <summary>
        /// Receives the CodeLists that we must use to create the tree view.
        /// </summary>
        /// <param name="codeLists">All CodeLists that are available.</param>
        internal void LoadNodes(string header, List<Capability> codeLists)
        {
            CodeListSet.BeginUpdate();   // Suppresses painting of the tree until we have added all elements.
            CodeListSet.Nodes.Clear();   // Make sure to start with empty view.
            this._codeLists = codeLists;
            TreeNode headerNode = new TreeNode(header)
            {
                Checked = true
            };
            CodeListSet.Nodes.Add(headerNode);
            foreach (Capability cl in codeLists)
            {
                TreeNode valueNode = new TreeNode(cl.Name)
                {
                    Checked = false
                };
                headerNode.Nodes.Add(valueNode);
            }
            headerNode.Expand();
            CodeListSet.EndUpdate();
        }

        /// <summary>
        /// Must be called on return from the dialog in order to receive the items that the user has selected. These are copied in the same
        /// order as they were in the original sorted list. This guarantees that processing will be in the correct order.
        /// </summary>
        /// <returns>List of selected CodeLists.</returns>
        internal List<Capability> GetCheckedNodes()
        {
            List<Capability> selectedCap = new List<Capability>();
            foreach (TreeNode clNode in CodeListSet.Nodes[0].Nodes)
            {
                if (clNode.Checked)
                {
                    int index = clNode.Index;
                    selectedCap.Add(this._codeLists[index]);
                }
            }
            return selectedCap;
        }

        /// <summary>
        /// Simply selects all CodeLists.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void All_Click(object sender, EventArgs e)
        {
            foreach (TreeNode node in CodeListSet.Nodes[0].Nodes) node.Checked = true;
            CodeListSet.Nodes[0].Checked = true;
            Ok.Enabled = true;
        }

        /// <summary>
        /// Simply un-selects all CodeLists.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void None_Click(object sender, EventArgs e)
        {
            foreach (TreeNode node in CodeListSet.Nodes[0].Nodes) node.Checked = false;
            CodeListSet.Nodes[0].Checked = false;
            Ok.Enabled = false;
        }

        /// <summary>
        /// Invoked whenever the user toggled a check-box.
        /// Checks whether we can release the 'ok' button now.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void CodeListSet_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Action != TreeViewAction.Unknown)
            {
                if (e.Node.Parent == null)  // Check/Uncheck parent replicates to children.
                {
                    foreach (TreeNode node in CodeListSet.Nodes[0].Nodes) node.Checked = e.Node.Checked;
                }

                foreach (TreeNode node in CodeListSet.Nodes[0].Nodes)
                {
                    if (node.Checked) Ok.Enabled = true;
                    return;
                }
                Ok.Enabled = false;
            }
        }
    }
}
