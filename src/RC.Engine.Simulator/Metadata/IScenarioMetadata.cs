using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Metadata
{
    /// <summary>
    /// Interface for accessing the metadata informations for RC scenarios.
    /// </summary>
    public interface IScenarioMetadata
    {
        /// <summary>
        /// Checks whether a unit type with the given name has been defined in this metadata.
        /// </summary>
        /// <param name="unitTypeName">The name of the unit type to check.</param>
        /// <returns>True if the unit type with the given name exists, false otherwise.</returns>
        bool HasUnitType(string unitTypeName);

        /// <summary>
        /// Checks whether an upgrade type with the given name has been defined in this metadata.
        /// </summary>
        /// <param name="upgradeTypeName">The name of the upgrade type to check.</param>
        /// <returns>True if the upgrade type with the given name exists, false otherwise.</returns>
        bool HasUpgradeType(string upgradeTypeName);

        /// <summary>
        /// Checks whether a building type with the given name has been defined in this metadata.
        /// </summary>
        /// <param name="buildingTypeName">The name of the building type to check.</param>
        /// <returns>True if the building type with the given name exists, false otherwise.</returns>
        bool HasBuildingType(string buildingTypeName);

        /// <summary>
        /// Checks whether an addon type with the given name has been defined in this metadata.
        /// </summary>
        /// <param name="addonTypeName">The name of the addon type to check.</param>
        /// <returns>True if the addon type with the given name exists, false otherwise.</returns>
        bool HasAddonType(string addonTypeName);

        /// <summary>
        /// Checks whether a missile type with the given name has been defined in this metadata.
        /// </summary>
        /// <param name="missileTypeName">The name of the missile type to check.</param>
        /// <returns>True if the missile type with the given name exists, false otherwise.</returns>
        bool HasMissileType(string missileTypeName);

        /// <summary>
        /// Checks whether a custom type with the given name has been defined in this metadata.
        /// </summary>
        /// <param name="customTypeName">The name of the custom type to check.</param>
        /// <returns>True if the custom type with the given name exists, false otherwise.</returns>
        bool HasCustomType(string customTypeName);

        /// <summary>
        /// Gets the unit type with the given name.
        /// </summary>
        /// <param name="unitTypeName">The name of the unit type.</param>
        /// <returns>The unit type with the given name.</returns>
        IUnitType GetUnitType(string unitTypeName);

        /// <summary>
        /// Gets the upgrade type with the given name.
        /// </summary>
        /// <param name="upgradeTypeName">The name of the upgrade type.</param>
        /// <returns>The upgrade type with the given name.</returns>
        IUpgradeType GetUpgradeType(string upgradeTypeName);

        /// <summary>
        /// Gets the building type with the given name.
        /// </summary>
        /// <param name="buildingTypeName">The name of the building type.</param>
        /// <returns>The building type with the given name.</returns>
        IBuildingType GetBuildingType(string buildingTypeName);

        /// <summary>
        /// Gets the addon type with the given name.
        /// </summary>
        /// <param name="addonTypeName">The name of the addon type.</param>
        /// <returns>The addon type with the given name.</returns>
        IAddonType GetAddonType(string addonTypeName);

        /// <summary>
        /// Gets the missile type with the given name.
        /// </summary>
        /// <param name="missileTypeName">The name of the missile type.</param>
        /// <returns>The missile type with the given name.</returns>
        IMissileType GetMissileType(string missileTypeName);

        /// <summary>
        /// Gets the custom type with the given name.
        /// </summary>
        /// <param name="customTypeName">The name of the custom type.</param>
        /// <returns>The custom type with the given name.</returns>
        IScenarioElementType GetCustomType(string customTypeName);

        /// <summary>
        /// Gets the scenario element type with the given name.
        /// </summary>
        /// <param name="typeName">The name of the element type.</param>
        /// <returns>The scenario element type with the given name.</returns>
        IScenarioElementType GetElementType(string typeName);

        /// <summary>
        /// Gets the list of all unit types defined in the metadata.
        /// </summary>
        IEnumerable<IUnitType> UnitTypes { get; }

        /// <summary>
        /// Gets the list of all upgrade types defined in the metadata.
        /// </summary>
        IEnumerable<IUpgradeType> UpgradeTypes { get; }

        /// <summary>
        /// Gets the list of all building types defined in the metadata.
        /// </summary>
        IEnumerable<IBuildingType> BuildingTypes { get; }

        /// <summary>
        /// Gets the list of all addon types defined in the metadata.
        /// </summary>
        IEnumerable<IAddonType> AddonTypes { get; }

        /// <summary>
        /// Gets the list of all missile types defined in the metadata.
        /// </summary>
        IEnumerable<IMissileType> MissileTypes { get; }

        /// <summary>
        /// Gets the list of all custom types defined in the metadata.
        /// </summary>
        IEnumerable<IScenarioElementType> CustomTypes { get; }

        /// <summary>
        /// Gets the list of all element types defined in the metadata.
        /// </summary>
        IEnumerable<IScenarioElementType> AllTypes { get; }

        /// <summary>
        /// Gets the element type with the given ID.
        /// </summary>
        /// <param name="typeID">The ID of the element type.</param>
        /// <returns>The element type with the given ID.</returns>
        IScenarioElementType this[int typeID] { get; }
    }
}
