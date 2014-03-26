using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Common.Diagnostics;
using RC.Engine.Maps.ComponentInterfaces;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Maps.Core
{
    /// <summary>
    /// Implementation of the navmesh loader component.
    /// </summary>
    [Component("RC.Engine.Maps.NavMeshLoader")]
    class NavMeshLoader : INavMeshLoader
    {
        #region INavMeshLoader methods

        /// <see cref="INavMeshLoader.NewNavMesh"/>
        public INavMesh NewNavMesh(IWalkabilityGrid walkabilityGrid)
        {
            if (walkabilityGrid == null) { throw new ArgumentNullException("walkabilityGrid"); }

            NavMesh newNavmesh = new NavMesh(walkabilityGrid, MAX_NAVMESH_ERROR);
            newNavmesh.SetWalkabilityHash(BitConverter.ToInt32(CRC32.ComputeHash(this.WalkabilityGridToBytes(walkabilityGrid)), 0));
            return newNavmesh;
        }

        /// <see cref="INavMeshLoader.LoadNavMesh"/>
        public INavMesh LoadNavMesh(byte[] data)
        {
            if (data == null) { throw new ArgumentNullException("data"); }

            /// Load the navmesh package from the byte array.
            RCPackage navmeshPackage = null;
            int offset = 0;
            while (offset < data.Length)
            {
                int parsedBytes;
                RCPackage package = RCPackage.Parse(data, offset, data.Length - offset, out parsedBytes);
                if (package == null || !package.IsCommitted) { throw new MapException("Syntax error!"); }
                offset += parsedBytes;
                if (package.PackageFormat.ID == MapFileFormat.NAVMESH) { navmeshPackage = package; }
            }

            /// Validate the package.
            if (navmeshPackage == null) { return null; }
            int navmeshWalkabilityHash = navmeshPackage.ReadInt(0);
            RCIntVector gridSize = new RCIntVector(navmeshPackage.ReadInt(1), navmeshPackage.ReadInt(2));
            byte[] vertexListPackageBytes = navmeshPackage.ReadByteArray(3);
            byte[] nodeListPackageBytes = navmeshPackage.ReadByteArray(4);

            /// Load the vertex and the node list packages.
            int bytesParsed;
            RCPackage vertexListPackage = RCPackage.Parse(vertexListPackageBytes, 0, vertexListPackageBytes.Length, out bytesParsed);
            if (vertexListPackage == null || !vertexListPackage.IsCommitted || bytesParsed != vertexListPackageBytes.Length) { throw new MapException("Syntax error!"); }
            RCPackage nodeListPackage = RCPackage.Parse(nodeListPackageBytes, 0, nodeListPackageBytes.Length, out bytesParsed);
            if (nodeListPackage == null || !nodeListPackage.IsCommitted || bytesParsed != nodeListPackageBytes.Length) { throw new MapException("Syntax error!"); }

            /// Load the node list and create the NavMesh object.
            List<RCPolygon> nodePolygons = this.LoadNodeList(vertexListPackage, nodeListPackage);
            NavMesh loadedNavmesh = new NavMesh(nodePolygons, gridSize);
            loadedNavmesh.SetWalkabilityHash(navmeshWalkabilityHash);
            return loadedNavmesh;
        }

        /// <see cref="INavMeshLoader.SaveNavMesh"/>
        public byte[] SaveNavMesh(INavMesh navmesh)
        {
            if (navmesh == null) { throw new ArgumentNullException("navmesh"); }

            /// Retrieve the vertex and the node lists from the navmesh.
            List<RCNumVector> vertexList = this.CreateVertexListFromNavmesh(navmesh);
            Dictionary<RCNumVector, int> vertexMap = this.CreateVertexMapFromVertexList(vertexList);
            List<int> nodeList = this.CreateNodeListFromNavmesh(navmesh, vertexMap);

            /// Create the arrays that contain the coordinates of the vertices in the vertex list.
            int[] xCoordBits = new int[vertexList.Count];
            int[] yCoordBits = new int[vertexList.Count];
            for (int i = 0; i < vertexList.Count; i++)
            {
                xCoordBits[i] = vertexList[i].X.Bits;
                yCoordBits[i] = vertexList[i].Y.Bits;
            }

            /// Create the vertex list package.
            RCPackage vertexListPackage = RCPackage.CreateCustomDataPackage(MapFileFormat.NAVMESH_VERTEX_LIST);
            vertexListPackage.WriteIntArray(0, xCoordBits);
            vertexListPackage.WriteIntArray(1, yCoordBits);

            /// Create the node list package.
            RCPackage nodeListPackage = RCPackage.CreateCustomDataPackage(MapFileFormat.NAVMESH_NODE_LIST);
            nodeListPackage.WriteIntArray(0, nodeList.ToArray());

            /// Write the vertex and the node list packages into byte arrays.
            byte[] vertexListPackageBytes = new byte[vertexListPackage.PackageLength];
            byte[] nodeListPackageBytes = new byte[nodeListPackage.PackageLength];
            vertexListPackage.WritePackageToBuffer(vertexListPackageBytes, 0);
            nodeListPackage.WritePackageToBuffer(nodeListPackageBytes, 0);

            /// Create the navmesh package.
            RCPackage navmeshPackage = RCPackage.CreateCustomDataPackage(MapFileFormat.NAVMESH);
            navmeshPackage.WriteInt(0, navmesh.WalkabilityHash);
            navmeshPackage.WriteInt(1, navmesh.GridSize.X);
            navmeshPackage.WriteInt(2, navmesh.GridSize.Y);
            navmeshPackage.WriteByteArray(3, vertexListPackageBytes);
            navmeshPackage.WriteByteArray(4, nodeListPackageBytes);

            /// Return the bytes of the navmesh package.
            byte[] retArray = new byte[navmeshPackage.PackageLength];
            navmeshPackage.WritePackageToBuffer(retArray, 0);
            return retArray;
        }

        /// <see cref="INavMeshLoader.CheckNavmeshIntegrity"/>
        public bool CheckNavmeshIntegrity(IWalkabilityGrid gridToCheck, INavMesh navmeshToCheck)
        {
            if (gridToCheck == null) { throw new ArgumentNullException("gridToCheck"); }
            if (navmeshToCheck == null) { throw new ArgumentNullException("navmeshToCheck"); }

            return navmeshToCheck.WalkabilityHash == BitConverter.ToInt32(CRC32.ComputeHash(this.WalkabilityGridToBytes(gridToCheck)), 0) &&
                   navmeshToCheck.GridSize.X == gridToCheck.Width &&
                   navmeshToCheck.GridSize.Y == gridToCheck.Height;
        }

        #endregion INavMeshLoader methods

        #region Internal helper method for loading

        /// <summary>
        /// Loads the list of the nodes from the given packages.
        /// </summary>
        /// <param name="vertexListPackage">The package that contains the vertex list.</param>
        /// <param name="nodeListPackage">The package that contains the node list.</param>
        /// <returns>The loaded node list.</returns>
        private List<RCPolygon> LoadNodeList(RCPackage vertexListPackage, RCPackage nodeListPackage)
        {
            /// Load the vertex list into a temporary list.
            List<RCNumVector> vertexList = new List<RCNumVector>();
            int[] xCoordBits = vertexListPackage.ReadIntArray(0);
            int[] yCoordBits = vertexListPackage.ReadIntArray(1);
            if (xCoordBits.Length != yCoordBits.Length) { throw new MapException("Syntax error!"); }
            for (int i = 0; i < xCoordBits.Length; i++) { vertexList.Add(new RCNumVector(new RCNumber(xCoordBits[i]), new RCNumber(yCoordBits[i]))); }

            /// Load the node list and index the temporary vertex list to retrieve the node polygons.
            List<RCPolygon> retList = new List<RCPolygon>();
            int[] nodeList = nodeListPackage.ReadIntArray(0);
            List<RCNumVector> currentPolygonVertices = new List<RCNumVector>();
            for (int i = 0; i < nodeList.Length; i++)
            {
                if (nodeList[i] == -1)
                {
                    /// End of the current polygon is indicated by a -1.
                    retList.Add(new RCPolygon(currentPolygonVertices));
                    currentPolygonVertices.Clear();
                }
                else
                {
                    /// Still collecting the polygon vertices.
                    currentPolygonVertices.Add(vertexList[nodeList[i]]);
                }
            }
            return retList;
        }

        #endregion Internal helper method for loading

        #region Internal helper method for saving

        /// <summary>
        /// Gets the list of the vertices in the given navmesh.
        /// </summary>
        /// <param name="navmesh">The navmesh to read.</param>
        /// <returns>The list of the vertices in the given navmesh.</returns>
        private List<RCNumVector> CreateVertexListFromNavmesh(INavMesh navmesh)
        {
            HashSet<RCNumVector> vertices = new HashSet<RCNumVector>();
            List<RCNumVector> vertexList = new List<RCNumVector>();
            foreach (INavMeshNode node in navmesh.Nodes)
            {
                for (int i = 0; i < node.Polygon.VertexCount; i++)
                {
                    if (vertices.Add(node.Polygon[i])) { vertexList.Add(node.Polygon[i]); }
                }
            }
            return vertexList;
        }

        /// <summary>
        /// Creates a dictionary that stores the index of the vertices in the given vertex list.
        /// </summary>
        /// <param name="vertexList">The vertex list.</param>
        /// <returns>The created dictionary.</returns>
        private Dictionary<RCNumVector, int> CreateVertexMapFromVertexList(List<RCNumVector> vertexList)
        {
            Dictionary<RCNumVector, int> retList = new Dictionary<RCNumVector, int>();
            for (int i = 0; i < vertexList.Count; i++) { retList.Add(vertexList[i], i); }
            return retList;
        }

        /// <summary>
        /// Creates the node list from the given navmesh and vertex map.
        /// </summary>
        /// <param name="navmesh">The navmesh.</param>
        /// <param name="vertexMap">The vertex map.</param>
        /// <returns>The created node list.</returns>
        private List<int> CreateNodeListFromNavmesh(INavMesh navmesh, Dictionary<RCNumVector, int> vertexMap)
        {
            List<int> retList = new List<int>();
            foreach (INavMeshNode node in navmesh.Nodes)
            {
                for (int i = 0; i < node.Polygon.VertexCount; i++) { retList.Add(vertexMap[node.Polygon[i]]); }
                retList.Add(-1);
            }
            return retList;
        }

        #endregion Internal helper method for saving

        /// <summary>
        /// Gets the byte sequence whose bits describes the given walkability grid.
        /// </summary>
        /// <param name="grid">The grid to convert to a byte sequence.</param>
        /// <returns>The byte sequence whose bits describes the given walkability grid.</returns>
        private IEnumerable<byte> WalkabilityGridToBytes(IWalkabilityGrid grid)
        {
            byte currentByte = 0x00;
            for (int row = 0; row < grid.Height; row++)
            {
                for (int col = 0; col < grid.Width; col++)
                {
                    if (grid[new RCIntVector(col, row)]) { currentByte |= BITMASK[col % BITS_PER_BYTE]; }
                    if (col % BITS_PER_BYTE == BITS_PER_BYTE - 1)
                    {
                        yield return currentByte;
                        currentByte = 0x00;
                    }
                }
                yield return currentByte;
                currentByte = 0x00;
            }
        }

        /// <summary>
        /// The number of bits in a byte.
        /// </summary>
        private const int BITS_PER_BYTE = 8;

        /// <summary>
        /// The bitmasks that are used when converting a walkability grid into a byte sequence.
        /// </summary>
        private static readonly byte[] BITMASK = new byte[8] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };

        /// <summary>
        /// The maximum value of the error when generating the navmesh of a walkability grid.
        /// </summary>
        private static readonly RCNumber MAX_NAVMESH_ERROR = 2;
    }
}
