using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine
{
    /// <summary>
    /// Interface of navigation cell data changeset objects.
    /// If you apply a changeset on a target the following happens:
    ///     - A set of navigation cells (called as the "target-set") is selected out of the target depending
    ///       on the description of the changeset.
    ///     - The data field (given in the description of the changeset) of the navigation cells of the target-set
    ///       is overwritten with a new value (given in the description of the changeset).
    /// A changeset instance can be applied multiple times but the target-sets of the same instance have to be
    /// disjunct from each other, otherwise you get an exception.
    /// You can undo a changeset on any of the target-sets it has been applied to if there is no other changeset
    /// applied to any part of the appropriate target-set. Otherwise you get an exception.
    /// </summary>
    public interface ICellDataChangeSet
    {
        /// <summary>Applies the changeset on the navigation cells of the given target.</summary>
        /// <param name="target">The target of the changeset.</param>
        void Apply(ICellDataChangeSetTarget target);

        /// <summary>Undo the effect of the changeset on the navigation cells of the given target.</summary>
        /// <param name="target">The target of the changeset.</param>
        void Undo(ICellDataChangeSetTarget target);

        /// <summary>
        /// Gets the tileset of this changeset.
        /// </summary>
        TileSet Tileset { get; }
    }

    /// <summary>
    /// This interface shall be implemented by any objects that we want to be the target of a navigation
    /// cell data changeset.
    /// </summary>
    public interface ICellDataChangeSetTarget
    {
        /// <summary>
        /// Gets the navigation cell of this changeset target at the given index.
        /// </summary>
        /// <param name="index">The index of the navigation cell to get.</param>
        /// <returns>
        /// The navigation cell at the given index or null if the given index is outside of the changeset target.
        /// </returns>
        INavCell GetNavCell(RCIntVector index);

        /// <summary>
        /// Gets the size of this changeset target in navigation cells.
        /// </summary>
        RCIntVector NavSize { get; }
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

        #region ICellDataChangeSet methods

        /// <see cref="ICellDataChangeSet.Apply"/>
        public void Apply(ICellDataChangeSetTarget target)
        {
            HashSet<RCIntVector> targetset = this.CollectTargetSet(target);
            foreach (RCIntVector targetCell in targetset)
            {
                this.ApplyOnCell(target, targetCell);
            }
        }

        /// <see cref="ICellDataChangeSet.Undo"/>
        public void Undo(ICellDataChangeSetTarget target)
        {
            /// TODO: implement this method.
            throw new NotImplementedException();
        }

        /// <see cref="ICellDataChangeSet.Tileset"/>
        public TileSet Tileset { get { return this.tileset; } }

        #endregion ICellDataChangeSet methods

        /// <summary>
        /// Collects the coordinates of the navigation cells of the target-set.
        /// </summary>
        /// <param name="target">The target of the changeset.</param>
        /// <returns>The collected coordinates.</returns>
        protected virtual HashSet<RCIntVector> CollectTargetSet(ICellDataChangeSetTarget target)
        {
            HashSet<RCIntVector> targetset = new HashSet<RCIntVector>();
            for (int x = 0; x < target.NavSize.X; x++)
            {
                for (int y = 0; y < target.NavSize.Y; y++)
                {
                    RCIntVector index = new RCIntVector(x, y);
                    if (target.GetNavCell(index) != null)
                    {
                        targetset.Add(index);
                    }
                }
            }
            return targetset;
        }

        /// <summary>
        /// Applies the changeset on the navigation cell of the given target at the given index.
        /// </summary>
        /// <param name="target">The target of the operation.</param>
        /// <param name="index">The index of the navigation cell to be applied.</param>
        private void ApplyOnCell(ICellDataChangeSetTarget target, RCIntVector index)
        {
            INavCell cell = target.GetNavCell(index);
            if (cell == null) { throw new RCEngineException(string.Format("Navigation cell at index {0} not found in the changeset target!", index)); }

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

        /// <see cref="CellDataChangeSetBase.CollectTargetSet"/>
        protected override HashSet<RCIntVector> CollectTargetSet(ICellDataChangeSetTarget target)
        {
            HashSet<RCIntVector> targetset = new HashSet<RCIntVector>();
            for (int x = 0; x < target.NavSize.X; x++)
            {
                for (int y = 0; y < target.NavSize.Y; y++)
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

                    if (isCellInQuarter)
                    {
                        RCIntVector index = new RCIntVector(x, y);
                        if (target.GetNavCell(index) != null)
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

        /// <see cref="CellDataChangeSetBase.CollectTargetSet"/>
        protected override HashSet<RCIntVector> CollectTargetSet(ICellDataChangeSetTarget target)
        {
            HashSet<RCIntVector> targetset = new HashSet<RCIntVector>();
            for (int x = 0; x < target.NavSize.X; x++)
            {
                RCIntVector index = new RCIntVector(x, this.targetRow);
                if (target.GetNavCell(index) != null)
                {
                    targetset.Add(index);
                }
            }
            return targetset;
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

        /// <see cref="CellDataChangeSetBase.CollectTargetSet"/>
        protected override HashSet<RCIntVector> CollectTargetSet(ICellDataChangeSetTarget target)
        {
            HashSet<RCIntVector> targetset = new HashSet<RCIntVector>();
            for (int y = 0; y < target.NavSize.Y; y++)
            {
                RCIntVector index = new RCIntVector(this.targetCol, y);
                if (target.GetNavCell(index) != null)
                {
                    targetset.Add(index);
                }
            }
            return targetset;
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

        /// <see cref="CellDataChangeSetBase.CollectTargetSet"/>
        protected override HashSet<RCIntVector> CollectTargetSet(ICellDataChangeSetTarget target)
        {
            return target.GetNavCell(this.targetCell) != null ? new HashSet<RCIntVector>() { this.targetCell }
                                                              : new HashSet<RCIntVector>();
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

        /// <see cref="CellDataChangeSetBase.CollectTargetSet"/>
        protected override HashSet<RCIntVector> CollectTargetSet(ICellDataChangeSetTarget target)
        {
            HashSet<RCIntVector> targetset = new HashSet<RCIntVector>();
            for (int x = this.targetRect.X; x < this.targetRect.Right; x++)
            {
                for (int y = this.targetRect.Y; y < this.targetRect.Bottom; y++)
                {
                    RCIntVector index = new RCIntVector(x, y);
                    if (target.GetNavCell(index) != null)
                    {
                        targetset.Add(index);
                    }
                }
            }
            return targetset;
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
