using System;
using System.Windows;
using System.Runtime.Remoting;
using System.Collections.Generic;
using Framework.Logging;
using Framework.EventConfig;
using Framework.Util;
using Framework.Controller;

namespace Framework.Event
{
    // This defines the various menu event trees that might exist. The number of entries depends on the implementation of the model repository.
    internal enum TreeScope { Undefined, Controller, PackageTree, Diagram }

    /// <summary>
    /// ObjectEventType defines what type of event has been raised on an object. The 'Closed' and 'Opened' events can ONLY be generated
    /// by Diagram objects.
    /// </summary>
    internal enum ObjectEventType { Undefined, Closed, Created, Deleted, Modified, Opened, Selected  }

    /// <summary>
    /// ObjectType defines the event source for an object event. DiagramObject is a special case of a Class that is displayed on a Diagram and 
    /// needs special attention in the context of that diagram. They are passed as Classes, but typically processed by the owning diagram.
    /// </summary>
    internal enum ObjectType { Undefined, Attribute, Class, Connector, Diagram, DiagramObject, Object, Package }

    internal class EventManager
    {
        /// <summary>
        /// Helper class that acts as a node in the menu event-tree. If the node is a 'group', it may contain other groups and events, but no handler.
        /// </summary>
        internal class MenuEventNode
        {
            private string _nodeName;
            private MenuEvent _handler;
            private bool _isGroup;

            internal string Name      { get { return this._nodeName; } }
            internal MenuEvent Handler    { get { return this._handler; } }
            internal bool IsGroup     { get { return this._isGroup; } }

            /// <summary>
            /// Constructor that creates a 'group' node.
            /// </summary>
            /// <param name="nodeName">Name of the group.</param>
            internal MenuEventNode(string nodeName)
            {
                this._nodeName = nodeName;
                this._handler = null;
                this._isGroup = true;
            }

            /// <summary>
            /// Constructor that creates an event node.
            /// </summary>
            /// <param name="nodeName">Name of the event.</param>
            /// <param name="handler">The associated Event object.</param>
            internal MenuEventNode(string nodeName, MenuEvent handler)
            {
                this._nodeName = nodeName;
                this._handler = handler;
                this._isGroup = false;
            }
        }

        /// <summary>
        /// The treeList maintains a set of trees, indexed by tree identifier. How many trees exist and the format of the identifier is tool-
        /// specific and controlled by the event handler configuration. It is opaque for the event manager.
        /// </summary>
        private SortedList<TreeScope, TreeNode<MenuEventNode>> _treeList;

        /// <summary>
        /// This is the list of all currently defined object event handlers, sorted by event type.
        /// </summary>
        private SortedList<ObjectEventType, List<ObjectEvent>> _objectEventList;

        /// <summary>
        /// The Event Manager is 'owned' by the Controller Singleton and must never be instantiated directly!
        /// The constructor populates the menu eventnode tree and the object event list from configuration.
        /// </summary>
        internal EventManager()
        {
            this._treeList = new SortedList<TreeScope, TreeNode<MenuEventNode>>();
            this._objectEventList = new SortedList<ObjectEventType, List<ObjectEvent>>();

            // Parse our configuration for a list of menu event handlers...
            var menuEventConfig = new MenuEventHandlerConfiguration();
            foreach (MenuEventTreeDescriptor descriptor in menuEventConfig.GetEventTrees())
            {
                // We name the top of our menu event tree 'root'...
                var root = new TreeNode<MenuEventNode>(new MenuEventNode("root"));
                this._treeList.Add(descriptor.Scope, root);

                // Tree might contain a list of event groups (which in turn might contain a list of event groups, etc.
                if (descriptor.GroupList != null) AddGroupList(root, descriptor.GroupList, descriptor.Scope);

                // Tree might contain a list of event handlers. These are leafs by definition (no recursion required).
                if (descriptor.HandlerList != null)
                {
                    foreach (MenuEventHandlerDescriptor eventDesc in descriptor.HandlerList)
                    {
                        MenuEvent newEvent = CreateMenuEvent(eventDesc);
                        if (newEvent != null)
                        {
                            newEvent.Scope = descriptor.Scope;
                            root.AddChild(new MenuEventNode(eventDesc.Name, newEvent));
                        }
                        else Logger.WriteWarning("Unable to add menu event '" + eventDesc.Name +
                                                 "' with scope '" + descriptor.Scope + "' due to exception!");
                    }
                }
            }

            // Parse our configuration for a list of object event handlers...
            var objectEventConfig = new ObjectEventHandlerConfiguration();
            foreach (ObjectEventListDescriptor descriptor in objectEventConfig.GetEventLists())
            {
                var handlerList = new List<ObjectEvent>();
                foreach (ObjectEventHandlerDescriptor handlerDesc in descriptor.EventList)
                {
                    ObjectEvent newEvent = CreateObjectEvent(descriptor.EventType, handlerDesc);
                    if (newEvent != null) handlerList.Add(newEvent);
                    else Logger.WriteWarning("Unable to add object event '" + handlerDesc.Name + 
                                             "' to event list for event type '" + descriptor.EventType + "' due to exception!");
                }
                // We'll only include events that have at least one handler.
                if (handlerList.Count > 0) this._objectEventList.Add(descriptor.EventType, handlerList);
            }
        }

        /// <summary>
        /// Returns the list of menu event nodes that are immediately below the specified root event.
        /// </summary>
        /// <param name="scope">Identifies the tree to be searched.</param>
        /// <param name="rootName">Name of the root node to be located.</param>
        /// <returns>List of child nodes or NULL in case of errors (or nothing found)</returns>
        internal List<MenuEventNode> GetMenuEventList(TreeScope scope, string rootName)
        {
            var visitor = new RetrieveNodeVisitor(rootName);
            if (this._treeList.ContainsKey(scope))
            {
                this._treeList[scope].Traverse(this._treeList[scope], visitor.VisitorAction);
                return visitor.Children;
            }
            Logger.WriteError("Framework.Event.EventManager.getMenuEventList >> Unknown treeID: " + scope);
            return null;
        }

        /// <summary>
        /// This method is called when a menu option has been selected by the user. It locates the specified event based on scope, parent- and
        /// event name and when a handler is specified, executes this handler. The method silently fails on errors.
        /// </summary>
        /// <param name="scope">Which tree to search for the menu event.</param>
        /// <param name="parentName">Name of the parent of the menu event.</param>
        /// <param name="eventName">Name of the menu event, must be unique in scope of parent.</param>
        internal void HandleMenuEvent(TreeScope scope, string parentName, string eventName)
        {
            Logger.WriteInfo("Framework.Event.EventManager.HandleMenuEvent >> scope = " + scope + ", parentName = " + parentName + ", eventName = " + eventName);
            try
            {
                var visitor = new RetrieveNodeVisitor(parentName);
                this._treeList[scope].Traverse(this._treeList[scope], visitor.VisitorAction);
                
                // Inspect children until we found the one we're looking for
                // We initially search for the parent to assure that we find the correct child (child names are not necessarily unique across all groups).
                foreach (MenuEventNode node in visitor.Children)
                {
                    Logger.WriteInfo("Framework.Event.EventManager.HandleMenuEvent >> Inspecting event node: " + node.Name + "...");
                    if (node.Name == eventName)
                    {
                        if (node.IsGroup || node.Handler == null)
                        {
                            Logger.WriteError("Framework.Event.EventManager.HandleMenuEvent >> Wrong event type or no handler detected for '" +
                                              scope + ":" + parentName + "/" + eventName + "'...");
                        }
                        else
                        {
                            // Before invoking the event, we flush the context to be sure that all existing Model Elements are removed.
                            // This assures that the event creates a 'fresh' context, avoiding the use of 'stale' model elements from
                            // previous events (which might not match the contents of the repository any more).
                            ControllerSlt.GetControllerSlt().Flush();
                            Logger.WriteInfo("Framework.Event.EventManager.HandleMenuEvent >> Going to process event...");
                            node.Handler.HandleEvent();
                        }
                        break;
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Framework.Event.EventManager.HandleMenuEvent >> Caught an exception: " + exc.ToString());
            }
        }

        /// <summary>
        /// The method is called when the repository reports an event on a model element or a diagram. When a handler has been registered for this 
        /// event, it is called with the affected event object as a parameter.
        /// The event target is EITHER a model element OR a diagram, depending on the object type. Please note that the eventObject is managed
        /// by the repository-specific controller object and should NOT be disposed here (or by any event implementation for that matter)!
        /// </summary>
        /// <param name="eventType">The type of event that has been raised.</param>
        /// <param name="objectType">The type of object represented by eventObject. Only objects according to this type are acceptable, which are either
        /// ModelElement objects or Diagram objects.</param>
        /// <param name="eventObject">An object of type indicated by objectType.</param>
        /// <param name="targetDiagram">In case of a 'create diagram object', this parameter contains the diagram that received the object.</param>
        /// <returns>Depending on the eventType, the return value has different meaning: for Modified/Selected events, the return value should be 'true', 
        /// in case of a 'Created' event, the return value indicates whether or not the object has been modified by the event handler(s) and in case of 
        /// a 'Deleted' event, the return value indicates whether or not the object is allowed to be deleted.</returns>
        internal bool HandleObjectEvent (ObjectEventType eventType, ObjectType objectType, object eventObject, View.Diagram targetDiagram = null)
        {
            bool finalResult = eventType == ObjectEventType.Created? false: true;
            if (eventType != ObjectEventType.Undefined && this._objectEventList.ContainsKey(eventType))
            {
                foreach (ObjectEvent handler in this._objectEventList[eventType])
                {
                    if (handler.IsValidState(eventType, objectType, eventObject))
                    {
                        bool handlerResult = handler.HandleEvent(eventType, objectType, eventObject, targetDiagram);

                        // In case of creation events, we must indicate whether or not the object has been modified by one or more handlers
                        // in the list so we must 'OR' the results together. In all other cases, an 'AND' of results must be performed.
                        finalResult = eventType == ObjectEventType.Created? finalResult |= handlerResult : finalResult &= handlerResult;
                    }
                }
            }
            return finalResult;
        }

        /// <summary>
        /// This method returns an indicator of the state of the specified menu event. A state of 'false' indicates that the event is not valid in
        /// the current context. In case of event groups, the group only has valid state if at least one child has valid state.
        /// Note that this function is used only for menu events!
        /// </summary>
        /// <param name="scope">Defines the tree to be searched.</param>
        /// <param name="parentName">Name of the parent of the event.</param>
        /// <param name="eventName">Name of the event, must be unique in scope of parent.</param>
        /// <returns>True in case of valid state, false otherwise.</returns>
        internal bool IsValidState(TreeScope scope, string parentName, string eventName)
        {
            bool result = false;
            try
            {
                var visitor = new RetrieveNodeVisitor(parentName);
                this._treeList[scope].Traverse(this._treeList[scope], visitor.VisitorAction);

                // Inspect children until we found the one we're looking for
                // We initially search for the parent to assure that we find the correct child (child names are not necessarily unique across all groups).
                foreach (MenuEventNode node in visitor.Children)
                {
                    if (node.Name == eventName)
                    {
                        if (node.IsGroup)
                        {
                            // If this is an event group, it will be valid if at least one of its subordinates is valid...
                            // From the current parent, we're searching downwards to retrieve all children that are part of the group.
                            var visitorChild = new RetrieveNodeVisitor(eventName);
                            this._treeList[scope].Traverse(visitor.ParentNode, visitorChild.VisitorAction);

                            // Recursively check the state of the children. We abort on the first child that returns 'true'...
                            if (visitorChild.Children != null)
                            {
                                foreach (MenuEventNode childNode in visitorChild.Children)
                                {
                                    if (result = IsValidState(scope, eventName, childNode.Name)) break;
                                }
                            }
                        }
                        else
                        {
                            // If this is an 'ordinary' event, the state must be reported by the handler. No handler means 'false' by definition.
							result = node.Handler != null && node.Handler.IsValidState();
                        }
                        break; // Make sure to terminate outer loop, we have found the event we were looking for!
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Framework.Event.EventManager.isValidState >> Caught an exception: " + exc.ToString());
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Recursively build lists of groups and event handlers (when found in those groups)... 
        /// Building starts from the specified root node.
        /// </summary>
        /// <param name="root">Treenode that is currently being processed.</param>
        /// <param name="groupList">List of event groups to be added.</param>
        /// <param name="scope">Identifies the event tree that is currently being processed.</param>
        private void AddGroupList(TreeNode<MenuEventNode> root, List<MenuEventGroupDescriptor> groupList, TreeScope scope)
        {
            foreach (MenuEventGroupDescriptor group in groupList)
            {
                var node = new MenuEventNode(group.Name);
                TreeNode<MenuEventNode> child = root.AddChild(node);
                if (group.SubGroupList != null) AddGroupList(child, group.SubGroupList, scope);
                if (group.HandlerList != null)
                {
                    foreach (MenuEventHandlerDescriptor eventDesc in group.HandlerList)
                    {
                        MenuEvent newEvent = CreateMenuEvent(eventDesc);
                        if (newEvent != null)
                        {
                            newEvent.Scope = scope;
                            var handlerNode = new MenuEventNode(eventDesc.Name, newEvent);
                            child.AddChild(handlerNode);
                        }
                        else Logger.WriteWarning("Unable to add menu event '" + eventDesc.Name +
                                                 "' with scope '" + scope + "' to group '" + group.Name + "' due to exception!");
                    }
                }
            }
        }

        /// <summary>
        /// Factory method that creates a menu event (or event group) using an Event Configuration descriptor as input. Descriptor must contain the
        /// following properties:
        /// Handler name       = eventname (mandatory)
        ///         packages   = Optional list of packages for which the event is valid.
        ///         classes    = Optional list of class names for which the event is valid.
        ///         stereotypes = Optional list of stereotypes for which the event is valid.
        ///         handlerFQN = FQN of handler implementation object.
        /// The method extracts the above information from the descriptor and constructs the appropriate Event object. If a handler is
        /// specified, the method attempts to construct the associated Event Implementation object and binds this to the created event.
        /// </summary>
        /// <param name="descriptor">Event creation properties as retrieved from configuration.</param>
        /// <returns>Created and initialized object event instance or null on errors.</returns>
        private MenuEvent CreateMenuEvent (MenuEventHandlerDescriptor descriptor)
        {
            MenuEvent handlerEvent;
            try
            {
                handlerEvent = new MenuEvent(descriptor.Name, descriptor.PackageList, descriptor.StereotypeList, descriptor.ClassList);
                if (descriptor.HandlerFQN != null)
                {
                    ObjectHandle handle = Activator.CreateInstance(null, descriptor.HandlerFQN);
                    handlerEvent.Bind((MenuEventImplementation)handle.Unwrap());
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Framework.Event.EventManager.CreateMenuEvent >> Caught an exception: " + exc.ToString());
                handlerEvent = null;
            }
            return handlerEvent;
        }

        /// <summary>
        /// Factory method that creates an object event using an Event Configuration descriptor as input. Descriptor must contain the
        /// following properties:
        /// Handler name       = eventname (mandatory)
        ///         eventTypes = Optional list of event types for which the event is valid.
        ///         objectTypes = Optional list of object types for which the event is valid.
        ///         stereotypes = Optional list of stereotypes for which the event is valid.
        ///         handlerFQN = FQN of handler implementation object.
        /// The method extracts the above information from the descriptor and constructs the appropriate Event object. If a handler is
        /// specified, the method attempts to construct the associated Event Implementation object and binds this to the created event.
        /// </summary>
        /// <param name="currentEventType">Identifies the event list we're currently building.</param>
        /// <param name="descriptor">Event creation properties as retrieved from configuration.</param>
        /// <returns>Created and initialized object event instance or null on errors.</returns>
        private ObjectEvent CreateObjectEvent(ObjectEventType currentEventType,  ObjectEventHandlerDescriptor descriptor)
        {
            ObjectEvent handlerEvent = null;

            try
            {
                // First of all, we're going to look for the event in one of the other lists...
                foreach (Enum value in Enum.GetValues(typeof(ObjectEventType)))
                {
                    if ((ObjectEventType)value != currentEventType) handlerEvent = FindObjectEvent((ObjectEventType)value, descriptor.Name);
                    if (handlerEvent != null) break;
                }

                if (handlerEvent == null)
                {
                    // Not found anywhere else, create the handler with the current event type...
                    handlerEvent = new ObjectEvent(descriptor.Name, currentEventType, descriptor.ObjectTypeList, descriptor.StereotypeList);
                    if (descriptor.HandlerFQN != null)
                    {
                        ObjectHandle handle = Activator.CreateInstance(null, descriptor.HandlerFQN);
                        handlerEvent.Bind((ObjectEventImplementation)handle.Unwrap());
                    }
                }
                else handlerEvent.AddEventType(currentEventType);   // Existing handler, just register our event type as a supported type for the handler.
            }
            catch (Exception exc)
            {
                Logger.WriteError("Framework.Event.EventManager.CreateObjectEvent >> Caught an exception: " + exc.ToString());
                handlerEvent = null;
            }
            return handlerEvent;
        }

        /// <summary>
        /// Simple helper function that searches the specified event list for a given handler by name.
        /// </summary>
        /// <param name="eventType">Identifies the event list to search.</param>
        /// <param name="name">The name to look for (must be an exact match).</param>
        /// <returns>Retrieved object event or null when not found.</returns>
        private ObjectEvent FindObjectEvent(ObjectEventType eventType, string name)
        {
            if (this._objectEventList.ContainsKey(eventType))
            {
                foreach (ObjectEvent evt in this._objectEventList[eventType]) if (evt.Name == name) return evt;
            }
            return null;
        }

        /// <summary>
        /// Helper class that allows us to traverse a tree and insert the specified node as child of provided parent node.
        /// </summary>
        private class GroupNodeVisitor
        {
            private string _parent;
            private MenuEventNode _node;

            /// <summary>
            /// Loads the necessary data for the check.
            /// </summary>
            /// <param name="node">The node to be inserted.</param>
            /// <param name="parent">The name of the parent we're looking for.</param>
            internal GroupNodeVisitor(MenuEventNode node, string parent)
            {
                this._parent = parent;
                this._node = node;
            }

            /// <summary>
            /// Action to be taken for each traversed node.
            /// </summary>
            /// <param name="treeNode">The current node in the tree.</param>
            /// <returns>True if we're done and traversal must stop, false if we need to continue.</returns>
            internal bool VisitorAction(TreeNode<MenuEventNode> treeNode)
            {
                bool done = false;
                if (treeNode.Data.Name == this._parent)
                {
                    treeNode.AddChild(this._node);
                    done = true;
                }
                return done;
            }
        }

        /// <summary>
        /// Helper class that allows us to find a node by name in a tree and return the list of child nodes.
        /// </summary>
        private class RetrieveNodeVisitor
        {
            private string _name;
            private List<MenuEventNode> _children;
            private TreeNode<MenuEventNode> _parentNode;

            /// <summary>
            /// Returns the node name.
            /// </summary>
            internal string Name {get {return this._name; }}

            /// <summary>
            /// Return the list of child nodes of the current node.
            /// </summary>
            internal List<MenuEventNode> Children {get {return this._children; }}

            /// <summary>
            /// Return the parent node of the current node.
            /// </summary>
            internal TreeNode<MenuEventNode> ParentNode {get {return this._parentNode; }}

            /// <summary>
            /// Loads the necessary data for the check.
            /// </summary>
            /// <param name="name">The name of the node we're looking for.</param>
            internal RetrieveNodeVisitor(string name)
            {
                this._name = name;
                this._children = new List<MenuEventNode>();
                this._parentNode = null;
            }

            /// <summary>
            /// Checks whether we have arrived at the node we're looking for and if so, store the data and abort the search.
            /// ParentNode contains the node at which processing has stopped and is the parent of the collected children.
            /// </summary>
            /// <param name="treeNode">The current node in the tree.</param>
            /// <returns>True if we're done and traversal must stop, false if we need to continue.</returns>
            internal bool VisitorAction(TreeNode<MenuEventNode> treeNode)
            {
                bool done = false;
                if (treeNode.Data.Name == this._name)
                {
                    foreach (TreeNode<MenuEventNode> node in treeNode.Children) this._children.Add(node.Data);
                    this._parentNode = treeNode;
                    done = true;
                }
                return done;
            }
        }
    }
}
