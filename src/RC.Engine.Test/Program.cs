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

namespace RC.Engine.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigurationManager.Initialize("../../../../config/RC.Engine.Test/RC.Engine.Test.root");
            bool testResult = SimulationHeapTest.StressTest();
            if (!testResult) { throw new Exception("Test failed!"); }
            PFTreeTest.PFTreeNeighbourTest();
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

            IHeapManager heapMgr = new HeapManager(testMetadata);

            short UNIT_TID = heapMgr.GetTypeID("Unit");
            int UNIT_HP_IDX = heapMgr.GetFieldIdx(UNIT_TID, "HitPoints");
            short UNIT_HP_TID = heapMgr.GetFieldTypeID(UNIT_TID, UNIT_HP_IDX);
            int UNIT_TESTARRAY_IDX = heapMgr.GetFieldIdx(UNIT_TID, "TestArray");
            short UNIT_TESTARRAY_TID = heapMgr.GetFieldTypeID(UNIT_TID, UNIT_TESTARRAY_IDX);
            int UNIT_TESTPTRARRAY_IDX = heapMgr.GetFieldIdx(UNIT_TID, "TestPtrArray");
            short UNIT_TESTPTRARRAY_TID = heapMgr.GetFieldTypeID(UNIT_TID, UNIT_TESTPTRARRAY_IDX);
            int UNIT_TESTPTR_IDX = heapMgr.GetFieldIdx(UNIT_TID, "TestPtr");
            short UNIT_TESTPTR_TID = heapMgr.GetFieldTypeID(UNIT_TID, UNIT_TESTPTR_IDX);

            short BUILDING_TID = heapMgr.GetTypeID("Building");
            int BUILDING_HP_IDX = heapMgr.GetFieldIdx(BUILDING_TID, "HitPoints");
            short BUILDING_HP_TID = heapMgr.GetFieldTypeID(BUILDING_TID, BUILDING_HP_IDX);
            int BUILDING_BUILDSTATUS_IDX = heapMgr.GetFieldIdx(BUILDING_TID, "BuildStatus");
            short BUILDING_BUILDSTATUS_TID = heapMgr.GetFieldTypeID(BUILDING_TID, BUILDING_BUILDSTATUS_IDX);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            for (int j = 0; j < 100000; j++)
            {
                IHeapData unit = heapMgr.New(UNIT_TID);
                IHeapData building0 = heapMgr.New(BUILDING_TID);
                IHeapData building1 = heapMgr.New(BUILDING_TID);

                ((IValueWrite<short>)building0.AccessField(BUILDING_HP_IDX)).Write(100);
                ((IValueWrite<short>)building0.AccessField(BUILDING_BUILDSTATUS_IDX)).Write(50);
                ((IValueWrite<short>)building1.AccessField(BUILDING_HP_IDX)).Write(50);
                ((IValueWrite<short>)building1.AccessField(BUILDING_BUILDSTATUS_IDX)).Write(100);

                ((IValueWrite<short>)unit.AccessField(UNIT_HP_IDX)).Write(88);
                unit.AccessField(UNIT_TESTPTR_IDX).PointTo(building0);

                unit.AccessField(UNIT_TESTARRAY_IDX).PointTo(heapMgr.NewArray(heapMgr.GetTypeID("int"), 5));
                for (int i = 0; i < 5; ++i)
                {
                    ((IValueWrite<int>)unit.AccessField(UNIT_TESTARRAY_IDX).Dereference().AccessArrayItem(i)).Write(i);
                }

                unit.AccessField(UNIT_TESTPTRARRAY_IDX).PointTo(heapMgr.NewArray(heapMgr.GetTypeID("Building*"), 5));
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
