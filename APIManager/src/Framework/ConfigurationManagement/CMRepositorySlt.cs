﻿using System;
using System.IO;
using System.Linq;
using System.Configuration;
using System.Diagnostics;
using System.Collections.Generic;
using LibGit2Sharp;
using Framework.Context;
using Framework.Logging;

namespace Framework.ConfigurationManagement
{
    /// <summary>
    /// Provides a generic interface for configuration management. The Configuration Management Repository contains both a local- and remote repository object
    /// and provides all necessary methods to create a repository, add branches and add- and commit files. It provides a GIT wrapper that only supports the
    /// bare minimum of operations required to manage plugin CI's.
    /// </summary>
    sealed internal class CMRepositorySlt
    {
        // Configuration settings used by this module...
        private const string _CMGITConfigTokens             = "CMGITConfigTokens";
        private const string _CMRepoCreateMessage           = "CMRepoCreateMessage";

        private const string _MasterBranch = "master";  // Name of the GIT master branch...
        private const string _GITDirectory = ".git";    // Name of the default GIT repository directory.

        private static readonly CMRepositorySlt _repositorySlt = new CMRepositorySlt();     // The singleton repository instance.

        private string _gitIgnorePatterns;              // Set of ignore patterns for the repository.
        private string _workingDirectory;               // Absolute path to the top of the repository.
        private bool _enabled;                          // Set to 'true' in when CM is enabled.
        private bool _hasRepository;                    // Set to 'true' when a local repository exists and is initialized.
        private Repository _gitRepository;              // Represents the actual GIT repository.
        private RemoteRepository _remote;               // The remote repository associated with our local repository.
        private Identity _identity;                     // Represents the user that is currently working with the repository.
        private Commit _lastCommit;                     // Identifies the last commit into the repository.
        private Branch _currentBranch;                  // Identifies the currrently active branch in the repository.

        private string _repositoryRootPath;             // Absolute path to the root of the local GIT repository (typically workingDirectory/.git).
        private bool _dirty;                            // Set to 'true' in case files have been added to the staging area, but not yet committed.

        /// <summary>
        /// Returns true if Configuration Management is enabled (and properly initialized), false otherwise. 
        /// None of the CM functions will work in case CM is disabled!
        /// </summary>
        internal bool IsCMEnabled { get { return _enabled && _hasRepository; } }

        /// <summary>
        /// Returns true when Configuration Management is enabled (and properly initialized) and the working directory has changed since the last commit.
        /// </summary>
        internal bool IsDirty { get { return IsCMEnabled && this._dirty; } }

        /// <summary>
        /// Public Repository "factory" method. Simply returns the static instance.
        /// </summary>
        /// <returns>Repository singleton object</returns>
        internal static CMRepositorySlt GetRepositorySlt() { return _repositorySlt; }

        /// <summary>
        /// Adds all files found at the specified path name to the staging area, which is at the HEAD of the currently active branch.
        /// The specified pathName MUST be a path that is RELATIVE to the root of the repository!
        /// All files that are found at the specified location will be added, including any files that are in sub-directories of the path.
        /// </summary>
        /// <param name="relativePath">Relative path name from repository root to files that must be staged.</param>
        /// <exception cref="ArgumentException">Is thrown when an absolute pathname is specified.</exception>
        internal void AddToStagingArea(string relativePath)
        {
            if (relativePath.StartsWith("/") || relativePath.StartsWith("\\"))
            {
                string message = "Framework.ConfigurationManagement.CMRepositorySlt.AddToStagingArea >> Pathname '" + relativePath + "' must be relative to repository root!";
                throw new ArgumentException(message);
            }
            Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.AddToStagingArea >> Going to add files found at '" + relativePath + "'...");
            Commands.Stage(this._gitRepository, relativePath);
            this._dirty = true;
        }

        /// <summary>
        /// Commits ALL files that are currently in the staging area to the repository at the HEAD of the currently active branch. If there is nothing
        /// to commit, we issue a warning message to the log to indicate that no operation has been performed. After the commit, the repository is
        /// synchronised with the remote (push to remote).
        /// </summary>
        /// <param name="message">Commit reporting message.</param>
        /// <exception cref="InvalidOperationException">Thrown when the user identity has not yet been registered.</exception>
        internal void CommitStagingArea(string message)
        {
            if (this._identity == null)
            {
                string errorMsg = "Framework.ConfigurationManagement.CMRepositorySlt.CommitStagingArea >> No Identity has been registered yet!";
                throw new InvalidOperationException(errorMsg);
            }

            try
            {
                if (this._dirty)
                {
                    Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.CommitStagingArea >> Committing staging area with message '" + message + "'...");
                    Signature authorSig = new Signature(this._identity, DateTime.Now);
                    Signature committerSig = authorSig;
                    this._lastCommit = this._gitRepository.Commit(message, authorSig, committerSig);
                    Push();
                    this._dirty = false;
                }
                else Logger.WriteWarning("No changes to commit!");
            }
            catch (Exception exc)
            {
                if (exc.Message.Contains("No changes")) Logger.WriteWarning("Nothing to commit!");
                else if (exc.Message.Contains("unknown certificate check failure"))
                {
                    Logger.WriteWarning("Spurious 'certificate failure' detected, retrying...");
                    CommitStagingArea(message);
                }
                else throw;
            }
        }

        /// <summary>
        /// Returns a list of all tags currently defined for this repository that start with the specified prefix.
        /// If the prefix is NULL or an empty string, the function returns all tags.
        /// </summary>
        /// <returns>List of tag names.</returns>
        internal List<string> GetTags(string prefix)
        {
            Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.GetTags >> Looking for tags with prefix '" + prefix + "'...");
            Debug.Assert(this._gitRepository != null);
            var tagList = new List<string>();
            bool allTags = string.IsNullOrEmpty(prefix);
            foreach (Tag t in this._gitRepository.Tags)
            {
                if (allTags) tagList.Add(t.FriendlyName);
                else if (t.FriendlyName.StartsWith(prefix)) tagList.Add(t.FriendlyName);
            }
            return tagList;
        }

        /// <summary>
        /// Merges the specified branch with the master. On return, we are on the 'master' branch!
        /// </summary>
        /// <param name="thisBranch">The branch that must be merged.</param>
        /// <exception cref="ArgumentException">Thrown when the branch to be merged could not be found or merge resulted in merge conflicts.</exception>
        /// <exception cref="InvalidOperationException">Thrown when Identity has not been registered yet.</exception>
        internal void Merge(string thisBranch)
        {
            Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.Merge >> Merging branch '" + thisBranch + "' with master...");

            if (this._identity == null)
            {
                string errorMsg = "Framework.ConfigurationManagement.CMRepositorySlt.Merge >> No Identity has been registered yet!";
                throw new InvalidOperationException(errorMsg);
            }

            // Since we must merge with master, make sure we're on the master...
            Branch master = this._gitRepository.Branches[_MasterBranch];
            if (this._currentBranch == null || this._currentBranch.FriendlyName != _MasterBranch) Commands.Checkout(this._gitRepository, master);
            this._currentBranch = master;

            Branch branch = this._gitRepository.Branches[thisBranch];
            if (branch == null)
            {
                string message = "Framework.ConfigurationManagement.CMRepositorySlt.Merge >> Branch '" + thisBranch + "' not found!";
                Logger.WriteError(message);
                throw new ArgumentException(message);
            }
            MergeResult result = this._gitRepository.Merge(branch, new Signature(this._identity, DateTime.Now));
            this._lastCommit = result.Commit;   // The commit at the merge head.
            if (result.Status == MergeStatus.Conflicts)
            {
                string message = "Framework.ConfigurationManagement.CMRepositorySlt.Merge >> Unable to merge branch '" + thisBranch + "' due to merge conflicts!";
                throw new ArgumentException(message);
            }
        }

        /// <summary>
        /// Updates the current branch by pulling the branch from the remote repository.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the pull resulted in merge conflicts.</exception>
        internal void Pull()
        {
            Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.Pull >> Pulling current branch '" +
                              this._currentBranch.FriendlyName + "' from remote...");
            this._remote.Pull();
        }

        /// <summary>
        /// Pushes commits on the current branch to the same branch on the remote repository. The operation is identified by the specified tag (name and message).
        /// </summary>
        /// <param name="tagName">Name of the tag that we must push with the changes.</param>
        /// <param name="message">Message that will be passed through the tag.</param>
        internal void Push(string tagName, string message)
        {
            Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.Push >> Pushing current branch '" +
                             this._currentBranch.FriendlyName + "' to remote with tag '" + tagName + "'...");
            this._remote.Push(this._currentBranch, tagName, message);
        }

        /// <summary>
        /// Pushes commits on the current branch to the same branch on the remote repository. The operation is untagged and is used only to synchronize local and
        /// remote repositories. If no work has been committed on the branch, only the branch itself is pushed (and thus made known to remote).
        /// </summary>
        internal void Push()
        {
            Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.Push >> Pushing current branch '" +
                             this._currentBranch.FriendlyName + "' to remote...");
            this._remote.Push(this._currentBranch);
        }

        /// <summary>
        /// Pushes a local branch to remote in order to assure that it exists. The branch MUST have been registered earlier.
        /// </summary>
        /// <param name="thisBranch">Name of branch to push.</param>
        /// <exception cref="ArgumentException">Thrown when specified branch does not exist.</exception>
        internal void PushBranch(string thisBranch)
        {
            Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.PushBranch >> Pushing branch '" + thisBranch + "' to remote...");
            Branch branch = this._gitRepository.Branches[thisBranch];
            if (branch == null)
            {
                string message = "Framework.ConfigurationManagement.CMRepositorySlt.PushBranch >> Branch '" + thisBranch + "' not found!";
                throw new ArgumentException(message);
            }
            this._remote.Push(branch);
        }

        /// <summary>
        /// When a branch is specified, this branch is deleted locally (and on remote) and master is checked-out.
        /// When no branch is specified, we simply check-out the master branch.
        /// </summary>
        internal void ResetBranch(string thisBranch = null)
        {
            Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.ResetBranch >> Delete branch '" +
                             thisBranch + "' and checkout the master branch...");
            if (!string.IsNullOrEmpty(thisBranch))
            {
                Branch deleteBranch = this._gitRepository.Branches[thisBranch];
                this._remote.DeleteBranch(deleteBranch);
                this._gitRepository.Branches.Remove(thisBranch);
            }
            Branch master = this._gitRepository.Branches[_MasterBranch];
            if (this._currentBranch == null || this._currentBranch.FriendlyName != _MasterBranch)
            {
                Commands.Checkout(this._gitRepository, master);
                this._currentBranch = master;
            }
        }

        /// <summary>
        /// The function activates the child-branch with given name. If the branch does not yet exist, it is created. On return, the branch has been
        /// made the active branch in the repository (checked-out).
        /// We call this a 'child-branch', since the branch is created from the specified parent branch. To create a branch that starts at
        /// the master, use the 'SetRootBranch' function instead.
        /// </summary>
        /// <param name="childBranch">Name of branch to activate.</param>
        /// <param name="parentBranch">Name of the branch from which we create the child.</param>
        /// <exception cref="ArgumentException">Thrown when branch creation failed.</exception>
        internal void SetChildBranch(string childBranch, string parentBranch)
        {
            Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.SetChildBranch >> Going to switch to child branch '" + childBranch + "'...");
            if (this._currentBranch != null && this._currentBranch.FriendlyName == childBranch) return;     // We're already on the correct branch!

            try
            {
                // First of all, make sure that we are on the correct parent (to get the HEAD right)...
                Branch parent = this._gitRepository.Branches[parentBranch];
                if (this._currentBranch == null || this._currentBranch.FriendlyName != parentBranch) Commands.Checkout(this._gitRepository, parent);

                Branch branch = this._gitRepository.Branches[childBranch];
                if (branch == null)
                {
                    Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.SetChildBranch >> New branch!");
                    branch = this._gitRepository.CreateBranch(childBranch);
                }
                this._currentBranch = Commands.Checkout(this._gitRepository, branch);
                this._remote.UpdateBranches();          // Instructs remote to track this new branch as well.
            }
            catch (Exception exc)
            {
                string message = "Framework.ConfigurationManagement.CMRepositorySlt.SetChildBranch >> Failed to switch to branch '" + childBranch + "' because: " + exc.Message;
                throw new ArgumentException(message);
            }
        }

        /// <summary>
        /// The function activates the root-branch with given name. If the branch does not yet exist, it is created. On return, the branch has been
        /// made the active branch in the repository (checked-out).
        /// We call this a 'root-branch', since they are all created from the HEAD of the 'master' branch. To create a branch that starts at another
        /// HEAD, use the 'SetChildBranch' function instead.
        /// </summary>
        /// <param name="thisBranch">Name of branch to activate.</param>
        /// <exception cref="ArgumentException">Thrown when branch creation failed.</exception>
        internal void SetRootBranch(string thisBranch)
        {
            Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.SetRootBranch >> Going to switch to branch '" + thisBranch + "'...");
            if (this._currentBranch != null && this._currentBranch.FriendlyName == thisBranch) return;     // We're already on the correct branch!

            try
            {
                // Since root-branches start at master.HEAD, make sure we're on the master...
                Branch master = this._gitRepository.Branches[_MasterBranch];
                if (this._currentBranch == null || this._currentBranch.FriendlyName != _MasterBranch) Commands.Checkout(this._gitRepository, master);

                Branch branch = this._gitRepository.Branches[thisBranch];
                if (branch == null)
                {
                    Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.SetRootBranch >> New branch!");
                    branch = this._gitRepository.CreateBranch(thisBranch);
                }
                this._currentBranch = Commands.Checkout(this._gitRepository, branch);
                this._remote.UpdateBranches();      // Instructs remote to track this new branch as well.
            }
            catch (Exception exc)
            {
                string message = "Framework.ConfigurationManagement.CMRepositorySlt.SetRootBranch >> Failed to switch to branch '" + thisBranch + "' because: " + exc.Message;
                throw new ArgumentException(message);
            }
        }

        /// <summary>
        /// Creates a new repository singleton instance.
        /// </summary>
        private CMRepositorySlt()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            RepositoryDescriptor repoDsc = CMRepositoryDscManagerSlt.GetRepositoryDscManagerSlt().GetCurrentDescriptor();
            if (repoDsc == null)
            {
                this._enabled = false;
                this._hasRepository = false;
                string message = "Unable to retrieve proper CM repository descriptor, aborting!";
                Logger.WriteError("Framework.ConfigurationManagement.CMRepositoryslt >>  " + message);
                throw new ConfigurationErrorsException(message);
            }
            Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositoryslt >> Initializing local repository at '" + repoDsc.LocalRootPath + "'...");

            this._workingDirectory = repoDsc.LocalRootPath;
            this._enabled = repoDsc.IsCMEnabled;
            this._hasRepository = false;
            this._gitIgnorePatterns = repoDsc.GITIgnoreList;
            this._identity = repoDsc.RepositoryUserID;

            try
            {
                if (this._enabled) OpenRepository();
                else
                {
                    Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt >> Configuration Management is disabled!");
                    this._gitRepository = null;
                    this._remote = null;
                    this._repositoryRootPath = string.Empty;
                    this._hasRepository = false;
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Framework.ConfigurationManagement.CMRepositorySlt >> Failed to initialize repository because: " + exc.Message);
                this._gitRepository = null;
                this._remote = null;
                this._repositoryRootPath = string.Empty;
                this._hasRepository = false;
            }
        }

        /// <summary>
        /// Checks whether we have a local repository. If not, we create one. If there is one, we initialize a local repository object.
        /// On return, the repository is open and initialized.
        /// </summary>
        private void OpenRepository()
        {
            ContextSlt context = ContextSlt.GetContextSlt();
            Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositoryslt.OpenRepository >> Initializing local repository at '" + this._workingDirectory + "'...");

            if (!this._hasRepository)
            {
                if (!Directory.Exists(this._workingDirectory + "/" + _GITDirectory))
                {
                    Logger.WriteWarning("No repository found at location '" + this._workingDirectory + "', creating one...");
                    try
                    {
                        Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositoryslt.OpenRepository >> Trying to clone remote to local...");
                        this._repositoryRootPath = RemoteRepository.Clone(this._workingDirectory);
                        this._gitRepository = new Repository(this._workingDirectory);
                    }
                    catch // On errors, assume we can't clone remote and thus only create a local repository...
                    {
                        Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositoryslt.OpenRepository >> Cloning failed, create empty local repository instead...");
                        Repository.Init(this._workingDirectory);
                        this._gitRepository = new Repository(this._workingDirectory);

                        // Bit of a hack: create an empty commit in case of local repository, so we have a valid HEAD to attach stuff to.
                        Signature authorSig = new Signature(this._identity, DateTime.Now);
                        Signature committerSig = authorSig;
                        CommitOptions options = new CommitOptions();
                        options.AllowEmptyCommit = true;
                        this._lastCommit = this._gitRepository.Commit(context.GetConfigProperty(_CMRepoCreateMessage), authorSig, committerSig, options);
                    }
                    this._dirty = false;
                    Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.OpenRepository >> Repository successfully created at location '" + _workingDirectory + "'.");
                }
                else
                {
                    try
                    {
                        Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.OpenRepository >> Opening existing repository at location '" + _workingDirectory + "'...");
                        this._gitRepository = new Repository(_workingDirectory);
                        this._dirty = this._gitRepository.RetrieveStatus(new StatusOptions()).IsDirty;
                    }
                    catch (Exception exc)
                    {
                        string msg = exc.Message;
                        Logger.WriteError("OpenRepository Failed with exception: " + msg);
                    }
                }
                UpdateGITConfiguration();

                this._repositoryRootPath = this._gitRepository.Info.Path;
                this._remote = new RemoteRepository(this._gitRepository);
                Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.OpenRepository >> Successfully created remote repository...");

                // Load ignore patterns for the current session....
                if (!string.IsNullOrEmpty(_gitIgnorePatterns))
                {
                    string[] ignorePatterns = _gitIgnorePatterns.Split(',');
                    this._gitRepository.Ignore.AddTemporaryRules(ignorePatterns);
                }

                // Determine our currently active branch, must always have one!...
                this._currentBranch = null;
                foreach (Branch branch in this._gitRepository.Branches.Where(branch => !branch.IsRemote))
                {
                    Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.OpenRepository >> Inspecting branch '" + branch.FriendlyName + "'...");
                    if (branch.IsCurrentRepositoryHead)
                    {
                        this._currentBranch = branch;
                        Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.OpenRepository >> Current branch initialized to '" + branch.FriendlyName + "'...");
                        break;
                    }
                }
                Debug.Assert(this._currentBranch != null);
                this._hasRepository = true; 
                Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.OpenRepository >> Repository root path set to '" + this._repositoryRootPath + "'.");
            }
        }

        /// <summary>
        /// Helper function that updates the GIT configuration of our local repository to include configured settings (core properties and/or 
        /// proxy configuration). Note that proxy settings are configured at global level (since these typically affect ALL traffic to and
        /// from remote repositories).
        /// </summary>
        private void UpdateGITConfiguration()
        {
            Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.UpdateGITConfiguration >> Updating configuration settings...");
            ContextSlt context = ContextSlt.GetContextSlt();
            LibGit2Sharp.Configuration cfg = this._gitRepository.Config;

            foreach (string configToken in context.GetConfigProperty(_CMGITConfigTokens).Split(','))
            {
                Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.UpdateGITConfiguration >> Processing config token '" + configToken + "'...");
                string[] kvPair = configToken.Split(':');
                cfg.Set<string>(kvPair[0].Trim(), kvPair[1].Trim(), ConfigurationLevel.Local);
            }
        }
    }
}
