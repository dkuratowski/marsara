using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.InternalInterfaces;

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
        public HeapedArrayImpl()
            : base()
        {
            this.isReadyToUse = false;
        }

        /// <see cref="HeapedArray<T>.this[]"/>
        public override IValue<T> this[int index]
        {
            get
            {
                if (!this.isReadyToUse) { throw new InvalidOperationException("Heap accessor is not ready to use!"); }
                if (index < 0 || index >= this.items.Length) { throw new IndexOutOfRangeException(); }
                return this.items[index];
            }
        }

        /// <see cref="HeapedArray<T>.New"/>
        public override void New(int length)
        {
            if (!this.isReadyToUse) { throw new InvalidOperationException("Heap accessor is not ready to use!"); }
            if (length < 0) { throw new ArgumentOutOfRangeException("length"); }
            throw new NotImplementedException();
        }

        /// <see cref="IEnumerable<IValue<T>>.GetEnumeratorImpl"/>
        protected override IEnumerator<IValue<T>> GetEnumeratorImpl()
        {
            if (!this.isReadyToUse) { throw new InvalidOperationException("Heap accessor is not ready to use!"); }
            return ((IEnumerable<IValue<T>>)this.items).GetEnumerator();
        }

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <see cref="IHeapedFieldAccessor.ReadyToUse"/>
        public void ReadyToUse()
        {
            this.isReadyToUse = true;
        }

        /// <summary>
        /// The items in this array.
        /// </summary>
        private HeapedValueImpl<T>[] items = new HeapedValueImpl<T>[0];

        /// <summary>
        /// This flag indicates whether this accessor object is ready to use or not.
        /// </summary>
        private bool isReadyToUse;
    }
}
