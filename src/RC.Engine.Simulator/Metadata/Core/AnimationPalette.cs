using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Metadata.Core
{
    /// <summary>
    /// Represents the animation palettes of the elements of the metadata.
    /// </summary>
    class AnimationPalette : IAnimationPalette
    {
        /// <summary>Constructs an animation palette.</summary>
        /// <param name="metadata">The metadata object that this sprite palette belongs to.</param>
        public AnimationPalette(ScenarioMetadata metadata)
        {
            if (metadata == null) { throw new ArgumentNullException("metadata"); }
            this.animations = new Dictionary<string, Animation>();
            this.previewAnimations = new List<Animation>();
            this.metadata = metadata;
        }

        #region IAnimationPalette members

        /// <see cref="IAnimationPalette.PreviewAnimation"/>
        public Animation PreviewAnimation
        {
            get
            {
                return this.previewAnimations.Count != 0 ?
                    this.previewAnimations[RandomService.DefaultGenerator.Next(this.previewAnimations.Count)] :
                    null;
            }
        }

        /// <see cref="IAnimationPalette.GetAnimation"/>
        public Animation GetAnimation(string animationName)
        {
            if (animationName == null) { throw new ArgumentNullException("animationName"); }
            if (!this.animations.ContainsKey(animationName)) { throw new SimulatorException(string.Format("Animation with name '{0}' doesn't exist!", animationName)); }
            return this.animations[animationName];
        }

        #endregion IAnimationPalette members

        /// <summary>
        /// Checks and finalizes the animation palette object. Buildup methods will be unavailable after
        /// calling this method.
        /// </summary>
        public void CheckAndFinalize()
        {
            if (!this.metadata.IsFinalized)
            {

            }
        }

        /// <summary>
        /// Adds an animation to this animation palette.
        /// </summary>
        /// <param name="name">The name of the animation to add.</param>
        /// <param name="animation">The animation to add.</param>
        /// <param name="isPreview">Flag indicating whether the animation can be used as a preview or not.</param>
        public void AddAnimation(string name, Animation animation, bool isPreview)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (name == null) { throw new ArgumentNullException("name"); }
            if (animation == null) { throw new ArgumentNullException("animation"); }
            if (this.animations.ContainsKey(name)) { throw new SimulatorException(string.Format("Animation with name '{0}' already exists!", name)); }

            this.animations.Add(name, animation);
            if (isPreview) { this.previewAnimations.Add(animation); }
        }

        /// <summary>
        /// List of the animations in this palette mapped by their names.
        /// </summary>
        private Dictionary<string, Animation> animations;

        /// <summary>
        /// List of the preview animations.
        /// </summary>
        private List<Animation> previewAnimations;

        /// <summary>
        /// Reference to the metadata object that this sprite palette belongs to.
        /// </summary>
        private ScenarioMetadata metadata;
    }
}
