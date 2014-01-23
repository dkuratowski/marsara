using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Represents a path derived from a cached path.
    /// </summary>
    class DerivedPath : Path
    {
        /// <summary>
        /// Constructs a new DerivedPath instance.
        /// </summary>
        /// <param name="srcConnectionAlgorithm">The algorithm that connects the source node to the cached path.</param>
        /// <param name="tgtConnectionAlgorithm">The algorithm that connects the target node to the cached path.</param>
        public DerivedPath(EndpointConnectionAlgorithm srcConnectionAlgorithm, EndpointConnectionAlgorithm tgtConnectionAlgorithm)
        {
            this.sourceConnectionAlgorithm = srcConnectionAlgorithm;
            this.targetConnectionAlgorithm = tgtConnectionAlgorithm;
        }

        /// <see cref="IPath.IsReadyForUse"/>
        public override bool IsReadyForUse { get { return this.sourceConnectionAlgorithm.IsFinished && this.targetConnectionAlgorithm.IsFinished; } }

        /// <see cref="Path.CollectNodesOnPath"/>
        protected override List<PFTreeNode> CollectNodesOnPath()
        {
            List<PFTreeNode> retList = new List<PFTreeNode>();
            PathNode currCachedPathNode = null;
            if (this.targetConnectionAlgorithm.PathNodeToReach != null)
            {
                PathNode currTCNode = this.targetConnectionAlgorithm.BestNode;
                retList.Add(currTCNode.Node);
                while (currTCNode != this.targetConnectionAlgorithm.FromNode)
                {
                    currTCNode = currTCNode.PreviousNode;
                    retList.Add(currTCNode.Node);
                }
                retList.Reverse();

                currCachedPathNode = this.targetConnectionAlgorithm.CachedAlgorithm.BestNode;
                while (currCachedPathNode.Node != this.targetConnectionAlgorithm.PathNodeToReach)
                {
                    currCachedPathNode = currCachedPathNode.PreviousNode;
                }
            }
            else
            {
                currCachedPathNode = this.targetConnectionAlgorithm.CachedAlgorithm.BestNode;
            }

            while (currCachedPathNode.Node != this.sourceConnectionAlgorithm.BestNode.Node)
            {
                currCachedPathNode = currCachedPathNode.PreviousNode;
                retList.Add(currCachedPathNode.Node);
            }

            PathNode currSCNode = this.sourceConnectionAlgorithm.BestNode;
            while (currSCNode.PreviousNode != null)
            {
                currSCNode = currSCNode.PreviousNode;
                retList.Add(currSCNode.Node);
            }

            retList.Reverse();
            return retList;
        }

        /// <see cref="Path.FromNode"/>
        protected override PFTreeNode FromNode { get { return this.sourceConnectionAlgorithm.FromNode.Node; } }

        /// <see cref="Path.ToNode"/>
        protected override PFTreeNode ToNode { get { return this.targetConnectionAlgorithm.FromNode.Node; } }

        /// <see cref="Path.CompletedNodes"/>
        protected internal override IEnumerable<PFTreeNode> CompletedNodes
        {
            get
            {
                List<PFTreeNode> retList = new List<PFTreeNode>();
                foreach (PathNode completedNode in this.sourceConnectionAlgorithm.CompletedNodes) { retList.Add(completedNode.Node); }
                foreach (PathNode completedNode in this.targetConnectionAlgorithm.CompletedNodes) { retList.Add(completedNode.Node); }
                return retList;
            }
        }

        /// <summary>
        /// Reference to the algorithm that connects the source node to the cached path.
        /// </summary>
        private EndpointConnectionAlgorithm sourceConnectionAlgorithm;

        /// <summary>
        /// Reference to the algorithm that connects the target node to the cached path.
        /// </summary>
        private EndpointConnectionAlgorithm targetConnectionAlgorithm;
    }
}
