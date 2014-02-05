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
        /// Constructs a Polygon.
        /// </summary>
        /// <param name="grid">The grid that contains the walkability informations.</param>
        /// <param name="upperLeftCorner">An upper-left corner cell in the area that shall be contoured by this Polygon.</param>
        /// <param name="maxError">The maximum error between the edge of the created polygon and the walkability informations.</param>
        /// <remarks>The polygon is created using the "Marching squares" and the "Douglas–Peucker" algorithms.</remarks>
        public Polygon(IWalkabilityGrid grid, RCIntVector upperLeftCorner, RCNumber maxError)
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
                if (Polygon.GetIndexAt(grid, currentPos) == 5) { previousStep = StepDirection.Left; }
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
            this.vertices = Polygon.SimplifyPolyline(initialVertices, maxError, true);
        }

        /// <summary>
        /// Gets the number of vertices of this Polygon.
        /// </summary>
        public int Length { get { return this.vertices.Count; } }

        /// <summary>
        /// Gets the vertex of this Polygon with the given index.
        /// </summary>
        /// <param name="index">The index of the vertex to get.</param>
        /// <returns>The vertex of this Polygon with the given index.</returns>
        public RCNumVector this[int index] { get { return this.vertices[index]; } }

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
            if (currentIndex == 1 || currentIndex == 13)
            {
                RCNumVector newVertex = new RCNumVector(currentPos) + new RCNumVector((RCNumber)1 / (RCNumber)2, (RCNumber)1 / (RCNumber)2);
                if (vertexList.Count > 0 && vertexList[0] == newVertex) { return StepDirection.None; }
                if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex) { vertexList.Add(newVertex); }
                return StepDirection.Down;
            }
            else if (currentIndex == 2 || currentIndex == 11)
            {
                RCNumVector newVertex = new RCNumVector(currentPos) + new RCNumVector((RCNumber)1 / (RCNumber)2, (RCNumber)1 / (RCNumber)2);
                if (vertexList.Count > 0 && vertexList[0] == newVertex) { return StepDirection.None; }
                if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex) { vertexList.Add(newVertex); }
                return StepDirection.Right;
            }
            else if (currentIndex == 3)
            {
                return StepDirection.Right;
            }
            else if (currentIndex == 4 || currentIndex == 7)
            {
                RCNumVector newVertex = new RCNumVector(currentPos) + new RCNumVector((RCNumber)1 / (RCNumber)2, (RCNumber)1 / (RCNumber)2);
                if (vertexList.Count > 0 && vertexList[0] == newVertex) { return StepDirection.None; }
                if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex) { vertexList.Add(newVertex); }
                return StepDirection.Up;
            }
            else if (currentIndex == 5)
            {
                if (previousStep == StepDirection.None || previousStep == StepDirection.Right)
                {
                    RCNumVector newVertex0 = new RCNumVector(currentPos) + new RCNumVector(0, (RCNumber)1 / (RCNumber)2);
                    if (vertexList.Count > 0 && vertexList[0] == newVertex0) { return StepDirection.None; }
                    if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex0) { vertexList.Add(newVertex0); }
                    RCNumVector newVertex1 = new RCNumVector(currentPos) + new RCNumVector((RCNumber)1 / (RCNumber)2, 0);
                    if (vertexList.Count > 0 && vertexList[0] == newVertex1) { return StepDirection.None; }
                    if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex1) { vertexList.Add(newVertex1); }
                    return StepDirection.Up;
                }
                else
                {
                    RCNumVector newVertex0 = new RCNumVector(currentPos) + new RCNumVector(1, (RCNumber)1 / (RCNumber)2);
                    if (vertexList.Count > 0 && vertexList[0] == newVertex0) { return StepDirection.None; }
                    if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex0) { vertexList.Add(newVertex0); }
                    RCNumVector newVertex1 = new RCNumVector(currentPos) + new RCNumVector((RCNumber)1 / (RCNumber)2, 1);
                    if (vertexList.Count > 0 && vertexList[0] == newVertex1) { return StepDirection.None; }
                    if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex1) { vertexList.Add(newVertex1); }
                    return StepDirection.Down;
                }
            }
            else if (currentIndex == 6)
            {
                return StepDirection.Up;
            }
            else if (currentIndex == 8 || currentIndex == 14)
            {
                RCNumVector newVertex = new RCNumVector(currentPos) + new RCNumVector((RCNumber)1 / (RCNumber)2, (RCNumber)1 / (RCNumber)2);
                if (vertexList.Count > 0 && vertexList[0] == newVertex) { return StepDirection.None; }
                if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex) { vertexList.Add(newVertex); }
                return StepDirection.Left;
            }
            else if (currentIndex == 9)
            {
                return StepDirection.Down;
            }
            else if (currentIndex == 10)
            {
                if (previousStep == StepDirection.None || previousStep == StepDirection.Up)
                {
                    RCNumVector newVertex0 = new RCNumVector(currentPos) + new RCNumVector((RCNumber)1 / (RCNumber)2, 1);
                    if (vertexList.Count > 0 && vertexList[0] == newVertex0) { return StepDirection.None; }
                    if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex0) { vertexList.Add(newVertex0); }
                    RCNumVector newVertex1 = new RCNumVector(currentPos) + new RCNumVector(0, (RCNumber)1 / (RCNumber)2);
                    if (vertexList.Count > 0 && vertexList[0] == newVertex1) { return StepDirection.None; }
                    if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex1) { vertexList.Add(newVertex1); }
                    return StepDirection.Left;
                }
                else
                {
                    RCNumVector newVertex0 = new RCNumVector(currentPos) + new RCNumVector((RCNumber)1 / (RCNumber)2, 0);
                    if (vertexList.Count > 0 && vertexList[0] == newVertex0) { return StepDirection.None; }
                    if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex0) { vertexList.Add(newVertex0); }
                    RCNumVector newVertex1 = new RCNumVector(currentPos) + new RCNumVector(1, (RCNumber)1 / (RCNumber)2);
                    if (vertexList.Count > 0 && vertexList[0] == newVertex1) { return StepDirection.None; }
                    if (vertexList.Count == 0 || vertexList.Count > 0 && vertexList[vertexList.Count - 1] != newVertex1) { vertexList.Add(newVertex1); }
                    return StepDirection.Right;
                }
            }
            else if (currentIndex == 12)
            {
                return StepDirection.Left;
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
                    RCNumber twoTimesTriangleArea = CalculateDoubleOfSignedArea(vertexList[0], vertexList[i], vertexList[vertexList.Count - 1]).Abs();
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
        /// Calculates the double of the signed area of the triangle given by its 3 vertices.
        /// </summary>
        /// <param name="p0">The first vertex of the triangle.</param>
        /// <param name="p1">The second vertex of the triangle.</param>
        /// <param name="p2">The third vertex of the triangle.</param>
        /// <returns>The double of the signed area of the given triangle.</returns>
        private static RCNumber CalculateDoubleOfSignedArea(RCNumVector p0, RCNumVector p1, RCNumVector p2)
        {
            return p0.X * (p1.Y - p2.Y) + p1.X * (p2.Y - p0.Y) + p2.X * (p0.Y - p1.Y);
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

        /// <summary>
        /// The vertices of this polygon in order such that the walkable area is at left hand side.
        /// </summary>
        private List<RCNumVector> vertices;
    }
}
