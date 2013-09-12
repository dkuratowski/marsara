using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Maps.PublicInterfaces;
using RC.Common;

namespace RC.Engine.Simulator.PublicInterfaces
{
    /// <summary>
    /// Defines the interface of a game object on a map.
    /// </summary>
    public interface IGameObject : IMapContent
    {
        /// <summary>
        /// Sends a command to this game object for execution.
        /// </summary>
        /// <param name="targetPoint">The target point of the command on the map.</param>
        void SendCommand(RCNumVector targetPoint);

        /// <summary>
        /// Checks whether this game object is in a stopped state or not.
        /// </summary>
        bool IsStopped { get; }
    }
}
