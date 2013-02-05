using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine
{
    /// <summary>
    /// Defines the interface of a map.
    /// </summary>
    public interface IMap
    {
        /// <summary>
        /// Gets the size of this map in quadratic tiles.
        /// </summary>
        RCIntVector Size { get; }

        /// <summary>
        /// Gets the size of this map in navigation cells.
        /// </summary>
        RCIntVector NavSize { get; }

        /// <summary>
        /// Gets the tileset of this map.
        /// </summary>
        TileSet Tileset { get; }

        /// <summary>Gets the quadratic tile at the given coordinates.</summary>
        /// <param name="coords">The coordinates of the quadratic tile to get.</param>
        /// <returns>The quadratic tile at the given coordinates.</returns>
        IQuadTile GetQuadTile(RCIntVector coords);

        /// <summary>
        /// Gets the isometric tile at the given coordinates or null if there is no isometric tile at that coordinates.
        /// </summary>
        /// <param name="coords">The coordinates of the isometric tile to get.</param>
        /// <returns>
        /// The isometric tile at the given coordinates or null if there is no isometric tile at that coordinates.
        /// </returns>
        IIsoTile GetIsoTile(RCIntVector coords);

        /// <summary>Gets the navigation cell at the given coordinates.</summary>
        /// <param name="coords">The coordinates of the navigation cell to get.</param>
        /// <returns>The navigation cell at the given coordinates.</returns>
        INavCell GetNavCell(RCIntVector coords);
    }
}
