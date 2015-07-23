using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Common
{
    /// <summary>
    /// Search tree that implements Binary Space Partitioning (BSP).
    /// </summary>
    public class BspSearchTree<T> : ISearchTree<T> where T : ISearchTreeContent
    {
        /// <summary>
        /// Constructs a BspSearchTree instance.
        /// </summary>
        /// <param name="area">The rectangular area covered by this search tree.</param>
        /// <param name="nodeCapacity">The maximum number of contents a BSP-tree node can hold without subdivision.</param>
        /// <param name="minNodeSize">The minimum size of a BSP-node.</param>
        public BspSearchTree(RCNumRectangle area, int nodeCapacity, int minNodeSize)
        {
            if (area == RCNumRectangle.Undefined) { throw new ArgumentNullException("area"); }
            if (nodeCapacity <= 0) { throw new ArgumentOutOfRangeException("nodeCapacity", "Node capacity must be greater than 0!"); }
            if (minNodeSize <= 0) { throw new ArgumentOutOfRangeException("minNodeSize", "Minimum BSP-node size must be positive!"); }

            this.attachedContents = new RCSet<T>();
            this.rootNode = new BspSearchTreeNode<T>(area, nodeCapacity, minNodeSize);
        }

        /// <summary>
        /// Gets the boundaries of the BSP-nodes.
        /// </summary>
        /// <remarks>TODO: this method is only for testing.</remarks>
        public List<RCNumRectangle> GetTreeNodeBoundaries()
        {
            List<RCNumRectangle> retList = new List<RCNumRectangle>();
            this.rootNode.CollectBoundaries(ref retList);
            return retList;
        }

        #region ISearchTree<T> methods

        /// <see cref="ISearchTree<T>.GetContents"/>
        public RCSet<T> GetContents()
        {
            return new RCSet<T>(this.attachedContents);
        }

        /// <see cref="ISearchTree<T>.GetContents"/>
        public RCSet<T> GetContents(RCNumVector position)
        {
            if (position == RCNumVector.Undefined) { throw new ArgumentNullException("position"); }

            RCSet<T> retList = new RCSet<T>();
            this.rootNode.CollectContents(position, ref retList);
            return retList;
        }

        /// <see cref="ISearchTree<T>.GetContents"/>
        public RCSet<T> GetContents(RCNumRectangle area)
        {
            if (area == RCNumRectangle.Undefined) { throw new ArgumentNullException("area"); }

            RCSet<T> retList = new RCSet<T>();
            this.rootNode.CollectContents(area, ref retList);
            return retList;
        }

        /// <see cref="ISearchTree<T>.AttachContent"/>
        public void AttachContent(T content)
        {
            if (content == null) { throw new ArgumentNullException("content"); }
            if (content.BoundingBox == RCNumRectangle.Undefined) { throw new ArgumentNullException("content.BoundingBox"); }
            if (this.attachedContents.Contains(content)) { throw new InvalidOperationException("The given content is already attached to this search tree!"); }

            content.BoundingBoxChanging += this.OnContentPositionChanging;
            content.BoundingBoxChanged += this.OnContentPositionChanged;
            this.rootNode.AttachContent(content);
            this.attachedContents.Add(content);
        }

        /// <see cref="ISearchTree<T>.DetachContent"/>
        public void DetachContent(T content)
        {
            if (content == null) { throw new ArgumentNullException("content"); }
            if (!this.attachedContents.Contains(content)) { throw new InvalidOperationException("The given content is not attached to this search tree!"); }

            this.attachedContents.Remove(content);
            this.rootNode.DetachContent(content);
            content.BoundingBoxChanging -= this.OnContentPositionChanging;
            content.BoundingBoxChanged -= this.OnContentPositionChanged;
        }

        /// <see cref="ISearchTree<T>.HasContent"/>
        public bool HasContent(T content)
        {
            if (content == null) { throw new ArgumentNullException("content"); }
            return this.attachedContents.Contains(content);
        }

        #endregion ISearchTree<T> methods

        /// <see cref="ISearchTreeContent.BoundingBoxChanging"/>
        private void OnContentPositionChanging(ISearchTreeContent sender)
        {
            this.rootNode.DetachContent((T)sender);
        }

        /// <see cref="ISearchTreeContent.BoundingBoxChanged"/>
        private void OnContentPositionChanged(ISearchTreeContent sender)
        {
            this.rootNode.AttachContent((T)sender);
        }

        /// <summary>
        /// List of the contents attached to this search tree.
        /// </summary>
        private RCSet<T> attachedContents;

        /// <summary>
        /// Reference to the root of the BSP-tree.
        /// </summary>
        private BspSearchTreeNode<T> rootNode;
    }
}
