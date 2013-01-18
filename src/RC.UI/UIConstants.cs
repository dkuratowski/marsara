using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Diagnostics;

namespace RC.UI
{
    /// <summary>
    /// This static class is used to access the IDs of the trace filters defined for the RC.UI module.
    /// </summary>
    static class UITraceFilters
    {
        public static readonly int ERROR = TraceManager.GetTraceFilterID("RC.UI.Error");
        public static readonly int WARNING = TraceManager.GetTraceFilterID("RC.UI.Warning");
        public static readonly int INFO = TraceManager.GetTraceFilterID("RC.UI.Info");
        public static readonly int DETAILS = TraceManager.GetTraceFilterID("RC.UI.Details");
    }
}
