using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Prototypes.NewPathfinding.MotionControl
{
    /// <summary>
    /// Defines constants for managing directions on the grid.
    /// </summary>
    static class GridDirections
    {
        /// <summary>
        /// The direction indices.
        /// </summary>
        public const int NORTH = 0;
        public const int NORTH_EAST = 1;
        public const int EAST = 2;
        public const int SOUTH_EAST = 3;
        public const int SOUTH = 4;
        public const int SOUTH_WEST = 5;
        public const int WEST = 6;
        public const int NORTH_WEST = 7;
        public const int DIRECTION_COUNT = 8;

        /// <summary>
        /// The direction vectors.
        /// </summary>
        public static readonly Size[] DIRECTION_VECTOR = new Size[DIRECTION_COUNT]
        {
            new Size(0, -1),    // NORTH
            new Size(1, -1),    // NORTH_EAST
            new Size(1, 0),     // EAST
            new Size(1, 1),     // SOUTH_EAST
            new Size(0, 1),     // SOUTH
            new Size(-1, 1),    // SOUTH_WEST
            new Size(-1, 0),    // WEST
            new Size(-1, -1)    // NORTH_WEST
        };
    }
}
