using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.InternalInterfaces;
using RC.Common;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// This class provides methods for accessing data stored on the simulation heap.
    /// </summary>
    class HeapData : IHeapData
    {
        /// <summary>
        /// Constructs a HeapData instance.
        /// </summary>
        public HeapData(int dataAddress, HeapType dataType, IHeap heap, IHeapDataFactory heapDataFactory, DeallocationFunc deallocFunc)
        {
            this.dataAddress = dataAddress;
            this.dataType = dataType;
            this.heap = heap;
            this.heapDataFactory = heapDataFactory;
            this.deallocationFunc = deallocFunc;
        }

        /// <summary>
        /// Gets the address of the data on the heap accessed by this instance.
        /// </summary>
        public int DataAddress { get { return this.dataAddress; } }

        /// <summary>
        /// Gets the type of the data on the heap accessed by this instance.
        /// </summary>
        public HeapType DataType { get { return this.dataType; } }

        /// <summary>
        /// Function declaration for performing deallocation procedures on deletion.
        /// </summary>
        /// <param name="address">The start address of the section to be deallocated.</param>
        /// <param name="length">The length of the section to be deallocated.</param>
        public delegate void DeallocationFunc(int address, int length);

        #region IHeapData methods

        /// <see cref="IHeapData.PointTo"/>
        public void PointTo(IHeapData target)
        {
            if (target != null)
            {
                HeapData targetInstance = (HeapData)target;
                if (this.dataType.PointedTypeID != targetInstance.dataType.ID) { throw new HeapException("Type mismatch!"); }
                this.heap.WriteInt(this.dataAddress, targetInstance.dataAddress);
            }
            else
            {
                this.heap.WriteInt(this.dataAddress, 0);
            }
        }

        /// <see cref="IHeapData.Dereference"/>
        public IHeapData Dereference()
        {
            if (this.dataType.PointedTypeID == -1) { throw new HeapException("Type mismatch!"); }

            int targetAddress = this.heap.ReadInt(this.dataAddress);
            return targetAddress != 0 ?
                   this.heapDataFactory.CreateHeapData(targetAddress, this.dataType.PointedTypeID) :
                   null;
        }

        /// <see cref="IHeapData.Delete"/>
        public void Delete()
        {
            this.deallocationFunc(this.dataAddress, this.dataType.AllocationSize);
        }

        /// <see cref="IHeapData.DeleteArray"/>
        public void DeleteArray()
        {
            int count = this.heap.ReadInt(this.dataAddress - 4);
            this.deallocationFunc(this.dataAddress - 4, count * this.dataType.AllocationSize + 4);
        }

        /// <see cref="IHeapData.AccessField"/>
        public IHeapData AccessField(int fieldIdx)
        {
            if (this.dataType.FieldOffsets == null) { throw new HeapException("Type mismatch!"); }
            return this.heapDataFactory.CreateHeapData(this.dataAddress + this.dataType.FieldOffsets[fieldIdx], this.dataType.FieldTypeIDs[fieldIdx]);
        }

        /// <see cref="IHeapData.AccessArrayItem"/>
        public IHeapData AccessArrayItem(int itemIdx)
        {
            return this.heapDataFactory.CreateHeapData(this.dataAddress + itemIdx * this.dataType.AllocationSize, this.dataType.ID);
        }

        #endregion IHeapData methods

        /// <summary>
        /// Gets a reference to the simulation heap.
        /// </summary>
        protected IHeap Heap { get { return this.heap; } }

        /// <summary>
        /// The address of the data on the heap accessed by this instance.
        /// </summary>
        private int dataAddress;

        /// <summary>
        /// The type of the data on the heap accessed by this instance.
        /// </summary>
        private HeapType dataType;

        /// <summary>
        /// Reference to the heap.
        /// </summary>
        private IHeap heap;

        /// <summary>
        /// Reference to the factory object.
        /// </summary>
        private IHeapDataFactory heapDataFactory;

        /// <summary>
        /// Function reference for performing deallocation procedures on deletion.
        /// </summary>
        private DeallocationFunc deallocationFunc;
    }
}
