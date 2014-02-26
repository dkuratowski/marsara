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
            Polygon border = Polygon.FromGrid(grid, sectorArea.TopLeftCell, maxError);
            if (border.VertexCount <= 2) { return; }
            List<Polygon> holes = new List<Polygon>();
            foreach (WalkabilityGridArea holeArea in sectorArea.Children)
            {
                if (holeArea.IsWalkable) { throw new ArgumentException("The area of the holes inside the sector area must be non-walkable!"); }
                Polygon holePolygon = Polygon.FromGrid(grid, holeArea.TopLeftCell, maxError);
                if (holePolygon.VertexCount > 2) { holes.Add(holePolygon); }
            }

            /// Perform the tessellation of the sector area.
            TessellationHelper tessellation = new TessellationHelper(border, holes);

            /// Collect the nodes of the created tessellation.
            this.nodes = tessellation.Nodes;
        }

        /// <summary>
        /// Gets the list of the nodes of this Sector or null if no nodes could be created to this Sector.
        /// </summary>
        public IEnumerable<NavMeshNode> Nodes { get { return this.nodes; } }

        /// <summary>
        /// The list of the nodes of this Sector of the navmesh.
        /// </summary>
        private HashSet<NavMeshNode> nodes;
    }
}
