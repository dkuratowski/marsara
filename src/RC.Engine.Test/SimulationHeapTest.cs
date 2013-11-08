using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Core;
using RC.Common;
using System.Diagnostics;
using System.Reflection;

namespace RC.Engine.Test
{
    static class SimulationHeapTest
    {
        public static bool StressTest()
        {
            ISimulationHeapMgr heapMgr = new SimulationHeapMgr(testMetadata);
            GetIDs(heapMgr);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            ISimHeapAccess prevObj = null;

            ISimHeapAccess[] allObjects = new ISimHeapAccess[100];
            for (int i = 0; i < 100; i++)
            {
                ISimHeapAccess currObj = CreateTestObj(heapMgr);
                allObjects[i] = currObj;
                if (prevObj != null) { prevObj.AccessField(TESTTYPE_NEXT_IDX).PointTo(currObj); }
                prevObj = currObj;
            }

            byte[] savedHeap = heapMgr.SaveState(new List<ISimHeapAccess>() { allObjects[0] });

            List<ISimHeapAccess> savedRefs = heapMgr.LoadState(savedHeap);
            if (savedRefs.Count != 1) { throw new Exception("Load error!"); }

            ISimHeapAccess curr = savedRefs[0];
            int objIdx = 0;
            do
            {
                allObjects[objIdx] = curr;
                objIdx++;
                CheckTestObj(curr);
                curr = curr.AccessField(TESTTYPE_NEXT_IDX).Dereference();
            } while (curr != null);

            for (int i = 0; i < 50; i++)
            {
                DeleteTestObj(allObjects[2 * i + 1]);
                allObjects[2 * i].AccessField(TESTTYPE_NEXT_IDX).PointTo(i < 49 ? allObjects[2 * i + 2] : null);
            }

            for (int i = 0; i < 50; i++)
            {
                DeleteTestObj(allObjects[2 * i]);
            }

            watch.Stop();

            FieldInfo freeSectionsHeadFI = typeof(SimulationHeapMgr).GetField("freeSectionsHead", BindingFlags.NonPublic | BindingFlags.Instance);
            SimHeapSection freeSectionsHead = (SimHeapSection)freeSectionsHeadFI.GetValue(heapMgr);

            return freeSectionsHead.Address == 4 &&
                   freeSectionsHead.Length == -1 &&
                   freeSectionsHead.Next == null &&
                   freeSectionsHead.Prev == null;
        }

        private static ISimHeapAccess CreateTestObj(ISimulationHeapMgr heapMgr)
        {
            ISimHeapAccess retObj = heapMgr.New(TESTTYPE_ID);
            retObj.AccessField(TESTTYPE_BYTEARRAY_IDX).PointTo(CreateByteArray(heapMgr, 1));
            retObj.AccessField(TESTTYPE_SHORTARRAY_IDX).PointTo(CreateShortArray(heapMgr, 2));
            retObj.AccessField(TESTTYPE_INTARRAY_IDX).PointTo(CreateIntArray(heapMgr, 3));
            retObj.AccessField(TESTTYPE_LONGARRAY_IDX).PointTo(CreateLongArray(heapMgr, 4));
            retObj.AccessField(TESTTYPE_NUMARRAY_IDX).PointTo(CreateNumArray(heapMgr, 5));
            retObj.AccessField(TESTTYPE_INTVECTARRAY_IDX).PointTo(CreateIntVectArray(heapMgr, 6));
            retObj.AccessField(TESTTYPE_NUMVECTARRAY_IDX).PointTo(CreateNumVectArray(heapMgr, 7));
            retObj.AccessField(TESTTYPE_INTRECTARRAY_IDX).PointTo(CreateIntRectArray(heapMgr, 8));
            retObj.AccessField(TESTTYPE_NUMRECTARRAY_IDX).PointTo(CreateNumRectArray(heapMgr, 9));
            return retObj;
        }

        private static void DeleteTestObj(ISimHeapAccess obj)
        {
            obj.AccessField(TESTTYPE_BYTEARRAY_IDX).Dereference().DeleteArray();
            obj.AccessField(TESTTYPE_SHORTARRAY_IDX).Dereference().DeleteArray();
            obj.AccessField(TESTTYPE_INTARRAY_IDX).Dereference().DeleteArray();
            obj.AccessField(TESTTYPE_LONGARRAY_IDX).Dereference().DeleteArray();
            obj.AccessField(TESTTYPE_NUMARRAY_IDX).Dereference().DeleteArray();
            obj.AccessField(TESTTYPE_INTVECTARRAY_IDX).Dereference().DeleteArray();
            obj.AccessField(TESTTYPE_NUMVECTARRAY_IDX).Dereference().DeleteArray();
            obj.AccessField(TESTTYPE_INTRECTARRAY_IDX).Dereference().DeleteArray();
            obj.AccessField(TESTTYPE_NUMRECTARRAY_IDX).Dereference().DeleteArray();
            obj.Delete();
        }

        private static void CheckTestObj(ISimHeapAccess obj)
        {
            CheckByteArray(obj.AccessField(TESTTYPE_BYTEARRAY_IDX).Dereference(), 1);
            CheckShortArray(obj.AccessField(TESTTYPE_SHORTARRAY_IDX).Dereference(), 2);
            CheckIntArray(obj.AccessField(TESTTYPE_INTARRAY_IDX).Dereference(),3);
            CheckLongArray(obj.AccessField(TESTTYPE_LONGARRAY_IDX).Dereference(), 4);
            CheckNumArray(obj.AccessField(TESTTYPE_NUMARRAY_IDX).Dereference(), 5);
            CheckIntVectArray(obj.AccessField(TESTTYPE_INTVECTARRAY_IDX).Dereference(), 6);
            CheckNumVectArray(obj.AccessField(TESTTYPE_NUMVECTARRAY_IDX).Dereference(), 7);
            CheckIntRectArray(obj.AccessField(TESTTYPE_INTRECTARRAY_IDX).Dereference(), 8);
            CheckNumRectArray(obj.AccessField(TESTTYPE_NUMRECTARRAY_IDX).Dereference(), 9);
        }

        private static ISimHeapAccess CreateByteArray(ISimulationHeapMgr heapMgr, int count)
        {
            ISimHeapAccess retObj = heapMgr.NewArray(heapMgr.GetTypeID("byte"), count);
            for (int i = 0; i < count; i++)
            {
                retObj.AccessArrayItem(i).WriteByte((byte)i);
            }
            return retObj;
        }

        private static ISimHeapAccess CreateShortArray(ISimulationHeapMgr heapMgr, int count)
        {
            ISimHeapAccess retObj = heapMgr.NewArray(heapMgr.GetTypeID("short"), count);
            for (int i = 0; i < count; i++)
            {
                retObj.AccessArrayItem(i).WriteShort((short)i);
            }
            return retObj;
        }

        private static ISimHeapAccess CreateIntArray(ISimulationHeapMgr heapMgr, int count)
        {
            ISimHeapAccess retObj = heapMgr.NewArray(heapMgr.GetTypeID("int"), count);
            for (int i = 0; i < count; i++)
            {
                retObj.AccessArrayItem(i).WriteInt(i);
            }
            return retObj;
        }

        private static ISimHeapAccess CreateLongArray(ISimulationHeapMgr heapMgr, int count)
        {
            ISimHeapAccess retObj = heapMgr.NewArray(heapMgr.GetTypeID("long"), count);
            for (int i = 0; i < count; i++)
            {
                retObj.AccessArrayItem(i).WriteLong(i);
            }
            return retObj;
        }

        private static ISimHeapAccess CreateNumArray(ISimulationHeapMgr heapMgr, int count)
        {
            ISimHeapAccess retObj = heapMgr.NewArray(heapMgr.GetTypeID("num"), count);
            for (int i = 0; i < count; i++)
            {
                retObj.AccessArrayItem(i).WriteNumber((RCNumber)i);
            }
            return retObj;
        }

        private static ISimHeapAccess CreateIntVectArray(ISimulationHeapMgr heapMgr, int count)
        {
            ISimHeapAccess retObj = heapMgr.NewArray(heapMgr.GetTypeID("intvect"), count);
            for (int i = 0; i < count; i++)
            {
                retObj.AccessArrayItem(i).WriteIntVector(new RCIntVector(i, i+1));
            }
            return retObj;
        }

        private static ISimHeapAccess CreateNumVectArray(ISimulationHeapMgr heapMgr, int count)
        {
            ISimHeapAccess retObj = heapMgr.NewArray(heapMgr.GetTypeID("numvect"), count);
            for (int i = 0; i < count; i++)
            {
                retObj.AccessArrayItem(i).WriteNumVector(new RCNumVector(i, i + 1));
            }
            return retObj;
        }

        private static ISimHeapAccess CreateIntRectArray(ISimulationHeapMgr heapMgr, int count)
        {
            ISimHeapAccess retObj = heapMgr.NewArray(heapMgr.GetTypeID("intrect"), count);
            for (int i = 0; i < count; i++)
            {
                retObj.AccessArrayItem(i).WriteIntRectangle(new RCIntRectangle(i, i + 1, i + 2, i + 3));
            }
            return retObj;
        }

        private static ISimHeapAccess CreateNumRectArray(ISimulationHeapMgr heapMgr, int count)
        {
            ISimHeapAccess retObj = heapMgr.NewArray(heapMgr.GetTypeID("numrect"), count);
            for (int i = 0; i < count; i++)
            {
                retObj.AccessArrayItem(i).WriteNumRectangle(new RCNumRectangle(i, i + 1, i + 2, i + 3));
            }
            return retObj;
        }

        private static void CheckByteArray(ISimHeapAccess arrayRef, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (arrayRef.AccessArrayItem(i).ReadByte() != (byte)i) { throw new Exception("Mismatch!"); }
            }
        }

        private static void CheckShortArray(ISimHeapAccess arrayRef, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (arrayRef.AccessArrayItem(i).ReadShort() != (short)i) { throw new Exception("Mismatch!"); }
            }
        }

        private static void CheckIntArray(ISimHeapAccess arrayRef, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (arrayRef.AccessArrayItem(i).ReadInt() != i) { throw new Exception("Mismatch!"); }
            }
        }

        private static void CheckLongArray(ISimHeapAccess arrayRef, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (arrayRef.AccessArrayItem(i).ReadLong() != i) { throw new Exception("Mismatch!"); }
            }
        }

        private static void CheckNumArray(ISimHeapAccess arrayRef, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (arrayRef.AccessArrayItem(i).ReadNumber() != i) { throw new Exception("Mismatch!"); }
            }
        }

        private static void CheckIntVectArray(ISimHeapAccess arrayRef, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (arrayRef.AccessArrayItem(i).ReadIntVector() != new RCIntVector(i, i + 1)) { throw new Exception("Mismatch!"); }
            }
        }

        private static void CheckNumVectArray(ISimHeapAccess arrayRef, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (arrayRef.AccessArrayItem(i).ReadNumVector() != new RCNumVector(i, i + 1)) { throw new Exception("Mismatch!"); }
            }
        }

        private static void CheckIntRectArray(ISimHeapAccess arrayRef, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (arrayRef.AccessArrayItem(i).ReadIntRectangle() != new RCIntRectangle(i, i + 1, i + 2, i + 3)) { throw new Exception("Mismatch!"); }
            }
        }

        private static void CheckNumRectArray(ISimHeapAccess arrayRef, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (arrayRef.AccessArrayItem(i).ReadNumRectangle() != new RCNumRectangle(i, i + 1, i + 2, i + 3)) { throw new Exception("Mismatch!"); }
            }
        }

        private static void GetIDs(ISimulationHeapMgr heapMgr)
        {
            TESTTYPE_ID = heapMgr.GetTypeID("TestType");
            TESTTYPE_BYTEARRAY_IDX = heapMgr.GetFieldIdx(TESTTYPE_ID, "ByteArray");
            TESTTYPE_SHORTARRAY_IDX = heapMgr.GetFieldIdx(TESTTYPE_ID, "ShortArray");
            TESTTYPE_INTARRAY_IDX = heapMgr.GetFieldIdx(TESTTYPE_ID, "IntArray");
            TESTTYPE_LONGARRAY_IDX = heapMgr.GetFieldIdx(TESTTYPE_ID, "LongArray");
            TESTTYPE_NUMARRAY_IDX = heapMgr.GetFieldIdx(TESTTYPE_ID, "NumArray");
            TESTTYPE_INTVECTARRAY_IDX = heapMgr.GetFieldIdx(TESTTYPE_ID, "IntVectArray");
            TESTTYPE_NUMVECTARRAY_IDX = heapMgr.GetFieldIdx(TESTTYPE_ID, "NumVectArray");
            TESTTYPE_INTRECTARRAY_IDX = heapMgr.GetFieldIdx(TESTTYPE_ID, "IntRectArray");
            TESTTYPE_NUMRECTARRAY_IDX = heapMgr.GetFieldIdx(TESTTYPE_ID, "NumRectArray");
            TESTTYPE_NEXT_IDX = heapMgr.GetFieldIdx(TESTTYPE_ID, "Next");
        }

        private static List<SimHeapType> testMetadata = new List<SimHeapType>()
        {
            new SimHeapType("TestType", new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("ByteArray", "byte*"),
                new KeyValuePair<string, string>("ShortArray", "short*"),
                new KeyValuePair<string, string>("IntArray", "int*"),
                new KeyValuePair<string, string>("LongArray", "long*"),
                new KeyValuePair<string, string>("NumArray", "num*"),
                new KeyValuePair<string, string>("IntVectArray", "intvect*"),
                new KeyValuePair<string, string>("NumVectArray", "numvect*"),
                new KeyValuePair<string, string>("IntRectArray", "intrect*"),
                new KeyValuePair<string, string>("NumRectArray", "numrect*"),
                new KeyValuePair<string, string>("Next", "TestType*"),
            }),
        };

        private static int TESTTYPE_ID;
        private static int TESTTYPE_BYTEARRAY_IDX;
        private static int TESTTYPE_SHORTARRAY_IDX;
        private static int TESTTYPE_INTARRAY_IDX;
        private static int TESTTYPE_LONGARRAY_IDX;
        private static int TESTTYPE_NUMARRAY_IDX;
        private static int TESTTYPE_INTVECTARRAY_IDX;
        private static int TESTTYPE_NUMVECTARRAY_IDX;
        private static int TESTTYPE_INTRECTARRAY_IDX;
        private static int TESTTYPE_NUMRECTARRAY_IDX;
        private static int TESTTYPE_NEXT_IDX;
    }
}
