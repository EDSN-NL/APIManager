using System;
using System.Collections.Generic;
using Framework.View;
using Framework.Context;
using Framework.Logging;
using Framework.Model;
using Framework.Util;
using Framework.ConfigurationManagement;

namespace Plugin.Application.CapabilityModel
{
    /// <summary>
    /// Release Management - Ticket is a Service-specific wrapper class around the generic 'Ticket' class. It is used to create and manage
    /// "UML Tickets" for our Capability Model.
    /// </summary>
    sealed internal class RMTicket
    {
        internal enum Status    { Unknown, Open, Closed, MergeRequest }
        internal enum TypeCode  { Unknown, Ticket, RootTicket, SubordinateTicket }

        // Private configuration properties used by this service...
        private const string _RMPackageName                 = "RMPackageName";
        private const string _RMPackageStereotype           = "RMPackageStereotype";
        private const string _RMRootReleasePackagePath      = "RMRootReleasePackagePath";
        private const string _RMTicketStereotype            = "RMTicketStereotype";
        private const string _RMReleaseStereotype           = "RMReleaseStereotype";
        private const string _RMAssociationStereotype       = "RMAssociationStereotype";
        private const string _RMTimelineParentRole          = "RMTimelineParentRole";
        private const string _RMTimelineChildRole           = "RMTimelineChildRole";
        private const string _RMHierarchySubordinateRole    = "RMHierarchySubordinateRole";
        private const string _RMHierarchyMasterRole         = "RMHierarchyMasterRole";
        private const string _RMReleaseRole                 = "RMReleaseRole";
        private const string _RMTicketRole                  = "RMTicketRole";
        private const string _RMRootTicketRole              = "RMRootTicketRole";
        private const string _RMServiceRole                 = "RMServiceRole";
        private const string _ReleaseHistoryPos             = "ReleaseHistoryPos";
        private const string _RMStatusTag                   = "RMStatusTag";
        private const string _RMTypeCodeTag                 = "RMTypeCodeTag";
        private const string _RMCreationTimestampTag        = "RMCreationTimestampTag";
        private const string _RMModificationTimestampTag    = "RMModificationTimestampTag";
        private const string _RMProjectOrderIDTag           = "RMProjectOrderIDTag";
        private const string _RMTicketIDTag                 = "RMTicketIDTag";
        private const string _RMDevelopmentBranchNameTag    = "RMDevelopmentBranchNameTag";
        private const string _RMDevelopmentRepositoryNSTag  = "RMDevelopmentRepositoryNSTag";
        private const string _RMExternalPriorityTag         = "RMExternalPriorityTag";
        private const string _RMExternalStatusCodeTag       = "RMExternalStatusCodeTag";
        private const string _RMReleasedVersionTag          = "RMReleasedVersionTag";

        private Service _trackedService;                    // The service associated with this RMTicket instance.
        private Ticket _ticket;                             // Associated low-level (Jira) Ticket.
        private MEPackage _ticketPackage;                   // The package in which our ticket(s) live.
        private MEClass _ticketClass;                       // Model element representing the ticket.
        private Status _status;                             // Current status of my ticket.
        private TypeCode _typeCode;                         // Ticket type.
        private Diagram _myDiagram;                         // Diagram containing our ticket classes.
        private string _projectOrderID;                     // Enexis project identifier for this ticket.

        /// <summary>
        /// Returns the unique Ticket identifier.
        /// </summary>
        internal string ID { get { return this._ticket.ID; } }

        /// <summary>
        /// Returns the qualified Ticket Identifier, which is a combination of ticket-project name and ticket ID, formatted as:
        /// [project-name]/[Ticket-ID]. Example: "CSTI Integration/CSTI-2345"
        /// </summary>
        internal string QualifiedID { get { return this._ticket.ProjectName + "/" + this._ticket.ID; } }

        /// <summary>
        /// Get- or set the ticket status.
        /// </summary>
        internal Status TicketStatus
        {
            get { return this._status; }
            set { SetStatus(value); }
        }

        /// <summary>
        /// Returns 'true' when this Ticket object is associated with a remote ticket.
        /// </summary>
        internal bool Valid { get { return this._ticket != null; } }

        /// <summary>
        /// The constructor checks whether a ticket with the specified ID already exists for the specified service. If not, it is created as a new
        /// ticket instance. If the ticket is already there, we simply collect the meta data.
        /// The constructor also creates the necessary package and diagrams when these do not yet exist.
        /// </summary>
        /// <param name="ticketID">Identifier of the ticket we want to create/connect to.</param>
        /// <param name="projectOrderID">Project Order Identifier associated with the ticket.</param>
        /// <param name="trackedService">Service associated with the ticket.</param>
        /// <exception cref="ArgumentException">Is thrown when the specified ticketID does not yield a valid ticket or when no valid PO number has been specified.</exception>
        internal RMTicket(string ticketID, string projectOrderID, Service trackedService)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.RMTicket >> Creating instance for Ticket '" + ticketID + 
                             "' and Service '" + trackedService.Name + "'...");

            ContextSlt context = ContextSlt.GetContextSlt();
            this._trackedService = trackedService;
            this._ticket = TicketServerSlt.GetTicketServerSlt().GetTicket(ticketID);
            if (this._ticket == null)
            {
                string message = "Plugin.Application.CapabilityModel.RMTicket >> Ticket '" + ticketID + 
                                 "' for Service '" + trackedService.Name + "' does not exist!";
                Logger.WriteError(message);
                throw new ArgumentException(message);
            }
            this._projectOrderID = projectOrderID;
            if (string.IsNullOrEmpty(projectOrderID))
            {
                string message = "Plugin.Application.CapabilityModel.RMTicket >> No valid PO ID for Service '" + trackedService.Name + "'!";
                Logger.WriteError(message);
                throw new ArgumentException(message);
            }

            // Various configuration items...
            string ticketPackageName = context.GetConfigProperty(_RMPackageName);
            string ticketPackageStereotype = context.GetConfigProperty(_RMPackageStereotype);
            string ticketClassStereotype = context.GetConfigProperty(_RMTicketStereotype);

            // Check whether we have a Release Management package for this service and if not, create one...
            this._ticketPackage = trackedService.DeclarationPkg.FindPackage(ticketPackageName, ticketPackageStereotype);
            if (this._ticketPackage == null)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.RMTicket >> No environment yet for Release Management, creating one...");
                int packagePos;  // We try to read the relative package position from configuration and if this failed, use '10' as default value.
                if (!int.TryParse(context.GetConfigProperty(_ReleaseHistoryPos), out packagePos)) packagePos = 10;
                this._ticketPackage = trackedService.DeclarationPkg.CreatePackage(ticketPackageName, ticketPackageStereotype, packagePos);
            }

            // Retrieve our diagram (showing all service tickets) and if not found, create one...
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
            this._ticketClass = this._ticketPackage.FindClass(QualifiedID, ticketClassStereotype);
            if (this._ticketClass != null)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.RMTicket >> Existing ticket, get some metadata...");
                this._status = EnumConversions<Status>.StringToEnum(this._ticketClass.GetTag(context.GetConfigProperty(_RMStatusTag)));
                this._typeCode = EnumConversions<TypeCode>.StringToEnum(this._ticketClass.GetTag(context.GetConfigProperty(_RMTypeCodeTag)));
                LoadMetadata();             // Assure that ticket metadata is in sync with the remote ticket.
            }
            else CreateTimelineTicket();    // New ticket, create in package and show on diagram.
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
        /// Creates a new Ticket class in the timeline. This is either a root- or an 'ordinary' ticket, depending on the state of the timeline.
        /// </summary>
        private void CreateTimelineTicket()
        {
            // Before creating the new ticket, search the package for the end of the timeline, which will be the anchor for the new ticket...
            MEClass myParent = GetTimelineTail();
            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();

            var diagramAssocList = new List<MEAssociation>();
            var diagramClassList = new List<MEClass>();

            // We need a bunch of configuration items...
            string timelineParentRole = context.GetConfigProperty(_RMTimelineParentRole);
            string timelineChildRole = context.GetConfigProperty(_RMTimelineChildRole);
            string ticketRole = context.GetConfigProperty(_RMTicketRole);
            string rootTicketRole = context.GetConfigProperty(_RMRootTicketRole);
            string serviceRole = context.GetConfigProperty(_RMServiceRole);
            string ticketClassStereotype = context.GetConfigProperty(_RMTicketStereotype);
            string assocStereotype = context.GetConfigProperty(_RMAssociationStereotype);

            // Create the ticket...
            this._ticketClass = this._ticketPackage.CreateClass(QualifiedID, ticketClassStereotype);
            this.TicketStatus = Status.Open;

            // Now, check if we have a parent to connect to...
            if (myParent != null)
            {
                // Parent exists, we created an 'ordinary' ticket, which we're going to link to this parent...
                this._typeCode = TypeCode.Ticket;
                var ticketParent = new EndpointDescriptor(myParent, "1", timelineParentRole, null, true);
                var myTicket = new EndpointDescriptor(this._ticketClass, "1", timelineChildRole, null, true);
                var parentAssoc = model.CreateAssociation(ticketParent, myTicket, MEAssociation.AssociationType.Association);
                parentAssoc.AddStereotype(assocStereotype, MEAssociation.AssociationEnd.Association);
                diagramAssocList.Add(parentAssoc);
            }
            else this._typeCode = TypeCode.RootTicket;
            this._ticketClass.SetTag(context.GetConfigProperty(_RMTypeCodeTag), EnumConversions<TypeCode>.EnumToString(this._typeCode), true);
            diagramClassList.Add(this._ticketClass);

            // Now that we know the type of ticket, we can create an association with our Service...
            var source = new EndpointDescriptor(this._trackedService.ServiceClass, "1", serviceRole, null, true);
            var target = new EndpointDescriptor(this._ticketClass, "1", this._typeCode == TypeCode.RootTicket? rootTicketRole: ticketRole, null, true);
            var serviceAssoc = model.CreateAssociation(source, target, MEAssociation.AssociationType.Association);
            serviceAssoc.AddStereotype(assocStereotype, MEAssociation.AssociationEnd.Association);
            diagramAssocList.Add(serviceAssoc);

            LoadMetadata();     // Initialize the new ticket class with relevant ticket information.

            // Update the diagram...
            this._myDiagram.AddClassList(diagramClassList);
            this._myDiagram.AddAssociationList(diagramAssocList);
            this._myDiagram.Redraw();
        }

        /// <summary>
        /// Find the end of the ticket timeline, which must be the only ticket in our ticket package that does not have a child 
        /// relationship (e.g. the end of the chain).
        /// To find this, we iterate through all tickets in the package and check outgoing timeline associations.
        /// </summary>
        /// <returns>End of the timeline or NULL when nothing there yet.</returns>
        private MEClass GetTimelineTail()
        {
            MEClass tail = null;
            ContextSlt context = ContextSlt.GetContextSlt();
            string ticketClassStereotype = context.GetConfigProperty(_RMTicketStereotype);
            string parentRole = context.GetConfigProperty(_RMTimelineParentRole);

            foreach (MEClass ticket in this._ticketPackage.GetClasses(ticketClassStereotype))
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.RMTicket.GetTimelineTail >> Inspecting Ticket '" + ticket.Name + "'...");
                bool isParent = false;
                foreach (MEAssociation assoc in ticket.AssociationList)
                {
                    if (assoc.Source.Role == parentRole)
                    {
                        isParent = true;
                        break;
                    }
                }

                // There should be exactly ONE ticket without this parent role, so the moment we found it, we're done.
                if (!isParent)
                {
                    tail = ticket;
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.RMTicket.GetTimelineTail >> Found tail '" + ticket.Name + "'...");
                    break;
                }
            }
            return tail;
        }

        /// <summary>
        /// Copies relevant attributes from the external ticket to our local ticket class.
        /// </summary>
        private void LoadMetadata()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.RMTicket.LoadMetadata >> Retrieving ticket data...");
            ContextSlt context = ContextSlt.GetContextSlt();

            this._ticketClass.SetTag(context.GetConfigProperty(_RMCreationTimestampTag), this._ticket.CreationTimestamp.ToString());
            this._ticketClass.SetTag(context.GetConfigProperty(_RMModificationTimestampTag), this._ticket.UpdateTimestamp.ToString());
            this._ticketClass.SetTag(context.GetConfigProperty(_RMTicketIDTag), this._ticket.ID);
            this._ticketClass.SetTag(context.GetConfigProperty(_RMExternalPriorityTag), this._ticket.Priority);
            this._ticketClass.SetTag(context.GetConfigProperty(_RMExternalStatusCodeTag), this._ticket.Status);
            this._ticketClass.SetTag(context.GetConfigProperty(_RMProjectOrderIDTag), this._projectOrderID);
        }

        /// <summary>
        /// Helper function that updates the status of the ticket class.
        /// </summary>
        /// <param name="newStatus">Updated status.</param>
        private void SetStatus(Status newStatus)
        {
            if (this._status != newStatus)
            {
                this._status = newStatus;
                this._ticketClass.SetTag(ContextSlt.GetContextSlt().GetConfigProperty(_RMStatusTag), 
                                         EnumConversions<Status>.EnumToString(this._status), true);
            }
        }
    }
}
