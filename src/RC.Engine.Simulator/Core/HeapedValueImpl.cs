using RC.Engine.Simulator.InternalInterfaces;
using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// The implementation of the HeapedValue class.
    /// </summary>
    /// <typeparam name="T">The type of the member/reference.</typeparam>
    class HeapedValueImpl<T> : HeapedValue<T>, IHeapedFieldAccessor
    {
        /// <summary>
        /// Constructs a HeapedValueImpl instance.
        /// </summary>
        /// <param name="isReference">
        /// True if this accessor is a reference to a HeapedObject or just a simple value.
        /// </param>
        public HeapedValueImpl()
            : base()
        {
            this.isReadyToUse = false;
            this.connector = null;
            this.valueInterface = null;
            this.cachedValue = default(T);
        }

        #region IValue<T> methods

        /// <see cref="IValue<T>.Read"/>
        public override T Read()
        {
            if (!this.isReadyToUse) { throw new InvalidOperationException("Heap accessor is not ready to use!"); }
            return this.cachedValue;
        }

        /// <see cref="IValue<T>.Write"/>
        public override void Write(T newVal)
        {
            if (!this.isReadyToUse) { throw new InvalidOperationException("Heap accessor is not ready to use!"); }
            this.cachedValue = newVal;
            this.SynchToHeap();
            this.RaiseValueChangedEvt();
        }

        #endregion IValue<T> methods

        #region IHeapedFieldAccessor methods

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (this.connector != null) { throw new InvalidOperationException("Heap accessor is still attached to the heap!"); }

            this.isReadyToUse = false;
            this.connector = null;
            this.valueInterface = null;
            this.cachedValue = default(T);
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
            this.valueInterface = connector as IValue<T>;
            this.SynchToHeap();
        }

        /// <see cref="IHeapedFieldAccessor.DetachFromHeap"/>
        public void DetachFromHeap()
        {
            if (!this.isReadyToUse) { throw new InvalidOperationException("Heap accessor is not ready to use!"); }
            if (this.connector == null) { throw new InvalidOperationException("Heap accessor not attached to the heap!"); }

            this.connector = null;
            this.valueInterface = null;
        }

        #endregion IHeapedFieldAccessor methods

        /// <summary>
        /// Writes the cached value to the heap if attached.
        /// </summary>
        private void SynchToHeap()
        {
            if (this.valueInterface != null)
            {
                this.valueInterface.Write(this.cachedValue);
            }
            else if (this.connector != null)
            {
                if (this.cachedValue != null)
                {
                    HeapedObject referredObj = this.cachedValue as HeapedObject;
                    foreach (IHeapConnector objConnector in referredObj.HeapConnectors)
                    {
                        if (this.connector.DataType.PointedTypeID == objConnector.DataType.ID)
                        {
                            this.connector.PointTo(objConnector);
                        }
                    }
                }
                else
                {
                    this.connector.PointTo(null);
                }
            }
        }

        /// <summary>
        /// This field stores the cached value;
        /// </summary>
        private T cachedValue;

        /// <summary>
        /// Reference to the connector object if attached to the heap; otherwise null.
        /// </summary>
        private IHeapConnector connector;

        /// <summary>
        /// Reference to the IValue interface of the connector object if it connects to a simple value or
        /// null if it is a reference to another HeapedObject.
        /// </summary>
        private IValue<T> valueInterface;

        /// <summary>
        /// This flag indicates whether this accessor object is ready to use or not.
        /// </summary>
        private bool isReadyToUse;
    }
}
