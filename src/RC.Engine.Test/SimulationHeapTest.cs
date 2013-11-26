using System;
using System.Collections.Generic;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Core;
using RC.Common;
using System.Diagnostics;
using System.Reflection;
using RC.Engine.Simulator.InternalInterfaces;
using RC.Engine.Simulator.ComponentInterfaces;

namespace RC.Engine.Test
{
    static class SimulationHeapTest
    {
        public static bool StressTest()
        {
            HeapManager heapMgr = new HeapManager(testMetadata);
            heapMgr.CreateHeap();
            GetIDs(heapMgr);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            IHeapConnector prevObj = null;

            IHeapConnector[] allObjects = new IHeapConnector[100];
            for (int i = 0; i < 100; i++)
            {
                IHeapConnector currObj = CreateTestObj(heapMgr);
                allObjects[i] = currObj;
                if (prevObj != null) { prevObj.AccessField(TESTTYPE_NEXT_IDX).PointTo(currObj); }
                prevObj = currObj;
            }

            //byte[] savedHeap = heapMgr.UnloadHeap(new List<IHeapConnector>() { allObjects[0] });

            //List<IHeapConnector> savedRefs = heapMgr.LoadHeap(savedHeap);
            //if (savedRefs.Count != 1) { throw new Exception("Load error!"); }

            //IHeapConnector curr = savedRefs[0];
            //int objIdx = 0;
            //do
            //{
            //    allObjects[objIdx] = curr;
            //    objIdx++;
            //    CheckTestObj(curr);
            //    curr = curr.AccessField(TESTTYPE_NEXT_IDX).Dereference();
            //} while (curr != null);

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

            FieldInfo freeSectionsHeadFI = typeof(HeapManager).GetField("freeSectionsHead", BindingFlags.NonPublic | BindingFlags.Instance);
            HeapSection freeSectionsHead = (HeapSection)freeSectionsHeadFI.GetValue(heapMgr);

            return freeSectionsHead.Address == 4 &&
                   freeSectionsHead.Length == -1 &&
                   freeSectionsHead.Next == null &&
                   freeSectionsHead.Prev == null;
        }

        private static IHeapConnector CreateTestObj(IHeapManagerInternals heapMgr)
        {
            IHeapConnector retObj = heapMgr.New(testType.ID);
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

        private static void DeleteTestObj(IHeapConnector obj)
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

        private static void CheckTestObj(IHeapConnector obj)
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

        private static IHeapConnector CreateByteArray(IHeapManagerInternals heapMgr, int count)
        {
            IHeapConnector retObj = heapMgr.NewArray(heapMgr.GetHeapType("byte").ID, count);
            for (int i = 0; i < count; i++)
            {
                ((IValueWrite<byte>)retObj.AccessArrayItem(i)).Write((byte)i);
            }
            return retObj;
        }

        private static IHeapConnector CreateShortArray(IHeapManagerInternals heapMgr, int count)
        {
            IHeapConnector retObj = heapMgr.NewArray(heapMgr.GetHeapType("short").ID, count);
            for (int i = 0; i < count; i++)
            {
                ((IValueWrite<short>)retObj.AccessArrayItem(i)).Write((short)i);
            }
            return retObj;
        }

        private static IHeapConnector CreateIntArray(IHeapManagerInternals heapMgr, int count)
        {
            IHeapConnector retObj = heapMgr.NewArray(heapMgr.GetHeapType("int").ID, count);
            for (int i = 0; i < count; i++)
            {
                ((IValueWrite<int>)retObj.AccessArrayItem(i)).Write(i);
            }
            return retObj;
        }

        private static IHeapConnector CreateLongArray(IHeapManagerInternals heapMgr, int count)
        {
            IHeapConnector retObj = heapMgr.NewArray(heapMgr.GetHeapType("long").ID, count);
            for (int i = 0; i < count; i++)
            {
                ((IValueWrite<long>)retObj.AccessArrayItem(i)).Write(i);
            }
            return retObj;
        }

        private static IHeapConnector CreateNumArray(IHeapManagerInternals heapMgr, int count)
        {
            IHeapConnector retObj = heapMgr.NewArray(heapMgr.GetHeapType("num").ID, count);
            for (int i = 0; i < count; i++)
            {
                ((IValueWrite<RCNumber>)retObj.AccessArrayItem(i)).Write((RCNumber)i);
            }
            return retObj;
        }

        private static IHeapConnector CreateIntVectArray(IHeapManagerInternals heapMgr, int count)
        {
            IHeapConnector retObj = heapMgr.NewArray(heapMgr.GetHeapType("intvect").ID, count);
            for (int i = 0; i < count; i++)
            {
                ((IValueWrite<RCIntVector>)retObj.AccessArrayItem(i)).Write(new RCIntVector(i, i+1));
            }
            return retObj;
        }

        private static IHeapConnector CreateNumVectArray(IHeapManagerInternals heapMgr, int count)
        {
            IHeapConnector retObj = heapMgr.NewArray(heapMgr.GetHeapType("numvect").ID, count);
            for (int i = 0; i < count; i++)
            {
                ((IValueWrite<RCNumVector>)retObj.AccessArrayItem(i)).Write(new RCNumVector(i, i + 1));
            }
            return retObj;
        }

        private static IHeapConnector CreateIntRectArray(IHeapManagerInternals heapMgr, int count)
        {
            IHeapConnector retObj = heapMgr.NewArray(heapMgr.GetHeapType("intrect").ID, count);
            for (int i = 0; i < count; i++)
            {
                ((IValueWrite<RCIntRectangle>)retObj.AccessArrayItem(i)).Write(new RCIntRectangle(i, i + 1, i + 2, i + 3));
            }
            return retObj;
        }

        private static IHeapConnector CreateNumRectArray(IHeapManagerInternals heapMgr, int count)
        {
            IHeapConnector retObj = heapMgr.NewArray(heapMgr.GetHeapType("numrect").ID, count);
            for (int i = 0; i < count; i++)
            {
                ((IValueWrite<RCNumRectangle>)retObj.AccessArrayItem(i)).Write(new RCNumRectangle(i, i + 1, i + 2, i + 3));
            }
            return retObj;
        }

        private static void CheckByteArray(IHeapConnector arrayRef, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (((IValueRead<byte>)arrayRef.AccessArrayItem(i)).Read() != (byte)i) { throw new Exception("Mismatch!"); }
            }
        }

        private static void CheckShortArray(IHeapConnector arrayRef, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (((IValueRead<short>)arrayRef.AccessArrayItem(i)).Read() != (short)i) { throw new Exception("Mismatch!"); }
            }
        }

        private static void CheckIntArray(IHeapConnector arrayRef, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (((IValueRead<int>)arrayRef.AccessArrayItem(i)).Read() != i) { throw new Exception("Mismatch!"); }
            }
        }

        private static void CheckLongArray(IHeapConnector arrayRef, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (((IValueRead<long>)arrayRef.AccessArrayItem(i)).Read() != i) { throw new Exception("Mismatch!"); }
            }
        }

        private static void CheckNumArray(IHeapConnector arrayRef, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (((IValueRead<RCNumber>)arrayRef.AccessArrayItem(i)).Read() != i) { throw new Exception("Mismatch!"); }
            }
        }

        private static void CheckIntVectArray(IHeapConnector arrayRef, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (((IValueRead<RCIntVector>)arrayRef.AccessArrayItem(i)).Read() != new RCIntVector(i, i + 1)) { throw new Exception("Mismatch!"); }
            }
        }

        private static void CheckNumVectArray(IHeapConnector arrayRef, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (((IValueRead<RCNumVector>)arrayRef.AccessArrayItem(i)).Read() != new RCNumVector(i, i + 1)) { throw new Exception("Mismatch!"); }
            }
        }

        private static void CheckIntRectArray(IHeapConnector arrayRef, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (((IValueRead<RCIntRectangle>)arrayRef.AccessArrayItem(i)).Read() != new RCIntRectangle(i, i + 1, i + 2, i + 3)) { throw new Exception("Mismatch!"); }
            }
        }

        private static void CheckNumRectArray(IHeapConnector arrayRef, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (((IValueRead<RCNumRectangle>)arrayRef.AccessArrayItem(i)).Read() != new RCNumRectangle(i, i + 1, i + 2, i + 3)) { throw new Exception("Mismatch!"); }
            }
        }

        private static void GetIDs(IHeapManagerInternals heapMgr)
        {
            testType = heapMgr.GetHeapType("TestType");
            TESTTYPE_BYTEARRAY_IDX = testType.GetFieldIdx("ByteArray");
            TESTTYPE_SHORTARRAY_IDX = testType.GetFieldIdx("ShortArray");
            TESTTYPE_INTARRAY_IDX = testType.GetFieldIdx("IntArray");
            TESTTYPE_LONGARRAY_IDX = testType.GetFieldIdx("LongArray");
            TESTTYPE_NUMARRAY_IDX = testType.GetFieldIdx("NumArray");
            TESTTYPE_INTVECTARRAY_IDX = testType.GetFieldIdx("IntVectArray");
            TESTTYPE_NUMVECTARRAY_IDX = testType.GetFieldIdx("NumVectArray");
            TESTTYPE_INTRECTARRAY_IDX = testType.GetFieldIdx("IntRectArray");
            TESTTYPE_NUMRECTARRAY_IDX = testType.GetFieldIdx("NumRectArray");
            TESTTYPE_NEXT_IDX = testType.GetFieldIdx("Next");
        }

        private static List<HeapType> testMetadata = new List<HeapType>()
        {
            new HeapType("TestType", new List<KeyValuePair<string, string>>()
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

        private static IHeapType testType;
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
