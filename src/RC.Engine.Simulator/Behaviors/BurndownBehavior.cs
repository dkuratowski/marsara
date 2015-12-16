using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Engine.Behaviors
{
    /// <summary>
    /// Responsible for performing burndown effect on buildings.
    /// </summary>
    public class BurndownBehavior : EntityBehavior
    {
        /// <summary>
        /// Constructs a BurndownBehavior with the given parameters.
        /// </summary>
        /// <param name="smallBurnAnimation">The name of the small burn animation to be played.</param>
        /// <param name="heavyBurnAnimation">The name of the heavy burn animation to be played.</param>
        /// <param name="hpPerFrame">The speed of the destruction of the owner entity during the heavy burn.</param>
        public BurndownBehavior(string smallBurnAnimation, string heavyBurnAnimation, RCNumber hpPerFrame)
        {
            if (smallBurnAnimation == null) { throw new ArgumentNullException("smallBurnAnimation"); }
            if (heavyBurnAnimation == null) { throw new ArgumentNullException("heavyBurnAnimation"); }
            if (hpPerFrame < 0) { throw new ArgumentOutOfRangeException("hpPerFrame", "Destruction speed must be non-negative!"); }

            this.hpPerFrame = this.ConstructField<RCNumber>("hpPerFrame");

            this.smallBurnAnimation = smallBurnAnimation;
            this.heavyBurnAnimation = heavyBurnAnimation;
            this.hpPerFrame.Write(hpPerFrame);
        }

        #region Overrides

        /// <see cref="EntityBehavior.UpdateState"/>
        public override void UpdateState(Entity entity)
        {
            if (!entity.Biometrics.IsUnderConstruction && entity.Biometrics.HP <= (RCNumber)entity.ElementType.MaxHP.Read()/(RCNumber)3)
            {
                entity.Biometrics.Damage(hpPerFrame.Read());
            }
        }

        /// <see cref="EntityBehavior.UpdateMapObject"/>
        public override void UpdateMapObject(Entity entity)
        {
            if (entity.Biometrics.IsUnderConstruction)
            {
                entity.MapObject.StopAnimation(this.smallBurnAnimation);
                entity.MapObject.StopAnimation(this.heavyBurnAnimation);
                return;
            }

            if (entity.Biometrics.HP <= (RCNumber)entity.ElementType.MaxHP.Read() / (RCNumber)3)
            {
                entity.MapObject.StopAnimation(this.smallBurnAnimation);
                entity.MapObject.StartAnimation(this.heavyBurnAnimation);
            }
            else if (entity.Biometrics.HP <= 2*(RCNumber) entity.ElementType.MaxHP.Read()/(RCNumber) 3)
            {
                entity.MapObject.StartAnimation(this.smallBurnAnimation);
                entity.MapObject.StopAnimation(this.heavyBurnAnimation);
            }
            else
            {
                entity.MapObject.StopAnimation(this.smallBurnAnimation);
                entity.MapObject.StopAnimation(this.heavyBurnAnimation);
            }
        }

        #endregion Overrides

        /// <summary>
        /// The name of the small burn animation.
        /// </summary>
        private readonly string smallBurnAnimation;

        /// <summary>
        /// The name of the heavy burn animation.
        /// </summary>
        private readonly string heavyBurnAnimation;

        /// <summary>
        /// The speed of the destruction of the owner entity during the heavy burn.
        /// </summary>
        private readonly HeapedValue<RCNumber> hpPerFrame;
    }
}
