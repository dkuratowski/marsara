using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Prototypes.NewPathfinding.MotionControl
{
    /// <summary>
    /// Represents a region-label.
    /// </summary>
    class Label
    {
        /// <summary>
        /// Constructs a Label instance.
        /// </summary>
        public Label(int objectSize, Sector sector)
        {
            this.parent = this;
            this.sector = sector;
            this.rank = 0;
            this.objectSize = objectSize;
            this.exitCells = new HashSet<Cell>[GridDirections.DIRECTION_COUNT];
            for (int direction = 0; direction < GridDirections.DIRECTION_COUNT; direction++)
            {
                this.exitCells[direction] = new HashSet<Cell>();
            }
            this.allCells = new HashSet<Cell>();
        }

        /// <summary>
        /// Joins this label with the other and returns the result label.
        /// </summary>
        /// <param name="other">The other label.</param>
        /// <returns>The result label.</returns>
        public Label Join(Label other)
        {
            Label rootOfThis = this.Root;
            Label rootOfOther = other.Root;
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
        /// Gets the region that belongs to this label.
        /// </summary>
        public Region GetRegion()
        {
            if (this.region != null) { return this.region; }
            Label rootOfThis = this.Root;
            if (rootOfThis.region == null) { rootOfThis.region = new Region(this.objectSize, this.sector, rootOfThis.exitCells, rootOfThis.allCells); }
            if (this.region == null) { this.region = rootOfThis.region; }
            return this.region;
        }

        /// <summary>
        /// Registers the given cell if it is an exit cell.
        /// </summary>
        /// <param name="cell">The cell to register.</param>
        public void RegisterCellIfExit(Cell cell)
        {
            Label rootOfThis = this.Root;
            rootOfThis.allCells.Add(cell);
            for (int direction = 0; direction < GridDirections.DIRECTION_COUNT; direction++)
            {
                Cell neighbourCell = cell.GetNeighbour(direction);
                if (neighbourCell != null && neighbourCell.IsWalkable(rootOfThis.objectSize) && neighbourCell.Sector != rootOfThis.sector)
                {
                    /// Neighbour cell is an exit to the current direction.
                    rootOfThis.exitCells[direction].Add(cell);
                }
            }
        }

        /// <summary>
        /// Gets the root of this label.
        /// </summary>
        public Label Root { get { return this.parent != this ? this.parent.Root : this; } }

        /// <summary>
        /// The parent of this label.
        /// </summary>
        private Label parent;

        /// <summary>
        /// The region created by this label if finalized; otherwise null.
        /// </summary>
        private Region region;

        /// <summary>
        /// Reference to the sector that this label belongs to.
        /// </summary>
        private Sector sector;

        /// <summary>
        /// The exit cells found by this label for each directions.
        /// </summary>
        private HashSet<Cell>[] exitCells;

        /// <summary>
        /// All cells found by this label.
        /// </summary>
        private HashSet<Cell> allCells;

        /// <summary>
        /// The rank of this label if this is a root.
        /// </summary>
        private int rank;

        /// <summary>
        /// The maximum size of objects for which this label is created.
        /// </summary>
        private int objectSize;
    }
}
