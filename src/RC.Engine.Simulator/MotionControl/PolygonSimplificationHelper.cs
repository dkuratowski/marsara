using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// Helper class for simplifying a list of polygons by keeping their topological relationships.
    /// </summary>
    class PolygonSimplificationHelper
    {
        /// <summary>
        /// Constructs a PolygonSimplificationHelper instance.
        /// </summary>
        /// <param name="vertexLists">The vertex lists of the polygons to be simplified.</param>
        /// <param name="maxError">The maximum error between the simplified and the original polygons.</param>
        public PolygonSimplificationHelper(List<List<RCNumVector>> vertexLists, RCNumber maxError)
        {
            if (vertexLists == null) { throw new ArgumentNullException("vertexLists"); }
            if (maxError < 0) { throw new ArgumentOutOfRangeException("maxError"); }

            this.segmentQueue = new Queue<PolylineSegment>();
            this.segmentList = new Dictionary<List<RCNumVector>, Dictionary<int, PolylineSegment>>();
            foreach (List<RCNumVector> vertexList in vertexLists)
            {
                if (vertexList.Count >= 3) { this.CreateInitialSegments(vertexList); }
            }

            /// Split the polyline segments while necessary.
            while (this.segmentQueue.Count > 0)
            {
                PolylineSegment segmentToSplit = this.segmentQueue.Dequeue();
                Tuple<PolylineSegment, PolylineSegment> splitSegments = segmentToSplit.Split(maxError);
                if (splitSegments != null)
                {
                    /// The segment has been split -> push the 2 new segments into the queue.
                    this.segmentQueue.Enqueue(splitSegments.Item1);
                    this.segmentQueue.Enqueue(splitSegments.Item2);

                    /// Remove the old segment from the segment list and add the 2 new segments to the list.
                    this.segmentList[segmentToSplit.VertexList].Remove(segmentToSplit.BeginIndex);
                    this.segmentList[segmentToSplit.VertexList].Add(splitSegments.Item1.BeginIndex, splitSegments.Item1);
                    this.segmentList[segmentToSplit.VertexList].Add(splitSegments.Item2.BeginIndex, splitSegments.Item2);
                }
            }

            /// Simplify the vertex lists based on the calculated polyline segments.
            foreach (List<RCNumVector> vertexList in this.segmentList.Keys) { this.SimplifyVertexList(vertexList); }
        }

        /// <summary>
        /// Simplifies the given vertex list based on the calculated polyline segments.
        /// </summary>
        /// <param name="vertexList">The vertex list to be simplified.</param>
        private void SimplifyVertexList(List<RCNumVector> vertexList)
        {
            /// Collect the vertices that shall be kept.
            int currentVertexIndex = 0;
            List<RCNumVector> collectedVertices = new List<RCNumVector>();
            Dictionary<int, PolylineSegment> segmentation = this.segmentList[vertexList];
            do
            {
                collectedVertices.Add(vertexList[currentVertexIndex]);
                currentVertexIndex = segmentation[currentVertexIndex].EndIndex;
            } while (currentVertexIndex != 0);

            /// Clear the original vertex list and put back the collected vertices.
            vertexList.Clear();
            vertexList.AddRange(collectedVertices);
        }

        /// <summary>
        /// Creates the initial segments for the given vertex list. Two segments will be created: from the first vertex
        /// to the vertex with the maximum distance from it, and back to the first vertex.
        /// </summary>
        /// <param name="vertexList">The vertex list.</param>
        private void CreateInitialSegments(List<RCNumVector> vertexList)
        {
            if (vertexList.Count < 3) { throw new ArgumentException("The vertex list must contain at least 3 vertices!", "vertexList"); }

            /// Search the vertex with the maximum distance from the first vertex.
            RCNumber maxDistance = 0;
            int maxDistanceIdx = 0;
            for (int i = 1; i < vertexList.Count; i++)
            {
                RCNumber distance = (vertexList[i] - vertexList[0]).Length;
                if (distance > maxDistance)
                {
                    maxDistanceIdx = i;
                    maxDistance = distance;
                }
            }

            /// Create the 2 initial polyline segments.
            PolylineSegment firstHalf = new PolylineSegment(vertexList, 0, maxDistanceIdx);
            PolylineSegment secondHalf = new PolylineSegment(vertexList, maxDistanceIdx, 0);

            /// Add the segments to the queue and to the segment list.
            this.segmentQueue.Enqueue(firstHalf);
            this.segmentQueue.Enqueue(secondHalf);
            this.segmentList.Add(vertexList, new Dictionary<int, PolylineSegment>());
            this.segmentList[vertexList].Add(0, firstHalf);
            this.segmentList[vertexList].Add(maxDistanceIdx, secondHalf);
        }

        /// <summary>
        /// The FIFO list of the segments to be split.
        /// </summary>
        private Queue<PolylineSegment> segmentQueue;

        /// <summary>
        /// The list of the collected segments.
        /// </summary>
        private Dictionary<List<RCNumVector>, Dictionary<int, PolylineSegment>> segmentList;
    }
}
