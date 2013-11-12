using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Contains the definition of an addon type.
    /// </summary>
    class AddonType : EntityType
    {
        /// <summary>
        /// Constructs a new addon type.
        /// </summary>
        /// <param name="name">The name of this addon type.</param>
        /// <param name="metadata">The metadata object that this addon type belongs to.</param>
        public AddonType(string name, SimMetadata metadata)
            : base(name, metadata)
        {
            this.upgradeTypes = new HashSet<UpgradeType>();
            this.mainBuilding = null;
        }

        #region AddonType buildup methods

        /// <summary>
        /// Adds an upgrade type to this addon type.
        /// </summary>
        /// <param name="upgradeType">The upgrade type to add.</param>
        public void AddUpgradeType(UpgradeType upgradeType)
        {
            if (this.Metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (upgradeType == null) { throw new ArgumentNullException("upgradeType"); }
            this.upgradeTypes.Add(upgradeType);
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
        
        /// <see cref="SimObjectType.BuildupReferencesImpl"/>
        protected override void BuildupReferencesImpl()
        {
            if (this.Metadata.IsFinalized) { return; }
            if (this.mainBuilding == null) { throw new SimulatorException(string.Format("Main building not defined for AddonType '{0}'!", this.Name)); }

            if (!this.Metadata.HasBuildingType(this.mainBuilding)) { throw new SimulatorException(string.Format("BuildingType with name '{0}' doesn't exist!", this.mainBuilding)); }
            this.Metadata.GetBuildingType(this.mainBuilding).AddAddonType(this);
        }

        #endregion AddonType buildup methods

        /// <summary>
        /// List of the upgrade types that are performed in addons of this type.
        /// </summary>
        private HashSet<UpgradeType> upgradeTypes;

        /// <summary>
        /// The name of the building type that creates this type of addon or null if no such a building type exists.
        /// </summary>
        private string mainBuilding;
    }
}
