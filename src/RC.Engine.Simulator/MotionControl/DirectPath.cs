using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// Represents a path computed directly between two nodes.
    /// </summary>
    class DirectPath : Path
    {
        /// <summary>
        /// Constructs a new DirectPath instance.
        /// </summary>
        /// <param name="searchAlgorithm">The algorithm that computes the path.</param>
        public DirectPath(DirectPathFindingAlgorithm searchAlgorithm)
        {
            this.searchAlgorithm = searchAlgorithm;
            this.blockedEdges = new HashSet<Tuple<int, int>>();
            searchAlgorithm.CopyBlockedEdges(ref this.blockedEdges);
        }

        /// <see cref="IPath.IsReadyForUse"/>
        public override bool IsReadyForUse { get { return this.searchAlgorithm.IsFinished; } }

        /// <see cref="Path.CompletedNodes"/>
        public override void CopyBlockedEdges(ref HashSet<Tuple<int, int>> targetSet)
        {
            foreach (Tuple<int, int> blockedEdge in this.blockedEdges) { targetSet.Add(blockedEdge); }
        }

        /// <see cref="Path.CollectNodesOnPath"/>
        protected override List<PFTreeNode> CollectNodesOnPath()
        {
            List<PFTreeNode> retList = new List<PFTreeNode>();
            PathNode currNode = this.searchAlgorithm.BestNode;
            retList.Add(currNode.Node);
            while (currNode != this.searchAlgorithm.FromNode)
            {
                currNode = currNode.PreviousNode;
                retList.Add(currNode.Node);
            }
            retList.Reverse();
            return retList;
        }

        /// <see cref="Path.FromNode"/>
        public override PFTreeNode FromNode { get { return this.searchAlgorithm.FromNode.Node; } }

        /// <see cref="Path.ToNode"/>
        public override PFTreeNode ToNode { get { return this.searchAlgorithm.ToNode; } }

        /// <see cref="Path.CompletedNodes"/>
        protected internal override IEnumerable<PFTreeNode> CompletedNodes
        {
            get
            {
                List<PFTreeNode> retList = new List<PFTreeNode>();
                foreach (PathNode completedNode in this.searchAlgorithm.CompletedNodes) { retList.Add(completedNode.Node); }
                return retList;
            }
        }

        /// <see cref="Path.ForgetBlockedEdgesImpl"/>
        protected override void ForgetBlockedEdgesImpl() { this.blockedEdges.Clear(); }

        /// <summary>
        /// Reference to the algorithm that computes this path.
        /// </summary>
        private DirectPathFindingAlgorithm searchAlgorithm;

        /// <summary>
        /// List of the blocked edges.
        /// </summary>
        private HashSet<Tuple<int, int>> blockedEdges;
    }
}
