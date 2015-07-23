using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Maps.Core
{
    /// <summary>
    /// Represents a node in a navmesh graph.
    /// </summary>
    class NavMeshNode : INavMeshNode
    {
        /// <summary>
        /// Constructs a triangular NavMeshNode.
        /// </summary>
        /// <param name="vertex0">The first vertex of the triangle.</param>
        /// <param name="vertex1">The second vertex of the triangle.</param>
        /// <param name="vertex2">The third vertex of the triangle.</param>
        /// <param name="idSource">The enumerator that provides the IDs for the NavMeshNodes.</param>
        internal NavMeshNode(RCNumVector vertex0, RCNumVector vertex1, RCNumVector vertex2, IEnumerator<int> idSource)
            : this(idSource)
        {
            this.polygon = new RCPolygon(vertex0, vertex1, vertex2);
            if (this.polygon.DoubleOfSignedArea == 0) { throw new ArgumentException("The vertices of the triangle are colinear!"); }

            /// If the vertices are in counter-clockwise order put them into clockwise order.
            if (this.polygon.DoubleOfSignedArea < 0) { this.polygon = new RCPolygon(vertex2, vertex1, vertex0); }
        }

        /// <summary>
        /// Constructs a NavMeshNode with custom area.
        /// </summary>
        /// <param name="nodePolygon">The polygon that represents the area of the node.</param>
        /// <param name="idSource">The enumerator that provides the IDs for the NavMeshNodes.</param>
        internal NavMeshNode(RCPolygon nodePolygon, IEnumerator<int> idSource)
            : this(idSource)
        {
            if (nodePolygon == null) { throw new ArgumentNullException("nodePolygon"); }
            if (nodePolygon.DoubleOfSignedArea <= 0) { throw new ArgumentException("The polygon must be in clockwise order!", "nodePolygon"); }

            this.polygon = nodePolygon;
        }

        #region ISearchTreeNode members

        /// <see cref="ISearchTreeContent.BoundingBox"/>
        public RCNumRectangle BoundingBox { get { return this.polygon.BoundingBox; } }

        /// <see cref="ISearchTreeContent.BoundingBoxChanging"/>
        public event ContentBoundingBoxChangeHdl BoundingBoxChanging;

        /// <see cref="ISearchTreeContent.BoundingBoxChanged"/>
        public event ContentBoundingBoxChangeHdl BoundingBoxChanged;

        #endregion ISearchTreeNode members

        #region INavMeshNode members

        /// <see cref="INavMeshNode.Polygon"/>
        public RCPolygon Polygon { get { return this.polygon; } }

        /// <see cref="INavMeshNode.ID"/>
        public int ID { get { return this.id; } }

        /// <see cref="INavMeshNode.Neighbours"/>
        public IEnumerable<INavMeshNode> Neighbours { get { return this.neighbours.Keys; } }

        /// <see cref="INavMeshNode.Neighbours"/>
        public INavMeshEdge GetEdge(INavMeshNode toNode)
        {
            if (toNode == null) { throw new ArgumentNullException("toNode"); }
            NavMeshNode toNodeCasted = toNode as NavMeshNode;
            if (toNodeCasted == null || !this.neighbours.ContainsKey(toNodeCasted)) { throw new ArgumentException("The given node is not the neighbour of this node!"); }
            return this.neighbours[toNodeCasted];
        }

        #endregion INavMeshNode members

        #region Internal methods

        /// <summary>
        /// Collects the nodes that are reachable from this node.
        /// </summary>
        /// <param name="collectedNodes">The list of the collected nodes.</param>
        internal void CollectReachableNodes(ref RCSet<NavMeshNode> collectedNodes)
        {
            if (!collectedNodes.Add(this)) { return; }
            foreach (NavMeshNode neighbour in this.neighbours.Keys) { neighbour.CollectReachableNodes(ref collectedNodes); }
        }

        /// <summary>
        /// Merges this NavMeshNode with one of its given parent. After the merge operation this NavMeshNode will represent the merged node and the
        /// NavMeshNode given in the parameter will be detached from its parents and will become obsolate.
        /// </summary>
        /// <param name="neighbour">The neighbour to merge with.</param>
        internal void MergeWith(NavMeshNode neighbour)
        {
            if (neighbour == null) { throw new ArgumentNullException("neighbour"); }
            if (!this.neighbours.ContainsKey(neighbour)) { throw new ArgumentException("The given node is not the neighbour of this node!", "neighbour"); }

            /// Remove the reference between the given neighbour and this node in both directions.
            this.neighbours.Remove(neighbour);
            neighbour.neighbours.Remove(this);

            /// Process all the remaining neighbours of the given neighbour to point only to this node.
            foreach (NavMeshNode neighbourOfNeighbour in neighbour.neighbours.Keys)
            {
                if (neighbourOfNeighbour.neighbours.Remove(neighbour))
                {
                    neighbourOfNeighbour.neighbours[this] = null;
                    this.neighbours[neighbourOfNeighbour] = null;
                }
            }
            neighbour.neighbours.Clear();

            /// Merge the polygons of the two nodes.
            this.MergePolygons(neighbour.polygon);
        }

        /// <summary>
        /// Removes all the neighbourhood relationships between this node and its neighbours.
        /// </summary>
        internal void RemoveNeighbours()
        {
            foreach (NavMeshNode neighbour in this.neighbours.Keys) { neighbour.neighbours.Remove(this); }
            this.neighbours.Clear();
        }

        /// <summary>
        /// Removes the neighbourhood relationship between this node and its given neighbour.
        /// </summary>
        /// <param name="neighbour">The given neighbour.</param>
        internal void RemoveNeighbour(NavMeshNode neighbour)
        {
            if (neighbour == null) { throw new ArgumentNullException("neighbour"); }

            neighbour.neighbours.Remove(this);
            this.neighbours.Remove(neighbour);
        }

        /// <summary>
        /// Sets the neighbourhood relationship between this node and the given node.
        /// </summary>
        /// <param name="otherNode">The given node.</param>
        internal void AddNeighbour(NavMeshNode otherNode)
        {
            if (otherNode == null) { throw new ArgumentNullException("otherNode"); }

            otherNode.neighbours[this] = null;
            this.neighbours[otherNode] = null;
        }

        /// <summary>
        /// Slices this node along the given cut diagonal.
        /// </summary>
        /// <param name="cutBegin">The beginning of the cut diagonal.</param>
        /// <param name="cutEnd">The end of the cut diagonal.</param>
        /// <returns>The list of the slices.</returns>
        internal IEnumerable<NavMeshNode> Slice(RCNumVector cutBegin, RCNumVector cutEnd)
        {
            if (cutBegin == RCNumVector.Undefined) { throw new ArgumentNullException("cutBegin"); }
            if (cutEnd == RCNumVector.Undefined) { throw new ArgumentNullException("cutEnd"); }

            int cutBeginIdx = this.polygon.IndexOf(cutBegin);
            int cutEndIdx = this.polygon.IndexOf(cutEnd);
            if (cutBeginIdx == -1) { throw new ArgumentException("The beginning of the cut shall be a vertex of this node!", "cut"); }
            if (cutEndIdx == -1) { throw new ArgumentException("The end of the cut shall be a vertex of this node!", "cut"); }
            if ((cutBeginIdx + 1) % this.polygon.VertexCount == cutEndIdx ||
                (cutEndIdx + 1) % this.polygon.VertexCount == cutBeginIdx) { throw new ArgumentException("The given cut equals with an edge!"); }

            /// Collect the original neighbours of this node in the order of the vertices of this node.
            NavMeshNode[] originalNeighbours = this.GetNeighboursByEdges();

            /// Clear the neighbour relationship between this node and its neighbours.
            this.neighbours.Clear();
            for (int i = 0; i < this.polygon.VertexCount; i++)
            {
                if (originalNeighbours[i] != null) { originalNeighbours[i].neighbours.Remove(this); }
            }

            /// Create the new polygon of this node and set back the neighbour relationship between this node and its remaining
            /// neighbours after cut.
            List<RCNumVector> thisPolygonCW = new List<RCNumVector>();
            for (int i = cutEndIdx; i != cutBeginIdx; i = (i + 1) % this.polygon.VertexCount)
            {
                thisPolygonCW.Add(this.polygon[i]);
                if (originalNeighbours[i] != null)
                {
                    originalNeighbours[i].neighbours[this] = null;
                    this.neighbours[originalNeighbours[i]] = null;
                }
            }
            thisPolygonCW.Add(this.polygon[cutBeginIdx]);

            /// Create the new node and set its neighbour relationships.
            List<RCNumVector> newNodePolygonCW = new List<RCNumVector>();
            NavMeshNode newNode = new NavMeshNode(this.idSource);
            for (int i = cutBeginIdx; i != cutEndIdx; i = (i + 1) % this.polygon.VertexCount)
            {
                newNodePolygonCW.Add(this.polygon[i]);
                if (originalNeighbours[i] != null)
                {
                    originalNeighbours[i].neighbours[newNode] = null;
                    newNode.neighbours[originalNeighbours[i]] = null;
                }
            }
            newNodePolygonCW.Add(this.polygon[cutEndIdx]);

            /// Create the new polygons...
            RCPolygon thisNewPolygon = new RCPolygon(thisPolygonCW);
            RCPolygon newNodePolygon = new RCPolygon(newNodePolygonCW);
            if (thisNewPolygon.DoubleOfSignedArea <= 0) { throw new InvalidOperationException("Vertices of the new polygon are in CCW order!"); }
            if (newNodePolygon.DoubleOfSignedArea <= 0) { throw new InvalidOperationException("Vertices of the new polygon are in CCW order!"); }
            this.neighbours[newNode] = null;
            newNode.neighbours[this] = null;

            /// ...and update the bounding boxes.
            if (this.BoundingBoxChanging != null) { this.BoundingBoxChanging(this); }
            this.polygon = thisNewPolygon;
            if (this.BoundingBoxChanged != null) { this.BoundingBoxChanged(this); }
            if (newNode.BoundingBoxChanging != null) { newNode.BoundingBoxChanging(newNode); }
            newNode.polygon = newNodePolygon;
            if (newNode.BoundingBoxChanged != null) { newNode.BoundingBoxChanged(newNode); }

            return new List<NavMeshNode>() { this, newNode };
        }

        /// <summary>
        /// Joins the given vertex to all vertices on the boundary of this node and constructs new slices along these edges.
        /// </summary>
        /// <param name="vertex">The vertex inside this node.</param>
        /// <returns>The list of the slices.</returns>
        /// <exception cref="ArgumentException">If the given vertex is not inside the polygon of this node.</exception>
        /// <exception cref="InvalidOperationException">If the polygon of this node is not convex.</exception>
        internal IEnumerable<NavMeshNode> Slice(RCNumVector vertex)
        {
            if (!this.polygon.Contains(vertex)) { throw new ArgumentException("The vertex shall be inside the polygon of this node!", "vertex"); }

            /// Collect the original neighbours of this node in the order of the vertices of this node.
            NavMeshNode[] originalNeighbours = this.GetNeighboursByEdges();

            /// Create the new slices and set their neighbourhood between each other and the original neighbours of this node.
            RCSet<NavMeshNode> slices = new RCSet<NavMeshNode>();
            NavMeshNode prevSlice = null;
            NavMeshNode firstSlice = null;
            NavMeshNode lastSlice = null;
            for (int i = 0; i < this.polygon.VertexCount - 1; i++)
            {
                NavMeshNode newSlice = new NavMeshNode(this.polygon[i], this.polygon[i + 1], vertex, this.idSource);
                slices.Add(newSlice);
                if (i == 0) { firstSlice = newSlice; } else if (i == this.polygon.VertexCount - 2) { lastSlice = newSlice; }
                if (prevSlice != null)
                {
                    prevSlice.neighbours[newSlice] = null;
                    newSlice.neighbours[prevSlice] = null;
                }
                if (originalNeighbours[i] != null)
                {
                    originalNeighbours[i].neighbours.Remove(this);
                    originalNeighbours[i].neighbours[newSlice] = null;
                    newSlice.neighbours[originalNeighbours[i]] = null;
                }
                prevSlice = newSlice;
            }

            /// Finally modify the polygon of this node, add it to the slice list, set its neighbours and return with the created slice list.
            RCPolygon thisNewPolygon = new RCPolygon(this.polygon[this.polygon.VertexCount - 1], this.polygon[0], vertex);
            if (this.BoundingBoxChanging != null) { this.BoundingBoxChanging(this); }
            this.polygon = thisNewPolygon;
            if (this.BoundingBoxChanged != null) { this.BoundingBoxChanged(this); }

            this.neighbours.Clear();
            this.neighbours[firstSlice] = null;
            firstSlice.neighbours[this] = null;
            this.neighbours[lastSlice] = null;
            lastSlice.neighbours[this] = null;
            if (originalNeighbours[originalNeighbours.Length - 1] != null)
            {
                this.neighbours[originalNeighbours[originalNeighbours.Length - 1]] = null;
                originalNeighbours[originalNeighbours.Length - 1].neighbours[this] = null;
            }
            slices.Add(this);
            return slices;
        }

        /// <summary>
        /// Calculates the informations of the edges of this navmesh node.
        /// </summary>
        internal void CalculateEdgeData()
        {
            List<NavMeshNode> neighboursCopy = new List<NavMeshNode>(this.neighbours.Keys);
            foreach (NavMeshNode neighbour in neighboursCopy) { this.neighbours[neighbour] = new NavMeshEdge(this, neighbour); }
        }

        #endregion Internal methods

        #region Private methods

        /// <summary>
        /// Internal ctor.
        /// </summary>
        /// <param name="idSource">The enumerator that provides the IDs for the NavMeshNodes.</param>
        private NavMeshNode(IEnumerator<int> idSource)
        {
            if (idSource == null) { throw new ArgumentNullException("idSource"); }
            if (!idSource.MoveNext()) { throw new InvalidOperationException("No ID for this NavMeshNode could be retrieved!"); }

            this.idSource = idSource;
            this.id = this.idSource.Current;
            this.neighbours = new Dictionary<NavMeshNode, NavMeshEdge>();
        }

        /// <summary>
        /// Merges the polygon of this node with the given polygon.
        /// </summary>
        /// <param name="neighbour">The polygon to merge with.</param>
        private void MergePolygons(RCPolygon neighbour)
        {
            /// Collect the vertices of this polygon in CW order and the vertices of the neighbour polygon in CCW order.
            List<RCNumVector> thisPolygonCW = new List<RCNumVector>();
            List<RCNumVector> neighbourPolygonCCW = new List<RCNumVector>();
            for (int i = 0; i < this.polygon.VertexCount; i++) { thisPolygonCW.Add(this.polygon[i]); }
            for (int i = neighbour.VertexCount - 1; i >= 0; i--) { neighbourPolygonCCW.Add(neighbour[i]); }

            /// Calculate the first and the last index of the common part in the vertex list of this node.
            int firstCommonIdxInThis = -1;
            int lastCommonIdxInThis = -1;
            for (int idxInThis = 0; idxInThis < thisPolygonCW.Count; idxInThis++)
            {
                int idxInNeighbour = neighbourPolygonCCW.IndexOf(thisPolygonCW[idxInThis]);
                int nextIdxInNeighbour = neighbourPolygonCCW.IndexOf(thisPolygonCW[(idxInThis + 1) % thisPolygonCW.Count]);
                if (idxInNeighbour != -1 && nextIdxInNeighbour == -1)
                {
                    if (lastCommonIdxInThis != -1) { throw new InvalidOperationException("The series of common edges is not continuous in the vertex list of this node!"); }
                    lastCommonIdxInThis = idxInThis;
                }
                else if (idxInNeighbour == -1 && nextIdxInNeighbour != -1)
                {
                    if (firstCommonIdxInThis != -1) { throw new InvalidOperationException("The series of common edges is not continuous in the vertex list of this node!"); }
                    firstCommonIdxInThis = (idxInThis + 1) % thisPolygonCW.Count;
                }
                else if (idxInNeighbour != -1 && nextIdxInNeighbour != -1 && (idxInNeighbour + 1) % neighbourPolygonCCW.Count != nextIdxInNeighbour)
                {
                    if (firstCommonIdxInThis != -1 || lastCommonIdxInThis != -1) { throw new InvalidOperationException("The series of common edges is not continuous in the vertex list of this node!"); }
                    firstCommonIdxInThis = (idxInThis + 1) % thisPolygonCW.Count;
                    lastCommonIdxInThis = idxInThis; 
                }
            }
            if (lastCommonIdxInThis == -1 || firstCommonIdxInThis == -1) { throw new InvalidOperationException("The series of common edges could not be found in the vertex list of this node!"); }
            
            /// Calculate the first and the last index of the common part in the vertex list of the neighbour node.
            int firstCommonIdxInNeighbour = -1;
            int lastCommonIdxInNeighbour = -1;
            for (int idxInNeighbour = 0; idxInNeighbour < neighbourPolygonCCW.Count; idxInNeighbour++)
            {
                int idxInThis = thisPolygonCW.IndexOf(neighbourPolygonCCW[idxInNeighbour]);
                int nextIdxInThis = thisPolygonCW.IndexOf(neighbourPolygonCCW[(idxInNeighbour + 1) % neighbourPolygonCCW.Count]);
                if (idxInThis != -1 && nextIdxInThis == -1)
                {
                    if (lastCommonIdxInNeighbour != -1) { throw new InvalidOperationException("The series of common edges is not continuous in the vertex list of the neighbour node!"); }
                    lastCommonIdxInNeighbour = idxInNeighbour;
                }
                else if (idxInThis == -1 && nextIdxInThis != -1)
                {
                    if (firstCommonIdxInNeighbour != -1) { throw new InvalidOperationException("The series of common edges is not continuous in the vertex list of the neighbour node!"); }
                    firstCommonIdxInNeighbour = (idxInNeighbour + 1) % neighbourPolygonCCW.Count;
                }
                else if (idxInThis != -1 && nextIdxInThis != -1 && (idxInThis + 1) % thisPolygonCW.Count != nextIdxInThis)
                {
                    if (firstCommonIdxInNeighbour != -1 || lastCommonIdxInNeighbour != -1) { throw new InvalidOperationException("The series of common edges is not continuous in the vertex list of the neighbour node!"); }
                    firstCommonIdxInNeighbour = (idxInNeighbour + 1) % neighbourPolygonCCW.Count;
                    lastCommonIdxInNeighbour = idxInNeighbour;
                }
            }
            if (lastCommonIdxInNeighbour == -1 || firstCommonIdxInNeighbour == -1) { throw new InvalidOperationException("The series of common edges could not be found in the vertex list of the neighbour node!"); }

            /// Calculate the number of common vertices in this node and in the neighbour node. Check if they are equals.
            int commonVerticesInThis = lastCommonIdxInThis >= firstCommonIdxInThis ? lastCommonIdxInThis - firstCommonIdxInThis + 1 : thisPolygonCW.Count - (firstCommonIdxInThis - lastCommonIdxInThis - 1);
            int commonVerticesInNeighbour = lastCommonIdxInNeighbour >= firstCommonIdxInNeighbour ? lastCommonIdxInNeighbour - firstCommonIdxInNeighbour + 1 : neighbourPolygonCCW.Count - (firstCommonIdxInNeighbour - lastCommonIdxInNeighbour - 1);
            if (commonVerticesInThis != commonVerticesInNeighbour) { throw new InvalidOperationException("Common vertex count mismatch!"); }

            /// Collect the vertices of the new polygon.
            List<RCNumVector> newPolygonCW = new List<RCNumVector>();
            for (int i = lastCommonIdxInThis; i != firstCommonIdxInThis; i = (i + 1) % thisPolygonCW.Count)
            {
                newPolygonCW.Add(thisPolygonCW[i]);
            }
            newPolygonCW.Add(thisPolygonCW[firstCommonIdxInThis]);
            for (int i = (firstCommonIdxInNeighbour + neighbourPolygonCCW.Count - 1) % neighbourPolygonCCW.Count;
                 i != lastCommonIdxInNeighbour;
                 i = (i + neighbourPolygonCCW.Count - 1) % neighbourPolygonCCW.Count)
            {
                newPolygonCW.Add(neighbourPolygonCCW[i]);
            }

            /// Create the new polygon and update the bounding box.
            RCPolygon thisNewPolygon = new RCPolygon(newPolygonCW);
            if (thisNewPolygon.DoubleOfSignedArea <= 0) { throw new InvalidOperationException("Vertices of the new polygon are in CCW order!"); }
            if (this.BoundingBoxChanging != null) { this.BoundingBoxChanging(this); }
            this.polygon = thisNewPolygon;
            if (this.BoundingBoxChanged != null) { this.BoundingBoxChanged(this); }
        }

        /// <summary>
        /// Collects the neighbours of this node in the order of the vertices of this node.
        /// </summary>
        /// <returns>
        /// An array that contains references to the neighbours of this node in the order of the vertices of this node.
        /// The Nth item in this array is the reference to the neighbour along the edge between vertex N and (N+1).
        /// If there is no neighbour along the edge between vertex N and (N+1) then the Nth item in this array is null.
        /// </returns>
        private NavMeshNode[] GetNeighboursByEdges()
        {
            NavMeshNode[] neighbourList = new NavMeshNode[this.polygon.VertexCount];
            RCSet<NavMeshNode> neighboursCopy = new RCSet<NavMeshNode>(this.neighbours.Keys);
            for (int edgeIdx = 0; edgeIdx < this.polygon.VertexCount; edgeIdx++)
            {
                NavMeshNode neighbourAtEdge = null;
                foreach (NavMeshNode neighbour in this.neighbours.Keys)
                {
                    int edgeBeginIdxInNeighbour = neighbour.polygon.IndexOf(this.polygon[edgeIdx]);
                    int edgeEndIdxInNeighbour = neighbour.polygon.IndexOf(this.polygon[(edgeIdx + 1) % this.polygon.VertexCount]);
                    if (edgeBeginIdxInNeighbour != -1 && edgeEndIdxInNeighbour != -1 &&
                        (edgeEndIdxInNeighbour + 1) % neighbour.polygon.VertexCount == edgeBeginIdxInNeighbour)
                    {
                        neighbourAtEdge = neighbour;
                        break;
                    }
                }
                neighbourList[edgeIdx] = neighbourAtEdge;
                if (neighbourAtEdge != null) { neighboursCopy.Remove(neighbourAtEdge); }
            }
            if (neighboursCopy.Count != 0) { throw new InvalidOperationException("Not every neighbours have been found by edges!"); }
            return neighbourList;
        }

        #endregion Private methods

        /// <summary>
        /// The list of neighbours of this node and the informations about the corresponding edges.
        /// </summary>
        private Dictionary<NavMeshNode, NavMeshEdge> neighbours;

        /// <summary>
        /// The polygon that represents the area of this node on the 2D plane.
        /// </summary>
        private RCPolygon polygon;

        /// <summary>
        /// The ID of this node.
        /// </summary>
        private int id;

        /// <summary>
        /// The enumerator that provides the IDs for the NavMeshNodes.
        /// </summary>
        private IEnumerator<int> idSource;
    }
}
