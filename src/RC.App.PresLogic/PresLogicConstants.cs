using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Diagnostics;

namespace RC.App.PresLogic
{
    /// <summary>
    /// This static class is used to access the trace filters defined for the RC.App.PresLogic module.
    /// </summary>
    static class PresLogicTraceFilters
    {
        public static readonly int INFO = TraceManager.GetTraceFilterID("RC.App.PresLogic.Info");
    }
}
