using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Represents a simple A* pathfinding algorithm between two nodes on the map.
    /// </summary>
    class DirectPathFindingAlgorithm : PathFindingAlgorithm
    {
        /// <summary>
        /// Constructs an instance of an A* pathfinding algorithm.
        /// </summary>
        /// <param name="fromNode">The starting node of the pathfinding.</param>
        /// <param name="toNode">The target node of the pathfinding.</param>
        /// <param name="iterationLimit">The maximum number of iterations to execute when searching this path.</param>
        /// <param name="sourceEstimation">The estimated length of the path from the source node to the target.</param>
        public DirectPathFindingAlgorithm(PFTreeNode fromNode, PFTreeNode toNode, int iterationLimit)
            : base(fromNode, PathFindingAlgorithm.ComputeDistance(fromNode.Center, toNode.Center), iterationLimit)
        {
            this.toNode = toNode;
            this.blockedEdges = new HashSet<Tuple<int, int>>();
        }
                
        /// <summary>
        /// Constructs a DirectPathFindingAlgorithm based on a previously computed path.
        /// </summary>
        /// <param name="detouredPath">The original path.</param>
        /// <param name="abortedSectionIdx">The index of the aborted section of the original path.</param>
        /// <param name="iterationLimit">The maximum number of iterations to execute when searching this path.</param>
        public DirectPathFindingAlgorithm(Path detouredPath, int abortedSectionIdx, int iterationLimit)
            : base(detouredPath.GetPathNode(abortedSectionIdx),
                   PathFindingAlgorithm.ComputeDistance(detouredPath.GetPathNode(abortedSectionIdx).Center, detouredPath.GetPathNode(detouredPath.Length - 1).Center), iterationLimit)
        {
            this.toNode = detouredPath.ToNode;
            this.blockedEdges = new HashSet<Tuple<int, int>>();
            detouredPath.CopyBlockedEdges(ref this.blockedEdges);
            this.blockedEdges.Add(new Tuple<int, int>(detouredPath.GetPathNode(abortedSectionIdx).Index, detouredPath.GetPathNode(abortedSectionIdx + 1).Index));
        }

        /// <summary>
        /// Gets the target node of the pathfinding.
        /// </summary>
        public PFTreeNode ToNode { get { return this.toNode; } }

        /// <summary>
        /// Copies the blocked edges of this search algorithm to the target set.
        /// </summary>
        /// <param name="targetSet">The target set to copy.</param>
        public void CopyBlockedEdges(ref HashSet<Tuple<int, int>> targetSet)
        {
            foreach (Tuple<int, int> blockedEdge in this.blockedEdges) { targetSet.Add(blockedEdge); }
        }

        #region Overrides

        /// <see cref="PathFindingAlgorithm.CheckNeighbour"/>
        protected override bool CheckNeighbour(PFTreeNode current, PFTreeNode neighbour)
        {
            return neighbour.IsWalkable && !this.IsEdgeBlocked(current, neighbour);
        }

        /// <see cref="PathFindingAlgorithm.GetEstimation"/>
        protected override int GetEstimation(PFTreeNode node)
        {
            return PathFindingAlgorithm.ComputeDistance(node.Center, this.toNode.Center);
        }

        /// <see cref="PathFindingAlgorithm.CheckExitCriteria"/>
        protected override bool CheckExitCriteria(PFTreeNode currentNode)
        {
            return currentNode == this.toNode;
        }

        #endregion Overrides

        /// <summary>
        /// Checks whether the edge between the two given pathfinder tree nodes is blocked or not.
        /// </summary>
        /// <param name="nodeA">The first node of the edge.</param>
        /// <param name="nodeB">The second node of the edge.</param>
        /// <returns>True if the edge is blocked, false otherwise.</returns>
        private bool IsEdgeBlocked(PFTreeNode nodeA, PFTreeNode nodeB)
        {
            return this.blockedEdges.Contains(new Tuple<int, int>(nodeA.Index, nodeB.Index)) ||
                   this.blockedEdges.Contains(new Tuple<int, int>(nodeB.Index, nodeA.Index));
        }

        /// <summary>
        /// The target node of the pathfinding.
        /// </summary>
        private PFTreeNode toNode;

        /// <summary>
        /// Contains the blocked edges that have to be bypassed by this search algorithm.
        /// </summary>
        private HashSet<Tuple<int, int>> blockedEdges;
    }
}
