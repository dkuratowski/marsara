using RC.Engine.Simulator.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// Represents a production line that produces upgrades.
    /// </summary>
    public class UpgradeProductionLine : ProductionLine
    {
        /// <summary>
        /// Constructs a new UpgradeProductionLine instance for the given building with the given list of upgrade types it can produce.
        /// </summary>
        /// <param name="ownerEntity">The owner of this production line.</param>
        /// <param name="upgradeProducts">The type of upgrade products that can be produced by this production line.</param>
        public UpgradeProductionLine(Entity ownerEntity, List<IUpgradeType> upgradeProducts)
            : base(ownerEntity, UPGRADE_PRODUCTION_LINE_CAPACITY, new List<IScenarioElementType>(upgradeProducts))
        {
            this.upgradeProducts = new Dictionary<string, IUpgradeType>();
            foreach (IUpgradeType upgradeProduct in upgradeProducts) { this.upgradeProducts.Add(upgradeProduct.Name, upgradeProduct); }
        }

        /// <see cref="ProductionLine.CreateJob"/>
        protected override ProductionJob CreateJob(string productName, int jobID)
        {
            return new UpgradeProductionJob(this.Owner, this.upgradeProducts[productName], jobID);
        }

        /// <summary>
        /// The list of the upgrade types that can be produced by this production line mapped by their names.
        /// </summary>
        private readonly Dictionary<string, IUpgradeType> upgradeProducts;

        private const int UPGRADE_PRODUCTION_LINE_CAPACITY = 1;
    }
}
