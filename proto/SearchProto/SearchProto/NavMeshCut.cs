using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchProto
{
    /// <summary>
    /// Enumerates the possible orientations of a NavMeshCut.
    /// </summary>
    enum NavMeshOrientation
    {
        Horizontal = 0,
        Vertical = 1
    }

    /// <summary>
    /// Represents a cut on the NavMesh between two NavMeshRegions.
    /// </summary>
    class NavMeshCut
    {
        /// <summary>
        /// Constructs a NavMeshCut object.
        /// </summary>
        /// <param name="orientation">The orientation of the new cut.</param>
        /// <param name="rowColBefore">
        /// The rowColBefore of the row in case of horizontal cuts or the column in case of vertical cuts after which this NavMeshCut cuts the NavMesh.
        /// </param>
        /// <param name="first">The index of the first node in this NavMeshCut.</param>
        /// <param name="last">The index of the last node in this NavMeshCut.</param>
        public NavMeshCut(NavMeshOrientation orientation, int rowColBefore, int first, int last)
        {
            if (rowColBefore < 0) { throw new ArgumentOutOfRangeException("rowColBefore"); }
            if (first < 0) { throw new ArgumentOutOfRangeException("first"); }
            if (last < 0) { throw new ArgumentOutOfRangeException("last"); }
            if (first > last) { throw new ArgumentException("first cannot be greater than last"); }

            this.orientation = orientation;
            this.rowColBefore = rowColBefore;
            this.first = first;
            this.last = last;
        }

        /// <summary>
        /// The index of the first node in this NavMeshCut.
        /// </summary>
        private int first;

        /// <summary>
        /// The index of the last node in this NavMeshCut.
        /// </summary>
        private int last;

        /// <summary>
        /// The index of the row in case of horizontal cuts or the column in case of vertical cuts after which this NavMeshCut cuts the NavMesh.
        /// </summary>
        private int rowColBefore;

        /// <summary>
        /// Orientation of this NavMeshCut.
        /// </summary>
        private NavMeshOrientation orientation;
    }
}
