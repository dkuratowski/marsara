using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Represents a path computed by the pathfinder.
    /// </summary>
    abstract class Path : IPath
    {
        /// <summary>
        /// Constructs a new Path instance.
        /// </summary>
        public Path()
        {
            this.nodesOnPath = null;
            this.blockedEdges = new HashSet<Tuple<int, int>>();
        }

        #region IPath methods

        /// <see cref="IPath.IsReadyForUse"/>
        public abstract bool IsReadyForUse { get; }

        /// <see cref="IPath.IsTargetFound"/>
        public bool IsTargetFound
        {
            get
            {
                if (!this.IsReadyForUse) { throw new InvalidOperationException("Path is not ready for use!"); }
                if (this.nodesOnPath == null) { this.nodesOnPath = this.CollectNodesOnPath(); }
                return this.nodesOnPath[this.Length - 1] == this.ToNode;
            }
        }
        
        /// <see cref="IPath.this[]"/>
        public RCIntRectangle this[int index]
        {
            get
            {
                if (!this.IsReadyForUse) { throw new InvalidOperationException("Path is not ready for use!"); }
                if (this.nodesOnPath == null) { this.nodesOnPath = this.CollectNodesOnPath(); }
                return this.nodesOnPath[index].AreaOnMap;
            }
        }

        /// <see cref="IPath.Length"/>
        public int Length
        {
            get
            {
                if (!this.IsReadyForUse) { throw new InvalidOperationException("Path is not ready for use!"); }
                if (this.nodesOnPath == null) { this.nodesOnPath = this.CollectNodesOnPath(); }
                return this.nodesOnPath.Count;
            }
        }

        /// <see cref="IPath.ForgetBlockedEdges"/>
        public void ForgetBlockedEdges()
        {
            if (!this.IsReadyForUse) { throw new InvalidOperationException("Path is not ready for use!"); }
            this.blockedEdges.Clear();
        }

        #endregion IPath methods

        #region Methods for debugging

        /// <summary>
        /// Gets the list of nodes in the completed list.
        /// </summary>
        /// <remarks>TODO: this is only for debugging!</remarks>
        protected internal abstract IEnumerable<PFTreeNode> CompletedNodes { get; }

        #endregion Methods for debugging

        #region Internal methods

        /// <summary>
        /// Gets a node of the path.
        /// </summary>
        /// <param name="index">The index of the node to get.</param>
        /// <returns>The node at the given index.</returns>
        /// <exception cref="InvalidOperationException">If the path is not ready for use.</exception>
        public PFTreeNode GetPathNode(int index)
        {
            if (!this.IsReadyForUse) { throw new InvalidOperationException("Path is not ready for use!"); }
            if (this.nodesOnPath == null) { this.nodesOnPath = this.CollectNodesOnPath(); }
            return this.nodesOnPath[index];
        }

        /// <summary>
        /// Checks whether the edge between the two given pathfinder tree nodes is blocked or not.
        /// </summary>
        /// <param name="nodeA">The first node of the edge.</param>
        /// <param name="nodeB">The second node of the edge.</param>
        /// <returns>True if the edge is blocked, false otherwise.</returns>
        public bool IsEdgeBlocked(PFTreeNode nodeA, PFTreeNode nodeB)
        {
            return this.blockedEdges.Contains(new Tuple<int, int>(nodeA.Index, nodeB.Index)) ||
                   this.blockedEdges.Contains(new Tuple<int, int>(nodeB.Index, nodeA.Index));
        }

        /// <summary>
        /// Gets the source node of this path.
        /// </summary>
        protected abstract PFTreeNode FromNode { get; }

        /// <summary>
        /// Gets the target node of this path.
        /// </summary>
        protected abstract PFTreeNode ToNode { get; }

        #endregion Internal methods

        /// <summary>
        /// Collects the nodes along the computed path.
        /// </summary>
        protected abstract List<PFTreeNode> CollectNodesOnPath();

        /// <summary>
        /// The list of the nodes among this path.
        /// </summary>
        private List<PFTreeNode> nodesOnPath;

        /// <summary>
        /// List of the blocked edges in the pathfinding graph.
        /// </summary>
        private HashSet<Tuple<int, int>> blockedEdges;
    }
}
