using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.Engine;

namespace RC.Engine.Simulator.PlacementConstraints
{
    /// <summary>
    /// This entity constraint checks whether the checked area on the map is exactly over a VespeneGeyser.
    /// </summary>
    public class VespeneGeyserConstraint : EntityPlacementConstraint
    {
        /// <see cref="EntityPlacementConstraint.CheckImpl"/>
        protected override RCSet<RCIntVector> CheckImpl(Scenario scenario, RCIntVector position, RCSet<Entity> entitiesToIgnore)
        {
            RCIntRectangle objArea = new RCIntRectangle(position, scenario.Map.CellToQuadSize(this.EntityType.Area.Read()));
            RCSet<RCIntVector> retList = new RCSet<RCIntVector>();
            VespeneGeyser foundVespeneGeyser = null;
            bool isOK = true;
            for (int absY = objArea.Top; absY < objArea.Bottom; absY++)
            {
                for (int absX = objArea.Left; absX < objArea.Right; absX++)
                {
                    RCIntVector absQuadCoords = new RCIntVector(absX, absY);
                    if (absQuadCoords.X >= 0 && absQuadCoords.X < scenario.Map.Size.X &&
                        absQuadCoords.Y >= 0 && absQuadCoords.Y < scenario.Map.Size.Y)
                    {
                        retList.Add(absQuadCoords - position);
                        VespeneGeyser vespeneGeyserAtCoords = scenario.GetFixedEntity<VespeneGeyser>(absQuadCoords);
                        if (vespeneGeyserAtCoords == null || (foundVespeneGeyser != null && foundVespeneGeyser != vespeneGeyserAtCoords))
                        {
                            /// There is no VespeneGeyser at the given coordinates OR
                            /// the VespeneGeyser at the given coordinates is another VespeneGeyser.
                            isOK = false;
                            continue;
                        }
                        foundVespeneGeyser = vespeneGeyserAtCoords;
                    }
                }
            }
            if (isOK) { retList.Clear(); }
            return retList;
        }
    }
}
