using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Behaviors
{
    /// <summary>
    /// Responsible for playing construction animation on buildings and addons.
    /// </summary>
    public class ConstructionBehavior : EntityBehavior
    {
        /// <summary>
        /// Constructs a ConstructionBehavior with the given animations.
        /// </summary>
        /// <param name="firstAnimation">The name of the first construction animation to be played.</param>
        /// <param name="furtherAnimations">The name of the further construction animation to be played.</param>
        public ConstructionBehavior(string firstAnimation, params string[] furtherAnimations)
        {
            if (firstAnimation == null) { throw new ArgumentNullException("firstAnimation"); }
            if (furtherAnimations == null) { throw new ArgumentNullException("furtherAnimations"); }

            this.dummyField = this.ConstructField<byte>("dummyField");

            this.constructionAnimations = new string[furtherAnimations.Length + 1];
            this.constructionAnimations[0] = firstAnimation;
            for (int i = 0; i < furtherAnimations.Length; i++)
            {
                this.constructionAnimations[i + 1] = furtherAnimations[i];
            }
        }

        #region Overrides

        /// <see cref="EntityBehavior.UpdateMapObject"/>
        public override void UpdateMapObject(Entity entity)
        {
            if (entity.Biometrics.IsUnderConstruction)
            {
                /// Entity is under construction -> play the animation that belongs to the current construction progress.
                RCNumber progressPerIndex = (RCNumber) entity.ElementType.BuildTime.Read()
                                          / (RCNumber) (this.constructionAnimations.Length);
                this.PlayConstructionAnimation(entity, (int)(entity.Biometrics.ConstructionProgress / progressPerIndex));
            }
            else
            {
                /// Entity is not under construction -> stop every construction animations.
                this.StopStartAnimations(entity, new RCSet<string>(this.constructionAnimations), new RCSet<string>());
            }
        }

        #endregion Overrides

        /// <summary>
        /// Starts the construction animation at the given index and stops playing every other construction animation.
        /// </summary>
        /// <param name="entity">The target entity.</param>
        /// <param name="index">The index of the construction animation to be played.</param>
        private void PlayConstructionAnimation(Entity entity, int index)
        {
            RCSet<string> animationsToStop = new RCSet<string>();
            RCSet<string> animationsToStart = new RCSet<string>() { this.constructionAnimations[index] };
            for (int i = 0; i < this.constructionAnimations.Length; i++)
            {
                if (i != index) { animationsToStop.Add(this.constructionAnimations[i]); }
            }
            this.StopStartAnimations(entity, animationsToStop, animationsToStart);
        }

        /// <summary>
        /// The names of the construction animations to be played.
        /// </summary>
        private readonly string[] constructionAnimations;

        /// <summary>
        /// Dummy heaped value because we are deriving from HeapedObject.
        /// TODO: change HeapedObject to be possible to derive from it without heaped values.
        /// </summary>
        private readonly HeapedValue<byte> dummyField;
    }
}
