using System.Collections.Generic;
using System;
using System.Xml;
using Framework.Logging;
using Framework.Util;
using Framework.Event;

namespace Framework.EventConfig
{
    /// <summary>
    /// Helper class that represents a single object event handler definition from configuration....
    /// </summary>
    internal sealed class ObjectEventHandlerDescriptor
    {
        /// <summary>
        /// Event handler name (IDentifier)
        /// </summary>
        internal string Name { get; }

        /// <summary>
        /// The FQN represents the handler entry point (the actual code).
        /// </summary>
        internal string HandlerFQN { get; }

        /// <summary>
        /// List of Stereotypes that the event source must have in order to be selected as a target for this handler
        /// </summary>
        internal List<string> StereotypeList { get; }

        /// <summary>
        /// Source object type must match an entry from this list in order for the handler to be selected. If the list is absent, ALL object
        /// types will trigger this handler.
        /// </summary>
        internal List<ObjectType> ObjectTypeList { get; }

        /// <summary>
        /// Constructor creates a new descriptor from an XML node.
        /// </summary>
        /// <param name="node">Node containing the definition.</param>
        internal ObjectEventHandlerDescriptor(XmlNode node, XmlNamespaceManager nsMgr)
        {
            Name = string.Empty;
            HandlerFQN = string.Empty;
            StereotypeList = null;
            ObjectTypeList = null;

            try
            {
                XmlAttributeCollection attribs = node.Attributes;
                Name = ((XmlAttribute)attribs.GetNamedItem("ID")).InnerText;

                // Retrieve simple names...
                XmlNode currentNode = node.SelectSingleNode("child::ns:HandlerFQN", nsMgr);
                if (currentNode != null) HandlerFQN = currentNode.InnerText;

                currentNode = node.SelectSingleNode("child::ns:ObjectTypeList", nsMgr);
                if (currentNode != null)
                {
                    ObjectTypeList = new List<ObjectType>();
                    foreach (XmlNode nameNode in currentNode.SelectNodes("child::ns:ObjectType", nsMgr))
                    {
                        ObjectTypeList.Add(EnumConversions<ObjectType>.StringToEnum(nameNode.InnerText));
                    }
                }

                currentNode = node.SelectSingleNode("child::ns:StereotypeList", nsMgr);
                if (currentNode != null)
                {
                    StereotypeList = new List<string>();
                    foreach (XmlNode nameNode in currentNode.SelectNodes("child::ns:StereotypeName", nsMgr))
                    {
                        StereotypeList.Add(nameNode.InnerText);
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Framework.EventConfig.ObjectEventHandlerDescriptor >> Error parsing config. descriptor: " + exc.ToString());
            }
        }
    }
}
