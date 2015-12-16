using RC.Common;
using RC.Engine.Simulator.Core;
using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Metadata
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
    /// Enumerates the possible types of a weapon.
    /// </summary>
    public enum WeaponTypeEnum
    {
        [EnumMapping(XmlMetadataConstants.GROUNDWEAPON_ELEM)]
        Ground = 0,     /// The weapon can target only ground entities.
        [EnumMapping(XmlMetadataConstants.AIRWEAPON_ELEM)]
        Air = 1,        /// The weapon can target only flying entities.
        [EnumMapping(XmlMetadataConstants.AIRGROUNDWEAPON_ELEM)]
        AirGround = 2   /// The weapon can target both ground and flying entities.
    }

    /// <summary>
    /// Interface of a weapon definition of an element type.
    /// </summary>
    public interface IWeaponData
    {
        /// <summary>
        /// Gets the name of this weapon.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the displayed name of this weapon.
        /// </summary>
        string DisplayedName { get; }

        /// <summary>
        /// Gets the type of the weapon.
        /// </summary>
        IValueRead<WeaponTypeEnum> WeaponType { get; }
            
        /// <summary>
        /// Gets the cooldown value of the weapon.
        /// </summary>
        IValueRead<int> Cooldown { get; }

        /// <summary>
        /// Gets the damage value of the weapon.
        /// </summary>
        IValueRead<int> Damage { get; }

        /// <summary>
        /// Gets the damage type of the weapon.
        /// </summary>
        IValueRead<DamageTypeEnum> DamageType { get; }

        /// <summary>
        /// Gets the increment value of the weapon.
        /// </summary>
        IValueRead<int> Increment { get; }

        /// <summary>
        /// Gets the maximum range of the weapon.
        /// </summary>
        IValueRead<int> RangeMax { get; }

        /// <summary>
        /// Gets the minimum range of the weapon.
        /// </summary>
        IValueRead<int> RangeMin { get; }

        /// <summary>
        /// Gets the splash type of the weapon.
        /// </summary>
        IValueRead<SplashTypeEnum> SplashType { get; }

        /// <summary>
        /// Gets the missiles defined for this weapon data.
        /// </summary>
        IEnumerable<IMissileData> Missiles { get; }
    }
}
