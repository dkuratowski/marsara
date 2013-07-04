using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Common.ComponentModel;

namespace RC.Engine.Simulator.ComponentInterfaces
{
    /// <summary>
    /// The public interface of the pathfinder component.
    /// </summary>
    [ComponentInterface]
    public interface IPathFinder
    {
        /// <summary>
        /// Initializes the pathfinder component with the given map.
        /// </summary>
        /// <param name="map">The map to initialize with.</param>
        /// <remarks>
        /// Don't call this method from the UI thread because initialization is a long running procedure. Use a background thread instead.
        /// </remarks>
        void Initialize(IMapAccess map);

        /// <summary>
        /// Computes a path from one cell to another on the map.
        /// </summary>
        /// <param name="fromCoords">The coordinates of the starting cell of the path.</param>
        /// <param name="toCoords">The coordinates of the target cell of the path.</param>
        /// <param name="size">The size of the object that requested the pathfinding.</param>
        /// <return>The interface of the computed path.</return>
        IPath FindPath(RCIntVector fromCoords, RCIntVector toCoords, RCNumVector size);

        /// <summary>
        /// Computes an alternative path to the target from the given cell on the given original path.
        /// </summary>
        /// <param name="originalPath">The original path.</param>
        /// <param name="fromCoords">The coordinates of the starting cell of the path.</param>
        /// <returns>The interface of the computed alternative path.</returns>
        IPath FindAlternativePath(IPath originalPath, RCIntVector fromCoords);
    }
}
