using RC.Prototypes.NewPathfinding.MotionControl;
using RC.Prototypes.NewPathfinding.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Prototypes.JumpPointSearch.MotionControl
{
    /// <summary>
    /// Represents the graph of a transit region for a low-level pathfinding.
    /// </summary>
    class TransitRegionGraph : IGraph<Cell>
    {
        /// <summary>
        /// Constructs a TransitRegionGraph for a low-level pathfinding.
        /// </summary>
        /// <param name="currentRegion">The current region on the high-level path.</param>
        /// <param name="nextRegion">The next region on the high-level path.</param>
        public TransitRegionGraph(Region currentRegion, Region nextRegion)
        {
            this.currentRegion = currentRegion;
            this.nextRegion = nextRegion;
        }

        #region IGraph<Cell> members

        /// <see cref="IGraph&lt;Cell&gt;.Distance"/>
        public int Distance(Cell cellA, Cell cellB)
        {
            int horizontalDistance = Math.Abs(cellA.Coords.X - cellB.Coords.X);
            int verticalDistance = Math.Abs(cellA.Coords.Y - cellB.Coords.Y);
            int difference = Math.Abs(horizontalDistance - verticalDistance);
            return Math.Min(horizontalDistance, verticalDistance) * Grid.DIAGONAL_UNIT_DISTANCE + difference * Grid.STRAIGHT_UNIT_DISTANCE;
        }

        /// <see cref="IGraph&lt;Cell&gt;.EstimationToTarget"/>
        public int EstimationToTarget(Cell node)
        {
            int horizontalDistance = Math.Abs(node.Coords.X - this.nextRegion.Sector.Center.X);
            int verticalDistance = Math.Abs(node.Coords.Y - this.nextRegion.Sector.Center.Y);
            int difference = Math.Abs(horizontalDistance - verticalDistance);
            return Math.Min(horizontalDistance, verticalDistance) * Grid.DIAGONAL_UNIT_DISTANCE + difference * Grid.STRAIGHT_UNIT_DISTANCE;
        }

        /// <see cref="IGraph&lt;Cell&gt;.GetNeighbours"/>
        public IEnumerable<Cell> GetNeighbours(Cell node)
        {
            Cell north = node.GetNeighbour(GridDirections.NORTH);
            if (north != null && north.Sector == this.currentRegion.Sector && north.IsWalkable(currentRegion.ObjectSize)) { yield return north; }

            Cell northEast = node.GetNeighbour(GridDirections.NORTH_EAST);
            if (northEast != null && northEast.Sector == this.currentRegion.Sector && northEast.IsWalkable(currentRegion.ObjectSize)) { yield return northEast; }

            Cell east = node.GetNeighbour(GridDirections.EAST);
            if (east != null && east.Sector == this.currentRegion.Sector && east.IsWalkable(currentRegion.ObjectSize)) { yield return east; }

            Cell southEast = node.GetNeighbour(GridDirections.SOUTH_EAST);
            if (southEast != null && southEast.Sector == this.currentRegion.Sector && southEast.IsWalkable(currentRegion.ObjectSize)) { yield return southEast; }

            Cell south = node.GetNeighbour(GridDirections.SOUTH);
            if (south != null && south.Sector == this.currentRegion.Sector && south.IsWalkable(currentRegion.ObjectSize)) { yield return south; }

            Cell southWest = node.GetNeighbour(GridDirections.SOUTH_WEST);
            if (southWest != null && southWest.Sector == this.currentRegion.Sector && southWest.IsWalkable(currentRegion.ObjectSize)) { yield return southWest; }

            Cell west = node.GetNeighbour(GridDirections.WEST);
            if (west != null && west.Sector == this.currentRegion.Sector && west.IsWalkable(currentRegion.ObjectSize)) { yield return west; }

            Cell northWest = node.GetNeighbour(GridDirections.NORTH_WEST);
            if (northWest != null && northWest.Sector == this.currentRegion.Sector && northWest.IsWalkable(currentRegion.ObjectSize)) { yield return northWest; }

        }

        /// <see cref="IGraph&lt;Cell&gt;.IsTargetNode"/>
        public bool IsTargetNode(Cell node)
        {
            return this.currentRegion.GetExistsToNeighbours(this.nextRegion).Contains(node);
        }

        #endregion IGraph<Cell> members

        /// <summary>
        /// The current region on the high-level path.
        /// </summary>
        private Region currentRegion;

        /// <summary>
        /// The next region on the high-level path.
        /// </summary>
        private Region nextRegion;
    }
}
