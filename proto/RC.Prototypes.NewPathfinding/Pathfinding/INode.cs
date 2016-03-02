using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Prototypes.NewPathfinding.Pathfinding
{
    /// <summary>
    /// The interface of the nodes in pathfinding graphs.
    /// </summary>
    interface INode<TNode>
    {
        /// <summary>
        /// Calculates the distance of this node to the given other node.
        /// </summary>
        /// <param name="other">The other node.</param>
        /// <returns>The distance of this node to the given other node.</returns>
        int Distance(TNode other);

        /// <summary>
        /// Gets the successors of this node in the graph for the given object size.
        /// </summary>
        /// <param name="objectSize">The maximum size of objects for which the successors are to be searched.</param>
        /// <returns>The successors of this node in the graph for the given object size.</returns>
        IEnumerable<TNode> GetSuccessors(int objectSize);
    }
}
