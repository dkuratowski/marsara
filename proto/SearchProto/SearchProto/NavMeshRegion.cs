using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchProto
{
    /// <summary>
    /// Represents a large region of the navmesh that can be used to help the pathfinding.
    /// </summary>
    public class NavMeshRegion
    {
        /// <summary>
        /// Constructs a NavMeshRegion object.
        /// </summary>
        public NavMeshRegion()
        {
            this.cutsOnTop = new List<NavMeshCut>();
            this.cutsOnBottom = new List<NavMeshCut>();
            this.cutsOnLeft = new List<NavMeshCut>();
            this.cutsOnRight = new List<NavMeshCut>();
            this.referenceNode = null;
            this.id = instanceCount;
            instanceCount++;
        }

        /// <summary>
        /// For debugging.
        /// </summary>
        public int ID { get { return this.id; } }

        /// <summary>
        /// For debugging.
        /// </summary>
        public override string ToString()
        {
            return this.id.ToString();
        }

        /// <summary>
        /// Reference node of this NavMeshRegion.
        /// </summary>
        private NavMeshNode referenceNode;

        /// <summary>
        /// List of the cuts on the top of this region in order from left to right.
        /// </summary>
        private List<NavMeshCut> cutsOnTop;

        /// <summary>
        /// List of the cuts on the bottom of this region in order from left to right.
        /// </summary>
        private List<NavMeshCut> cutsOnBottom;

        /// <summary>
        /// List of the cuts on the left side of this region in order from top to bottom.
        /// </summary>
        private List<NavMeshCut> cutsOnLeft;

        /// <summary>
        /// List of the cuts on the right side of this region in order from top to bottom.
        /// </summary>
        private List<NavMeshCut> cutsOnRight;

        /// <summary>
        /// For debugging.
        /// </summary>
        private int id;

        /// <summary>
        /// For debugging.
        /// </summary>
        private static int instanceCount = 0;
    }
}
