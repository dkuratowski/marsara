using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Diagnostics;

namespace RC.UI.XnaPlugin
{
    /// <summary>
    /// This static class is used to access the IDs of the trace filters defined for the RC.UI.XnaPlugin module.
    /// </summary>
    static class XnaTraceFilters
    {
        public static readonly int ERROR = TraceManager.GetTraceFilterID("RC.UI.XnaPlugin.Error");
        public static readonly int WARNING = TraceManager.GetTraceFilterID("RC.UI.XnaPlugin.Warning");
        public static readonly int INFO = TraceManager.GetTraceFilterID("RC.UI.XnaPlugin.Info");
        public static readonly int DETAILS = TraceManager.GetTraceFilterID("RC.UI.XnaPlugin.Details");
    }
}
