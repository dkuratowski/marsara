using RC.Common;
using RC.Common.Diagnostics;
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
            this.edgeCells = new RCSet<Cell>[GridDirections.DIRECTION_COUNT];
            for (int dir = 0; dir < GridDirections.DIRECTION_COUNT; dir++)
            {
                this.edgeCells[dir] = new RCSet<Cell>();
            }
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
        /// Gets the transition cells of this region into the given other region to the given direction.
        /// </summary>
        /// <param name="targetRegion">The given target region.</param>
        /// <param name="direction">The given direction.</param>
        /// <returns>The transition cells of this region into the given other region to the given direction.</returns>
        public RCSet<Cell> GetTransitionCells(Region targetRegion, int direction)
        {
            int oppositeDir = (direction + GridDirections.DIRECTION_COUNT / 2) % GridDirections.DIRECTION_COUNT;
            Region rootOfThis = this.Root;
            Region rootOfTarget = targetRegion.Root;
            if (rootOfThis.edgeCells[direction].Count == 0) { return new RCSet<Cell>(); }
            if (rootOfTarget.edgeCells[oppositeDir].Count == 0) { return new RCSet<Cell>(); }

            RCSet<Cell> transitionCells = new RCSet<Cell>();
            foreach (Cell edgeCell in rootOfThis.edgeCells[direction])
            {
                Cell straightNeighbour = edgeCell.GetNeighbour(direction);
                if (straightNeighbour == null) { continue; }

                if (rootOfTarget.edgeCells[oppositeDir].Contains(straightNeighbour))
                {
                    transitionCells.Add(edgeCell);
                    continue;
                }

                if (direction == GridDirections.NORTH || direction == GridDirections.EAST || direction == GridDirections.SOUTH || direction == GridDirections.WEST)
                {
                    Cell leftStraightNeighbour = edgeCell.GetNeighbour((direction + GridDirections.DIRECTION_COUNT - 1) % GridDirections.DIRECTION_COUNT);
                    if (leftStraightNeighbour == null) { continue; }

                    if (rootOfTarget.edgeCells[oppositeDir].Contains(leftStraightNeighbour))
                    {
                        transitionCells.Add(edgeCell);
                        continue;
                    }

                    Cell rightStraightNeighbour = edgeCell.GetNeighbour((direction + 1) % GridDirections.DIRECTION_COUNT);
                    if (rightStraightNeighbour == null) { continue; }

                    if (rootOfTarget.edgeCells[oppositeDir].Contains(rightStraightNeighbour))
                    {
                        transitionCells.Add(edgeCell);
                        continue;
                    }
                }
            }
            return transitionCells;
        }

        ///// <summary>
        ///// Gets the neighbours of this region calculated for the given agent.
        ///// </summary>
        ///// <param name="agent">The given agent.</param>
        ///// <returns>The neighbours of this region.</returns>
        //public IEnumerable<Region> GetNeighbours(Agent agent)
        //{
        //    if (!this.IsValid) { throw new InvalidOperationException("Unable to get the neighbours of an out-of-date region!"); }

        //    if (this.exitsToNeighbours == null)
        //    {
        //        this.exitsToNeighbours = new Dictionary<Region, RCSet<Cell>>();
        //        for (int direction = 0; direction < exitCellsToOtherSector.Length; direction++)
        //        {
        //            foreach (Cell exitCell in this.exitCellsToOtherSector[direction])
        //            {
        //                Cell neighbourCell = exitCell.GetNeighbour(direction);
        //                if (neighbourCell != null && this.subdivision.IsCellWalkable(neighbourCell) && neighbourCell.Sector != this.subdivision.Sector)
        //                {
        //                    SectorSubdivision subdivision = neighbourCell.Sector.GetSubdivisionForAgent(agent);
        //                    Region neighbourRegion = neighbourCell.GetRegion(subdivision);
        //                    if (!this.exitsToNeighbours.ContainsKey(neighbourRegion))
        //                    {
        //                        this.exitsToNeighbours[neighbourRegion] = new RCSet<Cell>();
        //                    }
        //                    this.exitsToNeighbours[neighbourRegion].Add(exitCell);
        //                }
        //            }
        //        }
        //    }
        //    return this.exitsToNeighbours.Keys;
        //}

        ///// <summary>
        ///// Gets the exit cells to the given neighbour region.
        ///// </summary>
        ///// <param name="neighbour">The neighbour region.</param>
        ///// <returns>The exit cells to the given neighbour region.</returns>
        //public RCSet<Cell> GetExistsToNeighbours(Region neighbour)
        //{
        //    return this.exitsToNeighbours.ContainsKey(neighbour) ? this.exitsToNeighbours[neighbour] : new RCSet<Cell>();
        //}

        /// <summary>
        /// Adds the given cell as an edge cell to this region. If the given cell is not an edge cell then this function has no effect.
        /// </summary>
        /// <param name="cell">The cell to add.</param>
        public void AddEdgeCell(Cell cell)
        {
            Region rootOfThis = this.Root;
            bool north = cell.Coords.Y == rootOfThis.subdivision.Sector.AreaOnGrid.Top;
            bool east = cell.Coords.X == rootOfThis.subdivision.Sector.AreaOnGrid.Right - 1;
            bool south = cell.Coords.Y == rootOfThis.subdivision.Sector.AreaOnGrid.Bottom - 1;
            bool west = cell.Coords.X == rootOfThis.subdivision.Sector.AreaOnGrid.Left;
            if (north) { rootOfThis.edgeCells[GridDirections.NORTH].Add(cell); }
            if (east) { rootOfThis.edgeCells[GridDirections.EAST].Add(cell); }
            if (south) { rootOfThis.edgeCells[GridDirections.SOUTH].Add(cell); }
            if (west) { rootOfThis.edgeCells[GridDirections.WEST].Add(cell); }
            if (north && east) { rootOfThis.edgeCells[GridDirections.NORTH_EAST].Add(cell); }
            if (south && east) { rootOfThis.edgeCells[GridDirections.SOUTH_EAST].Add(cell); }
            if (south && west) { rootOfThis.edgeCells[GridDirections.SOUTH_WEST].Add(cell); }
            if (north && west) { rootOfThis.edgeCells[GridDirections.NORTH_WEST].Add(cell); }
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
                    rootOfOther.edgeCells[direction].UnionWith(rootOfThis.edgeCells[direction]);
                }
                return rootOfOther;
            }
            else
            {
                rootOfOther.parent = rootOfThis;
                for (int direction = 0; direction < GridDirections.DIRECTION_COUNT; direction++)
                {
                    rootOfThis.edgeCells[direction].UnionWith(rootOfOther.edgeCells[direction]);
                }
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
        /// The cells that are on the edge of the sector that this region belongs to.
        /// </summary>
        private readonly RCSet<Cell>[] edgeCells;
    }
}
