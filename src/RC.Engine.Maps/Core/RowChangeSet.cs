﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Maps.Core
{
    /// <summary>
    /// This changeset overwrites a specific field in a given row of the target.
    /// </summary>
    class RowChangeSet : CellDataChangeSetBase
    {
        /// <summary>
        /// Constructs a changeset for overwriting an integer field.
        /// </summary>
        /// <param name="targetRow">The row of the target to perform the changeset.</param>
        /// <param name="modifier">Reference to the modifier.</param>
        /// <param name="tileset">The tileset of this changeset.</param>
        public RowChangeSet(int targetRow, ICellDataModifier modifier, TileSet tileset)
            : base(modifier, tileset)
        {
            this.CheckAndAssignCtorParams(targetRow);
        }

        /// <see cref="CellDataChangeSetBase.CollectTargetSet"/>
        protected override HashSet<RCIntVector> CollectTargetSet(ICellDataChangeSetTarget target)
        {
            HashSet<RCIntVector> targetset = new HashSet<RCIntVector>();
            for (int x = 0; x < target.CellSize.X; x++)
            {
                RCIntVector index = new RCIntVector(x, this.targetRow);
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
}
