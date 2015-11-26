using RC.Common;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Commands
{
    /// <summary>
    /// Factory for construction cancel command executions.
    /// </summary>
    public class ConstructionCancelExecutionFactory : CommandExecutionFactoryBase<Entity>
    {
        /// <summary>
        /// Constructs a ConstructionCancelExecutionFactory instance.
        /// </summary>
        /// <param name="entityType">The type of the recipient entities.</param>
        public ConstructionCancelExecutionFactory(string entityType)
            : base(COMMAND_TYPE, entityType)
        {
        }

        /// <see cref="CommandExecutionFactoryBase.GetCommandAvailability"/>
        protected override AvailabilityEnum GetCommandAvailability(RCSet<Entity> entitiesToHandle, RCSet<Entity> fullEntitySet, string parameter)
        {
            if (entitiesToHandle.Count != 1) { return AvailabilityEnum.Unavailable; }

            Entity producerEntity = entitiesToHandle.First();
            return producerEntity.Biometrics.IsUnderConstruction ? AvailabilityEnum.Enabled : AvailabilityEnum.Unavailable;
        }

        /// <see cref="CommandExecutionFactoryBase.CreateCommandExecutions"/>
        protected override IEnumerable<CmdExecutionBase> CreateCommandExecutions(RCSet<Entity> entitiesToHandle, RCSet<Entity> fullEntitySet, RCNumVector targetPosition, int targetEntityID, string parameter)
        {
            /// Create the command executions.
            foreach (Entity entity in entitiesToHandle)
            {
                ConstructionCancelExecution constructionCancelExecution = new ConstructionCancelExecution(entity);
                yield return constructionCancelExecution;
            }
        }

        /// <summary>
        /// The type of the command handled by this factory.
        /// </summary>
        private const string COMMAND_TYPE = "CancelConstruction";
    }
}
