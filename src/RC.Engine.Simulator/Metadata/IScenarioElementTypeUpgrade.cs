using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Metadata
{
    /// <summary>
    /// Interface that can be used for applying upgrades for an underlying scenario element type.
    /// </summary>
    public interface IScenarioElementTypeUpgrade
    {
        /// <summary>
        /// Gets or sets the armor level of the underlying scenario element type.
        /// </summary>
        int ArmorLevel { get; set; }

        /// <summary>
        /// Gets the cumulated upgrade to the armor of the underlying scenario element type.
        /// </summary>
        int CumulatedArmorUpgrade { get; }

        /// <summary>
        /// Gets or sets the upgrade to the max energy of the underlying scenario element type.
        /// </summary>
        int MaxEnergyUpgrade { get; set; }

        /// <summary>
        /// Gets the cumulated upgrade to the max energy of the underlying scenario element type.
        /// </summary>
        int CumulatedMaxEnergyUpgrade { get; }

        /// <summary>
        /// Gets or sets the upgrade to the sight range of the underlying scenario element type.
        /// </summary>
        int SightRangeUpgrade { get; set; }

        /// <summary>
        /// Gets the cumulated upgrade to the sight range of the underlying scenario element type.
        /// </summary>
        int CumulatedSightRangeUpgrade { get; }

        /// <summary>
        /// Gets or sets the upgrade to the speed of the underlying scenario element type.
        /// </summary>
        RCNumber SpeedUpgrade { get; set; }

        /// <summary>
        /// Gets the cumulated upgrade to the speed of the underlying scenario element type.
        /// </summary>
        RCNumber CumulatedSpeedUpgrade { get; }

        /// <summary>
        /// Gets the upgrade interfaces of the standard weapons of the underlying scenario element type.
        /// </summary>
        IEnumerable<IWeaponDataUpgrade> StandardWeaponUpgrades { get; }
    }
}
