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
    /// model repository. It combines the local model with local CM settings and a corresponding remote.
    /// </summary>
    sealed internal class RepositoryDescriptor
    {
        /// <summary>
        /// Simple structure containing a set of 'editable' properties to be used to manage descriptors through a user interface.
        /// This is a copy of all descriptor properties, without means for serialization.
        /// </summary>
        internal struct DescriptorProperties
        {
            internal string _name;
            internal string _description;
            internal string _localPath;
            internal bool _useCM;
            internal string _GITIgnore;
            internal Uri _remoteURL;
            internal Uri _remoteNamespace;
            internal SecureString _password;
            internal Identity _identity;
        }

        // We use this as an extra security precaution when encrypting/decrypting settings. Don't change this value or you can't retrieve
        // existing encrypted values anymore!
        internal const string _Salt = "ConfigurationSaltyStuff";

        private string _repoName;               // Corresponds with the name of the modelling repository.
        private string _description;            // Generic, descriptive text for this repository.
        private string _localRootPath;          // Absolute path to local repository;
        private bool _useCM;                    // Set to 'true' to enable Configuration MAnagement for this repository.
        private string _GITIgnore;              // List of tokens to be ignored by GIT.

        private Uri _remoteURL;                 // URL to access remote repository (root).
        private Uri _remoteNamespace;           // Relative path from remoteURL to the actual GIT repository.
        private SecureString _password;         // Password or access token, depending on remote configuration.
        private Identity _identity;             // Identifies the current user by username and e- mail.

        private bool _dirty;                    // Used to determine whether or not we must serialize the object.

        /// <summary>
        /// Get- or set the repository descriptor name.
        /// </summary>
        internal string Name
        {
            get { return this._repoName?? string.Empty; }
            set { if (value != this._repoName) { this._repoName = value; this._dirty = true; } }
        }

        /// <summary>
        /// Get- or set the repository description.
        /// </summary>
        internal string Description
        {
            get { return this._description?? string.Empty; }
            set { if (value != this._description) { this._description = value; this._dirty = true; } }
        }

        /// <summary>
        /// Get- or set the local root path for te repository.
        /// </summary>
        internal string LocalRootPath
        {
            get { return this._localRootPath?? string.Empty; }
            set { if (value != this._localRootPath) { this._localRootPath = value; this._dirty = true; } }
        }

        /// <summary>
        /// Get- or set the 'CM enabled' indicator
        /// </summary>
        internal bool IsCMEnabled
        {
            get { return this._useCM; }
            set { if (value != this._useCM) { this._useCM = value; this._dirty = true; } }
        }

        /// <summary>
        /// Get- or set the local root path for te repository.
        /// </summary>
        internal string GITIgnoreList
        {
            get { return this._GITIgnore ?? string.Empty; }
            set { if (value != this._GITIgnore) { this._GITIgnore = value; this._dirty = true; } }
        }

        /// <summary>
        /// Get- or set the remote repository URL (repository root).
        /// </summary>
        internal Uri RemoteURL
        {
            get { return this._remoteURL; }
            set { if (value != this._remoteURL) { this._remoteURL = value; this._dirty = true; } }
        }

        /// <summary>
        /// Get- or set the relative path from remote URL to te actual repository.
        /// </summary>
        internal Uri RemoteRepositoryNamespace
        {
            get { return this._remoteNamespace; }
            set { if (value != this._remoteNamespace) { this._remoteNamespace = value; this._dirty = true; } }
        }

        /// <summary>
        /// Get or set the remote repository access password (or access token, depending on remote configuration).
        /// </summary>
        internal SecureString Password
        {
            get { return this._password?? new SecureString(); }
            set { this._password = value; }
        }

        /// <summary>
        /// Get- or set the user identity (username and e-mail).
        /// If we don't have a valid identity, we return an illegal one with "Dummy.User" and "Dummy.EMail".
        /// </summary>
        internal Identity UserIdentity
        {
            get { return this._identity?? new Identity("Dummy.User", "Dummy.EMail"); }
            set { if (value != this._identity) { this._identity = value; this._dirty = true; } }
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
                    _localPath          = this.LocalRootPath,
                    _GITIgnore          = this.GITIgnoreList,
                    _remoteURL          = this.RemoteURL,
                    _identity           = this.UserIdentity,
                    _password           = this.Password,
                    _remoteNamespace    = this.RemoteRepositoryNamespace
                };
            }
            set
            {
                // We use properties instead of private attribute since these properly update the 'dirty' flag.
                // Advantage is that we avoid unnecessary serializations in case we assign identical property values.
                this.Name                       = value._name;
                this.Description                = value._description;
                this.IsCMEnabled                = value._useCM;
                this.LocalRootPath              = value._localPath;
                this.GITIgnoreList              = value._GITIgnore;
                this.RemoteURL                  = value._remoteURL;
                this.UserIdentity               = value._identity;
                this.Password                   = value._password;
                this.RemoteRepositoryNamespace  = value._remoteNamespace;
                Serialize();
            }
        }

        /// <summary>
        /// Specialized constructor that creates a new instance using the specified XML node as root. This MUST be a 'Repository' element.
        /// </summary>
        /// <param name="rootNode">'Repository' node to be used for creation.</param>
        internal RepositoryDescriptor(XmlNode rootNode)
        {
            Logger.WriteInfo("Framework.ConfigurationManagement.RepositoryDescriptor >> Build instance from XML node...");
            DeserializeConfiguration(rootNode);
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
            if (currentNode != null)
            {
                this._useCM = currentNode.InnerText.ToLower().Contains("true");
                Logger.WriteInfo("Framework.ConfigurationManagement.RepositoryDescriptor.DeserializeConfiguration >> Use CM Indicator set to: " + this._useCM);
            }

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

            currentNode = configNode.SelectSingleNode("Remote/RepositoryRootURL");
            if (currentNode != null)
            {
                this._remoteURL = new Uri(currentNode.InnerText, UriKind.Absolute);
                Logger.WriteInfo("Framework.ConfigurationManagement.RepositoryDescriptor.DeserializeConfiguration >> Remote URL set to: " + this._remoteURL);
            }

            currentNode = configNode.SelectSingleNode("Remote/RepositoryNamespace");
            if (currentNode != null)
            {
                this._remoteNamespace = new Uri(currentNode.InnerText, UriKind.Relative);
                Logger.WriteInfo("Framework.ConfigurationManagement.RepositoryDescriptor.DeserializeConfiguration >> Remote namespace set to: " + this._remoteNamespace);
            }

            currentNode = configNode.SelectSingleNode("Remote/User/UserName");
            if (currentNode != null)
            {
                string userName = currentNode.InnerText;
                currentNode = configNode.SelectSingleNode("Remote/User/EMail");
                if (currentNode != null)
                {
                    this._identity = new Identity(userName, currentNode.InnerText);
                    Logger.WriteInfo("Framework.ConfigurationManagement.RepositoryDescriptor.DeserializeConfiguration >> Identity set to: '" +
                                     this._identity.Name + "' with e-mail: '" + this._identity.Email + "'...");
                }
            }

            currentNode = configNode.SelectSingleNode("Remote/User/Password");
            if (currentNode != null)
            {
                this._password = CryptString.Decrypt(CryptString.ToSecureString(currentNode.InnerText), _Salt);
                Logger.WriteInfo("Framework.ConfigurationManagement.RepositoryDescriptor.DeserializeConfiguration >> Password read.");
            }
        }

        /// <summary>
        /// Must be called to create a persistent version of this descriptor in the user profile, i.e. after one or more update operations.
        /// If Serialize is not called, the updates will be lost on program exit!
        /// </summary>
        private void Serialize()
        {
            if (!this._dirty == true) return;       // Nothing changed, do nothing!

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
                configNode.Attributes.Append(repoName);
                repoName.Value = this._repoName;

                XmlAttribute useCMInd = xmlConfig.CreateAttribute("useCM");
                configNode.Attributes.Append(useCMInd);
                useCMInd.Value = this._useCM? "true": "false";

                XmlNode rootPathNode = xmlConfig.CreateElement("RootPath");
                configNode.AppendChild(rootPathNode);
                rootPathNode.AppendChild(xmlConfig.CreateTextNode(this._localRootPath));

                if (this._description != string.Empty)
                {
                    XmlNode descNode = xmlConfig.CreateElement("Description");
                    configNode.AppendChild(descNode);
                    descNode.AppendChild(xmlConfig.CreateTextNode(this._description));
                }

                if (this._GITIgnore != string.Empty)
                {
                    XmlNode ignoreNode = xmlConfig.CreateElement("GITIgnore");
                    configNode.AppendChild(ignoreNode);
                    ignoreNode.AppendChild(xmlConfig.CreateTextNode(this._GITIgnore));
                }

                if (this._useCM)
                {
                    XmlNode remoteNode = xmlConfig.CreateElement("Remote");
                    configNode.AppendChild(remoteNode);
                    XmlNode userNode = xmlConfig.CreateElement("User");
                    remoteNode.AppendChild(userNode);
                    XmlNode userName = xmlConfig.CreateElement("UserName");
                    userNode.AppendChild(userName);
                    userName.AppendChild(xmlConfig.CreateTextNode(this._identity.Name));
                    XmlNode userMail = xmlConfig.CreateElement("EMail");
                    userNode.AppendChild(userMail);
                    userMail.AppendChild(xmlConfig.CreateTextNode(this._identity.Email));
                    XmlNode userPW = xmlConfig.CreateElement("Password");
                    userNode.AppendChild(userPW);
                    userPW.AppendChild(xmlConfig.CreateTextNode(CryptString.ToPlainString(CryptString.Encrypt(this._password, _Salt))));

                    XmlNode rootURLNode = xmlConfig.CreateElement("RepositoryRootURL");
                    remoteNode.AppendChild(rootURLNode);
                    rootURLNode.AppendChild(xmlConfig.CreateTextNode(this._remoteURL.ToString()));

                    XmlNode namespaceNode = xmlConfig.CreateElement("RepositoryNamespace");
                    remoteNode.AppendChild(namespaceNode);
                    namespaceNode.AppendChild(xmlConfig.CreateTextNode(this._remoteNamespace.ToString()));
                }

                using (var stringWriter = new StringWriter())
                using (var xmlTextWriter = XmlWriter.Create(stringWriter))
                {
                    xmlConfig.WriteTo(xmlTextWriter);
                    xmlTextWriter.Flush();
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
