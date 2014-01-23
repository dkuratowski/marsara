using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Represents a convex region around a pathfinder tree node.
    /// </summary>
    class Region
    {
        /// <summary>
        /// Constructs a convex region around the given pathfinder tree node.
        /// </summary>
        /// <param name="center">The center of the region. Must be a leaf node.</param>
        /// <param name="radius">The radius of the region.</param>
        public Region(PFTreeNode center, int radius)
        {
            if (!center.IsLeafNode) { throw new ArgumentException("The given node must be a leaf node!", "center"); }
            if (radius < 0) { throw new ArgumentOutOfRangeException("radius"); }

            this.refCount = 0;
            this.nodesOfRegion = new HashSet<PFTreeNode>();

            RegionFindAlgorithm findAlgo = new RegionFindAlgorithm(center, radius, 5000);
            findAlgo.Continue();

            foreach (PathNode node in findAlgo.CompletedNodes)
            {
                if (node.PreviousNode == null || node.PreviousNode.DistanceFromSource <= radius) { this.nodesOfRegion.Add(node.Node); }
            }
        }

        /// <summary>
        /// Increments the reference counter of this Region object. When the reference counter becomes greater than 0, this region object
        /// registers itself to it's contained nodes.
        /// </summary>
        public void AddRef()
        {
            if (this.refCount == 0)
            {
                /// Add the collected nodes to this region.
                foreach (PFTreeNode node in this.nodesOfRegion) { node.AddToRegion(this); }
            }
            this.refCount++;
        }

        /// <summary>
        /// Decrements the reference counter of this Region object. When the reference counter becomes 0, this region object unregisters
        /// itself from it's contained nodes.
        /// </summary>
        public void Release()
        {
            if (this.refCount == 0) { throw new InvalidOperationException("The reference counter is 0!"); }

            this.refCount--;
            if (this.refCount == 0)
            {
                /// Remove the collected nodes from this region.
                foreach (PFTreeNode node in this.nodesOfRegion) { node.RemoveFromRegion(this); }
            }
        }

        /// <summary>
        /// Checks whether the given node is contained by this Region.
        /// </summary>
        /// <param name="node">The node to check.</param>
        /// <returns>True if the given node is contained by this Region; otherwise false.</returns>
        public bool HasNode(PFTreeNode node) { return this.nodesOfRegion.Contains(node); }

        #region Methods for debugging

        /// <summary>
        /// Gets the list of nodes in this region.
        /// </summary>
        /// <remarks>TODO: this is only for debugging!</remarks>
        internal IEnumerable<PFTreeNode> ContainedNodes
        {
            get
            {
                return this.nodesOfRegion;
            }
        }

        #endregion Methods for debugging

        /// <summary>
        /// List of the nodes contained by this region.
        /// </summary>
        private HashSet<PFTreeNode> nodesOfRegion;

        /// <summary>
        /// The reference counter of this region. When the reference counter becomes greater than 0, this region object registers itself to
        /// it's contained nodes. When the referenc counter becomes 0, this region object unregisters itself from it's contained nodes.
        /// </summary>
        private int refCount;
    }
}
