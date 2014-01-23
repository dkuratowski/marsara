using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Core
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
            this.pathCache = new Dictionary<Tuple<Region, Region>, PathCacheItem>();
        }

        /// <summary>
        /// Finds a cached pathfinding algorithm between the regions of the given nodes.
        /// </summary>
        /// <param name="fromNode">The source node.</param>
        /// <param name="toNode">The target node.</param>
        /// <returns>
        /// The PathCacheItem that contains the requested pathfinding algorithm or null if there is no cached pathfinding algorithm between the
        /// regions of the given nodes.
        /// </returns>
        public PathCacheItem FindCachedPathFinding(PFTreeNode fromNode, PFTreeNode toNode)
        {
            /// Try to find a cached pathfinding algorithms between the given nodes.
            PathCacheItem returnedItem = null;
            foreach (Region fromRegion in fromNode.Regions)
            {
                foreach (Region toRegion in toNode.Regions)
                {
                    if (this.pathCache.ContainsKey(new Tuple<Region,Region>(fromRegion, toRegion)))
                    {
                        returnedItem = this.pathCache[new Tuple<Region, Region>(fromRegion, toRegion)];
                        break;
                    }
                }
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
        public void SavePathFinding(DirectPathFindingAlgorithm algorithmToSave)
        {
            PathCacheItem similarPath = null;
            foreach (Region fromRegion in algorithmToSave.FromNode.Node.Regions)
            {
                foreach (Region toRegion in algorithmToSave.ToNode.Regions)
                {
                    if (this.pathCache.ContainsKey(new Tuple<Region, Region>(fromRegion, toRegion)))
                    {
                        similarPath = this.pathCache[new Tuple<Region, Region>(fromRegion, toRegion)];
                        break;
                    }
                }
            }

            /// Save the pathfinding algorithm if a similar pathfinding algorithm was not found in the cache.
            if (similarPath == null)
            {
                Region fromRegion = new Region(algorithmToSave.FromNode.Node, REGION_RADIUS);
                if (fromRegion.HasNode(algorithmToSave.ToNode))
                {
                    return;
                }
                else
                {
                    Region toRegion = new Region(algorithmToSave.ToNode, REGION_RADIUS);
                    if (this.pathCacheHeap.Count == this.capacity)
                    {
                        /// We have reached the capacity of the cache -> remove the least valueable item from the cache.
                        PathCacheItem itemToRemove = this.pathCacheHeap.MaxMinItem;
                        this.pathCacheHeap.DeleteMaxMin();
                        this.pathCache.Remove(new Tuple<Region, Region>(itemToRemove.SourceRegion, itemToRemove.TargetRegion));
                        itemToRemove.SourceRegion.Release();
                        itemToRemove.TargetRegion.Release();
                    }

                    /// Add the new item to the cache.
                    PathCacheItem itemToAdd = new PathCacheItem(fromRegion, toRegion, algorithmToSave);
                    this.pathCache.Add(new Tuple<Region, Region>(fromRegion, toRegion), itemToAdd);
                    this.pathCacheHeap.Insert(itemToAdd);
                    itemToAdd.SourceRegion.AddRef();
                    itemToAdd.TargetRegion.AddRef();
                }
            }
        }

        /// <summary>
        /// The heap of the cached paths.
        /// </summary>
        private BinaryHeap<PathCacheItem> pathCacheHeap;

        /// <summary>
        /// The list of the cached paths mapped by their source and target regions.
        /// </summary>
        private Dictionary<Tuple<Region, Region>, PathCacheItem> pathCache;

        /// <summary>
        /// The maximum number of paths stored by this PathCache.
        /// </summary>
        private int capacity;

        /// <summary>
        /// The radius of the regions created by this PathCache.
        /// </summary>
        private const int REGION_RADIUS = 40;
    }
}
