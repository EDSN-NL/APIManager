using System;
using System.IO;
using System.Linq;
using System.Configuration;
using System.Diagnostics;
using System.Collections.Generic;
using Framework.Context;
using Framework.Logging;
using Framework.Util;
using Atlassian.Jira;

namespace Framework.ConfigurationManagement
{
    /// <summary>
    /// Ticket Server is used to provide information regarding (Jira) tickets. These are used as the basis for ECDM 'workflow'.
    /// </summary>
    sealed internal class TicketServerSlt
    {
        private static readonly TicketServerSlt _ticketServerSlt = new TicketServerSlt();     // The actual ticket server instance.

        private Uri _jiraURL;               // The URL used to access Jira.
        private Jira _jira;                 // Our current Jira session.
        private bool _enabled;              // Set to 'true' when properly initialized and CM is active.

        /// <summary>
        /// Public Ticket Server "factory" method. Simply returns the static instance.
        /// </summary>
        /// <returns>Ticket Server singleton object</returns>
        internal static TicketServerSlt GetTicketServerSlt() { return _ticketServerSlt; }

        /// <summary>
        /// Returns TRUE if CM is enabled and server connection has been established.
        /// </summary>
        internal bool IsValidServer { get { return this._enabled; } }

        /// <summary>
        /// Retrieves the ticket with the specified ID from the external ticket server. This MUST be an open ticket!
        /// </summary>
        /// <returns>Retrieved ticket or NULL if ticket not found (or no valid ticket / ticket server).</returns>
        internal Ticket GetTicket(string ticketID)
        {
            if (this._enabled && !string.IsNullOrEmpty(ticketID))
            {
                try
                {
                    Ticket theTicket = null;
                    var issues = from i in this._jira.Issues.Queryable
                                 where i.Key == ticketID && i.Status != "Done"
                                 select i;
                    // There SHOULD be exactly ONE ticket with the specified key. But just in case, we execute the loop and simply
                    // take the first item returned by the query...
                    foreach (Issue issue in issues)
                    {
                        theTicket = new Ticket(GetProjectName(issue.Project), issue);
                        break;
                    }
                    return theTicket;
                }
                catch (Exception exc)
                {
                    if (!exc.InnerException.Message.Contains("404") && !exc.InnerException.Message.Contains("400")) // Ticket not found.
                    {
                        Logger.WriteWarning("Ticket server query failed because: " + Environment.NewLine + exc.InnerException.Message);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Creates a new Jira singleton instance.
        /// </summary>
        private TicketServerSlt()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            RepositoryDescriptor repoDsc = CMRepositoryDscManagerSlt.GetRepositoryDscManagerSlt().GetCurrentDescriptor();
            if (repoDsc == null)
            {
                this._enabled = false;
                string message = "Unable to retrieve proper CM descriptor, aborting!";
                Logger.WriteError("Framework.ConfigurationManagement.JiraSlt >>  " + message);
                throw new ConfigurationErrorsException(message);
            }
            Logger.WriteInfo("Framework.ConfigurationManagement.JiraSlt >> Initializing Jira session for '" + repoDsc.Name + "'...");

            this._enabled = repoDsc.IsCMEnabled;
            this._jiraURL = repoDsc.JiraURL;

            try
            {
                if (this._enabled)
                {
                    Logger.WriteInfo("Framework.ConfigurationManagement.JiraSlt >> Creating Jira session to '" + repoDsc.JiraURL + "'...");
                    this._jira = Jira.CreateRestClient(this._jiraURL.AbsoluteUri, repoDsc.JiraUserName, CryptString.ToPlainString(repoDsc.JiraPassword));
                }
                else
                {
                    Logger.WriteInfo("Framework.ConfigurationManagement.JiraSlt >> Configuration Management is disabled!");
                    this._jira = null;
                    this._jiraURL = null;
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Framework.ConfigurationManagement.JiraSlt >> Failed to initialize repository because: " + exc.Message);
                this._jira = null;
                this._jiraURL = null;
                this._enabled = false;
            }
        }

        /// <summary>
        /// Retrieves the project name associated with the specified project ID.
        /// </summary>
        /// <param name="projectID">Project to be queried.</param>
        /// <returns>Project name or empty string on errors/not found.</returns>
        private string GetProjectName(string projectID)
        {
            if (this._enabled && !string.IsNullOrEmpty(projectID))
            {
                try
                {
                    var project = this._jira.Projects.GetProjectAsync(projectID).Result;
                    return project.Name;
                }
                catch (Exception exc)
                {
                    if (!exc.InnerException.Message.Contains("404") && !exc.InnerException.Message.Contains("400")) // Project not found.
                    {
                        Logger.WriteWarning("Ticket server project lookup failed because: " + Environment.NewLine + exc.InnerException.Message);
                    }
                }
            }
            return string.Empty;
        }
    }
}
