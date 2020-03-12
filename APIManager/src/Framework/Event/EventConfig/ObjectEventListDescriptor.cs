using System.Collections.Generic;
using System;
using System.Xml;
using Framework.Logging;
using Framework.Event;
using Framework.Util;

namespace Framework.EventConfig
{
    /// <summary>
    /// Helper class that represents a list of event handlers associated with a specific event type. 
    /// </summary>
    internal sealed class ObjectEventListDescriptor
    {
        /// <summary>
        /// The event type that is associated with this list.
        /// </summary>
        internal ObjectEventType EventType { get; }

        /// <summary>
        /// The actual list of event handlers for the associated event type.
        /// </summary>
        internal List<ObjectEventHandlerDescriptor> EventList { get; }

        /// <summary>
        /// Constructor creates a new list of handlers from an XML node.
        /// </summary>
        /// <param name="node">Node containing the definition.</param>
        /// <param name="nsMgr">Namespace descriptor</param>
        internal ObjectEventListDescriptor(XmlNode node, XmlNamespaceManager nsMgr)
        {
            try
            {
                XmlAttributeCollection attribs = node.Attributes;
                EventType = EnumConversions<ObjectEventType>.StringToEnum(((XmlAttribute)attribs.GetNamedItem("eventType")).InnerText);
                EventList = new List<ObjectEventHandlerDescriptor>();

                XmlNode listNode = node.SelectSingleNode("child::ns:EventHandlerList", nsMgr);
                if (listNode != null)
                {
                    foreach (XmlNode groupNode in listNode.SelectNodes("child::ns:EventHandler", nsMgr))
                    {
                        EventList.Add(new ObjectEventHandlerDescriptor(groupNode, nsMgr));
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Framework.EventConfig.ObjectEventListDescriptor >> Error parsing config. descriptor: " + exc.ToString());
            }
        }
    }
}
