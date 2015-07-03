using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Metadata.Core
{
    /// <summary>
    /// Contains the definition of a building type.
    /// </summary>
    class BuildingType : ScenarioElementType, IBuildingType
    {
        /// <summary>
        /// Constructs a new building type.
        /// </summary>
        /// <param name="name">The name of this building type.</param>
        /// <param name="metadata">The metadata object that this building type belongs to.</param>
        public BuildingType(string name, ScenarioMetadata metadata)
            : base(name, metadata)
        {
            this.unitTypes = new Dictionary<string, UnitType>();
            this.addonTypes = new Dictionary<string, AddonType>();
            this.upgradeTypes = new Dictionary<string, UpgradeType>();
        }

        #region IBuildingType members

        /// <see cref="IBuildingType.HasAddonType"/>
        public bool HasAddonType(string addonTypeName)
        {
            if (addonTypeName == null) { throw new ArgumentNullException("addonTypeName"); }
            return this.addonTypes.ContainsKey(addonTypeName);
        }

        /// <see cref="IBuildingType.HasUnitType"/>
        public bool HasUnitType(string unitTypeName)
        {
            if (unitTypeName == null) { throw new ArgumentNullException("unitTypeName"); }
            return this.unitTypes.ContainsKey(unitTypeName);
        }

        /// <see cref="IBuildingType.HasUpgradeType"/>
        public bool HasUpgradeType(string upgradeTypeName)
        {
            if (upgradeTypeName == null) { throw new ArgumentNullException("upgradeTypeName"); }
            return this.upgradeTypes.ContainsKey(upgradeTypeName);
        }

        /// <see cref="IBuildingType.GetAddonType"/>
        public IAddonType GetAddonType(string addonTypeName)
        {
            return this.GetAddonTypeImpl(addonTypeName);
        }

        /// <see cref="IBuildingType.GetUnitType"/>
        public IUnitType GetUnitType(string unitTypeName)
        {
            return this.GetUnitTypeImpl(unitTypeName);
        }

        /// <see cref="IBuildingType.GetUpgradeType"/>
        public IUpgradeType GetUpgradeType(string upgradeTypeName)
        {
            return this.GetUpgradeTypeImpl(upgradeTypeName);
        }
        
        #endregion IBuildingType members

        #region Internal public methods

        /// <see cref="IBuildingType.GetAddonType"/>
        public AddonType GetAddonTypeImpl(string addonTypeName)
        {
            if (addonTypeName == null) { throw new ArgumentNullException("addonTypeName"); }
            return this.addonTypes[addonTypeName];
        }

        /// <see cref="IBuildingType.GetUnitType"/>
        public UnitType GetUnitTypeImpl(string unitTypeName)
        {
            if (unitTypeName == null) { throw new ArgumentNullException("unitTypeName"); }
            return this.unitTypes[unitTypeName];
        }

        /// <see cref="IBuildingType.GetUpgradeType"/>
        public UpgradeType GetUpgradeTypeImpl(string upgradeTypeName)
        {
            if (upgradeTypeName == null) { throw new ArgumentNullException("upgradeTypeName"); }
            return this.upgradeTypes[upgradeTypeName];
        }
        
        #endregion Internal public methods

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
