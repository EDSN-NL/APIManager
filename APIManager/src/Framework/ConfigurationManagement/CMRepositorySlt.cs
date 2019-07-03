using System;
using System.IO;
using System.Linq;
using System.Configuration;
using System.Collections.Generic;
using LibGit2Sharp;
using Framework.Context;
using Framework.Logging;

namespace Framework.ConfigurationManagement
{
    /// <summary>
    /// Provides a generic interface for configuration management. The Configuration Management Repository contains both a local- and remote repository object
    /// and provides all necessary methods to create a repository, add branches and add- and commit files. It provides a GIT wrapper that only supports the
    /// bare minimum of operations required to manage CDM CI's.
    /// </summary>
    sealed internal class CMRepositorySlt : IDisposable
    {
        // Configuration settings used by this module...
        private const string _CMGITConfigTokens = "CMGITConfigTokens";
        private const string _CMRepoCreateMessage = "CMRepoCreateMessage";
        private const string _CMReleaseBranchName = "CMReleaseBranchName";

        // Some constants...
        private const string _GITDirectory = ".git";        // Name of the default GIT repository directory.
        private const string _NoChanges = "No changes";     // Check GIT response status for 'no changes made'.
        private const string _CertError = "unknown certificate check failure";  // Check GIT response status for 'certificate errors'.
        private const string _RemoteName = "origin";
        private const string _Conflicts = "conflicts prevent checkout";          // Failed to switch to a branch 'cause of conflicts.
        private const string _Conflict = "conflict prevents checkout";          // Failed to switch to a branch 'cause of conflicts.

        private static readonly CMRepositorySlt _repositorySlt = new CMRepositorySlt();     // The singleton repository instance.

        private string _gitIgnorePatterns;                  // Set of ignore patterns for the repository.
        private string _workingDirectory;                   // Absolute path to the top of the repository.
        private bool _enabled;                              // Set to 'true' in when CM is enabled.
        private bool _hasRepository;                        // Set to 'true' when a local repository exists and is initialized.
        private Repository _gitRepository;                  // Represents the actual GIT repository.
        private RemoteRepository _remote;                   // The remote repository associated with our local repository.
        private Identity _identity;                         // Represents the user that is currently working with the repository.
        private Commit _lastCommit;                         // Identifies the last commit into the repository.
        private Branch _currentBranch;                      // Identifies the currrently active branch in the repository.
        private string _releaseBranchName;                  // Name of the persistent GIT release branch (typically, this will be 'master').

        private string _repositoryRootPath;                 // Absolute path to the root of the local GIT repository (typically workingDirectory/.git).
        private bool _dirty;                                // Set to 'true' in case files have been added to the staging area, but not yet committed.
        private bool _disposed;                             // Set to 'true' after an object dispose operation.

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
        /// Returns the name of the persistent release branch.
        /// </summary>
        internal string ReleaseBranchName { get { return this._releaseBranchName; } }

        /// <summary>
        /// Adds all files found at the specified path name to the staging area, which is at the HEAD of the currently active branch.
        /// The specified pathName MUST be a path that is RELATIVE to the root of the repository!
        /// All files that are found at the specified location will be added, including any files that are in sub-directories of the path.
        /// </summary>
        /// <param name="relativePath">Relative path name from repository root to files that must be staged.</param>
        /// <exception cref="ArgumentException">Is thrown when an absolute pathname is specified.</exception>
        internal void AddToStagingArea(string relativePath)
        {
            HasRepository("AddToStagingArea");
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
        /// Performs a checkout of the specified tag to a temporary branch.
        /// This facilitates working with artefacts from the tag without disrupting the main flow of the CM repository. 
        /// The function returns the temporary branch created, whic should be cleaned-up as soon as possible.
        /// Tag must exist locally or we're throwing an 'InvalidArgument' exception.
        /// </summary>
        /// <param name="tagName">The tag to checkout</param>
        /// <returns>Name of the temporary branch.</returns>
        /// <exception cref="ArgumentException">Is thrown when the tag could not be found or we could not perform the requested operation.</exception>
        internal string CheckoutTag(string tagName)
        {
            Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.CheckoutTag >> Checkout tag '" + tagName + "'...");
            HasRepository("CheckoutTag");
            Branch tempBranch = null;
            Tag myTag = null;
            myTag = this._gitRepository.Tags[tagName];
            if (myTag != null) Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.CheckoutTag >> Found the tag!");
            else
            {
                string msg = "Attempt to check-out unknown tag '" + tagName + "'!";
                Logger.WriteError("Framework.ConfigurationManagement.CMRepositorySlt.CheckoutTag >> " + msg);
                throw new ArgumentException(msg);
            }

            try
            {
                string tempName = tagName.Replace("feature", "temporary");  // Temporary name is based on the received feature tag.
                Branch detachedHead = Commands.Checkout(this._gitRepository, myTag.CanonicalName);
                if (detachedHead != null)
                {
                    tempBranch = this._gitRepository.CreateBranch(tempName);
                    if (tempBranch != null)
                    {
                        tempBranch = Commands.Checkout(this._gitRepository, tempBranch);
                        if (tempBranch != null)
                        {
                            this._currentBranch = tempBranch;
                            return tempBranch.FriendlyName;
                        }
                    }
                }

                // Fall-through in case of errors...
                GotoBranch(_CMReleaseBranchName);   // On checkout errors, return to the release branch!
                if (tempBranch != null) this._gitRepository.Branches.Remove(tempBranch);
                string msg = "Unable to checkout tag '" + tagName + "'!";
                Logger.WriteError("Framework.ConfigurationManagement.CMRepositorySlt.CheckoutTag >> " + msg);
                throw new ArgumentException(msg);
            }
            catch (ArgumentException) { throw; }
            catch (Exception exc)
            {
                GotoBranch(_CMReleaseBranchName);   // On checkout errors, return to the release branch!
                if (tempBranch != null) this._gitRepository.Branches.Remove(tempBranch);
                string msg = "Unable to checkout tag '" + tagName + "' because of exception!";
                Logger.WriteError("Framework.ConfigurationManagement.CMRepositorySlt.CheckoutTag >> " + msg + Environment.NewLine + exc.ToString());
                throw new ArgumentException(msg, exc);
            }
        }

        /// <summary>
        /// Commits ALL files that are currently in the staging area to the repository at the HEAD of the currently active branch. If there is nothing
        /// to commit, we issue a warning message to the log to indicate that no operation has been performed. After the commit, the repository is
        /// synchronised with the remote (push to remote).
        /// </summary>
        /// <param name="message">Commit reporting message.</param>
        /// <param name="pushToRemote">When set to 'true', the commit is also pushed to the remote repository.</param>
        /// <returns>True on successfull commit, false when there is nothing to commit.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the user identity has not yet been registered.</exception>
        internal bool CommitStagingArea(string message, bool pushToRemote)
        {
            bool retVal = false;
            if (this._identity == null)
            {
                string errorMsg = "Framework.ConfigurationManagement.CMRepositorySlt.CommitStagingArea >> No Identity has been registered yet!";
                throw new InvalidOperationException(errorMsg);
            }

            try
            {
                HasRepository("CommitStagingArea");
                if (this._dirty)
                {
                    Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.CommitStagingArea >> Committing staging area with message '" + message + "'...");
                    Signature authorSig = new Signature(this._identity, DateTime.Now);
                    Signature committerSig = authorSig;
                    this._lastCommit = this._gitRepository.Commit(message, authorSig, committerSig);
                    if (pushToRemote && this._lastCommit != null) Push();
                    this._dirty = false;
                    retVal = true;
                }
                else Logger.WriteWarning("No changes to commit!");
            }
            catch (CMOutOfSyncException)
            {
                string msg = "Unable to push service to remote repository because build numbers are out of sync!";
                Logger.WriteError("Framework.ConfigurationManagement.CMRepositorySlt.CommitStagingArea >> " + msg);
                throw;
            }
            catch (Exception exc)
            {
                if (exc.Message.Contains(_NoChanges)) Logger.WriteWarning("Nothing to commit!");
                else if (exc.Message.Contains(_CertError))
                {
                    Logger.WriteWarning("Spurious 'certificate failure' detected, retrying...");
                    CommitStagingArea(message, pushToRemote);
                }
                else throw;
            }
            return retVal;
        }

        /// <summary>
        /// Deletes the specified tag, both locally and remote.
        /// </summary>
        /// <param name="thisTag">Tag to be deleted.</param>
        internal void DeleteTag(Tag thisTag)
        {
            HasRepository("DeleteTag");
            this._remote.DeleteTag(thisTag);
            this._gitRepository.Tags.Remove(thisTag);
        }

        /// <summary>
        /// Call this to remove the repository session. Note that this can not be recovered!
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Returns the Tag object associated with the specified name.
        /// </summary>
        /// <param name="name">Name of tag to locate.</param>
        /// <returns>Tag object associated with name or null if nothing found.</returns>
        internal Tag GetTag(string name)
        {
            Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.GetTag >> Retrieving tag '" + name + "'...");
            HasRepository("GetTag");
            return this._gitRepository.Tags[name];
        }

        /// <summary>
        /// Returns a list of all tags currently defined for this repository that contain the specified filter.
        /// If the filter is NULL or an empty string, the function returns all tags.
        /// </summary>
        /// <returns>List of tag names.</returns>
        internal List<Tag> GetTags(string filter)
        {
            Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.GetTags >> Looking for tags with filter '" + filter + "'...");
            HasRepository("GetTags");
            var tagList = new List<Tag>();
            bool allTags = string.IsNullOrEmpty(filter);
            foreach (Tag t in this._gitRepository.Tags)
            {
                if (allTags) tagList.Add(t);
                else if (t.FriendlyName.Contains(filter)) tagList.Add(t);
            }
            return tagList;
        }

        /// <summary>
        /// Switch to the specified branch. If targetBranch does not exist, we will throw an Argument Exception.
        /// </summary>
        /// <param name="targetBranch">The branch that will be active on return.</param>
        /// <exception cref="ArgumentException">Thrown when the specified target branch does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the repository is not properly initialised at this point.</exception>
        internal void GotoBranch(string targetBranch)
        {
            Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.GotoBranch >> Checkout branch '" + targetBranch + "'...");
            Branch target = null;
            if (this._gitRepository != null)
            {
                target = DoCheckout(targetBranch);
                if (target == null)
                {
                    // Branch is not in our 'current branches' list, but it might exist on remote...
                    try
                    {
                        Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.GotoBranch >> Trying to get '" + targetBranch + "' from remote...");
                        string trackedBranchName = _RemoteName + "/" + targetBranch;
                        Branch trackedBranch = this._gitRepository.Branches[trackedBranchName];                // Get a reference on the remote tracking branch...
                        Branch branch = this._gitRepository.CreateBranch(targetBranch, trackedBranch.Tip);     // ...and create a local branch pointing at the same Commit
                        
                        // Finally, let's configure the local branch to track the remote one.
                        target = this._gitRepository.Branches.Update(branch, b => b.TrackedBranch = trackedBranch.CanonicalName);
                        if (target == null)
                        {
                            string msg = "Target branch '" + targetBranch + "' not found!";
                            Logger.WriteError("Framework.ConfigurationManagement.CMRepositorySlt.GotoBranch >> " + msg);
                            throw new ArgumentException(msg);
                        }
                        this._currentBranch = target;
                    }
                    catch (Exception exc)
                    {
                        string msg = "Target branch '" + targetBranch + "' not found!";
                        Logger.WriteError("Framework.ConfigurationManagement.CMRepositorySlt.GotoBranch >> Caught an exception:" + 
                                          Environment.NewLine + exc.ToString());
                        Logger.WriteError("Framework.ConfigurationManagement.CMRepositorySlt.GotoBranch >> " + msg);
                        throw new ArgumentException(msg);
                    }
                }
            }
            else
            {
                string msg = "CM Environment not (properly) initialised!";
                Logger.WriteError("Framework.ConfigurationManagement.CMRepositorySlt.GotoBranch >> " + msg);
                throw new InvalidOperationException(msg);
            }
        }

        /// <summary>
        /// Merges specified branch with specified target branch. On return, we are on the target branch!
        /// </summary>
        /// <param name="thisBranch">The branch that must be merged.</param>
        /// <param name="targetBranch">The branch to which we will merge.</param>
        /// <exception cref="ArgumentException">Thrown when the branch to be merged could not be found or merge resulted in merge conflicts.</exception>
        /// <exception cref="InvalidOperationException">Thrown when Identity has not been registered yet.</exception>
        internal void Merge(string thisBranch, string targetBranch)
        {
            Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.Merge >> Merging branch '" + 
                             thisBranch + "' with '" + targetBranch + "'...");

            if (this._identity == null)
            {
                string errorMsg = "No Identity has been registered yet!";
                Logger.WriteError("Framework.ConfigurationManagement.CMRepositorySlt.Merge >> " + errorMsg);
                throw new InvalidOperationException(errorMsg);
            }
            HasRepository("Merge");

            // Since we must merge with target, make sure we're on that target...
            Branch target = DoCheckout(targetBranch);
            if (target == null)
            {
                string message = "Branch '" + targetBranch + "' not found!";
                Logger.WriteError("Framework.ConfigurationManagement.CMRepositorySlt.Merge >> " + message);
                throw new ArgumentException(message);
            }

            Branch branch = this._gitRepository.Branches[thisBranch];
            if (branch == null)
            {
                string message = "Branch '" + thisBranch + "' not found!";
                Logger.WriteError("Framework.ConfigurationManagement.CMRepositorySlt.Merge >> " + message);
                throw new ArgumentException(message);
            }
            MergeResult result = this._gitRepository.Merge(branch, new Signature(this._identity, DateTime.Now));
            this._lastCommit = result.Commit;   // The commit at the merge head.
            if (result.Status == MergeStatus.Conflicts)
            {
                this._gitRepository.Reset(ResetMode.Hard);  // To avoid any more damage due to merge conflict, reset the repository to the last commit.
                string message = "Failed to merge branch '" + thisBranch + "' due to merge conflicts, repository has been reset!";
                Logger.WriteError("Framework.ConfigurationManagement.CMRepositorySlt.Merge >> " + message);
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
            HasRepository("Pull");
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
            HasRepository("Push");
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
            HasRepository("Push");
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
            HasRepository("PushBranch");
            Branch branch = this._gitRepository.Branches[thisBranch];
            if (branch == null)
            {
                string message = "Branch '" + thisBranch + "' not found!";
                Logger.WriteError("Framework.ConfigurationManagement.CMRepositorySlt.PushBranch >> " + message);
                throw new ArgumentException(message);
            }
            this._remote.Push(branch);
        }

        /// <summary>
        /// Deletes a branch and then switches to the specified branch. If the branch to be deleted can't be find, the function simply
        /// switches to the specified target branch.
        /// </summary>
        /// <param name="thisBranch">The branch to be deleted.</param>
        /// <param name="targetBranch">The branch that will be active on return.</param>
        /// <param name="localOnly">When set to true, the remove operation will not be pushed to remote (local impact only).</param>
        /// <exception cref="ArgumentException">Thrown when delete- and target branch are identical or the target does not exist.</exception>
        internal void RemoveBranch(string thisBranch, string targetBranch, bool localOnly = false)
        {
            Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.RemoveBranch >> Delete branch '" +
                             thisBranch + "' and checkout '" + targetBranch + "' branch...");

            if (thisBranch == targetBranch)
            {
                string msg = "Attempt to delete- and switch to the same branch '" + thisBranch + "'!";
                Logger.WriteError("Framework.ConfigurationManagement.CMRepositorySlt.RemoveBranch >> " + msg);
                throw new ArgumentException(msg);
            }
            HasRepository("RemoveBranch");

            // First of all, switch to the target branch so we're sure not to delete our current head!
            Branch target = DoCheckout(targetBranch);
            if (target == null)
            {
                string msg = "Target branch '" + targetBranch + "' could not be found on a remove branch operation!";
                Logger.WriteError("Framework.ConfigurationManagement.CMRepositorySlt.RemoveBranch >> " + msg);
                throw new ArgumentException(msg);
            }

            Branch deleteBranch = this._gitRepository.Branches[thisBranch];
            if (deleteBranch != null)
            {
                if (!localOnly) this._remote.DeleteBranch(deleteBranch);
                this._gitRepository.Branches.Remove(thisBranch);
            }
        }

        /// <summary>
        /// The function activates the child-branch with given name. If the branch does not yet exist, it is created. On return, the branch has been
        /// made the active branch in the repository (checked-out).
        /// The new branch is created from the specified parent branch and is pushed to remote (if not yet existing).
        /// </summary>
        /// <param name="childBranch">Name of branch to activate.</param>
        /// <param name="parentBranch">Name of the branch from which we create the child.</param>
        /// <exception cref="ArgumentException">Thrown when branch creation failed (or illegal branch names were supplied).</exception>
        internal void SetBranch(string childBranch, string parentBranch)
        {
            if (string.IsNullOrEmpty(childBranch) || string.IsNullOrEmpty(parentBranch))
            {
                string msg = "Illegal or missing branch names";
                Logger.WriteError("Framework.ConfigurationManagement.CMRepositorySlt.SetBranch >> " + msg);
                throw new ArgumentException(msg);
            }
            HasRepository("SetBranch");

            Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.SetBranch >> Going to switch to child branch '" + childBranch + "'...");
            if (this._currentBranch != null && this._currentBranch.FriendlyName == childBranch) return;     // We're already on the correct branch!

            Branch parent, branch;
            try
            {
                // If the branch already exists, we can switch to it directly....
                branch = DoCheckout(childBranch);
                if (branch == null)
                {
                    // Branch does not exist, switch to parent before creating it...
                    parent = DoCheckout(parentBranch);
                    if (parent == null)
                    {
                        string msg = "Specified parent branch '" + parentBranch + "' does not exist!";
                        Logger.WriteError("Framework.ConfigurationManagement.CMRepositorySlt.SetBranch >> " + msg);
                        throw new ArgumentException(msg);
                    }

                    Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.SetChildBranch >> Creating new branch '" + childBranch + "'...");
                    branch = this._gitRepository.CreateBranch(childBranch);
                    if (branch == null)
                    {
                        string msg = "Specified child branch '" + childBranch + "' could not be created!";
                        Logger.WriteError("Framework.ConfigurationManagement.CMRepositorySlt.SetBranch >> " + msg);
                        throw new ArgumentException(msg);
                    }
                    DoCheckout(childBranch);
                    this._remote.UpdateBranches();          // Instructs remote to track this new branch as well.
                }
            }
            catch (Exception exc)
            {
                string message = "Failed to switch to branch '" + childBranch + "' because: " + exc.Message;
                Logger.WriteError("Framework.ConfigurationManagement.CMRepositorySlt.SetChildBranch >> " + message + Environment.NewLine + exc.ToString());
                throw new ArgumentException(message, exc);
            }
        }

        /// <summary>
        /// Remove ALL tags from the local repository and then retrieve all current tags from remote.
        /// </summary>
        internal void SynchroniseTags()
        {
            Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositoryslt.SynchroniseTags >> Delete all local tags and retrieve from remote...");
            // We can't iterate over the tags and delete them at the same time. So, first collect all tags and subsequently delete them.
            // After deleting all local tags, we retrieve the list of tags from remote and re-populate the repository.
            List<Tag> tagList = new List<Tag>();
            foreach (Tag t in this._gitRepository.Tags) tagList.Add(t);
            foreach (Tag t in tagList) this._gitRepository.Tags.Remove(t);
            this._remote.FetchTags();
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
            this._releaseBranchName = context.GetConfigProperty(_CMReleaseBranchName);

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
                Logger.WriteError("Framework.ConfigurationManagement.CMRepositorySlt >> Failed to initialize repository because: " + exc.ToString());
                this._gitRepository = null;
                this._remote = null;
                this._repositoryRootPath = string.Empty;
                this._hasRepository = false;
            }

        }

        /// <summary>
        /// Helper function that attempts to perform a checkout of the specified branch. It first attempts a 'regular' checkout. If this results
        /// in conflict errors, the function performs a 'forced checkout', which will effectively discard the conflicting files.
        /// If we're already on the requested branch, the function does not perform any operations and simply returns the current branch.
        /// Note that the function DOES set the 'Current Branch' class property to the checked-out branch!
        /// </summary>
        /// <param name="branchName">Branch to be checked-out.</param>
        /// <returns>Checked-out branch or NULL if a branch with the specified name does not exist.</returns>
        private Branch DoCheckout(string branchName)
        {
            Branch target = this._gitRepository.Branches[branchName];
            if (target != null)
            {
                // If the current branch is equal to the requested branch, do nothing since our branch is already checked-out!
                if (this._currentBranch != null && this._currentBranch.FriendlyName == branchName) return this._currentBranch;

                try
                {
                    Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.DoCheckout >> Checkout branch '" + target.FriendlyName + "'...");
                    target = Commands.Checkout(this._gitRepository, target.FriendlyName);
                }
                catch (Exception exc)
                {
                    Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.DoCheckout >> Got an exception:" + Environment.NewLine + exc.Message);
                    if (exc.Message.Contains(_Conflict) || exc.Message.Contains(_Conflicts))
                    {
                        Logger.WriteWarning("Can't checkout branch '" + target.FriendlyName + "', attempting brute-force...!");
                        var checkoutOptions = new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force };
                        target = Commands.Checkout(this._gitRepository, target, checkoutOptions);
                    }
                    else throw;
                }
            }
            if (target != null) this._currentBranch = target;
            return target;
        }

        /// <summary>
        /// Checks whether we have a local repository. If not, we create one. If there is one, we initialize a local repository object.
        /// On return, the repository is open and initialized and the development branch is the active branch.
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
                        Logger.WriteError("OpenRepository Failed with exception: " + exc.ToString());
                    }
                }
                GotoBranch(this._releaseBranchName);    // Make sure we're on 'master' to have a consistent starting context.
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
                this._hasRepository = true; 
                Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositorySlt.OpenRepository >> Repository root path set to '" + this._repositoryRootPath + "'.");
            }
        }

        /// <summary>
        /// The destructor is declared as a safeguard to assure that the repository session is properly cleared.
        /// </summary>
        ~CMRepositorySlt()
        {
            Dispose(false);
        }

        /// <summary>
        /// This is the actual disposing interface, which takes case of structural removal of the implementation type when no longer
        /// needed.
        /// </summary>
        /// <param name="disposing">Set to 'true' when called directly. Set to 'false' when called from the finalizer 
        /// (ignored for the time being).</param>
        private void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                this._remote = null;
                this._identity = null;
                this._lastCommit = null;
                this._currentBranch = null;
                if (this._gitRepository != null)
                {
                    this._gitRepository.Dispose();
                    this._gitRepository = null;
                }
                this._disposed = true;
            }
        }

        /// <summary>
        /// Performs a health-check on the current repository and throws an argument exception in case things don't look right.
        /// </summary>
        /// <param name="methodName">Name of the calling method.</param>
        private void HasRepository(string methodName)
        {
            if (!this._hasRepository)
            {
                string msg = "CM Environment not (properly) initialised!";
                Logger.WriteError("Framework.ConfigurationManagement.CMRepositorySlt." + methodName + " >> " + msg);
                throw new InvalidOperationException(msg);
            }
        }

        /// <summary>
        /// Helper function that updates the GIT configuration of our local repository to include configured settings (core properties and/or 
        /// proxy configuration).
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
