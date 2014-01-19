using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Maps.PublicInterfaces;
using RC.Common;

namespace RC.Engine.Simulator.Core
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
            this.pathsUnderSearch = null;
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
            this.pathsUnderSearch = new Queue<Path>();
        }

        /// <see cref="IPathFinder.ContinueSearching"/>
        public void ContinueSearching()
        {
            if (this.pathfinderTreeRoot == null) { throw new InvalidOperationException("Pathfinder not initialized!"); }
            if (this.pathsUnderSearch.Count == 0) { return; }

            int remainingIterations = this.maxIterationsPerFrame;
            do
            {
                Path pathToSearch = this.pathsUnderSearch.Peek();
                if (!pathToSearch.IsAborted) { remainingIterations -= pathToSearch.Search(remainingIterations); }
                if (pathToSearch.IsReadyForUse || pathToSearch.IsAborted) { this.pathsUnderSearch.Dequeue(); }
            } while (remainingIterations > 0 && this.pathsUnderSearch.Count > 0);
        }

        /// <see cref="IPathFinder.StartPathSearching"/>
        public IPath StartPathSearching(RCIntVector fromCoords, RCIntVector toCoords, int iterationLimit)
        {
            if (this.pathfinderTreeRoot == null) { throw new InvalidOperationException("Pathfinder not initialized!"); }
            if (fromCoords == RCIntVector.Undefined) { throw new ArgumentNullException("fromCoords"); }
            if (toCoords == RCIntVector.Undefined) { throw new ArgumentNullException("toCoords"); }
            if (iterationLimit <= 0) { throw new ArgumentOutOfRangeException("iterationLimit", "Iteration limit must be greater than 0!"); }

            PFTreeNode fromNode = this.pathfinderTreeRoot.GetLeafNode(fromCoords);
            PFTreeNode toNode = this.pathfinderTreeRoot.GetLeafNode(toCoords);
            Path retPath = new Path(fromNode, toNode, iterationLimit);
            this.pathsUnderSearch.Enqueue(retPath);
            return retPath;
        }

        /// <see cref="IPathFinder.StartAlternativePathSearching"/>
        public IPath StartAlternativePathSearching(IPath originalPath, int abortedSectionIdx, int iterationLimit)
        {
            if (this.pathfinderTreeRoot == null) { throw new InvalidOperationException("Pathfinder not initialized!"); }
            if (originalPath == null) { throw new ArgumentNullException("originalPath"); }
            if (abortedSectionIdx < 0 || abortedSectionIdx >= originalPath.Length - 1) { throw new ArgumentOutOfRangeException("abortedSectionIdx"); }
            if (iterationLimit <= 0) { throw new ArgumentOutOfRangeException("iterationLimit", "Iteration limit must be greater than 0!"); }

            Path retPath = new Path((Path)originalPath, abortedSectionIdx, iterationLimit);
            this.pathsUnderSearch.Enqueue(retPath);
            return retPath;
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
            this.pathsUnderSearch = new Queue<Path>();
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
        /// The FIFO list of the paths that are currently being searched.
        /// </summary>
        private Queue<Path> pathsUnderSearch;

        /// <summary>
        /// The maximum number of search iterations per frame.
        /// </summary>
        private int maxIterationsPerFrame;
    }
}
