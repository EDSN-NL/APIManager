using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Framework.Model;
using Framework.Logging;
using Framework.ConfigurationManagement;

namespace Plugin.Application.Forms
{
    internal partial class CollectAnnotation : Form
    {
        private List<MEClass> _operations;
        private bool _newMinorVersion;

        /// <summary>
        /// Returns the annotation text entered by the user.
        /// </summary>
        internal string Annotation { get { return AnnotationFld.Text; } }

        /// <summary>
        /// Returns the value of the new-minor-version checkbox.
        /// </summary>
        internal bool MinorVersionIndicator { get { return this._newMinorVersion; } }

        /// <summary>
        /// Dialog constructor.
        /// </summary>
        internal CollectAnnotation()
        {
            Logger.WriteInfo("Plugin.Application.Forms.CollectAnnotation >> Creating new dialog...");
            InitializeComponent();
            this._newMinorVersion = false;
            NewMinorVersion.Checked = false;
            if (CMRepositorySlt.GetRepositorySlt().IsCMEnabled) NewMinorVersion.Visible = false;
        }

        /// <summary>
        /// Must be called on return from the dialog in order to receive the items that the user has selected. 
        /// These are copied in the same order as they were in the original sorted list. This guarantees that 
        /// processing will be in the correct order.
        /// </summary>
        /// <returns>List of selected CodeLists.</returns>
        internal List<MEClass> GetCheckedNodes()
        {
            List<MEClass> selectedClasses = new List<MEClass>();
            foreach (TreeNode clNode in OperationTree.Nodes[0].Nodes)
            {
                if (clNode.Checked)
                {
                    int index = clNode.Index;
                    selectedClasses.Add(this._operations[index]);
                }
            }
            return selectedClasses;
        }

        /// <summary>
        /// Receives a text to be printed in the header node as well as a list of classes that we want to 
        /// display underneath the header. If we have a "list of one", the class is selected by default, otherwise
        /// none of the provided classes are selected and the user has to make a choice.
        /// </summary>
        /// <param name="header">Text to be printed in the tree header.</param>
        /// <param name="operations">List of classes that we will show underneath the header for the user to select.</param>
        internal void LoadNodes(string header, List<MEClass> operations)
        {
            OperationTree.BeginUpdate();   // Suppresses painting of the tree until we have added all elements.
            OperationTree.Nodes.Clear();   // Make sure to start with empty view.
            this._operations = operations;
            TreeNode headerNode = new TreeNode(header);
            headerNode.Checked = true;

            OperationTree.Nodes.Add(headerNode);
            foreach (MEClass cl in operations)
            {
                TreeNode valueNode = new TreeNode(cl.Name);
                valueNode.Checked = (operations.Count == 1)? true: false;
                headerNode.Nodes.Add(valueNode);
            }
            headerNode.Expand();
            OperationTree.EndUpdate();
        }

        /// <summary>
        /// Called whenever the user changes the state of the checkbox.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void NewMinorVersion_CheckedChanged(object sender, EventArgs e)
        {
            this._newMinorVersion = NewMinorVersion.Checked;
        }

        /// <summary>
        /// Invoked when the user toggles the checkbox for the root node.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Context for operation.</param>
        private void OperationTree_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Action != TreeViewAction.Unknown)
            {
                if (e.Node.Parent == null)  // Check/Uncheck parent replicates to children.
                {
                    foreach (TreeNode node in OperationTree.Nodes[0].Nodes) node.Checked = e.Node.Checked;
                }
            }
        }
    }
}
