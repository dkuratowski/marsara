using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Commands
{
    /// <summary>
    /// Responsible for executing production start commands.
    /// </summary>
    public class ProductionExecution : CmdExecutionBase
    {
        /// <summary>
        /// Creates a ProductionExecution instance.
        /// </summary>
        /// <param name="recipientEntity">The recipient entity of this command execution.</param>
        /// <param name="product">The typename of the product.</param>
        public ProductionExecution(Entity recipientEntity, string product)
            : base(new RCSet<Entity> { recipientEntity })
        {
            this.recipientEntity = this.ConstructField<Entity>("recipientEntity");
            this.recipientEntity.Write(recipientEntity);
            this.product = product;
        }

        #region Overrides

        /// <see cref="CmdExecutionBase.ContinueImpl"/>
        protected override bool ContinueImpl()
        {
            if (this.recipientEntity.Read().IsProductAvailable(product))
            {
                if (this.recipientEntity.Read().IsProductEnabled(product))
                {
                    this.recipientEntity.Read().StartProduction(product);
                }
            }
            return true;
        }

        #endregion Overrides

        /// <summary>
        /// Reference to the recipient entity of this command execution.
        /// </summary>
        private readonly HeapedValue<Entity> recipientEntity;

        /// <summary>
        /// The typename of the product.
        /// </summary>
        /// TODO: heap this field!
        private readonly string product;
    }
}
