using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Maps.PublicInterfaces
{
    /// <summary>
    /// Defines the interface of a cell on the map.
    /// </summary>
    public interface ICell
    {
        /// <summary>
        /// Gets the data attached to this cell.
        /// </summary>
        ICellData Data { get; }

        /// <summary>
        /// Gets the quadratic tile that this cell belongs to.
        /// </summary>
        IQuadTile ParentQuadTile { get; }

        /// <summary>
        /// Gets the isometric tile that this cell belongs to.
        /// </summary>
        IIsoTile ParentIsoTile { get; }
    }
}
