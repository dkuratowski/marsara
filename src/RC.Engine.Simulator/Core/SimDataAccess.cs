using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.InternalInterfaces;
using RC.Common;

namespace RC.Engine.Simulator.Core
{
    class SimDataAccess : ISimDataAccess
    {
        /// <summary>
        /// Constructs a SimDataAccess instance.
        /// </summary>
        public SimDataAccess(int dataAddress, SimHeapType dataType, ISimulationHeap heap, List<SimHeapType> allTypes, DeallocationFunc deallocFunc)
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

        #region ISimDataAccess methods

        /// <see cref="ISimDataAccess.ReadByte"/>
        public byte ReadByte()
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.Byte) { throw new SimulationHeapException("Type mismatch!"); }
            return this.heap.ReadByte(this.dataAddress);
        }

        /// <see cref="ISimDataAccess.ReadShort"/>
        public short ReadShort()
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.Short) { throw new SimulationHeapException("Type mismatch!"); }
            return this.heap.ReadShort(this.dataAddress);
        }

        /// <see cref="ISimDataAccess.ReadInt"/>
        public int ReadInt()
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.Integer) { throw new SimulationHeapException("Type mismatch!"); }
            return this.heap.ReadInt(this.dataAddress);
        }

        /// <see cref="ISimDataAccess.ReadLong"/>
        public long ReadLong()
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.Long) { throw new SimulationHeapException("Type mismatch!"); }
            return this.heap.ReadLong(this.dataAddress);
        }

        /// <see cref="ISimDataAccess.ReadNumber"/>
        public RCNumber ReadNumber()
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.Number) { throw new SimulationHeapException("Type mismatch!"); }
            return new RCNumber(this.heap.ReadInt(this.dataAddress));
        }

        /// <see cref="ISimDataAccess.ReadIntVector"/>
        public RCIntVector ReadIntVector()
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.IntVector) { throw new SimulationHeapException("Type mismatch!"); }
            return new RCIntVector(this.heap.ReadInt(this.dataAddress),
                                   this.heap.ReadInt(this.dataAddress + 4));
        }

        /// <see cref="ISimDataAccess.ReadNumVector"/>
        public RCNumVector ReadNumVector()
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.NumVector) { throw new SimulationHeapException("Type mismatch!"); }
            return new RCNumVector(new RCNumber(this.heap.ReadInt(this.dataAddress)),
                                   new RCNumber(this.heap.ReadInt(this.dataAddress + 4)));
        }

        /// <see cref="ISimDataAccess.ReadIntRectangle"/>
        public RCIntRectangle ReadIntRectangle()
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.IntRectangle) { throw new SimulationHeapException("Type mismatch!"); }
            return new RCIntRectangle(this.heap.ReadInt(this.dataAddress),
                                      this.heap.ReadInt(this.dataAddress + 4),
                                      this.heap.ReadInt(this.dataAddress + 8),
                                      this.heap.ReadInt(this.dataAddress + 12));
        }

        /// <see cref="ISimDataAccess.ReadNumRectangle"/>
        public RCNumRectangle ReadNumRectangle()
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.NumRectangle) { throw new SimulationHeapException("Type mismatch!"); }
            return new RCNumRectangle(new RCNumber(this.heap.ReadInt(this.dataAddress)),
                                      new RCNumber(this.heap.ReadInt(this.dataAddress + 4)),
                                      new RCNumber(this.heap.ReadInt(this.dataAddress + 8)),
                                      new RCNumber(this.heap.ReadInt(this.dataAddress + 12)));
        }

        /// <see cref="ISimDataAccess.WriteByte"/>
        public void WriteByte(byte newVal)
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.Byte) { throw new SimulationHeapException("Type mismatch!"); }
            this.heap.WriteByte(this.dataAddress, newVal);
        }

        /// <see cref="ISimDataAccess.WriteShort"/>
        public void WriteShort(short newVal)
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.Short) { throw new SimulationHeapException("Type mismatch!"); }
            this.heap.WriteShort(this.dataAddress, newVal);
        }

        /// <see cref="ISimDataAccess.WriteInt"/>
        public void WriteInt(int newVal)
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.Integer) { throw new SimulationHeapException("Type mismatch!"); }
            this.heap.WriteInt(this.dataAddress, newVal);
        }

        /// <see cref="ISimDataAccess.WriteLong"/>
        public void WriteLong(long newVal)
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.Long) { throw new SimulationHeapException("Type mismatch!"); }
            this.heap.WriteLong(this.dataAddress, newVal);
        }

        /// <see cref="ISimDataAccess.WriteNumber"/>
        public void WriteNumber(RCNumber newVal)
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.Number) { throw new SimulationHeapException("Type mismatch!"); }
            this.heap.WriteInt(this.dataAddress, newVal.Bits);
        }

        /// <see cref="ISimDataAccess.WriteIntVector"/>
        public void WriteIntVector(RCIntVector newVal)
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.IntVector) { throw new SimulationHeapException("Type mismatch!"); }
            this.heap.WriteInt(this.dataAddress, newVal.X);
            this.heap.WriteInt(this.dataAddress + 4, newVal.Y);
        }

        /// <see cref="ISimDataAccess.WriteNumVector"/>
        public void WriteNumVector(RCNumVector newVal)
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.NumVector) { throw new SimulationHeapException("Type mismatch!"); }
            this.heap.WriteInt(this.dataAddress, newVal.X.Bits);
            this.heap.WriteInt(this.dataAddress + 4, newVal.Y.Bits);
        }

        /// <see cref="ISimDataAccess.WriteIntRectangle"/>
        public void WriteIntRectangle(RCIntRectangle newVal)
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.IntRectangle) { throw new SimulationHeapException("Type mismatch!"); }
            this.heap.WriteInt(this.dataAddress, newVal.X);
            this.heap.WriteInt(this.dataAddress + 4, newVal.Y);
            this.heap.WriteInt(this.dataAddress + 8, newVal.Width);
            this.heap.WriteInt(this.dataAddress + 12, newVal.Height);
        }

        /// <see cref="ISimDataAccess.WriteNumRectangle"/>
        public void WriteNumRectangle(RCNumRectangle newVal)
        {
            if (this.dataType.BuiltInType != BuiltInTypeEnum.NumRectangle) { throw new SimulationHeapException("Type mismatch!"); }
            this.heap.WriteInt(this.dataAddress, newVal.X.Bits);
            this.heap.WriteInt(this.dataAddress + 4, newVal.Y.Bits);
            this.heap.WriteInt(this.dataAddress + 8, newVal.Width.Bits);
            this.heap.WriteInt(this.dataAddress + 12, newVal.Height.Bits);
        }

        /// <see cref="ISimDataAccess.PointTo"/>
        public void PointTo(ISimDataAccess target)
        {
            if (target != null)
            {
                SimDataAccess targetInstance = (SimDataAccess)target;
                if (this.dataType.PointedTypeID != targetInstance.dataType.ID) { throw new SimulationHeapException("Type mismatch!"); }
                this.heap.WriteInt(this.dataAddress, targetInstance.dataAddress);
            }
            else
            {
                this.heap.WriteInt(this.dataAddress, 0);
            }
        }

        /// <see cref="ISimDataAccess.Dereference"/>
        public ISimDataAccess Dereference()
        {
            if (this.dataType.PointedTypeID == -1) { throw new SimulationHeapException("Type mismatch!"); }

            int targetAddress = this.heap.ReadInt(this.dataAddress);
            return targetAddress != 0 ? new SimDataAccess(targetAddress, this.allTypes[this.dataType.PointedTypeID], this.heap, this.allTypes, this.deallocationFunc) : null;
        }

        /// <see cref="ISimDataAccess.Delete"/>
        public void Delete()
        {
            this.deallocationFunc(this.dataAddress, this.dataType.AllocationSize);
        }

        /// <see cref="ISimDataAccess.DeleteArray"/>
        public void DeleteArray()
        {
            int count = this.heap.ReadInt(this.dataAddress - 4);
            this.deallocationFunc(this.dataAddress - 4, count * this.dataType.AllocationSize + 4);
        }

        /// <see cref="ISimDataAccess.AccessField"/>
        public ISimDataAccess AccessField(int fieldIdx)
        {
            if (this.dataType.FieldOffsets == null) { throw new SimulationHeapException("Type mismatch!"); }
            return new SimDataAccess(this.dataAddress + this.dataType.FieldOffsets[fieldIdx], this.allTypes[this.dataType.FieldTypeIDs[fieldIdx]], this.heap, this.allTypes, this.deallocationFunc);
        }

        /// <see cref="ISimDataAccess.AccessArrayItem"/>
        public ISimDataAccess AccessArrayItem(int itemIdx)
        {
            return new SimDataAccess(this.dataAddress + itemIdx * this.dataType.AllocationSize, this.dataType, this.heap, this.allTypes, this.deallocationFunc);
        }

        #endregion ISimDataAccess methods

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
