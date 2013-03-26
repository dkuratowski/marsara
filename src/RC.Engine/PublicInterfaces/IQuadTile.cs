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
        /// Gets the neighbour of this quadratic tile at the given direction.
        /// </summary>
        /// <param name="direction">The direction of the neighbour to get.</param>
        /// <returns>The neighbour of this quadratic tile at the given direction or null if there is no neighbour in that direction.</returns>
        IQuadTile GetNeighbour(MapDirection direction);

        /// <summary>
        /// Gets the list of the neighbours of this quadratic tile.
        /// </summary>
        IEnumerable<IQuadTile> Neighbours { get; }

        /// <summary>
        /// Gets the reference to the parent isometric tile.
        /// </summary>
        IIsoTile IsoTile { get; }
    }
}
