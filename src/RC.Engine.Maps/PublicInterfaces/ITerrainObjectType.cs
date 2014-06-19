using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Maps.PublicInterfaces
{
    /// <summary>
    /// Defines the interface of a terrain object type.
    /// </summary>
    public interface ITerrainObjectType
    {
        /// <summary>
        /// Gets the name of this terrain object type.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the size of the TerrainObjectType in quadratic tiles.
        /// </summary>
        RCIntVector QuadraticSize { get; }

        /// <summary>
        /// Gets the cell data changesets of this terrain object type.
        /// </summary>
        IEnumerable<ICellDataChangeSet> CellDataChangesets { get; }

        /// <summary>
        /// Gets the tileset of this terrain object type.
        /// </summary>
        ITileSet Tileset { get; }

        /// <summary>
        /// Gets the image data of this terrain object type.
        /// </summary>
        byte[] ImageData { get; }

        /// <summary>
        /// Gets the transparent color of this terrain object type.
        /// </summary>
        RCColor TransparentColor { get; }

        /// <summary>
        /// Gets the index of this terrain object type in the tileset.
        /// </summary>
        int Index { get; }

        /// <summary>
        /// Collects all the quadratic coordinates that violate the placement constraints of this terrain object type
        /// if it were placed to the given position on the given map.
        /// </summary>
        /// <param name="map">The map to be checked.</param>
        /// <param name="position">The position to be checked.</param>
        /// <returns>
        /// The list of the quadratic coordinates (relative to the top-left corner) violating the placement constraints
        /// of this terrain object type at the given position on the given map.
        /// </returns>
        HashSet<RCIntVector> CheckConstraints(IMapAccess map, RCIntVector position);

        /// <summary>
        /// Collects all the quadratic coordinates of this terrain object type that intersects any of the terrain objects on the given map
        /// if it were placed to the given position.
        /// </summary>
        /// <param name="map">The map to be checked.</param>
        /// <param name="position">The position to be checked.</param>
        /// <returns>
        /// The list of the quadratic coordinates (relative to the top-left corner) of this terrain object type that intersects any of the
        /// terrain objects on the map at the given position.
        /// </returns>
        HashSet<RCIntVector> CheckTerrainObjectIntersections(IMapAccess map, RCIntVector position);

        /// <summary>
        /// Checks whether the given quadratic position is excluded from this terrain object type or not.
        /// </summary>
        /// <param name="position">The quadratic position to check.</param>
        /// <returns>True if the given quadratic position is excluded from this terrain object type, false otherwise.</returns>
        bool IsExcluded(RCIntVector position);
    }
}
