using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Maps.PublicInterfaces
{
    /// <summary>
    /// Defines the interface of an isometric tile.
    /// </summary>
    public interface IIsoTile : ICellDataChangeSetTarget
    {
        /// <summary>
        /// Gets the map coordinates of this isometric tile.
        /// </summary>
        RCIntVector MapCoords { get; }

        /// <summary>
        /// Gets the type of this isometric tile.
        /// </summary>
        IIsoTileType Type { get; }

        /// <summary>
        /// Gets the currently selected variant of this isometric tile.
        /// </summary>
        IIsoTileVariant Variant { get; }

        /// <summary>
        /// Gets the index of the currently selected variant of this isometric tile.
        /// </summary>
        int VariantIdx { get; }

        /// <summary>
        /// Gets the neighbour of this isometric tile at the given direction.
        /// </summary>
        /// <param name="direction">The direction of the neighbour to get.</param>
        /// <returns>The neighbour of this isometric tile at the given direction or null if there is no neighbour in that direction.</returns>
        IIsoTile GetNeighbour(MapDirection direction);

        /// <summary>
        /// Gets the list of the neighbours of this isometric tile.
        /// </summary>
        IEnumerable<IIsoTile> Neighbours { get; }

        /// <summary>
        /// Exchanges the type of this isometric tile.
        /// </summary>
        /// <param name="newType">The new type.</param>
        /// <remarks>
        /// This method can only be called during a tile exchanging operation.
        /// </remarks>
        void ExchangeType(IIsoTileType newType);

        /// <summary>
        /// Gets the map coordinates of the cell at the given index.
        /// </summary>
        /// <param name="index">The index of the cell relative to this isometric tile.</param>
        /// <returns>The map coordinates of the cell.</returns>
        RCIntVector GetCellMapCoords(RCIntVector index);
    }
}
