using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.Core;
using RC.Common;
using System.Drawing;
using System.Drawing.Imaging;

namespace RC.Engine.Test
{
    static class PFTreeTest
    {
        public static void PFTreeBuildupTest()
        {
            PFTreeNode treeNode = new PFTreeNode(2);
            treeNode.AddObstacle(new RCIntVector(0, 0));
            treeNode.AddObstacle(new RCIntVector(2, 2));
            treeNode.AddObstacle(new RCIntVector(3, 2));
            treeNode.AddObstacle(new RCIntVector(2, 3));
            treeNode.AddObstacle(new RCIntVector(3, 3));
        }

        public static void PFTreeNeighbourTest()
        {
            PFTreeNode pfTreeRoot = PFTreeTest.ReadTestMap("neighbours_test_input.png");

            Bitmap outputBmp = new Bitmap(pfTreeRoot.AreaOnMap.Width * CELL_SIZE, pfTreeRoot.AreaOnMap.Height * CELL_SIZE);
            Graphics outputGC = Graphics.FromImage(outputBmp);
            HashSet<PFTreeNode> leafNodes = pfTreeRoot.GetAllLeafNodes();
            foreach (PFTreeNode leafNode in leafNodes)
            {
                RCIntRectangle nodeRect = leafNode.AreaOnMap * new RCIntVector(CELL_SIZE, CELL_SIZE);
                outputGC.FillRectangle(leafNode.IsWalkable ? Brushes.Green : Brushes.Red,
                    nodeRect.X, nodeRect.Y, nodeRect.Width, nodeRect.Height);
                outputGC.DrawRectangle(Pens.Black, nodeRect.X, nodeRect.Y, nodeRect.Width, nodeRect.Height);
            }

            foreach (PFTreeNode leafNode in leafNodes)
            {
                foreach (PFTreeNode neighbourNode in leafNode.Neighbours)
                {
                    RCIntVector startPoint = (RCIntVector)((leafNode.Center + new RCNumVector((RCNumber)1 / 2, (RCNumber)1 / 2)) * new RCIntVector(CELL_SIZE, CELL_SIZE));
                    RCIntVector endPoint = (RCIntVector)((neighbourNode.Center + new RCNumVector((RCNumber)1 / 2, (RCNumber)1 / 2)) * new RCIntVector(CELL_SIZE, CELL_SIZE));
                    outputGC.DrawLine(Pens.Yellow, startPoint.X, startPoint.Y, endPoint.X, endPoint.Y);
                }
            }

            outputGC.Dispose();
            outputBmp.Save("neighbours_test_output.png", ImageFormat.Png);
        }

        static PFTreeNode ReadTestMap(string fileName)
        {
            /// Load the test map.
            Bitmap testMapBmp = (Bitmap)Bitmap.FromFile(fileName);
            if (testMapBmp.PixelFormat != PixelFormat.Format24bppRgb)
            {
                throw new Exception("Pixel format of the test Bitmap must be PixelFormat.Format24bppRgb");
            }

            /// Find the number of subdivision levels.
            int boundingBoxSize = Math.Max(testMapBmp.Width, testMapBmp.Height);
            int subdivisionLevels = 1;
            while (boundingBoxSize > (int)Math.Pow(2, subdivisionLevels)) { subdivisionLevels++; }

            /// Create the root of the pathfinder tree.
            PFTreeNode pfTreeRoot = new PFTreeNode(subdivisionLevels);

            /// Add obstacles to the pathfinder tree
            for (int row = 0; row < pfTreeRoot.AreaOnMap.Height; row++)
            {
                for (int column = 0; column < pfTreeRoot.AreaOnMap.Width; column++)
                {
                    if (row >= testMapBmp.Height || column >= testMapBmp.Width)
                    {
                        /// Everything out of the map range is considered to be obstacle.
                        pfTreeRoot.AddObstacle(new RCIntVector(column, row));
                    }
                    else
                    {
                        /// Add obstacle depending on the color of the pixel in the test map image.
                        if (testMapBmp.GetPixel(column, row) == Color.FromArgb(0, 0, 0))
                        {
                            pfTreeRoot.AddObstacle(new RCIntVector(column, row));
                        }
                    }
                }
            }

            return pfTreeRoot;
        }

        const int CELL_SIZE = 10;
    }
}
