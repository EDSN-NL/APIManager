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
    /// Release Management - ReleaseTicket represents a 'release', e.g. a set of changes for one or multiple services, which are deployed to
    /// production as one set. The class manages the Release History package.
    /// </summary>
    sealed internal class RMReleaseTicket
    {
        // Private configuration properties used by this service...
        private const string _RMPackageName                 = "RMPackageName";
        private const string _RMPackageStereotype           = "RMPackageStereotype";
        private const string _RMRootReleasePackagePath      = "RMRootReleasePackagePath";
        private const string _RMReleasePackageName          = "RMReleasePackageName";
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

        private RMTicket _ticket;                           // Associated Ticket.
        private MEPackage _releasePackage;                  // The package in which our ticket(s) live.
        private MEClass _ticketClass;                       // Model element representing the release ticket.
        private Diagram _myDiagram;                         // Diagram containing our ticket classes.
        private int _releaseVersion;                        // Multiple releases based on the same ticket each increment this number.

        /// <summary>
        /// Returns the unique Ticket identifier.
        /// </summary>
        internal string ID { get { return this._ticket.ID; } }

        /// <summary>
        /// Returns the qualified Ticket Identifier, which is a combination of ticket-project name, ticket ID and release version, formatted as:
        /// [project-name]/[Ticket-ID].[Release-Version]. Example: "CSTI Integration/CSTI-2345.01"
        /// The release version is formatted as a two-digit number, including leading zeroes when necessary.
        /// </summary>
        internal string QualifiedID { get { return this._ticket.ProjectName + "/" + this._ticket.ID + "." + this._releaseVersion.ToString("00"); } }

        /// <summary>
        /// Returns 'true' when this Ticket object is associated with a remote ticket.
        /// </summary>
        internal bool Valid { get { return this._ticket.Valid; } }

        /// <summary>
        /// The constructor creates a new release ticket with specified Ticket ID, Project ID and release version.
        /// If a release ticket with the exact same ID (and version) already exists (but associated with another service), we only add the
        /// new Service Ticket. This implies that for each service that is part of a release, we can create 'identical' release tickets
        /// without the need to check whether the ticket already exists.
        /// </summary>
        /// <param name="ticketID">Identifier of the ticket we want to create/connect to.</param>
        /// <param name="releaseVersion">Optional release version, we assume '1' when not specified.</param>
        /// <exception cref="ArgumentException">Is thrown when the specified ticketID does not yield a valid ticket or when no valid PO number has been specified.</exception>
        internal RMReleaseTicket(RMTicket ticket, int releaseVersion = 1)
        {
            Logger.WriteInfo("Plugin.Application.CapabilityModel.RMReleaseTicket >> Creating release for Ticket '" + 
                             ticket.ProjectName + "/" + ticket.ID + "." + releaseVersion + "' for service '" + ticket.TrackedService.Name + "'...");

            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            this._ticket = ticket;
            this._releaseVersion = releaseVersion;

            // Various configuration items...
            string releasePackageName = context.GetConfigProperty(_RMReleasePackageName);
            string releasePackagePath = context.GetConfigProperty(_RMRootReleasePackagePath);
            string releaseTicketStereotype = context.GetConfigProperty(_RMReleaseStereotype);

            // Load the release package, this MUST Exist!
            this._releasePackage = model.FindPackage(releasePackagePath, releasePackageName);
            if (this._releasePackage == null)
            {
                string message = "Plugin.Application.CapabilityModel.RMReleaseTicket >> Release package '" + releasePackagePath + 
                                 ":" + releasePackageName + "' does not exist!";
                Logger.WriteError(message);
                throw new ArgumentException(message);
            }

            // Retrieve our diagram (showing all release tickets), this MUST Exist!
            this._myDiagram = this._releasePackage.FindDiagram(this._releasePackage.Name);
            if (this._myDiagram == null)
            {
                string message = "Plugin.Application.CapabilityModel.RMReleaseTicket >> Release package '" + releasePackagePath + 
                                 ":" + releasePackageName + "' does not have a diagram!";
                Logger.WriteError(message);
                throw new ArgumentException(message);
            }

            // Now that we have a package, try to find the ticket...
            this._ticketClass = this._releasePackage.FindClass(QualifiedID, releaseTicketStereotype);
            if (this._ticketClass != null)
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.RMTicket >> Existing release ticket...");
                //UpdateReleaseTicket();

            }
            else
            {
                Logger.WriteInfo("Plugin.Application.CapabilityModel.RMTicket >> New release ticket...");
                CreateReleaseTicket();
            }
        }

        /// <summary>
        /// Returns a string representation of the ticket.
        /// </summary>
        /// <returns>Human friendly, formatted ticket information.</returns>
        public override string ToString()
        {
            return this._ticket.ToString() + Environment.NewLine + "Release version: '" + this._releaseVersion + "'.";
        }

        /// <summary>
        /// Creates a new Release ticket and associate with service ticket. If the release version is > 1, we try to locate the last release
        /// for the same ticket and link the new release to the previous release.
        /// </summary>
        private void CreateReleaseTicket()
        {
            // Before creating the new ticket, search the package for the end of the timeline, which will be the anchor for the new ticket...
            MEClass myParent = GetPreviousRelease();
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
            this._ticketClass = this._releasePackage.CreateClass(QualifiedID, ticketClassStereotype);

            // Now, check if we have a parent to connect to...
            /************
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
            *************/
        }

        /// <summary>
        /// Check whether we have a previous release ticket (with lower release version) and return the associated class.
        /// </summary>
        /// <returns>Class representing previous release or NULL when nothing there yet.</returns>
        private MEClass GetPreviousRelease()
        {
            MEClass previous = null;
            if (this._releaseVersion > 1)
            {
                ContextSlt context = ContextSlt.GetContextSlt();
                string releaseClassStereotype = context.GetConfigProperty(_RMReleaseStereotype);
                string parentRole = context.GetConfigProperty(_RMTimelineParentRole);
                int foundReleaseVersion = 0;

                foreach (MEClass ticket in this._releasePackage.FindClasses(this._ticket.ProjectName + "/" + this._ticket.ID, releaseClassStereotype))
                {
                    Logger.WriteInfo("Plugin.Application.CapabilityModel.RMTicket.GetTimelineTail >> Inspecting Ticket '" + ticket.Name + "'...");
                    int thisVersion = 0;
                    if (int.TryParse(ticket.Name.Substring(ticket.Name.Length - 2), out thisVersion))
                    {
                        if (thisVersion > foundReleaseVersion)
                        {
                            foundReleaseVersion = thisVersion;
                            previous = ticket;
                        }
                    }
                }
            }
            Logger.WriteInfo("Plugin.Application.CapabilityModel.RMTicket.GetTimelineTail >> Returning Class '" + 
                             (previous != null? previous.Name: "-NOTHING-") + "'...");
            return previous;
        }
    }
}
