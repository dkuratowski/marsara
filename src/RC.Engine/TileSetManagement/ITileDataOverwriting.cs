using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine
{
    /// <summary>
    /// Interface of tile data overwriting operations.
    /// </summary>
    public interface ITileDataOverwriting
    {
        /// <summary>
        /// Applies the data overwriting operation on the navigation cells of the given isometric tile.
        /// </summary>
        /// <param name="target">The target isometric tile of the operation.</param>
        void Apply(IIsoTile target);

        /// <summary>
        /// Gets the tileset of this operation.
        /// </summary>
        TileSet Tileset { get; }
    }

    /// <summary>
    /// This operation overwrites a specific field in every cells of an isometric tile.
    /// </summary>
    public class TileDataOverwriting : ITileDataOverwriting
    {
        /// <summary>
        /// Constructs a TileDataOverwriting operation for overwriting an integer field.
        /// </summary>
        /// <param name="targetField">The name of the target field.</param>
        /// <param name="value">The new value of the target field.</param>
        /// <param name="tileset">The tileset of this operation.</param>
        public TileDataOverwriting(string targetField, int value, TileSet tileset)
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
        /// Constructs a TileDataOverwriting operation for overwriting a bool field.
        /// </summary>
        /// <param name="targetField">The name of the target field.</param>
        /// <param name="value">The new value of the target field.</param>
        /// <param name="tileset">The tileset of this operation.</param>
        public TileDataOverwriting(string targetField, bool value, TileSet tileset)
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
        /// Gets the name of the field to overwrite.
        /// </summary>
        public string TargetField { get { return this.targetField; } }

        /// <summary>
        /// Gets the index of the field to overwrite.
        /// </summary>
        public int TargetFieldIdx { get { return this.targetFieldIdx; } }

        /// <summary>
        /// Gets the type of the field to overwrite.
        /// </summary>
        public CellDataType TargetFieldType { get { return this.targetFieldType; } }

        /// <summary>
        /// Gets the value of the field to overwrite (in case of CellDataType.INT).
        /// </summary>
        public int IntValue { get { return this.intValue; } }

        /// <summary>
        /// Gets the value of the field to overwrite (in case of CellDataType.BOOL).
        /// </summary>
        public bool BoolValue { get { return this.boolValue; } }

        /// <see cref="ITileDataOverwriting.Tileset"/>
        public TileSet Tileset { get { return this.tileset; } }

        /// <see cref="ITileDataOverwriting.Apply"/>
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
        /// Applies the overwriting on the navigation cell of the given tile at the given coordinates. If there
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
        /// The name of the field to overwrite.
        /// </summary>
        private string targetField;

        /// <summary>
        /// The index of the field to overwrite.
        /// </summary>
        private int targetFieldIdx;

        /// <summary>
        /// The type of the field to overwrite.
        /// </summary>
        private CellDataType targetFieldType;

        /// <summary>
        /// The value of the field to overwrite (in case of CellDataType.INT).
        /// </summary>
        private int intValue;

        /// <summary>
        /// The value of the field to overwrite (in case of CellDataType.BOOL).
        /// </summary>
        private bool boolValue;

        /// <summary>
        /// The tileset of this operation.
        /// </summary>
        private TileSet tileset;
    }

    /// <summary>
    /// This operation overwrites a specific field in a given quarter of an isometric tile.
    /// </summary>
    public class QuarterOverwriting : TileDataOverwriting
    {
        /// <summary>
        /// Constructs a QuarterOverwriting operation for overwriting an integer field.
        /// </summary>
        /// <param name="targetQuarter">The quarter of the isometric tile to perform the operation.</param>
        /// <param name="targetField">The name of the target field.</param>
        /// <param name="value">The new value of the target field.</param>
        /// <param name="tileset">The tileset of this operation.</param>
        public QuarterOverwriting(MapDirection targetQuarter, string targetField, int value, TileSet tileset)
            : base(targetField, value, tileset)
        {
            this.CheckAndAssignCtorParams(targetQuarter);
        }

        /// <summary>
        /// Constructs a QuarterOverwriting operation for overwriting a bool field.
        /// </summary>
        /// <param name="targetQuarter">The quarter of the isometric tile to perform the operation.</param>
        /// <param name="targetField">The name of the target field.</param>
        /// <param name="value">The new value of the target field.</param>
        /// <param name="tileset">The tileset of this operation.</param>
        public QuarterOverwriting(MapDirection targetQuarter, string targetField, bool value, TileSet tileset)
            : base(targetField, value, tileset)
        {
            this.CheckAndAssignCtorParams(targetQuarter);
        }

        /// <summary>
        /// Gets the target quarter of this operation.
        /// </summary>
        public MapDirection TargetQuarter { get { return this.targetQuarter; } }

        /// <see cref="ITileDataOverwriting.Apply"/>
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
        /// <param name="targetQuarter">The target quarter of this operation.</param>
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
        /// The target quarter of this operation.
        /// </summary>
        private MapDirection targetQuarter;
    }

    /// <summary>
    /// This operation overwrites a specific field in a given row of an isometric tile.
    /// </summary>
    public class RowOverwriting : TileDataOverwriting
    {
        /// <summary>
        /// Constructs a RowOverwriting operation for overwriting an integer field.
        /// </summary>
        /// <param name="targetRow">The row of the isometric tile to perform the operation.</param>
        /// <param name="targetField">The name of the target field.</param>
        /// <param name="value">The new value of the target field.</param>
        /// <param name="tileset">The tileset of this operation.</param>
        public RowOverwriting(int targetRow, string targetField, int value, TileSet tileset)
            : base(targetField, value, tileset)
        {
            this.CheckAndAssignCtorParams(targetRow);
        }

        /// <summary>
        /// Constructs a RowOverwriting operation for overwriting a bool field.
        /// </summary>
        /// <param name="targetRow">The row of the isometric tile to perform the operation.</param>
        /// <param name="targetField">The name of the target field.</param>
        /// <param name="value">The new value of the target field.</param>
        /// <param name="tileset">The tileset of this operation.</param>
        public RowOverwriting(int targetRow, string targetField, bool value, TileSet tileset)
            : base(targetField, value, tileset)
        {
            this.CheckAndAssignCtorParams(targetRow);
        }

        /// <summary>
        /// Gets the target row of this operation.
        /// </summary>
        public int TargetRow { get { return this.targetRow; } }

        /// <see cref="ITileDataOverwriting.Apply"/>
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
        /// <param name="targetRow">The target row of this operation.</param>
        private void CheckAndAssignCtorParams(int targetRow)
        {
            /// TODO: check the parameter.
            this.targetRow = targetRow;
        }

        /// <summary>
        /// The target row of this operation.
        /// </summary>
        private int targetRow;
    }

    /// <summary>
    /// This operation overwrites a specific field in a given column of an isometric tile.
    /// </summary>
    public class ColOverwriting : TileDataOverwriting
    {
        /// <summary>
        /// Constructs a ColOverwriting operation for overwriting an integer field.
        /// </summary>
        /// <param name="targetCol">The column of the isometric tile to perform the operation.</param>
        /// <param name="targetField">The name of the target field.</param>
        /// <param name="value">The new value of the target field.</param>
        /// <param name="tileset">The tileset of this operation.</param>
        public ColOverwriting(int targetCol, string targetField, int value, TileSet tileset)
            : base(targetField, value, tileset)
        {
            this.CheckAndAssignCtorParams(targetCol);
        }

        /// <summary>
        /// Constructs a ColOverwriting operation for overwriting a bool field.
        /// </summary>
        /// <param name="targetCol">The column of the isometric tile to perform the operation.</param>
        /// <param name="targetField">The name of the target field.</param>
        /// <param name="value">The new value of the target field.</param>
        /// <param name="tileset">The tileset of this operation.</param>
        public ColOverwriting(int targetCol, string targetField, bool value, TileSet tileset)
            : base(targetField, value, tileset)
        {
            this.CheckAndAssignCtorParams(targetCol);
        }

        /// <summary>
        /// Gets the target column of this operation.
        /// </summary>
        public int TargetCol { get { return this.targetCol; } }

        /// <see cref="ITileDataOverwriting.Apply"/>
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
        /// <param name="targetCol">The target column of this operation.</param>
        private void CheckAndAssignCtorParams(int targetCol)
        {
            /// TODO: check the parameter.
            this.targetCol = targetCol;
        }

        /// <summary>
        /// The target column of this operation.
        /// </summary>
        private int targetCol;
    }

    /// <summary>
    /// This operation overwrites a specific field in a given cell of an isometric tile.
    /// </summary>
    public class CellOverwriting : TileDataOverwriting
    {
        /// <summary>
        /// Constructs a CellOverwriting operation for overwriting an integer field.
        /// </summary>
        /// <param name="targetCell">The cell of the isometric tile to perform the operation.</param>
        /// <param name="targetField">The name of the target field.</param>
        /// <param name="value">The new value of the target field.</param>
        /// <param name="tileset">The tileset of this operation.</param>
        public CellOverwriting(RCIntVector targetCell, string targetField, int value, TileSet tileset)
            : base(targetField, value, tileset)
        {
            this.CheckAndAssignCtorParams(targetCell);
        }

        /// <summary>
        /// Constructs a CellOverwriting operation for overwriting a bool field.
        /// </summary>
        /// <param name="targetCell">The cell of the isometric tile to perform the operation.</param>
        /// <param name="targetField">The name of the target field.</param>
        /// <param name="value">The new value of the target field.</param>
        /// <param name="tileset">The tileset of this operation.</param>
        public CellOverwriting(RCIntVector targetCell, string targetField, bool value, TileSet tileset)
            : base(targetField, value, tileset)
        {
            this.CheckAndAssignCtorParams(targetCell);
        }

        /// <summary>
        /// Gets the target cell of this operation.
        /// </summary>
        public RCIntVector TargetCell { get { return this.targetCell; } }

        /// <see cref="ITileDataOverwriting.Apply"/>
        public override void Apply(IIsoTile target)
        {
            this.ApplyOnCell(target, this.targetCell);
        }

        /// <summary>
        /// Checks and assigns the parameters coming from the constructor.
        /// </summary>
        /// <param name="targetCell">The target cell of this operation.</param>
        private void CheckAndAssignCtorParams(RCIntVector targetCell)
        {
            if (targetCell == RCIntVector.Undefined) { throw new ArgumentNullException("targetCell"); }

            /// TODO: check the parameter.
            this.targetCell = targetCell;
        }

        /// <summary>
        /// The target cell of this operation.
        /// </summary>
        private RCIntVector targetCell;
    }

    /// <summary>
    /// This operation overwrites a specific field in a given rectangle of an isometric tile.
    /// </summary>
    public class RectOverwriting : TileDataOverwriting
    {
        /// <summary>
        /// Constructs a RectOverwriting operation for overwriting an integer field.
        /// </summary>
        /// <param name="targetRect">The rectangle of the isometric tile to perform the operation.</param>
        /// <param name="targetField">The name of the target field.</param>
        /// <param name="value">The new value of the target field.</param>
        /// <param name="tileset">The tileset of this operation.</param>
        public RectOverwriting(RCIntRectangle targetRect, string targetField, int value, TileSet tileset)
            : base(targetField, value, tileset)
        {
            this.CheckAndAssignCtorParams(targetRect);
        }

        /// <summary>
        /// Constructs a RectOverwriting operation for overwriting a bool field.
        /// </summary>
        /// <param name="targetRect">The rectangle of the isometric tile to perform the operation.</param>
        /// <param name="targetField">The name of the target field.</param>
        /// <param name="value">The new value of the target field.</param>
        /// <param name="tileset">The tileset of this operation.</param>
        public RectOverwriting(RCIntRectangle targetRect, string targetField, bool value, TileSet tileset)
            : base(targetField, value, tileset)
        {
            this.CheckAndAssignCtorParams(targetRect);
        }

        /// <summary>
        /// Gets the target rectangle of this operation.
        /// </summary>
        public RCIntRectangle TargetRect { get { return this.targetRect; } }

        /// <see cref="ITileDataOverwriting.Apply"/>
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
        /// <param name="targetRect">The target rectangle of this operation.</param>
        private void CheckAndAssignCtorParams(RCIntRectangle targetRect)
        {
            if (targetRect == RCIntRectangle.Undefined) { throw new ArgumentNullException("targetRect"); }

            /// TODO: check the parameter.
            this.targetRect = targetRect;
        }

        /// <summary>
        /// The target rectangle of this operation.
        /// </summary>
        private RCIntRectangle targetRect;
    }
}
