using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Engine;

namespace RC.Engine.Simulator.Commands
{
    /// <summary>
    /// Factory for production command executions.
    /// </summary>
    public class ProductionExecutionFactory : CommandExecutionFactoryBase<Entity>
    {
        /// <summary>
        /// Constructs a BasicCmdExecutionFactory instance.
        /// </summary>
        /// <param name="productType">The type of the product.</param>
        /// <param name="entityType">The type of the recipient entities.</param>
        public ProductionExecutionFactory(string productType, string entityType)
            : base(COMMAND_TYPE, entityType)
        {
            this.productType = productType;
        }

        /// <see cref="CommandExecutionFactoryBase.GetCommandAvailability"/>
        protected override AvailabilityEnum GetCommandAvailability(RCSet<Entity> entitiesToHandle, RCSet<Entity> fullEntitySet)
        {
            /// TODO: implement this method!
            return AvailabilityEnum.Enabled;
        }

        /// <see cref="CommandExecutionFactoryBase.CreateCommandExecutions"/>
        protected override IEnumerable<CmdExecutionBase> CreateCommandExecutions(RCSet<Entity> entitiesToHandle, RCSet<Entity> fullEntitySet, RCNumVector targetPosition, int targetEntityID, string parameter)
        {
            /// TODO: implement this method!
            yield break;
        }

        /// <summary>
        /// The type of the product.
        /// </summary>
        private readonly string productType;

        /// <summary>
        /// The type of the command selected by this type of listeners.
        /// </summary>
        private const string COMMAND_TYPE = "StartProduction";
    }
}
