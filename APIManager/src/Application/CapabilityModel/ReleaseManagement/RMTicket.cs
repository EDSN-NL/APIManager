using System;
using Framework.View;
using Framework.Context;
using Framework.Logging;
using Framework.Model;
using Framework.ConfigurationManagement;

namespace Plugin.Application.CapabilityModel
{
    /// <summary>
    /// Release Management - Ticket is an abstract base class used to manage either service- or release tickets with their UML counterparts.
    /// The class is used to manage packages and diagrams containing UML representations of various specialized Ticket variants.
    /// </summary>
    abstract internal class RMTicket: IEquatable<RMTicket>
    {
        // Private configuration properties used by this service...
        private const string _RMPackageName                 = "RMPackageName";
        private const string _RMPackageStereotype           = "RMPackageStereotype";
        private const string _RMTicketStereotype            = "RMTicketStereotype";
        private const string _RMReleasedVersionTag          = "RMReleasedVersionTag";
        private const string _ReleaseHistoryPos             = "ReleaseHistoryPos";
        private const string _RMProjectOrderIDTag           = "RMProjectOrderIDTag";
        private const string _RMTicketIDTag                 = "RMTicketIDTag";

        private Ticket _ticket;                             // Associated low-level (Jira) Ticket.
        private MEPackage _ticketPackage;                   // The package in which our ticket(s) live.
        private MEClass _ticketClass;                       // Model element representing the ticket.
        private string _projectOrderID;                     // Project order number associated with the ticket.
        private Diagram _myDiagram;                         // Diagram containing our ticket classes.
        private bool _isInitialized;                        // Set to true after full initialization.
        private bool _isExistingTicket;                     // Set to true if an UML ticket already exists.

        /// <summary>
        /// Returns the qualified Ticket Identifier, the format of which depends on the actual ticket implementation (service- and
        /// release tickets have different qualified ID formats).
        /// </summary>
        internal abstract string GetQualifiedID();

        /// <summary>
        /// Returns 'true' in case the ticket is closed in the remote ticket server, 'false' otherwise.
        /// </summary>
        internal bool Closed { get { return this._ticket.Closed; } }

        /// <summary>
        /// Returns the unique (Jira) Ticket identifier.
        /// </summary>
        internal string ID { get { return this._ticket.ID; } }

        /// <summary>
        /// Returns true when the UML ticket is an existing ticket.
        /// </summary>
        internal bool IsExistingTicket { get { return this._isExistingTicket; } }

        /// <summary>
        /// Returns the (Jira) project name associated with the ticket or an empty string in case no valid ticket exists.
        /// </summary>
        internal string ProjectName { get { return this._ticket != null ? this._ticket.ProjectName : string.Empty; } }

        /// <summary>
        /// Returns the Order ID associated with the ticket.
        /// </summary>
        internal string ProjectOrderID { get { return this._projectOrderID; } }

        /// <summary>
        /// Returns the low-level (Jira) Ticket
        /// </summary>
        internal Ticket Ticket { get { return this._ticket; } }

        /// <summary>
        /// Returns the UML Ticket Class representing this RMTicket.
        /// </summary>
        internal MEClass TicketClass { get { return this._ticketClass; } }

        /// <summary>
        /// This function is called from within the 'CreateTicketClass' method in case we could not find the specified UML ticket class.
        /// The function must create the appropriate UML Ticket class within the specified package and show on the specified diagram.
        /// </summary>
        protected abstract MEClass CreateNewTicketClass(MEPackage ticketPackage, Diagram ticketDiagram);

        /// <summary>
        /// Returns the package in which the UML Ticket Class is defined
        /// </summary>
        protected MEPackage TicketPackage { get { return this._ticketPackage; } }

        /// <summary>
        /// Returns the diagram showing the new ticket.
        /// </summary>
        protected Diagram TicketDiagram { get { return this._myDiagram; } }

        /// <summary>
        /// Returns 'true' when this Ticket object is associated with a valid remote (Jira) ticket.
        /// </summary>
        internal bool Valid { get { return this._ticket != null && this._isInitialized; } }

        /// <summary>
        /// Constructor that creates an RMTicket class based on an existing UML Ticket class.
        /// </summary>
        /// <param name="ticketClass">Class that represents the ticket.</param>
        internal RMTicket(MEClass ticketClass)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.RMTicket >> Retrieving existing instance '" + ticketClass.Name + "'...");

            ContextSlt context = ContextSlt.GetContextSlt();
            this._projectOrderID = ticketClass.GetTag(context.GetConfigProperty(_RMProjectOrderIDTag));
            this._ticketClass = ticketClass;
            this._ticketPackage = ticketClass.OwningPackage;
            this._ticket = TicketServerSlt.GetTicketServerSlt().GetTicket(ticketClass.GetTag(context.GetConfigProperty(_RMTicketIDTag)));
            this._isInitialized = true;
            this._isExistingTicket = true;
        }

        /// <summary>
        /// The constructor creates a basic object containing the Jira ticket. Subsequent calls are required to actually create the
        /// ticket administration within the model repository (or read it in case of existing tickets). We left this out of the 
        /// constructor since context depends on the ticket implementation classes and these might require additional work until 
        /// they can actually initiate package/class retrieval or creation.
        /// </summary>
        /// <param name="ticketID">Identifier of the ticket we want to create/connect to.</param>
        /// <param name="projectOrderID=">Project Order Identifier associated with the ticket.</param>
        /// <exception cref="ArgumentException">Is thrown when the specified ticketID does not yield a valid ticket or when no valid PO number has been specified.</exception>
        internal RMTicket(string ticketID, string projectOrderID)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.RMTicket >> Creating new instance for Ticket '" + ticketID +
                             "' and project '" + projectOrderID + "'...");

            this._isInitialized = false;
            this._isExistingTicket = false;
            this._projectOrderID = projectOrderID;
            this._ticket = TicketServerSlt.GetTicketServerSlt().GetTicket(ticketID);
            this._isExistingTicket = this._ticket != null;

            // We don't know these yet and we depend on the specialized class to help us initializing them...
            this._ticketClass = null;
            this._ticketPackage = null;
            this._myDiagram = null;

            if (!this._isExistingTicket)
            {
                string message = "Plugin.Application.CapabilityModel.RMTicket >> Ticket '" + ticketID + "' does not exist!";
                Logger.WriteError(message);
                throw new ArgumentException(message);
            }

            if (string.IsNullOrEmpty(projectOrderID))
            {
                string message = "Plugin.Application.CapabilityModel.RMTicket >> No valid PO ID for Ticket '" + ticketID + "'!";
                Logger.WriteError(message);
                throw new ArgumentException(message);
            }
        }

        /// <summary>
        /// The constructor creates a basic object containing the Jira ticket. Subsequent calls are required to actually create the
        /// ticket administration within the model repository (or read it in case of existing tickets). We left this out of the 
        /// constructor since context depends on the ticket implementation classes and these might require additional work until 
        /// they can actually initiate package/class retrieval or creation.
        /// </summary>
        /// <param name="remoteTicket">The associated Jira Ticket.</param>
        /// <param name="projectOrderID=">Project Order Identifier associated with the ticket.</param>
        /// <exception cref="ArgumentException">Is thrown when the specified ticketID does not yield a valid ticket or when no valid PO number has been specified.</exception>
        internal RMTicket(Ticket remoteTicket, string projectOrderID)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.RMTicket >> Creating new instance for Ticket '" + remoteTicket.ID +
                             "' and project '" + projectOrderID + "'...");

            this._isInitialized = false;
            this._isExistingTicket = false;
            this._projectOrderID = projectOrderID;
            this._ticket = remoteTicket;
            this._isExistingTicket = true;

            // We don't know these yet and we depend on the specialized class to help us initializing them...
            this._ticketClass = null;
            this._ticketPackage = null;
            this._myDiagram = null;

            if (string.IsNullOrEmpty(projectOrderID))
            {
                string message = "Plugin.Application.CapabilityModel.RMTicket >> No valid PO ID for Ticket '" + remoteTicket.ID + "'!";
                Logger.WriteError(message);
                throw new ArgumentException(message);
            }
        }

        /// <summary>
        /// Override method that compares an RMTicket with another Object. Returns true if both elements are of 
        /// identical type and have identical ID's.
        /// </summary>
        /// <param name="obj">The thing to compare against.</param>
        /// <returns>True if same object, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var objticket = obj as RMTicket;
            return (objticket != null) && Equals(objticket);
        }

        /// <summary>
        /// Compares one RMTicket with another, returning true if the two elements have the same qualified ID.
        /// </summary>
        /// <param name="other">Ticket to compare against.</param>
        /// <returns>TRUE if same object, false otherwise.</returns>
        public bool Equals(RMTicket other)
        {
            return (other != null) ? this.GetQualifiedID() == other.GetQualifiedID() : false;
        }

        /// <summary>
        /// Determine a hash of the RMTicket, which is the hash of the qualified ID.
        /// </summary>
        /// <returns>Hash of Ticket Qualified ID</returns>
        public override int GetHashCode()
        {
            return GetQualifiedID().GetHashCode();
        }

        /// <summary>
        /// Override of compare operator. Two RMTicket objects are equal if they have identical qualified ID's.
        /// </summary>
        /// <param name="elementa">First Ticket to compare.</param>
        /// <param name="elementb">Second Ticket to compare.</param>
        /// <returns>True if both tickets have identical qualified ID's</returns>
        public static bool operator ==(RMTicket elementa, RMTicket elementb)
        {
            // Tricky to implement correctly. These first statements make sure that we check whether we are actually
            // dealing with identical objects and/or whether one or both are NULL.
            if (ReferenceEquals(elementa, elementb)) return true;
            if (ReferenceEquals(elementa, null)) return false;
            if (ReferenceEquals(elementb, null)) return false;

            // We have two different interface instances, now check whether they share the same identifiers....
            return elementa.Equals(elementb);
        }

        /// <summary>
        /// Override of compare operator. Two RMTicket objects are different if they have different qualified ID's.
        /// </summary>
        /// <param name="elementa">First Ticket to compare.</param>
        /// <param name="elementb">Second Ticket to compare.</param>
        /// <returns>True if both tickets have different qualified ID's</returns>
        public static bool operator !=(RMTicket elementa, RMTicket elementb)
        {
            return !(elementa == elementb);
        }

        /// <summary>
        /// Returns a string representation of the ticket.
        /// </summary>
        /// <returns>Human friendly, formatted ticket information.</returns>
        public override string ToString()
        {
            return "Ticket '" + this._ticket.ProjectName + "/" + this._ticket.ID + "', PO number '" + this._projectOrderID + "' summary info:" + Environment.NewLine +
                   "Type: '" + this._ticket.Type + "', Status: '" + this._ticket.Status + "', Priority: '" + this._ticket.Priority + "'." + Environment.NewLine +
                   "Created at: '" + this._ticket.CreationTimestamp.ToLongDateString() + "', Updated at: '" + this._ticket.UpdateTimestamp.ToLongDateString() + "'." + Environment.NewLine +
                   "Assigned to: '" + this._ticket.Assignee + "'" + Environment.NewLine +
                   "Summary text: " + Environment.NewLine + this._ticket.Summary;
        }

        /// <summary>
        /// Checks whether the specified ID identifies a valid ticket. Valid tickets exist at the server and have a status of 'open'.
        /// </summary>
        /// <param name="ticketID">Ticket ID to validate.</param>
        /// <returns>True in case of valid ID, false otherwise.</returns>
        internal static bool IsValidID(string ticketID)
        {
            return TicketServerSlt.GetTicketServerSlt().GetTicket(ticketID) != null;
        }

        /// <summary>
        /// Copies relevant attributes from the external ticket to our local ticket class. This method only stores the generic properties
        /// that are available to all Ticket types. Base classes must explicitly invoke the method in case of override.
        /// </summary>
        internal virtual void UpdateTicket()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.RMTicket.UpdateTicket >> Storing ticket data...");
            ContextSlt context = ContextSlt.GetContextSlt();

            TicketClass.SetTag(context.GetConfigProperty(_RMTicketIDTag), this._ticket.ID);
            TicketClass.SetTag(context.GetConfigProperty(_RMProjectOrderIDTag), this._projectOrderID);
        }

        /// <summary>
        /// Follow-up initialization call used to actually read the appropriate UML ticket classes, package and diagrams.
        /// Must be invoked by derived specializations before the ticket is declared valid.
        /// If we can't find the ticket, the call is bounced back to the specialized class by invoking CreateNewTicketClass, so the
        /// specialized class better invoke LoadTicketClass at the appropriate moment.
        /// </summary>
        /// <param name="parentPackage">Existing package that is parent of the ticket package.</param>
        /// <param name="ticketStereotype">Stereotype to be used for the newly created UML Ticket class.</param>
        /// <exception cref="ArgumentException">Is thrown in case we are not able to create a valid ticket environment.</exception>
        protected void LoadTicketClass(MEPackage parentPackage, string ticketStereotype)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string ticketPackageStereotype = context.GetConfigProperty(_RMPackageStereotype);
            string ticketPackageName = context.GetConfigProperty(_RMPackageName);

            // Check whether we have a Release Management package for this ticket and if not, create one...
            this._ticketPackage = parentPackage.FindPackage(ticketPackageName, ticketPackageStereotype);
            if (this._ticketPackage == null)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.RMTicket.CreateTicketClass >> No environment yet for Release Management, creating one...");
                int packagePos;  // We try to read the relative package position from configuration and if this failed, use '10' as default value.
                if (!int.TryParse(context.GetConfigProperty(_ReleaseHistoryPos), out packagePos)) packagePos = 10;
                this._ticketPackage = parentPackage.CreatePackage(ticketPackageName, ticketPackageStereotype, packagePos);
            }

            // Retrieve our diagram (showing all tickets) and if not found, create one...
            this._myDiagram = this._ticketPackage.FindDiagram(this._ticketPackage.Name);
            if (this._myDiagram == null)
            {
                // When the package does not have a diagram yet, create one...
                this._myDiagram = this._ticketPackage.CreateDiagram();
                this._myDiagram.AddDiagramProperties();
                this._myDiagram.ShowConnectorStereotypes(false);
                this._myDiagram.Show();
            }

            // Now that we have a package, try to find the ticket...
            this._ticketClass = this._ticketPackage.FindClass(GetQualifiedID(), ticketStereotype);
            if (this._ticketClass != null)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.RMTicket >> Existing ticket, get some metadata...");
                this._isExistingTicket = true;
            }
            else
            {
                // New ticket, ask specialized class to create the appropriate UML Ticket Class.
                this._isExistingTicket = false;
                this._ticketClass = CreateNewTicketClass(this._ticketPackage, this._myDiagram);
            }
            UpdateTicket(); // Assure that ticket metadata is in sync with the remote ticket.
            this._isInitialized = true;
        }
    }
}
