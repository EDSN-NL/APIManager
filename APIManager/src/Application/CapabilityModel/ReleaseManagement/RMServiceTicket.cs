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
    /// Release Management - Service Ticket is used to create and manage service-based "UML Tickets" for our Capability Model.
    /// Service Tickets are administered in a package underneath the associated Service Declaration package.
    /// </summary>
    sealed internal class RMServiceTicket: RMTicket
    {
        internal enum Status   { Unknown, Open, Closed }
        internal enum TypeCode { Unknown, Ticket, RootTicket, SubordinateTicket }

        // Private configuration properties used by this service...
        private const string _RMTicketStereotype            = "RMTicketStereotype";
        private const string _RMTimelineParentRole          = "RMTimelineParentRole";
        private const string _RMTimelineChildRole           = "RMTimelineChildRole";
        private const string _RMTicketRole                  = "RMTicketRole";
        private const string _RMRootTicketRole              = "RMRootTicketRole";
        private const string _RMServiceRole                 = "RMServiceRole";
        private const string _RMAssociationStereotype       = "RMAssociationStereotype";
        private const string _RMTypeCodeTag                 = "RMTypeCodeTag";
        private const string _RMExternalPriorityTag         = "RMExternalPriorityTag";
        private const string _RMExternalStatusCodeTag       = "RMExternalStatusCodeTag";
        private const string _RMReleasedVersionTag          = "RMReleasedVersionTag";
        private const string _RMCreationTimestampTag        = "RMCreationTimestampTag";
        private const string _RMModificationTimestampTag    = "RMModificationTimestampTag";

        private Service _trackedService;                    // The service associated with this RMTicket instance.
        private Status _status;                             // Current status of my ticket.
        private TypeCode _typeCode;                         // Ticket type.

        /// <summary>
        /// Returns the service associated with the ticket.
        /// </summary>
        internal Service TrackedService { get { return this._trackedService; } }

        /// <summary>
        /// Get the ticket status (either open or closed).
        /// </summary>
        internal Status TicketStatus { get { return this._status; } }

        /// <summary>
        /// Read the type of our ticket.
        /// </summary>
        internal TypeCode TicketTypeCode { get { return this._typeCode; } }

        /// <summary>
        /// Constructor that loads a Service Ticket based on an existing UML Ticket class and its Tracked Service.
        /// </summary>
        /// <param name="ticketClass">Class that represents the ticket.</param>
        /// <param name="trackedService">the Service for which we are administering the ticket.</param>
        /// <exception cref="ArgumentException">Is thrown when the provided class is not a Service Ticket.</exception>
        internal RMServiceTicket(MEClass ticketClass, Service trackedService): base(ticketClass)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.RMServiceTicket >> Retrieving existing instance '" + ticketClass.Name + 
                             "' for Service '" + trackedService.Name + "...");

            ContextSlt context = ContextSlt.GetContextSlt();
            if (!ticketClass.HasStereotype(context.GetConfigProperty(_RMTicketStereotype)))
            {
                string message = "Plugin.Application.CapabilityModel.RMServiceTicket >> Provided class '" + ticketClass.Name + "' is not a Service Ticket!";
                Logger.WriteError(message);
                throw new ArgumentException(message);
            }
            this._trackedService = trackedService;
            this._status = Ticket.Closed ? Status.Closed : Status.Open;
            this._typeCode = EnumConversions<TypeCode>.StringToEnum(ticketClass.GetTag(context.GetConfigProperty(_RMTypeCodeTag)));

            // Check whether we can find the provided Service...
            string serviceRole = context.GetConfigProperty(_RMServiceRole);
            bool svcFound = false;
            foreach (MEAssociation assoc in ticketClass.AssociationList)
            {
                if (assoc.Destination.Role == serviceRole && assoc.Destination.EndPoint.ElementID == trackedService.ServiceClass.ElementID)
                {
                    svcFound = true;
                    break;
                }
            }
            if (!svcFound)
            {
                string message = "Plugin.Application.CapabilityModel.RMServiceTicket >> Provided Service Ticket '" + ticketClass.Name +
                                 "does not have an association with Service '" + trackedService.Name + "'!";
                Logger.WriteError(message);
                throw new ArgumentException(message);
            }
        }

        /// <summary>
        /// The constructor checks whether a ticket with the specified ID already exists for the specified service. If not, it is created as a new
        /// ticket instance. If the ticket is already there, we simply collect the meta data.
        /// The constructor also creates the necessary package and diagrams when these do not yet exist.
        /// </summary>
        /// <param name="ticketID">Identifier of the ticket we want to create/connect to.</param>
        /// <param name="projectOrderID=">Project Order Identifier associated with the ticket.</param>
        /// <param name="trackedService">Service associated with the ticket.</param>
        /// <exception cref="ArgumentException">Is thrown when the specified ticketID does not yield a valid ticket or when no valid PO number has been specified.</exception>
        internal RMServiceTicket(string ticketID, string projectOrderID, Service trackedService): base(ticketID, projectOrderID)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.RMServiceTicket >> Creating instance for Ticket '" + ticketID +
                             "' and Service '" + trackedService.Name + "'...");
            ContextSlt context = ContextSlt.GetContextSlt();
            this._trackedService = trackedService;

            // This will either receive an existing ticket of force creation of a new one...
            LoadTicketClass(trackedService.DeclarationPkg, ContextSlt.GetContextSlt().GetConfigProperty(_RMTicketStereotype));
            if (IsExistingTicket)
            {
                // In case of loading an existing ticket, make sure to read our in-memory properties...
                this._status = Ticket.Closed ? Status.Closed : Status.Open;
                this._typeCode = EnumConversions<TypeCode>.StringToEnum(this.TicketClass.GetTag(context.GetConfigProperty(_RMTypeCodeTag)));
            }
        }

        /// <summary>
        /// The constructor receives a remote Ticket as an argument and checks whether the ticket already exists for the specified service. 
        /// If not, it is created as a new ticket instance. If the ticket is already there, we simply collect the meta data.
        /// The constructor also creates the necessary package and diagrams when these do not yet exist.
        /// </summary>
        /// <param name="remoteTicket">Ticket instance from remote ticket server.</param>
        /// <param name="projectOrderID=">Project Order Identifier associated with the ticket.</param>
        /// <param name="trackedService">Service associated with the ticket.</param>
        /// <exception cref="ArgumentException">Is thrown when the specified ticketID does not yield a valid ticket or when no valid PO number has been specified.</exception>
        internal RMServiceTicket(Ticket remoteTicket, string projectOrderID, Service trackedService) : base(remoteTicket, projectOrderID)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.RMServiceTicket >> Creating instance for Ticket '" + remoteTicket.ID +
                             "' and Service '" + trackedService.Name + "'...");
            ContextSlt context = ContextSlt.GetContextSlt();
            this._trackedService = trackedService;

            // This will either receive an existing ticket of force creation of a new one...
            LoadTicketClass(trackedService.DeclarationPkg, ContextSlt.GetContextSlt().GetConfigProperty(_RMTicketStereotype));
            if (IsExistingTicket)
            {
                // In case of loading an existing ticket, make sure to read our in-memory properties...
                this._status = Ticket.Closed ? Status.Closed : Status.Open;
                this._typeCode = EnumConversions<TypeCode>.StringToEnum(this.TicketClass.GetTag(context.GetConfigProperty(_RMTypeCodeTag)));
            }
        }

        /// <summary>
        /// Returns the qualified Ticket Identifier, which is a combination of ticket-project name and ticket ID, formatted as:
        /// [project-name]/[Ticket-ID]. Example: "CSTI Integration/CSTI-2345"
        /// </summary>
        /// <returns>Service Ticket identifier.</returns>
        internal override string GetQualifiedID()
        {
            return ProjectName + "/" + ID;
        }

        /// <summary>
        /// Creates a new instance of an UML Ticket Class in the specified package and diagram. We don't need to load all tags since this
        /// will be coordinated by te base class.
        /// </summary>
        /// <param name="ticketPackage">Package that should contain the new ticket.</param>
        /// <param name="ticketDiagram">Diagram that should show the new ticket.</param>
        protected override MEClass CreateNewTicketClass(MEPackage ticketPackage, Diagram ticketDiagram)
        {
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

            // Before creating the new ticket, search for a possible parent ticket (if we do this after the create, we will always
            // find our new ticket to be the last one added, which of course is not what we want)
            MEClass myParent = GetTimelineTail(ticketPackage);

            // Create the ticket...
            MEClass ticketClass = ticketPackage.CreateClass(GetQualifiedID(), ticketClassStereotype);
            diagramClassList.Add(ticketClass);
            this._status = Ticket.Closed ? Status.Closed : Status.Open;

            // Now, check whether we found a parent to connect to...
            if (myParent != null)
            {
                // Parent exists, we created an 'ordinary' ticket, which we're going to link to this parent.
                // We create two associations, one from parent to child and one in the other direction. This way, we can
                // navigate either way (framework code does not support bidirectional associations at the moment).
                this._typeCode = TypeCode.Ticket;
                var parentSource = new EndpointDescriptor(myParent, "1", timelineParentRole, null, false);
                var parentDestination = new EndpointDescriptor(myParent, "1", timelineParentRole, null, true);
                var childSource = new EndpointDescriptor(ticketClass, "1", timelineChildRole, null, false);
                var childDestination = new EndpointDescriptor(ticketClass, "1", timelineChildRole, null, true);
                var parentAssoc = model.CreateAssociation(parentSource, childDestination, MEAssociation.AssociationType.Association);
                var clientAssoc = model.CreateAssociation(childSource, parentDestination, MEAssociation.AssociationType.Association);
                parentAssoc.AddStereotype(assocStereotype, MEAssociation.AssociationEnd.Association);
                clientAssoc.AddStereotype(assocStereotype, MEAssociation.AssociationEnd.Association);
                diagramAssocList.Add(parentAssoc);
                diagramAssocList.Add(clientAssoc);
            }
            else this._typeCode = TypeCode.RootTicket;

            // Now that we know the type of ticket, we can create an association with our Service. Again, we create two associations
            // so that we can navigate from Ticket to Service or the other way around.
            var serviceSource = new EndpointDescriptor(this._trackedService.ServiceClass, "1", serviceRole, null, false);
            var serviceDestination = new EndpointDescriptor(this._trackedService.ServiceClass, "1", serviceRole, null, true);
            var ticketSource = new EndpointDescriptor(ticketClass, "1", this._typeCode == TypeCode.RootTicket? rootTicketRole: ticketRole, null, false);
            var ticketDestination = new EndpointDescriptor(ticketClass, "1", this._typeCode == TypeCode.RootTicket ? rootTicketRole : ticketRole, null, true);
            var serviceAssoc = model.CreateAssociation(serviceSource, ticketDestination, MEAssociation.AssociationType.Association);
            var ticketAssoc = model.CreateAssociation(ticketSource, serviceDestination, MEAssociation.AssociationType.Association);
            serviceAssoc.AddStereotype(assocStereotype, MEAssociation.AssociationEnd.Association);
            ticketAssoc.AddStereotype(assocStereotype, MEAssociation.AssociationEnd.Association);
            diagramAssocList.Add(serviceAssoc);
            diagramAssocList.Add(ticketAssoc);

            // Update the diagram...
            ticketDiagram.AddClassList(diagramClassList);
            ticketDiagram.AddAssociationList(diagramAssocList);
            ticketDiagram.Redraw();
            return ticketClass;
        }

        /// <summary>
        /// Updates the model with the current 'release version' of our tracked service. Typically, this is called from a Release Ticket constructor at the moment 
        /// that the release is actually registered.
        /// </summary>
        internal void UpdateReleasedVersion()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.RMServiceTicket.UpdateReleasedVersion >> Syncing version...");
            ContextSlt context = ContextSlt.GetContextSlt();
            string version = this._trackedService.Version.Item1 + "." +
                             this._trackedService.Version.Item2 + "." +
                             this._trackedService.BuildNumber;
            TicketClass.SetTag(context.GetConfigProperty(_RMReleasedVersionTag), version, MEClass.CreateNewTag);
        }

        /// <summary>
        /// Copies relevant attributes from the (Jira) ticket to our UML ticket class.
        /// </summary>
        internal override void UpdateTicket()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.RMServiceTicket.UpdateTicket >> Syncing ticket data...");
            ContextSlt context = ContextSlt.GetContextSlt();
            base.UpdateTicket();   // First of all, we copy the generic properties.
            string version = this._trackedService.Version.Item1 + "." + 
                             this._trackedService.Version.Item2 + "." + 
                             this._trackedService.BuildNumber;

            TicketClass.SetTag(context.GetConfigProperty(_RMExternalPriorityTag), Ticket.Priority);
            TicketClass.SetTag(context.GetConfigProperty(_RMExternalStatusCodeTag), Ticket.Status);
            TicketClass.SetTag(context.GetConfigProperty(_RMTypeCodeTag), EnumConversions<TypeCode>.EnumToString(this._typeCode), MEClass.CreateNewTag);
            TicketClass.SetTag(context.GetConfigProperty(_RMReleasedVersionTag), version, MEClass.CreateNewTag);
            TicketClass.SetTag(context.GetConfigProperty(_RMCreationTimestampTag), Ticket.CreationTimestamp.ToString());
            TicketClass.SetTag(context.GetConfigProperty(_RMModificationTimestampTag), Ticket.UpdateTimestamp.ToString());
        }

        /// <summary>
        /// Find the end of the ticket timeline, which must be the only ticket in our ticket package that does not have a child 
        /// relationship (e.g. the end of the chain).
        /// To find this, we iterate through all tickets in the package and check outgoing timeline associations.
        /// </summary>
        /// <returns>End of the timeline or NULL when nothing there yet.</returns>
        private MEClass GetTimelineTail(MEPackage ticketPackage)
        {
            MEClass tail = null;
            ContextSlt context = ContextSlt.GetContextSlt();
            string ticketClassStereotype = context.GetConfigProperty(_RMTicketStereotype);
            string parentRole = context.GetConfigProperty(_RMTimelineParentRole);

            foreach (MEClass ticket in ticketPackage.GetClasses(ticketClassStereotype))
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.RMServiceTicket.GetTimelineTail >> Inspecting Ticket '" + ticket.Name + "'...");
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
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.RMServiceTicket.GetTimelineTail >> Found tail '" + ticket.Name + "'...");
                    break;
                }
            }
            return tail;
        }
    }
}
