using System;
using Atlassian.Jira;
using Framework.Logging;

namespace Framework.ConfigurationManagement
{
    /// <summary>
    /// Represents a ticket as registered in an external ticket server (Jira).
    /// </summary>
    sealed internal class Ticket
    {
        private const string _NoTicket = "Empty";   // Used as default contents when RM is not active.

        private string _ticketID;           // Ticket key.
        private string _projectID;          // Identifier of project containing the ticket.
        private string _projectName;        // Name of project containing the ticket.
        private string _type;               // Type of ticket, some are predefined but one can define additional types.
        private string _status;             // Whether the ticket is open, on-hold, in progress, etc.
        private string _priority;           // Ticket priority.
        private string _summary;            // Short descriptive text.
        private string _assignee;           // The person who is assigned to this ticket.
        private DateTime _created;          // Date and time of creation.
        private DateTime _updated;          // Date and time of most recent update.

        /// <summary>
        /// Getters for all properties.
        /// </summary>
        internal string ID                      { get { return this._ticketID; } }
        internal string ProjectID               { get { return this._projectID; } }
        internal string ProjectName             { get { return this._projectName; } }
        internal string Type                    { get { return this._type; } }
        internal string Status                  { get { return this._status; } }
        internal string Priority                { get { return this._priority; } }
        internal string Summary                 { get { return this._summary; } }
        internal string Assignee                { get { return this._assignee; } }
        internal DateTime CreationTimestamp     { get { return this._created; } }
        internal DateTime UpdateTimestamp       { get { return this._updated; } }

        /// <summary>
        /// Returns 'true' when the ticket is marked 'closed' in the remote ticket server, 'false' otherwise.
        /// </summary>
        internal bool Closed
        {
            get { return TicketServerSlt.GetTicketServerSlt().IsClosed(this._ticketID); }
        }

        /// <summary>
        /// Returns a string representation of the ticket.
        /// </summary>
        /// <returns>Formatted ticket information.</returns>
        public override string ToString()
        {
            return "Ticket '" + this._projectName + "/" + this._ticketID + "' summary info:" + Environment.NewLine +
                   "Type: '" + this._type + "', Status: '" + this._status + "', Priority: '" + this._priority + "'." + Environment.NewLine +
                   "Created at: '" + this._created.ToLongDateString() + "', Updated at: '" + this._updated.ToLongDateString() + "'." + Environment.NewLine +
                   "Assigned to: '" + this._assignee + "'" + Environment.NewLine +
                   "Summary text: " + Environment.NewLine + this._summary;
        }

        /// <summary>
        /// Constructor that creates an 'APIManager' ticket from a 'Jira' issue.
        /// </summary>
        /// <param name="projectName">The name of the project that 'owns' the issue.</param>
        /// <param name="issue">Jira issue to be used for creation.</param>
        internal Ticket(string projectName, Issue issue)
        {
            this._ticketID = issue.Key.ToString();
            this._projectID = issue.Project;
            this._projectName = projectName;
            this._type = issue.Type.Name;
            this._status = issue.Status.Name;
            this._priority = issue.Priority.Name;
            this._summary = issue.Summary;
            this._assignee = issue.Assignee;
            this._created = (DateTime)issue.Created;
            this._updated = (DateTime)issue.Updated;
        }

        /// <summary>
        /// The default constructor is used when RM is not active. We create a dummy ticket in this case.
        /// When we attempt to use the default constructor while RM is active, the constructor throws an InvalidOperationException!
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the default constructor is used while RM is enabled.</exception>
        internal Ticket()
        {
            RepositoryDescriptor descriptor = CMRepositoryDscManagerSlt.GetRepositoryDscManagerSlt().GetCurrentDescriptor();
            if (descriptor == null || !descriptor.IsRMEnabled)
            {
                this._ticketID = _NoTicket;
                this._projectID = _NoTicket;
                this._projectName = _NoTicket;
                this._type = _NoTicket;
                this._status = _NoTicket;
                this._priority = _NoTicket;
                this._summary = _NoTicket;
                this._assignee = _NoTicket;
                this._created = DateTime.Now;
                this._updated = DateTime.Now;
            }
            else
            {
                string msg = "Attempt to create a dummy Ticket while Release Management is enabled!";
                Logger.WriteError("Framework.ConfigurationManagement.Ticket >> " + msg);
                throw new InvalidOperationException(msg);
            }
        }
    }
}
