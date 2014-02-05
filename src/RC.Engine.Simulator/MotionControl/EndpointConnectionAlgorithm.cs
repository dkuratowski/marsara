using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// Represents a pathfinding algorithm that connects a given endpoint to a path computed by a given cached algorithm.
    /// </summary>
    class EndpointConnectionAlgorithm : PathFindingAlgorithm
    {
        /// <summary>
        /// Constructs a EndpointConnectionAlgorithm.
        /// </summary>
        /// <param name="endpoint">The endpoint that shall be connected.</param>
        /// <param name="endpointRegion">The region that contains the endpoint.</param>
        /// <param name="cachedAlgorithm">The cached algorithm that produces the path.</param>
        /// <param name="iterationLimit">The maximum number of iterations to execute.</param>
        public EndpointConnectionAlgorithm(PFTreeNode endpoint, Region endpointRegion, DirectPathFindingAlgorithm cachedAlgorithm, int iterationLimit)
            : base(endpoint, 0, iterationLimit)
        {
            if (!endpointRegion.HasNode(endpoint)) { throw new ArgumentException("The given endpoint is not contained by the given endpoint region!"); }
            this.endpointRegion = endpointRegion;
            this.endpointRegion.AddRef();
            this.cachedAlgorithm = cachedAlgorithm;
            this.pathNodeToReach = null;
        }

        /// <summary>
        /// Gets the node to be reached from the endpoint node.
        /// </summary>
        public PFTreeNode PathNodeToReach { get { return this.pathNodeToReach; } }

        /// <summary>
        /// Gets the cached algorithm that produces the path.
        /// </summary>
        public DirectPathFindingAlgorithm CachedAlgorithm { get { return this.cachedAlgorithm; } }

        #region Overrides

        /// <see cref="PathFindingAlgorithm.CheckExitCriteria"/>
        protected override bool CheckExitCriteria(PFTreeNode currentNode)
        {
            /// Exit if reached the appropriate path node.
            return this.pathNodeToReach == currentNode;
        }

        /// <see cref="PathFindingAlgorithm.CheckExitCriteria"/>
        protected override bool CheckNeighbour(PFTreeNode current, PFTreeNode neighbour)
        {
            return neighbour.IsWalkable && (this.endpointRegion.HasNode(neighbour) || this.endpointRegion.HasNode(current));
        }

        /// <see cref="PathFindingAlgorithm.GetEstimation"/>
        protected override int GetEstimation(PFTreeNode node)
        {
            return PathFindingAlgorithm.ComputeDistance(node.Center, this.pathNodeToReach.Center);
        }

        /// <see cref="PathFindingAlgorithm.OnFinished"/>
        protected override void OnFinished()
        {
            this.endpointRegion.Release();
        }

        /// <see cref="PathFindingAlgorithm.OnStarting"/>
        protected override bool OnStarting()
        {
            if (!this.cachedAlgorithm.IsFinished) { throw new InvalidOperationException("The cached pathfinding algorithm must be finished before starting an EndpointConnectionAlgorithm!"); }

            /// Search the node that has to be connected to the endpoint node.
            PathNode currNode = this.cachedAlgorithm.BestNode;
            bool insideEndpointRegion = this.endpointRegion.HasNode(currNode.Node);
            while (currNode.PreviousNode != null)
            {
                if (insideEndpointRegion && !this.endpointRegion.HasNode(currNode.PreviousNode.Node))
                {
                    this.pathNodeToReach = currNode.PreviousNode.Node;
                    break;
                }
                else if (!insideEndpointRegion && this.endpointRegion.HasNode(currNode.PreviousNode.Node))
                {
                    this.pathNodeToReach = currNode.Node;
                    break;
                }
                else
                {
                    currNode = currNode.PreviousNode;
                }
            }

            return this.pathNodeToReach != null;
        }

        #endregion Overrides

        /// <summary>
        /// Reference to the region of this endpoint connection algorithm.
        /// </summary>
        private Region endpointRegion;

        /// <summary>
        /// The cached algorithm that produces the path.
        /// </summary>
        private DirectPathFindingAlgorithm cachedAlgorithm;

        /// <summary>
        /// Reference to the node to be reached from the endpoint node.
        /// </summary>
        private PFTreeNode pathNodeToReach;
    }
}
