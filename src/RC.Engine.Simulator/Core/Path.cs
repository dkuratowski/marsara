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
            this.toNode = this.fromNode.GetLeafNode(toCoords);
            this.objectSize = size;

            this.SearchPath();
        }

        #region IPath methods

        /// <see cref="IPath.FindPathSection<T>"/>
        public List<RCIntVector> FindPathSection<T>(RCIntVector fromCoords, IMapContentManager<T> mapContentMgr) where T : IMapContent
        {
            throw new NotImplementedException();
        }

        #endregion IPath methods

        /// <summary>
        /// Gets the list of nodes along the computed path in a reverse order.
        /// </summary>
        /// <remarks>TODO: this is only for debugging!</remarks>
        internal List<PFTreeNode> ComputedPath
        {
            get
            {
                List<PFTreeNode> retList = new List<PFTreeNode>();
                PFTreeNode currNode = this.lastNodeOnPath;
                retList.Add(currNode);
                while (currNode != this.fromNode)
                {
                    PathNode currPathNode = this.completedNodesMap[currNode];
                    currNode = currPathNode.PrevNode.Node;
                    retList.Add(currNode);
                }
                return retList;
            }
        }

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

        /// <summary>
        /// Implements the A* pathfinding algorithm.
        /// </summary>
        private void SearchPath()
        {
            /// Initialize the A* pathfinding algorithm.
            this.queuedNodes = new BinaryHeap<PathNode>(BinaryHeap<PathNode>.HeapType.MinHeap);
            this.queuedNodesMap = new Dictionary<PFTreeNode, PathNode>();
            this.completedNodesMap = new Dictionary<PFTreeNode, PathNode>();
            RCNumber sourceEstimation = Path.ComputeDistance(this.fromNode.Center, this.toCoords);
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
                /// Get the next node from the priority queue and add it to the completed set.
                PathNode currentPathNode = this.queuedNodes.MaxMinItem;
                this.queuedNodes.DeleteMaxMin();
                this.completedNodesMap.Add(currentPathNode.Node, currentPathNode);

                /// Save it's reference if its distance to the target node is less than the minimum distance found.
                if (this.lastNodeOnPath == null ||
                    Path.ComputeDistance(currentPathNode.Node.Center, this.toCoords) < Path.ComputeDistance(this.lastNodeOnPath.Center, this.toCoords))
                {
                    this.lastNodeOnPath = currentPathNode.Node;
                }

                /// Process the neighbours of the next node.
                foreach (PFTreeNode neighbour in currentPathNode.Node.Neighbours)
                {
                    /// Process the current neighbour only if it is walkable and is not yet in the completed set.
                    if (neighbour.IsWalkable && neighbour.AreaOnMap.Width >= this.objectSize.X && neighbour.AreaOnMap.Height >= this.objectSize.Y && !this.completedNodesMap.ContainsKey(neighbour))
                    {
                        /// Compute an estimated distance from the current neighbour to the target.
                        RCNumber neighbourEstimation = Path.ComputeDistance(neighbour.Center, this.toCoords);
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
                        RCNumber currentToNeighbourDist = Path.ComputeDistance(currentPathNode.Node.Center, neighbourPathNode.Node.Center);
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
        /// Computes the distance between two points on the map.
        /// </summary>
        /// <param name="fromCoords">The first point on the map.</param>
        /// <param name="toCoords">The second point on the map.</param>
        /// <returns>The computed distance between the given points.</returns>
        private static RCNumber ComputeDistance(RCNumVector fromCoords, RCNumVector toCoords)
        {
            RCNumber horz = (toCoords.X - fromCoords.X).Abs();
            RCNumber vert = (toCoords.Y - fromCoords.Y).Abs();
            RCNumber diff = (horz - vert).Abs();
            return (horz < vert ? horz : vert) * ROOT_OF_TWO + diff;
        }

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
        /// The square root of 2 used in distance calculations.
        /// </summary>
        private static readonly RCNumber ROOT_OF_TWO = (RCNumber)14142 / (RCNumber)10000;
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
