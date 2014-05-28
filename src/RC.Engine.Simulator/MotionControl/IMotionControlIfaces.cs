using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.MotionControl
{
    /// <summary>
    /// Contains velocity and position informations about a dynamic obstacle in the environment of a motion controlled target.
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

    /// <summary>
    /// Interface for accessing the position and the velocity of a motion controlled target.
    /// </summary>
    public interface IMotionControlTarget
    {
        /// <summary>
        /// Gets the current position of the controlled target.
        /// </summary>
        RCNumRectangle Position { get; }

        /// <summary>
        /// Gets the current velocity of the controlled target.
        /// </summary>
        RCNumVector Velocity { get; }
    }

    /// <summary>
    /// Interface for changing the velocity of a motion controlled target.
    /// </summary>
    public interface IMotionControlActuator
    {
        /// <summary>
        /// Changes the velocity of the controlled target.
        /// </summary>
        /// <param name="selectedVelocityIndex">
        /// The index of the selected new velocity in the admissible velocity list or -1 to indicate that the
        /// controlled target has to stop immediately.
        /// </param>
        void SelectNewVelocity(int selectedVelocityIndex);

        /// <summary>
        /// Gets the list of the currently admissible velocities of the controlled target.
        /// </summary>
        /// <returns>A list that contains the currently admissible velocities.</returns>
        IEnumerable<RCNumVector> AdmissibleVelocities { get; }
    }

    /// <summary>
    /// Interface for getting informations about the dynamic obstacles in the environment of a motion controlled target
    /// and its currently preferred velocity.
    /// </summary>
    public interface IMotionControlEnvironment
    {
        /// <summary>
        /// Gets the preferred velocity of the controlled target.
        /// </summary>
        RCNumVector PreferredVelocity { get; }

        /// <summary>
        /// Gets the list of the dynamic obstacles in the environment of the controlled target.
        /// </summary>
        /// <returns>A list that contains the dynamic obstacles in the environment of the controlled target.</returns>
        IEnumerable<DynamicObstacleInfo> DynamicObstacles { get; }

        /// <summary>
        /// Checks whether the controlled target remains inside the followed path with the given velocity.
        /// </summary>
        /// <param name="velocity">The velocity to be check.</param>
        /// <returns>True if the given velocity is valid; otherwise false.</returns>
        bool ValidateVelocity(RCNumVector velocity);
    }
}
