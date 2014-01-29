using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Scenarios
{
    /// <summary>
    /// Contains the informations about a dynamic obstacle that is needed for the motion controller.
    /// </summary>
    struct DynamicObstacleInfo
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

    /// <summary>
    /// Internal interface that is used by the motion controllers to control their corresponding entity.
    /// </summary>
    interface IEntityActuator
    {
        /// <summary>
        /// Gets the current position of the controlled entity.
        /// </summary>
        RCNumRectangle CurrentPosition { get; }

        /// <summary>
        /// Gets the current velocity of the controlled entity.
        /// </summary>
        RCNumVector CurrentVelocity { get; }

        /// <summary>
        /// Gets the preferred velocity of the controlled entity.
        /// </summary>
        RCNumVector PreferredVelocity { get; }

        /// <summary>
        /// Changes the velocity of the controlled entity.
        /// </summary>
        /// <param name="selectedVelocityIndex">The index of the selected velocity in the admissible velocity list.</param>
        void SetVelocity(int selectedVelocityIndex);

        /// <summary>
        /// Gets the list of the currently admissible velocities of the controlled entity.
        /// </summary>
        /// <returns>A list that contains the currently admissible velocities.</returns>
        List<RCNumVector> GetAdmissibleVelocities();

        /// <summary>
        /// Gets the list of the dynamic obstacles inside the neighbor region of the controlled entity.
        /// </summary>
        /// <returns>A list that contains the dynamic obstacles inside the neighbor region of the controlled entity.</returns>
        List<DynamicObstacleInfo> GetDynamicObstacles();
    }
}
