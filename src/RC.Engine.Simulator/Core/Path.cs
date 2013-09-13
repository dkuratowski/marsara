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
        /// <param name="size">The size of the object that requested the pathfinding.</param>
        public Path(PFTreeNode fromNode, RCIntVector toCoords, RCNumVector size)
        {
            this.fromNode = fromNode;
            this.toCoords = toCoords;
            this.lastNodeOnPath = null;
            this.nodesOnPath = null;
            this.toNode = this.fromNode.GetLeafNode(toCoords);
            this.objectSize = size;
            this.blockedEdges = new Dictionary<PFTreeNode, HashSet<PFTreeNode>>();

            this.SearchPath();
            this.CollectNodes();
        }

        /// <summary>
        /// Constructs a new Path instance by computing an alternative path from the given section of the given original path.
        /// </summary>
        /// <param name="originalPath">The original path.</param>
        /// <param name="abortedSectionIdx">The index of the aborted section of the original path.</param>
        public Path(Path originalPath, int abortedSectionIdx)
        {
            this.fromNode = originalPath.nodesOnPath[abortedSectionIdx];
            this.toCoords = originalPath.toCoords;
            this.lastNodeOnPath = null;
            this.nodesOnPath = null;
            this.toNode = originalPath.toNode;
            this.objectSize = originalPath.objectSize;
            
            this.blockedEdges = new Dictionary<PFTreeNode, HashSet<PFTreeNode>>();
            foreach (KeyValuePair<PFTreeNode, HashSet<PFTreeNode>> item in originalPath.blockedEdges)
            {
                this.blockedEdges.Add(item.Key, new HashSet<PFTreeNode>(item.Value));
            }
            this.AddBlockedEdge(originalPath.nodesOnPath[abortedSectionIdx], originalPath.nodesOnPath[abortedSectionIdx + 1]);

            this.SearchPath();
            this.CollectNodes();
        }

        #region IPath methods
        
        /// <see cref="IPath.this[]"/>
        public RCIntRectangle this[int index]
        {
            get { return this.nodesOnPath[index].AreaOnMap; }
        }

        /// <see cref="IPath.Length"/>
        public int Length { get { return this.nodesOnPath.Count; } }

        /// <see cref="IPath.ForgetBlockedEdges"/>
        public void ForgetBlockedEdges()
        {
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
                return this.completedNodesMap.Keys;
            }
        }

        #endregion Methods for debugging

        #region Internal methods

        /// <summary>
        /// Implements the A* pathfinding algorithm.
        /// </summary>
        private void SearchPath()
        {
            /// Initialize the A* pathfinding algorithm.
            this.queuedNodes = new BinaryHeap<PathNode>(BinaryHeap<PathNode>.HeapType.MinHeap);
            this.queuedNodesMap = new Dictionary<PFTreeNode, PathNode>();
            this.completedNodesMap = new Dictionary<PFTreeNode, PathNode>();
            RCNumber sourceEstimation = MapUtils.ComputeDistance(this.fromNode.Center, this.toCoords);
            this.queuedNodes.Insert(sourceEstimation.Bits,
                new PathNode()
                {
                    Node = this.fromNode,
                    DistanceFromSource = 0,
                    EstimationToTarget = sourceEstimation,
                    PrevNode = null
                });

            /// Here begins the A* pathfinding algorithm. It runs until the priority queue becomes empty.
            /// TODO: We might have to limit the number of iterations.
            while (this.queuedNodes.Count != 0)
            {
                /// Get the current node from the priority queue and add it to the completed set.
                PathNode currentPathNode = this.queuedNodes.MaxMinItem;
                this.queuedNodes.DeleteMaxMin();
                this.completedNodesMap.Add(currentPathNode.Node, currentPathNode);

                /// Save it's reference if its distance to the target node is less than the minimum distance found.
                if (this.lastNodeOnPath == null ||
                    MapUtils.ComputeDistance(currentPathNode.Node.Center, this.toCoords) < MapUtils.ComputeDistance(this.lastNodeOnPath.Center, this.toCoords))
                {
                    this.lastNodeOnPath = currentPathNode.Node;
                }

                /// Process the neighbours of the current node.
                foreach (PFTreeNode neighbour in currentPathNode.Node.Neighbours)
                {
                    /// Process the current neighbour only if it is walkable, is not yet in the completed set, and the edge from the current node
                    /// to that neighbour is not blocked.
                    if (neighbour.IsWalkable &&
                        neighbour.AreaOnMap.Width >= this.objectSize.X &&
                        neighbour.AreaOnMap.Height >= this.objectSize.Y &&
                        !this.completedNodesMap.ContainsKey(neighbour) &&
                        !this.IsEdgeBlocked(currentPathNode.Node, neighbour))
                    {
                        /// Compute an estimated distance from the current neighbour to the target.
                        RCNumber neighbourEstimation = MapUtils.ComputeDistance(neighbour.Center, this.toCoords);
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
                            neighbourPathNode = new PathNode()
                            {
                                Node = neighbour,
                                DistanceFromSource = -1,    /// Will be filled later
                                EstimationToTarget = neighbourEstimation
                            };
                            newNode = true;
                        }

                        /// Reconsider the distance from source to the current neighbour and overwrite if necessary.
                        RCNumber currentToNeighbourDist = MapUtils.ComputeDistance(currentPathNode.Node.Center, neighbourPathNode.Node.Center);
                        if (neighbourPathNode.DistanceFromSource == -1 ||
                            neighbourPathNode.DistanceFromSource > currentPathNode.DistanceFromSource + currentToNeighbourDist)
                        {
                            neighbourPathNode.DistanceFromSource = currentPathNode.DistanceFromSource + currentToNeighbourDist;
                            neighbourPathNode.PrevNode = currentPathNode;
                        }

                        /// If the neighbour has not yet been queued do it now.
                        if (newNode)
                        {
                            this.queuedNodes.Insert((neighbourPathNode.DistanceFromSource + neighbourPathNode.EstimationToTarget).Bits,
                                                    neighbourPathNode);
                            this.queuedNodesMap.Add(neighbour, neighbourPathNode);
                        }
                    }
                } /// End of neighbour processing.

                if (currentPathNode.Node == this.toNode) { break; }
            } /// End of processing the head of the queue.
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
                PathNode currPathNode = this.completedNodesMap[currNode];
                currNode = currPathNode.PrevNode.Node;
                this.nodesOnPath.Add(currNode);
            }
            this.nodesOnPath.Reverse();
        }

        /// <summary>
        /// Registers a blocked edge between two nodes in the pathfinder tree.
        /// </summary>
        /// <param name="nodeA">The first node of the blocked edge.</param>
        /// <param name="nodeB">The second node of the blocked edge.</param>
        private void AddBlockedEdge(PFTreeNode nodeA, PFTreeNode nodeB)
        {
            if (!this.blockedEdges.ContainsKey(nodeA))
            {
                this.blockedEdges.Add(nodeA, new HashSet<PFTreeNode>());
                this.blockedEdges.Add(nodeB, new HashSet<PFTreeNode>());
            }

            this.blockedEdges[nodeA].Add(nodeB);
            this.blockedEdges[nodeB].Add(nodeA);
        }

        /// <summary>
        /// Checks whether the edge between the two given pathfinder tree nodes is blocked or not.
        /// </summary>
        /// <param name="nodeA">The first node of the edge.</param>
        /// <param name="nodeB">The second node of the edge.</param>
        /// <returns>True if the edge is blocked, false otherwise.</returns>
        private bool IsEdgeBlocked(PFTreeNode nodeA, PFTreeNode nodeB)
        {
            return this.blockedEdges.ContainsKey(nodeA) && this.blockedEdges[nodeA].Contains(nodeB);
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
        /// The coordinates of the target cell of the path.
        /// </summary>
        private RCIntVector toCoords;

        /// <summary>
        /// The size of the object that requested the pathfinding.
        /// </summary>
        private RCNumVector objectSize;

        /// <summary>
        /// The priority queue of the nodes for the pathfinding.
        /// </summary>
        private BinaryHeap<PathNode> queuedNodes;

        /// <summary>
        /// The list of the nodes in the priority queue mapped by their corresponding tree nodes.
        /// </summary>
        private Dictionary<PFTreeNode, PathNode> queuedNodesMap;

        /// <summary>
        /// The list of the completed nodes mapped by their corresponding tree nodes.
        /// </summary>
        private Dictionary<PFTreeNode, PathNode> completedNodesMap;

        /// <summary>
        /// The list of the nodes among this path.
        /// </summary>
        private List<PFTreeNode> nodesOnPath;

        /// <summary>
        /// List of the blocked edges in the pathfinding graph.
        /// </summary>
        private Dictionary<PFTreeNode, HashSet<PFTreeNode>> blockedEdges;
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
        public RCNumber DistanceFromSource = -1;

        /// <summary>
        /// The estimated distance of this node from the target.
        /// </summary>
        public RCNumber EstimationToTarget = -1;

        /// <summary>
        /// Reference to the previous node on the path.
        /// </summary>
        public PathNode PrevNode = null;
    }
}
