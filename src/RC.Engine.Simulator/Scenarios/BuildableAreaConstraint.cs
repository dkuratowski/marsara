using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Scenarios
{
    /// <summary>
    /// This entity constraint checks whether the checked area on the map is buildable or not.
    /// </summary>
    public class BuildableAreaConstraint : EntityConstraint
    {
        /// <see cref="EntityConstraint.CheckImpl"/>
        protected override HashSet<RCIntVector> CheckImpl(Scenario scenario, RCIntVector position)
        {
            int isBuildableFieldIdx = scenario.Map.Tileset.GetCellDataFieldIndex("IsBuildable");
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
                        IQuadTile checkedQuadTile = scenario.Map.GetQuadTile(absQuadCoords);
                        bool isBuildable = true;
                        for (int row = 0; row < checkedQuadTile.CellSize.Y; row++)
                        {
                            for (int col = 0; col < checkedQuadTile.CellSize.X; col++)
                            {
                                ICell checkedCell = checkedQuadTile.GetCell(new RCIntVector(col, row));
                                if (!checkedCell.Data.ReadBool(isBuildableFieldIdx))
                                {
                                    isBuildable = false;
                                    break;
                                }
                            }
                        }
                        if (!isBuildable) { retList.Add(absQuadCoords - position); }
                    }
                }
            }
            return retList;
        }
    }
}
