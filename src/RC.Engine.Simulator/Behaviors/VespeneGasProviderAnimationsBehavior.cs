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
    /// Responsible for playing animations of vespene gas provider entities.
    /// </summary>
    public class VespeneGasProviderAnimationsBehavior : EntityBehavior
    {
        /// <summary>
        /// Constructs a VespeneGasProviderAnimationsBehavior with the given parameters.
        /// </summary>
        /// <param name="depletedAnimation">The name of the animation to be played in depleted state.</param>
        /// <param name="normalAnimation">The name of the animation to be played in normal state.</param>
        public VespeneGasProviderAnimationsBehavior(string depletedAnimation, string normalAnimation)
        {
            if (depletedAnimation == null) { throw new ArgumentNullException("depletedAnimation"); }
            if (normalAnimation == null) { throw new ArgumentNullException("normalAnimation"); }

            this.dummyField = this.ConstructField<byte>("dummyField");
            this.depletedAnimation = new RCSet<string> { depletedAnimation };
            this.normalAnimation = new RCSet<string> { normalAnimation };
        }

        #region Overrides

        /// <see cref="EntityBehavior.UpdateMapObject"/>
        public override void UpdateMapObject(Entity entity)
        {
            /// Check if the entity is a vespene gas provider.
            IResourceProvider entityAsResourceProvider = entity as IResourceProvider;
            if (entityAsResourceProvider == null) { throw new InvalidOperationException("VespeneGasProviderConstructionBehavior can only be used for Entities implementing the IResourceProvider interface!"); }
            if (entityAsResourceProvider.VespeneGasAmount == -1) { throw new InvalidOperationException("VespeneGasProviderConstructionBehavior can only be used for Entities providing vespene gas!"); }

            /// Do nothing while under construction.
            if (entity.Biometrics.IsUnderConstruction) { return; }

            /// Entity is under construction -> play the animation that belongs to the current construction progress and state.
            if (entityAsResourceProvider.VespeneGasAmount > 0)
            {
                this.StopStartAnimations(entity, this.depletedAnimation, this.normalAnimation);
            }
            else
            {
                this.StopStartAnimations(entity, this.normalAnimation, this.depletedAnimation);
            }
        }

        #endregion Overrides

        /// <summary>
        /// The name of the animation to be played in depleted state.
        /// </summary>
        private readonly RCSet<string> depletedAnimation;

        /// <summary>
        /// The name of the animation to be played in normal state.
        /// </summary>
        private readonly RCSet<string> normalAnimation;

        /// <summary>
        /// Dummy heaped value because we are deriving from HeapedObject.
        /// TODO: change HeapedObject to be possible to derive from it without heaped values.
        /// </summary>
        private readonly HeapedValue<byte> dummyField;
    }
}
