using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// Represents a job in a production line that is producing units.
    /// </summary>
    public class UnitProductionJob : ProductionJob
    {
        /// <summary>
        /// Constructs a UnitProductionJob instance.
        /// </summary>
        /// <param name="ownerBuilding">The owner building of this job.</param>
        /// <param name="unitProduct">The type of unit to be created by this job.</param>
        /// <param name="jobID">The ID of this job.</param>
        public UnitProductionJob(Building ownerBuilding, IUnitType unitProduct, int jobID)
            : base(ownerBuilding, unitProduct, jobID)
        {
            this.unitProduct = unitProduct;
            this.ownerBuilding = this.ConstructField<Building>("ownerBuilding");
            this.ownerBuilding.Write(ownerBuilding);
        }

        /// <see cref="ProductionJob.CompleteImpl"/>
        protected override bool CompleteImpl()
        {
            return this.ElementFactory.CreateElement(this.unitProduct.Name, this.ownerBuilding.Read());
        }

        /// <summary>
        /// The type of unit created by this job.
        /// </summary>
        private readonly IUnitType unitProduct;

        /// <summary>
        /// The owner building of this job.
        /// </summary>
        private readonly HeapedValue<Building> ownerBuilding;
    }
}
