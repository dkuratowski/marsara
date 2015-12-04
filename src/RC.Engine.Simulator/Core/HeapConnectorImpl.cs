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
    class HeapByteConnector : HeapConnector, IValue<byte>
    {
        public HeapByteConnector(int dataAddress, HeapType dataType, IHeap heap, IHeapConnectorFactory heapDataFactory, DeallocationFunc deallocFunc)
            : base(dataAddress, dataType, heap, heapDataFactory, deallocFunc)
        {
            if (dataType.BuiltInType != BuiltInTypeEnum.Byte) { throw new InvalidOperationException("Invalid heap type!"); }
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
    class HeapShortConnector : HeapConnector, IValue<short>
    {
        public HeapShortConnector(int dataAddress, HeapType dataType, IHeap heap, IHeapConnectorFactory heapDataFactory, DeallocationFunc deallocFunc)
            : base(dataAddress, dataType, heap, heapDataFactory, deallocFunc)
        {
            if (dataType.BuiltInType != BuiltInTypeEnum.Short) { throw new InvalidOperationException("Invalid heap type!"); }
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
    class HeapIntConnector : HeapConnector, IValue<int>
    {
        public HeapIntConnector(int dataAddress, HeapType dataType, IHeap heap, IHeapConnectorFactory heapDataFactory, DeallocationFunc deallocFunc)
            : base(dataAddress, dataType, heap, heapDataFactory, deallocFunc)
        {
            if (dataType.BuiltInType != BuiltInTypeEnum.Integer) { throw new InvalidOperationException("Invalid heap type!"); }
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
    class HeapLongConnector : HeapConnector, IValue<long>
    {
        public HeapLongConnector(int dataAddress, HeapType dataType, IHeap heap, IHeapConnectorFactory heapDataFactory, DeallocationFunc deallocFunc)
            : base(dataAddress, dataType, heap, heapDataFactory, deallocFunc)
        {
            if (dataType.BuiltInType != BuiltInTypeEnum.Long) { throw new InvalidOperationException("Invalid heap type!"); }
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
    class HeapNumberConnector : HeapConnector, IValue<RCNumber>
    {
        public HeapNumberConnector(int dataAddress, HeapType dataType, IHeap heap, IHeapConnectorFactory heapDataFactory, DeallocationFunc deallocFunc)
            : base(dataAddress, dataType, heap, heapDataFactory, deallocFunc)
        {
            if (dataType.BuiltInType != BuiltInTypeEnum.Number) { throw new InvalidOperationException("Invalid heap type!"); }
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
    class HeapIntVectorConnector : HeapConnector, IValue<RCIntVector>
    {
        public HeapIntVectorConnector(int dataAddress, HeapType dataType, IHeap heap, IHeapConnectorFactory heapDataFactory, DeallocationFunc deallocFunc)
            : base(dataAddress, dataType, heap, heapDataFactory, deallocFunc)
        {
            if (dataType.BuiltInType != BuiltInTypeEnum.IntVector) { throw new InvalidOperationException("Invalid heap type!"); }
        }

        #region IValueRead<T> methods

        /// <see cref="IValueRead<T>.Read"/>
        public RCIntVector Read()
        {
            bool isDefined = this.Heap.ReadByte(this.DataAddress) == (byte)0x01;
            return isDefined ? new RCIntVector(this.Heap.ReadInt(this.DataAddress + 1),
                                               this.Heap.ReadInt(this.DataAddress + 5))
                : RCIntVector.Undefined;
        }

        #endregion IValueRead<T> methods

        #region IValueWrite<T> methods

        /// <see cref="IValueWrite<T>.Write"/>
        public void Write(RCIntVector newVal)
        {
            bool isDefined = (newVal != RCIntVector.Undefined);
            this.Heap.WriteByte(this.DataAddress, isDefined ? (byte)0x01 : (byte)0x00);
            this.Heap.WriteInt(this.DataAddress + 1, newVal.X);
            this.Heap.WriteInt(this.DataAddress + 5, newVal.Y);
        }

        #endregion IValueWrite<T> methods
    }

    /// <summary>
    /// Class for accessing RCNumVectors on the simulation heap.
    /// </summary>
    class HeapNumVectorConnector : HeapConnector, IValue<RCNumVector>
    {
        public HeapNumVectorConnector(int dataAddress, HeapType dataType, IHeap heap, IHeapConnectorFactory heapDataFactory, DeallocationFunc deallocFunc)
            : base(dataAddress, dataType, heap, heapDataFactory, deallocFunc)
        {
            if (dataType.BuiltInType != BuiltInTypeEnum.NumVector) { throw new InvalidOperationException("Invalid heap type!"); }
        }

        #region IValueRead<T> methods

        /// <see cref="IValueRead<T>.Read"/>
        public RCNumVector Read()
        {
            bool isDefined = this.Heap.ReadByte(this.DataAddress) == (byte)0x01;
            return isDefined ? new RCNumVector(new RCNumber(this.Heap.ReadInt(this.DataAddress + 1)),
                                               new RCNumber(this.Heap.ReadInt(this.DataAddress + 5)))
                : RCNumVector.Undefined;
        }

        #endregion IValueRead<T> methods

        #region IValueWrite<T> methods

        /// <see cref="IValueWrite<T>.Write"/>
        public void Write(RCNumVector newVal)
        {
            bool isDefined = (newVal != RCNumVector.Undefined);
            this.Heap.WriteByte(this.DataAddress, isDefined ? (byte)0x01 : (byte)0x00);
            this.Heap.WriteInt(this.DataAddress + 1, newVal.X.Bits);
            this.Heap.WriteInt(this.DataAddress + 5, newVal.Y.Bits);
        }

        #endregion IValueWrite<T> methods
    }

    /// <summary>
    /// Class for accessing RCIntRectangles on the simulation heap.
    /// </summary>
    class HeapIntRectangleConnector : HeapConnector, IValue<RCIntRectangle>
    {
        public HeapIntRectangleConnector(int dataAddress, HeapType dataType, IHeap heap, IHeapConnectorFactory heapDataFactory, DeallocationFunc deallocFunc)
            : base(dataAddress, dataType, heap, heapDataFactory, deallocFunc)
        {
            if (dataType.BuiltInType != BuiltInTypeEnum.IntRectangle) { throw new InvalidOperationException("Invalid heap type!"); }
        }

        #region IValueRead<T> methods

        /// <see cref="IValueRead<T>.Read"/>
        public RCIntRectangle Read()
        {
            bool isDefined = this.Heap.ReadByte(this.DataAddress) == (byte)0x01;
            return isDefined ? new RCIntRectangle(this.Heap.ReadInt(this.DataAddress + 1),
                                                  this.Heap.ReadInt(this.DataAddress + 5),
                                                  this.Heap.ReadInt(this.DataAddress + 9),
                                                  this.Heap.ReadInt(this.DataAddress + 13))
                : RCIntRectangle.Undefined;
        }

        #endregion IValueRead<T> methods

        #region IValueWrite<T> methods

        /// <see cref="IValueWrite<T>.Write"/>
        public void Write(RCIntRectangle newVal)
        {
            bool isDefined = (newVal != RCIntRectangle.Undefined);
            this.Heap.WriteByte(this.DataAddress, isDefined ? (byte)0x01 : (byte)0x00);
            this.Heap.WriteInt(this.DataAddress + 1, newVal.X);
            this.Heap.WriteInt(this.DataAddress + 5, newVal.Y);
            this.Heap.WriteInt(this.DataAddress + 9, newVal.Width);
            this.Heap.WriteInt(this.DataAddress + 13, newVal.Height);
        }

        #endregion IValueWrite<T> methods
    }

    /// <summary>
    /// Class for accessing RCNumRectangles on the simulation heap.
    /// </summary>
    class HeapNumRectangleConnector : HeapConnector, IValue<RCNumRectangle>
    {
        public HeapNumRectangleConnector(int dataAddress, HeapType dataType, IHeap heap, IHeapConnectorFactory heapDataFactory, DeallocationFunc deallocFunc)
            : base(dataAddress, dataType, heap, heapDataFactory, deallocFunc)
        {
            if (dataType.BuiltInType != BuiltInTypeEnum.NumRectangle) { throw new InvalidOperationException("Invalid heap type!"); }
        }

        #region IValueRead<T> methods

        /// <see cref="IValueRead<T>.Read"/>
        public RCNumRectangle Read()
        {
            bool isDefined = this.Heap.ReadByte(this.DataAddress) == (byte)0x01;
            return isDefined ? new RCNumRectangle(new RCNumber(this.Heap.ReadInt(this.DataAddress + 1)),
                                                  new RCNumber(this.Heap.ReadInt(this.DataAddress + 5)),
                                                  new RCNumber(this.Heap.ReadInt(this.DataAddress + 9)),
                                                  new RCNumber(this.Heap.ReadInt(this.DataAddress + 13)))
                : RCNumRectangle.Undefined;
        }

        #endregion IValueRead<T> methods

        #region IValueWrite<T> methods

        /// <see cref="IValueWrite<T>.Write"/>
        public void Write(RCNumRectangle newVal)
        {
            bool isDefined = (newVal != RCNumRectangle.Undefined);
            this.Heap.WriteByte(this.DataAddress, isDefined ? (byte)0x01 : (byte)0x00);
            this.Heap.WriteInt(this.DataAddress + 1, newVal.X.Bits);
            this.Heap.WriteInt(this.DataAddress + 5, newVal.Y.Bits);
            this.Heap.WriteInt(this.DataAddress + 9, newVal.Width.Bits);
            this.Heap.WriteInt(this.DataAddress + 13, newVal.Height.Bits);
        }

        #endregion IValueWrite<T> methods
    }
}
