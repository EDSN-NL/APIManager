using System.Collections.Generic;
using System;
using System.Xml;
using Framework.Logging;
using Framework.Context;

namespace Framework.EventConfig
{
    /// <summary>
    /// Create event handlers from framework-specific XML configuration file.
    /// </summary>
    internal sealed class EventHandlerConfiguration
    {
        // Configuration properties for the event manager...
        private const string _EventHandlerConfigFile = "EventHandlerConfigFile";    // Identifies the name of the event handler configuration file.
        private const string _EventHandlerConfigNS   = "EventHandlerConfigNS";      // Identifies the namespace of the configuration file.

        private XmlDocument _configuration = null;
        private XmlNamespaceManager _nsManager = null;

        /// <summary>
        /// The constructor opens and parses the configuration file and prepares for querying.
        /// </summary>
        internal EventHandlerConfiguration()
        {
            try
            {
                string configFile = this.GetType().Assembly.Location;

                // Location also returns the assembly name so we remove the part after the last path-symbol and replace it with the name of 
                // our own configuration file...
                configFile = configFile.Substring(0, configFile.LastIndexOf('\\') + 1) + ContextSlt.GetContextSlt().GetConfigProperty(_EventHandlerConfigFile);

                this._configuration = new XmlDocument();
                this._configuration.LoadXml(System.IO.File.ReadAllText(configFile));

                this._nsManager = new XmlNamespaceManager(this._configuration.NameTable);
                this._nsManager.AddNamespace("ns", ContextSlt.GetContextSlt().GetConfigProperty(_EventHandlerConfigNS));
            }
            catch (Exception exception)
            {
                Logger.WriteError("Framework.EventConfig.EventHandlerConfiguration >> Exception during initialisation: " + exception.Message);
            }
        }

        /// <summary>
        /// This internal method returns the list of event trees, which are defined in the configuration file.
        /// </summary>
        /// <returns>List of tree descriptors or NULL when empty or on errors.</returns>
        internal List<EventTreeDescriptor> GetEventTrees()
        {
            var treeList = new List<EventTreeDescriptor>();
            XmlNode root = this._configuration.DocumentElement;

            foreach (XmlNode node in root.SelectNodes("child::ns:EventTree", this._nsManager))
            {
                treeList.Add(new EventTreeDescriptor(node, this._nsManager));
            }
            return treeList;
        }
    }
}