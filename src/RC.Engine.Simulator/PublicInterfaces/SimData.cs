using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace RC.Engine.Simulator.PublicInterfaces
{
    [StructLayout(LayoutKind.Explicit)]
    public struct SimData
    {
        private struct VectLayout
        {
            public int X;
            public int Y;
        }

        private struct RectLayout
        {
            public int X;
            public int Y;
            public int Width;
            public int Height;
        }

        [FieldOffset(0)]
        private byte byteData;
        [FieldOffset(0)]
        private short shortData;
        [FieldOffset(0)]
        private int intData;
        [FieldOffset(0)]
        private long longData;
        [FieldOffset(0)]
        private int numData;
        [FieldOffset(0)]
        private VectLayout intVectData;
        [FieldOffset(0)]
        private VectLayout numVectData;
        [FieldOffset(0)]
        private RectLayout intRectData;
        [FieldOffset(0)]
        private RectLayout numRectData;
    }
}
