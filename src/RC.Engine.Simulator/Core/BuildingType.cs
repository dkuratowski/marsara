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
        /// <param name="spritePalette">The sprite palette of this building type.</param>
        public BuildingType(string name, SpritePalette spritePalette)
            : base(name, spritePalette)
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
            return this.addonTypes.ContainsKey(addonTypeName);
        }

        /// <summary>
        /// Adds a unit type to this building type.
        /// </summary>
        /// <param name="unitType">The unit type to add.</param>
        public void AddUnitType(UnitType unitType)
        {
            if (this.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (unitType == null) { throw new ArgumentNullException("unitType"); }
            this.unitTypes.Add(unitType.Name, unitType);
        }

        /// <summary>
        /// Adds an addon type to this building type.
        /// </summary>
        /// <param name="addonType">The addon type to add.</param>
        public void AddAddonType(AddonType addonType)
        {
            if (this.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (addonType == null) { throw new ArgumentNullException("addonType"); }
            this.addonTypes.Add(addonType.Name, addonType);
        }

        /// <summary>
        /// Adds an upgrade type to this building type.
        /// </summary>
        /// <param name="upgradeType">The upgrade type to add.</param>
        public void AddUpgradeType(UpgradeType upgradeType)
        {
            if (this.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (upgradeType == null) { throw new ArgumentNullException("upgradeType"); }
            this.upgradeTypes.Add(upgradeType.Name, upgradeType);
        }

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
