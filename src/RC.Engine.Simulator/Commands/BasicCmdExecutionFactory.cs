using System.Collections.Generic;
using RC.Common;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Engine;

namespace RC.Engine.Simulator.Commands
{
    /// <summary>
    /// Enumerates the basic commands.
    /// </summary>
    public enum BasicCommandEnum
    {
        Undefined = -1,
        [EnumMapping("Move")]
        Move = 0,
        [EnumMapping("Stop")]
        Stop = 1,
        [EnumMapping("Attack")]
        Attack = 2,
        [EnumMapping("Patrol")]
        Patrol = 3,
        [EnumMapping("Hold")]
        Hold = 4
    }

    /// <summary>
    /// Factory for basic command executions.
    /// </summary>
    public class BasicCmdExecutionFactory : CommandExecutionFactoryBase<Entity>
    {
        /// <summary>
        /// Constructs a BasicCmdExecutionFactory instance.
        /// </summary>
        /// <param name="command">The type of the basic command.</param>
        /// <param name="entityType">The type of the recipient entities.</param>
        public BasicCmdExecutionFactory(BasicCommandEnum command, string entityType)
            : base(command != BasicCommandEnum.Undefined ? EnumMap<BasicCommandEnum, string>.Map(command) : null, entityType)
        {
            this.commandType = command;
        }

        /// <see cref="CommandExecutionFactoryBase.GetCommandAvailability"/>
        protected override AvailabilityEnum GetCommandAvailability(HashSet<Entity> entitiesToHandle, HashSet<Entity> fullEntitySet)
        {
            /// TODO: implement this method!
            return AvailabilityEnum.Enabled;
        }

        /// <see cref="CommandExecutionFactoryBase.CreateCommandExecutions"/>
        protected override IEnumerable<CmdExecutionBase> CreateCommandExecutions(HashSet<Entity> entitiesToHandle, HashSet<Entity> fullEntitySet, RCNumVector targetPosition, int targetEntityID, string parameter)
        {
            switch (this.commandType)
            {
                case BasicCommandEnum.Move:
                    return this.CreateMoveExecutions(entitiesToHandle, fullEntitySet, targetPosition, targetEntityID);
                case BasicCommandEnum.Stop:
                    return this.CreateStopExecutions(entitiesToHandle);
                case BasicCommandEnum.Attack:
                    return this.CreateAttackExecutions(entitiesToHandle, fullEntitySet, targetPosition, targetEntityID);
                case BasicCommandEnum.Patrol:
                    return this.CreatePatrolExecutions(entitiesToHandle, fullEntitySet, targetPosition);
                case BasicCommandEnum.Hold:
                    return this.CreateHoldExecutions(entitiesToHandle);
                default:
                    /// TODO: implement more intelligent handling of undefined commands!
                    return this.CreateMoveExecutions(entitiesToHandle, fullEntitySet, targetPosition, targetEntityID);
            }
        }

        /// <summary>
        /// Creates stop executions for the given entities.
        /// </summary>
        /// <param name="entitiesToHandle">The entities to stop.</param>
        private IEnumerable<CmdExecutionBase> CreateStopExecutions(HashSet<Entity> entitiesToHandle)
        {
            foreach (Entity entity in entitiesToHandle) { yield return new StopExecution(entity); }
        }

        /// <summary>
        /// Creates hold executions for the given entities.
        /// </summary>
        /// <param name="entitiesToHandle">The entities to hold.</param>
        private IEnumerable<CmdExecutionBase> CreateHoldExecutions(HashSet<Entity> entitiesToHandle)
        {
            foreach (Entity entity in entitiesToHandle) { yield return new HoldExecution(entity); }
        }

        /// <summary>
        /// Creates move executions for the given entities.
        /// </summary>
        /// <param name="entitiesToHandle">The entities to move.</param>
        /// <param name="fullEntitySet">The set of selected entities.</param>
        /// <param name="targetPosition">The target position.</param>
        /// <param name="targetEntityID">The ID of the target entity or -1 if undefined.</param>
        private IEnumerable<CmdExecutionBase> CreateMoveExecutions(HashSet<Entity> entitiesToHandle, HashSet<Entity> fullEntitySet, RCNumVector targetPosition, int targetEntityID)
        {
            MagicBox magicBox = new MagicBox(fullEntitySet, targetPosition);
            foreach (Entity entity in entitiesToHandle) { yield return new MoveExecution(entity, magicBox.GetTargetPosition(entity), targetEntityID); }
        }

        /// <summary>
        /// Creates attack executions for the given entities.
        /// </summary>
        /// <param name="entitiesToHandle">The attacking entities.</param>
        /// <param name="fullEntitySet">The set of selected entities.</param>
        /// <param name="targetPosition">The target position.</param>
        /// <param name="targetEntityID">The ID of the target entity or -1 if undefined.</param>
        private IEnumerable<CmdExecutionBase> CreateAttackExecutions(HashSet<Entity> entitiesToHandle, HashSet<Entity> fullEntitySet, RCNumVector targetPosition, int targetEntityID)
        {
            MagicBox magicBox = new MagicBox(fullEntitySet, targetPosition);
            foreach (Entity entity in entitiesToHandle) { yield return new AttackExecution(entity, magicBox.GetTargetPosition(entity), targetEntityID); }
        }

        /// <summary>
        /// Creates patrol executions for the given entities.
        /// </summary>
        /// <param name="entitiesToHandle">The entities to patrol.</param>
        /// <param name="fullEntitySet">The set of selected entities.</param>
        /// <param name="targetPosition">The target position.</param>
        private IEnumerable<CmdExecutionBase> CreatePatrolExecutions(HashSet<Entity> entitiesToHandle, HashSet<Entity> fullEntitySet, RCNumVector targetPosition)
        {
            MagicBox magicBox = new MagicBox(fullEntitySet, targetPosition);
            foreach (Entity entity in entitiesToHandle) { yield return new PatrolExecution(entity, magicBox.GetTargetPosition(entity)); }
        }

        /// <summary>
        /// The type of the command execution that this factory creates.
        /// </summary>
        private readonly BasicCommandEnum commandType;
    }
}
