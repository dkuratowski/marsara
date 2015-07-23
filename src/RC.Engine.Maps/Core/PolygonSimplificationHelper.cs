using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Maps.Core
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

            /// Create the necessary data structures for the segments.
            this.segmentQueue = new Queue<PolylineSegment>();
            this.segmentQueueSet = new RCSet<PolylineSegment>();
            this.segmentList = new Dictionary<List<RCNumVector>, Dictionary<int, PolylineSegment>>();
            RCNumber minX = 0, maxX = 0, minY = 0, maxY = 0;
            bool firstIteration = true;
            foreach (List<RCNumVector> vertexList in vertexLists)
            {
                for (int i = 0; i < vertexList.Count; i++)
                {
                    if (firstIteration)
                    {
                        minX = maxX = vertexList[i].X;
                        minY = maxY = vertexList[i].Y;
                        firstIteration = false;
                        continue;
                    }

                    if (vertexList[i].X < minX) { minX = vertexList[i].X; }
                    if (vertexList[i].X > maxX) { maxX = vertexList[i].X; }
                    if (vertexList[i].Y < minY) { minY = vertexList[i].Y; }
                    if (vertexList[i].Y > maxY) { maxY = vertexList[i].Y; }
                }
            }
            this.segmentSearchTree = new BspSearchTree<PolylineSegment>(new RCNumRectangle(minX, minY, maxX - minX, maxY - minY), BSP_NODE_CAPACITY, BSP_MIN_NODE_SIZE);

            /// Create the initial segments of the vertex lists.
            foreach (List<RCNumVector> vertexList in vertexLists) { if (vertexList.Count >= 3) { this.CreateInitialSegments(vertexList); } }

            /// Split the polyline segments while necessary.
            while (this.segmentQueue.Count > 0)
            {
                PolylineSegment segmentToSplit = this.segmentQueue.Dequeue();
                this.segmentQueueSet.Remove(segmentToSplit);
                Tuple<PolylineSegment, PolylineSegment> splitSegments = segmentToSplit.Split(maxError);
                if (splitSegments != null)
                {
                    /// The segment has been split -> push the 2 new segments into the queue.
                    this.segmentQueue.Enqueue(splitSegments.Item1);
                    this.segmentQueue.Enqueue(splitSegments.Item2);
                    this.segmentQueueSet.Add(splitSegments.Item1);
                    this.segmentQueueSet.Add(splitSegments.Item2);

                    /// Remove the old segment from the segment list and from the search tree and add the 2 new segments to the list
                    /// and to the search tree.
                    this.segmentList[segmentToSplit.VertexList].Remove(segmentToSplit.BeginIndex);
                    this.segmentSearchTree.DetachContent(segmentToSplit);
                    this.segmentList[segmentToSplit.VertexList].Add(splitSegments.Item1.BeginIndex, splitSegments.Item1);
                    this.segmentList[segmentToSplit.VertexList].Add(splitSegments.Item2.BeginIndex, splitSegments.Item2);
                    this.segmentSearchTree.AttachContent(splitSegments.Item1);
                    this.segmentSearchTree.AttachContent(splitSegments.Item2);

                    /// Register the splitting vertex as a new foreign vertex for the appropriate segments.
                    foreach (PolylineSegment targetSegment in this.segmentSearchTree.GetContents(splitSegments.Item1.VertexList[splitSegments.Item1.EndIndex]))
                    {
                        if (targetSegment != splitSegments.Item1 && targetSegment != splitSegments.Item2)
                        {
                            targetSegment.RegisterForeignVertex(splitSegments.Item1.VertexList[splitSegments.Item1.EndIndex]);
                            if (this.segmentQueueSet.Add(targetSegment)) { this.segmentQueue.Enqueue(targetSegment); }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Simplifies the vertex lists of the polygons to be simplified.
        /// </summary>
        public void Simplify()
        {
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

            /// Add the segments to the queue, to the segment list, and to the segment search tree.
            this.segmentQueue.Enqueue(firstHalf);
            this.segmentQueue.Enqueue(secondHalf);
            this.segmentQueueSet.Add(firstHalf);
            this.segmentQueueSet.Add(secondHalf);
            this.segmentList.Add(vertexList, new Dictionary<int, PolylineSegment>());
            this.segmentList[vertexList].Add(0, firstHalf);
            this.segmentList[vertexList].Add(maxDistanceIdx, secondHalf);
            this.segmentSearchTree.AttachContent(firstHalf);
            this.segmentSearchTree.AttachContent(secondHalf);
        }

        /// <summary>
        /// The FIFO list of the segments to be split.
        /// </summary>
        private Queue<PolylineSegment> segmentQueue;

        /// <summary>
        /// This set contains the segments that are currently in the FIFO list.
        /// </summary>
        private RCSet<PolylineSegment> segmentQueueSet;

        /// <summary>
        /// The list of the collected segments.
        /// </summary>
        private Dictionary<List<RCNumVector>, Dictionary<int, PolylineSegment>> segmentList;

        /// <summary>
        /// The search tree of the collected segments.
        /// </summary>
        private ISearchTree<PolylineSegment> segmentSearchTree;

        /// <summary>
        /// Constants of the PolylineSegment search-tree.
        /// </summary>
        private const int BSP_NODE_CAPACITY = 16;
        private const int BSP_MIN_NODE_SIZE = 10;
    }
}
