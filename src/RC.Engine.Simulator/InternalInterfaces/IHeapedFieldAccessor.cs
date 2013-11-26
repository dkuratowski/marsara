using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.InternalInterfaces
{
    /// <summary>
    /// Interface of field accessors.
    /// </summary>
    interface IHeapedFieldAccessor : IDisposable
    {
        /// <summary>
        /// Indicates the this field accessor object is ready to use.
        /// </summary>
        void ReadyToUse();

        /// <summary>
        /// Attaches this field accessor to the heap.
        /// </summary>
        /// <param name="connector">The connector object.</param>
        void AttachToHeap(IHeapConnector connector);

        /// <summary>
        /// Detaches this field accessor from the heap.
        /// </summary>
        void DetachFromHeap();
    }
}
