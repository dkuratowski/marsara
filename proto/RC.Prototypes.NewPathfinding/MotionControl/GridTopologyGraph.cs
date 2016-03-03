using RC.Prototypes.NewPathfinding.Pathfinding;
using RC.Prototypes.NewPathfinding.MotionControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Prototypes.NewPathfinding.MotionControl
{
    /// <summary>
    /// Represents the topology graph of the grid for a high-level pathfinding.
    /// </summary>
    class GridTopologyGraph : IGraph<Region>
    {
        /// <summary>
        /// Constructs a GridTopologyGraph instance for the given target cell.
        /// </summary>
        /// <param name="targetCell">The given target cell.</param>
        /// <param name="objectSize">The size of objects.</param>
        public GridTopologyGraph(Cell targetCell, int objectSize)
        {
            this.targetRegion = targetCell.GetRegion(objectSize);
        }

        #region IGraph<Region> members

        /// <see cref="IGraph&lt;Region&gt;.Distance"/>
        public int Distance(Region regionA, Region regionB)
        {
            int horizontalDistance = Math.Abs(regionA.Sector.Center.X - regionB.Sector.Center.X);
            int verticalDistance = Math.Abs(regionA.Sector.Center.Y - regionB.Sector.Center.Y);
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
            return node.GetNeighbours();
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
    }
}
