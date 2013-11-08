using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.InternalInterfaces;
using RC.Common;

namespace RC.Engine.Simulator.Core
{
    class SimHeapAccess : ISimHeapAccess
    {
        /// <summary>
        /// Constructs a SimHeapAccess instance.
        /// </summary>
        public SimHeapAccess(int dataAddress, SimHeapType dataType, ISimulationHeap heap, List<SimHeapType> allTypes, DeallocationFunc deallocFunc)
        {
            this.dataAddress = dataAddress;
            this.dataType = dataType;
            this.heap = heap;
            this.allTypes = allTypes;
            this.deallocationFunc = deallocFunc;
        }

        /// <summary>
        /// Gets the address of the data on the heap accessed by this instance.
        /// </summary>
        public int DataAddress { get { return this.dataAddress; } }

        /// <summary>
        /// Gets the type of the data on the heap accessed by this instance.
        /// </summary>
        public SimHeapType DataType { get { return this.dataType; } }

        /// <summary>
        /// Function declaration for performing deallocation procedures on deletion.
        /// </summary>
        /// <param name="address">The start address of the section to be deallocated.</param>
        /// <param name="length">The length of the section to be deallocated.</param>
        public delegate void DeallocationFunc(int address, int length);

        #region ISimHeapAccess methods

        /// <see cref="ISimHeapAccess.ReadByte"/>
        public byte ReadByte()
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.Byte) { throw new SimulationHeapException("Type mismatch!"); }
            return this.heap.ReadByte(this.dataAddress);
        }

        /// <see cref="ISimHeapAccess.ReadShort"/>
        public short ReadShort()
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.Short) { throw new SimulationHeapException("Type mismatch!"); }
            return this.heap.ReadShort(this.dataAddress);
        }

        /// <see cref="ISimHeapAccess.ReadInt"/>
        public int ReadInt()
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.Integer) { throw new SimulationHeapException("Type mismatch!"); }
            return this.heap.ReadInt(this.dataAddress);
        }

        /// <see cref="ISimHeapAccess.ReadLong"/>
        public long ReadLong()
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.Long) { throw new SimulationHeapException("Type mismatch!"); }
            return this.heap.ReadLong(this.dataAddress);
        }

        /// <see cref="ISimHeapAccess.ReadNumber"/>
        public RCNumber ReadNumber()
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.Number) { throw new SimulationHeapException("Type mismatch!"); }
            return new RCNumber(this.heap.ReadInt(this.dataAddress));
        }

        /// <see cref="ISimHeapAccess.ReadIntVector"/>
        public RCIntVector ReadIntVector()
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.IntVector) { throw new SimulationHeapException("Type mismatch!"); }
            return new RCIntVector(this.heap.ReadInt(this.dataAddress),
                                   this.heap.ReadInt(this.dataAddress + 4));
        }

        /// <see cref="ISimHeapAccess.ReadNumVector"/>
        public RCNumVector ReadNumVector()
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.NumVector) { throw new SimulationHeapException("Type mismatch!"); }
            return new RCNumVector(new RCNumber(this.heap.ReadInt(this.dataAddress)),
                                   new RCNumber(this.heap.ReadInt(this.dataAddress + 4)));
        }

        /// <see cref="ISimHeapAccess.ReadIntRectangle"/>
        public RCIntRectangle ReadIntRectangle()
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.IntRectangle) { throw new SimulationHeapException("Type mismatch!"); }
            return new RCIntRectangle(this.heap.ReadInt(this.dataAddress),
                                      this.heap.ReadInt(this.dataAddress + 4),
                                      this.heap.ReadInt(this.dataAddress + 8),
                                      this.heap.ReadInt(this.dataAddress + 12));
        }

        /// <see cref="ISimHeapAccess.ReadNumRectangle"/>
        public RCNumRectangle ReadNumRectangle()
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.NumRectangle) { throw new SimulationHeapException("Type mismatch!"); }
            return new RCNumRectangle(new RCNumber(this.heap.ReadInt(this.dataAddress)),
                                      new RCNumber(this.heap.ReadInt(this.dataAddress + 4)),
                                      new RCNumber(this.heap.ReadInt(this.dataAddress + 8)),
                                      new RCNumber(this.heap.ReadInt(this.dataAddress + 12)));
        }

        /// <see cref="ISimHeapAccess.WriteByte"/>
        public void WriteByte(byte newVal)
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.Byte) { throw new SimulationHeapException("Type mismatch!"); }
            this.heap.WriteByte(this.dataAddress, newVal);
        }

        /// <see cref="ISimHeapAccess.WriteShort"/>
        public void WriteShort(short newVal)
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.Short) { throw new SimulationHeapException("Type mismatch!"); }
            this.heap.WriteShort(this.dataAddress, newVal);
        }

        /// <see cref="ISimHeapAccess.WriteInt"/>
        public void WriteInt(int newVal)
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.Integer) { throw new SimulationHeapException("Type mismatch!"); }
            this.heap.WriteInt(this.dataAddress, newVal);
        }

        /// <see cref="ISimHeapAccess.WriteLong"/>
        public void WriteLong(long newVal)
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.Long) { throw new SimulationHeapException("Type mismatch!"); }
            this.heap.WriteLong(this.dataAddress, newVal);
        }

        /// <see cref="ISimHeapAccess.WriteNumber"/>
        public void WriteNumber(RCNumber newVal)
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.Number) { throw new SimulationHeapException("Type mismatch!"); }
            this.heap.WriteInt(this.dataAddress, newVal.Bits);
        }

        /// <see cref="ISimHeapAccess.WriteIntVector"/>
        public void WriteIntVector(RCIntVector newVal)
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.IntVector) { throw new SimulationHeapException("Type mismatch!"); }
            this.heap.WriteInt(this.dataAddress, newVal.X);
            this.heap.WriteInt(this.dataAddress + 4, newVal.Y);
        }

        /// <see cref="ISimHeapAccess.WriteNumVector"/>
        public void WriteNumVector(RCNumVector newVal)
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.NumVector) { throw new SimulationHeapException("Type mismatch!"); }
            this.heap.WriteInt(this.dataAddress, newVal.X.Bits);
            this.heap.WriteInt(this.dataAddress + 4, newVal.Y.Bits);
        }

        /// <see cref="ISimHeapAccess.WriteIntRectangle"/>
        public void WriteIntRectangle(RCIntRectangle newVal)
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.IntRectangle) { throw new SimulationHeapException("Type mismatch!"); }
            this.heap.WriteInt(this.dataAddress, newVal.X);
            this.heap.WriteInt(this.dataAddress + 4, newVal.Y);
            this.heap.WriteInt(this.dataAddress + 8, newVal.Width);
            this.heap.WriteInt(this.dataAddress + 12, newVal.Height);
        }

        /// <see cref="ISimHeapAccess.WriteNumRectangle"/>
        public void WriteNumRectangle(RCNumRectangle newVal)
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.NumRectangle) { throw new SimulationHeapException("Type mismatch!"); }
            this.heap.WriteInt(this.dataAddress, newVal.X.Bits);
            this.heap.WriteInt(this.dataAddress + 4, newVal.Y.Bits);
            this.heap.WriteInt(this.dataAddress + 8, newVal.Width.Bits);
            this.heap.WriteInt(this.dataAddress + 12, newVal.Height.Bits);
        }

        /// <see cref="ISimHeapAccess.PointTo"/>
        public void PointTo(ISimHeapAccess target)
        {
            if (target != null)
            {
                SimHeapAccess targetInstance = (SimHeapAccess)target;
                if (this.dataType.PointedTypeID != targetInstance.dataType.ID) { throw new SimulationHeapException("Type mismatch!"); }
                this.heap.WriteInt(this.dataAddress, targetInstance.dataAddress);
            }
            else
            {
                this.heap.WriteInt(this.dataAddress, 0);
            }
        }

        /// <see cref="ISimHeapAccess.Dereference"/>
        public ISimHeapAccess Dereference()
        {
            if (this.dataType.PointedTypeID == -1) { throw new SimulationHeapException("Type mismatch!"); }

            int targetAddress = this.heap.ReadInt(this.dataAddress);
            return targetAddress != 0 ? new SimHeapAccess(targetAddress, this.allTypes[this.dataType.PointedTypeID], this.heap, this.allTypes, this.deallocationFunc) : null;
        }

        /// <see cref="ISimHeapAccess.Delete"/>
        public void Delete()
        {
            this.deallocationFunc(this.dataAddress, this.dataType.AllocationSize);
        }

        /// <see cref="ISimHeapAccess.DeleteArray"/>
        public void DeleteArray()
        {
            int count = this.heap.ReadInt(this.dataAddress - 4);
            this.deallocationFunc(this.dataAddress - 4, count * this.dataType.AllocationSize + 4);
        }

        /// <see cref="ISimHeapAccess.AccessField"/>
        public ISimHeapAccess AccessField(int fieldIdx)
        {
            if (this.dataType.FieldOffsets == null) { throw new SimulationHeapException("Type mismatch!"); }
            return new SimHeapAccess(this.dataAddress + this.dataType.FieldOffsets[fieldIdx], this.allTypes[this.dataType.FieldTypeIDs[fieldIdx]], this.heap, this.allTypes, this.deallocationFunc);
        }

        /// <see cref="ISimHeapAccess.AccessArrayItem"/>
        public ISimHeapAccess AccessArrayItem(int itemIdx)
        {
            return new SimHeapAccess(this.dataAddress + itemIdx * this.dataType.AllocationSize, this.dataType, this.heap, this.allTypes, this.deallocationFunc);
        }

        #endregion ISimHeapAccess methods

        /// <summary>
        /// The address of the data on the heap accessed by this instance.
        /// </summary>
        private int dataAddress;

        /// <summary>
        /// The type of the data on the heap accessed by this instance.
        /// </summary>
        private SimHeapType dataType;

        /// <summary>
        /// Reference to the heap.
        /// </summary>
        private ISimulationHeap heap;

        /// <summary>
        /// List of all registered types.
        /// </summary>
        private List<SimHeapType> allTypes;

        /// <summary>
        /// Function reference for performing deallocation procedures on deletion.
        /// </summary>
        private DeallocationFunc deallocationFunc;
    }
}
