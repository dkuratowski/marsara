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
        public HeapedValueImpl()
            : base()
        {
            this.isReadyToUse = false;
        }

        /// <see cref="IValue<T>.Read"/>
        public override T Read()
        {
            if (!this.isReadyToUse) { throw new InvalidOperationException("Heap accessor is not ready to use!"); }
            throw new NotImplementedException();
        }

        /// <see cref="IValue<T>.Write"/>
        public override void Write(T newVal)
        {
            if (!this.isReadyToUse) { throw new InvalidOperationException("Heap accessor is not ready to use!"); }
            throw new NotImplementedException();
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
        /// This flag indicates whether this accessor object is ready to use or not.
        /// </summary>
        private bool isReadyToUse;
    }
}
