using RC.Common;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Behaviors
{
    /// <summary>
    /// Responsible for playing basic animations.
    /// </summary>
    public class BasicAnimationsBehavior : EntityBehavior
    {
        /// <summary>
        /// Constructs a BasicAnimationsBehavior with the given parameters.
        /// </summary>
        /// <param name="movementAnimation">The name of the animation to be played when moving.</param>
        /// <param name="attackAnimation">The name of the animation to be played when attacking.</param>
        /// <param name="normalAnimation">The name of the animation to be played when not moving.</param>
        public BasicAnimationsBehavior(string movementAnimation, string attackAnimation, string normalAnimation)
        {
            if (movementAnimation == null) { throw new ArgumentNullException("movementAnimation"); }
            if (attackAnimation == null) { throw new ArgumentNullException("attackAnimation"); }
            if (normalAnimation == null) { throw new ArgumentNullException("normalAnimation"); }

            this.dummyField = this.ConstructField<byte>("dummyField");
            this.movementAnimation = movementAnimation;
            this.attackAnimation = attackAnimation;
            this.normalAnimation = normalAnimation;
        }

        #region Overrides

        /// <see cref="EntityBehavior.UpdateMapObject"/>
        public override void UpdateMapObject(Entity entity)
        {
            if (entity.MotionControl.VelocityVector.Read() != new RCNumVector(0, 0))
            {
                entity.MapObject.StopAnimation(this.normalAnimation);
                entity.MapObject.StopAnimation(this.attackAnimation);
                entity.MapObject.StartAnimation(this.movementAnimation, entity.MotionControl.VelocityVector, entity.Armour.TargetVector);
            }
            else
            {
                entity.MapObject.StopAnimation(this.movementAnimation);
                if (this.normalAnimation != this.attackAnimation)
                {
                    if (entity.Armour.Target != null)
                    {
                        entity.MapObject.StopAnimation(this.normalAnimation);
                        entity.MapObject.StartAnimation(this.attackAnimation, entity.MotionControl.VelocityVector, entity.Armour.TargetVector);
                    }
                    else
                    {
                        entity.MapObject.StopAnimation(this.attackAnimation);
                        entity.MapObject.StartAnimation(this.normalAnimation, entity.MotionControl.VelocityVector, entity.Armour.TargetVector);
                    }
                }
                else
                {
                    entity.MapObject.StartAnimation(this.normalAnimation, entity.MotionControl.VelocityVector, entity.Armour.TargetVector);
                }
            }
        }

        #endregion Overrides

        /// <summary>
        /// The name of the animation to be played when moving.
        /// </summary>
        private readonly string movementAnimation;

        /// <summary>
        /// The name of the animation to be played when attacking.
        /// </summary>
        private readonly string attackAnimation;

        /// <summary>
        /// The name of the animation to be played when not moving.
        /// </summary>
        private readonly string normalAnimation;

        /// <summary>
        /// Dummy heaped value because we are deriving from HeapedObject.
        /// TODO: change HeapedObject to be possible to derive from it without heaped values.
        /// </summary>
        private readonly HeapedValue<byte> dummyField;
    }
}
