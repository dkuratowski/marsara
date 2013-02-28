using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.PublicInterfaces
{
    /// <summary>
    /// Enumerates the possible directions on a map.
    /// </summary>
    public enum MapDirection
    {
        [EnumMapping("North")]
        North = 0,

        [EnumMapping("NorthEast")]
        NorthEast = 1,

        [EnumMapping("East")]
        East = 2,

        [EnumMapping("SouthEast")]
        SouthEast = 3,

        [EnumMapping("South")]
        South = 4,

        [EnumMapping("SouthWest")]
        SouthWest = 5,

        [EnumMapping("West")]
        West = 6,

        [EnumMapping("NorthWest")]
        NorthWest = 7
    }
}
