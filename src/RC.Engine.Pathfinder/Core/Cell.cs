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
        /// <param name="isWalkable">The flag indicating whether this cell is walkable.</param>
        /// <param name="grid">The grid that this cell belongs to.</param>
        /// <param name="sector">The sector that this cell belongs to.</param>
        public Cell(RCIntVector coords, bool isWalkable, Grid grid, Sector sector)
        {
            this.coords = coords;
            this.grid = grid;
            this.sector = sector;
            this.regions = new RCSet<Region>();

            this.wallCellDistance = Math.Min(this.grid.MaxMovingSize, Math.Min(this.grid.Width - coords.X, this.grid.Height - coords.Y));
            this.agents = new RCSet<Agent>[this.grid.MaxMovingSize];
            for (int i = 0; i < this.grid.MaxMovingSize; i++) { this.agents[i] = new RCSet<Agent>(); }

            /// Set the neighbours of this cell.
            RCIntVector northWestNeighbourCoords = this.coords + GridDirections.DIRECTION_TO_VECTOR[GridDirections.NORTH_WEST];
            RCIntVector northNeighbourCoords = this.coords + GridDirections.DIRECTION_TO_VECTOR[GridDirections.NORTH];
            RCIntVector northEastNeighbourCoords = this.coords + GridDirections.DIRECTION_TO_VECTOR[GridDirections.NORTH_EAST];
            RCIntVector westNeighbourCoords = this.coords + GridDirections.DIRECTION_TO_VECTOR[GridDirections.WEST];
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

            /// If this is a non-walkable cell -> set the wall cell-distances of its environment.
            if (!isWalkable)
            {
                for (int envCol = -this.grid.MaxMovingSize + 1; envCol <= 0; envCol++)
                {
                    for (int envRow = -this.grid.MaxMovingSize + 1; envRow <= 0; envRow++)
                    {
                        int absCol = coords.X + envCol;
                        int absRow = coords.Y + envRow;
                        Cell cellToSet = this.coords == new RCIntVector(absCol, absRow) ? this : this.grid[absCol, absRow];
                        if (cellToSet != null)
                        {
                            cellToSet.wallCellDistance = Math.Min(cellToSet.wallCellDistance, this.grid.ObstacleEnvironment[envCol, envRow]);
                        }
                    }
                }
            }
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
        /// Gets the set of agents in this cell for moving agents of the given size.
        /// </summary>
        /// <param name="movingSize">The given moving agent size.</param>
        /// <returns>The set of agents in this cell for moving agents of the given size.</returns>
        public RCSet<Agent> GetAgents(int movingSize)
        {
            return this.agents[movingSize - 1];
        }

        /// <summary>
        /// Adds the given agent to this cell for moving agents of the given size or bigger.
        /// </summary>
        /// <param name="movingSize">The given moving agent size.</param>
        /// <param name="agent">The agent to be added.</param>
        public void AddAgent(int movingSize, Agent agent)
        {
            for (int size = movingSize; size <= this.grid.MaxMovingSize; size++)
            {
                this.agents[size - 1].Add(agent);
            }
        }

        /// <summary>
        /// Removes the given agent from this cell for all moving agents.
        /// </summary>
        /// <param name="agent">The agent to be removed.</param>
        public void RemoveAgent(Agent agent)
        {
            for (int size = 1; size <= this.grid.MaxMovingSize; size++)
            {
                this.agents[size - 1].Remove(agent);
            }
        }

        /// <summary>
        /// Adds this cell to the given region.
        /// </summary>
        /// <param name="region">The region</param>
        public void AddToRegion(Region region)
        {
            this.regions.Add(region);
        }

        /// <summary>
        /// Removes this cell from the region that belongs to the given subdivision.
        /// </summary>
        /// <param name="subdivision">The given subdivision.</param>
        public void RemoveFromRegion(SectorSubdivision subdivision)
        {
            Region regionToRemove = this.regions.FirstOrDefault(region => region.Subdivision == subdivision);
            if (regionToRemove != null) { this.regions.Remove(regionToRemove); }
        }

        /// <summary>
        /// Gets the region of the given subdivision that this cell belongs to or null if no such region found.
        /// </summary>
        /// <param name="subdivision">The given subdivision.</param>
        public Region GetRegion(SectorSubdivision subdivision)
        {
            return this.regions.FirstOrDefault(region => region.Subdivision == subdivision);
        }

        /// <summary>
        /// Gets the coordinates of this cell on the grid-layer.
        /// </summary>
        public RCIntVector Coords { get { return this.coords; } }

        /// <summary>
        /// Gets the sector that this cell belongs to.
        /// </summary>
        public Sector Sector { get { return this.sector; } }

        /// <summary>
        /// Gets the cell-distance of the nearest wall from this cell.
        /// </summary>
        internal int WallCellDistance { get { return this.wallCellDistance; } }

        /// <summary>
        /// The cell-distance of the nearest wall from this cell.
        /// </summary>
        private int wallCellDistance;

        /// <summary>
        /// The Nth element of this array is the set of agents in this cell for moving agents of size (N+1).
        /// </summary>
        private RCSet<Agent>[] agents;

        /// <summary>
        /// References to the neighbours of this cell.
        /// </summary>
        private readonly Cell[] neighbours;

        /// <summary>
        /// The coordinates of this cell on the grid-layer.
        /// </summary>
        private readonly RCIntVector coords;

        /// <summary>
        /// The grid that this cell belongs to.
        /// </summary>
        private readonly Grid grid;

        /// <summary>
        /// The sector that this cell belongs to.
        /// </summary>
        private readonly Sector sector;

        /// <summary>
        /// Reference to the valid regions that this cell belongs to.
        /// </summary>
        private readonly RCSet<Region> regions;
    }
}
