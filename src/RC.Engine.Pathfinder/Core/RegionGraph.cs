using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Engine.Pathfinder.Core
{
    /// <summary>
    /// Represents a graph of grid regions for a high-level pathfinding.
    /// </summary>
    class RegionGraph : IGraph<Region>
    {
        /// <summary>
        /// Constructs a RegionGraph instance with the given target region for the given agent.
        /// </summary>
        /// <param name="targetRegion">The given target region.</param>
        /// <param name="agent">The given agent.</param>
        public RegionGraph(Region targetRegion, Agent agent)
        {
            this.targetRegion = targetRegion;
            this.agent = agent;
        }

        #region IGraph<Region> members

        /// <see cref="IGraph&lt;Region&gt;.Distance"/>
        public int Distance(Region regionA, Region regionB)
        {
            int horizontalDistance = Math.Abs(regionA.Subdivision.Sector.Center.X - regionB.Subdivision.Sector.Center.X);
            int verticalDistance = Math.Abs(regionA.Subdivision.Sector.Center.Y - regionB.Subdivision.Sector.Center.Y);
            int difference = Math.Abs(horizontalDistance - verticalDistance);
            return Math.Min(horizontalDistance, verticalDistance) * Grid.DIAGONAL_UNIT_DISTANCE + difference * Grid.STRAIGHT_UNIT_DISTANCE;
        }

        /// <see cref="IGraph&lt;Region&gt;.EstimationToTarget"/>
        public int EstimationToTarget(Region node)
        {
            return this.Distance(node, this.targetRegion);
        }

        /// <see cref="IGraph&lt;Region&gt;.GetNeighbours"/>
        public IEnumerable<Region> GetNeighbours(Region node)
        {
            return node.GetNeighbours(this.agent);
        }

        /// <see cref="IGraph&lt;Region&gt;.IsTargetNode"/>
        public bool IsTargetNode(Region node)
        {
            return node == this.targetRegion;
        }

        #endregion IGraph<Region> members

        /// <summary>
        /// Reference to the target region.
        /// </summary>
        private Region targetRegion;

        /// <summary>
        /// Reference to the agent.
        /// </summary>
        private Agent agent;
    }
}
