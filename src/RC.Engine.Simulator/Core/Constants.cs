using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Configuration;
using RC.Common.Diagnostics;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// This static class is used to access the constants of the RC.Engine.Simulator module.
    /// </summary>
    static class Constants
    {
        public static readonly int SIM_HEAP_PAGESIZE = ConstantsTable.Get<int>("RC.Engine.Simulator.SimulationHeapPageSize");
        public static readonly int SIM_HEAP_CAPACITY = ConstantsTable.Get<int>("RC.Engine.Simulator.SimulationHeapCapacity");
    }

    /// <summary>
    /// This static class is used to access the trace filters defined for the RC.Engine.Simulator module.
    /// </summary>
    static class TraceFilters
    {
        public static readonly int ERROR = TraceManager.GetTraceFilterID("RC.Engine.Simulator.Error");
        public static readonly int WARNING = TraceManager.GetTraceFilterID("RC.Engine.Simulator.Warning");
        public static readonly int INFO = TraceManager.GetTraceFilterID("RC.Engine.Simulator.Info");
        public static readonly int DETAILS = TraceManager.GetTraceFilterID("RC.Engine.Simulator.Details");
    }
}
