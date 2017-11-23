using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Util
{
    /// <summary>
    /// Helper class for the creation of tree structures. Each node in the tree has an optional parent and zero to many children.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class TreeNode<T>
    {
        private T _data;
        private LinkedList<TreeNode<T>> _children;
        private TreeNode<T> _parent;

        /// <summary>
        /// Getters that return the list of children nodes and the parent node. Both could be NULL.
        /// Also option to return the payload.
        /// </summary>
        internal LinkedList<TreeNode<T>> Children
        {
            get { return this._children; }
        }

        /// <summary>
        /// Returns the list of actual child objects (instead of the nodes).
        /// </summary>
        internal List<T> ChildObjects
        {
            get
            {
                var childList = new List<T>();
                foreach (var child in this._children) childList.Add(child.Data);
                return childList;
            }
        }

        internal TreeNode<T> Parent
        {
            get { return this._parent; }
            set { this._parent = value; }
        }

        internal T Data
        {
            get { return this._data; }
        }

        /// <summary>
        /// Creates an empty tree node instance without a parent node ('top-level' node).
        /// </summary>
        internal TreeNode()
        {
            this._data = default(T);
            this._children = new LinkedList<TreeNode<T>>();
            this._parent = null;
        }

        /// <summary>
        /// Creates a new tree node instance without a parent node ('top-level' node).
        /// </summary>
        /// <param name="data">Data element for the node (payload).</param>
        internal TreeNode(T data)
        {
            this._data = data;
            this._children = new LinkedList<TreeNode<T>>();
            this._parent = null; 
        }

        /// <summary>
        /// Creates a new tree node instance associated with a parent node.
        /// </summary>
        /// <param name="data">Data element for the node (payload).</param>
        /// <param name="parent">The parent node.</param>
        internal TreeNode(T data, TreeNode<T> parent)
        {
            this._data = data;
            this._children = new LinkedList<TreeNode<T>>();
            this._parent = parent;
        }

        /// <summary>
        /// Method that adds a new data object as a child of the given node. The new node will be added to the end of the list.
        /// The method creates an implicit child TreeNode that is linked to the current TreeNode. This created node is returned.
        /// </summary>
        /// <param name="child">Child object that must be added to the current node.</param>
        /// <returns>The constructed child node.</returns>
        internal TreeNode<T> AddChild(T child)
        {
            var childNode = new TreeNode<T>(child, this);
            this._children.AddLast(childNode);
            return childNode;
        }

        /// <summary>
        /// Method that adds a new data object as a child of the given node. The new node will be added to the end of the list.
        /// This method receives a child TreeNode instead of the child itself and simply links to that TreeNode.
        /// </summary>
        /// <param name="child">Child object that must be added to the current node.</param>
        /// <returns>The constructed child node.</returns>
        internal TreeNode<T> AddChild(TreeNode<T> childNode)
        {
            childNode.Parent = this;
            this._children.AddLast(childNode);
            return childNode;
        }

        /// <summary>
        /// The method searches the list of nodes for the specified child. If found, the child node is removed from the list. If not found, the
        /// method fails silently. In order to properly detect the node to be deleted, the list type 'T' MUST implement the 'Equals' method. 
        /// </summary>
        /// <param name="child">Child object that must be deleted from the current node.</param>
        internal void DeleteChild(T child)
        {
            foreach (TreeNode<T> node in this._children)
            {
                if (node._data.Equals(child))
                {
                    this._children.Remove(node);
                    node.Parent = null;
                    return;
                }
            }
        }

        /// <summary>
        /// Checks whether the specified type is registered as a child of the current TreeNode.
        /// </summary>
        /// <param name="child">Child to be located.</param>
        /// <returns>True if child present, false otherwise.</returns>
        internal bool HasChild(T child)
        {
            foreach(TreeNode<T> node in this._children) if (node.Data.Equals(child)) return true;
            return false;
        }
        /// <summary>
        /// Recursively traverses the entire tree hierarchy, starting at given node. For each node, the provided function delegate
        /// is invoked, which receives the node data as parameter.
        /// As long as the delegate returns 'false', the traversal continues (until all nodes have been processed). It is therefor
        /// possible to abort traversal by letting the delegate return a 'true' value (as in 'done').
        /// </summary>
        /// <param name="startNode">Starting node.</param>
        /// <param name="visitor">Action that must be performed on each node.</param>
        internal void Traverse (TreeNode<T> startNode, Func<TreeNode<T>, bool> visitor)
        {
            if (visitor(startNode)) return;
            foreach (TreeNode<T> child in startNode._children) Traverse(child, visitor);
        }
    }
}
