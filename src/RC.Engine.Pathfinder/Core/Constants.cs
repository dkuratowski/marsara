using RC.Common.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Pathfinder.Core
{
    /// <summary>
    /// This static class is used to access the trace filters defined for the RC.Engine.Pathfinder module.
    /// </summary>
    static class TraceFilters
    {
        public static readonly int ERROR = TraceManager.GetTraceFilterID("RC.Engine.Pathfinder.Error");
        public static readonly int WARNING = TraceManager.GetTraceFilterID("RC.Engine.Pathfinder.Warning");
        public static readonly int INFO = TraceManager.GetTraceFilterID("RC.Engine.Pathfinder.Info");
        public static readonly int DETAILS = TraceManager.GetTraceFilterID("RC.Engine.Pathfinder.Details");
    }
}
