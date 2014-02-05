using RC.Engine.Simulator.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// Represents a node on a computed path.
    /// </summary>
    class PathNode : BinaryHeapItem
    {
        /// <summary>
        /// Creates a PathNode instance for the source node.
        /// </summary>
        /// <param name="node">The referenced PFTreeNode.</param>
        /// <param name="estimationToTarget">The target distance estimation.</param>
        /// <returns></returns>
        public static PathNode CreateSourceNode(PFTreeNode node, int estimationToTarget)
        {
            PathNode sourceNode = new PathNode(node, estimationToTarget);
            sourceNode.distanceFromSource = 0;
            sourceNode.OnKeyChanged(sourceNode.distanceFromSource + 2 * sourceNode.estimationToTarget);
            return sourceNode;
        }

        /// <summary>
        /// Constructs a PathNode instance for the given PFTreeNode with the given target estimation.
        /// </summary>
        /// <param name="node">The referenced PFTreeNode.</param>
        /// <param name="targetEstimation">The target distance estimation.</param>
        public PathNode(PFTreeNode node, int estimationToTarget)
        {
            this.node = node;
            this.estimationToTarget = estimationToTarget;
            this.distanceFromSource = -1;
            this.previousNode = null;
        }

        /// <summary>
        /// Modifies the computed shortest path to this node if the length of the shortest path to the given previous node
        /// plus the distance from the previous node to this node is shorter than the current shortest path to this node.
        /// </summary>
        /// <param name="prevNode">The previous node.</param>
        public void Approximate(PathNode prevNode)
        {
            int newPathLength = prevNode.distanceFromSource + PathFindingAlgorithm.ComputeDistance(prevNode.node.Center, this.node.Center);
            if (this.distanceFromSource == -1 || this.distanceFromSource > newPathLength)
            {
                this.distanceFromSource = newPathLength;
                this.previousNode = prevNode;
                this.OnKeyChanged(this.distanceFromSource + 2 * this.estimationToTarget);
            }
        }

        /// <summary>
        /// Gets a reference to the previous node on the path.
        /// </summary>
        public PathNode PreviousNode { get { return this.previousNode; } }

        /// <summary>
        /// Gets the referenced tree node.
        /// </summary>
        public PFTreeNode Node { get { return this.node; } }

        /// <summary>
        /// Gets the distance of the shortest path from the source node to this node.
        /// </summary>
        public int DistanceFromSource { get { return this.distanceFromSource; } }

        /// <summary>
        /// The distance of this node from the source.
        /// </summary>
        private int distanceFromSource;

        /// <summary>
        /// The referenced tree node.
        /// </summary>
        private PFTreeNode node;

        /// <summary>
        /// The estimated distance of this node from the target.
        /// </summary>
        private int estimationToTarget;

        /// <summary>
        /// Reference to the previous node on the path.
        /// </summary>
        private PathNode previousNode;
    }
}
