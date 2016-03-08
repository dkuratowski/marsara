using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Engine.Pathfinder.Core
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
        /// The mapping of directions to vectors.
        /// </summary>
        public static readonly RCIntVector[] DIRECTION_TO_VECTOR = new RCIntVector[DIRECTION_COUNT]
        {
            new RCIntVector(0, -1),    // NORTH
            new RCIntVector(1, -1),    // NORTH_EAST
            new RCIntVector(1, 0),     // EAST
            new RCIntVector(1, 1),     // SOUTH_EAST
            new RCIntVector(0, 1),     // SOUTH
            new RCIntVector(-1, 1),    // SOUTH_WEST
            new RCIntVector(-1, 0),    // WEST
            new RCIntVector(-1, -1)    // NORTH_WEST
        };

        /// <summary>
        /// The mapping of directions to step lengths.
        /// </summary>
        public static readonly RCNumber[] DIRECTION_TO_STEPLENGTH = new RCNumber[DIRECTION_COUNT]
        {
            1,                      // NORTH
            RCNumber.ROOT_OF_TWO,   // NORTH_EAST
            1,                      // EAST
            RCNumber.ROOT_OF_TWO,   // SOUTH_EAST
            1,                      // SOUTH
            RCNumber.ROOT_OF_TWO,   // SOUTH_WEST
            1,                      // WEST
            RCNumber.ROOT_OF_TWO,   // NORTH_WEST
        };

        /// <summary>
        /// The mapping of vectors to directions.
        /// </summary>
        /// <param name="vector">The vector to map.</param>
        /// <returns>
        /// The vector is parallel with the X- and Y-axis -> NORTH/EAST/SOUTH/WEST
        /// The vector is not parallel with the X- and Y-axis -> NORTH_EAST/SOUTH_EAST/SOUTH_WEST/NORTH_WEST
        /// The vector is null vector -> -1
        /// </returns>
        public static int VECTOR_TO_DIRECTION(RCIntVector vector)
        {
            return VECTOR_TO_DIRECTION_INTERNAL[Math.Sign(vector.X) + 1, Math.Sign(vector.Y) + 1];
        }

        /// <summary>
        /// The internal array that stores the vector-to-direction mappings.
        /// </summary>
        private static int[,] VECTOR_TO_DIRECTION_INTERNAL = new int[3, 3]
        {
            { NORTH_WEST, WEST, SOUTH_WEST },
            { NORTH, -1, SOUTH },
            { NORTH_EAST, EAST, SOUTH_EAST }
        };
    }
}
