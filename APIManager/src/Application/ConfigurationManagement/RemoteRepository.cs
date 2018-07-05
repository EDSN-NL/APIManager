using System;
using System.Reflection;
using LibGit2Sharp;
using NGitLab;
using NGitLab.Models;
using Framework.Context;
using Framework.Logging;

namespace Plugin.Application.ConfigurationManagement
{
    /// <summary>
    /// The RemoteRepository class implements an interface between our APIManager plugin and the GitLab remote CI/CD platform.
    /// The class wraps all functionality required to access the repository, create/manage our group structure and issue Git commands.
    /// It is a statefull class that maintains a session with a single service repository.
    /// </summary>
    sealed internal class RemoteRepository
    {
        private const string _RemoteName = "origin";                // The default name we use for our remote GIT repository.

        // Configuration settings used by this module...
        private const string _CMAPIVersion                          = "CMAPIVersion";
        private const string _CMRemoteGITExtension                  = "CMRemoteGITExtension";

        // These are a set of static configuration properties, read from user configuration...
        private static string _userName;                            // Configured remote user name.
        private static string _userEMail;                           // Configured remote user e-mail.
        private static string _accessToken;                         // Configured remote user access token (to be used as password).
        private static string _repositoryBaseURL;                   // Our GitLab URL.
        private static string _repositoryNamespace;                 // References root of our repository, RELATIVE to BaseURL.
        private static string _repositoryURL;                       // Fully qualified repository URL for our service.
        private static UsernamePasswordCredentials _remoteCredentials;   // Remote credentials based on userName, EMail and access token.
        private static bool _configLoaded = false;                  // Prevents us from re-loading these each time around. 

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
        internal RemoteRepository(Repository repositoryObject)
        {
            Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository >> Connecting with remote repository...");
            this._repository = repositoryObject;
            this._myIdentity = null;

            LoadConfiguration();

            //InitializeRemote();     // Make sure that the remote repository actually exists...

            // If the remote repository is not registered yet, we perform an explicit registration.
            if (this._repository.Network.Remotes[_RemoteName] == null)
            {
                Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository >> Explicit registration of remote repository '" + _RemoteName + "'...");
                this._myRemote = this._repository.Network.Remotes.Add(_RemoteName, _repositoryURL);
            }
            else
            {
                this._myRemote = this._repository.Network.Remotes[_RemoteName];
                if (this._myRemote.Url != _repositoryURL)
                {
                    Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository >> Registered URL '" + this._myRemote.Url + 
                                     "' is different from configured URL '" + _repositoryURL + "'; updating registration...");
                    this._repository.Network.Remotes.Remove(_RemoteName);
                    this._myRemote = this._repository.Network.Remotes.Add(_RemoteName, _repositoryURL);
                }
            }
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
        internal static string Clone(string localRepositoryRootPath)
        {
            try
            {
                LoadConfiguration();

                Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository.Clone >> Cloning '" + _repositoryURL +
                                 "' to local path '" + localRepositoryRootPath + "'...");
                var cloneOptions = new CloneOptions();
                cloneOptions.CredentialsProvider = (url, user, cred) => _remoteCredentials;
                return Repository.Clone(_repositoryURL, localRepositoryRootPath, cloneOptions);
            }
            catch (Exception exc)
            {
                string message = "Framework.ConfigurationManagement.RemoteRepository.Clone >> Unable to clone remote repository to local path '" + 
                                 localRepositoryRootPath + "' because: " + Environment.NewLine + exc.Message;
                Logger.WriteError(message);
                throw new ArgumentException(message);
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
        /// Pull all changed data from the tracked branches on my remote repository and merge on my current branch.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the pull resulted in merge conflicts.</exception>
        internal void Pull()
        {
            Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository.Pull >> Going to pull from remote...");

            var pullOptions = new PullOptions();
            pullOptions.FetchOptions = new FetchOptions();
            pullOptions.FetchOptions.CredentialsProvider = (url, user, cred) => _remoteCredentials;
            Signature authorSig = new Signature(this._myIdentity, DateTime.Now);
            MergeResult result = Commands.Pull(this._repository, authorSig, pullOptions);
            if (result.Status == MergeStatus.Conflicts)
            {
                string message = "Framework.ConfigurationManagement.RemoteRepository.Pull >> Unable to pull from remote due to merge conflicts!";
                Logger.WriteError(message);
                throw new ArgumentException(message);
            }
        }

        /// <summary>
        /// Pushes the most recent commit on the specified branch to the same branch on the remote repository. The function also creates an
        /// annotated tag using the provided message and pushes the tag to remote as well.
        /// </summary>
        internal void Push(LibGit2Sharp.Branch branch, string tagName, string message)
        {
            Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository.Push >> Going to push changes on branch '" +
                              branch.FriendlyName + "' to remote, using tag '" + tagName + "'...");

            // Create an annotated tag for the most recent commit...
            LibGit2Sharp.Tag tag = this._repository.ApplyTag(tagName, new Signature(this._myIdentity, DateTime.Now), message);

            var pushOptions = new PushOptions();
            pushOptions.CredentialsProvider = (url, user, cred) => _remoteCredentials;
            this._repository.Network.Push(branch, pushOptions);
            this._repository.Network.Push(this._myRemote, tag.CanonicalName, pushOptions);
        }

        /// <summary>
        /// Pushes the most recent commit on the specified branch to the same branch on the remote repository. No tags are exchanged.
        /// If no work has been committed on the branch, only the branch itself is pushed.
        /// </summary>
        internal void Push(LibGit2Sharp.Branch branch)
        {
            Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository.Push >> Going to push changes on branch '" +
                              branch.FriendlyName + "' to remote...");

            var pushOptions = new PushOptions();
            pushOptions.CredentialsProvider = (url, user, cred) => _remoteCredentials;
            this._repository.Network.Push(branch, pushOptions);
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
            Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository.UpdateBranches >> Update branch administration on remote...");

            // Below must be read as:
            // The branch pointed at by the HEAD on this._repository will be, by default, configured to track a branch bearing 
            // the same name (this._repository.Head.CanonicalName) in the distant repository identified by myRemote.
            this._repository.Branches.Update(this._repository.Head,
                                             b => b.Remote = this._myRemote.Name,
                                             b => b.UpstreamBranch = this._repository.Head.CanonicalName);
        }

        /**********************
        /// <summary>
        /// Helper function that checks whether the remote repository already exists. If not, a new repository is created.
        /// On return, we should have a valid connection with our remote repository.
        /// </summary>
        /// <exception cref="InvalidOperationException">Is thrown when remote configuration is found to be missing or incomplete.</exception>
        private void InitializeRemote()
        {
            string extension = ContextSlt.GetContextSlt().GetConfigProperty(_CMRemoteGITExtension);
            Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository.InitializeRemote >> Checking remote URL '" + this._repositoryURL + "'...");
            NGitLab.Impl.Api.ApiVersion version = ContextSlt.GetContextSlt().GetConfigProperty(_CMAPIVersion).Contains("v4") ? NGitLab.Impl.Api.ApiVersion.V4 : 
                                                                                                                               NGitLab.Impl.Api.ApiVersion.V3;
            GitLabClient client = GitLabClient.Connect(_repositoryBaseURL, _accessToken, version);
            try
            {
                Project p = client.Projects.Get(this._repositoryNamespace + "/" + this._repositoryName);
                string s = p.NameWithNamespace;
            }
            catch (Exception e)
            {
                string msg = e.Message;
            }
            try
            {
                Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository.InitializeRemote >> Checking '" + this._repositoryNamespace + "/" + this._repositoryName + "'...");
                Project project = client.Projects.Get(this._repositoryNamespace + "/" + this._repositoryName);
                Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository.InitializeRemote >> Remote repository exists with ID '" + project.Id + "'...");
                this._repositoryID = project.Id;
            }
            catch (Exception exc)
            {
                if (exc.Message.Contains("404"))    // Not found: requested repository does not exist.
                {
                    Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository.InitializeRemote >> Remote repository does not yet exist, creating one...");
                    int namespaceID = -1;
                    foreach (var ns in client.Groups.GetNamespaces())
                    {
                        Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository.InitializeRemote >> Reading name '" + ns.Name + "'...");
                        if (ns.Name == this._repositoryGroupName)
                        {
                            namespaceID = ns.Id;
                            Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository.InitializeRemote >> Found our namspaceID to be '" + namespaceID + "'...");
                            break;
                        }
                    }
                    if (namespaceID != -1)
                    {
                        ProjectCreate newProject = new ProjectCreate();
                        newProject.VisibilityLevel = VisibilityLevel.Internal;
                        newProject.Name = this._repositoryName;
                        newProject.NamespaceId = namespaceID;
                        this._repositoryID = client.Projects.Create(newProject).Id;
                        Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository.InitializeRemote >> Remote repository created with ID '" + this._repositoryID + "'...");
                    }
                    else
                    {
                        string message = "Framework.ConfigurationManagement.RemoteRepository.InitializeRemote >> Mandatory remote group '" + this._repositoryGroupName + "' does not exist!";
                        Logger.WriteError(message);
                        throw new InvalidOperationException(message); 
                    }
                }
                else
                {
                    string message = "Framework.ConfigurationManagement.RemoteRepository.InitializeRemote >> Locating remote repository failed because: " + exc.Message;
                    Logger.WriteError(message);
                    throw;
                }
            }
        }
        ***********************/

        /// <summary>
        /// Helper method that initializes all static configuration items.
        /// </summary>
        /// <exception cref="InvalidOperationException">Is thrown when configuration items are missing.</exception>
        private static void LoadConfiguration()
        {
            if (!_configLoaded)
            {
                ContextSlt context = ContextSlt.GetContextSlt();
                _userName = context.GetStringSetting(FrameworkSettings._GLUserName);
                _userEMail = context.GetStringSetting(FrameworkSettings._GLEMail);
                _accessToken = context.GetStringSetting(FrameworkSettings._GLAccessToken, true);
                _repositoryBaseURL = context.GetStringSetting(FrameworkSettings._GLRepositoryBaseURL);
                _repositoryNamespace = context.GetStringSetting(FrameworkSettings._GLRepositoryNamespace);
                _repositoryURL = _repositoryBaseURL + "/" + _repositoryNamespace;

                Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository.LoadConfiguration >> Repository URL set to: '" + _repositoryURL + "'...");

                if (string.IsNullOrEmpty(_repositoryBaseURL) || string.IsNullOrEmpty(_repositoryNamespace))
                {
                    string message = "Framework.ConfigurationManagement.RemoteRepository.LoadConfiguration >> Configuration items are not properly initialized!";
                    Logger.WriteError(message);
                    throw new InvalidOperationException(message);
                }

                _remoteCredentials = new UsernamePasswordCredentials
                {
                    Username = _userName,
                    Password = _accessToken
                };

                _configLoaded = true;
                Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository.LoadConfiguration >> Retrieved remote repository '" + _repositoryBaseURL + "'...");
                Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository.LoadConfiguration >> User '" + _userName + "', with e-mail '" + _userEMail + "'...");
                Logger.WriteInfo("Framework.ConfigurationManagement.RemoteRepository.LoadConfiguration >> Repository namespace set to '" + _repositoryNamespace + "'...");
            }
        }
    }
}
