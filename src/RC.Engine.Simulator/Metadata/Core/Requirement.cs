using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Metadata.Core
{
    /// <summary>
    /// Represents a requirement for creating a building/unit/addon/upgrade type.
    /// </summary>
    class Requirement : IRequirement
    {
        /// <summary>
        /// Constructs a requirement object.
        /// </summary>
        /// <param name="buildingTypeName">The name of the required building type.</param>
        /// <param name="addonTypeName">
        /// The name of the required addon type or null if there is no addon type defined in this requirement.
        /// </param>
        /// <param name="metadata">The metadata object that this requirement belongs to.</param>
        public Requirement(string buildingTypeName, string addonTypeName, ScenarioMetadata metadata)
        {
            if (metadata == null) { throw new ArgumentNullException("metadata"); }
            if (buildingTypeName == null) { throw new ArgumentNullException("buildingTypeName"); }

            this.requiredBuildingTypeName = buildingTypeName;
            this.requiredAddonTypeName = addonTypeName;

            this.metadata = metadata;
        }

        #region IRequirement methods

        /// <see cref="IRequirement.RequiredBuildingType"/>
        public IBuildingType RequiredBuildingType { get { return this.requiredBuildingType != null ? new IBuildingType(this.requiredBuildingType) : null; } }

        /// <see cref="IRequirement.RequiredAddonType"/>
        public IAddonType RequiredAddonType { get { return this.requiredAddonType != null ? new IAddonType(this.requiredAddonType) : null; } }

        #endregion IRequirement methods

        /// <summary>
        /// Checks and finalizes this requirement definition.
        /// </summary>
        public void CheckAndFinalize()
        {
            if (!this.metadata.IsFinalized)
            {
                if (!this.metadata.HasBuildingType(this.requiredBuildingTypeName)) { throw new SimulatorException(string.Format("BuildingType '{0}' doesn't exist!", this.requiredBuildingTypeName)); }
                this.requiredBuildingType = this.metadata.GetBuildingTypeImpl(this.requiredBuildingTypeName);

                if (this.requiredAddonTypeName != null)
                {
                    if (!this.requiredBuildingType.HasAddonType(this.requiredAddonTypeName)) { throw new SimulatorException(string.Format("BuildingType '{0}' doesn't have AddonType '{1}'!", this.requiredBuildingTypeName, this.requiredAddonTypeName)); }
                    this.requiredAddonType = this.requiredBuildingType.GetAddonTypeImpl(this.requiredAddonTypeName);
                }
            }
        }

        /// <summary>
        /// Reference to the required building type.
        /// </summary>
        private BuildingType requiredBuildingType;

        /// <summary>
        /// Reference to the required addon type.
        /// </summary>
        private AddonType requiredAddonType;

        /// <summary>
        /// Name of the required building type.
        /// </summary>
        private readonly string requiredBuildingTypeName;

        /// <summary>
        /// Name of the required addon type.
        /// </summary>
        private readonly string requiredAddonTypeName;

        /// <summary>
        /// Reference to the metadata object that this requirement belongs to.
        /// </summary>
        private readonly ScenarioMetadata metadata;
    }
}
