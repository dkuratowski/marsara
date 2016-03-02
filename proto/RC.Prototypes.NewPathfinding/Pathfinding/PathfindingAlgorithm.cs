using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Prototypes.NewPathfinding.Pathfinding
{
    /// <summary>
    /// Implements the A* pathfinding algorithm.
    /// </summary>
    /// <typeparam name="TNode">The type of the nodes of the graph on which to search.</typeparam>
    class PathfindingAlgorithm<TNode> where TNode : class, INode<TNode>
    {
        /// <summary>
        /// Constructs a pathfinding algorithm that will search a path from the given source node to the given target node.
        /// </summary>
        /// <param name="sourceNode">The source node in the graph.</param>
        /// <param name="targetNode">The target node in the graph.</param>
        /// <param name="objectSize">The size of the object for which to pathfind.</param>
        public PathfindingAlgorithm(TNode sourceNode, TNode targetNode, int objectSize)
        {
            this.targetNode = targetNode;
            this.sourceNode = sourceNode;
            this.objectSize = objectSize;
        }

        /// <summary>
        /// Runs this pathfinding algorithm.
        /// </summary>
        /// <returns>The result of the pathfinding.</returns>
        public PathfindingResult<TNode> Run()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            /// Initialize the pathfinding algorithm.
            this.bestPathNode = null;
            this.openQueue = new PriorityQueue<TNode>();
            this.openSet = new Dictionary<TNode, PathNode<TNode>>();
            this.closedSet = new Dictionary<TNode, PathNode<TNode>>();

            PathNode<TNode> sourcePathNode = new PathNode<TNode>(this.sourceNode, this.sourceNode.Distance(this.targetNode));
            this.openQueue.Insert(sourcePathNode);
            this.openSet.Add(sourceNode, sourcePathNode);

            while (this.openQueue.Count != 0)
            {
                /// Get the current pathnode from the priority queue and add it to the closed set.
                PathNode<TNode> currentPathNode = this.openQueue.TopItem;
                //Console.WriteLine("CurrentNode: {0}", currentPathNode.Node);
                this.openQueue.DeleteTopItem();
                this.openSet.Remove(currentPathNode.Node);
                if (this.closedSet.ContainsKey(currentPathNode.Node))
                {
                    /// The current pathnode is already in the closed set (this could happen if it has been inserted to the priority queue
                    /// more than once).
                    continue;
                }
                this.closedSet.Add(currentPathNode.Node, currentPathNode);

                /// Check if the current pathnode is better than the currently best pathnode.
                if (this.bestPathNode == null || currentPathNode.EstimationToTarget < this.bestPathNode.EstimationToTarget)
                {
                    this.bestPathNode = currentPathNode;
                }

                /// Finish the algorithm if we reached the target.
                if (currentPathNode.Node == this.targetNode) { break; }

                /// Process the successors of the current node.
                foreach (TNode successor in currentPathNode.Node.GetSuccessors(this.objectSize))
                {
                    //Console.WriteLine("Successor: {0}", successor);
                    if (!this.closedSet.ContainsKey(successor))
                    {
                        /// Try to get the pathnode of the successor from the open set.
                        if (!this.openSet.ContainsKey(successor))
                        {
                            /// If not found, calculate the estimated distance from the successor to the target,
                            /// create a new pathnode for it and put it into the open set.
                            int successorEstimation = successor.Distance(this.targetNode);
                            PathNode<TNode> successorPathNode = new PathNode<TNode>(successor, successorEstimation, currentPathNode);
                            this.openQueue.Insert(successorPathNode);
                            this.openSet[successor] = successorPathNode;
                        }
                        else
                        {
                            /// If found, try to set the current node as a new parent.
                            PathNode<TNode> successorPathNode = this.openSet[successor];
                            if (successorPathNode.TrySetNewParent(currentPathNode))
                            {
                                /// If the new parent has been set, we have to insert again the successor into the open queue because
                                /// its priority has been changed.
                                this.openQueue.Insert(successorPathNode);
                            }
                        }
                    }
                }
            }

            watch.Stop();

            return new PathfindingResult<TNode>()
            {
                Path = this.bestPathNode.CalculatePath(),
                ExploredNodes = new List<TNode>(this.closedSet.Keys),
                ElapsedTime = watch.ElapsedMilliseconds
            };
        }

        /// <summary>
        /// The priority queue of the opened pathnodes.
        /// </summary>
        private PriorityQueue<TNode> openQueue;

        /// <summary>
        /// The set of the opened pathnodes mapped by the corresponding graph nodes.
        /// </summary>
        private Dictionary<TNode, PathNode<TNode>> openSet;

        /// <summary>
        /// The set of the closed pathnodes mapped by the corresponding graph nodes.
        /// </summary>
        private Dictionary<TNode, PathNode<TNode>> closedSet;

        /// <summary>
        /// The best pathnode that has been found during the search.
        /// </summary>
        private PathNode<TNode> bestPathNode;

        /// <summary>
        /// The target node.
        /// </summary>
        private TNode targetNode;

        /// <summary>
        /// The source node.
        /// </summary>
        private TNode sourceNode;

        /// <summary>
        /// The size of the object for which to pathfind.
        /// </summary>
        private int objectSize;
    }
}
