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
    }
}
