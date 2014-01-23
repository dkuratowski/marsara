using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Represents an A* pathfinding algorithm that finds a detour from a given node on an existing path.
    /// </summary>
    class DetourFindAlgorithm : PathFindingAlgorithm
    {
        /// <summary>
        /// Constructs a DetourFindAlgorithm based on a previously computed path.
        /// </summary>
        /// <param name="detouredPath">The original path.</param>
        /// <param name="abortedSectionIdx">The index of the aborted section of the original path.</param>
        /// <param name="iterationLimit">The maximum number of iterations to execute when searching this path.</param>
        public DetourFindAlgorithm(Path detouredPath, int abortedSectionIdx, int iterationLimit)
            : base(detouredPath.GetPathNode(abortedSectionIdx),
                   PathFindingAlgorithm.ComputeDistance(detouredPath.GetPathNode(abortedSectionIdx).Center, detouredPath.GetPathNode(detouredPath.Length - 1).Center), iterationLimit)
        {
            this.detouredPath = detouredPath;
            //this.blockedEdges = blockedEdges;
            //this.pathNodesToRejoin = new HashSet<PFTreeNode>();

            //PathNode currNode = this.detouredPath.BestNode;
            //bool fromNodeFound = false;
            //while (currNode.PreviousNode != null)
            //{
            //    this.pathNodesToRejoin.Add(currNode.Node);
            //    if (currNode.PreviousNode.Node == fromNode)
            //    {
            //        fromNodeFound = true;
            //        break;
            //    }
            //    currNode = currNode.PreviousNode;
            //}

            //if (!fromNodeFound) { throw new ArgumentException("The starting node must be on the detoured path!"); }
        }

        #region Overrides

        /// <see cref="PathFindingAlgorithm.CheckNeighbour"/>
        protected override bool CheckNeighbour(PFTreeNode current, PFTreeNode neighbour)
        {
            return neighbour.IsWalkable && !this.IsEdgeBlocked(current, neighbour);
        }

        /// <see cref="PathFindingAlgorithm.GetEstimation"/>
        //protected override int GetEstimation(PFTreeNode node)
        //{
        //    return PathFindingAlgorithm.ComputeDistance(node.Center, this.detouredPath.BestNode.Node.Center);
        //}

        /// <see cref="PathFindingAlgorithm.CheckExitCriteria"/>
        protected override bool CheckExitCriteria(PFTreeNode currentNode)
        {
            /// TODO: implement this method!
            return this.pathNodesToRejoin.Contains(currentNode);
        }

        #endregion Overrides

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
        /// The original path.
        /// </summary>
        private Path detouredPath;

        /// <summary>
        /// List of the remaining nodes on the original path after the starting node of the detour.
        /// </summary>
        private HashSet<PFTreeNode> pathNodesToRejoin;

        /// <summary>
        /// Contains the blocked edges that have to be bypassed by this search algorithm.
        /// </summary>
        private HashSet<Tuple<int, int>> blockedEdges;
    }
}
