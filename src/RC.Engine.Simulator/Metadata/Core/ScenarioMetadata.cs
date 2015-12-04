using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Common.Configuration;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Simulator.Metadata.Core
{
    /// <summary>
    /// Represents the metadata informations for RC scenarios
    /// </summary>
    class ScenarioMetadata : IScenarioMetadata
    {
        /// <summary>
        /// Constructs a ScenarioMetadata object.
        /// </summary>
        public ScenarioMetadata()
        {
            this.isFinalized = false;
            this.buildingTypes = new Dictionary<string, BuildingType>();
            this.addonTypes = new Dictionary<string, AddonType>();
            this.unitTypes = new Dictionary<string, UnitType>();
            this.upgradeTypes = new Dictionary<string, UpgradeType>();
            this.missileTypes = new Dictionary<string, MissileType>();
            this.customTypes = new Dictionary<string, ScenarioElementType>();
            this.allTypes = new List<ScenarioElementType>();
            this.shadowPalette = null;
        }

        #region IScenarioMetadata members

        /// <see cref="IScenarioMetadata.HasUnitType"/>
        public bool HasUnitType(string unitTypeName)
        {
            if (unitTypeName == null) { throw new ArgumentNullException("unitTypeName"); }
            return this.unitTypes.ContainsKey(unitTypeName);
        }

        /// <see cref="IScenarioMetadata.HasUpgradeType"/>
        public bool HasUpgradeType(string upgradeTypeName)
        {
            if (upgradeTypeName == null) { throw new ArgumentNullException("upgradeTypeName"); }
            return this.upgradeTypes.ContainsKey(upgradeTypeName);
        }

        /// <see cref="IScenarioMetadata.HasBuildingType"/>
        public bool HasBuildingType(string buildingTypeName)
        {
            if (buildingTypeName == null) { throw new ArgumentNullException("buildingTypeName"); }
            return this.buildingTypes.ContainsKey(buildingTypeName);
        }

        /// <see cref="IScenarioMetadata.HasAddonType"/>
        public bool HasAddonType(string addonTypeName)
        {
            if (addonTypeName == null) { throw new ArgumentNullException("addonTypeName"); }
            return this.addonTypes.ContainsKey(addonTypeName);
        }

        /// <see cref="IScenarioMetadata.HasMissileType"/>
        public bool HasMissileType(string missileTypeName)
        {
            if (missileTypeName == null) { throw new ArgumentNullException("missileTypeName"); }
            return this.missileTypes.ContainsKey(missileTypeName);
        }

        /// <see cref="IScenarioMetadata.HasCustomType"/>
        public bool HasCustomType(string customTypeName)
        {
            if (customTypeName == null) { throw new ArgumentNullException("customTypeName"); }
            return this.customTypes.ContainsKey(customTypeName);
        }

        /// <see cref="IScenarioMetadata.HasElementType"/>
        public bool HasElementType(string typeName)
        {
            if (typeName == null) { throw new ArgumentNullException("typeName"); }

            return this.unitTypes.ContainsKey(typeName) ||
                   this.upgradeTypes.ContainsKey(typeName) ||
                   this.buildingTypes.ContainsKey(typeName) ||
                   this.addonTypes.ContainsKey(typeName) ||
                   this.missileTypes.ContainsKey(typeName) ||
                   this.customTypes.ContainsKey(typeName);
        }

        /// <see cref="IScenarioMetadata.GetUnitType"/>
        public IUnitType GetUnitType(string unitTypeName)
        {
            return new IUnitType(this.GetUnitTypeImpl(unitTypeName));
        }

        /// <see cref="IScenarioMetadata.GetUpgradeType"/>
        public IUpgradeType GetUpgradeType(string upgradeTypeName)
        {
            return new IUpgradeType(this.GetUpgradeTypeImpl(upgradeTypeName));
        }

        /// <see cref="IScenarioMetadata.GetBuildingType"/>
        public IBuildingType GetBuildingType(string buildingTypeName)
        {
            return new IBuildingType(this.GetBuildingTypeImpl(buildingTypeName));
        }

        /// <see cref="IScenarioMetadata.GetAddonType"/>
        public IAddonType GetAddonType(string addonTypeName)
        {
            return new IAddonType(this.GetAddonTypeImpl(addonTypeName));
        }

        /// <see cref="IScenarioMetadata.GetMissileType"/>
        public IMissileType GetMissileType(string missileTypeName)
        {
            return new IMissileType(this.GetMissileTypeImpl(missileTypeName));
        }

        /// <see cref="IScenarioMetadata.GetCustomType"/>
        public IScenarioElementType GetCustomType(string customTypeName)
        {
            return new IScenarioElementType(this.GetCustomTypeImpl(customTypeName));
        }

        /// <see cref="IScenarioMetadata.GetElementType"/>
        public IScenarioElementType GetElementType(string typeName)
        {
            return new IScenarioElementType(this.GetElementTypeImpl(typeName));
        }

        /// <see cref="IScenarioMetadata.UnitTypes"/>
        public IEnumerable<IUnitType> UnitTypes
        {
            get
            {
                List<IUnitType> retList = new List<IUnitType>();
                foreach (IUnitTypeInternal unitType in this.unitTypes.Values) { retList.Add(new IUnitType(unitType)); }
                return retList;
            }
        }

        /// <see cref="IScenarioMetadata.UpgradeTypes"/>
        public IEnumerable<IUpgradeType> UpgradeTypes
        {
            get
            {
                List<IUpgradeType> retList = new List<IUpgradeType>();
                foreach (IUpgradeTypeInternal upgradeType in this.upgradeTypes.Values) { retList.Add(new IUpgradeType(upgradeType)); }
                return retList;
            }
        }
        
        /// <see cref="IScenarioMetadata.BuildingTypes"/>
        public IEnumerable<IBuildingType> BuildingTypes
        {
            get
            {
                List<IBuildingType> retList = new List<IBuildingType>();
                foreach (IBuildingTypeInternal buildingType in this.buildingTypes.Values) { retList.Add(new IBuildingType(buildingType)); }
                return retList;
            }
        }

        /// <see cref="IScenarioMetadata.AddonTypes"/>
        public IEnumerable<IAddonType> AddonTypes
        {
            get
            {
                List<IAddonType> retList = new List<IAddonType>();
                foreach (IAddonTypeInternal addonType in this.addonTypes.Values) { retList.Add(new IAddonType(addonType)); }
                return retList;
            }
        }

        /// <see cref="IScenarioMetadata.MissileTypes"/>
        public IEnumerable<IMissileType> MissileTypes
        {
            get
            {
                List<IMissileType> retList = new List<IMissileType>();
                foreach (IMissileTypeInternal missileType in this.missileTypes.Values) { retList.Add(new IMissileType(missileType)); }
                return retList;
            }
        }

        /// <see cref="IScenarioMetadata.CustomTypes"/>
        public IEnumerable<IScenarioElementType> CustomTypes
        {
            get
            {
                List<IScenarioElementType> retList = new List<IScenarioElementType>();
                foreach (IScenarioElementTypeInternal elementType in this.customTypes.Values) { retList.Add(new IScenarioElementType(elementType)); }
                return retList;
            }
        }

        /// <see cref="IScenarioMetadata.AllTypes"/>
        public IEnumerable<IScenarioElementType> AllTypes
        {
            get
            {
                List<IScenarioElementType> retList = new List<IScenarioElementType>();
                foreach (IScenarioElementTypeInternal elementType in this.allTypes) { retList.Add(new IScenarioElementType(elementType)); }
                return retList;
            }
        }

        /// <see cref="IScenarioMetadata.this[]"/>
        public IScenarioElementType this[int typeID] { get { return new IScenarioElementType(this.allTypes[typeID]); } }

        /// <see cref="IScenarioMetadata.ShadowPalette"/>
        public ISpritePalette ShadowPalette { get { return this.shadowPalette; } }

        #endregion IScenarioMetadata members

        #region Internal public methods

        /// <see cref="IScenarioMetadata.GetUnitType"/>
        public UnitType GetUnitTypeImpl(string unitTypeName)
        {
            if (unitTypeName == null) { throw new ArgumentNullException("unitTypeName"); }
            return this.unitTypes[unitTypeName];
        }

        /// <see cref="IScenarioMetadata.GetUpgradeType"/>
        public UpgradeType GetUpgradeTypeImpl(string upgradeTypeName)
        {
            if (upgradeTypeName == null) { throw new ArgumentNullException("upgradeTypeName"); }
            return this.upgradeTypes[upgradeTypeName];
        }

        /// <see cref="IScenarioMetadata.GetBuildingType"/>
        public BuildingType GetBuildingTypeImpl(string buildingTypeName)
        {
            if (buildingTypeName == null) { throw new ArgumentNullException("buildingTypeName"); }
            return this.buildingTypes[buildingTypeName];
        }

        /// <see cref="IScenarioMetadata.GetAddonType"/>
        public AddonType GetAddonTypeImpl(string addonTypeName)
        {
            if (addonTypeName == null) { throw new ArgumentNullException("addonTypeName"); }
            return this.addonTypes[addonTypeName];
        }

        /// <see cref="IScenarioMetadata.GetMissileType"/>
        public MissileType GetMissileTypeImpl(string missileTypeName)
        {
            if (missileTypeName == null) { throw new ArgumentNullException("missileTypeName"); }
            return this.missileTypes[missileTypeName];
        }

        /// <see cref="IScenarioMetadata.GetCustomType"/>
        public ScenarioElementType GetCustomTypeImpl(string customTypeName)
        {
            if (customTypeName == null) { throw new ArgumentNullException("customTypeName"); }
            return this.customTypes[customTypeName];
        }

        /// <see cref="IScenarioMetadata.GetElementType"/>
        public ScenarioElementType GetElementTypeImpl(string typeName)
        {
            if (typeName == null) { throw new ArgumentNullException("typeName"); }
            if (this.unitTypes.ContainsKey(typeName)) { return this.unitTypes[typeName]; }
            else if (this.upgradeTypes.ContainsKey(typeName)) { return this.upgradeTypes[typeName]; }
            else if (this.buildingTypes.ContainsKey(typeName)) { return this.buildingTypes[typeName]; }
            else if (this.addonTypes.ContainsKey(typeName)) { return this.addonTypes[typeName]; }
            else if (this.missileTypes.ContainsKey(typeName)) { return this.missileTypes[typeName]; }
            else if (this.customTypes.ContainsKey(typeName)) { return this.customTypes[typeName]; }
            else { throw new SimulatorException(string.Format("Scenario element type '{0}' doesn't exist!", typeName)); }
        }

        #endregion Internal public methods

        #region ScenarioMetadata buildup methods

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
            if (this.isFinalized) { throw new InvalidOperationException("ScenarioMetadata object already finalized!"); }
            if (buildingType == null) { throw new ArgumentNullException("buildingType"); }
            if (this.buildingTypes.ContainsKey(buildingType.Name) ||
                this.unitTypes.ContainsKey(buildingType.Name) ||
                this.addonTypes.ContainsKey(buildingType.Name) ||
                this.upgradeTypes.ContainsKey(buildingType.Name) ||
                this.missileTypes.ContainsKey(buildingType.Name) ||
                this.customTypes.ContainsKey(buildingType.Name)) { throw new InvalidOperationException(string.Format("ScenarioMetadata element with name '{0}' already defined!", buildingType.Name)); }

            this.buildingTypes.Add(buildingType.Name, buildingType);
            buildingType.SetID(this.allTypes.Count);
            this.allTypes.Add(buildingType);
        }

        /// <summary>
        /// Adds a unit type definition to the metadata.
        /// </summary>
        /// <param name="unitType">The unit type definition to add.</param>
        public void AddUnitType(UnitType unitType)
        {
            if (this.isFinalized) { throw new InvalidOperationException("ScenarioMetadata object already finalized!"); }
            if (unitType == null) { throw new ArgumentNullException("unitType"); }
            if (this.buildingTypes.ContainsKey(unitType.Name) ||
                this.unitTypes.ContainsKey(unitType.Name) ||
                this.addonTypes.ContainsKey(unitType.Name) ||
                this.upgradeTypes.ContainsKey(unitType.Name) ||
                this.missileTypes.ContainsKey(unitType.Name) ||
                this.customTypes.ContainsKey(unitType.Name)) { throw new InvalidOperationException(string.Format("ScenarioMetadata element with name '{0}' already defined!", unitType.Name)); }

            this.unitTypes.Add(unitType.Name, unitType);
            unitType.SetID(this.allTypes.Count);
            this.allTypes.Add(unitType);
        }

        /// <summary>
        /// Adds an addon type definition to the metadata.
        /// </summary>
        /// <param name="addonType">The addon type definition to add.</param>
        public void AddAddonType(AddonType addonType)
        {
            if (this.isFinalized) { throw new InvalidOperationException("ScenarioMetadata object already finalized!"); }
            if (addonType == null) { throw new ArgumentNullException("addonType"); }
            if (this.buildingTypes.ContainsKey(addonType.Name) ||
                this.unitTypes.ContainsKey(addonType.Name) ||
                this.addonTypes.ContainsKey(addonType.Name) ||
                this.upgradeTypes.ContainsKey(addonType.Name) ||
                this.missileTypes.ContainsKey(addonType.Name) ||
                this.customTypes.ContainsKey(addonType.Name)) { throw new InvalidOperationException(string.Format("ScenarioMetadata element with name '{0}' already defined!", addonType.Name)); }

            this.addonTypes.Add(addonType.Name, addonType);
            addonType.SetID(this.allTypes.Count);
            this.allTypes.Add(addonType);
        }

        /// <summary>
        /// Adds an upgrade type definition to the metadata.
        /// </summary>
        /// <param name="upgradeType">The upgrade type definition to add.</param>
        public void AddUpgradeType(UpgradeType upgradeType)
        {
            if (this.isFinalized) { throw new InvalidOperationException("ScenarioMetadata object already finalized!"); }
            if (upgradeType == null) { throw new ArgumentNullException("upgradeType"); }
            if (this.buildingTypes.ContainsKey(upgradeType.Name) ||
                this.unitTypes.ContainsKey(upgradeType.Name) ||
                this.addonTypes.ContainsKey(upgradeType.Name) ||
                this.upgradeTypes.ContainsKey(upgradeType.Name) ||
                this.missileTypes.ContainsKey(upgradeType.Name) ||
                this.customTypes.ContainsKey(upgradeType.Name)) { throw new InvalidOperationException(string.Format("ScenarioMetadata element with name '{0}' already defined!", upgradeType.Name)); }

            this.upgradeTypes.Add(upgradeType.Name, upgradeType);
            upgradeType.SetID(this.allTypes.Count);
            this.allTypes.Add(upgradeType);
        }

        /// <summary>
        /// Adds a missile type definition to the metadata.
        /// </summary>
        /// <param name="missileType">The missile type definition to add.</param>
        public void AddMissileType(MissileType missileType)
        {
            if (this.isFinalized) { throw new InvalidOperationException("ScenarioMetadata object already finalized!"); }
            if (missileType == null) { throw new ArgumentNullException("missileType"); }
            if (this.buildingTypes.ContainsKey(missileType.Name) ||
                this.unitTypes.ContainsKey(missileType.Name) ||
                this.addonTypes.ContainsKey(missileType.Name) ||
                this.upgradeTypes.ContainsKey(missileType.Name) ||
                this.missileTypes.ContainsKey(missileType.Name) ||
                this.customTypes.ContainsKey(missileType.Name)) { throw new InvalidOperationException(string.Format("ScenarioMetadata element with name '{0}' already defined!", missileType.Name)); }

            this.missileTypes.Add(missileType.Name, missileType);
            missileType.SetID(this.allTypes.Count);
            this.allTypes.Add(missileType);
        }

        /// <summary>
        /// Adds a custom type definition to the metadata.
        /// </summary>
        /// <param name="customType">The custom type definition to add.</param>
        public void AddCustomType(ScenarioElementType customType)
        {
            if (this.isFinalized) { throw new InvalidOperationException("ScenarioMetadata object already finalized!"); }
            if (customType == null) { throw new ArgumentNullException("customType"); }
            if (this.buildingTypes.ContainsKey(customType.Name) ||
                this.unitTypes.ContainsKey(customType.Name) ||
                this.addonTypes.ContainsKey(customType.Name) ||
                this.upgradeTypes.ContainsKey(customType.Name) ||
                this.missileTypes.ContainsKey(customType.Name) ||
                this.customTypes.ContainsKey(customType.Name)) { throw new InvalidOperationException(string.Format("ScenarioMetadata element with name '{0}' already defined!", customType.Name)); }

            this.customTypes.Add(customType.Name, customType);
            customType.SetID(this.allTypes.Count);
            this.allTypes.Add(customType);
        }

        /// <summary>
        /// Sets the shadow palette for this metadata.
        /// </summary>
        /// <param name="shadowPalette">The shadow palette for this metadata.</param>
        public void SetShadowPalette(ISpritePalette shadowPalette)
        {
            if (this.isFinalized) { throw new InvalidOperationException("ScenarioMetadata object already finalized!"); }
            if (shadowPalette == null) { throw new ArgumentNullException("shadowPalette"); }
            this.shadowPalette = shadowPalette;
        }

        /// <summary>
        /// Checks and finalizes the metadata object. Buildup methods will be unavailable after calling this method.
        /// </summary>
        public void CheckAndFinalize()
        {
            /// Buildup the references of the missile types.
            foreach (MissileType missileType in this.missileTypes.Values) { missileType.BuildupReferences(); }

            /// Buildup the references of the upgrade types.
            foreach (UpgradeType upgradeType in this.upgradeTypes.Values) { upgradeType.BuildupReferences(); }

            /// Buildup the references of the addon types.
            foreach (AddonType addonType in this.addonTypes.Values) { addonType.BuildupReferences(); }

            /// Buildup the references of the unit types.
            foreach (UnitType unitType in this.unitTypes.Values) { unitType.BuildupReferences(); }

            /// Buildup the references of the building types.
            foreach (BuildingType buildingType in this.buildingTypes.Values) { buildingType.BuildupReferences(); }

            /// Buildup the references of the custom types.
            foreach (ScenarioElementType customType in this.customTypes.Values) { customType.BuildupReferences(); }

            /// Finalize all object types and set the sprite palette indices.
            int currSpritePaletteIdx = 0;
            int currHPIconPaletteIdx = 0;
            foreach (ScenarioElementType objType in this.allTypes)
            {
                if (objType.SpritePalette != null)
                {
                    objType.SpritePalette.SetIndex(currSpritePaletteIdx);
                    currSpritePaletteIdx++;
                }

                if (objType.HPIconPalette != null)
                {
                    objType.HPIconPalette.SetIndex(currHPIconPaletteIdx);
                    currHPIconPaletteIdx++;
                }

                objType.CheckAndFinalize();
            }
            this.shadowPalette.SetIndex(0);

            this.isFinalized = true;
        }

        #endregion ScenarioMetadata buildup methods

        /// <summary>
        /// List of the defined building types mapped by their names.
        /// </summary>
        private readonly Dictionary<string, BuildingType> buildingTypes;

        /// <summary>
        /// List of the defined addon types mapped by their names.
        /// </summary>
        private readonly Dictionary<string, AddonType> addonTypes;

        /// <summary>
        /// List of the defined unit types mapped by their names.
        /// </summary>
        private readonly Dictionary<string, UnitType> unitTypes;

        /// <summary>
        /// List of the defined upgrade types mapped by their names.
        /// </summary>
        private readonly Dictionary<string, UpgradeType> upgradeTypes;

        /// <summary>
        /// List of the defined missile types mapped by their names.
        /// </summary>
        private readonly Dictionary<string, MissileType> missileTypes;

        /// <summary>
        /// List of the defined custom types mapped by their names.
        /// </summary>
        private readonly Dictionary<string, ScenarioElementType> customTypes;

        /// <summary>
        /// List of all defined types mapped by their IDs.
        /// </summary>
        private readonly List<ScenarioElementType> allTypes;

        /// <summary>
        /// The shadow palette defined for this metadata or null if no shadow palette has been defined.
        /// </summary>
        private ISpritePalette shadowPalette;

        /// <summary>
        /// Indicates whether this metadata object has been finalized or not.
        /// </summary>
        private bool isFinalized;
    }
}
