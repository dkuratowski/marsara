using RC.Engine.Simulator.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        /// <param name="srcRegion">The source region of the underlying path.</param>
        /// <param name="targetRegion">The target region of the underlying path.</param>
        /// <param name="searchAlgorithm">The underlying search algorithm.</param>
        public PathCacheItem(Region srcRegion, Region targetRegion, DirectPathFindingAlgorithm searchAlgorithm)
        {
            this.sourceRegion = srcRegion;
            this.targetRegion = targetRegion;
            this.algorithm = searchAlgorithm;

            this.valueOfThisItem = MAX_VALUE;
            this.OnKeyChanged(this.valueOfThisItem);
        }

        /// <summary>
        /// This method is called by the PathCache to indicate that this PathCacheItem has been used from the cache.
        /// </summary>
        public void ThisItemUsed()
        {
            this.valueOfThisItem = MAX_VALUE;
            this.OnKeyChanged(this.valueOfThisItem);
        }

        /// <summary>
        /// This method is called by the PathCache to indicate that another PathCacheItem has been used from the cache.
        /// </summary>
        public void AnotherItemUsed()
        {
            this.valueOfThisItem--;
            this.OnKeyChanged(this.valueOfThisItem);
        }

        /// <summary>
        /// Gets the pathfinding algorithm cached by this item.
        /// </summary>
        public DirectPathFindingAlgorithm Algorithm { get { return this.algorithm; } }

        /// <summary>
        /// Gets the source region of the underlying path.
        /// </summary>
        public Region SourceRegion { get { return this.sourceRegion; } }

        /// <summary>
        /// Gets the target region of the underlying path.
        /// </summary>
        public Region TargetRegion { get { return this.targetRegion; } }

        /// <summary>
        /// The source region of the underlying pathfinding algorithm.
        /// </summary>
        private Region sourceRegion;

        /// <summary>
        /// The target region of the underlying pathfinding algorithm.
        /// </summary>
        private Region targetRegion;

        /// <summary>
        /// The underlying pathfinding algorithm.
        /// </summary>
        private DirectPathFindingAlgorithm algorithm;

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
