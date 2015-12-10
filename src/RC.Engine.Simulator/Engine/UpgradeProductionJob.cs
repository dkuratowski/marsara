using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// Represents a job in a production line that is producing addons.
    /// </summary>
    public class UpgradeProductionJob : ProductionJob
    {
        /// <summary>
        /// Constructs a UpgradeProductionJob instance.
        /// </summary>
        /// <param name="owner">The owner entity of this job.</param>
        /// <param name="upgradeProduct">The type of upgrade to be created by this job.</param>
        /// <param name="jobID">The ID of this job.</param>
        public UpgradeProductionJob(Entity owner, IUpgradeType upgradeProduct, int jobID)
            : base(owner, upgradeProduct, jobID)
        {
            this.upgradeProduct = upgradeProduct;
            this.dummyField = this.ConstructField<int>("dummyField");
            this.dummyField.Write(0);
        }

        /// <see cref="ProductionJob.StartImpl"/>
        protected override bool StartImpl()
        {
            /// Create the upgrade.
            this.OwnerPlayer.AddUpgrade(this.upgradeProduct.Name);
            return true;
        }

        /// <see cref="ProductionJob.AbortImpl"/>
        protected override void AbortImpl(int lockedMinerals, int lockedVespeneGas, int lockedSupplies)
        {
            /// Give back the locked resources and supply to the owner player.  
            this.OwnerPlayer.GiveResources(lockedMinerals, lockedVespeneGas);
            this.OwnerPlayer.UnlockSupply(lockedSupplies);

            /// Remove the upgrade from the owner player.
            this.OwnerPlayer.RemoveUpgrade(this.upgradeProduct.Name);
        }

        /// <summary>
        /// The type of upgrade created by this job.
        /// </summary>
        private readonly IUpgradeType upgradeProduct;

        /// <summary>
        /// A dummy field to keep the heap manager framework happy.
        /// </summary>
        private readonly HeapedValue<int> dummyField;
    }
}
