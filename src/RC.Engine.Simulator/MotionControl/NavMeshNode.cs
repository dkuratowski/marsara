using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// Represents a node in a navmesh graph.
    /// </summary>
    class NavMeshNode : ISearchTreeContent
    {
        /// <summary>
        /// Constructs a triangular NavMeshNode.
        /// </summary>
        /// <param name="vertex0">The first vertex of the triangle.</param>
        /// <param name="vertex1">The second vertex of the triangle.</param>
        /// <param name="vertex2">The third vertex of the triangle.</param>
        internal NavMeshNode(RCNumVector vertex0, RCNumVector vertex1, RCNumVector vertex2) : this()
        {
            this.polygon = new Polygon(vertex0, vertex1, vertex2);
            if (this.polygon.DoubleOfSignedArea == 0) { throw new ArgumentException("The vertices of the triangle are colinear!"); }
            this.neighbours = new HashSet<NavMeshNode>();

            /// If the vertices are in counter-clockwise order put them into clockwise order.
            if (this.polygon.DoubleOfSignedArea < 0) { this.polygon = new Polygon(vertex2, vertex1, vertex0); }

            /// Calculate the bounding box of this NavMeshNode.
            this.UpdateBoundingBox();
        }

        #region ISearchTreeNode members

        /// <see cref="ISearchTreeContent.BoundingBox"/>
        public RCNumRectangle BoundingBox { get { return this.boundingBox; } }

        /// <see cref="ISearchTreeContent.BoundingBoxChanging"/>
        public event ContentBoundingBoxChangeHdl BoundingBoxChanging;

        /// <see cref="ISearchTreeContent.BoundingBoxChanged"/>
        public event ContentBoundingBoxChangeHdl BoundingBoxChanged;

        #endregion ISearchTreeNode members

        /// <summary>
        /// Gets the neighbours of this node.
        /// </summary>
        public IEnumerable<NavMeshNode> Neighbours { get { return this.neighbours; } }

        /// <summary>
        /// Merges this NavMeshNode with one of its given parent. After the merge operation this NavMeshNode will represent the merged node and the
        /// NavMeshNode given in the parameter will be detached from its parents and will become obsolate.
        /// </summary>
        /// <param name="neighbour">The neighbour to merge with.</param>
        public void MergeWith(NavMeshNode neighbour)
        {
            if (neighbour == null) { throw new ArgumentNullException("neighbour"); }
            if (!this.neighbours.Contains(neighbour)) { throw new ArgumentException("The given node is not the neighbour of this node!", "neighbour"); }

            /// Remove the reference between the given neighbour and this node in both directions.
            this.neighbours.Remove(neighbour);
            neighbour.neighbours.Remove(this);

            /// Process all the remaining neighbours of the given neighbour to point only to this node.
            foreach (NavMeshNode neighbourOfNeighbour in neighbour.neighbours)
            {
                if (neighbourOfNeighbour.neighbours.Remove(neighbour))
                {
                    neighbourOfNeighbour.neighbours.Add(this);
                    this.neighbours.Add(neighbourOfNeighbour);
                }
            }
            neighbour.neighbours.Clear();

            /// Merge the polygons of the two nodes.
            this.MergePolygons(neighbour.polygon);
        }

        /// <summary>
        /// Deletes this node.
        /// </summary>
        public void Delete()
        {
            foreach (NavMeshNode neighbour in this.neighbours) { neighbour.neighbours.Remove(this); }
            this.neighbours.Clear();
        }

        /// <summary>
        /// Slices this node along a given series of segments.
        /// </summary>
        /// <param name="cut">The series of the cut segments.</param>
        /// <returns>The list of the slices.</returns>
        public IEnumerable<NavMeshNode> Slice(List<RCNumVector> cut)
        {
            if (cut == null) { throw new ArgumentNullException("cut"); }
            if (cut.Count < 2) { throw new ArgumentException("Cut shall contain at least 2 vertices!", "cut"); }

            int cutBeginIdx = this.polygon.IndexOf(cut[0]);
            int cutEndIdx = this.polygon.IndexOf(cut[cut.Count - 1]);
            if (cutBeginIdx == -1) { throw new ArgumentException("The beginning of the cut shall be a vertex of this node!", "cut"); }
            if (cutEndIdx == -1) { throw new ArgumentException("The end of the cut shall be a vertex of this node!", "cut"); }
            for (int i = 1; i < cut.Count - 1; i++) { if (!this.polygon.Contains(cut[i])) { throw new ArgumentException("Every vertex of the cut shall be inside this node!", "cut"); } }
            if (cut.Count == 2 &&
                ((cutBeginIdx + 1) % this.polygon.VertexCount == cutEndIdx ||
                (cutEndIdx + 1) % this.polygon.VertexCount == cutBeginIdx)) { throw new ArgumentException("The given cut equals with an edge!"); }

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
                    originalNeighbours[i].neighbours.Add(this);
                    this.neighbours.Add(originalNeighbours[i]);
                }
            }
            for (int i = 0; i < cut.Count - 1; i++) { thisPolygonCW.Add(cut[i]); }

            /// Create the new node and set its neighbour relationships.
            List<RCNumVector> newNodePolygonCW = new List<RCNumVector>();
            NavMeshNode newNode = new NavMeshNode();
            for (int i = cutBeginIdx; i != cutEndIdx; i = (i + 1) % this.polygon.VertexCount)
            {
                newNodePolygonCW.Add(this.polygon[i]);
                if (originalNeighbours[i] != null)
                {
                    originalNeighbours[i].neighbours.Add(newNode);
                    newNode.neighbours.Add(originalNeighbours[i]);
                }
            }
            for (int i = cut.Count - 1; i > 0; i--) { newNodePolygonCW.Add(cut[i]); }

            /// Create the new polygons and update the bounding boxes.
            this.polygon = new Polygon(thisPolygonCW);
            newNode.polygon = new Polygon(newNodePolygonCW);
            if (this.polygon.DoubleOfSignedArea <= 0) { throw new InvalidOperationException("Vertices of the new polygon are in CCW order!"); }
            if (newNode.polygon.DoubleOfSignedArea <= 0) { throw new InvalidOperationException("Vertices of the new polygon are in CCW order!"); }
            this.neighbours.Add(newNode);
            newNode.neighbours.Add(this);
            this.UpdateBoundingBox();
            newNode.UpdateBoundingBox();

            return new List<NavMeshNode>() { this, newNode };
        }

        /// <summary>
        /// Joins the given vertex to all vertices on the boundary of this node and constructs new slices along these edges.
        /// </summary>
        /// <param name="vertex">The vertex inside this node.</param>
        /// <returns>The list of the slices.</returns>
        /// <exception cref="ArgumentException">If the given vertex is not inside the polygon of this node.</exception>
        /// <exception cref="InvalidOperationException">If the polygon of this node is not convex.</exception>
        public IEnumerable<NavMeshNode> Slice(RCNumVector vertex)
        {
            if (!this.polygon.Contains(vertex)) { throw new ArgumentException("The vertex shall be inside the polygon of this node!", "vertex"); }

            /// Collect the original neighbours of this node in the order of the vertices of this node.
            NavMeshNode[] originalNeighbours = this.GetNeighboursByEdges();

            /// Create the new slices and set their neighbourhood between each other and the original neighbours of this node.
            HashSet<NavMeshNode> slices = new HashSet<NavMeshNode>();
            NavMeshNode prevSlice = null;
            NavMeshNode firstSlice = null;
            NavMeshNode lastSlice = null;
            for (int i = 0; i < this.polygon.VertexCount - 1; i++)
            {
                NavMeshNode newSlice = new NavMeshNode(this.polygon[i], this.polygon[i + 1], vertex);
                slices.Add(newSlice);
                if (i == 0) { firstSlice = newSlice; } else if (i == this.polygon.VertexCount - 2) { lastSlice = newSlice; }
                if (prevSlice != null)
                {
                    prevSlice.neighbours.Add(newSlice);
                    newSlice.neighbours.Add(prevSlice);
                }
                if (originalNeighbours[i] != null)
                {
                    originalNeighbours[i].neighbours.Remove(this);
                    originalNeighbours[i].neighbours.Add(newSlice);
                    newSlice.neighbours.Add(originalNeighbours[i]);
                }
                prevSlice = newSlice;
            }

            /// Finally modify the polygon of this node, add it to the slice list, set its neighbours and return with the created slice list.
            this.polygon = new Polygon(this.polygon[this.polygon.VertexCount - 1], this.polygon[0], vertex);
            this.UpdateBoundingBox();
            this.neighbours.Clear();
            this.neighbours.Add(firstSlice);
            firstSlice.neighbours.Add(this);
            this.neighbours.Add(lastSlice);
            lastSlice.neighbours.Add(this);
            if (originalNeighbours[originalNeighbours.Length - 1] != null)
            {
                this.neighbours.Add(originalNeighbours[originalNeighbours.Length - 1]);
                originalNeighbours[originalNeighbours.Length - 1].neighbours.Add(this);
            }
            slices.Add(this);
            return slices;
        }

        /// <summary>
        /// Gets the polygon that represents the area of this node on the 2D plane.
        /// </summary>
        public Polygon Polygon { get { return this.polygon; } }

        /// <summary>
        /// Internal ctor.
        /// </summary>
        private NavMeshNode()
        {
            this.id = nextID++;
            this.neighbours = new HashSet<NavMeshNode>();
        }
        internal int ID { get { return this.id; } }
        private int id;
        private static int nextID = 0;

        /// <summary>
        /// Merges the polygon of this node with the given polygon.
        /// </summary>
        /// <param name="neighbour">The polygon to merge with.</param>
        private void MergePolygons(Polygon neighbour)
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
            this.polygon = new Polygon(newPolygonCW);
            if (this.polygon.DoubleOfSignedArea <= 0) { throw new InvalidOperationException("Vertices of the new polygon are in CCW order!"); }
            this.UpdateBoundingBox();
        }

        /// <summary>
        /// Updates the bounding box of this NavMeshNode.
        /// </summary>
        private void UpdateBoundingBox()
        {
            RCNumber minX = 0, maxX = 0, minY = 0, maxY = 0;
            for (int i = 0; i < this.polygon.VertexCount; i++)
            {
                if (i == 0)
                {
                    minX = maxX = this.polygon[i].X;
                    minY = maxY = this.polygon[i].Y;
                    continue;
                }

                if (this.polygon[i].X < minX) { minX = this.polygon[i].X; }
                if (this.polygon[i].X > maxX) { maxX = this.polygon[i].X; }
                if (this.polygon[i].Y < minY) { minY = this.polygon[i].Y; }
                if (this.polygon[i].Y > maxY) { maxY = this.polygon[i].Y; }
            }

            if (this.BoundingBoxChanging != null) { this.BoundingBoxChanging(this); }
            this.boundingBox = new RCNumRectangle(minX, minY, maxX - minX, maxY - minY);
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
            foreach (NavMeshNode neighbour in this.neighbours)
            {
                List<int> commonNodeIndices = new List<int>();
                for (int i = neighbour.polygon.VertexCount - 1; i >= 0; i--)
                {
                    int idx = this.polygon.IndexOf(neighbour.polygon[i]);
                    if (idx != -1) { commonNodeIndices.Add(idx); }
                }
                if (commonNodeIndices.Count < 2) { throw new InvalidOperationException("Common edge not found between the sliced node and one of its neighbours!"); }
                commonNodeIndices.Sort();
                for (int i = 1; i < commonNodeIndices.Count; i++)
                {
                    if (commonNodeIndices[i] - commonNodeIndices[i - 1] == 1) { neighbourList[commonNodeIndices[i - 1]] = neighbour; }
                }
                if (commonNodeIndices[commonNodeIndices.Count - 1] == this.polygon.VertexCount - 1 && commonNodeIndices[0] == 0)
                {
                    neighbourList[commonNodeIndices[commonNodeIndices.Count - 1]] = neighbour;
                }
            }
            return neighbourList;
        }

        /// <summary>
        /// The list of neighbours of this node.
        /// </summary>
        private HashSet<NavMeshNode> neighbours;

        /// <summary>
        /// The polygon that represents the area of this node on the 2D plane.
        /// </summary>
        private Polygon polygon;

        /// <summary>
        /// The bounding box of this NavMeshNode.
        /// </summary>
        private RCNumRectangle boundingBox;
    }
}
