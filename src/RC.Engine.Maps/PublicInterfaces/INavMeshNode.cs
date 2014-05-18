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
        /// Gets the ID of this node.
        /// </summary>
        int ID { get; }

        /// <summary>
        /// Gets the neighbours of this node.
        /// </summary>
        IEnumerable<INavMeshNode> Neighbours { get; }

        /// <summary>
        /// Gets the edge from this node to the given node.
        /// </summary>
        /// <param name="toNode">The target node of the edge.</param>
        /// <returns>The edge from this node to the given node.</returns>
        INavMeshEdge GetEdge(INavMeshNode toNode);
    }
}
