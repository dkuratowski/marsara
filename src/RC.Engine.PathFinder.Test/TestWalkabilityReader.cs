using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Pathfinder.PublicInterfaces;

namespace RC.Engine.PathFinder.Test
{
    /// <summary>
    /// This implementation of the IWalkabilityReader interface reads walkability informations from an image.
    /// </summary>
    class TestWalkabilityReader : IWalkabilityReader
    {
        /// <summary>
        /// Constructs a TestWalkabilityReader instance from the given image.
        /// </summary>
        /// <param name="walkabilityImg">The image that contains the walkability informations.</param>
        /// <remarks>
        /// Each pixel of the given image represents a cell on the pathfinding-grid. If the color of a pixel is
        /// white (RGB=FFFFFF) then the corresponding cell will be walkable; otherwise non-walkable.
        /// </remarks>
        public TestWalkabilityReader(Bitmap walkabilityImg)
        {
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

        /// <see cref="IWalkabilityReader.this[]"/>
        public bool this[int x, int y]
        {
            get { return x >= 0 && x < this.width && y >= 0 && y < this.height ? this.grid[x, y] : false; }
        }

        /// <see cref="IWalkabilityReader.Width"/>
        public int Width { get { return this.width; } }

        /// <see cref="IWalkabilityReader.Height"/>
        public int Height { get { return this.height; } }

        /// <summary>
        /// The 2D array that contains the walkability informations.
        /// </summary>
        private bool[,] grid;

        /// <summary>
        /// The width of the grid.
        /// </summary>
        private int width;

        /// <summary>
        /// The height of the grid.
        /// </summary>
        private int height;
    }
}
