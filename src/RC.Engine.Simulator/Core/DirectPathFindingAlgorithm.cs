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
        }

        /// <summary>
        /// Gets the target node of the pathfinding.
        /// </summary>
        public PFTreeNode ToNode { get { return this.toNode; } }

        #region Overrides

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
        /// The target node of the pathfinding.
        /// </summary>
        private PFTreeNode toNode;
    }
}
