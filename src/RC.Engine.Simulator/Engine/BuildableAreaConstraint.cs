using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// This entity constraint checks whether the checked area on the map is buildable or not.
    /// </summary>
    public class BuildableAreaConstraint : EntityConstraint
    {
        /// <see cref="EntityConstraint.CheckImpl"/>
        protected override RCSet<RCIntVector> CheckImpl(Scenario scenario, RCIntVector position)
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
                        IQuadTile checkedQuadTile = scenario.Map.GetQuadTile(absQuadCoords);
                        if (!checkedQuadTile.IsBuildable) { retList.Add(absQuadCoords - position); }
                    }
                }
            }
            return retList;
        }
    }
}
