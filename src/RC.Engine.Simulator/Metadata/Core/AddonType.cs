using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Metadata.Core
{
    /// <summary>
    /// Contains the definition of an addon type.
    /// </summary>
    class AddonType : ScenarioElementType, IAddonTypeInternal
    {
        /// <summary>
        /// Constructs a new addon type.
        /// </summary>
        /// <param name="name">The name of this addon type.</param>
        /// <param name="metadata">The metadata object that this addon type belongs to.</param>
        public AddonType(string name, ScenarioMetadata metadata)
            : base(name, metadata)
        {
            this.upgradeTypes = new Dictionary<string, UpgradeType>();
            this.mainBuilding = null;
        }

        #region IAddonTypeInternal members

        /// <see cref="IAddonTypeInternal.HasUpgradeType"/>
        public bool HasUpgradeType(string upgradeTypeName)
        {
            if (upgradeTypeName == null) { throw new ArgumentNullException("upgradeTypeName"); }
            return this.upgradeTypes.ContainsKey(upgradeTypeName);
        }

        /// <see cref="IAddonTypeInternal.GetUpgradeType"/>
        public IUpgradeTypeInternal GetUpgradeType(string upgradeTypeName)
        {
            return this.GetUpgradeTypeImpl(upgradeTypeName);
        }

        /// <see cref="IAddonTypeInternal.UpgradeTypes"/>
        public IEnumerable<IUpgradeTypeInternal> UpgradeTypes { get { return this.upgradeTypes.Values; } }

        #endregion IAddonTypeInternal members

        #region Internal public methods

        /// <see cref="IAddonType.GetUpgradeType"/>
        public UpgradeType GetUpgradeTypeImpl(string upgradeTypeName)
        {
            if (upgradeTypeName == null) { throw new ArgumentNullException("upgradeTypeName"); }
            return this.upgradeTypes[upgradeTypeName];
        }

        #endregion Internal public methods

        #region AddonType buildup methods

        /// <summary>
        /// Adds an upgrade type to this addon type.
        /// </summary>
        /// <param name="upgradeType">The upgrade type to add.</param>
        public void AddUpgradeType(UpgradeType upgradeType)
        {
            if (this.Metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (upgradeType == null) { throw new ArgumentNullException("upgradeType"); }
            this.upgradeTypes.Add(upgradeType.Name, upgradeType);
        }

        /// <summary>
        /// Sets the name of the main building of this addon type.
        /// </summary>
        /// <param name="mainBuilding">The name of the main building of this addon type.</param>
        public void SetMainBuilding(string mainBuilding)
        {
            if (this.Metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (mainBuilding == null) { throw new ArgumentNullException("mainBuilding"); }
            this.mainBuilding = mainBuilding;
        }
        
        /// <see cref="ScenarioElementType.BuildupReferencesImpl"/>
        protected override void BuildupReferencesImpl()
        {
            if (this.Metadata.IsFinalized) { return; }
            if (this.mainBuilding == null) { throw new SimulatorException(string.Format("Main building not defined for AddonType '{0}'!", this.Name)); }

            if (!this.Metadata.HasBuildingType(this.mainBuilding)) { throw new SimulatorException(string.Format("BuildingType with name '{0}' doesn't exist!", this.mainBuilding)); }
            this.Metadata.GetBuildingTypeImpl(this.mainBuilding).AddAddonType(this);
        }

        #endregion AddonType buildup methods

        /// <summary>
        /// List of the upgrade types that are performed in addons of this type mapped by their names.
        /// </summary>
        private readonly Dictionary<string, UpgradeType> upgradeTypes;

        /// <summary>
        /// The name of the building type that creates this type of addon or null if no such a building type exists.
        /// </summary>
        private string mainBuilding;
    }
}
