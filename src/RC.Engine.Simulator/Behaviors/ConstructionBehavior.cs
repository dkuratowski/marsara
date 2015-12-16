using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Engine.Behaviors
{
    /// <summary>
    /// Responsible for playing construction animation on buildings and addons.
    /// </summary>
    public class ConstructionBehavior : EntityBehavior
    {
        /// <summary>
        /// Constructs a ConstructionBehavior with the given parameters.
        /// </summary>
        /// <param name="constructionAnimation">The name of the construction animation to be played.</param>
        /// <param name="afterConstructionAnimation">The name of the animation to be played when the construction is completed.</param>
        public ConstructionBehavior(string constructionAnimation, string afterConstructionAnimation)
        {
            if (constructionAnimation == null) { throw new ArgumentNullException("constructionAnimation"); }
            if (afterConstructionAnimation == null) { throw new ArgumentNullException("afterConstructionAnimation"); }

            this.dummyField = this.ConstructField<byte>("dummyField");
            this.constructionAnimation = constructionAnimation;
            this.afterConstructionAnimation = afterConstructionAnimation;
        }

        #region Overrides

        /// <see cref="EntityBehavior.UpdateMapObject"/>
        public override void UpdateMapObject(Entity entity)
        {
            if (entity.Biometrics.IsUnderConstruction)
            {
                entity.MapObject.StopAnimation(this.afterConstructionAnimation);
                entity.MapObject.StartAnimation(this.constructionAnimation);
            }
            else
            {
                entity.MapObject.StopAnimation(this.constructionAnimation);
                entity.MapObject.StartAnimation(this.afterConstructionAnimation);
            }
        }

        #endregion Overrides

        /// <summary>
        /// The name of the construction animation.
        /// </summary>
        private readonly string constructionAnimation;

        /// <summary>
        /// The name of the animation to be played when the construction is completed.
        /// </summary>
        private readonly string afterConstructionAnimation;

        /// <summary>
        /// Dummy heaped value because we are deriving from HeapedObject.
        /// TODO: change HeapedObject to be possible to derive from it without heaped values.
        /// </summary>
        private readonly HeapedValue<byte> dummyField;
    }
}
