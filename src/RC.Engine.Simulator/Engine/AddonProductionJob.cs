using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;
using RC.Common.Diagnostics;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Core;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Common;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// Represents a job in a production line that is producing addons.
    /// </summary>
    public class AddonProductionJob : ProductionJob
    {
        /// <summary>
        /// Constructs a AddonProductionJob instance.
        /// </summary>
        /// <param name="ownerBuilding">The owner building of this job.</param>
        /// <param name="addonProduct">The type of addon to be created by this job.</param>
        /// <param name="jobID">The ID of this job.</param>
        public AddonProductionJob(Building ownerBuilding, IAddonType addonProduct, int jobID)
            : base(ownerBuilding.Owner, addonProduct, jobID)
        {
            this.addonProduct = addonProduct;
            this.ownerBuilding = this.ConstructField<Building>("ownerBuilding");
            this.ownerBuilding.Write(ownerBuilding);
        }

        /// <see cref="ProductionJob.StartImpl"/>
        protected override bool StartImpl()
        {
            /// Create the addon and begin its construction.
            bool success = this.ElementFactory.CreateElement(this.addonProduct.Name, this.ownerBuilding.Read());
            if (success)
            {
                this.ownerBuilding.Read().CurrentAddon.Biometrics.Construct();
            }
            return success;
        }

        /// <see cref="ProductionJob.ContinueImpl"/>
        protected override bool ContinueImpl()
        {
            if (this.ownerBuilding.Read().CurrentAddon == null)
            {
                /// The addon has been destroyed -> do not continue this production job!
                return false;
            }

            /// Continue the construction of the addon.
            this.ownerBuilding.Read().CurrentAddon.Biometrics.Construct();
            return true;
        }

        /// <see cref="ProductionJob.AbortImpl"/>
        protected override void AbortImpl(int lockedMinerals, int lockedVespeneGas, int lockedSupplies)
        {
            /// Give back the locked supply to the player of the owner building (if the player still exists).
            this.OwnerPlayer.UnlockSupply(lockedSupplies);

            /// Destroy the addon being under construction.
            if (this.ownerBuilding.Read().CurrentAddon != null)
            {
                this.ownerBuilding.Read().CurrentAddon.Biometrics.CancelConstruct();
            }
        }

        /// <summary>
        /// The type of addon created by this job.
        /// </summary>
        private readonly IAddonType addonProduct;

        /// <summary>
        /// The owner building of this job.
        /// </summary>
        private readonly HeapedValue<Building> ownerBuilding;
    }
}
