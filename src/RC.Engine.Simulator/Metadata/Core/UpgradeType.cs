using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Metadata.Core
{
    /// <summary>
    /// Contains the definition of an upgrade type.
    /// </summary>
    class UpgradeType : ScenarioElementType, IUpgradeType
    {
        /// <summary>
        /// Constructs a new upgrade type.
        /// </summary>
        /// <param name="name">The name of this upgrade type.</param>
        /// <param name="metadata">The metadata object that this upgrade type belongs to.</param>
        public UpgradeType(string name, ScenarioMetadata metadata)
            : base(name, metadata)
        {
            this.previousLevel = null;
            this.nextLevel = null;
            this.researchedIn = null;
            this.previousLevelName = null;
        }

        #region IUpgradeType members

        /// <see cref="IUpgradeType.PreviousLevel"/>
        public IUpgradeType PreviousLevel { get { return this.previousLevel; } }

        /// <see cref="IUpgradeType.NextLevel"/>
        public IUpgradeType NextLevel { get { return this.nextLevel; } }

        #endregion IUpgradeType members

        #region UpgradeType buildup methods

        /// <summary>
        /// Sets the name of the building/addon type that researches this type of upgrade.
        /// </summary>
        /// <param name="typeName">The name of the building/addon type that researches this type of upgrade.</param>
        public void SetResearchedIn(string typeName)
        {
            if (this.Metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (typeName == null) { throw new ArgumentNullException("typeName"); }
            this.researchedIn = typeName;
        }

        /// <summary>
        /// Sets the name of the upgrade type that is the previous level of this upgrade type.
        /// </summary>
        public void SetPreviousLevelName(string previousLevelName)
        {
            if (this.Metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (previousLevelName == null) { throw new ArgumentNullException("previousLevelName"); }
            this.previousLevelName = previousLevelName;
        }

        /// <see cref="ScenarioElementType.BuildupReferencesImpl"/>
        protected override void BuildupReferencesImpl()
        {
            if (this.Metadata.IsFinalized) { return; }

            if (this.researchedIn != null)
            {
                if (this.Metadata.HasBuildingType(this.researchedIn))
                {
                    this.Metadata.GetBuildingTypeImpl(this.researchedIn).AddUpgradeType(this);
                }
                else if (this.Metadata.HasAddonType(this.researchedIn))
                {
                    this.Metadata.GetAddonTypeImpl(this.researchedIn).AddUpgradeType(this);
                }
                else
                {
                    throw new SimulatorException(string.Format("BuildingType or AddonType with name '{0}' doesn't exist!", this.researchedIn));
                }
            }

            if (this.previousLevelName != null)
            {
                if (!this.Metadata.HasUpgradeType(this.previousLevelName)) { throw new SimulatorException(string.Format("UpgradeType with name '{0}' doesn't exist!", this.previousLevelName)); }

                UpgradeType previousLevelUpg = this.Metadata.GetUpgradeTypeImpl(this.previousLevelName);
                this.CheckPotentialPreviousLevel(previousLevelUpg);
                this.previousLevel = previousLevelUpg;
                this.previousLevel.nextLevel = this;
            }
        }

        /// <summary>
        /// Checks the given upgrade type if it is OK to attach to this upgrade type as previous level.
        /// </summary>
        /// <param name="potentialPreviousLevel">The upgrade type to check.</param>
        private void CheckPotentialPreviousLevel(UpgradeType potentialPreviousLevel)
        {
            if (potentialPreviousLevel.nextLevel != null) { throw new SimulatorException(string.Format("UpgradeType '{0}' already has a next level!", potentialPreviousLevel.Name)); }

            HashSet<UpgradeType> levelPath = new HashSet<UpgradeType>();
            levelPath.Add(this);
            levelPath.Add(potentialPreviousLevel);
            UpgradeType currPathNode = potentialPreviousLevel;
            while (currPathNode.previousLevel != null)
            {
                if (levelPath.Contains(currPathNode.previousLevel)) { throw new SimulatorException(string.Format("Cycle found from UpgradeType '{0}' to '{1}'!", potentialPreviousLevel.Name, this.Name)); }
                levelPath.Add(currPathNode.previousLevel);
                currPathNode = currPathNode.previousLevel;
            }
        }

        #endregion UpgradeType buildup methods

        /// <summary>
        /// Reference to the previous level of this upgrade type or null if this upgrade type has no previous level.
        /// </summary>
        private UpgradeType previousLevel;

        /// <summary>
        /// Reference to the next level of this upgrade type or null if this upgrade type has no next level.
        /// </summary>
        private UpgradeType nextLevel;

        /// <summary>
        /// The name of the building/addon type that researches this type of upgrade or null if no such a building/addon
        /// type exists.
        /// </summary>
        private string researchedIn;

        /// <summary>
        /// The name of the upgrade type that is the previous level of this upgrade type.
        /// </summary>
        private string previousLevelName;
    }
}
