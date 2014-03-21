using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Maps.Core
{
    /// <summary>
    /// Helper class for creating the tessellation of a polygon with holes.
    /// </summary>
    class TessellationHelper
    {
        /// <summary>
        /// Constructs a TessellationHelper instance for the given area.
        /// </summary>
        /// <param name="border">The outer border of the area.</param>
        /// <param name="holes">The holes inside the area.</param>
        /// <remarks>
        /// To prevent undefined behavior the following requirements shall be fulfilled:
        ///     - The vertex order of the border polygon shall be clockwise.
        ///     - The vertex order of the hole polygons shall be counter-clockwise.
        ///     - The border and the hole polygons shall not be self intersecting polygons.
        ///     - The hole polygons shall be entirely contained within the border polygon.
        ///     - The hole polygons shall not intersect each other and the border polygon.
        /// WARNING!!! These requirements are not checked automatically!
        /// </remarks>
        public TessellationHelper(RCPolygon border, List<RCPolygon> holes)
        {
            if (border == null) { throw new ArgumentNullException("border"); }
            if (holes == null) { throw new ArgumentNullException("holes"); }

            /// Retrieve the polygons along the border and the holes.
            this.border = border;
            this.holes = new List<RCPolygon>(holes);

            NavMeshNode superTriangle = new NavMeshNode(SUPERTRIANGLE_VERTEX0, SUPERTRIANGLE_VERTEX1, SUPERTRIANGLE_VERTEX2);
            this.vertexMap = new Dictionary<RCNumVector, HashSet<NavMeshNode>>();
            this.vertexMap.Add(SUPERTRIANGLE_VERTEX0, new HashSet<NavMeshNode>() { superTriangle });
            this.vertexMap.Add(SUPERTRIANGLE_VERTEX1, new HashSet<NavMeshNode>() { superTriangle });
            this.vertexMap.Add(SUPERTRIANGLE_VERTEX2, new HashSet<NavMeshNode>() { superTriangle });
            this.nodeSearchTree = new BspSearchTree<NavMeshNode>(superTriangle.BoundingBox, BSP_NODE_CAPACITY, BSP_MIN_NODE_SIZE);
            this.nodeSearchTree.AttachContent(superTriangle);

            /// Add the vertices into this tessellation.
            for (int i = 0; i < this.border.VertexCount; i++) { this.AddVertex(this.border[i]); }
            for (int holeIdx = 0; holeIdx < this.holes.Count; holeIdx++)
            {
                for (int i = 0; i < this.holes[holeIdx].VertexCount; i++) { this.AddVertex(this.holes[holeIdx][i]); }
            }

            /// Add the constraints to this tessellation.
            this.AddConstraint(this.border);
            foreach (RCPolygon hole in this.holes) { this.AddConstraint(hole); }

            /// Remove the nodes of the super-triangle.
            this.RemoveVertex(SUPERTRIANGLE_VERTEX0);
            this.RemoveVertex(SUPERTRIANGLE_VERTEX1);
            this.RemoveVertex(SUPERTRIANGLE_VERTEX2);

            /// Drop the non-walkable nodes that are outside of the border or inside a hole.
            this.DropNonWalkableNodes(this.border);
            foreach (RCPolygon hole in this.holes) { this.DropNonWalkableNodes(hole); }
        }

        /// <summary>
        /// Gets all nodes created by this TessellationHelper.
        /// </summary>
        public HashSet<NavMeshNode> Nodes { get { return this.nodeSearchTree.GetContents(); } }

        #region Tessellation methods

        /// <summary>
        /// Adds a new vertex to the tessellation.
        /// </summary>
        /// <param name="vertex">The vertex to be added.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the vertex is not contained within at least 1 existing NavMeshNode.</exception>
        private void AddVertex(RCNumVector vertex)
        {
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
            TessellationHelper.CollectViolatingNodes(vertex, containingNode, ref nodesToMerge);

            /// Merge the collected nodes.
            this.MergeNodes(ref nodesToMerge);

            /// Slice the merged node and update the vertex map and the search-tree.
            NavMeshNode mergedNode = nodesToMerge.First();
            this.nodeSearchTree.DetachContent(mergedNode);
            this.RemoveNodeFromVertexMap(mergedNode);
            foreach (NavMeshNode slice in mergedNode.Slice(vertex))
            {
                this.nodeSearchTree.AttachContent(slice);
                this.AddNodeToVertexMap(slice);
            }
        }

        /// <summary>Adds a constraint to this tessellation.</summary>
        /// <param name="constraint">
        /// The polygon that describes the constraint to be added. Each vertex of this polygon must have already been
        /// added to the vertex set of this tessellation.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If the given constraint contains a vertex that is not in the vertex set of this tessellation.
        /// </exception>
        private void AddConstraint(RCPolygon constraint)
        {
            /// Cut the triangles along the edges of the constraint polygon if necessary.
            for (int constraintEdgeIdx = 0; constraintEdgeIdx < constraint.VertexCount; constraintEdgeIdx++)
            {
                /// Check if the current constraint edge is already the part of this tessellation.
                RCNumVector edgeBegin = constraint[constraintEdgeIdx];
                RCNumVector edgeEnd = constraint[(constraintEdgeIdx + 1) % constraint.VertexCount];
                HashSet<NavMeshNode> matchingNodesCopy = new HashSet<NavMeshNode>(this.vertexMap[edgeBegin]);
                matchingNodesCopy.IntersectWith(this.vertexMap[edgeEnd]);
                if (matchingNodesCopy.Count == 2) { continue; }
                else if (matchingNodesCopy.Count != 0) { new InvalidOperationException("Constraint edge matches to only 1 node!"); }

                if (!this.vertexMap.ContainsKey(edgeBegin)) { throw new ArgumentException(string.Format("Vertex {0} in constraint polygon has not been added to the tessellation!", edgeBegin), string.Format("constraint[{0}]", constraintEdgeIdx)); }
                if (!this.vertexMap.ContainsKey(edgeEnd)) { throw new ArgumentException(string.Format("Vertex {0} in constraint polygon has not been added to the tessellation!", edgeEnd), string.Format("constraint[{0}]", (constraintEdgeIdx + 1) % constraint.VertexCount)); }
                
                /// Collect the NavMeshNodes that has intersection with the current edge.
                HashSet<NavMeshNode> nodesToMerge = new HashSet<NavMeshNode>();
                foreach (NavMeshNode checkedNode in this.vertexMap[edgeBegin])
                {
                    if (TessellationHelper.CheckSegmentPolygonIntersection(edgeBegin, edgeEnd, checkedNode.Polygon))
                    {
                        nodesToMerge.Add(checkedNode);
                        TessellationHelper.CollectIntersectingNodes(edgeBegin, edgeEnd, checkedNode, ref nodesToMerge);
                    }
                }

                /// Merge the collected nodes.
                if (nodesToMerge.Count > 1) { this.MergeNodes(ref nodesToMerge); }

                /// Cut the merged node along the current constraint edge if possible.
                NavMeshNode mergedNode = nodesToMerge.First();
                this.nodeSearchTree.DetachContent(mergedNode);
                this.RemoveNodeFromVertexMap(mergedNode);
                foreach (NavMeshNode slice in mergedNode.Slice(edgeBegin, edgeEnd))
                {
                    this.nodeSearchTree.AttachContent(slice);
                    this.AddNodeToVertexMap(slice);
                }
            }
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

            /// Remove the nodes from the vertex map and the search-tree.
            this.RemoveNodeFromVertexMap(nodeToMerge);
            this.RemoveNodeFromVertexMap(neighbourToMergeWith);
            this.nodeSearchTree.DetachContent(nodeToMerge);
            this.nodeSearchTree.DetachContent(neighbourToMergeWith);

            /// Merge the nodes.
            nodeToMerge.MergeWith(neighbourToMergeWith);
            nodesToMerge.Remove(neighbourToMergeWith);

            /// Update the vertex map and the search-tree.
            this.AddNodeToVertexMap(nodeToMerge);
            this.nodeSearchTree.AttachContent(nodeToMerge);

            /// Call this method recursively.
            this.MergeNodes(ref nodesToMerge);
        }

        /// <summary>
        /// Removes the given vertex and all matching nodes from this tessellation.
        /// </summary>
        /// <param name="vertex">The vertex to remove.</param>
        private void RemoveVertex(RCNumVector vertex)
        {
            if (this.vertexMap.ContainsKey(vertex))
            {
                foreach (NavMeshNode matchingNode in this.vertexMap[vertex])
                {
                    this.nodeSearchTree.DetachContent(matchingNode);
                    matchingNode.RemoveNeighbours();
                    for (int i = 0; i < matchingNode.Polygon.VertexCount; i++)
                    {
                        if (matchingNode.Polygon[i] != vertex)
                        {
                            this.vertexMap[matchingNode.Polygon[i]].Remove(matchingNode);
                            if (this.vertexMap[matchingNode.Polygon[i]].Count == 0) { this.vertexMap.Remove(matchingNode.Polygon[i]); }
                        }
                    }
                }
                this.vertexMap.Remove(vertex);
            }
        }

        /// <summary>
        /// Drops the non-walkable nodes that are outside/inside of the given border/hole.
        /// </summary>
        /// <param name="borderOrHole">The given border/hole.</param>
        private void DropNonWalkableNodes(RCPolygon borderOrHole)
        {
            HashSet<NavMeshNode> collectPathStartNodes = new HashSet<NavMeshNode>();
            for (int vertexIdx = 0; vertexIdx < borderOrHole.VertexCount; vertexIdx++)
            {
                HashSet<NavMeshNode> matchingNodesCopy = new HashSet<NavMeshNode>(this.vertexMap[borderOrHole[vertexIdx]]);
                matchingNodesCopy.IntersectWith(this.vertexMap[borderOrHole[(vertexIdx + 1) % borderOrHole.VertexCount]]);
                if (matchingNodesCopy.Count == 2)
                {
                    List<NavMeshNode> matchingNodesList = new List<NavMeshNode>(matchingNodesCopy);
                    matchingNodesList[0].RemoveNeighbour(matchingNodesList[1]);

                    int edgeBeginIdxIn0 = matchingNodesList[0].Polygon.IndexOf(borderOrHole[vertexIdx]);
                    int edgeEndIdxIn0 = matchingNodesList[0].Polygon.IndexOf(borderOrHole[(vertexIdx + 1) % borderOrHole.VertexCount]);
                    int edgeBeginIdxIn1 = matchingNodesList[1].Polygon.IndexOf(borderOrHole[vertexIdx]);
                    int edgeEndIdxIn1 = matchingNodesList[1].Polygon.IndexOf(borderOrHole[(vertexIdx + 1) % borderOrHole.VertexCount]);
                    if ((edgeBeginIdxIn0 + 1) % matchingNodesList[0].Polygon.VertexCount != edgeEndIdxIn0)
                    {
                        /// Drop node0.
                        collectPathStartNodes.Add(matchingNodesList[0]);
                    }
                    else if ((edgeBeginIdxIn1 + 1) % matchingNodesList[1].Polygon.VertexCount != edgeEndIdxIn1)
                    {
                        /// Drop node1.
                        collectPathStartNodes.Add(matchingNodesList[1]);
                    }
                }
            }

            HashSet<NavMeshNode> nodesToDrop = new HashSet<NavMeshNode>();
            while (collectPathStartNodes.Count != 0)
            {
                HashSet<NavMeshNode> collectedNodes = new HashSet<NavMeshNode>();
                collectPathStartNodes.First().CollectReachableNodes(ref collectedNodes);
                collectPathStartNodes.ExceptWith(collectedNodes);
                nodesToDrop.UnionWith(collectedNodes);
            }
            foreach (NavMeshNode nodeToDrop in nodesToDrop) { this.DropNode(nodeToDrop); }
        }

        /// <summary>
        /// Drops the given node.
        /// </summary>
        /// <param name="node">The node to be dropped</param>
        private void DropNode(NavMeshNode node)
        {
            this.nodeSearchTree.DetachContent(node);
            node.RemoveNeighbours();
            this.RemoveNodeFromVertexMap(node);
        }

        /// <summary>
        /// Removes the given node from the vertex map.
        /// </summary>
        /// <param name="node">The node to remove.</param>
        private void RemoveNodeFromVertexMap(NavMeshNode node)
        {
            for (int i = 0; i < node.Polygon.VertexCount; i++)
            {
                if (!this.vertexMap[node.Polygon[i]].Remove(node)) { throw new InvalidOperationException("Node not found in the vertex map!"); }
                if (this.vertexMap[node.Polygon[i]].Count == 0) { this.vertexMap.Remove(node.Polygon[i]); }
            }
        }

        /// <summary>
        /// Adds the given node to the vertex map.
        /// </summary>
        /// <param name="node">The node to add.</param>
        private void AddNodeToVertexMap(NavMeshNode node)
        {
            for (int i = 0; i < node.Polygon.VertexCount; i++)
            {
                if (!this.vertexMap.ContainsKey(node.Polygon[i])) { this.vertexMap.Add(node.Polygon[i], new HashSet<NavMeshNode>()); }
                this.vertexMap[node.Polygon[i]].Add(node);
            }
        }

        /// <summary>
        /// Collects the neighbours of the given node that have to be merged when a new vertex is added to this tessellation.
        /// </summary>
        /// <param name="newVertex">The new vertex.</param>
        /// <param name="currNode">The node whose neighbours shall be checked.</param>
        /// <param name="collectedNodes">The list of the violating nodes.</param>
        private static void CollectViolatingNodes(RCNumVector newVertex, NavMeshNode currNode, ref HashSet<NavMeshNode> collectedNodes)
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
            foreach (NavMeshNode neighbour in neighboursToContinue)
            {
                TessellationHelper.CollectViolatingNodes(newVertex, neighbour, ref collectedNodes);
            }
        }

        /// <summary>
        /// Collect the neighbours of the given node that have intersection with the given edge.
        /// </summary>
        /// <param name="edgeBegin">The vertex at the beginning of the edge.</param>
        /// <param name="edgeEnd">The vertex at the end of the edge.</param>
        /// <param name="currNode">The node whose neighbours shall be checked.</param>
        /// <param name="collectedNodes">The list of the nodes that have intersection with the given edge.</param>
        private static void CollectIntersectingNodes(RCNumVector edgeBegin, RCNumVector edgeEnd, NavMeshNode currNode, ref HashSet<NavMeshNode> collectedNodes)
        {
            HashSet<NavMeshNode> neighboursToContinue = new HashSet<NavMeshNode>();
            foreach (NavMeshNode neighbour in currNode.Neighbours)
            {
                if (!collectedNodes.Contains(neighbour))
                {
                    if (TessellationHelper.CheckSegmentPolygonIntersection(edgeBegin, edgeEnd, neighbour.Polygon))
                    {
                        neighboursToContinue.Add(neighbour);
                    }
                }
            }

            /// Append the collected neighbours to the list.
            collectedNodes.UnionWith(neighboursToContinue);

            /// Call this method recursively on the collected neighbours.
            foreach (NavMeshNode neighbour in neighboursToContinue)
            {
                TessellationHelper.CollectIntersectingNodes(edgeBegin, edgeEnd, neighbour, ref collectedNodes);
            }
        }

        /// <summary>
        /// Checks whether the given segment intersects the given polygon.
        /// </summary>
        /// <param name="segmentBegin">The beginning of the segment.</param>
        /// <param name="segmentEnd">The end of the segment.</param>
        /// <param name="checkedPolygon">The polygon to be checked.</param>
        /// <returns>True if the given segment intersects the given polygon; otherwise false.</returns>
        /// <remarks>
        /// TODO: This only works for polygons handled by the TessellationHelper! Make this algorithm more general if necessary!
        /// </remarks>
        private static bool CheckSegmentPolygonIntersection(RCNumVector segmentBegin, RCNumVector segmentEnd, RCPolygon checkedPolygon)
        {
            /// Check if the beginning or the end of the segment equals with a vertex of the polygon.
            int segmentBeginIdx = checkedPolygon.IndexOf(segmentBegin);
            int segmentEndIdx = checkedPolygon.IndexOf(segmentEnd);
            bool isDiagonal = false;
            if (segmentBeginIdx != -1 && segmentEndIdx != -1)
            {
                /// If the beginning and the end of the segment are adjacent then  segment equals with an edge -> trivial interection.
                if ((segmentBeginIdx + 1) % checkedPolygon.VertexCount == segmentEndIdx || (segmentEndIdx + 1) % checkedPolygon.VertexCount == segmentBeginIdx) { return true; }

                /// Otherwise its a diagonal and we might need further checks later.
                isDiagonal = true;
            }
            else if (segmentBeginIdx == -1 && segmentEndIdx != -1 && checkedPolygon.Contains(segmentBegin) ||
                     segmentBeginIdx != -1 && segmentEndIdx == -1 && checkedPolygon.Contains(segmentEnd))
            {
                /// The polygon contains the beginning or the end of the segment -> trivial intersection.
                return true;
            }

            /// Check intersections with the edges.
            for (int i = 0; i < checkedPolygon.VertexCount; i++)
            {
                RCNumVector edgeBegin = checkedPolygon[i];
                RCNumVector edgeEnd = checkedPolygon[(i + 1) % checkedPolygon.VertexCount];
                if (edgeBegin != segmentBegin && edgeEnd != segmentBegin && edgeBegin != segmentEnd && edgeEnd != segmentEnd)
                {
                    /// Check the signums of the signed areas of these polygons.
                    RCPolygon testPoly0 = TessellationHelper.CheckSegmentVertexIntersection(segmentBegin, segmentEnd, edgeBegin);
                    RCPolygon testPoly1 = TessellationHelper.CheckSegmentVertexIntersection(segmentBegin, segmentEnd, edgeEnd);
                    RCPolygon testPoly2 = TessellationHelper.CheckSegmentVertexIntersection(edgeBegin, edgeEnd, segmentBegin);
                    RCPolygon testPoly3 = TessellationHelper.CheckSegmentVertexIntersection(edgeBegin, edgeEnd, segmentEnd);
                    if ((testPoly0.DoubleOfSignedArea < 0 && testPoly1.DoubleOfSignedArea >= 0 || testPoly0.DoubleOfSignedArea >= 0 && testPoly1.DoubleOfSignedArea < 0) &&
                        (testPoly2.DoubleOfSignedArea < 0 && testPoly3.DoubleOfSignedArea >= 0 || testPoly2.DoubleOfSignedArea >= 0 && testPoly3.DoubleOfSignedArea < 0))
                    {
                        /// Signums are different -> intersection.
                        return true;
                    }
                }
            }

            if (isDiagonal)
            {
                /// If the segment is a diagonal then we have to check the vertex order of the two halves it creates
                /// from the polygon.
                List<RCNumVector> endToBeginHalfVertices = new List<RCNumVector>();
                for (int i = segmentEndIdx; i != segmentBeginIdx; i = (i + 1) % checkedPolygon.VertexCount) { endToBeginHalfVertices.Add(checkedPolygon[i]); }
                endToBeginHalfVertices.Add(checkedPolygon[segmentBeginIdx]);
                RCPolygon endToBeginHalf = new RCPolygon(endToBeginHalfVertices);
                List<RCNumVector> beginToEndHalfVertices = new List<RCNumVector>();
                for (int i = segmentBeginIdx; i != segmentEndIdx; i = (i + 1) % checkedPolygon.VertexCount) { beginToEndHalfVertices.Add(checkedPolygon[i]); }
                beginToEndHalfVertices.Add(checkedPolygon[segmentEndIdx]);
                RCPolygon beginToEndHalf = new RCPolygon(beginToEndHalfVertices);

                /// The diagonal is inside if and only if the two halves have the same vertex order.
                return endToBeginHalf.DoubleOfSignedArea >= 0 && beginToEndHalf.DoubleOfSignedArea >= 0 ||
                       endToBeginHalf.DoubleOfSignedArea < 0 && beginToEndHalf.DoubleOfSignedArea < 0;
            }

            /// No intersection.
            return false;
        }

        /// <summary>
        /// Checks whether the given vertex is on the given segment or not.
        /// </summary>
        /// <param name="segmentBegin">The begininning of the segment to test.</param>
        /// <param name="segmentEnd">The end of the segment to test.</param>
        /// <param name="vertex">The vertex to test.</param>
        /// <exception cref="InvalidOperationException">If the vertex is on the given segment.</exception>
        /// <returns>The polygon created from the segmentBegin, segmentEnd, vertex series.</returns>
        private static RCPolygon CheckSegmentVertexIntersection(RCNumVector segmentBegin, RCNumVector segmentEnd, RCNumVector vertex)
        {
            RCPolygon testPoly = new RCPolygon(segmentBegin, segmentEnd, vertex);
            if (testPoly.DoubleOfSignedArea == 0 && vertex != segmentBegin && vertex != segmentEnd)
            {
                if (vertex.X >= (segmentBegin.X <= segmentEnd.X ? segmentBegin.X : segmentEnd.X) &&
                    vertex.X <= (segmentBegin.X >= segmentEnd.X ? segmentBegin.X : segmentEnd.X) &&
                    vertex.Y >= (segmentBegin.Y <= segmentEnd.Y ? segmentBegin.Y : segmentEnd.Y) &&
                    vertex.Y <= (segmentBegin.Y >= segmentEnd.Y ? segmentBegin.Y : segmentEnd.Y))
                {
                    throw new InvalidOperationException("A vertex is colinear with the given segment!");
                }
            }
            return testPoly;
        }

        #endregion Tessellation methods

        /// <summary>
        /// The vertex-map of the tessellation that stores the incident NavMeshNodes for each vertex.
        /// </summary>
        private Dictionary<RCNumVector, HashSet<NavMeshNode>> vertexMap;

        /// <summary>
        /// Search-tree for searching NavMeshNodes.
        /// </summary>
        private ISearchTree<NavMeshNode> nodeSearchTree;

        /// <summary>
        /// The polygon that defines the outer border of the area that this TessellationHelper is working on.
        /// </summary>
        private RCPolygon border;

        /// <summary>
        /// List of the polygons defining the holes inside the area that this TessellationHelper is working on.
        /// </summary>
        private List<RCPolygon> holes;

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
        private const int BSP_MIN_NODE_SIZE = 10;
    }
}
