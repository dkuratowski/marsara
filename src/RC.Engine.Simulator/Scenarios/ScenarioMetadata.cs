using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Common.Configuration;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Simulator.Scenarios
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
            this.customTypes = new Dictionary<string, ScenarioElementType>();
            this.allTypes = new List<ScenarioElementType>();
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

        /// <see cref="IScenarioMetadata.HasCustomType"/>
        public bool HasCustomType(string customTypeName)
        {
            if (customTypeName == null) { throw new ArgumentNullException("customTypeName"); }
            return this.customTypes.ContainsKey(customTypeName);
        }

        /// <see cref="IScenarioMetadata.GetUnitType"/>
        public IUnitType GetUnitType(string unitTypeName)
        {
            return this.GetUnitTypeImpl(unitTypeName);
        }

        /// <see cref="IScenarioMetadata.GetUpgradeType"/>
        public IUpgradeType GetUpgradeType(string upgradeTypeName)
        {
            return this.GetUpgradeTypeImpl(upgradeTypeName);
        }

        /// <see cref="IScenarioMetadata.GetBuildingType"/>
        public IBuildingType GetBuildingType(string buildingTypeName)
        {
            return this.GetBuildingTypeImpl(buildingTypeName);
        }

        /// <see cref="IScenarioMetadata.GetAddonType"/>
        public IAddonType GetAddonType(string addonTypeName)
        {
            return this.GetAddonTypeImpl(addonTypeName);
        }

        /// <see cref="IScenarioMetadata.GetAddonType"/>
        public IScenarioElementType GetCustomType(string customTypeName)
        {
            return this.GetCustomTypeImpl(customTypeName);
        }

        /// <see cref="IScenarioMetadata.GetElementType"/>
        public IScenarioElementType GetElementType(string typeName)
        {
            return this.GetElementTypeImpl(typeName);
        }

        /// <see cref="IScenarioMetadata.UnitTypes"/>
        public IEnumerable<IUnitType> UnitTypes { get { return this.unitTypes.Values; } }

        /// <see cref="IScenarioMetadata.UpgradeTypes"/>
        public IEnumerable<IUpgradeType> UpgradeTypes { get { return this.upgradeTypes.Values; } }

        /// <see cref="IScenarioMetadata.BuildingTypes"/>
        public IEnumerable<IBuildingType> BuildingTypes { get { return this.buildingTypes.Values; } }

        /// <see cref="IScenarioMetadata.AddonTypes"/>
        public IEnumerable<IAddonType> AddonTypes { get { return this.addonTypes.Values; } }

        /// <see cref="IScenarioMetadata.CustomTypes"/>
        public IEnumerable<IScenarioElementType> CustomTypes { get { return this.customTypes.Values; } }

        /// <see cref="IScenarioMetadata.AllTypes"/>
        public IEnumerable<IScenarioElementType> AllTypes { get { return this.allTypes; } }

        /// <see cref="IScenarioMetadata.this[]"/>
        public IScenarioElementType this[int typeID] { get { return this.allTypes[typeID]; } }

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

        /// <see cref="IScenarioMetadata.GetCustomTypeImpl"/>
        public ScenarioElementType GetCustomTypeImpl(string customTypeName)
        {
            if (customTypeName == null) { throw new ArgumentNullException("customTypeName"); }
            return this.customTypes[customTypeName];
        }

        /// <see cref="IScenarioMetadata.GetElementTypeImpl"/>
        public ScenarioElementType GetElementTypeImpl(string typeName)
        {
            if (typeName == null) { throw new ArgumentNullException("typeName"); }
            if (this.unitTypes.ContainsKey(typeName)) { return this.unitTypes[typeName]; }
            else if (this.upgradeTypes.ContainsKey(typeName)) { return this.upgradeTypes[typeName]; }
            else if (this.buildingTypes.ContainsKey(typeName)) { return this.buildingTypes[typeName]; }
            else if (this.addonTypes.ContainsKey(typeName)) { return this.addonTypes[typeName]; }
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
                this.customTypes.ContainsKey(upgradeType.Name)) { throw new InvalidOperationException(string.Format("ScenarioMetadata element with name '{0}' already defined!", upgradeType.Name)); }

            this.upgradeTypes.Add(upgradeType.Name, upgradeType);
            upgradeType.SetID(this.allTypes.Count);
            this.allTypes.Add(upgradeType);
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
                this.customTypes.ContainsKey(customType.Name)) { throw new InvalidOperationException(string.Format("ScenarioMetadata element with name '{0}' already defined!", customType.Name)); }

            this.customTypes.Add(customType.Name, customType);
            customType.SetID(this.allTypes.Count);
            this.allTypes.Add(customType);
        }

        /// <summary>
        /// Checks and finalizes the metadata object. Buildup methods will be unavailable after calling this method.
        /// </summary>
        public void CheckAndFinalize()
        {
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
            int currIdx = 0;
            foreach (ScenarioElementType objType in this.allTypes)
            {
                ISpritePalette<MapDirection> spritePalette = objType.SpritePalette;
                if (spritePalette != null)
                {
                    spritePalette.SetIndex(currIdx);
                    currIdx++;
                }

                objType.CheckAndFinalize();
            }

            this.isFinalized = true;
        }

        #endregion ScenarioMetadata buildup methods

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
        /// List of the defined custom types mapped by their names.
        /// </summary>
        private Dictionary<string, ScenarioElementType> customTypes;

        /// <summary>
        /// List of all defined types mapped by their IDs.
        /// </summary>
        private List<ScenarioElementType> allTypes;

        /// <summary>
        /// Indicates whether this metadata object has been finalized or not.
        /// </summary>
        private bool isFinalized;
    }
}
