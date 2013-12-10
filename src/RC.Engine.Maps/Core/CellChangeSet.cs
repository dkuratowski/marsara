using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Maps.Core
{
    /// <summary>
    /// This changeset overwrites a specific field in a given cell of the target.
    /// </summary>
    class CellChangeSet : CellDataChangeSetBase
    {
        /// <summary>
        /// Constructs a changeset for overwriting an integer field.
        /// </summary>
        /// <param name="targetCell">The cell of the target to perform the changeset.</param>
        /// <param name="modifier">Reference to the modifier.</param>
        /// <param name="tileset">The tileset of this changeset.</param>
        public CellChangeSet(RCIntVector targetCell, ICellDataModifier modifier, TileSet tileset)
            : base(modifier, tileset)
        {
            this.CheckAndAssignCtorParams(targetCell);
        }

        /// <see cref="CellDataChangeSetBase.CollectTargetSet"/>
        protected override HashSet<RCIntVector> CollectTargetSet(ICellDataChangeSetTarget target)
        {
            return target.GetCell(this.targetCell) != null ? new HashSet<RCIntVector>() { this.targetCell }
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
}
