using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// Represents a segment of a polyline.
    /// </summary>
    class PolylineSegment
    {
        /// <summary>
        /// Constructs a PolylineSegment instance.
        /// </summary>
        /// <param name="vertexList">The list of the vertices of the original polyline.</param>
        /// <param name="beginIdx">The index of the vertex at the beginning of this segment.</param>
        /// <param name="endIdx">The index of the vertex at the end of this segment.</param>
        public PolylineSegment(List<RCNumVector> vertexList, int beginIdx, int endIdx)
        {
            this.vertexList = vertexList;
            this.beginIndex = beginIdx;
            this.endIndex = endIdx;
        }

        /// <summary>
        /// Gets the list of the vertices of the original polyline.
        /// </summary>
        public List<RCNumVector> VertexList { get { return this.vertexList; } }

        /// <summary>
        /// Gets the index of the vertex at the beginning of this segment.
        /// </summary>
        public int BeginIndex { get { return this.beginIndex; } }

        /// <summary>
        /// Gets the index of the vertex at the end of this segment.
        /// </summary>
        public int EndIndex { get { return this.endIndex; } }

        /// <summary>
        /// Splits this polyline segment if necessary.
        /// </summary>
        /// <param name="maxError">The maximum distance between the segment and its vertices.</param>
        /// <returns>
        /// The created segments if this segment has been split, or null if this segment was not needed to split.
        /// </returns>
        public Tuple<PolylineSegment, PolylineSegment> Split(RCNumber maxError)
        {
            /// Search the maximum distance from the line between the first and last vertices.
            RCNumber maxDistance = 0;
            int maxDistanceIdx = 0;
            RCNumber maxErrorValue = (this.vertexList[this.endIndex] - this.vertexList[this.beginIndex]).Length * maxError;
            for (int i = (this.beginIndex + 1) % this.vertexList.Count; i != this.endIndex; i = (i + 1) % this.vertexList.Count)
            {
                RCNumber twoTimesTriangleArea =
                    (new Polygon(this.vertexList[this.beginIndex], this.vertexList[i], this.vertexList[this.endIndex])).DoubleOfSignedArea.Abs();
                if (twoTimesTriangleArea > maxDistance)
                {
                    maxDistanceIdx = i;
                    maxDistance = twoTimesTriangleArea;
                }
            }

            /// If the maximum distance is greater than the given threshold, split this polyline.
            if (maxDistance > maxErrorValue)
            {
                PolylineSegment firstHalf = new PolylineSegment(this.vertexList, this.beginIndex, maxDistanceIdx);
                PolylineSegment secondHalf = new PolylineSegment(this.vertexList, maxDistanceIdx, this.endIndex);
                return new Tuple<PolylineSegment, PolylineSegment>(firstHalf, secondHalf);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// The list of the vertices of the original polyline.
        /// </summary>
        private List<RCNumVector> vertexList;

        /// <summary>
        /// The index of the vertex at the beginning of this segment.
        /// </summary>
        private int beginIndex;

        /// <summary>
        /// The index of the vertex at the end of this segment.
        /// </summary>
        private int endIndex;
    }
}
