using RC.Common;
using RC.Engine.Simulator.InternalInterfaces;
using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Class for accessing bytes on the simulation heap.
    /// </summary>
    class HeapByte : HeapData, IValueRead<byte>, IValueWrite<byte>
    {
        public HeapByte(int dataAddress, HeapType dataType, IHeap heap, IHeapDataFactory heapDataFactory, DeallocationFunc deallocFunc)
            : base(dataAddress, dataType, heap, heapDataFactory, deallocFunc)
        {
            if (dataType.BuiltInType != HeapType.BuiltInTypeEnum.Byte) { throw new InvalidOperationException("Invalid heap type!"); }
        }

        #region IValueRead<T> methods

        /// <see cref="IValueRead<T>.Read"/>
        public byte Read()
        {
            return this.Heap.ReadByte(this.DataAddress);
        }

        #endregion IValueRead<T> methods

        #region IValueWrite<T> methods

        /// <see cref="IValueWrite<T>.Write"/>
        public void Write(byte newVal)
        {
            this.Heap.WriteByte(this.DataAddress, newVal);
        }

        #endregion IValueWrite<T> methods
    }

    /// <summary>
    /// Class for accessing shorts on the simulation heap.
    /// </summary>
    class HeapShort : HeapData, IValueRead<short>, IValueWrite<short>
    {
        public HeapShort(int dataAddress, HeapType dataType, IHeap heap, IHeapDataFactory heapDataFactory, DeallocationFunc deallocFunc)
            : base(dataAddress, dataType, heap, heapDataFactory, deallocFunc)
        {
            if (dataType.BuiltInType != HeapType.BuiltInTypeEnum.Short) { throw new InvalidOperationException("Invalid heap type!"); }
        }

        #region IValueRead<T> methods

        /// <see cref="IValueRead<T>.Read"/>
        public short Read()
        {
            return this.Heap.ReadShort(this.DataAddress);
        }

        #endregion IValueRead<T> methods

        #region IValueWrite<T> methods

        /// <see cref="IValueWrite<T>.Write"/>
        public void Write(short newVal)
        {
            this.Heap.WriteShort(this.DataAddress, newVal);
        }

        #endregion IValueWrite<T> methods
    }

    /// <summary>
    /// Class for accessing ints on the simulation heap.
    /// </summary>
    class HeapInt : HeapData, IValueRead<int>, IValueWrite<int>
    {
        public HeapInt(int dataAddress, HeapType dataType, IHeap heap, IHeapDataFactory heapDataFactory, DeallocationFunc deallocFunc)
            : base(dataAddress, dataType, heap, heapDataFactory, deallocFunc)
        {
            if (dataType.BuiltInType != HeapType.BuiltInTypeEnum.Integer) { throw new InvalidOperationException("Invalid heap type!"); }
        }

        #region IValueRead<T> methods

        /// <see cref="IValueRead<T>.Read"/>
        public int Read()
        {
            return this.Heap.ReadInt(this.DataAddress);
        }

        #endregion IValueRead<T> methods

        #region IValueWrite<T> methods

        /// <see cref="IValueWrite<T>.Write"/>
        public void Write(int newVal)
        {
            this.Heap.WriteInt(this.DataAddress, newVal);
        }

        #endregion IValueWrite<T> methods
    }

    /// <summary>
    /// Class for accessing longs on the simulation heap.
    /// </summary>
    class HeapLong : HeapData, IValueRead<long>, IValueWrite<long>
    {
        public HeapLong(int dataAddress, HeapType dataType, IHeap heap, IHeapDataFactory heapDataFactory, DeallocationFunc deallocFunc)
            : base(dataAddress, dataType, heap, heapDataFactory, deallocFunc)
        {
            if (dataType.BuiltInType != HeapType.BuiltInTypeEnum.Long) { throw new InvalidOperationException("Invalid heap type!"); }
        }

        #region IValueRead<T> methods

        /// <see cref="IValueRead<T>.Read"/>
        public long Read()
        {
            return this.Heap.ReadLong(this.DataAddress);
        }

        #endregion IValueRead<T> methods

        #region IValueWrite<T> methods

        /// <see cref="IValueWrite<T>.Write"/>
        public void Write(long newVal)
        {
            this.Heap.WriteLong(this.DataAddress, newVal);
        }

        #endregion IValueWrite<T> methods
    }

    /// <summary>
    /// Class for accessing RCNumbers on the simulation heap.
    /// </summary>
    class HeapNumber : HeapData, IValueRead<RCNumber>, IValueWrite<RCNumber>
    {
        public HeapNumber(int dataAddress, HeapType dataType, IHeap heap, IHeapDataFactory heapDataFactory, DeallocationFunc deallocFunc)
            : base(dataAddress, dataType, heap, heapDataFactory, deallocFunc)
        {
            if (dataType.BuiltInType != HeapType.BuiltInTypeEnum.Number) { throw new InvalidOperationException("Invalid heap type!"); }
        }

        #region IValueRead<T> methods

        /// <see cref="IValueRead<T>.Read"/>
        public RCNumber Read()
        {
            return new RCNumber(this.Heap.ReadInt(this.DataAddress));
        }

        #endregion IValueRead<T> methods

        #region IValueWrite<T> methods

        /// <see cref="IValueWrite<T>.Write"/>
        public void Write(RCNumber newVal)
        {
            this.Heap.WriteInt(this.DataAddress, newVal.Bits);
        }

        #endregion IValueWrite<T> methods
    }

    /// <summary>
    /// Class for accessing RCIntVectors on the simulation heap.
    /// </summary>
    class HeapIntVector : HeapData, IValueRead<RCIntVector>, IValueWrite<RCIntVector>
    {
        public HeapIntVector(int dataAddress, HeapType dataType, IHeap heap, IHeapDataFactory heapDataFactory, DeallocationFunc deallocFunc)
            : base(dataAddress, dataType, heap, heapDataFactory, deallocFunc)
        {
            if (dataType.BuiltInType != HeapType.BuiltInTypeEnum.IntVector) { throw new InvalidOperationException("Invalid heap type!"); }
        }

        #region IValueRead<T> methods

        /// <see cref="IValueRead<T>.Read"/>
        public RCIntVector Read()
        {
            return new RCIntVector(this.Heap.ReadInt(this.DataAddress),
                                   this.Heap.ReadInt(this.DataAddress + 4));
        }

        #endregion IValueRead<T> methods

        #region IValueWrite<T> methods

        /// <see cref="IValueWrite<T>.Write"/>
        public void Write(RCIntVector newVal)
        {
            this.Heap.WriteInt(this.DataAddress, newVal.X);
            this.Heap.WriteInt(this.DataAddress + 4, newVal.Y);
        }

        #endregion IValueWrite<T> methods
    }

    /// <summary>
    /// Class for accessing RCNumVectors on the simulation heap.
    /// </summary>
    class HeapNumVector : HeapData, IValueRead<RCNumVector>, IValueWrite<RCNumVector>
    {
        public HeapNumVector(int dataAddress, HeapType dataType, IHeap heap, IHeapDataFactory heapDataFactory, DeallocationFunc deallocFunc)
            : base(dataAddress, dataType, heap, heapDataFactory, deallocFunc)
        {
            if (dataType.BuiltInType != HeapType.BuiltInTypeEnum.NumVector) { throw new InvalidOperationException("Invalid heap type!"); }
        }

        #region IValueRead<T> methods

        /// <see cref="IValueRead<T>.Read"/>
        public RCNumVector Read()
        {
            return new RCNumVector(new RCNumber(this.Heap.ReadInt(this.DataAddress)),
                                   new RCNumber(this.Heap.ReadInt(this.DataAddress + 4)));
        }

        #endregion IValueRead<T> methods

        #region IValueWrite<T> methods

        /// <see cref="IValueWrite<T>.Write"/>
        public void Write(RCNumVector newVal)
        {
            this.Heap.WriteInt(this.DataAddress, newVal.X.Bits);
            this.Heap.WriteInt(this.DataAddress + 4, newVal.Y.Bits);
        }

        #endregion IValueWrite<T> methods
    }

    /// <summary>
    /// Class for accessing RCIntRectangles on the simulation heap.
    /// </summary>
    class HeapIntRectangle : HeapData, IValueRead<RCIntRectangle>, IValueWrite<RCIntRectangle>
    {
        public HeapIntRectangle(int dataAddress, HeapType dataType, IHeap heap, IHeapDataFactory heapDataFactory, DeallocationFunc deallocFunc)
            : base(dataAddress, dataType, heap, heapDataFactory, deallocFunc)
        {
            if (dataType.BuiltInType != HeapType.BuiltInTypeEnum.IntRectangle) { throw new InvalidOperationException("Invalid heap type!"); }
        }

        #region IValueRead<T> methods

        /// <see cref="IValueRead<T>.Read"/>
        public RCIntRectangle Read()
        {
            return new RCIntRectangle(this.Heap.ReadInt(this.DataAddress),
                                      this.Heap.ReadInt(this.DataAddress + 4),
                                      this.Heap.ReadInt(this.DataAddress + 8),
                                      this.Heap.ReadInt(this.DataAddress + 12));
        }

        #endregion IValueRead<T> methods

        #region IValueWrite<T> methods

        /// <see cref="IValueWrite<T>.Write"/>
        public void Write(RCIntRectangle newVal)
        {
            this.Heap.WriteInt(this.DataAddress, newVal.X);
            this.Heap.WriteInt(this.DataAddress + 4, newVal.Y);
            this.Heap.WriteInt(this.DataAddress + 8, newVal.Width);
            this.Heap.WriteInt(this.DataAddress + 12, newVal.Height);
        }

        #endregion IValueWrite<T> methods
    }

    /// <summary>
    /// Class for accessing RCNumRectangles on the simulation heap.
    /// </summary>
    class HeapNumRectangle : HeapData, IValueRead<RCNumRectangle>, IValueWrite<RCNumRectangle>
    {
        public HeapNumRectangle(int dataAddress, HeapType dataType, IHeap heap, IHeapDataFactory heapDataFactory, DeallocationFunc deallocFunc)
            : base(dataAddress, dataType, heap, heapDataFactory, deallocFunc)
        {
            if (dataType.BuiltInType != HeapType.BuiltInTypeEnum.NumRectangle) { throw new InvalidOperationException("Invalid heap type!"); }
        }

        #region IValueRead<T> methods

        /// <see cref="IValueRead<T>.Read"/>
        public RCNumRectangle Read()
        {
            return new RCNumRectangle(new RCNumber(this.Heap.ReadInt(this.DataAddress)),
                                      new RCNumber(this.Heap.ReadInt(this.DataAddress + 4)),
                                      new RCNumber(this.Heap.ReadInt(this.DataAddress + 8)),
                                      new RCNumber(this.Heap.ReadInt(this.DataAddress + 12)));
        }

        #endregion IValueRead<T> methods

        #region IValueWrite<T> methods

        /// <see cref="IValueWrite<T>.Write"/>
        public void Write(RCNumRectangle newVal)
        {
            this.Heap.WriteInt(this.DataAddress, newVal.X.Bits);
            this.Heap.WriteInt(this.DataAddress + 4, newVal.Y.Bits);
            this.Heap.WriteInt(this.DataAddress + 8, newVal.Width.Bits);
            this.Heap.WriteInt(this.DataAddress + 12, newVal.Height.Bits);
        }

        #endregion IValueWrite<T> methods
    }
}
