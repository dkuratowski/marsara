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
        /// <param name="grid">The grid that contains the walkability informations.</param>
        /// <param name="sectorArea">The area of the sector on the grid.</param>
        /// <param name="maxError">The maximum error between the edges of the created sector and the walkability informations.</param>
        public Sector(IWalkabilityGrid grid, WalkabilityGridArea sectorArea, RCNumber maxError)
        {
            if (grid == null) { throw new ArgumentNullException("grid"); }
            if (sectorArea == null) { throw new ArgumentNullException("sectorArea"); }
            if (!sectorArea.IsWalkable) { throw new ArgumentException("The area of the sector must be walkable!"); }
            if (maxError < 0) { throw new ArgumentOutOfRangeException("maxError", "The maximum error shall not be negative!"); }

            this.nodes = null;

            /// Create the polygons of the border and the holes.
            this.border = Polygon.FromGrid(grid, sectorArea.TopLeftCell, maxError);
            if (this.border == null) { return; }
            this.holes = new List<Polygon>();
            foreach (WalkabilityGridArea holeArea in sectorArea.Children)
            {
                if (holeArea.IsWalkable) { throw new ArgumentException("The area of the holes inside the sector area must be non-walkable!"); }
                Polygon holePolygon = Polygon.FromGrid(grid, holeArea.TopLeftCell, maxError);
                if (holePolygon != null) { this.holes.Add(holePolygon); }
            }

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
