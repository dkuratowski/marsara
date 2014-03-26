using RC.Engine.Simulator.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Maps.PublicInterfaces;
using RC.Common;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// Represents a node on a computed path.
    /// </summary>
    class PathNode : BinaryHeapItem
    {
        /// <summary>
        /// Creates a PathNode instance for the given navmesh node.
        /// </summary>
        /// <param name="node">The referenced navmesh node.</param>
        /// <param name="estimationToTarget">The target distance estimation.</param>
        /// <returns></returns>
        public static PathNode CreateSourceNode(INavMeshNode node, RCNumber estimationToTarget)
        {
            PathNode sourceNode = new PathNode(node, estimationToTarget);
            sourceNode.distanceFromSource = 0;
            sourceNode.OnKeyChanged((sourceNode.distanceFromSource + 2 * sourceNode.estimationToTarget).Bits);
            return sourceNode;
        }

        /// <summary>
        /// Constructs a PathNode instance for the given navmesh node with the given target estimation.
        /// </summary>
        /// <param name="node">The referenced navmesh node.</param>
        /// <param name="targetEstimation">The target distance estimation.</param>
        public PathNode(INavMeshNode node, RCNumber estimationToTarget)
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
            RCNumber newPathLength = prevNode.distanceFromSource
                                   + MapUtils.ComputeDistance(prevNode.node.Polygon.Center, this.node.Polygon.Center);
            if (this.distanceFromSource == -1 || this.distanceFromSource > newPathLength)
            {
                this.distanceFromSource = newPathLength;
                this.previousNode = prevNode;
                this.OnKeyChanged((this.distanceFromSource + 2 * this.estimationToTarget).Bits);
            }
        }

        /// <summary>
        /// Gets a reference to the previous node on the path.
        /// </summary>
        public PathNode PreviousNode { get { return this.previousNode; } }

        /// <summary>
        /// Gets the referenced navmesh node.
        /// </summary>
        public INavMeshNode Node { get { return this.node; } }

        /// <summary>
        /// Gets the distance of the shortest path from the source node to this node.
        /// </summary>
        public RCNumber DistanceFromSource { get { return this.distanceFromSource; } }

        /// <summary>
        /// The distance of this node from the source.
        /// </summary>
        private RCNumber distanceFromSource;

        /// <summary>
        /// The referenced navmesh node.
        /// </summary>
        private INavMeshNode node;

        /// <summary>
        /// The estimated distance of this node from the target.
        /// </summary>
        private RCNumber estimationToTarget;

        /// <summary>
        /// Reference to the previous node on the path.
        /// </summary>
        private PathNode previousNode;
    }
}
