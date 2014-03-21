using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RC.Common;
using RC.Engine.Maps.Core;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.UnitTests
{
    /// <summary>
    /// Internal class used for validating navmeshes. WARNING! This validator can only validate generated navmeshes.
    /// Loaded navmeshes cannot be validated using this validator.
    /// </summary>
    class NavmeshValidator
    {
        /// <summary>
        /// Constructs a navmesh validator instance for the given navmesh.
        /// </summary>
        /// <param name="navmesh">The navmesh to be validated.</param>
        /// <param name="grid">The walkability grid that the navmesh is based on.</param>
        public NavmeshValidator(NavMesh navmesh, IWalkabilityGrid grid)
        {
            if (navmesh == null) { throw new ArgumentNullException("navmesh"); }
            if (grid == null) { throw new ArgumentNullException("grid"); }

            this.navmesh = navmesh;
            this.grid = grid;
            this.coverageError = 0.0f;
            this.isValidated = false;
        }

        /// <summary>
        /// Gets the value of the ratio between the number of incorrectly covered cells and the number of all cells.
        /// </summary>
        public float CoverageError
        {
            get
            {
                if (!this.isValidated) { throw new InvalidOperationException("Navmesh has not yet been validated!"); }
                return this.coverageError;
            }
        }

        /// <summary>
        /// Validates the navmesh and throws an exception in case of validation error. If the navmesh has already
        /// been validated then this function has no effect.
        /// </summary>
        public void Validate()
        {
            if (this.isValidated) { return; }

            HashSet<NavMeshNode> allNodes = new HashSet<NavMeshNode>();
            List<TessellationHelper> helperList = this.GetCopyOfHelperList();
            Assert.IsNotNull(helperList, "The navmesh is not a generated but a loaded navmesh!");
            foreach (TessellationHelper helper in helperList)
            {
                CheckTessellationHelper(helper);
                CheckSectorInterconnection(helper);
                CheckNeighbourhood(helper);
                foreach (NavMeshNode node in helper.Nodes) { CheckNeighbourEdgeMatching(node); }

                HashSet<NavMeshNode> commonNodesWithPreviousSectors = new HashSet<NavMeshNode>(helper.Nodes);
                commonNodesWithPreviousSectors.IntersectWith(allNodes);
                Assert.AreEqual(0, commonNodesWithPreviousSectors.Count);
                allNodes.UnionWith(helper.Nodes);
            }

            this.CheckNavmeshCoverage();
            this.isValidated = true;
        }

        #region Private methods

        /// <summary>
        /// Checks the coverage of the navmesh.
        /// </summary>
        private void CheckNavmeshCoverage()
        {
            RCNumRectangle gridBoundary = new RCNumRectangle(-((RCNumber)1 / (RCNumber)2), -((RCNumber)1 / (RCNumber)2), this.grid.Width, this.grid.Height);
            BspSearchTree<NavMeshNode> allNodes = new BspSearchTree<NavMeshNode>(gridBoundary, 16, 10);
            foreach (NavMeshNode node in this.navmesh.Nodes)
            {
                Assert.IsTrue(gridBoundary.Contains(node.BoundingBox));
                allNodes.AttachContent(node);
            }

            int correctCells = 0;
            int incorrectCells = 0;
            for (int row = 0; row < this.grid.Height; row++)
            {
                for (int col = 0; col < this.grid.Width; col++)
                {
                    RCNumVector point = new RCNumVector(col, row);
                    bool insideNode = false;
                    foreach (NavMeshNode node in allNodes.GetContents(point))
                    {
                        if (node.Polygon.Contains(point)) { insideNode = true; break; }
                    }

                    if (insideNode && this.grid[new RCIntVector(col, row)] || !insideNode && !this.grid[new RCIntVector(col, row)])
                    {
                        correctCells++;
                    }
                    else
                    {
                        incorrectCells++;
                    }
                }
            }

            this.coverageError = (float)incorrectCells / (float)(correctCells + incorrectCells);
        }

        /// <summary>
        /// Creates a copy of the tessellation helper list of the navmesh.
        /// </summary>
        /// <returns>
        /// A copy of the tessellation helper list of the navmesh or null if the navmesh has no tessellation helpers.
        /// </returns>
        private List<TessellationHelper> GetCopyOfHelperList()
        {
            PrivateObject navmeshObj = new PrivateObject(this.navmesh);
            List<TessellationHelper> helpers = (List<TessellationHelper>)navmeshObj.GetField("helpers");
            return helpers != null ? new List<TessellationHelper>(helpers) : null;
        }

        #endregion Private methods

        #region Private static helper methods

        /// <summary>
        /// Checks the given tessellation helper.
        /// </summary>
        /// <param name="helper">The tessellation helper to be checked.</param>
        private static void CheckTessellationHelper(TessellationHelper helper)
        {
            Dictionary<RCNumVector, HashSet<NavMeshNode>> vertexMap = GetCopyOfVertexMap(helper);
            foreach (NavMeshNode node in helper.Nodes)
            {
                for (int i = 0; i < node.Polygon.VertexCount; ++i)
                {
                    RCNumVector vertex = node.Polygon[i];
                    Assert.IsTrue(vertexMap[vertex].Remove(node));
                    if (vertexMap[vertex].Count == 0) { vertexMap.Remove(vertex); }
                }
            }
            Assert.IsTrue(vertexMap.Count == 0);
        }

        /// <summary>
        /// Checks whether the sector that corresponds to the given tessellation helper is correctly interconnected.
        /// </summary>
        /// <param name="helper">The tessellation helper to check.</param>
        private static void CheckSectorInterconnection(TessellationHelper helper)
        {
            HashSet<NavMeshNode> nodesOfSector = new HashSet<NavMeshNode>(helper.Nodes);
            CollectReachableNodes(nodesOfSector.First(), ref nodesOfSector);
            Assert.AreEqual(0, nodesOfSector.Count);
        }

        /// <summary>
        /// Checks that if two nodes in the given sector share at least one common edge then they are neighbours.
        /// </summary>
        /// <param name="sector">The sector to check.</param>
        private static void CheckNeighbourhood(TessellationHelper sector)
        {
            Dictionary<RCNumVector, HashSet<NavMeshNode>> vertexMap = GetCopyOfVertexMap(sector);
            foreach (NavMeshNode node in sector.Nodes)
            {
                for (int i = 0; i < node.Polygon.VertexCount; i++)
                {
                    RCNumVector edgeBegin = node.Polygon[i];
                    RCNumVector edgeEnd = node.Polygon[(i + 1) % node.Polygon.VertexCount];
                    HashSet<NavMeshNode> matchingNodesCopy = vertexMap[edgeBegin];
                    matchingNodesCopy.IntersectWith(vertexMap[edgeEnd]);
                    if (matchingNodesCopy.Count == 2)
                    {
                        List<NavMeshNode> matchingNodesList = new List<NavMeshNode>(matchingNodesCopy);
                        Assert.IsTrue(matchingNodesList[0].Neighbours.Contains(matchingNodesList[1]));
                        Assert.IsTrue(matchingNodesList[1].Neighbours.Contains(matchingNodesList[0]));
                    }
                }
            }
        }

        /// <summary>
        /// Checks whether the neighbours of the given have at least one common edge with the given node.
        /// </summary>
        /// <param name="node">The node to be checked.</param>
        private static void CheckNeighbourEdgeMatching(NavMeshNode node)
        {
            /// Collect the edges of the current node.
            HashSet<Tuple<RCNumVector, RCNumVector>> nodeEdges = new HashSet<Tuple<RCNumVector, RCNumVector>>();
            for (int i = 0; i < node.Polygon.VertexCount; i++)
            {
                nodeEdges.Add(new Tuple<RCNumVector, RCNumVector>(node.Polygon[i], node.Polygon[(i + 1) % node.Polygon.VertexCount]));
            }

            /// Check the neighbours of the current node.
            foreach (NavMeshNode neighbour in node.Neighbours)
            {
                /// Collect the edges of the current neighbour.
                HashSet<Tuple<RCNumVector, RCNumVector>> neighbourEdges = new HashSet<Tuple<RCNumVector, RCNumVector>>();
                for (int i = 0; i < neighbour.Polygon.VertexCount; i++)
                {
                    neighbourEdges.Add(new Tuple<RCNumVector, RCNumVector>(neighbour.Polygon[(i + 1) % neighbour.Polygon.VertexCount], neighbour.Polygon[i]));
                }

                /// Check if the neighbour has common edges with the current node.
                neighbourEdges.IntersectWith(nodeEdges);
                Assert.IsTrue(neighbourEdges.Count > 0);

                /// Remove the common edges from the current node.
                nodeEdges.ExceptWith(neighbourEdges);
            }
        }

        /// <summary>
        /// Recursive method for visiting all the reachable nodes starting from the given node.
        /// </summary>
        /// <param name="initialNode">The node to start from.</param>
        /// <param name="unvisitedNodes">List of the unvisited nodes.</param>
        private static void CollectReachableNodes(NavMeshNode initialNode, ref HashSet<NavMeshNode> unvisitedNodes)
        {
            Assert.IsTrue(unvisitedNodes.Remove(initialNode));
            foreach (NavMeshNode neighbour in initialNode.Neighbours)
            {
                bool isBidirectional = false;
                foreach (NavMeshNode neighbourOfNeighbour in neighbour.Neighbours) { if (neighbourOfNeighbour == initialNode) { isBidirectional = true; break; } }
                Assert.IsTrue(isBidirectional);
                if (unvisitedNodes.Contains(neighbour))
                {
                    CollectReachableNodes(neighbour, ref unvisitedNodes);
                }
            }
        }

        /// <summary>
        /// Creates a copy of the vertex-map of the given tessellation helper.
        /// </summary>
        /// <param name="helper">The tessellation helper.</param>
        /// <returns>The created copy of the vertex-map of the given tessellation helper.</returns>
        private static Dictionary<RCNumVector, HashSet<NavMeshNode>> GetCopyOfVertexMap(TessellationHelper helper)
        {
            PrivateObject helperObj = new PrivateObject(helper);
            Dictionary<RCNumVector, HashSet<NavMeshNode>> originalMap =
                (Dictionary<RCNumVector, HashSet<NavMeshNode>>)helperObj.GetField("vertexMap");

            Dictionary<RCNumVector, HashSet<NavMeshNode>> mapCopy = new Dictionary<RCNumVector, HashSet<NavMeshNode>>();
            foreach (KeyValuePair<RCNumVector, HashSet<NavMeshNode>> item in originalMap)
            {
                mapCopy.Add(item.Key, new HashSet<NavMeshNode>(item.Value));
            }
            return mapCopy;
        }

        #endregion Private static helper methods

        /// <summary>
        /// Reference to the navmesh to be validated.
        /// </summary>
        private NavMesh navmesh;

        /// <summary>
        /// The walkability grid that the navmesh is based on.
        /// </summary>
        private IWalkabilityGrid grid;

        /// <summary>
        /// This flag indicates whether the navmesh has already been validated or not.
        /// </summary>
        private bool isValidated;

        /// <summary>
        /// The value of the ratio between the number of incorrectly covered cells and the number of all cells.
        /// </summary>
        private float coverageError;
    }
}
