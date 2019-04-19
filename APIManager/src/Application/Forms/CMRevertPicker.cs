using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Logging;

namespace Plugin.Application.Forms
{
    internal partial class CMRevertPicker : Form
    {
        private string _featureTag;     // The tag selected by the user.

        /// <summary>
        /// Returns the tag thas has been selected by the user.
        /// </summary>
        internal string SelectedTag { get { return this._featureTag; } }

        /// <summary>
        /// Returns 'true' when the user wants to create a new version based on a selected feature tag.
        /// </summary>
        internal bool CreateNewFeatureTagVersion { get { return DoCreateNewVersion.Checked; } }

        /// <summary>
        /// Dialog constructor.
        /// </summary>
        internal CMRevertPicker(List<string> tagList)
        {
            InitializeComponent();

            // Load Feature Tags tree-view
            this._featureTag = string.Empty;
            foreach (string tag in tagList)
            {
                // Split the tag in its separate components, which are separated by '/' characters:
                // 'feature/<ticket-id>/<buss-function.container/<service>_V<major>P<minor>B<build>'
                // For the tree, we will only use <ticket-id> and <service>+<version>.
                // When RM is disabled, we have to consider tags that only have three elements (ticket-part is missing)!
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
                    string key = tagElements[1];     // This will be the business function and container now.
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
        /// This event is raised when the user selects a (new) item in the tag list. If we selected a leaf node, the associated
        /// tag is assigned to the featureTag property so it can be retrieved.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void FeatureTags_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode node = FeatureTags.SelectedNode;
            if (node.Level != 0) this._featureTag = node.Tag as string;
        }
    }
}
