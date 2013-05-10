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
            TestSimulationHeap();
            //ConfigurationManager.Initialize("../../../../config/RC.Engine.Test/RC.Engine.Test.root");            

            //ComponentManager.RegisterComponents("RC.Engine.Test, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
            //                                    new string[3] { "C0", "C1", "C2" });
            //ComponentManager.StartComponents();
            //A0 a0 = ComponentManager.GetInterface<A0>();
            //B0 b0 = ComponentManager.GetInterface<B0>();
            //C0 c0 = ComponentManager.GetInterface<C0>();
            //A1 a1 = ComponentManager.GetInterface<A1>();
            //A2 a2 = ComponentManager.GetInterface<A2>();
            //B2 b2 = ComponentManager.GetInterface<B2>();

            //Callback0 callback0 = new Callback0();
            //Callback1 callback1 = new Callback1();
            //ComponentManager.ConnectToComponent<A0>(callback0);
            //ComponentManager.ConnectToComponent<A1>(callback0);
            //ComponentManager.ConnectToComponent<A2>(callback0);
            //ComponentManager.ConnectToComponent<A0>(callback1);
            //ComponentManager.ConnectToComponent<A1>(callback1);
            //ComponentManager.ConnectToComponent<A2>(callback1);

            //ComponentManager.DisconnectFromComponent<A0>(callback0);
            //ComponentManager.DisconnectFromComponent<A1>(callback0);
            //ComponentManager.DisconnectFromComponent<A2>(callback0);
            //ComponentManager.DisconnectFromComponent<A0>(callback1);
            //ComponentManager.DisconnectFromComponent<A1>(callback1);
            //ComponentManager.DisconnectFromComponent<A2>(callback1);

            //ComponentManager.StopComponents();
            //ComponentManager.UnregisterComponents();

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
            Dictionary<string, Dictionary<string, string>> testMetadata = new Dictionary<string, Dictionary<string, string>>()
            {
                {
                    "Unit",
                    new Dictionary<string, string>
                    {
                        { "HitPoints", "short" },
                        { "TestArray", "int*" },
                        { "TestPtrArray", "Building**" },
                        { "TestPtr", "Building*" },
                    }
                },
                {
                    "Building",
                    new Dictionary<string, string>
                    {
                        { "HitPoints", "short" },
                        { "BuildStatus", "short" },
                    }
                }
            };

            ISimulationHeap heap = new SimulationHeap(1024);
            ISimulationHeapMgr heapMgr = new SimulationHeapMgr(heap, testMetadata);

            int UNIT_TID = heapMgr.GetTypeID("Unit");
            int UNIT_HP_IDX = heapMgr.GetFieldIdx(UNIT_TID, "HitPoints");
            int UNIT_HP_TID = heapMgr.GetFieldTypeID(UNIT_TID, UNIT_HP_IDX);
            int UNIT_TESTARRAY_IDX = heapMgr.GetFieldIdx(UNIT_TID, "TestArray");
            int UNIT_TESTARRAY_TID = heapMgr.GetFieldTypeID(UNIT_TID, UNIT_TESTARRAY_IDX);
            int UNIT_TESTPTRARRAY_IDX = heapMgr.GetFieldIdx(UNIT_TID, "TestPtrArray");
            int UNIT_TESTPTRARRAY_TID = heapMgr.GetFieldTypeID(UNIT_TID, UNIT_TESTPTRARRAY_IDX);
            int UNIT_TESTPTR_IDX = heapMgr.GetFieldIdx(UNIT_TID, "TestPtr");
            int UNIT_TESTPTR_TID = heapMgr.GetFieldTypeID(UNIT_TID, UNIT_TESTPTR_IDX);

            int BUILDING_TID = heapMgr.GetTypeID("Building");
            int BUILDING_HP_IDX = heapMgr.GetFieldIdx(BUILDING_TID, "HitPoints");
            int BUILDING_HP_TID = heapMgr.GetFieldTypeID(BUILDING_TID, BUILDING_HP_IDX);
            int BUILDING_BUILDSTATUS_IDX = heapMgr.GetFieldIdx(BUILDING_TID, "BuildStatus");
            int BUILDING_BUILDSTATUS_TID = heapMgr.GetFieldTypeID(BUILDING_TID, BUILDING_BUILDSTATUS_IDX);

            ISimElement unit = heapMgr.New(UNIT_TID);
            ISimElement building0 = heapMgr.New(BUILDING_TID);
            ISimElement building1 = heapMgr.New(BUILDING_TID);

            building0.AccessField(BUILDING_HP_IDX).Write<short>(100);
            building0.AccessField(BUILDING_BUILDSTATUS_IDX).Write<short>(50);
            building1.AccessField(BUILDING_HP_IDX).Write<short>(50);
            building1.AccessField(BUILDING_BUILDSTATUS_IDX).Write<short>(100);

            unit.AccessField(UNIT_HP_IDX).Write<short>(88);
            unit.AccessField(UNIT_TESTPTR_IDX).PointTo(building0);

            unit.AccessField(UNIT_TESTARRAY_IDX).PointTo(heapMgr.NewArray(heapMgr.GetTypeID("System.Int32"), 5));
            for (int i = 0; i < 5; ++i)
            {
                unit.AccessField(UNIT_TESTARRAY_IDX).Dereference(i).Write<int>(i);
            }

            unit.AccessField(UNIT_TESTPTRARRAY_IDX).PointTo(heapMgr.NewArray(heapMgr.GetTypeID("Building*"), 5));
            unit.AccessField(UNIT_TESTPTRARRAY_IDX).Dereference(0).PointTo(building0);
            unit.AccessField(UNIT_TESTPTRARRAY_IDX).Dereference(1).PointTo(building1);

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
