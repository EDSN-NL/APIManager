using System;
using System.IO;
using System.Security;
using System.Xml;
using LibGit2Sharp;
using Framework.Logging;
using Framework.Util;
using APIManager.SparxEA.Properties;        // Addresses the "settings" environment so we can retrieve run-time settings.

namespace Framework.ConfigurationManagement
{
    /// <summary>
    /// The Repository Descriptor is a configuration item that is used to store repository settings. One such descriptor must be used for each
    /// model repository. It combines the local model with local CM settings, a corresponding remote and a Jira Project.
    /// </summary>
    sealed internal class RepositoryDescriptor
    {
        /// <summary>
        /// Simple structure containing a set of 'editable' properties to be used to manage descriptors through a user interface.
        /// This is a copy of all descriptor properties, without means for serialization.
        /// </summary>
        internal struct DescriptorProperties
        {
            // Generic settings...
            internal string _name;
            internal string _description;
            internal string _localPath;
            internal bool _useCM;           // Configuration Management (GIT)
            internal bool _useRM;           // Release Management (JIRA)

            // GIT settings...
            internal string _GITIgnore;
            internal Uri _remoteURL;
            internal Uri _remoteNamespace;
            internal SecureString _remotePassword;
            internal Identity _identity;

            // Jira settings...
            internal Uri _jiraURL;
            internal string _jiraUser;
            internal SecureString _jiraPassword;
        }

        // We use this as an extra security precaution when encrypting/decrypting settings. Don't change this value or you can't retrieve
        // existing encrypted values anymore!
        internal const string _Salt = "ConfigurationSaltyStuff";

        private string _repoName;               // Corresponds with the name of the modelling repository.
        private string _description;            // Generic, descriptive text for this repository.
        private string _localRootPath;          // Absolute path to local repository;
        private bool _useCM;                    // Set to 'true' to enable Configuration Management for this repository.
        private bool _useRM;                    // Set to 'true' to enable Release Management for this repository.
        private string _GITIgnore;              // List of tokens to be ignored by GIT.

        private Uri _remoteURL;                 // URL to access remote repository (root).
        private Uri _remoteNamespace;           // Relative path from remoteURL to the actual GIT repository.
        private SecureString _remotePassword;   // Password or access token, depending on remote configuration.
        private Identity _identity;             // Identifies the current user by username and e- mail.

        // Jira settings...
        private Uri _jiraURL;                   // Location of Jira platform;
        private string _jiraUser;               // Username for Jira.
        private SecureString _jiraPassword;     // Password or access token for Jira.

        private bool _dirty;                    // Used to determine whether or not we must serialize the object.

        /// <summary>
        /// Get- or set the repository descriptor name.
        /// </summary>
        internal string Name
        {
            get { return this._repoName?? string.Empty; }
            set
            {
                string repoName = value.Trim();
                if (string.IsNullOrEmpty(this._repoName) || repoName != this._repoName)
                {
                    this._repoName = repoName;
                    this._dirty = true;
                }
            }
        }

        /// <summary>
        /// Get- or set the repository description.
        /// </summary>
        internal string Description
        {
            get { return this._description?? string.Empty; }
            set { if (string.IsNullOrEmpty(this._description) || value != this._description) { this._description = value; this._dirty = true; } }
        }

        /// <summary>
        /// Get- or set the local root path for the repository.
        /// </summary>
        internal string LocalRootPath
        {
            get { return this._localRootPath?? string.Empty; }
            set { if (string.IsNullOrEmpty(this._localRootPath) || value != this._localRootPath) { this._localRootPath = value; this._dirty = true; } }
        }

        /// <summary>
        /// Get- or set the 'CM enabled' indicator.
        /// Note: When CM is disabled, RM will be disabled as well!
        /// </summary>
        internal bool IsCMEnabled
        {
            get { return this._useCM; }
            set
            {
                if (value != this._useCM)
                {
                    this._useCM = value;
                    if (this._useCM == false) this._useRM = false;
                    this._dirty = true;
                }
            }
        }

        /// <summary>
        /// Get- or set the 'RM enabled' indicator
        /// Note: When CM is disabled, RM will be disabled as well! This also implies that you can NOT set RM to true 
        /// as long as CM is disabled.
        /// </summary>
        internal bool IsRMEnabled
        {
            get { return this._useRM; }
            set
            {
                if (value != this._useRM && this._useCM)
                {
                    this._useRM = value;
                    this._dirty = true;
                }
            }
        }

        /// <summary>
        /// Get- or set the local root path for te repository.
        /// </summary>
        internal string GITIgnoreList
        {
            get { return this._GITIgnore ?? string.Empty; }
            set { if (string.IsNullOrEmpty(this._GITIgnore) || value != this._GITIgnore) { this._GITIgnore = value; this._dirty = true; } }
        }
        
        /// <summary>
        /// Get- or set the Jira URL.
        /// </summary>
        internal Uri JiraURL
        {
            get { return this._jiraURL; }
            set { if (this._jiraURL == null || value != this._jiraURL) { this._jiraURL = value; this._dirty = true; } }
        }

        /// <summary>
        /// Get- or set the remote repository URL (repository root).
        /// </summary>
        internal Uri RemoteURL
        {
            get { return this._remoteURL; }
            set { if (this._remoteURL == null || value != this._remoteURL) { this._remoteURL = value; this._dirty = true; } }
        }

        /// <summary>
        /// Get- or set the relative path from remote URL to te actual repository.
        /// </summary>
        internal Uri RemoteRepositoryNamespace
        {
            get { return this._remoteNamespace; }
            set { if (this._remoteNamespace == null || value != this._remoteNamespace) { this._remoteNamespace = value; this._dirty = true; } }
        }

        /// <summary>
        /// Get or set the remote repository access password (or access token, depending on remote configuration).
        /// </summary>
        internal SecureString RepositoryPassword
        {
            get { return this._remotePassword?? new SecureString(); }
            set { if (!CryptString.IsEqual(this._remotePassword, value)) { this._remotePassword = value; this._dirty = true; } }
        }

        /// <summary>
        /// Get or set the Jira password (or access token, depending on remote configuration).
        /// </summary>
        internal SecureString JiraPassword
        {
            get { return this._jiraPassword ?? new SecureString(); }
            set { if (!CryptString.IsEqual(this._jiraPassword, value)) { this._jiraPassword = value; this._dirty = true; } }
        }

        /// <summary>
        /// Get or set the Jira user name.
        /// </summary>
        internal string JiraUserName
        {
            get { return this._jiraUser ?? string.Empty; }
            set { if (string.IsNullOrEmpty(this._jiraUser) || this._jiraUser != value) { this._jiraUser = value; this._dirty = true; } }
        }

        /// <summary>
        /// Get- or set the user identity (username and e-mail).
        /// If we don't have a valid identity, we return an illegal one with "Dummy.User" and "Dummy.EMail".
        /// </summary>
        internal Identity RepositoryUserID
        {
            get { return this._identity?? new Identity("Dummy.User", "Dummy.EMail"); }
            set
            {
                if (value != null && (this._identity == null || value.Email != this._identity.Email || value.Name != this._identity.Name))
                {
                    this._identity = new Identity(value.Name, value.Email);
                    this._dirty = true;
                }
            }
        }

        /// <summary>
        /// Get- and set all descriptor properties in one go. Primary use is interaction with te GUI for property management.
        /// </summary>
        internal DescriptorProperties Properties
        {
            get
            {
                return new DescriptorProperties
                {
                    _name               = this.Name,
                    _description        = this.Description,
                    _useCM              = this.IsCMEnabled,
                    _useRM              = this.IsRMEnabled,
                    _localPath          = this.LocalRootPath,
                    _GITIgnore          = this.GITIgnoreList,
                    _remoteURL          = this.RemoteURL,
                    _identity           = this.RepositoryUserID,
                    _remotePassword     = this.RepositoryPassword,
                    _remoteNamespace    = this.RemoteRepositoryNamespace,
                    _jiraPassword       = this.JiraPassword,
                    _jiraURL            = this.JiraURL,
                    _jiraUser           = this.JiraUserName
                };
            }
            set
            {
                // We use properties instead of private attribute since these properly update the 'dirty' flag.
                // Advantage is that we avoid unnecessary serializations in case we assign identical property values.
                this.Name                       = value._name;
                this.Description                = value._description;
                this.IsCMEnabled                = value._useCM;
                this.IsRMEnabled                = value._useRM;
                this.LocalRootPath              = value._localPath;
                this.GITIgnoreList              = value._GITIgnore;
                this.RemoteURL                  = value._remoteURL;
                this.RepositoryUserID           = value._identity;
                this.RepositoryPassword         = value._remotePassword;
                this.RemoteRepositoryNamespace  = value._remoteNamespace;
                this.JiraPassword               = value._jiraPassword;
                this.JiraURL                    = value._jiraURL;
                this.JiraUserName               = value._jiraUser;
                Serialize();
            }
        }

        /// <summary>
        /// Specialized constructor that creates a new instance using the specified XML node as root. This MUST be a 'Repository' element.
        /// </summary>
        /// <param name="node">'Repository' node to be used for creation.</param>
        internal RepositoryDescriptor(XmlNode node)
        {
            Logger.WriteInfo("Framework.ConfigurationManagement.RepositoryDescriptor >> Build instance from XML node...");
            DeserializeConfiguration(node);
        }

        /// <summary>
        /// Descriptor used to create a new descriptor using a set of user-provided properties.
        /// </summary>
        /// <param name="properties">Set of properties for the new descriptor.</param>
        internal RepositoryDescriptor(DescriptorProperties properties)
        {
            Properties = properties;
        }

        /// <summary>
        /// Helper method that parses a single repository configuration node and extracts the config data.
        /// </summary>
        /// <param name="configNode">Retrieved XML configuration.</param>
        /// <param name="nsManager">Namespace manager for configuration.</param>
        private void DeserializeConfiguration(XmlNode configNode)
        {
            XmlNode currentNode = configNode.SelectSingleNode("@name");
            if (currentNode != null)
            {
                this._repoName = currentNode.InnerText;
                Logger.WriteInfo("Framework.ConfigurationManagement.RepositoryDescriptor.DeserializeConfiguration >> Name set to: " + this._repoName);
            }

            currentNode = configNode.SelectSingleNode("@useCM");
            this._useCM = currentNode != null ? currentNode.InnerText.ToLower().Contains("true") : false;
            Logger.WriteInfo("Framework.ConfigurationManagement.RepositoryDescriptor.DeserializeConfiguration >> Use CM Indicator set to: " + this._useCM);

            currentNode = configNode.SelectSingleNode("@useRM");
            this._useRM = currentNode != null ? currentNode.InnerText.ToLower().Contains("true") : false;
            Logger.WriteInfo("Framework.ConfigurationManagement.RepositoryDescriptor.DeserializeConfiguration >> Use RM Indicator set to: " + this._useRM);

            currentNode = configNode.SelectSingleNode("RootPath");
            if (currentNode != null)
            {
                this._localRootPath = currentNode.InnerText;
                Logger.WriteInfo("Framework.ConfigurationManagement.RepositoryDescriptor.DeserializeConfiguration >> Local Root Path set to: " + this._localRootPath);
            }

            currentNode = configNode.SelectSingleNode("Description");
            if (currentNode != null)
            {
                this._description = currentNode.InnerText;
                Logger.WriteInfo("Framework.ConfigurationManagement.RepositoryDescriptor.DeserializeConfiguration >> Description set to: " + this._description);
            }

            currentNode = configNode.SelectSingleNode("GITIgnore");
            if (currentNode != null)
            {
                this._GITIgnore = currentNode.InnerText;
                Logger.WriteInfo("Framework.ConfigurationManagement.RepositoryDescriptor.DeserializeConfiguration >> GIT Ignore list set to: " + this._GITIgnore);
            }

            currentNode = configNode.SelectSingleNode("GitRemote/RepositoryRootURL");
            if (currentNode != null)
            {
                this._remoteURL = new Uri(currentNode.InnerText, UriKind.Absolute);
                Logger.WriteInfo("Framework.ConfigurationManagement.RepositoryDescriptor.DeserializeConfiguration >> Remote URL set to: " + this._remoteURL);
            }

            currentNode = configNode.SelectSingleNode("GitRemote/RepositoryNamespace");
            if (currentNode != null)
            {
                this._remoteNamespace = new Uri(currentNode.InnerText, UriKind.Relative);
                Logger.WriteInfo("Framework.ConfigurationManagement.RepositoryDescriptor.DeserializeConfiguration >> Remote namespace set to: " + this._remoteNamespace);
            }

            currentNode = configNode.SelectSingleNode("GitRemote/User/UserName");
            if (currentNode != null)
            {
                string userName = currentNode.InnerText;
                currentNode = configNode.SelectSingleNode("GitRemote/User/EMail");
                if (currentNode != null)
                {
                    this._identity = new Identity(userName, currentNode.InnerText);
                    Logger.WriteInfo("Framework.ConfigurationManagement.RepositoryDescriptor.DeserializeConfiguration >> Identity set to: '" +
                                     this._identity.Name + "' with e-mail: '" + this._identity.Email + "'...");
                }
            }

            currentNode = configNode.SelectSingleNode("GitRemote/User/Password");
            if (currentNode != null)
            {
                this._remotePassword = CryptString.Decrypt(CryptString.ToSecureString(currentNode.InnerText), _Salt);
                Logger.WriteInfo("Framework.ConfigurationManagement.RepositoryDescriptor.DeserializeConfiguration >> Git Password read.");
            }

            currentNode = configNode.SelectSingleNode("JiraRemote/User/UserName");
            if (currentNode != null)
            {
                this._jiraUser = currentNode.InnerText;
                Logger.WriteInfo("Framework.ConfigurationManagement.RepositoryDescriptor.DeserializeConfiguration >> Jira user name set to: " + this._jiraUser);
            }

            currentNode = configNode.SelectSingleNode("JiraRemote/User/Password");
            if (currentNode != null)
            {
                this._jiraPassword = CryptString.Decrypt(CryptString.ToSecureString(currentNode.InnerText), _Salt);
                Logger.WriteInfo("Framework.ConfigurationManagement.RepositoryDescriptor.DeserializeConfiguration >> Jira Password read.");
            }

            currentNode = configNode.SelectSingleNode("JiraRemote/URL");
            if (currentNode != null)
            {
                this._jiraURL = new Uri(currentNode.InnerText, UriKind.Absolute);
                Logger.WriteInfo("Framework.ConfigurationManagement.RepositoryDescriptor.DeserializeConfiguration >> Jira URL set to: " + this._jiraURL);
            }
        }

        /// <summary>
        /// Must be called to create a persistent version of this descriptor in the user profile, i.e. after one or more update operations.
        /// If Serialize is not called, the updates will be lost on program exit!
        /// </summary>
        private void Serialize()
        {
            if (!this._dirty) return;       // Nothing changed, do nothing!
            this._dirty = false;

            XmlDocument xmlConfig = new XmlDocument();
            XmlNode containerNode = null;

            string currentList = Settings.Default.RepositoryDescriptorList;
            if (!string.IsNullOrEmpty(currentList))
            {
                xmlConfig.LoadXml(Compression.StringUnzip(currentList));
                XmlNode myNode = xmlConfig.SelectSingleNode("RepositoryList/Repository[@name='" + this._repoName + "']");
                if (myNode != null)
                {
                    // We already have a copy of this repository config. Remove this first...
                    containerNode = xmlConfig.SelectSingleNode("RepositoryList");
                    Logger.WriteInfo("Framework.ConfigurationManagement.RepositoryDescriptor.SerializeConfiguration >> Remove existing node...");
                    containerNode.RemoveChild(myNode);
                }
            }
            else
            {
                Logger.WriteInfo("Framework.ConfigurationManagement.RepositoryDescriptor.SerializeConfiguration >> No configuration yet, create skeleton...");
                containerNode = xmlConfig.CreateElement("RepositoryList");
                xmlConfig.AppendChild(containerNode);
            }

            if (containerNode == null) containerNode = xmlConfig.SelectSingleNode("RepositoryList");
            if (containerNode != null)
            {
                XmlNode configNode = xmlConfig.CreateElement("Repository");
                containerNode.AppendChild(configNode);

                XmlAttribute repoName = xmlConfig.CreateAttribute("name");
                repoName.Value = this._repoName;
                configNode.Attributes.Append(repoName);

                XmlAttribute useCMInd = xmlConfig.CreateAttribute("useCM");
                useCMInd.Value = this._useCM ? "true" : "false";
                configNode.Attributes.Append(useCMInd);

                XmlAttribute useRMInd = xmlConfig.CreateAttribute("useRM");
                useRMInd.Value = this._useRM ? "true" : "false";
                configNode.Attributes.Append(useRMInd);

                XmlNode rootPathNode = xmlConfig.CreateElement("RootPath");
                rootPathNode.AppendChild(xmlConfig.CreateTextNode(this._localRootPath));
                configNode.AppendChild(rootPathNode);

                if (this._description != string.Empty)
                {
                    XmlNode descNode = xmlConfig.CreateElement("Description");
                    descNode.AppendChild(xmlConfig.CreateTextNode(this._description));
                    configNode.AppendChild(descNode);
                }

                if (this._GITIgnore != string.Empty)
                {
                    XmlNode ignoreNode = xmlConfig.CreateElement("GITIgnore");
                    ignoreNode.AppendChild(xmlConfig.CreateTextNode(this._GITIgnore));
                    configNode.AppendChild(ignoreNode);
                }

                if (this._useCM)
                {
                    XmlNode remoteNode = xmlConfig.CreateElement("GitRemote");
                    configNode.AppendChild(remoteNode);

                    XmlNode userNode = xmlConfig.CreateElement("User");
                    remoteNode.AppendChild(userNode);
                    XmlNode userName = xmlConfig.CreateElement("UserName");
                    userName.AppendChild(xmlConfig.CreateTextNode(this._identity.Name));
                    userNode.AppendChild(userName);
                    XmlNode userMail = xmlConfig.CreateElement("EMail");
                    userMail.AppendChild(xmlConfig.CreateTextNode(this._identity.Email));
                    userNode.AppendChild(userMail);
                    XmlNode userPW = xmlConfig.CreateElement("Password");
                    userPW.AppendChild(xmlConfig.CreateTextNode(CryptString.ToPlainString(CryptString.Encrypt(this._remotePassword, _Salt))));
                    userNode.AppendChild(userPW);

                    XmlNode rootURLNode = xmlConfig.CreateElement("RepositoryRootURL");
                    rootURLNode.AppendChild(xmlConfig.CreateTextNode(this._remoteURL.ToString()));
                    remoteNode.AppendChild(rootURLNode);

                    XmlNode namespaceNode = xmlConfig.CreateElement("RepositoryNamespace");
                    namespaceNode.AppendChild(xmlConfig.CreateTextNode(this._remoteNamespace.ToString()));
                    remoteNode.AppendChild(namespaceNode);
                }

                if (this._useRM)
                {
                    XmlNode jiraRemote = xmlConfig.CreateElement("JiraRemote");
                    configNode.AppendChild(jiraRemote);

                    XmlNode jiraURLNode = xmlConfig.CreateElement("URL");
                    jiraURLNode.AppendChild(xmlConfig.CreateTextNode(this._jiraURL.ToString()));
                    jiraRemote.AppendChild(jiraURLNode);

                    XmlNode jiraUserNode = xmlConfig.CreateElement("User");
                    jiraRemote.AppendChild(jiraUserNode);
                    XmlNode jiraUserName = xmlConfig.CreateElement("UserName");
                    jiraUserName.AppendChild(xmlConfig.CreateTextNode(this._jiraUser));
                    jiraUserNode.AppendChild(jiraUserName);
                    XmlNode jiraUserPW = xmlConfig.CreateElement("Password");
                    jiraUserPW.AppendChild(xmlConfig.CreateTextNode(CryptString.ToPlainString(CryptString.Encrypt(this._jiraPassword, _Salt))));
                    jiraUserNode.AppendChild(jiraUserPW);
                }

                using (var stringWriter = new StringWriter())
                using (var xmlTextWriter = XmlWriter.Create(stringWriter))
                {
                    xmlConfig.WriteTo(xmlTextWriter);
                    xmlTextWriter.Flush();
                    Logger.WriteInfo("Framework.ConfigurationManagement.RepositoryDescriptor.SerializeConfiguration >> Created XML: " + Environment.NewLine + stringWriter.GetStringBuilder().ToString());
                    Settings.Default.RepositoryDescriptorList = Compression.StringZip(stringWriter.GetStringBuilder().ToString());
                    Settings.Default.Save();
                }
            }
            else
            {
                Logger.WriteError("Framework.ConfigurationManagement.RepositoryDescriptor.SerializeConfiguration >> Repository configuration missing or corrupt, resetting!");
                Settings.Default.RepositoryDescriptorList = string.Empty;
                Settings.Default.Save();
            }
        }
    }
}
