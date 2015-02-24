using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Views;
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

        /// <summary>
        /// Defines the colors of the players.
        /// </summary>
        public static readonly Dictionary<PlayerEnum, RCColor> PLAYER_COLOR_MAPPINGS = new Dictionary<PlayerEnum, RCColor>()
        {
            { PlayerEnum.Neutral, RCColor.Black },
            { PlayerEnum.Player0, RCColor.Red },
            { PlayerEnum.Player1, RCColor.Blue },
            { PlayerEnum.Player2, RCColor.Cyan },
            { PlayerEnum.Player3, RCColor.Magenta },
            { PlayerEnum.Player4, RCColor.LightMagenta },
            { PlayerEnum.Player5, RCColor.Green },
            { PlayerEnum.Player6, RCColor.WhiteHigh },
            { PlayerEnum.Player7, RCColor.Yellow }
        };
    }
}
