using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.PublicInterfaces
{
    /// <summary>
    /// Defines the interface of a quadratic tile.
    /// </summary>
    public interface IQuadTile : ICellDataChangeSetTarget
    {
        /// <summary>
        /// Gets the map coordinates of this quadratic tile.
        /// </summary>
        RCIntVector MapCoords { get; }

        /// <summary>
        /// Gets the reference to the parent isometric tile.
        /// </summary>
        IIsoTile IsoTile { get; }
    }
}
