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
    class SimulationMetadata
    {
        /// <summary>
        /// Constructs a SimulationMetadata object.
        /// </summary>
        public SimulationMetadata()
        {
            this.isFinalized = false;
            this.buildingTypes = new Dictionary<string, BuildingType>();
            this.addonTypes = new Dictionary<string, AddonType>();
            this.unitTypes = new Dictionary<string, UnitType>();
            this.upgradeTypes = new Dictionary<string, UpgradeType>();
        }

        #region Metadata buildup methods

        /// <summary>
        /// Adds a building type definition to the metadata.
        /// </summary>
        /// <param name="buildingType">The building type definition to add.</param>
        public void AddBuildingType(BuildingType buildingType)
        {
            if (this.isFinalized) { throw new InvalidOperationException("Metadata object already finalized!"); }
            if (buildingType == null) { throw new ArgumentNullException("buildingType"); }
            if (this.buildingTypes.ContainsKey(buildingType.Name) ||
                this.unitTypes.ContainsKey(buildingType.Name) ||
                this.addonTypes.ContainsKey(buildingType.Name) ||
                this.upgradeTypes.ContainsKey(buildingType.Name)) { throw new InvalidOperationException(string.Format("Metadata element with name '{0}' already defined!", buildingType.Name)); }

            this.buildingTypes.Add(buildingType.Name, buildingType);
        }

        /// <summary>
        /// Adds a unit type definition to the metadata.
        /// </summary>
        /// <param name="buildingType">The unit type definition to add.</param>
        public void AddUnitType(UnitType unitType)
        {
            if (this.isFinalized) { throw new InvalidOperationException("Metadata object already finalized!"); }
            if (unitType == null) { throw new ArgumentNullException("unitType"); }
            if (this.buildingTypes.ContainsKey(unitType.Name) ||
                this.unitTypes.ContainsKey(unitType.Name) ||
                this.addonTypes.ContainsKey(unitType.Name) ||
                this.upgradeTypes.ContainsKey(unitType.Name)) { throw new InvalidOperationException(string.Format("Metadata element with name '{0}' already defined!", unitType.Name)); }

            this.unitTypes.Add(unitType.Name, unitType);
        }

        /// <summary>
        /// Adds a unit type definition to the metadata.
        /// </summary>
        /// <param name="buildingType">The unit type definition to add.</param>
        public void AddAddonType(AddonType addonType)
        {
            if (this.isFinalized) { throw new InvalidOperationException("Metadata object already finalized!"); }
            if (addonType == null) { throw new ArgumentNullException("addonType"); }
            if (this.buildingTypes.ContainsKey(addonType.Name) ||
                this.unitTypes.ContainsKey(addonType.Name) ||
                this.addonTypes.ContainsKey(addonType.Name) ||
                this.upgradeTypes.ContainsKey(addonType.Name)) { throw new InvalidOperationException(string.Format("Metadata element with name '{0}' already defined!", addonType.Name)); }

            this.addonTypes.Add(addonType.Name, addonType);
        }

        /// <summary>
        /// Adds a unit type definition to the metadata.
        /// </summary>
        /// <param name="buildingType">The unit type definition to add.</param>
        public void AddUpgradeType(UpgradeType upgradeType)
        {
            if (this.isFinalized) { throw new InvalidOperationException("Metadata object already finalized!"); }
            if (upgradeType == null) { throw new ArgumentNullException("upgradeType"); }
            if (this.buildingTypes.ContainsKey(upgradeType.Name) ||
                this.unitTypes.ContainsKey(upgradeType.Name) ||
                this.addonTypes.ContainsKey(upgradeType.Name) ||
                this.upgradeTypes.ContainsKey(upgradeType.Name)) { throw new InvalidOperationException(string.Format("Metadata element with name '{0}' already defined!", upgradeType.Name)); }

            this.upgradeTypes.Add(upgradeType.Name, upgradeType);
        }

        /// <summary>
        /// Checks and finalizes the metadata object. Buildup methods will be unavailable after calling this method.
        /// </summary>
        public void CheckAndFinalize()
        {
            /// TODO: check and finalize here.
            this.isFinalized = true;
        }

        #endregion Metadata buildup methods

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
        /// Becomes true when this metadata object is finalized.
        /// </summary>
        private bool isFinalized;
    }
}
