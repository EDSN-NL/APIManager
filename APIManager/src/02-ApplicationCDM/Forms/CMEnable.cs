using System;
using System.Windows.Forms;
using Framework.ConfigurationManagement;
using Plugin.Application.CapabilityModel;

namespace Plugin.Application.Forms
{
    internal partial class CMEnable : Form
    {
        private Tuple<int, int> _newVersion;
        private Ticket _remoteTicket;
        private bool _hasRM;                // True when Release Management has been enabled.

        /// <summary>
        /// Getters for the various dialog properties...
        /// </summary>
        internal Ticket RemoteTicket        { get { return this._remoteTicket; } }
        internal string ProjectOrderID      { get { return ProjectIDFld.Text; } }
        internal Tuple<int, int> NewVersion { get { return this._newVersion; } }

        /// <summary>
        /// Enable Configuration Management dialog, user must enter a ticket-id and project number to get started.
        /// </summary>
        internal CMEnable(Service service)
        {
            InitializeComponent();
            this._newVersion = new Tuple<int, int>(service.Version.Item1, service.Version.Item2);
            ExistingVersionFld.Text = service.Version.Item1 + "." + service.Version.Item2;
            NewVersionFld.Text = service.Version.Item1 + "." + (service.Version.Item2);

            this._hasRM = RMTicket.IsRMEnabled();
            if (!this._hasRM)
            {
                // No RM, disable the (part of the) Administration section and pretend we have a valid ticket & projectID
                // (this will enable the Ok button on name only).
                TicketBox.Enabled = false;
                this._remoteTicket = new Ticket();  // Create a dummy ticket.
                Ok.Enabled = true;
            }
            else
            {
                Ok.Enabled = false;
                this._remoteTicket = null;
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
            bool validTicketID = false;
            if (TicketIDFld.Text != string.Empty)
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
                    validTicketID = true;
                }
            }
            Ok.Enabled = (validTicketID && ProjectIDFld.Text != string.Empty);
        }

        /// <summary>
        /// This event is raised when the user changed the contents of the 'new version' field.
        /// We check the entered version, which must be in format major.minor.
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
                    this._newVersion = new Tuple<int, int>(major, minor);
                }
            }
        }
    }
}
