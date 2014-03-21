using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RC.Common;
using System.Drawing.Imaging;
using System.Collections.Generic;
using RC.Engine.Maps.Core;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.UnitTests
{
    /// <summary>
    /// Implements test cases for testing navigation mesh generation.
    /// </summary>
    [TestClass]
    public class NavmeshGeneratorTest
    {
        /// <summary>
        /// The input and output directories.
        /// </summary>
        public const string INPUT_DIR = ".\\NavmeshGeneratorTest_in";
        public const string OUTPUT_DIR = ".\\NavmeshGeneratorTest_out";

        /// <summary>
        /// Contains test context informations.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// This initializer method creates the output directory if it doesn't exist.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize(TestContext context) { Directory.CreateDirectory(OUTPUT_DIR); }

        /// <summary>
        /// Test cases for different grids.
        /// </summary>
        [TestMethod]
        public void NavmeshGenTest_Grid0() { this.NavmeshGenTestImpl("grid0.png", "grid0_navmesh.png"); }
        [TestMethod]
        public void NavmeshGenTest_Grid1() { this.NavmeshGenTestImpl("grid1.png", "grid1_navmesh.png"); }
        [TestMethod]
        public void NavmeshGenTest_Grid2() { this.NavmeshGenTestImpl("grid2.png", "grid2_navmesh.png"); }
        [TestMethod]
        public void NavmeshGenTest_Grid3() { this.NavmeshGenTestImpl("grid3.png", "grid3_navmesh.png"); }
        [TestMethod]
        public void NavmeshGenTest_Grid4() { this.NavmeshGenTestImpl("grid4.png", "grid4_navmesh.png"); }
        [TestMethod]
        public void NavmeshGenTest_Grid5() { this.NavmeshGenTestImpl("grid5.png", "grid5_navmesh.png", 0.05f); }
        [TestMethod]
        public void NavmeshGenTest_Grid6() { this.NavmeshGenTestImpl("grid6.png", "grid6_navmesh.png", 0.19f); }

        /// <summary>
        /// The implementation of the NavMeshGenerationTest_XXX test cases.
        /// </summary>
        /// <param name="inputFile">The path to the input file.</param>
        /// <param name="outputFile">The path to the output file.</param>
        /// <param name="checkCoverage">
        /// This flag indicates if navmesh coverage shall be checked at the end of the test or not.
        /// </param>
        private void NavmeshGenTestImpl(string inputFile, string outputFile, float maxCoverageError = 0.02f)
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
            NavmeshPainter painter = new NavmeshPainter(grid, CELL_SIZE, OFFSET);

            // ***********************************************
            //List<TessellationHelper> helpers = this.GetCopyOfHelperList(navmesh);
            //foreach (TessellationHelper helper in helpers)
            //{
            //    PrivateObject helperObj = new PrivateObject(helper);
            //    RCPolygon border = (RCPolygon)helperObj.GetField("border");
            //    List<RCPolygon> holes = (List<RCPolygon>)helperObj.GetField("holes");
            //    DrawPolygon(border, gc);
            //    foreach (RCPolygon hole in holes) { DrawPolygon(hole, gc); }
            //}
            // ***********************************************

            /// Draw the navmesh nodes and the neighbourhood relationships between them.
            foreach (NavMeshNode node in navmesh.Nodes) { painter.DrawNode(node); }
            foreach (NavMeshNode node in navmesh.Nodes) { painter.DrawNeighbourLines(node); }

            /// Save the output image and dispose the resources.
            painter.OutputImage.Save(outputPath);
            TestContext.AddResultFile(outputPath);
            painter.Dispose();

            TestContext.WriteLine("Validating navmesh...");
            NavmeshValidator validator = new NavmeshValidator(navmesh, grid);
            validator.Validate();
            TestContext.WriteLine("Coverage error: {0}%", validator.CoverageError);
            Assert.IsTrue(validator.CoverageError <= maxCoverageError);
            TestContext.WriteLine("OK");
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
