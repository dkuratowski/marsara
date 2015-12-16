using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Engine.Behaviors
{
    /// <summary>
    /// Implements additional addon-specific logic for Addons.
    /// </summary>
    public class AddonBehavior : EntityBehavior
    {
        /// <summary>
        /// Constructs an AddonBehavior instance.
        /// </summary>
        public AddonBehavior(string onlineAnimation, string offlineAnimation)
        {
            if (onlineAnimation == null) { throw new ArgumentNullException("onlineAnimation"); }
            if (offlineAnimation == null) { throw new ArgumentNullException("offlineAnimation"); }

            this.dummyField = this.ConstructField<byte>("dummyField");
            this.onlineAnimation = onlineAnimation;
            this.offlineAnimation = offlineAnimation;
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
            }
            else
            {
                /// If the addon has no main building, then it has to be neutral.
                if (addon.Owner != null) { addon.Owner.RemoveAddon(addon); }
            }
        }

        /// <see cref="EntityBehavior.UpdateMapObject"/>
        public override void UpdateMapObject(Entity entity)
        {
            Addon addon = (Addon)entity;

            /// Do nothing while under construction.
            if (addon.Biometrics.IsUnderConstruction) { return; }

            /// Play online or offline animation depending on the current state.
            if (addon.CurrentMainBuilding != null)
            {
                addon.MapObject.StopAnimation(this.offlineAnimation);
                addon.MapObject.StartAnimation(this.onlineAnimation);
            }
            else
            {
                addon.MapObject.StopAnimation(this.onlineAnimation);
                addon.MapObject.StartAnimation(this.offlineAnimation);
            }
        }

        #endregion Overrides

        /// <summary>
        /// The name of the animation to be played when online.
        /// </summary>
        private readonly string onlineAnimation;

        /// <summary>
        /// The name of the animation to be played when offline.
        /// </summary>
        private readonly string offlineAnimation;

        /// <summary>
        /// Dummy heaped value because we are deriving from HeapedObject.
        /// TODO: change HeapedObject to be possible to derive from it without heaped values.
        /// </summary>
        private HeapedValue<byte> dummyField;
    }
}
