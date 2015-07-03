using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Metadata
{
    /// <summary>
    /// Interface of the building types defined in the metadata.
    /// </summary>
    public interface IBuildingType : IScenarioElementType
    {
        /// <summary>
        /// Checks whether this building type has an addon type with the given name.
        /// </summary>
        /// <param name="addonTypeName">The name of the searched addon type.</param>
        /// <returns>True if this building type has an addon type with the given name, false otherwise.</returns>
        bool HasAddonType(string addonTypeName);

        /// <summary>
        /// Checks whether this building type has a unit type with the given name.
        /// </summary>
        /// <param name="unitTypeName">The name of the searched unit type.</param>
        /// <returns>True if this building type has a unit type with the given name, false otherwise.</returns>
        bool HasUnitType(string unitTypeName);

        /// <summary>
        /// Checks whether this building type has an upgrade type with the given name.
        /// </summary>
        /// <param name="upgradeTypeName">The name of the searched upgrade type.</param>
        /// <returns>True if this building type has an upgrade type with the given name, false otherwise.</returns>
        bool HasUpgradeType(string upgradeTypeName);

        /// <summary>
        /// Gets the addon type of this building type with the given name.
        /// </summary>
        /// <param name="addonTypeName">The name of the addon type.</param>
        /// <returns>The addon type with the given name.</returns>
        IAddonType GetAddonType(string addonTypeName);

        /// <summary>
        /// Gets the unit type of this building type with the given name.
        /// </summary>
        /// <param name="unitTypeName">The name of the unit type.</param>
        /// <returns>The unit type with the given name.</returns>
        IUnitType GetUnitType(string unitTypeName);

        /// <summary>
        /// Gets the upgrade type of this building type with the given name.
        /// </summary>
        /// <param name="upgradeTypeName">The name of the upgrade type.</param>
        /// <returns>The upgrade type with the given name.</returns>
        IUpgradeType GetUpgradeType(string upgradeTypeName);
    }
}
