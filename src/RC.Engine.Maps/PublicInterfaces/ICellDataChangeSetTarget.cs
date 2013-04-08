using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Maps.PublicInterfaces
{
    /// <summary>
    /// This interface shall be implemented by any objects that we want to be the target of a cell data changeset.
    /// </summary>
    public interface ICellDataChangeSetTarget
    {
        /// <summary>Gets the cell of this changeset target at the given index.</summary>
        /// <param name="index">The index of the cell to get.</param>
        /// <returns>The cell at the given index or null if the given index is outside of the changeset target.</returns>
        ICell GetCell(RCIntVector index);

        /// <summary>
        /// Gets the size of this changeset target in cells.
        /// </summary>
        RCIntVector CellSize { get; }
    }
}
