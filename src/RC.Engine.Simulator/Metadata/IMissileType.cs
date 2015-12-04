using RC.Engine.Simulator.Metadata.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Metadata
{
    /// <summary>
    /// Interface of the missile types defined in the metadata.
    /// This interface is defined as a class because we want to overload the equality/inequality operators.
    /// </summary>
    public class IMissileType : IScenarioElementType
    {
        #region Interface methods

        /// <summary>
        /// Gets the name of the animation to be rendered when launching this type of missiles or null if no such animation has been defined.
        /// </summary>
        public string LaunchAnimation { get { return this.implementation.LaunchAnimation; } }

        /// <summary>
        /// Gets the delay of the launch of this type of missiles in frames from the beginning of the launch animation, or 0 if no
        /// launch animation has been defined or the missile shall be launched immediately.
        /// </summary>
        public int LaunchDelay { get { return this.implementation.LaunchDelay; } }

        /// <summary>
        /// Gets the name of the animation to be rendered when this type of missiles are flying or null if no such animation has been defined.
        /// </summary>
        public string FlyingAnimation { get { return this.implementation.FlyingAnimation; } }

        /// <summary>
        /// Gets the name of the animation to be rendered as the trail of this type of missiles during flying or null if no such animation has been defined.
        /// </summary>
        public string TrailAnimation { get { return this.implementation.TrailAnimation; } }

        /// <summary>
        /// Gets the number of frames between rendering trail animations or 0 if no trail animation has been defined.
        /// </summary>
        public int TrailAnimationFrequency { get { return this.implementation.TrailAnimationFrequency; } }

        /// <summary>
        /// Gets the name of the animation to be rendered when this type of missiles impacts their target or null if no such animation has been defined.
        /// </summary>
        public string ImpactAnimation { get { return this.implementation.ImpactAnimation; } }

        #endregion Interface methods

        /// <summary>
        /// Constructs an instance of this interface with the given implementation.
        /// </summary>
        /// <param name="implementation">The implementation of this interface.</param>
        internal IMissileType(IMissileTypeInternal implementation) : base(implementation)
        {
            if (implementation == null) { throw new ArgumentNullException("implementation"); }
            this.implementation = implementation;
        }

        /// <summary>
        /// Gets the implementation of this interface.
        /// </summary>
        internal IMissileTypeInternal MissileTypeImpl { get { return this.implementation; } }

        /// <summary>
        /// Reference to the implementation of this interface.
        /// </summary>
        private IMissileTypeInternal implementation;
    }
}
