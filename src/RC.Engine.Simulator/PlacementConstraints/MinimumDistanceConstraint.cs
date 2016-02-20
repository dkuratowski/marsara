using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.Engine;

namespace RC.Engine.Simulator.PlacementConstraints
{
    /// <summary>
    /// This entity constraint checks whether the checked area on the map is enough distance from entities of a given type.
    /// </summary>
    /// <typeparam name="T">The type of the entities from which the distance is minimized by this constraint.</typeparam>
    public class MinimumDistanceConstraint<T> : EntityPlacementConstraint where T : Entity
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

        /// <see cref="EntityPlacementConstraint.CheckImpl"/>
        protected override RCSet<RCIntVector> CheckImpl(Scenario scenario, RCIntVector position, RCSet<Entity> entitiesToIgnore)
        {
            RCIntRectangle objArea = new RCIntRectangle(position, scenario.Map.CellToQuadSize(this.EntityType.Area.Read()));
            RCSet<RCIntVector> retList = new RCSet<RCIntVector>();
            for (int absY = objArea.Top; absY < objArea.Bottom; absY++)
            {
                for (int absX = objArea.Left; absX < objArea.Right; absX++)
                {
                    RCIntVector absQuadCoords = new RCIntVector(absX, absY);
                    if (absQuadCoords.X >= 0 && absQuadCoords.X < scenario.Map.Size.X &&
                        absQuadCoords.Y >= 0 && absQuadCoords.Y < scenario.Map.Size.Y)
                    {
                        /// Collect all the entities that are on the ground, are too close and not to be ignored.
                        RCIntRectangle checkedQuadRect = new RCIntRectangle(absQuadCoords - this.minimumDistance, this.checkedQuadRectSize);
                        RCNumRectangle checkedArea = (RCNumRectangle)scenario.Map.QuadToCellRect(checkedQuadRect) - new RCNumVector(1, 1) / 2;
                        RCSet<T> entitiesTooClose = scenario.GetElementsOnMap<T>(checkedArea, MapObjectLayerEnum.GroundObjects);
                        if (entitiesTooClose.Any(entityTooClose => !entitiesToIgnore.Contains(entityTooClose)))
                        {
                            retList.Add(absQuadCoords - position);
                        }
                    }
                }
            }
            return retList;
        }

        /// <summary>
        /// Rectangle computed from the minimum distance.
        /// </summary>
        private readonly RCIntVector checkedQuadRectSize;

        /// <summary>
        /// The minimum distance in horizontal and vertical directions.
        /// </summary>
        private readonly RCIntVector minimumDistance;
    }
}
