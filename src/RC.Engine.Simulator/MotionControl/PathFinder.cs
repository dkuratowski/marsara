using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Maps.PublicInterfaces;
using RC.Common;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// Implementation of the pathfinder component.
    /// </summary>
    [Component("RC.Engine.Simulator.PathFinder")]
    class PathFinder : IPathFinder
    {
        /// <summary>
        /// Constructs a PathFinder object.
        /// </summary>
        public PathFinder()
        {
            this.pathfinderTreeRoot = null;
            this.algorithmQueue = null;
            this.pathCache = null;
            this.maxIterationsPerFrame = 0;
        }

        /// <see cref="IPathFinder.Initialize"/>
        public void Initialize(IMapAccess map, int maxIterationsPerFrame)
        {
            if (map == null) { throw new ArgumentNullException("map"); }
            if (maxIterationsPerFrame <= 0) { throw new ArgumentOutOfRangeException("maxIterationsPerFrame", "Maximum number of iterations per frame must be greater than 0!"); }
            this.maxIterationsPerFrame = maxIterationsPerFrame;

            /// Find the number of subdivision levels.
            int boundingBoxSize = Math.Max(map.CellSize.X, map.CellSize.Y);
            int subdivisionLevels = 1;
            while (boundingBoxSize > (int)Math.Pow(2, subdivisionLevels)) { subdivisionLevels++; }

            /// Create the root node of the pathfinder tree.
            this.pathfinderTreeRoot = new PFTreeNode(subdivisionLevels);

            /// Add the obstacles to the pathfinder tree.
            for (int row = 0; row < this.pathfinderTreeRoot.AreaOnMap.Height; row++)
            {
                for (int column = 0; column < this.pathfinderTreeRoot.AreaOnMap.Width; column++)
                {
                    if (row >= map.CellSize.Y || column >= map.CellSize.X)
                    {
                        /// Everything out of the map range is considered to be obstacle.
                        this.pathfinderTreeRoot.AddObstacle(new RCIntVector(column, row));
                    }
                    else
                    {
                        /// Add obstacle depending on the "IsWalkable" flag of the cell.
                        if (!map.GetCell(new RCIntVector(column, row)).IsWalkable)
                        {
                            this.pathfinderTreeRoot.AddObstacle(new RCIntVector(column, row));
                        }
                    }
                }
            }

            /// Search the neighbours of the leaf nodes.
            foreach (PFTreeNode leafNode in this.pathfinderTreeRoot.GetAllLeafNodes())
            {
                leafNode.SearchNeighbours();
            }

            this.pathfinderTreeRoot.SetLeafNodeIndices();
            this.algorithmQueue = new Queue<PathFindingAlgorithm>();
            this.pathCache = new PathCache(PATH_CACHE_CAPACITY);
        }

        /// <see cref="IPathFinder.ContinueSearching"/>
        public void ContinueSearching()
        {
            if (this.pathfinderTreeRoot == null) { throw new InvalidOperationException("Pathfinder not initialized!"); }
            if (this.algorithmQueue.Count == 0) { return; }

            int remainingIterations = this.maxIterationsPerFrame;
            do
            {
                PathFindingAlgorithm currentAlgorithm = this.algorithmQueue.Peek();
                remainingIterations -= currentAlgorithm.Continue(remainingIterations);
                if (currentAlgorithm.IsFinished) { this.algorithmQueue.Dequeue(); }
            } while (remainingIterations > 0 && this.algorithmQueue.Count > 0);
        }

        /// <see cref="IPathFinder.StartPathSearching"/>
        public IPath StartPathSearching(RCIntVector fromCoords, RCIntVector toCoords, int iterationLimit)
        {
            if (this.pathfinderTreeRoot == null) { throw new InvalidOperationException("Pathfinder not initialized!"); }
            if (fromCoords == RCIntVector.Undefined) { throw new ArgumentNullException("fromCoords"); }
            if (toCoords == RCIntVector.Undefined) { throw new ArgumentNullException("toCoords"); }
            if (iterationLimit <= 0) { throw new ArgumentOutOfRangeException("iterationLimit", "Iteration limit must be greater than 0!"); }

            /// Determine the source and the target nodes in the pathfinding tree.
            PFTreeNode fromNode = this.pathfinderTreeRoot.GetLeafNode(fromCoords);
            if (!fromNode.IsWalkable) { throw new ArgumentException("The starting cell of the path must be walkable!"); }
            PFTreeNode toNode = this.pathfinderTreeRoot.GetLeafNode(toCoords);

            /// Try to find a cached path between the regions of the source and the target node.
            PathCacheItem cachedAlgorithm = this.pathCache.FindCachedPathFinding(fromNode, toNode);
            if (cachedAlgorithm != null)
            {
                /// Cached path was found -> Create a new Path instance based on the cached path.
                EndpointConnectionAlgorithm srcConnectionAlgorithm = new EndpointConnectionAlgorithm(fromNode, cachedAlgorithm.SourceRegion, cachedAlgorithm.Algorithm, iterationLimit);
                EndpointConnectionAlgorithm tgtConnectionAlgorithm = new EndpointConnectionAlgorithm(toNode, cachedAlgorithm.TargetRegion, cachedAlgorithm.Algorithm, iterationLimit);
                Path retPath = new DerivedPath(srcConnectionAlgorithm, tgtConnectionAlgorithm);
                this.algorithmQueue.Enqueue(srcConnectionAlgorithm);
                this.algorithmQueue.Enqueue(tgtConnectionAlgorithm);
                return retPath;
            }
            else
            {
                /// No cached path was found -> Create a totally new Path instance and save it to the cache.
                DirectPathFindingAlgorithm searchAlgorithm = new DirectPathFindingAlgorithm(fromNode, toNode, iterationLimit);
                Path newPath = new DirectPath(searchAlgorithm);
                this.pathCache.SavePathFinding(searchAlgorithm);
                this.algorithmQueue.Enqueue(searchAlgorithm);
                return newPath;
            }
        }

        /// <see cref="IPathFinder.StartDetourSearching"/>
        public IPath StartDetourSearching(IPath originalPath, int abortedSectionIdx, int iterationLimit)
        {
            if (this.pathfinderTreeRoot == null) { throw new InvalidOperationException("Pathfinder not initialized!"); }
            if (originalPath == null) { throw new ArgumentNullException("originalPath"); }
            if (abortedSectionIdx < 0 || abortedSectionIdx >= originalPath.Length - 1) { throw new ArgumentOutOfRangeException("abortedSectionIdx"); }
            if (iterationLimit <= 0) { throw new ArgumentOutOfRangeException("iterationLimit", "Iteration limit must be greater than 0!"); }

            DirectPathFindingAlgorithm searchAlgorithm = new DirectPathFindingAlgorithm((Path)originalPath, abortedSectionIdx, iterationLimit);
            Path newPath = new DirectPath(searchAlgorithm);
            this.algorithmQueue.Enqueue(searchAlgorithm);
            return newPath;
        }

        /// <see cref="IPathFinder.CheckObstacleIntersection"/>
        public bool CheckObstacleIntersection(RCNumRectangle area)
        {
            if (this.pathfinderTreeRoot == null) { throw new InvalidOperationException("Pathfinder not initialized!"); }
            if (area == RCNumRectangle.Undefined) { throw new ArgumentNullException("area"); }

            int left = area.Left.Round();
            int top = area.Top.Round();
            int right = area.Right.Round();
            int bottom = area.Bottom.Round();
            RCIntRectangle areaCells = new RCIntRectangle(left, top, right - left + 1, bottom - top + 1);
            return this.pathfinderTreeRoot.CheckObstacleIntersection(areaCells);
        }

        /// <see cref="IPathFinder.GetTreeNodes"/>
        public List<RCIntRectangle> GetTreeNodes(RCIntRectangle area)
        {
            if (this.pathfinderTreeRoot == null) { throw new InvalidOperationException("Pathfinder not initialized!"); }
            if (area == RCNumRectangle.Undefined) { throw new ArgumentNullException("area"); }

            List<RCIntRectangle> retList = new List<RCIntRectangle>();
            foreach (PFTreeNode treeNode in this.pathfinderTreeRoot.GetAllLeafNodes(area))
            {
                retList.Add(treeNode.AreaOnMap);
            }
            return retList;
        }

        /// <summary>
        /// Initializes the pathfinder component with the given pathfinder tree root.
        /// </summary>
        /// <param name="pfTreeRoot">The tree root to initialize with.</param>
        /// <param name="maxIterationsPerFrame">The maximum number of search iterations per frame.</param>
        /// <remarks>TODO: this is only for debugging!</remarks>
        internal void Initialize(PFTreeNode pfTreeRoot, int maxIterationsPerFrame)
        {
            if (pfTreeRoot == null) { throw new ArgumentNullException("pfTreeRoot"); }
            if (maxIterationsPerFrame <= 0) { throw new ArgumentOutOfRangeException("maxIterationsPerFrame", "Maximum number of iterations per frame must be greater than 0!"); }
            this.pathfinderTreeRoot = pfTreeRoot;
            this.maxIterationsPerFrame = maxIterationsPerFrame;

            /// Search the neighbours of the leaf nodes.
            foreach (PFTreeNode leafNode in this.pathfinderTreeRoot.GetAllLeafNodes())
            {
                leafNode.SearchNeighbours();
            }

            this.pathfinderTreeRoot.SetLeafNodeIndices();
            this.algorithmQueue = new Queue<PathFindingAlgorithm>();
            this.pathCache = new PathCache(PATH_CACHE_CAPACITY);
        }

        /// <summary>
        /// Gets the leaf node with the given ID.
        /// </summary>
        /// <param name="id">The ID of the leaf node to get.</param>
        /// <returns>The leaf node with the given ID.</returns>
        /// <remarks>TODO: this is only for debugging!</remarks>
        internal PFTreeNode GetLeafNodeByID(int id)
        {
            if (this.leafNodeMap == null)
            {
                this.leafNodeMap = new PFTreeNode[this.pathfinderTreeRoot.LeafCount];
                foreach (PFTreeNode leafNode in this.pathfinderTreeRoot.GetAllLeafNodes())
                {
                    this.leafNodeMap[leafNode.Index] = leafNode;
                }
            }

            return this.leafNodeMap[id];
        }

        /// <summary>
        /// Gets the root node of the pathfinder tree.
        /// </summary>
        /// <remarks>TODO: this is only for debugging!</remarks>
        internal PFTreeNode PathfinderTreeRoot { get { return this.pathfinderTreeRoot; } }

        /// <summary>
        /// Reference to the root of the pathfinder tree.
        /// </summary>
        private PFTreeNode pathfinderTreeRoot;

        /// <summary>
        /// The FIFO list of the pathfinding algorithm that are currently being executed.
        /// </summary>
        private Queue<PathFindingAlgorithm> algorithmQueue;

        /// <summary>
        /// Reference to the path cache.
        /// </summary>
        private PathCache pathCache;

        /// <summary>
        /// The maximum number of search iterations per frame.
        /// </summary>
        private int maxIterationsPerFrame;

        /// <summary>
        /// The list of the leaf nodes mapped by their IDs.
        /// </summary>
        /// <remarks>TODO: this is only for debugging!</remarks>
        private PFTreeNode[] leafNodeMap;

        /// <summary>
        /// The capacity of the path cache.
        /// </summary>
        private const int PATH_CACHE_CAPACITY = 20;
    }
}
