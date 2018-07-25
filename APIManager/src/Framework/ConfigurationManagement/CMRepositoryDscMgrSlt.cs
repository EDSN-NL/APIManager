using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using Framework.Util;
using Framework.Logging;
using Framework.Model;
using APIManager.SparxEA.Properties;        // Addresses the "settings" environment so we can retrieve run-time settings.

namespace Framework.ConfigurationManagement
{
    /// <summary>
    /// Manages the set of repository descriptors available for projects. Each descriptor contains information regarding the local- and remote
    /// repositories available for creation and management of model artifacts (Interface Contracts, Message Schemas, Model Schemas, etc.).
    /// </summary>
    sealed internal class CMRepositoryDscManagerSlt
    {
        private static readonly CMRepositoryDscManagerSlt _repositoryMgrSlt = new CMRepositoryDscManagerSlt();  // The singleton repository manager instance.
        private SortedList<string, RepositoryDescriptor> _cmDescriptors;                                        // Our "descriptor cache".

        /// <summary>
        /// Returns a read-only interface to the list of repository descriptors. Primary use is update of GUI for descriptor management.
        /// </summary>
        internal IReadOnlyList<RepositoryDescriptor> DescriptorList
        {
            get { return new ReadOnlyCollection<RepositoryDescriptor>(this._cmDescriptors.Values); }
        }

        /// <summary>
        /// Public Repository Manager "factory" method. Simply returns the static instance.
        /// </summary>
        /// <returns>Repository Manager singleton object</returns>
        internal static CMRepositoryDscManagerSlt GetRepositoryDscManagerSlt() { return _repositoryMgrSlt; }

        /// <summary>
        /// Creates a new repository descriptor using the set of provided properties. Also assures that the new descriptor is properly serialized
        /// to the user profile.
        /// </summary>
        /// <param name="properties">Properties for the new descriptor.</param>
        /// <returns>True when succesfully added, false on duplicate name.</returns>
        internal bool AddDescriptor(RepositoryDescriptor.DescriptorProperties properties)
        {
            bool result = false;
            if (!this._cmDescriptors.ContainsKey(properties._name))
            {
                RepositoryDescriptor newDsc = new RepositoryDescriptor(properties);
                this._cmDescriptors.Add(newDsc.Name.ToLower(), newDsc);
                result = true;
            }
            return result;
        }

        /// <summary>
        /// The method removes ALL repository configurations from the user profile.
        /// </summary>
        internal void DeleteAllDescriptors()
        {
            Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositoryDscManagerSlt.DeleteAllDescriptors >> Purging administration...");
            Settings.Default.RepositoryDescriptorList = string.Empty;
            Settings.Default.Save();
        }

        /// <summary>
        /// The method removes the specified descriptor from the user profile. If not found, no operations are performed.
        /// </summary>
        /// <param name="repoName">Name of repository descriptor to delete.</param>
        internal void DeleteDescriptor(string repoName)
        {
            string currentList = Settings.Default.RepositoryDescriptorList;
            if (!string.IsNullOrEmpty(currentList))
            {
                var xmlConfig = new XmlDocument();
                xmlConfig.LoadXml(Compression.StringUnzip(currentList));
                XmlNode myNode = xmlConfig.SelectSingleNode("RepositoryList/Repository[@name='" + repoName + "']");
                if (myNode != null)
                {
                    XmlNode repoNode = xmlConfig.SelectSingleNode("RepositoryList");
                    Logger.WriteInfo("Framework.ConfigurationManagement.CMRepositoryDscManagerSlt.DeleteDescriptor >> Removing descriptor '" + repoName + "'...");
                    repoNode.RemoveChild(myNode);

                    using (var stringWriter = new StringWriter())
                    using (var xmlTextWriter = XmlWriter.Create(stringWriter))
                    {
                        xmlConfig.WriteTo(xmlTextWriter);
                        xmlTextWriter.Flush();
                        Settings.Default.RepositoryDescriptorList = Compression.StringZip(stringWriter.GetStringBuilder().ToString());
                        Settings.Default.Save();
                    }

                    this._cmDescriptors.Remove(repoName.ToLower());
                }
            }
        }

        /// <summary>
        /// The method updates the descriptor with the specified name with one or more new/updated properties. If the update would result in
        /// a name clash, no action is performed and thhe function returns false.
        /// </summary>
        /// <param name="repoName">Original descriptor name.</param>
        /// <param name="properties">Set of properties to add/update.</param>
        /// <returns>True when updated successfully, false on name clash.</returns>
        internal bool EditDescriptor(string repoName, RepositoryDescriptor.DescriptorProperties properties)
        {
            bool result = false;
            if (repoName == properties._name || !this._cmDescriptors.ContainsKey(properties._name))
            {
                RepositoryDescriptor dsc = this._cmDescriptors[repoName.ToLower()];
                dsc.Properties = properties;

                if (repoName != properties._name)
                {
                    this._cmDescriptors.Remove(repoName);
                    this._cmDescriptors.Add(properties._name.ToLower(), dsc);
                }
                result = true;
            }
            return result;
        }

        /// <summary>
        /// The function returns true when a repository descriptor with the given name exists in the list.
        /// </summary>
        /// <param name="repoName">Name to be tested.</param>
        /// <returns>True when name exists, false when not found.</returns>
        internal bool Exists(string repoName)
        {
            return this._cmDescriptors.ContainsKey(repoName.ToLower());
        }

        /// <summary>
        /// Returns the repository descriptor with the given name or NULL when nothing found.
        /// </summary>
        /// <param name="repoName">Name of descriptor to locate.</param>
        /// <returns>Descriptor or NULL when nothing found.</returns>
        internal RepositoryDescriptor Find(string repoName)
        {
            string key = repoName.ToLower();
            return this._cmDescriptors.ContainsKey(key) ? this._cmDescriptors[key] : null;
        }

        /// <summary>
        /// Returns the repository descriptor that matches the name of the currently opened project.
        /// When no such descriptor exists, the function returns NULL.
        /// </summary>
        /// <returns>Descriptor or NULL when nothing found.</returns>
        internal RepositoryDescriptor GetCurrentDescriptor()
        {
            string key = ModelSlt.GetModelSlt().GetModelName().ToLower();
            return this._cmDescriptors.ContainsKey(key) ? this._cmDescriptors[key] : null;
        }

        /// <summary>
        /// Creates the actual descriptor manager instance. Loads all repository descriptors for quick access.
        /// </summary>
        private CMRepositoryDscManagerSlt()
        {
            this._cmDescriptors = new SortedList<string, RepositoryDescriptor>();
            string currentList = Settings.Default.RepositoryDescriptorList;
            if (!string.IsNullOrEmpty(currentList))
            {
                XmlDocument xmlConfig = new XmlDocument();
                xmlConfig.LoadXml(Compression.StringUnzip(Settings.Default.RepositoryDescriptorList));
                foreach (XmlNode configNode in xmlConfig.SelectNodes("RepositoryList/Repository"))
                {
                    RepositoryDescriptor dsc = new RepositoryDescriptor(configNode);
                    this._cmDescriptors.Add(dsc.Name.ToLower(), dsc);
                }
            }
        }
    }
}
