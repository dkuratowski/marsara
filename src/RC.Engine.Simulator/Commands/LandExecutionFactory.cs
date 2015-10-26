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
    /// Factory for land command executions.
    /// </summary>
    public class LandExecutionFactory : CommandExecutionFactoryBase<Entity>
    {
        /// <summary>
        /// Constructs a LandExecutionFactory instance.
        /// </summary>
        /// <param name="entityType">The type of the recipient entities.</param>
        public LandExecutionFactory(string entityType)
            : base(COMMAND_TYPE, entityType)
        {
        }

        /// <see cref="CommandExecutionFactoryBase.GetCommandAvailability"/>
        protected override AvailabilityEnum GetCommandAvailability(RCSet<Entity> entitiesToHandle, RCSet<Entity> fullEntitySet, string parameter)
        {
            if (entitiesToHandle.Count != 1) { return AvailabilityEnum.Unavailable; }

            Entity recipientEntity = entitiesToHandle.First();
            return recipientEntity.MotionControl.IsFlying && recipientEntity.ActiveProductionLine == null ? AvailabilityEnum.Enabled : AvailabilityEnum.Unavailable;
        }

        /// <see cref="CommandExecutionFactoryBase.CreateCommandExecutions"/>
        protected override IEnumerable<CmdExecutionBase> CreateCommandExecutions(RCSet<Entity> entitiesToHandle, RCSet<Entity> fullEntitySet, RCNumVector targetPosition, int targetEntityID, string parameter)
        {
            /// Create the command executions.
            foreach (Entity entity in entitiesToHandle)
            {
                yield return new LandExecution(entity, (RCIntVector)targetPosition);
            }
        }

        /// <summary>
        /// The type of the command handled by this factory.
        /// </summary>
        private const string COMMAND_TYPE = "Land";
    }
}
