using System;
using Framework.Context;
using Framework.Logging;
using Plugin.Application.CapabilityModel;

namespace Plugin.Application.ConfigurationManagement
{
    /// <summary>
    /// Each Service has its own CMContext object. The context provides the interface between the service and configuration management
    /// by encapsulating a service-specific local- and remote repository path.
    /// The CMContext class provides all interfacing between the service and configuration management.
    /// Note that this context class is used ONLY when configuration management is enabled.
    /// The class supports the following flow:
    /// 1) Create new service --> 
    /// </summary>
    sealed internal class CMContext
    {
        // Private configuration properties used by this service...
        private const string _CMBranchTag               = "CMBranchTag";

        // Some constants...
        private const string _NoRemoteBranch            = "reference was not fetched";
        private const string _NoTrackingRemoteBranch    = "no tracking information";

        private CMRepositorySlt _repository;            // Repository instance (provides interface with remote as well).
        private Service _trackedService;                // The Service instance for which we perform configuration management.
        private string _branchName;                     // The name of the branch that we're currently using to track our service.

        /// <summary>
        /// Constructor, receives the Service object that we want to manage and loads service-specific configuration. The constructor performs
        /// only minimal initialization. Before actually using the repository, one must invoke the 'CheckoutService' operation in order to 
        /// assure that the repository is in the correct state for performing necessary operations.
        /// </summary>
        /// <param name="trackedService">Service under configuration management.</param>
        /// <exception cref="InvalidOperationException">Thrown if unable to properly initialize the repository.</exception>
        internal CMContext(Service trackedService)
        {
            Logger.WriteInfo("Framework.ConfigurationManagement.CMContext >> Creating context for service '" + trackedService.Name + "'...");
            ContextSlt context = ContextSlt.GetContextSlt();
            this._trackedService = trackedService;
            this._repository = CMRepositorySlt.GetRepositorySlt();
            if (this._repository.IsCMEnabled)
            {
                this._repository.SetIdentity(trackedService.ServiceClass.Author, context.GetStringSetting(FrameworkSettings._GLEMail));
                InitializeBranch();
            }
            else
            {
                string message = "Framework.ConfigurationManagement.CMContext >> Unable to properly initialize repository, aborting!";
                throw new InvalidOperationException(message);
            }
        }

        /// <summary>
        /// Commits all files that are in the service build path to the repository at the HEAD of the current branch. 
        /// First of all, we instruct the repository to add all files in the current build path to the staging area. If there is nothing
        /// to commit, we issue a warning message to the log to indicate that no operation has been performed.
        /// The files will be pushed to the remote repository on the current path, but without a tag.
        /// </summary>
        /// <param name="message">Commit reporting message.</param>
        /// <exception cref="InvalidOperationException">Is thrown when configuration settings are missing or identity has not been initialized yet or the
        /// service is not in the correct state to be committed.</exception>
        internal void CommitService(string message)
        {
            if (this._trackedService.ConfigurationMgmtState == CMState.Created ||
                this._trackedService.ConfigurationMgmtState == CMState.Modified)
            {
                Logger.WriteInfo("Framework.ConfigurationManagement.CMContext.CommitService >> Committing service with message '" + message + "'...");
                this._repository.SetRootBranch(this._branchName);
                this._repository.AddToStagingArea(this._trackedService.ServiceBuildPath);
                this._repository.CommitStagingArea(message);
                this._trackedService.ConfigurationMgmtState = CMState.Committed;
            }
            else
            {
                string errMsg = "Framework.ConfigurationManagement.CMContext.CommitService >> Service '" + this._trackedService.Name + "' must be created/modified before it can be committed!";
                throw new InvalidOperationException(errMsg);
            }
        }

        /// <summary>
        /// This method must be called when we want to start working with a service. The current branch for the service is determined and if this is found to 
        /// be new, a new branch is created in the repository. The new/existing branch is then checked-out so work on the service will commence on that branch. 
        /// Before doing all this, we perform a 'pull' on the remote (from master) to assure that we are in sync. 
        /// If the pull resulted in a 'reference not fetched' exception, this indicates that we have a new local branch that does not yet exist at remote. We
        /// create the branch at remote to assure other developers are aware of it.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the pull resulted in merge conflicts.</exception>
        internal void CheckoutService()
        {
            try
            {
                InitializeBranch();
                if (this._branchName != string.Empty) this._repository.SetRootBranch(this._branchName);
                else this._repository.ResetBranch();
                this._repository.Pull();
                if (this._repository.IsDirty) this._trackedService.ConfigurationMgmtState = CMState.Modified;
            }
            catch (Exception exc)
            {
                if (this._branchName != string.Empty)
                {
                    if (exc.Message.Contains(_NoRemoteBranch) || exc.Message.Contains(_NoTrackingRemoteBranch))
                    {
                        Logger.WriteInfo("Framework.ConfigurationManagement.CMContext.CheckoutService >> Pulled branch '" + this._branchName +
                                         "' does not (yet) exist at remote, creating one...");
                        this._repository.PushBranch(this._branchName);
                    }
                    else throw;
                }
            }
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
            Logger.WriteInfo("Framework.ConfigurationManagement.CMContext.ReleaseService >> Releasing Service to remote with message '" + message + "'...");

            if (this._trackedService.ConfigurationMgmtState == CMState.Committed)
            {
                int buildNr = this._trackedService.BuildNumber;
                string tagName = this._trackedService.BusinessFunctionID + "." + this._trackedService.ContainerPkg.Name + "." + this._trackedService.Name + "_";
                if (!this._trackedService.IsDefaultOperationalStatus) tagName += this._trackedService.OperationalStatus + "-";
                tagName += "V" + this._trackedService.Version.Item1 + "P" + this._trackedService.Version.Item2 + "B" + buildNr;

                Logger.WriteInfo("Framework.ConfigurationManagement.CMContext.ReleaseService >> Merging and Pushing to remote with tag '" +
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
                string errMsg = "Framework.ConfigurationManagement.CMContext.ReleaseService >> Service '" + this._trackedService.Name + "' must be committed before it can be released!";
                throw new InvalidOperationException(errMsg);
            }
        }

        /// <summary>
        /// Helper function that determines the configuration management state of the service and loads the branch name accordingly.
        /// This branch name is based on the fully qualified service name, major- and minor versions and 
        /// build number. The branch is used only for service processing and is merged when the service is released.
        /// When the current service CM state is disabled or released, we don't have a valid branch and thus the branch name is set to an empty string.
        /// </summary>
        private void InitializeBranch()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            string CMBranchTagName = context.GetConfigProperty(_CMBranchTag);
            CMState cmState = this._trackedService.ConfigurationMgmtState;

            if (cmState != CMState.Disabled && cmState != CMState.Released)
            {
                this._branchName = this._trackedService.ServiceClass.GetTag(CMBranchTagName);
                if (string.IsNullOrEmpty(this._branchName))
                {
                    string operationalStatus = this._trackedService.IsDefaultOperationalStatus ? string.Empty : _trackedService.OperationalStatus;
                    this._branchName = this._trackedService.BusinessFunctionID + "." + this._trackedService.ContainerPkg.Name + "_";
                    this._branchName += (operationalStatus == string.Empty) ? this._trackedService.Name + "_V" + this._trackedService.Version.Item1.ToString() :
                                                                              this._trackedService.Name + "_" + operationalStatus + "_V" + this._trackedService.Version.Item1.ToString();
                    this._branchName += "P" + this._trackedService.Version.Item2 + "B" + this._trackedService.BuildNumber;
                    Logger.WriteInfo("Framework.ConfigurationManagement.CMContext.InitializeBranch >> Branch set to: '" + this._branchName + "'.");
                    this._trackedService.ServiceClass.SetTag(CMBranchTagName, this._branchName, true);
                }
            }
            else
            {
                // When configuration management is disabled or released, clear the branch name...
                Logger.WriteInfo("Framework.ConfigurationManagement.CMContext.InitializeBranch >> Branch set to empty!");
                this._branchName = string.Empty;
                this._trackedService.ServiceClass.SetTag(CMBranchTagName, string.Empty);
            }
        }
    }
}
