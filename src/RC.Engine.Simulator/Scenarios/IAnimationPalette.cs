using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Scenarios
{
    /// <summary>
    /// Interface of the animation palette of scenario element types.
    /// </summary>
    public interface IAnimationPalette
    {
        /// <summary>
        /// Gets one of the preview animations from the animation palette randomly or null if there is
        /// no preview animation in the animation palette.
        /// </summary>
        Animation PreviewAnimation { get; }

        /// <summary>
        /// Gets the animation with the given name from the animation palette.
        /// </summary>
        /// <param name="animationName">The name of the animation to get.</param>
        /// <returns>The animation with the given name.</returns>
        Animation GetAnimation(string animationName);
    }
}
