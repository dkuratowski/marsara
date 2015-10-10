using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.ComponentInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.Core;
using RC.Engine.Simulator.Metadata;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// Represents an addon.
    /// </summary>
    public abstract class Addon : Entity
    {
        /// <summary>
        /// Constructs an Addon instance.
        /// </summary>
        /// <param name="addonTypeName">The name of the type of this addon.</param>
        protected Addon(string addonTypeName)
            : base(addonTypeName, false)
        {
            this.addonType = ComponentManager.GetInterface<IScenarioLoader>().Metadata.GetAddonType(addonTypeName);

            // Create and register the basic production lines of this addon based on the metadata.
            List<IScenarioElementType> upgradeTypes = new List<IScenarioElementType>(this.addonType.UpgradeTypes);
            if (upgradeTypes.Count > 0)
            {
                ProductionLine upgradeProductionLine = new ProductionLine(this, Constants.UPGRADE_PRODUCTION_LINE_CAPACITY, upgradeTypes);
                this.RegisterProductionLine(upgradeProductionLine);
            }
        }

        /// <summary>
        /// The type of this addon.
        /// </summary>
        private readonly IAddonType addonType;
    }
}
