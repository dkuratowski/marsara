using RC.Common;
using RC.Common.ComponentModel;
using RC.Common.Configuration;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Metadata.Core
{
    /// <summary>
    /// Stores upgrade informations for a scenario element type.
    /// </summary>
    class ScenarioElementTypeUpgrade :
        IAddonTypeInternal,
        IBuildingTypeInternal,
        IMissileTypeInternal,
        IUnitTypeInternal,
        IUpgradeTypeInternal,
        IScenarioElementTypeInternal,
        IScenarioElementTypeUpgrade
    {
        /// <summary>
        /// Constructs a ScenarioElementTypeUpgrade instance for the given scenario element and for the given type.
        /// </summary>
        /// <param name="metadataUpgrade">The metadata upgrade that this instance belongs to.</param>
        /// <param name="elementTypeName">The name of the element type that this instance is upgrading.</param>
        public ScenarioElementTypeUpgrade(ScenarioMetadataUpgrade metadataUpgrade, string elementTypeName)
        {
            if (metadataUpgrade == null) { throw new ArgumentNullException("metadataUpgrade"); }
            if (elementTypeName == null) { throw new ArgumentNullException("elementTypeName"); }

            this.metadataUpgrade = metadataUpgrade;
            this.elementTypeName = elementTypeName;
            this.weaponDataUpgrades = null;
            this.weaponDataUpgradesByName = null;

            this.armorModifier = new IntValueModifier();
            this.maxEnergyModifier = new IntValueModifier();
            this.sightRangeModifier = new IntValueModifier();
            this.speedModifier = new NumberValueModifier();

            this.relativeQuadCoordsInSight = null;
            this.lastKnownSightRange = -1;

            this.Reset();
        }

        #region IScenarioElementTypeInternal

        /// <see cref="IScenarioElementTypeInternal.Name"/>
        string IScenarioElementTypeInternal.Name { get { return this.originalElementType.Name; } }

        /// <see cref="IScenarioElementTypeInternal.DisplayedName"/>
        string IScenarioElementTypeInternal.DisplayedName { get { return this.originalElementType.DisplayedName; } }

        /// <see cref="IScenarioElementTypeInternal.ID"/>
        int IScenarioElementTypeInternal.ID { get { return this.originalElementType.ID; } }

        /// <see cref="IScenarioElementTypeInternal.HasOwner"/>
        bool IScenarioElementTypeInternal.HasOwner { get { return this.originalElementType.HasOwner; } }

        /// <see cref="IScenarioElementTypeInternal.ShadowSpriteIndex"/>
        int IScenarioElementTypeInternal.ShadowSpriteIndex { get { return this.originalElementType.ShadowSpriteIndex; } }

        /// <see cref="IScenarioElementTypeInternal.ShadowOffset"/>
        RCNumVector IScenarioElementTypeInternal.ShadowOffset { get { return this.originalElementType.ShadowOffset; } }

        /// <see cref="IScenarioElementTypeInternal.SpritePalette"/>
        ISpritePalette<MapDirection> IScenarioElementTypeInternal.SpritePalette { get { return this.originalElementType.SpritePalette; } }

        /// <see cref="IScenarioElementTypeInternal.HPIconPalette"/>
        ISpritePalette IScenarioElementTypeInternal.HPIconPalette { get { return this.originalElementType.HPIconPalette; } }

        /// <see cref="IScenarioElementTypeInternal.AnimationPalette"/>
        IAnimationPalette IScenarioElementTypeInternal.AnimationPalette { get { return this.originalElementType.AnimationPalette; } }

        /// <see cref="IScenarioElementTypeInternal.BuildTime"/>
        IValueRead<int> IScenarioElementTypeInternal.BuildTime { get { return this.originalElementType.BuildTime; } }

        /// <see cref="IScenarioElementTypeInternal.SupplyUsed"/>
        IValueRead<int> IScenarioElementTypeInternal.SupplyUsed { get { return this.originalElementType.SupplyUsed; } }

        /// <see cref="IScenarioElementTypeInternal.SupplyProvided"/>
        IValueRead<int> IScenarioElementTypeInternal.SupplyProvided { get { return this.originalElementType.SupplyProvided; } }

        /// <see cref="IScenarioElementTypeInternal.MineralCost"/>
        IValueRead<int> IScenarioElementTypeInternal.MineralCost { get { return this.originalElementType.MineralCost; } }

        /// <see cref="IScenarioElementTypeInternal.GasCost"/>
        IValueRead<int> IScenarioElementTypeInternal.GasCost { get { return this.originalElementType.GasCost; } }

        /// <see cref="IScenarioElementTypeInternal.Area"/>
        IValueRead<RCNumVector> IScenarioElementTypeInternal.Area { get { return this.originalElementType.Area; } }

        /// <see cref="IScenarioElementTypeInternal.Armor"/>
        IValueRead<int> IScenarioElementTypeInternal.Armor { get { return this.armorModifier.HasAttachedModifiedValue() ? this.armorModifier : null; } }

        /// <see cref="IScenarioElementTypeInternal.MaxEnergy"/>
        IValueRead<int> IScenarioElementTypeInternal.MaxEnergy { get { return this.maxEnergyModifier.HasAttachedModifiedValue() ? this.maxEnergyModifier : null; } }

        /// <see cref="IScenarioElementTypeInternal.MaxHP"/>
        IValueRead<int> IScenarioElementTypeInternal.MaxHP { get { return this.originalElementType.MaxHP; } }

        /// <see cref="IScenarioElementTypeInternal.SightRange"/>
        IValueRead<int> IScenarioElementTypeInternal.SightRange { get { return this.sightRangeModifier.HasAttachedModifiedValue() ? this.sightRangeModifier : null; } }

        /// <see cref="IScenarioElementTypeInternal.Size"/>
        IValueRead<SizeEnum> IScenarioElementTypeInternal.Size { get { return this.originalElementType.Size; } }

        /// <see cref="IScenarioElementTypeInternal.Speed"/>
        IValueRead<RCNumber> IScenarioElementTypeInternal.Speed { get { return this.speedModifier.HasAttachedModifiedValue() ? this.speedModifier : null; } }

        /// <see cref="IScenarioElementTypeInternal.StandardWeapons"/>
        IEnumerable<IWeaponData> IScenarioElementTypeInternal.StandardWeapons { get { return this.weaponDataUpgrades; } }

        /// <see cref="IScenarioElementTypeInternal.Requirements"/>
        IEnumerable<IRequirement> IScenarioElementTypeInternal.Requirements { get { return this.originalElementType.Requirements; } }

        /// <see cref="IScenarioElementTypeInternal.RelativeQuadCoordsInSight"/>
        IEnumerable<RCIntVector> IScenarioElementTypeInternal.RelativeQuadCoordsInSight
        {
            get
            {
                IValueRead<int> currentSightRangeValue = ((IScenarioElementTypeInternal)this).SightRange;
                int currentSightRange = currentSightRangeValue != null ? currentSightRangeValue.Read() : -1;
                if (currentSightRange != this.lastKnownSightRange)
                {
                    /// Recalculate the visible quadratic coordinates.
                    this.lastKnownSightRange = currentSightRange;
                    this.CalculateRelativeQuadCoordsInSight();
                }
                return this.relativeQuadCoordsInSight;
            }
        }

        /// <see cref="IScenarioElementTypeInternal.CheckPlacementConstraints"/>
        RCSet<RCIntVector> IScenarioElementTypeInternal.CheckPlacementConstraints(Scenario scenario, RCIntVector position)
        {
            return this.originalElementType.CheckPlacementConstraints(scenario, position);
        }

        /// <see cref="IScenarioElementTypeInternal.CheckPlacementConstraints"/>
        RCSet<RCIntVector> IScenarioElementTypeInternal.CheckPlacementConstraints(Entity entity, RCIntVector position)
        {
            return this.originalElementType.CheckPlacementConstraints(entity, position);
        }

        #endregion IScenarioElementTypeInternal

        #region IAddonTypeInternal

        /// <see cref="IAddonTypeInternal.HasUpgradeType"/>
        bool IAddonTypeInternal.HasUpgradeType(string upgradeTypeName) { return this.originalAddonType.HasUpgradeType(upgradeTypeName); }

        /// <see cref="IAddonTypeInternal.GetUpgradeType"/>
        IUpgradeTypeInternal IAddonTypeInternal.GetUpgradeType(string upgradeTypeName) { return this.metadataUpgrade.GetElementTypeUpgradeImpl(this.originalAddonType.GetUpgradeType(upgradeTypeName).Name); }

        /// <see cref="IAddonTypeInternal.UpgradeTypes"/>
        IEnumerable<IUpgradeTypeInternal> IAddonTypeInternal.UpgradeTypes
        {
            get
            {
                List<IUpgradeTypeInternal> retList = new List<IUpgradeTypeInternal>();
                foreach (IUpgradeTypeInternal upgradeType in this.originalAddonType.UpgradeTypes)
                {
                    retList.Add(this.metadataUpgrade.GetElementTypeUpgradeImpl(upgradeType.Name));
                }
                return retList;
            }
        }


        /// <see cref="IAddonTypeInternal.CheckPlacementConstraints"/>
        RCSet<RCIntVector> IAddonTypeInternal.CheckPlacementConstraints(Building mainBuilding, RCIntVector position) { return this.originalAddonType.CheckPlacementConstraints(mainBuilding, position); }

        #endregion IAddonTypeInternal

        #region IBuildingTypeInternal

        /// <see cref="IBuildingTypeInternal.HasAddonType"/>
        bool IBuildingTypeInternal.HasAddonType(string addonTypeName) { return this.originalBuildingType.HasAddonType(addonTypeName); }

        /// <see cref="IBuildingTypeInternal.HasUnitType"/>
        bool IBuildingTypeInternal.HasUnitType(string unitTypeName) { return this.originalBuildingType.HasUnitType(unitTypeName); }

        /// <see cref="IBuildingTypeInternal.HasUpgradeType"/>
        bool IBuildingTypeInternal.HasUpgradeType(string upgradeTypeName) { return this.originalBuildingType.HasUpgradeType(upgradeTypeName); }

        /// <see cref="IBuildingTypeInternal.GetAddonType"/>
        IAddonTypeInternal IBuildingTypeInternal.GetAddonType(string addonTypeName) { return this.metadataUpgrade.GetElementTypeUpgradeImpl(this.originalBuildingType.GetAddonType(addonTypeName).Name); }

        /// <see cref="IBuildingTypeInternal.GetUnitType"/>
        IUnitTypeInternal IBuildingTypeInternal.GetUnitType(string unitTypeName) { return this.metadataUpgrade.GetElementTypeUpgradeImpl(this.originalBuildingType.GetUnitType(unitTypeName).Name); }

        /// <see cref="IBuildingTypeInternal.GetUpgradeType"/>
        IUpgradeTypeInternal IBuildingTypeInternal.GetUpgradeType(string upgradeTypeName) { return this.metadataUpgrade.GetElementTypeUpgradeImpl(this.originalBuildingType.GetUpgradeType(upgradeTypeName).Name); }

        /// <see cref="IBuildingTypeInternal.AddonTypes"/>
        IEnumerable<IAddonTypeInternal> IBuildingTypeInternal.AddonTypes
        {
            get
            {
                List<IAddonTypeInternal> retList = new List<IAddonTypeInternal>();
                foreach (IAddonTypeInternal addonType in this.originalBuildingType.AddonTypes)
                {
                    retList.Add(this.metadataUpgrade.GetElementTypeUpgradeImpl(addonType.Name));
                }
                return retList;
            }
        }

        /// <see cref="IBuildingTypeInternal.UnitTypes"/>
        IEnumerable<IUnitTypeInternal> IBuildingTypeInternal.UnitTypes
        {
            get
            {
                List<IUnitTypeInternal> retList = new List<IUnitTypeInternal>();
                foreach (IUnitTypeInternal unitType in this.originalBuildingType.UnitTypes)
                {
                    retList.Add(this.metadataUpgrade.GetElementTypeUpgradeImpl(unitType.Name));
                }
                return retList;
            }
        }

        /// <see cref="IBuildingTypeInternal.UpgradeTypes"/>
        IEnumerable<IUpgradeTypeInternal> IBuildingTypeInternal.UpgradeTypes
        {
            get
            {
                List<IUpgradeTypeInternal> retList = new List<IUpgradeTypeInternal>();
                foreach (IUpgradeTypeInternal upgradeType in this.originalBuildingType.UpgradeTypes)
                {
                    retList.Add(this.metadataUpgrade.GetElementTypeUpgradeImpl(upgradeType.Name));
                }
                return retList;
            }
        }

        /// <see cref="IBuildingTypeInternal.CheckPlacementConstraints"/>
        RCSet<RCIntVector> IBuildingTypeInternal.CheckPlacementConstraints(Scenario scenario, RCIntVector position, IAddonTypeInternal addonType) { return this.originalBuildingType.CheckPlacementConstraints(scenario, position, addonType); }

        /// <see cref="IBuildingTypeInternal.CheckPlacementConstraints"/>
        RCSet<RCIntVector> IBuildingTypeInternal.CheckPlacementConstraints(Building building, RCIntVector position, IAddonTypeInternal addonType) { return this.originalBuildingType.CheckPlacementConstraints(building, position, addonType); }

        /// <see cref="IBuildingTypeInternal.GetPlacementSuggestions"/>
        RCSet<Tuple<RCIntRectangle, RCIntVector>> IBuildingTypeInternal.GetPlacementSuggestions(Scenario scenario, RCIntRectangle area) { return this.originalBuildingType.GetPlacementSuggestions(scenario, area); }

        /// <see cref="IBuildingTypeInternal.GetRelativeAddonPosition"/>
        RCIntVector IBuildingTypeInternal.GetRelativeAddonPosition(IMapAccess map, IAddonTypeInternal addonType) { return this.originalBuildingType.GetRelativeAddonPosition(map, addonType); }

        #endregion IBuildingTypeInternal

        #region IMissileTypeInternal

        /// <see cref="IMissileTypeInternal.LaunchAnimation"/>
        string IMissileTypeInternal.LaunchAnimation { get { return this.originalMissileType.LaunchAnimation; } }

        /// <see cref="IMissileTypeInternal.LaunchDelay"/>
        int IMissileTypeInternal.LaunchDelay { get { return this.originalMissileType.LaunchDelay; } }

        /// <see cref="IMissileTypeInternal.FlyingAnimation"/>
        string IMissileTypeInternal.FlyingAnimation { get { return this.originalMissileType.FlyingAnimation; } }

        /// <see cref="IMissileTypeInternal.TrailAnimation"/>
        string IMissileTypeInternal.TrailAnimation { get { return this.originalMissileType.TrailAnimation; } }

        /// <see cref="IMissileTypeInternal.TrailAnimationFrequency"/>
        int IMissileTypeInternal.TrailAnimationFrequency { get { return this.originalMissileType.TrailAnimationFrequency; } }

        /// <see cref="IMissileTypeInternal.ImpactAnimation"/>
        string IMissileTypeInternal.ImpactAnimation { get { return this.originalMissileType.ImpactAnimation; } }

        #endregion IMissileTypeInternal

        #region IUnitTypeInternal

        /// <see cref="IUnitTypeInternal.NecessaryAddon"/>
        IAddonTypeInternal IUnitTypeInternal.NecessaryAddon
        {
            get
            {
                if (this.originalUnitType.NecessaryAddon == null) { return null; }
                return this.metadataUpgrade.GetElementTypeUpgradeImpl(this.originalUnitType.NecessaryAddon.Name);
            }
        }

        #endregion IUnitTypeInternal

        #region IUpgradeTypeInternal

        /// <see cref="IUpgradeTypeInternal.PreviousLevel"/>
        IUpgradeTypeInternal IUpgradeTypeInternal.PreviousLevel
        {
            get
            {
                if (this.originalUpgradeType.PreviousLevel == null) { return null; }
                return this.metadataUpgrade.GetElementTypeUpgradeImpl(this.originalUpgradeType.PreviousLevel.Name);
            }
        }

        /// <see cref="IUpgradeTypeInternal.NextLevel"/>
        IUpgradeTypeInternal IUpgradeTypeInternal.NextLevel
        {
            get
            {
                if (this.originalUpgradeType.NextLevel == null) { return null; }
                return this.metadataUpgrade.GetElementTypeUpgradeImpl(this.originalUpgradeType.NextLevel.Name);
            }
        }

        #endregion IUpgradeTypeInternal

        #region IScenarioElementTypeUpgrade

        /// <see cref="IScenarioElementTypeUpgrade.ArmorUpgrade"/>
        int IScenarioElementTypeUpgrade.ArmorUpgrade
        {
            get { return this.armorModifier.Modification; }
            set { this.armorModifier.Modification = value; }
        }

        /// <see cref="IScenarioElementTypeUpgrade.CumulatedArmorUpgrade"/>
        int IScenarioElementTypeUpgrade.CumulatedArmorUpgrade
        {
            get { return this.originalUpgradeIface != null ? this.originalUpgradeIface.CumulatedArmorUpgrade + this.armorModifier.Modification : this.armorModifier.Modification; }
        }

        /// <see cref="IScenarioElementTypeUpgrade.MaxEnergyUpgrade"/>
        int IScenarioElementTypeUpgrade.MaxEnergyUpgrade
        {
            get { return this.maxEnergyModifier.Modification; }
            set { this.maxEnergyModifier.Modification = value; }
        }

        /// <see cref="IScenarioElementTypeUpgrade.CumulatedMaxEnergyUpgrade"/>
        int IScenarioElementTypeUpgrade.CumulatedMaxEnergyUpgrade
        {
            get { return this.originalUpgradeIface != null ? this.originalUpgradeIface.CumulatedMaxEnergyUpgrade + this.maxEnergyModifier.Modification : this.maxEnergyModifier.Modification; }
        }

        /// <see cref="IScenarioElementTypeUpgrade.SightRangeUpgrade"/>
        int IScenarioElementTypeUpgrade.SightRangeUpgrade
        {
            get { return this.sightRangeModifier.Modification; }
            set { this.sightRangeModifier.Modification = value; }
        }

        /// <see cref="IScenarioElementTypeUpgrade.CumulatedSightRangeUpgrade"/>
        int IScenarioElementTypeUpgrade.CumulatedSightRangeUpgrade
        {
            get { return this.originalUpgradeIface != null ? this.originalUpgradeIface.CumulatedSightRangeUpgrade + this.sightRangeModifier.Modification : this.sightRangeModifier.Modification; }
        }

        /// <see cref="IScenarioElementTypeUpgrade.SpeedUpgrade"/>
        RCNumber IScenarioElementTypeUpgrade.SpeedUpgrade
        {
            get { return this.speedModifier.Modification; }
            set { this.speedModifier.Modification = value; }
        }

        /// <see cref="IScenarioElementTypeUpgrade.CumulatedSpeedUpgrade"/>
        RCNumber IScenarioElementTypeUpgrade.CumulatedSpeedUpgrade
        {
            get { return this.originalUpgradeIface != null ? this.originalUpgradeIface.CumulatedSpeedUpgrade + this.speedModifier.Modification : this.speedModifier.Modification; }
        }

        /// <see cref="IScenarioElementTypeUpgrade.WeaponUpgrades"/>
        IEnumerable<IWeaponDataUpgrade> IScenarioElementTypeUpgrade.WeaponUpgrades { get { return this.weaponDataUpgrades; } }

        #endregion IScenarioElementTypeUpgrade

        #region Internal public methods

        /// <summary>
        /// Resets this instance.
        /// </summary>
        internal void Reset()
        {
            this.originalElementType = null;
            this.originalAddonType = null;
            this.originalBuildingType = null;
            this.originalMissileType = null;
            this.originalUnitType = null;
            this.originalUpgradeType = null;
            this.originalUpgradeIface = null;

            this.armorModifier.AttachModifiedValue(null);
            this.maxEnergyModifier.AttachModifiedValue(null);
            this.sightRangeModifier.AttachModifiedValue(null);
            this.speedModifier.AttachModifiedValue(null);

            if (this.metadataUpgrade.AttachedMetadata != null)
            {
                IScenarioMetadata metadata = this.metadataUpgrade.AttachedMetadata;
                if (metadata.HasCustomType(this.elementTypeName)) { this.originalElementType = metadata.GetCustomType(this.elementTypeName).ElementTypeImpl; }
                else if (metadata.HasAddonType(this.elementTypeName)) { this.originalElementType = this.originalAddonType = metadata.GetAddonType(this.elementTypeName).AddonTypeImpl; }
                else if (metadata.HasBuildingType(this.elementTypeName)) { this.originalElementType = this.originalBuildingType = metadata.GetBuildingType(this.elementTypeName).BuildingTypeImpl; }
                else if (metadata.HasMissileType(this.elementTypeName)) { this.originalElementType = this.originalMissileType = metadata.GetMissileType(this.elementTypeName).MissileTypeImpl; }
                else if (metadata.HasUnitType(this.elementTypeName)) { this.originalElementType = this.originalUnitType = metadata.GetUnitType(this.elementTypeName).UnitTypeImpl; }
                else if (metadata.HasUpgradeType(this.elementTypeName)) { this.originalElementType = this.originalUpgradeType = metadata.GetUpgradeType(this.elementTypeName).UpgradeTypeImpl; }
                else
                {
                    throw new InvalidOperationException(string.Format("Scenario element type '{0}' is not defined in the metadata!", this.elementTypeName));
                }

                this.originalUpgradeIface = this.originalElementType as IScenarioElementTypeUpgrade;

                this.armorModifier.AttachModifiedValue(this.originalElementType.Armor);
                this.maxEnergyModifier.AttachModifiedValue(this.originalElementType.MaxEnergy);
                this.sightRangeModifier.AttachModifiedValue(this.originalElementType.SightRange);
                this.speedModifier.AttachModifiedValue(this.originalElementType.Speed);

                if (this.weaponDataUpgrades == null)
                {
                    /// First time attach -> create the weapon data upgrades.
                    this.weaponDataUpgrades = new List<WeaponDataUpgrade>();
                    this.weaponDataUpgradesByName = new Dictionary<string, WeaponDataUpgrade>();
                    foreach (IWeaponData weaponData in this.originalElementType.StandardWeapons)
                    {
                        WeaponDataUpgrade weaponUpgrade = new WeaponDataUpgrade(this.metadataUpgrade, weaponData);
                        this.weaponDataUpgrades.Add(weaponUpgrade);
                        this.weaponDataUpgradesByName.Add(weaponData.Name, weaponUpgrade);
                    }
                }
                else
                {
                    /// Reattach -> check the compatibility of the existing weapon data upgrades and reset them.
                    if (this.weaponDataUpgrades.Count != this.originalElementType.StandardWeapons.Count()) { throw new InvalidOperationException("Unable to attach non-compatible scenario element type!"); }
                    foreach (IWeaponData weaponData in this.originalElementType.StandardWeapons)
                    {
                        if (!this.weaponDataUpgradesByName.ContainsKey(weaponData.Name)) { throw new InvalidOperationException("Unable to attach non-compatible scenario element type!"); }
                        this.weaponDataUpgradesByName[weaponData.Name].Reset(weaponData);
                    }
                }
            }
        }

        #endregion Internal public methods

        /// <summary>
        /// Calculates the list of visible quadratic coordinates from the last known value of the sight range.
        /// </summary>
        private void CalculateRelativeQuadCoordsInSight()
        {
            if (this.lastKnownSightRange == -1) { this.relativeQuadCoordsInSight = null; }
            else
            {
                RCIntVector nullVector = new RCIntVector(0, 0);
                this.relativeQuadCoordsInSight = new RCSet<RCIntVector>();
                for (int x = -this.lastKnownSightRange; x <= this.lastKnownSightRange; x++)
                {
                    for (int y = -this.lastKnownSightRange; y <= this.lastKnownSightRange; y++)
                    {
                        RCIntVector quadCoord = new RCIntVector(x, y);
                        if (MapUtils.ComputeDistance(nullVector, quadCoord) < this.lastKnownSightRange)
                        {
                            this.relativeQuadCoordsInSight.Add(quadCoord);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reference to metadata upgrade that this instance belongs to.
        /// </summary>
        private ScenarioMetadataUpgrade metadataUpgrade;

        /// <summary>
        /// The name of the element type that this instance is upgrading.
        /// </summary>
        private string elementTypeName;

        /// <summary>
        /// Reference to the upgraded scenario element type.
        /// </summary>
        private IScenarioElementTypeInternal originalElementType;
        private IAddonTypeInternal originalAddonType;
        private IBuildingTypeInternal originalBuildingType;
        private IMissileTypeInternal originalMissileType;
        private IUnitTypeInternal originalUnitType;
        private IUpgradeTypeInternal originalUpgradeType;
        private IScenarioElementTypeUpgrade originalUpgradeIface;

        /// <summary>
        /// Modifier instances for the upgradable values of the underlying scenario element type.
        /// </summary>
        private ValueModifier<int> armorModifier;
        private ValueModifier<int> maxEnergyModifier;
        private ValueModifier<int> sightRangeModifier;
        private ValueModifier<RCNumber> speedModifier;

        /// <summary>
        /// List of the upgrades of the weapons of the underlying scenario element type.
        /// </summary>
        private List<WeaponDataUpgrade> weaponDataUpgrades;
        private Dictionary<string, WeaponDataUpgrade> weaponDataUpgradesByName;

        /// <summary>
        /// The quadratic coordinates relative to the origin that are inside the sight range or null if the underlying element type has no sight range defined.
        /// </summary>
        private RCSet<RCIntVector> relativeQuadCoordsInSight;

        /// <summary>
        /// The last known value of the sight range or -1 if the underlying element type has no sight range defined.
        /// </summary>
        private int lastKnownSightRange;
    }
}
