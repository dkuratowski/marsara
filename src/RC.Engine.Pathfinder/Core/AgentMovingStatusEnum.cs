using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Pathfinder.Core
{
    /// <summary>
    /// Enumerates the possible moving states of an agent.
    /// </summary>
    enum AgentMovingStatusEnum
    {
        Moving = 0,     /// The agent is currently moving.
        Stopped = 1,    /// The agent is currently stopped.
        Static = 2      /// The agent shall be considered as a static obstacle.
    }
}
