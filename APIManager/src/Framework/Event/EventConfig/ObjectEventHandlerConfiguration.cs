using System.Collections.Generic;
using System;
using System.Xml;
using Framework.Logging;
using Framework.Context;

namespace Framework.EventConfig
{
    /// <summary>
    /// Create object event handlers from framework-specific XML configuration file.
    /// </summary>
    internal sealed class ObjectEventHandlerConfiguration
    {
        // Configuration properties for the event manager...
        private const string _ObjectEventHandlerConfigFile = "ObjectEventHandlerConfigFile";    // Identifies the name of the event handler configuration file.
        private const string _ObjectEventHandlerConfigNS   = "ObjectEventHandlerConfigNS";      // Identifies the namespace of the configuration file.

        private XmlDocument _configuration = null;
        private XmlNamespaceManager _nsManager = null;

        /// <summary>
        /// The constructor opens and parses the configuration file and prepares for querying.
        /// </summary>
        internal ObjectEventHandlerConfiguration()
        {
            try
            {
                string configFile = this.GetType().Assembly.Location;

                // Location also returns the assembly name so we remove the part after the last path-symbol and replace it with the name of 
                // our own configuration file...
                configFile = configFile.Substring(0, configFile.LastIndexOf('\\') + 1) + ContextSlt.GetContextSlt().GetConfigProperty(_ObjectEventHandlerConfigFile);

                this._configuration = new XmlDocument();
                this._configuration.LoadXml(System.IO.File.ReadAllText(configFile));

                this._nsManager = new XmlNamespaceManager(this._configuration.NameTable);
                this._nsManager.AddNamespace("ns", ContextSlt.GetContextSlt().GetConfigProperty(_ObjectEventHandlerConfigNS));
            }
            catch (Exception exception)
            {
                Logger.WriteError("Framework.EventConfig.ObjectEventHandlerConfiguration >> Exception during initialisation: " + exception.ToString());
            }
        }

        /// <summary>
        /// This internal method returns the list of object event trees, which are defined in the configuration file.
        /// </summary>
        /// <returns>List of tree descriptors or NULL when empty or on errors.</returns>
        internal List<ObjectEventListDescriptor> GetEventLists()
        {
            var list = new List<ObjectEventListDescriptor>();
            XmlNode root = this._configuration.DocumentElement;

            foreach (XmlNode node in root.SelectNodes("child::ns:EventList", this._nsManager))
            {
                list.Add(new ObjectEventListDescriptor(node, this._nsManager));
            }
            return list;
        }
    }
}