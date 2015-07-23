using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Maps.Core
{
    /// <summary>
    /// Interface of cell data modifiers.
    /// </summary>
    interface ICellDataModifier
    {
        /// <summary>
        /// Modifies the corresponding data of the given cell.
        /// </summary>
        /// <param name="cell">The cell to modify.</param>
        void ModifyData(ICell cell);

        /// <summary>
        /// Undos the last modification on the corresponding data of the given cell.
        /// </summary>
        /// <param name="cell">The cell to undo.</param>
        void UndoModification(ICell cell);
    }

    /// <summary>
    /// Walkability flag modifier.
    /// </summary>
    class WalkabilityFlagModifier : ICellDataModifier
    {
        /// <summary>
        /// Constructs a WalkabilityFlagModifier instance.
        /// </summary>
        /// <param name="newValue">The new value of the walkability flag.</param>
        public WalkabilityFlagModifier(bool newValue)
        {
            this.newValue = newValue;
        }

        #region ICellDataModifier methods

        /// <see cref="ICellDataModifier.ModifyData"/>
        public void ModifyData(ICell cell) { cell.ChangeWalkability(this.newValue); }

        /// <see cref="ICellDataModifier.UndoModification"/>
        public void UndoModification(ICell cell) { cell.UndoWalkabilityChange(); }

        #endregion ICellDataModifier methods

        /// <summary>
        /// The new value of the walkability flag.
        /// </summary>
        private bool newValue;
    }

    /// <summary>
    /// Buildability flag modifier.
    /// </summary>
    class BuildabilityFlagModifier : ICellDataModifier
    {
        /// <summary>
        /// Constructs a BuildabilityFlagModifier instance.
        /// </summary>
        /// <param name="newValue">The new value of the buildability flag.</param>
        public BuildabilityFlagModifier(bool newValue)
        {
            this.newValue = newValue;
        }

        #region ICellDataModifier methods

        /// <see cref="ICellDataModifier.ModifyData"/>
        public void ModifyData(ICell cell) { cell.ChangeBuildability(this.newValue); }

        /// <see cref="ICellDataModifier.UndoModification"/>
        public void UndoModification(ICell cell) { cell.UndoBuildabilityChange(); }

        #endregion ICellDataModifier methods

        /// <summary>
        /// The new value of the buildability flag.
        /// </summary>
        private bool newValue;
    }

    /// <summary>
    /// Ground level value modifier.
    /// </summary>
    class GroundLevelModifier : ICellDataModifier
    {
        /// <summary>
        /// Constructs a GroundLevelModifier instance.
        /// </summary>
        /// <param name="newValue">The new value of the ground level.</param>
        public GroundLevelModifier(int newValue)
        {
            this.newValue = newValue;
        }

        #region ICellDataModifier methods

        /// <see cref="ICellDataModifier.ModifyData"/>
        public void ModifyData(ICell cell) { cell.ChangeGroundLevel(this.newValue); }

        /// <see cref="ICellDataModifier.UndoModification"/>
        public void UndoModification(ICell cell) { cell.UndoGroundLevelChange(); }

        #endregion ICellDataModifier methods

        /// <summary>
        /// The new value of the ground level.
        /// </summary>
        private int newValue;
    }

    /// <summary>
    /// This changeset overwrites a specific field in every cells of the target.
    /// </summary>
    class CellDataChangeSetBase : ICellDataChangeSet
    {
        /// <summary>
        /// Constructs a changeset for overwriting an integer field.
        /// </summary>
        /// <param name="modifier">Reference to the modifier object.</param>
        /// <param name="tileset">The tileset of this changeset.</param>
        public CellDataChangeSetBase(ICellDataModifier modifier, TileSet tileset)
        {
            if (tileset == null) { throw new ArgumentNullException("tileset"); }
            if (modifier == null) { throw new ArgumentNullException("modifier"); }

            this.tileset = tileset;
            this.modifier = modifier;
        }

        #region ICellDataChangeSet methods

        /// <see cref="ICellDataChangeSet.Apply"/>
        public void Apply(ICellDataChangeSetTarget target)
        {
            RCSet<RCIntVector> targetset = this.CollectTargetSet(target);
            foreach (RCIntVector targetCell in targetset)
            {
                ICell cell = target.GetCell(targetCell);
                if (cell == null) { throw new MapException(string.Format("Cell at index {0} not found in the changeset target!", targetCell)); }
                this.modifier.ModifyData(cell);
            }
        }

        /// <see cref="ICellDataChangeSet.Undo"/>
        public void Undo(ICellDataChangeSetTarget target)
        {
            RCSet<RCIntVector> targetset = this.CollectTargetSet(target);
            foreach (RCIntVector targetCell in targetset)
            {
                ICell cell = target.GetCell(targetCell);
                if (cell == null) { throw new MapException(string.Format("Cell at index {0} not found in the changeset target!", targetCell)); }
                this.modifier.UndoModification(cell);
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
        protected virtual RCSet<RCIntVector> CollectTargetSet(ICellDataChangeSetTarget target)
        {
            RCSet<RCIntVector> targetset = new RCSet<RCIntVector>();
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
        /// Reference to the modifier object.
        /// </summary>
        private ICellDataModifier modifier;

        /// <summary>
        /// The tileset of this changeset.
        /// </summary>
        private TileSet tileset;
    }
}
