using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine
{
    /// <summary>
    /// Interface of tile data changeset objects.
    /// </summary>
    public interface ICellDataChangeSet
    {
        /// <summary>
        /// Applies the changeset on the navigation cells of the given isometric tile.
        /// </summary>
        /// <param name="target">The target isometric tile of the changeset.</param>
        void Apply(IIsoTile target);

        /// <summary>
        /// Applies the changeset on the navigation cells of the given terrain object.
        /// </summary>
        /// <param name="target">The target terrain object of the changeset.</param>
        //void Apply(ITerrainObject target); TODO!

        /// <summary>
        /// Gets the tileset of this changeset.
        /// </summary>
        TileSet Tileset { get; }
    }

    /// <summary>
    /// This changeset overwrites a specific field in every cells of the target.
    /// </summary>
    public class CellDataChangeSetBase : ICellDataChangeSet
    {
        /// <summary>
        /// Constructs a changeset for overwriting an integer field.
        /// </summary>
        /// <param name="targetField">The name of the target field.</param>
        /// <param name="value">The new value of the target field.</param>
        /// <param name="tileset">The tileset of this changeset.</param>
        public CellDataChangeSetBase(string targetField, int value, TileSet tileset)
        {
            if (tileset == null) { throw new ArgumentNullException("tileset"); }

            this.targetFieldIdx = tileset.GetFieldIndex(targetField);
            this.targetField = targetField;
            this.targetFieldType = tileset.GetFieldType(this.targetFieldIdx);
            if (this.targetFieldType != CellDataType.INT) { throw new TileSetException(string.Format("Field '{0}' was not declared as CellDataType.INT!", targetField)); }
            this.intValue = value;
            this.tileset = tileset;
        }

        /// <summary>
        /// Constructs a changeset for overwriting a bool field.
        /// </summary>
        /// <param name="targetField">The name of the target field.</param>
        /// <param name="value">The new value of the target field.</param>
        /// <param name="tileset">The tileset of this changeset.</param>
        public CellDataChangeSetBase(string targetField, bool value, TileSet tileset)
        {
            if (tileset == null) { throw new ArgumentNullException("tileset"); }

            this.targetFieldIdx = tileset.GetFieldIndex(targetField);
            this.targetField = targetField;
            this.targetFieldType = tileset.GetFieldType(this.targetFieldIdx);
            if (this.targetFieldType != CellDataType.BOOL) { throw new TileSetException(string.Format("Field '{0}' was not declared as CellDataType.BOOL!", targetField)); }
            this.boolValue = value;
            this.tileset = tileset;
        }

        /// <summary>
        /// Gets the name of the field that this changeset overwrites.
        /// </summary>
        public string TargetField { get { return this.targetField; } }

        /// <summary>
        /// Gets the index of the field that this changeset overwrites.
        /// </summary>
        public int TargetFieldIdx { get { return this.targetFieldIdx; } }

        /// <summary>
        /// Gets the type of the field that this changeset overwrites.
        /// </summary>
        public CellDataType TargetFieldType { get { return this.targetFieldType; } }

        /// <summary>
        /// Gets the target value of the field that this changeset overwrites (in case of CellDataType.INT).
        /// </summary>
        public int IntValue { get { return this.intValue; } }

        /// <summary>
        /// Gets the target value of the field that this changeset overwrites (in case of CellDataType.BOOL).
        /// </summary>
        public bool BoolValue { get { return this.boolValue; } }

        /// <see cref="ICellDataChangeSet.Tileset"/>
        public TileSet Tileset { get { return this.tileset; } }

        /// <see cref="ICellDataChangeSet.Apply"/>
        public virtual void Apply(IIsoTile target)
        {
            for (int x = 0; x < Map.QUAD_PER_ISO_VERT * Map.NAVCELL_PER_QUAD; x++)
            {
                for (int y = 0; y < Map.QUAD_PER_ISO_HORZ * Map.NAVCELL_PER_QUAD; y++)
                {
                    this.ApplyOnCell(target, new RCIntVector(x, y));
                }
            }
        }

        /// <summary>
        /// Applies the changeset on the navigation cell of the given isometric tile at the given coordinates. If there
        /// is no cell at the given coordinates then this method has no effect.
        /// </summary>
        /// <param name="target">The target isometric tile of the operation.</param>
        /// <param name="coords">The coordinates of the navigation cell to be applied.</param>
        protected void ApplyOnCell(IIsoTile target, RCIntVector coords)
        {
            INavCell cell = target.GetNavCell(coords);
            if (cell != null)
            {
                if (this.targetFieldType == CellDataType.INT)
                {
                    cell.Data.WriteInt(this.targetFieldIdx, this.intValue);
                }
                else if (this.targetFieldType == CellDataType.BOOL)
                {
                    cell.Data.WriteBool(this.targetFieldIdx, this.boolValue);
                }
                else
                {
                    throw new RCEngineException("Unknown field type!");
                }
            }
        }

        /// <summary>
        /// The name of the field that this changeset overwrites.
        /// </summary>
        private string targetField;

        /// <summary>
        /// The index of the field that this changeset overwrites.
        /// </summary>
        private int targetFieldIdx;

        /// <summary>
        /// The type of the field that this changeset overwrites.
        /// </summary>
        private CellDataType targetFieldType;

        /// <summary>
        /// The target value of the field that this changeset overwrites (in case of CellDataType.INT).
        /// </summary>
        private int intValue;

        /// <summary>
        /// The target value of the field that this changeset overwrites (in case of CellDataType.BOOL).
        /// </summary>
        private bool boolValue;

        /// <summary>
        /// The tileset of this changeset.
        /// </summary>
        private TileSet tileset;
    }

    /// <summary>
    /// This changeset overwrites a specific field in a given quarter of an isometric tile.
    /// This changeset cannot be applied to terrain objects.
    /// </summary>
    public class IsoQuarterChangeSet : CellDataChangeSetBase
    {
        /// <summary>
        /// Constructs a changeset for overwriting an integer field.
        /// </summary>
        /// <param name="targetQuarter">The quarter of the isometric tile to perform the changeset.</param>
        /// <param name="targetField">The name of the target field.</param>
        /// <param name="value">The new value of the target field.</param>
        /// <param name="tileset">The tileset of this changeset.</param>
        public IsoQuarterChangeSet(MapDirection targetQuarter, string targetField, int value, TileSet tileset)
            : base(targetField, value, tileset)
        {
            this.CheckAndAssignCtorParams(targetQuarter);
        }

        /// <summary>
        /// Constructs a changeset for overwriting a bool field.
        /// </summary>
        /// <param name="targetQuarter">The quarter of the isometric tile to perform the changeset.</param>
        /// <param name="targetField">The name of the target field.</param>
        /// <param name="value">The new value of the target field.</param>
        /// <param name="tileset">The tileset of this changeset.</param>
        public IsoQuarterChangeSet(MapDirection targetQuarter, string targetField, bool value, TileSet tileset)
            : base(targetField, value, tileset)
        {
            this.CheckAndAssignCtorParams(targetQuarter);
        }

        /// <summary>
        /// Gets the target quarter of this changeset.
        /// </summary>
        public MapDirection TargetQuarter { get { return this.targetQuarter; } }

        /// <see cref="ICellDataChangeSet.Apply"/>
        public override void Apply(IIsoTile target)
        {
            for (int x = 0; x < Map.QUAD_PER_ISO_VERT * Map.NAVCELL_PER_QUAD; x++)
            {
                for (int y = 0; y < Map.QUAD_PER_ISO_HORZ * Map.NAVCELL_PER_QUAD; y++)
                {
                    RCNumVector cellIsoCoordsDbl = Map.NavCellIsoTransform.TransformAB(new RCNumVector(x, y)) * 2;
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
                        throw new RCEngineException("Unexpected quarter!");
                    }

                    if (isCellInQuarter) { this.ApplyOnCell(target, new RCIntVector(x, y)); }                    
                }
            }
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

    /// <summary>
    /// This changeset overwrites a specific field in a given row of the target.
    /// </summary>
    public class RowChangeSet : CellDataChangeSetBase
    {
        /// <summary>
        /// Constructs a changeset for overwriting an integer field.
        /// </summary>
        /// <param name="targetRow">The row of the target to perform the changeset.</param>
        /// <param name="targetField">The name of the target field.</param>
        /// <param name="value">The new value of the target field.</param>
        /// <param name="tileset">The tileset of this changeset.</param>
        public RowChangeSet(int targetRow, string targetField, int value, TileSet tileset)
            : base(targetField, value, tileset)
        {
            this.CheckAndAssignCtorParams(targetRow);
        }

        /// <summary>
        /// Constructs a changeset for overwriting a bool field.
        /// </summary>
        /// <param name="targetRow">The row of the target to perform the changeset.</param>
        /// <param name="targetField">The name of the target field.</param>
        /// <param name="value">The new value of the target field.</param>
        /// <param name="tileset">The tileset of this changeset.</param>
        public RowChangeSet(int targetRow, string targetField, bool value, TileSet tileset)
            : base(targetField, value, tileset)
        {
            this.CheckAndAssignCtorParams(targetRow);
        }

        /// <summary>
        /// Gets the target row of this changeset.
        /// </summary>
        public int TargetRow { get { return this.targetRow; } }

        /// <see cref="ICellDataChangeSet.Apply"/>
        public override void Apply(IIsoTile target)
        {
            for (int x = 0; x < Map.QUAD_PER_ISO_VERT * Map.NAVCELL_PER_QUAD; x++)
            {
                this.ApplyOnCell(target, new RCIntVector(x, this.targetRow));
            }
        }

        /// <summary>
        /// Checks and assigns the parameters coming from the constructor.
        /// </summary>
        /// <param name="targetRow">The target row of this changeset.</param>
        private void CheckAndAssignCtorParams(int targetRow)
        {
            /// TODO: check the parameter.
            this.targetRow = targetRow;
        }

        /// <summary>
        /// The target row of this changeset.
        /// </summary>
        private int targetRow;
    }

    /// <summary>
    /// This changeset overwrites a specific field in a given column of the target.
    /// </summary>
    public class ColumnChangeSet : CellDataChangeSetBase
    {
        /// <summary>
        /// Constructs a changeset for overwriting an integer field.
        /// </summary>
        /// <param name="targetCol">The column of the target to perform the changeset.</param>
        /// <param name="targetField">The name of the target field.</param>
        /// <param name="value">The new value of the target field.</param>
        /// <param name="tileset">The tileset of this changeset.</param>
        public ColumnChangeSet(int targetCol, string targetField, int value, TileSet tileset)
            : base(targetField, value, tileset)
        {
            this.CheckAndAssignCtorParams(targetCol);
        }

        /// <summary>
        /// Constructs a changeset for overwriting a bool field.
        /// </summary>
        /// <param name="targetCol">The column of the target to perform the changeset.</param>
        /// <param name="targetField">The name of the target field.</param>
        /// <param name="value">The new value of the target field.</param>
        /// <param name="tileset">The tileset of this changeset.</param>
        public ColumnChangeSet(int targetCol, string targetField, bool value, TileSet tileset)
            : base(targetField, value, tileset)
        {
            this.CheckAndAssignCtorParams(targetCol);
        }

        /// <summary>
        /// Gets the target column of this changeset.
        /// </summary>
        public int TargetCol { get { return this.targetCol; } }

        /// <see cref="ICellDataChangeSet.Apply"/>
        public override void Apply(IIsoTile target)
        {
            for (int y = 0; y < Map.QUAD_PER_ISO_HORZ * Map.NAVCELL_PER_QUAD; y++)
            {
                this.ApplyOnCell(target, new RCIntVector(this.targetCol, y));
            }
        }

        /// <summary>
        /// Checks and assigns the parameters coming from the constructor.
        /// </summary>
        /// <param name="targetCol">The target column of this changeset.</param>
        private void CheckAndAssignCtorParams(int targetCol)
        {
            /// TODO: check the parameter.
            this.targetCol = targetCol;
        }

        /// <summary>
        /// The target column of this changeset.
        /// </summary>
        private int targetCol;
    }

    /// <summary>
    /// This changeset overwrites a specific field in a given cell of the target.
    /// </summary>
    public class CellChangeSet : CellDataChangeSetBase
    {
        /// <summary>
        /// Constructs a changeset for overwriting an integer field.
        /// </summary>
        /// <param name="targetCell">The cell of the target to perform the changeset.</param>
        /// <param name="targetField">The name of the target field.</param>
        /// <param name="value">The new value of the target field.</param>
        /// <param name="tileset">The tileset of this changeset.</param>
        public CellChangeSet(RCIntVector targetCell, string targetField, int value, TileSet tileset)
            : base(targetField, value, tileset)
        {
            this.CheckAndAssignCtorParams(targetCell);
        }

        /// <summary>
        /// Constructs a changeset for overwriting a bool field.
        /// </summary>
        /// <param name="targetCell">The cell of the target to perform the changeset.</param>
        /// <param name="targetField">The name of the target field.</param>
        /// <param name="value">The new value of the target field.</param>
        /// <param name="tileset">The tileset of this changeset.</param>
        public CellChangeSet(RCIntVector targetCell, string targetField, bool value, TileSet tileset)
            : base(targetField, value, tileset)
        {
            this.CheckAndAssignCtorParams(targetCell);
        }

        /// <summary>
        /// Gets the target cell of this changeset.
        /// </summary>
        public RCIntVector TargetCell { get { return this.targetCell; } }

        /// <see cref="ICellDataChangeSet.Apply"/>
        public override void Apply(IIsoTile target)
        {
            this.ApplyOnCell(target, this.targetCell);
        }

        /// <summary>
        /// Checks and assigns the parameters coming from the constructor.
        /// </summary>
        /// <param name="targetCell">The target cell of this changeset.</param>
        private void CheckAndAssignCtorParams(RCIntVector targetCell)
        {
            if (targetCell == RCIntVector.Undefined) { throw new ArgumentNullException("targetCell"); }

            /// TODO: check the parameter.
            this.targetCell = targetCell;
        }

        /// <summary>
        /// The target cell of this changeset.
        /// </summary>
        private RCIntVector targetCell;
    }

    /// <summary>
    /// This changeset overwrites a specific field in a given rectangle of the target.
    /// </summary>
    public class RectangleChangeSet : CellDataChangeSetBase
    {
        /// <summary>
        /// Constructs a changeset for overwriting an integer field.
        /// </summary>
        /// <param name="targetRect">The rectangle of the target to perform the changeset.</param>
        /// <param name="targetField">The name of the target field.</param>
        /// <param name="value">The new value of the target field.</param>
        /// <param name="tileset">The tileset of this changeset.</param>
        public RectangleChangeSet(RCIntRectangle targetRect, string targetField, int value, TileSet tileset)
            : base(targetField, value, tileset)
        {
            this.CheckAndAssignCtorParams(targetRect);
        }

        /// <summary>
        /// Constructs a changeset for overwriting a bool field.
        /// </summary>
        /// <param name="targetRect">The rectangle of the target to perform the changeset.</param>
        /// <param name="targetField">The name of the target field.</param>
        /// <param name="value">The new value of the target field.</param>
        /// <param name="tileset">The tileset of this changeset.</param>
        public RectangleChangeSet(RCIntRectangle targetRect, string targetField, bool value, TileSet tileset)
            : base(targetField, value, tileset)
        {
            this.CheckAndAssignCtorParams(targetRect);
        }

        /// <summary>
        /// Gets the target rectangle of this changeset.
        /// </summary>
        public RCIntRectangle TargetRect { get { return this.targetRect; } }

        /// <see cref="ICellDataChangeSet.Apply"/>
        public override void Apply(IIsoTile target)
        {
            for (int x = this.targetRect.X; x < this.targetRect.Right; x++)
            {
                for (int y = this.targetRect.Y; y < this.targetRect.Bottom; y++)
                {
                    this.ApplyOnCell(target, new RCIntVector(x, y));
                }
            }
        }

        /// <summary>
        /// Checks and assigns the parameters coming from the constructor.
        /// </summary>
        /// <param name="targetRect">The target rectangle of this changeset.</param>
        private void CheckAndAssignCtorParams(RCIntRectangle targetRect)
        {
            if (targetRect == RCIntRectangle.Undefined) { throw new ArgumentNullException("targetRect"); }

            /// TODO: check the parameter.
            this.targetRect = targetRect;
        }

        /// <summary>
        /// The target rectangle of this changeset.
        /// </summary>
        private RCIntRectangle targetRect;
    }
}
