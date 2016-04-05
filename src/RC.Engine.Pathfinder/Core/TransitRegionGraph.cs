using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Engine.Pathfinder.Core
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
        /// <param name="targetCoords">The coordinates of the target point to which the distance estimation shall be calculated.</param>
        /// <param name="cellsToReach">The list of the cells to reach.</param>
        public TransitRegionGraph(Region currentRegion, RCIntVector targetCoords, RCSet<Cell> cellsToReach)
        {
            this.currentRegion = currentRegion;
            this.targetCoords = targetCoords;
            this.cellsToReach = cellsToReach;
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
            int horizontalDistance = Math.Abs(node.Coords.X - this.targetCoords.X);
            int verticalDistance = Math.Abs(node.Coords.Y - this.targetCoords.Y);
            int difference = Math.Abs(horizontalDistance - verticalDistance);
            return Math.Min(horizontalDistance, verticalDistance) * Grid.DIAGONAL_UNIT_DISTANCE + difference * Grid.STRAIGHT_UNIT_DISTANCE;
        }

        /// <see cref="IGraph&lt;Cell&gt;.GetNeighbours"/>
        public IEnumerable<Cell> GetNeighbours(Cell node)
        {
            Cell north = node.GetNeighbour(GridDirections.NORTH);
            if (north != null && north.Sector == this.currentRegion.Subdivision.Sector && this.currentRegion.Subdivision.IsCellWalkable(north)) { yield return north; }

            Cell northEast = node.GetNeighbour(GridDirections.NORTH_EAST);
            if (northEast != null && northEast.Sector == this.currentRegion.Subdivision.Sector && this.currentRegion.Subdivision.IsCellWalkable(northEast)) { yield return northEast; }

            Cell east = node.GetNeighbour(GridDirections.EAST);
            if (east != null && east.Sector == this.currentRegion.Subdivision.Sector && this.currentRegion.Subdivision.IsCellWalkable(east)) { yield return east; }

            Cell southEast = node.GetNeighbour(GridDirections.SOUTH_EAST);
            if (southEast != null && southEast.Sector == this.currentRegion.Subdivision.Sector && this.currentRegion.Subdivision.IsCellWalkable(southEast)) { yield return southEast; }

            Cell south = node.GetNeighbour(GridDirections.SOUTH);
            if (south != null && south.Sector == this.currentRegion.Subdivision.Sector && this.currentRegion.Subdivision.IsCellWalkable(south)) { yield return south; }

            Cell southWest = node.GetNeighbour(GridDirections.SOUTH_WEST);
            if (southWest != null && southWest.Sector == this.currentRegion.Subdivision.Sector && this.currentRegion.Subdivision.IsCellWalkable(southWest)) { yield return southWest; }

            Cell west = node.GetNeighbour(GridDirections.WEST);
            if (west != null && west.Sector == this.currentRegion.Subdivision.Sector && this.currentRegion.Subdivision.IsCellWalkable(west)) { yield return west; }

            Cell northWest = node.GetNeighbour(GridDirections.NORTH_WEST);
            if (northWest != null && northWest.Sector == this.currentRegion.Subdivision.Sector && this.currentRegion.Subdivision.IsCellWalkable(northWest)) { yield return northWest; }

        }

        /// <see cref="IGraph&lt;Cell&gt;.IsTargetNode"/>
        public bool IsTargetNode(Cell node)
        {
            return this.cellsToReach.Contains(node);
        }

        #endregion IGraph<Cell> members

        /// <summary>
        /// The current region on the high-level path.
        /// </summary>
        private Region currentRegion;

        /// <summary>
        /// The coordinates of the target point to which the distance estimation shall be calculated.
        /// </summary>
        private RCIntVector targetCoords;

        /// <summary>
        /// The list of the cells to reach.
        /// </summary>
        private RCSet<Cell> cellsToReach;
    }
}
