using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Configuration;
using RC.Common.Diagnostics;

namespace RC.Engine
{
    /// <summary>
    /// This static class is used to access the constants of the RC.Engine module.
    /// </summary>
    static class RCEngineConstants
    {
        public static readonly int BSP_NODE_CAPACITY = ConstantsTable.Get<int>("RC.Engine.BspNodeCapacity");
        public static readonly int BSP_MIN_NODE_SIZE = ConstantsTable.Get<int>("RC.Engine.BspMinNodeSize");
    }

    /// <summary>
    /// This static class is used to access the trace filters defined for the RC.Engine module.
    /// </summary>
    static class RCEngineTraceFilters
    {
        public static readonly int ERROR = TraceManager.GetTraceFilterID("RC.Engine.Error");
        public static readonly int WARNING = TraceManager.GetTraceFilterID("RC.Engine.Warning");
        public static readonly int INFO = TraceManager.GetTraceFilterID("RC.Engine.Info");
        public static readonly int DETAILS = TraceManager.GetTraceFilterID("RC.Engine.Details");
    }
}
