using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.PublicInterfaces
{
    /// <summary>
    /// Common interface of simulation element behaviors.
    /// </summary>
    public interface ISimElemBehavior
    {
        /// <summary>
        /// Checks whether this simulation behavior is currently active or not.
        /// </summary>
        /// <returns>True if this behavior is currently active, false otherwise.</returns>
        bool CheckIfActive();

        /// <summary>
        /// Steps this behavior to the next frame.
        /// </summary>
        void StepNextFrame();
    }
}
