using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// Represents a Polygon.
    /// </summary>
    class Polygon
    {
        /// <summary>
        /// Constructs a Polygon by giving its vertices.
        /// </summary>
        /// <param name="vertex0">The first vertex of this Polygon.</param>
        /// <param name="vertex1">The second vertex of this Polygon.</param>
        /// <param name="vertex2">The third vertex of this Polygon.</param>
        /// <param name="otherVertices">List of the fourth and other vertices of this Polygon (optional).</param>
        public Polygon(RCNumVector vertex0, RCNumVector vertex1, RCNumVector vertex2, params RCNumVector[] otherVertices)
        {
            if (vertex0 == RCNumVector.Undefined) { throw new ArgumentNullException("vertex0"); }
            if (vertex1 == RCNumVector.Undefined) { throw new ArgumentNullException("vertex1"); }
            if (vertex2 == RCNumVector.Undefined) { throw new ArgumentNullException("vertex2"); }

            this.doubleOfSignedAreaCache = new CachedValue<RCNumber>(this.CalculateDoubleOfSignedArea);
            this.centerCache = new CachedValue<RCNumVector>(this.CalculateCenter);
            this.isConvexCache = new CachedValue<bool>(this.CalculateConvexity);
            this.boundingBoxCache = new CachedValue<RCNumRectangle>(this.CalculateBoundingBox);

            HashSet<RCNumVector> vertexSet = new HashSet<RCNumVector>();
            this.vertices = new List<RCNumVector>() { vertex0, vertex1, vertex2 };
            if (!vertexSet.Add(vertex0)) { throw new ArgumentException("Duplicated vertices!"); }
            if (!vertexSet.Add(vertex1)) { throw new ArgumentException("Duplicated vertices!"); }
            if (!vertexSet.Add(vertex2)) { throw new ArgumentException("Duplicated vertices!"); }

            foreach (RCNumVector vertex in otherVertices)
            {
                if (!vertexSet.Add(vertex)) { throw new ArgumentException("Duplicated vertices!"); }
                this.vertices.Add(vertex);
            }
        }

        /// <summary>
        /// Constructs a Polygon by giving its vertices.
        /// </summary>
        /// <param name="vertexList">List of the vertices of this Polygon.</param>
        /// <exception cref="ArgumentException">
        /// If the given vertex list contains less than 3 vertices.
        /// If the given vertex list contains RCNumVector.Undefined.
        /// If the given vertex list contains duplicated vertices.
        /// </exception>
        public Polygon(List<RCNumVector> vertexList)
        {
            if (vertexList == null) { throw new ArgumentNullException("vertexList"); }
            if (vertexList.Count < 3) { throw new ArgumentException("A polygon must have at least 3 vertices!", "vertexList"); }

            this.doubleOfSignedAreaCache = new CachedValue<RCNumber>(this.CalculateDoubleOfSignedArea);
            this.centerCache = new CachedValue<RCNumVector>(this.CalculateCenter);
            this.isConvexCache = new CachedValue<bool>(this.CalculateConvexity);
            this.boundingBoxCache = new CachedValue<RCNumRectangle>(this.CalculateBoundingBox);

            HashSet<RCNumVector> vertexSet = new HashSet<RCNumVector>();
            this.vertices = new List<RCNumVector>();
            foreach (RCNumVector vertex in vertexList)
            {
                if (vertex == RCNumVector.Undefined) { throw new ArgumentException("Undefined vertex found in the vertex list!", "vertexList"); }
                if (!vertexSet.Add(vertex)) { throw new ArgumentException("Duplicated vertices!"); }
                this.vertices.Add(vertex);
            }
        }

        /// <summary>
        /// Gets the number of vertices of this Polygon.
        /// </summary>
        public int VertexCount { get { return this.vertices.Count; } }

        /// <summary>
        /// Gets the vertex of this Polygon with the given index.
        /// </summary>
        /// <param name="index">The index of the vertex to get.</param>
        /// <returns>The vertex of this Polygon with the given index.</returns>
        public RCNumVector this[int index] { get { return this.vertices[index]; } }

        /// <summary>
        /// Gets the index of the given vertex in the vertex-list of this polygon.
        /// </summary>
        /// <param name="vertex">The vertex to be searched.</param>
        /// <returns>The zero-based index of the given vertex in the vertex-list of this polygon is found; otherwise -1.</returns>
        public int IndexOf(RCNumVector vertex) { return this.vertices.IndexOf(vertex); }

        /// <summary>
        /// Gets the double of the signed area of this polygon. The signed area is positive if the vertices of this polygon
        /// are ordered clockwise and negative if they are ordered counter-clockwise.
        /// In case of self-intersecting polygons the signed area tells you whether the orientation is "mostly" clockwise or
        /// counter-clockwise.
        /// </summary>
        public RCNumber DoubleOfSignedArea { get { return this.doubleOfSignedAreaCache.Value; } }

        /// <summary>
        /// Gets the center of this polygon. The center of a polygon equals with the average of its vertices.
        /// </summary>
        public RCNumVector Center { get { return this.centerCache.Value; } }

        /// <summary>
        /// Gets the bounding box of this polygon.
        /// </summary>
        public RCNumRectangle BoundingBox { get { return this.boundingBoxCache.Value; } }

        /// <summary>
        /// Gets whether this polygon is convex or not.
        /// </summary>
        public bool IsConvex { get { return this.isConvexCache.Value; } }
        
        /// <summary>
        /// Determines if the specified point is contained within this polygon.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <returns>Returns true if the point is contained within this polygon, false otherwise.</returns>
        /// <remarks>Points on the edges are considered to be contained by this polygon.</remarks>
        public bool Contains(RCNumVector point)
        {
            if (point == RCNumVector.Undefined) { throw new ArgumentNullException("point"); }

            /// Count the intersections of a ray starting from the point and going to infinity along the positive X-axis.
            int intersections = 0;
            for (int edgeIdx = 0; edgeIdx < this.vertices.Count; edgeIdx++)
            {
                /// Get the endpoints of the current edge.
                RCNumVector edgeBegin = this.vertices[edgeIdx];
                RCNumVector edgeEnd = this.vertices[edgeIdx < this.vertices.Count - 1 ? edgeIdx + 1 : 0];

                /// Check the trivial cases.
                if (point == edgeBegin || point == edgeEnd) { return true; } /// Trivial containment.
                if (point.Y == edgeBegin.Y && point.Y == edgeEnd.Y &&
                   (point.X >= edgeBegin.X && point.X <= edgeEnd.X || point.X >= edgeEnd.X && point.X <= edgeBegin.X)) { return true; } /// Trivial containment.
                if (point.X == edgeBegin.X && point.X == edgeEnd.X &&
                   (point.Y >= edgeBegin.Y && point.Y <= edgeEnd.Y || point.Y >= edgeEnd.Y && point.Y <= edgeBegin.Y)) { return true; } /// Trivial containment.
                if (edgeBegin.X < point.X && edgeEnd.X < point.X) { continue; } /// Trivial no intersection.
                if (edgeBegin.Y <= point.Y && edgeEnd.Y <= point.Y) { continue; } /// Trivial no intersection.
                if (edgeBegin.Y > point.Y && edgeEnd.Y > point.Y) { continue; } /// Trivial no intersection.
                if (edgeBegin.X >= point.X && edgeEnd.X >= point.X &&
                   (edgeBegin.Y <= point.Y && edgeEnd.Y > point.Y || edgeEnd.Y <= point.Y && edgeBegin.Y > point.Y))
                {
                    /// Trivial intersection.
                    intersections++;
                    continue;
                }

                /// Check the non-trivial case.
                Polygon testTriangle = edgeBegin.Y < edgeEnd.Y ? new Polygon(point, edgeBegin, edgeEnd) : new Polygon(point, edgeEnd, edgeBegin);
                if (testTriangle.DoubleOfSignedArea == 0) { return true; } /// Point exactly on the edge -> containment.
                if (testTriangle.DoubleOfSignedArea > 0) { intersections++; }
            }

            /// The point is inside the polygon if and only if the number of intersections is odd.
            return intersections % 2 != 0;
        }

        #region Calculation methods for the cached properties of this polygon

        /// <summary>
        /// Calculates the double of the signed area of this polygon.
        /// </summary>
        /// <returns>The double of the signed area of this polygon.</returns>
        private RCNumber CalculateDoubleOfSignedArea()
        {
            RCNumber doubleOfSignedArea = 0;
            for (int i = 0; i < this.vertices.Count; i++)
            {
                RCNumVector v1 = this.vertices[i];
                RCNumVector v2 = this.vertices[(i + 1) % this.vertices.Count];
                doubleOfSignedArea += (v1.X - v2.X) * (v1.Y + v2.Y);
            }
            return doubleOfSignedArea;
        }

        /// <summary>
        /// Calculates the center of this polygon.
        /// </summary>
        /// <returns>The center of this polygon.</returns>
        private RCNumVector CalculateCenter()
        {
            RCNumVector center = new RCNumVector(0, 0);
            for (int i = 0; i < this.vertices.Count; i++) { center += this.vertices[i]; }
            center /= this.vertices.Count;
            return center;
        }

        /// <summary>
        /// Calculates the convexity of this polygon.
        /// </summary>
        /// <returns>True if this polygon is convex; otherwise false.</returns>
        private bool CalculateConvexity()
        {
            int curveDirection = 0; /// 0: undefined, +1: CW, -1: CCW
            for (int i = 0; i < this.vertices.Count; i++)
            {
                RCNumVector vPrev = this.vertices[i > 0 ? i - 1 : this.vertices.Count - 1];
                RCNumVector vCurr = this.vertices[i];
                RCNumVector vNext = this.vertices[(i + 1) % this.vertices.Count];

                Polygon testTriangle = new Polygon(vCurr, vNext, vPrev);
                if (testTriangle.DoubleOfSignedArea > 0)
                {
                    if (curveDirection == -1) { return false; }
                    curveDirection = +1;
                }
                else if (testTriangle.DoubleOfSignedArea < 0)
                {
                    if (curveDirection == +1) { return false; }
                    curveDirection = -1;
                }
            }
            if (curveDirection == 0) { throw new InvalidOperationException("Unable to determine convexity!"); }
            return true;
        }

        /// <summary>
        /// Calculate the bounding box of this polygon.
        /// </summary>
        /// <returns>The bounding box of this polygon.</returns>
        private RCNumRectangle CalculateBoundingBox()
        {
            RCNumber minX = 0, maxX = 0, minY = 0, maxY = 0;
            for (int i = 0; i < this.vertices.Count; i++)
            {
                if (i == 0)
                {
                    minX = maxX = this.vertices[i].X;
                    minY = maxY = this.vertices[i].Y;
                    continue;
                }

                if (this.vertices[i].X < minX) { minX = this.vertices[i].X; }
                if (this.vertices[i].X > maxX) { maxX = this.vertices[i].X; }
                if (this.vertices[i].Y < minY) { minY = this.vertices[i].Y; }
                if (this.vertices[i].Y > maxY) { maxY = this.vertices[i].Y; }
            }

            if (maxX - minX != 0 && maxY - minY != 0) { return new RCNumRectangle(minX, minY, maxX - minX + new RCNumber(1), maxY - minY + new RCNumber(1)); }
            else { return RCNumRectangle.Undefined; }
        }

        #endregion Calculation methods for the cached properties of this polygon

        /// <summary>
        /// The vertices of this polygon in order such that the walkable area is at left hand side.
        /// </summary>
        private List<RCNumVector> vertices;

        /// <summary>
        /// The cache of the double of the signed area of this polygon.
        /// </summary>
        private CachedValue<RCNumber> doubleOfSignedAreaCache;

        /// <summary>
        /// The cache of the center of this polygon.
        /// </summary>
        private CachedValue<RCNumVector> centerCache;

        /// <summary>
        /// The cache of the bounding box of this polygon.
        /// </summary>
        private CachedValue<RCNumRectangle> boundingBoxCache;

        /// <summary>
        /// The cache of the convexity of this polygon.
        /// </summary>
        private CachedValue<bool> isConvexCache;
    }
}
