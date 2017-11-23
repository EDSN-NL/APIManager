using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Framework.Logging;
using Framework.Model;
using Framework.Context;

namespace Plugin.Application.Forms
{
    /// <summary>
    /// This form guides the user through all steps necessary to define one or more Code List components. The form is used to
    /// create one or more new Code Lists, or add one or more Code Lists to an existing set.
    /// </summary>
    internal partial class CodeListDirector : Form
    {
        /// <summary>
        /// Helper class used to group some context properties together.
        /// </summary>
        internal class DirectorContext
        {
            internal MEEnumeratedType sourceEnum;           // The enumeration used as basis.
            internal string name;                           // The original name from the request.
            internal string agencyID;                       // ID of agency responsible for managing the enum.
            internal string agencyName;                     // Name of agency responsible for managing the enum.
            internal List<MEAttribute> selectedAttribs;     // The list of attributes that the user has selected.
            internal bool[] completion;                     // Keeps track of completed steps.

            internal bool CompletedIndicator
            {
                get
                {
                    bool result = true;
                    for (int i = 0; i < this.completion.Length; result &= this.completion[i++]);
                    return result;
                }
            }

            internal DirectorContext(string ctxName)
            {
                this.name = ctxName;
                this.sourceEnum = null;
                this.agencyID = string.Empty;
                this.agencyName = string.Empty;
                this.selectedAttribs = null;
                this.completion = new bool[3];
                for (int i = 0; i < this.completion.Length; this.completion[i++] = false);
            }
        }

        // Configuration properties used by this module:
        private const string _CodeListClassStereotype       = "CodeListClassStereotype";
        private const string _CodeListPkgName               = "CodeListPkgName";
        private const string _FrameworkRootPath             = "FrameworkRootPath";
        private const string _IdentifiersPkgName            = "IdentifiersPkgName";
        private const string _CoreDataTypesPathName         = "CoreDataTypesPathName";

        // Configuration properties that define which attributes to parse from an Identifier Object:
        private const string _TypeCodeToken                 = "TypeCodeToken";
        private const string _AgencyIDToken                 = "AgencyIDToken";
        private const string _AgencyNameToken               = "AgencyNameToken";
        private const string _AgencyIDTypeCode              = "AgencyIDTypeCode";

        // Constants used by this module:
        private const string _CodeListEnumClassifierType    = "CodeType";
        private const string _STEP1                         = "step1";
        private const string _STEP2                         = "step2";
        private const string _STEP3                         = "step3";
        private const string _STEP1_Label                   = "1: Select source enumeration...";
        private const string _STEP2_Label                   = "2: Select managing agency identifier...";
        private const string _STEP3_Label                   = "3: Select enumeration values to copy...";

        private SortedList<string, DirectorContext> _context;
        private bool _checkState;                          // Used when users want to click check-boxes. We record old state to avoid changes.

        internal CodeListDirector(string codeListDeclName)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeListDirector >> Initializing director for declaration '" + codeListDeclName + "'...");
            InitializeComponent();
            TopLabel.Text = "Code List Declaration: " + codeListDeclName;
            this._context = new SortedList<string, DirectorContext>();
            this._checkState = false;
        }

        /// <summary>
        /// Returns the result of user actions.
        /// </summary>
        internal SortedList<string, DirectorContext> Context {  get { return this._context; } }

        /// <summary>
        /// Loads all the root-node names with the generic steps below them, initializing the director view.
        /// </summary>
        /// <param name="rootNames">List of Code List names we're going to process.</param>
        internal void LoadRootNodes(List<string> rootNames)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeListDirector.loadRootNodes >> Loading root names...");

            CodeListProgress.BeginUpdate();   // Suppresses painting of the tree until we have added all elements.
            CodeListProgress.Nodes.Clear();   // Make sure to start with empty view.

            foreach (string rootName in rootNames)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeListDirector.loadRootNodes >> Adding node: " + rootName);
                TreeNode rootNode = new TreeNode(rootName)
                {
                    Checked = false,
                    Name = rootName
                };
                TreeNode step1Node = new TreeNode(_STEP1_Label);
                TreeNode step2Node = new TreeNode(_STEP2_Label);
                TreeNode step3Node = new TreeNode(_STEP3_Label);
                step1Node.Checked = false;
                step1Node.Name = _STEP1;
                step2Node.Checked = false;
                step2Node.Name = _STEP2;
                step3Node.Checked = false;
                step3Node.Name = _STEP3;
                rootNode.Nodes.Add(step1Node);
                rootNode.Nodes.Add(step2Node);
                rootNode.Nodes.Add(step3Node);
                CodeListProgress.Nodes.Add(rootNode);
                this._context.Add(rootName, new DirectorContext(rootName));
            }
            Ok.Enabled = false;
            CodeListProgress.EndUpdate();
        }

        /// <summary>
        /// This handler is activated when the user clicks on one of the nodes in the director. Depending on the node that is clicked,
        /// the user is asked to perform a step that takes him closer to completion of Code List definition.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Used to retrieve selected node.</param>
        private void CodeListProgress_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode currentNode = e.Node;
            ContextSlt appContext = ContextSlt.GetContextSlt();

            if (currentNode.Parent != null)
            {
                // We are in one of the steps, find out which one and retrieve the associated context...
                // We ignore clicks on the parent nodes.
                DirectorContext currContext = this._context[currentNode.Parent.Name];
                switch (currentNode.Name)
                {
                    case _STEP1:
                        // We selected the 'select source enumeration' step, make sure that we MUST also (re-)do step 3...
                        currentNode.Parent.Checked = false;
                        currContext.selectedAttribs = null;
                        currContext.completion[2] = false;
                        foreach (TreeNode node in currentNode.Parent.Nodes)
                        {
                            if (node.Name == _STEP3)
                            {
                                node.Text = _STEP3_Label;
                                node.Checked = false;
                                break;
                            }
                        }
                        currContext.sourceEnum = appContext.SelectDataType(true) as MEEnumeratedType;
                        if (currContext.sourceEnum != null)
                        {
                            currentNode.Text = "1: Source enumeration: '" + currContext.sourceEnum.Name + "'.";
                            currentNode.Checked = true;
                            currContext.completion[0] = true;
                        }
                        else
                        {
                            // User cancelled the selection! Back to default...
                            currentNode.Text = _STEP1_Label;
                            currentNode.Checked = false;
                            currContext.completion[0] = false;
                        }
                        break;

                    case _STEP2:
                        // We selected the 'select managing acency' step...
                        string agencyID = string.Empty; ;
                        string agencyName = string.Empty;
                        if (GetAgencyMetaData(out agencyID, out agencyName))
                        {
                            currentNode.Text = "2: Agency: '" + agencyName + "'.";
                            currentNode.Checked = true;
                            currContext.agencyID = agencyID;
                            currContext.agencyName = agencyName;
                            currContext.completion[1] = true;
                            if (currContext.CompletedIndicator) currentNode.Parent.Checked = true;
                        }
                        break;

                    case _STEP3:
                        // We selected the 'Select enumeration values to copy' step...
                        // This requires step 1 to be completed!
                        if (currContext.completion[0] && currContext.sourceEnum != null)
                        {
                            using (CodeListEnumPicker enumPicker = new CodeListEnumPicker())
                            {
                                enumPicker.LoadNodes(currContext.sourceEnum, currContext.selectedAttribs);
                                if (enumPicker.ShowDialog() == DialogResult.OK)
                                {
                                    currContext.selectedAttribs = enumPicker.GetCheckedNodes();
                                    currentNode.Text = "3: Attributes selected.";
                                    currentNode.Checked = true;
                                    currContext.completion[2] = true;
                                    if (currContext.CompletedIndicator) currentNode.Parent.Checked = true;
                                }
                            }
                        }
                        else MessageBox.Show("Please select a source enumeration first!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;

                    default:
                        break;
                }
            }

            // Check is we processed all root nodes, in which case we can release the 'Ok' button...
            bool exitAllowed = true;
            foreach (DirectorContext ctx in this._context.Values) exitAllowed &= ctx.CompletedIndicator;
            Ok.Enabled = exitAllowed;
        }

        /// <summary>
        /// Helper function that requests the user to select the proper agency identifier object to use for the Code List.
        /// </summary>
        /// <param name="agencyID">Will receive selected agency ID (URN).</param>
        /// <param name="agencyName">Will receive selected agency name.</param>
        /// <returns>True when the user has selected a proper object, False on cancel.</returns>
        private bool GetAgencyMetaData(out string agencyID, out string agencyName)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.CodeListDirector.getAgencyMetaData >> Collecting Agency...");

            // Let the user select the Agency IDentifier...
            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            string agencyIDTypeCode = context.GetConfigProperty(_AgencyIDTypeCode);
            string typeCodeToken = context.GetConfigProperty(_TypeCodeToken);
            string agencyIDToken = context.GetConfigProperty(_AgencyIDToken);
            string agencyNameToken = context.GetConfigProperty(_AgencyNameToken);
            bool correctID = false;
            agencyID = string.Empty;
            agencyName = string.Empty;

            MEPackage identifierRepository = model.FindPackage(context.GetConfigProperty(_FrameworkRootPath), context.GetConfigProperty(_IdentifiersPkgName));
            if (identifierRepository != null)
            {
                identifierRepository.ShowInTree();
                do
                {
                    MEObject agencyDescriptor = context.SelectIdentifier();
                    if (agencyDescriptor != null)
                    {
                        List<Tuple<string, string>> stateVars = agencyDescriptor.RunTimeState;
                        for (int i = 0; i < stateVars.Count; i++)
                        {
                            if (stateVars[i].Item1 == typeCodeToken && stateVars[i].Item2 == agencyIDTypeCode) correctID = true;
                            if (stateVars[i].Item1 == agencyIDToken) agencyID = stateVars[i].Item2;
                            if (stateVars[i].Item1 == agencyNameToken) agencyName = stateVars[i].Item2;
                        }
                        if (!correctID)
                        {
                            Logger.WriteWarning("Plugin.Application.CapabilityModel.CodeListDirector.getAgencyMetaData >> Wrong type of identifier selected!");
                            MessageBox.Show("Please select an Identifier of type '" + _AgencyIDTypeCode + "'!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        agencyDescriptor.Dispose();
                    }
                    else return false;  // User has cancelled!
                } while (!correctID);
                return true;
            }
            else
            {
                MessageBox.Show("Can't find the Framework 'Identifiers' package, aborting!", "Fatal error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// User should not be able to change the checkmarks. So, when the user attempts to click on one, we record the old value
        /// and write this back immediately after clicking...
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Used to retrieve the selected node.</param>
        private void CodeListProgress_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Action != TreeViewAction.Unknown) e.Node.Checked = this._checkState;
        }

        /// <summary>
        /// User should not be able to change the checkmarks. So, when the user attempts to click on one, we record the old value
        /// and write this back immediately after clicking...
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Used to retrieve the selected node.</param>
        private void CodeListProgress_BeforeCheck(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Action != TreeViewAction.Unknown) this._checkState = e.Node.Checked;
        }
    }
}
