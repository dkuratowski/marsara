using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Maps.PublicInterfaces
{
    /// <summary>
    /// The interface of the nodes in a navigation mesh.
    /// </summary>
    public interface INavMeshNode : ISearchTreeContent
    {
        /// <summary>
        /// Gets the polygon that represents the area of this node on the 2D plane.
        /// </summary>
        RCPolygon Polygon { get; }

        /// <summary>
        /// Gets the neighbours of this node.
        /// </summary>
        IEnumerable<INavMeshNode> Neighbours { get; }
    }
}
