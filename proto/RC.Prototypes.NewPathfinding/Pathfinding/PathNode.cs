using RC.Prototypes.NewPathfinding.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Prototypes.NewPathfinding.Pathfinding
{
    /// <summary>
    /// Represents a node in the pathfinding tree calculated during the execution of a pathfinding algorithm.
    /// </summary>
    /// <typeparam name="TNode">The type of the wrapped graph node.</typeparam>
    class PathNode<TNode>
    {
        /// <summary>
        /// Constructs a source PathNode over the given graph node.
        /// </summary>
        /// <param name="node">The wrapped graph node.</param>
        /// <param name="graph">The pathfinding graph.</param>
        public PathNode(TNode node, IGraph<TNode> graph)
        {
            this.node = node;
            this.parentPathNode = null;
            this.graph = graph;
            this.distanceFromSource = 0;
            this.priority = this.graph.EstimationToTarget(this.node);
        }

        /// <summary>
        /// Constructs a successor PathNode over the given graph node.
        /// </summary>
        /// <param name="node">The wrapped graph node.</param>
        /// <param name="graph">The pathfinding graph.</param>
        /// <param name="parentPathNode">The parent of this PathNode.</param>
        public PathNode(TNode node, IGraph<TNode> graph, PathNode<TNode> parentPathNode)
        {
            this.node = node;
            this.parentPathNode = parentPathNode;
            this.graph = graph;
            this.distanceFromSource = this.parentPathNode.DistanceFromSource + this.graph.Distance(this.parentPathNode.Node, this.node);
            this.priority = this.distanceFromSource + this.graph.EstimationToTarget(this.node);
        }

        /// <summary>
        /// Modifies the parent of this pathnode if the path from the source to this node through the given node is shorter then the
        /// currently found shortest path to this node.
        /// </summary>
        /// <param name="checkedPathNode">The given pathnode to set as new parent if it gives a shorter path to this node.</param>
        /// <returns>True if the given pathnode has been set as a new parent; otherwise false.</returns>
        public bool TrySetNewParent(PathNode<TNode> checkedPathNode)
        {
            int newPathLength = checkedPathNode.DistanceFromSource + this.graph.Distance(checkedPathNode.Node, this.node);
            if (this.parentPathNode == null || this.distanceFromSource > newPathLength)
            {
                this.distanceFromSource = newPathLength;
                this.parentPathNode = checkedPathNode;
                this.priority = this.distanceFromSource + this.graph.EstimationToTarget(this.node);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Calculates the path that ends at this pathnode.
        /// </summary>
        /// <returns>The calculated path.</returns>
        public List<TNode> CalculatePath()
        {
            List<TNode> path = new List<TNode>();
            path.Add(this.node);

            PathNode<TNode> currentPathNode = this;
            while (currentPathNode.parentPathNode != null)
            {
                currentPathNode = currentPathNode.parentPathNode;
                path.Add(currentPathNode.node);
            }
            path.Reverse();
            return path;
        }

        /// <summary>
        /// Gets the distance from the source graph node.
        /// </summary>
        public int DistanceFromSource { get { return this.distanceFromSource; } }

        /// <summary>
        /// Gets the priority of this pathnode (less value -> higher priority).
        /// </summary>
        public int Priority { get { return this.priority; } }

        /// <summary>
        /// Gets the wrapped graph node.
        /// </summary>
        public TNode Node { get { return this.node; } }

        /// <summary>
        /// The wrapped graph node.
        /// </summary>
        private TNode node;

        /// <summary>
        /// The parent pathnode of this pathnode in the pathfinding tree.
        /// </summary>
        private PathNode<TNode> parentPathNode;

        /// <summary>
        /// The distance from the source graph node.
        /// </summary>
        private int distanceFromSource;

        /// <summary>
        /// The priority of this pathnode (less value -> higher priority).
        /// </summary>
        private int priority;

        /// <summary>
        /// The pathfinding graph.
        /// </summary>
        private IGraph<TNode> graph;
    }
}
