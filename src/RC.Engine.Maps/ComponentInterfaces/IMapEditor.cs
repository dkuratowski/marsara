using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Maps.ComponentInterfaces
{
    /// <summary>
    /// Component interface for editing maps.
    /// </summary>
    [ComponentInterface]
    public interface IMapEditor
    {
        /// <summary>
        /// Draws the given terrain on the given isometric tile of the given map.
        /// </summary>
        /// <param name="targetMap">The target map.</param>
        /// <param name="targetTile">The target isometric tile.</param>
        /// <param name="terrainType">The terrain type to draw.</param>
        IEnumerable<IIsoTile> DrawTerrain(IMapAccess targetMap, IIsoTile targetTile, ITerrainType terrainType);

        /// <summary>
        /// Places a new terrain object on the given map at the given position.
        /// </summary>
        /// <param name="targetMap">The target map.</param>
        /// <param name="targetTile">The quadratic tile where the top-left corner of the new terrain object shall be placed.</param>
        /// <param name="type">The type of the new terrain object.</param>
        /// <returns>
        /// Reference to the interface of the new terrain object or null if the terrain object could not be placed.
        /// </returns>
        ITerrainObject PlaceTerrainObject(IMapAccess targetMap, IQuadTile targetTile, ITerrainObjectType type);

        /// <summary>
        /// Removes the given terrain object from the given map.
        /// </summary>
        /// <param name="targetMap">The target map.</param>
        /// <param name="terrainObject">The terrain object to be removed.</param>
        void RemoveTerrainObject(IMapAccess targetMap, ITerrainObject terrainObject);
    }
}
