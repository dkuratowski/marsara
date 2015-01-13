using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Scenarios
{
    /// <summary>
    /// This entity constraint checks whether the checked area on the map is enough distance from entities of a given type.
    /// </summary>
    /// <typeparam name="T">The type of the entities from which the distance is minimized by this constraint.</typeparam>
    public class MinimumDistanceConstraint<T> : EntityConstraint where T : Entity
    {
        /// <summary>
        /// Constructs a MinimumDistanceConstraint instance.
        /// </summary>
        /// <param name="minimumDistance">The minimum distance in horizontal and vertical direction.</param>
        public MinimumDistanceConstraint(RCIntVector minimumDistance)
        {
            if (minimumDistance == RCIntVector.Undefined) { throw new ArgumentNullException("minimumDistance"); }
            if (minimumDistance.X < 0 || minimumDistance.Y < 0) { throw new ArgumentOutOfRangeException("minimumDistance"); }

            this.minimumDistance = minimumDistance;
            this.checkedQuadRectSize = new RCIntVector(2 * this.minimumDistance.X + 1, 2 * this.minimumDistance.Y + 1);
        }

        /// <see cref="EntityConstraint.CheckImpl"/>
        protected override HashSet<RCIntVector> CheckImpl(Scenario scenario, RCIntVector position)
        {
            RCIntRectangle objArea = new RCIntRectangle(position, scenario.Map.CellToQuadSize(this.EntityType.Area.Read()));
            HashSet<RCIntVector> retList = new HashSet<RCIntVector>();
            for (int absY = objArea.Top; absY < objArea.Bottom; absY++)
            {
                for (int absX = objArea.Left; absX < objArea.Right; absX++)
                {
                    RCIntVector absQuadCoords = new RCIntVector(absX, absY);
                    if (absQuadCoords.X >= 0 && absQuadCoords.X < scenario.Map.Size.X &&
                        absQuadCoords.Y >= 0 && absQuadCoords.Y < scenario.Map.Size.Y)
                    {
                        RCIntRectangle checkedQuadRect = new RCIntRectangle(absQuadCoords - this.minimumDistance, this.checkedQuadRectSize);
                        RCNumRectangle checkedArea = (RCNumRectangle)scenario.Map.QuadToCellRect(checkedQuadRect) - new RCNumVector(1, 1) / 2;
                        HashSet<T> objectsTooClose = scenario.GetEntitiesOnMap<T>(checkedArea);
                        if (objectsTooClose.Count != 0) { retList.Add(absQuadCoords - position); }
                    }
                }
            }
            return retList;
        }

        /// <summary>
        /// Rectangle computed from the minimum distance.
        /// </summary>
        private RCIntVector checkedQuadRectSize;

        /// <summary>
        /// The minimum distance in horizontal and vertical directions.
        /// </summary>
        private RCIntVector minimumDistance;
    }
}
