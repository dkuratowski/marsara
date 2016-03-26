using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Engine.Pathfinder.Core
{
    /// <summary>
    /// The interface of a pathfinding graph.
    /// </summary>
    /// <typeparam name="TNode">The type of nodes in the pathfinding graph.</typeparam>
    interface IGraph<TNode>
    {
        /// <summary>
        /// Calculates the distance between 2 nodes in the pathfinding graph.
        /// </summary>
        /// <param name="nodeA">The first node.</param>
        /// <param name="nodeB">The second node.</param>
        /// <returns>The distance between the 2 nodes in the pathfinding graph.</returns>
        int Distance(TNode nodeA, TNode nodeB);

        /// <summary>
        /// Calculates the estimated distance from the given node to the target of the pathfinding.
        /// </summary>
        /// <param name="node">The given node.</param>
        /// <returns>The estimated distance from the given node to the target of the pathfinding.</returns>
        int EstimationToTarget(TNode node);

        /// <summary>
        /// Gets the neighbours of the given node in the pathfinding graph.
        /// </summary>
        /// <param name="node">The given node.</param>
        /// <returns>The neighbours of the given node in the pathfinding graph.</returns>
        IEnumerable<TNode> GetNeighbours(TNode node);

        /// <summary>
        /// Checks whether the given node is a target of the pathfinding.
        /// </summary>
        /// <param name="node">The checked node.</param>
        /// <returns>True if the given node is a target of the pathfinding.</returns>
        bool IsTargetNode(TNode node);
    }
}
