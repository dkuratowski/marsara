using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// Helper class for creating the tessellation of a vertex set.
    /// </summary>
    class Tessellation
    {
        /// <summary>
        /// Constructs a Tessellation instance.
        /// </summary>
        public Tessellation()
        {
            this.noMoreVertex = false;

            NavMeshNode superTriangle = new NavMeshNode(SUPERTRIANGLE_VERTEX0, SUPERTRIANGLE_VERTEX1, SUPERTRIANGLE_VERTEX2);
            this.vertexMap = new Dictionary<RCNumVector, HashSet<NavMeshNode>>();
            this.vertexMap.Add(SUPERTRIANGLE_VERTEX0, new HashSet<NavMeshNode>() { superTriangle });
            this.vertexMap.Add(SUPERTRIANGLE_VERTEX1, new HashSet<NavMeshNode>() { superTriangle });
            this.vertexMap.Add(SUPERTRIANGLE_VERTEX2, new HashSet<NavMeshNode>() { superTriangle });
        }

        /// <summary>
        /// Gets the root node of the constructed tessellation.
        /// </summary>
        public NavMeshNode RootNode
        {
            get
            {
                /// TODO: remove the 3 vertices of the super-triangle if they have not yet been removed.
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Adds a new vertex to the tessellation.
        /// </summary>
        /// <param name="vertex">The vertex to be added.</param>
        /// <exception cref="InvalidOperationException">
        /// If Tessellation.AddBorder has already been called at least once.
        /// </exception>
        public void AddVertex(RCNumVector vertex)
        {
            if (this.noMoreVertex) { throw new InvalidOperationException("A border has already been added!"); }
            if (this.vertexMap.ContainsKey(vertex)) { throw new ArgumentException("vertex", "The given vertex has already been added to this tessellation!"); }
            /// TODO: implement this method!
        }

        /// <summary>
        /// Remove every vertex from this tessellation that is outside of the given border. Here "outside" means "on the
        /// left hand side" of the border if we follow the vertex order of the Polygon that represents the border.
        /// </summary>
        /// <param name="border">
        /// The Polygon that describes the border of the tessellation. Each vertex of this Polygon must have already been
        /// added to the vertex set of this tessellation.
        /// </param>
        /// <remarks>No more vertex can be added to this tessellation after calling this method.</remarks>
        /// <exception cref="ArgumentException">
        /// If the given border contains a vertex that is not in the vertex set of this tessellation.
        /// </exception>
        public void AddBorder(Polygon border)
        {
            this.noMoreVertex = true;

            /// TODO: implement this method!
        }

        /// <summary>
        /// The vertex-map of the tessellation that stores the incident NavMeshNodes for each vertex.
        /// </summary>
        private Dictionary<RCNumVector, HashSet<NavMeshNode>> vertexMap;

        /// <summary>
        /// This flag becomes true on the first call to Tessellation.AddBorder.
        /// </summary>
        private bool noMoreVertex;

        /// <summary>
        /// The vertices of the super-triangle that is the initial triangle of the tessellation.
        /// </summary>
        private static readonly RCNumVector SUPERTRIANGLE_VERTEX0 = new RCNumVector(-10, -10);
        private static readonly RCNumVector SUPERTRIANGLE_VERTEX1 = new RCNumVector(1054, -10);
        private static readonly RCNumVector SUPERTRIANGLE_VERTEX2 = new RCNumVector(-10, 1054);
    }
}
