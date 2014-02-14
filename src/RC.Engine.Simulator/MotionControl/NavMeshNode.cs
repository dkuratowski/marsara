using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// Represents a node in a navigation mesh that is a convex polygon.
    /// </summary>
    class NavMeshNode
    {
        /// <summary>
        /// Constructs a triangular NavMeshNode.
        /// </summary>
        /// <param name="vertex0">The first vertex of the triangle.</param>
        /// <param name="vertex1">The second vertex of the triangle.</param>
        /// <param name="vertex2">The third vertex of the triangle.</param>
        internal NavMeshNode(RCNumVector vertex0, RCNumVector vertex1, RCNumVector vertex2)
        {
            RCNumber doubleArea = NavMesh.CalculateDoubleOfSignedArea(vertex0, vertex1, vertex2);
            if (doubleArea == 0) { throw new ArgumentException("The vertices of the triangle are colinear!"); }
            this.vertices = doubleArea > 0 ? new List<RCNumVector>() { vertex0, vertex1, vertex2 } : new List<RCNumVector>() { vertex2, vertex1, vertex0 };
            this.neighboursAtEdges = new List<NavMeshNode>() { null, null, null };
        }

        /// <summary>
        /// Gets the number of vertices of the polygon represented by this NavMeshNode.
        /// </summary>
        public int VertexCount { get { return this.vertices.Count; } }

        /// <summary>
        /// Gets the vertex of the polygon represented by this NavMeshNode with the given index.
        /// </summary>
        /// <param name="index">The index of the vertex to get.</param>
        /// <returns>The vertex of the polygon represented by this NavMeshNode with the given index.</returns>
        public RCNumVector this[int index] { get { return this.vertices[index]; } }

        /// <summary>
        /// Gets the neighbour at the other side of the given edge.
        /// </summary>
        /// <param name="edgeIndex">The index of the edge.</param>
        /// <returns>
        /// The neighbour at the other side of the given edge or null if there is no neighbour at the other side of the given edge.
        /// </returns>
        /// <remarks>The endpoints of edge N are vertices N and N+1.</remarks>
        public NavMeshNode GetNeighbourAtEdge(int edgeIndex) { return this.neighboursAtEdges[edgeIndex]; }

        /// <summary>
        /// The list of neighbours of this node in order of the edges.
        /// </summary>
        private List<NavMeshNode> neighboursAtEdges;

        /// <summary>
        /// The vertices of the polygon represented by this NavMeshNode in clockwise order.
        /// </summary>
        private List<RCNumVector> vertices;
    }
}
