using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Framework.Logging;

using System.Windows.Forms;

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
        private static System.Configuration.Configuration _defaultConfig = null;    // Contains all static (=compile time) configuration keys.
        private static System.Configuration.Configuration _currentConfig = null;    // Combines static with dynamic keys and is the actual used config!
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
                    // Open DLL-specific configuration... (user-bound, roaming etc. does not work for DLL's).
                    // Below code fragment is way to primitive and does not provides the correct result. New
                    // code is present in 'LoadConfiguration'...
                    //string configPath = this.GetType().Assembly.Location;       // Obtain pointer to the assembly.
                    //_currentConfig = ConfigurationManager.OpenExeConfiguration(configPath);
                    LoadConfiguration();
                    this._propertyCache = new SortedList<string, string>();
                    if (_currentConfig != null) isOpen = true;
                }
            }
            catch (System.SystemException exc)
            {
                Logger.WriteError("Framework.Context.ConfigurationSlt >> initialization failed because: " + exc.Message);
            }
        }

        /// <summary>
        /// Helper function that creates a per-user roaming profile for the plugin (if not yet there) and maps all static configuration
        /// keys to that profile. On return, the _defaultConfig property contains all static configuration options and the _currentConfig
        /// contains all user-specific options.
        /// </summary>
        private void LoadConfiguration()
        {
            // Retrieves the roaming profile for this application and user.
            System.Configuration.Configuration roamingConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoaming);

            // Since this is a DLL that is part of EA, we will now get an entry 'somewhere' three-levels down in the Sparx EA config path.
            // We want to replace this with a 'clean' configuration file for the plugin and to support this, we have to move three-levels
            // up the directory tree and create an explicit configuration directory there...
            string configFileName = Path.GetFileName(roamingConfig.FilePath);
            string configDirectory = Directory.GetParent(roamingConfig.FilePath).Parent.Parent.Parent.FullName;
            string newConfigFilePath = configDirectory + @"\APIManager\" + configFileName;

            // Next, we map the roaming configuration file to the local '_currentConfig'. This enables the application to access the 
            // configuration file using the System.Configuration.Configuration class...
            // ConfigurationUserLevel = None --> Specifies the application configuration file.
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
            configFileMap.ExeConfigFilename = newConfigFilePath;
            _currentConfig = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

            // Next, we retrieve the static (default) configuration file for this assembly and copy all settings that are
            // not yet present in the user (roaming) config file...
            string defaultConfigFilePath = string.Empty;
            if (_defaultConfig == null)
            {
                defaultConfigFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                _defaultConfig = ConfigurationManager.OpenExeConfiguration(defaultConfigFilePath);
            }

            // Copy all keys from default config to roaming config...
            foreach (KeyValueConfigurationElement configEntry in _defaultConfig.AppSettings.Settings)
            {
                if (!_currentConfig.AppSettings.Settings.AllKeys.Contains(configEntry.Key))
                {
                    _currentConfig.AppSettings.Settings.Add(configEntry.Key, configEntry.Value);
                }
            }
            _currentConfig.Save();
        }
    }
}
