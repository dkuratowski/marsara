using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Metadata.Core
{
    /// <summary>
    /// Stores the data of a weapon of an element type.
    /// </summary>
    class WeaponData : IWeaponData
    {
        /// <summary>
        /// Constructs a weapon data struct for an element type.
        /// </summary>
        /// <param name="name">The name of this weapon.</param>
        /// <param name="metadata">The metadata object that this weapon data belongs to.</param>
        /// <param name="weaponType">The type of this weapon.</param>
        public WeaponData(string name, ScenarioMetadata metadata, WeaponTypeEnum weaponType)
        {
            if (name == null) { throw new ArgumentNullException("name"); }
            if (metadata == null) { throw new ArgumentNullException("metadata"); }

            this.name = name;
            this.displayedName = null;
            this.weaponType = new ConstValue<WeaponTypeEnum>(weaponType);
            this.cooldown = null;
            this.damage = null;
            this.damageType = null;
            this.increment = new ConstValue<int>(0);
            this.rangeMax = null;
            this.rangeMin = new ConstValue<int>(0);
            this.splashType = new ConstValue<SplashTypeEnum>(SplashTypeEnum.None);

            this.missiles = new List<MissileData>();
            this.metadata = metadata;
        }

        #region IWeaponData methods

        /// <see cref="IWeaponData.Name"/>
        public string Name { get { return this.name; } }

        /// <see cref="IWeaponData.DisplayedName"/>
        public string DisplayedName { get { return this.displayedName; } }

        /// <see cref="IWeaponData.WeaponType"/>
        public IValueRead<WeaponTypeEnum> WeaponType { get { return this.weaponType; } }

        /// <see cref="IWeaponData.Cooldown"/>
        public IValueRead<int> Cooldown { get { return this.cooldown; } }

        /// <see cref="IWeaponData.Damage"/>
        public IValueRead<int> Damage { get { return this.damage; } }

        /// <see cref="IWeaponData.DamageType"/>
        public IValueRead<DamageTypeEnum> DamageType { get { return this.damageType; } }

        /// <see cref="IWeaponData.Increment"/>
        public IValueRead<int> Increment { get { return this.increment; } }

        /// <see cref="IWeaponData.RangeMax"/>
        public IValueRead<int> RangeMax { get { return this.rangeMax; } }

        /// <see cref="IWeaponData.RangeMin"/>
        public IValueRead<int> RangeMin { get { return this.rangeMin; } }

        /// <see cref="IWeaponData.SplashType"/>
        public IValueRead<SplashTypeEnum> SplashType { get { return this.splashType; } }

        /// <see cref="IWeaponData.Missiles"/>
        public IEnumerable<IMissileData> Missiles { get { return this.missiles; } }

        #endregion IWeaponData methods

        #region WeaponData buildup methods

        /// <summary>
        /// Sets the displayed name of this weapon. Default value is null.
        /// </summary>
        public void SetDisplayedName(string displayedName)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            this.displayedName = displayedName;
        }

        /// <summary>
        /// Sets the cooldown value of the weapon. No default value, must be set.
        /// </summary>
        public void SetCooldown(int cooldown)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            this.cooldown = new ConstValue<int>(cooldown);
        }

        /// <summary>
        /// Sets the damage value of the weapon. No default value, must be set.
        /// </summary>
        public void SetDamage(int damage)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            this.damage = new ConstValue<int>(damage);
        }

        /// <summary>
        /// Sets the damage type of the weapon. No default value, must be set.
        /// </summary>
        public void SetDamageType(DamageTypeEnum damageType)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            this.damageType = new ConstValue<DamageTypeEnum>(damageType);
        }

        /// <summary>
        /// Sets the increment value of the weapon. Default value is 0.
        /// </summary>
        public void SetIncrement(int increment)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            this.increment = new ConstValue<int>(increment);
        }

        /// <summary>
        /// Sets the maximum range of the weapon. No default value, must be set.
        /// </summary>
        public void SetRangeMax(int rangeMax)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            this.rangeMax = new ConstValue<int>(rangeMax);
        }

        /// <summary>
        /// Sets the minimum range of the weapon. Default value is 0.
        /// </summary>
        public void SetRangeMin(int rangeMin)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            this.rangeMin = new ConstValue<int>(rangeMin);
        }

        /// <summary>
        /// Sets the splash type of the weapon. Default value is SplashType.None.
        /// </summary>
        public void SetSplashType(SplashTypeEnum splashType)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            this.splashType = new ConstValue<SplashTypeEnum>(splashType);
        }

        /// <summary>
        /// Adds a missile definition to this weapon.
        /// </summary>
        /// <param name="missile">The missile definition to be added.</param>
        public void AddMissile(MissileData missile)
        {
            if (this.metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (missile == null) { throw new ArgumentNullException("missile"); }

            this.missiles.Add(missile);
        }

        /// <summary>
        /// Validates and finalizes this data structure.
        /// </summary>
        public void CheckAndFinalize()
        {
            if (!this.metadata.IsFinalized)
            {
                if (this.cooldown == null) { throw new SimulatorException("Cooldown must be set!"); }
                if (this.damage == null) { throw new SimulatorException("Damage must be set!"); }
                if (this.damageType == null) { throw new SimulatorException("DamageType must be set!"); }
                if (this.increment == null) { throw new SimulatorException("Increment must be set!"); }
                if (this.rangeMax == null) { throw new SimulatorException("RangeMax must be set!"); }
                if (this.rangeMin == null) { throw new SimulatorException("RangeMin must be set!"); }
                if (this.splashType == null) { throw new SimulatorException("SplashType must be set!"); }

                if (this.cooldown.Read() <= 0) { throw new SimulatorException("Cooldown cannot be 0 or less!"); }
                if (this.damage.Read() <= 0) { throw new SimulatorException("Damage cannot be 0 or less!"); }
                if (this.increment.Read() < 0) { throw new SimulatorException("Increment must be non-negative!"); }
                if (this.rangeMax.Read() < 0) { throw new SimulatorException("RangeMax must be non-negative!"); }
                if (this.rangeMin.Read() < 0) { throw new SimulatorException("RangeMin must be non-negative!"); }
                if (this.rangeMin.Read() > this.rangeMax.Read()) { throw new SimulatorException("RangeMin must be less than or equal to RangeMax!"); }

                // TODO: remove this comment when all weapon data has been defined correctly in the metadata!
                //if (this.missiles.Count == 0) { throw new SimulatorException("A weapon data must have at least 1 missile definition!"); }
                foreach (MissileData missile in this.missiles)
                {
                    missile.CheckAndFinalize();
                }
            }
        }

        #endregion WeaponData buildup methods

        /// <summary>
        /// The values of this weapon data struct.
        /// </summary>
        private ConstValue<WeaponTypeEnum> weaponType;
        private ConstValue<int> cooldown;
        private ConstValue<int> damage;
        private ConstValue<DamageTypeEnum> damageType;
        private ConstValue<int> increment;
        private ConstValue<int> rangeMax;
        private ConstValue<int> rangeMin;
        private ConstValue<SplashTypeEnum> splashType;

        /// <summary>
        /// List of the missile definitions of this weapon data.
        /// </summary>
        private readonly List<MissileData> missiles;

        /// <summary>
        /// Reference to the metadata that this weapon data struct belongs to.
        /// </summary>
        private readonly ScenarioMetadata metadata;

        /// <summary>
        /// The name of this weapon.
        /// </summary>
        private readonly string name;

        /// <summary>
        /// The displayed name of this weapon.
        /// </summary>
        private string displayedName;
    }
}
