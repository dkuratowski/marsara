using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Contains the definition of an upgrade type.
    /// </summary>
    class UpgradeType : SimulatedObjectType
    {
        /// <summary>
        /// Constructs a new upgrade type.
        /// </summary>
        /// <param name="name">The name of this upgrade type.</param>
        public UpgradeType(string name)
            : base(name)
        {
            this.previousLevel = null;
            this.nextLevel = null;
            this.researchedIn = null;
            this.previousLevelName = null;
        }

        /// <summary>
        /// Gets the previous level of this upgrade type.
        /// </summary>
        public UpgradeType PreviousLevel
        {
            get { return this.previousLevel; }
            set
            {
                if (this.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
                if (value == this.previousLevel) { return; }

                if (value != null && this.previousLevel == null)
                {
                    this.CheckPotentialPreviousLevel(value);
                    this.previousLevel = value;
                    this.previousLevel.nextLevel = this;
                }
                else if (value == null && this.previousLevel != null)
                {
                    this.previousLevel.nextLevel = null;
                    this.previousLevel = null;
                }
            }
        }

        /// <summary>
        /// Gets the next level of this upgrade type.
        /// </summary>
        public UpgradeType NextLevel
        {
            get { return this.previousLevel; }
            set
            {
                if (this.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
                if (value == this.nextLevel) { return; }

                if (value != null && this.nextLevel == null)
                {
                    this.CheckPotentialNextLevel(value);
                    this.nextLevel = value;
                    this.nextLevel.previousLevel = this;
                }
                else if (value == null && this.nextLevel != null)
                {
                    this.nextLevel.previousLevel = null;
                    this.nextLevel = null;
                }
            }
        }

        /// <summary>
        /// Gets the name of the building/addon type that researches this type of upgrade or null if no such a
        /// building/addon type exists.
        /// </summary>
        public string ResearchedIn
        {
            get { return this.researchedIn; }
            set
            {
                if (this.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
                this.researchedIn = value;
            }
        }

        /// <summary>
        /// Gets the name of the upgrade type that is the previous level of this upgrade type.
        /// </summary>
        public string PreviousLevelName
        {
            get { return this.previousLevelName; }
            set
            {
                if (this.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
                this.previousLevelName = value;
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

        /// <summary>
        /// Checks the given upgrade type if it is OK to attach to this upgrade type as next level.
        /// </summary>
        /// <param name="potentialNextLevel">The upgrade type to check.</param>
        private void CheckPotentialNextLevel(UpgradeType potentialNextLevel)
        {
            if (potentialNextLevel.previousLevel != null) { throw new SimulatorException(string.Format("UpgradeType '{0}' already has a previous level!", potentialNextLevel.Name)); }

            HashSet<UpgradeType> levelPath = new HashSet<UpgradeType>();
            levelPath.Add(this);
            levelPath.Add(potentialNextLevel);
            UpgradeType currPathNode = potentialNextLevel;
            while (currPathNode.nextLevel != null)
            {
                if (levelPath.Contains(currPathNode.nextLevel)) { throw new SimulatorException(string.Format("Cycle found from UpgradeType '{0}' to '{1}'!", potentialNextLevel.Name, this.Name)); }
                levelPath.Add(currPathNode.nextLevel);
                currPathNode = currPathNode.nextLevel;
            }
        }

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
