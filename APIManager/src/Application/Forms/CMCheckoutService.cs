using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Framework.ConfigurationManagement;
using Plugin.Application.CapabilityModel;
using Framework.Logging;

namespace Plugin.Application.Forms
{
    internal partial class CMCheckoutService : Form
    {
        private const string _UseFeatureLabel = "Revert";

        private Service _service;
        private string _featureTag;
        private Tuple<int, int> _newVersion;
        private RMServiceTicket _ticket;
        private Ticket _remoteTicket;

        /// <summary>
        /// Returns 'true' when the user has selected a version to use (either existing or new version)
        /// </summary>
        internal bool UseVersion    { get { return this._newVersion != null; } }

        /// <summary>
        /// Returns 'true' when the user has selected a feature tag to use.
        /// </summary>
        internal bool UseFeatureTag { get { return !string.IsNullOrEmpty(this._featureTag); } }

        /// <summary>
        /// Returns 'true' when the user wants to create a new version based on a selected feature tag.
        /// </summary>
        internal bool CreateNewFeatureTagVersion { get { return FeatureNewVersion.Checked; } }

        /// <summary>
        /// Getters for the various dialog properties...
        /// </summary>
        internal string FeatureTag          { get { return this._featureTag; } }
        internal Tuple<int,int> NewVersion  { get { return this._newVersion; } }
        internal string TicketID            { get { return TicketIDFld.Text; } }
        internal string ProjectOrderID      { get { return ProjectIDFld.Text; } }
        internal Ticket RemoteTicket        { get { return this._remoteTicket; } }

        /// <summary>
        /// Returns the ORIGINAL service ticket. When the user changed ticket ID and/or project number, this property will be forced to 
        /// NULL to indicate that a new ticket must be created. If the original service did not have a ticket, the property will also be null.
        /// </summary>
        internal RMServiceTicket Ticket { get { return this._ticket; } }

        /// <summary>
        /// Service Checkout dialog constructor. Receives the Service that we want to check-out.
        /// </summary>
        /// <param name="service">Service to be checked-out</param>
        internal CMCheckoutService(Service service)
        {
            InitializeComponent();

            this._service = service;
            this._featureTag = null;
            this._newVersion = new Tuple<int, int>(service.Version.Item1, service.Version.Item2 );
            ExistingVersion.Text = service.Version.Item1 + "." + service.Version.Item2;
            NewVersionFld.Text = service.Version.Item1 + "." + service.Version.Item2;

            if (service.Ticket != null && !service.Ticket.Closed)
            {
                this._ticket = service.Ticket;
                TicketIDFld.Text = this._ticket.ID;
                ProjectIDFld.Text = this._ticket.ProjectOrderID;
                Ok.Enabled = true;
            }
            else Ok.Enabled = false;

            // Load Feature Tags tree-view.
            List<LibGit2Sharp.Tag> tagList = CMRepositorySlt.GetRepositorySlt().GetTags(this._service.BusinessFunctionID + "." + 
                                                                                        this._service.ContainerPkg.Name + "/" + 
                                                                                        this._service.Name);
            foreach (LibGit2Sharp.Tag tag in tagList)
            {
                // Split the tag in its separate components, which are separated by '/' characters:
                // 'feature/<ticket-id>/<buss-function.container/<service>_V<major>P<minor>B<build>'
                // For the tree, we will only use <ticket-id> and <service>+<version>.
                string[] tagElements = tag.FriendlyName.Split('/');
                if (tagElements.Length == 4)    // To comply with our standard, a tag must contain exactly 4 fields.
                {
                    string key = tagElements[1];
                    TreeNode myNode;
                    if (!FeatureTags.Nodes.ContainsKey(key)) myNode = FeatureTags.Nodes.Add(key, key);
                    else myNode = FeatureTags.Nodes[key];
                    TreeNode childNode = new TreeNode(tagElements[3]);
                    childNode.Tag = tag;
                    myNode.Nodes.Add(childNode);
                }
                else Logger.WriteWarning("Ignored non-standard tag '" + tag + "'!");
            }
        }

        /// <summary>
        /// This event is raised when the user leaves one of the key fields (TicketID or ProjectID).
        /// We enable the Ok button when BOTH these fields have a value (any value).
        /// If the service had an existing ticket and the new values for ID or project nr. are different, we delete the existing
        /// service to indicate that a new ticket must be created. We don't create the ticket here since this immediately creates
        /// ticket classes in the model and we want to postpone this until we're sure that the user indeed wants to go ahead
        /// with all provided information.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored.</param>
        private void Keyfield_Leave(object sender, EventArgs e)
        {
            bool validTicketID = false;
            if (TicketIDFld.Text != string.Empty)
            {
                if (this._ticket == null || (this._ticket != null && this._ticket.ID != TicketIDFld.Text))
                {
                    TicketServerSlt ticketSrv = TicketServerSlt.GetTicketServerSlt();
                    Ticket remoteTicket = ticketSrv.GetTicket(TicketIDFld.Text);
                    if (remoteTicket == null || ticketSrv.IsClosed(remoteTicket))
                    {
                        MessageBox.Show("Specified ticket does not exist or is closed, please try again!",
                                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        TicketIDFld.Text = string.Empty;
                    }
                    else
                    {
                        this._remoteTicket = remoteTicket;
                        this._ticket = null;
                        validTicketID = true;
                    }
                }
                else if (this._ticket != null && this._ticket.ID == TicketIDFld.Text) validTicketID = true;
            }
            Ok.Enabled = (validTicketID && ProjectIDFld.Text != string.Empty);
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
                    SelectedTag.Text = string.Empty;
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
                this._featureTag = ((LibGit2Sharp.Tag)node.Tag).FriendlyName;
                SelectedTag.Text = node.Parent.Text + "/" + node.Text;
            }
        }

        /// <summary>
        /// Performs a last check to see whether the user has entered a new version number without explicitly leaving the field...
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        private void Ok_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(NewVersionFld.Text))
            {
                string[] newVersion = NewVersionFld.Text.Split('.');
                if (newVersion.Length == 2)
                {
                    int major, minor;
                    if (int.TryParse(newVersion[0], out major) && int.TryParse(newVersion[1], out minor))
                    {
                        var version = new Tuple<int, int>(major, minor);
                        if (this._newVersion == null || this._newVersion.Item1 != version.Item1 || this._newVersion.Item2 != version.Item2)
                        {
                            this._newVersion = version;
                            this._featureTag = null;
                            SelectedTag.Text = string.Empty;
                        }
                    }
                }
            }
        }
    }
}
