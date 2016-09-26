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
    /// Implements additional addon-specific logic for Addons.
    /// </summary>
    public class AddonBehavior : EntityBehavior
    {
        /// <summary>
        /// Constructs an AddonBehavior instance.
        /// </summary>
        public AddonBehavior(string goingOnlineAnimation, string startingProductionAnimation, string stoppingProductionAnimation, string goingOfflineAnimation)
        {
            if (goingOnlineAnimation == null) { throw new ArgumentNullException("goingOnlineAnimation"); }
            if (startingProductionAnimation == null) { throw new ArgumentNullException("startingProductionAnimation"); }
            if (stoppingProductionAnimation == null) { throw new ArgumentNullException("stoppingProductionAnimation"); }
            if (goingOfflineAnimation == null) { throw new ArgumentNullException("goingOfflineAnimation"); }

            this.hasProducedSinceOnline = this.ConstructField<byte>("hasProducedSinceOnline");
            this.hasProducedSinceOnline.Write(0x00);
            this.goingOnlineAnimation = goingOnlineAnimation;
            this.startingProductionAnimation = startingProductionAnimation;
            this.stoppingProductionAnimation = stoppingProductionAnimation;
            this.goingOfflineAnimation = goingOfflineAnimation;
        }

        #region Overrides

        /// <see cref="EntityBehavior.UpdateState"/>
        public override void UpdateState(Entity entity)
        {
            Addon addon = (Addon)entity;

            /// Cancel the construction of the addon if its main building has been destroyed in the meantime.
            if (addon.Biometrics.IsUnderConstruction && addon.CurrentMainBuilding == null)
            {
                addon.Biometrics.CancelConstruct();
            }

            if (addon.CurrentMainBuilding != null)
            {
                /// If the addon has a main building, then synchronize their owners.
                Building mainBuilding = addon.CurrentMainBuilding;
                if (addon.Owner == null && mainBuilding.Owner != null)
                {
                    mainBuilding.Owner.AddAddon(addon);
                }
                else if (addon.Owner != null && mainBuilding.Owner == null)
                {
                    addon.Owner.RemoveAddon(addon);
                }
                else if (addon.Owner != null && mainBuilding.Owner != null && addon.Owner != mainBuilding.Owner)
                {
                    addon.Owner.RemoveAddon(addon);
                    mainBuilding.Owner.AddAddon(addon);
                }

                if (addon.ActiveProductionLine != null) { this.hasProducedSinceOnline.Write(0x01); }
            }
            else
            {
                /// If the addon has no main building, then it has to be neutral.
                if (addon.Owner != null) { addon.Owner.RemoveAddon(addon); }
                this.hasProducedSinceOnline.Write(0x00);
            }
        }

        /// <see cref="EntityBehavior.UpdateMapObject"/>
        public override void UpdateMapObject(Entity entity)
        {
            Addon addon = (Addon)entity;

            /// Do nothing while under construction.
            if (addon.Biometrics.IsUnderConstruction) { return; }

            /// Play the appropriate animation depending on the current state of the addon.
            if (addon.CurrentMainBuilding != null)
            {
                if (addon.ActiveProductionLine != null)
                {
                    /// Starting production.
                    this.StopStartAnimations(addon,
                        new RCSet<string> { this.goingOfflineAnimation, this.goingOnlineAnimation, this.stoppingProductionAnimation },
                        new RCSet<string> { this.startingProductionAnimation });
                }
                else if (this.hasProducedSinceOnline.Read() == 0x01)
                {
                    /// Stopping production.
                    this.StopStartAnimations(addon,
                        new RCSet<string> { this.goingOfflineAnimation, this.goingOnlineAnimation, this.startingProductionAnimation },
                        new RCSet<string> { this.stoppingProductionAnimation });
                }
                else
                {
                    /// Going online.
                    this.StopStartAnimations(addon,
                        new RCSet<string> { this.goingOfflineAnimation, this.stoppingProductionAnimation, this.startingProductionAnimation },
                        new RCSet<string> { this.goingOnlineAnimation });
                }
            }
            else
            {
                /// Going offline.
                this.StopStartAnimations(addon,
                    new RCSet<string> { this.goingOnlineAnimation, this.stoppingProductionAnimation, this.startingProductionAnimation },
                    new RCSet<string> { this.goingOfflineAnimation });
            }
        }

        #endregion Overrides

        /// <summary>
        /// The name of the animation to be played when going online.
        /// </summary>
        private readonly string goingOnlineAnimation;
        
        /// <summary>
        /// The name of the animation to be played when going offline.
        /// </summary>
        private readonly string goingOfflineAnimation;

        /// <summary>
        /// The name of the animation to be played when starting production.
        /// </summary>
        private readonly string startingProductionAnimation;

        /// <summary>
        /// The name of the animation to be played when stopping production.
        /// </summary>
        private readonly string stoppingProductionAnimation;

        /// <summary>
        /// This flag indicates whether the addon has already produced since it is online (0x01) or not (0x00).
        /// </summary>
        private HeapedValue<byte> hasProducedSinceOnline;
    }
}
