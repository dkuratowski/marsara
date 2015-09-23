using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Engine;

namespace RC.Engine.Simulator.Commands
{
    /// <summary>
    /// Factory for production cancel command executions.
    /// </summary>
    public class ProductionCancelExecutionFactory : CommandExecutionFactoryBase<Entity>
    {
        /// <summary>
        /// Constructs a ProductionCancelExecutionFactory instance.
        /// </summary>
        /// <param name="entityType">The type of the recipient entities.</param>
        public ProductionCancelExecutionFactory(string entityType)
            : base(COMMAND_TYPE, entityType)
        {
        }

        /// <see cref="CommandExecutionFactoryBase.GetCommandAvailability"/>
        protected override AvailabilityEnum GetCommandAvailability(RCSet<Entity> entitiesToHandle, RCSet<Entity> fullEntitySet, string parameter)
        {
            if (entitiesToHandle.Count != 1) { return AvailabilityEnum.Unavailable; }

            Entity producerEntity = entitiesToHandle.First();
            return producerEntity.ActiveProductionLine != null ? AvailabilityEnum.Enabled : AvailabilityEnum.Unavailable;
        }

        /// <see cref="CommandExecutionFactoryBase.CreateCommandExecutions"/>
        protected override IEnumerable<CmdExecutionBase> CreateCommandExecutions(RCSet<Entity> entitiesToHandle, RCSet<Entity> fullEntitySet, RCNumVector targetPosition, int targetEntityID, string parameter)
        {
            /// Check the incoming parameter.
            int productionJobID;
            if (!int.TryParse(parameter, NumberStyles.Integer, CultureInfo.InvariantCulture, out productionJobID))
            {
                throw new ArgumentException(string.Format("The command parameter '{0}' is not an integer!", parameter), "parameter");
            }

            /// Create the command executions.
            foreach (Entity entity in entitiesToHandle)
            {
                ProductionCancelExecution productCancelExecution = new ProductionCancelExecution(entity, productionJobID);
                yield return productCancelExecution;
            }
        }

        /// <summary>
        /// The type of the command handled by this factory.
        /// </summary>
        private const string COMMAND_TYPE = "CancelProduction";
    }
}
