using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using RC.Engine.Simulator.InternalInterfaces;
using RC.Engine.Simulator.ComponentInterfaces;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// The implementation of the HeapedArrayImpl class.
    /// </summary>
    /// <typeparam name="T">The type of the array items.</typeparam>
    class HeapedArrayImpl<T> : HeapedArray<T>, IHeapedFieldAccessor
    {
        /// <summary>
        /// Constructs an instance of HeapedArrayImpl.
        /// </summary>
        /// <param name="heapMgr">Reference to the heap manager.</param>
        public HeapedArrayImpl(IHeapManagerInternals heapMgr)
            : base()
        {
            this.isReadyToUse = false;
            this.connector = null;
            this.items = new HeapedValueImpl<T>[0];
            this.heapManager = heapMgr;
        }

        /// <see cref="HeapedArray<T>.this[]"/>
        public override IValue<T> this[int index]
        {
            get
            {
                //if (!this.isReadyToUse) { throw new InvalidOperationException("Heap accessor is not ready to use!"); }
                if (index < 0 || index >= this.items.Length) { throw new IndexOutOfRangeException(); }
                return this.items[index];
            }
        }

        /// <see cref="HeapedArray<T>.Length"/>
        public override int Length { get { return this.items.Length; } }

        /// <see cref="HeapedArray<T>.New"/>
        public override void New(int length)
        {
            //if (!this.isReadyToUse) { throw new InvalidOperationException("Heap accessor is not ready to use!"); }
            if (length < 0) { throw new ArgumentOutOfRangeException("length"); }

            /// Delete the old array if exists.
            this.DeleteFromHeap();

            /// Create the new array.
            this.items = new HeapedValueImpl<T>[length];
            for (int i = 0; i < this.items.Length; i++)
            {
                this.items[i] = new HeapedValueImpl<T>();
                this.items[i].ReadyToUse();
            }

            /// Synchronize to the heap.
            this.SynchToHeap();
        }

        /// <see cref="IEnumerable<IValue<T>>.GetEnumeratorImpl"/>
        protected override IEnumerator<IValue<T>> GetEnumeratorImpl()
        {
            //if (!this.isReadyToUse) { throw new InvalidOperationException("Heap accessor is not ready to use!"); }
            return ((IEnumerable<IValue<T>>)this.items).GetEnumerator();
        }

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (this.connector != null) { throw new InvalidOperationException("Heap accessor is still attached to the heap!"); }
            this.isReadyToUse = false;
            this.connector = null;
        }

        /// <see cref="IHeapedFieldAccessor.ReadyToUse"/>
        public void ReadyToUse()
        {
            this.isReadyToUse = true;
        }

        /// <see cref="IHeapedFieldAccessor.AttachToHeap"/>
        public void AttachToHeap(IHeapConnector connector)
        {
            if (!this.isReadyToUse) { throw new InvalidOperationException("Heap accessor is not ready to use!"); }
            if (this.connector != null) { throw new InvalidOperationException("Heap accessor already attached to the heap!"); }

            this.connector = connector;
            this.SynchToHeap();
        }

        /// <see cref="IHeapedFieldAccessor.DetachFromHeap"/>
        public void DetachFromHeap()
        {
            if (!this.isReadyToUse) { throw new InvalidOperationException("Heap accessor is not ready to use!"); }
            if (this.connector == null) { throw new InvalidOperationException("Heap accessor not attached to the heap!"); }

            this.DeleteFromHeap();
            this.connector = null;
        }

        /// <summary>
        /// Writes the contents of the array to the heap if attached.
        /// </summary>
        private void SynchToHeap()
        {
            /// Synchronize to the heap.
            if (this.connector != null)
            {
                if (this.items.Length > 0)
                {
                    this.connector.PointTo(this.heapManager.NewArray(this.connector.DataType.PointedTypeID, this.items.Length));
                    for (int i = 0; i < this.items.Length; i++)
                    {
                        this.items[i].AttachToHeap(this.connector.Dereference().AccessArrayItem(i));
                    }
                }
                else
                {
                    this.connector.PointTo(null);
                }
            }
        }

        /// <summary>
        /// Deletes the array from the heap if attached.
        /// </summary>
        private void DeleteFromHeap()
        {
            if (this.connector != null && this.items.Length > 0)
            {
                this.connector.Dereference().DeleteArray();
            }
        }

        /// <summary>
        /// The items in this array.
        /// </summary>
        private HeapedValueImpl<T>[] items;

        /// <summary>
        /// Reference to the connector object if attached to the heap; otherwise null.
        /// </summary>
        private IHeapConnector connector;

        /// <summary>
        /// Reference to the heap manager.
        /// </summary>
        private IHeapManagerInternals heapManager;

        /// <summary>
        /// This flag indicates whether this accessor object is ready to use or not.
        /// </summary>
        private bool isReadyToUse;
    }
}
