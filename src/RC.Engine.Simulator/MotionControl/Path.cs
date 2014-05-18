using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// Represents a path computed by the pathfinder.
    /// </summary>
    class Path : IPath
    {
        /// <summary>
        /// Constructs a new Path instance.
        /// </summary>
        /// <param name="searchAlgorithm">The algorithm that computes the path.</param>
        /// <param name="toCoords">The target coordinates of the path.</param>
        public Path(PathFindingAlgorithm searchAlgorithm, RCNumVector toCoords)
        {
            this.nodesOnPath = null;
            this.searchAlgorithm = searchAlgorithm;
            this.toCoords = toCoords;
        }

        #region IPath methods

        /// <see cref="IPath.IsReadyForUse"/>
        public bool IsReadyForUse { get { return this.searchAlgorithm.IsFinished; } }

        /// <see cref="IPath.IsTargetFound"/>
        public bool IsTargetFound
        {
            get
            {
                if (!this.IsReadyForUse) { throw new InvalidOperationException("Path is not ready for use!"); }
                if (this.nodesOnPath == null) { this.nodesOnPath = this.CollectNodesOnPath(); }
                return this.nodesOnPath[this.Length - 1].Polygon.Contains(this.ToCoords);
            }
        }
        
        /// <see cref="IPath.this[]"/>
        public INavMeshNode this[int index]
        {
            get
            {
                if (!this.IsReadyForUse) { throw new InvalidOperationException("Path is not ready for use!"); }
                if (this.nodesOnPath == null) { this.nodesOnPath = this.CollectNodesOnPath(); }
                return this.nodesOnPath[index];
            }
        }

        /// <see cref="IPath.IndexOf"/>
        public int IndexOf(INavMeshNode node)
        {
            if (!this.IsReadyForUse) { throw new InvalidOperationException("Path is not ready for use!"); }
            if (this.nodesOnPath == null) { this.nodesOnPath = this.CollectNodesOnPath(); }
            return this.nodesOnPath.IndexOf(node);
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

        /// <see cref="IPath.ToCoords"/>
        public RCNumVector ToCoords { get { return this.toCoords; } }

        #endregion IPath methods

        #region Methods for debugging

        /// <summary>
        /// Gets the list of nodes in the completed list.
        /// </summary>
        /// <remarks>TODO: this is only for debugging!</remarks>
        internal IEnumerable<INavMeshNode> CompletedNodes
        {
            get
            {
                List<INavMeshNode> retList = new List<INavMeshNode>();
                foreach (PathNode completedNode in this.searchAlgorithm.CompletedNodes) { retList.Add(completedNode.Node); }
                return retList;
            }
        }

        #endregion Methods for debugging

        #region Internal methods

        /// <summary>
        /// Gets a node of the path.
        /// </summary>
        /// <param name="index">The index of the node to get.</param>
        /// <returns>The node at the given index.</returns>
        /// <exception cref="InvalidOperationException">If the path is not ready for use.</exception>
        public INavMeshNode GetPathNode(int index)
        {
            if (!this.IsReadyForUse) { throw new InvalidOperationException("Path is not ready for use!"); }
            if (this.nodesOnPath == null) { this.nodesOnPath = this.CollectNodesOnPath(); }
            return this.nodesOnPath[index];
        }

        /// <summary>
        /// Gets the source node of this path.
        /// </summary>
        public INavMeshNode FromNode { get { return this.searchAlgorithm.FromNode.Node; } }

        #endregion Internal methods

        /// <summary>
        /// Collects the nodes along the computed path.
        /// </summary>
        private List<INavMeshNode> CollectNodesOnPath()
        {
            List<INavMeshNode> retList = new List<INavMeshNode>();
            PathNode currNode = this.searchAlgorithm.BestNode;
            retList.Add(currNode.Node);
            while (currNode != this.searchAlgorithm.FromNode)
            {
                currNode = currNode.PreviousNode;
                retList.Add(currNode.Node);
            }
            retList.Reverse();
            return retList;
        }

        /// <summary>
        /// The list of the nodes along this path.
        /// </summary>
        private List<INavMeshNode> nodesOnPath;

        /// <summary>
        /// Reference to the algorithm that computes this path.
        /// </summary>
        private PathFindingAlgorithm searchAlgorithm;

        /// <summary>
        /// The target coordinates of this path.
        /// </summary>
        private RCNumVector toCoords;
    }
}
