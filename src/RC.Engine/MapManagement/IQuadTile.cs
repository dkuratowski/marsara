using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine
{
    /// <summary>
    /// Defines the interface of a quadratic tile.
    /// </summary>
    public interface IQuadTile
    {
        /// <summary>
        /// Gets the reference to the parent isometric tile.
        /// </summary>
        IIsoTile IsoTile { get; }

        /// <summary>
        /// Gets the map coordinates of this quadratic tile.
        /// </summary>
        RCIntVector MapCoords { get; }

        /// <summary>
        /// Gets the number of navigation cell columns and rows in this quadratic tile.
        /// </summary>
        RCIntVector NavCellDims { get; }

        /// <summary>Gets the navigation cell of this quadratic tile at the given index.</summary>
        /// <param name="index">The index of the navigation cell to get.</param>
        /// <returns>The navigation cell of this quadratic tile at the given index.</returns>
        INavCell GetNavCell(RCIntVector index);
    }
}
