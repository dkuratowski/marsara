using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.PublicInterfaces;

namespace RC.Engine.Core
{
    /// <summary>
    /// Map content manager that uses a Binary Space Partitioning (BSP) tree.
    /// </summary>
    public class BspMapContentManager<T> : IMapContentManager<T> where T : IMapContent
    {
        /// <summary>
        /// Constructs a BspMapContentManager instance.
        /// </summary>
        /// <param name="area">The rectangular area of the world in world-coordinates.</param>
        /// <param name="nodeCapacity">The maximum number of contents a BSP-tree node can hold without subdivision.</param>
        /// <param name="minNodeSize">The minimum size of a BSP-node.</param>
        public BspMapContentManager(RCNumRectangle area, int nodeCapacity, int minNodeSize)
        {
            if (area == RCNumRectangle.Undefined) { throw new ArgumentNullException("area"); }
            if (nodeCapacity <= 0) { throw new ArgumentOutOfRangeException("nodeCapacity", "Node capacity must be greater than 0!"); }
            if (minNodeSize <= 0) { throw new ArgumentOutOfRangeException("minNodeSize", "Minimum BSP-node size must be positive!"); }

            this.attachedContents = new HashSet<IMapContent>();
            this.rootNode = new BspTreeNode<T>(area, nodeCapacity, minNodeSize);
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

        #region IMapContentManager methods

        /// <see cref="IMapContentManager<T>.GetContents"/>
        public HashSet<T> GetContents(RCNumVector position)
        {
            if (position == RCNumVector.Undefined) { throw new ArgumentNullException("position"); }

            HashSet<T> retList = new HashSet<T>();
            this.rootNode.CollectContents(position, ref retList);
            return retList;
        }

        /// <see cref="IMapContentManager<T>.GetContents"/>
        public HashSet<T> GetContents(RCNumRectangle selectionBox)
        {
            if (selectionBox == RCNumRectangle.Undefined) { throw new ArgumentNullException("selectionBox"); }

            HashSet<T> retList = new HashSet<T>();
            this.rootNode.CollectContents(selectionBox, ref retList);
            return retList;
        }

        /// <see cref="IMapContentManager<T>.AttachContent"/>
        public void AttachContent(T content)
        {
            if (content == null) { throw new ArgumentNullException("content"); }
            if (content.Position == RCNumRectangle.Undefined) { throw new ArgumentNullException("content.Position"); }
            if (this.attachedContents.Contains(content)) { throw new InvalidOperationException("The given content is already attached to this content manager!"); }

            content.PositionChanging += this.OnContentPositionChanging;
            content.PositionChanged += this.OnContentPositionChanged;
            this.rootNode.AttachContent(content);
            this.attachedContents.Add(content);
        }

        /// <see cref="IMapContentManager<T>.DetachContent"/>
        public void DetachContent(T content)
        {
            if (content == null) { throw new ArgumentNullException("content"); }
            if (!this.attachedContents.Contains(content)) { throw new InvalidOperationException("The given content is not attached to this content manager!"); }

            this.attachedContents.Remove(content);
            this.rootNode.DetachContent(content);
            content.PositionChanging -= this.OnContentPositionChanging;
            content.PositionChanged -= this.OnContentPositionChanged;
        }

        /// <see cref="IMapContentManager<T>.HasContent"/>
        public bool HasContent(T content)
        {
            if (content == null) { throw new ArgumentNullException("content"); }
            return this.attachedContents.Contains(content);
        }

        #endregion IMapContentManager methods

        /// <see cref="IMapContent.PositionChanging"/>
        private void OnContentPositionChanging(IMapContent sender)
        {
            this.rootNode.DetachContent((T)sender);
        }

        /// <see cref="IMapContent.PositionChanged"/>
        private void OnContentPositionChanged(IMapContent sender)
        {
            this.rootNode.AttachContent((T)sender);
        }

        /// <summary>
        /// List of the map contents attached to this content manager.
        /// </summary>
        private HashSet<IMapContent> attachedContents;

        /// <summary>
        /// Reference to the root of the BSP-tree.
        /// </summary>
        private BspTreeNode<T> rootNode;
    }
}
