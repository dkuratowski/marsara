using RC.Prototypes.NewPathfinding.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Prototypes.NewPathfinding.MotionControl
{
    /// <summary>
    /// Represents a region of a sector on a motion control grid.
    /// </summary>
    class Region
    {
        /// <summary>
        /// Constructs a region instance.
        /// </summary>
        /// <param name="objectSize">The maximum size of objects that can use this region.</param>
        /// <param name="sector">The sector that this region belongs to.</param>
        /// <param name="exitCells">The exit cells of this region for each directions.</param>
        public Region(int objectSize, Sector sector, HashSet<Cell>[] exitCells, HashSet<Cell> allCells)
        {
            this.objectSize = objectSize;
            this.sector = sector;
            this.isUpToDate = true;
            this.exitsToNeighbours = null;
            this.exitCells = exitCells;
            this.allCells = allCells;
        }

        /// <summary>
        /// Invalidates this region.
        /// </summary>
        public void Invalidate()
        {
            this.isUpToDate = false;
        }

        /// <summary>
        /// Gets whether this region is up-to-date.
        /// </summary>
        public bool IsUpToDate { get { return this.isUpToDate; } }

        /// <summary>
        /// Gets the sector that this region belongs to.
        /// </summary>
        public Sector Sector { get { return this.sector; } }

        /// <summary>
        /// Gets the maximum size of objects that can use this region.
        /// </summary>
        public int ObjectSize { get { return this.objectSize; } }

        /// <summary>
        /// Gets the neighbours of this region.
        /// </summary>
        /// <returns>The neighbours of this region.</returns>
        public IEnumerable<Region> GetNeighbours()
        {
            if (!this.isUpToDate) { throw new InvalidOperationException("Unable to get the neighbours of an out-of-date region!"); }

            if (this.exitsToNeighbours == null)
            {
                this.exitsToNeighbours = new Dictionary<Region, HashSet<Cell>>();
                for (int direction = 0; direction < exitCells.Length; direction++)
                {
                    foreach (Cell exitCell in this.exitCells[direction])
                    {
                        Cell neighbourCell = exitCell.GetNeighbour(direction);
                        if (neighbourCell != null && neighbourCell.IsWalkable(this.objectSize) && neighbourCell.Sector != this.sector)
                        {
                            neighbourCell.Sector.CalculateRegions(this.objectSize);
                            Region neighbourRegion = neighbourCell.GetRegion(this.objectSize);
                            if (!this.exitsToNeighbours.ContainsKey(neighbourRegion))
                            {
                                this.exitsToNeighbours[neighbourRegion] = new HashSet<Cell>();
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
        public HashSet<Cell> GetExistsToNeighbours(Region neighbour)
        {
            return this.exitsToNeighbours.ContainsKey(neighbour) ? this.exitsToNeighbours[neighbour] : new HashSet<Cell>();
        }

        /// <summary>
        /// The maximum size of objects that can use this region.
        /// </summary>
        private readonly int objectSize;

        /// <summary>
        /// The sector that this region belongs to.
        /// </summary>
        private readonly Sector sector;

        /// <summary>
        /// The exit cells of this region for each directions.
        /// </summary>
        private readonly HashSet<Cell>[] exitCells;
        public HashSet<Cell>[] ExitCells { get { return this.exitCells; } } /// TODO: this is for debugging!

        /// TODO: this is for debugging!
        private readonly HashSet<Cell> allCells;
        public HashSet<Cell> AllCells { get { return this.allCells; } }

        /// <summary>
        /// Stores the exit cells of this region for each of its neighbours.
        /// </summary>
        private Dictionary<Region, HashSet<Cell>> exitsToNeighbours;

        /// <summary>
        /// This flag indicates whether this region is up-to-date.
        /// </summary>
        private bool isUpToDate;
    }
}
