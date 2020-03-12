using System.Collections.Generic;
using System;
using System.Xml;
using Framework.Logging;

namespace Framework.EventConfig
{
    /// <summary>
    /// Helper class that represents a menu event group. May contain other groups and/or event handlers.
    /// </summary>
    internal sealed class MenuEventGroupDescriptor
    {
        private string _name = string.Empty;
        private List<MenuEventGroupDescriptor> _subGroupList = null;
        private List<MenuEventHandlerDescriptor> _handlerList = null;

        // Retrieve attributes...
        internal string Name                              { get { return this._name; } }
        internal List<MenuEventGroupDescriptor> SubGroupList  { get { return this._subGroupList; } }
        internal List<MenuEventHandlerDescriptor> HandlerList { get { return this._handlerList; } }

        /// <summary>
        /// Constructor creates a new group descriptor from an XML node.
        /// </summary>
        /// <param name="node">Node containing the definition.</param>
        internal MenuEventGroupDescriptor(XmlNode node, XmlNamespaceManager nsMgr)
        {
            try
            {
                XmlAttributeCollection attribs = node.Attributes;
                this._name = ((XmlAttribute)attribs.GetNamedItem("ID")).InnerText;

                XmlNode nodeList = node.SelectSingleNode("child::ns:EventGroupList", nsMgr);
                if (nodeList != null)
                {
                    this._subGroupList = new List<MenuEventGroupDescriptor>();
                    foreach (XmlNode groupNode in nodeList.SelectNodes("child::ns:EventGroup", nsMgr))
                    {
                        this._subGroupList.Add(new MenuEventGroupDescriptor(groupNode, nsMgr));
                    }
                }

                nodeList = node.SelectSingleNode("child::ns:EventHandlerList", nsMgr);
                if (nodeList != null)
                {
                    this._handlerList = new List<MenuEventHandlerDescriptor>();
                    foreach (XmlNode handlerNode in nodeList.SelectNodes("child::ns:EventHandler", nsMgr))
                    {
                        this._handlerList.Add(new MenuEventHandlerDescriptor(handlerNode, nsMgr));
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Framework.EventConfig.MenuEventGroupDescriptor >> Error parsing config. descriptor: " + exc.ToString());
            }
        }
    }
}
