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
    /// Responsible for playing construction animation on vespene gas provider buildings.
    /// </summary>
    public class VespeneGasProviderConstructionBehavior : EntityBehavior
    {
        /// <summary>
        /// Constructs a VespeneGasProviderConstructionBehavior with the given sub-behaviors.
        /// </summary>
        /// <param name="depletedAnimations">The construction animations for the depleted state.</param>
        /// <param name="normalAnimations">The construction animations for the normal state.</param>
        public VespeneGasProviderConstructionBehavior(string[] depletedAnimations, string[] normalAnimations)
        {
            if (depletedAnimations == null || depletedAnimations.Length == 0) { throw new ArgumentNullException("depletedAnimations"); }
            if (normalAnimations == null || normalAnimations.Length == 0) { throw new ArgumentNullException("normalAnimations"); }

            this.depletedBehavior = this.ConstructField<ConstructionBehavior>("depletedBehavior");
            this.normalBehavior = this.ConstructField<ConstructionBehavior>("normalBehavior");
            this.allConstructionAnimations = new RCSet<string>();

            /// Collect the depleted animations for constructing the sub-behavior for the depleted state.
            string firstDepletedAnimation = depletedAnimations[0];
            string[] furtherDepletedAnimations = new string[depletedAnimations.Length - 1];
            this.allConstructionAnimations.Add(depletedAnimations[0]);
            for (int i = 1; i < depletedAnimations.Length; i++)
            {
                furtherDepletedAnimations[i - 1] = depletedAnimations[i];
                this.allConstructionAnimations.Add(depletedAnimations[i]);
            }

            /// Collect the normal animations for constructing the sub-behavior for the normal state.
            string firstNormalAnimation = normalAnimations[0];
            string[] furtherNormalAnimations = new string[normalAnimations.Length - 1];
            this.allConstructionAnimations.Add(normalAnimations[0]);
            for (int i = 1; i < normalAnimations.Length; i++)
            {
                furtherNormalAnimations[i - 1] = normalAnimations[i];
                this.allConstructionAnimations.Add(normalAnimations[i]);
            }

            /// Create the sub-behaviors.
            this.depletedBehavior.Write(new ConstructionBehavior(firstDepletedAnimation, furtherDepletedAnimations));
            this.normalBehavior.Write(new ConstructionBehavior(firstNormalAnimation, furtherNormalAnimations));
        }

        #region Overrides

        /// <see cref="EntityBehavior.UpdateMapObject"/>
        public override void UpdateMapObject(Entity entity)
        {
            IResourceProvider entityAsResourceProvider = entity as IResourceProvider;
            if (entityAsResourceProvider == null) { throw new InvalidOperationException("VespeneGasProviderConstructionBehavior can only be used for Entities implementing the IResourceProvider interface!"); }
            if (entityAsResourceProvider.VespeneGasAmount == -1) { throw new InvalidOperationException("VespeneGasProviderConstructionBehavior can only be used for Entities providing vespene gas!"); }

            if (entity.Biometrics.IsUnderConstruction)
            {
                /// Entity is under construction -> play the animation that belongs to the current construction progress and state.
                if (entityAsResourceProvider.VespeneGasAmount > 0)
                {
                    this.normalBehavior.Read().UpdateMapObject(entity);
                }
                else
                {
                    this.depletedBehavior.Read().UpdateMapObject(entity);
                }
            }
            else
            {
                /// Entity is not under construction -> stop every construction animations.
                this.StopStartAnimations(entity, this.allConstructionAnimations, new RCSet<string>());
            }
        }

        #endregion Overrides

        /// <summary>
        /// The sub-behavior for the depleted state.
        /// </summary>
        private readonly HeapedValue<ConstructionBehavior> depletedBehavior;

        /// <summary>
        /// The sub-behavior for the normal state.
        /// </summary>
        private readonly HeapedValue<ConstructionBehavior> normalBehavior;

        /// <summary>
        /// The list of all construction animations.
        /// </summary>
        private readonly RCSet<string> allConstructionAnimations;
    }
}
