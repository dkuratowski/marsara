using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Configuration;
using RC.Common.Diagnostics;

namespace RC.Engine.Maps
{
    /// <summary>
    /// This static class is used to access the constants of the RC.Engine.Maps module.
    /// </summary>
    static class Constants
    {
        public static readonly int BSP_NODE_CAPACITY = ConstantsTable.Get<int>("RC.Engine.Maps.BspNodeCapacity");
        public static readonly int BSP_MIN_NODE_SIZE = ConstantsTable.Get<int>("RC.Engine.Maps.BspMinNodeSize");
    }

    /// <summary>
    /// This static class is used to access the trace filters defined for the RC.Engine.Maps module.
    /// </summary>
    static class TraceFilters
    {
        public static readonly int ERROR = TraceManager.GetTraceFilterID("RC.Engine.Maps.Error");
        public static readonly int WARNING = TraceManager.GetTraceFilterID("RC.Engine.Maps.Warning");
        public static readonly int INFO = TraceManager.GetTraceFilterID("RC.Engine.Maps.Info");
        public static readonly int DETAILS = TraceManager.GetTraceFilterID("RC.Engine.Maps.Details");
    }
}
