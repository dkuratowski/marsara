using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.PublicInterfaces
{
    /// <summary>
    /// Interface of cell data changeset objects.
    /// If you apply a changeset on a target the following happens:
    ///     - A set of cells (called as the "target-set") is selected out of the target depending
    ///       on the description of the changeset.
    ///     - The appropriate data field (given in the description of the changeset) of the cells of the target-set
    ///       is overwritten with a new value (again given in the description of the changeset).
    /// If you undo a changeset on a target the following happens:
    ///     - A set of cells (called as the "target-set") is selected out of the target depending
    ///       on the description of the changeset.
    ///     - The cells of the target-set are set back to their previous state.
    /// A changeset instance can be applied as many times and on as many target-sets as necessary, but undoing them
    /// in the right order is the callers responsibility.
    /// </summary>
    public interface ICellDataChangeSet
    {
        /// <summary>Applies the changeset on the cells of the given target.</summary>
        /// <param name="target">The target of the changeset.</param>
        void Apply(ICellDataChangeSetTarget target);

        /// <summary>Undo the effect of the changeset on the cells of the given target.</summary>
        /// <param name="target">The target of the changeset.</param>
        void Undo(ICellDataChangeSetTarget target);

        /// <summary>
        /// Gets the tileset of this changeset.
        /// </summary>
        ITileSet Tileset { get; }
    }
}
