using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Engine.Pathfinder.Core
{
    /// <summary>
    /// Represents a cell on a grid-layer.
    /// </summary>
    class Cell
    {
        /// <summary>
        /// Constructs a cell instance.
        /// </summary>
        /// <param name="coords">The coordinates of this cell on the grid-layer.</param>
        /// <param name="gridLayer">The grid-layer that this cell belongs to.</param>
        public Cell(RCIntVector coords, GridLayer gridLayer)
        {
            this.coords = coords;
            this.gridLayer = gridLayer;

            /// Set the neighbours of this cell.
            RCIntVector northWestNeighbourCoords = this.coords + GridDirections.DIRECTION_TO_VECTOR[GridDirections.NORTH_WEST];
            RCIntVector northNeighbourCoords = this.coords + GridDirections.DIRECTION_TO_VECTOR[GridDirections.NORTH];
            RCIntVector northEastNeighbourCoords = this.coords + GridDirections.DIRECTION_TO_VECTOR[GridDirections.NORTH_EAST];
            RCIntVector westNeighbourCoords = this.coords + GridDirections.DIRECTION_TO_VECTOR[GridDirections.WEST];
            this.neighbours = new Cell[GridDirections.DIRECTION_COUNT];
            this.neighbours[GridDirections.NORTH_WEST] = this.gridLayer[northWestNeighbourCoords.X, northWestNeighbourCoords.Y];
            this.neighbours[GridDirections.NORTH] = this.gridLayer[northNeighbourCoords.X, northNeighbourCoords.Y];
            this.neighbours[GridDirections.NORTH_EAST] = this.gridLayer[northEastNeighbourCoords.X, northEastNeighbourCoords.Y];
            this.neighbours[GridDirections.WEST] = this.gridLayer[westNeighbourCoords.X, westNeighbourCoords.Y];

            /// Set this cell as the neighbour of its neighbours.
            if (this.neighbours[GridDirections.NORTH_WEST] != null) { this.neighbours[GridDirections.NORTH_WEST].neighbours[GridDirections.SOUTH_EAST] = this; }
            if (this.neighbours[GridDirections.NORTH] != null) { this.neighbours[GridDirections.NORTH].neighbours[GridDirections.SOUTH] = this; }
            if (this.neighbours[GridDirections.NORTH_EAST] != null) { this.neighbours[GridDirections.NORTH_EAST].neighbours[GridDirections.SOUTH_WEST] = this; }
            if (this.neighbours[GridDirections.WEST] != null) { this.neighbours[GridDirections.WEST].neighbours[GridDirections.EAST] = this; }
        }

        /// <summary>
        /// Gets the neighbour of this cell at the given direction.
        /// </summary>
        /// <param name="direction">The direction of the neighbour to get (see GridDirections class for more information).</param>
        /// <returns>The neighbour of this cell at the given direction or null if this cell has no neighbour at the given direction.</returns>
        public Cell GetNeighbour(int direction)
        {
            return this.neighbours[direction];
        }

        /// <summary>
        /// Gets the coordinates of this cell on the grid-layer.
        /// </summary>
        public RCIntVector Coords { get { return this.coords; } }

        /// <summary>
        /// The coordinates of this cell on the grid-layer.
        /// </summary>
        private RCIntVector coords;

        /// <summary>
        /// References to the neighbours of this cell.
        /// </summary>
        private Cell[] neighbours;

        /// <summary>
        /// The grid-layer that this cell belongs to.
        /// </summary>
        private GridLayer gridLayer;
    }
}
