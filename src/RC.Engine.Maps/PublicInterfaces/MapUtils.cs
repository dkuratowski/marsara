using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Maps.PublicInterfaces
{
    /// <summary>
    /// Contains utility methods for maps.
    /// </summary>
    public static class MapUtils
    {
        /// <summary>
        /// Computes the distance between two points on the map.
        /// </summary>
        /// <param name="fromCoords">The first point on the map.</param>
        /// <param name="toCoords">The second point on the map.</param>
        /// <returns>The computed distance between the given points.</returns>
        public static RCNumber ComputeDistance(RCNumVector fromCoords, RCNumVector toCoords)
        {
            RCNumber horz = (toCoords.X - fromCoords.X).Abs();
            RCNumber vert = (toCoords.Y - fromCoords.Y).Abs();
            RCNumber diff = (horz - vert).Abs();
            return (horz < vert ? horz : vert) * ROOT_OF_TWO + diff;
        }

        /// <summary>
        /// The square root of 2 used in distance calculations.
        /// </summary>
        private static readonly RCNumber ROOT_OF_TWO = (RCNumber)14142 / (RCNumber)10000;
    }
}
