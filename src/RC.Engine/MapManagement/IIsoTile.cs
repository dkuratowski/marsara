using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine
{
    /// <summary>
    /// Defines the interface of an isometric tile.
    /// </summary>
    public interface IIsoTile
    {
        /// <summary>
        /// Gets the map coordinates of this isometric tile.
        /// </summary>
        RCIntVector MapCoords { get; }

        /// <summary>
        /// Gets the parent map of this isometric tile.
        /// </summary>
        IMap ParentMap { get; }

        /// <summary>
        /// Gets the type of this isometric tile.
        /// </summary>
        TileType Type { get; }

        /// <summary>
        /// Gets the currently selected variant of this isometric tile.
        /// </summary>
        TileVariant Variant { get; }

        /// <summary>
        /// Gets the neighbour of this isometric tile at the given direction.
        /// </summary>
        /// <param name="direction">The direction of the neighbour to get.</param>
        /// <returns>The neighbour of this isometric tile at the given direction or null if there is no neighbour in that direction.</returns>
        IIsoTile GetNeighbour(MapDirection direction);

        /// <summary>
        /// Gets the map coordinates of the navigation cell at the given index.
        /// </summary>
        /// <param name="index">The index of the navigation cell.</param>
        /// <returns>The coordinates of the navigation cell at the given index.</returns>
        RCIntVector GetNavCellCoords(RCIntVector index);
    }
}
