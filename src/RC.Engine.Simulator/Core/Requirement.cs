using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Represents a requirement for creating a building/unit/addon/upgrade type.
    /// </summary>
    class Requirement
    {
        /// <summary>
        /// Constructs a requirement object.
        /// </summary>
        /// <param name="buildingTypeName">The name of the required building type.</param>
        /// <param name="addonTypeName">
        /// The name of the required addon type or null if there is no addon type defined in this requirement.
        /// </param>
        public Requirement(string buildingTypeName, string addonTypeName)
        {
            if (buildingTypeName == null) { throw new ArgumentNullException("buildingTypeName"); }

            this.requiredBuildingTypeName = buildingTypeName;
            this.requiredAddonTypeName = addonTypeName;
        }

        /// <summary>
        /// Gets the required building type defined by this requirement.
        /// </summary>
        public BuildingType RequiredBuildingType
        {
            get { return this.requiredBuildingType; }
            set
            {
                if (this.isFinalized) { throw new InvalidOperationException("Already finalized!"); }
                this.requiredBuildingType = value;
            }
        }

        /// <summary>
        /// Gets the required addon type defined by this requirement.
        /// </summary>
        public AddonType RequiredAddonType
        {
            get { return this.requiredAddonType; }
            set
            {
                if (this.isFinalized) { throw new InvalidOperationException("Already finalized!"); }
                this.requiredAddonType = value;
            }
        }

        /// <summary>
        /// Gets the name of the required building type defined by this requirement.
        /// </summary>
        public string RequiredBuildingTypeName
        {
            get { return this.requiredBuildingTypeName; }
        }

        /// <summary>
        /// Gets the name of the required addon type defined by this requirement.
        /// </summary>
        public string RequiredAddonTypeName
        {
            get { return this.requiredBuildingTypeName; }
        }

        /// <summary>
        /// Checks and finalizes this requirement definition.
        /// </summary>
        public void CheckAndFinalize()
        {
            this.isFinalized = true;
        }

        /// <summary>
        /// Name of the required building type.
        /// </summary>
        private string requiredBuildingTypeName;

        /// <summary>
        /// Name of the required addon type.
        /// </summary>
        private string requiredAddonTypeName;

        /// <summary>
        /// Reference to the required building type.
        /// </summary>
        private BuildingType requiredBuildingType;

        /// <summary>
        /// Reference to the required addon type.
        /// </summary>
        private AddonType requiredAddonType;

        /// <summary>
        /// Indicates whether this requirement definition has been finalized or not.
        /// </summary>
        private bool isFinalized;
    }
}
