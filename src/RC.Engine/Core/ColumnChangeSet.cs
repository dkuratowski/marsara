using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.PublicInterfaces;

namespace RC.Engine.Core
{
    /// <summary>
    /// This changeset overwrites a specific field in a given column of the target.
    /// </summary>
    class ColumnChangeSet : CellDataChangeSetBase
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
            for (int y = 0; y < target.CellSize.Y; y++)
            {
                RCIntVector index = new RCIntVector(this.targetCol, y);
                if (target.GetCell(index) != null)
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
}
