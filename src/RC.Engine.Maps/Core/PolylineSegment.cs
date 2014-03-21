using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Maps.Core
{
    /// <summary>
    /// Represents a segment of a polyline.
    /// </summary>
    class PolylineSegment : ISearchTreeContent
    {
        /// <summary>
        /// Constructs a PolylineSegment instance.
        /// </summary>
        /// <param name="vertexList">The list of the vertices of the original polyline.</param>
        /// <param name="beginIdx">The index of the vertex at the beginning of this segment.</param>
        /// <param name="endIdx">The index of the vertex at the end of this segment.</param>
        public PolylineSegment(List<RCNumVector> vertexList, int beginIdx, int endIdx)
        {
            this.maxDistanceCriteriaSatisfied = false;
            this.vertexList = vertexList;
            this.beginIndex = beginIdx;
            this.endIndex = endIdx;

            List<RCNumVector> segmentVertices = new List<RCNumVector>();
            for (int i = this.beginIndex; i != this.endIndex; i = (i + 1) % this.vertexList.Count) { segmentVertices.Add(this.vertexList[i]); }
            segmentVertices.Add(this.vertexList[this.endIndex]);
            this.segmentPolygon = segmentVertices.Count >= 3 ? new RCPolygon(segmentVertices) : null;

            this.boundingBoxCache = new CachedValue<RCNumRectangle>(this.CalculateBoundingBox);

            this.foreignVertices = new Dictionary<RCNumVector, bool>();
        }

        #region ISearchTreeNode members

        /// <see cref="ISearchTreeContent.BoundingBox"/>
        public RCNumRectangle BoundingBox { get { return this.boundingBoxCache.Value; } }

        /// <see cref="ISearchTreeContent.BoundingBoxChanging"/>
        public event ContentBoundingBoxChangeHdl BoundingBoxChanging;

        /// <see cref="ISearchTreeContent.BoundingBoxChanged"/>
        public event ContentBoundingBoxChangeHdl BoundingBoxChanged;

        #endregion ISearchTreeNode members

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
        /// Registers a new foreign vertex at this segment.
        /// </summary>
        /// <param name="foreignVertex">The new foreign vertex to be registered.</param>
        public void RegisterForeignVertex(RCNumVector foreignVertex)
        {
            if (foreignVertex == RCNumVector.Undefined) { throw new ArgumentNullException("foreignVertex"); }
            if (this.foreignVertices.ContainsKey(foreignVertex)) { throw new InvalidOperationException("Foreign vertex already registered at this segment!"); }

            if (this.maxDistanceCriteriaSatisfied)
            {
                if (this.segmentPolygon != null) { this.foreignVertices[foreignVertex] = !this.segmentPolygon.Contains(foreignVertex); }
                else { this.foreignVertices[foreignVertex] = true; }
            }
            else { this.foreignVertices[foreignVertex] = false; }
        }

        /// <summary>
        /// Splits this polyline segment if necessary.
        /// </summary>
        /// <param name="maxError">The maximum distance between the segment and its vertices.</param>
        /// <returns>
        /// The created segments if this segment has been split, or null if this segment was not needed to split.
        /// </returns>
        public Tuple<PolylineSegment, PolylineSegment> Split(RCNumber maxError)
        {
            /// Check if the segment can even be split.
            if ((this.beginIndex + 1) % this.vertexList.Count == this.endIndex) { return null; }

            /// Search the vertex with the maximum distance from the line between the first and last vertices.
            RCNumber maxDoubleOfTriangleArea = 0;
            int furthestVertexIdx = 0;
            this.FindFurthestVertex(out furthestVertexIdx, out maxDoubleOfTriangleArea);

            if (!this.maxDistanceCriteriaSatisfied)
            {
                /// Calculate the maximum error value.
                RCNumber maxErrorValue = (this.vertexList[this.endIndex] - this.vertexList[this.beginIndex]).Length * maxError;

                /// If the maximum distance is greater than the given threshold, split this polyline.
                if (maxDoubleOfTriangleArea > maxErrorValue)
                {
                    return this.ExecuteSplit(furthestVertexIdx);
                }
                else
                {
                    /// Otherwise we have reached the maximum distance criteria so first we need to calculate the correctness
                    /// of the registered foreign vertices...
                    this.maxDistanceCriteriaSatisfied = true;
                    this.CalculateForeignVerticesCorrectness();

                    /// ...and then continue splitting if the topology is still incorrect.
                    return this.SplitIfTopologyIsIncorrect(furthestVertexIdx);
                }
            }
            else
            {
                return this.SplitIfTopologyIsIncorrect(furthestVertexIdx);
            }
        }

        /// <summary>
        /// Splits this polyline segment at the given vertex if the topology is incorrect.
        /// </summary>
        /// <param name="splittingVertexIdx">The index of the splitting vertex.</param>
        /// <returns>
        /// The created segments if this segment has been split, or null if this segment was not needed to split.
        /// </returns>
        private Tuple<PolylineSegment, PolylineSegment> SplitIfTopologyIsIncorrect(int splittingVertexIdx)
        {
            if (splittingVertexIdx == this.beginIndex || splittingVertexIdx == this.endIndex) { return null; }
            if (this.beginIndex <= this.endIndex && (splittingVertexIdx < this.beginIndex || splittingVertexIdx > this.endIndex) ||
                this.beginIndex > this.endIndex && (splittingVertexIdx > this.endIndex && splittingVertexIdx < this.beginIndex))
            {
                throw new InvalidOperationException("Invalid splitting vertex index!");
            }

            bool isTopologyCorrect = true;
            foreach (bool correctnessFlag in this.foreignVertices.Values) { if (!correctnessFlag) { isTopologyCorrect = false; break; } }

            if (isTopologyCorrect) { return null; } /// Topology is correct -> no need to split.

            Tuple<PolylineSegment, PolylineSegment> splittedSegments = this.ExecuteSplit(splittingVertexIdx);
            splittedSegments.Item1.CalculateForeignVerticesCorrectness();
            splittedSegments.Item2.CalculateForeignVerticesCorrectness();
            return splittedSegments;
        }

        /// <summary>
        /// Executes the splitting operation on this segment.
        /// </summary>
        /// <param name="splittingVertexIdx">The index of the splitting vertex.</param>
        /// <returns>The created segments.</returns>
        private Tuple<PolylineSegment, PolylineSegment> ExecuteSplit(int splittingVertexIdx)
        {
            PolylineSegment firstHalf = new PolylineSegment(this.vertexList, this.beginIndex, splittingVertexIdx);
            PolylineSegment secondHalf = new PolylineSegment(this.vertexList, splittingVertexIdx, this.endIndex);
            foreach (KeyValuePair<RCNumVector, bool> item in this.foreignVertices)
            {
                if (firstHalf.BoundingBox.Contains(item.Key)) { firstHalf.foreignVertices.Add(item.Key, item.Value); }
                if (secondHalf.BoundingBox.Contains(item.Key)) { secondHalf.foreignVertices.Add(item.Key, item.Value); }
            }
            firstHalf.maxDistanceCriteriaSatisfied = this.maxDistanceCriteriaSatisfied;
            secondHalf.maxDistanceCriteriaSatisfied = this.maxDistanceCriteriaSatisfied;
            return new Tuple<PolylineSegment, PolylineSegment>(firstHalf, secondHalf);
        }

        /// <summary>
        /// Calculates the topological correctness of the registered foreign vertices.
        /// </summary>
        private void CalculateForeignVerticesCorrectness()
        {
            List<RCNumVector> foreignVertexList = new List<RCNumVector>(this.foreignVertices.Keys);
            foreach (RCNumVector foreignVertex in foreignVertexList)
            {
                if (this.segmentPolygon != null) { this.foreignVertices[foreignVertex] = !this.segmentPolygon.Contains(foreignVertex); }
                else { this.foreignVertices[foreignVertex] = true; }
            }
        }

        /// <summary>
        /// Finds the vertex of the original polyline that is furthest from this segment and calculates the double
        /// of the area of the triangle between that vertex and the segment endpoints.
        /// </summary>
        /// <param name="furthestVertexIdx">The index of the found vertex.</param>
        /// <param name="maxDoubleOfTriangleArea">
        /// The double of the area of the triangle between the found vertex and the segment endpoints.
        /// </param>
        private void FindFurthestVertex(out int furthestVertexIdx, out RCNumber maxDoubleOfTriangleArea)
        {
            maxDoubleOfTriangleArea = 0;
            furthestVertexIdx = 0;
            for (int i = (this.beginIndex + 1) % this.vertexList.Count; i != this.endIndex; i = (i + 1) % this.vertexList.Count)
            {
                RCNumber doubleOfTriangleArea =
                    (new RCPolygon(this.vertexList[this.beginIndex], this.vertexList[i], this.vertexList[this.endIndex])).DoubleOfSignedArea.Abs();
                if (doubleOfTriangleArea > maxDoubleOfTriangleArea)
                {
                    furthestVertexIdx = i;
                    maxDoubleOfTriangleArea = doubleOfTriangleArea;
                }
            }
        }

        /// <summary>
        /// Calculates the bounding box of this segment.
        /// </summary>
        /// <returns>The bounding box of this segment.</returns>
        private RCNumRectangle CalculateBoundingBox()
        {
            if (this.segmentPolygon != null)
            {
                if (this.segmentPolygon.BoundingBox != RCNumRectangle.Undefined) { return this.segmentPolygon.BoundingBox; }
                else
                {
                    return PolylineSegment.CalculateBoundingBoxForDegeneratedCases(this.segmentPolygon[0], this.segmentPolygon[this.segmentPolygon.VertexCount - 1]);
                }
            }
            else
            {
                if (this.vertexList[this.beginIndex].X != this.vertexList[this.endIndex].X && this.vertexList[this.beginIndex].Y != this.vertexList[this.endIndex].Y)
                {
                    return new RCNumRectangle(
                        this.vertexList[this.beginIndex].X <= this.vertexList[this.endIndex].X ? this.vertexList[this.beginIndex].X : this.vertexList[this.endIndex].X,
                        this.vertexList[this.beginIndex].Y <= this.vertexList[this.endIndex].Y ? this.vertexList[this.beginIndex].Y : this.vertexList[this.endIndex].Y,
                        (this.vertexList[this.beginIndex].X - this.vertexList[this.endIndex].X).Abs() + new RCNumber(1),
                        (this.vertexList[this.beginIndex].Y - this.vertexList[this.endIndex].Y).Abs() + new RCNumber(1));
                }
                else { return PolylineSegment.CalculateBoundingBoxForDegeneratedCases(this.vertexList[this.beginIndex], this.vertexList[this.endIndex]); }
            }
        }

        /// <summary>
        /// Calculates the bounding box of a horizontal or vertical segment.
        /// </summary>
        /// <param name="segmentBegin">The beginning of the segment.</param>
        /// <param name="segmentEnd">The end of the segment.</param>
        /// <returns>The bounding box of the given horizontal or vertical segment.</returns>
        private static RCNumRectangle CalculateBoundingBoxForDegeneratedCases(RCNumVector segmentBegin, RCNumVector segmentEnd)
        {
            if (segmentBegin.X == segmentEnd.X)
            {
                /// Vertical case.
                return new RCNumRectangle((2 * segmentBegin.X - 1) / 2,
                                          segmentBegin.Y <= segmentEnd.Y ? segmentBegin.Y : segmentEnd.Y,
                                          1, (segmentBegin.Y - segmentEnd.Y).Abs() + new RCNumber(1));
            }
            if (segmentBegin.Y == segmentEnd.Y)
            {
                /// Horizontal case.
                return new RCNumRectangle(segmentBegin.X <= segmentEnd.X ? segmentBegin.X : segmentEnd.X,
                                          (2 * segmentBegin.Y - 1) / 2,
                                          (segmentBegin.X - segmentEnd.X).Abs() + new RCNumber(1), 1);
            }
            else { throw new InvalidOperationException("The given segment is neither vertical nor horizontal!"); }
        }

        /// <summary>
        /// The list of the vertices of the original polyline.
        /// </summary>
        private List<RCNumVector> vertexList;

        /// <summary>
        /// The polygon that contains the vertices of the original polyline that belongs to this segment, or null
        /// if this segment has only 2 vertices.
        /// </summary>
        private RCPolygon segmentPolygon;

        /// <summary>
        /// The index of the vertex at the beginning of this segment.
        /// </summary>
        private int beginIndex;

        /// <summary>
        /// The index of the vertex at the end of this segment.
        /// </summary>
        private int endIndex;

        /// <summary>
        /// The list of the foreign vertices and a flag that indicates their topological correctness.
        /// </summary>
        private Dictionary<RCNumVector, bool> foreignVertices;

        /// <summary>
        /// This flag indicates whether this segment already satisfies the maximum distance criteria or not.
        /// </summary>
        private bool maxDistanceCriteriaSatisfied;

        /// <summary>
        /// The cache of the bounding box of this segment.
        /// </summary>
        private CachedValue<RCNumRectangle> boundingBoxCache;
    }
}
