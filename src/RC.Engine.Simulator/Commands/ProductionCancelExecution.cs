using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Commands
{
    /// <summary>
    /// Responsible for executing production cancel commands.
    /// </summary>
    public class ProductionCancelExecution : CmdExecutionBase
    {
        /// <summary>
        /// Creates a ProductionCancelExecution instance.
        /// </summary>
        /// <param name="recipientEntity">The recipient entity of this command execution.</param>
        /// <param name="productionJobID">The ID of the production job to be cancelled.</param>
        public ProductionCancelExecution(Entity recipientEntity, int productionJobID)
            : base(new RCSet<Entity> { recipientEntity })
        {
            this.recipientEntity = this.ConstructField<Entity>("recipientEntity");
            this.productionJobID = this.ConstructField<int>("productionJobID");
            this.recipientEntity.Write(recipientEntity);
            this.productionJobID.Write(productionJobID);
        }

        #region Overrides

        /// <see cref="CmdExecutionBase.ContinueImpl"/>
        protected override bool ContinueImpl()
        {
            this.recipientEntity.Read().CancelProduction(this.productionJobID.Read());
            return true;
        }

        #endregion Overrides

        /// <summary>
        /// Reference to the recipient entity of this command execution.
        /// </summary>
        private readonly HeapedValue<Entity> recipientEntity;

        /// <summary>
        /// The ID of the production job to be cancelled.
        /// </summary>
        private readonly HeapedValue<int> productionJobID;
    }
}
