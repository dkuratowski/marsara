using System;
using System.Collections.Generic;
using RC.Common;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Engine;

namespace RC.Engine.Simulator.Commands
{
    /// <summary>
    /// The abstract base class of command execution factories.
    /// </summary>
    public abstract class CommandExecutionFactoryBase<T> : ICommandExecutionFactory where T : Entity
    {
        #region ICommandExecutionFactory members

        /// <see cref="ICommandExecutionFactory.CommandType"/>
        public string CommandType { get { return this.commandType; } }

        /// <see cref="ICommandExecutionFactory.EntityType"/>
        public string EntityType { get { return this.entityType; } }

        /// <see cref="ICommandExecutionFactory.GetCommandAvailability"/>
        public AvailabilityEnum GetCommandAvailability(HashSet<Entity> entitySet)
        {
            HashSet<T> entitiesToHandle = this.CollectEntitiesToHandle(entitySet);
            return this.GetCommandAvailability(entitiesToHandle, entitySet);
        }

        /// <see cref="ICommandExecutionFactory.StartCommandExecution"/>
        public void StartCommandExecution(HashSet<Entity> entitySet, RCNumVector targetPosition, int targetEntityID, string parameter)
        {
            HashSet<T> entitiesToHandle = this.CollectEntitiesToHandle(entitySet);
            foreach (CmdExecutionBase commandExecution in this.CreateCommandExecutions(entitiesToHandle, entitySet, targetPosition, targetEntityID, parameter))
            {
                commandExecution.AttachToScenario();
            }
        }

        #endregion ICommandExecutionFactory members

        /// <summary>
        /// Construct a CommandExecutionFactoryBase instance.
        /// </summary>
        /// <param name="commandType">The type of the command for which this factory creates executions.</param>
        /// <param name="entityType">The type of entities for which this factory creates executions.</param>
        protected CommandExecutionFactoryBase(string commandType, string entityType)
        {
            if (commandType != null && string.IsNullOrWhiteSpace(commandType)) { throw new ArgumentException("The command type of the factory shall be null or a non-empty string!", "commandType"); }
            if (string.IsNullOrWhiteSpace(entityType)) { throw new ArgumentException("The entity type of the factory cannot be null or whitespace!", "entityType"); }

            this.commandType = commandType;
            this.entityType = entityType;
        }

        #region Overridables

        /// <summary>
        /// Gets the availability of the command from the point of view of this factory for the given entity set.
        /// </summary>
        /// <param name="entitiesToHandle">The subset of the full entity set that this factory has to handle.</param>
        /// <param name="fullEntitySet">The full entity set.</param>
        /// <returns>The availability of the command from the point of view of this factory for the given entity set.</returns>
        protected abstract AvailabilityEnum GetCommandAvailability(HashSet<T> entitiesToHandle, HashSet<Entity> fullEntitySet);

        /// <summary>
        /// Creates the appropriate command executions on the given entity set with the given parameters.
        /// </summary>
        /// <param name="entitiesToHandle">The subset of the full entity set that this factory has to handle.</param>
        /// <param name="fullEntitySet">The entity set.</param>
        /// <param name="targetPosition">The target position.</param>
        /// <param name="targetEntityID">The ID of the target entity or -1 if undefined.</param>
        /// <param name="parameter">The optional parameter.</param>
        protected abstract IEnumerable<CmdExecutionBase> CreateCommandExecutions(HashSet<T> entitiesToHandle, HashSet<Entity> fullEntitySet, RCNumVector targetPosition, int targetEntityID, string parameter);

        #endregion Overridables

        /// <summary>
        /// Collects the entities to be handled by this factory from the given entity set.
        /// </summary>
        /// <param name="entitySet">The entity set.</param>
        /// <returns>The collected entities to be handled by this factory.</returns>
        private HashSet<T> CollectEntitiesToHandle(HashSet<Entity> entitySet)
        {
            HashSet<T> entitiesToHandle = new HashSet<T>();
            foreach (Entity entity in entitySet)
            {
                if (entity.ElementType.Name == this.entityType) { entitiesToHandle.Add((T)entity); }
            }
            if (entitiesToHandle.Count == 0) { throw new InvalidOperationException("Invalid set of entities sent to a command execution factory!"); }
            return entitiesToHandle;
        }

        /// <summary>
        /// The type of the command for which this factory creates executions.
        /// </summary>
        private readonly string commandType;

        /// <summary>
        /// The type of entities for which this factory creates executions.
        /// </summary>
        private readonly string entityType;
    }
}
