using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Represents a path computed by the pathfinder.
    /// </summary>
    class Path : IPath
    {
        /// <summary>
        /// Constructs a new Path instance.
        /// </summary>
        /// <param name="fromNode">The node that contains the starting cell of the path.</param>
        /// <param name="toCoords">The coordinates of the target cell of the path.</param>
        /// <param name="iterationLimit">The maximum number of iterations to execute when searching this path.</param>
        public Path(PFTreeNode fromNode, PFTreeNode toNode, int iterationLimit)
        {
            this.fromNode = fromNode;
            this.toNode = toNode;
            this.lastNodeOnPath = null;
            this.nodesOnPath = null;
            this.isAborted = false;
            this.iterationLimit = iterationLimit;
            this.iterationsExecuted = 0;
            this.blockedEdges = new HashSet<Tuple<int, int>>();

            this.InitSearchAlgorithm();
        }

        /// <summary>
        /// Constructs a new Path instance by computing an alternative path from the given section of the given original path.
        /// </summary>
        /// <param name="originalPath">The original path.</param>
        /// <param name="abortedSectionIdx">The index of the aborted section of the original path.</param>
        /// <param name="iterationLimit">The maximum number of iterations to execute when searching this path.</param>
        public Path(Path originalPath, int abortedSectionIdx, int iterationLimit)
        {
            this.fromNode = originalPath.nodesOnPath[abortedSectionIdx];
            this.toNode = originalPath.toNode;
            this.lastNodeOnPath = null;
            this.nodesOnPath = null;
            this.isAborted = false;
            this.iterationLimit = iterationLimit;
            this.iterationsExecuted = 0;

            this.blockedEdges = new HashSet<Tuple<int, int>>();
            foreach (Tuple<int, int> item in originalPath.blockedEdges)
            {
                this.blockedEdges.Add(item);
            }
            this.blockedEdges.Add(new Tuple<int,int>(originalPath.nodesOnPath[abortedSectionIdx].Index,
                                                     originalPath.nodesOnPath[abortedSectionIdx + 1].Index));

            this.InitSearchAlgorithm();
        }

        #region IPath methods

        /// <see cref="IPath.IsReadyForUse"/>
        public bool IsReadyForUse { get { return this.nodesOnPath != null; } }

        /// <see cref="IPath.IsTargetFound"/>
        public bool IsTargetFound
        {
            get
            {
                if (!this.IsReadyForUse) { throw new InvalidOperationException("Path is not ready for use!"); }
                return this.lastNodeOnPath == this.toNode;
            }
        }

        /// <see cref="IPath.AbortSearch"/>
        public void AbortSearch()
        {
            if (this.IsReadyForUse) { throw new InvalidOperationException("Path is ready for use!"); }
            if (this.isAborted) { throw new InvalidOperationException("Path has already been aborted!"); }
            this.isAborted = true;
        }
        
        /// <see cref="IPath.this[]"/>
        public RCIntRectangle this[int index]
        {
            get
            {
                if (!this.IsReadyForUse) { throw new InvalidOperationException("Path is not ready for use!"); }
                return this.nodesOnPath[index].AreaOnMap;
            }
        }

        /// <see cref="IPath.Length"/>
        public int Length
        {
            get
            {
                if (!this.IsReadyForUse) { throw new InvalidOperationException("Path is not ready for use!"); }
                return this.nodesOnPath.Count;
            }
        }

        /// <see cref="IPath.ForgetBlockedEdges"/>
        public void ForgetBlockedEdges()
        {
            if (!this.IsReadyForUse) { throw new InvalidOperationException("Path is not ready for use!"); }
            this.blockedEdges.Clear();
        }

        #endregion IPath methods

        #region Methods for debugging

        /// <summary>
        /// Gets the list of nodes in the completed list.
        /// </summary>
        /// <remarks>TODO: this is only for debugging!</remarks>
        internal IEnumerable<PFTreeNode> CompletedNodes
        {
            get
            {
                if (!this.IsReadyForUse) { throw new InvalidOperationException("Path is not ready for use!"); }
                List<PFTreeNode> retList = new List<PFTreeNode>();
                for (int i = 0; i < this.completedNodesMap.Length; i++)
                {
                    if (this.completedNodesMap[i] != null) { retList.Add(this.completedNodesMap[i].Node); }
                }
                return retList;
            }
        }

        #endregion Methods for debugging

        #region Internal methods

        /// <summary>
        /// Gets whether searching this path has been aborted or not.
        /// </summary>
        internal bool IsAborted { get { return this.isAborted; } }

        /// <summary>
        /// Implements the A* pathfinding algorithm.
        /// </summary>
        /// <param name="maxIterations">The maximum number of iterations to execute in this call.</param>
        /// <returns>The number of iterations actually executed.</returns>
        internal int Search(int maxIterations)
        {
            if (this.IsReadyForUse) { throw new InvalidOperationException("Path is ready for use!"); }
            if (this.isAborted) { throw new InvalidOperationException("Path has already been aborted!"); }
            if (maxIterations <= 0) { throw new ArgumentOutOfRangeException("maxIterations", "Number of iteration must be greater than 0!"); }

            /// Here begins the A* pathfinding algorithm. It runs while the priority queue is not empty and we haven't reached the maximum number of iterations
            /// and the iteration limit.
            int iterationsInCurrCall = 0;
            bool toNodeReached = false;
            while (this.queuedNodes.Count != 0 && iterationsInCurrCall < maxIterations && this.iterationsExecuted < this.iterationLimit)
            {
                /// Get the current node from the priority queue and add it to the completed set.
                PathNode currentPathNode = this.queuedNodes.MaxMinItem;
                this.queuedNodes.DeleteMaxMin();
                this.queuedNodesMap[currentPathNode.Node.Index] = null;
                this.completedNodesMap[currentPathNode.Node.Index] = currentPathNode;

                /// Save it's reference if its distance to the target node is less than the minimum distance found.
                if (this.lastNodeOnPath == null ||
                    Path.ComputeDistance(currentPathNode.Node.Center, this.toNode.Center) < Path.ComputeDistance(this.lastNodeOnPath.Center, this.toNode.Center))
                {
                    this.lastNodeOnPath = currentPathNode.Node;
                }

                /// Process the neighbours of the current node.
                foreach (PFTreeNode neighbour in currentPathNode.Node.Neighbours)
                {
                    /// Process the current neighbour only if it is walkable, is not yet in the completed set, and the edge from the current node
                    /// to that neighbour is not blocked.
                    if (neighbour.IsWalkable &&
                        this.completedNodesMap[neighbour.Index] == null &&
                        !this.IsEdgeBlocked(currentPathNode.Node, neighbour))
                    {
                        /// Compute an estimated distance from the current neighbour to the target.
                        int neighbourEstimation = Path.ComputeDistance(neighbour.Center, this.toNode.Center);
                        PathNode neighbourPathNode = null;
                        bool newNode = false;

                        if (this.queuedNodesMap[neighbour.Index] != null)
                        {
                            /// If the current neighbour is already queued just take it from there.
                            neighbourPathNode = this.queuedNodesMap[neighbour.Index];
                        }
                        else
                        {
                            /// Otherwise create a new PathNode.
                            neighbourPathNode = new PathNode()
                            {
                                Node = neighbour,
                                DistanceFromSource = -1,    /// Will be filled later
                                EstimationToTarget = neighbourEstimation
                            };
                            newNode = true;
                        }

                        /// Reconsider the distance from source to the current neighbour and overwrite if necessary.
                        int currentToNeighbourDist = Path.ComputeDistance(currentPathNode.Node.Center, neighbourPathNode.Node.Center);
                        //int currentToNeighbourDist = 1;// Math.Max(32 - neighbourPathNode.Node.AreaOnMap.Width, 1);
                        if (neighbourPathNode.DistanceFromSource == -1 ||
                            neighbourPathNode.DistanceFromSource > currentPathNode.DistanceFromSource + currentToNeighbourDist)
                        {
                            neighbourPathNode.DistanceFromSource = currentPathNode.DistanceFromSource + currentToNeighbourDist;
                            neighbourPathNode.PrevNode = currentPathNode;
                        }

                        /// If the neighbour has not yet been queued do it now.
                        if (newNode)
                        {
                            this.queuedNodes.Insert((neighbourPathNode.DistanceFromSource + neighbourPathNode.EstimationToTarget),
                                                    neighbourPathNode);
                            this.queuedNodesMap[neighbour.Index] =  neighbourPathNode;
                        }
                    }
                } /// End of neighbour processing.

                iterationsInCurrCall++;
                this.iterationsExecuted++;
                if (currentPathNode.Node == this.toNode)
                {
                    /// Path found.
                    toNodeReached = true;
                    break;
                }
            } /// End of processing the head of the queue.

            if (toNodeReached || this.queuedNodes.Count == 0 || this.iterationsExecuted == this.iterationLimit) { this.CollectNodes(); }
            return iterationsInCurrCall;
        }

        /// <summary>
        /// Collects the list of nodes among this path.
        /// </summary>
        private void CollectNodes()
        {
            this.nodesOnPath = new List<PFTreeNode>();
            PFTreeNode currNode = this.lastNodeOnPath;
            this.nodesOnPath.Add(currNode);
            while (currNode != this.fromNode)
            {
                PathNode currPathNode = this.completedNodesMap[currNode.Index];
                currNode = currPathNode.PrevNode.Node;
                this.nodesOnPath.Add(currNode);
            }
            this.nodesOnPath.Reverse();
        }

        /// <summary>
        /// Checks whether the edge between the two given pathfinder tree nodes is blocked or not.
        /// </summary>
        /// <param name="nodeA">The first node of the edge.</param>
        /// <param name="nodeB">The second node of the edge.</param>
        /// <returns>True if the edge is blocked, false otherwise.</returns>
        private bool IsEdgeBlocked(PFTreeNode nodeA, PFTreeNode nodeB)
        {
            return this.blockedEdges.Contains(new Tuple<int, int>(nodeA.Index, nodeB.Index)) ||
                   this.blockedEdges.Contains(new Tuple<int, int>(nodeB.Index, nodeA.Index));
        }

        /// <summary>
        /// Initializes the A* pathfinding algorithm.
        /// </summary>
        private void InitSearchAlgorithm()
        {
            this.queuedNodes = new BinaryHeap<PathNode>(BinaryHeap<PathNode>.HeapType.MinHeap);
            this.queuedNodesMap = new PathNode[this.fromNode.LeafCount];
            this.completedNodesMap = new PathNode[this.fromNode.LeafCount];
            int sourceEstimation = Path.ComputeDistance(this.fromNode.Center, this.toNode.Center);
            this.queuedNodes.Insert(sourceEstimation,
                new PathNode()
                {
                    Node = this.fromNode,
                    DistanceFromSource = 0,
                    EstimationToTarget = sourceEstimation,
                    PrevNode = null
                });
        }

        /// <summary>
        /// Computes the distance between 2 points on the map for the pathfinding algorithm.
        /// </summary>
        /// <param name="fromCoords">The first point on the map.</param>
        /// <param name="toCoords">The second point on the map.</param>
        /// <returns>The computed distance between the given points.</returns>
        private static int ComputeDistance(RCIntVector fromCoords, RCIntVector toCoords)
        {
            int horz = Math.Abs(toCoords.X - fromCoords.X);
            int vert = Math.Abs(toCoords.Y - fromCoords.Y);
            int diff = Math.Abs(horz - vert);
            return (horz < vert ? horz : vert) * 3 + diff * 2;
        }

        #endregion Internal methods

        /// <summary>
        /// The node that contains the starting cell of the path.
        /// </summary>
        private PFTreeNode fromNode;

        /// <summary>
        /// The node that contains the target cell of the path.
        /// </summary>
        private PFTreeNode toNode;

        /// <summary>
        /// The last node on the computed path. Equals with the target node if it's reachable from the starting node. Otherwise
        /// it's the reference to the nearest node.
        /// </summary>
        private PFTreeNode lastNodeOnPath;

        /// <summary>
        /// The priority queue of the nodes for the pathfinding.
        /// </summary>
        private BinaryHeap<PathNode> queuedNodes;

        /// <summary>
        /// The list of the nodes in the priority queue mapped by the IDs of their corresponding tree nodes.
        /// </summary>
        private PathNode[] queuedNodesMap;

        /// <summary>
        /// The list of the completed nodes mapped by the IDs of their corresponding tree nodes.
        /// </summary>
        private PathNode[] completedNodesMap;

        /// <summary>
        /// The list of the nodes among this path.
        /// </summary>
        private List<PFTreeNode> nodesOnPath;

        /// <summary>
        /// List of the blocked edges in the pathfinding graph.
        /// </summary>
        private HashSet<Tuple<int, int>> blockedEdges;

        /// <summary>
        /// This flag indicates whether searching this path has been aborted or not.
        /// </summary>
        private bool isAborted;

        /// <summary>
        /// The maximum number of iterations to execute when searching this path.
        /// </summary>
        private int iterationLimit;

        /// <summary>
        /// The total number of iterations already executed.
        /// </summary>
        private int iterationsExecuted;
    }

    /// <summary>
    /// Represents a node on a computed path.
    /// </summary>
    class PathNode
    {
        /// <summary>
        /// The referenced tree node.
        /// </summary>
        public PFTreeNode Node = null;

        /// <summary>
        /// The distance of this node from the source.
        /// </summary>
        public int DistanceFromSource = -1;

        /// <summary>
        /// The estimated distance of this node from the target.
        /// </summary>
        public int EstimationToTarget = -1;

        /// <summary>
        /// Reference to the previous node on the path.
        /// </summary>
        public PathNode PrevNode = null;
    }
}
