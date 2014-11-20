using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Maps.PublicInterfaces
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
        /// Gets the reference to the isometric tile that contains the center of this quadratic tile.
        /// </summary>
        IIsoTile PrimaryIsoTile { get; }

        /// <summary>
        /// Gets the reference to the isometric tile that is different from the PrimaryIsoTile and contains the center of at least 1 cell of
        /// this quadratic tile. If no such isometric tile exists then this property is null.
        /// </summary>
        IIsoTile SecondaryIsoTile { get; }

        /// <summary>
        /// Gets the terrain object that is present on this quadratic tile or null if there is no terrain object on this quadratic tile.
        /// </summary>
        ITerrainObject TerrainObject { get; }

        /// <summary>
        /// Gets the buildability flag of this quadratic tile. A quadratic tile is buildable if all the contained cells are buildables.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the map is not finalized.</exception>
        bool IsBuildable { get; }

        /// <summary>
        /// Gets the ground level of this quadratic tile. The ground level of a quadratic tile is the average of the ground levels
        /// of the contained cells rounded to the neares integer value.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the map is not finalized.</exception>
        int GroundLevel { get; }
    }
}
