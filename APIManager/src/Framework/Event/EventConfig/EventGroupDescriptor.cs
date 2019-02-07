using System.Collections.Generic;
using System;
using System.Xml;
using Framework.Logging;

namespace Framework.EventConfig
{
    /// <summary>
    /// Helper class that represents an event group. May contain other groups and/or event handlers.
    /// </summary>
    internal sealed class EventGroupDescriptor
    {
        private string _name = string.Empty;
        private List<EventGroupDescriptor> _subGroupList = null;
        private List<EventHandlerDescriptor> _handlerList = null;

        // Retrieve attributes...
        internal string Name                              { get { return this._name; } }
        internal List<EventGroupDescriptor> SubGroupList  { get { return this._subGroupList; } }
        internal List<EventHandlerDescriptor> HandlerList { get { return this._handlerList; } }

        /// <summary>
        /// Constructor creates a new group descriptor from an XML node.
        /// </summary>
        /// <param name="node">Node containing the definition.</param>
        internal EventGroupDescriptor(XmlNode node, XmlNamespaceManager nsMgr)
        {
            try
            {
                XmlAttributeCollection attribs = node.Attributes;
                this._name = ((XmlAttribute)attribs.GetNamedItem("ID")).InnerText;

                XmlNode nodeList = node.SelectSingleNode("child::ns:EventGroupList", nsMgr);
                if (nodeList != null)
                {
                    this._subGroupList = new List<EventGroupDescriptor>();
                    foreach (XmlNode groupNode in nodeList.SelectNodes("child::ns:EventGroup", nsMgr))
                    {
                        this._subGroupList.Add(new EventGroupDescriptor(groupNode, nsMgr));
                    }
                }

                nodeList = node.SelectSingleNode("child::ns:EventHandlerList", nsMgr);
                if (nodeList != null)
                {
                    this._handlerList = new List<EventHandlerDescriptor>();
                    foreach (XmlNode handlerNode in nodeList.SelectNodes("child::ns:EventHandler", nsMgr))
                    {
                        this._handlerList.Add(new EventHandlerDescriptor(handlerNode, nsMgr));
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Framework.EventConfig.EventGroupDescriptor >> Error parsing config. descriptor: " + exc.ToString());
            }
        }
    }
}
