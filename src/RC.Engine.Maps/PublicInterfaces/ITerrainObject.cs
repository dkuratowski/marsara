using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Maps.PublicInterfaces
{
    /// <summary>
    /// Defines the interface of a terrain object on the map.
    /// </summary>
    public interface ITerrainObject : ISearchTreeContent, ICellDataChangeSetTarget
    {
        /// <summary>
        /// Gets the quadratic tile of this terrain object at the given index.
        /// </summary>
        /// <param name="index">The index of the quadratic tile to get.</param>
        /// <returns>The quadratic at the given index or null if the given index is outside of this terrain object.</returns>
        IQuadTile GetQuadTile(RCIntVector index);

        /// <summary>
        /// Gets the type of this terrain object as it is defined in the tileset.
        /// </summary>
        ITerrainObjectType Type { get; }

        /// <summary>
        /// Gets the coordinates of the top-left quadratic tile of this terrain object.
        /// </summary>
        RCIntVector MapCoords { get; }

        /// <summary>
        /// Gets the map that this terrain object belongs to.
        /// </summary>
        IMapAccess ParentMap { get; }
    }
}
