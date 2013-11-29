using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Scenarios
{
    /// <summary>
    /// Interface of a weapon definition of an element type.
    /// </summary>
    public interface IWeaponData
    {
        /// <summary>
        /// Gets the cooldown value of the weapon.
        /// </summary>
        ConstValue<int> Cooldown { get; }

        /// <summary>
        /// Gets the damage value of the weapon.
        /// </summary>
        ConstValue<int> Damage { get; }

        /// <summary>
        /// Gets the damage type of the weapon.
        /// </summary>
        ConstValue<DamageTypeEnum> DamageType { get; }

        /// <summary>
        /// Gets the increment value of the weapon.
        /// </summary>
        ConstValue<int> Increment { get; }

        /// <summary>
        /// Gets the maximum range of the weapon.
        /// </summary>
        ConstValue<int> RangeMax { get; }

        /// <summary>
        /// Gets the minimum range of the weapon.
        /// </summary>
        ConstValue<int> RangeMin { get; }

        /// <summary>
        /// Gets the splash type of the weapon.
        /// </summary>
        ConstValue<SplashTypeEnum> SplashType { get; }
    }
}
