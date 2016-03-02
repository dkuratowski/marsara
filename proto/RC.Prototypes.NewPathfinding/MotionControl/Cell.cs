using RC.Prototypes.NewPathfinding.Pathfinding;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Prototypes.NewPathfinding.MotionControl
{
    /// <summary>
    /// Represents a cell on the grid.
    /// </summary>
    class Cell : INode<Cell>
    {
        /// <summary>
        /// Constructs a Cell that is walkable for objects with any size.
        /// </summary>
        /// <param name="coords">The coordinates of this cell on the grid.</param>
        /// <param name="grid">The grid that this cell belongs to.</param>
        /// <param name="sector">The sector that this cell belongs to.</param>
        public Cell(Point coords, Grid grid, Sector sector)
        {
            this.coords = coords;
            this.grid = grid;
            this.sector = sector;
            this.maxObjectSizeHeap = new MinHeapInt();
            this.maxObjectSizeHeap.Insert(Grid.MAX_OBJECT_SIZE);
            this.regions = new Region[Grid.MAX_OBJECT_SIZE];

            /// Set the neighbours of this cell.
            Point northWestNeighbourCoords = this.coords + GridDirections.DIRECTION_VECTOR[GridDirections.NORTH_WEST];
            Point northNeighbourCoords = this.coords + GridDirections.DIRECTION_VECTOR[GridDirections.NORTH];
            Point northEastNeighbourCoords = this.coords + GridDirections.DIRECTION_VECTOR[GridDirections.NORTH_EAST];
            Point westNeighbourCoords = this.coords + GridDirections.DIRECTION_VECTOR[GridDirections.WEST];
            this.neighbours = new Cell[GridDirections.DIRECTION_COUNT];
            this.neighbours[GridDirections.NORTH_WEST] = this.grid[northWestNeighbourCoords.X, northWestNeighbourCoords.Y];
            this.neighbours[GridDirections.NORTH] = this.grid[northNeighbourCoords.X, northNeighbourCoords.Y];
            this.neighbours[GridDirections.NORTH_EAST] = this.grid[northEastNeighbourCoords.X, northEastNeighbourCoords.Y];
            this.neighbours[GridDirections.WEST] = this.grid[westNeighbourCoords.X, westNeighbourCoords.Y];

            /// Set this cell as the neighbour of its neighbours.
            if (this.neighbours[GridDirections.NORTH_WEST] != null) { this.neighbours[GridDirections.NORTH_WEST].neighbours[GridDirections.SOUTH_EAST] = this; }
            if (this.neighbours[GridDirections.NORTH] != null) { this.neighbours[GridDirections.NORTH].neighbours[GridDirections.SOUTH] = this; }
            if (this.neighbours[GridDirections.NORTH_EAST] != null) { this.neighbours[GridDirections.NORTH_EAST].neighbours[GridDirections.SOUTH_WEST] = this; }
            if (this.neighbours[GridDirections.WEST] != null) { this.neighbours[GridDirections.WEST].neighbours[GridDirections.EAST] = this; }
        }

        #region INode<Cell> methods

        /// <see cref="INode&lt;Cell&gt;.Distance"/>
        public int Distance(Cell other)
        {
            int horizontalDistance = Math.Abs(other.coords.X - this.coords.X);
            int verticalDistance = Math.Abs(other.coords.Y - this.coords.Y);
            int difference = Math.Abs(horizontalDistance - verticalDistance);
            return Math.Min(horizontalDistance, verticalDistance) * Grid.DIAGONAL_UNIT_DISTANCE + difference * Grid.STRAIGHT_UNIT_DISTANCE;
        }

        /// <see cref="INode&lt;Cell&gt;.GetSuccessors"/>
        public IEnumerable<Cell> GetSuccessors(int objectSize)
        {
            /// TODO: This implementation returns all the walkable neighbours of this cell. Implement jump point search here!
            Cell north = this.neighbours[GridDirections.NORTH];
            if (north != null && objectSize <= north.maxObjectSizeHeap.TopItem) { yield return north; }
            Cell northEast = this.neighbours[GridDirections.NORTH_EAST];
            if (northEast != null && objectSize <= northEast.maxObjectSizeHeap.TopItem) { yield return northEast; }
            Cell east = this.neighbours[GridDirections.EAST];
            if (east != null && objectSize <= east.maxObjectSizeHeap.TopItem) { yield return east; }
            Cell southEast = this.neighbours[GridDirections.SOUTH_EAST];
            if (southEast != null && objectSize <= southEast.maxObjectSizeHeap.TopItem) { yield return southEast; }
            Cell south = this.neighbours[GridDirections.SOUTH];
            if (south != null && objectSize <= south.maxObjectSizeHeap.TopItem) { yield return south; }
            Cell southWest = this.neighbours[GridDirections.SOUTH_WEST];
            if (southWest != null && objectSize <= southWest.maxObjectSizeHeap.TopItem) { yield return southWest; }
            Cell west = this.neighbours[GridDirections.WEST];
            if (west != null && objectSize <= west.maxObjectSizeHeap.TopItem) { yield return west; }
            Cell northWest = this.neighbours[GridDirections.NORTH_WEST];
            if (northWest != null && objectSize <= northWest.maxObjectSizeHeap.TopItem) { yield return northWest; }
        }

        #endregion INode<Cell> methods

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
        /// Gets whether this cell is walkable for the given size of objects.
        /// </summary>
        /// <param name="objectSize">The given size of objects.</param>
        /// <returns>True if this cell is walkable for the given size of objects.</returns>
        public bool IsWalkable(int objectSize)
        {
            return this.maxObjectSizeHeap.TopItem >= objectSize;
        }

        /// <summary>
        /// Adds a size constraint to this cell.
        /// </summary>
        /// <param name="sizeConstraint">The size constraint to add.</param>
        public void AddSizeConstraint(int sizeConstraint)
        {
            this.maxObjectSizeHeap.Insert(sizeConstraint);
        }

        /// <summary>
        /// Removes a size constraint from this cell.
        /// </summary>
        /// <param name="sizeConstraint">The size constraint to remove.</param>
        public void RemoveSizeConstraint(int sizeConstraint)
        {
            this.maxObjectSizeHeap.Delete(sizeConstraint);
        }

        /// <summary>
        /// Adds this cell to the given region.
        /// </summary>
        /// <param name="region">The region to which this cell to add.</param>
        public void AddToRegion(int objectSize, Region region)
        {
            this.regions[objectSize - 1] = region;
        }

        /// <summary>
        /// Gets the coordinates of this cell.
        /// </summary>
        public Point Coords { get { return this.coords; } }

        /// <summary>
        /// Gets the sector that this cell belongs to.
        /// </summary>
        public Sector Sector { get { return this.sector; } }

        /// <summary>
        /// Gets the region that this cell belongs to.
        /// </summary>
        public Region GetRegion(int objectSize) { return this.regions[objectSize - 1]; }

        /// <summary>
        /// Gets the string representation of this cell.
        /// </summary>
        /// <returns>The string representation of this cell.</returns>
        public override string ToString()
        {
            return string.Format("{0} - {1}", this.coords.ToString(), this.maxObjectSizeHeap.TopItem);
        }

        /// <summary>
        /// The top of this heap stores the maximum size of object for which this cell is walkable. 
        /// </summary>
        private readonly MinHeapInt maxObjectSizeHeap;

        /// <summary>
        /// The coordinates of this cell.
        /// </summary>
        private readonly Point coords;

        /// <summary>
        /// Reference to the grid that this cell belongs to.
        /// </summary>
        private readonly Grid grid;

        /// <summary>
        /// Reference to the sector that this cell belongs to.
        /// </summary>
        private readonly Sector sector;

        /// <summary>
        /// Reference to the neighbours of this cell.
        /// </summary>
        private readonly Cell[] neighbours;

        /// <summary>
        /// Reference to the region that this cell belongs to.
        /// </summary>
        private Region[] regions;
    }
}
