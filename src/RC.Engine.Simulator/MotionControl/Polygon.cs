﻿using RC.Common;
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
        /// Constructs a Polygon that is the countour of a continuous walkable area on the given walkability grid.
        /// </summary>
        /// <param name="grid">The grid that contains the walkability informations.</param>
        /// <param name="upperLeftCorner">An upper-left corner cell in the area that shall be contoured by this Polygon.</param>
        /// <param name="maxError">The maximum error between the edge of the created polygon and the walkability informations.</param>
        /// <remarks>The polygon is created using the "Marching squares" and the "Douglas–Peucker" algorithms.</remarks>
        public static Polygon FromGrid(IWalkabilityGrid grid, RCIntVector upperLeftCorner, RCNumber maxError)
        {
            if (grid == null) { throw new ArgumentNullException("grid"); }
            if (upperLeftCorner == RCIntVector.Undefined) { throw new ArgumentNullException("upperLeftCorner"); }
            if (maxError < 0) { throw new ArgumentOutOfRangeException("maxError", "The maximum error shall not be negative!"); }

            List<RCNumVector> initialVertices = new List<RCNumVector>();

            /// Initialize the search algorithm.
            RCIntVector currentPos = upperLeftCorner + new RCIntVector(-1, -1);
            StepDirection nextStep = StepDirection.None;
            StepDirection previousStep = StepDirection.None;
            if (grid[upperLeftCorner] && !grid[upperLeftCorner + new RCIntVector(0, -1)] && !grid[upperLeftCorner + new RCIntVector(-1, 0)])
            {
                if (Polygon.GetIndexAt(grid, currentPos) == 5) { previousStep = StepDirection.Up; }
            }
            else if (grid[upperLeftCorner] || !grid[upperLeftCorner + new RCIntVector(0, -1)] || !grid[upperLeftCorner + new RCIntVector(-1, 0)] || !grid[upperLeftCorner + new RCIntVector(-1, -1)])
            {
                throw new ArgumentException("The given cell must be an upper left corner cell in the area being contoured!", "upperLeftCorner");
            }

            /// Make steps until we get back to the starting point.
            do
            {
                /// Evaluate our state, and set up our next direction
                nextStep = Polygon.Step(grid, initialVertices, currentPos, previousStep);

                if (nextStep == StepDirection.Up) { currentPos += new RCIntVector(0, -1); }
                else if (nextStep == StepDirection.Left) { currentPos += new RCIntVector(-1, 0); }
                else if (nextStep == StepDirection.Down) { currentPos += new RCIntVector(0, 1); }
                else if (nextStep == StepDirection.Right) { currentPos += new RCIntVector(1, 0); }
                previousStep = nextStep;

            } while (nextStep != StepDirection.None);

            /// Create a smooth polygon from the found vertices.
            List<RCNumVector> vertices = Polygon.SimplifyPolyline(initialVertices, maxError, true);
            if (vertices.Count < 3) { return null; }
            Polygon newPolygon = new Polygon(vertices);
            return newPolygon;
        }

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

            this.isAreaCalculated = false;
            this.isCenterCalculated = false;
            this.isConvexityDetermined = false;

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

            this.isAreaCalculated = false;
            this.isCenterCalculated = false;
            this.isConvexityDetermined = false;

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
        public RCNumber DoubleOfSignedArea
        {
            get
            {
                if (!this.isAreaCalculated) { this.CalculateArea(); }
                return this.doubleOfSignedArea;
            }
        }

        /// <summary>
        /// Gets the center of this polygon.
        /// </summary>
        public RCNumVector Center
        {
            get
            {
                if (!this.isCenterCalculated) { this.CalculateCenter(); }
                return this.center;
            }
        }

        /// <summary>
        /// Gets whether this polygon is convex or not.
        /// </summary>
        public bool IsConvex
        {
            get
            {
                if (!this.isConvexityDetermined) { this.DetermineConvexity(); }
                return this.isConvex;
            }
        }
        
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

        #region Polygon buildup methods

        /// <summary>
        /// A simple enumeration to represent the direction we just moved, and the direction we will next move.
        /// </summary>
        private enum StepDirection
        {
            None = 0,
            Up = 1,
            Left = 2,
            Down = 3,
            Right = 4
        }

        /// <summary>
        /// Makes a step at the current position.
        /// </summary>
        /// <param name="grid">The grid that contains the walkability informations.</param>
        /// <param name="vertexList">The vertex list currently being built.</param>
        /// <param name="currentPos">The current position.</param>
        /// <param name="previousStep">The direction of the previous step.</param>
        /// <returns>The direction of the next step.</returns>
        private static StepDirection Step(IWalkabilityGrid grid, List<RCNumVector> vertexList, RCIntVector currentPos, StepDirection previousStep)
        {
            int currentIndex = Polygon.GetIndexAt(grid, currentPos);
            if (currentIndex == 1 || currentIndex == 7)
            {
                RCNumVector newVertex = new RCNumVector(currentPos) + new RCNumVector((RCNumber)1 / (RCNumber)2, (RCNumber)1 / (RCNumber)2);
                if (vertexList.Count > 0 && vertexList[0] == newVertex) { return StepDirection.None; }
                if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex) { vertexList.Add(newVertex); }
                return StepDirection.Left;
            }
            else if (currentIndex == 2 || currentIndex == 14)
            {
                RCNumVector newVertex = new RCNumVector(currentPos) + new RCNumVector((RCNumber)1 / (RCNumber)2, (RCNumber)1 / (RCNumber)2);
                if (vertexList.Count > 0 && vertexList[0] == newVertex) { return StepDirection.None; }
                if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex) { vertexList.Add(newVertex); }
                return StepDirection.Down;
            }
            else if (currentIndex == 3)
            {
                return StepDirection.Left;
            }
            else if (currentIndex == 4 || currentIndex == 13)
            {
                RCNumVector newVertex = new RCNumVector(currentPos) + new RCNumVector((RCNumber)1 / (RCNumber)2, (RCNumber)1 / (RCNumber)2);
                if (vertexList.Count > 0 && vertexList[0] == newVertex) { return StepDirection.None; }
                if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex) { vertexList.Add(newVertex); }
                return StepDirection.Right;
            }
            else if (currentIndex == 5)
            {
                if (previousStep == StepDirection.None || previousStep == StepDirection.Down)
                {
                    RCNumVector newVertex0 = new RCNumVector(currentPos) + new RCNumVector((RCNumber)1 / (RCNumber)2, 0);
                    if (vertexList.Count > 0 && vertexList[0] == newVertex0) { return StepDirection.None; }
                    if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex0) { vertexList.Add(newVertex0); }
                    RCNumVector newVertex1 = new RCNumVector(currentPos) + new RCNumVector(0, (RCNumber)1 / (RCNumber)2);
                    if (vertexList.Count > 0 && vertexList[0] == newVertex1) { return StepDirection.None; }
                    if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex1) { vertexList.Add(newVertex1); }
                    return StepDirection.Left;
                }
                else
                {
                    RCNumVector newVertex0 = new RCNumVector(currentPos) + new RCNumVector((RCNumber)1 / (RCNumber)2, 1);
                    if (vertexList.Count > 0 && vertexList[0] == newVertex0) { return StepDirection.None; }
                    if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex0) { vertexList.Add(newVertex0); }
                    RCNumVector newVertex1 = new RCNumVector(currentPos) + new RCNumVector(1, (RCNumber)1 / (RCNumber)2);
                    if (vertexList.Count > 0 && vertexList[0] == newVertex1) { return StepDirection.None; }
                    if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex1) { vertexList.Add(newVertex1); }
                    return StepDirection.Right;
                }
            }
            else if (currentIndex == 6)
            {
                return StepDirection.Down;
            }
            else if (currentIndex == 8 || currentIndex == 11)
            {
                RCNumVector newVertex = new RCNumVector(currentPos) + new RCNumVector((RCNumber)1 / (RCNumber)2, (RCNumber)1 / (RCNumber)2);
                if (vertexList.Count > 0 && vertexList[0] == newVertex) { return StepDirection.None; }
                if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex) { vertexList.Add(newVertex); }
                return StepDirection.Up;
            }
            else if (currentIndex == 9)
            {
                return StepDirection.Up;
            }
            else if (currentIndex == 10)
            {
                if (previousStep == StepDirection.None || previousStep == StepDirection.Right)
                {
                    RCNumVector newVertex0 = new RCNumVector(currentPos) + new RCNumVector(0, (RCNumber)1 / (RCNumber)2);
                    if (vertexList.Count > 0 && vertexList[0] == newVertex0) { return StepDirection.None; }
                    if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex0) { vertexList.Add(newVertex0); }
                    RCNumVector newVertex1 = new RCNumVector(currentPos) + new RCNumVector((RCNumber)1 / (RCNumber)2, 1);
                    if (vertexList.Count > 0 && vertexList[0] == newVertex1) { return StepDirection.None; }
                    if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex1) { vertexList.Add(newVertex1); }
                    return StepDirection.Down;
                }
                else
                {
                    RCNumVector newVertex0 = new RCNumVector(currentPos) + new RCNumVector(1, (RCNumber)1 / (RCNumber)2);
                    if (vertexList.Count > 0 && vertexList[0] == newVertex0) { return StepDirection.None; }
                    if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex0) { vertexList.Add(newVertex0); }
                    RCNumVector newVertex1 = new RCNumVector(currentPos) + new RCNumVector((RCNumber)1 / (RCNumber)2, 0);
                    if (vertexList.Count > 0 && vertexList[0] == newVertex1) { return StepDirection.None; }
                    if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex1) { vertexList.Add(newVertex1); }
                    return StepDirection.Up;
                }
            }
            else if (currentIndex == 12)
            {
                return StepDirection.Right;
            }
            else
            {
                throw new InvalidOperationException("Unexpected case!");
            }
        }

        /// <summary>
        /// Simplifies the polyline given by its vertex list.
        /// </summary>
        /// <param name="vertexList">The vertex list of the original polyline.</param>
        /// <param name="maxError">The maximum error between the simplified and the original polylines.</param>
        /// <param name="isClosed">True if the polyline shall be considered as a closed polyline; otherwise false.</param>
        /// <returns>The vertex list of the simplified polygon.</returns>
        private static List<RCNumVector> SimplifyPolyline(List<RCNumVector> vertexList, RCNumber maxError, bool isClosed)
        {
            /// Find the vertex where we cut the original polyline and continue this recursive algorithm on the two halves.
            RCNumber maxVertexValue = 0;
            RCNumber maxErrorValue = maxError;
            int index = 0;
            if (!isClosed)
            {
                /// In this case we search the maximum distance from the line between the first and last vertices.
                maxErrorValue = (vertexList[vertexList.Count - 1] - vertexList[0]).Length * maxError;
                for (int i = 1; i < vertexList.Count - 1; i++)
                {
                    RCNumber twoTimesTriangleArea =
                        (new Polygon(vertexList[0], vertexList[i], vertexList[vertexList.Count - 1])).DoubleOfSignedArea.Abs();
                    if (twoTimesTriangleArea > maxVertexValue)
                    {
                        index = i;
                        maxVertexValue = twoTimesTriangleArea;
                    }
                }
            }
            else
            {
                /// In this case we search the maximum distance from the first vertex.
                for (int i = 1; i < vertexList.Count; i++)
                {
                    RCNumber distance = (vertexList[i] - vertexList[0]).Length;
                    if (distance > maxVertexValue)
                    {
                        index = i;
                        maxVertexValue = distance;
                    }
                }
            }

            /// If the maximum vertex value is greater than the given threshold, recursively simplify.
            if (maxVertexValue > maxErrorValue)
            {
                /// Recursively call this algorithm again on the two section of the original polyline.
                List<RCNumVector> firstHalf = new List<RCNumVector>();
                List<RCNumVector> secondHalf = new List<RCNumVector>();
                for (int i = 0; i <= index; i++) { firstHalf.Add(vertexList[i]); }
                for (int i = index; i < vertexList.Count; i++) { secondHalf.Add(vertexList[i]); }
                if (isClosed) { secondHalf.Add(vertexList[0]); }

                List<RCNumVector> firstHalfResult = Polygon.SimplifyPolyline(firstHalf, maxError, false);
                List<RCNumVector> secondHalfResult = Polygon.SimplifyPolyline(secondHalf, maxError, false);
                
                /// Merge the results.
                List<RCNumVector> simplifiedPolyline = new List<RCNumVector>();
                for (int i = 0; i < firstHalfResult.Count - 1; i++) { simplifiedPolyline.Add(firstHalfResult[i]); }
                for (int i = 0; i < (isClosed ? secondHalfResult.Count - 1 : secondHalfResult.Count); i++) { simplifiedPolyline.Add(secondHalfResult[i]); }
                return simplifiedPolyline;
            }
            else
            {
                /// Otherwise this is the end of recursion.
                return isClosed ? new List<RCNumVector>() { vertexList[0] }
                                : new List<RCNumVector>() { vertexList[0], vertexList[vertexList.Count - 1] };
            }
        }

        /// <summary>
        /// Constructs a 4-bit integer of the given 2x2 square that takes its bits (starting from the LSB) from the walkability informations at the top-left,
        /// top-right, bottom-left and bottom-right corners of the square, respectively. The appropriate bit will be 0 if the corresponding corner is walkable;
        /// otherwise 1.
        /// </summary>
        /// <param name="grid">The grid that contains the walkability informations.</param>
        /// <param name="position">The position of the top-left corner of the 2x2 square.</param>
        /// <returns>The index of the given square.</returns>
        private static int GetIndexAt(IWalkabilityGrid grid, RCIntVector position)
        {
            bool topLeft = !grid[position];
            bool topRight = !grid[position + new RCIntVector(1, 0)];
            bool bottomRight = !grid[position + new RCIntVector(1, 1)];
            bool bottomLeft = !grid[position + new RCIntVector(0, 1)];
            return (topLeft ? 0x08 : 0x00) | (topRight ? 0x04 : 0x00) | (bottomRight ? 0x02 : 0x00) | (bottomLeft ? 0x01 : 0x00);
        }

        #endregion Polygon buildup methods

        /// <summary>
        /// Calculates the orientation of this polygon.
        /// </summary>
        private void CalculateArea()
        {
            this.doubleOfSignedArea = 0;
            for (int i = 0; i < this.vertices.Count; i++)
            {
                RCNumVector v1 = this.vertices[i];
                RCNumVector v2 = this.vertices[(i + 1) % this.vertices.Count];
                this.doubleOfSignedArea += (v1.X - v2.X) * (v1.Y + v2.Y);
            }
            this.isAreaCalculated = true;
        }

        /// <summary>
        /// Calculates the center of this polygon.
        /// </summary>
        private void CalculateCenter()
        {
            this.center = new RCNumVector(0, 0);
            for (int i = 0; i < this.vertices.Count; i++) { this.center += this.vertices[i]; }
            this.center /= this.vertices.Count;

            this.isCenterCalculated = true;
        }

        /// <summary>
        /// Determines the convexity of this polygon.
        /// </summary>
        private void DetermineConvexity()
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
                    if (curveDirection == -1) { this.isConvex = false; return; }
                    curveDirection = +1;
                }
                else if (testTriangle.DoubleOfSignedArea < 0)
                {
                    if (curveDirection == +1) { this.isConvex = false; return; }
                    curveDirection = -1;
                }
            }
            if (curveDirection == 0) { throw new InvalidOperationException("Unable to determine convexity!"); }
            this.isConvex = true;
            this.isConvexityDetermined = true;
        }

        /// <summary>
        /// The vertices of this polygon in order such that the walkable area is at left hand side.
        /// </summary>
        private List<RCNumVector> vertices;

        /// <summary>
        /// The double of the signed area of this polygon.
        /// </summary>
        private RCNumber doubleOfSignedArea;

        /// <summary>
        /// The center of this polygon.
        /// </summary>
        private RCNumVector center;

        /// <summary>
        /// True if this polygon is convex; otherwise false.
        /// </summary>
        private bool isConvex;

        /// <summary>
        /// This flag indicates whether the area of this polygon has already been calculated or not.
        /// </summary>
        private bool isAreaCalculated;

        /// <summary>
        /// This flag indicates whether the center of this polygon has already been calculated or not.
        /// </summary>
        private bool isCenterCalculated;

        /// <summary>
        /// This flag indicates whether the convexity of this polygon has already been determined or not.
        /// </summary>
        private bool isConvexityDetermined;
    }
}
