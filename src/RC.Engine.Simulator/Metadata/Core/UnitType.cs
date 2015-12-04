using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Metadata.Core
{
    /// <summary>
    /// Contains the definition of a unit type.
    /// </summary>
    class UnitType : ScenarioElementType, IUnitTypeInternal
    {
        /// <summary>
        /// Constructs a new unit type.
        /// </summary>
        /// <param name="name">The name of this unit type.</param>
        /// <param name="metadata">The metadata object that this unit type belongs to.</param>
        public UnitType(string name, ScenarioMetadata metadata)
            : base(name, metadata)
        {
            this.necessaryAddon = null;
            this.createdIn = null;
        }

        #region IUnitTypeInternal members

        /// <see cref="IUnitTypeInternal.NecessaryAddon"/>
        public IAddonTypeInternal NecessaryAddon { get { return this.necessaryAddon; } }

        #endregion IUnitTypeInternal members

        #region UnitType buildup methods

        /// <summary>
        /// Sets the name of the building type that creates this type of units.
        /// </summary>
        public void SetCreatedIn(string buildingName)
        {
            if (this.Metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (buildingName == null) { throw new ArgumentNullException("buildingName"); }

            this.createdIn = buildingName;
        }

        /// <summary>
        /// Sets the name of the addon type that is necessary to be attached to the building that creates
        /// this type of units.
        /// </summary>
        public void SetNecessaryAddonName(string addonName)
        {
            if (this.Metadata.IsFinalized) { throw new InvalidOperationException("Already finalized!"); }
            if (addonName == null) { throw new ArgumentNullException("addonName"); }

            this.necessaryAddonName = addonName;
        }

        /// <see cref="ScenarioElementType.BuildupReferencesImpl"/>
        protected override void BuildupReferencesImpl()
        {
            if (this.Metadata.IsFinalized) { return; }

            if (this.createdIn != null)
            {
                if (!this.Metadata.HasBuildingType(this.createdIn)) { throw new SimulatorException(string.Format("BuildingType with name '{0}' doesn't exist!", this.createdIn)); }
                if (this.necessaryAddonName != null)
                {
                    if (!this.Metadata.HasAddonType(this.necessaryAddonName)) { throw new SimulatorException(string.Format("AddonType with name '{0}' doesn't exist!", this.necessaryAddonName)); }
                    if (!this.Metadata.GetBuildingTypeImpl(this.createdIn).HasAddonType(this.necessaryAddonName)) { throw new SimulatorException(string.Format("BuildingType '{0}' doesn't have AddonType '{1}'!", this.createdIn, this.necessaryAddonName)); }
                    this.necessaryAddon = this.Metadata.GetAddonTypeImpl(this.necessaryAddonName);
                }
                this.Metadata.GetBuildingTypeImpl(this.createdIn).AddUnitType(this);
            }
            else if (this.necessaryAddonName != null)
            {
                throw new SimulatorException(string.Format("UnitType '{0}' has necessary AddonType but creator BuildingType is not set!", this.Name));
            }
        }

        #endregion UnitType buildup methods

        /// <summary>
        /// Reference to the addon type that is necessary to be attached to the building to be able to create
        /// this type of units.
        /// </summary>
        private AddonType necessaryAddon;

        /// <summary>
        /// Name of the addon type that is necessary to be attached to the building to be able to create
        /// this type of units.
        /// </summary>
        private string necessaryAddonName;

        /// <summary>
        /// The name of the building type that creates this type of units or null if no such a building type exists.
        /// </summary>
        private string createdIn;
    }
}
