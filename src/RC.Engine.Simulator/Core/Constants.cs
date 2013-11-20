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
        public static readonly int BSP_NODE_CAPACITY = ConstantsTable.Get<int>("RC.Engine.Simulator.BspNodeCapacity");
        public static readonly int BSP_MIN_NODE_SIZE = ConstantsTable.Get<int>("RC.Engine.Simulator.BspMinNodeSize");
        public static readonly string METADATA_DIR = ConstantsTable.Get<string>("RC.Engine.Simulator.MetadataDir");

        /// <summary>
        /// The name of the field of the composite heap types that contains the data from the base class.
        /// </summary>
        public const string NAME_OF_BASE_TYPE_FIELD = "base";
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
