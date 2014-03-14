using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RC.Engine.Simulator.MotionControl;
using RC.Common;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace RC.UnitTests
{
    /// <summary>
    /// Implements test cases for testing navigation mesh generation.
    /// </summary>
    [TestClass]
    public class NavMeshTest
    {
        /// <summary>
        /// The input and output directories.
        /// </summary>
        public const string INPUT_DIR = ".\\NavMeshTestInputs";
        public const string OUTPUT_DIR = ".\\NavMeshTestOutputs";

        /// <summary>
        /// This initializer method creates the output directory if it doesn't exist.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize(TestContext context) { Directory.CreateDirectory(OUTPUT_DIR); }

        /// <summary>
        /// Test cases for different grids.
        /// </summary>
        [TestMethod]
        public void NavMeshGenerationTest_Grid0() { this.NavMeshGenerationTestImpl("grid0.png", "grid0_navmesh.png"); }
        [TestMethod]
        public void NavMeshGenerationTest_Grid1() { this.NavMeshGenerationTestImpl("grid1.png", "grid1_navmesh.png"); }
        [TestMethod]
        public void NavMeshGenerationTest_Grid2() { this.NavMeshGenerationTestImpl("grid2.png", "grid2_navmesh.png"); }
        [TestMethod]
        public void NavMeshGenerationTest_Grid3() { this.NavMeshGenerationTestImpl("grid3.png", "grid3_navmesh.png"); }
        [TestMethod]
        public void NavMeshGenerationTest_Grid4() { this.NavMeshGenerationTestImpl("grid4.png", "grid4_navmesh.png"); }
        [TestMethod]
        public void NavMeshGenerationTest_Grid5() { this.NavMeshGenerationTestImpl("grid5.png", "grid5_navmesh.png"); }
        [TestMethod]
        public void NavMeshGenerationTest_Grid6() { this.NavMeshGenerationTestImpl("grid6.png", "grid6_navmesh.png", false); }

        /// <summary>
        /// Contains test context informations.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// The implementation of the NavMeshGenerationTest_XXX test cases.
        /// </summary>
        /// <param name="inputFile">The path to the input file.</param>
        /// <param name="outputFile">The path to the output file.</param>
        private void NavMeshGenerationTestImpl(string inputFile, string outputFile)
        {
            this.NavMeshGenerationTestImpl(inputFile, outputFile, true);
        }

        /// <summary>
        /// The implementation of the NavMeshGenerationTest_XXX test cases.
        /// </summary>
        /// <param name="inputFile">The path to the input file.</param>
        /// <param name="outputFile">The path to the output file.</param>
        /// <param name="checkCoverage">
        /// This flag indicates if navmesh coverage shall be checked at the end of the test or not.
        /// </param>
        private void NavMeshGenerationTestImpl(string inputFile, string outputFile, bool checkCoverage)
        {
            string inputPath = System.IO.Path.Combine(INPUT_DIR, inputFile);
            string outputPath = System.IO.Path.Combine(OUTPUT_DIR, outputFile);

            TestContext.WriteLine("Reading walkability grid: {0}", inputPath);
            TestWalkabilityGrid grid = new TestWalkabilityGrid((Bitmap)Bitmap.FromFile(inputPath));

            TestContext.WriteLine("Generating navmesh...");
            Stopwatch watch = Stopwatch.StartNew();
            NavMesh navmesh = new NavMesh(grid, 2);
            TestContext.WriteLine("Navmesh generation completed. Duration: {0} ms", watch.ElapsedMilliseconds);

            TestContext.WriteLine("Creating output file: {0}", outputPath);

            /// Create the output image.
            Bitmap outputImg = new Bitmap((grid.Width + 2 * OFFSET.X) * CELL_SIZE, (grid.Height + 2 * OFFSET.Y) * CELL_SIZE, PixelFormat.Format24bppRgb);
            Graphics gc = Graphics.FromImage(outputImg);
            gc.Clear(Color.White);
                                   
            /// Draw the original grid enlarged.
            for (int row = 0; row < grid.Height; row++)
            {
                for (int col = 0; col < grid.Width; col++)
                {
                    if (!grid[new RCIntVector(col, row)])
                    {
                        gc.FillRectangle(Brushes.Black, (col + OFFSET.X) * CELL_SIZE, (row + OFFSET.Y) * CELL_SIZE, CELL_SIZE, CELL_SIZE);
                    }
                }
            }

            // ***********************************************
            //foreach (Sector sector in navmesh.Sectors)
            //{
            //    PrivateObject sectorObj = new PrivateObject(sector);
            //    Polygon border = (Polygon)sectorObj.GetField("border");
            //    List<Polygon> holes = (List<Polygon>)sectorObj.GetField("holes");
            //    DrawPolygon(border, gc);
            //    foreach (Polygon hole in holes) { DrawPolygon(hole, gc); }
            //}
            // TODO: SETBACK ***********************************************

            /// Draw the navmesh nodes.
            foreach (Sector sector in navmesh.Sectors) { foreach (NavMeshNode node in sector.Nodes) { DrawNode(node, gc); } }
            foreach (Sector sector in navmesh.Sectors) { foreach (NavMeshNode node in sector.Nodes) { DrawNeighbourLines(node, gc); } }

            /// Save the output image and dispose the resources.
            outputImg.Save(outputPath);
            TestContext.AddResultFile(outputPath);
            gc.Dispose();
            outputImg.Dispose();

            TestContext.WriteLine("Checking navmesh...");
            CheckNavmesh(navmesh);
            if (checkCoverage) { CheckNavmeshCoverage(navmesh, grid); }
            TestContext.WriteLine("OK");
        }

        /// <summary>
        /// Checks the given navmesh.
        /// </summary>
        /// <param name="navmesh">The navmesh to be checked.</param>
        private void CheckNavmesh(NavMesh navmesh)
        {
            HashSet<NavMeshNode> allNodes = new HashSet<NavMeshNode>();
            foreach (Sector sector in navmesh.Sectors)            
            {
                CheckTessellationHelper(sector);
                CheckSectorInterconnection(sector);
                CheckNeighbourhood(sector);
                foreach (NavMeshNode node in sector.Nodes) { CheckNeighbourEdgeMatching(node); }

                HashSet<NavMeshNode> commonNodesWithPreviousSectors = new HashSet<NavMeshNode>(sector.Nodes);
                commonNodesWithPreviousSectors.IntersectWith(allNodes);
                Assert.AreEqual(0, commonNodesWithPreviousSectors.Count);
                allNodes.UnionWith(sector.Nodes);
            }
        }

        /// <summary>
        /// Checks the coverage of the given navmesh.
        /// </summary>
        /// <param name="navmesh">The navmesh to check.</param>
        /// <param name="grid">The walkability grid.</param>
        private void CheckNavmeshCoverage(NavMesh navmesh, IWalkabilityGrid grid)
        {
            RCNumRectangle gridBoundary = new RCNumRectangle(-((RCNumber)1 / (RCNumber)2), -((RCNumber)1 / (RCNumber)2), grid.Width, grid.Height);
            BspSearchTree<NavMeshNode> allNodes = new BspSearchTree<NavMeshNode>(gridBoundary, 16, 10);
            foreach (Sector sector in navmesh.Sectors)
            {
                foreach (NavMeshNode node in sector.Nodes)
                {
                    Assert.IsTrue(gridBoundary.Contains(node.BoundingBox));
                    allNodes.AttachContent(node);
                }
            }

            int correctCells = 0;
            int incorrectCells = 0;
            for (int row = 0; row < grid.Height; row++)
            {
                for (int col = 0; col < grid.Width; col++)
                {
                    RCNumVector point = new RCNumVector(col, row);
                    bool insideNode = false;
                    foreach (NavMeshNode node in allNodes.GetContents(point))
                    {
                        if (node.Polygon.Contains(point)) { insideNode = true; break; }
                    }

                    if (insideNode && grid[new RCIntVector(col, row)] || !insideNode && !grid[new RCIntVector(col, row)])
                    {
                        correctCells++;
                    }
                    else
                    {
                        incorrectCells++;
                    }
                }
            }

            Assert.IsTrue((float)incorrectCells / (float)(correctCells + incorrectCells) < 0.05f);
        }

        /// <summary>
        /// Checks the given sector.
        /// </summary>
        /// <param name="sector">The sector to be checked.</param>
        private void CheckTessellationHelper(Sector sector)
        {
            PrivateObject sectorObj = new PrivateObject(sector);
            TessellationHelper helper = (TessellationHelper)sectorObj.GetField("tessellation");

            Dictionary<RCNumVector, HashSet<NavMeshNode>> vertexMap = GetCopyOfVertexMap(helper);
            foreach (NavMeshNode node in helper.Nodes)
            {
                for (int i = 0; i < node.Polygon.VertexCount; ++i)
                {
                    RCNumVector vertex = node.Polygon[i];
                    Assert.IsTrue(vertexMap[vertex].Remove(node));
                    if (vertexMap[vertex].Count == 0) { vertexMap.Remove(vertex); }
                }
            }
            Assert.IsTrue(vertexMap.Count == 0);
        }

        /// <summary>
        /// Checks whether the given sector is correctly interconnected.
        /// </summary>
        /// <param name="sector">The sector to check.</param>
        private void CheckSectorInterconnection(Sector sector)
        {
            HashSet<NavMeshNode> nodesOfSector = new HashSet<NavMeshNode>(sector.Nodes);
            CollectReachableNodes(nodesOfSector.First(), ref nodesOfSector);
            Assert.AreEqual(0, nodesOfSector.Count);
        }

        /// <summary>
        /// Checks that if two nodes in the given sector share at least one common edge then they are neighbours.
        /// </summary>
        /// <param name="sector">The sector to check.</param>
        private void CheckNeighbourhood(Sector sector)
        {
            PrivateObject sectorObj = new PrivateObject(sector);
            TessellationHelper helper = (TessellationHelper)sectorObj.GetField("tessellation");
            Dictionary<RCNumVector, HashSet<NavMeshNode>> vertexMap = GetCopyOfVertexMap(helper);

            foreach (NavMeshNode node in sector.Nodes)
            {
                for (int i = 0; i < node.Polygon.VertexCount; i++)
                {
                    RCNumVector edgeBegin = node.Polygon[i];
                    RCNumVector edgeEnd = node.Polygon[(i + 1) % node.Polygon.VertexCount];
                    HashSet<NavMeshNode> matchingNodesCopy = vertexMap[edgeBegin];
                    matchingNodesCopy.IntersectWith(vertexMap[edgeEnd]);
                    if (matchingNodesCopy.Count == 2)
                    {
                        List<NavMeshNode> matchingNodesList = new List<NavMeshNode>(matchingNodesCopy);
                        Assert.IsTrue(matchingNodesList[0].Neighbours.Contains(matchingNodesList[1]));
                        Assert.IsTrue(matchingNodesList[1].Neighbours.Contains(matchingNodesList[0]));
                    }
                }
            }
        }

        /// <summary>
        /// Checks whether the neighbours of the given have at least one common edge with the given node.
        /// </summary>
        /// <param name="node">The node to be checked.</param>
        private void CheckNeighbourEdgeMatching(NavMeshNode node)
        {
            /// Collect the edges of the current node.
            HashSet<Tuple<RCNumVector, RCNumVector>> nodeEdges = new HashSet<Tuple<RCNumVector, RCNumVector>>();
            for (int i = 0; i < node.Polygon.VertexCount; i++)
            {
                nodeEdges.Add(new Tuple<RCNumVector, RCNumVector>(node.Polygon[i], node.Polygon[(i + 1) % node.Polygon.VertexCount]));
            }

            /// Check the neighbours of the current node.
            foreach (NavMeshNode neighbour in node.Neighbours)
            {
                /// Collect the edges of the current neighbour.
                HashSet<Tuple<RCNumVector, RCNumVector>> neighbourEdges = new HashSet<Tuple<RCNumVector, RCNumVector>>();
                for (int i = 0; i < neighbour.Polygon.VertexCount; i++)
                {
                    neighbourEdges.Add(new Tuple<RCNumVector, RCNumVector>(neighbour.Polygon[(i + 1) % neighbour.Polygon.VertexCount], neighbour.Polygon[i]));
                }

                /// Check if the neighbour has common edges with the current node.
                neighbourEdges.IntersectWith(nodeEdges);
                Assert.IsTrue(neighbourEdges.Count > 0);

                /// Remove the common edges from the current node.
                nodeEdges.ExceptWith(neighbourEdges);
            }
        }

        /// <summary>
        /// Recursive method for visiting all the reachable nodes starting from the given node.
        /// </summary>
        /// <param name="initialNode">The node to start from.</param>
        /// <param name="unvisitedNodes">List of the unvisited nodes.</param>
        private void CollectReachableNodes(NavMeshNode initialNode, ref HashSet<NavMeshNode> unvisitedNodes)
        {
            Assert.IsTrue(unvisitedNodes.Remove(initialNode));
            foreach (NavMeshNode neighbour in initialNode.Neighbours)
            {
                bool isBidirectional = false;
                foreach (NavMeshNode neighbourOfNeighbour in neighbour.Neighbours) { if (neighbourOfNeighbour == initialNode) { isBidirectional = true; break; } }
                Assert.IsTrue(isBidirectional);
                if (unvisitedNodes.Contains(neighbour))
                {
                    CollectReachableNodes(neighbour, ref unvisitedNodes);
                }
            }
        }

        /// <summary>
        /// Creates a copy of the vertex-map of the given tessellation helper.
        /// </summary>
        /// <param name="helper">The tessellation helper.</param>
        /// <returns>The created copy of the vertex-map of the given tessellation helper.</returns>
        private Dictionary<RCNumVector, HashSet<NavMeshNode>> GetCopyOfVertexMap(TessellationHelper helper)
        {
            PrivateObject helperObj = new PrivateObject(helper);
            Dictionary<RCNumVector, HashSet<NavMeshNode>> originalMap =
                (Dictionary<RCNumVector, HashSet<NavMeshNode>>)helperObj.GetField("vertexMap");

            Dictionary<RCNumVector, HashSet<NavMeshNode>> mapCopy = new Dictionary<RCNumVector, HashSet<NavMeshNode>>();
            foreach (KeyValuePair<RCNumVector, HashSet<NavMeshNode>> item in originalMap)
            {
                mapCopy.Add(item.Key, new HashSet<NavMeshNode>(item.Value));
            }
            return mapCopy;
        }

        /// <summary>
        /// Draws the given navmesh node to the given graphic context.
        /// </summary>
        /// <param name="node">The navmesh node to be drawn.</param>
        /// <param name="gc">The graphic context.</param>
        private void DrawNode(NavMeshNode node, Graphics gc)
        {
            this.DrawPolygon(node.Polygon, gc);

            RCNumVector nodeCenter = (node.Polygon.Center + OFFSET) * new RCNumVector(CELL_SIZE, CELL_SIZE);
            gc.DrawString(node.ID.ToString(), SystemFonts.SmallCaptionFont, Brushes.Green, nodeCenter.Round().X, nodeCenter.Round().Y);
        }

        /// <summary>
        /// Draws the given polygon to the given graphic context.
        /// </summary>
        /// <param name="polygon">The polygon to be drawn.</param>
        /// <param name="gc">The graphic context.</param>
        private void DrawPolygon(Polygon polygon, Graphics gc)
        {
            RCNumVector prevPoint = RCNumVector.Undefined;
            for (int i = 0; i < polygon.VertexCount; i++)
            {
                RCNumVector currPoint = (polygon[i] + new RCNumVector((RCNumber)1 / (RCNumber)2, (RCNumber)1 / (RCNumber)2) + OFFSET) * new RCNumVector(CELL_SIZE, CELL_SIZE);
                if (prevPoint != RCNumVector.Undefined)
                {
                    gc.DrawLine(Pens.Red, prevPoint.Round().X, prevPoint.Round().Y, currPoint.Round().X, currPoint.Round().Y);
                }
                prevPoint = currPoint;
            }

            RCNumVector lastPoint = (polygon[polygon.VertexCount - 1] + new RCNumVector((RCNumber)1 / (RCNumber)2, (RCNumber)1 / (RCNumber)2) + OFFSET) * new RCNumVector(CELL_SIZE, CELL_SIZE);
            RCNumVector firstPoint = (polygon[0] + new RCNumVector((RCNumber)1 / (RCNumber)2, (RCNumber)1 / (RCNumber)2) + OFFSET) * new RCNumVector(CELL_SIZE, CELL_SIZE);
            gc.DrawLine(Pens.Red, lastPoint.Round().X, lastPoint.Round().Y, firstPoint.Round().X, firstPoint.Round().Y);
        }

        /// <summary>
        /// Draws the neigbour relationships of the given navmesh node to the given graphic context.
        /// </summary>
        /// <param name="node">The navmesh node whose neighbour relationships have to be drawn.</param>
        /// <param name="gc">The graphic context.</param>
        /// <remarks>
        /// Bi-directional neighbour relationships will be drawn with blue, one-way neighbour relationships will be
        /// drawn with yellow.
        /// </remarks>
        private void DrawNeighbourLines(NavMeshNode node, Graphics gc)
        {
            RCNumVector nodeCenter = (node.Polygon.Center + OFFSET) * new RCNumVector(CELL_SIZE, CELL_SIZE);
            gc.DrawEllipse(Pens.Blue, nodeCenter.Round().X - 3, nodeCenter.Round().Y - 3, 6, 6);
            foreach (NavMeshNode neighbour in node.Neighbours)
            {
                bool isBidirectional = false;
                foreach (NavMeshNode neighbourOfNeighbour in neighbour.Neighbours) { if (neighbourOfNeighbour == node) { isBidirectional = true; break; } }

                RCNumVector neighbourCenter = (neighbour.Polygon.Center + OFFSET) * new RCNumVector(CELL_SIZE, CELL_SIZE);
                gc.DrawLine(isBidirectional ? Pens.Blue : Pens.Yellow, nodeCenter.Round().X, nodeCenter.Round().Y, neighbourCenter.Round().X, neighbourCenter.Round().Y);
            }
        }

        /// <summary>
        /// The size of 1 cell on the result image.
        /// </summary>
        private static int CELL_SIZE = 8;

        /// <summary>
        /// The offset of the top-left cell on the result image.
        /// </summary>
        private static RCIntVector OFFSET = new RCIntVector(10, 10);
    }
}
