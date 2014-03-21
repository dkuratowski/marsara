using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Maps.Core
{
    /// <summary>
    /// Represents a navmesh graph on the 2D plane.
    /// </summary>
    class NavMesh : INavMesh
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

            this.walkabilityHash = 0;
            this.walkabilityHashSet = false;
            this.nodes = new HashSet<NavMeshNode>();

            this.helpers = new List<TessellationHelper>();
            WalkabilityQuadTreeNode quadTreeRoot = WalkabilityQuadTreeNode.CreateQuadTree(grid);
            WalkabilityGridArea rootArea = new WalkabilityGridArea(grid, quadTreeRoot.GetLeafNode(new RCIntVector(-1, -1)), maxError);

            List<List<RCNumVector>> borders = new List<List<RCNumVector>>();
            NavMesh.CollectBorders(rootArea, ref borders);
            PolygonSimplificationHelper simplificationHelper = new PolygonSimplificationHelper(borders, maxError);
            simplificationHelper.Simplify();

            foreach (WalkabilityGridArea sectorArea in rootArea.Children) { NavMesh.CreateSectors(sectorArea, ref this.helpers); }
            foreach (TessellationHelper sector in this.helpers) { this.nodes.UnionWith(sector.Nodes); }
        }

        /// <summary>
        /// Constructs a navigation mesh from the given node list.
        /// </summary>
        /// <param name="nodePolygons">The list that contains the areas of the nodes.</param>
        public NavMesh(List<RCPolygon> nodePolygons)
        {
            if (nodePolygons == null) { throw new ArgumentNullException(); }

            this.walkabilityHash = 0;
            this.walkabilityHashSet = false;
            this.nodes = new HashSet<NavMeshNode>();
            this.helpers = null;

            /// Create a temporary vertex map that is used for setting the neighbourhood relationships between the nodes.
            Dictionary<RCNumVector, HashSet<NavMeshNode>> vertexMap = new Dictionary<RCNumVector, HashSet<NavMeshNode>>();
            foreach (RCPolygon nodePolygon in nodePolygons)
            {
                NavMeshNode node = new NavMeshNode(nodePolygon);
                for (int vertexIdx = 0; vertexIdx < nodePolygon.VertexCount; vertexIdx++)
                {
                    if (!vertexMap.ContainsKey(nodePolygon[vertexIdx])) { vertexMap.Add(nodePolygon[vertexIdx], new HashSet<NavMeshNode>()); }
                    vertexMap[nodePolygon[vertexIdx]].Add(node);
                }
                this.nodes.Add(node);
            }

            /// Set the neighbourhood relationships between the created nodes using the vertex map.
            foreach (NavMeshNode node in this.nodes)
            {
                for (int edgeIdx = 0; edgeIdx < node.Polygon.VertexCount; edgeIdx++)
                {
                    RCNumVector edgeBegin = node.Polygon[edgeIdx];
                    RCNumVector edgeEnd = node.Polygon[(edgeIdx + 1) % node.Polygon.VertexCount];
                    HashSet<NavMeshNode> matchingNodesCopy = new HashSet<NavMeshNode>(vertexMap[edgeBegin]);
                    matchingNodesCopy.IntersectWith(vertexMap[edgeEnd]);
                    if (matchingNodesCopy.Count == 2)
                    {
                        if (!matchingNodesCopy.Remove(node)) { throw new InvalidOperationException("Invalid edge matching!"); }
                        node.AddNeighbour(matchingNodesCopy.First());
                    }
                    else if (matchingNodesCopy.Count == 1)
                    {
                        if (!matchingNodesCopy.Contains(node)) { throw new InvalidOperationException("Invalid edge matching!"); }
                    }
                    else { throw new InvalidOperationException("Invalid edge matching!"); }
                }
            }
        }

        #region INavMesh methods

        /// <see cref="INavMesh.WalkabilityHash"/>
        public int WalkabilityHash
        {
            get
            {
                if (!this.walkabilityHashSet) { throw new InvalidOperationException("Walkability hash value has not yet been set for this navmesh!"); }
                return this.walkabilityHash;
            }
        }

        /// <see cref="INavMesh.Nodes"/>
        public IEnumerable<INavMeshNode> Nodes { get { return this.nodes; } }

        #endregion INavMesh methods

        /// <summary>
        /// Sets the walkability hash value of this navmesh.
        /// </summary>
        /// <param name="hashValue">The value of the walkability hash.</param>
        internal void SetWalkabilityHash(int hashValue)
        {
            if (this.walkabilityHashSet) { throw new InvalidOperationException("Walkability hash value has already been set for this navmesh!"); }
            this.walkabilityHash = hashValue;
            this.walkabilityHashSet = true;
        }

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
        /// <param name="helperList">The list where we collect the created tessellation helpers.</param>
        private static void CreateSectors(WalkabilityGridArea sectorArea, ref List<TessellationHelper> helperList)
        {
            /// Collect the holes of the current sector.
            List<RCPolygon> holes = new List<RCPolygon>();
            foreach (WalkabilityGridArea wallArea in sectorArea.Children)
            {
                if (wallArea.Border.Count >= 3) { holes.Add(new RCPolygon(wallArea.Border)); }
            }

            /// Create the tessellation helper.
            if (sectorArea.Border.Count >= 3) { helperList.Add(new TessellationHelper(new RCPolygon(sectorArea.Border), holes)); }

            /// Call this method recursively on the contained sector areas.
            foreach (WalkabilityGridArea wallArea in sectorArea.Children)
            {
                foreach (WalkabilityGridArea containedSectorArea in wallArea.Children)
                {
                    NavMesh.CreateSectors(containedSectorArea, ref helperList);
                }
            }
        }

        /// <summary>
        /// The list of the tessellation helpers used to construct this navigation mesh. This field is used only for unittesting.
        /// </summary>
        private List<TessellationHelper> helpers;

        /// <summary>
        /// The list of the nodes of this navigation mesh.
        /// </summary>
        private HashSet<NavMeshNode> nodes;

        /// <summary>
        /// The hash value of the walkability grid that this navmesh is based on.
        /// </summary>
        private int walkabilityHash;

        /// <summary>
        /// This flag indicates whether the walkability hash has been set for this navmesh.
        /// </summary>
        private bool walkabilityHashSet;

        /// <summary>
        /// The maximum size of the walkability grid.
        /// </summary>
        private const int MAX_GRIDSIZE = 1024;
    }
}
