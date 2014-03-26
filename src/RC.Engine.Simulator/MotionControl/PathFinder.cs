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
            this.algorithmQueue = null;
            this.pathCache = null;
            this.navmesh = null;
            this.navmeshNodes = null;
            this.maxIterationsPerFrame = 0;
        }

        /// <see cref="IPathFinder.Initialize"/>
        public void Initialize(INavMesh navmesh, int maxIterationsPerFrame)
        {
            if (navmesh == null) { throw new ArgumentNullException("navmesh"); }
            if (maxIterationsPerFrame <= 0) { throw new ArgumentOutOfRangeException("maxIterationsPerFrame", "Maximum number of iterations per frame must be greater than 0!"); }
            this.maxIterationsPerFrame = maxIterationsPerFrame;

            /// Construct the search tree for the navmesh nodes.
            this.navmeshNodes = new BspSearchTree<INavMeshNode>(
                new RCNumRectangle(-(RCNumber)1 / (RCNumber)2,
                                   -(RCNumber)1 / (RCNumber)2,
                                   navmesh.GridSize.X,
                                   navmesh.GridSize.Y),
                BSP_NODE_CAPACITY, BSP_MIN_NODE_SIZE);

            /// Attach the navmesh nodes into the search tree.
            foreach (INavMeshNode navmeshNode in navmesh.Nodes) { this.navmeshNodes.AttachContent(navmeshNode); }

            /// Construct the algorithm-queue and the path-cache.
            this.algorithmQueue = new Queue<PathFindingAlgorithm>();
            this.pathCache = new PathCache(PATH_CACHE_CAPACITY);
            this.navmesh = navmesh;
        }

        /// <see cref="IPathFinder.ContinueSearching"/>
        public void ContinueSearching()
        {
            if (this.navmeshNodes == null) { throw new InvalidOperationException("Pathfinder not initialized!"); }
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
        public IPath StartPathSearching(RCNumVector fromCoords, RCNumVector toCoords, int iterationLimit)
        {
            if (this.navmeshNodes == null) { throw new InvalidOperationException("Pathfinder not initialized!"); }
            if (fromCoords == RCNumVector.Undefined) { throw new ArgumentNullException("fromCoords"); }
            if (toCoords == RCNumVector.Undefined) { throw new ArgumentNullException("toCoords"); }
            if (iterationLimit <= 0) { throw new ArgumentOutOfRangeException("iterationLimit", "Iteration limit must be greater than 0!"); }

            /// Determine the source and the target nodes in the navmesh.
            INavMeshNode fromNode = this.navmeshNodes.GetContents(fromCoords).FirstOrDefault();
            if (fromNode == null) { throw new ArgumentException("The beginning of the path must be walkable!"); }
            INavMeshNode toNode = this.navmeshNodes.GetContents(toCoords).FirstOrDefault();

            /// Try to find a cached path between the regions of the source and the target node.
            PathCacheItem cachedAlgorithm = toNode != null ? this.pathCache.FindCachedPathFinding(fromNode, toNode) : null;
            if (cachedAlgorithm != null)
            {
                /// Cached path was found -> Create a new Path instance based on the cached path.
                return new Path(cachedAlgorithm.Algorithm);
            }
            else
            {
                /// No cached path was found -> Create a totally new Path instance and save it to the cache.
                PathFindingAlgorithm searchAlgorithm = new PathFindingAlgorithm(fromNode, toCoords, iterationLimit);
                Path newPath = new Path(searchAlgorithm);
                if (toNode != null) { this.pathCache.SavePathFinding(searchAlgorithm, toNode); }
                this.algorithmQueue.Enqueue(searchAlgorithm);
                return newPath;
            }
        }

        /// <see cref="IPathFinder.StartDetourSearching"/>
        public IPath StartDetourSearching(IPath originalPath, int abortedSectionIdx, int iterationLimit)
        {
            if (this.navmeshNodes == null) { throw new InvalidOperationException("Pathfinder not initialized!"); }
            if (originalPath == null) { throw new ArgumentNullException("originalPath"); }
            if (abortedSectionIdx < 0 || abortedSectionIdx >= originalPath.Length - 1) { throw new ArgumentOutOfRangeException("abortedSectionIdx"); }
            if (iterationLimit <= 0) { throw new ArgumentOutOfRangeException("iterationLimit", "Iteration limit must be greater than 0!"); }

            PathFindingAlgorithm searchAlgorithm = new PathFindingAlgorithm((Path)originalPath, abortedSectionIdx, iterationLimit);
            Path newPath = new Path(searchAlgorithm);
            this.algorithmQueue.Enqueue(searchAlgorithm);
            return newPath;
        }

        /// <summary>
        /// Gets the navmesh that this pathfinder is initialized with.
        /// </summary>
        /// <remarks>TODO: only for debugging!</remarks>
        internal INavMesh Navmesh { get { return this.navmesh; } }

        /// <summary>
        /// The FIFO list of the pathfinding algorithm that are currently being executed.
        /// </summary>
        private Queue<PathFindingAlgorithm> algorithmQueue;

        /// <summary>
        /// Reference to the path cache.
        /// </summary>
        private PathCache pathCache;

        /// <summary>
        /// The search tree of the navmesh nodes.
        /// </summary>
        private ISearchTree<INavMeshNode> navmeshNodes;

        /// <summary>
        /// Reference to the navmesh that this PathFinder is currently initialized with.
        /// </summary>
        private INavMesh navmesh;

        /// <summary>
        /// The maximum number of search iterations per frame.
        /// </summary>
        private int maxIterationsPerFrame;

        /// <summary>
        /// The capacity of the path cache.
        /// </summary>
        private const int PATH_CACHE_CAPACITY = 20;

        /// <summary>
        /// Constants of the INavMeshNode search-tree.
        /// </summary>
        private const int BSP_NODE_CAPACITY = 16;
        private const int BSP_MIN_NODE_SIZE = 10;
    }
}
