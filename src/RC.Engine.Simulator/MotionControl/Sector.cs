using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// Represents a connected part of a navmesh. A Sector might contain walls. A wall is a connected sub-part of the interior area of the sector that doesn't
    /// belong to the sector.
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

            this.border = new Polygon(grid, sectorArea.TopLeftCell, maxError);
            this.walls = new List<Polygon>();
            foreach (WalkabilityGridArea wallArea in sectorArea.Children)
            {
                if (wallArea.IsWalkable) { throw new ArgumentException("The area of the walls inside the sector must be non-walkable!"); }
                Polygon wallPolygon = new Polygon(grid, wallArea.TopLeftCell, maxError);
                if (wallPolygon.Length > 2) { this.walls.Add(wallPolygon); }
            }
        }

        /// <summary>
        /// Gets the border of this Sector.
        /// </summary>
        public Polygon Border { get { return this.border; } }

        /// <summary>
        /// Gets the list of the walls of this Sector.
        /// </summary>
        public IEnumerable<Polygon> Walls { get { return this.walls; } }

        /// <summary>
        /// The Polygon that defines the border of this Sector. A point on the 2D plane is said to be inside this Sector if and only if
        /// it is inside this border but outside of any of the walls of this Sector.
        /// </summary>
        private Polygon border;

        /// <summary>
        /// List of the walls of this Sector.
        /// </summary>
        private List<Polygon> walls;
    }
}
