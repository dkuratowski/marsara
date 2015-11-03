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
    /// A trivial command execution factory for testing.
    /// </summary>
    public class TestCmdExecutionFactory : CommandExecutionFactoryBase<Entity>
    {
        /// <summary>
        /// Constructs a TestCmdExecutionFactory instance.
        /// </summary>
        /// <param name="commandType">The type of the command for which this factory creates executions.</param>
        /// <param name="entityType">The type of entities for which this factory creates executions.</param>
        public TestCmdExecutionFactory(string commandType, string entityType)
            : base(commandType, entityType)
        {
        }

        #region Overrides

        /// <see cref="CommandExecutionFactoryBase.GetCommandAvailability"/>
        protected override AvailabilityEnum GetCommandAvailability(RCSet<Entity> entitiesToHandle, RCSet<Entity> fullEntitySet, string parameter)
        {
            return AvailabilityEnum.Enabled;
        }

        /// <see cref="CommandExecutionFactoryBase.CreateCommandExecutions"/>
        protected override IEnumerable<CmdExecutionBase> CreateCommandExecutions(RCSet<Entity> entitiesToHandle, RCSet<Entity> fullEntitySet, RCNumVector targetPosition, int targetEntityID, string parameter)
        {
            return new List<CmdExecutionBase>();
        }

        #endregion Overrides
    }
}
