using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Metadata
{
    /// <summary>
    /// Interface that can be used for applying upgrades for a weapon of an underlying scenario element type.
    /// </summary>
    public interface IWeaponDataUpgrade
    {
        /// <summary>
        /// Gets or sets the upgrade to the damage of the underlying weapon.
        /// </summary>
        int DamageUpgrade { get; set; }

        /// <summary>
        /// Gets the cumulated upgrade to the damage of the underlying weapon.
        /// </summary>
        int CumulatedDamageUpgrade { get; }

        /// <summary>
        /// Gets or sets the upgrade to the cooldown of the underlying weapon.
        /// </summary>
        int CooldownUpgrade { get; set; }

        /// <summary>
        /// Gets the cumulated upgrade to the cooldown of the underlying weapon.
        /// </summary>
        int CumulatedCooldownUpgrade { get; }

        /// <summary>
        /// Gets or sets the upgrade to the maximum range of the underlying weapon.
        /// </summary>
        int RangeMaxUpgrade { get; set; }

        /// <summary>
        /// Gets the cumulated upgrade to the maximum range of the underlying weapon.
        /// </summary>
        int CumulatedRangeMaxUpgrade { get; }

        /// <summary>
        /// Gets or sets the upgrade to the minimum range of the underlying weapon.
        /// </summary>
        int RangeMinUpgrade { get; set; }

        /// <summary>
        /// Gets the cumulated upgrade to the minimum range of the underlying weapon.
        /// </summary>
        int CumulatedRangeMinUpgrade { get; }
    }
}
