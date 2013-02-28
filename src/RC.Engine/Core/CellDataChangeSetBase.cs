using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.PublicInterfaces;

namespace RC.Engine.Core
{
    /// <summary>
    /// This changeset overwrites a specific field in every cells of the target.
    /// </summary>
    class CellDataChangeSetBase : ICellDataChangeSet
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

            this.targetFieldIdx = tileset.GetCellDataFieldIndex(targetField);
            this.targetField = targetField;
            this.targetFieldType = tileset.GetCellDataFieldType(this.targetFieldIdx);
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

            this.targetFieldIdx = tileset.GetCellDataFieldIndex(targetField);
            this.targetField = targetField;
            this.targetFieldType = tileset.GetCellDataFieldType(this.targetFieldIdx);
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
            HashSet<RCIntVector> targetset = this.CollectTargetSet(target);
            foreach (RCIntVector targetCell in targetset)
            {
                this.UndoOnCell(target, targetCell);
            }
        }

        /// <see cref="ICellDataChangeSet.Tileset"/>
        public ITileSet Tileset { get { return this.tileset; } }

        #endregion ICellDataChangeSet methods

        /// <summary>
        /// Collects the coordinates of the cells of the target-set.
        /// </summary>
        /// <param name="target">The target of the changeset.</param>
        /// <returns>The collected coordinates.</returns>
        protected virtual HashSet<RCIntVector> CollectTargetSet(ICellDataChangeSetTarget target)
        {
            HashSet<RCIntVector> targetset = new HashSet<RCIntVector>();
            for (int x = 0; x < target.CellSize.X; x++)
            {
                for (int y = 0; y < target.CellSize.Y; y++)
                {
                    RCIntVector index = new RCIntVector(x, y);
                    if (target.GetCell(index) != null)
                    {
                        targetset.Add(index);
                    }
                }
            }
            return targetset;
        }

        /// <summary>
        /// Applies the changeset on the cell of the given target at the given index.
        /// </summary>
        /// <param name="target">The target of the operation.</param>
        /// <param name="index">The index of the cell to be applied.</param>
        private void ApplyOnCell(ICellDataChangeSetTarget target, RCIntVector index)
        {
            ICell cell = target.GetCell(index);
            if (cell == null) { throw new RCEngineException(string.Format("Cell at index {0} not found in the changeset target!", index)); }

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
        /// Undos the changeset on the cell of the given target at the given index.
        /// </summary>
        /// <param name="target">The target of the operation.</param>
        /// <param name="index">The index of the cell to be undone.</param>
        private void UndoOnCell(ICellDataChangeSetTarget target, RCIntVector index)
        {
            ICell cell = target.GetCell(index);
            if (cell == null) { throw new RCEngineException(string.Format("Cell at index {0} not found in the changeset target!", index)); }

            cell.Data.Undo();
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
}
