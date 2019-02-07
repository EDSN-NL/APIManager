using System;
using System.Runtime.Remoting;
using System.Collections.Generic;
using Framework.Logging;
using Framework.EventConfig;
using Framework.Util;
using Framework.Controller;

namespace Framework.Event
{
    // This defines the various event trees that might exist. The number of entries depends on the implementation of the model repository.
    internal enum TreeScope { Undefined, Controller, PackageTree, Diagram }

    internal class EventManager
    {
        /// <summary>
        /// Helper class that acts as a node in the event-tree. If the node is a 'group', it may contain other groups and events, but no handler.
        /// </summary>
        internal class EventNode
        {
            private string _nodeName;
            private Event _handler;
            private bool _isGroup;

            internal string Name      { get { return this._nodeName; } }
            internal Event Handler    { get { return this._handler; } }
            internal bool IsGroup     { get { return this._isGroup; } }

            /// <summary>
            /// Constructor that creates a 'group' node.
            /// </summary>
            /// <param name="nodeName">Name of the group.</param>
            internal EventNode(string nodeName)
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
            internal EventNode(string nodeName, Event handler)
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
        private SortedList<TreeScope, TreeNode<EventNode>> _treeList;

        /// <summary>
        /// The Event Manager is 'owned' by the Controller Singleton and must never be instantiated directly!
        /// The constructor populates the eventnode tree from the configuration. The top-node has name "root".
        /// </summary>
        internal EventManager()
        {
            this._treeList = new SortedList<TreeScope, TreeNode<EventNode>>();

            var eventConfig = new EventHandlerConfiguration();
            foreach (EventTreeDescriptor descriptor in eventConfig.GetEventTrees())
            {
                var root = new TreeNode<EventNode>(new Framework.Event.EventManager.EventNode("root"));
                this._treeList.Add(descriptor.Scope, root);

                // Tree might contain a list of event groups (which in turn might contain a list of event groups, etc.
                if (descriptor.GroupList != null) AddGroupList(root, descriptor.GroupList, descriptor.Scope);

                // Tree might contain a list of event handlers. These are leafs by definition (no recursion required).
                if (descriptor.HandlerList != null)
                {
                    foreach (EventHandlerDescriptor eventDesc in descriptor.HandlerList)
                    {
                        Event newEvent = CreateEvent(eventDesc);
                        newEvent.Scope = descriptor.Scope;
                        root.AddChild(new EventNode(eventDesc.Name, newEvent));
                    }
                }
            }
        }

        /// <summary>
        /// Returns the list of event nodes that are immediately below the specified root event.
        /// </summary>
        /// <param name="scope">Identifies the tree to be searched.</param>
        /// <param name="rootName">Name of the root node to be located.</param>
        /// <returns>List of child nodes or NULL in case of errors (or nothing found)</returns>
        internal List<EventNode> GetEventList(TreeScope scope, string rootName)
        {
            var visitor = new RetrieveNodeVisitor(rootName);
            if (this._treeList.ContainsKey(scope))
            {
                this._treeList[scope].Traverse(this._treeList[scope], visitor.VisitorAction);
                return visitor.Children;
            }
            Logger.WriteError("Framework.Event.EventManager.getEventList >> Unknown treeID: " + scope);
            return null;
        }

        /// <summary>
        /// This method is called when an event has been selected by the user. It locates the specified event based on scope, parent- and
        /// event name and when a handler is specified, executes this handler. The method silently fails on errors.
        /// </summary>
        /// <param name="scope">Which tree to search for the event.</param>
        /// <param name="parentName">Name of the parent of the event.</param>
        /// <param name="eventName">Name of the event, must be unique in scope of parent.</param>
        internal void HandleEvent(TreeScope scope, string parentName, string eventName)
        {
            Logger.WriteInfo("Framework.Event.EventManager.handleEvent >> scope = " + scope + ", parentName = " + parentName + ", eventName = " + eventName);
            try
            {
                var visitor = new RetrieveNodeVisitor(parentName);
                this._treeList[scope].Traverse(this._treeList[scope], visitor.VisitorAction);
                
                // Inspect children until we found the one we're looking for
                // We initially search for the parent to assure that we find the correct child (child names are not necessarily unique across all groups).
                foreach (EventNode node in visitor.Children)
                {
                    Logger.WriteInfo("Framework.Event.EventManager.handleEvent >> Inspecting event node: " + node.Name + "...");
                    if (node.Name == eventName)
                    {
                        if (node.IsGroup || node.Handler == null)
                        {
                            Logger.WriteError("Framework.Event.EventManager.handleEvent >> Wrong event type or no handler detected for '" +
                                              scope + ":" + parentName + "/" + eventName + "'...");
                        }
                        else
                        {
                            // Before invoking the event, we flush the context to be sure that all existing Model Elements are removed.
                            // This assures that the event creates a 'fresh' context, avoiding the use of 'stale' model elements from
                            // previous events (which might not match the contents of the repository any more).
                            ControllerSlt.GetControllerSlt().Flush();
                            Logger.WriteInfo("Framework.Event.EventManager.handleEvent >> Going to process event...");
                            node.Handler.HandleEvent();
                        }
                        break;
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Framework.Event.EventManager.handleEvent >> Caught an exception: " + exc.ToString());
            }
        }

        /// <summary>
        /// This method returns an indicator of the state of the specified event. A state of 'false' indicates that the event is not valid in
        /// the current context. In case of event groups, the group only has valid state if at least one child has valid state.
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
                foreach (EventNode node in visitor.Children)
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
                                foreach (EventNode childNode in visitorChild.Children)
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
        /// Factory method that creates an event (or event group) using an Event Configuration descriptor as input. Descriptor must contain the
        /// following properties:
        /// Handler name       = eventname (mandatory)
        ///         packages   = Optional list of packages for which the event is valid.
        ///         classes    = Optional list of class names for which the event is valid.
        ///         stereotypes = Optional list of stereotypes for which the event is valid.
        ///         handlerFQN = FQN of handler implementation object.
        /// The method extracts the above information from the descriptor and constructs the appropriate Event objects. If a handler is
        /// specified, the method attempts to construct the associated Event Implementation object and binds this to the created event.
        /// </summary>
        /// <param name="descriptor">Event creation properties as retrieved from configuration.</param>
        private Event CreateEvent (EventHandlerDescriptor descriptor)
        {
            Event handlerEvent = null;
            try
            {
                handlerEvent = new Event(descriptor.Name, descriptor.PackageList, descriptor.StereotypeList, descriptor.ClassList);
                if (descriptor.HandlerFQN != null)
                {
                    ObjectHandle handle = Activator.CreateInstance(null, descriptor.HandlerFQN);
                    handlerEvent.Bind((EventImplementation)handle.Unwrap());
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Framework.Event.EventManager.createEvent >> Caught an exception: " + exc.ToString());
            }
            return handlerEvent;
        }

        /// <summary>
        /// Recursively build lists of groups and event handlers (when found in those groups)... 
        /// Building starts from the specified root node.
        /// </summary>
        /// <param name="root">Treenode that is currently being processed.</param>
        /// <param name="groupList">List of event groups to be added.</param>
        /// <param name="scope">Identifies the event tree that is currently being processed.</param>
        private void AddGroupList(TreeNode<EventNode> root, List<EventGroupDescriptor> groupList, TreeScope scope)
        {
            foreach (EventGroupDescriptor group in groupList)
            {
                var node = new EventNode(group.Name);
                TreeNode<EventNode> child = root.AddChild(node);
                if (group.SubGroupList != null) AddGroupList(child, group.SubGroupList, scope);
                if (group.HandlerList != null)
                {
                    foreach (EventHandlerDescriptor eventDesc in group.HandlerList)
                    {
                        Event newEvent = CreateEvent(eventDesc);
                        newEvent.Scope = scope;
                        var handlerNode = new EventNode(eventDesc.Name, newEvent);
                        child.AddChild(handlerNode);
                    }
                }
            }
        }

        /// <summary>
        /// Helper class that allows us to traverse a tree and insert the specified node as child of provided parent node.
        /// </summary>
        private class GroupNodeVisitor
        {
            private string _parent;
            private EventNode _node;

            /// <summary>
            /// Loads the necessary data for the check.
            /// </summary>
            /// <param name="node">The node to be inserted.</param>
            /// <param name="parent">The name of the parent we're looking for.</param>
            internal GroupNodeVisitor(EventNode node, string parent)
            {
                this._parent = parent;
                this._node = node;
            }

            /// <summary>
            /// Action to be taken for each traversed node.
            /// </summary>
            /// <param name="treeNode">The current node in the tree.</param>
            /// <returns>True if we're done and traversal must stop, false if we need to continue.</returns>
            internal bool VisitorAction(TreeNode<EventNode> treeNode)
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
            private List<EventNode> _children;
            private TreeNode<EventNode> _parentNode;

            internal string Name                    {get {return this._name; }}
            internal List<EventNode> Children       {get {return this._children; }}
            internal TreeNode<EventNode> ParentNode {get {return this._parentNode; }}

            /// <summary>
            /// Loads the necessary data for the check.
            /// </summary>
            /// <param name="name">The name of the node we're looking for.</param>
            internal RetrieveNodeVisitor(string name)
            {
                this._name = name;
                this._children = new List<EventNode>();
                this._parentNode = null;
            }

            /// <summary>
            /// Checks whether we have arrived at the node we're looking for and if so, store the data and abort the search.
            /// ParentNode contains the node at which processing has stopped and is the parent of the collected children.
            /// </summary>
            /// <param name="treeNode">The current node in the tree.</param>
            /// <returns>True if we're done and traversal must stop, false if we need to continue.</returns>
            internal bool VisitorAction(TreeNode<EventNode> treeNode)
            {
                bool done = false;
                if (treeNode.Data.Name == this._name)
                {
                    foreach (TreeNode<EventNode> node in treeNode.Children) this._children.Add(node.Data);
                    this._parentNode = treeNode;
                    done = true;
                }
                return done;
            }
        }
    }
}
