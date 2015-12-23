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
    /// Responsible for playing production animation.
    /// </summary>
    public class ProductionAnimationBehavior : EntityBehavior
    {
        /// <summary>
        /// Constructs a ProductionAnimationBehavior with the given parameters.
        /// </summary>
        /// <param name="productionAnimation">The name of the animation to be played when producing.</param>
        /// <param name="normalAnimation">The name of the animation to be played when not producing.</param>
        /// <param name="products">
        /// The list of the products for which to play the production animation or an empty list for playing the animation
        /// for all products.
        /// </param>
        public ProductionAnimationBehavior(string productionAnimation, string normalAnimation, params string[] products)
        {
            if (productionAnimation == null) { throw new ArgumentNullException("productionAnimation"); }
            if (normalAnimation == null) { throw new ArgumentNullException("normalAnimation"); }
            if (products == null) { throw new ArgumentNullException("products"); }

            this.dummyField = this.ConstructField<byte>("dummyField");
            this.productionAnimation = productionAnimation;
            this.normalAnimation = normalAnimation;
            this.products = new RCSet<string>(products);
        }

        #region Overrides

        /// <see cref="EntityBehavior.UpdateMapObject"/>
        public override void UpdateMapObject(Entity entity)
        {
            if (!entity.Biometrics.IsUnderConstruction && !entity.MotionControl.IsFlying)
            {
                if (entity.ActiveProductionLine != null)
                {
                    /// Active production line exists -> check the product filter.
                    if (this.products.Count == 0 ||
                        this.products.Contains(entity.ActiveProductionLine.GetProduct(0).Name))
                    {
                        /// No filter or the filter contains the currently running job -> stop normal animation, start production animation
                        entity.MapObject.StopAnimation(this.normalAnimation);
                        entity.MapObject.StartAnimation(this.productionAnimation);
                    }
                    else
                    {
                        /// The filter doesn't contain the currently running job -> stop production animation, start normal animation.
                        entity.MapObject.StopAnimation(this.productionAnimation);
                        entity.MapObject.StartAnimation(this.normalAnimation);
                    }
                }
                else
                {
                    /// No active production line -> stop production animation, start normal animation.
                    entity.MapObject.StopAnimation(this.productionAnimation);
                    entity.MapObject.StartAnimation(this.normalAnimation);
                }
            }
        }

        #endregion Overrides

        /// <summary>
        /// The name of the animation to be played when producing.
        /// </summary>
        private readonly string productionAnimation;

        /// <summary>
        /// The name of the animation to be played when not producing.
        /// </summary>
        private readonly string normalAnimation;

        /// <summary>
        /// The list of the products for which to play the production animation or an empty list for playing the animation
        /// for all products.
        /// </summary>
        private readonly RCSet<string> products;

        /// <summary>
        /// Dummy heaped value because we are deriving from HeapedObject.
        /// TODO: change HeapedObject to be possible to derive from it without heaped values.
        /// </summary>
        private readonly HeapedValue<byte> dummyField;
    }
}
