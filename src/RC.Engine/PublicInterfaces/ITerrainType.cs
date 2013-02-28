using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.PublicInterfaces
{
    /// <summary>
    /// Defines the interface of a terrain type.
    /// </summary>
    public interface ITerrainType
    {
        /// <summary>
        /// Checks whether this terrain type is a direct or indirect descendant of the given terrain type.
        /// </summary>
        /// <param name="other">The given terrain type.</param>
        /// <returns>
        /// True if this terrain type is a direct or indirect descendant of the given terrain type, false otherwise.
        /// </returns>
        bool IsDescendantOf(ITerrainType other);

        /// <summary>
        /// Finds a route from this terrain type to the given terrain type in the terrain tree.
        /// </summary>
        /// <param name="target">The target terrain type of the route.</param>
        /// <returns>
        /// A list that contains the route to the target. The first item in the list is this terrain type,
        /// the last item in the list is the target terrain type. If target equals with this, then the list
        /// will contain only one item: this terrain type.
        /// </returns>
        ITerrainType[] FindRoute(ITerrainType target);

        /// <summary>
        /// Gets the name of this terrain type.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the children of this terrain type.
        /// </summary>
        IEnumerable<ITerrainType> Children { get; }

        /// <summary>
        /// Gets the parent of this terrain type.
        /// </summary>
        ITerrainType Parent { get; }

        /// <summary>
        /// Gets the transition length from the parent to this terrain type. 
        /// </summary>
        int TransitionLength { get; }

        /// <summary>
        /// Gets the tileset of this terrain type.
        /// </summary>
        ITileSet Tileset { get; }

        /// <summary>
        /// Gets whether this terrain type has children or not.
        /// </summary>
        bool HasChildren { get; }
    }
}
