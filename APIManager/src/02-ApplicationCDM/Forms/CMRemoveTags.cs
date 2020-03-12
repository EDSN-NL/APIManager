using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Logging;

namespace Plugin.Application.Forms
{
    internal partial class CMRemoveTags : Form
    {
        private List<string> _featureTags;     // The tags selected by the user.

        /// <summary>
        /// Returns the tag thas has been selected by the user.
        /// </summary>
        internal List<string> SelectedTags { get { return this._featureTags; } }

        /// <summary>
        /// Dialog constructor.
        /// </summary>
        internal CMRemoveTags(List<string> tagList)
        {
            InitializeComponent();
            this._featureTags = new List<string>();

            // Load Feature Tags tree-view
            foreach (string tag in tagList)
            {
                // Typically, a tag has 4 elements. When RM is disabled, we might have either 3 or 4 elements (since old tags
                // could still be around that DO have a ticket ID in them). So we have to check for both!
                string[] tagElements = tag.Split('/');
                if (tagElements.Length == 4)
                {
                    string key = tagElements[1];
                    TreeNode myNode;
                    if (!FeatureTags.Nodes.ContainsKey(key)) myNode = FeatureTags.Nodes.Add(key, key);
                    else myNode = FeatureTags.Nodes[key];
                    TreeNode childNode = new TreeNode(tagElements[3]);
                    childNode.Tag = tag;
                    myNode.Nodes.Add(childNode);
                }
                else if (tagElements.Length == 3)    // When RM is disabled, the 'ticket part' is missing.
                {
                    string key = tagElements[1];    // This will be the business function and container now.
                    TreeNode myNode;
                    if (!FeatureTags.Nodes.ContainsKey(key)) myNode = FeatureTags.Nodes.Add(key, key);
                    else myNode = FeatureTags.Nodes[key];
                    TreeNode childNode = new TreeNode(tagElements[2]);
                    childNode.Tag = tag;
                    myNode.Nodes.Add(childNode);
                }
                else Logger.WriteWarning("Ignored non-standard tag '" + tag + "'!");
            }
        }

        /// <summary>
        /// This event is raised when the user selects a (new) item in the tag list. If the user selected a root node, we will
        /// copy the select state (check/uncheck) to all children. Otherwise, the event does nothing.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Context for the call.</param>
        private void FeatureTags_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Action != TreeViewAction.Unknown && e.Node.Level == 0 && e.Node.Nodes.Count > 0)
            {
                foreach (TreeNode child in e.Node.Nodes) child.Checked = e.Node.Checked;
            }
        }

        /// <summary>
        /// This event is raised when the user clicks the 'ok' button. We now collect all selected nodes and build the actual
        /// list of tags to delete.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void Ok_Click(object sender, EventArgs e)
        {
            foreach (TreeNode parent in FeatureTags.Nodes)
            {
                foreach (TreeNode child in parent.Nodes)
                {
                    if (child.Checked) this._featureTags.Add((string)child.Tag);
                }
            }
        }
    }
}
