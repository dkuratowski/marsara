using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// Responsible for playing the appropriate animations for the liftoff and landing of entities with such capabilities.
    /// </summary>
    public class LiftoffBehavior : EntityBehavior
    {
        /// <summary>
        /// Constructs a LiftoffBehavior with the given parameters.
        /// </summary>
        /// <param name="onGroundAnimation">The name of the animation to be played when the entity is on ground.</param>
        /// <param name="liftoffAnimation">The name of the animation to be played during liftoff.</param>
        /// <param name="flyingAnimation">The name of the animation to be played when the entity is flying.</param>
        /// <param name="landingAnimation">The name of the animation to be played during landing.</param>
        public LiftoffBehavior(string onGroundAnimation, string liftoffAnimation, string flyingAnimation, string landingAnimation)
        {
            if (onGroundAnimation == null) { throw new ArgumentNullException("onGroundAnimation"); }
            if (liftoffAnimation == null) { throw new ArgumentNullException("liftoffAnimation"); }
            if (flyingAnimation == null) { throw new ArgumentNullException("flyingAnimation"); }
            if (landingAnimation == null) { throw new ArgumentNullException("landingAnimation"); }

            this.dummyField = this.ConstructField<byte>("dummyField");
            this.onGroundAnimation = onGroundAnimation;
            this.liftoffAnimation = liftoffAnimation;
            this.flyingAnimation = flyingAnimation;
            this.landingAnimation = landingAnimation;
        }

        #region Overrides

        /// <see cref="EntityBehavior.UpdateMapObject"/>
        public override void UpdateMapObject(Entity entity)
        {
            if (!entity.MotionControl.IsFlying)
            {
                entity.MapObject.StopAnimation(this.landingAnimation);
                entity.MapObject.StartAnimation(this.onGroundAnimation);
            }
            else if (entity.MotionControl.Status == MotionControlStatusEnum.TakingOff)
            {
                entity.MapObject.StopAnimation(this.onGroundAnimation);
                entity.MapObject.StartAnimation(this.liftoffAnimation);
            }
            else if (entity.MotionControl.Status == MotionControlStatusEnum.InAir)
            {
                entity.MapObject.StopAnimation(this.liftoffAnimation);
                entity.MapObject.StartAnimation(this.flyingAnimation);
            }
            else if (entity.MotionControl.Status == MotionControlStatusEnum.Landing)
            {
                entity.MapObject.StopAnimation(this.flyingAnimation);
                entity.MapObject.StartAnimation(this.landingAnimation);
            }
        }

        #endregion Overrides

        /// <summary>
        /// The name of the animation to be played when the entity is on ground.
        /// </summary>
        private readonly string onGroundAnimation;

        /// <summary>
        /// The name of the animation to be played during liftoff.
        /// </summary>
        private readonly string liftoffAnimation;

        /// <summary>
        /// The name of the animation to be played when the entity is flying.
        /// </summary>
        private readonly string flyingAnimation;

        /// <summary>
        /// The name of the animation to be played during landing.
        /// </summary>
        private readonly string landingAnimation;

        /// <summary>
        /// Dummy heaped value because we are deriving from HeapedObject.
        /// TODO: change HeapedObject to be possible to derive from it without heaped values.
        /// </summary>
        private readonly HeapedValue<byte> dummyField;
    }
}
