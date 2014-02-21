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
            this.nodeSearchTree = new BspSearchTree<NavMeshNode>(superTriangle.BoundingBox, BSP_NODE_CAPACITY, BXP_MIN_NODE_SIZE);
            this.nodeSearchTree.AttachContent(superTriangle);
        }

        /// <summary>
        /// Gets the root node of the constructed tessellation.
        /// </summary>
        public NavMeshNode RootNode
        {
            get
            {
                /// TODO: remove the 3 vertices of the super-triangle if they have not yet been removed.
                return this.vertexMap[SUPERTRIANGLE_VERTEX0].First();
            }
        }

        /// <summary>
        /// Adds a new vertex to the tessellation.
        /// </summary>
        /// <param name="vertex">The vertex to be added.</param>
        /// <exception cref="InvalidOperationException">If Tessellation.AddBorder has already been called at least once.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the vertex is not contained within at least 1 existing NavMeshNode.</exception>
        public void AddVertex(RCNumVector vertex)
        {
            if (this.noMoreVertex) { throw new InvalidOperationException("It is not possible to add more vertex if at least one border has already been added!"); }
            if (this.vertexMap.ContainsKey(vertex)) { throw new ArgumentException("vertex", "The given vertex has already been added to this tessellation!"); }

            /// Find the NavMeshNode that contains the new vertex.
            NavMeshNode containingNode = null;
            foreach (NavMeshNode node in this.nodeSearchTree.GetContents(vertex))
            {
                if (node.Polygon.Contains(vertex)) { containingNode = node; break; }
            }
            if (containingNode == null) { throw new ArgumentOutOfRangeException("vertex", "Vertex must be contained within at least 1 existing NavMeshNode!"); }

            /// Collect the NavMeshNodes whose circumcircle contains the new vertex.
            HashSet<NavMeshNode> nodesToMerge = new HashSet<NavMeshNode>() { containingNode };
            this.CollectNodesToMerge(vertex, containingNode, ref nodesToMerge);

            /// Merge the collected nodes.
            this.MergeNodes(ref nodesToMerge);

            /// Slice the merged node and update the vertex map and the search-tree.
            NavMeshNode mergedNode = nodesToMerge.First();
            this.nodeSearchTree.DetachContent(mergedNode);
            for (int i = 0; i < mergedNode.Polygon.VertexCount; i++)
            {
                if (!this.vertexMap[mergedNode.Polygon[i]].Remove(mergedNode)) { throw new InvalidOperationException("Merged node not found!"); }
            }
            this.vertexMap.Add(vertex, new HashSet<NavMeshNode>());
            foreach (NavMeshNode slice in mergedNode.Slice(vertex))
            {
                this.nodeSearchTree.AttachContent(slice);
                for (int i = 0; i < slice.Polygon.VertexCount; i++) { this.vertexMap[slice.Polygon[i]].Add(slice); }
            }
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
        /// Collects the neighbours of the given node that have to be merged when a new vertex is added to this tessellation.
        /// </summary>
        /// <param name="newVertex">The new vertex.</param>
        /// <param name="currNode">The node whose neighbours shall be checked.</param>
        /// <param name="collectedNodes">The nodes to be merged.</param>
        private void CollectNodesToMerge(RCNumVector newVertex, NavMeshNode currNode, ref HashSet<NavMeshNode> collectedNodes)
        {
            if (currNode.Polygon.VertexCount != 3) { throw new ArgumentException("Node shall be a triangle!", "currNode"); }

            HashSet<NavMeshNode> neighboursToContinue = new HashSet<NavMeshNode>();
            foreach (NavMeshNode neighbour in currNode.Neighbours)
            {
                if (!collectedNodes.Contains(neighbour))
                {
                    /// Calculate the determinant for deciding whether the new vertex is contained within the circumcircle of
                    /// the current neighbour.
                    RCNumVector v0 = new RCNumVector(neighbour.Polygon[0].X, neighbour.Polygon[0].Y);
                    RCNumVector v1 = new RCNumVector(neighbour.Polygon[1].X, neighbour.Polygon[1].Y);
                    RCNumVector v2 = new RCNumVector(neighbour.Polygon[2].X, neighbour.Polygon[2].Y);
                    RCNumber d00 = v0.X - newVertex.X;
                    RCNumber d01 = v0.Y - newVertex.Y;
                    RCNumber d02 = v0.X * v0.X - newVertex.X * newVertex.X + v0.Y * v0.Y - newVertex.Y * newVertex.Y;
                    RCNumber d10 = v1.X - newVertex.X;
                    RCNumber d11 = v1.Y - newVertex.Y;
                    RCNumber d12 = v1.X * v1.X - newVertex.X * newVertex.X + v1.Y * v1.Y - newVertex.Y * newVertex.Y;
                    RCNumber d20 = v2.X - newVertex.X;
                    RCNumber d21 = v2.Y - newVertex.Y;
                    RCNumber d22 = v2.X * v2.X - newVertex.X * newVertex.X + v2.Y * v2.Y - newVertex.Y * newVertex.Y;
                    RCMatrix mat0 = new RCMatrix(d11, d12, d21, d22);
                    RCMatrix mat1 = new RCMatrix(d01, d02, d21, d22);
                    RCMatrix mat2 = new RCMatrix(d01, d02, d11, d12);
                    RCNumber determinant = d00 * mat0.Determinant - d10 * mat1.Determinant + d20 * mat2.Determinant;

                    /// The node is contained within the circumcircle iff the determinant is non-negative.
                    if (determinant >= 0) { neighboursToContinue.Add(neighbour); }
                }
            }

            /// Append the collected neighbours to the list.
            collectedNodes.UnionWith(neighboursToContinue);

            /// Call this method recursively on the collected neighbours.
            foreach (NavMeshNode neighbour in neighboursToContinue) { this.CollectNodesToMerge(newVertex, neighbour, ref collectedNodes); }
        }

        /// <summary>
        /// Merges the given nodes to one common node.
        /// </summary>
        /// <param name="nodesToMerge">The nodes to be merged. This list will contain only the merged node after the merge operation.</param>
        private void MergeNodes(ref HashSet<NavMeshNode> nodesToMerge)
        {
            if (nodesToMerge.Count == 0) { throw new ArgumentException("Empty node list!", "nodesToMerge"); }

            /// If the node-list contains only 1 element then this is the end of the recursion.
            if (nodesToMerge.Count == 1) { return; }

            /// Find two adjacent nodes to merge in this step.
            NavMeshNode nodeToMerge = nodesToMerge.First();
            NavMeshNode neighbourToMergeWith = null;
            foreach (NavMeshNode neighbour in nodeToMerge.Neighbours)
            {
                if (nodesToMerge.Contains(neighbour)) { neighbourToMergeWith = neighbour; break; }
            }
            if (neighbourToMergeWith == null) { throw new InvalidOperationException("Unable to find a NavMeshNode to merge with!"); }

            /// Merge the nodes.
            nodeToMerge.MergeWith(neighbourToMergeWith);
            nodesToMerge.Remove(neighbourToMergeWith);

            /// Update the vertex map and the search-tree.
            for (int i = 0; i < neighbourToMergeWith.Polygon.VertexCount; i++) { this.vertexMap[neighbourToMergeWith.Polygon[i]].Remove(neighbourToMergeWith); }
            for (int i = 0; i < nodeToMerge.Polygon.VertexCount; i++) { this.vertexMap[nodeToMerge.Polygon[i]].Add(nodeToMerge); }
            this.nodeSearchTree.DetachContent(neighbourToMergeWith);

            /// Call this method recursively.
            this.MergeNodes(ref nodesToMerge);
        }

        /// <summary>
        /// The vertex-map of the tessellation that stores the incident NavMeshNodes for each vertex.
        /// </summary>
        private Dictionary<RCNumVector, HashSet<NavMeshNode>> vertexMap;

        /// <summary>
        /// Search-tree for searching NavMeshNodes.
        /// </summary>
        private ISearchTree<NavMeshNode> nodeSearchTree;

        /// <summary>
        /// This flag becomes true on the first call to Tessellation.AddBorder.
        /// </summary>
        private bool noMoreVertex;

        /// <summary>
        /// The vertices of the super-triangle that is the initial triangle of the tessellation.
        /// </summary>
        private static readonly RCNumVector SUPERTRIANGLE_VERTEX0 = new RCNumVector(-10, -10);
        private static readonly RCNumVector SUPERTRIANGLE_VERTEX1 = new RCNumVector(2078, -10);
        private static readonly RCNumVector SUPERTRIANGLE_VERTEX2 = new RCNumVector(-10, 2078);

        /// <summary>
        /// Constants of the NavMeshNode search-tree.
        /// </summary>
        private const int BSP_NODE_CAPACITY = 16;
        private const int BXP_MIN_NODE_SIZE = 10;
    }
}
