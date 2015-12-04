using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Metadata.Core
{
    /// <summary>
    /// Internal interface of the missile types defined in the metadata.
    /// </summary>
    interface IMissileTypeInternal : IScenarioElementTypeInternal
    {
        /// <summary>
        /// Gets the name of the animation to be rendered when launching this type of missiles or null if no such animation has been defined.
        /// </summary>
        string LaunchAnimation { get; }

        /// <summary>
        /// Gets the delay of the launch of this type of missiles in frames from the beginning of the launch animation, or 0 if no
        /// launch animation has been defined or the missile shall be launched immediately.
        /// </summary>
        int LaunchDelay { get; }

        /// <summary>
        /// Gets the name of the animation to be rendered when this type of missiles are flying or null if no such animation has been defined.
        /// </summary>
        string FlyingAnimation { get; }

        /// <summary>
        /// Gets the name of the animation to be rendered as the trail of this type of missiles during flying or null if no such animation has been defined.
        /// </summary>
        string TrailAnimation { get; }

        /// <summary>
        /// Gets the number of frames between rendering trail animations or 0 if no trail animation has been defined.
        /// </summary>
        int TrailAnimationFrequency { get; }

        /// <summary>
        /// Gets the name of the animation to be rendered when this type of missiles impacts their target or null if no such animation has been defined.
        /// </summary>
        string ImpactAnimation { get; }
    }
}
