using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// Represents a navmesh graph on the 2D plane.
    /// </summary>
    class NavMesh
    {
        /// <summary>
        /// Constructs a navigation mesh over the given walkability grid.
        /// </summary>
        /// <param name="grid">The grid that contains the walkability informations.</param>
        /// <param name="maxError">The maximum error between the edge of the created polygon and the walkability informations.</param>
        public NavMesh(IWalkabilityGrid grid, RCNumber maxError)
        {
            if (grid == null) { throw new ArgumentNullException("grid"); }
            if (grid.Width > MAX_GRIDSIZE || grid.Height > MAX_GRIDSIZE) { throw new ArgumentOutOfRangeException("grid", "Walkability grid size exceeded the limits!"); }
            if (maxError < 0) { throw new ArgumentOutOfRangeException("maxError", "The maximum error shall not be negative!"); }

            this.sectors = new List<Sector>();
            WalkabilityQuadTreeNode quadTreeRoot = WalkabilityQuadTreeNode.CreateQuadTree(grid);
            WalkabilityGridArea rootArea = new WalkabilityGridArea(grid, quadTreeRoot.GetLeafNode(new RCIntVector(-1, -1)), maxError);

            List<List<RCNumVector>> borders = new List<List<RCNumVector>>();
            NavMesh.CollectBorders(rootArea, ref borders);
            PolygonSimplificationHelper simplificationHelper = new PolygonSimplificationHelper(borders, maxError);
            simplificationHelper.Simplify();

            foreach (WalkabilityGridArea sectorArea in rootArea.Children) { NavMesh.CreateSectors(sectorArea, ref this.sectors); }
        }

        /// <summary>
        /// Gets the list of the sectors of this NavMesh.
        /// </summary>
        public IEnumerable<Sector> Sectors { get { return this.sectors; } }

        /// <summary>
        /// Collects all the borders of the given area and of its children recursively.
        /// </summary>
        /// <param name="area">The given area.</param>
        /// <param name="borderList">The list where we collect the borders.</param>
        private static void CollectBorders(WalkabilityGridArea area, ref List<List<RCNumVector>> borderList)
        {
            if (area.Border != null) { borderList.Add(area.Border); }
            foreach (WalkabilityGridArea childArea in area.Children) { NavMesh.CollectBorders(childArea, ref borderList); }
        }

        /// <summary>
        /// Creates the sectors of the given area and of its children recursively.
        /// </summary>
        /// <param name="sectorArea">The area of the sector.</param>
        /// <param name="sectorList">The list where we collect the created sectors.</param>
        private static void CreateSectors(WalkabilityGridArea sectorArea, ref List<Sector> sectorList)
        {
            /// Collect the holes of the currently created sector.
            List<Polygon> holes = new List<Polygon>();
            foreach (WalkabilityGridArea wallArea in sectorArea.Children)
            {
                if (wallArea.Border.Count >= 3) { holes.Add(new Polygon(wallArea.Border)); }
            }

            /// Create the sector.
            if (sectorArea.Border.Count >= 3) { sectorList.Add(new Sector(new Polygon(sectorArea.Border), holes)); }

            /// Call this method recursively on the contained sector areas.
            foreach (WalkabilityGridArea wallArea in sectorArea.Children)
            {
                foreach (WalkabilityGridArea containedSectorArea in wallArea.Children)
                {
                    NavMesh.CreateSectors(containedSectorArea, ref sectorList);
                }
            }
        }

        /// <summary>
        /// The list of the sectors of this navigation mesh.
        /// </summary>
        private List<Sector> sectors;

        /// <summary>
        /// The maximum size of the walkability grid.
        /// </summary>
        private const int MAX_GRIDSIZE = 1024;
    }
}
