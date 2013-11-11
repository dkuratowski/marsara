using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        /// <param name="spritePalette">The sprite palette of this addon type.</param>
        public AddonType(string name, SpritePalette spritePalette)
            : base(name, spritePalette)
        {
            this.upgradeTypes = new HashSet<UpgradeType>();
            this.mainBuilding = null;
        }

        /// <summary>
        /// Adds an upgrade type to this addon type.
        /// </summary>
        /// <param name="upgradeType">The upgrade type to add.</param>
        public void AddUpgradeType(UpgradeType upgradeType)
        {
            if (this.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (upgradeType == null) { throw new ArgumentNullException("upgradeType"); }
            this.upgradeTypes.Add(upgradeType);
        }

        /// <summary>
        /// Gets the name of the building type that creates this type of addon or null if no such a building type exists.
        /// </summary>
        public string MainBuilding
        {
            get { return this.mainBuilding; }
            set
            {
                if (this.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
                this.mainBuilding = value;
            }
        }
        
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
