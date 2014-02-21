using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Common
{
    /// <summary>
    /// This class represents a node in the BSP-tree.
    /// </summary>
    class BspSearchTreeNode<T> where T : ISearchTreeContent
    {
        /// <summary>
        /// Constructs a BspSearchTreeNode instance.
        /// </summary>
        /// <param name="area">The rectangular area covered by this BspSearchTreeNode.</param>
        /// <param name="capacity">The maximum number of contents this node can hold without subdivision.</param>
        /// <param name="minSize">The minimum size of a BSP-node.</param>
        public BspSearchTreeNode(RCNumRectangle area, int capacity, int minSize)
        {
            this.contents = new HashSet<T>();
            this.isLeaf = true;
            this.capacity = capacity;
            this.minSize = minSize;
            this.area = area;
            this.firstChild = null;
            this.secondChild = null;
        }

        /// <summary>
        /// Collects the boundaries of all BSP-nodes.
        /// </summary>
        /// <remarks>TODO: this method is only for testing.</remarks>
        public void CollectBoundaries(ref List<RCNumRectangle> boundaries)
        {
            boundaries.Add(this.area);
            if (!this.isLeaf)
            {
                this.firstChild.CollectBoundaries(ref boundaries);
                this.secondChild.CollectBoundaries(ref boundaries);
            }
        }

        /// <summary>
        /// Attaches the given content to this BspSearchTreeNode and it's appropriate descendants.
        /// </summary>
        /// <param name="content">The content to be attached.</param>
        public void AttachContent(T content)
        {
            /// Attach the content to this node.
            this.contents.Add(content);

            if (this.isLeaf)
            {
                /// Leaf node -> check the capacity.
                if (this.contents.Count > this.capacity && this.area.Width > this.minSize && this.area.Height > this.minSize)
                {
                    /// Capacity exceeded -> subdivide this node and propagate the call to the appropriat child(ren).
                    this.Subdivide();
                    foreach (T item in this.contents)
                    {
                        if (item.BoundingBox.IntersectsWith(this.firstChild.area)) { this.firstChild.AttachContent(item); }
                        if (item.BoundingBox.IntersectsWith(this.secondChild.area)) { this.secondChild.AttachContent(item); }
                    }
                }
            }
            else
            {
                /// Not a leaf node -> propagate the call to the appropriate child(ren).
                if (content.BoundingBox.IntersectsWith(this.firstChild.area)) { this.firstChild.AttachContent(content); }
                if (content.BoundingBox.IntersectsWith(this.secondChild.area)) { this.secondChild.AttachContent(content); }
            }
        }

        /// <summary>
        /// Detaches the given content from this BspSearchTreeNode and it's appropriate descendants.
        /// </summary>
        /// <param name="content">The content to be detached.</param>
        public void DetachContent(T content)
        {
            /// Detach the content from this node.
            this.contents.Remove(content);

            if (!this.isLeaf)
            {
                /// Not a leaf node -> check the capacity.
                if (this.contents.Count <= this.capacity)
                {
                    /// Below the capacity again -> merge the children.
                    this.MergeChildren();
                }
                else
                {
                    /// Still over capacity -> propagate the call to the appropriate child(ren).
                    if (content.BoundingBox.IntersectsWith(this.firstChild.area)) { this.firstChild.DetachContent(content); }
                    if (content.BoundingBox.IntersectsWith(this.secondChild.area)) { this.secondChild.DetachContent(content); }
                }
            }
        }

        /// <summary>
        /// Collects every content attached to this BspSearchTreeNode at the given position.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <param name="outputList">
        /// A list the contains every content attached to this BspSearchTreeNode at the given position.
        /// </param>
        public void CollectContents(RCNumVector position, ref HashSet<T> outputList)
        {
            if (this.isLeaf)
            {
                /// Leaf node -> collect the contents at the given position
                foreach (T content in this.contents)
                {
                    if (content.BoundingBox.Contains(position)) { outputList.Add(content); }
                }
            }
            else
            {
                /// Not a leaf node -> propagate the call to the appropriate child.
                if (this.firstChild.area.Contains(position)) { this.firstChild.CollectContents(position, ref outputList); }
                else if (this.secondChild.area.Contains(position)) { this.secondChild.CollectContents(position, ref outputList); }
            }
        }
        
        /// <summary>
        /// Collects every content attached to this BspSearchTreeNode inside the given rectangular area.
        /// </summary>
        /// <param name="area">The rectangular area.</param>
        /// <param name="outputList">
        /// A list that contains every content attached to this BspSearchTreeNode inside the given rectangular area.
        /// </param>
        public void CollectContents(RCNumRectangle area, ref HashSet<T> outputList)
        {
            if (this.isLeaf)
            {
                /// Leaf node -> collect the contents inside the given selection box.
                foreach (T content in this.contents)
                {
                    if (content.BoundingBox.IntersectsWith(area)) { outputList.Add(content); }
                }
            }
            else
            {
                /// Not a leaf node -> propagate the call to the appropriate child(ren).
                if (this.firstChild.area.IntersectsWith(area)) { this.firstChild.CollectContents(area, ref outputList); }
                if (this.secondChild.area.IntersectsWith(area)) { this.secondChild.CollectContents(area, ref outputList); }
            }
        }

        /// <summary>
        /// Subdivides this BspSearchTreeNode if necessary.
        /// </summary>
        /// <remarks>Subdivision is only allowed in case of leaf-nodes.</remarks>
        private void Subdivide()
        {
            if (!this.isLeaf) { throw new InvalidOperationException("Only leaf-nodes can be subdivided!"); }

            this.isLeaf = false;
            if (this.firstChild == null || this.secondChild == null)
            {
                /// Children have to be created.
                RCNumRectangle firstChildArea = this.area.Width > this.area.Height
                                              ? new RCNumRectangle(this.area.X, this.area.Y, this.area.Width / 2, this.area.Height)
                                              : new RCNumRectangle(this.area.X, this.area.Y, this.area.Width, this.area.Height / 2);
                RCNumRectangle secondChildArea = this.area.Width > this.area.Height
                                               ? firstChildArea + new RCNumVector(firstChildArea.Width, 0)
                                               : firstChildArea + new RCNumVector(0, firstChildArea.Height);
                this.firstChild = new BspSearchTreeNode<T>(firstChildArea, this.capacity, this.minSize);
                this.secondChild = new BspSearchTreeNode<T>(secondChildArea, this.capacity, this.minSize);
            }
            else
            {
                /// Children already exist.
                this.firstChild.isLeaf = true;
                this.secondChild.isLeaf = true;
                this.firstChild.contents.Clear();
                this.secondChild.contents.Clear();
            }
        }

        /// <summary>
        /// Merges the children of this BspSearchTreeNode.
        /// </summary>
        /// <remarks>Merging is only allowed in case of non-leaf-nodes.</remarks>
        private void MergeChildren()
        {
            if (this.isLeaf) { throw new InvalidOperationException("Only non-leaf-nodes can be merged!"); }
            this.isLeaf = true;
        }

        /// <summary>
        /// List of the contents that have intersection with the area covered by this BspSearchTreeNode.
        /// </summary>
        private HashSet<T> contents;

        /// <summary>
        /// Reference to the first child of this BspSearchTreeNode.
        /// </summary>
        private BspSearchTreeNode<T> firstChild;

        /// <summary>
        /// Reference to the second child of this BspSearchTreeNode.
        /// </summary>
        private BspSearchTreeNode<T> secondChild;

        /// <summary>
        /// This flag indicates whether this is a leaf node or not.
        /// </summary>
        private bool isLeaf;

        /// <summary>
        /// The maximum number of contents this node can hold without subdivision.
        /// </summary>
        private int capacity;

        /// <summary>
        /// The minimum size of a BSP-node.
        /// </summary>
        private int minSize;

        /// <summary>
        /// The rectangular area covered by this BspSearchTreeNode.
        /// </summary>
        private RCNumRectangle area;
    }
}
