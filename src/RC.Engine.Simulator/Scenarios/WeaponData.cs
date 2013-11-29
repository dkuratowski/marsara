using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Scenarios
{
    /// <summary>
    /// Enumerates the possible damage types of a weapon.
    /// </summary>
    public enum DamageTypeEnum
    {
        [EnumMapping("Normal")]
        Normal = 0,
        [EnumMapping("Concussive")]
        Concussive = 1,
        [EnumMapping("Explosive")]
        Explosive = 2
    }

    /// <summary>
    /// Enumerates the possible splash types of a weapon.
    /// </summary>
    public enum SplashTypeEnum
    {
        [EnumMapping("None")]
        None = 0,
        [EnumMapping("Friendly")]
        Friendly = 1,
        [EnumMapping("General")]
        General = 2
    }

    /// <summary>
    /// Stores the data of a weapon of an element type.
    /// </summary>
    class WeaponData : IWeaponData
    {
        /// <summary>
        /// Constructs a weapon data struct for an element type.
        /// </summary>
        /// <param name="metadata">The metadata object that this weapon data belongs to.</param>
        public WeaponData(ScenarioMetadata metadata)
        {
            if (metadata == null) { throw new ArgumentNullException("metadata"); }

            this.cooldown = null;
            this.damage = null;
            this.damageType = null;
            this.increment = new ConstValue<int>(0);
            this.rangeMax = null;
            this.rangeMin = new ConstValue<int>(0);
            this.splashType = new ConstValue<SplashTypeEnum>(SplashTypeEnum.None);

            this.metadata = metadata;
        }

        /// <summary>
        /// Gets the cooldown value of the weapon.
        /// </summary>
        public ConstValue<int> Cooldown { get { return this.cooldown; } }

        /// <summary>
        /// Gets the damage value of the weapon.
        /// </summary>
        public ConstValue<int> Damage { get { return this.damage; } }

        /// <summary>
        /// Gets the damage type of the weapon.
        /// </summary>
        public ConstValue<DamageTypeEnum> DamageType { get { return this.damageType; } }

        /// <summary>
        /// Gets the increment value of the weapon.
        /// </summary>
        public ConstValue<int> Increment { get { return this.increment; } }

        /// <summary>
        /// Gets the maximum range of the weapon.
        /// </summary>
        public ConstValue<int> RangeMax { get { return this.rangeMax; } }

        /// <summary>
        /// Gets the minimum range of the weapon.
        /// </summary>
        public ConstValue<int> RangeMin { get { return this.rangeMin; } }

        /// <summary>
        /// Gets the splash type of the weapon.
        /// </summary>
        public ConstValue<SplashTypeEnum> SplashType { get { return this.splashType; } }

        #region WeaponData buildup methods

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
                if (this.rangeMax.Read() <= 0) { throw new SimulatorException("RangeMax cannot be 0 or less!"); }
                if (this.rangeMin.Read() < 0) { throw new SimulatorException("RangeMin must be non-negative!"); }
                if (this.rangeMin.Read() >= this.rangeMax.Read()) { throw new SimulatorException("RangeMin must be less than RangeMax!"); }
            }
        }

        #endregion WeaponData buildup methods

        /// <summary>
        /// The values of this weapon data struct.
        /// </summary>
        private ConstValue<int> cooldown;
        private ConstValue<int> damage;
        private ConstValue<DamageTypeEnum> damageType;
        private ConstValue<int> increment;
        private ConstValue<int> rangeMax;
        private ConstValue<int> rangeMin;
        private ConstValue<SplashTypeEnum> splashType;

        /// <summary>
        /// Reference to the metadata that this weapon data struct belongs to.
        /// </summary>
        private ScenarioMetadata metadata;
    }
}
