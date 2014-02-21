using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Configuration;
using System.IO;
using System.Reflection;
using RC.Common;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using RC.Common.ComponentModel;
using RC.Engine.Maps.Core;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Core;
using RC.Engine.Simulator.InternalInterfaces;
using RC.Engine.Simulator.PublicInterfaces;
using System.Collections;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.MotionControl;
using System.Drawing.Drawing2D;

namespace RC.Engine.Test
{
    class Program
    {
        static int CELL_SIZE = 8;
        static List<Pen> PENS = new List<Pen>()
        {
            Pens.Red, Pens.Green, Pens.Blue, Pens.Cyan, Pens.Magenta, Pens.Orange, Pens.LightBlue
        };

        static void Main(string[] args)
        {
            //Polygon p = new Polygon(new RCNumVector(1, 1), new RCNumVector(2, 2), new RCNumVector(3, 1), new RCNumVector(5, 1), new RCNumVector(5, 5), new RCNumVector(3, 5), new RCNumVector(2, 4), new RCNumVector(1, 5));
            //bool b = p.Contains(new RCNumVector(1, 1));
            //b = p.Contains(new RCNumVector(2, 2));
            //b = p.Contains(new RCNumVector(3, 1));
            //b = p.Contains(new RCNumVector(5, 1));
            //b = p.Contains(new RCNumVector(5, 5));
            //b = p.Contains(new RCNumVector(3, 5));
            //b = p.Contains(new RCNumVector(2, 4));
            //b = p.Contains(new RCNumVector(1, 5));

            //b = p.Contains((new RCNumVector(1, 1) + new RCNumVector(2, 2)) / 2);
            //b = p.Contains((new RCNumVector(2, 2) + new RCNumVector(3, 1)) / 2);
            //b = p.Contains((new RCNumVector(3, 1) + new RCNumVector(5, 1)) / 2);
            //b = p.Contains((new RCNumVector(5, 1) + new RCNumVector(5, 5)) / 2);
            //b = p.Contains((new RCNumVector(5, 5) + new RCNumVector(3, 5)) / 2);
            //b = p.Contains((new RCNumVector(3, 5) + new RCNumVector(2, 4)) / 2);
            //b = p.Contains((new RCNumVector(2, 4) + new RCNumVector(1, 5)) / 2);
            //b = p.Contains((new RCNumVector(1, 5) + new RCNumVector(1, 1)) / 2);

            //b = p.Contains(new RCNumVector((RCNumber)3 / (RCNumber)2, 2));
            //b = p.Contains(new RCNumVector((RCNumber)3 / (RCNumber)2, 4));
            //b = p.Contains(new RCNumVector(0, 4));
            //b = p.Contains(new RCNumVector(0, 5));
            //b = p.Contains(new RCNumVector(0, 1));

            TestWalkabilityGrid testGrid = new TestWalkabilityGrid((Bitmap)Bitmap.FromFile("testgrid.png"));

            /// QUAD-TREE TEST **************************************************************************************************************************
            //WalkabilityQuadTreeNode quadTreeRoot = WalkabilityQuadTreeNode.CreateQuadTree(testGrid);
            //Bitmap outputBmp = new Bitmap(testGrid.Width * CELL_SIZE, testGrid.Height * CELL_SIZE);
            //Graphics outputGC = Graphics.FromImage(outputBmp);
            //HashSet<WalkabilityQuadTreeNode> leafNodes = new HashSet<WalkabilityQuadTreeNode>();
            //quadTreeRoot.CollectLeafNodes(leafNodes);
            //foreach (WalkabilityQuadTreeNode leafNode in leafNodes)
            //{
            //    RCIntRectangle nodeRect = leafNode.AreaOnGrid * new RCIntVector(CELL_SIZE, CELL_SIZE);
            //    outputGC.FillRectangle(leafNode.IsWalkable ? Brushes.Green : Brushes.Red,
            //        nodeRect.X, nodeRect.Y, nodeRect.Width, nodeRect.Height);
            //    outputGC.DrawRectangle(Pens.Black, nodeRect.X, nodeRect.Y, nodeRect.Width, nodeRect.Height);

            //    RCIntVector startPoint = new RCIntVector((nodeRect.Left + nodeRect.Right) / 2, (nodeRect.Top + nodeRect.Bottom) / 2);
            //    foreach (WalkabilityQuadTreeNode neighbourNode in leafNode.Neighbours)
            //    {
            //        RCIntRectangle neighbourNodeRect = neighbourNode.AreaOnGrid * new RCIntVector(CELL_SIZE, CELL_SIZE);
            //        RCIntVector endPoint = new RCIntVector((neighbourNodeRect.Left + neighbourNodeRect.Right) / 2, (neighbourNodeRect.Top + neighbourNodeRect.Bottom) / 2);
            //        outputGC.DrawLine(Pens.Yellow, startPoint.X, startPoint.Y, endPoint.X, endPoint.Y);
            //    }
            //}
            //outputGC.Dispose();
            //outputBmp.Save("testgrid_neighbours.png", ImageFormat.Png);
            //outputBmp.Dispose();
            /// *****************************************************************************************************************************************

            Stopwatch watch = new Stopwatch();
            watch.Start();
            NavMesh navmesh = new NavMesh(testGrid, 2);
            Console.WriteLine(watch.ElapsedMilliseconds);

            Bitmap outputImg = new Bitmap(testGrid.Width * CELL_SIZE, testGrid.Height * CELL_SIZE, PixelFormat.Format24bppRgb);
            Graphics gc = Graphics.FromImage(outputImg);
            gc.Clear(Color.White);

            /// Draw the original grid enlarged.
            for (int row = 0; row < testGrid.Height; row++)
            {
                for (int col = 0; col < testGrid.Width; col++)
                {
                    if (!testGrid[new RCIntVector(col, row)])
                    {
                        gc.FillRectangle(Brushes.Black, col * CELL_SIZE, row * CELL_SIZE, CELL_SIZE, CELL_SIZE);
                    }
                }
            }

            /// Draw the sectors.
            int i = 0;
            foreach (Sector sector in navmesh.Sectors)
            {
                foreach (NavMeshNode node in sector.Nodes)
                {
                    DrawPolygon(node.Polygon, gc, PENS[i % PENS.Count]);
                    //DrawNeighbourArrows(node, gc);
                }
                //if (i == 6)
                //{
                    //DrawPolygon(sector.Border, gc, PENS[i % PENS.Count]);
                    //foreach (Polygon wallPolygon in sector.Walls)
                    //{
                    //    DrawPolygon(wallPolygon, gc, PENS[i % PENS.Count]);
                    //}
                //}
                ++i;
            }

            gc.Dispose();
            outputImg.Save("testoutput.png");
            outputImg.Dispose();

            //ConfigurationManager.Initialize("../../../../config/RC.Engine.Test/RC.Engine.Test.root");
            //ComponentManager.RegisterComponents("RC.Engine.Simulator, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", new string[1] { "RC.Engine.Simulator.HeapManager" });
            //ComponentManager.RegisterPluginAssembly("RC.Engine.Simulator.Terran, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            //ComponentManager.StartComponents();

            //bool testResult = SimulationHeapTest.StressTest();
            //if (!testResult) { throw new Exception("Test failed!"); }

            //IHeapManager heapMgr = ComponentManager.GetInterface<IHeapManager>();
            //heapMgr.CreateHeap();

            //TreeBranch node0 = new TreeBranch();
            //TreeBranch node1 = new TreeBranch();
            //TreeBranch node2 = new TreeBranch();
            //TreeBranch node3 = new TreeBranch();
            //TreeLeaf node4 = new TreeLeaf();
            //TreeLeaf node5 = new TreeLeaf();
            //TreeLeaf node6 = new TreeLeaf();
            //TreeLeaf node7 = new TreeLeaf();
            //TreeLeaf node8 = new TreeLeaf();
            //node0.Children.New(3);
            //node0.Children[0].Write(node1);
            //node0.Children[1].Write(node2);
            //node0.Children[2].Write(node3);

            //node1.Children.New(2);
            //node1.Children[0].Write(node4);
            //node1.Children[1].Write(node5);

            //node2.Children.New(1);
            //node2.Children[0].Write(node6);

            //node3.Children.New(2);
            //node3.Children[0].Write(node7);
            //node3.Children[1].Write(node8);

            //node4.Values.New(3);
            //node4.Values[0].Write(4);
            //node4.Values[1].Write(5);
            //node4.Values[2].Write(6);

            //node0.Dispose();
            //node1.Dispose();
            //node2.Dispose();
            //node3.Dispose();
            //node4.Dispose();
            //node5.Dispose();
            //node6.Dispose();
            //node7.Dispose();
            //node8.Dispose();

            //PFTreeTest.PFTreeNeighbourTest();
            //TestSimulationHeap();

            //Stopwatch watch = new Stopwatch();
            //watch.Start();
            //engineRoot.MapLoader.Initialize();
            //watch.Stop();
            //Console.WriteLine("Initializing: " + watch.ElapsedMilliseconds);

            //watch.Restart();
            //IMapEdit map = engineRoot.MapLoader.CreateMap(tileset, "HighGrass", new RCIntVector(64, 64));
            //watch.Stop();
            //Console.WriteLine("Creating new map: " + watch.ElapsedMilliseconds);

            //IIsoTile tile = map.GetQuadTile(new RCIntVector(1, 0)).IsoTile;
            //map.DrawTerrain(tile, map.Tileset.GetTerrainType("Structure"));

            Console.WriteLine("Ready!");
            Console.ReadLine();
        }

        static void DrawPolygon(Polygon polygon, Graphics gc, Pen pen)
        {
            RCNumVector prevPoint = RCNumVector.Undefined;
            for (int i = 0; i < polygon.VertexCount; i++)
            {
                RCNumVector currPoint = (polygon[i] + new RCNumVector((RCNumber)1 / (RCNumber)2, (RCNumber)1 / (RCNumber)2)) * new RCNumVector(CELL_SIZE, CELL_SIZE);
                if (prevPoint != RCNumVector.Undefined)
                {
                    gc.DrawLine(pen, prevPoint.Round().X, prevPoint.Round().Y, currPoint.Round().X, currPoint.Round().Y);
                }
                prevPoint = currPoint;
            }

            RCNumVector lastPoint = (polygon[polygon.VertexCount - 1] + new RCNumVector((RCNumber)1 / (RCNumber)2, (RCNumber)1 / (RCNumber)2)) * new RCNumVector(CELL_SIZE, CELL_SIZE);
            RCNumVector firstPoint = (polygon[0] + new RCNumVector((RCNumber)1 / (RCNumber)2, (RCNumber)1 / (RCNumber)2)) * new RCNumVector(CELL_SIZE, CELL_SIZE);
            gc.DrawLine(pen, lastPoint.Round().X, lastPoint.Round().Y, firstPoint.Round().X, firstPoint.Round().Y);
        }

        static void DrawNeighbourArrows(NavMeshNode node, Graphics gc)
        {
            Pen oneWayPen = new Pen(Brushes.Yellow);
            Pen biDirPen = new Pen(Brushes.Blue);
            //GraphicsPath capPath = new GraphicsPath();
            //capPath.AddLine(-20, 0, 20, 0);
            //capPath.AddLine(-20, 0, 0, 20);
            //capPath.AddLine(0, 20, 20, 0);

            //arrowPen.CustomEndCap = new CustomLineCap(null, capPath);

            RCNumVector nodeCenter = new RCNumVector(0, 0);
            for (int i = 0; i < node.Polygon.VertexCount; i++) { nodeCenter += node.Polygon[i]; }
            nodeCenter /= node.Polygon.VertexCount;
            nodeCenter *= new RCNumVector(CELL_SIZE, CELL_SIZE);

            foreach (NavMeshNode neighbour in node.Neighbours)
            {
                bool isBidirectional = false;
                foreach (NavMeshNode neighbourOfNeighbour in neighbour.Neighbours) { if (neighbourOfNeighbour == node) { isBidirectional = true; break; } }
                RCNumVector neighbourCenter = new RCNumVector(0, 0);
                for (int i = 0; i < neighbour.Polygon.VertexCount; i++) { neighbourCenter += neighbour.Polygon[i]; }
                neighbourCenter /= neighbour.Polygon.VertexCount;
                neighbourCenter *= new RCNumVector(CELL_SIZE, CELL_SIZE);
                gc.DrawLine(isBidirectional ? biDirPen : oneWayPen, nodeCenter.Round().X, nodeCenter.Round().Y, neighbourCenter.Round().X, neighbourCenter.Round().Y);
            }
            //capPath.Dispose();
            oneWayPen.Dispose();
        }

        static void TestSimulationHeap()
        {
            List<HeapType> testMetadata = new List<HeapType>()
            {
                new HeapType("Unit", new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("HitPoints", "short"),
                    new KeyValuePair<string, string>("TestArray", "int*"),
                    new KeyValuePair<string, string>("TestPtrArray", "Building**"),
                    new KeyValuePair<string, string>("TestPtr", "Building*"),
                }),
                new HeapType("Building", new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("HitPoints", "short"),
                    new KeyValuePair<string, string>("BuildStatus", "short"),
                }),
            };

            IHeapManagerInternals heapMgr = new HeapManager(testMetadata);

            IHeapType unitType = heapMgr.GetHeapType("Unit");
            int UNIT_HP_IDX = unitType.GetFieldIdx("HitPoints");
            short UNIT_HP_TID = unitType.GetFieldTypeID("HitPoints");
            int UNIT_TESTARRAY_IDX = unitType.GetFieldIdx("TestArray");
            short UNIT_TESTARRAY_TID = unitType.GetFieldTypeID("TestArray");
            int UNIT_TESTPTRARRAY_IDX = unitType.GetFieldIdx("TestPtrArray");
            short UNIT_TESTPTRARRAY_TID = unitType.GetFieldTypeID("TestPtrArray");
            int UNIT_TESTPTR_IDX = unitType.GetFieldIdx("TestPtr");
            short UNIT_TESTPTR_TID = unitType.GetFieldTypeID("TestPtr");

            IHeapType buildingType = heapMgr.GetHeapType("Building");
            int BUILDING_HP_IDX = buildingType.GetFieldIdx("HitPoints");
            short BUILDING_HP_TID = buildingType.GetFieldTypeID("HitPoints");
            int BUILDING_BUILDSTATUS_IDX = buildingType.GetFieldIdx("BuildStatus");
            short BUILDING_BUILDSTATUS_TID = buildingType.GetFieldTypeID("BuildStatus");

            Stopwatch watch = new Stopwatch();
            watch.Start();

            for (int j = 0; j < 100000; j++)
            {
                IHeapConnector unit = heapMgr.New(unitType.ID);
                IHeapConnector building0 = heapMgr.New(buildingType.ID);
                IHeapConnector building1 = heapMgr.New(buildingType.ID);

                ((IValueWrite<short>)building0.AccessField(BUILDING_HP_IDX)).Write(100);
                ((IValueWrite<short>)building0.AccessField(BUILDING_BUILDSTATUS_IDX)).Write(50);
                ((IValueWrite<short>)building1.AccessField(BUILDING_HP_IDX)).Write(50);
                ((IValueWrite<short>)building1.AccessField(BUILDING_BUILDSTATUS_IDX)).Write(100);

                ((IValueWrite<short>)unit.AccessField(UNIT_HP_IDX)).Write(88);
                unit.AccessField(UNIT_TESTPTR_IDX).PointTo(building0);

                unit.AccessField(UNIT_TESTARRAY_IDX).PointTo(heapMgr.NewArray(heapMgr.GetHeapType("int").ID, 5));
                for (int i = 0; i < 5; ++i)
                {
                    ((IValueWrite<int>)unit.AccessField(UNIT_TESTARRAY_IDX).Dereference().AccessArrayItem(i)).Write(i);
                }

                unit.AccessField(UNIT_TESTPTRARRAY_IDX).PointTo(heapMgr.NewArray(heapMgr.GetHeapType("Building*").ID, 5));
                unit.AccessField(UNIT_TESTPTRARRAY_IDX).Dereference().AccessArrayItem(0).PointTo(building0);
                unit.AccessField(UNIT_TESTPTRARRAY_IDX).Dereference().AccessArrayItem(1).PointTo(building1);

                unit.AccessField(UNIT_TESTARRAY_IDX).Dereference().DeleteArray();
                unit.AccessField(UNIT_TESTPTRARRAY_IDX).Dereference().DeleteArray();
                unit.Delete();
                building0.Delete();
                building1.Delete();
            }

            watch.Stop();
            // TODO: test heap saving/loading
        }

        static void DrawFlood(FloodArea flood, string fileName)
        {
            Bitmap target = new Bitmap(2 * (2 * flood.CurrentRadius + 1), 2 * (2 * flood.CurrentRadius + 1), PixelFormat.Format24bppRgb);
            foreach (FloodItem item in flood)
            {
                RCIntVector coords = 2 * (item.Coordinates + new RCIntVector(flood.CurrentRadius, flood.CurrentRadius));
                bool[] pixels = null;
                if (item.Combination == TerrainCombination.Simple) { pixels = new bool[4] { true, true, true, true }; }
                if (item.Combination == TerrainCombination.AAAB) { pixels = new bool[4] { false, false, false, true }; }
                if (item.Combination == TerrainCombination.AABA) { pixels = new bool[4] { false, false, true, false }; }
                if (item.Combination == TerrainCombination.AABB) { pixels = new bool[4] { false, false, true, true }; }
                if (item.Combination == TerrainCombination.ABAA) { pixels = new bool[4] { false, true, false, false }; }
                if (item.Combination == TerrainCombination.ABBA) { pixels = new bool[4] { false, true, true, false }; }
                if (item.Combination == TerrainCombination.ABBB) { pixels = new bool[4] { false, true, true, true }; }
                if (item.Combination == TerrainCombination.BAAA) { pixels = new bool[4] { true, false, false, false }; }
                if (item.Combination == TerrainCombination.BAAB) { pixels = new bool[4] { true, false, false, true }; }
                if (item.Combination == TerrainCombination.BABB) { pixels = new bool[4] { true, false, true, true }; }
                if (item.Combination == TerrainCombination.BBAA) { pixels = new bool[4] { true, true, false, false }; }
                if (item.Combination == TerrainCombination.BBAB) { pixels = new bool[4] { true, true, false, true }; }
                if (item.Combination == TerrainCombination.BBBA) { pixels = new bool[4] { true, true, true, false }; }

                target.SetPixel(coords.X, coords.Y, pixels[0] ? Color.Green : Color.Red);
                target.SetPixel(coords.X + 1, coords.Y, pixels[1] ? Color.Green : Color.Red);
                target.SetPixel(coords.X + 1, coords.Y + 1, pixels[2] ? Color.Green : Color.Red);
                target.SetPixel(coords.X, coords.Y + 1, pixels[3] ? Color.Green : Color.Red);
            }
            target.Save(fileName, ImageFormat.Png);
            target.Dispose();
        }
    }
}
