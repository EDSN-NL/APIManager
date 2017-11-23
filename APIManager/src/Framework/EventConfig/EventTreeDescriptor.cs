using System.Collections.Generic;
using System;
using System.Xml;
using Framework.Logging;
using Framework.Event;

namespace Framework.EventConfig
{
    /// <summary>
    /// Helper class that represents an even tree. May contain other groups or event handlers.
    /// </summary>
    internal sealed class EventTreeDescriptor
    {
        private TreeScope _scope = TreeScope.Undefined;
        private List<EventGroupDescriptor> _groupList = null;
        private List<EventHandlerDescriptor> _handlerList = null;

        // Retrieve attributes...
        internal TreeScope Scope                          { get { return this._scope; } }
        internal List<EventGroupDescriptor> GroupList     { get { return this._groupList; } }
        internal List<EventHandlerDescriptor> HandlerList { get { return this._handlerList; } }

        /// <summary>
        /// Constructor creates a new tree descriptor from an XML node.
        /// </summary>
        /// <param name="node">Node containing the definition.</param>
        /// <param name="nsMgr">Namespace descriptor</param>
        internal EventTreeDescriptor(XmlNode node, XmlNamespaceManager nsMgr)
        {
            try
            {
                XmlAttributeCollection attribs = node.Attributes;
                switch (((XmlAttribute)attribs.GetNamedItem("scope")).InnerText)
                {
                    case "Controller":
                        this._scope = TreeScope.Controller;
                        break;

                    case "Diagram":
                        this._scope = TreeScope.Diagram;
                        break;

                    case "PackageTree":
                        this._scope = TreeScope.PackageTree;
                        break;

                    default:
                        this._scope = TreeScope.Undefined;
                        break;
                }

                XmlNode listNode = node.SelectSingleNode("child::ns:EventGroupList", nsMgr);
                if (listNode != null)
                {
                    this._groupList = new List<EventGroupDescriptor>();
                    foreach (XmlNode groupNode in listNode.SelectNodes("child::ns:EventGroup", nsMgr))
                    {
                        this._groupList.Add(new EventGroupDescriptor(groupNode, nsMgr));
                    }
                }

                listNode = node.SelectSingleNode("child::ns:EventHandlerList", nsMgr);
                if (listNode != null)
                {
                    this._handlerList = new List<EventHandlerDescriptor>();
                    foreach (XmlNode handlerNode in listNode.SelectNodes("child::ns:EventHandler", nsMgr))
                    {
                        this._handlerList.Add(new EventHandlerDescriptor(handlerNode, nsMgr));
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Framework.EventConfig.EventTreeDescriptor >> Error parsing config. descriptor: " + exc.Message);
            }
        }
    }
}
