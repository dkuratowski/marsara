using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.PresLogic
{
    /// <summary>
    /// Enumerates the possible map-scrolling directions.
    /// </summary>
    public enum MapScrollDirection
    {
        NoScroll = -1,
        North = 0,
        NorthEast = 1,
        East = 2,
        SouthEast = 3,
        South = 4,
        SouthWest = 5,
        West = 6,
        NorthWest = 7
    }
}
