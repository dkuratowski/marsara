using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
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

    static class PresLogicConstants
    {
        /// <summary>
        /// The default color of the transparent parts of the sprites.
        /// </summary>
        public static readonly RCColor DEFAULT_TRANSPARENT_COLOR = new RCColor(255, 0, 255);

        /// <summary>
        /// The default mask color of the sprites.
        /// </summary>
        public static readonly RCColor DEFAULT_MASK_COLOR = new RCColor(0, 255, 255);
    }
}
