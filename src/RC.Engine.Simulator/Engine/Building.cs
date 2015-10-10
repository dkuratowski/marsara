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
        /// Constructs a Building instance.
        /// </summary>
        /// <param name="buildingTypeName">The name of the type of this building.</param>
        /// <param name="behaviors">The list of behaviors of this entity.</param>
        protected Building(string buildingTypeName, params EntityBehavior[] behaviors)
            : base(buildingTypeName, false, behaviors)
        {
            this.buildingType = ComponentManager.GetInterface<IScenarioLoader>().Metadata.GetBuildingType(buildingTypeName);

            // Create and register the basic production lines of this building based on the metadata.
            List<IScenarioElementType> unitTypes = new List<IScenarioElementType>(this.buildingType.UnitTypes);
            List<IScenarioElementType> addonTypes = new List<IScenarioElementType>(this.buildingType.AddonTypes);
            List<IScenarioElementType> upgradeTypes = new List<IScenarioElementType>(this.buildingType.UpgradeTypes);
            if (unitTypes.Count > 0)
            {
                ProductionLine unitProductionLine = new ProductionLine(this, Constants.UNIT_PRODUCTION_LINE_CAPACITY, unitTypes);
                this.RegisterProductionLine(unitProductionLine);
            }
            if (addonTypes.Count > 0)
            {
                ProductionLine addonProductionLine = new ProductionLine(this, Constants.ADDON_PRODUCTION_LINE_CAPACITY, addonTypes);
                this.RegisterProductionLine(addonProductionLine);
            }
            if (upgradeTypes.Count > 0)
            {
                ProductionLine upgradeProductionLine = new ProductionLine(this, Constants.UPGRADE_PRODUCTION_LINE_CAPACITY, upgradeTypes);
                this.RegisterProductionLine(upgradeProductionLine);
            }

            this.attachedAddon = this.ConstructField<Addon>("attachedAddon");
            this.attachedAddon.Write(null);
        }

        /// <see cref="Entity.IsProductionEnabledImpl"/>
        protected override bool IsProductionEnabledImpl(IScenarioElementType product)
        {
            IUnitType productAsUnitType = product as IUnitType;
            return productAsUnitType == null ||
                   productAsUnitType.NecessaryAddon == null ||
                   (this.attachedAddon.Read() != null &&
                    this.attachedAddon.Read().ElementType == productAsUnitType.NecessaryAddon);
        }

        /// <summary>
        /// Reference to the addon that is attached to this building or null if there is no attached addon.
        /// </summary>
        private readonly HeapedValue<Addon> attachedAddon;

        /// <summary>
        /// The type of this building.
        /// </summary>
        private readonly IBuildingType buildingType;
    }
}
