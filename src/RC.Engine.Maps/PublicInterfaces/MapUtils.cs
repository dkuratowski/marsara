using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.Core;

namespace RC.Engine.Maps.PublicInterfaces
{
    /// <summary>
    /// Contains utility methods for maps.
    /// </summary>
    public static class MapUtils
    {
        /// <summary>
        /// Converts the distance given in quadratic tiles to a distance given in cells.
        /// </summary>
        /// <param name="quadDistance">The distance given in quadratic tiles.</param>
        /// <returns>The distance given in cells.</returns>
        public static RCNumber QuadToCellDistance(RCNumber quadDistance)
        {
            if (quadDistance < 0) { throw new ArgumentOutOfRangeException("quadDistance", "Distance cannot be negative!"); }
            return quadDistance * MapStructure.NAVCELL_PER_QUAD;
        }

        /// <summary>
        /// Converts the distance given in cells to a distance given in quadratic tiles.
        /// </summary>
        /// <param name="cellDistance">The distance given in cells.</param>
        /// <returns>The distance given in quadratic tiles.</returns>
        public static RCNumber CellToQuadDistance(RCNumber cellDistance)
        {
            if (cellDistance < 0) { throw new ArgumentOutOfRangeException("cellDistance", "Distance cannot be negative!"); }
            return cellDistance / MapStructure.NAVCELL_PER_QUAD;
        }

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
            return (horz < vert ? horz : vert) * RCNumber.ROOT_OF_TWO + diff;
        }

        /// <summary>
        /// Computes the distance between two rectangles on the map.
        /// </summary>
        /// <param name="fromRectangle">The first rectangle on the map.</param>
        /// <param name="toRectangle">The second rectangle on the map.</param>
        /// <returns>The computed distance between the given rectangles.</returns>
        public static RCNumber ComputeDistance(RCNumRectangle fromRectangle, RCNumRectangle toRectangle)
        {
            /// The distance is 0 in case of intersection.
            if (fromRectangle.IntersectsWith(toRectangle)) { return 0; }

            /// Calculate the relative position.
            bool isOnLeftSide = fromRectangle.Right <= toRectangle.Left;
            bool isOnRightSide = toRectangle.Right <= fromRectangle.Left;
            bool isAbove = fromRectangle.Bottom <= toRectangle.Top;
            bool isBelow = toRectangle.Bottom <= fromRectangle.Top;

            /// Handle the 8 possible cases.
            if (isOnLeftSide && isAbove && !isOnRightSide && !isBelow)
            {
                return MapUtils.ComputeDistance(
                    new RCNumVector(fromRectangle.Right, fromRectangle.Bottom),
                    new RCNumVector(toRectangle.Left, toRectangle.Top));
            }
            else if (isOnRightSide && isAbove && !isOnLeftSide && !isBelow)
            {
                return MapUtils.ComputeDistance(
                    new RCNumVector(fromRectangle.Left, fromRectangle.Bottom),
                    new RCNumVector(toRectangle.Right, toRectangle.Top));
            }
            else if (isOnLeftSide && isBelow && !isOnRightSide && !isAbove)
            {
                return MapUtils.ComputeDistance(
                    new RCNumVector(fromRectangle.Right, fromRectangle.Top),
                    new RCNumVector(toRectangle.Left, toRectangle.Bottom));
            }
            else if (isOnRightSide && isBelow && !isOnLeftSide && !isAbove)
            {
                return MapUtils.ComputeDistance(
                    new RCNumVector(fromRectangle.Left, fromRectangle.Top),
                    new RCNumVector(toRectangle.Right, toRectangle.Bottom));
            }
            else if (isAbove && !isOnLeftSide && !isOnRightSide && !isBelow)
            {
                return toRectangle.Top - fromRectangle.Bottom;
            }
            else if (isOnRightSide && !isOnLeftSide && !isAbove && !isBelow)
            {
                return fromRectangle.Left - toRectangle.Right;
            }
            else if (isBelow && !isOnLeftSide && !isOnRightSide && !isAbove)
            {
                return fromRectangle.Top - toRectangle.Bottom;
            }
            else if (isOnLeftSide && !isOnRightSide && !isAbove && !isBelow)
            {
                return toRectangle.Left - fromRectangle.Right;
            }
            else
            {
                throw new InvalidOperationException("Impossible case!");
            }
        }
    }
}
