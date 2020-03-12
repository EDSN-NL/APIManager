using System.Collections.Generic;
using System;
using System.Xml;
using Framework.Logging;
using Framework.Event;

namespace Framework.EventConfig
{
    /// <summary>
    /// Helper class that represents a menu event tree. May contain other groups or event handlers.
    /// </summary>
    internal sealed class MenuEventTreeDescriptor
    {
        internal TreeScope Scope { get; }
        internal List<MenuEventGroupDescriptor> GroupList { get; }
        internal List<MenuEventHandlerDescriptor> HandlerList { get; }

        /// <summary>
        /// Constructor creates a new tree descriptor from an XML node.
        /// </summary>
        /// <param name="node">Node containing the definition.</param>
        /// <param name="nsMgr">Namespace descriptor</param>
        internal MenuEventTreeDescriptor(XmlNode node, XmlNamespaceManager nsMgr)
        {
            try
            {
                Scope = TreeScope.Undefined;
                GroupList = null;
                HandlerList = null;

                XmlAttributeCollection attribs = node.Attributes;
                switch (((XmlAttribute)attribs.GetNamedItem("scope")).InnerText)
                {
                    case "Controller":
                        Scope = TreeScope.Controller;
                        break;

                    case "Diagram":
                        Scope = TreeScope.Diagram;
                        break;

                    case "PackageTree":
                        Scope = TreeScope.PackageTree;
                        break;

                    default:
                        Scope = TreeScope.Undefined;
                        break;
                }

                XmlNode listNode = node.SelectSingleNode("child::ns:EventGroupList", nsMgr);
                if (listNode != null)
                {
                    GroupList = new List<MenuEventGroupDescriptor>();
                    foreach (XmlNode groupNode in listNode.SelectNodes("child::ns:EventGroup", nsMgr))
                    {
                        GroupList.Add(new MenuEventGroupDescriptor(groupNode, nsMgr));
                    }
                }

                listNode = node.SelectSingleNode("child::ns:EventHandlerList", nsMgr);
                if (listNode != null)
                {
                    HandlerList = new List<MenuEventHandlerDescriptor>();
                    foreach (XmlNode handlerNode in listNode.SelectNodes("child::ns:EventHandler", nsMgr))
                    {
                        HandlerList.Add(new MenuEventHandlerDescriptor(handlerNode, nsMgr));
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Framework.EventConfig.MenuEventTreeDescriptor >> Error parsing config. descriptor: " + exc.ToString());
            }
        }
    }
}
