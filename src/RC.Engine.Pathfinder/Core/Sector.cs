using RC.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Engine.Pathfinder.Core
{
    /// <summary>
    /// Represents a sector of the pathfinding grid.
    /// </summary>
    class Sector
    {
        /// <summary>
        /// Constructs a sector of the given grid with the given area.
        /// </summary>
        /// <param name="areaOnGrid">The area of this sector on the grid.</param>
        /// <param name="grid">The grid that this sector belongs to.</param>
        public Sector(RCIntRectangle areaOnGrid, Grid grid)
        {
            this.grid = grid;
            this.areaOnGrid = areaOnGrid;
            this.center = new RCIntVector((this.areaOnGrid.Left + this.areaOnGrid.Right) / 2, (this.areaOnGrid.Top + this.areaOnGrid.Bottom) / 2);
            this.subdivisions = new RCSet<SectorSubdivision>();
            this.staticAgents = new RCSet<Agent>[this.grid.MaxMovingSize];
            for (int i = 0; i < this.grid.MaxMovingSize; i++) { this.staticAgents[i] = new RCSet<Agent>(); }

            /// Set the neighbours of this sector.
            RCIntVector northWestNeighbourCellCoords = this.areaOnGrid.Location + GridDirections.DIRECTION_TO_VECTOR[GridDirections.NORTH_WEST];
            RCIntVector northNeighbourCellCoords = this.areaOnGrid.Location + GridDirections.DIRECTION_TO_VECTOR[GridDirections.NORTH];
            RCIntVector northEastNeighbourCellCoords = new RCIntVector(this.areaOnGrid.Right - 1, this.areaOnGrid.Top) + GridDirections.DIRECTION_TO_VECTOR[GridDirections.NORTH_EAST];
            RCIntVector westNeighbourCellCoords = this.areaOnGrid.Location + GridDirections.DIRECTION_TO_VECTOR[GridDirections.WEST];
            Cell northWestNeighbourCell = this.grid[northWestNeighbourCellCoords.X, northWestNeighbourCellCoords.Y];
            Cell northNeighbourCell = this.grid[northNeighbourCellCoords.X, northNeighbourCellCoords.Y];
            Cell northEastNeighbourCell = this.grid[northEastNeighbourCellCoords.X, northEastNeighbourCellCoords.Y];
            Cell westNeighbourCell = this.grid[westNeighbourCellCoords.X, westNeighbourCellCoords.Y];
            this.neighbours = new Sector[GridDirections.DIRECTION_COUNT];
            this.neighbours[GridDirections.NORTH_WEST] = northWestNeighbourCell != null ? northWestNeighbourCell.Sector : null;
            this.neighbours[GridDirections.NORTH] = northNeighbourCell != null ? northNeighbourCell.Sector : null;
            this.neighbours[GridDirections.NORTH_EAST] = northEastNeighbourCell != null ? northEastNeighbourCell.Sector : null;
            this.neighbours[GridDirections.WEST] = westNeighbourCell != null ? westNeighbourCell.Sector : null;

            /// Set this sector as the neighbour of its neighbours.
            if (this.neighbours[GridDirections.NORTH_WEST] != null) { this.neighbours[GridDirections.NORTH_WEST].neighbours[GridDirections.SOUTH_EAST] = this; }
            if (this.neighbours[GridDirections.NORTH] != null) { this.neighbours[GridDirections.NORTH].neighbours[GridDirections.SOUTH] = this; }
            if (this.neighbours[GridDirections.NORTH_EAST] != null) { this.neighbours[GridDirections.NORTH_EAST].neighbours[GridDirections.SOUTH_WEST] = this; }
            if (this.neighbours[GridDirections.WEST] != null) { this.neighbours[GridDirections.WEST].neighbours[GridDirections.EAST] = this; }
        }

        /// <summary>
        /// Gets the neighbour of this sector at the given direction.
        /// </summary>
        /// <param name="direction">The direction of the neighbour to get (see GridDirections class for more information).</param>
        /// <returns>The neighbour of this sector at the given direction or null if this sector has no neighbour at the given direction.</returns>
        public Sector GetNeighbour(int direction)
        {
            return this.neighbours[direction];
        }

        /// <summary>
        /// Adds the given static agent to this sector for moving agents of the given size or bigger.
        /// </summary>
        /// <param name="movingSize">The given moving agent size.</param>
        /// <param name="agent">The agent to be added.</param>
        public void AddStaticAgent(int movingSize, Agent agent)
        {
            for (int size = movingSize; size <= this.grid.MaxMovingSize; size++)
            {
                if (this.staticAgents[size - 1].Add(agent))
                {
                    RCSet<SectorSubdivision> subdivisionsCopy = new RCSet<SectorSubdivision>(this.subdivisions);
                    foreach (SectorSubdivision subdivisionToInvalidate in subdivisionsCopy.Where(subdivision => subdivision.MovingSize == size))
                    {
                        subdivisionToInvalidate.Invalidate();
                        this.subdivisions.Remove(subdivisionToInvalidate);
                    }
                }
            }
        }

        /// <summary>
        /// Removes the given static agent from this sector for all moving agents.
        /// </summary>
        /// <param name="agent">The agent to be removed.</param>
        public void RemoveStaticAgent(Agent agent)
        {
            for (int size = 1; size <= this.grid.MaxMovingSize; size++)
            {
                if (this.staticAgents[size - 1].Remove(agent))
                {
                    RCSet<SectorSubdivision> subdivisionsCopy = new RCSet<SectorSubdivision>(this.subdivisions);
                    foreach (SectorSubdivision subdivisionToInvalidate in subdivisionsCopy.Where(subdivision => subdivision.MovingSize == size))
                    {
                        subdivisionToInvalidate.Invalidate();
                        this.subdivisions.Remove(subdivisionToInvalidate);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the subdivision that is calculated for the given agent.
        /// </summary>
        /// <param name="agent">The given agent.</param>
        /// <returns>The subdivision that is calculated for the given agent.</returns>
        public SectorSubdivision GetSubdivisionForAgent(Agent agent)
        {
            /// Collect the overlap enabled agents.
            RCSet<Agent> overlapEnabledAgents = new RCSet<Agent>();
            RCSet<Agent> currentlyOverlappingAgents = this.grid[agent.Area.X, agent.Area.Y].GetAgents(agent.MovingSize);
            foreach (Agent staticAgent in this.staticAgents[agent.MovingSize - 1])
            {               
                if (currentlyOverlappingAgents.Contains(staticAgent) ||
                    staticAgent.Client.IsOverlapEnabled(agent.Client) ||
                    agent.Client.IsOverlapEnabled(staticAgent.Client))
                {
                    overlapEnabledAgents.Add(staticAgent);
                }
            }

            /// Try to find an already existing sector subdivision for the agent.
            SectorSubdivision subdivisionForAgent =
                this.subdivisions.FirstOrDefault(subdivision => subdivision.MovingSize == agent.MovingSize &&
                                                                subdivision.OverlapEnabledAgents.SetEquals(overlapEnabledAgents));
            if (subdivisionForAgent == null)
            {
                /// Create a new one if not found.
                subdivisionForAgent = new SectorSubdivision(this, agent.MovingSize, overlapEnabledAgents);
                this.subdivisions.Add(subdivisionForAgent);
            }
            return subdivisionForAgent;
        }

        /// <summary>
        /// Gets the area of this sector on the grid.
        /// </summary>
        public RCIntRectangle AreaOnGrid { get { return this.areaOnGrid; } }

        /// <summary>
        /// Gets the coordinates of the center of this sector.
        /// </summary>
        public RCIntVector Center { get { return this.center; } }

        /// <summary>
        /// Gets the grid that this sector belongs to.
        /// </summary>
        public Grid Grid { get { return this.grid; } }

        /// <summary>
        /// Creates the initial subdivisions of this sector.
        /// </summary>
        public void CreateInitialSubdivisions()
        {
            for (int movingSize = 1; movingSize <= this.grid.MaxMovingSize; movingSize++)
            {
                this.subdivisions.Add(new SectorSubdivision(this, movingSize, new RCSet<Agent>()));
            }
        }

        /// <summary>
        /// Gets the string representation of this sector.
        /// </summary>
        /// <returns>The string representation of this sector.</returns>
        public override string ToString()
        {
            return string.Format("Sector {0}", this.areaOnGrid);
        }

        /// <summary>
        /// The grid that this sector belongs to.
        /// </summary>
        private readonly Grid grid;

        /// <summary>
        /// The area of this sector on the grid.
        /// </summary>
        private readonly RCIntRectangle areaOnGrid;

        /// <summary>
        /// The coordinates of the center of this sector.
        /// </summary>
        private readonly RCIntVector center;

        /// <summary>
        /// The calculated subdivisions of this sector.
        /// </summary>
        private readonly RCSet<SectorSubdivision> subdivisions;

        /// <summary>
        /// The Nth element of this array is the set of static agents in this sector for moving agents of size (N+1).
        /// </summary>
        private readonly RCSet<Agent>[] staticAgents;

        /// <summary>
        /// References to the neighbours of this sector.
        /// </summary>
        private readonly Sector[] neighbours;

        /// <summary>
        /// The size of the sectors of a motion control grid.
        /// </summary>
        public const int SECTOR_SIZE = 32;
    }
}
