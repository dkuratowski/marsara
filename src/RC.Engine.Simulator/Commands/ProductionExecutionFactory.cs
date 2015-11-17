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
        /// Constructs a ProductionExecutionFactory instance.
        /// </summary>
        /// <param name="entityType">The type of the recipient entities.</param>
        /// <param name="firstProductType">The type of the first product that this execution factory is responsible for.</param>
        /// <param name="furtherProductTypes">The types of the further products that this execution factory is responsible for.</param>
        public ProductionExecutionFactory(string entityType, string firstProductType, params string[] furtherProductTypes)
            : base(COMMAND_TYPE, entityType)
        {
            if (firstProductType == null) { throw new ArgumentNullException("firstProductType"); }
            if (furtherProductTypes == null) { throw new ArgumentNullException("furtherProductTypes"); }

            this.productTypes = new RCSet<string>();
            this.productTypes.Add(firstProductType);
            foreach (string furtherProductType in furtherProductTypes)
            {
                this.productTypes.Add(furtherProductType);
            }
        }

        /// <see cref="CommandExecutionFactoryBase.GetCommandAvailability"/>
        protected override AvailabilityEnum GetCommandAvailability(RCSet<Entity> entitiesToHandle, RCSet<Entity> fullEntitySet, string parameter)
        {
            if (!this.productTypes.Contains(parameter)) { return AvailabilityEnum.Unavailable; }
            if (entitiesToHandle.Count != 1) { return AvailabilityEnum.Unavailable; }

            Entity producerEntity = entitiesToHandle.First();
            if (!producerEntity.IsProductAvailable(parameter)) { return AvailabilityEnum.Unavailable; }
            return producerEntity.IsProductEnabled(parameter) ? AvailabilityEnum.Enabled : AvailabilityEnum.Disabled;
        }

        /// <see cref="CommandExecutionFactoryBase.CreateCommandExecutions"/>
        protected override IEnumerable<CmdExecutionBase> CreateCommandExecutions(RCSet<Entity> entitiesToHandle, RCSet<Entity> fullEntitySet, RCNumVector targetPosition, int targetEntityID, string parameter)
        {
            foreach (Entity entity in entitiesToHandle)
            {
                ProductionExecution productExecution = new ProductionExecution(entity, parameter);
                yield return productExecution;
            }
        }

        /// <summary>
        /// The types of the products that this execution factory is responsible for.
        /// </summary>
        private readonly RCSet<string> productTypes;

        /// <summary>
        /// The type of the command handled by this factory.
        /// </summary>
        private const string COMMAND_TYPE = "StartProduction";
    }
}
