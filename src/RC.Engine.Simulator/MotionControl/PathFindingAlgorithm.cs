using RC.Common;
using RC.Engine.Simulator.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// Abstract base class of the pathfinding algorithms.
    /// </summary>
    abstract class PathFindingAlgorithm
    {
        /// <summary>
        /// Computes the distance between 2 points on the map for the pathfinding algorithms.
        /// </summary>
        /// <param name="fromCoords">The first point on the map.</param>
        /// <param name="toCoords">The second point on the map.</param>
        /// <returns>The computed distance between the given points.</returns>
        public static int ComputeDistance(RCIntVector fromCoords, RCIntVector toCoords)
        {
            int horz = Math.Abs(toCoords.X - fromCoords.X);
            int vert = Math.Abs(toCoords.Y - fromCoords.Y);
            int diff = Math.Abs(horz - vert);
            return (horz < vert ? horz : vert) * 3 + diff * 2;
        }

        /// <summary>
        /// Constructs an instance of a pathfinding algorithm.
        /// </summary>
        /// <param name="fromNode">The starting node of the pathfinding.</param>
        /// <param name="fromNodeEstimation">The estimated length of the path from the source node to the target.</param>
        /// <param name="iterationLimit">The maximum number of iterations to execute when searching this path.</param>
        public PathFindingAlgorithm(PFTreeNode fromNode, int fromNodeEstimation, int iterationLimit)
        {
            this.bestNode = null;
            this.iterationLimit = iterationLimit;
            this.iterationsExecuted = 0;
            this.isFinished = false;
            this.queuedNodes = new BinaryHeap<PathNode>(BinaryHeap<PathNode>.HeapType.MinHeap);
            this.queuedNodesMap = new Dictionary<int, PathNode>();
            this.completedNodesMap = new Dictionary<int, PathNode>();
            this.fromNode = PathNode.CreateSourceNode(fromNode, fromNodeEstimation);
            this.queuedNodes.Insert(this.fromNode);
            this.queuedNodesMap.Add(this.fromNode.Node.Index, this.fromNode);
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

            /// Notify the derived class if this is the first time this algorithm is running.
            if (this.iterationsExecuted == 0)
            {
                if (!this.OnStarting())
                {
                    this.isFinished = true;
                    this.OnFinished();
                    return 0;
                }
            }

            /// Here begins the pathfinding algorithm. It runs while the priority queue is not empty and we haven't reached the maximum number of iterations
            /// and the iteration limit.
            int iterationsInCurrCall = 0;
            while (this.queuedNodes.Count != 0 && (maxIterations == -1 || iterationsInCurrCall < maxIterations) && this.iterationsExecuted < this.iterationLimit)
            {
                /// Get the current node from the priority queue and add it to the completed set.
                PathNode currentPathNode = this.queuedNodes.MaxMinItem;
                this.queuedNodes.DeleteMaxMin();
                this.queuedNodesMap.Remove(currentPathNode.Node.Index);
                this.completedNodesMap.Add(currentPathNode.Node.Index, currentPathNode);

                /// Check if the current node is better than the currently best node.
                if (this.bestNode == null || this.GetEstimation(currentPathNode.Node) < this.GetEstimation(this.bestNode.Node))
                {
                    this.bestNode = currentPathNode;
                }

                /// Process the neighbours of the current node.
                foreach (PFTreeNode neighbour in currentPathNode.Node.Neighbours)
                {
                    /// Process the current neighbour only if it is walkable, is not yet in the completed set, and the edge from the current node
                    /// to that neighbour is not blocked.
                    if (!this.completedNodesMap.ContainsKey(neighbour.Index) && this.CheckNeighbour(currentPathNode.Node, neighbour))
                    {
                        /// Compute an estimated distance from the current neighbour to the target.
                        int neighbourEstimation = this.GetEstimation(neighbour);
                        PathNode neighbourPathNode = null;
                        bool newNode = false;

                        if (this.queuedNodesMap.ContainsKey(neighbour.Index))
                        {
                            /// If the current neighbour is already queued just take it from there.
                            neighbourPathNode = this.queuedNodesMap[neighbour.Index];
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
                            this.queuedNodesMap.Add(neighbour.Index, neighbourPathNode);
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
            if (this.isFinished) { this.OnFinished(); }
            return iterationsInCurrCall;
        }

        #endregion Public members

        #region Overridables

        /// <summary>
        /// Checks whether the given neighbour of the given current node has to be processed or not.
        /// </summary>
        /// <param name="current">The current node.</param>
        /// <param name="neighbour">The neighbour node.</param>
        /// <returns>True if the given neighbour has to be processed; otherwise false.</returns>
        /// <remarks>Can be overriden in the derived classes.</remarks>
        protected virtual bool CheckNeighbour(PFTreeNode current, PFTreeNode neighbour)
        {
            return neighbour.IsWalkable;
        }

        /// <summary>
        /// Gets the estimated distance from the given node to the target of the pathfinding.
        /// </summary>
        /// <param name="node">The node to estimate.</param>
        /// <returns>The estimated distance from the given node to the target of the pathfinding.</returns>
        /// <remarks>Can be overriden in the derived classes.</remarks>
        protected virtual int GetEstimation(PFTreeNode node)
        {
            return 0;
        }

        /// <summary>
        /// Checks whether the target of this pathfinding has been reached or not.
        /// </summary>
        /// <param name="currentNode">The current node.</param>
        /// <returns>True if the pathfinding has to be finished; otherwise false.</returns>
        /// <remarks>Must be overriden in the derived classes.</remarks>
        protected abstract bool CheckExitCriteria(PFTreeNode currentNode);

        /// <summary>
        /// This method is called when this algorithm has been finished.
        /// </summary>
        /// <remarks>Can be overriden in the derived classes.</remarks>
        protected virtual void OnFinished() { }

        /// <summary>
        /// This method is called when this algorithm is being started.
        /// </summary>
        /// <returns>True if the algorithm can start; false otherwise.</returns>
        /// <remarks>
        /// If this method returns false then the algorithm will finish immediately without executing any iteration (the PathFindingAlgorithm.IsFinished
        /// flag will be true). This method can be overriden in the derived classes, the default implementation does nothing.
        /// </remarks>
        protected virtual bool OnStarting() { return true; }

        #endregion Overridables

        /// <summary>
        /// The priority queue of the nodes for the A* algorithm.
        /// </summary>
        private BinaryHeap<PathNode> queuedNodes;

        /// <summary>
        /// The list of the nodes in the priority queue mapped by the IDs of their corresponding tree nodes.
        /// </summary>
        private Dictionary<int, PathNode> queuedNodesMap;

        /// <summary>
        /// The list of the completed nodes mapped by the IDs of their corresponding tree nodes.
        /// </summary>
        private Dictionary<int, PathNode> completedNodesMap;

        /// <summary>
        /// The starting node of the pathfinding.
        /// </summary>
        private PathNode fromNode;

        /// <summary>
        /// Reference to the best node found by the algorithm.
        /// </summary>
        private PathNode bestNode;

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
