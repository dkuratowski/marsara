using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Represents an algorithm that collects the nodes from the region around a given node.
    /// </summary>
    class RegionFindAlgorithm : PathFindingAlgorithm
    {
        /// <summary>
        /// Constructs an instance of a region finding algorithm.
        /// </summary>
        /// <param name="center">The center node of the region.</param>
        /// <param name="radius">The radius of the region.</param>
        /// <param name="iterationLimit">The maximum number of iterations to execute when searching this path.</param>
        public RegionFindAlgorithm(PFTreeNode center, int radius, int iterationLimit)
            : base(center, 0, iterationLimit)
        {
            this.center = center;
            this.radius = radius;
        }

        #region Overrides

        /// <see cref="PathFindingAlgorithm.CheckExitCriteria"/>
        protected override bool CheckExitCriteria(PFTreeNode currentNode)
        {
            /// No exit criteria.
            return false;
        }

        /// <see cref="PathFindingAlgorithm.CheckExitCriteria"/>
        protected override bool CheckNeighbour(PFTreeNode current, PFTreeNode neighbour)
        {
            return this.center.IsWalkable == neighbour.IsWalkable && PathFindingAlgorithm.ComputeDistance(this.center.Center, current.Center) <= this.radius;
        }

        #endregion Overrides

        /// <summary>
        /// The center node of the region.
        /// </summary>
        private PFTreeNode center;

        /// <summary>
        /// The radius of the region.
        /// </summary>
        private int radius;
    }
}
