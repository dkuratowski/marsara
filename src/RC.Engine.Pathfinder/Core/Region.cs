using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Engine.Pathfinder.Core
{
    /// <summary>
    /// Represents a region of a sector on a motion control grid.
    /// </summary>
    class Region
    {
        /// <summary>
        /// Constructs a region instance.
        /// </summary>
        /// <param name="subdivision">The sector subdivision that this region belongs to.</param>
        public Region(SectorSubdivision subdivision)
        {
            this.parent = this;
            this.rank = 0;
            this.subdivision = subdivision;
            this.exitsToNeighbours = null;
            this.exitCells = new RCSet<Cell>[GridDirections.DIRECTION_COUNT];
            for (int dir = 0; dir < GridDirections.DIRECTION_COUNT; dir++)
            {
                this.exitCells[dir] = new RCSet<Cell>();
            }
            this.allCells = new RCSet<Cell>();
        }

        /// <summary>
        /// Gets whether this region is still valid.
        /// </summary>
        public bool IsValid { get { return this.subdivision.IsValid; } }

        /// <summary>
        /// Gets the sector subdivision that this region belongs to.
        /// </summary>
        public SectorSubdivision Subdivision { get { return this.subdivision; } }

        /// <summary>
        /// Gets the neighbours of this region calculated for the given agent.
        /// </summary>
        /// <param name="agent">The given agent.</param>
        /// <returns>The neighbours of this region.</returns>
        public IEnumerable<Region> GetNeighbours(Agent agent)
        {
            if (!this.IsValid) { throw new InvalidOperationException("Unable to get the neighbours of an out-of-date region!"); }

            if (this.exitsToNeighbours == null)
            {
                this.exitsToNeighbours = new Dictionary<Region, RCSet<Cell>>();
                for (int direction = 0; direction < exitCells.Length; direction++)
                {
                    foreach (Cell exitCell in this.exitCells[direction])
                    {
                        Cell neighbourCell = exitCell.GetNeighbour(direction);
                        if (neighbourCell != null && this.subdivision.IsCellWalkable(neighbourCell) && neighbourCell.Sector != this.subdivision.Sector)
                        {
                            SectorSubdivision subdivision = neighbourCell.Sector.GetSubdivisionForAgent(agent);
                            Region neighbourRegion = neighbourCell.GetRegion(subdivision);
                            if (!this.exitsToNeighbours.ContainsKey(neighbourRegion))
                            {
                                this.exitsToNeighbours[neighbourRegion] = new RCSet<Cell>();
                            }
                            this.exitsToNeighbours[neighbourRegion].Add(exitCell);
                        }
                    }
                }
            }
            return this.exitsToNeighbours.Keys;
        }

        /// <summary>
        /// Gets the exit cells to the given neighbour region.
        /// </summary>
        /// <param name="neighbour">The neighbour region.</param>
        /// <returns>The exit cells to the given neighbour region.</returns>
        public RCSet<Cell> GetExistsToNeighbours(Region neighbour)
        {
            return this.exitsToNeighbours.ContainsKey(neighbour) ? this.exitsToNeighbours[neighbour] : new RCSet<Cell>();
        }

        /// <summary>
        /// Registers the given cell if it is an exit cell of this region.
        /// </summary>
        /// <param name="cell">The cell to register.</param>
        public void RegisterCellIfExit(Cell cell)
        {
            Region rootOfThis = this.Root;
            rootOfThis.allCells.Add(cell);
            for (int direction = 0; direction < GridDirections.DIRECTION_COUNT; direction++)
            {
                Cell neighbourCell = cell.GetNeighbour(direction);
                if (neighbourCell != null && this.subdivision.IsCellWalkable(neighbourCell) && neighbourCell.Sector != rootOfThis.subdivision.Sector)
                {
                    /// Neighbour cell is an exit to the current direction.
                    rootOfThis.exitCells[direction].Add(cell);
                }
            }
        }

        /// <summary>
        /// Joins this region with the other and returns the result region.
        /// </summary>
        /// <param name="other">The other region.</param>
        /// <returns>The result region.</returns>
        public Region Join(Region other)
        {
            Region rootOfThis = this.Root;
            Region rootOfOther = other.Root;
            if (rootOfThis == rootOfOther) { return rootOfThis; }

            if (rootOfThis.rank < rootOfOther.rank)
            {
                rootOfThis.parent = rootOfOther;
                for (int direction = 0; direction < GridDirections.DIRECTION_COUNT; direction++)
                {
                    rootOfOther.exitCells[direction].UnionWith(rootOfThis.exitCells[direction]);
                }
                rootOfOther.allCells.UnionWith(rootOfThis.allCells);
                return rootOfOther;
            }
            else
            {
                rootOfOther.parent = rootOfThis;
                for (int direction = 0; direction < GridDirections.DIRECTION_COUNT; direction++)
                {
                    rootOfThis.exitCells[direction].UnionWith(rootOfOther.exitCells[direction]);
                }
                rootOfThis.allCells.UnionWith(rootOfOther.allCells);
                if (rootOfThis.rank == rootOfOther.rank) { rootOfThis.rank++; }
                return rootOfThis;
            }
        }

        /// <summary>
        /// Gets the root of this region.
        /// </summary>
        public Region Root { get { return this.parent != this ? this.parent.Root : this; } }

        /// <summary>
        /// The sector subdivision that this region belongs to.
        /// </summary>
        private readonly SectorSubdivision subdivision;

        /// <summary>
        /// The parent of this region.
        /// </summary>
        private Region parent;

        /// <summary>
        /// The rank of this region if this is a root.
        /// </summary>
        private int rank;

        /// <summary>
        /// The exit cells of this region for each directions.
        /// </summary>
        private readonly RCSet<Cell>[] exitCells;
        public RCSet<Cell>[] ExitCells { get { return this.exitCells; } } /// TODO: this is for debugging!

        /// TODO: this is for debugging!
        private readonly RCSet<Cell> allCells;
        public RCSet<Cell> AllCells { get { return this.allCells; } }

        /// <summary>
        /// Stores the exit cells of this region for each of its neighbours.
        /// </summary>
        private Dictionary<Region, RCSet<Cell>> exitsToNeighbours;
    }
}
