using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using Framework.Logging;
using APIManager.SparxEA.Properties;        // Addresses the "settings" environment so we can retrieve run-time settings.

namespace Framework.Context
{
    /// <summary>
    /// The Configuration class provides an interface to the standard Microsoft configuration mechanism. 
    /// It accesses configuration properties from the configuration XML file (APIManager.dll.config in this case). 
    /// Since this is installed together with the DLL in ProgramFiles, it is READ ONLY!
    /// This module creates a user-specific (roaming) configuration that is user extensible and read-write.
    /// This configuration class only contains the low-level access mechanisms for the configuration file, it does NOT contain
    /// any configuration property definitions, since these are platform- and implementation specific. Typically, the properties
    /// are maintained by the platform-specific Context implementation class.
    /// </summary>
    internal sealed class Configuration
    {
        private static System.Configuration.Configuration _currentConfig = null;    // Static configuration options for the running program.
        private bool isOpen = false;
        private SortedList<string, string> _propertyCache;                          // We keep retrieved configuration properties in a cache to speed-up retrieval.

        /// <summary>
        /// Helper function that performs the actual retrieval of properties. 
        /// The function checks for substitution properties, which is a property name surrounded by '%' characters. This name must be a property supported by the configuration.
        /// The function replaces this name by the value of the property.
        /// </summary>
        /// <param name="key">Property key.</param>
        /// <returns>Property value or empty string if not found (or no contents)</returns>
        internal string GetProperty(string key)
        {
            if (!isOpen) return string.Empty;

            if (this._propertyCache.ContainsKey(key)) return this._propertyCache[key];

            string outputValue = string.Empty;
            KeyValueConfigurationElement element = _currentConfig.AppSettings.Settings[key];

            // If we have an element and it contains a '%' character, we might have to perform element substitution... We check the '%' first to avoid
            // having to go through expensive regex. parsing since 90% of the configuration items don't need this.
            if (element != null && !string.IsNullOrEmpty(element.Value))
            {
                if (element.Value.Contains("%"))
                {
                    int startIndex = 0;                 // We will use this to keep track of where to start getting substrings from the original input string.
                    const string substitutionPattern = @"%\w+%";    // Matches any word surrounded by '%' (but without special characters/whitespace).
                    var regex = new Regex(substitutionPattern);
                    MatchCollection matches = regex.Matches(element.Value);  // Find all occurences of the pattern in this value string.
                    foreach (Match match in matches)
                    {
                        // We found an occurence, retrieve property and replace by property value...
                        string property = match.Value.Substring(1, match.Value.Length - 2);  // Extract the enclosing '%' characters... 
                        string propertyValue = GetProperty(property);                        // Recursive retrieval...
                        outputValue += String.Concat(element.Value.Substring(startIndex, match.Index - startIndex), propertyValue);
                        startIndex = match.Index + match.Length;
                    }
                    if (startIndex < element.Value.Length)
                    {
                        // We have no substitution properties, or there is still a part left in the input string...
                        outputValue += element.Value.Substring(startIndex, element.Value.Length - startIndex);
                    }
                }
                else outputValue = element.Value;
            }
            this._propertyCache.Add(key, outputValue);
            return outputValue;
        }

        /// <summary>
        /// Returns all properties that have the same key. This DOES NOT support substitution, so each property must be self-contained.
        /// Since the configuration does not support identical keys, we append a sequence number, starting with 01.
        /// The method does not really tests these sequence numbers explicitly, it only assumes that the configuration key 'starts with'
        /// the provided key. So any mechanism that creates unique keys is valid, as long as the key name is in the first part.
        /// This method does not use the property cache.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>List of all property values that share the specified key.</returns>
        internal List<string> GetPropertyList(string key)
        {
            var retVal = new List<string>();
            if (!isOpen) return retVal;

            foreach (KeyValueConfigurationElement kvElement in _currentConfig.AppSettings.Settings)
            {
                if (kvElement.Key.StartsWith(key) && !string.IsNullOrEmpty(kvElement.Value)) retVal.Add(kvElement.Value);
            }
            return retVal;
        }

        /// <summary>
        /// Helper function used to implement property updates. 
        /// The operation does not attempt to check whether the new value is indeed different from the old value.
        /// </summary>
        /// <param name="key">Key of the property to update.</param>
        /// <param name="value">The new property value.</param>
        internal void SetProperty(string key, string value)
        {
            KeyValueConfigurationElement element = _currentConfig.AppSettings.Settings[key];
            if (element != null && key != String.Empty)
            {
                _currentConfig.AppSettings.Settings[key].Value = value;
                _currentConfig.Save(ConfigurationSaveMode.Minimal);
                ConfigurationManager.RefreshSection("appSettings");
                if (this._propertyCache.ContainsKey(key)) this._propertyCache[key] = value;
                else this._propertyCache.Add(key, value);
            }
        }

        /// <summary>
        /// Default constructor attempts to open and read the assembly configuration and prepares for queries.
        /// </summary>
        internal Configuration()
        {
            try
            {
                if (!isOpen)
                {
                    LoadConfiguration();
                    this._propertyCache = new SortedList<string, string>();
                    if (_currentConfig != null) isOpen = true;
                }
            }
            catch (System.SystemException exc)
            {
                Logger.WriteError("Framework.Context.ConfigurationSlt >> initialization failed because: " + exc.ToString());
            }
        }

        /// <summary>
        /// Helper function that assures that we have a user-specific profile as well as a static configuration.
        /// </summary>
        private void LoadConfiguration()
        {
            // Retrieves the applicable profile for this application and user. This can be either roaming- or local depending on configuration.
            // This profile contains all user-defined settings ('userSettings' section), NOT the static properties!
            var userConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);

            // Since this is a DLL that is part of EA, we will now get an entry 'somewhere' three-levels down in the Sparx EA config path.
            // Since this location depends on the version of EA that is currently installed, we want to make a copy of the settings
            // in order to be able to restore them when new EA versions are installed.
            string configFileName = Path.GetFileName(userConfig.FilePath);
            string configDirectory = Directory.GetParent(userConfig.FilePath).Parent.Parent.Parent.FullName;
            string userConfigBackup = configDirectory + @"\APIManager\userConfigBackup";

            // If our current configuration returns a missing file, this implies that we either have never been here before, or we
            // have installed a new application release that has its own configuration (which has not been defined yet).
            // Let's see whether we have a backup that we can re-use...
            if (!userConfig.HasFile)
            {
                if (File.Exists(userConfigBackup))
                {
                    // We don't have an application config file, but we DO have a top-level backup. 
                    // Copy this backup to the profile path and re-initialize...
                    Directory.CreateDirectory(Directory.GetParent(userConfig.FilePath).FullName);
                    File.Copy(userConfigBackup, userConfig.FilePath);
                    Settings.Default.Reload();      // Make sure that the newly loaded configuration is actually used.
                }
            }
            else userConfig.SaveAs(userConfigBackup); // If we have a user config, save as a backup.

            // Next, we assign our static configuration to the static configuration file for our assembly....
            _currentConfig = ConfigurationManager.OpenExeConfiguration(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }
    }
}
