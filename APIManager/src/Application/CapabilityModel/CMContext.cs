using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using LibGit2Sharp;
using Framework.Context;
using Framework.Logging;
using Framework.Model;
using Framework.Util;
using Framework.ConfigurationManagement;

namespace Plugin.Application.CapabilityModel
{
    /// <summary>
    /// Configuration Management - Context provides the interface between the service and configuration management
    /// by encapsulating a service-specific local- and remote repository path.
    /// The CMContext class provides all interfacing between the service and configuration management.
    /// Note that this context class is used ONLY when configuration management is enabled.
    /// </summary>
    sealed internal class CMContext
    {
        /// <summary>
        /// Defines the scope of a commit operation. Choices are:
        /// Local - Only commit to local repository;
        /// Remote - Commit locally, then push to branch on remote;
        /// Release - Commit locally, then create a release;
        /// </summary>
        internal enum CommitScope { Unknown, Local, Remote, Release }

        // These are used by the ActivateBranch function to indicate whether we must pull the branch from remote or not...
        internal bool _DoSync = true;
        internal bool _NoSync = false;

        /// <summary>
        /// In case we're using configuration management, the service can be in any of the following states:
        /// Created - New service that has not yet been added to configuration management;
        /// Modified - Service is in configuration management, but configuration items have been changed (and not yet committed);
        /// Committed - Configuration items have been committed to local repository, but have not yet been released to central repository;
        /// Tagged - Committed service has been pushed to remote with a commit tag, indicating a handover to development;
        /// Released - Configuration items have been committed and released to central repository;
        /// CheckedOut - Service has been checked-out from configuration management but no changes have been made (yet).
        /// Disabled indicates that configuration management is not used.
        /// Unknown indicates that the state is not (yet) known.
        /// </summary>
        internal enum CMState { Unknown, Disabled, Created, Modified, Committed, Released, CheckedOut }

        // Private configuration properties used by this service...
        private const string _CMBranchTag               = "CMBranchTag";
        private const string _CMStateTag                = "CMStateTag";
        private const string _CMDefaultState            = "CMDefaultState";
        private const string _XMIFileSuffix             = "XMIFileSuffix";
        private const string _CompressedFileSuffix      = "CompressedFileSuffix";
        private const string _RMTicketFQNTag            = "RMTicketFQNTag";
        private const string _RMTicketStereotype        = "RMTicketStereotype";
        private const string _RMPackageName             = "RMPackageName";
        private const string _CMFeatureBranchPrefix     = "CMFeatureBranchPrefix";
        private const string _CMArtefactsFolderName     = "CMArtefactsFolderName";
        private const string _CMSnapshotsFolderName     = "CMSnapshotsFolderName";
        private const string _ServiceDeclPkgStereotype  = "ServiceDeclPkgStereotype";
        private const string _ServiceContainerPkgStereotype = "ServiceContainerPkgStereotype";
        private const string _ServiceModelPkgName       = "ServiceModelPkgName";
        private const string _ServiceClassStereotype    = "ServiceClassStereotype";

        // Some constants...
        private const string _NoRemoteBranch            = "reference was not fetched";
        private const string _NoTrackingRemoteBranch    = "no tracking information";

        private CMRepositorySlt _repository;            // Singleton Repository instance (provides interface with remote as well).
        private Service _trackedService;                // The Service instance for which we perform configuration management.
        private string _branchName;                     // The name of the branch that we're currently using to track our service.
        private bool _snapshotCreated;                  // Used to keep track of snapshot creation in combination with auto-release.
        private RMServiceTicket _ticket;                // Our currently active ticket.
        private CMState _configurationMgmtState;        // Current configuration management state for the tracked service.
        private bool _isCMEnabled;                      // True when we use CM for the currently open repository.
        private string _commitPath;                     // Relative path to the folder that acts as working directory for the repository.
        private string _snapshotPath;                   // Absolute path to the folder that (might) contain a service snapshot.

        /// <summary>
        /// Returns 'true' when configuration management is enabled in general (for this repository).
        /// </summary>
        internal bool CMEnabledRepository { get { return this._isCMEnabled; } }

        /// <summary>
        /// Returns 'true' when configuration management is enabled for this particular service.
        /// </summary>
        internal bool CMEnabledService
        {
            get
            {
                return this._isCMEnabled && this._configurationMgmtState != CMState.Disabled && this._configurationMgmtState != CMState.Unknown;
            }
        }

        /// <summary>
        /// Get- or set the CM state for the service and (in case of set), update context accordingly.
        /// </summary>
        internal CMState State
        {
            get { return this._configurationMgmtState; }
            set { UpdateCMState(value); }
        }

        /// <summary>
        /// Get- or set the ticket for this service. If the service is in 'released' state, this is the ticket used for the most
        /// recent release. In case we don't have an associated ticket, get will return NULL.
        /// </summary>
        internal RMServiceTicket Ticket
        {
            get { return this._ticket; }
            set { SetTicket(value); }
        }

        /// <summary>
        /// Constructor, receives the Service object that we want to manage and loads service-specific configuration. The constructor performs
        /// only minimal initialization. Before actually using the repository, one must invoke the 'CheckoutService' operation in order to 
        /// assure that the repository is in the correct state for performing necessary operations.
        /// </summary>
        /// <param name="trackedService">Service under configuration management.</param
        /// <param name="ticket">Ticket in practice is only specified on a new service creation, otherwise this will be null and we
        /// attempt to retrieve the ticket from the service model.</param>
        /// <exception cref="InvalidOperationException">Thrown when the specified services does not contain a ticket ID or we can't find 
        /// the ticket package.</exception>
        /// <exception cref="ArgumentException">Thrown when we can't load the ticket.</exception>
        internal CMContext(Service trackedService, RMServiceTicket ticket)
        {
            Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext >> Creating context for service '" + trackedService.Name + "'...");
            ContextSlt context = ContextSlt.GetContextSlt();
            bool noCMStateService = false;
            this._repository = CMRepositorySlt.GetRepositorySlt();
            this._trackedService = trackedService;
            this._snapshotCreated = false;
            this._ticket = ticket;
            this._branchName = null;
            this._commitPath = null;    // Will be set on first commit.
            this._snapshotPath = null;  // Will be set when first needed.
            this._isCMEnabled = CMRepositorySlt.GetRepositorySlt().IsCMEnabled;

            if (this._isCMEnabled)
            {
                // If the Service contains a branch name, we retrieve it here (but only when te state allows us to do this).
                // This effectively allows us to restore the state of the CM context in case we close our modelling tool after a Checkout.
                string CMBranchTagName = context.GetConfigProperty(_CMBranchTag);
                string ticketIDTagName = context.GetConfigProperty(_RMTicketFQNTag);
                string stateTagName = context.GetConfigProperty(_CMStateTag);

                // Check whether we have a state registered in the service. There should be one, but we don't know for sure...
                string stateTag = trackedService.ServiceClass.GetTag(stateTagName);
                if (string.IsNullOrEmpty(stateTag))
                {
                    this._configurationMgmtState = CMState.Created;
                    trackedService.ServiceClass.SetTag(stateTagName, EnumConversions<CMState>.EnumToString(CMState.Created), true);
                }
                else
                {
                    // If we do have a current state, check whether it is the default state for new services.
                    // In this case, ignore the existing state and replace by 'created'...
                    this._configurationMgmtState = EnumConversions<CMState>.StringToEnum(stateTag);
                    if (stateTag == context.GetConfigProperty(_CMDefaultState))
                    {
                        if (ticket != null) 
                        {
                            this._configurationMgmtState = CMState.Created;
                            trackedService.ServiceClass.SetTag(stateTagName, EnumConversions<CMState>.EnumToString(CMState.Created), true);
                        }
                        else noCMStateService = true; // Service in state disabled and without a ticket can't be used with CM!
                    }
                }

                if (this._configurationMgmtState == CMState.Released)
                {
                    Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext >> Released service!");
                    string ticketID = this._trackedService.ServiceClass.GetTag(ticketIDTagName);
                    if (!string.IsNullOrEmpty(ticketID))
                    {
                        Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext >> Existing Ticket ID is: '" + ticketID + "'...");
                        this._ticket = GetTicket(ticketID);
                    }
                }
                else if (noCMStateService)
                {
                    Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext >> Service has no valid CM state, can't enable context!");
                    this._configurationMgmtState = CMState.Disabled;
                }
                else // Anything else...
                {
                    this._branchName = this._trackedService.ServiceClass.GetTag(CMBranchTagName);
                    Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext >> Service branch is: '" +
                                     (string.IsNullOrEmpty(this._branchName) ? "NO-BRANCH" : this._branchName) + "'.");

                    // We should also have an open Ticket, except in case of new services (in which case the ticket must be an argument
                    // to the constructor). A ticket argument always has precedence over a stored ticket!
                    if (ticket != null)
                    {
                        Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext >> Received (new) ticket ID '" + ticket.ID + "'...");
                        this._ticket = ticket;
                        this._trackedService.ServiceClass.SetTag(ticketIDTagName, ticket.GetQualifiedID(), true);
                    }
                    else
                    {
                        string ticketID = this._trackedService.ServiceClass.GetTag(ticketIDTagName);
                        if (!string.IsNullOrEmpty(ticketID))
                        {
                            Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext >> Existing Ticket ID is: '" + ticketID + "'...");
                            this._ticket = GetTicket(ticketID);
                        }
                        else
                        {
                            string msg = "Plugin.Application.ConfigurationManagement.CMContext >> Ticket missing in service '" + trackedService.Name + "'!";
                            Logger.WriteError(msg);
                            throw new InvalidOperationException(msg);
                        }
                    }
                }
            }
            else this._configurationMgmtState = CMState.Disabled;
        }

        /// <summary>
        /// Activates the feature branch for the service. This is either an existing branch (stored in the service model) or a new one (in case
        /// the service does not yet contain a cached branch name).
        /// </summary>
        /// <param name="doSync">When set to 'true' and we have an existing branch, the function performs a 'pull' from remote after
        /// switching to the branch. This assures that we're in sync with the remote branch.</param>
        internal void ActivateBranch(bool doSync)
        {
            try
            {
                ContextSlt context = ContextSlt.GetContextSlt();
                string CMBranchTagName = context.GetConfigProperty(_CMBranchTag);
                this._branchName = this._trackedService.ServiceClass.GetTag(CMBranchTagName);
                if (string.IsNullOrEmpty(this._branchName))
                {
                    this._branchName = GetFeatureBranchName();
                    this._trackedService.ServiceClass.SetTag(CMBranchTagName, this._branchName, true);
                    Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.ActivateBranch >> Branch created: '" + this._branchName + "'.");
                }

                // Activate our new branch so that artefact generation will be associated with this branch. 
                // Please note that, although we push the new branch to remote, it will not show there until we pushed a commit to it.
                this._repository.SetBranch(this._branchName, this._repository.ReleaseBranchName);
                if (doSync) this._repository.Pull();
            }
            catch (Exception exc)
            {
                if (exc.Message.Contains(_NoRemoteBranch) || exc.Message.Contains(_NoTrackingRemoteBranch))
                {
                    Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.ActivateBranch >> Service branch '" + this._branchName +
                                     "' does not (yet) exist at remote, creating one...");
                    this._repository.PushBranch(this._branchName);
                }
                else throw;
            }
        }

        /// <summary>
        /// This method must be called when we want to start working with a service. The current branch for the service is determined and if this is found to 
        /// be new, a new branch is created in the repository (as a fork from the development branch). The new/existing branch is then checked-out so work on 
        /// the service will commence on that branch. 
        /// Before doing all this, we perform a 'pull' on the remote (from development) to assure that we are in sync. 
        /// If the pull resulted in a 'reference not fetched' exception, this indicates that we have a new local branch that does not yet exist at remote. We
        /// create the branch at remote to assure other developers are aware of it.
        /// Depending on the context and the current CM state of the service, a new service CM state is determined and written to the service.
        /// When CM is disabled, or the service is already checked-out, the method does not perform any operation.
        /// </summary>
        /// <param name="ticket">A Service check-out must always be based on a change Ticket.</param>
        /// <exception cref="ArgumentException">Invalid ticket or we could not checkout the service (i.e. due to merge conflicts or syncronisation errors).</exception>
        /// <exception cref="InvalidOperationException">Thrown on merge conflicts or when the service is not in the correct state for checkout.</exception>
        internal void CheckoutService(RMServiceTicket ticket)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string ticketIDTagName = context.GetConfigProperty(_RMTicketFQNTag);
            RMServiceTicket currentTicket = this._ticket;
            this._ticket = ticket;
            this._trackedService.ServiceClass.SetTag(ticketIDTagName, ticket.GetQualifiedID(), true);

            // Nothing to do if CM not active or already in checked-out state...
            if (!CMEnabledService || this._configurationMgmtState == CMState.CheckedOut) return;

            if (!ticket.Valid)
            {
                this._ticket = currentTicket;   // Restore original ticket.
                string message = "Plugin.Application.ConfigurationManagement.CMContext.CheckoutService >> No valid ticket presented to checkout!";
                Logger.WriteError(message);
                throw new ArgumentException(message);
            }

            if (IsValidTransition(CMState.CheckedOut))
            {
                try
                {
                    this._repository.GotoBranch(this._repository.ReleaseBranchName); // Make sure we're on the master release branch
                    this._repository.Pull();                                         // Synchronise master branch (might affect our service).

                    // Now that the repository is in sync with remote, check whether versions are in sync.
                    // We only check this for services that are still in the repository (state is Committed or Released), for all other states,
                    // the check is not required since it has either performed in the past or we're dealing with a new service...
                    if (this._configurationMgmtState == CMState.Committed || this._configurationMgmtState == CMState.Released) CheckRemoteVersion();
                    ActivateBranch(_DoSync);   // Make sure we're on a valid branch for the service (and sync with remote).

                    // The new state will be changed only when the service is still in the repository (committed or released)...
                    if (this._configurationMgmtState == CMState.Committed || this._configurationMgmtState == CMState.Released) UpdateCMState(CMState.CheckedOut);
                    Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.CheckoutService >> Service CM state set to '" + this._configurationMgmtState + "'.");
                }
                catch (Exception exc)
                {
                    Logger.WriteError("Plugin.Application.ConfigurationManagement.CMContext.CheckoutService >> Caught an exception '" + exc.Message + "', passing on...");
                    this._ticket = currentTicket; // Restore original ticket.
                    throw;
                }
            }
            else
            {
                string errMsg = "Service '" + this._trackedService.Name + "' is not in the correct state for a checkout!";
                Logger.WriteError("Plugin.Application.ConfigurationManagement.CMContext.CheckoutService >> " + errMsg);
                throw new InvalidOperationException(errMsg);
            }
        }

        /// <summary>
        /// Checks the current version of the service against the last released version (if any). If major- and minor versions match but our build
        /// number is out of sync (which could be in case somebody else has issued an updated version), we simply update our local build number.
        /// If the major and/or minor versions are out of sync, we issue a warning (could be intentional).
        /// </summary>
        /// <returns>False in case major and/or minor versions lag behind remote, true otherwise.</returns>
        internal bool CheckRemoteVersion()
        {
            bool retVal = true;
            Tuple<int, int, int> lastReleased = GetLastReleasedVersion();
            Tuple<int, int> svcVersion = this._trackedService.Version;
            if (lastReleased.Item1 > 0)
            {
                Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.CheckRemoteVersion >> We have an existing release: '" +
                                 lastReleased.Item1 + "." + lastReleased.Item2 + "." + lastReleased.Item3 + "'...");
                if ((svcVersion.Item1 == lastReleased.Item1 && svcVersion.Item2 < lastReleased.Item2) ||
                    (svcVersion.Item1 < lastReleased.Item1))
                {
                    Logger.WriteWarning("Service '" + this._trackedService.Name + "' with version '" + svcVersion.Item1 + "." + svcVersion.Item2 +
                                        "' does not match last released version '" + lastReleased.Item1 + "." + lastReleased.Item2 + "'!");
                    retVal = false;
                }
                if (svcVersion.Item1 == lastReleased.Item1 && svcVersion.Item2 == lastReleased.Item2 && this._trackedService.BuildNumber <= lastReleased.Item3)
                {
                    Logger.WriteWarning("The build number of Service '" + this._trackedService.Name + 
                                        "' lags behind the last released version; service updated!");
                    this._trackedService.BuildNumber = lastReleased.Item3 + 1;
                }
            }
            return retVal;
        }

        /// <summary>
        /// Commits all files that are in the service build path to the repository at the HEAD of the current branch, which must be
        /// forked from the Development branch. 
        /// First of all, we instruct the repository to add all files in the current build path to the staging area. If there is nothing
        /// to commit, we issue a warning message to the log to indicate that no operation has been performed.
        /// When autorelease is 'false' (the default), we will NOT push anything to remote. When autorelease is 'true', the current
        /// contents of the local repository will be pushed as part of the release.
        /// So, we will never push anything to remote that is not tagged.
        /// If an auto-release has been ordered, we will invoke the release operation after a sucessfull commit.
        /// </summary>
        /// <param name="message">Commit reporting message.</param>
        /// <param name="commitScope">Defines whether we only perform a local commit, do a commit followed by a push to remote or do a commit
        /// followed by a release.</param>
        /// <returns>True when we actually committed something, false when there is nothing to commit (state has still been updated).</returns>
        /// <exception cref="InvalidOperationException">Is thrown when configuration settings are missing or identity has not been initialized yet or the
        /// service is not in the correct state to be committed.</exception>
        internal bool CommitService(string message, CommitScope commitScope = CommitScope.Local)
        {
            if (!this._repository.IsCMEnabled) return true;

            bool retVal = false;
            if (IsValidTransition(CMState.Committed))
            {
                Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.CommitService >> Committing service with message '" + message + "'...");

                this._snapshotCreated = false;  // We reset this on each commit since obviously the service state has changed (hence the new commit).
                SetupDirectories();             // Make sure we have our commit 'pointers' setup properly.
                ActivateBranch(_NoSync);        // Make sure we are on a valid branch (but don't pull the branch just yet since this might ruin the work we just done).

                // On auto-release, we create the snapshot on commit-time so it can be committed together with the other artefacts in a 
                // single transaction. Release will just perform a 'tagged-push' to remote...
                if (commitScope == CommitScope.Release)
                {
                    Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.CommitService >> Create snapshot early...");
                    CreateSnapshot();
                    this._snapshotCreated = true;
                }

                this._repository.AddToStagingArea(this._commitPath);
                if (this._repository.CommitStagingArea(message, commitScope == CommitScope.Remote))
                {
                    UpdateCMState(CMState.Committed);           // Must be updated before we proceed with the release!
                    if (commitScope == CommitScope.Release) ReleaseService(message);
                    retVal = true;
                } else UpdateCMState(CMState.Committed);         // Even though there were no changes, we still consider this a successfull commit.
            }
            else
            {
                string errMsg = "Service '" + this._trackedService.Name + "' is not in the correct state for a commit!";
                Logger.WriteError("Plugin.Application.ConfigurationManagement.CMContext.CommitService >> " + errMsg);
                throw new InvalidOperationException(errMsg);
            }
            return retVal;
        }

        /// <summary>
        /// Helper function that copies relevant CM state from an existing service class to a new service class. If we specify the
        /// 'forceState' flag, the CM State is always set to 'Created', irrespective of the state of the originating service.
        /// Branch- and ticket names are always copied from the existing service.
        /// </summary>
        /// <param name="existingServiceClass">Originating service.</param>
        /// <param name="newServiceClass"></param>
        /// <param name="forceCreated">Optional, when set to 'true', the new service will forced to state 'Created'.</param>
        internal static void CopyState(MEClass existingServiceClass, MEClass newServiceClass, bool forceCreated = false)
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string stateTagName = context.GetConfigProperty(_CMStateTag);
            string branchTagName = context.GetConfigProperty(_CMBranchTag);
            string ticketIDTagName = context.GetConfigProperty(_RMTicketFQNTag);

            string state = existingServiceClass.GetTag(stateTagName);
            string ticketID = existingServiceClass.GetTag(ticketIDTagName);
            string branchName = existingServiceClass.GetTag(branchTagName);

            if (forceCreated) newServiceClass.SetTag(stateTagName, EnumConversions<CMState>.EnumToString(CMState.Created), true);
            else if (!string.IsNullOrEmpty(state)) newServiceClass.SetTag(stateTagName, state, true);
            if (!string.IsNullOrEmpty(ticketID)) newServiceClass.SetTag(ticketIDTagName, ticketID, true);
            if (!string.IsNullOrEmpty(branchName)) newServiceClass.SetTag(branchTagName, branchName, true);
        }

        /// <summary>
        /// Deletes all specified tags from the repository (both locally and remote).
        /// </summary>
        /// <param name="tagList">List of tags to be deleted.</param>
        internal void DeleteTags(List<string> tagList)
        {
            foreach (string tag in tagList) this._repository.DeleteTag(this._repository.GetTag(tag));
        }

        /// <summary>
        /// Returns a list of all branches in the CM repository that have the specified CM state.
        /// </summary>
        /// <param name="thisState">State we're looking for</param>
        /// <returns>List of all branches in specified state. If a services is in the specified state, but does not have an active branch,
        /// the function returns the service name instead (should not happen).</returns>
        internal static string FindBranchesInState(CMState thisState)
        {
            Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.FindBranchesInState >> Looking for services in state '" + thisState.ToString() + "'...");

            ContextSlt context = ContextSlt.GetContextSlt();
            ModelSlt model = ModelSlt.GetModelSlt();
            string stateTagValue = EnumConversions<CMState>.EnumToString(thisState);
            string stateTagName = context.GetConfigProperty(_CMStateTag);
            string branchTagName = context.GetConfigProperty(_CMBranchTag);
            List<MEClass> classList = model.FindTaggedValue(stateTagName, stateTagValue);
            string retVal = string.Empty;
            bool firstOne = true;
            foreach (MEClass cl in classList)
            {
                string branchName = cl.GetTag(branchTagName);
                if (string.IsNullOrEmpty(branchName)) branchName = cl.Name;
                retVal += firstOne ? branchName : ", " + branchName;
                firstOne = false;
            }
            Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.FindCMStateServiceNames >> Found: '" + retVal + "'...");
            return retVal;
        }

        /// <summary>
        /// Returns a triplet containing the most recent released version of the associated service.
        /// </summary>
        /// <returns>Returns last released version as major, minor, build-number. Or 0,0,0 when no earlier release is found. NULL is returned on errors.</returns>
        internal Tuple<int, int, int> GetLastReleasedVersion()
        {
            Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.GetLastReleasedVersion >> Looking for released versions...");

            Debug.Assert(this._repository != null);
            try
            {
                List<Tag> myTags = GetReleaseTags(); // Returns tags sorted by version in descending order.
                if (myTags.Count > 0) return ParseTagVersion(myTags[0]);
                else
                {
                    Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.GetLastReleasedVersion >> No earlier release found!");
                    return new Tuple<int, int, int>(0, 0, 0);
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.ConfigurationManagement.CMContext.GetLastReleasedVersion >> Unable to retrieve tags because: " +
                                  Environment.NewLine + exc.ToString());
            }
            return null;
        }

        /// <summary>
        /// Returns a list of all release tags for the service in descending order (latest version comes first).
        /// </summary>
        /// <returns>All release tag names or empty list if no releases (yet).</returns>
        internal List<string> GetReleaseTagNames()
        {
            List<string> releaseTagNames = new List<string>();
            foreach (Tag t in GetReleaseTags()) releaseTagNames.Add(t.FriendlyName);
            return releaseTagNames;
        }

        /// <summary>
        /// The function checks whether a transition from the current state to the proposed state is allowed.
        /// The CMContext State Machine supports the following set of transitions:
        /// Disabled --> Created;
        /// Unknown --> Any-state;
        /// Any-state --> Disabled;
        /// Created --> Modified;
        /// Created --> Committed;
        /// Modified --> Committed;
        /// Committed --> Released;
        /// Committed --> CheckedOut;
        /// Released --> CheckedOut;
        /// CheckedOut --> Modified;
        /// CheckedOut --> Committed;
        /// </summary>
        /// <param name="newState">Proposed state.</param>
        /// <returns>True in case transition is allowed, false otherwise.</returns>
        internal bool IsValidTransition(CMState proposedState)
        {
            bool isOk = false;
            if (proposedState == CMState.Disabled               || 
                this._configurationMgmtState == CMState.Unknown || 
                this._configurationMgmtState == proposedState   ||
                (this._configurationMgmtState == CMState.Disabled   && proposedState == CMState.Created)    ||
                (this._configurationMgmtState == CMState.Created    && proposedState == CMState.Modified)   ||
                (this._configurationMgmtState == CMState.Created    && proposedState == CMState.Committed)  ||
                (this._configurationMgmtState == CMState.Modified   && proposedState == CMState.Committed)  ||
                (this._configurationMgmtState == CMState.Committed  && proposedState == CMState.CheckedOut) ||
                (this._configurationMgmtState == CMState.Committed  && proposedState == CMState.Released)   ||
                (this._configurationMgmtState == CMState.Released   && proposedState == CMState.CheckedOut) ||
                (this._configurationMgmtState == CMState.CheckedOut && proposedState == CMState.Committed)  ||
                (this._configurationMgmtState == CMState.CheckedOut && proposedState == CMState.Modified)) isOk = true;
            return isOk;
        }

        /// <summary>
        /// Pushes all Service Configuration Items for the local service to the appropriate branch on the remote repository. 
        /// Each release is accompanied by a Tag that provide additional info regarding the release. 
        /// The Tag is named according to the release: Fully-Qualified-Service-Name-[_OperationalState_]VxPyBz.
        /// Example: 3010.01.03.01.MyContainer.MyService_V1P1B4 --> Version 1.1 Build 4 of 'MyService'.
        /// Note that for each release, the build number is incremented. This also assures that the tag names remain unique within the repository.
        /// </summary>
        /// <param name="message">Release reporting message.</param>
        /// <exception cref="InvalidOperationException">Thrown when the service is not in the correct state to be released.</exception>
        internal void ReleaseService(string message)
        {
            if (!this._repository.IsCMEnabled) return;

            Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.ReleaseService >> Releasing Service to remote with message '" + message + "'...");
            if (this._configurationMgmtState == CMState.Committed)
            {
                int buildNr = this._trackedService.BuildNumber;
                string tagName = GetFeatureTagName();

                // Either create new release ticket, or link to existing one.
                // We don't use the release-version for the time being since services are released by build number and you can have many builds
                // for a single service. For now, we simply organise release tickets by service ticket and version = 1.
                RMReleaseTicket releaseTicket = new RMReleaseTicket(this._ticket);
                Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.ReleaseService >> Created release ticket '" + releaseTicket.GetQualifiedID() + "'...");

                if (!this._snapshotCreated)
                {
                    Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.ReleaseService >> We have not yet committed a snapshot...");         
                    CreateSnapshot();
                    this._repository.CommitStagingArea("Created release with tag '" + tagName + "'.", false);
                    this._snapshotCreated = true;
                }

                Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.ReleaseService >> Merging and Pushing to remote with tag '" +
                                 tagName + "' and message '" + message + "'...");
                // Make sure that our branch is up to date before attempting to merge...
                this._repository.GotoBranch(this._branchName);
                this._repository.Pull();
                this._repository.Merge(this._branchName, this._repository.ReleaseBranchName);
                this._repository.Push(tagName, message);

                // Since the service is now released, we can safely increment the build number, reset the branch name and checkout master...
                this._trackedService.BuildNumber++;
                this._repository.GotoBranch(this._repository.ReleaseBranchName);
                this._repository.RemoveBranch(this._branchName, this._repository.ReleaseBranchName);
                this._branchName = string.Empty;
                this._trackedService.ServiceClass.SetTag(ContextSlt.GetContextSlt().GetConfigProperty(_CMBranchTag), string.Empty);
                UpdateCMState(CMState.Released);
            }
            else
            {
                string errMsg = "Service '" + this._trackedService.Name + "' must be committed before it can be released!";
                Logger.WriteError("Plugin.Application.ConfigurationManagement.CMContext.ReleaseService >> " + errMsg);
                throw new InvalidOperationException(errMsg);
            }
        }

        /// <summary>
        /// Restores a service to the release specified by 'tagName'. When makeNewVersion is 'true', we will create a new major version of 
        /// the service in a new package, thus leaving the existing version alone. When makeNewVersion is 'false', we will overwrite te requested
        /// service with the state at the specified release.
        /// Since the specified tag does not necessarily refer to our current service, we consider the entire container when looking for a 
        /// version to import and/or overwrite.
        /// Note that the operation can not update the major/minor/build version numbers in the imported model since this requires a
        /// capability model tree that we can not construct at this time! We DO set the major- and minor version in the Service Class
        /// to the values determined by the import.
        /// </summary>
        /// <param name="tagName">The state to restore.</param>
        /// <param name="makeNewVersion">Indicates whether we should create an entire new version of the service.</param>
        /// <returns>The newly imported service class.</returns>
        /// <exception cref="ArgumentException">Is thrown on import errors.</exception>
        internal MEClass RevertService(string tagName, bool makeNewVersion)
        {
            Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.RevertService >> Reverting to tag '" + tagName +
                             "' with a 'new version' indicator of '" + makeNewVersion + "'...");
            SetupDirectories();    // Make sure that we have the appropriate pointers to the necessary directories...
            ContextSlt context = ContextSlt.GetContextSlt();
            string tempBranch = null;
            string servicePkgStereotype = ContextSlt.GetContextSlt().GetConfigProperty(_ServiceDeclPkgStereotype);
            MEPackage containerPkg = this._trackedService.ContainerPkg;
            MEClass importedClass = null;
            Tuple<int,int,int> tagVersion = null;

            try
            {
                tagVersion = ParseTagVersion(this._repository.GetTag(tagName)); // Extract major/minor/build version numbers from tag.
                tempBranch = this._repository.CheckoutTag(tagName);             // Retrieve context from CM on temporary branch.

                // First of all, fetch a list of all existing versions of this particular service. We need this either to determine a new
                // major version, or to find the package to replace in case the tag does not address our current service...
                List<MEPackage> svcList = containerPkg.FindPackages(this._trackedService.Name, servicePkgStereotype);

                // Now, check whether we must make a new version. This is the simple case since we can ignore the current context!
                if (makeNewVersion)
                {
                    // Since the list is ordered ascending by name, we can take the last entry to get the current highest version.
                    string version = svcList[svcList.Count - 1].Name.Substring(svcList[svcList.Count - 1].Name.LastIndexOf("_V") + 2);
                    int versionNr;
                    if (int.TryParse(version, out versionNr))
                    {
                        versionNr++;
                        Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.RevertService >> Going to create new version '" +
                                         versionNr + "', importing from version '" + tagVersion.Item1 + "'...");
                        Tuple<int, int> newVersion = new Tuple<int, int>(versionNr, 0);
                        importedClass = ImportSnapshot(newVersion, null, tagVersion.Item1);

                        // Make sure to set the context right! Remove old branches and correctly set the state to 'created'...
                        importedClass.SetTag(context.GetConfigProperty(_CMBranchTag), string.Empty);
                        importedClass.SetTag(context.GetConfigProperty(_CMStateTag), EnumConversions<CMState>.EnumToString(CMState.Created));
                    }
                    else
                    {
                        string msg = "Unable to determine new snapshot version from name '" + svcList[svcList.Count - 1].Name + "'!";
                        Logger.WriteError("Plugin.Application.ConfigurationManagement.CMContext.RevertService >> " + msg);
                        throw new ArgumentException(msg);
                    }
                }
                else // Restore exact snapshot version
                {
                    MEPackage revertPackage = null;
                    if (this._trackedService.MajorVersion != tagVersion.Item1)
                    {
                        // My version does not match the required version, search for Service package...
                        foreach (MEPackage pkg in svcList)
                        {
                            string version = pkg.Name.Substring(pkg.Name.LastIndexOf("_V") + 2);
                            if (tagVersion.Item1.ToString() == version)
                            {
                                Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.RevertService >> Found appropriate package '" + pkg.Name + "'...");
                                revertPackage = pkg;
                                break;
                            }
                        }
                    }
                    else revertPackage = this._trackedService.DeclarationPkg;   // Major versions match, use our own service.

                    // Now we're importing the snapshot in 'revertPackage'. Note that, if we could not find the requested package, the reference will
                    // be NULL, which would result in an entirely new version of the service to be created with the selected exact major/minor version nrs.
                    importedClass = ImportSnapshot(new Tuple<int, int>(tagVersion.Item1, tagVersion.Item2), revertPackage, tagVersion.Item1);

                    // Make sure to set the context right! Remove old branches and (for existing services), set the state to 'released'...
                    importedClass.SetTag(context.GetConfigProperty(_CMBranchTag), string.Empty);
                    importedClass.SetTag(context.GetConfigProperty(_CMStateTag), EnumConversions<CMState>.EnumToString(CMState.Released));

                    // We have now restored the UML model of the service to the state it had at the moment of release.
                    // We also have to recover what's currently in the temporary path as build files. Can't use simple merge/revert etc. operations since these would
                    // delete files from other services and probably cause a whole lot of merge conflicts. Instead, we simply save the contents of the working directory
                    // in a temporary archive and restore this after going back to the main branch...
                    string archive = SaveFiles(tagVersion.Item1);

                    // Now that we have recovered this 'old' version, we must remove all equal- and more recent tags since these are not valid anymore!
                    // Since we leave the associated commits alone, you could be able to retrieve the associated snapshots and/or other service
                    // contents if required. We do have to delete the tags since these would cause naming conflicts when we want to re-release the
                    // recovered service!
                    foreach (Tag tag in GetReleaseTags())
                    {
                        Tuple<int, int, int> thisVersion = ParseTagVersion(tag);
                        if (thisVersion.Item1 != tagVersion.Item1) continue;        // Different major version, ignore!
                        if ((thisVersion.Item2 > tagVersion.Item2) || (thisVersion.Item2 == tagVersion.Item2 && thisVersion.Item3 >= tagVersion.Item3))
                        {
                            Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.RevertService >> Deleting more recent release tag '" + tag.FriendlyName + "'...");
                            this._repository.DeleteTag(tag);
                        }
                    }

                    // Finally, we merge the state of the Detached Head back into master in order to restore the contents of the folders to the
                    // original state and then delete the temporary branch (the latter will be covered by the 'finally' clause).
                    //this._repository.Merge(tempBranch, this._repository.ReleaseBranchName);
                    this._repository.RemoveBranch(tempBranch, this._repository.ReleaseBranchName);
                    RestoreFiles(archive, tagVersion.Item1);
                }
            }
            finally
            {
                // get rid of the temporary branch 
                if (tempBranch != null) this._repository.RemoveBranch(tempBranch, this._repository.ReleaseBranchName, true);
            }
            Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.RevertService >> Returning new service class '" + importedClass.Name + "'...");
            return importedClass;
        }

        /// <summary>
        /// Helper function that takes a Service Class and checks whether the Configuration Management state of the class allows updates
        /// on the service metamodel. When CM is disabled, this is always allowed. Otherwise, the Service CM state must either be Created,
        /// Checked-Out or Modified.
        /// </summary>
        /// <param name="serviceClass">Service to be checked.</param>
        /// <returns>True in case the meta model is allowed to change, false otherwise.</returns>
        internal static bool UpdateAllowed(MEClass serviceClass)
        {
            string CMStateTagName = ContextSlt.GetContextSlt().GetConfigProperty(_CMStateTag);
            bool isAllowed = false;
            RepositoryDescriptor repoDsc = CMRepositoryDscManagerSlt.GetRepositoryDscManagerSlt().GetCurrentDescriptor();
            if (repoDsc != null)
            {
                if (repoDsc.IsCMEnabled)
                {
                    string configMgmtState = serviceClass.GetTag(CMStateTagName);
                    if (!string.IsNullOrEmpty(configMgmtState))
                    {
                        CMState state = EnumConversions<CMState>.StringToEnum(configMgmtState);
                        isAllowed = (state == CMState.CheckedOut || state == CMState.Created || state == CMState.Modified);
                    }
                    else isAllowed = true;
                }
                else isAllowed = true;
            }
            return isAllowed;
        }

        /// <summary>
        /// This function is called after a (possible) change of a service version. A new version will have impact on the
        /// CM branch, since these are version dependent. Therefor, we check whether a branch according to the current service
        /// version is indeed different from the currently registered branch. If so, the current CM state determines whether
        /// it is allowed to switch branches. This is allowed only when Committed or CheckedOut (in Released state, we don't
        /// have a current branch). All other states will result in an InvalidOperation exception.
        /// </summary>
        /// <exception cref="InvalidOperationException">Is thrown when the current CM state does not support branch switching.</exception>
        internal void UpdateVersion()
        {
            if (!this._repository.IsCMEnabled) return;

            string CMBranchTagName = ContextSlt.GetContextSlt().GetConfigProperty(_CMBranchTag);
            string newBranchName = GetFeatureBranchName();

            // If we don't have a branch yet, or if the new branch matches the current branch, do nothing. 
            // Else, we have to check whether we can switch branches...
            if (!string.IsNullOrEmpty(this._branchName) && newBranchName != this._branchName)
            {
                if (this._configurationMgmtState == CMState.Created   ||
                    this._configurationMgmtState == CMState.Committed || 
                    this._configurationMgmtState == CMState.CheckedOut)
                {
                    // If we are in created-, committed- or checked-out state (meaning: no changes made yet), we remove the 
                    // existing branch and create a new one that matches the new version. In 'released' state we should not 
                    // have a branch and in all other cases we can not make changes and thus will throw an InvalidOperation exception...
                    this._repository.GotoBranch(this._repository.ReleaseBranchName);
                    this._repository.RemoveBranch(this._branchName, this._repository.ReleaseBranchName);
                    this._repository.SetBranch(newBranchName, this._repository.ReleaseBranchName);
                    this._trackedService.ServiceClass.SetTag(CMBranchTagName, newBranchName);
                    this._branchName = newBranchName;
                }
                else
                {
                    string msg = "Service is in wrong CM state to support a version change!";
                    Logger.WriteError("Plugin.Application.ConfigurationManagement.CMContext.UpdateVersion >> " + msg);
                    throw new InvalidOperationException(msg);
                }
            }
        }

        /// <summary>
        /// We create a compressed XMI snapshot of the service model so we can reconstruct this exact release when required...
        /// This file is originally created in the TEMP directory and subsequently moved to its destination. We do this because Sparx EA does not
        /// release the folder handle, thus breaking future GIT merges.
        /// The snapshot is added to te GIT index but not committed since this depends on further context.
        /// </summary>
        private void CreateSnapshot()
        {
            Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.CreateSnapshot >> Create a snapshot of the service...");
            ContextSlt context = ContextSlt.GetContextSlt();

            string snapshotName = Path.GetTempPath() + this._trackedService.Name + context.GetConfigProperty(_XMIFileSuffix);
            this._trackedService.DeclarationPkg.ExportPackage(snapshotName);    // Create the snapshot.
            string zipFile = Compression.FileZip(snapshotName, true);           // Compress generated XMI and delete original XMI (huge savings in size). 

            SetupDirectories();                                                 // Make sure we have a place to store the snapshot.
            this._repository.Pull();  

            string destFile = this._snapshotPath + "/" + this._trackedService.Name + context.GetConfigProperty(_CompressedFileSuffix);
            File.Copy(zipFile, destFile, true);                                 // Copy instead of move, since this allows overwrite existing files!
            File.Delete(zipFile);
            this._repository.AddToStagingArea(this._commitPath);                // Update our staging area (should include the snapshot).
            this._snapshotCreated = true;
        }

        /// <summary>
        /// Helper function that creates a feature branch name using the current service state and ticket.
        /// The created name has format:
        /// prefix/ticket-id/business-funct.containerpkg/service-name[_operational-state]_Vmajor-vsnPminor-vsn
        /// </summary>
        /// <returns>Branch name.</returns>
        private string GetFeatureBranchName()
        {
            string branchName = string.Empty;
            string prefix = ContextSlt.GetContextSlt().GetConfigProperty(_CMFeatureBranchPrefix) + "/";
            string operationalState = this._trackedService.NonDefaultOperationalState; // Returns eiter empty string or non-default value as a string.

            branchName = prefix + this._ticket.ID + "/" + this._trackedService.BusinessFunctionID + "." + this._trackedService.ContainerPkg.Name + "/";
            branchName += (operationalState == string.Empty) ? this._trackedService.Name : this._trackedService.Name + "_" + operationalState;
            branchName += "_V" + this._trackedService.Version.Item1.ToString() + "P" + this._trackedService.Version.Item2;

            Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.GetFeatureBranchName >> Branch created: '" + branchName + "'.");
            return branchName;
        }

        /// <summary>
        /// Helper function that create a valid feature tag name based on current service state.
        /// The created name has format:
        /// prefix/ticket-id/business-funct.containerpkg/service-name[_operational-state]_Vmajor-vsnPminor-vsnBbuild-nr
        /// </summary>
        /// <returns>Tagname or empty string in case of missing ticket.</returns>
        private string GetFeatureTagName()
        {
            string tagName = GetFeatureBranchName() + "B" + this._trackedService.BuildNumber;
            Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.GetFeatureTagName >> Constructed tag '" + tagName + "'...");
            return tagName;
        }

        /// <summary>
        /// Returns a list of all release tags for this particular service, ordered by version in descending order (latest version first).
        /// </summary>
        /// <returns>List of all release tags (or empty list when no releases have been registered yet.</returns>
        private List<Tag> GetReleaseTags()
        {
            string operationalState = this._trackedService.NonDefaultOperationalState;
            string filter = this._trackedService.BusinessFunctionID + "." + this._trackedService.ContainerPkg.Name + "/";
            filter += (operationalState == string.Empty) ? this._trackedService.Name : this._trackedService.Name + "_" + operationalState;
            List<LibGit2Sharp.Tag> allTags = this._repository.GetTags(filter);
            allTags.Sort(SortByVersion);
            return allTags;
        }

        /// <summary>
        /// Either load an existing ticket or, when nothing found, create a new ticket with specified name and project Order.
        /// When projectOrderID has not been specified we expect to find an existing ticket. In this case, we throw an ArgumentException when 
        /// we can't find one. Also, when we DO find a ticket and the specified projectOrderID does not match the ticket, we will throw 
        /// an InvalidOperation.
        /// </summary>
        /// <param name="ticketName">Ticket identifier.</param>
        /// <param name="projectOrderID">Optional project order, required for creation of new tickets.</param>
        /// <returns>Either an existing ticket or a newly created one.</returns>
        /// <exception cref="ArgumentException">Thrown when we can't find the specified ticket and no project Order has been specified.</exception>
        /// <exception cref="InvalidOperationException">Thrown when a retrieved ticket does not match the specified projectOrderID or we can't
        /// find the ticket package.</exception>
        private RMServiceTicket GetTicket(string ticketName, string projectOrderID = null)
        {
            Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.GetTicket >> Retrieving ticket with ID '" + ticketName + "'...");
            ContextSlt context = ContextSlt.GetContextSlt();
            RMServiceTicket ticket = null;
            string ticketStereotype = context.GetConfigProperty(_RMTicketStereotype);
            string ticketPackageName = context.GetConfigProperty(_RMPackageName);
            MEPackage ticketPackageParent = this._trackedService.ServiceClass.OwningPackage.Parent;
            MEPackage ticketPackage = ticketPackageParent.FindPackage(ticketPackageName);
            if (ticketPackage != null)
            {
                MEClass ticketClass = ticketPackage.FindClass(ticketName, ticketStereotype);
                if (ticketClass != null)
                {
                    Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.GetTicket >> Existing ticket found!");
                    ticket = new RMServiceTicket(ticketClass, this._trackedService);
                    if (!string.IsNullOrEmpty(projectOrderID) && this._ticket.ProjectOrderID != projectOrderID)
                    {
                        string msg = "Ticket projectOrder '" + this._ticket.ProjectOrderID +
                                     "' does not match specified projectOrderID '" + projectOrderID + "'!";
                        Logger.WriteError("Plugin.Application.ConfigurationManagement.CMContext.GetTicket >> " + msg);
                        throw new InvalidOperationException(msg);
                    }
                }
                else if (!string.IsNullOrEmpty(projectOrderID))
                {
                    // No ticket (yet), create one...
                    ticket = new RMServiceTicket(ticketName, projectOrderID, this._trackedService);
                    Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.GetTicket >> Created new ticket.");
                }
                else
                {
                    string msg = "Ticket '" + ticketName + "' not found and insufficient information for creating a new one!";
                    Logger.WriteError("Plugin.Application.ConfigurationManagement.CMContext.GetTicket >> " + msg);
                    throw new ArgumentException(msg);
                }
            }
            else
            {
                string msg = "Unable to find ticket package!";
                Logger.WriteError("Plugin.Application.ConfigurationManagement.CMContext.GetTicket >> " + msg);
                throw new InvalidOperationException(msg);
            }
            return ticket;
        }

        /// <summary>
        /// Either imports a snapshot in the specified package (targetPkg is not NULL) OR import into a new package with
        /// major version specified by targetVersion. TargetVersion is thus ignored when a package is specified!
        /// If targetPkg is specified, the import will overwrite all contents!
        /// The snapshot can be found in the service 'snapshot' directory with the specified 'importVersion'.
        /// </summary>
        /// <param name="targetVersion">The version to be assigned to the imported service (only when targetPkg is not specified).</param>
        /// <param name="targetPkg">Where to import. If NULL, we must create a new package with version targetVersion.</param>
        /// <param name="importVersion">The major version that contains the snapshot to be imported.</param>
        /// <exception cref="ArgumentException">Is thrown when we could not import the package.</exception>
        /// <returns>The imported Service class with version set to requested version.</returns>
        private MEClass ImportSnapshot(Tuple<int,int> targetVersion, MEPackage targetPkg, int importVersion)
        {
            Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.ImportSnapshot >> Going to import snapshot in package '" +
                             (targetPkg == null ? "NEW" : targetPkg.Name) + "' from  version '" + importVersion +
                             "' and assign new version '" + targetVersion.Item1 + "." + targetVersion.Item2 + "'...");
            RepositoryDescriptor repoDsc = CMRepositoryDscManagerSlt.GetRepositoryDscManagerSlt().GetCurrentDescriptor();
            ContextSlt context = ContextSlt.GetContextSlt();
            string snapshotName = string.Empty;

            try
            {
                // We determine the current 'commit root' for our service (ends with [service-name]_V[version]). 
                // We have to adapt this to match the import version and determine the subdirectory where the snapshot lives...
                string artefactFolder = "/" + context.GetConfigProperty(_CMArtefactsFolderName);
                int folderIndex = this._trackedService.ServiceBuildPath.LastIndexOf(artefactFolder);
                string currentRoot = folderIndex >= 0 ? this._trackedService.ServiceBuildPath.Substring(0, folderIndex) : this._trackedService.ServiceBuildPath;
                string importPath = repoDsc.LocalRootPath + "/";
                importPath += currentRoot.Substring(0, currentRoot.LastIndexOf("_V")) + "_V" + importVersion + "/" + context.GetConfigProperty(_CMSnapshotsFolderName);
                Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.ImportSnapshot >> Importing from '" + importPath + "'...");

                string compressedSuffix = context.GetConfigProperty(_CompressedFileSuffix);
                string inflatedSuffix = context.GetConfigProperty(_XMIFileSuffix);
                string srcFile = importPath + "/" + this._trackedService.Name + compressedSuffix;
                snapshotName = srcFile.Substring(0, srcFile.LastIndexOf(compressedSuffix)) + inflatedSuffix;
                if (File.Exists(snapshotName)) File.Delete(snapshotName);
                Compression.FileUnzip(srcFile);
                Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.ImportSnapshot >> Restored snapshot '" + snapshotName + "'...");

                string importedPackageName = string.Empty;
                MEPackage importedPackage = null;
                MEPackage containerPkg = this._trackedService.ContainerPkg;

                if (targetPkg != null) // Ignore targetVersion here.
                {
                    Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.ImportSnapshot >> Importing to specified package '" +
                                     targetPkg.Name + "', deletes all contents...");
                    importedPackage = targetPkg;
                    importedPackageName = targetPkg.Name;
                    if (targetPkg.ImportPackage(snapshotName))
                    {
                        Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.ImportSnapshot >> Imported Ok!");
                    }
                    else
                    {
                        string msg = "Unable to import snapshot '" + snapshotName + "' into package '" + targetPkg.Name + "'!";
                        Logger.WriteError("Plugin.Application.ConfigurationManagement.CMContext.ImportSnapshot >> " + msg);
                        throw new ArgumentException(msg);
                    }
                }
                else   // Use major version from targetVersion.
                {
                    importedPackageName = this._trackedService.Name + "_V" + targetVersion.Item1;
                    Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.ImportSnapshot >> Importing to new package '" + importedPackageName + "'...");
                    string containerPkgStereotype = context.GetConfigProperty(_ServiceContainerPkgStereotype);
                    if (containerPkg.ImportPackage(snapshotName, containerPkg.Name, containerPkgStereotype, importedPackageName))
                    {
                        Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.ImportSnapshot >> Imported Ok!");
                    }
                    else
                    {
                        string msg = "Unable to import snapshot '" + snapshotName + "' into package '" + importedPackageName + "'!";
                        Logger.WriteError("Plugin.Application.ConfigurationManagement.CMContext.ImportSnapshot >> " + msg);
                        throw new ArgumentException(msg);
                    }
                }
                // Since all element id's might have changed due to the import, perform an explicit search for new packages and classes...
                importedPackage = containerPkg.FindPackage(importedPackageName);
                MEPackage servicePackage = importedPackage.FindPackage(context.GetConfigProperty(_ServiceModelPkgName));
                MEClass importedClass = servicePackage.FindClass(this._trackedService.Name, context.GetConfigProperty(_ServiceClassStereotype));
                importedClass.Version = targetVersion;
                return importedClass;
            }
            catch (Exception exc)
            {
                // If we get anything that is not an Argument Exception, catch an log and then 'translate' into an Argument Exception.
                if (exc is ArgumentException) throw;    // Probably one created by us, don't handle this here!
                else
                {
                    string msg = "Failed to import snapshot because: " + Environment.NewLine + exc.ToString();
                    Logger.WriteError("Plugin.Application.ConfigurationManagement.CMContext.ImportSnapshot >> " + msg);
                    throw new ArgumentException(msg, exc);
                }
            }
            finally
            {
                // Make sure we delete the uncompressed snapshot since we don't need it anymore...
                if (File.Exists(snapshotName)) File.Delete(snapshotName);
            }
        }

        /// <summary>
        /// Helper function that receives a tag and extracts the version information from it.
        /// </summary>
        /// <param name="tag">Tag to be parsed.</param>
        /// <returns>Version info as a triplet major,minor,build</returns>
        private static Tuple<int, int, int> ParseTagVersion(Tag tag)
        {
            int indexMajor = tag.FriendlyName.LastIndexOf("_V");
            int indexMinor = tag.FriendlyName.LastIndexOf('P');
            int indexBuild = tag.FriendlyName.LastIndexOf('B');

            int majorNr = Int32.Parse(tag.FriendlyName.Substring(indexMajor + 2, indexMinor - indexMajor - 2));
            int minorNr = Int32.Parse(tag.FriendlyName.Substring(indexMinor + 1, indexBuild - indexMinor - 1));
            int buildNr = Int32.Parse(tag.FriendlyName.Substring(indexBuild + 1));

            return new Tuple<int, int, int>(majorNr, minorNr, buildNr);
        }

        /// <summary>
        /// Restores the content of a service' build folders from an archive, previously created by 'SaveFiles'. The method deletes all current contents of the service working
        /// directory before restoring the archive. Next, the archive file is deleted.
        /// </summary>
        /// <param name="archivePath">Full pathname of archive file as returned from 'SaveFiles'.</param>
        /// <param name="majorVersion">The major service version in which we're going to restore the archive. This MUST match the version used to create the archive!</param>
        private void RestoreFiles(string archivePath, int majorVersion)
        {
            Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.RestoreFiles >> Going to restore contents from temporary archive '" + archivePath + "'...");
            RepositoryDescriptor repoDsc = CMRepositoryDscManagerSlt.GetRepositoryDscManagerSlt().GetCurrentDescriptor();
            ContextSlt context = ContextSlt.GetContextSlt();

            try
            {
                string artefactFolder = "/" + context.GetConfigProperty(_CMArtefactsFolderName);
                int folderIndex = this._trackedService.ServiceBuildPath.LastIndexOf(artefactFolder);
                string currentRoot = folderIndex >= 0 ? this._trackedService.ServiceBuildPath.Substring(0, folderIndex) : this._trackedService.ServiceBuildPath;
                string restorePath = repoDsc.LocalRootPath + "/";
                restorePath += currentRoot.Substring(0, currentRoot.LastIndexOf("_V")) + "_V" + majorVersion;
                Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.RestoreFiles >> Restoring to '" + restorePath + "'...");

                // First of all, remove ALL current contents from the build directory...
                foreach (string dirName in Directory.EnumerateDirectories(restorePath)) Directory.Delete(dirName, true);
                foreach (string fileName in Directory.EnumerateFiles(restorePath)) File.Delete(fileName);

                Compression.DirectoryUnzip(archivePath);
                File.Delete(archivePath);
            }
            catch (Exception exc)
            {
                // Restore errors are treated as Argument Exceptions.
                string msg = "Failed to restore build files for service '" + this._trackedService.Name + "' because: " + Environment.NewLine + exc.ToString();
                Logger.WriteError("Plugin.Application.ConfigurationManagement.CMContext.RestoreFiles >> " + msg);
                throw new ArgumentException(msg, exc);
            }
            finally
            {
                // Make sure we delete the uncompressed snapshot since we don't need it anymore...
                if (File.Exists(archivePath)) File.Delete(archivePath);
            }
        }

        /// <summary>
        /// Saves the contents of the build folders of the service to a temporary archive at container level. The method saves the folder tree indicated by 'majorVersion', which
        /// must exist at time of call. Since the archive is one level up from the actual CM working directory, it will not change when we perform a branch switch and is therefor
        /// suited to save the contents of a branch and restore as part of another branch.
        /// </summary>
        /// <returns>Full path to the created archive.</returns>
        private string SaveFiles(int majorVersion)
        {
            Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.SaveFiles >> Going to store current contents to temporary archive...");
            RepositoryDescriptor repoDsc = CMRepositoryDscManagerSlt.GetRepositoryDscManagerSlt().GetCurrentDescriptor();
            ContextSlt context = ContextSlt.GetContextSlt();
            string archive = string.Empty;

            try
            {
                string artefactFolder = "/" + context.GetConfigProperty(_CMArtefactsFolderName);
                int folderIndex = this._trackedService.ServiceBuildPath.LastIndexOf(artefactFolder);
                string currentRoot = folderIndex >= 0 ? this._trackedService.ServiceBuildPath.Substring(0, folderIndex) : this._trackedService.ServiceBuildPath;
                string savePath = repoDsc.LocalRootPath + "/";
                savePath += currentRoot.Substring(0, currentRoot.LastIndexOf("_V")) + "_V" + majorVersion;
                Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.SaveFiles >> Saving '" + savePath + "'...");
                archive = Compression.DirectoryZip(savePath);
                return archive;
            }
            catch (Exception exc)
            {
                if (File.Exists(archive)) File.Delete(archive);

                // Save errors are treated as Argument Exceptions.
                string msg = "Failed to save build files for service '" + this._trackedService.Name + "' because: " + Environment.NewLine + exc.ToString();
                Logger.WriteError("Plugin.Application.ConfigurationManagement.CMContext.SaveFiles >> " + msg);
                throw new ArgumentException(msg, exc);
            }
        }

        /// <summary>
        /// Helper function that is called in case we want to update/load our change ticket. This is only allowed in case of services that
        /// have their CM state disabled, have just been created or are released. In all other cases, changing the ticket might have unwanted 
        /// side-effects since branches and tag names depend on them.
        /// </summary>
        /// <param name="newTicket">The (new) ticket to load.</param>
        /// <exception cref="InvalidOperationException">Is thrown when the service is not in the correct state.</exception>
        private void SetTicket(RMServiceTicket newTicket)
        {
            Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.SetTicket >> Going to assign (new) ticket '" + newTicket.ID + "'...");
            if (this._ticket != null && this._ticket == newTicket) return; // No need to do anything if ticket is already a match!

            if (this._configurationMgmtState == CMState.Disabled ||
                this._configurationMgmtState == CMState.Created ||
                this._configurationMgmtState == CMState.Released)
            {
                ContextSlt context = ContextSlt.GetContextSlt();
                string branchTagName = context.GetConfigProperty(_CMBranchTag);
                this._ticket = newTicket;
                this._trackedService.ServiceClass.SetTag(context.GetConfigProperty(_RMTicketFQNTag), newTicket.GetQualifiedID(), true);

                // Only when our current state is 'released' do we allow a new feature branch to be created. In state 'created' or 'disabled',
                // this is postponed until we commit the changes into the repository.
                if (this._configurationMgmtState == CMState.Released)
                {
                    string newBranchName = GetFeatureBranchName();
                    if (!string.IsNullOrEmpty(this._branchName) && newBranchName != this._branchName)
                    {
                        this._repository.GotoBranch(this._repository.ReleaseBranchName);
                        this._repository.RemoveBranch(this._branchName, this._repository.ReleaseBranchName);
                        this._repository.SetBranch(newBranchName, this._repository.ReleaseBranchName);
                        this._trackedService.ServiceClass.SetTag(branchTagName, newBranchName, true);
                        this._branchName = newBranchName;
                    }
                    else if (string.IsNullOrEmpty(this._branchName))
                    {
                        this._repository.SetBranch(newBranchName, this._repository.ReleaseBranchName);
                        this._trackedService.ServiceClass.SetTag(branchTagName, newBranchName, true);
                        this._branchName = newBranchName;
                    }
                }
                else
                {
                    // In case of state disabled or created, we make sure that the branch tag is empty.
                    this._trackedService.ServiceClass.SetTag(branchTagName, string.Empty, true);
                }
            }
            else
            {
                string msg = "Service is in wrong CM state to support a ticket change!";
                Logger.WriteError("Plugin.Application.ConfigurationManagement.CMContext.SetTicket >> " + msg);
                throw new InvalidOperationException(msg);
            }
        }

        /// <summary>
        /// Helper function that assures that we have a pointer to our working directory for commits as well as a valid directory for the 
        /// creation of snapshots. We only create these 'on demand' when required.
        /// </summary>
        private void SetupDirectories()
        {
            RepositoryDescriptor repoDsc = CMRepositoryDscManagerSlt.GetRepositoryDscManagerSlt().GetCurrentDescriptor();
            ContextSlt context = ContextSlt.GetContextSlt();

            if (this._commitPath == null)
            {
                string artefactFolder = "/" + context.GetConfigProperty(_CMArtefactsFolderName);
                int folderIndex = this._trackedService.ServiceBuildPath.LastIndexOf(artefactFolder);
                this._commitPath = folderIndex >= 0 ? this._trackedService.ServiceBuildPath.Substring(0, folderIndex) : this._trackedService.ServiceBuildPath;
            }

            if (this._snapshotPath == null)
            {
                this._snapshotPath = repoDsc.LocalRootPath + "/" + this._commitPath + "/" + context.GetConfigProperty(_CMSnapshotsFolderName);
                if (!Directory.Exists(this._snapshotPath)) Directory.CreateDirectory(this._snapshotPath);
            }
        }

        /// <summary>
        /// Helper function that sorts two tags based on version info in descending order (highest version first). 
        /// It returns 1 if tagA has a lower version ten tagB, -1 if tagA has a higher version then tagB and 0 if versions are equal.
        /// The function does NOT check the remainder of the tag (e.g. the actual name is ignored).
        /// </summary>
        /// <param name="tagA">First tag to compare</param>
        /// <param name="tagB">Second tag to compare</param>
        /// <returns>-1 if tagA higher tagB, +1 if tagA lower tagB, 0 if tags are equal</returns>
        private static int SortByVersion(Tag tagA, Tag tagB)
        {
            Tuple<int, int, int> versionA = ParseTagVersion(tagA);
            Tuple<int, int, int> versionB = ParseTagVersion(tagB);

            if (versionA.Item1 < versionB.Item1) return 1;
            else if (versionA.Item1 > versionB.Item1) return -1;
            else
            {
                // Major versions identical, check minor versions...
                if (versionA.Item2 < versionB.Item2) return 1;
                else if (versionA.Item2 > versionB.Item2) return -1;
                else
                {
                    // Major and minor versions identical, check build number...
                    if (versionA.Item3 < versionB.Item3) return 1;
                    else if (versionA.Item3 > versionB.Item3) return -1;
                    else return 0;
                }
            }
        }

        /// <summary>
        /// Updates the CM state of the tracked service to the provided state. The function verifies whether the transition is allowed
        /// given the current state and if not, throws an Argument Exception.
        /// </summary>
        /// <param name="newState">New state.</param>
        /// <exception cref="ArgumentException">Thrown in case of an invalid transition.</exception>
        private void UpdateCMState(CMState newState)
        {
            if (this._configurationMgmtState != newState)
            {
                if (IsValidTransition(newState))
                {
                    this._trackedService.ServiceClass.SetTag(ContextSlt.GetContextSlt().GetConfigProperty(_CMStateTag),
                                                             EnumConversions<CMState>.EnumToString(newState), true);
                    this._configurationMgmtState = newState;
                }
                else
                {
                    string msg = "Transition from '" + this._configurationMgmtState.ToString() + "' to '" + newState.ToString() + "' is not allowed!";
                    Logger.WriteError("Plugin.Application.ConfigurationManagement.CMContext.UpdateCMState >> " + msg);
                    throw new ArgumentException(msg);
                }
            }
        }
    }
}
