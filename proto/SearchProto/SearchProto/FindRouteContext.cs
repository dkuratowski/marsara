using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchProto
{
    class FindRouteContext
    {
        public FindRouteContext(NavMeshNode source, NavMeshNode target)
        {
            this.queuedNodes = new BinaryHeap<NodeInfo>(HeapType.MinHeap);
            this.queuedNodesMap = new Dictionary<NavMeshNode, NodeInfo>();
            this.completedNodes = new Dictionary<NavMeshNode, NodeInfo>();
            this.source = source;
            this.target = target;

            int sourceHeuristic = this.source.ComputeHeuristic(target);
            this.queuedNodes.Insert(sourceHeuristic,
                                    new NodeInfo()
                                    {
                                        Node = this.source,
                                        DistanceFromSource = 0,
                                        HeuristicToTarget = sourceHeuristic,
                                        ParentDirection = -1
                                    });
        }

        public bool ProcessNextNode()
        {
            if (this.queuedNodes.Count != 0)
            {
                NodeInfo nextNodeInfo = this.queuedNodes.MaxMinItem;
                this.queuedNodes.DeleteMaxMin();
                this.completedNodes.Add(nextNodeInfo.Node, nextNodeInfo);

                if (nextNodeInfo.Node == this.target)
                {
                    return false;
                }

                for (int i = 0; i < NavMesh.MAX_DIRECTION; i++)
                {
                    NavMeshNode neighbour = nextNodeInfo.Node[i];
                    if (neighbour != null && !this.completedNodes.ContainsKey(neighbour))
                    {
                        int neighbourHeuristic = neighbour.ComputeHeuristic(target);

                        NodeInfo neighbourInfo = null;
                        bool newNode = false;
                        if (this.queuedNodesMap.ContainsKey(neighbour))
                        {
                            neighbourInfo = this.queuedNodesMap[neighbour];
                        }
                        else
                        {
                            neighbourInfo = new NodeInfo()
                            {
                                Node = neighbour,
                                DistanceFromSource = -1,
                                HeuristicToTarget = neighbourHeuristic
                            };
                            this.queuedNodesMap.Add(neighbour, neighbourInfo);
                            newNode = true;
                        }

                        if (neighbourInfo.DistanceFromSource == -1 ||
                            neighbourInfo.DistanceFromSource + neighbourInfo.HeuristicToTarget >
                            nextNodeInfo.DistanceFromSource + nextNodeInfo.HeuristicToTarget + (i % 2 == 0 ? NavMesh.SIDE_LENGTH : NavMesh.DIAGONAL_LENGTH))
                        {
                            neighbourInfo.DistanceFromSource = nextNodeInfo.DistanceFromSource + (i % 2 == 0 ? NavMesh.SIDE_LENGTH : NavMesh.DIAGONAL_LENGTH);
                            neighbourInfo.ParentDirection = NavMesh.Backward(i);
                        }

                        if (newNode)
                        {
                            this.queuedNodes.Insert(neighbourInfo.DistanceFromSource + neighbourInfo.HeuristicToTarget, neighbourInfo);
                        }
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<Tuple<NavMeshNode, int>> Route
        {
            get
            {
                if (!this.completedNodes.ContainsKey(this.target)) { return null; }

                List<Tuple<NavMeshNode, int>> retList = new List<Tuple<NavMeshNode, int>>();
                NavMeshNode currentNode = this.target;
                retList.Add(new Tuple<NavMeshNode,int>(currentNode, -1));
                while (currentNode != this.source)
                {
                    NodeInfo currNodeInfo = this.completedNodes[currentNode];
                    currentNode = currentNode[currNodeInfo.ParentDirection];
                    retList.Add(new Tuple<NavMeshNode,int>(currentNode, NavMesh.Backward(currNodeInfo.ParentDirection)));
                }
                retList.Reverse();
                return retList;
            }
        }

        public List<NavMeshNode> TouchedNodes { get { return this.completedNodes.Keys.ToList(); } }

        private BinaryHeap<NodeInfo> queuedNodes;
        private Dictionary<NavMeshNode, NodeInfo> completedNodes;
        private Dictionary<NavMeshNode, NodeInfo> queuedNodesMap;

        private NavMeshNode source;
        private NavMeshNode target;
    }

    class NodeInfo
    {
        public NavMeshNode Node = null;
        public int DistanceFromSource = -1;
        public int HeuristicToTarget = -1;
        public int ParentDirection = -1;
    }
}
