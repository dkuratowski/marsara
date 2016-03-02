using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace RC.Prototypes.NewPathfinding.MotionControl
{
    /// <summary>
    /// Represents a grid on which the motion control is working.
    /// </summary>
    class Grid
    {
        /// <summary>
        /// Constructs a Grid instance from the given image.
        /// </summary>
        /// <param name="walkabilityImg">The image that contains the walkability informations.</param>
        /// <remarks>
        /// Each pixel of the given image represents a cell on the grid. If the color of a pixel is
        /// white (RGB=FFFFFF) then the corresponding cell will be walkable; otherwise non-walkable.
        /// </remarks>
        public Grid(Bitmap walkabilityImg)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            /// Create the array of sectors
            int horzSectorCount = walkabilityImg.Width / Sector.SECTOR_SIZE;
            int vertSectorCount = walkabilityImg.Height / Sector.SECTOR_SIZE;
            if (walkabilityImg.Width % Sector.SECTOR_SIZE > 0) { horzSectorCount++; }
            if (walkabilityImg.Height % Sector.SECTOR_SIZE > 0) { vertSectorCount++; }
            Sector[,] sectors = new Sector[horzSectorCount, vertSectorCount];

            /// Fill the grid
            bool[,] walkability = new bool[walkabilityImg.Width, walkabilityImg.Height];
            this.cells = new Cell[walkabilityImg.Width, walkabilityImg.Height];
            this.width = walkabilityImg.Width;
            this.height = walkabilityImg.Height;

            /// Create the sectors and the cells and fill walkability informations.
            for (int row = 0; row < walkabilityImg.Height; row++)
            {
                for (int column = 0; column < walkabilityImg.Width; column++)
                {
                    /// Get the sector of the cell to be created of create the sector if not yet been created.
                    Point sectorIndex = new Point(column / Sector.SECTOR_SIZE, row / Sector.SECTOR_SIZE);
                    Sector sectorOfCell = sectors[sectorIndex.X, sectorIndex.Y];
                    if (sectorOfCell == null)
                    {
                        Rectangle sectorArea = new Rectangle(sectorIndex.X * Sector.SECTOR_SIZE, sectorIndex.Y * Sector.SECTOR_SIZE, Sector.SECTOR_SIZE, Sector.SECTOR_SIZE);
                        sectorOfCell = new Sector(sectorArea, this);
                        sectors[sectorIndex.X, sectorIndex.Y] = sectorOfCell;
                    }

                    /// Create the cell and fill walkability informations.
                    this.cells[column, row] = new Cell(new Point(column, row), this, sectorOfCell);
                    bool isWalkable = walkabilityImg.GetPixel(column, row) == Color.FromArgb(255, 255, 255);
                    walkability[column, row] = isWalkable;
                    if (!isWalkable)
                    {
                        /// Cell is an obstacle -> add size constraint to its environment.
                        for (int envCol = 0; envCol < MAX_OBJECT_SIZE; envCol++)
                        {
                            for (int envRow = 0; envRow < MAX_OBJECT_SIZE; envRow++)
                            {
                                int absCol = column - (MAX_OBJECT_SIZE - 1) + envCol;
                                int absRow = row - (MAX_OBJECT_SIZE - 1) + envRow;
                                if (absCol >= 0 && absRow >= 0)
                                {
                                    this.cells[absCol, absRow].AddSizeConstraint(OBSTACLE_ENVIRONMENT[envCol, envRow]);
                                }
                            }
                        }
                    }
                }
            }

            /// Calculate the regions of the sectors.
            //sectors[0, 0].CalculateRegions(1);
            for (int row = 0; row < vertSectorCount; row++)
            {
                for (int column = 0; column < horzSectorCount; column++)
                {
                    Sector sector = sectors[column, row];
                    for (int n = 1; n <= MAX_OBJECT_SIZE; n++)
                    {
                        sector.CalculateRegions(n);
                    }
                }
            }

            watch.Stop();
            Console.WriteLine("Full grid build-up time: {0} ms", watch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Gets the cell of the grid at the given coordinates.
        /// </summary>
        /// <param name="x">The X-coordinate of the cell.</param>
        /// <param name="y">The Y-coordinate of the cell.</param>
        /// <returns>The cell at the given coordinates or null if the given coordinates are outside of the grid.</returns>
        public Cell this[int x, int y]
        {
            get
            {
                return x >= 0 && x < this.width && y >= 0 && y < this.height ? this.cells[x, y] : null;
            }
        }

        /// <summary>
        /// Gets the width of the grid.
        /// </summary>
        public int Width { get { return this.width; } }

        /// <summary>
        /// Gets the height of the grid.
        /// </summary>
        public int Height { get { return this.height; } }

        /// <summary>
        /// Static ctor.
        /// </summary>
        static Grid()
        {
            OBSTACLE_ENVIRONMENT = new int[MAX_OBJECT_SIZE, MAX_OBJECT_SIZE];
            for (int row = 0; row < MAX_OBJECT_SIZE; row++)
            {
                for (int col = 0; col < MAX_OBJECT_SIZE; col++)
                {
                    OBSTACLE_ENVIRONMENT[col, row] = Math.Max(MAX_OBJECT_SIZE - 1 - col, MAX_OBJECT_SIZE - 1 - row);
                }
            }
        }

        /// <summary>
        /// The 2D array that contains the cells of this grid.
        /// </summary>
        private readonly Cell[,] cells;

        /// <summary>
        /// The width of the walkability grid.
        /// </summary>
        private readonly int width;

        /// <summary>
        /// The height of the walkability grid.
        /// </summary>
        private readonly int height;

        /// <summary>
        /// The walkability informations at the environment of an obstacle.
        /// </summary>
        private static readonly int[,] OBSTACLE_ENVIRONMENT;

        /// <summary>
        /// The unit distances in straight and diagonal directions.
        /// </summary>
        public const int STRAIGHT_UNIT_DISTANCE = 2;
        public const int DIAGONAL_UNIT_DISTANCE = 3;

        /// <summary>
        /// The maximum size of objects that can be placed on grids.
        /// </summary>
        public const int MAX_OBJECT_SIZE = 4;
    }
}
