using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine
{
    /// <summary>
    /// Defines the interface of a terrain object on the map.
    /// </summary>
    public interface ITerrainObject : IMapContent
    {
        /// <summary>
        /// Gets the parent map of this terrain object.
        /// </summary>
        IMap ParentMap { get; }

        /// <summary>
        /// Gets the type of this terrain object as it is defined in the tileset.
        /// </summary>
        TerrainObjectType Type { get; }

        /// <summary>
        /// Gets or sets the coordinates of the top-left quadratic tile of this terrain object.
        /// </summary>
        /// <remarks>
        /// Setting this property is allowed only if the terrain object has not yet been attached to the map.
        /// </remarks>
        RCIntVector MapCoords { get; set; }

        /// <summary>
        /// Gets all the violating quadratic coordinates of this terrain object.
        /// </summary>
        /// <remarks>
        /// Getting this property is allowed only if the terrain object has not yet been attached to the map.
        /// </remarks>
        IEnumerable<RCIntVector> ViolatingQuadCoords { get; }

        /// <summary>
        /// Attaches this terrain object to the map.
        /// </summary>
        /// <exception cref="MapException">
        /// If the terrain object cannot be attached to the map at the current position for any reason.
        /// </exception>
        /// <remarks>
        /// Calling this method is allowed only if the terrain object has not yet been attached to the map.
        /// </remarks>
        void Attach();

        /// <summary>
        /// Gets the quadratic tile of this terrain object at the given index.
        /// </summary>
        /// <param name="index">The index of the quadratic tile to get.</param>
        /// <returns>
        /// The quadratic tile at the given index or null if the given index is outside of the terrain object.
        /// </returns>
        /// <remarks>
        /// Calling this method is allowed only after the terrain object has been attached to the map.
        /// </remarks>
        IQuadTile GetQuadTile(RCIntVector index);
    }
}
