using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Represents the metadata informations for the simulation.
    /// </summary>
    class SimMetadata
    {
        /// <summary>
        /// Constructs a SimMetadata object.
        /// </summary>
        public SimMetadata()
        {
            this.isFinalized = false;
            this.buildingTypes = new Dictionary<string, BuildingType>();
            this.addonTypes = new Dictionary<string, AddonType>();
            this.unitTypes = new Dictionary<string, UnitType>();
            this.upgradeTypes = new Dictionary<string, UpgradeType>();
        }

        /// <summary>
        /// Checks whether a unit type with the given name has been defined in this metadata.
        /// </summary>
        /// <param name="unitTypeName">The name of the unit type to check.</param>
        /// <returns>True if the unit type with the given name exists, false otherwise.</returns>
        public bool HasUnitType(string unitTypeName)
        {
            if (unitTypeName == null) { throw new ArgumentNullException("unitTypeName"); }
            return this.unitTypes.ContainsKey(unitTypeName);
        }

        /// <summary>
        /// Checks whether an upgrade type with the given name has been defined in this metadata.
        /// </summary>
        /// <param name="upgradeTypeName">The name of the upgrade type to check.</param>
        /// <returns>True if the upgrade type with the given name exists, false otherwise.</returns>
        public bool HasUpgradeType(string upgradeTypeName)
        {
            if (upgradeTypeName == null) { throw new ArgumentNullException("upgradeTypeName"); }
            return this.upgradeTypes.ContainsKey(upgradeTypeName);
        }

        /// <summary>
        /// Checks whether a building type with the given name has been defined in this metadata.
        /// </summary>
        /// <param name="buildingTypeName">The name of the building type to check.</param>
        /// <returns>True if the building type with the given name exists, false otherwise.</returns>
        public bool HasBuildingType(string buildingTypeName)
        {
            if (buildingTypeName == null) { throw new ArgumentNullException("buildingTypeName"); }
            return this.buildingTypes.ContainsKey(buildingTypeName);
        }

        /// <summary>
        /// Checks whether an addon type with the given name has been defined in this metadata.
        /// </summary>
        /// <param name="addonTypeName">The name of the addon type to check.</param>
        /// <returns>True if the addon type with the given name exists, false otherwise.</returns>
        public bool HasAddonType(string addonTypeName)
        {
            if (addonTypeName == null) { throw new ArgumentNullException("addonTypeName"); }
            return this.addonTypes.ContainsKey(addonTypeName);
        }

        /// <summary>
        /// Gets the unit type with the given name.
        /// </summary>
        /// <param name="unitTypeName">The name of the unit type.</param>
        /// <returns>The unit type with the given name.</returns>
        public UnitType GetUnitType(string unitTypeName)
        {
            if (unitTypeName == null) { throw new ArgumentNullException("unitTypeName"); }
            return this.unitTypes[unitTypeName];
        }

        /// <summary>
        /// Gets the upgrade type with the given name.
        /// </summary>
        /// <param name="upgradeTypeName">The name of the upgrade type.</param>
        /// <returns>The upgrade type with the given name.</returns>
        public UpgradeType GetUpgradeType(string upgradeTypeName)
        {
            if (upgradeTypeName == null) { throw new ArgumentNullException("upgradeTypeName"); }
            return this.upgradeTypes[upgradeTypeName];
        }

        /// <summary>
        /// Gets the building type with the given name.
        /// </summary>
        /// <param name="buildingTypeName">The name of the building type.</param>
        /// <returns>The building type with the given name.</returns>
        public BuildingType GetBuildingType(string buildingTypeName)
        {
            if (buildingTypeName == null) { throw new ArgumentNullException("buildingTypeName"); }
            return this.buildingTypes[buildingTypeName];
        }

        /// <summary>
        /// Gets the addon type with the given name.
        /// </summary>
        /// <param name="addonTypeName">The name of the addon type.</param>
        /// <returns>The addon type with the given name.</returns>
        public AddonType GetAddonType(string addonTypeName)
        {
            if (addonTypeName == null) { throw new ArgumentNullException("addonTypeName"); }
            return this.addonTypes[addonTypeName];
        }

        #region SimMetadata buildup methods

        /// <summary>
        /// Gets whether this metadata object has been finalized or not.
        /// </summary>
        public bool IsFinalized { get { return this.isFinalized; } }

        /// <summary>
        /// Adds a building type definition to the metadata.
        /// </summary>
        /// <param name="buildingType">The building type definition to add.</param>
        /// <exception cref="InvalidOperationException">If this metadata object has been finalized.</exception>
        public void AddBuildingType(BuildingType buildingType)
        {
            if (this.isFinalized) { throw new InvalidOperationException("SimMetadata object already finalized!"); }
            if (buildingType == null) { throw new ArgumentNullException("buildingType"); }
            if (this.buildingTypes.ContainsKey(buildingType.Name) ||
                this.unitTypes.ContainsKey(buildingType.Name) ||
                this.addonTypes.ContainsKey(buildingType.Name) ||
                this.upgradeTypes.ContainsKey(buildingType.Name)) { throw new InvalidOperationException(string.Format("SimMetadata element with name '{0}' already defined!", buildingType.Name)); }

            this.buildingTypes.Add(buildingType.Name, buildingType);
        }

        /// <summary>
        /// Adds a unit type definition to the metadata.
        /// </summary>
        /// <param name="buildingType">The unit type definition to add.</param>
        public void AddUnitType(UnitType unitType)
        {
            if (this.isFinalized) { throw new InvalidOperationException("SimMetadata object already finalized!"); }
            if (unitType == null) { throw new ArgumentNullException("unitType"); }
            if (this.buildingTypes.ContainsKey(unitType.Name) ||
                this.unitTypes.ContainsKey(unitType.Name) ||
                this.addonTypes.ContainsKey(unitType.Name) ||
                this.upgradeTypes.ContainsKey(unitType.Name)) { throw new InvalidOperationException(string.Format("SimMetadata element with name '{0}' already defined!", unitType.Name)); }

            this.unitTypes.Add(unitType.Name, unitType);
        }

        /// <summary>
        /// Adds a unit type definition to the metadata.
        /// </summary>
        /// <param name="buildingType">The unit type definition to add.</param>
        public void AddAddonType(AddonType addonType)
        {
            if (this.isFinalized) { throw new InvalidOperationException("SimMetadata object already finalized!"); }
            if (addonType == null) { throw new ArgumentNullException("addonType"); }
            if (this.buildingTypes.ContainsKey(addonType.Name) ||
                this.unitTypes.ContainsKey(addonType.Name) ||
                this.addonTypes.ContainsKey(addonType.Name) ||
                this.upgradeTypes.ContainsKey(addonType.Name)) { throw new InvalidOperationException(string.Format("SimMetadata element with name '{0}' already defined!", addonType.Name)); }

            this.addonTypes.Add(addonType.Name, addonType);
        }

        /// <summary>
        /// Adds a unit type definition to the metadata.
        /// </summary>
        /// <param name="buildingType">The unit type definition to add.</param>
        public void AddUpgradeType(UpgradeType upgradeType)
        {
            if (this.isFinalized) { throw new InvalidOperationException("SimMetadata object already finalized!"); }
            if (upgradeType == null) { throw new ArgumentNullException("upgradeType"); }
            if (this.buildingTypes.ContainsKey(upgradeType.Name) ||
                this.unitTypes.ContainsKey(upgradeType.Name) ||
                this.addonTypes.ContainsKey(upgradeType.Name) ||
                this.upgradeTypes.ContainsKey(upgradeType.Name)) { throw new InvalidOperationException(string.Format("SimMetadata element with name '{0}' already defined!", upgradeType.Name)); }

            this.upgradeTypes.Add(upgradeType.Name, upgradeType);
        }

        /// <summary>
        /// Checks and finalizes the metadata object. Buildup methods will be unavailable after calling this method.
        /// </summary>
        public void CheckAndFinalize()
        {
            List<SimObjectType> objList = new List<SimObjectType>();

            /// Buildup the references of the upgrade types.
            foreach (UpgradeType upgradeType in this.upgradeTypes.Values)
            {
                upgradeType.BuildupReferences();
                objList.Add(upgradeType);
            }

            /// Buildup the references of the addon types.
            foreach (AddonType addonType in this.addonTypes.Values)
            {
                addonType.BuildupReferences();
                objList.Add(addonType);
            }

            /// Buildup the references of the unit types.
            foreach (UnitType unitType in this.unitTypes.Values)
            {
                unitType.BuildupReferences();
                objList.Add(unitType);
            }

            /// Buildup the references of the building types.
            foreach (BuildingType buildingType in this.buildingTypes.Values)
            {
                buildingType.BuildupReferences();
                objList.Add(buildingType);
            }

            /// Finalize all object types.
            foreach (SimObjectType objType in objList)
            {
                objType.CheckAndFinalize();
            }

            this.isFinalized = true;
        }

        #endregion SimMetadata buildup methods

        /// <summary>
        /// List of the defined building types mapped by their names.
        /// </summary>
        private Dictionary<string, BuildingType> buildingTypes;

        /// <summary>
        /// List of the defined addon types mapped by their names.
        /// </summary>
        private Dictionary<string, AddonType> addonTypes;

        /// <summary>
        /// List of the defined unit types mapped by their names.
        /// </summary>
        private Dictionary<string, UnitType> unitTypes;

        /// <summary>
        /// List of the defined upgrade types mapped by their names.
        /// </summary>
        private Dictionary<string, UpgradeType> upgradeTypes;

        /// <summary>
        /// Indicates whether this metadata object has been finalized or not.
        /// </summary>
        private bool isFinalized;
    }
}
