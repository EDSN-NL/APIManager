using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Framework.Logging;

namespace Plugin.Application.Forms
{
    public partial class RESTAuthEditAPIKeys : Form
    {
        private const string _RootKeyName   = "APIKeys";
        private const string _GroupName     = "Group_";

        private string _location;           // Defines the location of the key, currently "header" or "query" are allowed.
        private string _apiKeys;            // Defines the grouping, name and location of API Keys.
        private TreeNode _currentGroup;     // Currently active group or NULL if nothing selected.
        private TreeNode _currentKey;       // Currently selected key or NULL if nothing selected.
        private TreeNode _rootNode;         // Root of the tree is always available.
        int _lastGroupID;                   // Contains the last-used group ID.

        internal string APIKeys { get { return GetAPIKeys(); } }

        /// <summary>
        /// Constructor for the dialog. Receives the existing configuration as a string and parses this to
        /// populate the tree.
        /// </summary>
        /// <param name="apiKeys">Existing configuration.</param>
        public RESTAuthEditAPIKeys(string apiKeys)
        {
            Logger.WriteInfo("Plugin.Application.Forms.RESTAuthEditAPIKeys >> Initializing with configuration: '" + apiKeys + "'...");
            InitializeComponent();
            InitializeTree(apiKeys);


            // Fetch the default location...
            foreach (Control control in LocationBox.Controls)
            {
                if (control is RadioButton && ((RadioButton)control).Checked)
                {
                    Logger.WriteInfo("Plugin.Application.Forms.RESTAuthEditAPIKeys >> Default location is: '" + control.Tag + "'.");
                    this._location = control.Tag as string;
                    break;
                }
            }
        }

        /// <summary>
        /// Parses the KeyTree and builds a string of key:location tuples, ordered by group.
        /// Groups are returned as "(key:location,key2:location,...)"
        /// </summary>
        /// <returns></returns>
        private string GetAPIKeys()
        {
            if (this._apiKeys == string.Empty)
            {
                foreach (TreeNode node in this._rootNode.Nodes)
                {
                    if (node.Text.StartsWith(_GroupName))
                    {
                        this._apiKeys += (this._apiKeys == string.Empty) ? "(" : ",(";
                        bool firstOne = true;
                        foreach (TreeNode groupNode in node.Nodes)
                        {
                            this._apiKeys += firstOne ? groupNode.Text : "," + groupNode.Text;
                            firstOne = false;
                        }
                        this._apiKeys += ")";
                    }
                    else
                    {
                        this._apiKeys += (this._apiKeys == string.Empty) ? node.Text : "," + node.Text;
                    }
                }
            }
            Logger.WriteInfo("Plugin.Application.Forms.RESTAuthEditAPIKeys.GetAPIKeys >> Retrieved: '" + this._apiKeys + "'.");
            return this._apiKeys;
        }

        /// <summary>
        /// Initialization method parses the list of key:location tuples, looking for group separators.
        /// It splits the list per group (or lists of non-grouped nodes) and calls LoadKeys to actually
        /// load the keys into the tree.
        /// </summary>
        /// <param name="apiKeys"></param>
        private void InitializeTree(string apiKeys)
        {
            try
            {
                Logger.WriteInfo("Plugin.Application.Forms.RESTAuthEditAPIKeys >> Parsing API Keys '" + apiKeys + "'...");

                this._lastGroupID = 0;
                this._currentGroup = null;
                this._currentKey = null;
                this._rootNode = new TreeNode(_RootKeyName);
                this._apiKeys = apiKeys;
                KeyTree.Nodes.Add(this._rootNode);

                int groupIndex = 0;
                while (apiKeys.Length > 0)
                {
                    groupIndex = apiKeys.IndexOf("(");
                    if (groupIndex != -1)
                    {
                        if (groupIndex > 0)
                        {
                            string separateKeys = apiKeys.Substring(0, groupIndex - 1);
                            LoadKeys(separateKeys, false);
                        }
                        int endOfGroup = apiKeys.IndexOf(")");
                        string groupKeys = apiKeys.Substring(groupIndex + 1, endOfGroup - groupIndex - 1);
                        LoadKeys(groupKeys, true);
                        if (endOfGroup == apiKeys.Length - 1) apiKeys = string.Empty;
                        else apiKeys = apiKeys.Substring(endOfGroup + 2);   // Skip the ")," pattern at the end of the group.
                    }
                    else
                    {
                        // No (more) groups, process remainder of string...
                        LoadKeys(apiKeys, false);
                        break;
                    }
                }
            }
            catch
            {
                Logger.WriteError("Plugin.Application.Forms.RESTAuthEditAPIKeys >> Format error in API Keys '" + apiKeys + "', discarded!");
                KeyTree.Nodes.Clear();
                KeyTree.Nodes.Add(new TreeNode(_RootKeyName));
            }
            this._apiKeys = string.Empty;   // Clear this to facilitate fresh result collection latereon.
        }

        /// <summary>
        /// Is called with a string of key:location tuples and a flag that indicates whether or not the string
        /// has to go into a group or not. If it has to go into a group, a new group node is created first,
        /// otherwise, all tuples are added directly to the root node.
        /// </summary>
        /// <param name="keys">Comma separated list of key:location tuples.</param>
        /// <param name="inGroup">True when the list has to go into a new group.</param>
        private void LoadKeys(string keys, bool inGroup)
        {
            TreeNode parentNode = null;
            if (inGroup)
            {
                this._lastGroupID++;   // Create new group number.
                parentNode = new TreeNode(_GroupName + this._lastGroupID);
                this._rootNode.Nodes.Add(parentNode);
            }
            string[] tuples = keys.Split(',');
            foreach (string tuple in tuples)
            {
                if (parentNode != null) parentNode.Nodes.Add(new TreeNode(tuple));
                else this._rootNode.Nodes.Add(new TreeNode(tuple));
            }
        }

        /// <summary>
        /// This event is raised whenever we toggle the 'location' radio button.
        /// We will update the location with a new value.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void LocationHeader_CheckedChanged(object sender, EventArgs e)
        {
            // Fetch the new location...
            foreach (Control control in LocationBox.Controls)
            {
                if (control is RadioButton && ((RadioButton)control).Checked)
                {
                    this._location = control.Tag as string;
                    break;
                }
            }
        }

        /// <summary>
        /// This event is raised when the user clicks the 'Add Key' button. The function
        /// retrieves the current key name and adds this the currently selected group. If
        /// no group is selected, the name will be a node by itself.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddKey_Click(object sender, EventArgs e)
        {
            string entry = KeyName.Text + ":" + this._location;
            this._currentKey = new TreeNode(entry);
            if (this._currentGroup == null) this._rootNode.Nodes.Add(this._currentKey);
            else this._currentGroup.Nodes.Add(this._currentKey);
            KeyTree.ExpandAll();
        }

        /// <summary>
        /// This event is raised whenever the user selected a node in the tree. Since at any time we
        /// only want one single node to be selected, we check whether the user selected a group or
        /// a single node and set the non-selected type to NULL.
        /// If the user selects the root node, both current group and current key are set to NULL.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void KeyTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (KeyTree.SelectedNode.Text.StartsWith(_GroupName))
            {
                this._currentGroup = KeyTree.SelectedNode;
                this._currentKey = null;
            }
            else if (KeyTree.SelectedNode.Text == _RootKeyName)
            {
                this._currentGroup = null;
                this._currentKey = null;
            }
            else
            {
                this._currentGroup = null;
                this._currentKey = KeyTree.SelectedNode;
            }
        }

        /// <summary>
        /// This event is raised whenever the user clicked the 'Add Group' button.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void AddGroup_Click(object sender, EventArgs e)
        {
            this._lastGroupID++;    // Create a new group ID.
            this._currentGroup = new TreeNode(_GroupName + this._lastGroupID);
            this._rootNode.Nodes.Add(this._currentGroup);
        }

        /// <summary>
        /// This event is raised whenever the user clicked the 'Delete Group' button. A group must be
        /// selected (currentGroup not NULL) to make this work. The event deletes the group and all 
        /// keys that are in that group.
        /// The group index is not affected by this.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteGroup_Click(object sender, EventArgs e)
        {
            if (this._currentGroup != null)
            {
                this._rootNode.Nodes.Remove(this._currentGroup);
                this._currentGroup = null;
            }
        }

        /// <summary>
        /// This event is raised whenever the user clicked the 'Delete Key' button. The event locates
        /// the parent of the currently selected key (if any) and if found, removes the key.
        /// Current key is set to NULL on return.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteKey_Click(object sender, EventArgs e)
        {
            if (this._currentKey != null)
            {
                TreeNode parent = this._currentKey.Parent;
                if (parent != null) parent.Nodes.Remove(this._currentKey);
                this._currentKey = null;
            }
        }
    }
}
