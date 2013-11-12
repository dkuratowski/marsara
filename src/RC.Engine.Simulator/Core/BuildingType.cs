using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Contains the definition of a building type.
    /// </summary>
    class BuildingType : EntityType
    {
        /// <summary>
        /// Constructs a new building type.
        /// </summary>
        /// <param name="name">The name of this building type.</param>
        /// <param name="metadata">The metadata object that this building type belongs to.</param>
        public BuildingType(string name, SimMetadata metadata)
            : base(name, metadata)
        {
            this.unitTypes = new Dictionary<string, UnitType>();
            this.addonTypes = new Dictionary<string, AddonType>();
            this.upgradeTypes = new Dictionary<string, UpgradeType>();
        }

        /// <summary>
        /// Checks whether this building type has an addon type with the given name.
        /// </summary>
        /// <param name="addonTypeName">The name of the searched addon type.</param>
        /// <returns>True if this building type has an addon type with the given name, false otherwise.</returns>
        public bool HasAddonType(string addonTypeName)
        {
            if (addonTypeName == null) { throw new ArgumentNullException("addonTypeName"); }
            return this.addonTypes.ContainsKey(addonTypeName);
        }

        /// <summary>
        /// Checks whether this building type has a unit type with the given name.
        /// </summary>
        /// <param name="unitTypeName">The name of the searched unit type.</param>
        /// <returns>True if this building type has a unit type with the given name, false otherwise.</returns>
        public bool HasUnitType(string unitTypeName)
        {
            if (unitTypeName == null) { throw new ArgumentNullException("unitTypeName"); }
            return this.unitTypes.ContainsKey(unitTypeName);
        }

        /// <summary>
        /// Checks whether this building type has an upgrade type with the given name.
        /// </summary>
        /// <param name="upgradeTypeName">The name of the searched upgrade type.</param>
        /// <returns>True if this building type has an upgrade type with the given name, false otherwise.</returns>
        public bool HasUpgradeType(string upgradeTypeName)
        {
            if (upgradeTypeName == null) { throw new ArgumentNullException("upgradeTypeName"); }
            return this.upgradeTypes.ContainsKey(upgradeTypeName);
        }

        /// <summary>
        /// Gets the addon type of this building type with the given name.
        /// </summary>
        /// <param name="addonTypeName">The name of the addon type.</param>
        /// <returns>The addon type with the given name.</returns>
        public AddonType GetAddonType(string addonTypeName)
        {
            if (addonTypeName == null) { throw new ArgumentNullException("addonTypeName"); }
            return this.addonTypes[addonTypeName];
        }

        /// <summary>
        /// Gets the unit type of this building type with the given name.
        /// </summary>
        /// <param name="unitTypeName">The name of the unit type.</param>
        /// <returns>The unit type with the given name.</returns>
        public UnitType GetUnitType(string unitTypeName)
        {
            if (unitTypeName == null) { throw new ArgumentNullException("unitTypeName"); }
            return this.unitTypes[unitTypeName];
        }

        /// <summary>
        /// Gets the upgrade type of this building type with the given name.
        /// </summary>
        /// <param name="upgradeTypeName">The name of the upgrade type.</param>
        /// <returns>The upgrade type with the given name.</returns>
        public UpgradeType GetUpgradeType(string upgradeTypeName)
        {
            if (upgradeTypeName == null) { throw new ArgumentNullException("upgradeTypeName"); }
            return this.upgradeTypes[upgradeTypeName];
        }

        #region BuildingType buildup methods

        /// <summary>
        /// Adds a unit type to this building type.
        /// </summary>
        /// <param name="unitType">The unit type to add.</param>
        public void AddUnitType(UnitType unitType)
        {
            if (this.Metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (unitType == null) { throw new ArgumentNullException("unitType"); }
            this.unitTypes.Add(unitType.Name, unitType);
        }

        /// <summary>
        /// Adds an addon type to this building type.
        /// </summary>
        /// <param name="addonType">The addon type to add.</param>
        public void AddAddonType(AddonType addonType)
        {
            if (this.Metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (addonType == null) { throw new ArgumentNullException("addonType"); }
            this.addonTypes.Add(addonType.Name, addonType);
        }

        /// <summary>
        /// Adds an upgrade type to this building type.
        /// </summary>
        /// <param name="upgradeType">The upgrade type to add.</param>
        public void AddUpgradeType(UpgradeType upgradeType)
        {
            if (this.Metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (upgradeType == null) { throw new ArgumentNullException("upgradeType"); }
            this.upgradeTypes.Add(upgradeType.Name, upgradeType);
        }

        #endregion BuildingType buildup methods

        /// <summary>
        /// List of the unit types that are created in buildings of this type mapped by their names.
        /// </summary>
        private Dictionary<string, UnitType> unitTypes;

        /// <summary>
        /// List of the addon types that are created by buildings of this type mapped by their names.
        /// </summary>
        private Dictionary<string, AddonType> addonTypes;

        /// <summary>
        /// List of the upgrade types that are performed in buildings of this type mapped by their names.
        /// </summary>
        private Dictionary<string, UpgradeType> upgradeTypes;
    }
}
