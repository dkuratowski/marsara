using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Engine.Pathfinder.PublicInterfaces
{
    /// <summary>
    /// The interface of the clients of agents that can provide additional informations for the pathfinder component.
    /// </summary>
    public interface IAgentClient
    {
        /// <summary>
        /// The maximum speed of the agent that this client belongs to.
        /// </summary>
        RCNumber MaxSpeed { get; }

        /// <summary>
        /// Additional check whether the agent of this client can overlap the agent of the given other client.
        /// </summary>
        /// <param name="otherClient">The other client to be checked.</param>
        /// <returns>True if the agent of this client can overlap the agent of the other client; otherwise false.</returns>
        bool IsOverlapEnabled(IAgentClient otherClient);
    }
}
