using System;
using System.Collections.Generic;
using Framework.View;
using Framework.Context;
using Framework.Logging;
using Framework.Model;

namespace Plugin.Application.CapabilityModel
{
    /// <summary>
    /// Release Management - ReleaseTicket represents a 'release', e.g. a set of changes for one or multiple services, which are deployed to
    /// production as one set. The class manages the Release History package.
    /// </summary>
    sealed internal class RMReleaseTicket: RMTicket
    {
        // Private configuration properties used by this service...
        private const string _RMReleasePackageParentPath    = "RMReleasePackageParentPath";
        private const string _RMReleasePackageParentName    = "RMReleasePackageParentName";
        private const string _RMReleaseStereotype           = "RMReleaseStereotype";
        private const string _RMTicketStereotype            = "RMTicketStereotype";
        private const string _RMAssociationStereotype       = "RMAssociationStereotype";
        private const string _RMTimelineParentRole          = "RMTimelineParentRole";
        private const string _RMTimelineChildRole           = "RMTimelineChildRole";
        private const string _RMReleaseRole                 = "RMReleaseRole";
        private const string _RMTicketRole                  = "RMTicketRole";
        private const string _RMServiceRole                 = "RMServiceRole";
        private const string _RMReleaseVersionNumberTag     = "RMReleaseVersionNumberTag";
        private const string _RMReleaseIDPrefix             = "RMReleaseIDPrefix";
        private const string _RMCreationTimestampTag        = "RMCreationTimestampTag";
        private const string _RMModificationTimestampTag    = "RMModificationTimestampTag";

        private int _releaseVersion;                        // Multiple releases based on the same ticket increment this number.
        private RMServiceTicket _serviceTicket;             // Associated service ticket for this release.

        /// <summary>
        /// Returns the release version number for this ticket.
        /// </summary>
        internal int ReleaseVersion { get { return this._releaseVersion; } }

        /// <summary>
        /// Returns the release identifier string for this release. Format of this identifier is:
        /// [prefix]/[Ticket-ID].[Release-Version]. Example: "release/CSTI-2345.01"
        /// </summary>
        internal string ReleaseID
        {
            get
            {
                return ContextSlt.GetContextSlt().GetConfigProperty(_RMReleaseIDPrefix) + "/" + ID + "." + this._releaseVersion.ToString("00");
            }
        }

        /// <summary>
        /// Returns the qualified Ticket Identifier, which is a combination of ticket-project name, ticket ID and release version, formatted as:
        /// [project-name]/[Ticket-ID].[Release-Version]. Example: "CSTI Integration/CSTI-2345.01"
        /// The release version is formatted as a two-digit number, including leading zeroes when necessary.
        /// </summary>
        internal override string GetQualifiedID()
        {
            return ProjectName + "/" + ID + "." + this._releaseVersion.ToString("00");
        }

        /// <summary>
        /// Constructor that loads a Release Ticket based on an existing UML Release Ticket class and it's associated Service Ticket.
        /// When Release Management is disabled, the constructor does not perform any actions and the class properties are set to 
        /// suitable default values.
        /// </summary>
        /// <param name="ticketClass">Class that represents the ticket.</param>
        /// <exception cref="ArgumentException">Is thrown when the provided class is not a Service Ticket.</exception>
        internal RMReleaseTicket(MEClass releaseTicketClass, RMServiceTicket serviceTicket) : base(releaseTicketClass)
        {
            if (IsReleaseManagementEnabled)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.RMReleaseTicket >> Retrieving existing instance '" + releaseTicketClass.Name + "'...");

                ContextSlt context = ContextSlt.GetContextSlt();
                if (!releaseTicketClass.HasStereotype(context.GetConfigProperty(_RMReleaseStereotype)))
                {
                    string message = "Plugin.Application.CapabilityModel.RMServiceTicket >> Provided class '" + releaseTicketClass.Name + "' is not a Release Ticket!";
                    Logger.WriteError(message);
                    throw new ArgumentException(message);
                }

                string releaseVersion = releaseTicketClass.GetTag(context.GetConfigProperty(_RMReleaseVersionNumberTag));
                if (!int.TryParse(releaseVersion, out this._releaseVersion))
                {
                    string message = "Plugin.Application.CapabilityModel.RMServiceTicket >> Provided class '" + releaseTicketClass.Name +
                                     "' has an illegal version '" + releaseVersion + "'!";
                    Logger.WriteError(message);
                    throw new ArgumentException(message);
                }

                // Check whether we can find the provided Service Ticket...
                string ticketRole = context.GetConfigProperty(_RMTicketRole);
                bool ticketFound = false;
                foreach (MEAssociation assoc in releaseTicketClass.AssociationList)
                {
                    if (assoc.Destination.Role == ticketRole && assoc.Destination.EndPoint.Name == serviceTicket.GetQualifiedID())
                    {
                        ticketFound = true;
                        break;
                    }
                }
                if (!ticketFound)
                {
                    string message = "Plugin.Application.CapabilityModel.RMServiceTicket >> Provided Release Ticket '" + releaseTicketClass.Name +
                                     "does not have an association with '" + serviceTicket.GetQualifiedID() + "'!";
                    Logger.WriteError(message);
                    throw new ArgumentException(message);
                }
                this._serviceTicket = serviceTicket;

                // We now set the modification date/time of the release ticket to the current date and time and we update the release version in the service ticket...
                this.TicketClass.SetTag(context.GetConfigProperty(_RMModificationTimestampTag), DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                this._serviceTicket.UpdateReleasedVersion();
            }
        }

        /// <summary>
        /// The constructor creates a new release ticket based on the specified 'Service Ticket' and release version.
        /// If a release ticket with the exact same ID (and version) already exists (but associated with another service), we only add the
        /// new Service Ticket. This implies that for each service that is part of a release, we can just create a new RMReleaseTicket
        /// object, which will all be associated with the same UML Release Ticket. When Release Management is disabled, the
        /// constructor does not perform any actions and the class properties are set to suitable default values.
        /// </summary>
        /// <param name="ticketID">Identifier of the ticket we want to create/connect to.</param>
        /// <param name="releaseVersion">Optional release version. When not specified, we search for the latest release and link to that
        /// or, when not found, we create a new release with version 1. When a version is specified, we either locate that exact version
        /// or, when not found, create a new ticket with that version. Proper versions are > 0.</param>
        /// <exception cref="InvalidOperationException">Thrown when the context for the operation is incorrect, i.e. necessary objects could not be found.</exception>
        /// <exception cref="ArgumentException">Is thrown when the specified ticketID does not yield a valid ticket or when no valid PO number has been specified.</exception>
        internal RMReleaseTicket(RMServiceTicket serviceTicket, int releaseVersion = 0): base(serviceTicket.ID, serviceTicket.ProjectOrderID)
        {
            if (!IsReleaseManagementEnabled) return;

            Logger.WriteInfo("Plugin.Application.CapabilityModel.RMReleaseTicket >> Creating release for Ticket '" +
                 serviceTicket.ProjectName + "/" + serviceTicket.ID + "." + releaseVersion + "' for service '" + serviceTicket.TrackedService.Name + "'...");

            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            this._serviceTicket = serviceTicket;
            this._releaseVersion = 0;
            string versionTag = context.GetConfigProperty(_RMReleaseVersionNumberTag);
            string releasePackageParentPath = context.GetConfigProperty(_RMReleasePackageParentPath);
            string releasePackageParentName = context.GetConfigProperty(_RMReleasePackageParentName);
            MEPackage releasePackageParent = model.FindPackage(releasePackageParentPath, releasePackageParentName);
            if (releasePackageParent == null)
            {
                string message = "Plugin.Application.CapabilityModel.RMReleaseTicket >> Could not obtain release package '" + 
                                  releasePackageParentPath + "/" + releasePackageParentName + "'!";
                Logger.WriteError(message);
                throw new InvalidOperationException(message);
            }

            if (releaseVersion == 0)
            {
                // Indicates that we want to link to the highest existing version (when available).
                MEClass thisVersionClass = GetLatestVersion();
                if (thisVersionClass != null)
                {
                    int thisVersion = 0;
                    if (!int.TryParse(thisVersionClass.GetTag(versionTag), out thisVersion))
                    {
                        string message = "Plugin.Application.CapabilityModel.RMReleaseTicket >> Could not obtain valid version from '" +
                                         thisVersionClass.Name + "'!";
                        Logger.WriteError(message);
                        throw new InvalidOperationException(message);
                    }
                    else this._releaseVersion = thisVersion;
                }
                else this._releaseVersion = 1;
            }
            else this._releaseVersion = releaseVersion;
            Logger.WriteInfo("Plugin.Application.CapabilityModel.RMReleaseTicket >> Version set to: '" + this._releaseVersion + "'...");

            // This call tries to load a ticket with te exact specified release version or forces creation of a new ticket when no match is found...
            LoadTicketClass(releasePackageParent, context.GetConfigProperty(_RMReleaseStereotype));

            // We could have created a RMReleaseTicket for an existing release (this will be the case when a release comprises
            // multiple services). In this case, we will link the service ticket to the existing release ticket.
            // We create two links, one in each direction. This way, we can navigate either way between tickets.
            // If we have an existing Release Ticket that already has an association with the specified Service Ticket, we assume 
            // that we're done constructing.
            if (IsExistingTicket && !HasServiceTicket(serviceTicket))
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.RMReleaseTicket >> Existing Release Ticket, link our Service Ticket...");
                string ticketRole = context.GetConfigProperty(_RMTicketRole);
                string releaseRole = context.GetConfigProperty(_RMReleaseRole);
                string assocStereotype = context.GetConfigProperty(_RMAssociationStereotype);

                var svcTicketSource = new EndpointDescriptor(serviceTicket.TicketClass, "1", ticketRole, null, false);
                var svcTicketDestination = new EndpointDescriptor(serviceTicket.TicketClass, "1", ticketRole, null, true);
                var rlTicketSource = new EndpointDescriptor(this.TicketClass, "1", releaseRole, null, false);
                var rlTicketDestination = new EndpointDescriptor(this.TicketClass, "1", releaseRole, null, true);
                var releaseAssoc = model.CreateAssociation(rlTicketSource, svcTicketDestination, MEAssociation.AssociationType.Association);
                var ticketAssoc = model.CreateAssociation(svcTicketSource, rlTicketDestination, MEAssociation.AssociationType.Association);
                releaseAssoc.AddStereotype(assocStereotype, MEAssociation.AssociationEnd.Association);
                ticketAssoc.AddStereotype(assocStereotype, MEAssociation.AssociationEnd.Association);

                // We now set the modification date/time of the release ticket to the current date and time...
                this.TicketClass.SetTag(context.GetConfigProperty(_RMModificationTimestampTag), DateTime.Now.ToString("yyyy-MM-dd HH:mm"));

                // Update the diagram to show the new ticket association....
                var diagramAssocList = new List<MEAssociation>();
                var diagramClassList = new List<MEClass>();
                diagramAssocList.Add(releaseAssoc);
                diagramAssocList.Add(ticketAssoc);
                //diagramClassList.Add(serviceTicket.TicketClass);
                this.TicketDiagram.AddClassList(diagramClassList);
                this.TicketDiagram.AddAssociationList(diagramAssocList);
                this.TicketDiagram.Redraw();
            }

            // Instruct the service ticket to register it's current release version (formal release):
            this._serviceTicket.UpdateReleasedVersion();
            
            if (!IsExistingTicket)
            {
                // For new tickets, we set both the creation- and modification timestamps to the current date and time...
                TicketClass.SetTag(context.GetConfigProperty(_RMModificationTimestampTag), DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                TicketClass.SetTag(context.GetConfigProperty(_RMCreationTimestampTag), DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
            }
        }

        /// <summary>
        /// A dummy ticket can only be created when Release Management is disabled. In that case, we create an empty ticket
        /// and operations on the ticket will yield no effect. When an attempt is made to create dummy tickets while Release Management
        /// is enabled, the base classs constructor will throw an InvalidOperationException!
        /// </summary>
        /// <exception cref="InvalidOperationException">Is thrown when an attempt is made to create a dummy ticket while Release
        /// Management is active.</exception>
        internal RMReleaseTicket() : base()
        {
            this._releaseVersion = 0;
            this._serviceTicket = null;
        }

        /// <summary>
        /// Checks whether this Release Ticket has an association with the provided Service Ticket. When Release Management is disabled,
        /// the function performs no operations and always returns false!
        /// </summary>
        /// <param name="svcTicket">Ticket to check.</param>
        /// <returns>True when an association with the specified Service Ticket exists (and RM is enabled), false otherwise.</returns>
        internal bool HasServiceTicket(RMServiceTicket svcTicket)
        {
            if (IsReleaseManagementEnabled)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.RMReleaseTicket.HasServiceTicket >> Check for association between '" +
                                 this.GetQualifiedID() + "' and '" + svcTicket.GetQualifiedID() + "'...");

                // First of all, we retrieve all Tickets with identical qualified ID...
                List<MEClass> ticketList = this.TicketClass.FindAssociatedClasses(svcTicket.GetQualifiedID(),
                                                                                   ContextSlt.GetContextSlt().GetConfigProperty(_RMTicketStereotype));
                if (ticketList.Count > 0)
                {
                    // At least one existing association, check whether they are associated with the same Service...
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.RMReleaseTicket.HasServiceTicket >> Found some tickets...");
                    int myServiceID = svcTicket.TrackedService.ServiceClass.ElementID;
                    foreach (MEClass ticket in ticketList)
                    {
                        foreach (MEAssociation assoc in ticket.AssociationList)
                        {
                            if (assoc.Destination.EndPoint.ElementID == myServiceID)
                            {
                                Logger.WriteInfo("Plugin.Application.CapabilityModel.RMReleaseTicket.HasServiceTicket >> Found Service association!");
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Creates a new instance of an UML Release Ticket Class in the specified package and diagram. We don't need to load all tags since this
        /// will be coordinated by the base class. When Release Management is disabled, the function performs no operations and always returns null!
        /// </summary>
        /// <param name="ticketPackage">Package that should contain the new ticket.</param>
        /// <param name="ticketDiagram">Diagram that should show the new ticket.</param>
        /// <returns>Created ticket class or NULL in case of errors or RM disabled.</returns>
        protected override MEClass CreateNewTicketClass(MEPackage ticketPackage, Diagram ticketDiagram)
        {
            if (!IsReleaseManagementEnabled) return null;

            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();

            var diagramAssocList = new List<MEAssociation>();
            var diagramClassList = new List<MEClass>();

            // We need a bunch of configuration items...
            string timelineParentRole = context.GetConfigProperty(_RMTimelineParentRole);
            string timelineChildRole = context.GetConfigProperty(_RMTimelineChildRole);
            string ticketRole = context.GetConfigProperty(_RMTicketRole);
            string releaseRole = context.GetConfigProperty(_RMReleaseRole);
            string ticketClassStereotype = context.GetConfigProperty(_RMReleaseStereotype);
            string assocStereotype = context.GetConfigProperty(_RMAssociationStereotype);
            string versionTag = context.GetConfigProperty(_RMReleaseVersionNumberTag);

            // Before we're going to create anything, get the currently latest version since that ticket will
            // act as our parent...
            MEClass myParent = GetLatestVersion();

            // Create the ticket...
            MEClass releaseTicketClass = ticketPackage.CreateClass(GetQualifiedID(), ticketClassStereotype);
            diagramClassList.Add(releaseTicketClass);
    
            // Now, check if we have a parent to connect to...
            if (myParent != null)
            {
                // Parent exists, we're going to link to this parent.
                // We create two associations, one from parent to child and one in the other direction. This way, we can
                // navigate either way...
                var parentSource = new EndpointDescriptor(myParent, "1", timelineParentRole, null, false);
                var parentDestination = new EndpointDescriptor(myParent, "1", timelineParentRole, null, true);
                var childSource = new EndpointDescriptor(releaseTicketClass, "1", timelineChildRole, null, false);
                var childDestination = new EndpointDescriptor(releaseTicketClass, "1", timelineChildRole, null, true);
                var parentAssoc = model.CreateAssociation(parentSource, childDestination, MEAssociation.AssociationType.Association);
                var clientAssoc = model.CreateAssociation(childSource, parentDestination, MEAssociation.AssociationType.Association);
                parentAssoc.AddStereotype(assocStereotype, MEAssociation.AssociationEnd.Association);
                clientAssoc.AddStereotype(assocStereotype, MEAssociation.AssociationEnd.Association);
                diagramAssocList.Add(parentAssoc);
                diagramAssocList.Add(clientAssoc);
            }

            // Now that we have a release ticket, create an association with the associated service ticket...
            var svcTicketSource = new EndpointDescriptor(this._serviceTicket.TicketClass, "1", ticketRole, null, false);
            var svcTicketDestination = new EndpointDescriptor(this._serviceTicket.TicketClass, "1", ticketRole, null, true);
            var rlTicketSource = new EndpointDescriptor(releaseTicketClass, "1", releaseRole, null, false);
            var rlTicketDestination = new EndpointDescriptor(releaseTicketClass, "1", releaseRole, null, true);
            var releaseAssoc = model.CreateAssociation(rlTicketSource, svcTicketDestination, MEAssociation.AssociationType.Association);
            var ticketAssoc = model.CreateAssociation(svcTicketSource, rlTicketDestination, MEAssociation.AssociationType.Association);
            releaseAssoc.AddStereotype(assocStereotype, MEAssociation.AssociationEnd.Association);
            ticketAssoc.AddStereotype(assocStereotype, MEAssociation.AssociationEnd.Association);
            diagramAssocList.Add(releaseAssoc);
            diagramAssocList.Add(ticketAssoc);
            //diagramClassList.Add(this._serviceTicket.TicketClass);

            // Update the diagram...
            ticketDiagram.AddClassList(diagramClassList);
            ticketDiagram.AddAssociationList(diagramAssocList);
            ticketDiagram.Redraw();
            return releaseTicketClass;
        }

        /// <summary>
        /// Copies relevant attributes to our UML ticket class.
        /// </summary>
        internal override void UpdateTicket()
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.RMReleaseTicket.UpdateTicket >> Syncing ticket data...");
            base.UpdateTicket();   // First of all, we copy the generic properties

            TicketClass.SetTag(ContextSlt.GetContextSlt().GetConfigProperty(_RMReleaseVersionNumberTag), this._releaseVersion.ToString());
            TicketClass.SetTag(ContextSlt.GetContextSlt().GetConfigProperty(_RMModificationTimestampTag), DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
        }

        /// <summary>
        /// We search through the list of Release Tickets with the same name and return the one with the highest version (or NULL when no
        /// tickets are available). When Release Management is disabled, the function always returns null!
        /// </summary>
        /// <returns>Class representing the latest release or NULL when no match found (or RM disabled).</returns>
        private MEClass GetLatestVersion()
        {
            MEClass targetTicket = null;
            if (IsReleaseManagementEnabled)
            {
                ContextSlt context = ContextSlt.GetContextSlt();
                string releaseClassStereotype = context.GetConfigProperty(_RMReleaseStereotype);
                string versionTag = context.GetConfigProperty(_RMReleaseVersionNumberTag);
                int foundReleaseVersion = 0;

                if (TicketPackage == null) return null;     // Ticket package does not (yet) exists, nothing found!

                // Locate all Classes that have the specified name fragment and stereotype. Note that the order in which the classes are 
                // returned is unspecified...
                foreach (MEClass ticket in TicketPackage.FindClasses(ProjectName + "/" + ID, releaseClassStereotype))
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.RMReleaseTicket.GetPreviousRelease >> Inspecting Ticket '" + ticket.Name + "'...");
                    int thisVersion = 0;
                    if (int.TryParse(ticket.GetTag(versionTag), out thisVersion))
                    {
                        if (thisVersion > foundReleaseVersion)
                        {
                            foundReleaseVersion = thisVersion;
                            targetTicket = ticket;
                        }
                    }
                }
            }
            Logger.WriteInfo("Plugin.Application.CapabilityModel.RMReleaseTicket.GetPreviousRelease >> Returning Class '" + 
                             (targetTicket != null? targetTicket.Name: "-NOTHING-") + "'...");
            return targetTicket;
        }

        /// <summary>
        /// Helper function that initializes class properties to suitable defaults in case Release Management is off.
        /// </summary>
        /// <returns>True when RM is enabled, false otherwise.</returns>
        private bool HasRMEnabled()
        {
            if (!IsReleaseManagementEnabled)
            {
                this._releaseVersion = 0;
                this._serviceTicket = null;
            }
            return IsReleaseManagementEnabled;
        }
    }
}
