using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Metadata.Core
{
    /// <summary>
    /// Contains the definition of a missile type.
    /// </summary>
    class MissileType : ScenarioElementType, IMissileType
    {
        /// <summary>
        /// Constructs a new missile type.
        /// </summary>
        /// <param name="name">The name of this missile type.</param>
        /// <param name="metadata">The metadata object that this missile type belongs to.</param>
        public MissileType(string name, ScenarioMetadata metadata)
            : base(name, metadata)
        {
            this.launchAnimation = null;
            this.launchDelay = 0;
            this.flyingAnimation = null;
            this.trailAnimation = null;
            this.trailAnimationFrequency = 0;
            this.impactAnimation = null;
        }

        #region IMissileType members

        /// <see cref="IMissileType.LaunchAnimation"/>
        public string LaunchAnimation { get { return this.launchAnimation; } }

        /// <see cref="IMissileType.LaunchDelay"/>
        public int LaunchDelay { get { return this.launchDelay; } }

        /// <see cref="IMissileType.FlyingAnimation"/>
        public string FlyingAnimation { get { return this.flyingAnimation; } }

        /// <see cref="IMissileType.TrailAnimation"/>
        public string TrailAnimation { get { return this.trailAnimation; } }

        /// <see cref="IMissileType.TrailAnimationFrequency"/>
        public int TrailAnimationFrequency { get { return this.trailAnimationFrequency; } }

        /// <see cref="IMissileType.ImpactAnimation"/>
        public string ImpactAnimation { get { return this.impactAnimation; } }

        #endregion IMissileType members

        #region MissileType buildup methods

        /// <summary>
        /// Sets the name of the launch animation and the delay of the launch of this missile type.
        /// </summary>
        public void SetLaunchAnimation(string launchAnimationName, int launchDelay)
        {
            if (this.Metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (launchAnimationName == null) { throw new ArgumentNullException("launchAnimationName"); }
            if (launchDelay < 0) { throw new ArgumentOutOfRangeException("launchDelay"); }

            this.launchAnimation = launchAnimationName;
            this.launchDelay = launchDelay;
        }

        /// <summary>
        /// Sets the name of the flying animation of this missile type.
        /// </summary>
        public void SetFlyingAnimation(string flyingAnimationName)
        {
            if (this.Metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (flyingAnimationName == null) { throw new ArgumentNullException("flyingAnimationName"); }

            this.flyingAnimation = flyingAnimationName;
        }

        /// <summary>
        /// Sets the name and the frequency of the trail animation of this missile type.
        /// </summary>
        public void SetTrailAnimation(string trailAnimationName, int trailAnimationFrequency)
        {
            if (this.Metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (trailAnimationName == null) { throw new ArgumentNullException("trailAnimationName"); }
            if (trailAnimationFrequency <= 0) { throw new ArgumentOutOfRangeException("trailAnimationFrequency"); }

            this.trailAnimation = trailAnimationName;
            this.trailAnimationFrequency = trailAnimationFrequency;
        }

        /// <summary>
        /// Sets the name of the impact animation of this missile type.
        /// </summary>
        public void SetImpactAnimation(string impactAnimationName)
        {
            if (this.Metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (impactAnimationName == null) { throw new ArgumentNullException("impactAnimationName"); }

            this.impactAnimation = impactAnimationName;
        }

        #endregion MissileType buildup methods

        /// <summary>
        /// The name of the animation to be rendered when launching this type of missiles or null if no such animation has been defined.
        /// </summary>
        private string launchAnimation;

        /// <summary>
        /// The delay of the launch of this type of missiles in frames from the beginning of the launch animation, or 0 if no
        /// launch animation has been defined or the missile shall be launched immediately.
        /// </summary>
        private int launchDelay;

        /// <summary>
        /// The name of the animation to be rendered when this type of missiles are flying or null if no such animation has been defined.
        /// </summary>
        private string flyingAnimation;

        /// <summary>
        /// The name of the animation to be rendered as the trail of this type of missiles during flying or null if no such animation has been defined.
        /// </summary>
        private string trailAnimation;

        /// <summary>
        /// The number of frames between rendering trail animations or -1 if no trail animation has been defined.
        /// </summary>
        private int trailAnimationFrequency;

        /// <summary>
        /// The name of the animation to be rendered when this type of missiles impacts their target or null if no such animation has been defined.
        /// </summary>
        private string impactAnimation;
    }
}
