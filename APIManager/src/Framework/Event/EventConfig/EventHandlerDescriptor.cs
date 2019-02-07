using System.Collections.Generic;
using System;
using System.Xml;
using Framework.Logging;

namespace Framework.EventConfig
{
    /// <summary>
    /// Helper class that represents a single event handler definition from configuration....
    /// </summary>
    internal sealed class EventHandlerDescriptor
    {
        private string _name = string.Empty;
        private string _handlerFQN = string.Empty;
        private List<string> _packageList = null;
        private List<string> _stereotypeList = null;
        private List<string> _classList = null;

        // Retrieve attributes...
        internal string Name                  { get { return this._name; } }
        internal string HandlerFQN            { get { return this._handlerFQN; } }
        internal List<string> PackageList     { get { return this._packageList; } }
        internal List<string> StereotypeList  { get { return this._stereotypeList; } }
        internal List<string> ClassList       { get { return this._classList; } }

        /// <summary>
        /// Constructor creates a new descriptor from an XML node.
        /// </summary>
        /// <param name="node">Node containing the definition.</param>
        internal EventHandlerDescriptor(XmlNode node, XmlNamespaceManager nsMgr)
        {
            try
            {
                XmlAttributeCollection attribs = node.Attributes;
                this._name = ((XmlAttribute)attribs.GetNamedItem("ID")).InnerText;

                // Retrieve simple names...
                XmlNode currentNode = node.SelectSingleNode("child::ns:HandlerFQN", nsMgr);
                if (currentNode != null) this._handlerFQN = currentNode.InnerText;

                currentNode = node.SelectSingleNode("child::ns:PackageList", nsMgr);
                if (currentNode != null)
                {
                    this._packageList = new List<string>();
                    foreach (XmlNode nameNode in currentNode.SelectNodes("child::ns:PackageName", nsMgr))
                    {
                        this._packageList.Add(nameNode.InnerText);
                    }
                }

                currentNode = node.SelectSingleNode("child::ns:StereotypeList", nsMgr);
                if (currentNode != null)
                {
                    this._stereotypeList = new List<string>();
                    foreach (XmlNode nameNode in currentNode.SelectNodes("child::ns:StereotypeName", nsMgr))
                    {
                        this._stereotypeList.Add(nameNode.InnerText);
                    }
                }

                currentNode = node.SelectSingleNode("child::ns:ClassList", nsMgr);
                if (currentNode != null)
                {
                    this._classList = new List<string>();
                    foreach (XmlNode nameNode in currentNode.SelectNodes("child::ns:ClassName", nsMgr))
                    {
                        this._classList.Add(nameNode.InnerText);
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Framework.EventConfig.EventHandlerDescriptor >> Error parsing config. descriptor: " + exc.ToString());
            }
        }
    }
}
