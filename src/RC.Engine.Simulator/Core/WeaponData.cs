using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Enumerates the possible damage types of a weapon.
    /// </summary>
    enum DamageTypeEnum
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
    enum SplashTypeEnum
    {
        [EnumMapping("None")]
        None = 0,
        [EnumMapping("Friendly")]
        Friendly = 1,
        [EnumMapping("General")]
        General = 2
    }

    /// <summary>
    /// Stores the data of a weapon of a building/unit type.
    /// </summary>
    class WeaponData
    {
        /// <summary>
        /// Constructs a weapon data struct for a building/unit type.
        /// </summary>
        public WeaponData()
        {
            this.cooldown = null;
            this.damage = null;
            this.damageType = null;
            this.increment = new ConstValue<int>(0);
            this.rangeMax = null;
            this.rangeMin = new ConstValue<int>(0);
            this.splashType = new ConstValue<SplashTypeEnum>(SplashTypeEnum.None);

            this.isFinalized = false;
        }

        /// <summary>
        /// Gets the cooldown value of the weapon. No default value, must be set.
        /// </summary>
        public ConstValue<int> Cooldown
        {
            get { return this.cooldown; }
            set
            {
                if (this.isFinalized) { throw new InvalidOperationException("Already finalized!"); }
                if (value == null) { throw new ArgumentNullException("Cooldown"); }
                this.cooldown = value;
            }
        }

        /// <summary>
        /// Gets the damage value of the weapon. No default value, must be set.
        /// </summary>
        public ConstValue<int> Damage
        {
            get { return this.damage; }
            set
            {
                if (this.isFinalized) { throw new InvalidOperationException("Already finalized!"); }
                if (value == null) { throw new ArgumentNullException("Damage"); }
                this.damage = value;
            }
        }

        /// <summary>
        /// Gets the damage type of the weapon. No default value, must be set.
        /// </summary>
        public ConstValue<DamageTypeEnum> DamageType
        {
            get { return this.damageType; }
            set
            {
                if (this.isFinalized) { throw new InvalidOperationException("Already finalized!"); }
                if (value == null) { throw new ArgumentNullException("DamageType"); }
                this.damageType = value;
            }
        }

        /// <summary>
        /// Gets the increment value of the weapon. Default value is 0.
        /// </summary>
        public ConstValue<int> Increment
        {
            get { return this.increment; }
            set
            {
                if (this.isFinalized) { throw new InvalidOperationException("Already finalized!"); }
                if (value == null) { throw new ArgumentNullException("Increment"); }
                this.increment = value;
            }
        }

        /// <summary>
        /// Gets the maximum range of the weapon. No default value, must be set.
        /// </summary>
        public ConstValue<int> RangeMax
        {
            get { return this.rangeMax; }
            set
            {
                if (this.isFinalized) { throw new InvalidOperationException("Already finalized!"); }
                if (value == null) { throw new ArgumentNullException("RangeMax"); }
                this.rangeMax = value;
            }
        }

        /// <summary>
        /// Gets the minimum range of the weapon. Default value is 0.
        /// </summary>
        public ConstValue<int> RangeMin
        {
            get { return this.rangeMin; }
            set
            {
                if (this.isFinalized) { throw new InvalidOperationException("Already finalized!"); }
                if (value == null) { throw new ArgumentNullException("RangeMin"); }
                this.rangeMin = value;
            }
        }

        /// <summary>
        /// Gets the splash type of the weapon. Default value is SplashType.None.
        /// </summary>
        public ConstValue<SplashTypeEnum> SplashType
        {
            get { return this.splashType; }
            set
            {
                if (this.isFinalized) { throw new InvalidOperationException("Already finalized!"); }
                if (value == null) { throw new ArgumentNullException("SplashType"); }
                this.splashType = value;
            }
        }

        /// <summary>
        /// Validates and finalizes this data structure.
        /// </summary>
        public void CheckAndFinalize()
        {
            if (!this.isFinalized)
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

                this.isFinalized = true;
            }
        }

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
        /// Indicates whether this weapon data struct has been finalized or not.
        /// </summary>
        private bool isFinalized;
    }
}
