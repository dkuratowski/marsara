using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RC.Common;
using RC.Common.Configuration;
using RC.Engine.Maps.Core;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.UnitTests
{
    /// <summary>
    /// Implements test cases for testing the navigation mesh of every released map.
    /// </summary>
    [TestClass]
    public class MapNavmeshTest
    {
        /// <summary>
        /// Class level constructor.
        /// </summary>
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            ConfigurationManager.Initialize("..\\..\\..\\..\\..\\config\\RC.Engine.Simulator\\RC.Engine.Simulator.node");
            ConstantsTable.Add("RC.App.Version", "1.0.0.0", "STRING");
            tilesets = new Dictionary<string, ITileSet>();
        }

        /// <summary>
        /// The input and output directories.
        /// </summary>
        public const string INPUT_DIR = "..\\..\\..\\..\\..\\maps";
        public const string OUTPUT_DIR = ".\\MapNavmeshTest_out";
        public const string TILESET_DIR = "..\\..\\..\\..\\..\\tilesets";

        /// <summary>
        /// Contains test context informations.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// This initializer method creates the output directory if it doesn't exist.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            tilesets = new Dictionary<string, ITileSet>();
            mapLoader = new MapLoader();
            mapLoader.Start();
            PrivateObject mapLoaderObj = new PrivateObject(mapLoader);
            ((RCThread)mapLoaderObj.GetField("initThread")).Join();
            navmeshLoader = new NavMeshLoader();

            /// Load the tilesets from the tileset directory
            DirectoryInfo rootDir = new DirectoryInfo(TILESET_DIR);
            FileInfo[] tilesetFiles = rootDir.GetFiles("*.xml", SearchOption.AllDirectories);
            TileSetLoader tilesetLoader = new TileSetLoader();
            foreach (FileInfo tilesetFile in tilesetFiles)
            {
                /// TODO: this is a hack! Later we will have binary tileset format.
                string xmlStr = File.ReadAllText(tilesetFile.FullName);
                string imageDir = tilesetFile.DirectoryName;
                RCPackage tilesetPackage = RCPackage.CreateCustomDataPackage(PackageFormats.TILESET_FORMAT);
                tilesetPackage.WriteString(0, xmlStr);
                tilesetPackage.WriteString(1, imageDir);

                byte[] buffer = new byte[tilesetPackage.PackageLength];
                tilesetPackage.WritePackageToBuffer(buffer, 0);
                ITileSet tileset = tilesetLoader.LoadTileSet(buffer);

                if (tilesets.ContainsKey(tileset.Name))
                {
                    throw new InvalidOperationException(string.Format("Tileset with name '{0}' already loaded!", tileset.Name));
                }

                tilesets.Add(tileset.Name, tileset);
            }

            Directory.CreateDirectory(OUTPUT_DIR);
        }

        /// <summary>
        /// Tests the navmesh generation, saving and loading for each released maps. Steps for each map:
        ///     - Load the map object from the map file.
        ///     - Load the navmesh from the map file.
        ///     - Generate a navmesh from the map.
        ///     - Check if the loaded and the generated navmeshes are equal.
        ///     - Serialize the generated navmesh into a byte array.
        ///     - Reload the generated navmesh from the byte array.
        ///     - Check if the generated and the reloaded navmeshes are equal.
        /// </summary>
        [TestMethod]
        public void MapNavmeshSaveLoadTest()
        {
            foreach (string mapFilePath in Directory.GetFiles(INPUT_DIR))
            {
                TestContext.WriteLine("Testing map file: {0}", mapFilePath);
                byte[] mapBytes = File.ReadAllBytes(mapFilePath);

                TestContext.WriteLine("Loading map data...");
                MapHeader mapHeader = mapLoader.LoadMapHeader(mapBytes);
                IMapAccess map = mapLoader.LoadMap(tilesets[mapHeader.TilesetName], mapBytes);

                TestContext.WriteLine("Loading navmesh data...");
                INavMesh loadedNavmesh = navmeshLoader.LoadNavMesh(mapBytes);
                Assert.IsNotNull(loadedNavmesh);

                TestContext.WriteLine("Generating navmesh...");
                Stopwatch watch = Stopwatch.StartNew();
                INavMesh generatedNavmesh = navmeshLoader.NewNavMesh(new MapWalkabilityReader(map));
                TestContext.WriteLine("Navmesh generation completed. Duration: {0} ms", watch.ElapsedMilliseconds);

                TestContext.WriteLine("Check the equality of the loaded and the generated navmesh...");
                this.CompareNavmeshes(loadedNavmesh, generatedNavmesh);
                Assert.AreEqual<RCIntVector>(map.CellSize, generatedNavmesh.GridSize);
                TestContext.WriteLine("OK");

                TestContext.WriteLine("Serializing the generated navmesh...");
                byte[] serializedNavmesh = navmeshLoader.SaveNavMesh(generatedNavmesh);
                TestContext.WriteLine("Deserializing the generated navmesh...");
                INavMesh reloadedNavmesh = navmeshLoader.LoadNavMesh(serializedNavmesh);

                TestContext.WriteLine("Check the equality of the original and the deserialized navmesh...");
                this.CompareNavmeshes(generatedNavmesh, reloadedNavmesh);
                TestContext.WriteLine("OK");

                TestContext.WriteLine("Closing map...");
                map.Close();
                TestContext.WriteLine("Map file OK: {0}", mapFilePath);
            }
        }

        /// <summary>
        /// Validates the navmesh of each released maps. Steps for each map:
        ///     - Load the map object from the map file.
        ///     - Load the navmesh from the map file.
        ///     - Check the walkability hash of the loaded navmesh.
        ///     - Validate the loaded navmesh.
        /// </summary>
        [TestMethod]
        public void MapNavmeshValidationTest()
        {
            foreach (string mapFilePath in Directory.GetFiles(INPUT_DIR))
            {
                string outputPath = Path.Combine(OUTPUT_DIR, Path.GetFileNameWithoutExtension(mapFilePath) + ".png");

                TestContext.WriteLine("Testing map file: {0}", mapFilePath);
                byte[] mapBytes = File.ReadAllBytes(mapFilePath);

                TestContext.WriteLine("Loading map data...");
                MapHeader mapHeader = mapLoader.LoadMapHeader(mapBytes);
                IMapAccess map = mapLoader.LoadMap(tilesets[mapHeader.TilesetName], mapBytes);

                TestContext.WriteLine("Generating navmesh...");
                Stopwatch watch = Stopwatch.StartNew();
                INavMesh generatedNavmesh = navmeshLoader.NewNavMesh(new MapWalkabilityReader(map));
                TestContext.WriteLine("Navmesh generation completed. Duration: {0} ms", watch.ElapsedMilliseconds);

                TestContext.WriteLine("Creating output file: {0}", outputPath);
                NavmeshPainter painter = new NavmeshPainter(new MapWalkabilityReader(map), CELL_SIZE, OFFSET);
                foreach (NavMeshNode node in generatedNavmesh.Nodes) { painter.DrawNode(node); }
                foreach (NavMeshNode node in generatedNavmesh.Nodes) { painter.DrawNeighbourLines(node); }
                painter.OutputImage.Save(outputPath);
                TestContext.AddResultFile(outputPath);
                painter.Dispose();

                TestContext.WriteLine("Validating navmesh...");
                Assert.IsTrue(navmeshLoader.CheckNavmeshIntegrity(new MapWalkabilityReader(map), generatedNavmesh));
                NavmeshValidator validator = new NavmeshValidator((NavMesh)generatedNavmesh, new MapWalkabilityReader(map));
                validator.Validate();
                TestContext.WriteLine("Coverage error: {0}%", validator.CoverageError);
                Assert.IsTrue(validator.CoverageError <= 0.05f);
                TestContext.WriteLine("OK");

                TestContext.WriteLine("Closing map...");
                map.Close();
                TestContext.WriteLine("Map file OK: {0}", mapFilePath);
            }
        }

        /// <summary>
        /// Checks whether the two given navmeshes are equal.
        /// </summary>
        /// <param name="navmeshA">The first navmesh to compare.</param>
        /// <param name="navmeshB">The second navmesh to compare.</param>
        private void CompareNavmeshes(INavMesh navmeshA, INavMesh navmeshB)
        {
            Assert.AreEqual<int>(navmeshA.WalkabilityHash, navmeshB.WalkabilityHash);
            Assert.AreEqual<RCIntVector>(navmeshA.GridSize, navmeshB.GridSize);

            List<INavMeshNode> nodesA = new List<INavMeshNode>(navmeshA.Nodes);
            List<INavMeshNode> nodesB = new List<INavMeshNode>(navmeshB.Nodes);
            Assert.AreEqual<int>(nodesA.Count, nodesB.Count);

            for (int nodeIdx = 0; nodeIdx < nodesA.Count; nodeIdx++)
            {
                this.ComparePolygons(nodesA[nodeIdx].Polygon, nodesB[nodeIdx].Polygon);

                HashSet<int> neighbourIndicesA = new HashSet<int>(
                    from neighbour in nodesA[nodeIdx].Neighbours
                    select nodesA.IndexOf(neighbour));
                HashSet<int> neighbourIndicesB = new HashSet<int>(
                    from neighbour in nodesB[nodeIdx].Neighbours
                    select nodesB.IndexOf(neighbour));

                Assert.IsTrue(neighbourIndicesA.SetEquals(neighbourIndicesB));
            }
        }

        /// <summary>
        /// Checks whether the two given polygons are equal.
        /// </summary>
        /// <param name="polygonA">The first polygon to compare.</param>
        /// <param name="polygonB">The second polygon to compare.</param>
        private void ComparePolygons(RCPolygon polygonA, RCPolygon polygonB)
        {
            Assert.AreEqual<int>(polygonA.VertexCount, polygonB.VertexCount);
            for (int vertexIdx = 0; vertexIdx < polygonA.VertexCount; vertexIdx++)
            {
                Assert.AreEqual<RCNumVector>(polygonA[vertexIdx], polygonB[vertexIdx]);
            }
        }

        /// <summary>
        /// Reference to the MapLoader component.
        /// </summary>
        private static MapLoader mapLoader;

        /// <summary>
        /// The list of the tilesets mapped by their names.
        /// </summary>
        private static Dictionary<string, ITileSet> tilesets;

        /// <summary>
        /// Reference to the NavMeshLoader component.
        /// </summary>
        private static NavMeshLoader navmeshLoader;

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
