using RC.Common;
using RC.Engine.Simulator.Core;
using RC.Engine.Simulator.MotionControl;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace RC.Engine.Test
{
    class TestWalkabilityGrid : IWalkabilityGrid
    {
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

        public bool this[RCIntVector position]
        {
            get
            {
                return position.X >= 0 && position.X < this.width && position.Y >= 0 && position.Y < this.height ? this.grid[position.X, position.Y] : false;
            }
        }

        public int Width { get { return this.width; } }
        public int Height { get { return this.height; } }

        private bool[,] grid;
        private int width;
        private int height;
    }
}
