using RC.Engine.Simulator.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// Represents an item in the path cache.
    /// </summary>
    class PathCacheItem : BinaryHeapItem
    {
        /// <summary>
        /// Constructs a PathCacheItem.
        /// </summary>
        /// <param name="srcNode">The source navmesh node of the underlying pathfinding.</param>
        /// <param name="targetNode">The target navmesh node of the underlying pathfinding.</param>
        /// <param name="searchAlgorithm">The underlying search algorithm.</param>
        public PathCacheItem(INavMeshNode srcNode, INavMeshNode targetNode, PathFindingAlgorithm searchAlgorithm)
        {
            this.sourceNode = srcNode;
            this.targetNode = targetNode;
            this.algorithm = searchAlgorithm;

            this.valueOfThisItem = MAX_VALUE;
            this.OnKeyChanged(this.valueOfThisItem);
        }

        /// <summary>
        /// Gets the pathfinding algorithm cached by this item.
        /// </summary>
        public PathFindingAlgorithm Algorithm { get { return this.algorithm; } }

        /// <summary>
        /// Gets the source navmesh node of the underlying path.
        /// </summary>
        public INavMeshNode SourceNode { get { return this.sourceNode; } }

        /// <summary>
        /// Gets the target navmesh node of the underlying path.
        /// </summary>
        public INavMeshNode TargetNode { get { return this.targetNode; } }

        /// <summary>
        /// This method is called by the PathCache to indicate that this PathCacheItem has been used from the cache.
        /// </summary>
        internal void ThisItemUsed()
        {
            this.valueOfThisItem = MAX_VALUE;
            this.OnKeyChanged(this.valueOfThisItem);
        }

        /// <summary>
        /// This method is called by the PathCache to indicate that another PathCacheItem has been used from the cache.
        /// </summary>
        internal void AnotherItemUsed()
        {
            this.valueOfThisItem--;
            this.OnKeyChanged(this.valueOfThisItem);
        }

        /// <summary>
        /// The source navmesh node of the underlying pathfinding algorithm.
        /// </summary>
        private INavMeshNode sourceNode;

        /// <summary>
        /// The target navmesh node of the underlying pathfinding algorithm.
        /// </summary>
        private INavMeshNode targetNode;

        /// <summary>
        /// The underlying pathfinding algorithm.
        /// </summary>
        private PathFindingAlgorithm algorithm;

        /// <summary>
        /// The value of this PathCacheItem.
        /// </summary>
        private int valueOfThisItem;

        /// <summary>
        /// The maximum value of a PathCacheItem.
        /// </summary>
        private const int MAX_VALUE = 250;
    }
}
