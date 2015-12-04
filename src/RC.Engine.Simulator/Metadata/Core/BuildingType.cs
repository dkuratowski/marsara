using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Engine;
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
    class BuildingType : ScenarioElementType, IBuildingTypeInternal
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
            this.suggestionProviders = new List<BuildingPlacementSuggestionProvider>();
        }

        #region IBuildingTypeInternal members

        /// <see cref="IBuildingTypeInternal.HasAddonType"/>
        public bool HasAddonType(string addonTypeName)
        {
            if (addonTypeName == null) { throw new ArgumentNullException("addonTypeName"); }
            return this.addonTypes.ContainsKey(addonTypeName);
        }

        /// <see cref="IBuildingTypeInternal.HasUnitType"/>
        public bool HasUnitType(string unitTypeName)
        {
            if (unitTypeName == null) { throw new ArgumentNullException("unitTypeName"); }
            return this.unitTypes.ContainsKey(unitTypeName);
        }

        /// <see cref="IBuildingTypeInternal.HasUpgradeType"/>
        public bool HasUpgradeType(string upgradeTypeName)
        {
            if (upgradeTypeName == null) { throw new ArgumentNullException("upgradeTypeName"); }
            return this.upgradeTypes.ContainsKey(upgradeTypeName);
        }

        /// <see cref="IBuildingTypeInternal.GetAddonType"/>
        public IAddonTypeInternal GetAddonType(string addonTypeName)
        {
            return this.GetAddonTypeImpl(addonTypeName);
        }

        /// <see cref="IBuildingTypeInternal.GetUnitType"/>
        public IUnitTypeInternal GetUnitType(string unitTypeName)
        {
            return this.GetUnitTypeImpl(unitTypeName);
        }

        /// <see cref="IBuildingTypeInternal.GetUpgradeType"/>
        public IUpgradeTypeInternal GetUpgradeType(string upgradeTypeName)
        {
            return this.GetUpgradeTypeImpl(upgradeTypeName);
        }

        /// <see cref="IBuildingTypeInternal.AddonTypes"/>
        public IEnumerable<IAddonTypeInternal> AddonTypes { get { return this.addonTypes.Values; } }

        /// <see cref="IBuildingTypeInternal.UnitTypes"/>
        public IEnumerable<IUnitTypeInternal> UnitTypes { get { return this.unitTypes.Values; } }

        /// <see cref="IBuildingTypeInternal.UpgradeTypes"/>
        public IEnumerable<IUpgradeTypeInternal> UpgradeTypes { get { return this.upgradeTypes.Values; } }

        /// <see cref="IBuildingTypeInternal.CheckPlacementConstraints"/>
        public RCSet<RCIntVector> CheckPlacementConstraints(Scenario scenario, RCIntVector position, IAddonTypeInternal addonType)
        {
            if (addonType == null) { throw new ArgumentNullException("addonType"); }
            if (!this.HasAddonType(addonType.Name)) { throw new ArgumentException(string.Format("Building type '{0}' is not defined as the main building for addon type '{1}'!", this.Name, addonType.Name)); }

            RCSet<RCIntVector> retList = this.CheckPlacementConstraints(scenario, position);

            RCIntVector relativeAddonPos = this.GetRelativeAddonPosition(scenario.Map, addonType);
            RCIntVector addonPos = position + relativeAddonPos;

            foreach (RCIntVector quadCoordViolatingByAddon in addonType.CheckPlacementConstraints(scenario, addonPos))
            {
                retList.Add(quadCoordViolatingByAddon + relativeAddonPos);
            }
            
            return retList;
        }

        /// <see cref="IBuildingTypeInternal.CheckPlacementConstraints"/>
        public RCSet<RCIntVector> CheckPlacementConstraints(Building building, RCIntVector position, IAddonTypeInternal addonType)
        {
            if (addonType == null) { throw new ArgumentNullException("addonType"); }
            if (!this.HasAddonType(addonType.Name)) { throw new ArgumentException(string.Format("Building type '{0}' is not defined as the main building for addon type '{1}'!", this.Name, addonType.Name)); }

            RCSet<RCIntVector> retList = this.CheckPlacementConstraints(building, position);

            RCIntVector relativeAddonPos = this.GetRelativeAddonPosition(building.Scenario.Map, addonType);
            RCIntVector addonPos = position + relativeAddonPos;

            foreach (RCIntVector quadCoordViolatingByAddon in addonType.CheckPlacementConstraints(building, addonPos))
            {
                retList.Add(quadCoordViolatingByAddon + relativeAddonPos);
            }

            return retList;
        }

        /// <see cref="IBuildingTypeInternal.GetPlacementSuggestions"/>
        public RCSet<Tuple<RCIntRectangle, RCIntVector>> GetPlacementSuggestions(Scenario scenario, RCIntRectangle area)
        {
            if (scenario == null) { throw new ArgumentNullException("scenario"); }
            if (area == RCIntRectangle.Undefined) { throw new ArgumentNullException("area"); }

            /// Get suggestions from the providers defined by this building type.
            RCSet<Tuple<RCIntRectangle, RCIntVector>> retList = new RCSet<Tuple<RCIntRectangle, RCIntVector>>();
            foreach (BuildingPlacementSuggestionProvider suggestionProvider in this.suggestionProviders)
            {
                retList.UnionWith(suggestionProvider.GetSuggestions(scenario, area));
            }

            return retList;
        }

        /// <see cref="IBuildingTypeInternal.GetRelativeAddonPosition"/>
        public RCIntVector GetRelativeAddonPosition(IMapAccess map, IAddonTypeInternal addonType)
        {
            if (map == null) { throw new ArgumentNullException("map"); }
            if (addonType == null) { throw new ArgumentNullException("addonType"); }
            if (!this.HasAddonType(addonType.Name)) { throw new ArgumentException(string.Format("Building type '{0}' is not defined as the main building for addon type '{1}'!", this.Name, addonType.Name)); }

            RCIntVector buildingQuadSize = map.CellToQuadSize(this.Area.Read());
            int addonQuadHeight = map.CellToQuadSize(addonType.Area.Read()).Y;
            return new RCIntVector(buildingQuadSize.X, buildingQuadSize.Y - addonQuadHeight);
        }

        #endregion IBuildingTypeInternal members

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

        /// <summary>
        /// Adds a placement suggestion provider to this building type.
        /// </summary>
        /// <param name="suggestionProvider">The placement suggestion provider to add.</param>
        public void AddPlacementSuggestionProvider(BuildingPlacementSuggestionProvider suggestionProvider)
        {
            if (this.Metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (suggestionProvider == null) { throw new ArgumentNullException("suggestionProvider"); }

            suggestionProvider.SetBuildingType(new IBuildingType(this));
            this.suggestionProviders.Add(suggestionProvider);
        }

        #endregion BuildingType buildup methods

        /// <summary>
        /// List of the unit types that are created in buildings of this type mapped by their names.
        /// </summary>
        private readonly Dictionary<string, UnitType> unitTypes;

        /// <summary>
        /// List of the addon types that are created by buildings of this type mapped by their names.
        /// </summary>
        private readonly Dictionary<string, AddonType> addonTypes;

        /// <summary>
        /// List of the upgrade types that are performed in buildings of this type mapped by their names.
        /// </summary>
        private readonly Dictionary<string, UpgradeType> upgradeTypes;

        /// <summary>
        /// List of the placement suggestion providers of this building type.
        /// </summary>
        private readonly List<BuildingPlacementSuggestionProvider> suggestionProviders;
    }
}
