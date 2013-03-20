using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Configuration;
using RC.Common.Diagnostics;

namespace RC.App.BizLogic.Core
{
    /// <summary>
    /// This static class is used to access the constants of the RC.App.BizLogic module.
    /// </summary>
    static class BizLogicConstants
    {
        /// <summary>
        /// The directory of the tilesets.
        /// </summary>
        public static readonly string TILESET_DIR = ConstantsTable.Get<string>("RC.App.BizLogic.TileSetDir");

        /// <summary>
        /// Name of the tile variant property that stores the transparent color.
        /// </summary>
        public const string TILEPROP_TRANSPARENTCOLOR = "TransparentColor";

        /// <summary>
        /// Number of pixels per cells in both horizontal and vertical direction.
        /// </summary>
        public const int PIXEL_PER_NAVCELL = 4;
    }

    /// <summary>
    /// This static class is used to access the trace filters defined for the RC.App.BizLogic module.
    /// </summary>
    static class BizLogicTraceFilters
    {
        public static readonly int ERROR = TraceManager.GetTraceFilterID("RC.App.BizLogic.Error");
        public static readonly int WARNING = TraceManager.GetTraceFilterID("RC.App.BizLogic.Warning");
        public static readonly int INFO = TraceManager.GetTraceFilterID("RC.App.BizLogic.Info");
        public static readonly int DETAILS = TraceManager.GetTraceFilterID("RC.App.BizLogic.Details");
    }
}
