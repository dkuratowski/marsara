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
        void ReadyToUse();
    }
}
