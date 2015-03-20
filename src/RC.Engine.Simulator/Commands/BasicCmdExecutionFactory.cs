using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Scenarios;

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

        /// <see cref="CommandExecutionFactoryBase.StartCommandExecution"/>
        protected override void StartCommandExecution(HashSet<Entity> entitiesToHandle, HashSet<Entity> fullEntitySet, RCNumVector targetPosition, int targetEntityID, string parameter)
        {
            switch (this.commandType)
            {
                case BasicCommandEnum.Move:
                    this.StartMoveExecution(entitiesToHandle, fullEntitySet, targetPosition, targetEntityID);
                    break;
                case BasicCommandEnum.Stop:
                    this.StartStopExecution(entitiesToHandle);
                    break;
                case BasicCommandEnum.Attack:
                    this.StartAttackExecution(entitiesToHandle, fullEntitySet, targetPosition, targetEntityID);
                    break;
                case BasicCommandEnum.Patrol:
                    break;
                case BasicCommandEnum.Hold:
                    break;
                default:
                    /// TODO: implement more intelligent handling of undefined commands!
                    this.StartMoveExecution(entitiesToHandle, fullEntitySet, targetPosition, targetEntityID);
                    break;
            }
        }

        /// <summary>
        /// Starts stop executions for the given entities.
        /// </summary>
        /// <param name="entitiesToHandle">The entities to stop.</param>
        private void StartStopExecution(HashSet<Entity> entitiesToHandle)
        {
            foreach (Entity entity in entitiesToHandle) { new StopExecution(entity); }
        }

        /// <summary>
        /// Starts move executions for the given entities.
        /// </summary>
        /// <param name="entitiesToHandle">The entities to move.</param>
        /// <param name="fullEntitySet">The set of selected entities.</param>
        /// <param name="targetPosition">The target position.</param>
        /// <param name="targetEntityID">The ID of the target entity or -1 if undefined.</param>
        private void StartMoveExecution(HashSet<Entity> entitiesToHandle, HashSet<Entity> fullEntitySet, RCNumVector targetPosition, int targetEntityID)
        {
            MagicBox magicBox = new MagicBox(fullEntitySet, targetPosition);
            foreach (Entity entity in entitiesToHandle) { new MoveExecution(entity, magicBox.GetTargetPosition(entity), targetEntityID); }
        }

        /// <summary>
        /// Starts attack executions for the given entities.
        /// </summary>
        /// <param name="entitiesToHandle">The attacking entities.</param>
        /// <param name="fullEntitySet">The set of selected entities.</param>
        /// <param name="targetPosition">The target position.</param>
        /// <param name="targetEntityID">The ID of the target entity or -1 if undefined.</param>
        private void StartAttackExecution(HashSet<Entity> entitiesToHandle, HashSet<Entity> fullEntitySet, RCNumVector targetPosition, int targetEntityID)
        {
            MagicBox magicBox = new MagicBox(fullEntitySet, targetPosition);
            foreach (Entity entity in entitiesToHandle) { new AttackExecution(entity, magicBox.GetTargetPosition(entity), targetEntityID); }
        }

        /// <summary>
        /// The type of the command execution that this factory creates.
        /// </summary>
        private readonly BasicCommandEnum commandType;
    }
}
