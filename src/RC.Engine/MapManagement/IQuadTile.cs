using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
