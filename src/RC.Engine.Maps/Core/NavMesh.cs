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
            this.walkabilityHashHasBeenSet = false;
            this.nodes = new RCSet<NavMeshNode>();
            this.gridSize = new RCIntVector(grid.Width, grid.Height);

            this.helpers = new List<TessellationHelper>();
            WalkabilityQuadTreeNode quadTreeRoot = WalkabilityQuadTreeNode.CreateQuadTree(grid);
            WalkabilityGridArea rootArea = new WalkabilityGridArea(grid, quadTreeRoot.GetLeafNode(new RCIntVector(-1, -1)), maxError);

            List<List<RCNumVector>> borders = new List<List<RCNumVector>>();
            NavMesh.CollectBorders(rootArea, ref borders);
            PolygonSimplificationHelper simplificationHelper = new PolygonSimplificationHelper(borders, maxError);
            simplificationHelper.Simplify();

            IEnumerator<int> idSource = this.IDSourceMethod();
            foreach (WalkabilityGridArea sectorArea in rootArea.Children) { NavMesh.CreateSectors(sectorArea, ref this.helpers, idSource); }
            foreach (TessellationHelper sector in this.helpers) { this.nodes.UnionWith(sector.Nodes); }

            /// Calculate the edge informations.
            foreach (NavMeshNode node in this.nodes) { node.CalculateEdgeData(); }
        }

        /// <summary>
        /// Constructs a navigation mesh from the given node list.
        /// </summary>
        /// <param name="nodePolygons">The list that contains the areas of the nodes.</param>
        /// <param name="gridSize">The size of the walkability grid that this navmesh is based on.</param>
        public NavMesh(List<RCPolygon> nodePolygons, RCIntVector gridSize)
        {
            if (nodePolygons == null) { throw new ArgumentNullException("nodePolygons"); }
            if (gridSize == RCIntVector.Undefined) { throw new ArgumentNullException("gridSize"); }
            if (gridSize.X <= 0 || gridSize.Y <= 0) { throw new ArgumentOutOfRangeException("gridSize", "Size of the walkability grid must be greater than 0 in both dimensions!"); }

            this.walkabilityHash = 0;
            this.walkabilityHashHasBeenSet = false;
            this.nodes = new RCSet<NavMeshNode>();
            this.helpers = null;
            this.gridSize = gridSize;

            /// Create a temporary vertex map that is used for setting the neighbourhood relationships between the nodes.
            Dictionary<RCNumVector, RCSet<NavMeshNode>> vertexMap = new Dictionary<RCNumVector, RCSet<NavMeshNode>>();
            IEnumerator<int> idSource = this.IDSourceMethod();
            foreach (RCPolygon nodePolygon in nodePolygons)
            {
                NavMeshNode node = new NavMeshNode(nodePolygon, idSource);
                for (int vertexIdx = 0; vertexIdx < nodePolygon.VertexCount; vertexIdx++)
                {
                    if (!vertexMap.ContainsKey(nodePolygon[vertexIdx])) { vertexMap.Add(nodePolygon[vertexIdx], new RCSet<NavMeshNode>()); }
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
                    RCSet<NavMeshNode> matchingNodesCopy = new RCSet<NavMeshNode>(vertexMap[edgeBegin]);
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

            /// Calculate the edge informations.
            foreach (NavMeshNode node in this.nodes) { node.CalculateEdgeData(); }
        }

        #region INavMesh methods

        /// <see cref="INavMesh.WalkabilityHash"/>
        public int WalkabilityHash
        {
            get
            {
                if (!this.walkabilityHashHasBeenSet) { throw new InvalidOperationException("Walkability hash value has not yet been set for this navmesh!"); }
                return this.walkabilityHash;
            }
        }

        /// <see cref="INavMesh.GridSize"/>
        public RCIntVector GridSize { get { return this.gridSize; } }

        /// <see cref="INavMesh.Nodes"/>
        public IEnumerable<INavMeshNode> Nodes { get { return this.nodes; } }

        #endregion INavMesh methods

        /// <summary>
        /// Sets the walkability hash value of this navmesh.
        /// </summary>
        /// <param name="hashValue">The value of the walkability hash.</param>
        internal void SetWalkabilityHash(int hashValue)
        {
            if (this.walkabilityHashHasBeenSet) { throw new InvalidOperationException("Walkability hash value has already been set for this navmesh!"); }
            this.walkabilityHash = hashValue;
            this.walkabilityHashHasBeenSet = true;
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
        /// <param name="idSource">The enumerator that provides the IDs for the NavMeshNodes.</param>
        private static void CreateSectors(WalkabilityGridArea sectorArea, ref List<TessellationHelper> helperList, IEnumerator<int> idSource)
        {
            /// Collect the holes of the current sector.
            List<RCPolygon> holes = new List<RCPolygon>();
            foreach (WalkabilityGridArea wallArea in sectorArea.Children)
            {
                if (wallArea.Border.Count >= 3) { holes.Add(new RCPolygon(wallArea.Border)); }
            }

            /// Create the tessellation helper.
            if (sectorArea.Border.Count >= 3) { helperList.Add(new TessellationHelper(new RCPolygon(sectorArea.Border), holes, idSource)); }

            /// Call this method recursively on the contained sector areas.
            foreach (WalkabilityGridArea wallArea in sectorArea.Children)
            {
                foreach (WalkabilityGridArea containedSectorArea in wallArea.Children)
                {
                    NavMesh.CreateSectors(containedSectorArea, ref helperList, idSource);
                }
            }
        }

        /// <summary>
        /// Method for providing IDs for the NavMeshNodes inside this NavMesh.
        /// </summary>
        /// <returns>An enumerator that provides the IDs for the NavMeshNodes inside this NavMesh.</returns>
        private IEnumerator<int> IDSourceMethod()
        {
            int i = 0;
            while (true) { yield return i++; }
        }

        /// <summary>
        /// The list of the tessellation helpers used to construct this navigation mesh. This field is used only for unittesting.
        /// </summary>
        private List<TessellationHelper> helpers;

        /// <summary>
        /// The list of the nodes of this navigation mesh.
        /// </summary>
        private RCSet<NavMeshNode> nodes;

        /// <summary>
        /// The size of the walkability grid that this navmesh is based on.
        /// </summary>
        private RCIntVector gridSize;

        /// <summary>
        /// The hash value of the walkability grid that this navmesh is based on.
        /// </summary>
        private int walkabilityHash;

        /// <summary>
        /// This flag indicates whether the walkability hash has been set for this navmesh.
        /// </summary>
        private bool walkabilityHashHasBeenSet;

        /// <summary>
        /// The maximum size of the walkability grid.
        /// </summary>
        private const int MAX_GRIDSIZE = 1024;
    }
}
