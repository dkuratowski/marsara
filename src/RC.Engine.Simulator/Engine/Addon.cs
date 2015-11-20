using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.ComponentInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.Core;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// Represents an addon.
    /// </summary>
    public abstract class Addon : Entity
    {
        /// <summary>
        /// Gets the type of this addon.
        /// </summary>
        public IAddonType AddonType { get { return this.addonType; } }

        /// <summary>
        /// Gets the current main building of this addon or null if this addon has no main building currently.
        /// </summary>
        public Building CurrentMainBuilding
        {
            get
            {
                if (this.MapObject == null) { throw new InvalidOperationException("This addon is detached from the map!"); }

                if (this.MotionControl.Status != MotionControlStatusEnum.Fixed) { return null; }
                RCIntVector buildingPosition = new RCIntVector(this.MapObject.QuadraticPosition.Left - 1, this.MapObject.QuadraticPosition.Bottom - 1);
                if (buildingPosition.X < 0 || buildingPosition.X >= this.Scenario.Map.Size.X || buildingPosition.Y < 0 || buildingPosition.Y >= this.Scenario.Map.Size.Y) { return null; }

                Building building = this.MapContext.FixedEntities[buildingPosition.X, buildingPosition.Y] as Building;
                if (building == null || building.Biometrics.IsUnderConstruction) { return null; }

                return building.BuildingType.HasAddonType(this.AddonType.Name) ? building : null;
            }
        }

        /// <summary>
        /// Constructs an Addon instance.
        /// </summary>
        /// <param name="addonTypeName">The name of the type of this addon.</param>
        /// <param name="behaviors">The list of behaviors of this addon.</param>
        protected Addon(string addonTypeName, params EntityBehavior[] behaviors)
            : base(addonTypeName, false, behaviors)
        {
            this.addonType = ComponentManager.GetInterface<IScenarioLoader>().Metadata.GetAddonType(addonTypeName);

            // Create and register the basic production lines of this addon based on the metadata.
            List<IUpgradeType> upgradeTypes = new List<IUpgradeType>(this.addonType.UpgradeTypes);
            if (upgradeTypes.Count > 0)
            {
                ProductionLine upgradeProductionLine = new UpgradeProductionLine(this, upgradeTypes);
                this.RegisterProductionLine(upgradeProductionLine);
            }
        }

        /// <summary>
        /// The type of this addon.
        /// </summary>
        private readonly IAddonType addonType;
    }
}
