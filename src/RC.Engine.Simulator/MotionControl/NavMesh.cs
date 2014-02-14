using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// Represents a navigation mesh over a walkability grid.
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
            WalkabilityGridArea rootArea = new WalkabilityGridArea(quadTreeRoot.GetLeafNode(new RCIntVector(-1, -1)));
            foreach (WalkabilityGridArea sectorArea in rootArea.Children)
            {
                NavMesh.CreateSectors(grid, sectorArea, this.sectors, maxError);
            }
        }

        /// <summary>
        /// Gets the list of the sectors of this NavMesh.
        /// </summary>
        public IEnumerable<Sector> Sectors { get { return this.sectors; } }

        /// <summary>
        /// Calculates the double of the signed area of the triangle given by its 3 vertices.
        /// </summary>
        /// <param name="p0">The first vertex of the triangle.</param>
        /// <param name="p1">The second vertex of the triangle.</param>
        /// <param name="p2">The third vertex of the triangle.</param>
        /// <returns>The double of the signed area of the given triangle.</returns>
        /// <remarks>
        /// If the vertices are given in clockwise order then the area is positive.
        /// If the vertices are given in counter-clockwise order then the are is negative.
        /// If the vertices are colinears then the area is 0.
        /// </remarks>
        public static RCNumber CalculateDoubleOfSignedArea(RCNumVector p0, RCNumVector p1, RCNumVector p2)
        {
            return p0.X * (p1.Y - p2.Y) + p1.X * (p2.Y - p0.Y) + p2.X * (p0.Y - p1.Y);
        }

        /// <summary>
        /// Creates the sectors from the given area-tree node recursively.
        /// </summary>
        /// <param name="grid">The grid that contains the walkability informations.</param>
        /// <param name="sectorArea">The area of the sector.</param>
        /// <param name="sectorList">The list where we collect the created sectors.</param>
        /// <param name="maxError">The maximum error between the edge of the created polygon and the walkability informations.</param>
        private static void CreateSectors(IWalkabilityGrid grid, WalkabilityGridArea sectorArea, List<Sector> sectorList, RCNumber maxError)
        {
            /// Create the new sector.
            Sector newSector = new Sector(grid, sectorArea, maxError);
            if (newSector.Border.VertexCount <= 2) { return; }
            sectorList.Add(newSector);

            /// Call this method recursively on the contained sector areas.
            foreach (WalkabilityGridArea wallArea in sectorArea.Children)
            {
                foreach (WalkabilityGridArea containedSectorArea in wallArea.Children)
                {
                    NavMesh.CreateSectors(grid, containedSectorArea, sectorList, maxError);
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
