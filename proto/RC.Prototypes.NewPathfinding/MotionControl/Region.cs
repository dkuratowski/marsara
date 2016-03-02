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
    class Region : INode<Region>
    {
        /// <summary>
        /// Constructs a region instance.
        /// </summary>
        /// <param name="objectSize">The maximum size of objects that can use this region.</param>
        /// <param name="sector">The sector that this region belongs to.</param>
        /// <param name="exitCells">The exit cells of this region for each directions.</param>
        public Region(int objectSize, Sector sector, HashSet<Cell>[] exitCells)
        {
            this.objectSize = objectSize;
            this.sector = sector;
            this.isUpToDate = true;
            this.exitsToNeighbours = null;
            this.exitCells = exitCells;
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

        #region INode<Region> methods

        /// <see cref="INode&lt;Region&gt;.Distance"/>
        public int Distance(Region other)
        {
            int horizontalDistance = Math.Abs(other.sector.Center.X - this.sector.Center.X);
            int verticalDistance = Math.Abs(other.sector.Center.Y - this.sector.Center.Y);
            int difference = Math.Abs(horizontalDistance - verticalDistance);
            return Math.Min(horizontalDistance, verticalDistance) * Grid.DIAGONAL_UNIT_DISTANCE + difference * Grid.STRAIGHT_UNIT_DISTANCE;
        }

        /// <see cref="INode&lt;Region&gt;.GetSuccessors"/>
        public IEnumerable<Region> GetSuccessors(int objectSize)
        {
            if (this.exitsToNeighbours == null)
            {
                if (!this.isUpToDate) { throw new InvalidOperationException("Unable to calculate the successors of an out-of-date region!"); }

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

        #endregion INode<Cell> methods

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

        /// <summary>
        /// Stores the exit cells of this region for each of its neighbours.
        /// </summary>
        private readonly Dictionary<Region, HashSet<Cell>> exitsToNeighbours;

        /// <summary>
        /// This flag indicates whether this region is up-to-date.
        /// </summary>
        private bool isUpToDate;
    }
}
