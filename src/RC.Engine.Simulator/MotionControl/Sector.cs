using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// Represents a connected part of a navmesh graph.
    /// </summary>
    class Sector
    {
        /// <summary>
        /// Constructs a new Sector instance.
        /// </summary>
        /// <param name="border">The border of the sector.</param>
        /// <param name="holes">The holes inside the sector.</param>
        public Sector(Polygon border, List<Polygon> holes)
        {
            if (border == null) { throw new ArgumentNullException("border"); }
            if (holes == null) { throw new ArgumentNullException("holes"); }

            /// Retrieve the polygons along the border and the holes.
            this.border = border;
            this.holes = new List<Polygon>(holes);

            /// Perform the tessellation of the sector area.
            this.tessellation = new TessellationHelper(this.border, this.holes);

            /// Collect the nodes of the created tessellation.
            this.nodes = this.tessellation.Nodes;
        }

        /// <summary>
        /// Gets the list of the nodes of this Sector or null if no nodes could be created to this Sector.
        /// </summary>
        public IEnumerable<NavMeshNode> Nodes { get { return this.nodes; } }

        /// <summary>
        /// Reference to the created tessellation helper.
        /// </summary>
        /// <remarks>Used in unit testing.</remarks>
        private TessellationHelper tessellation;

        /// <summary>
        /// The list of the nodes of this Sector of the navmesh.
        /// </summary>
        private HashSet<NavMeshNode> nodes;

        /// <summary>
        /// The Polygon that defines the outer border of the sector area.
        /// </summary>
        private Polygon border;

        /// <summary>
        /// List of the Polygons defining the holes inside the sector area.
        /// </summary>
        private List<Polygon> holes;
    }
}
