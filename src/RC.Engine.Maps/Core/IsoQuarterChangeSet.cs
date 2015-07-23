using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Maps.PublicInterfaces;
using RC.Common;

namespace RC.Engine.Maps.Core
{
    /// <summary>
    /// This changeset overwrites a specific field in a given quarter of an isometric tile.
    /// This changeset can only be applied to isometric tiles.
    /// </summary>
    class IsoQuarterChangeSet : CellDataChangeSetBase
    {
        /// <summary>
        /// Constructs a changeset for overwriting an integer field.
        /// </summary>
        /// <param name="targetQuarter">The quarter of the isometric tile to perform the changeset.</param>
        /// <param name="modifier">Reference to the modifier.</param>
        /// <param name="tileset">The tileset of this changeset.</param>
        public IsoQuarterChangeSet(MapDirection targetQuarter, ICellDataModifier modifier, TileSet tileset)
            : base(modifier, tileset)
        {
            this.CheckAndAssignCtorParams(targetQuarter);
        }

        /// <see cref="CellDataChangeSetBase.CollectTargetSet"/>
        protected override RCSet<RCIntVector> CollectTargetSet(ICellDataChangeSetTarget target)
        {
            RCSet<RCIntVector> targetset = new RCSet<RCIntVector>();
            for (int x = 0; x < target.CellSize.X; x++)
            {
                for (int y = 0; y < target.CellSize.Y; y++)
                {
                    RCNumVector cellIsoCoordsDbl = MapStructure.NavCellIsoTransform.TransformAB(new RCNumVector(x, y)) * 2;
                    bool isCellInQuarter = false;
                    if (this.targetQuarter == MapDirection.North)
                    {
                        isCellInQuarter = cellIsoCoordsDbl.X >= -1 && cellIsoCoordsDbl.X < 0 && cellIsoCoordsDbl.Y >= -1 && cellIsoCoordsDbl.Y < 0;
                    }
                    else if (this.targetQuarter == MapDirection.East)
                    {
                        isCellInQuarter = cellIsoCoordsDbl.X >= 0 && cellIsoCoordsDbl.X < 1 && cellIsoCoordsDbl.Y >= -1 && cellIsoCoordsDbl.Y < 0;
                    }
                    else if (this.targetQuarter == MapDirection.South)
                    {
                        isCellInQuarter = cellIsoCoordsDbl.X >= 0 && cellIsoCoordsDbl.X < 1 && cellIsoCoordsDbl.Y >= 0 && cellIsoCoordsDbl.Y < 1;
                    }
                    else if (this.targetQuarter == MapDirection.West)
                    {
                        isCellInQuarter = cellIsoCoordsDbl.X >= -1 && cellIsoCoordsDbl.X < 0 && cellIsoCoordsDbl.Y >= 0 && cellIsoCoordsDbl.Y < 1;
                    }
                    else
                    {
                        throw new MapException("Unexpected quarter!");
                    }

                    if (isCellInQuarter)
                    {
                        RCIntVector index = new RCIntVector(x, y);
                        if (target.GetCell(index) != null)
                        {
                            targetset.Add(index);
                        }
                    }
                }
            }
            return targetset;
        }

        /// <summary>
        /// Checks and assigns the parameters coming from the constructor.
        /// </summary>
        /// <param name="targetQuarter">The target quarter of this changeset.</param>
        private void CheckAndAssignCtorParams(MapDirection targetQuarter)
        {
            if (targetQuarter != MapDirection.North &&
                targetQuarter != MapDirection.East &&
                targetQuarter != MapDirection.South &&
                targetQuarter != MapDirection.West)
            {
                throw new ArgumentException("The target quarter must be one of the followings: MapDirection.North, MapDirection.East, MapDirection.South or MapDirection.West!", "targetQuarter");
            }

            this.targetQuarter = targetQuarter;
        }

        /// <summary>
        /// The target quarter of this changeset.
        /// </summary>
        private MapDirection targetQuarter;
    }
}
