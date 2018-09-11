using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Framework.ConfigurationManagement;
using Plugin.Application.CapabilityModel;

namespace Plugin.Application.Forms
{
    internal partial class CheckoutService : Form
    {
        private const string _UseFeatureLabel = "UseFeature";

        private Service _service;
        private string _featureTag;
        private Tuple<int, int> _newVersion;

        /// <summary>
        /// Use these properties to check which version information has been selected by the user (either one can be used).
        /// </summary>
        internal bool UseNewVersion { get { return this._newVersion != null; } }
        internal bool UseFeatureTag { get { return !string.IsNullOrEmpty(this._featureTag); } }

        /// <summary>
        /// Getters for the various dialog properties...
        /// </summary>
        internal string FeatureTag          { get { return this._featureTag; } }
        internal Tuple<int,int> NewVersion  { get { return this._newVersion; } }
        internal string TicketID            { get { return TicketIDFld.Text; } }
        internal string ProjectID           { get { return ProjectIDFld.Text; } }

        /// <summary>
        /// Service Checkout dialog constructor. Receives the Service that we want to check-out.
        /// </summary>
        /// <param name="service">Service to be checked-out</param>
        internal CheckoutService(Service service)
        {
            InitializeComponent();

            this._service = service;
            this._featureTag = null;
            this._newVersion = new Tuple<int, int>(service.Version.Item1, service.Version.Item2 + 1);
            ExistingVersion.Text = service.Version.Item1 + "." + service.Version.Item2;
            NewVersionFld.Text = service.Version.Item1 + "." + (service.Version.Item2 + 1);
            Ok.Enabled = false;

            // Load Feature Tags tree-view.
            List<string> tagList = CMRepositorySlt.GetRepositorySlt().GetTags("feature");
            foreach (string tag in tagList)
            {
                if (tag.Contains(this._service.Name))
                {
                    // Split the tag in its separate components, which are separated by '/' characters:
                    // 'feature/<ticket-id>/<buss-function.container/<service>_V<major>P<minor>B<build>'
                    // For the tree, we will only use <ticket-id> and <service>+<version>.
                    string[] tagElements = tag.Split('/');
                    if (!FeatureTags.Nodes.ContainsKey(tagElements[1])) FeatureTags.Nodes.Add(tagElements[1]);
                    FeatureTags.Nodes[tagElements[1]].Nodes.Add(tagElements[3]);
                }
            }
        }

        /// <summary>
        /// This event is raised when the user leaves one of the key fields (TicketID or ProjectID).
        /// We enable the Ok button when BOTH these fields have a value (any value).
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored.</param>
        private void Keyfield_Leave(object sender, EventArgs e)
        {
            Ok.Enabled = !string.IsNullOrEmpty(TicketIDFld.Text) && !string.IsNullOrEmpty(ProjectIDFld.Text);
        }

        /// <summary>
        /// This event is raised when the user changed the contents of the 'new version' field. In this case,
        /// we remove any feature tag that has been selected earlier and we check the entered version, which must
        /// be in format major.minor.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void NewVersion_Leave(object sender, EventArgs e)
        {
            string[] newVersion = NewVersionFld.Text.Split('.');
            if (newVersion.Length == 2)
            {
                int major, minor;
                if (int.TryParse(newVersion[0], out major) && int.TryParse(newVersion[1], out minor))
                {
                    this._featureTag = null;
                    this._newVersion = new Tuple<int, int>(major, minor);
                }
            }
        }

        /// <summary>
        /// This event is raised when the user clicks the 'Use Feature Tag' button. This copies the selected entry from the
        /// feature list and creates te corresponding feature tag.
        /// The button will only have an effect when a service-node in the tree is selected.
        /// We write a label string to the 'new version' box to indicate that this has been overruled.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void UseFeatureTag_Click(object sender, EventArgs e)
        {
            TreeNode node = FeatureTags.SelectedNode;
            if (node.Level != 0)
            {
                NewVersionFld.Text = _UseFeatureLabel;
                this._newVersion = null;
                TreeNode parentNode = node.Parent;
                this._featureTag = "feature/" + parentNode.Name + "/" + this._service.BusinessFunctionID + "." +
                                   this._service.ContainerPkg.Name + "/" + node.Name;
            }
        }
    }
}
