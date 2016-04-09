using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Engine.Pathfinder.PublicInterfaces
{
    /// <summary>
    /// The interface of an agent on the pathfinding grid.
    /// </summary>
    public interface IAgent
    {
        /// <summary>
        /// Gets the area occupied by this agent on the pathfinding grid.
        /// </summary>
        RCIntRectangle Area { get; }

        /// <summary>
        /// Orders this agent to move to the given target position.
        /// </summary>
        /// <param name="targetPosition">The target position of the top-left corner of this agent.</param>
        /// <exception cref="NotSupportedException">
        /// If the area of this agent is not square or exceeds the limit of the size of moving agents with which the pathfinder component was initialized.
        /// </exception>
        void MoveTo(RCIntVector targetPosition);

        /// <summary>
        /// Orders this agent to stop moving. If this agent is currently not moving then this function has no effect.
        /// </summary>
        void StopMoving();

        /// <summary>
        /// Gets whether this agent is currently performing a move operation or is currently stopped.
        /// </summary>
        bool IsMoving { get; }
    }
}
