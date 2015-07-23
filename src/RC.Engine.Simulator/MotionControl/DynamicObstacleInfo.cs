using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// Contains velocity and position informations about a dynamic obstacle in the environment of a motion controlled scenario element.
    /// </summary>
    public struct DynamicObstacleInfo
    {
        /// <summary>
        /// The current position of the dynamic obstacle.
        /// </summary>
        public RCNumRectangle Position;

        /// <summary>
        /// The current velocity of the dynamic obstacle.
        /// </summary>
        public RCNumVector Velocity;
    }
}
