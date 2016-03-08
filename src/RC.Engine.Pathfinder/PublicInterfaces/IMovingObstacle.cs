using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Engine.Pathfinder.PublicInterfaces
{
    /// <summary>
    /// The interface of a moving obstacle on the pathfinding grid. A moving obstacle is a special obstacle that can be ordered to move
    /// to a given position.
    /// </summary>
    public interface IMovingObstacle : IObstacle
    {
        /// <summary>
        /// Orders this moving obstacle to move to the given target position.
        /// </summary>
        /// <param name="targetPosition">The target position of the top-left corner of this moving obstacle.</param>
        void MoveTo(RCIntVector targetPosition);

        /// <summary>
        /// Orders this moving obstacle to stop. If this obstacle is currently not moving then this function has no effect.
        /// </summary>
        void StopMoving();

        /// <summary>
        /// Gets whether this moving obstacle is currently performing a move operation or is currently stopped.
        /// </summary>
        bool IsMoving { get; }
    }
}
