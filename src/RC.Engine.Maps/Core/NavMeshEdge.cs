using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Maps.Core
{
    /// <summary>
    /// Contains informations about and edge from a NavMeshNode to another NavMeshNode. These informations can be used for calculating
    /// the preferred velocities during the motion control of entities.
    /// </summary>
    class NavMeshEdge : INavMeshEdge
    {
        /// <summary>
        /// Constructs a navmesh edge between the 2 given navmesh nodes.
        /// </summary>
        /// <param name="fromNode">The node at the beginning of the edge.</param>
        /// <param name="toNode">The node at the end of the edge.</param>
        public NavMeshEdge(NavMeshNode fromNode, NavMeshNode toNode)
        {
            RCNumVector edgeBegin = RCNumVector.Undefined;
            RCNumVector edgeEnd = RCNumVector.Undefined;
            for (int vertexIdxInA = 0; vertexIdxInA < fromNode.Polygon.VertexCount; vertexIdxInA++)
            {
                RCNumVector currEdgeBegin = fromNode.Polygon[vertexIdxInA];
                RCNumVector currEdgeEnd = fromNode.Polygon[(vertexIdxInA + 1) % fromNode.Polygon.VertexCount];
                int edgeBeginIdxInB = toNode.Polygon.IndexOf(currEdgeBegin);
                int edgeEndIdxInB = toNode.Polygon.IndexOf(currEdgeEnd);
                if (edgeBeginIdxInB != -1 && edgeEndIdxInB != -1 && (edgeEndIdxInB + 1) % toNode.Polygon.VertexCount == edgeBeginIdxInB)
                {
                    edgeBegin = currEdgeBegin;
                    edgeEnd = currEdgeEnd;
                    break;
                }
            }
            if (edgeBegin == RCNumVector.Undefined || edgeEnd == RCNumVector.Undefined) { throw new ArgumentException("Common edge not found between the polygons of the given nodes!"); }

            RCNumVector edgeVector = (edgeEnd - edgeBegin);
            this.transitionVector = new RCNumVector(edgeVector.Y, -edgeVector.X);
            this.midpoint = (edgeBegin + edgeEnd) / 2;
        }

        #region INavMeshEdge methods

        /// <see cref="INavMeshEdge.TransitionVector"/>
        public RCNumVector TransitionVector { get { return this.transitionVector; } }

        /// <see cref="INavMeshEdge.Midpoint"/>
        public RCNumVector Midpoint { get { return this.midpoint; } }

        #endregion INavMeshEdge methods

        /// <summary>
        /// The vector that is perpendicular to this edge and points out of the edge's navmesh node. The length of this vector equals
        /// with the length of the edge.
        /// </summary>
        private RCNumVector transitionVector;

        /// <summary>
        /// The coordinates of the midpoint of this edge.
        /// </summary>
        private RCNumVector midpoint;
    }
}
