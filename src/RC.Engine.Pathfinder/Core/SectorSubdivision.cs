using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Pathfinder.Core
{
    /// <summary>
    /// Represents the subdivision of a sector.
    /// </summary>
    class SectorSubdivision
    {
        /// <summary>
        /// Constructs the subdivision of the given sector for the given moving size and the given set of overlap enabled agents.
        /// </summary>
        /// <param name="sector">The sector that this subdivision belongs to.</param>
        /// <param name="movingSize">The given moving size.</param>
        /// <param name="overlapEnabledAgents">The given set of overlap enabled agents.</param>
        public SectorSubdivision(Sector sector, int movingSize, RCSet<Agent> overlapEnabledAgents)
        {
            this.sector = sector;
            this.movingSize = movingSize;
            this.isValid = true;
            this.overlapEnabledAgents = new RCSet<Agent>(overlapEnabledAgents);
            this.regions = new RCSet<Region>();

            /// Execute a flood-fill algorithm to collect the regions of the given sector.
            Region[,] regionArray = new Region[this.sector.AreaOnGrid.Width, this.sector.AreaOnGrid.Height];
            for (int row = 0; row < this.sector.AreaOnGrid.Height; row++)
            {
                for (int column = 0; column < this.sector.AreaOnGrid.Width; column++)
                {
                    RCIntVector currentCoords = new RCIntVector(column, row);
                    RCIntVector currentAbsCoords = new RCIntVector(this.sector.AreaOnGrid.Left + column, this.sector.AreaOnGrid.Top + row);

                    /// Get the current cell.
                    Cell currentCell = this.sector.Grid[currentAbsCoords.X, currentAbsCoords.Y];
                    if (currentCell == null || !this.IsCellWalkable(currentCell)) { continue; }

                    /// Join the regions of the neighbours.
                    Region regionToSet = null;
                    for (int directionIdx = 0; directionIdx < DIRECTIONS_TO_CHECK.Length; directionIdx++)
                    {
                        int direction = DIRECTIONS_TO_CHECK[directionIdx];
                        Cell neighbourCell = currentCell.GetNeighbour(direction);
                        if (neighbourCell != null && neighbourCell.Sector == this.sector && this.IsCellWalkable(neighbourCell))
                        {
                            RCIntVector neighbourCoords = currentCoords + GridDirections.DIRECTION_TO_VECTOR[direction];
                            if (regionToSet == null) { regionToSet = regionArray[neighbourCoords.X, neighbourCoords.Y]; }
                            else { regionToSet = regionToSet.Join(regionArray[neighbourCoords.X, neighbourCoords.Y]); }
                        }
                    }

                    /// Set the cell to the found region or create one if not found.
                    regionArray[currentCoords.X, currentCoords.Y] = regionToSet != null ? regionToSet : new Region(this);
                    regionArray[currentCoords.X, currentCoords.Y].RegisterCellIfExit(currentCell);
                }
            }

            /// Set the calculated root regions to the appropriate cells with a second pass.
            for (int row = 0; row < this.sector.AreaOnGrid.Height; row++)
            {
                for (int column = 0; column < this.sector.AreaOnGrid.Width; column++)
                {
                    RCIntVector currentAbsCoords = new RCIntVector(this.sector.AreaOnGrid.Left + column, this.sector.AreaOnGrid.Top + row);
                    Cell currentCell = this.sector.Grid[currentAbsCoords.X, currentAbsCoords.Y];
                    if (regionArray[column, row] != null)
                    {
                        Region regionOfCell = regionArray[column, row].Root;
                        this.regions.Add(regionOfCell);
                        currentCell.AddToRegion(regionOfCell);
                    }
                }
            }
        }

        /// <summary>
        /// Invalidates this sector subdivision. If this sector subdivision is currently invalid then this function has no effect.
        /// </summary>
        public void Invalidate()
        {
            if (!this.isValid) { return; }

            /// Remove the calculated regions from the appropriate cells.
            for (int row = 0; row < this.sector.AreaOnGrid.Height; row++)
            {
                for (int column = 0; column < this.sector.AreaOnGrid.Width; column++)
                {
                    RCIntVector currentAbsCoords = new RCIntVector(this.sector.AreaOnGrid.Left + column, this.sector.AreaOnGrid.Top + row);
                    Cell currentCell = this.sector.Grid[currentAbsCoords.X, currentAbsCoords.Y];
                    if (currentCell != null)
                    {
                        currentCell.RemoveFromRegion(this);
                    }
                }
            }
            this.isValid = false;
        }

        /// <summary>
        /// Checks whether the given cell is walkable from the point of view of this subdivision.
        /// </summary>
        /// <param name="cell">The checked cell.</param>
        /// <returns>True if the given cell is walkable from the point of view of this subdivision; otherwise false.</returns>
        public bool IsCellWalkable(Cell cell)
        {
            if (cell.WallCellDistance < this.movingSize) { return false; }

            RCSet<Agent> agentsAtCell = cell.GetAgents(this.movingSize);
            foreach (Agent staticAgent in agentsAtCell.Where(agent => agent.MovingStatus == AgentMovingStatusEnum.Static))
            {
                if (!this.overlapEnabledAgents.Contains(staticAgent)) { return false; }
            }
            return true;
        }

        /// <summary>
        /// Gets the size of moving agents for which this sector subdivision is calculated.
        /// </summary>
        public int MovingSize { get { return this.movingSize; } }

        /// <summary>
        /// Gets the set of overlap enabled agents of the agents for which this sector subdivision is calculated.
        /// </summary>
        public RCSet<Agent> OverlapEnabledAgents { get { return this.overlapEnabledAgents; } }

        /// <summary>
        /// Gets the sector that this subdivision belongs to.
        /// </summary>
        public Sector Sector { get { return this.sector; } }

        /// <summary>
        /// Gets whether this sector subdivision is still valid.
        /// </summary>
        public bool IsValid { get { return this.isValid; } }

        /// <summary>
        /// The size of moving agents for which this sector subdivision is calculated.
        /// </summary>
        private readonly int movingSize;

        /// <summary>
        /// The set of overlap enabled agents of the agents for which this sector subdivision is calculated.
        /// </summary>
        private readonly RCSet<Agent> overlapEnabledAgents;

        /// <summary>
        /// The list of regions in this subdivision.
        /// </summary>
        private readonly RCSet<Region> regions;

        /// <summary>
        /// The sector that this subdivision belongs to.
        /// </summary>
        private readonly Sector sector;

        /// <summary>
        /// This flag indicates whether this sector subdivision is still valid or not.
        /// </summary>
        private bool isValid;

        /// <summary>
        /// The indices of the neighbours to check during the region calculations.
        /// </summary>
        private static readonly int[] DIRECTIONS_TO_CHECK = new int[]
        {
            GridDirections.NORTH_WEST, GridDirections.NORTH, GridDirections.NORTH_EAST, GridDirections.WEST
        };
    }
}
