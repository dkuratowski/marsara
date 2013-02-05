using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine
{
    /// <summary>
    /// Defines the interface of a navigation cell.
    /// </summary>
    public interface INavCell
    {
        /// <summary>
        /// Gets the data attached to this navigation cell.
        /// </summary>
        CellData Data { get; }

        /// <summary>
        /// Gets the quadratic tile that this navigation cell belongs to.
        /// </summary>
        IQuadTile ParentQuadTile { get; }

        /// <summary>
        /// Gets the isometric tile that this navigation cell belongs to.
        /// </summary>
        IIsoTile ParentIsoTile { get; }
    }
}
