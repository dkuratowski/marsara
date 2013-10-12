using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Simulator.PublicInterfaces
{
    /// <summary>
    /// Interface for controlling the state of a simulation element indicator on the map.
    /// </summary>
    public interface ISimElemIndicator
    {
        /// <summary>
        /// Selects an animation.
        /// </summary>
        /// <param name="name">The name of the animation to select.</param>
        /// <returns>The total number of frames in the selected animation.</returns>
        /// <exception cref="SimulatorException">If no animation exists with the given name.</exception>
        /// <remarks>
        /// You have to call ISimElemIndicator.StepCurrentAnimation if you want to display the first frame
        /// of the selected animation.
        /// </remarks>
        int SelectAnimation(string name);

        /// <summary>
        /// Steps the currently selected animation to its next frame.
        /// </summary>
        /// <returns>The number of remaining frames in the selected animation.</returns>
        /// <remarks>
        /// If the end of the animation has been reached then this method has no effect as the last frame
        /// will remain displayed.
        /// </remarks>
        int StepCurrentAnimation();

        /// <summary>
        /// Sets the position of the indicator on the map.
        /// </summary>
        /// <param name="newPosition">The new position of the indicator on the map.</param>
        void SetPosition(RCNumRectangle newPosition);
    }
}
