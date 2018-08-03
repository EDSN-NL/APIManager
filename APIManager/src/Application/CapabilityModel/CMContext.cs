using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using Framework.Context;
using Framework.Logging;
using Framework.Model;
using Framework.Util;
using Framework.ConfigurationManagement;

namespace Plugin.Application.CapabilityModel
{
    /// <summary>
    /// Each Service has its own CMContext object. The context provides the interface between the service and configuration management
    /// by encapsulating a service-specific local- and remote repository path.
    /// The CMContext class provides all interfacing between the service and configuration management.
    /// Note that this context class is used ONLY when configuration management is enabled.
    /// </summary>
    sealed internal class CMContext
    {
        // Private configuration properties used by this service...
        private const string _CMBranchTag               = "CMBranchTag";
        private const string _CMStateTag                = "CMStateTag";
        private const string _XMIFileSuffix             = "XMIFileSuffix";
        private const string _CompressedFileSuffix      = "CompressedFileSuffix";

        // Some constants...
        private const string _NoRemoteBranch            = "reference was not fetched";
        private const string _NoTrackingRemoteBranch    = "no tracking information";

        private CMRepositorySlt _repository;            // Repository instance (provides interface with remote as well).
        private Service _trackedService;                // The Service instance for which we perform configuration management.
        private string _branchName;                     // The name of the branch that we're currently using to track our service.
        private bool _snapshotCreated;                  // Used to keep track of snapshot creation in combination with auto-release.

        /// <summary>
        /// Returns 'true' when configuration management is enabled for this session.
        /// </summary>
        internal bool CMEnabled { get { return CMRepositorySlt.GetRepositorySlt().IsCMEnabled; } }

        /// <summary>
        /// Constructor, receives the Service object that we want to manage and loads service-specific configuration. The constructor performs
        /// only minimal initialization. Before actually using the repository, one must invoke the 'CheckoutService' operation in order to 
        /// assure that the repository is in the correct state for performing necessary operations.
        /// </summary>
        /// <param name="trackedService">Service under configuration management.</param>
        internal CMContext(Service trackedService)
        {
            Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext >> Creating context for service '" + trackedService.Name + "'...");
            ContextSlt context = ContextSlt.GetContextSlt();
            this._repository = CMRepositorySlt.GetRepositorySlt();
            this._trackedService = trackedService;
            this._snapshotCreated = false;

            if (this._repository.IsCMEnabled)
            {
                // We don't do this anymore. Instead, we always use the User and E-Mail address from the CM Repository Descriptor since
                // this acurately identifies the user working with te CM environment at this time.
                // this._repository.SetIdentity(trackedService.ServiceClass.Author, context.GetStringSetting(FrameworkSettings._GLEMail));
                InitializeBranch();
            }
        }

        /// <summary>
        /// Commits all files that are in the service build path to the repository at the HEAD of the current branch. 
        /// First of all, we instruct the repository to add all files in the current build path to the staging area. If there is nothing
        /// to commit, we issue a warning message to the log to indicate that no operation has been performed.
        /// The files will be pushed to the remote repository on the current path, but without a tag.
        /// </summary>
        /// <param name="message">Commit reporting message.</param>
        /// <param name="autoRelease">When set to 'true', we release the service directly after commit.</param>
        /// <exception cref="InvalidOperationException">Is thrown when configuration settings are missing or identity has not been initialized yet or the
        /// service is not in the correct state to be committed.</exception>
        internal void CommitService(string message, bool autoRelease)
        {
            if (!this._repository.IsCMEnabled) return;

            if (this._trackedService.ConfigurationMgmtState == CMState.Modified)
            {
                this._snapshotCreated = false;  // We reset this on each commit since obviously the service state has changed (hence the new commit).
                if (autoRelease)
                {
                    Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.CommitService >> Autorelease enabled, create snapshot...");
                    CreateSnapshot();
                }

                Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.CommitService >> Committing service with message '" + message + "'...");
                this._repository.SetRootBranch(this._branchName);
                this._repository.AddToStagingArea(this._trackedService.ServiceBuildPath);
                this._repository.CommitStagingArea(message);
                this._trackedService.ConfigurationMgmtState = CMState.Committed;

                if (autoRelease)
                {
                    Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.CommitService >> Autorelease enabled, release service...");
                    ReleaseService(message);
                }
            }
            else
            {
                string errMsg = "Plugin.Application.ConfigurationManagement.CMContext.CommitService >> Service '" + this._trackedService.Name + "' must be modified before it can be committed!";
                throw new InvalidOperationException(errMsg);
            }
        }

        /// <summary>
        /// This method must be called when we want to start working with a service. The current branch for the service is determined and if this is found to 
        /// be new, a new branch is created in the repository. The new/existing branch is then checked-out so work on the service will commence on that branch. 
        /// Before doing all this, we perform a 'pull' on the remote (from master) to assure that we are in sync. 
        /// If the pull resulted in a 'reference not fetched' exception, this indicates that we have a new local branch that does not yet exist at remote. We
        /// create the branch at remote to assure other developers are aware of it.
        /// Depending on the context and the current CM state of the service, a new service CM state is determined and written to the service.
        /// When CM is disabled, the method does not perform any operation.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when we could not checkout the service (i.e. due to merge conflicts or syncronisation errors).</exception>
        internal void CheckoutService()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            CMState cmState = this._trackedService.ConfigurationMgmtState;

            // Nothing to do if CM not active...
            if (!this._repository.IsCMEnabled) return;

            try
            {
                this._repository.ResetBranch();                     // Make sure we're on 'master'.
                this._repository.Pull();                            // Synchronise repository (might affect our service).

                // Now that the repository is in sync with remote, check whether we lag behind with our build number...
                Tuple<int,int,int> lastReleased = GetLastReleasedVersion();
                bool buildAdjusted = false;
                if (lastReleased.Item1 > 0)
                {
                    Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.CheckoutService >> We have an existing release: '" +
                                     lastReleased.Item1 + "." + lastReleased.Item2 + "." + lastReleased.Item3 + "'...");
                    Tuple<int, int> svcVersion = this._trackedService.Version;
                    if (svcVersion.Item1 == lastReleased.Item1 && svcVersion.Item2 == lastReleased.Item2 && this._trackedService.BuildNumber <= lastReleased.Item3)
                    {
                        Logger.WriteWarning("Service '" + this._trackedService.Name + "' has a build number '" + this._trackedService.BuildNumber + 
                                            "', which does not match last released number '" + lastReleased.Item3 + "'; service updated!");
                        this._trackedService.BuildNumber = lastReleased.Item3 + 1;
                        buildAdjusted = true;
                    }
                }

                // Note that, if we had a 'cached' branchname and had to adjust the build number, the service might be out of sync so we issue a warning.
                string CMBranchTagName = context.GetConfigProperty(_CMBranchTag);
                this._branchName = this._trackedService.ServiceClass.GetTag(CMBranchTagName);
                if (string.IsNullOrEmpty(this._branchName))
                {
                    string operationalStatus = this._trackedService.IsDefaultOperationalStatus ? string.Empty : _trackedService.OperationalStatus;
                    this._branchName = this._trackedService.BusinessFunctionID + "." + this._trackedService.ContainerPkg.Name + "_";
                    this._branchName += (operationalStatus == string.Empty) ? this._trackedService.Name + "_V" + this._trackedService.Version.Item1.ToString() :
                                                                              this._trackedService.Name + "_" + operationalStatus + "_V" + this._trackedService.Version.Item1.ToString();
                    this._branchName += "P" + this._trackedService.Version.Item2 + "B" + this._trackedService.BuildNumber;
                    Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.CheckoutService >> Branch created: '" + this._branchName + "'.");
                    this._trackedService.ServiceClass.SetTag(CMBranchTagName, this._branchName, true);
                }
                else if (buildAdjusted)
                {
                    Logger.WriteWarning("Cached branch lags behind last release, please verify consistency!");
                }

                this._repository.SetRootBranch(this._branchName);
            }
            catch (Exception exc)
            {
                if (this._branchName != string.Empty)
                {
                    if (exc.Message.Contains(_NoRemoteBranch) || exc.Message.Contains(_NoTrackingRemoteBranch))
                    {
                        Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.CheckoutService >> Pulled branch '" + this._branchName +
                                         "' does not (yet) exist at remote, creating one...");
                        this._repository.PushBranch(this._branchName);
                    }
                    else throw;
                }
            }

            // Determine the (new) CM state:
            // If the state used to be 'Disabled', the new state will be 'Created'.
            // If the service is in CM (Committed or Released), the new state will be 'CheckedOut'.
            // If the state is 'Created', 'Modified' or already 'CheckedOut', the state will not be changed.
            CMState newState = cmState;
            if (cmState == CMState.Disabled) newState = CMState.Created;
            else if (cmState == CMState.Committed || cmState == CMState.Released) newState = CMState.CheckedOut;
            Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.CheckoutService >> Service CM state set to '" + newState + "'.");
            if (newState != cmState) this._trackedService.ConfigurationMgmtState = newState;
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
            string tagName = this._trackedService.BusinessFunctionID + "." + this._trackedService.ContainerPkg.Name + "." + this._trackedService.Name;
            if (!this._trackedService.IsDefaultOperationalStatus) tagName += this._trackedService.OperationalStatus;
            tagName += "_V";

            try
            {
                List<string> myTags = this._repository.GetTags(tagName);
                if (myTags.Count > 0)
                {
                    string recentTag = myTags[myTags.Count - 1].Substring(tagName.Length);
                    Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.GetLastReleasedVersion >> Got tag '" + recentTag + "'...");
                    int indexMinor = recentTag.IndexOf('P');
                    int indexBuild = recentTag.IndexOf('B');
                    string major = recentTag.Substring(0, indexMinor);
                    string minor = recentTag.Substring(indexMinor + 1, indexBuild - indexMinor - 1);
                    string build = recentTag.Substring(indexBuild + 1);
                    Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.GetLastReleasedVersion >> Returning major: " +
                                     major + ", minor: " + minor + " and build: " + build);

                    int majorNr = Int32.Parse(major);
                    int minorNr = Int32.Parse(minor);
                    int buildNr = Int32.Parse(build);
                    return new Tuple<int, int, int>(majorNr, minorNr, buildNr);
                }
                else
                {
                    Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.GetLastReleasedVersion >> No earlier release found!");
                    return new Tuple<int, int, int>(0, 0, 0);
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Plugin.Application.ConfigurationManagement.CMContext.GetLastReleasedVersion >> Unable to retrieve tags because: " +
                                  Environment.NewLine + exc.Message);
            }
            return null;
        }

        /// <summary>
        /// Returns a list of all release tags for this particular service.
        /// </summary>
        /// <returns>List of all release tags (or empty list when no releases have been registered yet.</returns>
        internal List<string> GetReleaseTags()
        {
            string tagName = this._trackedService.BusinessFunctionID + "." + this._trackedService.ContainerPkg.Name + "." + this._trackedService.Name;
            if (!this._trackedService.IsDefaultOperationalStatus) tagName += this._trackedService.OperationalStatus;
            tagName += "_V";
            return this._repository.GetTags(tagName);
        }

        /// <summary>
        /// Pushes all Service Configuration Items for the local service to the appropriate branch on the remote repository. 
        /// Each release is accompanied by a Tag that provide additional info regarding the release. 
        /// The Tag is named according to the release: Fully-Qualified-Service-Name-[OperationalState-]VxPyBz.
        /// Example: 3010.01.03.01.MyContainer.MyService_V1P1B4 --> Version 1.1 Build 4 of 'MyService'.
        /// Note that for each release, the build number is incremented. This also assures that the tag names remain unique within the repository.
        /// </summary>
        /// <param name="message">Release reporting message.</param>
        /// <exception cref="InvalidOperationException">Thrown when the service is not in the correct state to be released.</exception>
        internal void ReleaseService(string message)
        {
            if (!this._repository.IsCMEnabled) return;

            Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.ReleaseService >> Releasing Service to remote with message '" + message + "'...");
            if (this._trackedService.ConfigurationMgmtState == CMState.Committed)
            {
                int buildNr = this._trackedService.BuildNumber;
                string tagName = this._trackedService.BusinessFunctionID + "." + this._trackedService.ContainerPkg.Name + "." + this._trackedService.Name + "_";
                if (!this._trackedService.IsDefaultOperationalStatus) tagName += this._trackedService.OperationalStatus + "-";
                tagName += "V" + this._trackedService.Version.Item1 + "P" + this._trackedService.Version.Item2 + "B" + buildNr;

                if (!this._snapshotCreated)
                {
                    Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.ReleaseService >> We still have to create a snapshot...");
                    CreateSnapshot();
                    this._repository.CommitStagingArea(message);
                }

                Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.ReleaseService >> Merging and Pushing to remote with tag '" +
                                 tagName + "' and message '" + message + "'...");
                this._repository.Merge(this._branchName);
                this._repository.Push(tagName, message);

                // Since the service is now released, we can safely increment the build number and reset the branch name...
                Logger.WriteInfo("Framework.ConfigurationManagement.CMContext.ReleaseService >> Branch set to empty!");
                this._repository.ResetBranch(this._branchName);
                this._branchName = string.Empty;
                this._trackedService.ServiceClass.SetTag(ContextSlt.GetContextSlt().GetConfigProperty(_CMBranchTag), string.Empty);
                this._trackedService.ConfigurationMgmtState = CMState.Released;
                this._trackedService.BuildNumber++;
            }
            else
            {
                string errMsg = "Plugin.Application.ConfigurationManagement.CMContext.ReleaseService >> Service '" + this._trackedService.Name + "' must be committed before it can be released!";
                throw new InvalidOperationException(errMsg);
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
            string destFile = this._trackedService.ServiceCIPath + "/" + this._trackedService.Name + context.GetConfigProperty(_CompressedFileSuffix);
            File.Copy(zipFile, destFile, true);                                 // Copy instead of move, since this allows overwrite existing files!
            File.Delete(zipFile);
            this._repository.AddToStagingArea(this._trackedService.ServiceBuildPath);   // Add compressed snapshot the the GIT Index.
            this._snapshotCreated = true;
        }

        /// <summary>
        /// Helper function that determines the configuration management state of the service and loads the branch name accordingly.
        /// This branch name is based on the fully qualified service name, major- and minor versions and 
        /// build number. The branch is used only for service processing and is merged when the service is released.
        /// When the current service CM state is disabled or released, we don't have a valid branch and thus the branch name is set 
        /// to an empty string.
        /// Note that the operation ONLY determines the branch name but does NOT (yet) creates the branch within the repository!
        /// The branch is ONLY created when a service is explicitly checked-out (i.e. prepared for processing).
        /// </summary>
        private void InitializeBranch()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string CMBranchTagName = context.GetConfigProperty(_CMBranchTag);
            CMState cmState = this._trackedService.ConfigurationMgmtState;

            if (this._repository.IsCMEnabled && cmState != CMState.Disabled && cmState != CMState.Released)
            {
                this._branchName = this._trackedService.ServiceClass.GetTag(CMBranchTagName);
                if (string.IsNullOrEmpty(this._branchName))
                {
                    string operationalStatus = this._trackedService.IsDefaultOperationalStatus ? string.Empty : _trackedService.OperationalStatus;
                    this._branchName = this._trackedService.BusinessFunctionID + "." + this._trackedService.ContainerPkg.Name + "_";
                    this._branchName += (operationalStatus == string.Empty) ? this._trackedService.Name + "_V" + this._trackedService.Version.Item1.ToString() :
                                                                              this._trackedService.Name + "_" + operationalStatus + "_V" + this._trackedService.Version.Item1.ToString();
                    this._branchName += "P" + this._trackedService.Version.Item2 + "B" + this._trackedService.BuildNumber;
                    this._trackedService.ServiceClass.SetTag(CMBranchTagName, this._branchName, true);
                    Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.InitializeBranch >> Branch set to: '" + this._branchName + "'.");
                }
            }
            else
            {
                // When configuration management is disabled or released, clear the branch name...
                Logger.WriteInfo("Plugin.Application.ConfigurationManagement.CMContext.InitializeBranch >> Branch set to empty!");
                this._branchName = string.Empty;
                this._trackedService.ServiceClass.SetTag(CMBranchTagName, string.Empty);
            }
        }
    }
}
