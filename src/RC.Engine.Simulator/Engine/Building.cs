using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Core;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// Represents a building.
    /// </summary>
    public abstract class Building : Entity
    {
        /// <summary>
        /// Gets the type of this building.
        /// </summary>
        public IBuildingType BuildingType { get { return this.buildingType; } }

        /// <summary>
        /// Gets the addon that is currently joined to this building or null if this building has no addon joined to it.
        /// </summary>
        public Addon CurrentAddon
        {
            get
            {
                if (this.MapObject == null) { return null; }

                if (this.MotionControl.Status != MotionControlStatusEnum.Fixed) { return null; }
                RCIntVector addonPosition = new RCIntVector(this.MapObject.QuadraticPosition.Right, this.MapObject.QuadraticPosition.Bottom - 1);
                if (addonPosition.X < 0 || addonPosition.X >= this.Scenario.Map.Size.X || addonPosition.Y < 0 || addonPosition.Y >= this.Scenario.Map.Size.Y) { return null; }

                Addon addon = this.MapContext.FixedEntities[addonPosition.X, addonPosition.Y] as Addon;
                if (addon == null) { return null; }

                return this.buildingType.HasAddonType(addon.AddonType.Name) ? addon : null;
            }
        }

        /// <summary>
        /// Checks whether the placement constraints of this building allows it to be placed together with an addon of the given addon type
        /// at the given quadratic position and collects all the violating quadratic coordinates relative to the given position.
        /// </summary>
        /// <param name="position">The position to be checked.</param>
        /// <param name="addonType">The addon type to be checked.</param>
        /// <returns>
        /// The list of the quadratic coordinates (relative to the given position) violating the constraints of this building.
        /// </returns>
        public RCSet<RCIntVector> CheckPlacementConstraints(RCIntVector position, IAddonType addonType)
        {
            return this.buildingType.CheckPlacementConstraints(this, position, addonType);
        }

        /// <summary>
        /// Constructs a Building instance.
        /// </summary>
        /// <param name="buildingTypeName">The name of the type of this building.</param>
        /// <param name="behaviors">The list of behaviors of this building.</param>
        protected Building(string buildingTypeName, params EntityBehavior[] behaviors)
            : base(buildingTypeName, false, behaviors)
        {
            this.buildingType = ComponentManager.GetInterface<IScenarioLoader>().Metadata.GetBuildingType(buildingTypeName);

            // Create and register the basic production lines of this building based on the metadata.
            List<IUnitType> unitTypes = new List<IUnitType>(this.buildingType.UnitTypes);
            List<IAddonType> addonTypes = new List<IAddonType>(this.buildingType.AddonTypes);
            List<IUpgradeType> upgradeTypes = new List<IUpgradeType>(this.buildingType.UpgradeTypes);
            if (unitTypes.Count > 0)
            {
                ProductionLine unitProductionLine = new UnitProductionLine(this, unitTypes);
                this.RegisterProductionLine(unitProductionLine);
            }
            if (addonTypes.Count > 0)
            {
                ProductionLine addonProductionLine = new AddonProductionLine(this, addonTypes);
                this.RegisterProductionLine(addonProductionLine);
            }
            if (upgradeTypes.Count > 0)
            {
                ProductionLine upgradeProductionLine = new UpgradeProductionLine(this, upgradeTypes);
                this.RegisterProductionLine(upgradeProductionLine);
            }
        }

        /// <summary>
        /// The type of this building.
        /// </summary>
        private readonly IBuildingType buildingType;
    }
}
