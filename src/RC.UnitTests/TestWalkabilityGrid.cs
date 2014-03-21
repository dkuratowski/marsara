using RC.Common;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.UnitTests
{
    /// <summary>
    /// This implementation of the IWalkabilityGrid interface can be used in unit tests.
    /// </summary>
    class TestWalkabilityGrid : IWalkabilityGrid
    {
        /// <summary>
        /// Constructs a TestWalkabilityGrid instance from the given image.
        /// </summary>
        /// <param name="walkabilityImg">The image that contains the walkability informations.</param>
        /// <remarks>
        /// Each pixel of the given image represents a cell on the walkability grid. If the color of a pixel is
        /// white (RGB=FFFFFF) then the corresponding cell will be walkable; otherwise non-walkable.
        /// </remarks>
        public TestWalkabilityGrid(Bitmap walkabilityImg)
        {
            if (walkabilityImg.PixelFormat != PixelFormat.Format24bppRgb)
            {
                throw new Exception("Pixel format of the test Bitmap must be PixelFormat.Format24bppRgb");
            }

            /// Fill the grid
            this.grid = new bool[walkabilityImg.Width, walkabilityImg.Height];
            this.width = walkabilityImg.Width;
            this.height = walkabilityImg.Height;
            for (int row = 0; row < walkabilityImg.Height; row++)
            {
                for (int column = 0; column < walkabilityImg.Width; column++)
                {
                    if (walkabilityImg.GetPixel(column, row) == Color.FromArgb(255, 255, 255))
                    {
                        this.grid[column, row] = true;
                    }
                }
            }
        }

        /// <see cref="IWalkabilityGrid.this[]"/>
        public bool this[RCIntVector position]
        {
            get
            {
                return position.X >= 0 && position.X < this.width && position.Y >= 0 && position.Y < this.height ? this.grid[position.X, position.Y] : false;
            }
        }

        /// <see cref="IWalkabilityGrid.Width"/>
        public int Width { get { return this.width; } }

        /// <see cref="IWalkabilityGrid.Height"/>
        public int Height { get { return this.height; } }

        /// <summary>
        /// The 2D array that contains the walkability informations.
        /// </summary>
        private bool[,] grid;

        /// <summary>
        /// The width of the walkability grid.
        /// </summary>
        private int width;

        /// <summary>
        /// The height of the walkability grid.
        /// </summary>
        private int height;
    }
}
