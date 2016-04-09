using RC.Common;
using RC.Common.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Engine.Pathfinder.PublicInterfaces
{
    /// <summary>
    /// The public interface of the pathfinder component.
    /// </summary>
    [ComponentInterface]
    public interface IPathfinder
    {
        /// <summary>
        /// Initializes the pathfinder component with the given walkability informations.
        /// </summary>
        /// <param name="walkabilityReader">The reader that provides the walkability informations.</param>
        /// <param name="maxMovingSize">The maximum size of moving agents.</param>
        void Initialize(IWalkabilityReader walkabilityReader, int maxMovingSize);

        /// <summary>
        /// Places an agent with the given area onto the pathfinding grid.
        /// </summary>
        /// <param name="area">The rectangular area of the agent.</param>
        /// <param name="client">The client of the placed agent that will provide additional informations for the pathfinder component.</param>
        /// <returns>A reference to the agent or null if the placement failed.</returns>
        /// <exception cref="InvalidOperationException">
        /// If the pathfinder component is not initialized.
        /// </exception>
        IAgent PlaceAgent(RCIntRectangle area, IAgentClient client);

        /// <summary>
        /// Removes the given agent from the pathfinding grid.
        /// </summary>
        /// <param name="agent">The agent to be removed.</param>
        /// <exception cref="InvalidOperationException">
        /// If the agent is not placed on the pathfinding grid.
        /// If the pathfinder component is not initialized.
        /// </exception>
        void RemoveAgent(IAgent agent);

        /// <summary>
        /// Updates the state of the pathfinder component.
        /// </summary>
        void Update();
    }
}
