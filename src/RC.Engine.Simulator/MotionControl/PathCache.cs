using RC.Engine.Simulator.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// Represents a cache that stores paths.
    /// </summary>
    class PathCache
    {
        /// <summary>
        /// Constructs a PathCache instance with the given capacity.
        /// </summary>
        /// <param name="capacity">The maximum number of pathfinding algorithms stored by this PathCache.</param>
        public PathCache(int capacity)
        {
            if (capacity <= 0) { throw new ArgumentOutOfRangeException("capacity", "The capacity of the path-cache must be greater than 0!"); }
            
            this.capacity = capacity;
            this.pathCacheHeap = new BinaryHeap<PathCacheItem>(BinaryHeap<PathCacheItem>.HeapType.MinHeap);
            this.pathCache = new Dictionary<Tuple<INavMeshNode, INavMeshNode>, PathCacheItem>();
        }

        /// <summary>
        /// Finds a cached pathfinding algorithm between the given navmesh nodes.
        /// </summary>
        /// <param name="fromNode">The source navmesh node.</param>
        /// <param name="toNode">The target navmesh node.</param>
        /// <returns>
        /// The PathCacheItem that contains the requested pathfinding algorithm or null if there is no cached pathfinding algorithm between
        /// the given navmesh nodes.
        /// </returns>
        public PathCacheItem FindCachedPathFinding(INavMeshNode fromNode, INavMeshNode toNode)
        {
            /// Try to find a cached pathfinding algorithms between the given nodes.
            PathCacheItem returnedItem = null;
            if (this.pathCache.ContainsKey(new Tuple<INavMeshNode, INavMeshNode>(fromNode, toNode)))
            {
                returnedItem = this.pathCache[new Tuple<INavMeshNode, INavMeshNode>(fromNode, toNode)];
            }

            /// Update the values of the PathCacheItems if a cached pathfinding algorithm was found.
            if (returnedItem != null)
            {
                returnedItem.ThisItemUsed();
                foreach (PathCacheItem item in this.pathCache.Values)
                {
                    if (item != returnedItem) { item.AnotherItemUsed(); }
                }
            }
            return returnedItem;
        }

        /// <summary>
        /// Saves the given pathfinding algorithm for future use if another similar pathfinding algorithm has not yet been saved.
        /// </summary>
        /// <param name="algorithmToSave">The pathfinding algorithm to be saved.</param>
        /// <param name="toNode">The target node of the pathfinding algorithm.</param>
        public void SavePathFinding(PathFindingAlgorithm algorithmToSave, INavMeshNode toNode)
        {
            PathCacheItem similarPath = null;
            if (this.pathCache.ContainsKey(new Tuple<INavMeshNode, INavMeshNode>(algorithmToSave.FromNode.Node, toNode)))
            {
                similarPath = this.pathCache[new Tuple<INavMeshNode, INavMeshNode>(algorithmToSave.FromNode.Node, toNode)];
            }

            /// Save the pathfinding algorithm if a similar pathfinding algorithm was not found in the cache.
            if (similarPath == null)
            {
                if (this.pathCacheHeap.Count == this.capacity)
                {
                    /// We have reached the capacity of the cache -> remove the least valueable item from the cache.
                    PathCacheItem itemToRemove = this.pathCacheHeap.MaxMinItem;
                    this.pathCacheHeap.DeleteMaxMin();
                    this.pathCache.Remove(new Tuple<INavMeshNode, INavMeshNode>(itemToRemove.SourceNode, itemToRemove.TargetNode));
                }

                /// Add the new item to the cache.
                PathCacheItem itemToAdd = new PathCacheItem(algorithmToSave.FromNode.Node, toNode, algorithmToSave);
                this.pathCache.Add(new Tuple<INavMeshNode, INavMeshNode>(algorithmToSave.FromNode.Node, toNode), itemToAdd);
                this.pathCacheHeap.Insert(itemToAdd);
            }
        }

        /// <summary>
        /// The heap of the cached paths.
        /// </summary>
        private BinaryHeap<PathCacheItem> pathCacheHeap;

        /// <summary>
        /// The list of the cached paths mapped by their source and target regions.
        /// </summary>
        private Dictionary<Tuple<INavMeshNode, INavMeshNode>, PathCacheItem> pathCache;

        /// <summary>
        /// The maximum number of paths stored by this PathCache.
        /// </summary>
        private int capacity;
    }
}
