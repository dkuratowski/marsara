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
            this.flyingAnimation = null;
            this.trailAnimation = null;
            this.trailAnimationFrequency = -1;
            this.impactAnimation = null;
        }

        #region IMissileType members

        /// <see cref="IMissileType.LaunchAnimation"/>
        public Animation LaunchAnimation { get { return this.launchAnimation; } }

        /// <see cref="IMissileType.FlyingAnimation"/>
        public Animation FlyingAnimation { get { return this.flyingAnimation; } }

        /// <see cref="IMissileType.TrailAnimation"/>
        public Animation TrailAnimation { get { return this.trailAnimation; } }

        /// <see cref="IMissileType.TrailAnimationFrequency"/>
        public int TrailAnimationFrequency { get { return this.trailAnimationFrequency; } }

        /// <see cref="IMissileType.ImpactAnimation"/>
        public Animation ImpactAnimation { get { return this.impactAnimation; } }

        #endregion IMissileType members

        #region MissileType buildup methods

        /// <summary>
        /// Sets the name of the launch animation of this missile type.
        /// </summary>
        public void SetLaunchAnimation(string launchAnimationName)
        {
            if (this.Metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (launchAnimationName == null) { throw new ArgumentNullException("launchAnimationName"); }

            this.launchAnimation = this.AnimationPalette.GetAnimation(launchAnimationName);
        }

        /// <summary>
        /// Sets the name of the flying animation of this missile type.
        /// </summary>
        public void SetFlyingAnimation(string flyingAnimationName)
        {
            if (this.Metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (flyingAnimationName == null) { throw new ArgumentNullException("flyingAnimationName"); }

            this.flyingAnimation = this.AnimationPalette.GetAnimation(flyingAnimationName);
        }

        /// <summary>
        /// Sets the name and the frequency of the trail animation of this missile type.
        /// </summary>
        public void SetTrailAnimation(string trailAnimationName, int trailAnimationFrequency)
        {
            if (this.Metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (trailAnimationName == null) { throw new ArgumentNullException("trailAnimationName"); }
            if (trailAnimationFrequency <= 0) { throw new ArgumentOutOfRangeException("trailAnimationFrequency"); }

            this.trailAnimation = this.AnimationPalette.GetAnimation(trailAnimationName);
            this.trailAnimationFrequency = trailAnimationFrequency;
        }

        /// <summary>
        /// Sets the name of the impact animation of this missile type.
        /// </summary>
        public void SetImpactAnimation(string impactAnimationName)
        {
            if (this.Metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (impactAnimationName == null) { throw new ArgumentNullException("impactAnimationName"); }

            this.impactAnimation = this.AnimationPalette.GetAnimation(impactAnimationName);
        }

        #endregion MissileType buildup methods

        /// <summary>
        /// The animation to be rendered when launching this type of missiles or null if no such animation has been defined.
        /// </summary>
        private Animation launchAnimation;

        /// <summary>
        /// The animation to be rendered when this type of missiles are flying or null if no such animation has been defined.
        /// </summary>
        private Animation flyingAnimation;

        /// <summary>
        /// The animation to be rendered as the trail of this type of missiles during flying or null if no such animation has been defined.
        /// </summary>
        private Animation trailAnimation;

        /// <summary>
        /// The number of frames between rendering trail animations or -1 if no trail animation has been defined.
        /// </summary>
        private int trailAnimationFrequency;

        /// <summary>
        /// The animation to be rendered when this type of missiles impacts their target or null if no such animation has been defined.
        /// </summary>
        private Animation impactAnimation;
    }
}
