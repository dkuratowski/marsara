using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Metadata.Core
{
    /// <summary>
    /// Stores upgrade informations for weapons of scenario element types.
    /// </summary>
    class WeaponDataUpgrade : IWeaponData, IWeaponDataUpgrade
    {
        /// <summary>
        /// Constructs a WeaponDataUpgrade instance.
        /// </summary>
        /// <param name="metadataUpgrade">The metadata upgrade that this instance belongs to.</param>
        /// <param name="originalWeaponData">The original weapon data that this instance is upgrading.</param>
        public WeaponDataUpgrade(ScenarioMetadataUpgrade metadataUpgrade, IWeaponData originalWeaponData)
        {
            if (metadataUpgrade == null) { throw new ArgumentNullException("metadataUpgrade"); }
            if (originalWeaponData == null) { throw new ArgumentNullException("originalWeaponData"); }

            this.metadataUpgrade = metadataUpgrade;
            this.missileDataWrappers = new List<MissileDataWrapper>();

            this.damageModifier = new IntValueModifier();
            this.cooldownModifier = new IntValueModifier();
            this.rangeMaxModifier = new IntValueModifier();
            this.rangeMinModifier = new IntValueModifier();

            this.Reset(originalWeaponData);
        }

        #region IWeaponData

        /// <see cref="IWeaponData.Name"/>
        string IWeaponData.Name { get { return this.originalWeaponData.Name; } }

        /// <see cref="IWeaponData.DisplayedName"/>
        string IWeaponData.DisplayedName { get { return this.originalWeaponData.DisplayedName; } }

        /// <see cref="IWeaponData.WeaponType"/>
        IValueRead<WeaponTypeEnum> IWeaponData.WeaponType { get { return this.originalWeaponData.WeaponType; } }

        /// <see cref="IWeaponData.Cooldown"/>
        IValueRead<int> IWeaponData.Cooldown { get { return this.cooldownModifier.HasAttachedModifiedValue() ? this.cooldownModifier : null; } }

        /// <see cref="IWeaponData.Damage"/>
        IValueRead<int> IWeaponData.Damage { get { return this.damageModifier.HasAttachedModifiedValue() ? this.damageModifier : null; } }

        /// <see cref="IWeaponData.DamageType"/>
        IValueRead<DamageTypeEnum> IWeaponData.DamageType { get { return this.originalWeaponData.DamageType; } }

        /// <see cref="IWeaponData.Increment"/>
        IValueRead<int> IWeaponData.Increment { get { return this.originalWeaponData.Increment; } }

        /// <see cref="IWeaponData.RangeMax"/>
        IValueRead<int> IWeaponData.RangeMax { get { return this.rangeMaxModifier.HasAttachedModifiedValue() ? this.rangeMaxModifier : null; } }

        /// <see cref="IWeaponData.RangeMin"/>
        IValueRead<int> IWeaponData.RangeMin { get { return this.rangeMinModifier.HasAttachedModifiedValue() ? this.rangeMinModifier : null; } }

        /// <see cref="IWeaponData.SplashType"/>
        IValueRead<SplashTypeEnum> IWeaponData.SplashType { get { return this.originalWeaponData.SplashType; } }

        /// <see cref="IWeaponData.Missiles"/>
        IEnumerable<IMissileData> IWeaponData.Missiles { get { return this.missileDataWrappers; } }

        #endregion IWeaponData

        #region IWeaponDataUpgrade

        /// <see cref="IWeaponDataUpgrade.DamageUpgrade"/>
        public int DamageUpgrade
        {
            get { return this.damageModifier.Modification; }
            set { this.damageModifier.Modification = value; }
        }

        /// <see cref="IWeaponDataUpgrade.CumulatedDamageUpgrade"/>
        public int CumulatedDamageUpgrade
        {
            get { return this.originalUpdateIface != null ? this.originalUpdateIface.CumulatedDamageUpgrade + this.damageModifier.Modification : this.damageModifier.Modification; }
        }

        /// <see cref="IWeaponDataUpgrade.CooldownUpgrade"/>
        public int CooldownUpgrade
        {
            get { return this.cooldownModifier.Modification; }
            set { this.cooldownModifier.Modification = value; }
        }

        /// <see cref="IWeaponDataUpgrade.CumulatedCooldownUpgrade"/>
        public int CumulatedCooldownUpgrade
        {
            get { return this.originalUpdateIface != null ? this.originalUpdateIface.CumulatedCooldownUpgrade + this.cooldownModifier.Modification : this.cooldownModifier.Modification; }
        }

        /// <see cref="IWeaponDataUpgrade.RangeMaxUpgrade"/>
        public int RangeMaxUpgrade
        {
            get { return this.rangeMaxModifier.Modification; }
            set { this.rangeMaxModifier.Modification = value; }
        }

        /// <see cref="IWeaponDataUpgrade.CumulatedRangeMaxUpgrade"/>
        public int CumulatedRangeMaxUpgrade
        {
            get { return this.originalUpdateIface != null ? this.originalUpdateIface.CumulatedRangeMaxUpgrade + this.rangeMaxModifier.Modification : this.rangeMaxModifier.Modification; }
        }

        /// <see cref="IWeaponDataUpgrade.RangeMinUpgrade"/>
        public int RangeMinUpgrade
        {
            get { return this.rangeMinModifier.Modification; }
            set { this.rangeMinModifier.Modification = value; }
        }

        /// <see cref="IWeaponDataUpgrade.CumulatedRangeMinUpgrade"/>
        public int CumulatedRangeMinUpgrade
        {
            get { return this.originalUpdateIface != null ? this.originalUpdateIface.CumulatedRangeMinUpgrade + this.rangeMinModifier.Modification : this.rangeMinModifier.Modification; }
        }

        #endregion IWeaponDataUpgrade

        #region Internal public methods

        /// <summary>
        /// Resets this instance.
        /// </summary>
        /// <param name="originalWeaponData">The original weapon data that this instance is upgrading.</param>
        internal void Reset(IWeaponData originalWeaponData)
        {
            if (originalWeaponData == null) { throw new ArgumentNullException("originalWeaponData"); }

            this.originalWeaponData = originalWeaponData;
            this.originalUpdateIface = originalWeaponData as IWeaponDataUpgrade;

            this.damageModifier.AttachModifiedValue(this.originalWeaponData.Damage);
            this.cooldownModifier.AttachModifiedValue(this.originalWeaponData.Cooldown);
            this.rangeMaxModifier.AttachModifiedValue(this.originalWeaponData.RangeMax);
            this.rangeMinModifier.AttachModifiedValue(this.originalWeaponData.RangeMin);

            this.missileDataWrappers.Clear();
            foreach (IMissileData originalMissileData in this.originalWeaponData.Missiles)
            {
                this.missileDataWrappers.Add(new MissileDataWrapper(this, originalMissileData));
            }
        }

        #endregion Internal public methods

        /// <summary>
        /// This is a wrapper over a missile data.
        /// </summary>
        private class MissileDataWrapper : IMissileData
        {
            /// <summary>
            /// Constructs a MissileDataWrapper instance for the given missile data.
            /// </summary>
            /// <param name="weaponDataUpgrade">The weapon data upgrade instance that this missile data wrapper belongs to.</param>
            /// <param name="originalMissileData">The wrapped missile data.</param>
            public MissileDataWrapper(WeaponDataUpgrade weaponDataUpgrade, IMissileData originalMissileData)
            {
                if (weaponDataUpgrade == null) { throw new ArgumentNullException("weaponDataUpgrade"); }
                if (originalMissileData == null) { throw new ArgumentNullException("originalMissileData"); }

                this.missileType = new IMissileType(weaponDataUpgrade.metadataUpgrade.GetElementTypeUpgradeImpl(originalMissileData.MissileType.Name));
                this.originalMissileData = originalMissileData;
            }

            /// <see cref="IMissileData.MissileType"/>
            public IMissileType MissileType { get { return this.missileType; } }

            /// <see cref="IMissileData.GetRelativeLaunchPosition"/>
            public RCNumVector GetRelativeLaunchPosition(MapDirection direction) { return this.originalMissileData.GetRelativeLaunchPosition(direction); }

            /// <summary>
            /// The wrapped missile type.
            /// </summary>
            private IMissileType missileType;

            /// <summary>
            /// The wrapped missile data.
            /// </summary>
            private IMissileData originalMissileData;
        }

        /// <summary>
        /// Reference to metadata upgrade that this instance belongs to.
        /// </summary>
        private ScenarioMetadataUpgrade metadataUpgrade;

        /// <summary>
        /// Reference to the upgraded weapon.
        /// </summary>
        private IWeaponData originalWeaponData;
        private IWeaponDataUpgrade originalUpdateIface;

        /// <summary>
        /// The list of the missile data wrappers.
        /// </summary>
        private List<MissileDataWrapper> missileDataWrappers;

        /// <summary>
        /// Modifier instances for the upgradable values of the underlying weapon.
        /// </summary>
        private ValueModifier<int> damageModifier;
        private ValueModifier<int> cooldownModifier;
        private ValueModifier<int> rangeMaxModifier;
        private ValueModifier<int> rangeMinModifier;
    }
}
