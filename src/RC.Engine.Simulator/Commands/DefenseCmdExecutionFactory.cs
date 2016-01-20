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
    /// Enumerates the defense commands.
    /// </summary>
    public enum DefenseCommandEnum
    {
        Undefined = -1,
        [EnumMapping("Stop")]
        Stop = 0,
        [EnumMapping("Attack")]
        Attack = 1,
    }

    /// <summary>
    /// Factory for defense command executions.
    /// </summary>
    public class DefenseCmdExecutionFactory : CommandExecutionFactoryBase<Entity>
    {
        /// <summary>
        /// Constructs a DefenseCmdExecutionFactory instance.
        /// </summary>
        /// <param name="command">The type of the defense command.</param>
        /// <param name="entityType">The type of the recipient entities.</param>
        public DefenseCmdExecutionFactory(DefenseCommandEnum command, string entityType)
            : base(command != DefenseCommandEnum.Undefined ? EnumMap<DefenseCommandEnum, string>.Map(command) : null, entityType)
        {
            this.commandType = command;
        }

        /// <see cref="CommandExecutionFactoryBase.GetCommandAvailability"/>
        protected override AvailabilityEnum GetCommandAvailability(RCSet<Entity> entitiesToHandle, RCSet<Entity> fullEntitySet, string parameter)
        {
            return entitiesToHandle.Count == 1 ? AvailabilityEnum.Enabled : AvailabilityEnum.Unavailable;
        }

        /// <see cref="CommandExecutionFactoryBase.CreateCommandExecutions"/>
        protected override IEnumerable<CmdExecutionBase> CreateCommandExecutions(RCSet<Entity> entitiesToHandle, RCSet<Entity> fullEntitySet, RCNumVector targetPosition, int targetEntityID, string parameter)
        {
            Entity recipientEntity = entitiesToHandle.First();
            switch (this.commandType)
            {
                case DefenseCommandEnum.Stop:
                    return new List<CmdExecutionBase> { new DefensiveStopExecution(recipientEntity) };
                case DefenseCommandEnum.Attack:
                    return new List<CmdExecutionBase> { new DefensiveAttackExecution(recipientEntity, targetEntityID) };
                default:
                    return new List<CmdExecutionBase> { new DefensiveAttackExecution(recipientEntity, targetEntityID) };
            }
        }

        /// <summary>
        /// The type of the command execution that this factory creates.
        /// </summary>
        private readonly DefenseCommandEnum commandType;
    }
}
