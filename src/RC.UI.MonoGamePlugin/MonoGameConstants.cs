using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Diagnostics;

namespace RC.UI.MonoGamePlugin
{
    /// <summary>
    /// This static class is used to access the IDs of the trace filters defined for the RC.UI.MonoGamePlugin module.
    /// </summary>
    static class MonoGameTraceFilters
    {
        public static readonly int ERROR = TraceManager.GetTraceFilterID("RC.UI.MonoGamePlugin.Error");
        public static readonly int WARNING = TraceManager.GetTraceFilterID("RC.UI.MonoGamePlugin.Warning");
        public static readonly int INFO = TraceManager.GetTraceFilterID("RC.UI.MonoGamePlugin.Info");
        public static readonly int DETAILS = TraceManager.GetTraceFilterID("RC.UI.MonoGamePlugin.Details");
    }
}
