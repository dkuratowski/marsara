using RC.Common.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Metadata.Core
{
    /// <summary>
    /// Upgrades to the metadata of RC scenarios can be applied using this class.
    /// </summary>
    class ScenarioMetadataUpgrade : IScenarioMetadata, IScenarioMetadataUpgrade
    {
        /// <summary>
        /// Constructs a ScenarioMetadataUpgrade instance.
        /// </summary>
        public ScenarioMetadataUpgrade()
        {
            this.attachedMetadata = null;
            this.elementTypeUpgrades = new Dictionary<string, ScenarioElementTypeUpgrade>();
        }

        #region IScenarioMetadata

        /// <see cref="IScenarioMetadata.HasUnitType"/>
        public bool HasUnitType(string unitTypeName)
        {
            if (this.attachedMetadata == null) { throw new InvalidOperationException("Metadata not yet attached!"); }
            return this.attachedMetadata.HasUnitType(unitTypeName);
        }

        /// <see cref="IScenarioMetadata.HasUpgradeType"/>
        public bool HasUpgradeType(string upgradeTypeName)
        {
            if (this.attachedMetadata == null) { throw new InvalidOperationException("Metadata not yet attached!"); }
            return this.attachedMetadata.HasUpgradeType(upgradeTypeName);
        }

        /// <see cref="IScenarioMetadata.HasBuildingType"/>
        public bool HasBuildingType(string buildingTypeName)
        {
            if (this.attachedMetadata == null) { throw new InvalidOperationException("Metadata not yet attached!"); }
            return this.attachedMetadata.HasBuildingType(buildingTypeName);
        }

        /// <see cref="IScenarioMetadata.HasAddonType"/>
        public bool HasAddonType(string addonTypeName)
        {
            if (this.attachedMetadata == null) { throw new InvalidOperationException("Metadata not yet attached!"); }
            return this.attachedMetadata.HasAddonType(addonTypeName);
        }

        /// <see cref="IScenarioMetadata.HasMissileType"/>
        public bool HasMissileType(string missileTypeName)
        {
            if (this.attachedMetadata == null) { throw new InvalidOperationException("Metadata not yet attached!"); }
            return this.attachedMetadata.HasMissileType(missileTypeName);
        }

        /// <see cref="IScenarioMetadata.HasCustomType"/>
        public bool HasCustomType(string customTypeName)
        {
            if (this.attachedMetadata == null) { throw new InvalidOperationException("Metadata not yet attached!"); }
            return this.attachedMetadata.HasCustomType(customTypeName);
        }

        /// <see cref="IScenarioMetadata.HasElementType"/>
        public bool HasElementType(string typeName)
        {
            if (this.attachedMetadata == null) { throw new InvalidOperationException("Metadata not yet attached!"); }
            return this.attachedMetadata.HasElementType(typeName);
        }

        /// <see cref="IScenarioMetadata.GetUnitType"/>
        public IUnitType GetUnitType(string unitTypeName)
        {
            if (this.attachedMetadata == null) { throw new InvalidOperationException("Metadata not yet attached!"); }

            IUnitType attachedUnitType = this.attachedMetadata.GetUnitType(unitTypeName);
            return new IUnitType(this.GetElementTypeUpgradeImpl(attachedUnitType.Name));
        }

        /// <see cref="IScenarioMetadata.GetUpgradeType"/>
        public IUpgradeType GetUpgradeType(string upgradeTypeName)
        {
            if (this.attachedMetadata == null) { throw new InvalidOperationException("Metadata not yet attached!"); }

            IUpgradeType attachedUpgradeType = this.attachedMetadata.GetUpgradeType(upgradeTypeName);
            return new IUpgradeType(this.GetElementTypeUpgradeImpl(attachedUpgradeType.Name));
        }

        /// <see cref="IScenarioMetadata.GetBuildingType"/>
        public IBuildingType GetBuildingType(string buildingTypeName)
        {
            if (this.attachedMetadata == null) { throw new InvalidOperationException("Metadata not yet attached!"); }

            IBuildingType attachedBuildingType = this.attachedMetadata.GetBuildingType(buildingTypeName);
            return new IBuildingType(this.GetElementTypeUpgradeImpl(attachedBuildingType.Name));
        }

        /// <see cref="IScenarioMetadata.GetAddonType"/>
        public IAddonType GetAddonType(string addonTypeName)
        {
            if (this.attachedMetadata == null) { throw new InvalidOperationException("Metadata not yet attached!"); }

            IAddonType attachedAddonType = this.attachedMetadata.GetAddonType(addonTypeName);
            return new IAddonType(this.GetElementTypeUpgradeImpl(attachedAddonType.Name));
        }

        /// <see cref="IScenarioMetadata.GetMissileType"/>
        public IMissileType GetMissileType(string missileTypeName)
        {
            if (this.attachedMetadata == null) { throw new InvalidOperationException("Metadata not yet attached!"); }

            IMissileType attachedMissileType = this.attachedMetadata.GetMissileType(missileTypeName);
            return new IMissileType(this.GetElementTypeUpgradeImpl(attachedMissileType.Name));
        }

        /// <see cref="IScenarioMetadata.GetCustomType"/>
        public IScenarioElementType GetCustomType(string customTypeName)
        {
            if (this.attachedMetadata == null) { throw new InvalidOperationException("Metadata not yet attached!"); }

            IScenarioElementType attachedCustomType = this.attachedMetadata.GetCustomType(customTypeName);
            return new IScenarioElementType(this.GetElementTypeUpgradeImpl(attachedCustomType.Name));
        }

        /// <see cref="IScenarioMetadata.GetElementType"/>
        public IScenarioElementType GetElementType(string typeName)
        {
            if (this.attachedMetadata == null) { throw new InvalidOperationException("Metadata not yet attached!"); }

            IScenarioElementType attachedElementType = this.attachedMetadata.GetElementType(typeName);
            return new IScenarioElementType(this.GetElementTypeUpgradeImpl(attachedElementType.Name));
        }

        /// <see cref="IScenarioMetadata.UnitTypes"/>
        public IEnumerable<IUnitType> UnitTypes
        {
            get
            {
                if (this.attachedMetadata == null) { throw new InvalidOperationException("Metadata not yet attached!"); }

                List<IUnitType> retList = new List<IUnitType>();
                foreach (IUnitType unitType in this.attachedMetadata.UnitTypes) { retList.Add(new IUnitType(this.GetElementTypeUpgradeImpl(unitType.Name))); }
                return retList;
            }
        }

        /// <see cref="IScenarioMetadata.UpgradeTypes"/>
        public IEnumerable<IUpgradeType> UpgradeTypes
        {
            get
            {
                if (this.attachedMetadata == null) { throw new InvalidOperationException("Metadata not yet attached!"); }

                List<IUpgradeType> retList = new List<IUpgradeType>();
                foreach (IUpgradeType upgradeType in this.attachedMetadata.UpgradeTypes) { retList.Add(new IUpgradeType(this.GetElementTypeUpgradeImpl(upgradeType.Name))); }
                return retList;
            }
        }

        /// <see cref="IScenarioMetadata.BuildingTypes"/>
        public IEnumerable<IBuildingType> BuildingTypes
        {
            get
            {
                if (this.attachedMetadata == null) { throw new InvalidOperationException("Metadata not yet attached!"); }

                List<IBuildingType> retList = new List<IBuildingType>();
                foreach (IBuildingType buildingType in this.attachedMetadata.BuildingTypes) { retList.Add(new IBuildingType(this.GetElementTypeUpgradeImpl(buildingType.Name))); }
                return retList;
            }
        }

        /// <see cref="IScenarioMetadata.AddonTypes"/>
        public IEnumerable<IAddonType> AddonTypes
        {
            get
            {
                if (this.attachedMetadata == null) { throw new InvalidOperationException("Metadata not yet attached!"); }

                List<IAddonType> retList = new List<IAddonType>();
                foreach (IAddonType addonType in this.attachedMetadata.AddonTypes) { retList.Add(new IAddonType(this.GetElementTypeUpgradeImpl(addonType.Name))); }
                return retList;
            }
        }

        /// <see cref="IScenarioMetadata.MissileTypes"/>
        public IEnumerable<IMissileType> MissileTypes
        {
            get
            {
                if (this.attachedMetadata == null) { throw new InvalidOperationException("Metadata not yet attached!"); }

                List<IMissileType> retList = new List<IMissileType>();
                foreach (IMissileType missileType in this.attachedMetadata.MissileTypes) { retList.Add(new IMissileType(this.GetElementTypeUpgradeImpl(missileType.Name))); }
                return retList;
            }
        }

        /// <see cref="IScenarioMetadata.CustomTypes"/>
        public IEnumerable<IScenarioElementType> CustomTypes
        {
            get
            {
                if (this.attachedMetadata == null) { throw new InvalidOperationException("Metadata not yet attached!"); }

                List<IScenarioElementType> retList = new List<IScenarioElementType>();
                foreach (IScenarioElementType customType in this.attachedMetadata.CustomTypes) { retList.Add(new IScenarioElementType(this.GetElementTypeUpgradeImpl(customType.Name))); }
                return retList;
            }
        }

        /// <see cref="IScenarioMetadata.AllTypes"/>
        public IEnumerable<IScenarioElementType> AllTypes
        {
            get
            {
                if (this.attachedMetadata == null) { throw new InvalidOperationException("Metadata not yet attached!"); }

                List<IScenarioElementType> retList = new List<IScenarioElementType>();
                foreach (IScenarioElementType type in this.attachedMetadata.AllTypes) { retList.Add(new IScenarioElementType(this.GetElementTypeUpgradeImpl(type.Name))); }
                return retList;
            }
        }

        /// <see cref="IScenarioMetadata.this[]"/>
        public IScenarioElementType this[int typeID]
        {
            get
            {
                if (this.attachedMetadata == null) { throw new InvalidOperationException("Metadata not yet attached!"); }

                IScenarioElementType attachedType = this.attachedMetadata[typeID];
                return new IScenarioElementType(this.GetElementTypeUpgradeImpl(attachedType.Name));
            }
        }

        /// <see cref="IScenarioMetadata.ShadowPalette"/>
        public ISpritePalette ShadowPalette
        {
            get
            {
                if (this.attachedMetadata == null) { throw new InvalidOperationException("Metadata not yet attached!"); }
                return this.attachedMetadata.ShadowPalette;
            }
        }

        #endregion IScenarioMetadata

        #region IScenarioMetadataUpgrade

        /// <see cref="IScenarioMetadataUpgrade.GetElementTypeUpgrade"/>
        public IScenarioElementTypeUpgrade GetElementTypeUpgrade(string typeName)
        {
            return this.GetElementTypeUpgradeImpl(typeName);
        }

        /// <see cref="IScenarioMetadataUpgrade.AttachMetadata"/>
        public void AttachMetadata(IScenarioMetadata metadata)
        {
            if (metadata == null) { throw new ArgumentNullException("metadata"); }

            bool firstTimeAttach = this.attachedMetadata == null;
            this.attachedMetadata = metadata;
            if (firstTimeAttach)
            {
                /// First time attach -> fill the upgrade list with the names.
                foreach (IScenarioElementType elementType in metadata.AllTypes) { this.elementTypeUpgrades.Add(elementType.Name, null); }
            }
            else
            {
                /// Reattach -> check metadata compatibility and reset the upgrades.
                foreach (IScenarioElementType elementType in metadata.AllTypes) { if (!this.elementTypeUpgrades.ContainsKey(elementType.Name)) { throw new InvalidOperationException("Unable to attach non-compatible metadata!"); } }
                foreach (KeyValuePair<string, ScenarioElementTypeUpgrade> upgrade in this.elementTypeUpgrades)
                {
                    if (!metadata.HasElementType(upgrade.Key)) { throw new InvalidOperationException("Unable to attach non-compatible metadata!"); }
                    if (upgrade.Value != null) { upgrade.Value.Reset(); }
                }
            }
        }

        #endregion IScenarioMetadataUpgrade

        #region Internal public methods

        /// <summary>
        /// Gets the upgrade of the given scenario element type.
        /// </summary>
        /// <param name="typeName">The name of the scenario element type.</param>
        /// <returns>The upgrade of the given scenario element type.</returns>
        internal ScenarioElementTypeUpgrade GetElementTypeUpgradeImpl(string typeName)
        {
            if (this.attachedMetadata == null) { throw new InvalidOperationException("Metadata not yet attached!"); }
            if (typeName == null) { throw new ArgumentNullException("typeName"); }
            
            if (this.elementTypeUpgrades[typeName] == null) { this.elementTypeUpgrades[typeName] = new ScenarioElementTypeUpgrade(this, typeName); }
            return this.elementTypeUpgrades[typeName];
        }

        /// <summary>
        /// Gets the attached metadata.
        /// </summary>
        internal IScenarioMetadata AttachedMetadata { get { return this.attachedMetadata; } }

        #endregion Internal public methods

        /// <summary>
        /// Reference to the attached metadata or null if no metadata is currently attached.
        /// </summary>
        private IScenarioMetadata attachedMetadata;

        /// <summary>
        /// The list of upgrades for the scenario element types mapped by their names.
        /// </summary>
        private Dictionary<string, ScenarioElementTypeUpgrade> elementTypeUpgrades;
    }
}
