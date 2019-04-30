using System;
using System.Configuration;
using System.Security;
using System.Diagnostics;
using LibGit2Sharp;
using Framework.Context;
using Framework.Logging;
using Framework.Util;

namespace Framework.ConfigurationManagement
{
    /// <summary>
    /// The RemoteRepository class implements an interface between our APIManager plugin and the GitLab remote CI/CD platform.
    /// The class wraps all functionality required to access the repository, create/manage our group structure and issue Git commands.
    /// It is a statefull class that maintains a session with a single service repository.
    /// </summary>
    sealed internal class RemoteRepository
    {
        // some constants...
        private const string _RemoteName                            = "origin";
        private const string _TagExists                             = "tag already exists";
        private const string _NonFastForwardableRef                 = "cannot push non-fastforwardable reference";
        private const string _UnknownCertificate                    = "unknown certificate check failure";

        // Configuration settings used by this module...
        private const string _CMGitLabAPIURLSuffix                  = "CMGitLabAPIURLSuffix";
        private const string _CMRemoteGITExtension                  = "CMRemoteGITExtension";

        // These are a set of configuration properties, read from the applicable repository descriptor.
        private SecureString _password;                             // Configured remote user password.
        private string _repositoryBaseURL;                          // Our GitLab URL.
        private string _repositoryNamespace;                        // References root of our repository, RELATIVE to BaseURL.
        private string _repositoryURL;                              // Fully qualified repository URL for our service.
        private UsernamePasswordCredentials _remoteCredentials;     // Remote credentials based on userName, EMail and access token.

        private Repository _repository;                             // Our GIT repository object, containing most of the interfacing commands.
        private Remote _myRemote;                                   // Association with the remote repository.
        private Identity _myIdentity;                               // Currently active user identity for repository pull/push operations.

        /// <summary>
        /// Constructor, retrieves remote settings from configuration and creates a remote repository object associated with the specified remote group and remote name.
        /// The group must be an existing group, present below the remote root URL. The repository may- or may not exist in this group. If it exists, we simply link
        /// to it, if it does not exist, a new repository is created.
        /// </summary>
        /// <param name="repositoryName">The name of the repository that must be made available at RootURL.</param>
        /// <exception cref="InvalidOperationException">Thrown when the user identity has not yet been registered.</exception>
        /// <exception cref="ConfigurationErrorsException">Thrown when we could not retrieve a valid repository descriptor.</exception>
        internal RemoteRepository(Repository repositoryObject)
        {
            Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository >> Connecting with remote repository...");
            ContextSlt context = ContextSlt.GetContextSlt();
            RepositoryDescriptor repoDsc = CMRepositoryDscManagerSlt.GetRepositoryDscManagerSlt().GetCurrentDescriptor();
            this._repository = repositoryObject;

            if (repoDsc == null)
            {
                string message = "Unable to retrieve proper CM repository descriptor, aborting!";
                Logger.WriteError("Framework.ConfigurationManagement.RemoteRepository >>  " + message);
                throw new ConfigurationErrorsException(message);
            }
            this._repositoryBaseURL = repoDsc.RemoteURL.AbsoluteUri;
            this._repositoryNamespace = repoDsc.RemoteRepositoryNamespace.OriginalString;
            this._myIdentity = repoDsc.RepositoryUserID;
            this._password = repoDsc.RepositoryPassword;
            this._repositoryURL = this._repositoryBaseURL + this._repositoryNamespace + context.GetConfigProperty(_CMRemoteGITExtension);

            Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository >> Repository URL set to: '" + this._repositoryURL + "'...");

            if (string.IsNullOrEmpty(this._repositoryBaseURL) || string.IsNullOrEmpty(this._repositoryNamespace))
            {
                string message = "Configuration items are not properly initialized!";
                Logger.WriteError("Framework.ConfigurationManagement.RemoteRepository >> " + message);
                throw new ConfigurationErrorsException(message);
            }

            this._remoteCredentials = new UsernamePasswordCredentials
            {
                Username = this._myIdentity.Name,
                Password = CryptString.ToPlainString(this._password)
            };
            var fetchOptions = new FetchOptions();
            fetchOptions.CredentialsProvider = (url, user, cred) => this._remoteCredentials;

            Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository >> Retrieved remote repository '" + this._repositoryBaseURL + "'...");
            Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository >> User '" + this._myIdentity.Name + "', with e-mail '" + this._myIdentity.Email + "'...");
            Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository >> Repository namespace set to '" + _repositoryNamespace + "'...");

            // If the remote repository is not registered yet, we perform an explicit registration.
            if (this._repository.Network.Remotes[_RemoteName] == null)
            {
                Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository >> Explicit registration of remote repository '" + _RemoteName + "'...");
                this._myRemote = this._repository.Network.Remotes.Add(_RemoteName, this._repositoryURL);
            }
            else
            {
                this._myRemote = this._repository.Network.Remotes[_RemoteName];
                if (this._myRemote.Url != this._repositoryURL)
                {
                    Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository >> Registered URL '" + this._myRemote.Url + 
                                     "' is different from configured URL '" + _repositoryURL + "'; updating registration...");
                    this._repository.Network.Remotes.Remove(_RemoteName);
                    this._myRemote = this._repository.Network.Remotes.Add(_RemoteName, this._repositoryURL);
                }
            }
            // Fetch branches and stuff....
            string[] refSpec = { "+refs/heads/*:refs/remotes/origin/*" };
            this._repository.Network.Fetch(_RemoteName, refSpec, fetchOptions);
            UpdateBranches();       // Assures that we're tracking the current HEAD.
        }

        /// <summary>
        /// Creates a new local repository at the specified root path by cloning the remote repository.
        /// To avoid a 'chicken-egg' problem when creating a new local repository that is clone of a remote repository, we made this a static method
        /// that can be invoked independent from the construction of RemoteRepository instances.
        /// </summary>
        /// <param name="localRepositoryRootPath">Absolute path to the root of the local repository.</param>
        /// <returns>Repository root path for created repository or empty string in case of errors.</returns>
        /// <exception cref="ArgumentException">Thrown on errors, probably because path arguments are wrong.</exception>
        /// <exception cref="ConfigurationErrorsException">Thrown when no valid repository descriptor could be read.</exception>
        internal static string Clone(string localRepositoryRootPath)
        {
            try
            {
                RepositoryDescriptor repoDsc = CMRepositoryDscManagerSlt.GetRepositoryDscManagerSlt().GetCurrentDescriptor();
                if (repoDsc == null)
                {
                    string message = "Unable to retrieve proper CM repository descriptor, aborting!";
                    Logger.WriteError("Framework.ConfigurationManagement.RemoteRepository.Clone >>  " + message);
                    throw new ConfigurationErrorsException(message);
                }
                var remoteCredentials = new UsernamePasswordCredentials
                {
                    Username = repoDsc.RepositoryUserID.Name,
                    Password = CryptString.ToPlainString(repoDsc.RepositoryPassword)
                };
                string repositoryURL = repoDsc.RemoteURL.AbsoluteUri + repoDsc.RemoteRepositoryNamespace.OriginalString;

                Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository.Clone >> Cloning '" + repositoryURL +
                                 "' to local path '" + localRepositoryRootPath + "'...");
                var cloneOptions = new CloneOptions();
                cloneOptions.CredentialsProvider = (url, user, cred) => remoteCredentials;
                return Repository.Clone(repositoryURL, localRepositoryRootPath, cloneOptions);
            }
            catch (Exception exc)
            {
                if (exc.InnerException is ConfigurationErrorsException) throw;
                string message = "Unable to clone remote repository to local path '" + 
                                 localRepositoryRootPath + "' because: " + Environment.NewLine + exc.Message;
                Logger.WriteError("Framework.ConfigurationManagement.RemoteRepository.Clone >> " + message);
                throw new ArgumentException(message, exc);
            }
        }

        /// <summary>
        /// Deletes the specified branch from remote.
        /// </summary>
        /// <param name="branch">The branch that must be deleted.</param>
        internal void DeleteBranch(LibGit2Sharp.Branch branch)
        {
            Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository.DeleteBranch >> Deleting branch '" + branch.FriendlyName + "' from remote...");
            var pushOpts = new PushOptions();
            pushOpts.CredentialsProvider = (url, user, cred) => _remoteCredentials;
            this._repository.Network.Push(this._myRemote, pushRefSpec: ":" + branch.CanonicalName, pushOptions: pushOpts);
        }

        /// <summary>
        /// Deletes the specified tag from remote.
        /// </summary>
        /// <param name="tag">The tag that must be deleted.</param>
        internal void DeleteTag(LibGit2Sharp.Tag tag)
        {
            Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository.DeleteTag >> Deleting tag '" + tag.FriendlyName + "' from remote...");
            var pushOpts = new PushOptions();
            pushOpts.CredentialsProvider = (url, user, cred) => _remoteCredentials;
            this._repository.Network.Push(this._myRemote, pushRefSpec: ":" + tag.CanonicalName, pushOptions: pushOpts);
        }

        /// <summary>
        /// Fetch all tags from remote.
        /// </summary>
        internal void FetchTags()
        {
            Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository.FetchTags >> Going to fetch tags from remote...");

            var fetchOptions = new FetchOptions();
            fetchOptions.CredentialsProvider = (url, user, cred) => _remoteCredentials;
            Signature authorSig = new Signature(this._myIdentity, DateTime.Now);
            Commands.Fetch(this._repository, this._myRemote.Name, new[]{"refs/tags/*:refs/tags/*"}, fetchOptions, "Retrieve tags from remote");
        }

        /// <summary>
        /// Pull all changed data from the tracked branches on my remote repository and merge on my current branch. If the pull failed due to
        /// a 'Merge Head not found' exception, we simply assume that the branch has never been pushed before and thus the pull is unnecessary.
        /// <paramref name="retry">Hidden parameter, set to 'true' when calling in a retry. Avoids endless loops</paramref>
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the pull resulted in merge conflicts.</exception>
        internal void Pull(bool retry = false)
        {
            Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository.Pull >> Going to pull from remote...");

            try
            {
                var pullOptions = new PullOptions();
                pullOptions.FetchOptions = new FetchOptions();
                pullOptions.FetchOptions.CredentialsProvider = (url, user, cred) => _remoteCredentials;
                Signature authorSig = new Signature(this._myIdentity, DateTime.Now);
                MergeResult result = Commands.Pull(this._repository, authorSig, pullOptions);
                if (result.Status == MergeStatus.Conflicts)
                {
                    string message = "Unable to pull from remote due to merge conflicts!";
                    Logger.WriteError("Framework.ConfigurationManagement.RemoteRepository.Pull >> " + message);
                    throw new InvalidOperationException(message);
                }
            }
            catch (LibGit2Sharp.MergeFetchHeadNotFoundException)
            {
                Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository.Pull >> Pulling the current branch from remote did not yield any results. Branch has probably never been pushed!");
            }
            catch (LibGit2Sharp.LibGit2SharpException exc)
            {
                if (exc.Message.Contains(_UnknownCertificate) && !retry)
                {
                    Logger.WriteWarning("Got a spurious 'unknown certificate check' error, retrying...");
                    Pull(true);
                }
                else throw;
            }
        }

        /// <summary>
        /// Pushes the most recent commit on the specified branch to the same branch on the remote repository. The function also creates an
        /// annotated tag using the provided message and pushes the tag to remote as well.
        /// </summary>
        /// <exception cref="CMOutOfSyncException">Exception is trown on duplicate tags, which typically implies that build numbers are out of sync.</exception>
        internal void Push(LibGit2Sharp.Branch branch, string tagName, string message)
        {
            PushOptions pushOptions = null;
            LibGit2Sharp.Tag tag = null;
            try
            {
                Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository.Push >> Going to push changes on branch '" +
                                  branch.FriendlyName + "' to remote, using tag '" + tagName + "'...");

                // Create an annotated tag for the most recent commit...
                tag = this._repository.ApplyTag(tagName, new Signature(this._myIdentity, DateTime.Now), message);

                pushOptions = new PushOptions();
                pushOptions.CredentialsProvider = (url, user, cred) => _remoteCredentials;
                this._repository.Network.Push(branch, pushOptions);
                this._repository.Network.Push(this._myRemote, tag.CanonicalName, pushOptions);
            }
            catch (Exception exc)
            {
                Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository.Push(tag,msg) >> Caught exception: " + exc.Message);
                if (exc.Message.Contains(_TagExists)) throw new CMOutOfSyncException("Existing tag '" + tagName + "' at remote!");
                else if (exc.Message.Contains(_NonFastForwardableRef))
                {
                    Logger.WriteWarning("Local repository is behind remote, pulling fresh data and retrying...");
                    Pull();
                    this._repository.Network.Push(branch, pushOptions);
                    this._repository.Network.Push(this._myRemote, tag.CanonicalName, pushOptions);
                }
                else throw;
            }
        }

        /// <summary>
        /// Pushes the most recent commit on the specified branch to the same branch on the remote repository. No tags are exchanged.
        /// If no work has been committed on the branch, only the branch itself is pushed.
        /// </summary>
        internal void Push(LibGit2Sharp.Branch branch)
        {
            Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository.Push >> Going to push changes on branch '" +
                              branch.FriendlyName + "' to remote...");
            try
            {
                var pushOptions = new PushOptions();
                pushOptions.CredentialsProvider = (url, user, cred) => _remoteCredentials;
                this._repository.Network.Push(branch, pushOptions);
            }
            catch (Exception exc)
            {
                Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository.Push >> Caught exception: " + exc.Message);
                if (exc.Message.Contains(_NonFastForwardableRef))
                {
                    Logger.WriteWarning("Local repository is behind remote, pulling fresh data and retrying...");
                    Pull();
                    Push(branch);
                }
                else throw;
            }
        }

        /// <summary>
        /// The identity must be loaded before files can be pulled- or pushed from/to the repository!
        /// </summary>
        /// <param name="myIdentity">Identity object for the current user.</param>
        internal void SetIdentity(Identity myIdentity)
        {
            this._myIdentity = myIdentity;
        }

        /// <summary>
        /// Method used to tell my repository that it must inform the remote repository to track my local branches using identical names.
        /// This must be called whenever a new HEAD is created so that we can tell the remote to track that new branch as well.
        /// A new HEAD is created when we create a new branch and perform a checkout on that branch.
        /// </summary>
        /// <param name="branch">(New) branch to be tracked</param>
        internal void UpdateBranches()
        {
            Debug.Assert(this._repository != null);
            Debug.Assert(this._repository.Head != null);
            Debug.Assert(this._myRemote != null);
            Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository.UpdateBranches >> Update branch administration on remote...");

            // Below must be read as:
            // The branch pointed at by the HEAD on this._repository will be, by default, configured to track a branch bearing 
            // the same name (this._repository.Head.CanonicalName) in the distant repository identified by myRemote.
            this._repository.Branches.Update(this._repository.Head,
                                             b => b.Remote = this._myRemote.Name,
                                             b => b.UpstreamBranch = this._repository.Head.CanonicalName);
            Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository.UpdateBranches >> Branches updated successfully.");
        }
    }
}
