using RC.Common;
using RC.Engine.Simulator.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// Implements the pathfinding algorithm.
    /// </summary>
    class PathFindingAlgorithm
    {
        /// <summary>
        /// Constructs an instance of a pathfinding algorithm.
        /// </summary>
        /// <param name="fromNode">The starting navmesh node of the pathfinding.</param>
        /// <param name="toCoords">The target coordinates of the pathfinding.</param>
        /// <param name="iterationLimit">The maximum number of iterations to execute when searching this path.</param>
        /// <param name="blockedEdges">The list of blocked edges to avoid.</param>
        public PathFindingAlgorithm(INavMeshNode fromNode, RCNumVector toCoords, int iterationLimit, List<INavMeshEdge> blockedEdges)
        {
            this.bestNode = null;
            this.toCoords = toCoords;
            this.iterationLimit = iterationLimit;
            this.iterationsExecuted = 0;
            this.isFinished = false;
            this.queuedNodes = new BinaryHeap<PathNode>(BinaryHeap<PathNode>.HeapType.MinHeap);
            this.queuedNodesMap = new Dictionary<INavMeshNode, PathNode>();
            this.completedNodesMap = new Dictionary<INavMeshNode, PathNode>();
            this.fromNode = PathNode.CreateSourceNode(fromNode, this.GetEstimation(fromNode));
            this.queuedNodes.Insert(this.fromNode);
            this.queuedNodesMap.Add(this.fromNode.Node, this.fromNode);
            this.blockedEdges = new HashSet<INavMeshEdge>(blockedEdges);
        }

        #region Public members

        /// <summary>
        /// Gets whether this pathfinding algorithm has been finished or not.
        /// </summary>
        public bool IsFinished { get { return this.isFinished; } }

        /// <summary>
        /// Gets the starting node of the pathfinding.
        /// </summary>
        public PathNode FromNode { get { return this.fromNode; } }

        /// <summary>
        /// Gets the target coordinates of the pathfinding.
        /// </summary>
        public RCNumVector ToCoords { get { return this.toCoords; } }

        /// <summary>
        /// Gets the best node found by the algorithm.
        /// </summary>
        public PathNode BestNode
        {
            get
            {
                if (!this.isFinished) { throw new InvalidOperationException("Pathfinding has not yet been finished!"); }
                return this.bestNode;
            }
        }

        /// <summary>
        /// Gets the list of the nodes visited by the algorithm.
        /// </summary>
        public IEnumerable<PathNode> CompletedNodes
        {
            get
            {
                if (!this.isFinished) { throw new InvalidOperationException("Pathfinding has not yet been finished!"); }
                return this.completedNodesMap.Values;
            }
        }

        /// <summary>
        /// Continues the execution of the pathfinding algorithm with no iteration limit.
        /// </summary>
        /// <returns>The number of iterations actually executed.</returns>
        public int Continue()
        {
            return this.Continue(-1);
        }

        /// <summary>
        /// Continues the execution of the pathfinding algorithm.
        /// </summary>
        /// <param name="maxIterations">The maximum number of iterations to execute in this call or -1 to execute the algorithm with no iteration limit.</param>
        /// <returns>The number of iterations actually executed.</returns>
        public int Continue(int maxIterations)
        {
            if (this.isFinished) { throw new InvalidOperationException("Pathfinding has already finished!"); }
            if (maxIterations != -1 && maxIterations <= 0) { throw new ArgumentOutOfRangeException("maxIterations", "Number of iterations must be greater than 0 or must be -1!"); }

            /// Here begins the pathfinding algorithm. It runs while the priority queue is not empty and we haven't reached the maximum number of iterations
            /// and the iteration limit.
            int iterationsInCurrCall = 0;
            while (this.queuedNodes.Count != 0 && (maxIterations == -1 || iterationsInCurrCall < maxIterations) && this.iterationsExecuted < this.iterationLimit)
            {
                /// Get the current node from the priority queue and add it to the completed set.
                PathNode currentPathNode = this.queuedNodes.MaxMinItem;
                this.queuedNodes.DeleteMaxMin();
                this.queuedNodesMap.Remove(currentPathNode.Node);
                this.completedNodesMap.Add(currentPathNode.Node, currentPathNode);

                /// Check if the current node is better than the currently best node.
                if (this.bestNode == null || this.GetEstimation(currentPathNode.Node) < this.GetEstimation(this.bestNode.Node))
                {
                    this.bestNode = currentPathNode;
                }

                /// Process the neighbours of the current node.
                foreach (INavMeshNode neighbour in currentPathNode.Node.Neighbours)
                {
                    /// Process the current neighbour only if it is walkable, is not yet in the completed set, and the edge from the current node
                    /// to that neighbour is not blocked.
                    if (!this.completedNodesMap.ContainsKey(neighbour) && this.CheckNeighbour(currentPathNode.Node, neighbour))
                    {
                        /// Compute an estimated distance from the current neighbour to the target.
                        RCNumber neighbourEstimation = this.GetEstimation(neighbour);
                        PathNode neighbourPathNode = null;
                        bool newNode = false;

                        if (this.queuedNodesMap.ContainsKey(neighbour))
                        {
                            /// If the current neighbour is already queued just take it from there.
                            neighbourPathNode = this.queuedNodesMap[neighbour];
                        }
                        else
                        {
                            /// Otherwise create a new PathNode.
                            neighbourPathNode = new PathNode(neighbour, neighbourEstimation);
                            newNode = true;
                        }

                        /// Approximate the neighbour node if possible.
                        neighbourPathNode.Approximate(currentPathNode);

                        /// If the neighbour has not yet been queued do it now.
                        if (newNode)
                        {
                            this.queuedNodes.Insert(neighbourPathNode);
                            this.queuedNodesMap.Add(neighbour, neighbourPathNode);
                        }
                    }
                } /// End of neighbour processing.

                iterationsInCurrCall++;
                this.iterationsExecuted++;

                /// Check the exit criteria.
                if (this.CheckExitCriteria(currentPathNode.Node))
                {
                    this.isFinished = true;
                    break;
                }

            } /// End of processing the head of the queue.

            /// Finish the algorithm if necessary.
            this.isFinished |= this.queuedNodes.Count == 0 || this.iterationsExecuted >= this.iterationLimit;
            return iterationsInCurrCall;
        }

        #endregion Public members

        #region Internal methods

        /// <summary>
        /// Checks whether the given neighbour of the given current navmesh node has to be processed or not.
        /// </summary>
        /// <param name="current">The current navmesh node.</param>
        /// <param name="neighbour">The neighbour navmesh node.</param>
        /// <returns>True if the given neighbour has to be processed; otherwise false.</returns>
        private bool CheckNeighbour(INavMeshNode current, INavMeshNode neighbour)
        {
            return !this.IsEdgeBlocked(current, neighbour);
        }

        /// <summary>
        /// Gets the estimated distance from the given navmesh node to the target of the pathfinding.
        /// </summary>
        /// <param name="node">The navmesh node to estimate.</param>
        /// <returns>The estimated distance from the given navmesh node to the target of the pathfinding.</returns>
        private RCNumber GetEstimation(INavMeshNode node)
        {
            return MapUtils.ComputeDistance(node.Polygon.Center, this.toCoords);
        }

        /// <summary>
        /// Checks whether the target of this pathfinding has been reached or not.
        /// </summary>
        /// <param name="currentNode">The current navmesh node.</param>
        /// <returns>True if the pathfinding has to be finished; otherwise false.</returns>
        private bool CheckExitCriteria(INavMeshNode currentNode)
        {
            return currentNode.Polygon.Contains(this.toCoords);
        }

        /// <summary>
        /// Checks whether the edge between the two given navmesh nodes is blocked or not.
        /// </summary>
        /// <param name="nodeA">The first navmesh node of the edge.</param>
        /// <param name="nodeB">The second navmesh node of the edge.</param>
        /// <returns>True if the edge is blocked, false otherwise.</returns>
        /// <remarks>Edges are considered to be 1-directional.</remarks>
        private bool IsEdgeBlocked(INavMeshNode nodeA, INavMeshNode nodeB)
        {
            return this.blockedEdges.Contains(nodeA.GetEdge(nodeB));
        }

        #endregion Internal methods

        /// <summary>
        /// The priority queue of the nodes for the A* algorithm.
        /// </summary>
        private BinaryHeap<PathNode> queuedNodes;

        /// <summary>
        /// The list of the nodes in the priority queue mapped by their corresponding navmesh nodes.
        /// </summary>
        private Dictionary<INavMeshNode, PathNode> queuedNodesMap;

        /// <summary>
        /// The list of the completed nodes mapped by their corresponding navmesh nodes.
        /// </summary>
        private Dictionary<INavMeshNode, PathNode> completedNodesMap;

        /// <summary>
        /// The starting node of the pathfinding.
        /// </summary>
        private PathNode fromNode;

        /// <summary>
        /// The target coordinates of the pathfinding.
        /// </summary>
        private RCNumVector toCoords;

        /// <summary>
        /// Reference to the best node found by the algorithm.
        /// </summary>
        private PathNode bestNode;

        /// <summary>
        /// Contains the blocked edges that have to be bypassed by this search algorithm.
        /// </summary>
        private HashSet<INavMeshEdge> blockedEdges;

        /// <summary>
        /// This flag indicates whether this pathfinding algorithm has been finished or not.
        /// </summary>
        private bool isFinished;

        /// <summary>
        /// The maximum number of iterations to execute.
        /// </summary>
        private int iterationLimit;

        /// <summary>
        /// The total number of iterations already executed.
        /// </summary>
        private int iterationsExecuted;
    }
}
