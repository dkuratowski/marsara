using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.Behaviors;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Terran.Behaviors;

namespace RC.Engine.Simulator.Terran.Units
{
    /// <summary>
    /// Represents a Terran Marine.
    /// </summary>
    class Marine : Unit
    {
        /// <summary>
        /// Constructs a Terran Marine instance.
        /// </summary>
        public Marine()
            : base(MARINE_TYPE_NAME, false, new BasicAnimationsBehavior("Walking", "Shooting", "Standing"), new StimPacksBehavior())
        {
            this.remainingStimPacksTime = this.ConstructField<int>("remainingStimPacksTime");
            this.remainingStimPacksTime.Write(0);
        }

        /// <summary>
        /// Activates the StimPacks for this Marine.
        /// </summary>
        public void ActivateStimPacks()
        {
            /// Check if this Marine has enough HP to apply StimPacks.
            if (this.Biometrics.HP <= 10)
            {
                /// TODO: send a message to the user that this Marine has not enough HP to activate the StimPacks.
                return;
            }

            /// Apply the constant damage caused by the StimPacks.
            this.Biometrics.Damage(TerranAbilities.STIMPACKS_DAMAGE);

            /// If StimPacks is currently inactive -> apply the effect of StimPacks.
            if (this.remainingStimPacksTime.Read() == 0)
            {
                this.ElementTypeUpgrade.SpeedUpgrade = TerranAbilities.STIMPACKS_SPEED_UPGRADE;
                foreach (IWeaponDataUpgrade weaponDataUpgrade in this.ElementTypeUpgrade.WeaponUpgrades)
                {
                    weaponDataUpgrade.CooldownUpgrade = TerranAbilities.STIMPACKS_COOLDOWN_UPGRADE;
                }
            }

            /// Reset the StimPacks timer.
            this.remainingStimPacksTime.Write(TerranAbilities.STIMPACKS_TIME);
        }

        /// <summary>
        /// Updates the status of the StimPacks for this Marine.
        /// </summary>
        public void UpdateStimPacksStatus()
        {
            /// If StimPacks is not active -> do nothing.
            if (this.remainingStimPacksTime.Read() == 0) { return; }

            /// Decrease the remaining StimPacks time.
            this.remainingStimPacksTime.Write(this.remainingStimPacksTime.Read() - 1);

            /// If StimPacks time elapsed -> remove the effect of the StimPacks.
            if (this.remainingStimPacksTime.Read() == 0)
            {
                this.ElementTypeUpgrade.SpeedUpgrade = 0;
                foreach (IWeaponDataUpgrade weaponDataUpgrade in this.ElementTypeUpgrade.WeaponUpgrades)
                {
                    weaponDataUpgrade.CooldownUpgrade = 0;
                }
            }
        }

        /// <see cref="Entity.DestructionAnimationName"/>
        protected override string DestructionAnimationName { get { return "Dying"; } }

        /// <summary>
        /// The name of the Marine element type.
        /// </summary>
        public const string MARINE_TYPE_NAME = "Marine";

        /// <summary>
        /// The remaining time from the effect of StimPacks or 0 if StimPacks is not used currently.
        /// </summary>
        private HeapedValue<int> remainingStimPacksTime;
    }
}
