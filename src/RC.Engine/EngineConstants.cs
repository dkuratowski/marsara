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
    static class EngineConstants
    {
        /// <summary>
        /// Test constant.
        /// </summary>
        public static readonly int TEST_CONSTANT = ConstantsTable.Get<int>("RC.Engine.TestConstant");
    }

    /// <summary>
    /// This static class is used to access the trace filters defined for the RC.Engine module.
    /// </summary>
    static class EngineTraceFilters
    {
        public static readonly int ERROR = TraceManager.GetTraceFilterID("RC.Engine.Error");
        public static readonly int WARNING = TraceManager.GetTraceFilterID("RC.Engine.Warning");
        public static readonly int INFO = TraceManager.GetTraceFilterID("RC.Engine.Info");
        public static readonly int DETAILS = TraceManager.GetTraceFilterID("RC.Engine.Details");
    }
}
