using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Prototypes.NewPathfinding.MotionControl
{
    /// <summary>
    /// Represents a sector of a motion control grid.
    /// </summary>
    class Sector
    {
        /// <summary>
        /// Constructs a sector of the given grid with the given area.
        /// </summary>
        /// <param name="areaOnGrid">The area of this sector on the grid.</param>
        /// <param name="grid">The grid that this sector belongs to.</param>
        public Sector(Rectangle areaOnGrid, Grid grid)
        {
            this.grid = grid;
            this.areaOnGrid = areaOnGrid;
            this.center = new Point((this.areaOnGrid.Left + this.areaOnGrid.Right) / 2, (this.areaOnGrid.Top + this.areaOnGrid.Bottom) / 2);
            this.isUpToDate = new bool[Grid.MAX_OBJECT_SIZE];
            this.currentRegions = new HashSet<Region>[Grid.MAX_OBJECT_SIZE];
            for (int n = 1; n <= Grid.MAX_OBJECT_SIZE; n++)
            {
                this.currentRegions[n - 1] = new HashSet<Region>();
            }
        }

        /// <summary>
        /// Gets the area of this sector on the grid.
        /// </summary>
        public Rectangle AreaOnGrid { get { return this.areaOnGrid; } }

        /// <summary>
        /// Gets the coordinates of the center of this sector.
        /// </summary>
        public Point Center { get { return this.center; } }

        /// <summary>
        /// Invalidates the regions of this sector.
        /// </summary>
        public void Invalidate()
        {
            for (int n = 1; n <= Grid.MAX_OBJECT_SIZE; n++)
            {
                foreach (Region region in this.currentRegions[n - 1])
                {
                    region.Invalidate();
                }
                this.currentRegions[n - 1].Clear();
                this.isUpToDate[n - 1] = false;
            }
        }

        /// <summary>
        /// Calculates the N-size regions of this sector if the sector is not up-to-date for N. Otherwise this function has no effect.
        /// </summary>
        /// <param name="objectSize">The given N as an object size.</param>
        public void CalculateRegions(int objectSize)
        {
            /// Do nothing if this sector is up-to-date for N.
            if (this.isUpToDate[objectSize - 1]) { return; }

            Stopwatch watch = new Stopwatch();
            watch.Start();

            /// Execute a flood-fill algorithm to collect the N-size regions of this sector.
            Label[,] labels = new Label[this.areaOnGrid.Width, this.areaOnGrid.Height];
            for (int row = 0; row < this.areaOnGrid.Height; row++)
            {
                for (int column = 0; column < this.areaOnGrid.Width; column++)
                {
                    Point currentCoords = new Point(column, row);
                    Point currentAbsCoords = new Point(this.areaOnGrid.Left + column, this.areaOnGrid.Top + row);

                    /// Get the current cell.
                    Cell currentCell = this.grid[currentAbsCoords.X, currentAbsCoords.Y];
                    if (currentCell == null || !currentCell.IsWalkable(objectSize)) { continue; }
                    
                    /// Join the labels of the neighbours.
                    Label labelToSet = null;
                    for (int directionIdx = 0; directionIdx < DIRECTIONS_TO_CHECK.Length; directionIdx++)
                    {
                        int direction = DIRECTIONS_TO_CHECK[directionIdx];
                        Cell neighbourCell = currentCell.GetNeighbour(direction);
                        if (neighbourCell != null && neighbourCell.Sector == this && neighbourCell.IsWalkable(objectSize))
                        {
                            Point neighbourCoords = currentCoords + GridDirections.DIRECTION_VECTOR[direction];
                            if (labelToSet == null) { labelToSet = labels[neighbourCoords.X, neighbourCoords.Y]; }
                            else { labelToSet = labelToSet.Join(labels[neighbourCoords.X, neighbourCoords.Y]); }
                        }
                    }

                    /// Set the cell to the found label or create one if not found.
                    labels[currentCoords.X, currentCoords.Y] = labelToSet != null ? labelToSet : new Label(objectSize, this);
                    labels[currentCoords.X, currentCoords.Y].RegisterCellIfExit(currentCell);
                }
            }

            /// Set the calculated regions to the appropriate cells with a second pass.
            for (int row = 0; row < this.areaOnGrid.Height; row++)
            {
                for (int column = 0; column < this.areaOnGrid.Width; column++)
                {
                    Point currentAbsCoords = new Point(this.areaOnGrid.Left + column, this.areaOnGrid.Top + row);
                    Cell currentCell = this.grid[currentAbsCoords.X, currentAbsCoords.Y];
                    if (labels[column, row] != null)
                    {
                        Region regionOfCell = labels[column, row].GetRegion();
                        this.currentRegions[objectSize - 1].Add(regionOfCell);
                        currentCell.AddToRegion(objectSize, regionOfCell);
                    }
                }
            }

            /// This sector is now up-to-date for N.
            this.isUpToDate[objectSize - 1] = true;

            watch.Stop();
            //Console.WriteLine("Region calculation time: {0} ms", watch.ElapsedMilliseconds);
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
        /// Gets the neighbour coordinates of the given point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>The neighbour coordinates of the given point.</returns>
        private List<Point> GetNeighbourCoords(Point point)
        {
            bool isTop = point.Y == 0;
            bool isLeft = point.X == 0;
            bool isRight = point.X == this.areaOnGrid.Width - 1;
            List<Point> retList = new List<Point>();

            if (!isTop && !isLeft) { retList.Add(new Point(point.X - 1, point.Y - 1)); }
            if (!isTop) { retList.Add(new Point(point.X, point.Y - 1)); }
            if (!isTop && !isRight) { retList.Add(new Point(point.X + 1, point.Y - 1)); }
            if (!isLeft) { retList.Add(new Point(point.X - 1, point.Y)); }

            return retList;
        }

        /// <summary>
        /// The grid that this sector belongs to.
        /// </summary>
        private readonly Grid grid;

        /// <summary>
        /// The area of this sector on the grid.
        /// </summary>
        private readonly Rectangle areaOnGrid;

        /// <summary>
        /// The coordinates of the center of this sector.
        /// </summary>
        private readonly Point center;

        /// <summary>
        /// The Nth item of this array is the set containing the N-size regions of this sector.
        /// </summary>
        private readonly HashSet<Region>[] currentRegions;

        /// <summary>
        /// The Nth item of this array is true if the N-size regions of this sector are up-to-date.
        /// </summary>
        private bool[] isUpToDate;

        /// <summary>
        /// The size of the sectors of a motion control grid.
        /// </summary>
        public const int SECTOR_SIZE = 32;

        /// <summary>
        /// The indices of the neighbours to check during the region calculations.
        /// </summary>
        private static readonly int[] DIRECTIONS_TO_CHECK = new int[]
        {
            GridDirections.NORTH_WEST, GridDirections.NORTH, GridDirections.NORTH_EAST, GridDirections.WEST
        };
    }
}
